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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;
using yWorks.Layout.Tree;
using GroupingSupport = yWorks.Layout.Grouping.GroupingSupport;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("TreeLayout")]
  public class TreeLayoutConfig : LayoutConfiguration
  {

    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public TreeLayoutConfig() {
      var aspectRatioNodePlacer = new AspectRatioNodePlacer();
      
      RoutingStyleForNonTreeEdgesItem = EnumRoute.Orthogonal;
      EdgeBundlingStrengthItem = 0.95;
      ActOnSelectionOnlyItem = false;

      DefaultLayoutOrientationItem = LayoutOrientation.TopToBottom;
      
      ConsiderNodeLabelsItem = false;
      
      NodePlacerItem = EnumNodePlacer.Default;

      SpacingItem = 20;
      RootAlignmentItem = EnumRootAlignment.Center;
      AlignPortsItem = false;
      AllowMultiParentsItem = false;
      PortAssignmentItem = PortAssignmentMode.None;
      
      NodePlacerAspectRatioItem = aspectRatioNodePlacer.AspectRatio;
      
      EdgeLabelingItem = EnumEdgeLabeling.None;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = NodePlacerItem != EnumNodePlacer.HV 
          ? ConfigureDefaultLayout()
          // use a default TreeLayout to show the 'Horizontal-Vertical' style
          : new TreeLayout();

      layout.ParallelEdgeRouterEnabled = false;
      ((ComponentLayout) layout.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;
      layout.SubgraphLayoutEnabled = ActOnSelectionOnlyItem;

      layout.PrependStage(CreateTreeReductionStage());

      var placeLabels = EdgeLabelingItem == EnumEdgeLabeling.Integrated || EdgeLabelingItem == EnumEdgeLabeling.Generic;

      // required to prevent WrongGraphStructure exception which may be thrown by TreeLayout if there are edges
      // between group nodes
      layout.PrependStage(new HandleEdgesBetweenGroupsStage(placeLabels));

      layout.ConsiderNodeLabels = ConsiderNodeLabelsItem;

      if (EdgeLabelingItem == EnumEdgeLabeling.Generic) {
        layout.IntegratedEdgeLabeling = false;

        var labeling = new GenericLabeling();
        labeling.PlaceEdgeLabels = true;
        labeling.PlaceNodeLabels = false;
        labeling.ReduceAmbiguity = ReduceAmbiguityItem;
        layout.LabelingEnabled = true;
        layout.Labeling = labeling;
      } else if (EdgeLabelingItem == EnumEdgeLabeling.Integrated) {
        layout.IntegratedEdgeLabeling = true;
      }

      return layout;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      LayoutData layoutData;
      if (NodePlacerItem == EnumNodePlacer.HV) {
        layoutData = CreateLayoutDataHorizontalVertical(graphControl);
      } else if (NodePlacerItem == EnumNodePlacer.DelegatingLayered) {
        layoutData = CreateLayoutDataDelegatingPlacer(graphControl);
      } else {
        var graph = graphControl.Graph;
        layoutData = new TreeLayoutData {
            GridNodePlacerRowIndices = {
                Delegate = node => {
                  var predecessors = graph.Predecessors(node);
                  var parent = predecessors.FirstOrDefault();
                  if (parent != null) {
                    var siblings = graph.Successors(parent).ToList();
                    return siblings.IndexOf(node) % (int) Math.Round(Math.Sqrt(siblings.Count));
                  }
                  return 0;
                }
            },
            LeftRightNodePlacerLeftNodes = {
                Delegate = node => {
                  var predecessors = graph.Predecessors(node);
                  var parent = predecessors.FirstOrDefault();
                  if (parent != null) {
                    var siblings = graph.Successors(parent).ToList();
                    return siblings.IndexOf(node) % 2 != 0;
                  }
                  return false;
                }
            },
            CompactNodePlacerStrategyMementos = new DictionaryMapper<INode, object>(),
            // AssistantNodes = {Delegate = node => node.Tag != null ? node.Tag.Assistant : null}
        };
      }

      return layoutData.CombineWith(
          CreateLabelingLayoutData(
              graphControl.Graph,
              LabelPlacementAlongEdgeItem,
              LabelPlacementSideOfEdgeItem,
              LabelPlacementOrientationItem,
              LabelPlacementDistanceItem
          )
      );
    }

    private LayoutData CreateLayoutDataHorizontalVertical(GraphControl graphControl) {
      return new TreeLayoutData {
          NodePlacers = {
              Delegate = node => {
                // children of selected nodes should be placed vertical and to the right of their child nodes, while
                // the children of non-selected horizontal downwards
                var childPlacement = graphControl.Selection.IsSelected(node)
                    ? ChildPlacement.VerticalToRight
                    : ChildPlacement.HorizontalDownward;

                var defaultNodePlacer = new DefaultNodePlacer(childPlacement, RootAlignment.LeadingOnBus, SpacingItem, SpacingItem);
                defaultNodePlacer.AlignPorts = AlignPortsItem;
                return defaultNodePlacer;
              }
          }
      };
      // TOOD Labeling?
    }


    private TreeLayoutData CreateLayoutDataDelegatingPlacer(GraphControl graphComponent) {
      var graph = graphComponent.Graph;
      //half the subtrees are delegated to the left placer and half to the right placer
      var leftNodes = new HashSet<INode>();
      var root = graph.Nodes.First(node => graph.InDegree(node) == 0);
      var left = true;
      foreach (var successor in graph.Successors(root)) {
        var stack = new List<INode> { successor };
        while (stack.Count > 0) {
          var child = stack[stack.Count - 1];
          stack.RemoveAt(stack.Count - 1);
          if (left) {
            leftNodes.Add(child);
          } // else: right node
          //push successors on stack -> whole subtree is either left or right
          stack.AddRange(graph.Successors(child));
        }
        left = !left;
      }

      var layoutData = new TreeLayoutData {
          DelegatingNodePlacerPrimaryNodes = { Delegate = node => leftNodes.Contains(node) },
          // tells the layout which node placer to use for a node
          NodePlacers = {
              Delegate = node => {
                if (node == root) {
                  return delegatingRootPlacer;
                }
                if (leftNodes.Contains(node)) {
                  return delegatingLeftPlacer;
                }
                return delegatingRightPlacer;
              }
          }
      };
      layoutData.TreeRoot.Item = root;
      return layoutData;
    }

    /// <summary>
    /// Configures the tree reduction stage that will handle edges that do not belong to the tree.
    /// </summary>
    private TreeReductionStage CreateTreeReductionStage() {
      var reductionStage = new TreeReductionStage();
      if (EdgeLabelingItem == EnumEdgeLabeling.Integrated) {
        reductionStage.NonTreeEdgeLabelingAlgorithm = new GenericLabeling();
      }
      reductionStage.MultiParentAllowed = 
          (NodePlacerItem == EnumNodePlacer.Default ||
           NodePlacerItem == EnumNodePlacer.Bus ||
           NodePlacerItem == EnumNodePlacer.LeftRight ||
           NodePlacerItem == EnumNodePlacer.Dendrogram) &&
          AllowMultiParentsItem;

      if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Organic) {
        reductionStage.NonTreeEdgeRouter = new OrganicEdgeRouter();
        reductionStage.NonTreeEdgeSelectionKey = OrganicEdgeRouter.AffectedEdgesDpKey;
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Orthogonal) {
        var edgeRouter = new EdgeRouter { Rerouting = true, Scope = Scope.RouteAffectedEdges };
        reductionStage.NonTreeEdgeRouter = edgeRouter;
        reductionStage.NonTreeEdgeSelectionKey = edgeRouter.AffectedEdgesDpKey;
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.StraightLine) {
        reductionStage.NonTreeEdgeRouter = reductionStage.CreateStraightLineRouter();
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Bundled) {
        var ebc = reductionStage.EdgeBundling;
        ebc.BundlingStrength = EdgeBundlingStrengthItem;
        ebc.DefaultBundleDescriptor = new EdgeBundleDescriptor { Bundled = true };
      }
      return reductionStage;
    }

    private TreeLayout ConfigureDefaultLayout() {
      var layout = new TreeLayout {
          LayoutOrientation = NodePlacerItem == EnumNodePlacer.AspectRatio
          ? LayoutOrientation.TopToBottom
          : DefaultLayoutOrientationItem
      };

      RootAlignment rootAlignment1 = RootAlignment.Center;
      RotatableNodePlacerBase.RootAlignment rootAlignment2 = RotatableNodePlacerBase.RootAlignment.Center;
      switch (RootAlignmentItem) {
        case EnumRootAlignment.Center:
          rootAlignment1 = RootAlignment.Center;
          rootAlignment2 = RotatableNodePlacerBase.RootAlignment.Center;
          break;
        case EnumRootAlignment.Median:
          rootAlignment1 = RootAlignment.Median;
          rootAlignment2 = RotatableNodePlacerBase.RootAlignment.Median;
          break;
        case EnumRootAlignment.Left:
          rootAlignment1 = RootAlignment.Leading;
          rootAlignment2 = RotatableNodePlacerBase.RootAlignment.Left;
          break;
        case EnumRootAlignment.Leading:
          rootAlignment1 = RootAlignment.LeadingOffset;
          rootAlignment2 = RotatableNodePlacerBase.RootAlignment.Leading;
          break;
        case EnumRootAlignment.Right:
          rootAlignment1 = RootAlignment.Trailing;
          rootAlignment2 = RotatableNodePlacerBase.RootAlignment.Right;
          break;
        case EnumRootAlignment.Trailing:
          rootAlignment1 = RootAlignment.TrailingOffset;
          rootAlignment2 = RotatableNodePlacerBase.RootAlignment.Trailing;
          break;
      }

      var allowMultiParents = AllowMultiParentsItem;

      switch (NodePlacerItem) {
        case EnumNodePlacer.Default:
          layout.DefaultNodePlacer = new DefaultNodePlacer {
              HorizontalDistance = SpacingItem, VerticalDistance = SpacingItem, RootAlignment = rootAlignment1, AlignPorts = AlignPortsItem
          };
          layout.MultiParentAllowed = allowMultiParents;
          break;
        case EnumNodePlacer.Simple:
          layout.DefaultNodePlacer = new SimpleNodePlacer { Spacing = SpacingItem, RootAlignment = rootAlignment2, AlignPorts = AlignPortsItem};
          break;
        case EnumNodePlacer.Bus:
          layout.DefaultNodePlacer = new BusNodePlacer { Spacing = SpacingItem, AlignPorts = AlignPortsItem };
          layout.MultiParentAllowed = allowMultiParents;
          break;
        case EnumNodePlacer.DoubleLine:
          layout.DefaultNodePlacer = new DoubleLineNodePlacer { Spacing = SpacingItem, RootAlignment = rootAlignment2, AlignPorts = AlignPortsItem };
          break;
        case EnumNodePlacer.LeftRight:
          layout.DefaultNodePlacer = new LeftRightNodePlacer { Spacing = SpacingItem, AlignPorts = AlignPortsItem };
          layout.MultiParentAllowed = allowMultiParents;
          break;
        case EnumNodePlacer.Layered:
          layout.DefaultNodePlacer = new LayeredNodePlacer {
              Spacing = SpacingItem, LayerSpacing = SpacingItem, RootAlignment = rootAlignment2, AlignPorts = AlignPortsItem
          };
          break;
        case EnumNodePlacer.AspectRatio:
          layout.DefaultNodePlacer = new AspectRatioNodePlacer {
              HorizontalDistance = SpacingItem, VerticalDistance = SpacingItem, AspectRatio = NodePlacerAspectRatioItem
          };
          break;
        case EnumNodePlacer.Dendrogram:
          layout.DefaultNodePlacer = new DendrogramNodePlacer {
              MinimumRootDistance = SpacingItem, MinimumSubtreeDistance = SpacingItem,
          };
          layout.MultiParentAllowed = allowMultiParents;
          break;
        case EnumNodePlacer.Grid:
          layout.DefaultNodePlacer = new GridNodePlacer { Spacing = SpacingItem, RootAlignment = rootAlignment2, AlignPorts = AlignPortsItem };
          break;
        case EnumNodePlacer.Compact:
          layout.DefaultNodePlacer = new CompactNodePlacer {
              HorizontalDistance = SpacingItem,
              VerticalDistance = SpacingItem,
              PreferredAspectRatio = NodePlacerAspectRatioItem
          };
          break;
        case EnumNodePlacer.DelegatingLayered:
          this.delegatingLeftPlacer =
              new LayeredNodePlacer(RotatableNodePlacerBase.Matrix.Rot270, RotatableNodePlacerBase.Matrix.Rot270) {
                  VerticalAlignment = 0,
                  RoutingStyle = LayeredRoutingStyle.Orthogonal,
                  Spacing = SpacingItem,
                  LayerSpacing = SpacingItem,
                  RootAlignment = rootAlignment2,
                  AlignPorts = AlignPortsItem
              };

          this.delegatingRightPlacer =
              new LayeredNodePlacer(RotatableNodePlacerBase.Matrix.Rot90, RotatableNodePlacerBase.Matrix.Rot90) {
                  VerticalAlignment = 0,
                  RoutingStyle = LayeredRoutingStyle.Orthogonal,
                  LayerSpacing = SpacingItem,
                  RootAlignment = rootAlignment2,
                  AlignPorts = AlignPortsItem
              };

          var delegatingRootPlacer = new DelegatingNodePlacer(RotatableNodePlacerBase.Matrix.Default,
              this.delegatingLeftPlacer,
              this.delegatingRightPlacer
          );
          delegatingRootPlacer.AlignPorts = AlignPortsItem;
          this.delegatingRootPlacer = delegatingRootPlacer;
          break;
      }

      layout.DefaultPortAssignment = new DefaultPortAssignment(PortAssignmentItem);
      layout.GroupingSupported = true;

      return layout;
    }

    private INodePlacer delegatingRootPlacer;
    private INodePlacer delegatingLeftPlacer;
    private INodePlacer delegatingRightPlacer;

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
   
    [Label("Node Placer")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object NodePlacerGroup;

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
      Orthogonal, Organic, StraightLine, Bundled
    }

    public enum EnumEdgeLabeling
    {
      None, Integrated, Generic
    }
    
    public enum EnumNodePlacer
    {
      Default, Simple, Bus, DoubleLine, LeftRight, Layered, AspectRatio, Dendrogram, Grid, Compact, HV, DelegatingLayered
    }

    public enum EnumRootAlignment
    {
      Center, Median, Left, Leading, Right, Trailing
    }

    /// <summary>
    /// Gets the description text.
    /// </summary>
    /// <value>
    /// The description text.
    /// </value>
    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText {
      get {
        return "<Paragraph>The various flavors of the tree layout styles are great for highlighting child-parent relationships in graphs that form one or more trees, "
               + "or trees with only few additional edges.</Paragraph>"
               + "<Paragraph>The need to visualize directed or undirected trees arises in many application areas, for example</Paragraph>"
               + "<List>"
               + "<ListItem><Paragraph>Dataflow analysis</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Software engineering</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Network management</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Bioinformatics</Paragraph></ListItem>"
               + "</List>";
      }
    }
    
    [Label("Routing Style for Non-Tree Edges")]
    [OptionGroup("NonTreeEdgesGroup", 10)]
    [DefaultValue(EnumRoute.Bundled)]
    [EnumValue("Orthogonal", EnumRoute.Orthogonal)]
    [EnumValue("Organic",EnumRoute.Organic)]
    [EnumValue("Straight-Line",EnumRoute.StraightLine)]
    [EnumValue("Bundled",EnumRoute.Bundled)]
    public EnumRoute RoutingStyleForNonTreeEdgesItem { get; set; }
    
    [Label("Bundling Strength")]
    [OptionGroup("NonTreeEdgesGroup", 20)]
    [DefaultValue(0.95d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }
    
    public bool ShouldDisableEdgeBundlingStrengthItem {
      get { return RoutingStyleForNonTreeEdgesItem != EnumRoute.Bundled; }
    }

    [Label("Act on Selection Only")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(false)]
    public bool ActOnSelectionOnlyItem { get; set; }
    
    [Label("Consider Node Labels")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool ConsiderNodeLabelsItem { get; set; }
    
    [Label("Node Placer")]
    [OptionGroup("NodePlacerGroup", 10)]
    [EnumValue("Default", EnumNodePlacer.Default)]
    [EnumValue("Simple", EnumNodePlacer.Simple)]
    [EnumValue("Bus", EnumNodePlacer.Bus)]
    [EnumValue("Double-Line", EnumNodePlacer.DoubleLine)]
    [EnumValue("Left-Right", EnumNodePlacer.LeftRight)]
    [EnumValue("Layered", EnumNodePlacer.Layered)]
    [EnumValue("Aspect Ratio", EnumNodePlacer.AspectRatio)]
    [EnumValue("Dendrogram", EnumNodePlacer.Dendrogram)]
    [EnumValue("Grid", EnumNodePlacer.Grid)]
    [EnumValue("Compact", EnumNodePlacer.Compact)]
    [EnumValue("Horizontal-Vertical", EnumNodePlacer.HV)]
    [EnumValue("Delegating & Layered", EnumNodePlacer.DelegatingLayered)]
    public EnumNodePlacer NodePlacerItem { get; set; }
    
    [Label("Spacing")]
    [OptionGroup("NodePlacerGroup", 20)]
    [MinMax(Min = 0, Max = 500)]
    [ComponentType(ComponentTypes.Slider)]
    public double SpacingItem { get; set; }

    [Label("Root Alignment")]
    [OptionGroup("NodePlacerGroup", 30)]
    [EnumValue("Center", EnumRootAlignment.Center)]
    [EnumValue("Median", EnumRootAlignment.Median)]
    [EnumValue("Left", EnumRootAlignment.Left)]
    [EnumValue("Leading", EnumRootAlignment.Leading)]
    [EnumValue("Right", EnumRootAlignment.Right)]
    [EnumValue("Trailing", EnumRootAlignment.Trailing)]
    public EnumRootAlignment RootAlignmentItem { get; set; }
    
    public bool ShouldDisableRootAlignmentItem {
      get {
        return NodePlacerItem == EnumNodePlacer.AspectRatio ||
               NodePlacerItem == EnumNodePlacer.Bus ||
               NodePlacerItem == EnumNodePlacer.Dendrogram ||
               NodePlacerItem == EnumNodePlacer.Compact;
      }
    }

    [Label("Align Ports")]
    [OptionGroup("NodePlacerGroup", 40)]
    [DefaultValue(false)]
    public bool AlignPortsItem { get; set; }

    public bool ShouldDisableAlignPortsItem {
      get {
        return
            (NodePlacerItem != EnumNodePlacer.Default &&
             NodePlacerItem != EnumNodePlacer.Simple &&
             NodePlacerItem != EnumNodePlacer.Bus &&
             NodePlacerItem != EnumNodePlacer.DoubleLine &&
             NodePlacerItem != EnumNodePlacer.LeftRight &&
             NodePlacerItem != EnumNodePlacer.Layered &&
             NodePlacerItem != EnumNodePlacer.Grid &&
             NodePlacerItem != EnumNodePlacer.DelegatingLayered &&
             NodePlacerItem != EnumNodePlacer.HV) ||
            (RootAlignmentItem != EnumRootAlignment.Center &&
             RootAlignmentItem != EnumRootAlignment.Median &&
             RootAlignmentItem != EnumRootAlignment.Left &&
             RootAlignmentItem != EnumRootAlignment.Right);
      }
    }

    [Label("Orientation")]
    [OptionGroup("GeneralGroup", 5)]
    [EnumValue("Top to Bottom", LayoutOrientation.TopToBottom)]
    [EnumValue("Left to Right", LayoutOrientation.LeftToRight )]
    [EnumValue("Bottom to Top", LayoutOrientation.BottomToTop )]
    [EnumValue("Right to Left", LayoutOrientation.RightToLeft )]
    public LayoutOrientation DefaultLayoutOrientationItem { get;set; }
    
    public bool ShouldDisableDefaultLayoutOrientationItem {
      get { return NodePlacerItem == EnumNodePlacer.AspectRatio || NodePlacerItem == EnumNodePlacer.Compact; }
    }
    
    [Label("Aspect Ratio")]
    [OptionGroup("NodePlacerGroup", 50)]
    [MinMax(Min = 0.1, Max = 4, Step = 0.01)]
    [ComponentType(ComponentTypes.Slider)]
    public double NodePlacerAspectRatioItem { get; set; }
    
    public bool ShouldDisableNodePlacerAspectRatioItem {
      get { return NodePlacerItem != EnumNodePlacer.AspectRatio && NodePlacerItem != EnumNodePlacer.Compact; }
    }

    [Label("Allow Multi-Parents")]
    [OptionGroup("NodePlacerGroup", 60)]
    public bool AllowMultiParentsItem { get; set; }
    
    public bool ShouldDisableAllowMultiParentsItem {
      get {
        return NodePlacerItem != EnumNodePlacer.Default &&
               NodePlacerItem != EnumNodePlacer.Dendrogram &&
               NodePlacerItem != EnumNodePlacer.Bus &&
               NodePlacerItem != EnumNodePlacer.LeftRight;
      }
    }
    
    [Label("Port Assignment")]
    [OptionGroup("EdgesGroup", 10)]
    [EnumValue("None", PortAssignmentMode.None)]
    [EnumValue("Distributed North", PortAssignmentMode.DistributedNorth)]
    [EnumValue("Distributed South", PortAssignmentMode.DistributedSouth)]
    [EnumValue("Distributed East", PortAssignmentMode.DistributedEast)]
    [EnumValue("Distributed West", PortAssignmentMode.DistributedWest)]
    public PortAssignmentMode PortAssignmentItem { get; set; }
    
    private EnumEdgeLabeling edgeLabelingItem;
    
    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(EnumEdgeLabeling.None)]
    [EnumValue("None", EnumEdgeLabeling.None)]
    [EnumValue("Integrated", EnumEdgeLabeling.Integrated)]
    [EnumValue("Generic", EnumEdgeLabeling.Generic)]
    public EnumEdgeLabeling EdgeLabelingItem {
      get { return edgeLabelingItem; }
      set {
        edgeLabelingItem = value;
        if (value == EnumEdgeLabeling.Integrated) {
          LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Parallel;
          LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.AtTarget;
          LabelPlacementDistanceItem = 0;
        }
      }
    }
    
    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
    public bool ReduceAmbiguityItem { get; set; }
    
    public bool ShouldDisableReduceAmbiguityItem {
      get { return EdgeLabelingItem != EnumEdgeLabeling.Generic; }
    }
    
    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal",EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal",EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical",EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }
    
    public bool ShouldDisableLabelPlacementOrientationItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }
    
    [Label("Along Edge")]
    [OptionGroup("PreferredPlacementGroup", 20)]
    [DefaultValue(EnumLabelPlacementAlongEdge.Centered)]
    [EnumValue("Anywhere", EnumLabelPlacementAlongEdge.Anywhere)]
    [EnumValue("At Source",EnumLabelPlacementAlongEdge.AtSource)]
    [EnumValue("At Source Port",EnumLabelPlacementAlongEdge.AtSourcePort)]
    [EnumValue("At Target",EnumLabelPlacementAlongEdge.AtTarget)]
    [EnumValue("At Target Port",EnumLabelPlacementAlongEdge.AtTargetPort)]
    [EnumValue("Centered",EnumLabelPlacementAlongEdge.Centered)]
    public EnumLabelPlacementAlongEdge LabelPlacementAlongEdgeItem { get; set; }
    
    public bool ShouldDisableLabelPlacementAlongEdgeItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }
    
    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge",EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left",EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right",EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right",EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }
    
    public bool ShouldDisableLabelPlacementSideOfEdgeItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }
    
    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }
    
    public bool ShouldDisableLabelPlacementDistanceItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None || LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge; }
    }

    /// <summary>
    /// This stage temporarily removes edges that are incident to group nodes.
    /// </summary>
    /// <remarks>
    /// The stage must be prepended to the layout algorithm and applies the following three steps:
    /// <list type="number">
    /// <item>Removes edges from the graph that are incident to group nodes.</item>
    /// <item>Invokes the core layout algorithm on the reduced graph.</item>
    /// <item>Re-inserts all previously removed edges and optionally places their labels.</item>
    /// </list>
    /// This stage can be useful for layout algorithms or stages that cannot handle edges between group nodes,
    /// e.g., <see cref="TreeReductionStage"/>. Optionally, <see cref="HandleEdgesBetweenGroupsStage"/> can also
    /// place the labels of the edges that were temporarily removed right after they are restored back to the graph.
    /// <para>
    /// The routing of the temporarily hidden edges can be customized by specifying an
    /// <see cref="MarkedEdgeRouter">edge routing algorithm</see> for those edges.
    /// </para>
    /// </remarks>
    private class HandleEdgesBetweenGroupsStage : LayoutStageBase
    {
      public HandleEdgesBetweenGroupsStage(bool placeLabels) {
        ConsiderEdgeLabels = placeLabels;
      }

      /// <summary>
      /// Gets or sets the key to register a data provider that will be used by the edge routing
      /// algorithm to determine the edges that need to be routed.
      /// </summary>
      public object EdgeSelectionKey { get; set; }

      /// <summary>
      /// Gets or sets the edge routing algorithm that is applied to the set of marked edges.
      /// </summary>
      /// <remarks>
      /// Note that is required that a suitable edge selection key is specified and the router's scope
      /// is reduced to the affected edges.
      /// </remarks>
      public ILayoutAlgorithm MarkedEdgeRouter { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether the stage should place the labels of the edges that
      /// have been temporarily hidden, when these edges will be restored back.
      /// </summary>
      public bool ConsiderEdgeLabels { get; set; }

      /// <summary>
      /// Removes all edges that are incident to group nodes and passes it to the core layout algorithm.
      /// </summary>
      /// <remarks>
      /// This stage removes some edges from the graph such that no edges incident to group nodes
      /// exist. Then, it applies the core layout algorithm to the reduced graph.
      /// After it produces the result, it re-inserts the previously removed edges and routes them.
      /// </remarks>
      public override void ApplyLayout(LayoutGraph graph) {
        var groupingSupport = new yWorks.Layout.Grouping.GroupingSupport(graph);

        if (!GroupingSupport.IsGrouped(graph)) {
          ApplyLayoutCore(graph);
        } else {
          var hiddenEdgesMap = Maps.CreateHashedEdgeMap();

          var edgeHider = new LayoutGraphHider(graph);

          var existHiddenEdges = false;
          foreach (var edge in graph.Edges) {
            if (groupingSupport.IsGroupNode(edge.Source) || groupingSupport.IsGroupNode(edge.Target)) {
              hiddenEdgesMap.Set(edge, true);
              edgeHider.Hide(edge);
              existHiddenEdges = true;
            } else {
              hiddenEdgesMap.Set(edge, false);
            }
          }

          ApplyLayoutCore(graph);

          if (existHiddenEdges) {
            edgeHider.UnhideAll();

            // routes the marked edges
            RouteMarkedEdges(graph, hiddenEdgesMap);

            if (ConsiderEdgeLabels) {
              // all labels of hidden edges should be marked
              var affectedLabelsDpKey = "affectedLabelsDpKey";
              var nonTreeLabelsMap = Maps.CreateHashedDataMap();

              foreach (var edge in graph.Edges) {
                var ell = graph.GetLabelLayout(edge);
                foreach (var labelLayout in ell) {
                  nonTreeLabelsMap.Set(labelLayout, hiddenEdgesMap.Get(edge));
                }
              }

              // add selection marker
              graph.AddDataProvider(affectedLabelsDpKey, nonTreeLabelsMap);

              // place marked labels
              var labeling = new GenericLabeling {
                  PlaceNodeLabels = false,
                  PlaceEdgeLabels = true,
                  AffectedLabelsDpKey = affectedLabelsDpKey,
              };
              labeling.ApplyLayout(graph);

              // dispose selection key
              graph.RemoveDataProvider(affectedLabelsDpKey);
            }
          }
        }
      }

      private void RouteMarkedEdges(LayoutGraph graph, IDataMap markedEdgesMap) {
        if (MarkedEdgeRouter == null) {
          return;
        }

        IDataProvider backupDp = null;
        if (EdgeSelectionKey != null) {
          backupDp = graph.GetDataProvider(EdgeSelectionKey);
          graph.AddDataProvider(EdgeSelectionKey, markedEdgesMap);
        }
        if (MarkedEdgeRouter is StraightLineEdgeRouter) {
          var router = (StraightLineEdgeRouter) MarkedEdgeRouter;
          router.Scope = Scope.RouteAffectedEdges;
          router.AffectedEdgesDpKey = EdgeSelectionKey;
        }

        MarkedEdgeRouter.ApplyLayout(graph);

        if (EdgeSelectionKey != null) {
          graph.RemoveDataProvider(EdgeSelectionKey);

          if (backupDp != null) {
            graph.AddDataProvider(EdgeSelectionKey, backupDp);
          }
        }
      }
    }
  }

}
