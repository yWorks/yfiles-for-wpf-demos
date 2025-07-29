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
using yWorks.Layout.Router;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("ParallelEdgeRouter")]
  public class ParallelEdgeRouterConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public ParallelEdgeRouterConfig() {
      var router = new ParallelEdgeRouter();
      ScopeItem = EnumScope.ScopeAllEdges;
      UseSelectedEdgesAsMasterItem = false;
      ConsiderEdgeDirectionItem = router.DirectedMode;
      UseAdaptiveLineDistanceItem = router.AdaptiveEdgeDistances;
      LineDistanceItem = (int) router.EdgeDistance;
      JoinEndsItem = router.JoinEnds;
      JoinDistanceItem = router.AbsoluteJoinEndDistance;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var router = new ParallelEdgeRouter();
      router.AdjustLeadingEdge = false;
      router.DirectedMode = ConsiderEdgeDirectionItem;
      router.AdaptiveEdgeDistances = UseAdaptiveLineDistanceItem;
      router.EdgeDistance = LineDistanceItem;
      router.JoinEnds = JoinEndsItem;
      router.AbsoluteJoinEndDistance = JoinDistanceItem;

      return router;
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var layoutData = new ParallelEdgeRouterData<INode, IEdge, ILabel, ILabel>();
      var selection = graphControl.Selection;

      if (ScopeItem == EnumScope.ScopeAtSelectedNodes) {
        layoutData.AffectedEdges.Predicate = edge =>
            selection.Contains(edge.GetSourceNode()) || selection.Contains(edge.GetTargetNode());
      } else if (ScopeItem == EnumScope.ScopeSelectedEdges) {
        layoutData.AffectedEdges.Source = selection.Edges;
      } else {
        layoutData.AffectedEdges.Predicate = edge => true;
      }

      if (UseSelectedEdgesAsMasterItem) {
        layoutData.LeadingEdges.Source = selection.Edges;
      }

      return layoutData;
    }


    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming

    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>The parallel edge routing algorithm routes parallel edges which connect the same pair of nodes in a graph." +
        " It is often used as layout stage for other layout algorithms to handle the parallel edges for those.</Paragraph>";

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LayoutGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    public enum EnumScope
    {
      ScopeAllEdges,
      ScopeSelectedEdges,
      ScopeAtSelectedNodes
    }

    [Label("Scope")]
    [OptionGroup("LayoutGroup", 10)]
    [DefaultValue(EnumScope.ScopeAllEdges)]
    [EnumValue("All Edges", EnumScope.ScopeAllEdges)]
    [EnumValue("Selected Edges", EnumScope.ScopeSelectedEdges)]
    [EnumValue("Edges at Selected Nodes", EnumScope.ScopeAtSelectedNodes)]
    public EnumScope ScopeItem { get; set; }

    [Label("Use Selected Edges As Leading Edges")]
    [OptionGroup("LayoutGroup", 20)]
    [DefaultValue(false)]
    public bool UseSelectedEdgesAsMasterItem { get; set; }

    [Label("Consider Edge Direction")]
    [OptionGroup("LayoutGroup", 30)]
    [DefaultValue(false)]
    public bool ConsiderEdgeDirectionItem { get; set; }

    [Label("Use Adaptive Line Distance")]
    [OptionGroup("LayoutGroup", 40)]
    [DefaultValue(true)]
    public bool UseAdaptiveLineDistanceItem { get; set; }

    [Label("Line Distance")]
    [OptionGroup("LayoutGroup", 50)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 50)]
    [ComponentType(ComponentTypes.Slider)]
    public int LineDistanceItem { get; set; }

    [Label("Join Ends")]
    [OptionGroup("LayoutGroup", 60)]
    [DefaultValue(false)]
    public bool JoinEndsItem { get; set; }

    [Label("Join Distance")]
    [OptionGroup("LayoutGroup", 70)]
    [DefaultValue(20.0d)]
    [MinMax(Min = 0, Max = 50)]
    [ComponentType(ComponentTypes.Slider)]
    public double JoinDistanceItem { get; set; }

    public bool ShouldDisableJoinDistanceItem => !JoinEndsItem;
  }
}
