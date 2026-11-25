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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.EdgeRouterIncremental
{
  /// <summary>
  ///   Demonstrates how to configure <see cref="EdgeRouter"/> to route a predefined set of edges only.
  /// </summary>
  [Demo("layout-edge-router-incremental", "layout-features", "Incremental Edge Router",
      Summary = "Shows how to run the edge router on a predefined set of edges.", Tags = ["incremental", "routing"],
      Keywords = ["layout", "subset", "edges", "set", "edge-set", "playground"])]
  public class EdgeRouterIncrementalDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", edgeTagType: typeof(EdgeData));

      GraphControl.FitGraphBounds();

      // Add a button to run the incremental edge routing
      ToolBar.AddButton("Route Edges", async () => await ApplyIncrementalEdgeRouting());
    }

    /**
     * Applies incremental edge routing.
     */
    private async Task ApplyIncrementalEdgeRouting() {
      var edgeRouter = new EdgeRouter();
      var edgeRouterData = edgeRouter.CreateLayoutData(Graph);
      edgeRouterData.Scope.Edges.Predicate = IsIncremental;

      // Apply edge router with incremental routing
      await GraphControl.ApplyLayoutAnimated(edgeRouter, TimeSpan.FromMilliseconds(250), edgeRouterData);
    }

    private static bool IsIncremental(IEdge edge) => edge.Tag is EdgeData data && data.Incremental == "true";

    private record EdgeData(string? Incremental);

    [STAThread]
    private static void Main() => Run<EdgeRouterIncrementalDemo>();
  }
}
