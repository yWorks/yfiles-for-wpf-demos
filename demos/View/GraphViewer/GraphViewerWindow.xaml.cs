/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Viewer
{
  /// <summary>
  /// This demo shows how to display a graph with the GraphViewer component.
  /// </summary>
  public partial class GraphViewerWindow
  {

    private FoldingManager manager;

    public GraphViewerWindow() {
      InitializeComponent();
    }

    public void OnLoaded(object source, EventArgs args) {
      EnableFolding();

      InitializeHighlightStyles();

      InitializeInputMode();

      IGraph graph = graphControl.Graph;

      graphControl.FileOperationsEnabled = true;

      IMapperRegistry masterRegistry = graph.GetFoldingView().Manager.MasterGraph.MapperRegistry;
      masterRegistry.CreateWeakMapper<INode, string>("ToolTip");
      masterRegistry.CreateWeakMapper<INode, string>("Description");
      masterRegistry.CreateWeakMapper<INode, string>("Url");
      masterRegistry.CreateWeakMapper<IGraph, string>("GraphDescription");

      graphControl.CurrentItemChanged += OnCurrentItemChanged;

      var ioHandler = graphControl.GraphMLIOHandler;

      ioHandler.AddGraphOutputData("GraphDescription", (g) => graphDescriptionTextBlock.Text);
      ioHandler.AddGraphInputData<string>("GraphDescription", (g, v) => graphDescriptionTextBlock.Text = v);

      ioHandler.AddRegistryInputMapper<INode, string>("Description");
      ioHandler.AddRegistryOutputMapper<INode, string>("Description", "Description");
      ioHandler.AddRegistryInputMapper<INode, string>("ToolTip");
      ioHandler.AddRegistryOutputMapper<INode, string>("ToolTip", "ToolTip");
      ioHandler.AddRegistryInputMapper<INode, string>("Url");
      ioHandler.AddRegistryOutputMapper<INode, string>("Url", "Url");
      
      graphChooserBox.ItemsSource = new[]
                                      {
                                        "computer-network", "movies", "family-tree", "hierarchy", "nesting",
                                        "social-network", "uml-diagram", "large-tree",
                                      };
      graphChooserBox.SelectedIndex = 0;
      graphControl.FitGraphBounds();
    }

    private void InitializeHighlightStyles() {
      // we want to create a non-default nice highlight styling
      // for the hover highlight, create semi transparent orange stroke first
      var orangeRed = Colors.OrangeRed;
      var orangePen = new Pen(new SolidColorBrush(Color.FromArgb(220, orangeRed.R, orangeRed.G, orangeRed.B)), 3);
      // freeze it for slightly improved performance
      orangePen.Freeze();

      // now decorate the nodes and edges with custom hover highlight styles
      var decorator = graphControl.Graph.GetDecorator();

      // nodes should be given a rectangular orange rectangle highlight shape
      var highlightShape = new ShapeNodeStyle {
          Shape = ShapeNodeShape.RoundRectangle,
          Pen = orangePen,
          Brush = null
      };

      var nodeStyleHighlight = new NodeStyleDecorationInstaller {
          NodeStyle = highlightShape,
          // that should be slightly larger than the real node
          Margins = new InsetsD(5),
          // but have a fixed size in the view coordinates
          ZoomPolicy = StyleDecorationZoomPolicy.ViewCoordinates
      };

      // register it as the default implementation for all nodes
      decorator.NodeDecorator.HighlightDecorator.SetImplementation(nodeStyleHighlight);

      // a similar style for the edges, however cropped by the highlight's insets
      var dummyCroppingArrow = new Arrow {
          Type = ArrowType.None,
          CropLength = 5
      };
      var edgeStyle = new PolylineEdgeStyle {
          Pen = orangePen,
          TargetArrow = dummyCroppingArrow,
          SourceArrow = dummyCroppingArrow
      };
      var edgeStyleHighlight = new EdgeStyleDecorationInstaller {
          EdgeStyle = edgeStyle,
          ZoomPolicy = StyleDecorationZoomPolicy.ViewCoordinates
      };
      decorator.EdgeDecorator.HighlightDecorator.SetImplementation(edgeStyleHighlight);
    }

    private void InitializeInputMode() {
      // we have a viewer application, so we can use the GraphViewerInputMode
      // -enable support for: tooltips on nodes and edges
      // -clicking on nodes
      // -focusing (via keyboard navigation) of nodes
      // -no selection
      // -no marquee
      var graphViewerInputMode = new GraphViewerInputMode
      {
          ToolTipItems = GraphItemTypes.LabelOwner,
          ClickableItems = GraphItemTypes.Node,
          FocusableItems = GraphItemTypes.Node,
          SelectableItems = GraphItemTypes.None,
          MarqueeSelectableItems = GraphItemTypes.None
      };

      // we want to enable the user to collapse and expand groups interactively, even though we
      // are just a "viewer" application
      graphViewerInputMode.NavigationInputMode.AllowCollapseGroup = true;
      graphViewerInputMode.NavigationInputMode.AllowExpandGroup = true;
      // after expand/collapse/enter/exit operations - perform a fitContent operation to adjust
      // reachable area.
      graphViewerInputMode.NavigationInputMode.FitContentAfterGroupActions = true;
      // we don't have selection enabled and thus the commands should use the "currentItem"
      // property instead - this property is changed when clicking on items or navigating via
      // the keyboard.
      graphViewerInputMode.NavigationInputMode.UseCurrentItemForCommands = true;

      // we want to get reports of the mouse being hovered over nodes and edges
      // first enable queries
      graphViewerInputMode.ItemHoverInputMode.Enabled = true;
      // set the items to be reported
      graphViewerInputMode.ItemHoverInputMode.HoverItems = GraphItemTypes.Edge | GraphItemTypes.Node;
      // if there are other items (most importantly labels) in front of edges or nodes
      // they should be discarded, rather than be reported as "null"
      graphViewerInputMode.ItemHoverInputMode.DiscardInvalidItems = false;
      // whenever the currently hovered item changes call our method
      graphViewerInputMode.ItemHoverInputMode.HoveredItemChanged += OnHoveredItemChanged;

      // when the mouse hovers for a longer time over an item we may optionally display a
      // tooltip. Use this callback for querying the tooltip contents.
      graphViewerInputMode.QueryItemToolTip += OnQueryItemToolTip;

      // if we click on an item we want to perform a custom action, so register a callback
      graphViewerInputMode.ItemClicked += OnItemClicked;

      // also if someone clicked on an empty area we want to perform a custom group action
      graphViewerInputMode.ClickInputMode.Clicked += OnClickInputModeOnClicked;

      graphControl.InputMode = graphViewerInputMode;
    }

    private void OnHoveredItemChanged(object sender, HoveredItemChangedEventArgs e) {
      // we use the highlight manager of the GraphComponent to highlight related items
      var manager = graphControl.HighlightIndicatorManager;

      // first remove previous highlights
      manager.ClearHighlights();
      // then see where we are hovering over, now
      var newItem = e.Item;
      if (newItem != null) {
        // we highlight the item itself
        manager.AddHighlight(newItem);
        var node = newItem as INode;
        var edge = newItem as IEdge;
        if (node != null) {
          // and if it's a node, we highlight all adjacent edges, too
          foreach (var adjacentEdge in graphControl.Graph.EdgesAt(node)) {
            manager.AddHighlight(adjacentEdge);
          }
        } else if (edge != null) {
          // if it's an edge - we highlight the adjacent nodes
          manager.AddHighlight(edge.GetSourceNode());
          manager.AddHighlight(edge.GetTargetNode());
        }
      }
    }

    private void OnClickInputModeOnClicked(object sender, ClickEventArgs args) {
      if (!graphControl.GraphModelManager.HitElementsAt(args.Location).Any()) { // nothing hit
        if ((args.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == (ModifierKeys.Shift | ModifierKeys.Control)) {
          if (GraphCommands.ExitGroup.CanExecute(null, graphControl) && !args.Handled) {
            GraphCommands.ExitGroup.Execute(null, graphControl);
            args.Handled = true;
          }
        }
      }
    }

    /// <summary>
    /// Enable folding - change the GraphControls graph to a managed view
    /// that provides the actual collapse/expand state.
    /// </summary>
    private void EnableFolding() {
      // create the manager
      manager = new FoldingManager();
      DemoStyles.InitDemoStyles(manager.MasterGraph, foldingEnabled: true);
      // replace the displayed graph with a managed view
      graphControl.Graph = manager.CreateFoldingView().Graph;
    }


    private void OnCurrentItemChanged(object sender, EventArgs propertyChangedEventArgs) {
      var currentItem = graphControl.CurrentItem;
      if (currentItem is INode) {
        var node = (INode)currentItem;
        nodeDescriptionTextBlock.Text = DescriptionMapper[node] ?? string.Empty;
        nodeLabelTextBlock.Text = node.Labels.Count > 0 ? node.Labels[0].Text : string.Empty;
        var url = UrlMapper[node];
        if (url != null) {
          nodeUrlButton.Content = url;
          nodeUrlButton.Tag = url;
          nodeUrlButton.IsEnabled = true;
        } else {
          nodeUrlButton.Content = "";
          nodeUrlButton.IsEnabled = false;
        }
      }
      else {
        nodeDescriptionTextBlock.Text = "";
        nodeLabelTextBlock.Text = "";
        nodeUrlButton.Content = null;
        nodeUrlButton.Tag = null;
        nodeUrlButton.IsEnabled = false;
      }
    }

    private void OnItemClicked(object sender, ItemClickedEventArgs<IModelItem> e) {
      if (e.Item is INode) {
        graphControl.CurrentItem = e.Item;
        if ((LastModifierKeys & (ModifierKeys.Shift | ModifierKeys.Control)) == (ModifierKeys.Shift | ModifierKeys.Control)) {
          if (GraphCommands.EnterGroup.CanExecute(e.Item, graphControl)) {
            GraphCommands.EnterGroup.Execute(e.Item, graphControl);
            e.Handled = true;
          }
        } else if ((LastModifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift) {
          var url = UrlMapper[(INode)e.Item];
          var descriptionWindow = new DescriptionWindow();
          if (url != null) {
            descriptionWindow.Uri = new Uri(url);
            descriptionWindow.Description = DescriptionMapper[(INode)e.Item];
            descriptionWindow.ShowDialog();
            e.Handled = true;
          }
        }
      }
    }

    private ModifierKeys LastModifierKeys {
      get {
        var mouse2DEventArgs = graphControl.LastInputEvent as Mouse2DEventArgs;
        if (mouse2DEventArgs != null) {
          return mouse2DEventArgs.Modifiers;
        }
        var touch2DEventArgs = graphControl.LastInputEvent as Touch2DEventArgs;
        if (touch2DEventArgs != null) {
          return touch2DEventArgs.Modifiers;
        }
        return ModifierKeys.None;
      }
    }

    private void OnQueryItemToolTip(object sender, QueryItemToolTipEventArgs<IModelItem> queryItemToolTipEventArgs) {
      if (queryItemToolTipEventArgs.Item is INode && !queryItemToolTipEventArgs.Handled) {
        INode node = (INode)queryItemToolTipEventArgs.Item;
        IMapper<INode, string> descriptionMapper = DescriptionMapper;
        var toolTip = ToolTipMapper[node] ?? (descriptionMapper != null ? descriptionMapper[node] : null);
        if (toolTip != null) {
          queryItemToolTipEventArgs.ToolTip = toolTip;
          queryItemToolTipEventArgs.Handled = true;
        }
      }
    }

    private IMapper<INode, string> DescriptionMapper {
      get { return graphControl.Graph.MapperRegistry.GetMapper<INode, string>("Description"); }
    }
    private IMapper<INode, string> ToolTipMapper {
      get { return graphControl.Graph.MapperRegistry.GetMapper<INode, string>("ToolTip"); }
    }
    private IMapper<INode, string> UrlMapper {
      get { return graphControl.Graph.MapperRegistry.GetMapper<INode, string>("Url"); }
    }


    private void ReadSampleGraph() {
      string fileName = string.Format("Resources" + Path.DirectorySeparatorChar + "{0}.graphml", graphChooserBox.SelectedItem.ToString());
      graphControl.Graph.Clear();
      using (var reader = new StreamReader(fileName)) {
        graphControl.ImportFromGraphML(reader);
      }
      graphControl.FitGraphBounds();
    }

    private void UpdateButtons() {
      nextButton.IsEnabled = graphChooserBox.SelectedIndex < graphChooserBox.Items.Count - 1;
      previousButton.IsEnabled = graphChooserBox.SelectedIndex > 0;
    }

    private void previousButton_Click(object sender, EventArgs e) {
      graphChooserBox.SelectedIndex--;
      UpdateButtons();
    }

    private void nextButton_Click(object sender, EventArgs e) {
      graphChooserBox.SelectedIndex++;
      UpdateButtons();
    }

    private void graphChooserBox_SelectedIndexChanged(object sender, EventArgs e) {
      ReadSampleGraph();
      UpdateButtons();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    private void nodeUrlButton_LinkClicked(object sender, EventArgs e) {
      // Open the link...
      var startInfo = new ProcessStartInfo { FileName = nodeUrlButton.Tag.ToString(), UseShellExecute = true };
      Process.Start(startInfo);
    }
  }

  [ValueConversion(typeof(double), typeof(double))]
  public class ZoomConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double v = (double)value;
      return Math.Log(v, 1.2);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return Math.Pow(1.2, (double)value);
    }
  }
}
