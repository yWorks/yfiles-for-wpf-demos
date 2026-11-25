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

namespace yWorks.Demo.SankeyLayout
{
  /// <summary>
  ///   Demonstrates how yFiles can be used to visualize a Sankey diagram.
  /// </summary>
  [Demo("sankey-diagram", "layout", "Sankey Diagram",
      Summary =
          "A diagram used for visualizing flow information in which the thickness of the edges is proportional to the flow quantity.",
      Tags = ["edge thickness", "style", "layout"],
      Keywords = ["context menu", "hierarchical", "generic labeling", "labels", "data analysis", "popover"])]
  public sealed partial class SankeyLayoutDemo
  {
    private SankeyLayout sankeyLayout;
    private bool inLayout;

    public SankeyLayoutDemo() {
      InitializeComponent();
    }

    protected override async void Initialize() {
      InitializeGraph();
      CreateSampleGraph();
      await RunLayout(false);
      await graphControl.FitGraphBounds();
      CreateInputMode();
      graphControl.Graph.GetUndoEngine().Clear();
    }

    private void InitializeGraph() {
      var graph = graphControl.Graph;
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Brush = (Brush) new SolidColorBrush(Color.FromRgb(0x42, 0x81, 0xA4)).GetAsFrozen(),
          Pen = null
      };
      graph.EdgeDefaults.Style = new DemoEdgeStyle(graphControl.Selection, false);

      // use a label model that stretches the label over the full node layout, with small margins
      var centerLabelModel = new StretchNodeLabelModel { Padding = new InsetsD(3) };
      graph.NodeDefaults.Labels.LayoutParameter =
          centerLabelModel.CreateParameter(StretchNodeLabelModel.Position.Center);

      // set the default style for the node labels
      graph.NodeDefaults.Labels.Style = new LabelStyle {
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
        if (item.Tag != null && e.OldValue != null && tag.brush != ((CustomTag) e.OldValue).brush) {
          ((ShapeNodeStyle) item.Style).Brush = ((CustomTag) item.Tag).brush; //GetNodeColor(item);
          graphControl.Invalidate();
        }
      };

      // enable the undo engine
      graph.SetUndoEngineEnabled(true);

      // disable all edge handles
      var edgeDecorator = graph.GetDecorator().Edges;
      edgeDecorator.HandleProvider.Hide();
      edgeDecorator.SelectionRenderer.Hide();
      edgeDecorator.BendCreator.Hide();
      edgeDecorator.PositionHandler.Hide();
      graph.GetDecorator().Bends.HandleProvider.Hide();

      // allow nodes to be only moved vertically
      graph.GetDecorator().Nodes.PositionHandler
          .AddWrapperFactory((node, handler) => new HorizontallyConstrainedPositionHandler(handler));

      graphControl.HighlightIndicatorManager = new HighlightManager();
    }

    private void CreateInputMode() {
      // initialize input mode
      var mode = new GraphEditorInputMode {
          SelectableItems = GraphItemTypes.None,
          DeletableItems = GraphItemTypes.None,
          AllowCreateEdge = false,
          AllowCreateNode = false,
          MovableUnselectedItems = GraphItemTypes.Node,
          MoveUnselectedItemsInputMode = { Priority = 38 },
          MoveSelectedItemsInputMode = { Enabled = false },
          MarqueeSelectionInputMode = { Enabled = false },
          MoveViewportInputMode = { BeginRecognizer = EventRecognizers.MouseDown }
      };

      // listener to react if a node has been dragged
      mode.MoveUnselectedItemsInputMode.DragFinished += async (sender, args) => { await RunLayout(); };

      // listener to react in edge label text changing
      mode.LabelEdited += async (sender, args) => {
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
      // add hover listener to implement edge and label highlighting
      mode.ItemHoverInputMode.HoveredItemChanged += (sender, args) => {
        var highlights = graphControl.Highlights;
        highlights.Clear();
        var item = args.Item;
        if (item != null) {
          highlights.Add(item);
          if (item is IEdge) {
            foreach (var label in ((IEdge) item).Labels) {
              highlights.Add(label);
            }
          } else {
            highlights.Add(((ILabel) item).Owner);
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
      var n0 = graph.CreateNode(tag: blackParty);
      graph.AddLabel(n0, "Black Party");
      var n1 = graph.CreateNode(tag: redParty);
      graph.AddLabel(n1, "Red Party");
      var n2 = graph.CreateNode(tag: yellowParty);
      graph.AddLabel(n2, "Yellow Party");
      var n3 = graph.CreateNode(tag: greenParty);
      graph.AddLabel(n3, "Green Party");
      var n4 = graph.CreateNode(tag: purpleParty);
      graph.AddLabel(n4, "Purple Party");
      var n5 = graph.CreateNode(tag: blackParty);
      graph.AddLabel(n5, "Black Party");
      var n6 = graph.CreateNode(tag: redParty);
      graph.AddLabel(n6, "Red Party");
      var n7 = graph.CreateNode(tag: yellowParty);
      graph.AddLabel(n7, "Yellow Party");
      var n8 = graph.CreateNode(tag: greenParty);
      graph.AddLabel(n8, "Green Party");
      var n9 = graph.CreateNode(tag: purpleParty);
      graph.AddLabel(n9, "Purple Party");
      var n10 = graph.CreateNode(tag: nonVoters);
      graph.AddLabel(n10, "Non-voter");
      var n11 = graph.CreateNode(tag: blackParty);
      graph.AddLabel(n11, "Black Party");
      var n12 = graph.CreateNode(tag: redParty);
      graph.AddLabel(n12, "Red Party");
      var n13 = graph.CreateNode(tag: yellowParty);
      graph.AddLabel(n13, "Yellow Party");
      var n14 = graph.CreateNode(tag: greenParty);
      graph.AddLabel(n14, "Green Party");
      var n15 = graph.CreateNode(tag: purpleParty);
      graph.AddLabel(n15, "Purple Party");
      var n16 = graph.CreateNode(tag: blueParty);
      graph.AddLabel(n16, "Blue Party");
      var n17 = graph.CreateNode(tag: nonVoters);
      graph.AddLabel(n17, "Non-voter");
      var n18 = graph.CreateNode(tag: blackParty);
      graph.AddLabel(n18, "Black Party");
      var n19 = graph.CreateNode(tag: redParty);
      graph.AddLabel(n19, "Red Party");
      var n20 = graph.CreateNode(tag: yellowParty);
      graph.AddLabel(n20, "Yellow Party");
      var n21 = graph.CreateNode(tag: greenParty);
      graph.AddLabel(n21, "Green Party");
      var n22 = graph.CreateNode(tag: purpleParty);
      graph.AddLabel(n22, "Purple Party");
      var n23 = graph.CreateNode(tag: blueParty);
      graph.AddLabel(n23, "Blue Party");
      var n24 = graph.CreateNode(tag: nonVoters);
      graph.AddLabel(n24, "Non-voter");
      var n25 = graph.CreateNode(tag: blackParty);
      graph.AddLabel(n25, "Black Party");
      var n26 = graph.CreateNode(tag: redParty);
      graph.AddLabel(n26, "Red Party");
      var n27 = graph.CreateNode(tag: yellowParty);
      graph.AddLabel(n27, "Yellow Party");
      var n28 = graph.CreateNode(tag: greenParty);
      graph.AddLabel(n28, "Green Party");
      var n29 = graph.CreateNode(tag: purpleParty);
      graph.AddLabel(n29, "Purple Party");
      var n30 = graph.CreateNode(tag: blueParty);
      graph.AddLabel(n30, "Blue Party");
      var n31 = graph.CreateNode(tag: nonVoters);
      graph.AddLabel(n31, "Non-voter");
      var n32 = graph.CreateNode(tag: nonVoters);
      graph.AddLabel(n32, "Non-voter");

      var e0 = graph.CreateEdge(n0, n5);
      graph.AddLabel(e0, "13654");
      var e1 = graph.CreateEdge(n0, n7);
      graph.AddLabel(e1, "1140");
      var e2 = graph.CreateEdge(n0, n8);
      graph.AddLabel(e2, "50");
      var e3 = graph.CreateEdge(n0, n9);
      graph.AddLabel(e3, "40");
      var e4 = graph.CreateEdge(n0, n10);
      graph.AddLabel(e4, "1080");
      var e5 = graph.CreateEdge(n1, n5);
      graph.AddLabel(e5, "880");
      var e6 = graph.CreateEdge(n1, n6);
      graph.AddLabel(e6, "9890");
      var e7 = graph.CreateEdge(n1, n7);
      graph.AddLabel(e7, "530");
      var e8 = graph.CreateEdge(n1, n8);
      graph.AddLabel(e8, "870");
      var e9 = graph.CreateEdge(n1, n9);
      graph.AddLabel(e9, "1100");
      var e10 = graph.CreateEdge(n1, n10);
      graph.AddLabel(e10, "2040");
      var e11 = graph.CreateEdge(n2, n7);
      graph.AddLabel(e11, "6278");
      var e12 = graph.CreateEdge(n2, n10);
      graph.AddLabel(e12, "70");
      var e13 = graph.CreateEdge(n3, n7);
      graph.AddLabel(e13, "30");
      var e14 = graph.CreateEdge(n3, n8);
      graph.AddLabel(e14, "3681");
      var e15 = graph.CreateEdge(n3, n9);
      graph.AddLabel(e15, "140");
      var e16 = graph.CreateEdge(n3, n10);
      graph.AddLabel(e16, "30");
      var e17 = graph.CreateEdge(n4, n7);
      graph.AddLabel(e17, "20");
      var e18 = graph.CreateEdge(n4, n9);
      graph.AddLabel(e18, "3837");
      var e19 = graph.CreateEdge(n4, n10);
      graph.AddLabel(e19, "300");
      var e20 = graph.CreateEdge(n5, n11);
      graph.AddLabel(e20, "14685");
      var e21 = graph.CreateEdge(n5, n16);
      graph.AddLabel(e21, "290");
      var e22 = graph.CreateEdge(n6, n11);
      graph.AddLabel(e22, "210");
      var e23 = graph.CreateEdge(n6, n12);
      graph.AddLabel(e23, "9755");
      var e24 = graph.CreateEdge(n6, n16);
      graph.AddLabel(e24, "180");
      var e25 = graph.CreateEdge(n7, n11);
      graph.AddLabel(e25, "2110");
      var e26 = graph.CreateEdge(n7, n12);
      graph.AddLabel(e26, "530");
      var e27 = graph.CreateEdge(n7, n13);
      graph.AddLabel(e27, "2084");
      var e28 = graph.CreateEdge(n7, n14);
      graph.AddLabel(e28, "170");
      var e29 = graph.CreateEdge(n7, n15);
      graph.AddLabel(e29, "90");
      var e30 = graph.CreateEdge(n7, n16);
      graph.AddLabel(e30, "430");
      var e31 = graph.CreateEdge(n7, n17);
      graph.AddLabel(e31, "460");
      var e32 = graph.CreateEdge(n8, n11);
      graph.AddLabel(e32, "420");
      var e33 = graph.CreateEdge(n8, n12);
      graph.AddLabel(e33, "550");
      var e34 = graph.CreateEdge(n8, n14);
      graph.AddLabel(e34, "3570");
      var e35 = graph.CreateEdge(n8, n16);
      graph.AddLabel(e35, "90");
      var e36 = graph.CreateEdge(n8, n17);
      graph.AddLabel(e36, "40");
      var e37 = graph.CreateEdge(n9, n11);
      graph.AddLabel(e37, "120");
      var e38 = graph.CreateEdge(n9, n12);
      graph.AddLabel(e38, "370");
      var e39 = graph.CreateEdge(n9, n14);
      graph.AddLabel(e39, "40");
      var e40 = graph.CreateEdge(n9, n15);
      graph.AddLabel(e40, "3780");
      var e41 = graph.CreateEdge(n9, n16);
      graph.AddLabel(e41, "340");
      var e42 = graph.CreateEdge(n9, n17);
      graph.AddLabel(e42, "320");
      var e43 = graph.CreateEdge(n10, n11);
      graph.AddLabel(e43, "1130");
      var e44 = graph.CreateEdge(n10, n12);
      graph.AddLabel(e44, "360");
      var e45 = graph.CreateEdge(n10, n16);
      graph.AddLabel(e45, "210");
      var e46 = graph.CreateEdge(n10, n17);
      graph.AddLabel(e46, "16835");
      var e47 = graph.CreateEdge(n11, n18);
      graph.AddLabel(e47, "15065");
      var e48 = graph.CreateEdge(n11, n20);
      graph.AddLabel(e48, "1360");
      var e49 = graph.CreateEdge(n11, n21);
      graph.AddLabel(e49, "30");
      var e50 = graph.CreateEdge(n11, n22);
      graph.AddLabel(e50, "90");
      var e51 = graph.CreateEdge(n11, n23);
      graph.AddLabel(e51, "980");
      var e52 = graph.CreateEdge(n11, n24);
      graph.AddLabel(e52, "1610");
      var e53 = graph.CreateEdge(n12, n18);
      graph.AddLabel(e53, "20");
      var e54 = graph.CreateEdge(n12, n19);
      graph.AddLabel(e54, "9276");
      var e55 = graph.CreateEdge(n12, n20);
      graph.AddLabel(e55, "450");
      var e56 = graph.CreateEdge(n12, n21);
      graph.AddLabel(e56, "380");
      var e57 = graph.CreateEdge(n12, n22);
      graph.AddLabel(e57, "430");
      var e58 = graph.CreateEdge(n12, n23);
      graph.AddLabel(e58, "470");
      var e59 = graph.CreateEdge(n13, n20);
      graph.AddLabel(e59, "2115");
      var e60 = graph.CreateEdge(n13, n23);
      graph.AddLabel(e60, "40");
      var e61 = graph.CreateEdge(n14, n20);
      graph.AddLabel(e61, "110");
      var e62 = graph.CreateEdge(n14, n21);
      graph.AddLabel(e62, "3545");
      var e63 = graph.CreateEdge(n14, n22);
      graph.AddLabel(e63, "170");
      var e64 = graph.CreateEdge(n14, n23);
      graph.AddLabel(e64, "40");
      var e65 = graph.CreateEdge(n15, n20);
      graph.AddLabel(e65, "60");
      var e66 = graph.CreateEdge(n15, n22);
      graph.AddLabel(e66, "3365");
      var e67 = graph.CreateEdge(n15, n23);
      graph.AddLabel(e67, "400");
      var e68 = graph.CreateEdge(n16, n23);
      graph.AddLabel(e68, "2793");
      var e69 = graph.CreateEdge(n17, n18);
      graph.AddLabel(e69, "380");
      var e70 = graph.CreateEdge(n17, n19);
      graph.AddLabel(e70, "360");
      var e71 = graph.CreateEdge(n17, n20);
      graph.AddLabel(e71, "700");
      var e72 = graph.CreateEdge(n17, n21);
      graph.AddLabel(e72, "230");
      var e73 = graph.CreateEdge(n17, n22);
      graph.AddLabel(e73, "270");
      var e74 = graph.CreateEdge(n17, n23);
      graph.AddLabel(e74, "1200");
      var e75 = graph.CreateEdge(n17, n24);
      graph.AddLabel(e75, "13072");
      var e76 = graph.CreateEdge(n18, n25);
      graph.AddLabel(e76, "11050");
      var e77 = graph.CreateEdge(n18, n26);
      graph.AddLabel(e77, "1530");
      var e78 = graph.CreateEdge(n18, n27);
      graph.AddLabel(e78, "490");
      var e79 = graph.CreateEdge(n18, n28);
      graph.AddLabel(e79, "920");
      var e80 = graph.CreateEdge(n18, n31);
      graph.AddLabel(e80, "50");
      var e81 = graph.CreateEdge(n19, n26);
      graph.AddLabel(e81, "8760");
      var e82 = graph.CreateEdge(n19, n28);
      graph.AddLabel(e82, "260");
      var e83 = graph.CreateEdge(n20, n26);
      graph.AddLabel(e83, "180");
      var e84 = graph.CreateEdge(n20, n27);
      graph.AddLabel(e84, "4160");
      var e85 = graph.CreateEdge(n20, n28);
      graph.AddLabel(e85, "240");
      var e86 = graph.CreateEdge(n21, n28);
      graph.AddLabel(e86, "5257");
      var e87 = graph.CreateEdge(n22, n25);
      graph.AddLabel(e87, "20");
      var e88 = graph.CreateEdge(n22, n26);
      graph.AddLabel(e88, "640");
      var e89 = graph.CreateEdge(n22, n27);
      graph.AddLabel(e89, "110");
      var e90 = graph.CreateEdge(n22, n28);
      graph.AddLabel(e90, "480");
      var e91 = graph.CreateEdge(n22, n29);
      graph.AddLabel(e91, "2267");
      var e92 = graph.CreateEdge(n22, n30);
      graph.AddLabel(e92, "90");
      var e93 = graph.CreateEdge(n22, n31);
      graph.AddLabel(e93, "320");
      var e94 = graph.CreateEdge(n23, n25);
      graph.AddLabel(e94, "80");
      var e95 = graph.CreateEdge(n23, n26);
      graph.AddLabel(e95, "260");
      var e96 = graph.CreateEdge(n23, n27);
      graph.AddLabel(e96, "210");
      var e97 = graph.CreateEdge(n23, n28);
      graph.AddLabel(e97, "60");
      var e98 = graph.CreateEdge(n23, n30);
      graph.AddLabel(e98, "4675");
      var e99 = graph.CreateEdge(n23, n31);
      graph.AddLabel(e99, "180");
      var e100 = graph.CreateEdge(n24, n26);
      graph.AddLabel(e100, "520");
      var e101 = graph.CreateEdge(n24, n27);
      graph.AddLabel(e101, "300");
      var e102 = graph.CreateEdge(n24, n31);
      graph.AddLabel(e102, "13584");
      var e103 = graph.CreateEdge(n32, n10);
      graph.AddLabel(e103, "14645");

      // assign node styles
      foreach (var node in graph.Nodes) {
        graph.SetStyle(node, new ShapeNodeStyle {
            Brush = ((CustomTag) node.Tag).brush,
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

    private async Task RunLayout(bool animated = true) {
      var graph = graphControl.Graph;
      if (graph.Nodes.Count == 0 || inLayout) {
        return;
      }
      if (sankeyLayout == null) {
        sankeyLayout = new SankeyLayout();
      }
      inLayout = true;
      SetUiDisabled(true);

      // configure the layout algorithm
      var fromSketchMode = useDrawingAsSketch.IsChecked == true;
      var hierarchicalLayout = sankeyLayout.ConfigureHierarchicalLayout(fromSketchMode);
      var hierarchicalLayoutData = sankeyLayout.CreateHierarchicalLayoutData(graph, fromSketchMode);

      // run the layout and animate the result
      try {
        if (animated) {
          await graphControl.ApplyLayoutAnimated(hierarchicalLayout, TimeSpan.FromSeconds(1), hierarchicalLayoutData);
        } else {
          graph.ApplyLayout(hierarchicalLayout, hierarchicalLayoutData);
        }
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
          edge.Tag = new CustomTag {
              thickness = 2,
              brush = oldTag.brush
          };
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
      graphControl.Graph.SetStyle(label, new LabelStyle { TextBrush = brush });
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

    [STAThread]
    private static void Main() {
      Run<SankeyLayoutDemo>();
    }
  }

  /// <summary>
  ///   A custom tag that holds style information
  /// </summary>
  internal class CustomTag
  {
    public double thickness;

    public SolidColorBrush brush;

    public static CustomTag NewNodeTag(byte r, byte g, byte b) =>
        new() { brush = (SolidColorBrush) new SolidColorBrush(Color.FromRgb(r, g, b)).GetAsFrozen() };

    public static CustomTag NewEdgeTag(CustomTag prototype) =>
        new() {
            thickness = 1,
            brush = (SolidColorBrush) new SolidColorBrush {
                Color = prototype.brush.Color,
                Opacity = 0.5
            }.GetAsFrozen()
        };
  }
}
