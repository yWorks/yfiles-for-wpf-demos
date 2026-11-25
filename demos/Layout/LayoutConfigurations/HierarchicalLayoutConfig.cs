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
using System.Linq;
using System.Reflection;
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Demo.Toolkit.OptionHandler;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;
using yWorks.Layout.Tree;
using PortAssignmentMode = yWorks.Layout.Hierarchical.PortAssignmentMode;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("HierarchicalLayout")]
  public class HierarchicalLayoutConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public HierarchicalLayoutConfig() {
      GroupHorizontalCompactionItem = true;
      GroupAlignmentItem = GroupAlignmentPolicy.Center;
      NodeLabelingItem = NodeLabelPlacement.Consider;
      MaximumSizeItem = 1000;
      ScaleItem = 1;
      ComponentArrangementPolicyItem = ComponentArrangementPolicy.Topmost;
      NodeCompactionItem = false;
      RankingPolicyItem = LayeringStrategy.HierarchicalOptimal;
      MinimumSlopeItem = 0.25;
      EdgeDirectednessItem = true;
      EdgeThicknessItem = true;
      MinimumEdgeDistanceItem = 15;
      MinimumEdgeLengthItem = 20;
      MinimumLastSegmentLengthItem = 15;
      MinimumFirstSegmentLengthItem = 10;
      EdgeRoutingItem = RoutingStyle.Orthogonal;
      MinimumLayerDistanceItem = 10;
      EdgeDistanceItem = 15;
      NodeToEdgeDistanceItem = 15;
      NodeDistanceItem = 30;
      SymmetricPlacementItem = SymmetryOptimizationStrategy.Strong;
      RecursiveEdgePolicyItem = RecursiveEdgePolicy.Off;
      StopDurationItem = 5;
      EdgeLabelingItem = EdgeLabelPlacement.Integrated;
      CompactEdgeLabelPlacementItem = true;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
      GroupLayeringPolicyItem = GroupLayeringPolicy.Recursive;
      GridEnabledItem = false;
      GridSpacingItem = 5;
      GridPortAssignmentItem = PortAssignmentMode.Default;
      OrientationItem = LayoutOrientation.TopToBottom;
      SubComponentsItem = false;
      HighlightCriticalPath = false;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new HierarchicalLayout();

      //  mark incremental elements if required
      var fromSketch = UseDrawingAsSketchItem;
      var incrementalLayout = SelectedElementsIncrementallyItem;
      var selectedElements = graphControl.Selection.Edges.Any() || graphControl.Selection.Nodes.Any();

      if ((incrementalLayout && selectedElements) || fromSketch) {
        layout.FromSketchMode = true;
      } else {
        layout.FromSketchMode = false;
      }

      layout.CoordinateAssigner.SymmetryOptimizationStrategy = SymmetricPlacementItem;

      layout.ComponentLayout.Enabled = LayoutComponentsSeparatelyItem;

      layout.MinimumLayerDistance = MinimumLayerDistanceItem;
      layout.NodeToEdgeDistance = NodeToEdgeDistanceItem;
      layout.NodeDistance = NodeDistanceItem;
      layout.EdgeDistance = EdgeDistanceItem;

      var nld = layout.DefaultNodeDescriptor;
      var eld = layout.DefaultEdgeDescriptor;

      layout.AutomaticEdgeGrouping = AutomaticEdgeGroupingEnabledItem;

      eld.RoutingStyleDescriptor = new RoutingStyleDescriptor(EdgeRoutingItem);
      eld.RoutingStyleDescriptor.CurveShortcuts = CurveShortcutsItem;
      eld.RoutingStyleDescriptor.CurveUTurnSymmetry = CurveUTurnSymmetryItem;
      eld.MinimumFirstSegmentLength = MinimumFirstSegmentLengthItem;
      eld.MinimumLastSegmentLength = MinimumLastSegmentLengthItem;

      eld.MinimumDistance = MinimumEdgeDistanceItem;
      eld.MinimumLength = MinimumEdgeLengthItem;

      eld.MinimumSlope = MinimumSlopeItem;

      eld.RecursiveEdgePolicy = RecursiveEdgePolicyItem;

      nld.MinimumDistance = Math.Min(layout.NodeDistance, layout.NodeToEdgeDistance);
      nld.MinimumLayerHeight = 0;
      nld.LayerAlignment = LayerAlignmentItem;

      layout.LayoutOrientation = OrientationItem;

      layout.NodeLabelPlacement = NodeLabelingItem;

      layout.EdgeLabelPlacement = EdgeLabelingItem;
      switch (EdgeLabelingItem) {
        case EdgeLabelPlacement.Ignore:
          break;
        case EdgeLabelPlacement.Generic:
          if (ReduceAmbiguityItem) {
            var labeling = layout.LayoutStages.Get<GenericLabeling>();
            labeling.DefaultEdgeLabelingCosts.AmbiguousPlacementCost = 1.0;
          }
          break;
        case EdgeLabelPlacement.Integrated:
          layout.CoordinateAssigner.LabelCompaction = CompactEdgeLabelPlacementItem;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      layout.FromScratchLayeringStrategy = RankingPolicyItem;
      layout.ComponentArrangementPolicy = ComponentArrangementPolicyItem;
      layout.CoordinateAssigner.NodeCompaction = NodeCompactionItem;

      // configure FromSketchLayerAssigner
      var layerer = layout.FromSketchMode
          ? layout.Core.FixedElementsLayerAssigner
          : layout.Core.FromScratchLayerAssigner;
      if (layerer is FromSketchLayerAssigner fsl) {
        fsl.NodeMargin = NodeMarginItem;
        fsl.NodeScalingFactor = ScaleItem;
        fsl.MinimumNodeSize = MinimumSizeItem;
        fsl.MaximumNodeSize = MaximumSizeItem;
      }

      // configure grouping
      layout.CoordinateAssigner.GroupCompaction = GroupHorizontalCompactionItem;

      if (!fromSketch) {
        layout.GroupAlignmentPolicy = GroupAlignmentItem;
        layout.GroupLayeringPolicy = GroupLayeringPolicyItem;
      } else {
        layout.GroupLayeringPolicy = GroupLayeringPolicy.RecursiveCompact;
      }

      if (TreatRootGroupAsSwimlanesItem) {
        var stage = new TopLevelGroupToSwimlaneStage {
            OrderSwimlanesFromSketch = UseOrderFromSketchItem,
            Spacing = SwimlineSpacingItem
        };
        layout.LayoutStages.Prepend(stage);
      }

      layout.DefaultEdgeDescriptor.BackLoopRouting = BackloopRoutingItem;
      layout.StopDuration = TimeSpan.FromSeconds(StopDurationItem);

      if (GridEnabledItem) {
        layout.GridSpacing = GridSpacingItem;
      }

      return layout;
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var layoutData = new HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel>();

      var selection = graphControl.Selection;

      if (((HierarchicalLayout) layout).FromSketchMode) {
        // configure the from-sketch mode
        layoutData.IncrementalNodeHints.MapperFunction = node => {
          if (SelectedElementsIncrementallyItem && selection.Contains(node)) {
            // a selected element can get the Incremental (i.e., arrange freely) hint
            return IncrementalNodeHint.Incremental;
          }

          // others get a sketch-based hint, more or less strong, depending on the selected policy
          if (SketchPolicyItem == EnumFromSketchPolicy.FromSketch) {
            return IncrementalNodeHint.FromSketch;
          }
          if (SketchPolicyItem == EnumFromSketchPolicy.ExactCoordinates) {
            return IncrementalNodeHint.ExactCoordinates;
          }
          return IncrementalNodeHint.KeepRelativeOrder;
        };

        // mark selected edges as incremental edges that are rerouted freely in any case
        if (selection.Edges.Any()) {
          layoutData.IncrementalEdges.Source = selection.Edges;
        }
      }

      if (RankingPolicyItem == LayeringStrategy.Bfs) {
        layoutData.BfsLayerAssignerCoreNodes.Predicate = selection.Contains;
      }

      if (GridEnabledItem) {
        var nld = ((HierarchicalLayout) layout).DefaultNodeDescriptor;
        layoutData.NodeDescriptors.MapperFunction = node => {
          var descriptor = new HierarchicalLayoutNodeDescriptor();
          descriptor.LayerAlignment = nld.LayerAlignment;
          descriptor.MinimumDistance = nld.MinimumDistance;
          descriptor.MinimumLayerHeight = nld.MinimumLayerHeight;
          // anchor nodes on grid according to their alignment within the layer
          descriptor.GridReference = new PointD(0.0, (nld.LayerAlignment - 0.5) * node.Layout.Height);
          descriptor.PortAssignment = GridPortAssignmentItem;
          return descriptor;
        };
      }

      if (EdgeDirectednessItem) {
        layoutData.EdgeDirectedness.MapperFunction = edge => {
          if (edge.Style is IArrowOwner && !Equals(((IArrowOwner) edge.Style).TargetArrow, Arrow.None)) {
            return 1;
          }
          return 0;
        };
      }

      if (EdgeThicknessItem) {
        layoutData.EdgeThickness.MapperFunction = edge => {
          var style = edge.Style as PolylineEdgeStyle;
          if (style != null) {
            return style.Pen.Thickness;
          }
          return 1;
        };
      }

      if (SubComponentsItem) {
        // layout all siblings with label 'TL' separately with tree layout
        var treeLayout = new TreeLayout { DefaultSubtreePlacer = new LeftRightSubtreePlacer() };
        foreach (var listOfNodes in FindSubComponents(graphControl.Graph, "TL")) {
          layoutData.Subcomponents.Add(new SubcomponentDescriptor(treeLayout)).Items = listOfNodes;
        }
        // layout all siblings with label 'HL' separately with hierarchical layout
        var hierarchicalLayout = new HierarchicalLayout { LayoutOrientation = LayoutOrientation.LeftToRight };
        foreach (var listOfNodes in FindSubComponents(graphControl.Graph, "HL")) {
          layoutData.Subcomponents.Add(new SubcomponentDescriptor(hierarchicalLayout)).Items = listOfNodes;
        }
        // layout all siblings with label 'OL' separately with organic layout
        var organicLayout = new OrganicLayout {
            DefaultPreferredEdgeLength = 100,
            Deterministic = true
        };
        foreach (var listOfNodes in FindSubComponents(graphControl.Graph, "OL")) {
          layoutData.Subcomponents.Add(new SubcomponentDescriptor(organicLayout)).Items = listOfNodes;
        }
      }

      if (HighlightCriticalPath) {
        // highlight the longest path in the graph as critical path
        // since the longest path algorithm only works for acyclic graphs,
        // feedback edges and self-loops have to be excluded here
        var feedbackEdgeSetResult = new FeedbackEdgeSet().Run(graphControl.Graph);
        var longestPath = new LongestPath {
            SubgraphEdges = {
                Excludes = {
                    Predicate = edge => feedbackEdgeSetResult.FeedbackEdgeSet.Contains(edge) || edge.IsSelfLoop()
                }
            }
        }.Run(graphControl.Graph);
        if (longestPath.Edges.Any()) {
          layoutData.CriticalEdgePriorities.MapperFunction = edge => {
            if (longestPath.Edges.Contains(edge)) {
              return 10;
            }
            return 1;
          };
        }
      }

      if (AutomaticBusRouting) {
        var allBusNodes = new HashSet<INode>();
        foreach (var node in graphControl.Graph.Nodes) {
          if (!graphControl.Graph.IsGroupNode(node) && !allBusNodes.Contains(node)) {
            // search for good opportunities for bus structures rooted at this node
            if (graphControl.Graph.InDegree(node) >= 4) {
              var gridDescriptor = new GridComponentDescriptor();
              var busEdges = GetBusEdges(graphControl.Graph, node, allBusNodes, graphControl.Graph.InEdgesAt(node));
              if (busEdges.Any()) {
                layoutData.GridComponents.Add(gridDescriptor).Items = busEdges;
              }
            }
            if (graphControl.Graph.OutDegree(node) >= 4) {
              var gridDescriptor = new GridComponentDescriptor();
              var busEdges = GetBusEdges(graphControl.Graph, node, allBusNodes, graphControl.Graph.OutEdgesAt(node));
              if (busEdges.Any()) {
                layoutData.GridComponents.Add(gridDescriptor).Items = busEdges;
              }
            }
          }
        }
      }

      return layoutData.CombineWith(CreateLabelingLayoutData(graphControl.Graph, LabelPlacementAlongEdgeItem,
          LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem));
    }

    private ICollection<IEdge> GetBusEdges(
        IGraph graph,
        INode node,
        HashSet<INode> allBusNodes,
        IEnumerable<IEdge> edges) {
      if (graph.IsGroupNode(node)) {
        // exclude groups for bus structures
        return new List<IEdge>();
      }

      var busNodes = new HashSet<INode>();
      // count the edges that are not bus edges but connect to the bus nodes
      // -> if there are many, then the bus structure may not look that great
      var interEdgeCount = 0;
      var busEdges = new List<IEdge>();
      var busNodesWithOtherEdges = new Stack<INode>();
      var node2BusEdge = new Dictionary<INode, IEdge>();
      foreach (var edge in edges) {
        var other = (INode) edge.Opposite(node);
        if (!busNodes.Contains(other)) {
          busNodes.Add(other);
          busEdges.Add(edge);
          node2BusEdge.Add(other, edge);
          var otherEdgesCount = graph.Degree(other) - 1;
          if (otherEdgesCount > 0) {
            busNodesWithOtherEdges.Push(other);
            interEdgeCount += otherEdgesCount;
          }
        }
      }

      var finalBusEdges = new List<IEdge>();
      var removedBusEdges = new HashSet<IEdge>();
      var busSize = busNodes.Count;
      while (busSize >= 4) {
        if (interEdgeCount <= busSize * 0.25) {
          // okay accept this as a bus
          foreach (var edge in busEdges) {
            if (!removedBusEdges.Contains(edge)) {
              finalBusEdges.Add(edge);
              allBusNodes.Add((INode) edge.Opposite(node));
            }
          }
          break;
        }
        // this bus has too many other edges remove some if possible
        if (busNodesWithOtherEdges.Any()) {
          var nodeToRemove = busNodesWithOtherEdges.Pop();
          removedBusEdges.Add(node2BusEdge[nodeToRemove]);
          busSize--;
          interEdgeCount -= graph.Degree(nodeToRemove) - 1;
        }
      }

      return finalBusEdges;
    }

    /// <summary>
    ///   Determines sub-components by label text and group membership.
    ///   This is necessary because <see cref="HierarchicalLayout"/> does not support sub-components with nodes
    ///   that belong to different parent groups.
    /// </summary>
    private ICollection<ICollection<INode>> FindSubComponents(IGraph graph, string labelText) {
      var nodeToComponent = new Dictionary<INode, ICollection<INode>>();
      foreach (var node in graph.Nodes) {
        if (node.Labels.Any() && node.Labels.First().Text == labelText) {
          var parent = graph.GetParent(node) ?? root;
          if (!nodeToComponent.ContainsKey(parent)) {
            nodeToComponent.Add(parent, new List<INode>());
          }
          nodeToComponent[parent].Add(node);
        }
      }
      return nodeToComponent.Values;
    }

    private readonly INode root = new SimpleNode();

    /// <summary>
    ///   Enables different layout styles for possibly detected sub-components.
    /// </summary>
    public void EnableSubComponents() {
      SubComponentsItem = true;
    }

    public void EnableAutomaticBusRouting() {
      AutomaticBusRouting = true;
    }

    public void EnableCurvedRouting() {
      EdgeRoutingItem = RoutingStyle.Curved;
    }

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming

    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GeneralGroup;

    [Label("Incremental Layout")]
    [OptionGroup("GeneralGroup", 70)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object IncrementalGroup;

    [OptionGroup("GeneralGroup", 60)]
    [Label("Minimum Distances")]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DistanceGroup;

    [Label("Edges")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object EdgeSettingsGroup;

    [Label("Layers")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object RankGroup;

    [Label("Labeling")]
    [OptionGroup("RootGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LabelingGroup;

    [Label("Node Settings")]
    [OptionGroup("LabelingGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object NodePropertiesGroup;

    [Label("Edge Settings")]
    [OptionGroup("LabelingGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object EdgePropertiesGroup;

    [Label("Preferred Edge Label Placement")]
    [OptionGroup("LabelingGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object PreferredPlacementGroup;

    [Label("Grouping")]
    [OptionGroup("RootGroup", 50)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GroupingGroup;

    [Label("Swimlanes")]
    [OptionGroup("RootGroup", 60)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object SwimlanesGroup;

    [Label("Grid")]
    [OptionGroup("RootGroup", 70)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GridGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>The hierarchical layout style highlights the main direction or flow of a directed graph. The layout algorithm places " +
        "the nodes of a graph in hierarchically arranged layers such that the (majority of) its edges follows the overall " +
        "orientation, for example, top-to-bottom.</Paragraph>" +
        "<Paragraph>This style is tailored for application domains in which it is crucial to clearly visualize the dependency relations between " +
        "entities. In particular, if such relations form a chain of dependencies between entities, this " +
        "layout style nicely exhibits them. Generally, whenever the direction of information flow matters, the hierarchical " +
        "layout style is an invaluable tool.</Paragraph>" +
        "<Paragraph>Suitable application domains of this layout style include, for example:</Paragraph>" + "<List>" +
        "<ListItem><Paragraph>Workflow visualization</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Software engineering like call graph visualization or activity diagrams</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Process modeling</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Database modeling and Entity-Relationship diagrams</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Bioinformatics, for example biochemical pathways</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Network management</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Decision diagrams</Paragraph></ListItem>" + "</List>";

    [Label("Selected Elements Incrementally")]
    [OptionGroup("IncrementalGroup", 10)]
    [DefaultValue(false)]
    public bool SelectedElementsIncrementallyItem { get; set; }

    [Label("Use Drawing as Sketch")]
    [OptionGroup("IncrementalGroup", 20)]
    [DefaultValue(false)]
    public bool UseDrawingAsSketchItem { get; set; }

    [Label("Sketch Policy")]
    [DefaultValue(EnumFromSketchPolicy.KeepOrder)]
    [OptionGroup("IncrementalGroup", 30)]
    [EnumValue("Keep Only Order", EnumFromSketchPolicy.KeepOrder)]
    [EnumValue("Keep Sketch", EnumFromSketchPolicy.FromSketch)]
    [EnumValue("Keep Exact Coordinates", EnumFromSketchPolicy.ExactCoordinates)]
    public EnumFromSketchPolicy SketchPolicyItem;

    public bool ShouldDisableSketchPolicyItem => !UseDrawingAsSketchItem && !SelectedElementsIncrementallyItem;

    [Label("Orientation")]
    [DefaultValue(LayoutOrientation.TopToBottom)]
    [OptionGroup("GeneralGroup", 20)]
    [EnumValue("Top to Bottom", LayoutOrientation.TopToBottom)]
    [EnumValue("Left to Right", LayoutOrientation.LeftToRight)]
    [EnumValue("Bottom to Top", LayoutOrientation.BottomToTop)]
    [EnumValue("Right to Left", LayoutOrientation.RightToLeft)]
    public LayoutOrientation OrientationItem;

    [Label("Layout Components Separately")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(false)]
    public bool LayoutComponentsSeparatelyItem { get; set; }

    [Label("Symmetry Optimization")]
    [DefaultValue(SymmetryOptimizationStrategy.Strong)]
    [OptionGroup("GeneralGroup", 40)]
    [EnumValue("Strong", SymmetryOptimizationStrategy.Strong)]
    [EnumValue("Weak", SymmetryOptimizationStrategy.Weak)]
    [EnumValue("None", SymmetryOptimizationStrategy.None)]
    public SymmetryOptimizationStrategy SymmetricPlacementItem { get; set; }

    [Label("Layout Subcomponents Separately")]
    [OptionGroup("GeneralGroup", 45)]
    public bool SubComponentsItem { get; set; }

    [Label("Stop Duration (sec)")]
    [MinMax(Min = 0, Max = 150)]
    [DefaultValue(5)]
    [OptionGroup("GeneralGroup", 50)]
    [ComponentType(ComponentTypes.Slider)]
    public int StopDurationItem;

    [Label("Node Distance")]
    [MinMax(Min = 0, Max = 100)]
    [DefaultValue(30.0d)]
    [OptionGroup("DistanceGroup", 10)]
    [ComponentType(ComponentTypes.Slider)]
    public double NodeDistanceItem { get; set; }

    [Label("Node to Edge Distance")]
    [MinMax(Min = 0, Max = 100)]
    [DefaultValue(15.0d)]
    [OptionGroup("DistanceGroup", 20)]
    [ComponentType(ComponentTypes.Slider)]
    public double NodeToEdgeDistanceItem { get; set; }

    [Label("Edge Distance")]
    [MinMax(Min = 0, Max = 100)]
    [DefaultValue(15.0d)]
    [OptionGroup("DistanceGroup", 30)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeDistanceItem { get; set; }

    [Label("Layer to Layer Distance")]
    [MinMax(Min = 0, Max = 100)]
    [DefaultValue(10.0d)]
    [OptionGroup("DistanceGroup", 40)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumLayerDistanceItem { get; set; }

    [Label("Edge Routing Style")]
    [OptionGroup("EdgeSettingsGroup", 10)]
    [DefaultValue(RoutingStyle.Orthogonal)]
    [EnumValue("Octilinear", RoutingStyle.Octilinear)]
    [EnumValue("Orthogonal", RoutingStyle.Orthogonal)]
    [EnumValue("Polyline", RoutingStyle.Polyline)]
    [EnumValue("Curved", RoutingStyle.Curved)]
    public RoutingStyle EdgeRoutingItem { get; set; }

    [Label("Backloop Routing")]
    [OptionGroup("EdgeSettingsGroup", 20)]
    [DefaultValue(false)]
    public bool BackloopRoutingItem { get; set; }

    [Label("Automatic Edge Grouping")]
    [OptionGroup("EdgeSettingsGroup", 40)]
    [DefaultValue(false)]
    public bool AutomaticEdgeGroupingEnabledItem { get; set; }

    [Label("Automatic Bus Routing")]
    [OptionGroup("EdgeSettingsGroup", 45)]
    public bool AutomaticBusRouting { get; set; }

    [Label("Highlight Critical Path")]
    [OptionGroup("EdgeSettingsGroup", 50)]
    [DefaultValue(false)]
    public bool HighlightCriticalPath { get; set; }

    [Label("Minimum First Segment Length")]
    [OptionGroup("EdgeSettingsGroup", 60)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumFirstSegmentLengthItem { get; set; }

    [Label("Minimum Last Segment Length")]
    [OptionGroup("EdgeSettingsGroup", 70)]
    [DefaultValue(15.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumLastSegmentLengthItem { get; set; }

    [Label("Minimum Edge Length")]
    [OptionGroup("EdgeSettingsGroup", 80)]
    [DefaultValue(20.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumEdgeLengthItem { get; set; }

    [Label("Minimum Edge Distance")]
    [OptionGroup("EdgeSettingsGroup", 90)]
    [DefaultValue(15.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumEdgeDistanceItem { get; set; }

    [MinMax(Min = 0.0d, Max = 5.0d, Step = 0.01d)]
    [Label("Minimum Slope")]
    [DefaultValue(0.25d)]
    [OptionGroup("EdgeSettingsGroup", 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumSlopeItem { get; set; }

    public bool ShouldDisableMinimumSlopeItem =>
        EdgeRoutingItem != RoutingStyle.Polyline && EdgeRoutingItem != RoutingStyle.Curved;

    [Label("Arrows Define Edge Direction")]
    [OptionGroup("EdgeSettingsGroup", 110)]
    public bool EdgeDirectednessItem { get; set; }

    [Label("Consider Edge Thickness")]
    [OptionGroup("EdgeSettingsGroup", 120)]
    public bool EdgeThicknessItem { get; set; }

    [Label("Recursive Edge Routing Policy")]
    [OptionGroup("EdgeSettingsGroup", 150)]
    [EnumValue("Off", RecursiveEdgePolicy.Off)]
    [EnumValue("Directed", RecursiveEdgePolicy.Directed)]
    [EnumValue("Undirected", RecursiveEdgePolicy.Undirected)]
    public RecursiveEdgePolicy RecursiveEdgePolicyItem { get; set; }

    [Label("U-turn Symmetry")]
    [OptionGroup("EdgeSettingsGroup", 160)]
    [DefaultValue(0)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.1d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CurveUTurnSymmetryItem { get; set; }

    public bool ShouldDisableCurveUTurnSymmetryItem => EdgeRoutingItem != RoutingStyle.Curved;

    [Label("Allow Shortcuts")]
    [OptionGroup("EdgeSettingsGroup", 170)]
    [DefaultValue(false)]
    public bool CurveShortcutsItem { get; set; }

    public bool ShouldDisableCurveShortcutsItem => EdgeRoutingItem != RoutingStyle.Curved;

    [Label("Layer Assignment Policy")]
    [OptionGroup("RankGroup", 10)]
    [DefaultValue(LayeringStrategy.HierarchicalOptimal)]
    [EnumValue("Hierarchical - Optimal", LayeringStrategy.HierarchicalOptimal)]
    [EnumValue("Hierarchical - Tight Tree Heuristic", LayeringStrategy.HierarchicalTightTree)]
    [EnumValue("BFS Layering", LayeringStrategy.Bfs)]
    [EnumValue("From Sketch", LayeringStrategy.FromSketch)]
    [EnumValue("Hierarchical - Topmost", LayeringStrategy.HierarchicalTopmost)]
    public LayeringStrategy RankingPolicyItem { get; set; }

    [Label("Alignment within Layer")]
    [OptionGroup("RankGroup", 20)]
    [DefaultValue(0.0d)]
    [EnumValue("Top Border of Nodes", 0d)]
    [EnumValue("Center of Nodes", 0.5d)]
    [EnumValue("Bottom Border of Nodes", 1d)]
    public double LayerAlignmentItem { get; set; }

    [Label("Component Arrangement")]
    [OptionGroup("RankGroup", 30)]
    [DefaultValue(ComponentArrangementPolicy.Topmost)]
    [EnumValue("Topmost", ComponentArrangementPolicy.Topmost)]
    [EnumValue("Compact", ComponentArrangementPolicy.Compact)]
    public ComponentArrangementPolicy ComponentArrangementPolicyItem { get; set; }

    [OptionGroup("RankGroup", 40)]
    [Label("Stacked Placement")]
    [DefaultValue(false)]
    public bool NodeCompactionItem { get; set; }

    [OptionGroup("RankGroup", 50)]
    [Label("From Sketch Layer Assignment")]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object SketchGroup;

    [OptionGroup("SketchGroup", 10)]
    [MinMax(Min = 0.0d, Max = 5.0d, Step = 0.01d)]
    [DefaultValue(1.0d)]
    [Label("Scale")]
    [ComponentType(ComponentTypes.Slider)]
    public double ScaleItem { get; set; }

    public bool ShouldDisableScaleItem => RankingPolicyItem != LayeringStrategy.FromSketch;

    [OptionGroup("SketchGroup", 20)]
    [Label("Node Margin")]
    [DefaultValue(0.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double NodeMarginItem { get; set; }

    public bool ShouldDisableMarginItem => RankingPolicyItem != LayeringStrategy.FromSketch;

    [OptionGroup("SketchGroup", 30)]
    [Label("Minimum Size")]
    [DefaultValue(0.0d)]
    [MinMax(Min = 0, Max = 1000)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumSizeItem { get; set; }

    public bool ShouldDisableMinimumSizeItem => RankingPolicyItem != LayeringStrategy.FromSketch;

    [OptionGroup("SketchGroup", 40)]
    [Label("Maximum Size")]
    [DefaultValue(1000.0d)]
    [MinMax(Min = 0, Max = 1000)]
    [ComponentType(ComponentTypes.Slider)]
    public double MaximumSizeItem { get; set; }

    public bool ShouldDisableMaximumSizeItem => RankingPolicyItem != LayeringStrategy.FromSketch;

    [OptionGroup("NodePropertiesGroup", 10)]
    [Label("Node Labeling")]
    [DefaultValue(NodeLabelPlacement.Consider)]
    [EnumValue("Consider", NodeLabelPlacement.Consider)]
    [EnumValue("Generic", NodeLabelPlacement.Generic)]
    [EnumValue("Ignore", NodeLabelPlacement.Ignore)]
    public NodeLabelPlacement NodeLabelingItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(EdgeLabelPlacement.Ignore)]
    [EnumValue("Ignore", EdgeLabelPlacement.Ignore)]
    [EnumValue("Integrated", EdgeLabelPlacement.Integrated)]
    [EnumValue("Generic", EdgeLabelPlacement.Generic)]
    public EdgeLabelPlacement EdgeLabelingItem { get; set; }

    [Label("Compact Placement")]
    [OptionGroup("EdgePropertiesGroup", 30)]
    [DefaultValue(true)]
    public bool CompactEdgeLabelPlacementItem { get; set; }

    public bool ShouldDisableCompactEdgeLabelPlacementItem => EdgeLabelingItem != EdgeLabelPlacement.Integrated;

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 40)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem => EdgeLabelingItem != EdgeLabelPlacement.Generic;

    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal", EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal", EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical", EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

    public bool ShouldDisableLabelPlacementOrientationItem => EdgeLabelingItem == EdgeLabelPlacement.Ignore;

    [Label("Along Edge")]
    [OptionGroup("PreferredPlacementGroup", 20)]
    [DefaultValue(EnumLabelPlacementAlongEdge.Centered)]
    [EnumValue("Anywhere", EnumLabelPlacementAlongEdge.Anywhere)]
    [EnumValue("At Source", EnumLabelPlacementAlongEdge.AtSource)]
    [EnumValue("At Source Port", EnumLabelPlacementAlongEdge.AtSourcePort)]
    [EnumValue("At Target", EnumLabelPlacementAlongEdge.AtTarget)]
    [EnumValue("At Target Port", EnumLabelPlacementAlongEdge.AtTargetPort)]
    [EnumValue("Centered", EnumLabelPlacementAlongEdge.Centered)]
    public EnumLabelPlacementAlongEdge LabelPlacementAlongEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementAlongEdgeItem => EdgeLabelingItem == EdgeLabelPlacement.Ignore;

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge", EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left", EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right", EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right", EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementSideOfEdgeItem => EdgeLabelingItem == EdgeLabelPlacement.Ignore;

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem =>
        EdgeLabelingItem == EdgeLabelPlacement.Ignore ||
        LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge;

    [OptionGroup("GroupingGroup", 10)]
    [Label("Layering Strategy")]
    [DefaultValue(GroupLayeringPolicy.Recursive)]
    [EnumValue("Ignore Groups", GroupLayeringPolicy.IgnoreGroups)]
    [EnumValue("Recursive", GroupLayeringPolicy.Recursive)]
    [EnumValue("Recursive Compact", GroupLayeringPolicy.RecursiveCompact)]
    public GroupLayeringPolicy GroupLayeringPolicyItem { get; set; }

    public bool ShouldDisableGroupLayeringStrategyItem => UseDrawingAsSketchItem;

    [OptionGroup("GroupingGroup", 20)]
    [Label("Vertical Alignment")]
    [DefaultValue(GroupAlignmentPolicy.Center)]
    [EnumValue("Top", GroupAlignmentPolicy.Top)]
    [EnumValue("Center", GroupAlignmentPolicy.Center)]
    [EnumValue("Bottom", GroupAlignmentPolicy.Bottom)]
    public GroupAlignmentPolicy GroupAlignmentItem { get; set; }

    public bool ShouldDisableGroupAlignmentItem => GroupLayeringPolicyItem == GroupLayeringPolicy.IgnoreGroups;

    [OptionGroup("GroupingGroup", 40)]
    [Label("Horizontal Group Compaction")]
    [DefaultValue(true)]
    public bool GroupHorizontalCompactionItem { get; set; }

    [OptionGroup("SwimlanesGroup", 10)]
    [Label("Treat Groups as Swimlanes")]
    [DefaultValue(false)]
    public bool TreatRootGroupAsSwimlanesItem { get; set; }

    [OptionGroup("SwimlanesGroup", 20)]
    [Label("Use Sketch for Lane Order")]
    [DefaultValue(false)]
    public bool UseOrderFromSketchItem { get; set; }

    public bool ShouldDisableUseOrderFromSketchItem => !TreatRootGroupAsSwimlanesItem;

    [OptionGroup("SwimlanesGroup", 30)]
    [Label("Lane Spacing")]
    [DefaultValue(0.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double SwimlineSpacingItem { get; set; }

    public bool ShouldDisableSwimlineSpacingItem => !TreatRootGroupAsSwimlanesItem;

    [OptionGroup("GridGroup", 10)]
    [Label("Grid")]
    [DefaultValue(false)]
    public bool GridEnabledItem { get; set; }

    [OptionGroup("GridGroup", 20)]
    [Label("Grid Spacing")]
    [DefaultValue(5.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double GridSpacingItem { get; set; }

    public bool ShouldDisableGridSpacingItem => !GridEnabledItem;

    [OptionGroup("GridGroup", 30)]
    [Label("Grid Port Style")]
    [DefaultValue(PortAssignmentMode.Default)]
    [EnumValue("Default", PortAssignmentMode.Default)]
    [EnumValue("On Grid", PortAssignmentMode.OnGrid)]
    [EnumValue("On Subgrid", PortAssignmentMode.OnSubgrid)]
    public PortAssignmentMode GridPortAssignmentItem { get; set; }

    public bool ShouldDisableGridPortAssignmentItem => !GridEnabledItem;

    public enum EnumFromSketchPolicy
    {
      KeepOrder,
      FromSketch,
      ExactCoordinates
    }
  }
}
