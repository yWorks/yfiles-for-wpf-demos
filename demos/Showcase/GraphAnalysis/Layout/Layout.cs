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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;

namespace yWorks.Demo.GraphAnalysis
{
  public sealed partial class GraphAnalysisDemo
  {
    private bool layoutRunning;

    /// <summary>
    ///   Runs a layout algorithm which calculates new positions and sizes for nodes, edges, and labels.
    /// </summary>
    /// <param name="affected">A list of nodes that are incrementally included in the layout</param>
    /// <param name="derived">Whether the layout is derived presentation and excluded from undo recording.</param>
    private async Task RunLayout(IReadOnlyCollection<INode>? affected, bool derived) {
      if (layoutRunning || !Graph.Nodes.Any()) {
        return;
      }

      layoutRunning = true;
      SetUIDisabled(true);
      var suspension = derived ? ((AnalysisGraph) Graph).SuspendRecording() : null;
      try {
        var (layout, data) = GetOrganicLayoutConfiguration(affected);
        await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(.5), data);
      } finally {
        suspension?.Dispose();
        SetUIDisabled(false);
        layoutRunning = false;
        componentSwitcher?.Invalidate();
      }
    }

    /// <summary>
    ///   Returns a configuration of organic layout that considers centrality results and labels.
    /// </summary>
    /// <param name="affected">A list of affected nodes. If it is defined, the layout only runs on a subset of nodes.</param>
    private (ILayoutAlgorithm, LayoutData<INode, IEdge, ILabel, ILabel>) GetOrganicLayoutConfiguration(
        IReadOnlyCollection<INode>? affected) {
      var organicLayout = new OrganicLayout {
          Deterministic = true,
          ComponentLayout = { Style = ComponentArrangementStyle.TryKeepCenters },
          GenericLabeling = {
              Enabled = true,
              Scope = LabelingScope.EdgeLabels,
              Deterministic = true
          }
      };
      organicLayout.LayoutStages.Prepend(new CentralityStage(organicLayout));

      var organicLayoutData = organicLayout.CreateLayoutData(Graph);
      organicLayoutData.PreferredEdgeLengths.MapperFunction = edge =>
          edge.Labels.Aggregate(100.0, (width, label) => Math.Max(label.GetLayout().Width + 50, width));
      organicLayoutData.MinimumNodeDistances.Constant = 30;
      if (affected != null) {
        organicLayoutData.Scope.ScopeModes.MapperFunction = node =>
            affected.Contains(node) ? OrganicScope.IncludeExtendedNeighborhood : OrganicScope.Fixed;
      }

      var labelingData = organicLayout.GenericLabeling.CreateLayoutData(Graph);
      labelingData.EdgeLabelPreferredPlacements.MapperFunction = label => new EdgeLabelPreferredPlacement {
          EdgeSide = Equals(label.Tag, "centrality") ? LabelEdgeSides.OnEdge : LabelEdgeSides.LeftOfEdge,
          DistanceToEdge = 5
      };

      var centralityData = Graph.CreateGenericLayoutData();
      var centrality = result?.Centrality;
      centralityData.AddItemMapping(CentralityStage.CentralityKey).MapperFunction = node =>
          centrality != null && centrality.TryGetValue(node, out var value) ? value : null;

      return (organicLayout, organicLayoutData.CombineWith(labelingData, centralityData));
    }
  }
}
