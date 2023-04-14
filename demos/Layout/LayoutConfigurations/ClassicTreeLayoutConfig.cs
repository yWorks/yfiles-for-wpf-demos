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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Controls;
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
  [Label("ClassicTreeLayout")]
  public class ClassicTreeLayoutConfig : LayoutConfiguration
  {

    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public ClassicTreeLayoutConfig() {
      var layout = new ClassicTreeLayout();

      RoutingStyleForNonTreeEdgesItem = EnumRoute.Orthogonal;
      EdgeBundlingStrengthItem = 0.95;
      ActOnSelectionOnlyItem = false;

      ClassicLayoutOrientationItem = LayoutOrientation.TopToBottom;

      MinimumNodeDistanceItem = (int) layout.MinimumNodeDistance;
      MinimumLayerDistanceItem = (int) layout.MinimumLayerDistance;
      PortStyleItem = PortStyle.NodeCenter;

      ConsiderNodeLabelsItem = false;

      OrthogonalEdgeRoutingItem = false;

      VerticalAlignmentItem = 0.5;
      LeafPlacementPolicyItem = LeafPlacement.SiblingsOnSameLayer;
      EnforceGlobalLayeringItem = false;
      
      BusAlignmentItem = 0.5;
      
      EdgeLabelingItem = EnumEdgeLabeling.None;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = ConfigureClassicLayout();

      layout.ParallelEdgeRouterEnabled = false;
      ((ComponentLayout) layout.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;
      layout.SubgraphLayoutEnabled = ActOnSelectionOnlyItem;

      layout.PrependStage(CreateTreeReductionStage());

      var placeLabels = EdgeLabelingItem == EnumEdgeLabeling.Integrated || EdgeLabelingItem == EnumEdgeLabeling.Generic;

      // required to prevent WrongGraphStructure exception which may be thrown by TreeLayout if there are edges
      // between group nodes
      layout.PrependStage(new HandleEdgesBetweenGroupsStage(placeLabels));

      layout.ConsiderNodeLabels = this.ConsiderNodeLabelsItem;


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
      return CreateLabelingLayoutData(
          graphControl.Graph,
          LabelPlacementAlongEdgeItem,
          LabelPlacementSideOfEdgeItem,
          LabelPlacementOrientationItem,
          LabelPlacementDistanceItem
      );
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
          !EnforceGlobalLayeringItem &&
           LeafPlacementPolicyItem != LeafPlacement.AllLeavesOnSameLayer;

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

    private ClassicTreeLayout ConfigureClassicLayout() {
      var layout = new ClassicTreeLayout();
      layout.MinimumNodeDistance = MinimumNodeDistanceItem;
      layout.MinimumLayerDistance = MinimumLayerDistanceItem;

      ((OrientationLayout) layout.OrientationLayout).Orientation = ClassicLayoutOrientationItem;

      if (OrthogonalEdgeRoutingItem) {
        layout.EdgeRoutingStyle = yWorks.Layout.Tree.EdgeRoutingStyle.Orthogonal;
      } else {
        layout.EdgeRoutingStyle = yWorks.Layout.Tree.EdgeRoutingStyle.Plain;
      }

      layout.LeafPlacement = LeafPlacementPolicyItem;
      layout.EnforceGlobalLayering = EnforceGlobalLayeringItem;
      layout.PortStyle = PortStyleItem;

      layout.VerticalAlignment = VerticalAlignmentItem;
      layout.BusAlignment = BusAlignmentItem;

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

    [Label("Edges")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object EdgesGroup;
    
    [Label("Labeling")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LabelingGroup;

    [Label("Non-Tree Edges")]
    [OptionGroup("EdgesGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object NonTreeEdgesGroup;

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
        return "<Paragraph>This layout is designed to arrange directed and undirected trees that have a unique root node. "
               + "All children are placed below their parent in relation to the main layout direction. "
               + "The edges of the graph are routed as straight-line segments or in an orthogonal bus-like fashion.</Paragraph>"
               + "<Paragraph>Tree layout algorithms are commonly used for visualizing relational data and for producing diagrams of high quality that are able to reveal possible hierarchic properties of the graph." +
               " More precisely, they find applications in</Paragraph>"
               + "<List>"
               + "<ListItem><Paragraph>Dataflow analysis</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Software engineering</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Network management</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Bioinformatics</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Business Administration</Paragraph></ListItem>"
               + "</List>";
      }
    }
 
    [Label("Routing Style for Non-Tree Edges")]
    [OptionGroup("NonTreeEdgesGroup", 10)]
    [DefaultValue(EnumRoute.Bundled)]
    [EnumValue("Orthogonal", EnumRoute.Orthogonal)]
    [EnumValue("Organic", EnumRoute.Organic)]
    [EnumValue("Straight-Line", EnumRoute.StraightLine)]
    [EnumValue("Bundled", EnumRoute.Bundled)]
    public EnumRoute RoutingStyleForNonTreeEdgesItem { get; set; }
    
    [Label("Bundling Strength")]
    [OptionGroup("NonTreeEdgesGroup", 30)]
    [DefaultValue(0.95d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }
    
    public bool ShouldDisableEdgeBundlingStrengthItem {
      get { return RoutingStyleForNonTreeEdgesItem != EnumRoute.Bundled; }
    }
    
    [Label("Act on Selection Only")]
    [OptionGroup("GeneralGroup", 60)]
    [DefaultValue(false)]
    public bool ActOnSelectionOnlyItem { get; set; }
    
    [Label("Consider Node Labels")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool ConsiderNodeLabelsItem { get; set; }
    
    [Label("Orientation")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(LayoutOrientation.BottomToTop)]
    [EnumValue("Top to Bottom", LayoutOrientation.TopToBottom)]
    [EnumValue("Left to Right", LayoutOrientation.LeftToRight)]
    [EnumValue("Bottom to Top", LayoutOrientation.BottomToTop)]
    [EnumValue("Right to Left", LayoutOrientation.RightToLeft)]
    public LayoutOrientation ClassicLayoutOrientationItem { get; set; }

    [Label("Minimum Node Distance")]
    [MinMax(Min = 1, Max = 100)]
    [DefaultValue(20)]
    [OptionGroup("GeneralGroup", 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumNodeDistanceItem { get; set; }
    
    [Label("Minimum Layer Distance")]
    [MinMax(Min = 10, Max = 300)]
    [DefaultValue(40)]
    [OptionGroup("GeneralGroup", 30)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumLayerDistanceItem { get; set; }
    
    [Label("Port Style")]
    [OptionGroup("EdgesGroup", 20)]
    [DefaultValue(PortStyle.NodeCenter)]
    [EnumValue("Node Centered", PortStyle.NodeCenter)]
    [EnumValue("Border Centered", PortStyle.BorderCenter)]
    [EnumValue("Border Distributed", PortStyle.BorderDistributed)]
    public PortStyle PortStyleItem { get; set; }
    
    [Label("Global Layering")]
    [OptionGroup("GeneralGroup", 35)]
    [DefaultValue(true)]
    public bool EnforceGlobalLayeringItem { get; set; }
    
    [Label("Orthogonal Edge Routing")]
    [OptionGroup("EdgesGroup", 10)]
    [DefaultValue(false)]
    public bool OrthogonalEdgeRoutingItem { get; set; }
    
    [Label("Edge Bus Alignment")]
    [OptionGroup("EdgesGroup", 30)]
    [DefaultValue(0.5d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double BusAlignmentItem { get; set; }
    
    public bool ShouldDisableBusAlignmentItem {
      get {
        return OrthogonalEdgeRoutingItem == false ||
               (EnforceGlobalLayeringItem == false && LeafPlacementPolicyItem != LeafPlacement.AllLeavesOnSameLayer);
      }
    }
    
    [Label("Vertical Child Alignment")]
    [OptionGroup("GeneralGroup", 50)]
    [DefaultValue(0.5d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double VerticalAlignmentItem { get; set; }
    
    public bool ShouldDisableVerticalAlignmentItem {
      get { return !EnforceGlobalLayeringItem; }
    }
    
    [Label("Leaf Placement")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(LeafPlacement.SiblingsOnSameLayer)]
    [EnumValue("Siblings in same Layer", LeafPlacement.SiblingsOnSameLayer)]
    [EnumValue("All Leaves in same Layer", LeafPlacement.AllLeavesOnSameLayer)]
    [EnumValue("Leaves stacked", LeafPlacement.LeavesStacked)]
    [EnumValue("Leaves stacked left", LeafPlacement.LeavesStackedLeft)]
    [EnumValue("Leaves stacked right", LeafPlacement.LeavesStackedRight)]
    [EnumValue("Leaves stacked left and right", LeafPlacement.LeavesStackedLeftAndRight)]
    public LeafPlacement LeafPlacementPolicyItem { get; set; }

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
    [EnumValue("Orthogonal", EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal", EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical", EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }
    
    public bool ShouldDisableLabelPlacementOrientationItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }
    
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
    
    public bool ShouldDisableLabelPlacementAlongEdgeItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }
    
    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge", EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left", EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right", EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right", EnumLabelPlacementSideOfEdge.LeftOrRight)]
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
    /// e.g., <see cref="TreeReductionStage"/>. Optionally, <see cref="Demo.yFiles.Layout.Configurations.TreeLayoutConfig.HandleEdgesBetweenGroupsStage"/> can also
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
              var labeling = new GenericLabeling
              {
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
