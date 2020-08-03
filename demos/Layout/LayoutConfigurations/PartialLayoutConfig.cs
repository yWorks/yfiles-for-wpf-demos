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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Partial;
using LayoutOrientation = yWorks.Layout.Partial.LayoutOrientation;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("PartialLayout")]
  public class PartialLayoutConfig : LayoutConfiguration {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public PartialLayoutConfig() {
      RoutingToSubgraphItem = EdgeRoutingStrategy.Automatic;
      ComponentAssignmentStrategyItem = ComponentAssignmentStrategy.Connected;
      SubgraphLayoutItem = EnumSubgraphLayouts.Hierarchic;
      SubgraphPlacementItem = SubgraphPlacement.FromSketch;
      MinimumNodeDistanceItem = 30;
      OrientationItem = LayoutOrientation.AutoDetect;
      AlignNodesItem = true;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new PartialLayout();
      layout.ConsiderNodeAlignment = AlignNodesItem;
      layout.MinimumNodeDistance = MinimumNodeDistanceItem;
      layout.SubgraphPlacement = SubgraphPlacementItem;
      layout.ComponentAssignmentStrategy = ComponentAssignmentStrategyItem;
      layout.LayoutOrientation = OrientationItem;
      layout.EdgeRoutingStrategy = RoutingToSubgraphItem;

      ILayoutAlgorithm subgraphLayout = null;
      if (ComponentAssignmentStrategyItem != ComponentAssignmentStrategy.Single) {
        switch (SubgraphLayoutItem) {
          case EnumSubgraphLayouts.Hierarchic:
            subgraphLayout = new HierarchicLayout();
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

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new PartialLayoutData();
      var selection = graphControl.Selection;

      layoutData.AffectedNodes.Source = selection.SelectedNodes;
      layoutData.AffectedEdges.Source = selection.SelectedEdges;

      return layoutData;
    }

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup; 

    [Label("Layout")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LayoutGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    public enum EnumSubgraphLayouts {
      Hierarchic, Organic, Circular, Orthogonal, AsIs
    }

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText {
      get {
        return "<Paragraph>Partial layout arranges user-specified parts of a diagram, the so-called partial elements, "
               + "while keeping the other parts fixed. "
               + "It is related to incremental graph layout. "
               + "This concept is a perfect fit for incremental scenarios where subsequently added parts should be arranged so that "
               + "they fit into a given, unchanged diagram.</Paragraph>"
               + "<Paragraph>In a first step, partial elements are combined to form subgraph components. "
               + "Subsequently, these are arranged and afterwards placed so that "
               +"the remainder of the diagram, which consists of the so-called fixed elements, is not affected.</Paragraph>"
               +"<Paragraph>Placing a subgraph component predominantly means finding a good position that both meets certain proximity criteria and "
               + "offers enough space to accommodate the subgraph component.</Paragraph>";
      }
    } 

    [Label("Edge Routing Style")]
    [OptionGroup("LayoutGroup", 10)]
    [DefaultValue(EdgeRoutingStrategy.Automatic)]
    [EnumValue("Auto-Detect", EdgeRoutingStrategy.Automatic)]
    [EnumValue("Octilinear",EdgeRoutingStrategy.Octilinear)]
    [EnumValue("Straight-Line",EdgeRoutingStrategy.Straightline)]
    [EnumValue("Orthogonal",EdgeRoutingStrategy.Orthogonal)]
    [EnumValue("Organic",EdgeRoutingStrategy.Organic)]
    public EdgeRoutingStrategy RoutingToSubgraphItem { get; set; }

    [Label("Placement Strategy")]
    [OptionGroup("LayoutGroup", 20)]
    [DefaultValue(ComponentAssignmentStrategy.Connected)]
    [EnumValue("Connected Nodes as a Unit", ComponentAssignmentStrategy.Connected)]                                     
    [EnumValue("Each Node Separately",ComponentAssignmentStrategy.Single)]
    [EnumValue("All Nodes as a Unit",ComponentAssignmentStrategy.Customized)]
    [EnumValue("Clustering",ComponentAssignmentStrategy.Clustering)]
    public ComponentAssignmentStrategy ComponentAssignmentStrategyItem { get; set; }

    [Label("Subgraph Layout")]
    [OptionGroup("LayoutGroup", 30)]
    [DefaultValue(EnumSubgraphLayouts.Hierarchic)]
    [EnumValue("Hierarchical", EnumSubgraphLayouts.Hierarchic)] 
    [EnumValue("Organic",EnumSubgraphLayouts.Organic)]
    [EnumValue("Circular",EnumSubgraphLayouts.Circular)]
    [EnumValue("Orthogonal",EnumSubgraphLayouts.Orthogonal)]
    [EnumValue("As Is",EnumSubgraphLayouts.AsIs)]
    public EnumSubgraphLayouts SubgraphLayoutItem { get; set; }

    [Label("Preferred Placement")]
    [OptionGroup("LayoutGroup", 40)]
    [DefaultValue(SubgraphPlacement.FromSketch)]
    [EnumValue("Close to Initial Position", SubgraphPlacement.FromSketch)] 
    [EnumValue("Close to Neighbors",SubgraphPlacement.Barycenter)]
    public SubgraphPlacement SubgraphPlacementItem { get; set; }

    [Label("Minimum Node Distance")]
    [OptionGroup("LayoutGroup", 50)]
    [DefaultValue(30)]
    [MinMax(Min = 1, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumNodeDistanceItem { get; set; }

    [Label("Orientation")]
    [OptionGroup("LayoutGroup", 60)]
    [DefaultValue(LayoutOrientation.AutoDetect)]
    [EnumValue("Auto Detect", LayoutOrientation.AutoDetect)]
    [EnumValue("Top to Bottom", LayoutOrientation.TopToBottom )]
    [EnumValue("Left to Right", LayoutOrientation.LeftToRight )]
    [EnumValue("Bottom to Top", LayoutOrientation.BottomToTop )]
    [EnumValue("Right to Left", LayoutOrientation.RightToLeft )]
    [EnumValue("None", LayoutOrientation.None )]
    public LayoutOrientation OrientationItem { get; set; }

    [Label("Align Nodes")]
    [OptionGroup("LayoutGroup", 70)]
    [DefaultValue(true)]
    public bool AlignNodesItem { get; set; }

  }
}