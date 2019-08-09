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

using System;
using System.Windows.Media;
using yWorks.Analysis;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Algorithms.GraphAnalysis
{
  /// <summary>
  /// Runs different centrality algorithms.
  /// </summary>
  public class CentralityConfig : AlgorithmConfiguration
  {
    private readonly CentralityMode algorithmType;

    /// <summary>
    /// Creates a new instance for the given <see cref="CentralityMode"/>.
    /// </summary>
    public CentralityConfig(CentralityMode algorithmType) {
      this.algorithmType = algorithmType;
    }

    /// <summary>
    /// Some algorithms support both directed and undirected graphs.
    /// </summary>
    public override bool SupportsDirectedAndUndirected {
      get {
        return algorithmType == CentralityMode.Graph ||
               algorithmType == CentralityMode.NodeEdgeBetweenness ||
               algorithmType == CentralityMode.Closeness;
      }
    }

    /// <summary>
    /// All algorithms except <see cref="CentralityMode.Degree"/> support edge weights.
    /// </summary>
    public override bool SupportsWeights {
      get { return algorithmType != CentralityMode.Degree; }
    }

    /// <summary>
    /// Runs the algorithm.
    /// </summary>
    public override void RunAlgorithm(IGraph graph) {
      ResetGraph(graph);

      double maximumNodeCentrality;
      double minimumNodeCentrality;
      ResultItemMapping<INode, double> normalizedNodeCentrality;

      switch (algorithmType) {
        case CentralityMode.Weight: {
          var result = new WeightCentrality {Weights = {Delegate = GetEdgeWeight}, ConsiderOutgoingEdges = true, ConsiderIncomingEdges = true}.Run(graph);
          maximumNodeCentrality = result.MaximumNodeCentrality;
          minimumNodeCentrality = result.MinimumNodeCentrality;
          normalizedNodeCentrality = result.NormalizedNodeCentrality;
          break;
        }
        case CentralityMode.Graph: {
          var result = new GraphCentrality {Weights = {Delegate = GetEdgeWeight}, Directed = Directed}.Run(graph);
          maximumNodeCentrality = result.MaximumNodeCentrality;
          minimumNodeCentrality = result.MinimumNodeCentrality;
          normalizedNodeCentrality = result.NormalizedNodeCentrality;
          break;
        }
        case CentralityMode.NodeEdgeBetweenness: {
          var result = new BetweennessCentrality {Directed = Directed, Weights = {Delegate = GetEdgeWeight}}.Run(graph);
          maximumNodeCentrality = result.MaximumNodeCentrality;
          minimumNodeCentrality = result.MinimumNodeCentrality;
          normalizedNodeCentrality = result.NormalizedNodeCentrality;
          result.NormalizedEdgeCentrality.ForEach((edge, centralityId) => {
            edge.Tag = Math.Round(centralityId * 100.0) / 100;
            graph.AddLabel(edge, edge.Tag.ToString(), tag : "Centrality");
          });
          break;
        }
        case CentralityMode.Closeness: {
          var result = new ClosenessCentrality { Directed = Directed, Weights = {Delegate = GetEdgeWeight}}.Run(graph);
          maximumNodeCentrality = result.MaximumNodeCentrality;
          minimumNodeCentrality = result.MinimumNodeCentrality;
          normalizedNodeCentrality = result.NormalizedNodeCentrality;
          break;
        }
        case CentralityMode.Degree:
        default: {
          var result = new DegreeCentrality { ConsiderOutgoingEdges = true, ConsiderIncomingEdges = true}.Run(graph);
          maximumNodeCentrality = result.MaximumNodeCentrality;
          minimumNodeCentrality = result.MinimumNodeCentrality;
          normalizedNodeCentrality = result.NormalizedNodeCentrality;
          break;
        }
      }
      
      var min = minimumNodeCentrality / maximumNodeCentrality;
      var diff = 1 - min;

      var mostCentralValue = 100;
      var leastCentralValue = 30;
      var colorNumber = 50;

      normalizedNodeCentrality.ForEach((node, centralityId) => {
        var textLabelStyle = new DefaultLabelStyle { TextBrush = Brushes.White };
        node.Tag = Math.Round(centralityId * 100.0) / 100;

        if (double.IsNaN((double) node.Tag) || double.IsNaN(diff)) {
          node.Tag = "Inf";
        }
        graph.AddLabel(node, node.Tag.ToString(), style : textLabelStyle, tag : "Centrality");

        if (diff == 0 || double.IsNaN(diff)) {
          graph.SetStyle(node, GetMarkedNodeStyle(0, colorNumber));
          graph.SetNodeLayout(node, new RectD(node.Layout.X, node.Layout.Y, leastCentralValue, leastCentralValue));
        } else {
          // adjust gradient color
          var colorScale = (colorNumber - 1) / diff;
          var index = Math.Max(0, Math.Min(colorNumber, (int) Math.Floor((centralityId - min) * colorScale)));
          graph.SetStyle(node, GetMarkedNodeStyle(index, colorNumber));
          // adjust size
          var sizeScale = (mostCentralValue - leastCentralValue) / diff;
          var size = leastCentralValue + (centralityId - min) * sizeScale;
          graph.SetNodeLayout(node, new RectD(node.Layout.X, node.Layout.Y, size, size));
        }
      });
    }

    /// <summary>
    /// Creates a node style with a color according to the given value.
    /// </summary>
    /// <remarks>
    /// The color is a blueish color which is darker for higher <paramref name="value"/>s.
    /// </remarks>
    /// <param name="value">The value to get the color for.</param>
    /// <param name="maxValue">The maximum value. The closer value is to maxValue the darker is the resulting node style.</param>
    /// <returns></returns>
    private INodeStyle GetMarkedNodeStyle(int value, int maxValue) {
      var colors = GenerateColors(true, maxValue, true);
      return new ShapeNodeStyle { Brush = new SolidColorBrush(colors[value % colors.Length]) };
    }
    
  }

  /// <summary>
  /// Defines how the centrality values should be calculated.
  /// </summary>
  public enum CentralityMode
  {
    /// <summary>
    /// Computes the centrality based on the number of incoming and outgoing edges using <see cref="DegreeCentrality"/>.
    /// </summary>
    Degree,
    /// <summary>
    /// Computes the centrality based on the weight of the incoming and outgoing edges using <see cref="WeightCentrality"/>.
    /// </summary>
    Weight,
    /// <summary>
    /// Computes the (shortest path) distances of a node to all other nodes in the graph using <see cref="GraphCentrality"/>.
    /// </summary>
    /// <remarks>
    /// Graph centrality is defined as the reciprocal of the maximum of all shortest path distances from a node to all other nodes in the graph.
    /// Nodes with high graph centrality have short distances to all other nodes in the graph.
    /// Similar to <see cref="Closeness"/>.
    /// </remarks>
    Graph,
    /// <summary>
    /// Computes how often a node/edge lies between all other node pairs in the graph using <see cref="BetweennessCentrality"/>.
    /// </summary>
    /// <remarks>
    /// The centrality can be computed for directed and undirected graphs. This algorithm also supports edge weights.
    /// </remarks>
    NodeEdgeBetweenness,
    /// <summary> 
    /// Computes the (shortest path) distances of a node to all other nodes in the graph using <see cref="ClosenessCentrality"/>.
    /// </summary>
    /// <remarks>
    /// Similar to <see cref="Graph"/>
    /// </remarks>
    Closeness
  }
}
