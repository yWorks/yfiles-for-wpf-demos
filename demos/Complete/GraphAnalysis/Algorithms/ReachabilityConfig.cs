/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Analysis;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.Algorithms.GraphAnalysis
{
  /// <summary>
  /// Calculates the reachability.
  /// </summary>
  public class ReachabilityConfig : AlgorithmConfiguration
  {
    private INode markedSource;

    public override bool SupportsDirectedAndUndirected {
      get { return true; }
    }

    public override void RunAlgorithm(IGraph graph) {
      ResetGraph(graph);

      if (graph.Nodes.Any()) {
        if (markedSource == null || !graph.Contains(markedSource)) {
          markedSource = graph.Nodes.First();
        }
      }

      var result = new Reachability {
          Directed = Directed, 
          StartNodes = {Item = markedSource}
      }.Run(graph);

      foreach (var node in result.ReachableNodes) {
        node.Tag = new Tag {
            CurrentColor = Colors.Blue,
            IsSource = node == markedSource
        };
      }

      foreach (var edge in graph.Edges) {
        if (result.IsReachable(edge.GetSourceNode()) &&
            result.IsReachable(edge.GetTargetNode())) {
          edge.Tag = new Tag {
              CurrentColor = Colors.Blue,
              Directed = Directed
          };
        }
      }
    }

    /// <summary>
    /// Adds a context menu to mark the source node for the reachability algorithm.
    /// </summary>
    public override void PopulateContextMenu(PopulateItemContextMenuEventArgs<IModelItem> args) {
      var item = args.Item as INode;
      if (item != null) {
        var menuItem = new MenuItem { Header = "Mark as Source" };
        menuItem.Click += (o, e) => {
          markedSource = item;
          var graph = args.Context.GetGraph();
          ResetGraph(graph);
          RunAlgorithm(graph);
        };
        args.Menu.Items.Add(menuItem);
      }
    }
  }
}
