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
using System.Threading.Tasks;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Utils;

namespace yWorks.Demo.OctilinearEdges
{
  internal static class ApplyOctilinearLayout
  {
    /// <summary>
    ///   The key under which the layout stores the cutting lengths for each edge's bends'. These values
    ///   are then used by the layout animation and written by it into the bend tags
    /// </summary>
    internal static readonly EdgeDataKey<double[]> EdgeBendsCuttingLengthKey = new("edgeBendsCuttingLengthKey");

    /// <summary>
    ///   Applies the given core layout to the graph and transforms the result to an orthogonal layout with the
    ///   <see cref="OctilinearEdgeStyleLayoutStage"/>.
    ///   It is assumed that the core-layout runs an octilinear edge router.
    /// </summary>
    /// <param name="graphControl">The <see cref="GraphControl"/> to which the layout is applied.</param>
    /// <param name="coreLayout">The layout that is run before transforming the result to an orthogonal layout.</param>
    /// <param name="coreLayoutData">The associated layout data for the core layout.</param>
    internal static async Task Apply(
        GraphControl graphControl,
        ILayoutAlgorithm coreLayout,
        LayoutData<INode, IEdge, ILabel, ILabel>? coreLayoutData = null) {
      // Ensure that the edges visualize the cutting length from the layout result. For example, when graphml files with
      // different edge styles have been loaded.
      foreach (var edge in graphControl.Graph.Edges) {
        if (edge.Style is not OctilinearEdgeStyle) {
          foreach (var bend in edge.Bends) {
            Utils.UpdateCuttingLength(bend, 0);
          }
          graphControl.Graph.SetStyle(edge, graphControl.Graph.EdgeDefaults.Style);
        }
      }

      // Register a result data-mapper to receive the octilinear cutting lengths from the layout result.
      var edgeBendsCuttingLengths = new DictionaryMapper<IEdge, double[]>();
      var genericLayoutData = graphControl.Graph.CreateGenericLayoutData();
      genericLayoutData.AddItemMapping(EdgeBendsCuttingLengthKey).Mapper = edgeBendsCuttingLengths;
      LayoutData<INode, IEdge, ILabel, ILabel> layoutData = genericLayoutData;
      if (coreLayoutData != null) {
        layoutData = layoutData.CombineWith(coreLayoutData);
      }

      // Apply the octilinear core layout that introduces bends for each octilinear edge segment, afterward,
      // transform the result to an orthogonal layout and get the calculated octilinear edge segment lengths.
      var layoutExecutor = new OctilinearLayoutExecutor(graphControl, new OctilinearEdgeStyleLayoutStage(coreLayout)) {
          LayoutData = layoutData,
          AnimationDuration = TimeSpan.FromSeconds(1),
          AnimateViewport = true
      };

      // Store the result data mapper on the layout executor such that the layout animation can use the target values.
      layoutExecutor.EdgeBendsCuttingLengths = edgeBendsCuttingLengths;

      await layoutExecutor.Start();
    }

    /// <summary>
    ///   Applies the initial layout synchronously before the demo becomes interactive.
    /// </summary>
    internal static void ApplyInitialOctilinearLayout(GraphControl graphControl, ILayoutAlgorithm coreLayout) {
      var graph = graphControl.Graph;
      var edgeBendsCuttingLengths = new DictionaryMapper<IEdge, double[]>();
      var layoutData = graph.CreateGenericLayoutData();
      layoutData.AddItemMapping(EdgeBendsCuttingLengthKey).Mapper = edgeBendsCuttingLengths;

      graph.ApplyLayout(new OctilinearEdgeStyleLayoutStage(coreLayout), layoutData);
      foreach (var edge in graph.Edges) {
        if (edgeBendsCuttingLengths[edge] is { } lengths && edge.Bends.Count == lengths.Length) {
          for (var i = 0; i < lengths.Length; i++) {
            Utils.UpdateCuttingLength(edge.Bends[i], lengths[i]);
          }
        }
      }
      graphControl.FitGraphBounds();
    }
  }
}
