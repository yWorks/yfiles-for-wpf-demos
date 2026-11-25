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
using yWorks.Graph;
using yWorks.Layout.Organic;

namespace yWorks.Demo.Organic
{
  [Demo("layout-organic", "layout-features", "Organic Layout",
      Summary = "Shows common configuration options for organic layout.", Tags = ["organic"],
      Keywords = ["layout", "graph shape", "compactness", "node distance", "simple", "playground"])]
  public class OrganicDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Initialize an organic layout algorithm
      var layout = new OrganicLayout {
          // Produce the same result on each run (default: true)
          Deterministic = true,
          // Increase the preferred lengths of edges (default: 40)
          DefaultPreferredEdgeLength = 100,
          // Control node spacing (range: 0 - 1)
          CompactnessFactor = 0.9,
          // Determine how node clusters are formed (default: NONE)
          ClusteringPolicy = ClusteringPolicy.EdgeBetweenness,
          // Disallow nodes from overlapping (default: false)
          AllowNodeOverlaps = false,
          // Set the minimum distance between nodes (only applies if allowNodeOverlaps is false)
          DefaultMinimumNodeDistance = 10,
          // Prevent nodes and edges from overlapping
          AvoidNodeEdgeOverlap = true,
          // Constrain the result to be wider than tall (aspect ratio 10:1)
          ShapeConstraint = ShapeConstraint.CreateAspectRatioConstraint(10)
      };

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json");

      // Apply the organic layout to the graph
      Graph.ApplyLayout(layout);
      GraphControl.FitGraphBounds();
    }

    [STAThread]
    private static void Main() => Run<OrganicDemo>();
  }
}
