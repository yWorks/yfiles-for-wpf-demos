/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Demo.LargeGraphs.Animations;
using yWorks.Demo.LargeGraphs.Styles;
using yWorks.Demo.LargeGraphs.Styles.Fast;
using yWorks.Demo.LargeGraphs.Styles.LevelOfDetail;
using yWorks.Demo.LargeGraphs.Styles.Selection;
using yWorks.Demo.LargeGraphs.Styles.Virtualization;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Utils;

namespace yWorks.Demo.LargeGraphs
{
  /// <summary>
  ///   Class for the LargeGraphs demo window.
  /// </summary>
  [Demo("large-graphs", "showcase", "Large Graphs",
      Summary = "Shows how to display large graphs with good performance.", Tags = ["performance"],
      Keywords = ["rendering", "large", "data management"], RelatedDemos = ["layout-styles", "graph-analysis"])]
  public sealed partial class LargeGraphsDemo
  {
    #region Wrapper styles so that styles are easier to change.

    /// <summary>Wrapper style for edge labels.</summary>
    private readonly WrapperLabelStyle edgeLabelStyle = new(null);

    /// <summary>Wrapper style for edges.</summary>
    private readonly WrapperEdgeStyle edgeStyle = new(null);

    /// <summary>Wrapper style for node labels</summary>
    private readonly WrapperLabelStyle nodeLabelStyle = new(null);

    /// <summary>Wrapper style for nodes.</summary>
    private readonly WrapperNodeStyle nodeStyle = new(null);

    #endregion

    /// <summary><see cref="Random"/> instance for various things.</summary>
    private readonly Random rnd = new();

    /// <summary>Optimal settings for the sample graphs.</summary>
    private PerformanceSettings[] bestSettings;

    /// <summary>
    ///   Initializes a new instance of the <see cref="LargeGraphsDemo"/> class.
    /// </summary>
    public LargeGraphsDemo() {
      InitializeComponent();
      Unloaded += (_, _) => animator?.Stop();
    }

    #region Dependency properties for binding

    /// <summary>
    ///   Dependency property for <see cref="PerformanceSettings"/>.
    /// </summary>
    public static readonly DependencyProperty PerformanceSettingsProperty = DependencyProperty.Register(
        "PerformanceSettings", typeof(PerformanceSettings), typeof(LargeGraphsDemo),
        new PropertyMetadata(default(PerformanceSettings)));

    /// <summary>
    ///   Dependency property for <see cref="Fps"/>.
    /// </summary>
    public static readonly DependencyProperty FpsProperty = DependencyProperty.Register("Fps", typeof(double),
        typeof(LargeGraphsDemo), new PropertyMetadata(default(double)));

    /// <summary>
    ///   Dependency property for <see cref="VisualChildren"/>.
    /// </summary>
    public static readonly DependencyProperty VisualChildrenProperty = DependencyProperty.Register("VisualChildren",
        typeof(int), typeof(LargeGraphsDemo), new PropertyMetadata(default(int)));

    /// <summary>
    ///   Dependency property for <see cref="SelectionCount"/>.
    /// </summary>
    public static readonly DependencyProperty SelectionCountProperty = DependencyProperty.Register("SelectionCount",
        typeof(int), typeof(LargeGraphsDemo), new PropertyMetadata(default(int)));

    private Animator animator;

    /// <summary>Currently active set of performance settings.</summary>
    public PerformanceSettings PerformanceSettings {
      get => (PerformanceSettings) GetValue(PerformanceSettingsProperty);
      set => SetValue(PerformanceSettingsProperty, value);
    }

    /// <summary>Current frames per second</summary>
    /// <seealso cref="InitializeFpsCounter"/>
    public double Fps {
      get => (double) GetValue(FpsProperty);
      set => SetValue(FpsProperty, value);
    }

    /// <summary>Current number of items in the visual tree.</summary>
    /// <seealso cref="InitializeVisualTreeCounter"/>
    public int VisualChildren {
      get => (int) GetValue(VisualChildrenProperty);
      set => SetValue(VisualChildrenProperty, value);
    }

    /// <summary>Current number of items in the visual tree.</summary>
    /// <seealso cref="InitializeVisualTreeCounter"/>
    public int SelectionCount {
      get => (int) GetValue(SelectionCountProperty);
      set => SetValue(SelectionCountProperty, value);
    }

    #endregion

    /// <summary>
    ///   Initializes the demo after it has loaded.
    /// </summary>
    /// <remarks>Most initialization work is done in here.</remarks>
    protected override void Initialize() {
      InitializePerformanceSettings();

      InitializeFpsCounter();
      InitializeVisualTreeCounter();
      InitializeSelectionCountUpdater();

      InitializeGraphChooserBox();

      InitializeInputMode();

      graphChooserBox.SelectedIndex = 0;

      animator = new Animator(graphControl);
    }

    [STAThread]
    private static void Main() {
      Run<LargeGraphsDemo>();
    }

    #region Initialization methods

    /// <summary>Initializes the list of optimal performance settings for the sample graphs.</summary>
    private void InitializePerformanceSettings() {
      bestSettings = [
          new PerformanceSettings {
              VirtualizationDisabled = false,
              MinimumEdgeLength = 0,
              EdgeBendThreshold = 0,
              EdgeLabelVisibilityThreshold = 50,
              NodeLabelVisibilityThreshold = 20,
              NodeVirtualizationThreshold = 12,
              EdgeVirtualizationThreshold = 20,
              NodeLabelTextThreshold = 40,
              EdgeLabelTextThreshold = 50,
              ComplexNodeStyleThreshold = 60
          },
          new PerformanceSettings {
              VirtualizationDisabled = false,
              MinimumEdgeLength = 10,
              EdgeBendThreshold = 50,
              EdgeLabelVisibilityThreshold = 80,
              NodeLabelVisibilityThreshold = 20,
              NodeVirtualizationThreshold = 10,
              EdgeVirtualizationThreshold = 15,
              NodeLabelTextThreshold = 40,
              EdgeLabelTextThreshold = 80,
              ComplexNodeStyleThreshold = 100
          },
          new PerformanceSettings {
              LabelModelBakingEnabled = true,
              VirtualizationDisabled = true,
              MinimumEdgeLength = 10,
              EdgeBendThreshold = 50,
              EdgeLabelVisibilityThreshold = 80,
              NodeLabelVisibilityThreshold = 20,
              NodeVirtualizationThreshold = 4,
              EdgeVirtualizationThreshold = 5,
              NodeLabelTextThreshold = 40,
              EdgeLabelTextThreshold = 80,
              ComplexNodeStyleThreshold = 100
          },
          new PerformanceSettings {
              LabelModelBakingEnabled = true,
              VirtualizationDisabled = true,
              MinimumEdgeLength = 10,
              EdgeBendThreshold = 50,
              EdgeLabelVisibilityThreshold = 80,
              NodeLabelVisibilityThreshold = 20,
              NodeVirtualizationThreshold = 3,
              EdgeVirtualizationThreshold = 4,
              NodeLabelTextThreshold = 40,
              EdgeLabelTextThreshold = 80,
              ComplexNodeStyleThreshold = 100
          },
          new PerformanceSettings {
              VirtualizationDisabled = false,
              MinimumEdgeLength = 0,
              EdgeBendThreshold = 0,
              EdgeLabelVisibilityThreshold = 50,
              NodeLabelVisibilityThreshold = 20,
              NodeVirtualizationThreshold = 10,
              EdgeVirtualizationThreshold = 18,
              NodeLabelTextThreshold = 40,
              EdgeLabelTextThreshold = 50,
              ComplexNodeStyleThreshold = 60
          },
          new PerformanceSettings {
              VirtualizationDisabled = false,
              MinimumEdgeLength = 10,
              EdgeBendThreshold = 0,
              EdgeLabelVisibilityThreshold = 50,
              NodeLabelVisibilityThreshold = 20,
              NodeVirtualizationThreshold = 2,
              EdgeVirtualizationThreshold = 4,
              NodeLabelTextThreshold = 40,
              EdgeLabelTextThreshold = 50,
              ComplexNodeStyleThreshold = 60
          },
          new PerformanceSettings {
              LabelModelBakingEnabled = true,
              VirtualizationDisabled = true,
              MinimumEdgeLength = 10,
              EdgeBendThreshold = 0,
              EdgeLabelVisibilityThreshold = 50,
              NodeLabelVisibilityThreshold = 20,
              NodeVirtualizationThreshold = 2,
              EdgeVirtualizationThreshold = 3,
              NodeLabelTextThreshold = 40,
              EdgeLabelTextThreshold = 50,
              ComplexNodeStyleThreshold = 60
          },
          new PerformanceSettings {
              LabelModelBakingEnabled = true,
              VirtualizationDisabled = true,
              MinimumEdgeLength = 10,
              EdgeBendThreshold = 0,
              EdgeLabelVisibilityThreshold = 80,
              NodeLabelVisibilityThreshold = 30,
              NodeVirtualizationThreshold = 1.2,
              EdgeVirtualizationThreshold = 1.8,
              NodeLabelTextThreshold = 40,
              EdgeLabelTextThreshold = 80,
              ComplexNodeStyleThreshold = 100
          }
      ];

      // The following settings are common to all configurations.
      foreach (var p in bestSettings) {
        p.OverviewDisabled = true;
        p.FastStylesEnabled = true;
        p.SelectionHandlesDisabled = true;
        p.CustomSelectionDecoratorEnabled = true;
      }
    }

    /// <summary>
    ///   Sets up a timer that measures frames per second and updates the <see cref="Fps"/> property.
    /// </summary>
    private void InitializeFpsCounter() {
      var stopwatch = new Stopwatch();
      var frames = 0;
      CompositionTarget.Rendering += delegate { frames++; };

      var dt = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(300) };
      dt.Tick += delegate {
        Fps = frames / stopwatch.Elapsed.TotalSeconds;
        frames = 0;
        stopwatch.Restart();
      };
      dt.Start();
    }

    /// <summary>
    ///   Sets up a timer that counts the elements in the visual tree and updates the <see cref="VisualChildren"/> property.
    /// </summary>
    private void InitializeVisualTreeCounter() {
      var dt = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(500) };
      dt.Tick += delegate { VisualChildren = GetDescendantCountInVisualTree(graphControl); };
      dt.Start();
    }

    /// <summary>
    ///   Sets up a timer that counts the currently selected elements and updates the <see cref="SelectionCount"/> property.
    /// </summary>
    private void InitializeSelectionCountUpdater() {
      var dt = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(100) };
      dt.Tick += delegate { SelectionCount = graphControl.Selection.Count; };
      dt.Start();
    }

    /// <summary>
    ///   Initializes the graph chooser.
    /// </summary>
    private void InitializeGraphChooserBox() {
      var resourceNames = typeof(LargeGraphsDemo).Assembly.GetManifestResourceNames().Where(name =>
          name.Contains("Resources") && name.EndsWith(".graphmlz", StringComparison.OrdinalIgnoreCase));

      var graphInfos = (from fullFilename in resourceNames
                        let filename = fullFilename.Substring(fullFilename.IndexOf("Resources.") + 10)
                        let filenameWithoutExtension = filename.Substring(0, filename.Length - ".graphmlz".Length)
                        let counts = Regex.Match(filenameWithoutExtension, @"_(?<nodes>\d+)_(?<edges>\d+)")
                        let nodes = int.Parse(counts.Groups["nodes"].Value, CultureInfo.InvariantCulture)
                        let edges = int.Parse(counts.Groups["edges"].Value, CultureInfo.InvariantCulture)
                        let type = filenameWithoutExtension.Contains("radialtree") ? "Tree" : "Hierarchical"
                        let displayName = string.Format("{0}: {1} nodes, {2} edges", type, nodes, edges)
                        select new SampleGraphInfo {
                            Filename = fullFilename,
                            DisplayName = displayName,
                            NodeCount = nodes,
                            EdgeCount = edges,
                            Type = type
                        }).OrderBy(i => i.Type).ThenBy(i => i.NodeCount).ToList();

      graphChooserBox.ItemsSource = graphInfos;
    }

    /// <summary>
    ///   Initializes the input mode for the <see cref="GraphControl"/>.
    /// </summary>
    private void InitializeInputMode() {
      var geim = new GraphEditorInputMode {
          // These two sub-input modes need to look at all elements in the graph to determine whether they are
          // responsible for a beginning drag. This slows down initial UI response to marquee selection or panning the
          // viewport in graphs with a large number of items.
          // Depending on the exact needs it might be better to enable those only when edges or bends actually need to
          // be created.
          // Of course, using a GraphViewerInputMode here sidesteps the problem completely, if no graph editing is
          // needed.
          CreateEdgeInputMode = { Enabled = false },
          CreateBendInputMode = { Enabled = false }
      };
      graphControl.InputMode = geim;
    }

    #endregion

    #region Event handlers

    /// <summary>
    ///   Called when the »exit« menu item was selected.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <summary>
    ///   Called when the »previous graph« button was clicked.
    /// </summary>
    private void OnPreviousButtonClicked(object sender, EventArgs e) {
      graphChooserBox.SelectedIndex--;
    }

    /// <summary>
    ///   Called when the »next graph« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void OnNextButtonClicked(object sender, EventArgs e) {
      graphChooserBox.SelectedIndex++;
    }

    /// <summary>
    ///   Called when the selected item in the graph chooser combo box has changed.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
    private async void OnGraphChooserSelectionChanged(object sender, SelectionChangedEventArgs e) {
      var info = graphChooserBox.SelectedValue as SampleGraphInfo;
      if (info == null) {
        return;
      }

      await LoadGraphAsync(info);
    }

    /// <summary>
    ///   Called when The »Zoom animation« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void OnZoomAnimationClicked(object sender, RoutedEventArgs e) {
      StartAnimation();
      var node = GetRandomNode();
      graphControl.Center = node.Layout.GetCenter();

      var animation = new ZoomInAndBackAnimation(graphControl, 10, TimeSpan.FromSeconds(5));
      await animator.Animate(animation);
      EndAnimation();
    }

    /// <summary>
    ///   Called when the »Pan animation« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void OnPanAnimationClicked(object sender, RoutedEventArgs e) {
      StartAnimation();
      var anim = new CirclePanAnimation(graphControl, 2, TimeSpan.FromSeconds(2));
      await animator.Animate(anim);
      EndAnimation();
    }

    /// <summary>
    ///   Called when the »Spiral zoom animation« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void OnSpiralZoomAnimationClicked(object sender, RoutedEventArgs e) {
      StartAnimation();
      var node = GetRandomNode();
      graphControl.Center = node.Layout.GetCenter() + new PointD(graphControl.Viewport.Width / 4, 0);

      var zoom = new ZoomInAndBackAnimation(graphControl, 10, TimeSpan.FromSeconds(10));
      var pan = new CirclePanAnimation(graphControl, 14, TimeSpan.FromSeconds(10));
      var animation = new IAnimation[] { zoom, pan }.CreateParallelAnimation();
      await animator.Animate(animation);
      EndAnimation();
    }

    /// <summary>
    ///   Called when »Move nodes« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void OnNodeAnimationClicked(object sender, RoutedEventArgs e) {
      StartAnimation();
      var selection = graphControl.Selection;
      // If there is nothing selected, just use a random node
      if (selection.Nodes.Count == 0) {
        selection.Add(GetRandomNode());
      }

      var animation = new CircleNodeAnimation(graphControl.Graph, selection.Nodes, graphControl.Viewport.Width / 10, 2,
          TimeSpan.FromSeconds(2));
      await animator.Animate(animation);
      EndAnimation();
    }

    /// <summary>
    ///   Called when the »Select nothing« button was clicked..
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnSelectNothingClicked(object sender, RoutedEventArgs e) {
      graphControl.Selection.Clear();
    }

    /// <summary>
    ///   Called when the »Select 1000 random nodes« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnSelect1000NodesClicked(object sender, RoutedEventArgs e) {
      Select1000(graphControl.Graph.Nodes);
    }

    /// <summary>
    ///   Called when the »Select 1000 random edges« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnSelect1000EdgesClicked(object sender, RoutedEventArgs e) {
      Select1000(graphControl.Graph.Edges);
    }

    /// <summary>
    ///   Called when the »Select 1000 random labels« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnSelect1000LabelsClicked(object sender, RoutedEventArgs e) {
      Select1000(graphControl.Graph.Labels);
    }

    /// <summary>
    ///   Called when the »Select all nodes« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnSelectAllNodesClicked(object sender, RoutedEventArgs e) {
      SelectAll(graphControl.Graph.Nodes);
    }

    /// <summary>
    ///   Called when the »Select all edges« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnSelectAllEdgesClicked(object sender, RoutedEventArgs e) {
      SelectAll(graphControl.Graph.Edges);
    }

    /// <summary>
    ///   Called when the »Select all labels« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnSelectAllLabelsClicked(object sender, RoutedEventArgs e) {
      SelectAll(graphControl.Graph.Labels);
    }

    /// <summary>
    ///   Called when the »Select everything« button was clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnSelectEverythingClicked(object sender, RoutedEventArgs e) {
      foreach (var item in graphControl.Graph.Nodes) {
        graphControl.Selection.Add(item);
      }
      foreach (var item in graphControl.Graph.Edges) {
        graphControl.Selection.Add(item);
      }
      foreach (var item in graphControl.Graph.Labels) {
        graphControl.Selection.Add(item);
      }
    }

    /// <summary>
    ///   Called when a property in <see cref="PerformanceSettings"/> changes.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    ///   This is the main dispatcher for changes in the performance settings. Styles and other settings are updated if needed,
    ///   depending on the property that changed.
    /// </remarks>
    private void OnPerformanceSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
      // If the property name is the empty string or null, it indicates that every property changed.
      if (string.IsNullOrEmpty(e.PropertyName)) {
        UpdateStyles();
        UpdateSelectionHandlesSetting();
        UpdateLabelModelBakingSetting(graphControl.Graph);
        UpdateOverviewDisabledSetting();
        graphControl.Invalidate();
        RefreshSelection();
        return;
      }
      switch (e.PropertyName) {
        case "FastStylesEnabled":
        case "MinimumEdgeLength":
        case "EdgeBendThreshold":
        case "EdgeLabelVisibilityThreshold":
        case "NodeLabelVisibilityThreshold":
        case "NodeLabelTextThreshold":
        case "EdgeLabelTextThreshold":
        case "ComplexNodeStyleThreshold":
        case "VirtualizationDisabled":
        case "NodeVirtualizationThreshold":
        case "EdgeVirtualizationThreshold":
          UpdateStyles();
          break;
        case "SelectionHandlesDisabled":
          UpdateSelectionHandlesSetting();
          RefreshSelection();
          break;
        case "CustomSelectionDecoratorEnabled":
          RefreshSelection();
          break;
        case "LabelModelBakingEnabled":
          UpdateLabelModelBakingSetting(graphControl.Graph);
          break;
        case "OverviewDisabled":
          UpdateOverviewDisabledSetting();
          break;
      }
    }

    #endregion

    #region Helper methods

    #region UI Helpers

    /// <summary>
    ///   Disables the »Previous/Next graph« buttons in the UI according to whether there is a previous/next graph to switch
    ///   to.
    /// </summary>
    private void UpdateButtons() {
      nextButton.IsEnabled = graphChooserBox.SelectedIndex < graphChooserBox.Items.Count - 1;
      previousButton.IsEnabled = graphChooserBox.SelectedIndex > 0;
    }

    /// <summary>
    ///   Gets the number of descendants of a given control in the WPF visual tree.
    /// </summary>
    /// <param name="root">The control to get descendants of.</param>
    /// <returns>The number of descendants of <paramref name="root"/></returns>
    /// <seealso cref="InitializeVisualTreeCounter"/>
    private static int GetDescendantCountInVisualTree(DependencyObject root) {
      if (root == null) {
        return 0;
      }
      var childrenCount = VisualTreeHelper.GetChildrenCount(root);
      var sum = 0;
      for (var i = 0; i < childrenCount; i++) {
        var child = VisualTreeHelper.GetChild(root, i);
        if (child != null) {
          sum += GetDescendantCountInVisualTree(child);
        }
      }
      return sum + 1;
    }

    /// <summary>
    ///   Loads a graph asynchronously and places it in the <see cref="GraphControl"/>.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <remarks>The loading indicator is shown prior to loading and hidden afterwards</remarks>
    private async Task LoadGraphAsync(SampleGraphInfo info) {
      graphChooserBox.IsEnabled = false;
      nextButton.IsEnabled = false;
      previousButton.IsEnabled = false;
      graphControl.IsEnabled = false;

      graphLoadingBar.Visibility = Visibility.Visible;
      graphLoadingBar.IsIndeterminate = true;
      graphLoadingLabel.Visibility = Visibility.Visible;

      UpdatePerformanceSettings(bestSettings[graphChooserBox.SelectedIndex]);
      var g = new DefaultGraph();
      SetDefaultStyles(g);
      UpdateStyles();
      SetSelectionDecorators(g);
      UpdateSelectionHandlesSetting();
      UpdateOverviewDisabledSetting();

      await Task.Run(() => {
        var ioh = new GraphMLIOHandler();
        ioh.DeserializationPropertyOverrides.Set(SerializationProperties.ParseLabelSize, false);

        using (var stream = new GZipStream(typeof(LargeGraphsDemo).Assembly.GetManifestResourceStream(info.Filename)!,
            CompressionMode.Decompress)) {
          ioh.Read(g, stream);
        }
      });

      UpdateLabelModelBakingSetting(g);

      graphChooserBox.IsEnabled = true;
      graphControl.IsEnabled = true;
      UpdateButtons();

      graphLoadingBar.Visibility = Visibility.Collapsed;
      graphLoadingBar.IsIndeterminate = false;
      graphLoadingLabel.Visibility = Visibility.Collapsed;

      graphControl.Graph = g;
      await graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Selects all of the given items in the graph.
    /// </summary>
    private void SelectAll(IEnumerable<IModelItem> items) {
      foreach (var item in items) {
        graphControl.Selection.Add(item);
      }
    }

    /// <summary>
    ///   Selects 1000 items at random of the given enumerable.
    /// </summary>
    private void Select1000(IEnumerable<IModelItem> items) {
      foreach (var item in items.OrderBy(_ => rnd.Next()).Take(1000)) {
        graphControl.Selection.Add(item);
      }
    }

    /// <summary>
    ///   Helper method to set the cursor and disable the animation buttons when starting an animation.
    /// </summary>
    private void StartAnimation() {
      Cursor = Cursors.Wait;
      grpAnimations.IsEnabled = false;
    }

    /// <summary>
    ///   Helper method to reset the cursor and re-enable the animation buttons when an animation has finished.
    /// </summary>
    private void EndAnimation() {
      Cursor = null;
      grpAnimations.IsEnabled = true;
    }

    #endregion

    #region Performance setting helpers

    /// <summary>
    ///   Sets a new <see cref="T:PerformanceSettings"/> instance as the current one.
    /// </summary>
    /// <param name="newSettings">The new settings instance.</param>
    /// <remarks>Since the instance is mutable, this will assign a copy to <see cref="PerformanceSettings"/>.</remarks>
    private void UpdatePerformanceSettings(PerformanceSettings newSettings) {
      if (PerformanceSettings != null) {
        PerformanceSettings.PropertyChanged -= OnPerformanceSettingsPropertyChanged;
      }
      PerformanceSettings = PerformanceSettings.GetCopy(newSettings);
      PerformanceSettings.PropertyChanged += OnPerformanceSettingsPropertyChanged;
    }

    private void UpdateOverviewDisabledSetting() {
      // Disconnect the disabled overview from the GraphControl to stop graph updates and avoid rendering overhead.
      OverviewControl.GraphControl = PerformanceSettings.OverviewDisabled ? null : graphControl;
    }

    /// <summary>
    /// </summary>
    /// <param name="graph"></param>
    private void SetDefaultStyles(IGraph graph) {
      graph.NodeDefaults.Style = nodeStyle;
      graph.EdgeDefaults.Style = edgeStyle;
      graph.NodeDefaults.Labels.Style = nodeLabelStyle;
      graph.EdgeDefaults.Labels.Style = edgeLabelStyle;
    }

    /// <summary>
    ///   Updates the styles according to the values in <see cref="PerformanceSettings"/>.
    /// </summary>
    /// <remarks>
    ///   See <see cref="LargeGraphs.PerformanceSettings.FastStylesEnabled"/> and
    ///   <see cref="LargeGraphs.PerformanceSettings.VirtualizationDisabled"/> for a detailed description of the optimizations
    ///   involved.
    /// </remarks>
    private void UpdateStyles() {
      var p = PerformanceSettings;

      // Default label styles (those are also used at high zoom levels)
      var simpleEdgeLabelStyle = DemoStyles.CreateDemoEdgeLabelStyle();
      var simpleNodeLabelStyle = DemoStyles.CreateDemoNodeLabelStyle();

      if (p.FastStylesEnabled) {
        // Nodes
        nodeStyle.Style = new LevelOfDetailNodeStyle {
            Styles = {
                { 0, DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Rectangle) },
                { p.ComplexNodeStyleThreshold / 100 / 2, DemoStyles.CreateDemoNodeStyle() }, {
                    p.ComplexNodeStyleThreshold / 100,
                    new RectangleNodeStyle {
                        Corners = Corners.TopLeft | Corners.BottomRight,
                        Pen = new Pen(Themes.PaletteOrange.Stroke, 1.5),
                        Brush = new LinearGradientBrush {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(0, 1),
                            GradientStops = {
                                new GradientStop(Themes.PaletteOrange.NodeLabelFill.Color, 0),
                                new GradientStop(Themes.PaletteOrange.Fill.Color, 1)
                            }
                        }
                    }
                }
            }
        };
        // Edges
        edgeStyle.Style = new FastEdgeStyle {
            DrawBendsThreshold = p.EdgeBendThreshold / 100,
            MinimumEdgeLength = p.MinimumEdgeLength
        };
        // Node labels
        nodeLabelStyle.Style = new LevelOfDetailLabelStyle {
            Styles = {
                { 0, VoidLabelStyle.Instance },
                { p.NodeLabelVisibilityThreshold / 100, new FastLabelStyle(AutoFlipMode.AutoFlip) },
                { p.NodeLabelTextThreshold / 100, simpleNodeLabelStyle }
            }
        };
        // Edge labels
        edgeLabelStyle.Style = new LevelOfDetailLabelStyle {
            Styles = {
                { 0, VoidLabelStyle.Instance }, {
                    p.EdgeLabelVisibilityThreshold / 100,
                    new FastLabelStyle(AutoFlipMode.AutoFlip) { BackgroundBrush = Brushes.White }
                },
                { p.EdgeLabelTextThreshold / 100, simpleEdgeLabelStyle }
            }
        };
      } else {
        nodeStyle.Style = DemoStyles.CreateDemoNodeStyle();
        edgeStyle.Style = DemoStyles.CreateDemoEdgeStyle(showTargetArrow: false);
        edgeLabelStyle.Style = simpleEdgeLabelStyle;
        nodeLabelStyle.Style = simpleNodeLabelStyle;
      }

      // If we disable virtualization we just wrap the style we had so far into a Virtualization*StyleDecorator
      if (p.VirtualizationDisabled) {
        nodeStyle.Style = new VirtualizationNodeStyleDecorator(p.NodeVirtualizationThreshold / 100, nodeStyle.Style);
        edgeStyle.Style = new VirtualizationEdgeStyleDecorator(p.EdgeVirtualizationThreshold / 100, edgeStyle.Style);
      }

      // Repaint the graph control to update the visuals according to the changed style
      graphControl.Invalidate();
    }

    /// <summary>
    ///   Sets the selection decorators on the given <see cref="IGraph"/> instance.
    /// </summary>
    /// <param name="g">The graph.</param>
    /// <remarks>
    ///   This actually sets the selection decorator implementation by using a custom predicate which simply queries the
    ///   current <see cref="PerformanceSettings"/>. Thus the decoration is always up-to-date; the only thing that's needed
    ///   when the setting changes is to re-select all selected items to re-create the respective selection decoration visuals.
    /// </remarks>
    private void SetSelectionDecorators(IGraph g) {
      var nodeStyleDecorationRenderer = new NodeStyleIndicatorRenderer {
          NodeStyle = new FastNodeSelectionStyle(null, new Pen(Brushes.DarkRed, 4)),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates,
          Margins = InsetsD.Empty
      };
      var edgeStyleDecorationRenderer = new EdgeStyleIndicatorRenderer {
          EdgeStyle = new FastEdgeSelectionStyle(new Pen(Brushes.DarkRed, 3)),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
      };
      var labelStyleDecorationRenderer = new LabelStyleIndicatorRenderer {
          LabelStyle = new FastLabelSelectionStyle(null, new Pen(Brushes.LightGray, 4)),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates,
          Margins = InsetsD.Empty
      };

      var decorator = g.GetDecorator();
      Predicate<IModelItem> useCustomDecorationPredicate = _ => PerformanceSettings.CustomSelectionDecoratorEnabled;
      decorator.Nodes.SelectionRenderer.AddConstant(useCustomDecorationPredicate, nodeStyleDecorationRenderer);
      decorator.Edges.SelectionRenderer.AddConstant(useCustomDecorationPredicate, edgeStyleDecorationRenderer);
      decorator.Labels.SelectionRenderer.AddConstant(useCustomDecorationPredicate, labelStyleDecorationRenderer);
    }

    /// <summary>
    ///   Updates the input mode to reflect the current value of the
    ///   <see cref="LargeGraphs.PerformanceSettings.SelectionHandlesDisabled"/> setting.
    /// </summary>
    /// <remarks>
    ///   See <see cref="LargeGraphs.PerformanceSettings.SelectionHandlesDisabled"/> for a rationale for this
    ///   optimization.
    /// </remarks>
    private void UpdateSelectionHandlesSetting() {
      var p = PerformanceSettings;
      var geim = graphControl.InputMode as GraphEditorInputMode;
      if (geim != null) {
        geim.ShowHandleItems = p.SelectionHandlesDisabled ? GraphItemTypes.None : GraphItemTypes.All;
      }
    }

    /// <summary>
    ///   Updates all labels in the graph according to the current value of the
    ///   <see cref="LargeGraphs.PerformanceSettings.LabelModelBakingEnabled"/> setting.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     See <see cref="LargeGraphs.PerformanceSettings.LabelModelBakingEnabled"/> for a rationale for this optimization.
    ///   </para>
    ///   <para>
    ///     When activating this setting, all labels get a <see cref="FreeLabelModel"/>. An
    ///     <see cref="ILabelModelParameterFinder"/> instance from the label model helps finding the correct parameter so that
    ///     the labels don't change their positions. When disabling this setting, the same process is used, just in reverse,
    ///     that is, the respective label model for node and edge labels is used and its parameter finder asked for a good
    ///     parameter.
    ///   </para>
    ///   <para>
    ///     Labels using the <see cref="FreeLabelModel"/> are positioned absolutely in the canvas. Thus they won't move
    ///     when their owners move. If there is no need of getting the last bit of performance out of yFiles,
    ///     <see cref="FreeNodeLabelModel"/> and <see cref="FreeEdgeLabelModel"/> can be used instead. They are a bit slower
    ///     than <see cref="FreeLabelModel"/>, but have the benefit that they are anchored relative to their owner, creating a
    ///     less jarring experience than labels that just stay where they were when their owner moves.
    ///   </para>
    ///   <para>
    ///     Another option (not shown in this demo) would be to convert between the label models on affected labels prior
    ///     to and after an edit operation, such as moving nodes, adding or moving bends to edges, etc. so that the editing
    ///     experience uses the expensive label models, but all non-affected labels (and after finishing the edit, all labels)
    ///     use a <see cref="FreeLabelModel"/>.
    ///   </para>
    /// </remarks>
    private void UpdateLabelModelBakingSetting(IGraph graph) {
      ILabelModel bakedNodeLabelModel;
      ILabelModel bakedEdgeLabelModel;
      ILabelModel bakedPortLabelModel;
      if (PerformanceSettings.LabelModelBakingEnabled) {
        bakedNodeLabelModel = FreeLabelModel.Instance;
        bakedEdgeLabelModel = FreeLabelModel.Instance;
        bakedPortLabelModel = FreeLabelModel.Instance;
      } else {
        bakedNodeLabelModel = new InteriorNodeLabelModel();
        bakedEdgeLabelModel = new EdgeSegmentLabelModel();
        bakedPortLabelModel = new ExteriorNodeLabelModel();
      }

      foreach (var l in graph.Labels) {
        ILabelModel bakedLabelModel = null;
        if (l.Owner is INode) {
          bakedLabelModel = bakedNodeLabelModel;
        } else if (l.Owner is IEdge) {
          bakedLabelModel = bakedEdgeLabelModel;
        } else if (l.Owner is IPort) {
          bakedLabelModel = bakedPortLabelModel;
        }
        var finder = bakedLabelModel?.GetContext(l).Lookup<ILabelModelParameterFinder>();
        if (finder != null) {
          var param = finder.FindBestParameter(l.GetLayout());
          graph.SetLabelLayoutParameter(l, param);
        }
      }
    }

    #endregion

    /// <summary>
    ///   Returns a random node from the graph.
    /// </summary>
    /// <returns>A random node from the graph.</returns>
    private INode GetRandomNode() {
      var nodes = graphControl.Graph.Nodes.ToList();
      var node = nodes[rnd.Next(nodes.Count)];
      return node;
    }

    /// <summary>
    ///   De-selects all elements and re-selects them again.
    /// </summary>
    /// <remarks>This is needed to update the visuals for the handles or selection decoration.</remarks>
    private void RefreshSelection() {
      var oldSelection = graphControl.Selection.ToList();
      graphControl.Selection.Clear();
      foreach (var item in oldSelection) {
        graphControl.Selection.Add(item);
      }
    }

    #endregion
  }

  /// <summary>
  ///   Simple class to model the properties of the supplied sample graphs.
  /// </summary>
  /// <seealso cref="LargeGraphsDemo.InitializeGraphChooserBox"/>
  public sealed class SampleGraphInfo
  {
    /// <summary>
    ///   Gets or sets the type of the graph.
    /// </summary>
    /// <remarks>This is either "Tree" or "Hierarchical".</remarks>
    public string Type { get; set; }

    /// <summary>
    ///   Gets or sets the file name of the <c>.graphmlz</c> file.
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    ///   Gets or sets the display name to show in the combo box.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///   Gets or sets the number of nodes in the graph.
    /// </summary>
    public int NodeCount { get; set; }

    /// <summary>
    ///   Gets or sets the number of edges in the graph.
    /// </summary>
    public int EdgeCount { get; set; }
  }

  /// <summary>
  ///   Negates a boolean value in both binding directions.
  /// </summary>
  public sealed class BooleanNegationConverter : IValueConverter
  {
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool flag ? !flag : DependencyProperty.UnsetValue;

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Convert(value, targetType, parameter, culture);
  }

  /// <summary>
  ///   Value converter to convert a number between 0 and 1 to a color between green and red.
  /// </summary>
  /// <remarks>
  ///   The parameter can be used to pass a scaling factor. If the scaling factor is negative, the color range is
  ///   inverted.
  /// </remarks>
  public sealed class GradientConverter : IValueConverter
  {
    /// <summary>The list of colors</summary>
    private readonly List<Color> colors = [Colors.LimeGreen, Colors.Orange, Colors.Red];

    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var param = System.Convert.ToSingle(parameter, CultureInfo.InvariantCulture);
      var val = System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
      // Perform optional reversal when the parameter is negative and scale the value
      var val2 = param < 0 ? 1 - (val / -param) : val / param;
      return new SolidColorBrush(GetInterpolatedColor(val2));
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        null; // Unsupported

    /// <summary>
    ///   Gets an interpolated color at the given position within the color list.
    /// </summary>
    /// <param name="position">A position in the color list.</param>
    /// <returns>
    ///   The color at the given position in the list. If fractional, an interpolated color is calculated between the
    ///   adjacent color values and returned.
    /// </returns>
    private Color GetInterpolatedColor(float position) {
      if (position <= 0) {
        return colors[0];
      }
      if (position >= 1) {
        return colors[colors.Count - 1];
      }
      if (colors.Count == 1) {
        return colors[0];
      }

      var index = (colors.Count - 1) * position;
      var left = colors[(int) Math.Floor(index)];
      var right = colors[(int) Math.Ceiling(index)];
      var between = index - (float) Math.Truncate(index);

      Func<float, float, float> adj = (a, b) => a + (b - a) * between;

      return new Color {
          ScA = adj(left.ScA, right.ScA),
          ScR = adj(left.ScR, right.ScR),
          ScG = adj(left.ScG, right.ScG),
          ScB = adj(left.ScB, right.ScB)
      };
    }
  }
}
