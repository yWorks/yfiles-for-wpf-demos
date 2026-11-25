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
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using yWorks.Controls;
using yWorks.Graph;
using static yWorks.Demo.PresentationSupport;

namespace yWorks.Demo
{
  /// <summary>
  ///   Base class for simple demos where a single <see cref="Controls.GraphControl"/> along with limited custom UI suffices.
  /// </summary>
  /// <remarks>
  ///   The <see cref="Description"/> will always be loaded from <c>description.html</c>.
  ///   A <see cref="ToolBar"/> with predefined buttons exists where other controls can be added.
  ///   A <see cref="SideBar"/> can be dynamically created by adding controls to it.
  ///   The <see cref="GraphControl"/> and its <see cref="Graph"/> can be obtained from the respective properties.
  /// </remarks>
  public abstract class SimpleDemo : Demo
  {
    /// <summary>
    ///   Gets the <see cref="Controls.GraphControl"/> for this demo.
    /// </summary>
    protected GraphControl GraphControl { get; }

    /// <summary>
    ///   Gets the graph displayed in this demo's <see cref="GraphControl"/>.
    /// </summary>
    protected IGraph Graph => GraphControl.Graph;

    /// <summary>
    ///   Gets the toolbar where more controls can be added.
    /// </summary>
    protected Menu ToolBar { get; }

    /// <summary>
    ///   Gets the sidebar where more controls can be added.
    /// </summary>
    protected Menu SideBar { get; }

    protected SimpleDemo() {
      DemoView demoView;
      GraphControl = new();
      var toolBar = new DemoToolBar {
          ButtonPresets = DemoToolBarButtonSet.Zoom,
          CommandTarget = GraphControl
      };
      DockPanel.SetDock(toolBar, Dock.Top);
      var contentBorder = new ClippingBorder {
          Child = new DockPanel {
              Children = {
                  toolBar,
                  GraphControl
              }
          }
      };
      contentBorder.SetResourceReference(StyleProperty, "DemoBorderStyle");

      Content = demoView = new DemoView {
          Children = {
              contentBorder,
              new Description { Source = "description.html" }
          }
      };
      ToolBar = new Menu(() => toolBar);
      SideBar = new Menu(() => {
        var factory = new FrameworkElementFactory(typeof(StackPanel));
        var template = new ItemsPanelTemplate(factory);
        var items = new ItemsControl { ItemsPanel = template };
        var sidebar = new SideBar {
            Header = "Interaction",
            Content = new ScrollViewer {
                Content = items,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            }
        };
        demoView.Children.Insert(1, sidebar);
        return items;
      });
      DemoStyles.InitializeSimpleDemoStyles(GraphControl.Graph);
    }

    /// <summary>
    ///   Creates a random graph.
    /// </summary>
    /// <param name="graph">The graph that will be modified. This defaults to the graph in this demo.</param>
    /// <param name="nodeCount">THe number of nodes in the graph. This defaults to 10.</param>
    /// <param name="edgeCount">
    ///   The number of edges in the graph. This defaults to <paramref name="nodeCount"/>&#xa0;&#x2212;
    ///   &#xa0;1 (tree structure) and is clamped between 0 (not connected) and <paramref name="nodeCount"/> × (
    ///   <paramref name="nodeCount"/>&#xa0;&#x2212;&#xa0;1) (strongly connected)
    /// </param>
    /// <param name="groupCount">The number of group nodes in the graph. This defaults to 0.</param>
    /// <param name="onNodeCreated">
    ///   A function that is called for each created node. The function receives the created node
    ///   and its index.
    /// </param>
    /// <param name="onEdgeCreated">
    ///   A function that is called for each created edge. The function receives the created edge
    ///   and its index.
    /// </param>
    protected void CreateRandomGraph(
        IGraph? graph = null,
        int nodeCount = 10,
        int edgeCount = -1,
        int groupCount = 0,
        Action<INode, int>? onNodeCreated = null,
        Action<IEdge, int>? onEdgeCreated = null) {
      graph ??= Graph;

      edgeCount = Math.Max(0, Math.Min(edgeCount is -1 ? nodeCount - 1 : edgeCount, nodeCount * (nodeCount - 1)));

      var random = Random.Shared;

      var rootNode = graph.CreateNode();

      // Create a random graph with a tree structure
      var nodes = new List<INode> { rootNode };
      var edges = new List<IEdge>();

      for (var i = 1; i < nodeCount; i++) {
        nodes.Add(graph.CreateNode());

        if (edgeCount > 0) {
          var randomIndex = random.Next(i);
          edges.Add(graph.CreateEdge(nodes[randomIndex], nodes[i]));
          edgeCount--;
        }
      }

      if (groupCount > 0) {
        var availableNodes = new List<INode>(nodes);
        for (var i = 0; i < groupCount; i++) {
          var group = graph.CreateGroupNode();
          var contentCount = Math.Max(1, (int) (random.NextDouble() * availableNodes.Count * 0.3));
          for (var j = 0; j < contentCount; j++) {
            var nodeIndex = random.Next(availableNodes.Count);
            var node = availableNodes[nodeIndex];
            availableNodes.RemoveAt(nodeIndex);
            graph.SetParent(node, group);
          }
          availableNodes.Add(group);
          nodes.Add(group);
        }
      }

      for (var i = 0; i < edgeCount; i++) {
        INode fromNode;
        INode toNode;
        do {
          fromNode = nodes[random.Next(nodeCount)];
          toNode = nodes[random.Next(nodeCount)];
        } while (fromNode == toNode || graph.GetEdge(fromNode, toNode) != null);

        edges.Add(graph.CreateEdge(fromNode, toNode));
      }

      if (onNodeCreated != null) {
        for (var i = 0; i < nodes.Count; i++) {
          onNodeCreated(nodes[i], i);
        }
      }

      if (onEdgeCreated != null) {
        for (var i = 0; i < edges.Count; i++) {
          onEdgeCreated(edges[i], i);
        }
      }
    }

    /// <summary>
    ///   Constructs and populates the <see cref="Graph"/> using the provided JSON resource name.
    /// </summary>
    /// <param name="resourceSuffix">
    ///   The name (suffix) of the resource, e.g. <c>graph-data.json</c>, to build the graph
    ///   structure.
    /// </param>
    protected void BuildGraphFromJson(
        string resourceSuffix,
        Type? nodeTagType = null,
        Type? groupNodeTagType = null,
        Type? edgeTagType = null,
        Type? nodeLabelTagType = null,
        Type? groupNodeLabelTagType = null,
        Type? edgeLabelTagType = null) {
      using var stream = LoadAssemblyResource(resourceSuffix);
      using var json = JsonDocument.Parse(stream);
      BuildGraphFromJson(resourceSuffix, Graph, nodeTagType, groupNodeTagType, edgeTagType, nodeLabelTagType,
          groupNodeLabelTagType, edgeLabelTagType);
    }

    /// <summary>
    ///   Tries to get a string property from a JSON element.
    /// </summary>
    /// <param name="tag">A <see cref="JsonElement"/> to retrieve the property from.</param>
    /// <param name="propertyName">The name of the string property.</param>
    /// <returns>The value of the string property or <c>null</c> if it doesn't exist.</returns>
    protected string? GetJsonStringProperty(object? tag, string propertyName) =>
        tag is JsonElement element && element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    /// <summary>
    ///   Tries to get an integer property from a JSON element.
    /// </summary>
    /// <param name="tag">A <see cref="JsonElement"/> to retrieve the property from.</param>
    /// <param name="propertyName">The name of the string property.</param>
    /// <param name="value">The out parameter to write the value of the property.</param>
    /// <returns>If the integer property exists and was assigned to the value parameter.</returns>
    protected bool TryGetJsonIntProperty(object? tag, string propertyName, out int value) {
      if (tag is JsonElement element && element.ValueKind == JsonValueKind.Object &&
          element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var intValue)) {
        value = intValue;
        return true;
      }

      value = 0;
      return false;
    }

    /// <summary>
    ///   Loads a graph from a GraphML resource.
    /// </summary>
    /// <param name="resourceSuffix">The name (suffix) of the resource, e.g. <c>sample-graph.graphml</c>, to load.</param>
    /// <param name="graph">The graph instance to be populated.</param>
    protected void BuildGraphFromGraphML(string resourceSuffix) {
      BuildGraphFromGraphML(resourceSuffix, Graph);
    }

    /// <summary>
    ///   Animates a highlight effect on the specified items within the <paramref name="graphControl"/> over a given duration.
    /// </summary>
    /// <param name="items">The items to highlight.</param>
    /// <param name="duration">The duration of the highlight animation. Defaults to 1 second.</param>
    /// <param name="graphControl">TGe GraphControl where the highlight animation will be displayed.</param>
    /// <returns>A task that completes when the highlight animation is completed.</returns>
    protected Task Highlight(
        IEnumerable<IModelItem> items,
        TimeSpan? duration = null,
        GraphControl? graphControl = null) =>
        HighlightItems(graphControl ?? GraphControl, items, duration ?? TimeSpan.FromSeconds(1));
  }
}
