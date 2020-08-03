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
using System.Windows.Media;
using yWorks.Analysis;
using yWorks.Graph;

namespace Demo.yFiles.Algorithms.GraphAnalysis
{
  /// <summary>
  /// Finds cycles in a graph.
  /// </summary>
  public class CyclesConfig : AlgorithmConfiguration
  {
    public override bool SupportsDirectedAndUndirected {
      get { return true; }
    }

    /// <summary>
    /// Calculates the cycles.
    /// </summary>
    /// <param name="graph"></param>
    public override void RunAlgorithm(IGraph graph) {
      // reset cycles to remove previous markings
      ResetGraph(graph);

      // find all edges that belong to a cycle
      var cycleEdgeFinder = new CycleEdges { Directed = Directed };
      var result = cycleEdgeFinder.Run(graph);
      MarkCycles(graph, result);
    }

    /// <summary>
    /// Marks the cycle nodes and edges.
    /// </summary>
    private void MarkCycles(IGraph graph, CycleEdges.Result cycleResult) {
      // hides all non-cycle edges to be able to find independent cycles
      var cycleEdgeSet = new HashSet<IEdge>(cycleResult.Edges);
      var filteredGraph = new FilteredGraphWrapper(graph, node => true, edge => cycleEdgeSet.Contains(edge));

      // now find independent cycles
      var result = new ConnectedComponents().Run(filteredGraph);

      // find the components that are affected by the user's move, if any
      var affectedComponents = GetAffectedNodeComponents(result.NodeComponents);
      // find the component with the larger number of elements
      var largestComponent = GetLargestComponent(affectedComponents);
      // hold a color for each affected components
      var color2AffectedComponent = new Dictionary<Component, Color>();
      // generate the colors for the components
      var colors = GenerateColors(false);

      for (var i = 0; i < result.Components.Count; i++) {
        var component = result.Components[i];

        foreach (var edge in component.InducedEdges) {

          // the source and target node get the same color, depending on their connecting edge
          var color = DetermineElementColor(colors, component, affectedComponents, color2AffectedComponent, largestComponent, result.Components, graph, edge);
          edge.Tag = new Tag {
              CurrentColor = color,
              Directed = Directed
          };
          var source = edge.GetSourceNode();
          source.Tag = new Tag {
              CurrentColor = color
          };

          var target = edge.GetTargetNode();
          target.Tag = new Tag {
              CurrentColor = color
          };
        }
      }
    }
   
  }
}
