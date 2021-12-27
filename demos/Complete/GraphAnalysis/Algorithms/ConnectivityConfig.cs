/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Analysis;
using yWorks.Graph;

namespace Demo.yFiles.Algorithms.GraphAnalysis
{

  /// <summary>
  /// Determines connected components.
  /// </summary>
  /// <remarks>
  /// Supports connected components, biconnected components, and strongly connected components.
  /// </remarks>
  public class ConnectivityConfig : AlgorithmConfiguration
  {
    private readonly ConnectivityMode algorithmType;

    public ConnectivityConfig(ConnectivityMode algorithmType) {
      this.algorithmType = algorithmType;
      Directed = algorithmType == ConnectivityMode.StronglyConnectedComponents;
    }

    public override void RunAlgorithm(IGraph graph) {
      switch (algorithmType) {
        case ConnectivityMode.BiconnectedComponents:
          CalculateBiconnectedComponents(graph); 
          break;
        case ConnectivityMode.StronglyConnectedComponents:
          CalculateConnectedComponents(graph, true);
          break;
        case ConnectivityMode.ConnectedComponents:
        default:
          CalculateConnectedComponents(graph, false);
          break;
      }
    }

    private void CalculateConnectedComponents(IGraph graph, bool stronglyConnected) {

      ResultItemCollection<Component> components;
      if (stronglyConnected) {
        var result = new StronglyConnectedComponents().Run(graph);
        components = result.Components;
      } else {
        var result = new ConnectedComponents().Run(graph);
        components = result.Components;
      }

      if (components.Count > 0) {
        // generate a color array
        var colors = GenerateColors(false, components.Count);

        var visitedEdges = new HashSet<IEdge>();

        for (var i = 0; i < components.Count; i++) {
          var component = components[i];
          var color = colors[i];

          foreach (var edge in component.InducedEdges) {
            // each edge of the same cycle get the same tag
            visitedEdges.Add(edge);
            var tag = InitializeTag(edge);
            tag.CurrentColor = color;
          }
          component.Nodes.ForEach(node => {
            InitializeTag(node).CurrentColor = color;
          });
        }

        foreach (var edge in graph.Edges) {
          if (visitedEdges.Add(edge)) {
            InitializeTag(edge);
          }
        }
      }
    }
    private void CalculateBiconnectedComponents(IGraph graph) {
      var result = new BiconnectedComponents().Run(graph);

      if (result.Components.Count > 0) {
        // generate a color array
        var colors = GenerateColors(false);

        // sets the style/tag for the edges
        for (var i = 0; i < result.Components.Count; i++) {
          var component = result.Components[i];
          var color = colors[i];

          component.Edges.ForEach(edge => { InitializeTag(edge).CurrentColor = color; });
          component.Nodes.ForEach(node => { InitializeTag(node).CurrentColor = color; });
        }
      }
    }


    private Tag InitializeTag(IModelItem modelItem) {
      var tag = modelItem.Tag as Tag;
      if (tag == null) {
        tag = new Tag();
        modelItem.Tag = tag;
      }
      tag.Directed = Directed;
      tag.CurrentColor = null;
      return tag;
    }
  }

  public enum ConnectivityMode
  {
    ConnectedComponents, BiconnectedComponents, StronglyConnectedComponents
  }
}
