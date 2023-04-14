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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace SankeyLayout
{
  /// <summary>
  /// Demonstrates how yFiles can be used to visualize a Sankey diagram.
  /// </summary>

  public partial class SankeyWindow
  {
    private Demo.yFiles.Layout.Sankey.SankeyLayout sankeyLayout;
    private bool inLayout;

    public SankeyWindow() {
      InitializeComponent();
    }

    private async void OnLoaded(object sender, EventArgs e) {
      InitializeGraph();
      CreateSampleGraph();
      await RunLayout();
      CreateInputMode();
      graphControl.Graph.GetUndoEngine().Clear();
    }

    private void InitializeGraph() {
      var graph = graphControl.Graph;
      graph.NodeDefaults.Style = new ShapeNodeStyle
      {
        Brush = (Brush) new SolidColorBrush(Color.FromRgb(0x42, 0x81, 0xA4)).GetAsFrozen(),
        Pen = null
      };
      graph.EdgeDefaults.Style = new BezierEdgeStyle(new DemoEdgeStyleRenderer(graphControl.Selection, false));

      // use a label model that stretches the label over the full node layout, with small insets
      var centerLabelModel = new InteriorStretchLabelModel { Insets = new InsetsD(3) };
      graph.NodeDefaults.Labels.LayoutParameter = centerLabelModel.CreateParameter(InteriorStretchLabelModel.Position.Center);

      // set the default style for the node labels
      graph.NodeDefaults.Labels.Style = new DefaultLabelStyle
      {
        TextBrush = Brushes.White,
        TextWrapping = TextWrapping.Wrap,
        VerticalTextAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Center
      };

      // set the default node size
      graph.NodeDefaults.Size = new SizeD(80, 30);

      // add a node tag listener to change the node brush when the tag changes
      graph.NodeTagChanged += (sender, e) => {
        var item = e.Item;
        var tag = (CustomTag) item.Tag;
        if (item.Tag != null && e.OldValue != null && tag.brush != ((CustomTag)e.OldValue).brush) {
          ((ShapeNodeStyle) item.Style).Brush = ((CustomTag)item.Tag).brush; //GetNodeColor(item);
          graphControl.Invalidate();
        }
      };

      // enable the undo engine
      graph.SetUndoEngineEnabled(true);

      // disable all edge handles
      var edgeDecorator = graph.GetDecorator().EdgeDecorator;
      edgeDecorator.HandleProviderDecorator.HideImplementation();
      edgeDecorator.SelectionDecorator.HideImplementation();
      edgeDecorator.BendCreatorDecorator.HideImplementation();
      edgeDecorator.PositionHandlerDecorator.HideImplementation();
      graph.GetDecorator().BendDecorator.HandleProviderDecorator.HideImplementation();

      // allow nodes to be only moved vertically 
      graph.GetDecorator().NodeDecorator.PositionHandlerDecorator.SetImplementationWrapper((node, handler) => new HorizontallyConstrainedPositionHandler(handler));

      graphControl.HighlightIndicatorManager = new HighlightManager();
    }

    private void CreateInputMode() {
      // initialize input mode
      var mode = new GraphEditorInputMode
      {
        SelectableItems = GraphItemTypes.None,
        DeletableItems = GraphItemTypes.None,
        AllowCreateEdge = false,
        AllowCreateNode = false,
        MovableItems = GraphItemTypes.Node,
        MoveUnselectedInputMode = { Enabled = true, Priority = 38 },
        MoveInputMode = { Enabled = false },
        MarqueeSelectionInputMode = { Enabled = false },
        MoveViewportInputMode = { PressedRecognizer = MouseEventRecognizers.LeftPressed }
      };

      // listener to react if a node has been dragged
      mode.MoveUnselectedInputMode.DragFinished += async (sender, args) => {
        await RunLayout();
      };

      // listener to react in edge label text changing
      mode.LabelTextChanged += async (sender, args) => {
        if (args.Item.Owner is IEdge) {
          await OnLabelChanged(args.Item);
        }
      };

      // listener to react in edge label addition
      mode.LabelAdded += async (sender, args) => {
        if (args.Item.Owner is IEdge) {
          await OnLabelChanged(args.Item);
        }
      };

      // listener to react to item deletion
      mode.DeletedSelection += async (sender, e) => {
        // start a compound edit to merge thickness changes and layout
        var compoundEdit = graphControl.Graph.BeginEdit("Deletion", "Deletion");
        NormalizeThickness();
        try {
          await RunLayout();
          compoundEdit.Commit();
        } catch {
          compoundEdit.Cancel();
        }
      };

      mode.ItemHoverInputMode.Enabled = true;
      mode.ItemHoverInputMode.HoverItems = GraphItemTypes.Edge | GraphItemTypes.EdgeLabel;
      mode.ItemHoverInputMode.DiscardInvalidItems = false;
      // add hover listener to implement edge and label highlighting
      mode.ItemHoverInputMode.HoveredItemChanged += (sender, args) => {
        var highlightManager = graphControl.HighlightIndicatorManager;
        highlightManager.ClearHighlights();
        var item = args.Item;
        if (item != null) {
          highlightManager.AddHighlight(item);
          if (item is IEdge) {
            foreach (var label in ((IEdge) item).Labels) {
              highlightManager.AddHighlight(label);
            }
          } else {
            highlightManager.AddHighlight(((ILabel) item).Owner);
          }
        }
      };

      graphControl.InputMode = mode;
    }

    private void SetUiDisabled(bool disabled) {
      layoutButton.IsEnabled = !disabled;
    }

    #region Sample Graph
    private void CreateSampleGraph() {
      var blackParty = CustomTag.NewNodeTag(0, 0, 0);
      var blueParty = CustomTag.NewNodeTag(0x24, 0x22, 0x65);
      var greenParty = CustomTag.NewNodeTag(0x56, 0x92, 0x6E);
      var redParty = CustomTag.NewNodeTag(0xDB, 0x3A, 0x34);
      var purpleParty = CustomTag.NewNodeTag(0x6C, 0x4F, 0x77);
      var yellowParty = CustomTag.NewNodeTag(0xF0, 0xC8, 0x08);
      var nonVoters = CustomTag.NewNodeTag(0x42, 0x81, 0xA4);

      var graph = graphControl.Graph;
      var n0 = graph.CreateNode(new RectD(0, 0, 30, 30), tag: blackParty);
      graph.AddLabel(n0, "Black Party");
      var n1 = graph.CreateNode(new RectD(0, 100, 30, 30), tag: redParty);
      graph.AddLabel(n1, "Red Party");
      var n2 = graph.CreateNode(new RectD(0, 200, 30, 30), tag: yellowParty);
      graph.AddLabel(n2, "Yellow Party");
      var n3 = graph.CreateNode(new RectD(0, 300, 30, 30), tag: greenParty);
      graph.AddLabel(n3, "Green Party");
      var n4 = graph.CreateNode(new RectD(0, 400, 30, 30), tag: purpleParty);
      graph.AddLabel(n4, "Purple Party");
      var n5 = graph.CreateNode(new RectD(100, 0, 30, 30), tag: blackParty);
      graph.AddLabel(n5, "Black Party");
      var n6 = graph.CreateNode(new RectD(100, 100, 30, 30), tag: redParty);
      graph.AddLabel(n6, "Red Party");
      var n7 = graph.CreateNode(new RectD(100, 200, 30, 30), tag: yellowParty);
      graph.AddLabel(n7, "Yellow Party");
      var n8 = graph.CreateNode(new RectD(100, 300, 30, 30), tag: greenParty);
      graph.AddLabel(n8, "Green Party");
      var n9 = graph.CreateNode(new RectD(100, 400, 30, 30), tag: purpleParty);
      graph.AddLabel(n9, "Purple Party");
      var n10 = graph.CreateNode(new RectD(100, 500, 30, 30), tag: nonVoters);
      graph.AddLabel(n10, "Non-voter");
      var n11 = graph.CreateNode(new RectD(200, 0, 30, 30), tag: blackParty);
      graph.AddLabel(n11, "Black Party");
      var n12 = graph.CreateNode(new RectD(200, 100, 30, 30), tag: redParty);
      graph.AddLabel(n12, "Red Party");
      var n13 = graph.CreateNode(new RectD(200, 200, 30, 30), tag: yellowParty);
      graph.AddLabel(n13, "Yellow Party");
      var n14 = graph.CreateNode(new RectD(200, 300, 30, 30), tag: greenParty);
      graph.AddLabel(n14, "Green Party");
      var n15 = graph.CreateNode(new RectD(200, 400, 30, 30), tag: purpleParty);
      graph.AddLabel(n15, "Purple Party");
      var n16 = graph.CreateNode(new RectD(200, 500, 30, 30), tag: blueParty);
      graph.AddLabel(n16, "Blue Party");
      var n17 = graph.CreateNode(new RectD(200, 600, 30, 30), tag: nonVoters);
      graph.AddLabel(n17, "Non-voter");

      var e0 = graph.CreateEdge(n0, n5);
      graph.AddLabel(e0, "13654000");
      var e1 = graph.CreateEdge(n0, n7);
      graph.AddLabel(e1, "1140000");
      var e2 = graph.CreateEdge(n0, n8);
      graph.AddLabel(e2, "50000");
      var e3 = graph.CreateEdge(n0, n9);
      graph.AddLabel(e3, "40000");
      var e4 = graph.CreateEdge(n0, n10);
      graph.AddLabel(e4, "1080000");
      var e5 = graph.CreateEdge(n1, n5);
      graph.AddLabel(e5, "880000");
      var e6 = graph.CreateEdge(n1, n6);
      graph.AddLabel(e6, "9890000");
      var e7 = graph.CreateEdge(n1, n7);
      graph.AddLabel(e7, "530000");
      var e8 = graph.CreateEdge(n1, n8);
      graph.AddLabel(e8, "870000");
      var e9 = graph.CreateEdge(n1, n9);
      graph.AddLabel(e9, "1100000");
      var e10 = graph.CreateEdge(n1, n10);
      graph.AddLabel(e10, "2040000");
      var e11 = graph.CreateEdge(n2, n7);
      graph.AddLabel(e11, "6278000");
      var e12 = graph.CreateEdge(n2, n10);
      graph.AddLabel(e12, "70000");
      var e13 = graph.CreateEdge(n3, n7);
      graph.AddLabel(e13, "30000");
      var e14 = graph.CreateEdge(n3, n8);
      graph.AddLabel(e14, "3681000");
      var e15 = graph.CreateEdge(n3, n9);
      graph.AddLabel(e15, "140000");
      var e16 = graph.CreateEdge(n3, n10);
      graph.AddLabel(e16, "30000");
      var e17 = graph.CreateEdge(n4, n7);
      graph.AddLabel(e17, "20000");
      var e18 = graph.CreateEdge(n4, n9);
      graph.AddLabel(e18, "3837000");
      var e19 = graph.CreateEdge(n4, n10);
      graph.AddLabel(e19, "300000");
      var e20 = graph.CreateEdge(n5, n11);
      graph.AddLabel(e20, "14685000");
      var e21 = graph.CreateEdge(n5, n16);
      graph.AddLabel(e21, "290000");
      var e22 = graph.CreateEdge(n6, n11);
      graph.AddLabel(e22, "210000");
      var e23 = graph.CreateEdge(n6, n12);
      graph.AddLabel(e23, "9755000");
      var e24 = graph.CreateEdge(n6, n16);
      graph.AddLabel(e24, "180000");
      var e25 = graph.CreateEdge(n7, n11);
      graph.AddLabel(e25, "2110000");
      var e26 = graph.CreateEdge(n7, n12);
      graph.AddLabel(e26, "530000");
      var e27 = graph.CreateEdge(n7, n13);
      graph.AddLabel(e27, "2084000");
      var e28 = graph.CreateEdge(n7, n14);
      graph.AddLabel(e28, "170000");
      var e29 = graph.CreateEdge(n7, n15);
      graph.AddLabel(e29, "90000");
      var e30 = graph.CreateEdge(n7, n16);
      graph.AddLabel(e30, "430000");
      var e31 = graph.CreateEdge(n7, n17);
      graph.AddLabel(e31, "460000");
      var e32 = graph.CreateEdge(n8, n11);
      graph.AddLabel(e32, "420000");
      var e33 = graph.CreateEdge(n8, n12);
      graph.AddLabel(e33, "550000");
      var e34 = graph.CreateEdge(n8, n14);
      graph.AddLabel(e34, "3570000");
      var e35 = graph.CreateEdge(n8, n16);
      graph.AddLabel(e35, "90000");
      var e36 = graph.CreateEdge(n8, n17);
      graph.AddLabel(e36, "40000");
      var e37 = graph.CreateEdge(n9, n11);
      graph.AddLabel(e37, "120000");
      var e38 = graph.CreateEdge(n9, n12);
      graph.AddLabel(e38, "370000");
      var e39 = graph.CreateEdge(n9, n14);
      graph.AddLabel(e39, "40000");
      var e40 = graph.CreateEdge(n9, n15);
      graph.AddLabel(e40, "3780000");
      var e41 = graph.CreateEdge(n9, n16);
      graph.AddLabel(e41, "340000");
      var e42 = graph.CreateEdge(n9, n17);
      graph.AddLabel(e42, "320000");
      var e43 = graph.CreateEdge(n10, n11);
      graph.AddLabel(e43, "1130000");
      var e44 = graph.CreateEdge(n10, n12);
      graph.AddLabel(e44, "360000");
      var e45 = graph.CreateEdge(n10, n16);
      graph.AddLabel(e45, "210000");

      // assign node styles
      foreach (var node in graph.Nodes) {
        graph.SetStyle(node, new ShapeNodeStyle {
          Brush = ((CustomTag)node.Tag).brush,
          Pen = null
        });

        foreach (var edge in graph.OutEdgesAt(node)) {
          edge.Tag = CustomTag.NewEdgeTag((CustomTag) node.Tag);
        }
      }

      // assign label styles
      foreach (var label in graph.Edges.SelectMany(e => e.Labels)) {
        SetLabelStyle(label);
      }

      // normalize the edges' thickness and run a new layout
      NormalizeThickness();
    }
    #endregion

    private async Task RunLayout() {
      var graph = graphControl.Graph;
      if (graph.Nodes.Count == 0 || inLayout) {
        return;
      }
      if (sankeyLayout == null) {
        sankeyLayout = new Demo.yFiles.Layout.Sankey.SankeyLayout(graphControl);
      }
      inLayout = true;
      SetUiDisabled(true);

      // configure the layout algorithm
      var fromSketchMode = useDrawingAsSketch.IsChecked == true;
      var hierarchicLayout = sankeyLayout.ConfigureHierarchicLayout(fromSketchMode);
      var hierarchicLayoutData = sankeyLayout.CreateHierarchicLayoutData();

      // run the layout and animate the result
      try {
        await graphControl.MorphLayout(hierarchicLayout, TimeSpan.FromSeconds(1), hierarchicLayoutData);
      } finally {
        SetUiDisabled(false);
        inLayout = false;
      }
    }

    private void RefreshEdgeColor(bool incoming) {
      // assign node styles
      var graph = graphControl.Graph;
      foreach (var node in graph.Nodes) {
        if (incoming) {
          foreach (var edge in graph.OutEdgesAt(node)) {
            edge.Tag = CustomTag.NewEdgeTag((CustomTag) node.Tag);
          }
        } else {
          foreach (var edge in graph.InEdgesAt(node)) {
            edge.Tag = CustomTag.NewEdgeTag((CustomTag) node.Tag);
          }
        }
      }
    }

    private void NormalizeThickness() {
      var min = double.PositiveInfinity;
      var max = double.NegativeInfinity;

      // find the minimum and maximum flow value from the graph's edge labels
      foreach (var label in graphControl.Graph.GetEdgeLabels()) {
        if (double.TryParse(label.Text, out var value)) {
          min = Math.Min(min, value);
          max = Math.Max(max, Math.Abs(value));
        }
      }

      var diff = max - min;
      var largestThickness = 200;
      var smallestThickness = 2;

      // normalize the thickness of the graph's edges
      foreach (var edge in graphControl.Graph.Edges) {
        var labels = edge.Labels;
        var oldTag = (CustomTag) edge.Tag;
        if (labels.Count == 0 || double.IsNaN(diff) || !double.TryParse(labels[0].Text, out _)) {
          edge.Tag = new CustomTag { thickness = 2, brush = oldTag.brush };
        } else {
          var value = Math.Max(0, double.Parse(labels[0].Text));
          var thicknessScale = (largestThickness - smallestThickness) / diff;
          edge.Tag = new CustomTag {
              thickness = Math.Floor(smallestThickness + (value - min) * thicknessScale),
              brush = oldTag.brush
          };
        }
        var tagUndoUnit = new TagUndoUnit("Thickness changed", "Thickness changed", oldTag, edge.Tag, edge);
        graphControl.Graph.GetUndoEngine().AddUnit(tagUndoUnit);
      }
    }

    private void SetLabelStyle(ILabel label) {
      // set the default style for the node labels
      var tag = ((IEdge) label.Owner).GetSourceNode().Tag;
      var brush = ((CustomTag) tag).brush;
      graphControl.Graph.SetStyle(label, new DefaultLabelStyle
      {
        TextBrush = brush
      });
    }

    private async Task OnLabelChanged(ILabel label) {
      var compoundEdit = graphControl.Graph.BeginEdit("Edge Label Text Changed", "Edge Label Text Changed");
      SetLabelStyle(label);
      NormalizeThickness();
      try {
        await RunLayout();
        compoundEdit.Commit();
      } catch {
        compoundEdit.Cancel();
      }
    }

    private async void LayoutButton_OnClick(object sender, RoutedEventArgs e) {
      await RunLayout();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
      var combo = (ComboBox) sender;
      if (graphControl != null && graphControl.IsInitialized) {
        RefreshEdgeColor(combo.SelectedIndex == 1);
        NormalizeThickness();
        graphControl.Invalidate();
      }
    }
  }

  /// <summary>
  /// A custom tag that holds style information
  /// </summary>
  class CustomTag
  {
    public double thickness;

    public SolidColorBrush brush;

    public static CustomTag NewNodeTag(byte r, byte g, byte b) {
      return new CustomTag {
        brush = (SolidColorBrush) new SolidColorBrush(Color.FromRgb(r, g, b)).GetAsFrozen()
      };
    }

    public static CustomTag NewEdgeTag(CustomTag prototype) {
      return new CustomTag {
        thickness = 1,
        brush = (SolidColorBrush) new SolidColorBrush { Color = prototype.brush.Color, Opacity = 0.5 }.GetAsFrozen()
      };
    }
  }
}
