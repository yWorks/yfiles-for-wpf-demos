/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Demo.yFiles.Layout.Tree.Configuration;
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Handler;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree
{
  /// <summary>
  /// Interaction logic for NodePlacerPanel.xaml
  /// </summary>
  public partial class NodePlacerPanel
  {
    private readonly IDictionary<string, NodePlacerDescriptor> levelConfigurations =
      new Dictionary<string, NodePlacerDescriptor>();

    private readonly IList<NodePlacerDescriptor> nodePlacers = new List<NodePlacerDescriptor>();
    private List<INodePlacerConfiguration> descriptorCollection;

    private TreeLayout previewLayout;
    private DefaultSelectionProvider<INodePlacerConfiguration> selectionProvider;

    public NodePlacerPanel() {
      InitializeComponent();
      nodePlacerTypeComboBox.Items.Add(NodePlacerConfigurations.DefaultNodePlacer.Name);
      nodePlacerTypeComboBox.Items.Add(NodePlacerConfigurations.SimpleNodePlacer.Name);
      nodePlacerTypeComboBox.Items.Add(NodePlacerConfigurations.BusPlacer.Name);
      nodePlacerTypeComboBox.Items.Add(NodePlacerConfigurations.DoubleLinePlacer.Name);
      nodePlacerTypeComboBox.Items.Add(NodePlacerConfigurations.LeftRightPlacer.Name);
      nodePlacerTypeComboBox.Items.Add(NodePlacerConfigurations.ARNodePlacer.Name);
      nodePlacerTypeComboBox.Items.Add(NodePlacerConfigurations.AssistantPlacer.Name);
      nodePlacerTypeComboBox.Items.Add(NodePlacerConfigurations.None.Name);

      SetupNodePlacerOptions();
      SetupPreview();
    }

    public IList<NodePlacerDescriptor> NodePlacers {
      get { return nodePlacers; }
    }

    #region UI event handlers that connect to the outside world

    /// <summary>
    /// Raises the <see cref="ReloadConfiguration"/> event.
    /// </summary>
    protected virtual void OnReloadConfiguration(EventArgs args) {
      if (ReloadConfiguration != null) {
        ReloadConfiguration(this, args);
      }
    }

    /// <summary>
    /// Occurs when a request to reload the configuration is issued.
    /// </summary>
    public event EventHandler ReloadConfiguration;

    /// <summary>
    /// Raises the <see cref="ApplyConfiguration"/> event.
    /// </summary>
    protected virtual void OnApplyConfiguration(EventArgs args) {
      if (ApplyConfiguration != null) {
        ApplyConfiguration(this, args);
      }
    }

    /// <summary>
    /// Occurs when a request to Apply the configuration is issued.
    /// </summary>
    public event EventHandler ApplyConfiguration;

    private void OnReloadButtonClicked(object sender, RoutedEventArgs e) {
      OnReloadConfiguration(e);
      levelConfigurations.Clear();
      //Force update of current property
      SetLevel(Level);
    }

    private void OnApplyButtonClicked(object sender, RoutedEventArgs e) {
      OnApplyConfiguration(e);
    }

    #endregion

    #region Level dependency property

    public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
      "Level", typeof (int), typeof (NodePlacerPanel), new FrameworkPropertyMetadata(0, OnLevelChanged, CoerceLevel));

    public int Level {
      get { return (int) GetValue(LevelProperty); }
      set { SetValue(LevelProperty, value); }
    }

    private static object CoerceLevel(DependencyObject d, object basevalue) {
      var level = (int) basevalue;
      var npp = (NodePlacerPanel) d;
      if (level < 0) {
        return 0;
      }
      if (level >= npp.NodePlacers.Count) {
        return npp.NodePlacers.Count - 1;
      }
      return level;
    }

    private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var npp = (NodePlacerPanel) d;
      npp.SetLevel((int) e.NewValue);
    }

    internal void SetLevel(int level) {
      levelConfigurations.Clear();
      Level = level;
      CurrentDescriptor = NodePlacers[level];
      layerVisualizationBorder.Background = CurrentBrush;
      nodePlacerTypeComboBox.SelectedItem = CurrentDescriptor.Name;
    }

    #endregion

    #region CurrentDescriptor dependency property

    public static readonly DependencyProperty CurrentDescriptorProperty = DependencyProperty.Register(
      "CurrentDescriptor", typeof (NodePlacerDescriptor), typeof (NodePlacerPanel),
      new FrameworkPropertyMetadata(OnDescriptorChanged));

    public NodePlacerDescriptor CurrentDescriptor {
      get { return (NodePlacerDescriptor) GetValue(CurrentDescriptorProperty); }
      set { SetValue(CurrentDescriptorProperty, value); }
    }

    private static void OnDescriptorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var npp = d as NodePlacerPanel;
      if (npp != null) {
        npp.SetDescriptor((NodePlacerDescriptor) e.NewValue);
      }
    }

    private void SetDescriptor(NodePlacerDescriptor npd) {
      nodePlacers[Level] = npd;
      if (descriptorCollection.Count == 0) {
        descriptorCollection.Add(npd.Configuration);
      } else {
        descriptorCollection[0] = npd.Configuration;
      }
      rotationGrid.IsEnabled = npd.Rotatable;
      if (npd.Configuration != null) {
        selectionProvider.UpdatePropertyViewsNow();
        editorControl.Visibility = Visibility.Visible;
      } else {
        editorControl.Visibility = Visibility.Hidden;
      }
      UpdatePreview();
    }

    #endregion

    #region Option handler setup

    /// <summary>
    /// Configure option handler related stuff
    /// </summary>
    private void SetupNodePlacerOptions() {
      descriptorCollection = new List<INodePlacerConfiguration>();
      selectionProvider = new DefaultSelectionProvider<INodePlacerConfiguration>(descriptorCollection,
                                                                                 delegate { return true; })
                            {
                              ContextLookup = Lookups.CreateContextLookupChainLink(
                                (subject, type) =>
                                ((type == typeof (IPropertyMapBuilder) && (subject is INodePlacerConfiguration))
                                   ? new AttributeBasedPropertyMapBuilderAttribute().CreateBuilder(subject.GetType())
                                   : null))
                            };
      //when the selection content changes, trigger this action (usually rebuild associated option handlers)
      selectionProvider.SelectedItemsChanged += SelectionProviderSelectionChanged;

      editorControl.OptionHandler = new OptionHandler("NodePlacerOptions");
      editorControl.IsAutoAdopt = true;
      editorControl.IsAutoCommit = true;
      selectionProvider.PropertyItemsChanged += delegate {
                                                  UpdatePreview();
                                                  SelectionProviderSelectionChanged(this, null);
                                                };
    }


    /// <summary>
    /// Update the properties when the current node placer descriptor has changed.
    /// </summary>
    private void SelectionProviderSelectionChanged(object sender, EventArgs e) {
      //We just rebuild the option handler from scratch
      editorControl.OptionHandler.BuildFromSelection(selectionProvider,
                                                     Lookups.CreateContextLookupChainLink(
                                                       (subject, type) =>
                                                       ((type == typeof (IOptionBuilder) && subject is INodePlacerConfiguration)? new SortableOptionBuilder() : null)));
    }

    #endregion

    #region Preview configuration and update

    /// <summary>
    /// Creates and configures the graph that is used for the placer configuration preview.
    /// </summary>
    private void SetupPreview() {
      IGraph graph = previewControl.Graph;
      DictionaryMapper<INode, bool> assistantMap =
        graph.MapperRegistry.CreateMapper<INode, bool>(AssistantNodePlacer.AssistantNodeDpKey);
      graph.NodeDefaults.Size = new SizeD(40, 30);
      graph.NodeDefaults.Style = new ShinyPlateNodeStyle
      {
        Brush = Brushes.LightGray,
        Insets = new InsetsD(5),
        DrawShadow = false,
        Pen = Pens.Black
      };
      var rootStyle = new ShinyPlateNodeStyle
      {
        Brush = Brushes.Red,
        Insets = new InsetsD(5),
        Pen = Pens.Black,
        DrawShadow = false
      };
      var assistantStyle = new ShinyPlateNodeStyle
      {
        Brush = Brushes.LightGray,
        Insets = new InsetsD(5),
        Pen = new Pen(Brushes.Black, 1) {DashStyle = DashStyles.Dash},
        DrawShadow = false
      };
      INode root = graph.CreateNode();
      graph.SetStyle(root, rootStyle);
      INode n1 = graph.CreateNode();
      graph.SetStyle(n1, assistantStyle);
      assistantMap[n1] = true;
      INode n2 = graph.CreateNode();
      INode n3 = graph.CreateNode();
      INode n4 = graph.CreateNode();
      INode n5 = graph.CreateNode(new RectD(0, 0, 60, 30));
      graph.CreateEdge(root, n1);
      graph.CreateEdge(root, n2);
      graph.CreateEdge(root, n3);
      graph.CreateEdge(root, n4);
      graph.CreateEdge(root, n5);
      previewLayout = new TreeLayout();
    }

    ///<summary>
    /// Update the preview canvas: Apply a new layout with the current placer.
    ///</summary>
    public void UpdatePreview() {
      INodePlacer placer = CurrentDescriptor.Configuration != null
                             ? CurrentDescriptor.Configuration.CreateNodePlacer()
                             : null;
      previewLayout.DefaultNodePlacer = placer ?? new DefaultNodePlacer();
      try {
        previewControl.Graph.ApplyLayout(previewLayout);
        previewControl.FitGraphBounds();
      } catch (Exception) {}
    }

    #endregion

    /// <summary>
    /// Switch the options whenever a new node placer type is selected
    /// </summary>
    /// <remarks>If we didn't change the level, try to retrieve a previously set configuration, otherwise, start from scratch</remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NodePlacerComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
      var name = nodePlacerTypeComboBox.SelectedItem as string;
      if (name != null) {
        if (CurrentDescriptor.Name != (string) nodePlacerTypeComboBox.SelectedItem) {
          //Check if we already have a configuration for this level...
          NodePlacerDescriptor desc;
          if (levelConfigurations.TryGetValue(name, out desc)) {
            CurrentDescriptor = desc;
          } else {
            //Load from descriptor configurations... we use reflection
            foreach (
              PropertyInfo propertyInfo in
                typeof (NodePlacerConfigurations).GetProperties(BindingFlags.Static | BindingFlags.Public)) {
              var d = propertyInfo.GetValue(null, null) as NodePlacerDescriptor;
              if (d != null && d.Name == name) {
                CurrentDescriptor = d;
                levelConfigurations[name] = d;
                break;
              }
            }
          }
        }
      }
    }

    private Brush CurrentBrush {
      get {
        Brush layerBrush = LayerBrushes[Level%LayerBrushes.Length];
        return layerBrush;
      }
    }


    public static readonly Brush[] LayerBrushes = {
                                                    Brushes.Red,
                                                    new SolidColorBrush(Color.FromRgb(255, 128, 0)),
                                                    new SolidColorBrush(Color.FromRgb(224, 224, 0)),
                                                    new SolidColorBrush(Color.FromRgb(64, 208, 64)),
                                                    new SolidColorBrush(Color.FromRgb(0, 255, 255)),
                                                    Brushes.Blue
                                                  };

    #region Rotation Buttons

    private void LeftRotateButtonClicked(object sender, RoutedEventArgs e) {
      var rotablePlacer = CurrentDescriptor.Configuration as IRotatableNodePlacerConfiguration;
      if (rotablePlacer != null) {
        //Rotate the placer 90 degrees counter clockwise - we modify the modification matrix of the placer configuration
        rotablePlacer.SetModificationMatrix(
          rotablePlacer.ModificationMatrix.Multiply(RotatableNodePlacerBase.Matrix.Rot90));
        UpdatePreview();
      }
    }

    private void RightRotateButtonClicked(object sender, RoutedEventArgs e) {
      var rotablePlacer = CurrentDescriptor.Configuration as IRotatableNodePlacerConfiguration;
      if (rotablePlacer != null) {
        //Rotate the placer 90 degrees clockwise - we modify the modification matrix of the placer configuration
        rotablePlacer.SetModificationMatrix(
          rotablePlacer.ModificationMatrix.Multiply(RotatableNodePlacerBase.Matrix.Rot270));
        UpdatePreview();
      }
    }

    private void MirrorHorizButtonClicked(object sender, RoutedEventArgs e) {
      var rotablePlacer = CurrentDescriptor.Configuration as IRotatableNodePlacerConfiguration;
      if (rotablePlacer != null) {
        //Mirror the placer horizontally - we modify the modification matrix of the placer configuration
        rotablePlacer.SetModificationMatrix(
          rotablePlacer.ModificationMatrix.Multiply(RotatableNodePlacerBase.Matrix.MirHor));
        UpdatePreview();
      }
    }

    private void MirrorVertButtonClicked(object sender, RoutedEventArgs e) {
      var rotablePlacer = CurrentDescriptor.Configuration as IRotatableNodePlacerConfiguration;
      if (rotablePlacer != null) {
        //Mirror the placer vertically - we modify the modification matrix of the placer configuration
        rotablePlacer.SetModificationMatrix(
          rotablePlacer.ModificationMatrix.Multiply(RotatableNodePlacerBase.Matrix.MirVert));
        UpdatePreview();
      }
    }

    #endregion
  }

  /// <summary>
  /// Convert a <see langword="null"/> value into the hidden visibility of an <see cref="UIElement"/>.
  /// </summary>
  /// <remarks>This is used to completely hide the configuration panels for the <see cref="NodePlacerConfigurations.None"/> configuration.</remarks>
  internal class DescriptorVisibilityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value == null || value is NonePlacerConfiguration ? Visibility.Hidden : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Converts the internal 0-based index to/from a 1-based 
  /// </summary>
  internal class LevelConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      int i = (int) value;
      return i + 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      string s = (string) value;
      int i = Int32.Parse(s);
      return i - 1;
    }
  }

  /// <summary>
  /// Custom option builder that allows to reorder the displayed properties based on an additional sort criterion (i.e. not alphabetically)
  /// </summary>
  internal class SortableOptionBuilder : AttributeBasedOptionBuilder
  {
    protected override PropertyInfo[] SortProperties(IOptionBuilderContext context, PropertyInfo[] properties) {
      Array.Sort(properties, new MyPropertyInfoComparer());
      return properties;
    }

    #region Nested type: MyPropertyInfoComparer

    private class MyPropertyInfoComparer : IComparer<PropertyInfo>
    {
      #region IComparer<PropertyInfo> Members

      public int Compare(PropertyInfo x1, PropertyInfo x2) {
        var oia1 =
          GetAttributes<OptionItemAttributeAttribute>(x1).FirstOrDefault(
            attr => ((OptionItemAttributeAttribute)attr).Name == "OptionItem.Index") as
          OptionItemAttributeAttribute;
        var oia2 =
          GetAttributes<OptionItemAttributeAttribute>(x2).FirstOrDefault(
            attr => ((OptionItemAttributeAttribute)attr).Name == "OptionItem.Index") as
          OptionItemAttributeAttribute;

        if (oia1 != null && oia2 != null) {
          return ((int) oia1.Value).CompareTo((int) oia2.Value);
        }
        string s1 = x1.Name;
        string s2 = x2.Name;
        return s1.CompareTo(s2);
      }

      #endregion

      private static IEnumerable<Attribute> GetAttributes<T>(PropertyInfo info) where T : Attribute {
        return Attribute.GetCustomAttributes(info, typeof (T));
      }
    }

    #endregion
  }
}