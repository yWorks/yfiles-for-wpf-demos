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
      var layout = new ClassicTreeLayout();
      var aspectRatioNodePlacer = new AspectRatioNodePlacer();
      var defaultNodePlacer = new DefaultNodePlacer();

      LayoutStyleItem = EnumStyle.Default;
      RoutingStyleForNonTreeEdgesItem = EnumRoute.Orthogonal;
      EdgeBundlingStrengthItem = 0.95;
      ActOnSelectionOnlyItem = false;

      DefaultLayoutOrientationItem = LayoutOrientation.TopToBottom;
      ClassicLayoutOrientationItem = LayoutOrientation.TopToBottom;

      MinimumNodeDistanceItem = (int) layout.MinimumNodeDistance;
      MinimumLayerDistanceItem = (int) layout.MinimumLayerDistance;
      PortStyleItem = PortStyle.NodeCenter;

      ConsiderNodeLabelsItem = false;

      OrthogonalEdgeRoutingItem = false;

      VerticalAlignmentItem = 0.5;
      ChildPlacementPolicyItem = LeafPlacement.SiblingsOnSameLayer;
      EnforceGlobalLayeringItem = false;

      NodePlacerItem = EnumNodePlacer.Default;

      SpacingItem = 20;
      RootAlignmentItem = EnumRootAlignment.Center;
      AllowMultiParentsItem = false;
      PortAssignmentItem = PortAssignmentMode.None;

      HvHorizontalSpaceItem = (int) defaultNodePlacer.HorizontalDistance;
      HvVerticalSpaceItem = (int) defaultNodePlacer.VerticalDistance;

      BusAlignmentItem = 0.5;

      ArHorizontalSpaceItem = (int) aspectRatioNodePlacer.HorizontalDistance;
      ArVerticalSpaceItem = (int) aspectRatioNodePlacer.VerticalDistance;
      NodePlacerAspectRatioItem = aspectRatioNodePlacer.AspectRatio;

      ArUseViewAspectRatioItem = true;
      CompactPreferredAspectRatioItem = aspectRatioNodePlacer.AspectRatio;

      EdgeLabelingItem = EnumEdgeLabeling.None;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      MultiStageLayout layout;

      switch (LayoutStyleItem) {
        default:
        case EnumStyle.Default:
          layout = ConfigureDefaultLayout();
          break;
        case EnumStyle.Classic:
          layout = ConfigureClassicLayout();
          break;
        case EnumStyle.HorizontalVertical:
          layout = new TreeLayout();
          break;
        case EnumStyle.Compact:
          layout = ConfigureCompactLayout(graphControl);
          break;
      }

      layout.ParallelEdgeRouterEnabled = false;
      ((ComponentLayout) layout.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;
      layout.SubgraphLayoutEnabled = ActOnSelectionOnlyItem;

      layout.PrependStage(CreateTreeReductionStage());

      var placeLabels = EdgeLabelingItem == EnumEdgeLabeling.Integrated || EdgeLabelingItem == EnumEdgeLabeling.Generic;

      // required to prevent WrongGraphStructure exception which may be thrown by TreeLayout if there are edges
      // between group nodes
      layout.PrependStage(new HandleEdgesBetweenGroupsStage(placeLabels));

      if (EdgeLabelingItem == EnumEdgeLabeling.Generic) {
        layout.LabelingEnabled = true;
        layout.Labeling = new GenericLabeling {
            PlaceEdgeLabels = true,
            PlaceNodeLabels = false,
            ReduceAmbiguity = ReduceAmbiguityItem
        };
      }

      AddPreferredPlacementDescriptor(graphControl.Graph, LabelPlacementAlongEdgeItem, LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem);

      return layout;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      if (LayoutStyleItem == EnumStyle.Default) {
        var graph = graphControl.Graph;
        return new TreeLayoutData {
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
            CompactNodePlacerStrategyMementos = new DictionaryMapper<INode, object>()
        };
      } else if (LayoutStyleItem == EnumStyle.HorizontalVertical) {
        return new TreeLayoutData {
            NodePlacers = {
                Delegate = node => {
                  // children of selected nodes should be placed vertical and to the right of their child nodes, while
                  // the children of non-selected horizontal downwards
                  var childPlacement = graphControl.Selection.IsSelected(node)
                      ? ChildPlacement.VerticalToRight
                      : ChildPlacement.HorizontalDownward;

                  return new DefaultNodePlacer(childPlacement, RootAlignment.LeadingOnBus, HvVerticalSpaceItem,
                      HvHorizontalSpaceItem);
                }
            }
        };
      }
      return null;
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
          (LayoutStyleItem == EnumStyle.Classic &&
           !EnforceGlobalLayeringItem &&
           ChildPlacementPolicyItem != LeafPlacement.AllLeavesOnSameLayer) ||
          (LayoutStyleItem == EnumStyle.Default &&
           (NodePlacerItem == EnumNodePlacer.Default ||
            NodePlacerItem == EnumNodePlacer.Bus ||
            NodePlacerItem == EnumNodePlacer.LeftRight ||
            NodePlacerItem == EnumNodePlacer.Dendrogram) &&
           AllowMultiParentsItem);

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

    private MultiStageLayout ConfigureDefaultLayout() {
      var layout = new TreeLayout();
      layout.LayoutOrientation =
          NodePlacerItem == EnumNodePlacer.AspectRatio
              ? LayoutOrientation.TopToBottom
              : DefaultLayoutOrientationItem;

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
              HorizontalDistance = SpacingItem,
              VerticalDistance = SpacingItem,
              RootAlignment = rootAlignment1
          };
          layout.MultiParentAllowed = allowMultiParents;
          break;
        case EnumNodePlacer.Simple:
          layout.DefaultNodePlacer = new SimpleNodePlacer {
              Spacing = SpacingItem,
              RootAlignment = rootAlignment2
          };
          break;
        case EnumNodePlacer.Bus:
          layout.DefaultNodePlacer = new BusNodePlacer {
              Spacing = SpacingItem,
          };
          layout.MultiParentAllowed = allowMultiParents;
          break;
        case EnumNodePlacer.DoubleLine:
          layout.DefaultNodePlacer = new DoubleLineNodePlacer {
              Spacing = SpacingItem,
              RootAlignment = rootAlignment2
          };
          break;
        case EnumNodePlacer.LeftRight:
          layout.DefaultNodePlacer = new LeftRightNodePlacer {
              Spacing = SpacingItem
          };
          layout.MultiParentAllowed = allowMultiParents;
          break;
        case EnumNodePlacer.Layered:
          layout.DefaultNodePlacer = new LayeredNodePlacer {
              Spacing = SpacingItem,
              LayerSpacing = SpacingItem,
              RootAlignment = rootAlignment2
          };
          break;
        case EnumNodePlacer.AspectRatio:
          layout.DefaultNodePlacer = new AspectRatioNodePlacer {
              HorizontalDistance = SpacingItem,
              VerticalDistance = SpacingItem,
              AspectRatio = NodePlacerAspectRatioItem
          };
          break;
        case EnumNodePlacer.Dendrogram:
          layout.DefaultNodePlacer = new DendrogramNodePlacer {
              MinimumRootDistance = SpacingItem,
              MinimumSubtreeDistance = SpacingItem,
          };
          layout.MultiParentAllowed = allowMultiParents;
          break;
        case EnumNodePlacer.Grid:
          layout.DefaultNodePlacer = new GridNodePlacer {
              Spacing = SpacingItem,
              RootAlignment = rootAlignment2
          };
          break;
        case EnumNodePlacer.Compact:
          layout.DefaultNodePlacer = new CompactNodePlacer {
              HorizontalDistance = SpacingItem,
              VerticalDistance = SpacingItem,
              PreferredAspectRatio = NodePlacerAspectRatioItem
          };
          break;
      }

      layout.DefaultPortAssignment = new DefaultPortAssignment(PortAssignmentItem);
      layout.GroupingSupported = true;

      return layout;
    }

    private MultiStageLayout ConfigureClassicLayout() {
      var layout = new ClassicTreeLayout();
      layout.MinimumNodeDistance = MinimumNodeDistanceItem;
      layout.MinimumLayerDistance = MinimumLayerDistanceItem;

      ((OrientationLayout) layout.OrientationLayout).Orientation = ClassicLayoutOrientationItem;

      if (OrthogonalEdgeRoutingItem) {
        layout.EdgeRoutingStyle = yWorks.Layout.Tree.EdgeRoutingStyle.Orthogonal;
      } else {
        layout.EdgeRoutingStyle = yWorks.Layout.Tree.EdgeRoutingStyle.Plain;
      }

      layout.LeafPlacement = ChildPlacementPolicyItem;
      layout.EnforceGlobalLayering = EnforceGlobalLayeringItem;
      layout.PortStyle = PortStyleItem;

      layout.VerticalAlignment = VerticalAlignmentItem;
      layout.BusAlignment = BusAlignmentItem;

      return layout;
    }

    private MultiStageLayout ConfigureCompactLayout(GraphControl graphControl) {
      var layout = new TreeLayout();
      var aspectRatioNodePlacer = new AspectRatioNodePlacer();

      if (graphControl != null && ArUseViewAspectRatioItem) {
        var size = graphControl.InnerSize;
        aspectRatioNodePlacer.AspectRatio = size.Width / size.Height;
      } else {
        aspectRatioNodePlacer.AspectRatio = CompactPreferredAspectRatioItem;
      }

      aspectRatioNodePlacer.HorizontalDistance = ArHorizontalSpaceItem;
      aspectRatioNodePlacer.VerticalDistance = ArVerticalSpaceItem;

      layout.DefaultNodePlacer = aspectRatioNodePlacer;
      return layout;
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

    [Label("Default")]
    [OptionGroup("RootGroup", 15)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DefaultGroup;

    [Label("Horizontal-Vertical")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object HVGroup;

    [Label("Compact")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object CompactGroup;

    [Label("Classic")]
    [OptionGroup("RootGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object ClassicGroup;

    [Label("Labeling")]
    [OptionGroup("RootGroup", 50)]
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

    public enum EnumStyle
    {
      Default, HorizontalVertical, Compact, Classic
    }

    public enum EnumNodePlacer
    {
      Default, Simple, Bus, DoubleLine, LeftRight, Layered, AspectRatio, Dendrogram, Grid, Compact
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

    [Label("Layout Style")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(EnumStyle.Default)]
    [EnumValue("Default", EnumStyle.Default)]
    [EnumValue("Horizontal-Vertical",EnumStyle.HorizontalVertical)]
    [EnumValue("Compact",EnumStyle.Compact)]
    [EnumValue("Classic",EnumStyle.Classic)]
    public EnumStyle LayoutStyleItem { get; set; }
       
    [Label("Routing Style for Non-Tree Edges")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(EnumRoute.Bundled)]
    [EnumValue("Orthogonal", EnumRoute.Orthogonal)]
    [EnumValue("Organic",EnumRoute.Organic)]
    [EnumValue("Straight-Line",EnumRoute.StraightLine)]
    [EnumValue("Bundled",EnumRoute.Bundled)]
    public EnumRoute RoutingStyleForNonTreeEdgesItem { get; set; }

    [Label("Bundling Strength")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(0.95d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }

    public bool ShouldDisableEdgeBundlingStrengthItem {
      get { return RoutingStyleForNonTreeEdgesItem != EnumRoute.Bundled; }
    }

    [Label("Act on Selection Only")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(false)]
    public bool ActOnSelectionOnlyItem { get; set; }

    [Label("Consider Node Labels")]
    [OptionGroup("GeneralGroup", 50)]
    [DefaultValue(false)]
    public bool ConsiderNodeLabelsItem { get; set; }

    [Label("Node Placer")]
    [OptionGroup("DefaultGroup", 10)]
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
    public EnumNodePlacer NodePlacerItem { get; set; }

    [Label("Spacing")]
    [OptionGroup("DefaultGroup", 20)]
    [MinMax(Min = 0, Max = 500)]
    [ComponentType(ComponentTypes.Slider)]
    public double SpacingItem { get; set; }

    [Label("Root Alignment")]
    [OptionGroup("DefaultGroup", 30)]
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

    [Label("Orientation")]
    [OptionGroup("DefaultGroup", 40)]
    [EnumValue("Top to Bottom", LayoutOrientation.TopToBottom)]
    [EnumValue("Left to Right", LayoutOrientation.LeftToRight )]
    [EnumValue("Bottom to Top", LayoutOrientation.BottomToTop )]
    [EnumValue("Right to Left", LayoutOrientation.RightToLeft )]
    public LayoutOrientation DefaultLayoutOrientationItem { get;set; }

    public bool ShouldDisableDefaultLayoutOrientationItem {
      get { return NodePlacerItem == EnumNodePlacer.AspectRatio || NodePlacerItem == EnumNodePlacer.Compact; }
    }

    [Label("Aspect Ratio")]
    [OptionGroup("DefaultGroup", 50)]
    [MinMax(Min = 0.1, Max = 4, Step = 0.01)]
    [ComponentType(ComponentTypes.Slider)]
    public double NodePlacerAspectRatioItem { get; set; }

    public bool ShouldDisableNodePlacerAspectRatioItem {
      get { return NodePlacerItem != EnumNodePlacer.AspectRatio && NodePlacerItem != EnumNodePlacer.Compact; }
    }

    [Label("Allow Multi-Parents")]
    [OptionGroup("DefaultGroup", 60)]
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
    [OptionGroup("DefaultGroup", 70)]
    [EnumValue("None", PortAssignmentMode.None)]
    [EnumValue("Distributed North", PortAssignmentMode.DistributedNorth)]
    [EnumValue("Distributed South", PortAssignmentMode.DistributedSouth)]
    [EnumValue("Distributed East", PortAssignmentMode.DistributedEast)]
    [EnumValue("Distributed West", PortAssignmentMode.DistributedWest)]
    public PortAssignmentMode PortAssignmentItem { get; set; }

    [Label("Horizontal Spacing")]
    [OptionGroup("HVGroup", 10)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int HvHorizontalSpaceItem { get; set; }
    
    [Label("Vertical Spacing")]
    [OptionGroup("HVGroup", 20)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int HvVerticalSpaceItem { get; set; }

    [Label("Horizontal Spacing")]
    [OptionGroup("CompactGroup", 10)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int ArHorizontalSpaceItem { get; set; }

    [Label("Vertical Spacing")]
    [OptionGroup("CompactGroup", 20)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int ArVerticalSpaceItem { get; set; }

    [Label("Use Aspect Ratio of View")]
    [OptionGroup("CompactGroup", 40)]
    [DefaultValue(true)]
    public bool ArUseViewAspectRatioItem { get; set; }

    [Label("Preferred Aspect Ratio")]
    [OptionGroup("CompactGroup", 50)]
    [DefaultValue(1.41d)]
    [MinMax(Min = 0.2d, Max = 5.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CompactPreferredAspectRatioItem { get; set; }

    public bool ShouldDisableCompactPreferredAspectRatioItem {
      get { return ArUseViewAspectRatioItem; }
    }

        [Label("Orientation")]
    [OptionGroup("ClassicGroup", 10)]
    [DefaultValue(LayoutOrientation.BottomToTop)]
    [EnumValue("Top to Bottom", LayoutOrientation.TopToBottom)] 
    [EnumValue("Left to Right", LayoutOrientation.LeftToRight )] 
    [EnumValue("Bottom to Top", LayoutOrientation.BottomToTop )] 
    [EnumValue("Right to Left", LayoutOrientation.RightToLeft )]
    public LayoutOrientation ClassicLayoutOrientationItem { get; set; }

    [Label("Minimum Node Distance")]
    [MinMax(Min = 1, Max = 100)]
    [DefaultValue(20)]
    [OptionGroup("ClassicGroup", 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumNodeDistanceItem { get; set; }

    [Label("Minimum Layer Distance")]
    [MinMax(Min = 10, Max = 300)]
    [DefaultValue(40)]
    [OptionGroup("ClassicGroup", 30)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumLayerDistanceItem { get; set; }

    [Label("Port Style")]
    [OptionGroup("ClassicGroup", 40)]
    [DefaultValue(PortStyle.NodeCenter)]
    [EnumValue("Node Centered", PortStyle.NodeCenter)] 
    [EnumValue("Border Centered", PortStyle.BorderCenter )] 
    [EnumValue("Border Distributed", PortStyle.BorderDistributed  )]
    public PortStyle PortStyleItem { get; set; }

    [Label("Global Layering")]
    [OptionGroup("ClassicGroup", 50)]
    [DefaultValue(true)]
    public bool EnforceGlobalLayeringItem { get; set; }

    [Label("Orthogonal Edge Routing")]
    [OptionGroup("ClassicGroup", 60)]
    [DefaultValue(false)]
    public bool OrthogonalEdgeRoutingItem { get; set; }

    [Label("Edge Bus Alignment")]
    [OptionGroup("ClassicGroup", 70)]
    [DefaultValue(0.5d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double BusAlignmentItem { get; set; }

    public bool ShouldDisableBusAlignmentItem {
      get {
        return OrthogonalEdgeRoutingItem == false ||
               (EnforceGlobalLayeringItem == false && ChildPlacementPolicyItem != LeafPlacement.AllLeavesOnSameLayer);
      }
    }

    [Label("Vertical Child Alignment")]
    [OptionGroup("ClassicGroup", 80)]
    [DefaultValue(0.5d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double VerticalAlignmentItem { get; set; }

    public bool ShouldDisableVerticalAlignmentItem {
      get { return !EnforceGlobalLayeringItem; }
    }


    [Label("Child Placement Policy")]
    [OptionGroup("ClassicGroup", 90)]
    [DefaultValue(LeafPlacement.SiblingsOnSameLayer)]
    [EnumValue("Siblings in same Layer", LeafPlacement.SiblingsOnSameLayer)] 
    [EnumValue("All Leaves in same Layer", LeafPlacement.AllLeavesOnSameLayer )] 
    [EnumValue("Leaves stacked", LeafPlacement.LeavesStacked )] 
    [EnumValue("Leaves stacked left", LeafPlacement.LeavesStackedLeft )] 
    [EnumValue("Leaves stacked right", LeafPlacement.LeavesStackedRight )] 
    [EnumValue("Leaves stacked left and right", LeafPlacement.LeavesStackedLeftAndRight )]
    public LeafPlacement ChildPlacementPolicyItem { get; set; }

    private EnumEdgeLabeling edgeLabelingItem;

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 20)]
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
    [OptionGroup("EdgePropertiesGroup", 30)]
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
    [EnumValue("At Target",EnumLabelPlacementAlongEdge.AtTarget)]
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
