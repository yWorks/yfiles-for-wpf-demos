/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Labeling;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;
using yWorks.Utils;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("TreeLayout")]
  public class TreeLayoutConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public TreeLayoutConfig() {
      var aspectRatioSubtreePlacer = new AspectRatioSubtreePlacer();

      RoutingStyleForNonTreeEdgesItem = EnumRoute.Orthogonal;
      EdgeBundlingStrengthItem = 0.95;
      ActOnSelectionOnlyItem = false;

      DefaultLayoutOrientationItem = LayoutOrientation.TopToBottom;

      NodeLabelingItem = NodeLabelPlacement.Consider;

      SubtreePlacerItem = EnumSubtreePlacer.SingleLayer;

      SpacingItem = 30;
      RootAlignmentItem = EnumRootAlignment.Center;
      AlignPortsItem = false;
      AllowMultiParentsItem = false;
      PortAssignmentItem = PortAssignment.Center;

      SubtreePlacerAspectRatioItem = aspectRatioSubtreePlacer.AspectRatio;

      EdgeLabelingItem = EdgeLabelPlacement.Integrated;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = SubtreePlacerItem != EnumSubtreePlacer.HV
          ? ConfigureDefaultLayout()
          // use a default TreeLayout to show the 'Horizontal-Vertical' style
          : new TreeLayout();

      layout.ParallelEdgeRouter.Enabled = false;
      layout.ComponentLayout.Style = ComponentArrangementStyle.MultiRows;

      var subgraphLayout = layout.LayoutStages.Get<SubgraphLayoutStage>();
      subgraphLayout.Enabled = ActOnSelectionOnlyItem;

      ConfigureTreeReductionStage(layout.TreeReductionStage);

      var placeLabels = EdgeLabelingItem == EdgeLabelPlacement.Integrated ||
          EdgeLabelingItem == EdgeLabelPlacement.Generic;

      // required to prevent WrongGraphStructure exception which may be thrown by TreeLayout if there are edges
      // between group nodes
      layout.LayoutStages.Prepend(new HandleEdgesBetweenGroupsStage(placeLabels));

      layout.NodeLabelPlacement = NodeLabelingItem;

      if (EdgeLabelingItem == EdgeLabelPlacement.Generic) {
        layout.EdgeLabelPlacement = EdgeLabelPlacement.Generic;

        var labeling = layout.LayoutStages.Get<GenericLabeling>();
        labeling.Scope = LabelingScope.EdgeLabels;
        if (ReduceAmbiguityItem) {
          labeling.DefaultEdgeLabelingCosts.AmbiguousPlacementCost = 1.0;
        }
      } else if (EdgeLabelingItem == EdgeLabelPlacement.Integrated) {
        layout.EdgeLabelPlacement = EdgeLabelPlacement.Integrated;
      }

      return layout;
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      TreeLayoutData<INode, IEdge, ILabel, ILabel> layoutData;
      if (SubtreePlacerItem == EnumSubtreePlacer.HV) {
        layoutData = CreateLayoutDataHorizontalVertical(graphControl);
      } else if (SubtreePlacerItem == EnumSubtreePlacer.SingleSplitLayered) {
        layoutData = CreateLayoutDataSingleSplitPlacer(graphControl);
      } else {
        var graph = graphControl.Graph;
        layoutData = new TreeLayoutData<INode, IEdge, ILabel, ILabel> {
            MultiLayerSubtreePlacerLayerIndices = {
                MapperFunction = node => {
                  var predecessors = graph.Predecessors(node);
                  var parent = predecessors.FirstOrDefault();
                  if (parent != null) {
                    var siblings = graph.Successors(parent).ToList();
                    return siblings.IndexOf(node) % (int) Math.Round(Math.Sqrt(siblings.Count));
                  }
                  return 0;
                }
            },
            LeftRightSubtreePlacerLeftNodes = {
                Predicate = node => {
                  var predecessors = graph.Predecessors(node);
                  var parent = predecessors.FirstOrDefault();
                  if (parent != null) {
                    var siblings = graph.Successors(parent).ToList();
                    return siblings.IndexOf(node) % 2 != 0;
                  }
                  return false;
                }
            },
            CompactSubtreePlacerStrategyMementos = new DictionaryMapper<INode, object>()
            // AssistantNodes = {Delegate = node => node.Tag != null ? node.Tag.Assistant : null}
        };
      }

      if (PortAssignmentItem != PortAssignment.Center) {
        layoutData.Ports.SourcePortCandidates.Constant = CreatePortCandidate(PortAssignmentItem);
        layoutData.Ports.TargetPortCandidates.Constant = CreatePortCandidate(PortAssignmentItem);
      }

      var combinedData = layoutData.CombineWith(CreateLabelingLayoutData(graphControl.Graph,
          LabelPlacementAlongEdgeItem, LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem,
          LabelPlacementDistanceItem));
      if (ActOnSelectionOnlyItem) {
        combinedData = combinedData.CombineWith(CreateSubgraphLayoutData(graphControl));
      }
      return combinedData;
    }

    private EdgePortCandidates CreatePortCandidate(PortAssignment portAssignment) {
      switch (portAssignment) {
        case PortAssignment.Center:
          throw new ArgumentException("Center cannot be translated to port side");
        case PortAssignment.DistributedTop:
          return new EdgePortCandidates().AddFreeCandidate(PortSides.Top);
          break;
        case PortAssignment.DistributedBottom:
          return new EdgePortCandidates().AddFreeCandidate(PortSides.Bottom);
          break;
        case PortAssignment.DistributedLeft:
          return new EdgePortCandidates().AddFreeCandidate(PortSides.Left);
          break;
        case PortAssignment.DistributedRight:
          return new EdgePortCandidates().AddFreeCandidate(PortSides.Right);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(portAssignment), portAssignment, null);
      }
    }

    private TreeLayoutData<INode, IEdge, ILabel, ILabel> CreateLayoutDataHorizontalVertical(GraphControl graphControl) {
      return new TreeLayoutData<INode, IEdge, ILabel, ILabel> {
          SubtreePlacers = {
              MapperFunction = node => {
                // children of selected nodes should be placed vertical and to the right of their child nodes, while
                // the children of non-selected horizontal downwards
                var subtreeTransform = graphControl.Selection.Contains(node)
                    ? SubtreeTransform.RotateLeftFlipY
                    : SubtreeTransform.None;

                var singleLayerSubtreePlacer = new SingleLayerSubtreePlacer(subtreeTransform,
                    SingleLayerSubtreePlacerRootAlignment.LeadingOnBus, verticalDistance : SpacingItem,
                    horizontalDistance : SpacingItem);
                singleLayerSubtreePlacer.AlignPorts = AlignPortsItem;
                return singleLayerSubtreePlacer;
              }
          }
      };
      // TOOD Labeling?
    }


    private TreeLayoutData<INode, IEdge, ILabel, ILabel>
        CreateLayoutDataSingleSplitPlacer(GraphControl graphComponent) {
      var graph = graphComponent.Graph;
      // half the subtrees are delegated to the left placer and half to the right placer
      var leftNodes = new HashSet<INode>();

      // note that the given graph may be a non-tree structure
      var root = graph.Nodes.FirstOrDefault(node => graph.InDegree(node) == 0) ?? graph.Nodes.First();
      var left = true;
      var seenNodes = new HashSet<INode>();
      seenNodes.Add(root);
      foreach (var successor in graph.Successors(root)) {
        var stack = new List<INode> { successor };
        while (stack.Count > 0) {
          var child = stack[stack.Count - 1];
          stack.RemoveAt(stack.Count - 1);
          if (left) {
            leftNodes.Add(child);
          } // else: right node

          // push successors on stack -> whole subtree is either left or right
          foreach (var succSucc in graph.Successors(child)) {
            if (seenNodes.Add(succSucc)) {
              stack.Add(succSucc);
            }
          }
        }
        left = !left;
      }

      var layoutData = new TreeLayoutData<INode, IEdge, ILabel, ILabel> {
          SingleSplitSubtreePlacerPrimaryNodes = { Predicate = node => leftNodes.Contains(node) },
          // tells the layout which subtree placer to use for a node
          SubtreePlacers = {
              MapperFunction = node => {
                if (node == root) {
                  return singleSplitRootPlacer;
                }
                if (leftNodes.Contains(node)) {
                  return singleSplitLeftPlacer;
                }
                return singleSplitRightPlacer;
              }
          }
      };
      layoutData.TreeRoot.Item = root;
      return layoutData;
    }

    /// <summary>
    ///   Configures the tree reduction stage that will handle edges that do not belong to the tree.
    /// </summary>
    /// <param name="reductionStage"></param>
    private void ConfigureTreeReductionStage(TreeReductionStage reductionStage) {
      if (EdgeLabelingItem == EdgeLabelPlacement.Integrated) {
        reductionStage.NonTreeEdgeLabeling = new GenericLabeling();
      }
      reductionStage.AllowMultiParent = (SubtreePlacerItem == EnumSubtreePlacer.SingleLayer ||
          SubtreePlacerItem == EnumSubtreePlacer.Bus || SubtreePlacerItem == EnumSubtreePlacer.LeftRight ||
          SubtreePlacerItem == EnumSubtreePlacer.Dendrogram) && AllowMultiParentsItem;

      if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Organic) {
        reductionStage.NonTreeEdgeRouter = new OrganicEdgeRouter();
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Orthogonal) {
        var edgeRouter = new EdgeRouter { Rerouting = true };
        reductionStage.NonTreeEdgeRouter = edgeRouter;
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.StraightLine) {
        reductionStage.NonTreeEdgeRouter = new StraightLineEdgeRouter();
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Bundled) {
        var ebc = reductionStage.EdgeBundling;
        ebc.BundlingStrength = EdgeBundlingStrengthItem;
        ebc.DefaultBundleDescriptor = new EdgeBundleDescriptor { Bundled = true };
      }
    }

    private TreeLayout ConfigureDefaultLayout() {
      var layout = new TreeLayout {
          LayoutOrientation = SubtreePlacerItem == EnumSubtreePlacer.AspectRatio
              ? LayoutOrientation.TopToBottom
              : DefaultLayoutOrientationItem
      };

      var rootAlignment1 = SingleLayerSubtreePlacerRootAlignment.Center;
      var rootAlignment2 = SubtreeRootAlignment.Center;
      switch (RootAlignmentItem) {
        case EnumRootAlignment.Center:
          rootAlignment1 = SingleLayerSubtreePlacerRootAlignment.Center;
          rootAlignment2 = SubtreeRootAlignment.Center;
          break;
        case EnumRootAlignment.Median:
          rootAlignment1 = SingleLayerSubtreePlacerRootAlignment.Median;
          rootAlignment2 = SubtreeRootAlignment.Median;
          break;
        case EnumRootAlignment.Left:
          rootAlignment1 = SingleLayerSubtreePlacerRootAlignment.Left;
          rootAlignment2 = SubtreeRootAlignment.Left;
          break;
        case EnumRootAlignment.Leading:
          rootAlignment1 = SingleLayerSubtreePlacerRootAlignment.Leading;
          rootAlignment2 = SubtreeRootAlignment.Leading;
          break;
        case EnumRootAlignment.Right:
          rootAlignment1 = SingleLayerSubtreePlacerRootAlignment.Right;
          rootAlignment2 = SubtreeRootAlignment.Right;
          break;
        case EnumRootAlignment.Trailing:
          rootAlignment1 = SingleLayerSubtreePlacerRootAlignment.Trailing;
          rootAlignment2 = SubtreeRootAlignment.Trailing;
          break;
      }

      var allowMultiParents = AllowMultiParentsItem;

      switch (SubtreePlacerItem) {
        case EnumSubtreePlacer.SingleLayer:
          layout.DefaultSubtreePlacer = new SingleLayerSubtreePlacer {
              HorizontalDistance = SpacingItem,
              VerticalDistance = SpacingItem,
              RootAlignment = rootAlignment1,
              AlignPorts = AlignPortsItem
          };
          layout.AllowMultiParent = allowMultiParents;
          break;
        case EnumSubtreePlacer.Bus:
          layout.DefaultSubtreePlacer = new BusSubtreePlacer { Spacing = SpacingItem };
          layout.AllowMultiParent = allowMultiParents;
          break;
        case EnumSubtreePlacer.DoubleLayer:
          layout.DefaultSubtreePlacer = new DoubleLayerSubtreePlacer {
              Spacing = SpacingItem, RootAlignment = rootAlignment2, AlignPorts = AlignPortsItem
          };
          break;
        case EnumSubtreePlacer.LeftRight:
          layout.DefaultSubtreePlacer =
              new LeftRightSubtreePlacer { Spacing = SpacingItem, AlignPorts = AlignPortsItem };
          layout.AllowMultiParent = allowMultiParents;
          break;
        case EnumSubtreePlacer.LevelAligned:
          layout.DefaultSubtreePlacer = new LevelAlignedSubtreePlacer {
              Spacing = SpacingItem,
              LayerSpacing = SpacingItem,
              RootAlignment = rootAlignment2,
              AlignPorts = AlignPortsItem
          };
          break;
        case EnumSubtreePlacer.AspectRatio:
          layout.DefaultSubtreePlacer = new AspectRatioSubtreePlacer {
              HorizontalDistance = SpacingItem,
              VerticalDistance = SpacingItem,
              AspectRatio = SubtreePlacerAspectRatioItem
          };
          break;
        case EnumSubtreePlacer.Dendrogram:
          layout.DefaultSubtreePlacer = new DendrogramSubtreePlacer {
              MinimumRootDistance = SpacingItem, MinimumSubtreeDistance = SpacingItem
          };
          layout.AllowMultiParent = allowMultiParents;
          break;
        case EnumSubtreePlacer.MultiLayer:
          layout.DefaultSubtreePlacer = new MultiLayerSubtreePlacer {
              Spacing = SpacingItem, RootAlignment = (MultiLayerSubtreePlacerRootAlignment) rootAlignment2
          };
          break;
        case EnumSubtreePlacer.Compact:
          layout.DefaultSubtreePlacer = new CompactSubtreePlacer {
              HorizontalDistance = SpacingItem,
              VerticalDistance = SpacingItem,
              PreferredAspectRatio = SubtreePlacerAspectRatioItem
          };
          break;
        case EnumSubtreePlacer.SingleSplitLayered:
          singleSplitLeftPlacer = new LevelAlignedSubtreePlacer(SubtreeTransform.RotateRight) {
              Transformation = SubtreeTransform.RotateRight,
              VerticalAlignment = 0,
              EdgeRoutingStyle = LevelAlignedSubtreePlacerRoutingStyle.Orthogonal,
              Spacing = SpacingItem,
              LayerSpacing = SpacingItem,
              RootAlignment = rootAlignment2,
              AlignPorts = AlignPortsItem
          };

          singleSplitRightPlacer = new LevelAlignedSubtreePlacer(SubtreeTransform.RotateLeft) {
              Transformation = SubtreeTransform.RotateLeft,
              VerticalAlignment = 0,
              EdgeRoutingStyle = LevelAlignedSubtreePlacerRoutingStyle.Orthogonal,
              LayerSpacing = SpacingItem,
              RootAlignment = rootAlignment2,
              AlignPorts = AlignPortsItem
          };

          singleSplitRootPlacer = new SingleSplitSubtreePlacer(singleSplitLeftPlacer, singleSplitRightPlacer);
          break;
      }

      layout.DefaultPortAssigner = new TreeLayoutPortAssigner(PortAssignmentItem == PortAssignment.Center
          ? PortAssignmentMode.Center
          : PortAssignmentMode.Distributed);

      return layout;
    }

    private ISubtreePlacer singleSplitRootPlacer;
    private ISubtreePlacer singleSplitLeftPlacer;
    private ISubtreePlacer singleSplitRightPlacer;

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

    [Label("Subtree Placer")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object SubtreePlacerGroup;

    [Label("Edges")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object EdgesGroup;

    [Label("Non-Tree Edges")]
    [OptionGroup("EdgesGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object NonTreeEdgesGroup;

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

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    public enum EnumRoute
    {
      Orthogonal,
      Organic,
      StraightLine,
      Bundled
    }

    public enum EnumSubtreePlacer
    {
      SingleLayer,
      Bus,
      DoubleLayer,
      LeftRight,
      LevelAligned,
      AspectRatio,
      Dendrogram,
      MultiLayer,
      Compact,
      HV,
      SingleSplitLayered
    }

    public enum EnumRootAlignment
    {
      Center,
      Median,
      Left,
      Leading,
      Right,
      Trailing
    }

    /// <summary>
    ///   Gets the description text.
    /// </summary>
    /// <value>
    ///   The description text.
    /// </value>
    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>The various flavors of the tree layout styles are great for highlighting child-parent relationships in graphs that form one or more trees, " +
        "or trees with only few additional edges.</Paragraph>" +
        "<Paragraph>The need to visualize directed or undirected trees arises in many application areas, for example</Paragraph>" +
        "<List>" + "<ListItem><Paragraph>Dataflow analysis</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Software engineering</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Network management</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Bioinformatics</Paragraph></ListItem>" + "</List>";

    [Label("Routing Style for Non-Tree Edges")]
    [OptionGroup("NonTreeEdgesGroup", 10)]
    [DefaultValue(EnumRoute.Bundled)]
    [EnumValue("Orthogonal", EnumRoute.Orthogonal)]
    [EnumValue("Organic", EnumRoute.Organic)]
    [EnumValue("Straight-Line", EnumRoute.StraightLine)]
    [EnumValue("Bundled", EnumRoute.Bundled)]
    public EnumRoute RoutingStyleForNonTreeEdgesItem { get; set; }

    [Label("Bundling Strength")]
    [OptionGroup("NonTreeEdgesGroup", 20)]
    [DefaultValue(0.95d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }

    public bool ShouldDisableEdgeBundlingStrengthItem => RoutingStyleForNonTreeEdgesItem != EnumRoute.Bundled;

    [Label("Act on Selection Only")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(false)]
    public bool ActOnSelectionOnlyItem { get; set; }

    [Label("Node Labeling")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(NodeLabelPlacement.Consider)]
    [EnumValue("Consider", NodeLabelPlacement.Consider)]
    [EnumValue("Generic", NodeLabelPlacement.Generic)]
    [EnumValue("Ignore", NodeLabelPlacement.Ignore)]
    public NodeLabelPlacement NodeLabelingItem { get; set; }

    [Label("Subtree Placer")]
    [OptionGroup("SubtreePlacerGroup", 10)]
    [EnumValue("Single Layer", EnumSubtreePlacer.SingleLayer)]
    [EnumValue("Compact", EnumSubtreePlacer.Compact)]
    [EnumValue("Bus", EnumSubtreePlacer.Bus)]
    [EnumValue("Double Layer", EnumSubtreePlacer.DoubleLayer)]
    [EnumValue("Left-Right", EnumSubtreePlacer.LeftRight)]
    [EnumValue("Level-Aligned", EnumSubtreePlacer.LevelAligned)]
    [EnumValue("Aspect Ratio", EnumSubtreePlacer.AspectRatio)]
    [EnumValue("Dendrogram", EnumSubtreePlacer.Dendrogram)]
    [EnumValue("Multi-Layer", EnumSubtreePlacer.MultiLayer)]
    [EnumValue("Horizontal-Vertical", EnumSubtreePlacer.HV)]
    [EnumValue("Single Split Layered", EnumSubtreePlacer.SingleSplitLayered)]
    public EnumSubtreePlacer SubtreePlacerItem { get; set; }

    [Label("Spacing")]
    [OptionGroup("SubtreePlacerGroup", 20)]
    [MinMax(Min = 0, Max = 500)]
    [ComponentType(ComponentTypes.Slider)]
    public double SpacingItem { get; set; }

    [Label("Root Alignment")]
    [OptionGroup("SubtreePlacerGroup", 30)]
    [EnumValue("Center", EnumRootAlignment.Center)]
    [EnumValue("Median", EnumRootAlignment.Median)]
    [EnumValue("Left", EnumRootAlignment.Left)]
    [EnumValue("Leading", EnumRootAlignment.Leading)]
    [EnumValue("Right", EnumRootAlignment.Right)]
    [EnumValue("Trailing", EnumRootAlignment.Trailing)]
    public EnumRootAlignment RootAlignmentItem { get; set; }

    public bool ShouldDisableRootAlignmentItem =>
        SubtreePlacerItem == EnumSubtreePlacer.AspectRatio || SubtreePlacerItem == EnumSubtreePlacer.Bus ||
        SubtreePlacerItem == EnumSubtreePlacer.Dendrogram || SubtreePlacerItem == EnumSubtreePlacer.Compact;

    [Label("Align Ports")]
    [OptionGroup("SubtreePlacerGroup", 40)]
    [DefaultValue(false)]
    public bool AlignPortsItem { get; set; }

    public bool ShouldDisableAlignPortsItem =>
        (SubtreePlacerItem != EnumSubtreePlacer.SingleLayer && SubtreePlacerItem != EnumSubtreePlacer.Bus &&
            SubtreePlacerItem != EnumSubtreePlacer.DoubleLayer && SubtreePlacerItem != EnumSubtreePlacer.LeftRight &&
            SubtreePlacerItem != EnumSubtreePlacer.LevelAligned && SubtreePlacerItem != EnumSubtreePlacer.MultiLayer &&
            SubtreePlacerItem != EnumSubtreePlacer.SingleSplitLayered && SubtreePlacerItem != EnumSubtreePlacer.HV) ||
        (RootAlignmentItem != EnumRootAlignment.Center && RootAlignmentItem != EnumRootAlignment.Median &&
            RootAlignmentItem != EnumRootAlignment.Left && RootAlignmentItem != EnumRootAlignment.Right);

    [Label("Orientation")]
    [OptionGroup("GeneralGroup", 5)]
    [EnumValue("Top to Bottom", LayoutOrientation.TopToBottom)]
    [EnumValue("Left to Right", LayoutOrientation.LeftToRight)]
    [EnumValue("Bottom to Top", LayoutOrientation.BottomToTop)]
    [EnumValue("Right to Left", LayoutOrientation.RightToLeft)]
    public LayoutOrientation DefaultLayoutOrientationItem { get; set; }

    public bool ShouldDisableDefaultLayoutOrientationItem =>
        SubtreePlacerItem == EnumSubtreePlacer.AspectRatio || SubtreePlacerItem == EnumSubtreePlacer.Compact;

    [Label("Aspect Ratio")]
    [OptionGroup("SubtreePlacerGroup", 50)]
    [MinMax(Min = 0.1, Max = 4, Step = 0.01)]
    [ComponentType(ComponentTypes.Slider)]
    public double SubtreePlacerAspectRatioItem { get; set; }

    public bool ShouldDisableSubtreePlacerAspectRatioItem =>
        SubtreePlacerItem != EnumSubtreePlacer.AspectRatio && SubtreePlacerItem != EnumSubtreePlacer.Compact;

    [Label("Allow Multi-Parents")]
    [OptionGroup("SubtreePlacerGroup", 60)]
    public bool AllowMultiParentsItem { get; set; }

    public bool ShouldDisableAllowMultiParentsItem =>
        SubtreePlacerItem != EnumSubtreePlacer.SingleLayer && SubtreePlacerItem != EnumSubtreePlacer.Dendrogram &&
        SubtreePlacerItem != EnumSubtreePlacer.Bus && SubtreePlacerItem != EnumSubtreePlacer.LeftRight;

    [Label("Port Assignment")]
    [OptionGroup("EdgesGroup", 10)]
    [EnumValue("None", PortAssignment.Center)]
    [EnumValue("Distributed Top", PortAssignment.DistributedTop)]
    [EnumValue("Distributed Bottom", PortAssignment.DistributedBottom)]
    [EnumValue("Distributed Right", PortAssignment.DistributedRight)]
    [EnumValue("Distributed Left", PortAssignment.DistributedLeft)]
    public PortAssignment PortAssignmentItem { get; set; }

    private EdgeLabelPlacement edgeLabelingItem;

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(EdgeLabelPlacement.Ignore)]
    [EnumValue("Ignore", EdgeLabelPlacement.Ignore)]
    [EnumValue("Integrated", EdgeLabelPlacement.Integrated)]
    [EnumValue("Generic", EdgeLabelPlacement.Generic)]
    public EdgeLabelPlacement EdgeLabelingItem {
      get => edgeLabelingItem;
      set {
        edgeLabelingItem = value;
        if (value == EdgeLabelPlacement.Integrated) {
          LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Parallel;
          LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.AtTarget;
          LabelPlacementDistanceItem = 0;
        }
      }
    }

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
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

    /// <summary>
    ///   This stage temporarily removes edges that are incident to group nodes.
    /// </summary>
    /// <remarks>
    ///   The stage must be prepended to the layout algorithm and applies the following three steps:
    ///   <list type="number">
    ///     <item>Removes edges from the graph that are incident to group nodes.</item>
    ///     <item>Invokes the core layout algorithm on the reduced graph.</item>
    ///     <item>Re-inserts all previously removed edges and optionally places their labels.</item>
    ///   </list>
    ///   This stage can be useful for layout algorithms or stages that cannot handle edges between group nodes,
    ///   e.g., <see cref="TreeReductionStage"/>. Optionally, <see cref="HandleEdgesBetweenGroupsStage"/> can also
    ///   place the labels of the edges that were temporarily removed right after they are restored back to the graph.
    ///   <para>
    ///     The routing of the temporarily hidden edges can be customized by specifying an
    ///     <see cref="MarkedEdgeRouter">edge routing algorithm</see> for those edges.
    ///   </para>
    /// </remarks>
    private class HandleEdgesBetweenGroupsStage : LayoutStageBase
    {
      public HandleEdgesBetweenGroupsStage(bool placeLabels) {
        ConsiderEdgeLabels = placeLabels;
      }

      /// <summary>
      ///   Gets or sets the edge routing algorithm that is applied to the set of marked edges.
      /// </summary>
      /// <remarks>
      ///   Note that is required that a suitable edge selection key is specified and the router's scope
      ///   is reduced to the affected edges.
      /// </remarks>
      public ILayoutAlgorithm MarkedEdgeRouter { get; }

      /// <summary>
      ///   Gets or sets a value indicating whether the stage should place the labels of the edges that
      ///   have been temporarily hidden, when these edges will be restored back.
      /// </summary>
      public bool ConsiderEdgeLabels { get; }

      /// <summary>
      ///   Removes all edges that are incident to group nodes and passes it to the core layout algorithm.
      /// </summary>
      /// <remarks>
      ///   This stage removes some edges from the graph such that no edges incident to group nodes
      ///   exist. Then, it applies the core layout algorithm to the reduced graph.
      ///   After it produces the result, it re-inserts the previously removed edges and routes them.
      /// </remarks>
      protected override void ApplyLayoutImpl(LayoutGraph graph) {
        var groupingSupport = LayoutGraphGrouping.CreateReadOnlyView(graph);

        if (!LayoutGraphGrouping.IsGrouped(graph)) {
          CoreLayout?.ApplyLayout(graph);
        } else {
          var hiddenEdgesMap = new DictionaryMapper<Edge, bool>();

          var edgeHider = new LayoutGraphHider(graph);

          var existHiddenEdges = false;
          foreach (var edge in graph.Edges) {
            if (groupingSupport.IsGroupNode(edge.Source) || groupingSupport.IsGroupNode(edge.Target)) {
              hiddenEdgesMap[edge] = true;
              edgeHider.Hide(edge);
              existHiddenEdges = true;
            } else {
              hiddenEdgesMap[edge] = false;
            }
          }

          CoreLayout?.ApplyLayout(graph);

          if (existHiddenEdges) {
            edgeHider.UnhideAll();

            // routes the marked edges
            RouteMarkedEdges(graph, hiddenEdgesMap);

            if (ConsiderEdgeLabels) {
              // all labels of hidden edges should be marked
              var affectedLabelsDataKey = new EdgeLabelDataKey<bool>(
                  "Demo.yFiles.Layout.Configurations.TreeLayoutConfig.HandleEdgesBetweenGroupsStage.AffectedLabelsDataKey");
              var nonTreeLabelsMap = new DictionaryMapper<EdgeLabel, bool>();

              foreach (var edge in graph.Edges) {
                foreach (var edgeLabel in edge.Labels) {
                  nonTreeLabelsMap[edgeLabel] = hiddenEdgesMap[edge];
                }
              }

              // place marked labels
              var labeling = new GenericLabeling { Scope = LabelingScope.EdgeLabels };
              var labelingData = labeling.CreateLayoutData(graph);
              labelingData.Scope.EdgeLabels.Mapper = nonTreeLabelsMap;
              graph.ApplyLayout(labeling, labelingData);
            }
          }
        }
      }

      private void RouteMarkedEdges(LayoutGraph graph, IMapper<Edge, bool> markedEdgesMap) {
        if (MarkedEdgeRouter == null) {
          return;
        }
        // temporarily add the selected edges to the layout context
        var contextStage = new ContextModificationStage(MarkedEdgeRouter);
        contextStage.AddReplacementMap(LayoutKeys.RouteEdgesDataKey, markedEdgesMap);

        contextStage.ApplyLayout(graph);
      }
    }

    public enum PortAssignment
    {
      Center,
      DistributedTop,
      DistributedBottom,
      DistributedLeft,
      DistributedRight
    }
  }
}
