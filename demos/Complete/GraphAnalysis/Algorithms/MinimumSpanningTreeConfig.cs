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
  /// Calculates the minimum spanning tree.
  /// </summary>
  public class MinimumSpanningTreeConfig : AlgorithmConfiguration
  {
    public override bool SupportsWeights {
      get { return true; }
    }

    public override void RunAlgorithm(IGraph graph) {
      // reset edge styles
      foreach (var edge in graph.Edges) {
        graph.SetStyle(edge, graph.EdgeDefaults.Style);
      }
      // calculate the spanning tree
      var result = new SpanningTree {Costs = {Delegate = GetEdgeWeight}}.Run(graph);
      // highlight the tree nodes and edges
      var nodes = new HashSet<INode>();
      foreach (var edge in result.Edges) {
        edge.Tag = new Tag{CurrentColor = Colors.Blue};
        nodes.Add(edge.GetSourceNode());
        nodes.Add(edge.GetTargetNode());
      }
      foreach (var node in nodes) {
        node.Tag = new Tag {
            CurrentColor = Colors.Blue
        };
      }
    }
  }
}
