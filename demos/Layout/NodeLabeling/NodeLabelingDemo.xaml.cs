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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Labeling;

namespace yWorks.Demo.NodeLabeling
{
  /// <summary>
  ///   Demonstrates automatic node label placement with preferred placement candidates.
  /// </summary>
  [Demo("node-labeling", "layout", "Automatic Node Labeling",
      Summary = "Shows how to place node labels at the preferred location with a labeling algorithm.",
      Tags = ["label placement"],
      Keywords = ["layoutfeatures", "generic labeling", "text", "background", "label candidates", "padding"])]
  public sealed partial class NodeLabelingDemo
  {
    /// <summary>
    ///   The graph component.
    /// </summary>
    private GraphControl GraphComponent => graphControl;

    /// <summary>
    ///   Holds whether a layout is in progress.
    /// </summary>
    private bool inLayout;

    /// <summary>
    ///   Holds the available label candidates.
    /// </summary>
    private List<NodeLabelCandidates> labelCandidateModes = [];

    /// <summary>
    ///   Holds the label styles
    /// </summary>
    private LabelStyle labelStyle = null!;

    private CityLabelStyle cityLabelStyle = null!;

    private LabelStyle pinnedLabelStyle = null!;
    private CityLabelStyle pinnedCityLabelStyle = null!;

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override async void Initialize() {
      // initialize the node label properties
      InitializeOptions();

      // set the default styles for nodes and labels
      InitializeGraph();

      // create the input mode
      InitializeInputMode();

      // create the sample graph
      await CreateSampleGraph();

      // wire up the UI
      InitializeUI();
    }

    /// <summary>
    ///   Initialize the node label options.
    /// </summary>
    private void InitializeOptions() {
      // A set of candidates that chooses from three distances of the cardinal directions
      var threePositionsCardinal = new NodeLabelCandidates();
      threePositionsCardinal.AddDiscreteCandidates(DiscreteNodeLabelPositions.Sides, 5);
      threePositionsCardinal.AddDiscreteCandidates(DiscreteNodeLabelPositions.Sides, 10, 0.9);
      threePositionsCardinal.AddDiscreteCandidates(DiscreteNodeLabelPositions.Sides, 15, 0.8);

      // A set of candidates that chooses from cardinal directions but prefers placing the label on top
      var cardinalPreferTop = new NodeLabelCandidates();
      cardinalPreferTop.AddDiscreteCandidates(DiscreteNodeLabelPositions.Top, 5);
      //but other cardinal positions should also be possible
      cardinalPreferTop.AddDiscreteCandidates(
          DiscreteNodeLabelPositions.Left | DiscreteNodeLabelPositions.Bottom | DiscreteNodeLabelPositions.Right, 5,
          0.8);

      // A set of candidates that prefers placing the label on top but chooses another position
      // if not possible
      var freePreferTopSideCandidates = new NodeLabelCandidates();
      freePreferTopSideCandidates.AddDiscreteCandidates(DiscreteNodeLabelPositions.Top, 5);
      freePreferTopSideCandidates.AddFreeCandidates(0.8);

      labelCandidateModes = [
          // Interior candidates
          new NodeLabelCandidates().AddDiscreteCandidates(DiscreteNodeLabelPositions.Inside, 0),
          // Exterior candidates
          new NodeLabelCandidates().AddDiscreteCandidates(DiscreteNodeLabelPositions.Outside, 20),
          threePositionsCardinal,
          cardinalPreferTop,
          // No preferred position, leave it up to the GenericLabeling algorithm
          new NodeLabelCandidates().AddFreeCandidates(),
          freePreferTopSideCandidates
      ];
    }

    /// <summary>
    ///   Initializes node and label styles that are applied when the graph is created.
    /// </summary>
    private void InitializeGraph() {
      var graph = GraphComponent.Graph;
      // set the default style for nodes
      graph.NodeDefaults.Style = DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse);

      // set the default size for nodes
      graph.NodeDefaults.Size = new SizeD(10, 10);

      // set the default style for labels
      labelStyle = DemoStyles.CreateDemoNodeLabelStyle();
      labelStyle.TextSize = 12;
      cityLabelStyle = new CityLabelStyle(labelStyle);
      pinnedLabelStyle = DemoStyles.CreateDemoNodeLabelStyle(Themes.PaletteGreen);
      pinnedLabelStyle.TextSize = 12;
      pinnedCityLabelStyle = new CityLabelStyle(pinnedLabelStyle);

      graph.NodeDefaults.Labels.Style = cityLabelStyle;
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Center;
      // add the background visual for the map
      using var stream = LoadAssemblyResource("usamap.svg");
      GraphComponent.RenderTree.CreateElement(GraphComponent.RenderTree.BackgroundGroup, new MapVisualCreator(stream));
    }

    /// <summary>
    ///   Creates the input mode.
    /// </summary>
    private void InitializeInputMode() {
      var inputMode = new GraphEditorInputMode {
          ShowHandleItems = GraphItemTypes.None,
          AllowCreateEdge = false
      };
      // add a label for each newly created node
      inputMode.NodeCreated += async (_, evt) => {
        GraphComponent.Graph.AddLabel(evt.Item, "City");
        await PlaceLabels();
      };

      inputMode.MoveUnselectedItemsInputMode.DragFinished += async (_, _) =>
          await PinLabels(inputMode.MoveUnselectedItemsInputMode.AffectedItems.Concat(
              inputMode.MoveSelectedItemsInputMode.AffectedItems));
      inputMode.MoveSelectedItemsInputMode.DragFinished += async (_, _) =>
          await PinLabels(inputMode.MoveUnselectedItemsInputMode.AffectedItems.Concat(
              inputMode.MoveSelectedItemsInputMode.AffectedItems));

      // Unpins the label on click and runs the currently selected label layout
      inputMode.ItemClicked += async (_, evt) => {
        var item = evt.Item;
        if (item is ILabel label && label.Tag != null) {
          GraphComponent.Graph.SetStyle(label, cityLabelStyle);
          label.Tag = null;
          await PlaceLabels();
        }
      };

      GraphComponent.InputMode = inputMode;
    }

    /// <summary>
    ///   Pins the label on drag finished.
    /// </summary>
    private async Task PinLabels(IEnumerable<IModelItem> items) {
      foreach (var item in items.OfType<ILabel>()) {
        GraphComponent.Graph.SetStyle(item, pinnedCityLabelStyle);
        item.Tag = "fixed";
        await PlaceLabels();
      }
    }

    /// <summary>
    ///   Unpins all labels
    /// </summary>
    private async Task UnpinLabels() {
      var graph = GraphComponent.Graph;
      foreach (var label in graph.Labels) {
        if (label.Tag != null) {
          label.Tag = null;
          graph.SetStyle(label, cityLabelStyle);
        }
      }
      await PlaceLabels();
    }

    /// <summary>
    ///   Configures the label model and runs the labeling algorithm.
    /// </summary>
    private async Task PlaceLabels() {
      if (inLayout) {
        return;
      }
      SetUIDisabled(true);
      // configure the labeling algorithm
      var labelingAlgorithm = new GenericLabeling { Scope = LabelingScope.NodeLabels };
      // select the label candidate set and configure the GenericLabelingData
      var labelCandidate = labelCandidateModes[labelCandidatesComboBox.SelectedIndex];
      var labelingData = labelingAlgorithm.CreateLayoutData(GraphComponent.Graph);
      labelingData.NodeLabelingCosts.Constant = new LabelingCosts(LabelingOptimizationStrategy.AmbiguousPlacements);
      labelingData.NodeLabelCandidates.Constant = labelCandidate;
      // excludes pinned labels from the layout
      labelingData.Scope.NodeLabels.Predicate = label => label.Tag is not string tag || tag != "fixed";

      try {
        await new LayoutExecutor(GraphComponent, labelingAlgorithm) {
            AnimationDuration = TimeSpan.FromMilliseconds(500),
            LayoutData = labelingData,
            EasedAnimation = true,
            AnimateViewport = false
        }.Start();
      } finally {
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Changes the state of the UI's elements.
    /// </summary>
    /// <param name="disabled">True if the UI elements should be enabled, false otherwise</param>
    private void SetUIDisabled(bool disabled) {
      inLayout = disabled;
      labelCandidatesComboBox.IsEnabled = !disabled;
      labelSizeSlider.IsEnabled = !disabled;
      placeLabelsButton.IsEnabled = !disabled;
      unpinLabelsButton.IsEnabled = !disabled;
    }

    /// <summary>
    ///   Wires up the UI.
    /// </summary>
    private void InitializeUI() {
      placeLabelsButton.Click += async (_, _) => await PlaceLabels();
      unpinLabelsButton.Click += async (_, _) => await UnpinLabels();
      labelSizeSlider.ValueChanged += async (_, _) => await ChangeFontSize();
      labelCandidatesComboBox.SelectionChanged += async (_, _) => await PlaceLabels();
    }

    /// <summary>
    ///   Creates the sample graph.
    /// </summary>
    private async Task CreateSampleGraph() {
      BuildGraphFromJson("graph-data.json", GraphComponent.Graph);

      // fit content
      await GraphComponent.FitGraphBounds();

      // places the node labels
      await PlaceLabels();
    }

    private async Task ChangeFontSize() {
      // Check if the label size input is valid. Valid inputs are greater than zero and less than 50.
      var textSize = labelSizeSlider.Value;
      if (double.IsNaN(textSize) || textSize <= 0 || textSize > 50) {
        MessageBox.Show("Label size should be greater than 0 and less than 50.");
        return;
      }

      var graph = GraphComponent.Graph;

      labelStyle.TextSize = textSize;
      pinnedLabelStyle.TextSize = textSize;

      cityLabelStyle = new CityLabelStyle(labelStyle);
      pinnedCityLabelStyle = new CityLabelStyle(pinnedLabelStyle);

      foreach (var label in graph.GetNodeLabels()) {
        // sets the label size based on the selected value of the corresponding text-field
        graph.SetStyle(label, label.Tag != null ? pinnedCityLabelStyle : cityLabelStyle);
      }

      graph.NodeDefaults.Labels.Style = cityLabelStyle;
      GraphComponent.Invalidate();
      await PlaceLabels();
    }

    [STAThread]
    private static void Main() => Run<NodeLabelingDemo>();

    public NodeLabelingDemo() {
      InitializeComponent();
    }
  }
}
