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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Partial;
using RoutingStyle = yWorks.Layout.Partial.RoutingStyle;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("PartialLayout")]
  public class PartialLayoutConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public PartialLayoutConfig() {
      RoutingToSubgraphItem = RoutingStyle.Automatic;
      ComponentAssignmentStrategyItem = ComponentAssignmentStrategy.Connected;
      SubgraphLayoutItem = EnumSubgraphLayouts.Hierarchical;
      SubgraphPlacementItem = SubgraphPlacement.FromSketch;
      MinimumNodeDistanceItem = 30;
      OrientationItem = PartialLayoutOrientation.AutoDetect;
      AlignNodesItem = true;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new PartialLayout();
      layout.ConsiderNodeAlignment = AlignNodesItem;
      layout.MinimumNodeDistance = MinimumNodeDistanceItem;
      layout.SubgraphPlacement = SubgraphPlacementItem;
      layout.ComponentAssignmentStrategy = GetComponentAssignmentStrategy(ComponentAssignmentStrategyItem);
      layout.LayoutOrientation = OrientationItem;
      layout.EdgeRoutingStyle = RoutingToSubgraphItem;
      layout.AllowMovingFixedElements = MoveFixedElementsItem;

      ILayoutAlgorithm subgraphLayout = null;
      if (ComponentAssignmentStrategyItem != ComponentAssignmentStrategy.Single) {
        switch (SubgraphLayoutItem) {
          case EnumSubgraphLayouts.Hierarchical:
            subgraphLayout = new HierarchicalLayout();
            break;
          case EnumSubgraphLayouts.Organic:
            subgraphLayout = new OrganicLayout();
            break;
          case EnumSubgraphLayouts.Circular:
            subgraphLayout = new CircularLayout();
            break;
          case EnumSubgraphLayouts.Orthogonal:
            subgraphLayout = new OrthogonalLayout();
            break;
        }
      }
      layout.CoreLayout = subgraphLayout;

      return layout;
    }

    private static yWorks.Layout.Partial.ComponentAssignmentStrategy GetComponentAssignmentStrategy(
        ComponentAssignmentStrategy strategy) {
      switch (strategy) {
        default:
        case ComponentAssignmentStrategy.Customized:
        case ComponentAssignmentStrategy.Single:
          return yWorks.Layout.Partial.ComponentAssignmentStrategy.Single;
        case ComponentAssignmentStrategy.Connected:
          return yWorks.Layout.Partial.ComponentAssignmentStrategy.Connected;
        case ComponentAssignmentStrategy.Clustering:
          return yWorks.Layout.Partial.ComponentAssignmentStrategy.Clustering;
      }
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var layoutData = new PartialLayoutData<INode, IEdge, ILabel, ILabel>();
      var selection = graphControl.Selection;

      layoutData.Scope.Nodes.Source = selection.Nodes;
      layoutData.Scope.Edges.Source = selection.Edges;

      if (ComponentAssignmentStrategyItem == ComponentAssignmentStrategy.Customized) {
        layoutData.ComponentIds.Constant = new object();
      }

      return layoutData;
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

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    public enum EnumSubgraphLayouts
    {
      Hierarchical,
      Organic,
      Circular,
      Orthogonal,
      AsIs
    }

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>Partial layout arranges user-specified parts of a diagram, the so-called partial elements, " +
        "while keeping the other parts fixed. " + "It is related to incremental graph layout. " +
        "This concept is a perfect fit for incremental scenarios where subsequently added parts should be arranged so that " +
        "they fit into a given, unchanged diagram.</Paragraph>" +
        "<Paragraph>In a first step, partial elements are combined to form subgraph components. " +
        "Subsequently, these are arranged and afterwards placed so that " +
        "the remainder of the diagram, which consists of the so-called fixed elements, is not affected.</Paragraph>" +
        "<Paragraph>Placing a subgraph component predominantly means finding a good position that both meets certain proximity criteria and " +
        "offers enough space to accommodate the subgraph component.</Paragraph>";

    [Label("Edge Routing Style")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(RoutingStyle.Automatic)]
    [EnumValue("Auto-Detect", RoutingStyle.Automatic)]
    [EnumValue("Octilinear", RoutingStyle.Octilinear)]
    [EnumValue("Straight-Line", RoutingStyle.StraightLine)]
    [EnumValue("Orthogonal", RoutingStyle.Orthogonal)]
    [EnumValue("Organic", RoutingStyle.Organic)]
    public RoutingStyle RoutingToSubgraphItem { get; set; }

    [Label("Placement Strategy")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(ComponentAssignmentStrategy.Connected)]
    [EnumValue("Connected Nodes as a Unit", ComponentAssignmentStrategy.Connected)]
    [EnumValue("Each Node Separately", ComponentAssignmentStrategy.Single)]
    [EnumValue("All Nodes as a Unit", ComponentAssignmentStrategy.Customized)]
    [EnumValue("Clustering", ComponentAssignmentStrategy.Clustering)]
    public ComponentAssignmentStrategy ComponentAssignmentStrategyItem { get; set; }

    [Label("Subgraph Layout")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(EnumSubgraphLayouts.Hierarchical)]
    [EnumValue("Hierarchical", EnumSubgraphLayouts.Hierarchical)]
    [EnumValue("Organic", EnumSubgraphLayouts.Organic)]
    [EnumValue("Circular", EnumSubgraphLayouts.Circular)]
    [EnumValue("Orthogonal", EnumSubgraphLayouts.Orthogonal)]
    [EnumValue("As Is", EnumSubgraphLayouts.AsIs)]
    public EnumSubgraphLayouts SubgraphLayoutItem { get; set; }

    [Label("Preferred Placement")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(SubgraphPlacement.FromSketch)]
    [EnumValue("Close to Initial Position", SubgraphPlacement.FromSketch)]
    [EnumValue("Close to Neighbors", SubgraphPlacement.Barycenter)]
    public SubgraphPlacement SubgraphPlacementItem { get; set; }

    [Label("Minimum Node Distance")]
    [OptionGroup("GeneralGroup", 50)]
    [DefaultValue(30)]
    [MinMax(Min = 1, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumNodeDistanceItem { get; set; }

    [Label("Orientation")]
    [OptionGroup("GeneralGroup", 60)]
    [DefaultValue(PartialLayoutOrientation.AutoDetect)]
    [EnumValue("Auto Detect", PartialLayoutOrientation.AutoDetect)]
    [EnumValue("Top to Bottom", PartialLayoutOrientation.TopToBottom)]
    [EnumValue("Left to Right", PartialLayoutOrientation.LeftToRight)]
    [EnumValue("Bottom to Top", PartialLayoutOrientation.BottomToTop)]
    [EnumValue("Right to Left", PartialLayoutOrientation.RightToLeft)]
    [EnumValue("None", PartialLayoutOrientation.None)]
    public PartialLayoutOrientation OrientationItem { get; set; }

    [Label("Align Nodes")]
    [OptionGroup("GeneralGroup", 70)]
    [DefaultValue(true)]
    public bool AlignNodesItem { get; set; }

    [Label("Allow Moving Fixed Elements")]
    [OptionGroup("GeneralGroup", 80)]
    [DefaultValue(false)]
    public bool MoveFixedElementsItem { get; set; }

    public enum ComponentAssignmentStrategy : sbyte
    {
      Single = 0,
      Connected = 1,
      Clustering = 2,
      Customized = 3
    }
  }
}
