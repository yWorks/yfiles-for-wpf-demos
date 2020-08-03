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

using System.Collections.Generic;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Layout;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="ParallelEdgeRouter"/>.
  /// </summary>
  public class ParallelEdgeRouterModule : LayoutModule
  {
    #region configuration constants

    private const string TOP_LEVEL = "TOP_LEVEL";
    private const string NAME = "PARALLEL_EDGES";
    private const string CONSIDER_EDGE_DIRECTION = "CONSIDER_EDGE_DIRECTION";
    private const string USE_ADAPTIVE_LINE_DISTANCE = "USE_ADAPTIVE_LINE_DISTANCE";
    private const string LINE_DISTANCE = "LINE_DISTANCE";
    private const string JOINS_ENDS = "JOINS_ENDS";
    private const string JOIN_DISTANCE = "JOIN_DISTANCE";
    private const string USE_SELECTED_EDGES_AS_MASTER = "USE_SELECTED_EDGES_AS_MASTER";
    private const string SCOPE = "SCOPE";
    private const string SCOPE_ALL_EDGES = "ALL_EDGES";
    private const string SCOPE_SELECTED_EDGES = "SELECTED_EDGES";
    private const string SCOPE_AT_SELECTED_NODES = "AT_SELECTED_NODES";
    private static readonly List<string> scopes = new List<string>();

    static ParallelEdgeRouterModule() {
      scopes.Add(SCOPE_ALL_EDGES);
      scopes.Add(SCOPE_SELECTED_EDGES);
      scopes.Add(SCOPE_AT_SELECTED_NODES);
    }

    #endregion

    private ParallelEdgeRouter router;

    /// <summary>
    /// Create new instance
    /// </summary>
    public ParallelEdgeRouterModule() : base(NAME) {}


    ///<inheritdoc/>
    protected override void SetupHandler() {
      createRouter();
      OptionGroup layoutGroup = Handler.AddGroup(TOP_LEVEL);
      layoutGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      layoutGroup.Attributes[DefaultEditorFactory.RenderingHintsAttribute] =
        DefaultEditorFactory.RenderingHints.Invisible;

      ConstraintManager cm = new ConstraintManager(Handler);

      layoutGroup.AddList(SCOPE, scopes, SCOPE_ALL_EDGES);
      layoutGroup.AddBool(USE_SELECTED_EDGES_AS_MASTER, false);
      layoutGroup.AddBool(CONSIDER_EDGE_DIRECTION, router.DirectedMode);
      layoutGroup.AddBool(USE_ADAPTIVE_LINE_DISTANCE, router.AdaptiveLineDistances);
      layoutGroup.AddInt(LINE_DISTANCE, (int)router.LineDistance, 0, 50);
      IOptionItem joinEndsItem = layoutGroup.AddBool(JOINS_ENDS, router.JoinEnds);
      IOptionItem joinDistanceItem = layoutGroup.AddInt(JOIN_DISTANCE, (int)router.AbsJoinEndDistance, 0, 50);

      cm.SetEnabledOnValueEquals(joinEndsItem, true, joinDistanceItem);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      OptionGroup layoutGroup = Handler.GetGroupByName(TOP_LEVEL);
      router.AdjustLeadingEdge = false;
      router.DirectedMode = (bool) layoutGroup[CONSIDER_EDGE_DIRECTION].Value;
      router.AdaptiveLineDistances = (bool) layoutGroup[USE_ADAPTIVE_LINE_DISTANCE].Value;
      router.LineDistance = (int) layoutGroup[LINE_DISTANCE].Value;
      router.JoinEnds = (bool) layoutGroup[JOINS_ENDS].Value;
      router.AbsJoinEndDistance = (int) layoutGroup[JOIN_DISTANCE].Value;
      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(layoutGroup[JOINS_ENDS], true, layoutGroup[JOIN_DISTANCE]);
      LayoutAlgorithm = router;
    }

    protected override void PerformPreLayout() {
      base.PerformPreLayout();
      OptionGroup layoutGroup = Handler.GetGroupByName(TOP_LEVEL);
      string choice = (string)layoutGroup[SCOPE].Value;
      if (choice == SCOPE_AT_SELECTED_NODES) {
        CurrentLayoutGraph.AddDataProvider(ParallelEdgeRouter.AffectedEdgesDpKey, new SelectedNodesDP(this));
      } else if (choice == SCOPE_SELECTED_EDGES) {
        CurrentLayoutGraph.AddDataProvider(ParallelEdgeRouter.AffectedEdgesDpKey, new SelectedEdgesDP(this));
      } else {
        CurrentLayoutGraph.AddDataProvider(ParallelEdgeRouter.AffectedEdgesDpKey, DataProviders.CreateConstantDataProvider(true));
      }

      if((bool) layoutGroup[USE_SELECTED_EDGES_AS_MASTER].Value) {
        CurrentLayoutGraph.AddDataProvider(ParallelEdgeRouter.LeadingEdgeDpKey, new SelectedEdgesDP(this));
      }
    }

    protected override void PerformPostLayout() {
      base.PerformPostLayout();
      CurrentLayoutGraph.RemoveDataProvider(ParallelEdgeRouter.AffectedEdgesDpKey);
      CurrentLayoutGraph.RemoveDataProvider(ParallelEdgeRouter.LeadingEdgeDpKey);
    }

    private void createRouter() {
      if (router != null) {
        return;
      }
      router = new ParallelEdgeRouter();
    }

    internal class SelectedEdgesDP : DataProviderAdapter
    {
      private readonly LayoutModule module;

      public SelectedEdgesDP(LayoutModule module) {
        this.module = module;
      }

      public override bool GetBool(object o) {
        return module.IsSelected((Edge)o);
      }
    }

    internal class SelectedNodesDP : DataProviderAdapter
    {
      private readonly LayoutModule module;

      public SelectedNodesDP(LayoutModule module) {
        this.module = module;
      }

      public override bool GetBool(object o) {
        return module.IsSelected(((Edge)o).Source) || module.IsSelected(((Edge)o).Target);
      }
    }
  }

}