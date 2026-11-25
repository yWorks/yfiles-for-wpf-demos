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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.BuildingGraphFromData
{
  [Demo("building-graph-from-data", "application-features", "Building Graphs From Data",
      Summary = "Shows how to build a graph from data in JSON format.",
      Keywords = ["read", "write", "input", "files", "data base", "playground"], Tags = ["json", "i/o"])]
  public sealed class BuildingGraphFromDataDemo : SimpleDemo
  {
    protected override void Initialize() {
      InitializeDefaultStyles(Graph);

      using var stream = LoadAssemblyResource("graph-data.json");
      using var graphData = JsonDocument.Parse(stream);
      BuildGraph(Graph, graphData.RootElement);

      // Fit the graph bounds
      GraphControl.FitGraphBounds();

      // Enable graph editing
      GraphControl.InputMode = new GraphEditorInputMode();

      // Only enable the undo engine after graph creation to prevent undoing of the graph creation
      Graph.SetUndoEngineEnabled(true);
    }

    private static void BuildGraph(IGraph graph, JsonElement graphData) {
      // Store groups and nodes in order to assign them as parents or connect them with edges afterward
      var groups = new Dictionary<string, INode>();
      var nodes = new Dictionary<string, INode>();

      // Iterate the group data and create the corresponding group nodes
      foreach (var groupData in graphData.GetProperty("groupsSource").EnumerateArray()) {
        var group = graph.CreateGroupNode(layout: GetLayout(groupData, graph.GroupNodeDefaults.Size),
            tag: groupData.Clone());
        AddLabel(graph, group, groupData);
        groups.Add(groupData.GetProperty("id").GetString()!, group);
      }

      // Iterate the node data and create the corresponding nodes
      foreach (var nodeData in graphData.GetProperty("nodesSource").EnumerateArray()) {
        var node = graph.CreateNode(GetLayout(nodeData, graph.NodeDefaults.Size), tag: nodeData.Clone());
        AddLabel(graph, node, nodeData);
        if (nodeData.TryGetProperty("fill", out var fill) && fill.ValueKind == JsonValueKind.String) {
          // If the node data specifies an individual fill color, adjust the style
          var nodeStyle = (RectangleNodeStyle) graph.NodeDefaults.Style.Clone();
          nodeStyle.Brush = (Brush) new BrushConverter().ConvertFromInvariantString(fill.GetString()!)!;
          graph.SetStyle(node, nodeStyle);
        }
        nodes.Add(nodeData.GetProperty("id").GetString()!, node);
      }

      // Set the parent groups after all nodes/groups are created
      foreach (var node in graph.Nodes) {
        if (node.Tag is JsonElement data && data.TryGetProperty("group", out var group) &&
            group.ValueKind == JsonValueKind.String) {
          graph.SetParent(node, groups[group.GetString()!]);
        }
      }

      // Iterate the edge data and create the corresponding edges
      foreach (var edgeData in graphData.GetProperty("edgesSource").EnumerateArray()) {
        // Note that nodes and groups need to have disjoint sets of ids, otherwise it is impossible to determine
        // which node is the correct source/target.
        var sourceId = edgeData.GetProperty("from").GetString()!;
        var targetId = edgeData.GetProperty("to").GetString()!;
        var edge = graph.CreateEdge(nodes.TryGetValue(sourceId, out var source) ? source : groups[sourceId],
            nodes.TryGetValue(targetId, out var target) ? target : groups[targetId], tag: edgeData.Clone());
        AddLabel(graph, edge, edgeData);
      }

      // If given, apply the edge layout information
      foreach (var edge in graph.Edges) {
        var edgeData = (JsonElement) edge.Tag!;
        if (edgeData.TryGetProperty("sourcePort", out var sourcePort) && sourcePort.ValueKind == JsonValueKind.Object) {
          graph.SetPortLocation(edge.SourcePort, GetPoint(sourcePort));
        }
        if (edgeData.TryGetProperty("targetPort", out var targetPort) && targetPort.ValueKind == JsonValueKind.Object) {
          graph.SetPortLocation(edge.TargetPort, GetPoint(targetPort));
        }
        if (edgeData.TryGetProperty("bends", out var bends) && bends.ValueKind == JsonValueKind.Array) {
          foreach (var bendLocation in bends.EnumerateArray()) {
            graph.AddBend(edge, GetPoint(bendLocation));
          }
        }
      }
    }

    private static RectD GetLayout(JsonElement data, SizeD defaultSize) {
      if (!data.TryGetProperty("layout", out var layout) || layout.ValueKind != JsonValueKind.Object) {
        return new RectD(PointD.Origin, defaultSize);
      }
      return new RectD(layout.GetProperty("x").GetDouble(), layout.GetProperty("y").GetDouble(),
          layout.GetProperty("width").GetDouble(), layout.GetProperty("height").GetDouble());
    }

    private static PointD GetPoint(JsonElement point) =>
        new(point.GetProperty("x").GetDouble(), point.GetProperty("y").GetDouble());

    private static void AddLabel(IGraph graph, ILabelOwner owner, JsonElement data) {
      if (data.TryGetProperty("label", out var label) && label.ValueKind != JsonValueKind.Null) {
        graph.AddLabel(owner, label.GetString()!);
      }
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeDefaultStyles(IGraph graph) {
      // Set the style, label and label parameter for group nodes
      graph.GroupNodeDefaults.Style = new GroupNodeStyle {
          TabBrush = new SolidColorBrush(Color.FromRgb(0x61, 0xa0, 0x44)),
          TabPosition = GroupNodeStyleTabPosition.LeftTrailing,
          DrawShadow = true,
          TabWidth = 70
      };
      graph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          TextAlignment = TextAlignment.Left,
          TextBrush = new SolidColorBrush(Color.FromRgb(0xee, 0xee, 0xee))
      };
      graph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabParameter();

      graph.NodeDefaults.Style = new RectangleNodeStyle {
          Brush = new SolidColorBrush(Color.FromRgb(0x64, 0xa8, 0xbe)),
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0xbd, 0xda, 0xe3)), 1.5),
          CornerStyle = CornerStyle.Round,
          CornerSize = 3.5
      };
    }

    [STAThread]
    private static void Main() {
      Run<BuildingGraphFromDataDemo>();
    }
  }
}
