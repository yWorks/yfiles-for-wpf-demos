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

using System.ComponentModel;
using System.Reflection;
using yWorks.Controls;
using yWorks.Demo.Toolkit.OptionHandler;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("OrganicEdgeRouter")]
  public class OrganicEdgeRouterConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public OrganicEdgeRouterConfig() {
      var router = new OrganicEdgeRouter();
      SelectionOnlyItem = false;
      MinimumNodeDistanceItem = router.MinimumDistance;
      KeepExistingBendsItem = router.KeepExistingBends;
      RouteOnlyNecessaryItem = !router.RouteAllEdges;
      AllowMovingNodesItem = false;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var router = new OrganicEdgeRouter();
      router.MinimumDistance = MinimumNodeDistanceItem;
      router.KeepExistingBends = KeepExistingBendsItem;
      router.RouteAllEdges = !RouteOnlyNecessaryItem;
      router.AllowMovingNodes = AllowMovingNodesItem;
      return router;
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var layoutData = new OrganicEdgeRouterData<INode, IEdge, ILabel, ILabel>();

      if (SelectionOnlyItem) {
        layoutData.Scope.Edges.Source = graphControl.Selection.Edges;
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

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>The organic edge routing algorithm routes edges in soft curves to ensure that they do not overlap with nodes." +
        " It is especially well suited for non-orthogonal, organic or circular diagrams.</Paragraph>";

    [Label("Route Selected Edges Only")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(false)]
    public bool SelectionOnlyItem { get; set; }

    [Label("Minimum Distance")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumNodeDistanceItem { get; set; }

    [Label("Keep Existing Bends")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(false)]
    public bool KeepExistingBendsItem { get; set; }

    [Label("Route Only Necessary")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(true)]
    public bool RouteOnlyNecessaryItem { get; set; }

    [Label("Allow Moving Nodes")]
    [OptionGroup("GeneralGroup", 50)]
    [DefaultValue(false)]
    public bool AllowMovingNodesItem { get; set; }
  }
}
