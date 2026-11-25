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

using System.Linq;
using System.Reflection;
using yWorks.Graph;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   Configures bidirectional highlighting between the Biofabric and circular node-link diagrams.
  /// </summary>
  public static class ConfigureBidirectionalInteraction
  {
    /// <summary>
    ///   Compares two tags for shallow equality to match corresponding nodes and edges across both graphs.
    /// </summary>
    private static bool ShallowEquals(object? first, object? second) {
      if (ReferenceEquals(first, second)) {
        return true;
      }
      if (first is null || second is null || first.GetType() != second.GetType()) {
        return false;
      }
      var properties = first.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
      return properties.Length == second.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Length &&
          properties.All(property => Equals(property.GetValue(first), property.GetValue(second)));
    }

    /// <summary>
    ///   Sets callbacks to find and highlight corresponding graph items in the other visualization.
    /// </summary>
    public static void Configure(Biofabric biofabric, CircularNodeLink nodeLink) {
      biofabric.HighlightItemCallback = (item, adjacent) => {
        if (item is INode node) {
          var correspondingNode =
              nodeLink.GraphComponent.Graph.Nodes.FirstOrDefault(other => ShallowEquals(other.Tag, node.Tag));
          if (correspondingNode is not null) {
            nodeLink.AddHighlight(correspondingNode, adjacent, true);
          }
        } else if (item is IEdge edge) {
          var correspondingEdge =
              nodeLink.GraphComponent.Graph.Edges.FirstOrDefault(other => ShallowEquals(other.Tag, edge.Tag));
          if (correspondingEdge is not null) {
            nodeLink.AddHighlight(correspondingEdge, adjacent, true);
          }
        } else if (item is ILabel label) {
          var correspondingLabel =
              nodeLink.GraphComponent.Graph.Labels.FirstOrDefault(other => other.Text == label.Text);
          if (correspondingLabel is not null) {
            nodeLink.AddHighlight(correspondingLabel, adjacent, true);
          }
        } else if (item is NodeGroup nodeGroup &&
            nodeLink.NodeGroups.TryGetValue(nodeGroup.Id, out var correspondingGroup)) {
          correspondingGroup.Highlighted = true;
        }
      };
      biofabric.ClearItemCallback = () => nodeLink.ClearHighlights(true);

      nodeLink.HighlightItemCallback = (item, adjacent) => {
        if (item is INode node) {
          var correspondingNode =
              biofabric.GraphComponent.Graph.Nodes.FirstOrDefault(other => ShallowEquals(other.Tag, node.Tag));
          if (correspondingNode is not null) {
            biofabric.AddHighlight(correspondingNode, adjacent, true);
          }
        } else if (item is IEdge edge) {
          var correspondingEdge =
              biofabric.GraphComponent.Graph.Edges.FirstOrDefault(other => ShallowEquals(other.Tag, edge.Tag));
          if (correspondingEdge is not null) {
            biofabric.AddHighlight(correspondingEdge, adjacent, true);
            foreach (var label in correspondingEdge.Labels) {
              biofabric.AddHighlight(label, muteCallback: true);
            }
          }
        } else if (item is ILabel label) {
          var correspondingLabel =
              biofabric.GraphComponent.Graph.Labels.FirstOrDefault(other => other.Text == label.Text);
          if (correspondingLabel is not null) {
            biofabric.AddHighlight(correspondingLabel, adjacent, true);
          }
        } else if (item is NodeGroup nodeGroup &&
            biofabric.NodeGroups.TryGetValue(nodeGroup.Id, out var correspondingGroup)) {
          correspondingGroup.Highlighted = true;
        }
      };
      nodeLink.ClearItemCallback = () => biofabric.ClearHighlights(true);
    }
  }
}
