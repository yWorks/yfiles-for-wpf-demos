/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Organic;
using yWorks.Layout.Router;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="OrganicEdgeRouter"/>.
  /// </summary>
  public class OrganicEdgeRouterModule : LayoutModule
  {
    #region configuration constants

    private const string TOP_LEVEL = "TOP_LEVEL";
    private const string NAME = "ORGANIC_EDGE_ROUTER";
    private const string MINIMAL_NODE_DISTANCE = "MINIMAL_NODE_DISTANCE";
    private const string USE_BENDS = "USE_BENDS";
    private const string ROUTE_ONLY_NECESSARY = "ROUTE_ONLY_NECESSARY";
    private const string SELECTION_ONLY = "SELECTION_ONLY";
    private const string ALLOW_MOVING_NODES = "ALLOW_MOVING_NODES";

    private const string LAYOUT_OPTIONS = "LAYOUT_OPTIONS";

    #endregion

    private OrganicEdgeRouter router;
    private SelectedEdgesDP affectedEdgesDP;

    /// <summary>
    /// Create new instance
    /// </summary>
    public OrganicEdgeRouterModule() : base(NAME) {}

    ///<inheritdoc/>
    protected override void SetupHandler() {
      createRouter();
      OptionGroup layoutGroup = Handler.AddGroup(LAYOUT_OPTIONS);
      layoutGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      layoutGroup.Attributes[DefaultEditorFactory.RenderingHintsAttribute] = DefaultEditorFactory.RenderingHints.Invisible;
      layoutGroup.AddBool(SELECTION_ONLY, false);
      layoutGroup.AddInt(MINIMAL_NODE_DISTANCE, (int)router.MinimumDistance, 10, int.MaxValue);
      layoutGroup.AddBool(USE_BENDS, router.KeepExistingBends);
      layoutGroup.AddBool(ROUTE_ONLY_NECESSARY, !router.RouteAllEdges);
      layoutGroup.AddBool(ALLOW_MOVING_NODES, false);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      OptionGroup layoutGroup = (OptionGroup) Handler.GetGroupByName(LAYOUT_OPTIONS);
      router.MinimumDistance = (int) layoutGroup[MINIMAL_NODE_DISTANCE].Value;
      router.KeepExistingBends = (bool) layoutGroup[USE_BENDS].Value;
      router.RouteAllEdges = (bool) layoutGroup[ROUTE_ONLY_NECESSARY].Value;

      SequentialLayout sl = new SequentialLayout();
      if ((bool) layoutGroup[ALLOW_MOVING_NODES].Value) {
        //if we are allowed to move nodes, we can improve the routing results by temporarily enlarging nodes and removing overlaps
        //(this strategy ensures that there is enough space for the edges)
        CompositeLayoutStage cls = new CompositeLayoutStage();
        cls.AppendStage(router.CreateNodeEnlargementStage());
        cls.AppendStage(new RemoveOverlapsStage(0));
        sl.AppendLayout(cls);
      }
      if (router.KeepExistingBends) {
        //we want to keep the original bends
        BendConverter bendConverter = new BendConverter();
        bendConverter.AffectedEdgesDpKey = OrganicEdgeRouter.AffectedEdgesDpKey;
        bendConverter.AdoptAffectedEdges = (bool) layoutGroup[SELECTION_ONLY].Value;
        bendConverter.CoreLayout = router;
        sl.AppendLayout(bendConverter);
      } else {
        sl.AppendLayout(router);
      }

      LayoutAlgorithm = new HideGroupsStage(sl);
    }


    protected override void PerformPreLayout() {
      base.PerformPreLayout();
      OptionGroup layoutGroup = (OptionGroup) Handler.GetGroupByName(LAYOUT_OPTIONS);
      if ((bool)layoutGroup[SELECTION_ONLY].Value) {
        affectedEdgesDP = new SelectedEdgesDP(this);
        CurrentLayoutGraph.AddDataProvider(OrganicEdgeRouter.AffectedEdgesDpKey, affectedEdgesDP);
      }
    }

    protected override void PerformPostLayout() {
      base.PerformPostLayout();
      if (affectedEdgesDP != null) {
        CurrentLayoutGraph.RemoveDataProvider(OrganicEdgeRouter.AffectedEdgesDpKey);
      }
    }

    private void createRouter() {
      if (router != null) {
        return;
      }
      router = new OrganicEdgeRouter();
    }

    internal class SelectedEdgesDP : DataProviderAdapter
    {
      private LayoutModule module;

      public SelectedEdgesDP(LayoutModule module) {
        this.module = module;
      }

      public override bool GetBool(object o) {
        return module.IsSelected((Edge)o);
      }
    }
  }
}
