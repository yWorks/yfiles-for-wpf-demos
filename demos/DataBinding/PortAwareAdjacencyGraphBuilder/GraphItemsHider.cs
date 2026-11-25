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

using System.Collections.Generic;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.DataBinding.PortAwareAdjacencyGraphBuilder
{
  internal static class GraphItemsHider
  {
    private static readonly Dictionary<IModelItem, object> itemToStyle = [];

    /// <summary>
    ///   Hides the visualizations of the given nodes and their related items.
    ///   In this context, the items related to a node are
    ///   - the edges connected to the node,
    ///   - the node's labels,
    ///   - the node's ports, and
    ///   - the labels of the node's ports.
    ///   This method does not remove items from the given graph.
    ///   This method has to be called prior to using method ShowNodesAndRelatedItems.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes whose visualizations will be hidden.</param>
    public static void HideNodesAndRelatedItems(IGraph graph, IEnumerable<INode> nodes) {
      itemToStyle.Clear();

      foreach (var node in nodes) {
        itemToStyle[node] = node.Style;
        graph.SetStyle(node, VoidNodeStyle.Instance);

        foreach (var edge in graph.EdgesAt(node)) {
          var style = edge.Style;
          if (style != VoidEdgeStyle.Instance) {
            itemToStyle[edge] = style;
            graph.SetStyle(edge, VoidEdgeStyle.Instance);
          }
        }

        foreach (var label in node.Labels) {
          itemToStyle[label] = label.Style;
          graph.SetStyle(label, VoidLabelStyle.Instance);
        }

        foreach (var port in node.Ports) {
          itemToStyle[port] = port.Style;
          graph.SetStyle(port, VoidPortStyle.Instance);

          foreach (var label in port.Labels) {
            itemToStyle[label] = label.Style;
            graph.SetStyle(label, VoidLabelStyle.Instance);
          }
        }
      }
    }

    /// <summary>
    ///   Shows the visualizations of the given nodes and their related items again.
    ///   Before using this method, HideNodesAndRelatedItems has to be called.
    ///   The given graph may not be modified structurally in-between calling
    ///   HideNodesAndRelatedItems and using this method.
    ///   The given set of nodes has to be the same set that was passed to
    ///   HideNodesAndRelatedItems before.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes whose visualizations will be shown.</param>
    public static void ShowNodesAndRelatedItems(IGraph graph, IEnumerable<INode> nodes) {
      foreach (var node in nodes) {
        graph.SetStyle(node, (INodeStyle) itemToStyle[node]);

        foreach (var edge in graph.EdgesAt(node)) {
          graph.SetStyle(edge, (IEdgeStyle) itemToStyle[edge]);
        }

        foreach (var label in node.Labels) {
          graph.SetStyle(label, (ILabelStyle) itemToStyle[label]);
        }

        foreach (var port in node.Ports) {
          graph.SetStyle(port, (IPortStyle) itemToStyle[port]);

          foreach (var label in port.Labels) {
            graph.SetStyle(label, (ILabelStyle) itemToStyle[label]);
          }
        }
      }

      itemToStyle.Clear();
    }
  }
}
