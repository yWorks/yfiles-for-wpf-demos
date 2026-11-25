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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.SplitEdges
{
  internal sealed class ContextMenuSupport
  {
    private readonly Func<Task> runLayout;
    private readonly GraphControl graphControl;

    private IGraph Graph => graphControl.Graph;

    public ContextMenuSupport(GraphControl graphControl, Func<Task> runLayout) {
      this.graphControl = graphControl;
      this.runLayout = runLayout;
    }

    /// <summary>
    ///   Adds menu items to the context menu depending on what type of graph element was hit.
    /// </summary>
    public void PopulateContextMenu(object? sender, PopulateItemContextMenuEventArgs<IModelItem> args) {
      if (args.Handled) {
        return;
      }

      var item = args.Item;
      UpdateSelection(item);
      var menuItems = new List<MenuItem>();
      if (item is IEdge) {
        var selectedEdges = graphControl.Selection.Edges.ToArray();
        if (selectedEdges.Length > 1 && selectedEdges.Any(edge1 => selectedEdges.Any(edge2 =>
            edge1 != edge2 &&
            ((Graph.IsGroupNode(edge1.GetSourceNode()) && edge1.GetSourceNode() == edge2.GetTargetNode()) ||
                (Graph.IsGroupNode(edge1.GetTargetNode()) && edge1.GetTargetNode() == edge2.GetSourceNode() &&
                    Graph.GetParent(edge1.GetTargetNode()) != Graph.GetParent(edge2.GetSourceNode())))))) {
          menuItems.Add(CreateMenuItem("Align Selected Edges", () => AlignSelectedEdges(selectedEdges)));
        }
        if (selectedEdges.Any(edge => HasSplitIds(GetTag(edge)))) {
          menuItems.Add(CreateMenuItem("Unalign Selected Edges", () => UnalignSelectedEdges(selectedEdges)));
          menuItems.Add(CreateMenuItem("Join Inter-Edges", () => JoinInterEdgesAtGroups(selectedEdges)));
        }
        if (selectedEdges.Any(edge => IsInterEdge(edge))) {
          menuItems.Add(CreateMenuItem("Split Inter-Edges", () => SplitInterEdgesAtGroups(selectedEdges)));
        }
      } else if (item is INode node && Graph.IsGroupNode(node)) {
        menuItems.Add(CreateMenuItem("Split Inter-Edges", () => SplitInterEdgesAtGroup(node)));
        menuItems.Add(CreateMenuItem("Join Inter-Edges", () => JoinInterEdgesAtGroup(node)));
      }

      args.Menu.Items.Clear();
      foreach (var menuItem in menuItems) {
        args.Menu.Items.Add(menuItem);
      }
      args.ShowMenu = menuItems.Count > 0;
    }

    /// <summary>
    ///   Adds the same split IDs to the given edges.
    /// </summary>
    private async Task AlignSelectedEdges(IEdge[] edges) {
      var splitId = Guid.NewGuid().ToString("N");
      var color = GetColor();
      foreach (var edge1 in edges) {
        if (edges.Any(edge2 => Graph.IsGroupNode(edge1.GetSourceNode()) &&
            edge1.GetSourceNode() == edge2.GetTargetNode() &&
            Graph.GetParent(edge1.GetTargetNode()) != Graph.GetParent(edge2.GetSourceNode()))) {
          var tag = GetOrCreateTag(edge1);
          tag.SourceSplitId = splitId;
          tag.Color = color;
          UpdateEdgeColor(edge1, color);
        }
        if (edges.Any(edge2 => Graph.IsGroupNode(edge1.GetTargetNode()) &&
            edge1.GetTargetNode() == edge2.GetSourceNode() &&
            Graph.GetParent(edge1.GetSourceNode()) != Graph.GetParent(edge2.GetTargetNode()))) {
          var tag = GetOrCreateTag(edge1);
          tag.TargetSplitId = splitId;
          tag.Color = color;
          UpdateEdgeColor(edge1, color);
        }
      }

      await runLayout();
    }

    /// <summary>
    ///   Updates the color of the edge.
    /// </summary>
    private void UpdateEdgeColor(IEdge edge, string color) {
      if (edge.Style is not PolylineEdgeStyle edgeStyle) {
        return;
      }
      var brush = CreateBrush(color);
      edgeStyle.Pen = new Pen(brush, 3);
      edgeStyle.TargetArrow = new Arrow(brush: brush, type: ArrowType.Triangle, widthScale: 1.5, lengthScale: 1.5);
    }

    /// <summary>
    ///   Removes the edges' split IDs.
    /// </summary>
    private async Task UnalignSelectedEdges(IEdge[] edges) {
      foreach (var edge in edges) {
        var sourceNode = edge.GetSourceNode();
        var targetNode = edge.GetTargetNode();
        var edgeTag = GetTag(edge);
        if (Graph.IsGroupNode(sourceNode)) {
          foreach (var inEdge in Graph.InEdgesAt(sourceNode).ToArray()) {
            var inTag = GetTag(inEdge);
            if (inTag?.TargetSplitId is { } incomingId && edgeTag?.SourceSplitId == incomingId && !Graph
                .InEdgesAt(inEdge.GetSourceNode()).Any(previousEdge => {
                  var previousTag = GetTag(previousEdge);
                  return previousTag?.TargetSplitId is { } targetId && targetId == previousTag.SourceSplitId;
                })) {
              inEdge.Tag = null;
              Graph.SetStyle(inEdge, Graph.EdgeDefaults.GetStyleInstance());
            }
          }
        }
        if (Graph.IsGroupNode(targetNode)) {
          foreach (var outEdge in Graph.OutEdgesAt(targetNode).ToArray()) {
            var outTag = GetTag(outEdge);
            if (outTag?.SourceSplitId is { } outgoingId && edgeTag?.TargetSplitId == outgoingId && !Graph
                .OutEdgesAt(outEdge.GetTargetNode()).Any(nextEdge => {
                  var nextTag = GetTag(nextEdge);
                  return nextTag?.SourceSplitId is { } sourceId && sourceId == nextTag.TargetSplitId;
                })) {
              outEdge.Tag = null;
              Graph.SetStyle(outEdge, Graph.EdgeDefaults.GetStyleInstance());
            }
          }
        }

        // Unalign the edge.
        edge.Tag = null;
        Graph.SetStyle(edge, Graph.EdgeDefaults.GetStyleInstance());
      }

      await runLayout();
    }

    /// <summary>
    ///   Joins all edges with the same split ID as the given edges.
    /// </summary>
    private async Task JoinInterEdgesAtGroups(IEdge[] edges) {
      var visited = new HashSet<IEdge>();
      foreach (var edge in edges) {
        if (visited.Contains(edge)) {
          continue;
        }
        var edgeTag = GetTag(edge);
        if (!HasSplitIds(edgeTag)) {
          continue;
        }

        visited.Add(edge);
        var sourceNode = edge.GetSourceNode();
        var predecessor = Graph.InEdgesAt(sourceNode).FirstOrDefault(inEdge =>
            GetTag(inEdge)?.TargetSplitId is { } id && id == edgeTag!.SourceSplitId);
        while (predecessor is not null) {
          visited.Add(predecessor);
          sourceNode = predecessor.GetSourceNode();
          predecessor = Graph.InEdgesAt(sourceNode).FirstOrDefault(inEdge =>
              GetTag(inEdge)?.TargetSplitId is { } id && id == edgeTag!.SourceSplitId);
        }

        var targetNode = edge.GetTargetNode();
        var successor = Graph.OutEdgesAt(targetNode).FirstOrDefault(outEdge =>
            GetTag(outEdge)?.SourceSplitId is { } id && id == edgeTag!.TargetSplitId);
        while (successor is not null) {
          visited.Add(successor);
          targetNode = successor.GetTargetNode();
          successor = Graph.OutEdgesAt(targetNode).FirstOrDefault(outEdge =>
              GetTag(outEdge)?.SourceSplitId is { } id && id == edgeTag!.TargetSplitId);
        }
        Graph.CreateEdge(sourceNode, targetNode);
      }
      foreach (var edge in visited) {
        Graph.Remove(edge);
      }

      await runLayout();
    }

    /// <summary>
    ///   Splits the inter-edges at all groups that they are crossing.
    /// </summary>
    private async Task SplitInterEdgesAtGroups(IEdge[] interEdges) {
      foreach (var edge in interEdges) {
        var sourceNode = edge.GetSourceNode();
        var targetNode = edge.GetTargetNode();
        var commonAncestor = Graph.GetGroupingSupport().GetNearestCommonAncestor(sourceNode, targetNode);

        var edgeTag = GetTag(edge);
        string splitId;
        string color;
        if (edgeTag is not null && (!(edgeTag.SourceSplitId is not null && edgeTag.TargetSplitId is not null) ||
            edgeTag.SourceSplitId == edgeTag.TargetSplitId)) {
          color = edgeTag.Color ?? GetColor();
          splitId = edgeTag.SourceSplitId ?? edgeTag.TargetSplitId ?? Guid.NewGuid().ToString("N");
        } else {
          color = GetColor();
          splitId = Guid.NewGuid().ToString("N");
        }

        var lastNode = sourceNode;
        var walkerGroup = Graph.GetParent(sourceNode);
        while (walkerGroup is not null && walkerGroup != commonAncestor && walkerGroup != targetNode) {
          var splitEdge = CreateSplitEdge(lastNode, walkerGroup, splitId, color);
          Graph.SetPortLocation(splitEdge.SourcePort,
              lastNode != sourceNode ? GetIntersection(edge, lastNode) : edge.SourcePort.GetLocation());
          Graph.SetPortLocation(splitEdge.TargetPort,
              walkerGroup != sourceNode ? GetIntersection(edge, walkerGroup) : edge.SourcePort.GetLocation());
          lastNode = walkerGroup;
          walkerGroup = Graph.GetParent(lastNode);
        }

        var lastSourceNode = lastNode;
        lastNode = targetNode;
        walkerGroup = Graph.GetParent(targetNode);
        while (walkerGroup is not null && walkerGroup != commonAncestor && walkerGroup != sourceNode) {
          var splitEdge = CreateSplitEdge(walkerGroup, lastNode, splitId, color);
          Graph.SetPortLocation(splitEdge.SourcePort,
              walkerGroup != targetNode ? GetIntersection(edge, walkerGroup) : edge.TargetPort.GetLocation());
          Graph.SetPortLocation(splitEdge.TargetPort,
              lastNode != targetNode ? GetIntersection(edge, lastNode) : edge.TargetPort.GetLocation());
          lastNode = walkerGroup;
          walkerGroup = Graph.GetParent(lastNode);
        }

        var crossingEdge = CreateSplitEdge(lastSourceNode, lastNode, splitId, color);
        Graph.SetPortLocation(crossingEdge.SourcePort,
            lastSourceNode != sourceNode ? GetIntersection(edge, lastSourceNode) : edge.SourcePort.GetLocation());
        Graph.SetPortLocation(crossingEdge.TargetPort,
            lastNode != targetNode ? GetIntersection(edge, lastNode) : edge.TargetPort.GetLocation());
        Graph.Remove(edge);
      }

      await runLayout();
    }

    /// <summary>
    ///   Splits all inter-edges at the given group.
    /// </summary>
    private async Task SplitInterEdgesAtGroup(INode group) {
      var descendants = Graph.GetGroupingSupport().GetDescendants(group).ToHashSet();
      var interEdges = descendants.SelectMany(node => Graph.EdgesAt(node)).Where(edge => {
        var sourceNode = edge.GetSourceNode();
        var targetNode = edge.GetTargetNode();
        return sourceNode != group && targetNode != group &&
            Graph.GetParent(sourceNode) != Graph.GetParent(targetNode) &&
            (!descendants.Contains(sourceNode) || !descendants.Contains(targetNode));
      }).Distinct().ToArray();

      foreach (var edge in interEdges) {
        var edgeTag = GetTag(edge);
        string splitId;
        string color;
        if (edgeTag is not null && (!(edgeTag.SourceSplitId is not null && edgeTag.TargetSplitId is not null) ||
            edgeTag.SourceSplitId == edgeTag.TargetSplitId)) {
          color = edgeTag.Color ?? GetColor();
          splitId = edgeTag.SourceSplitId ?? edgeTag.TargetSplitId ?? Guid.NewGuid().ToString("N");
        } else {
          color = GetColor();
          splitId = Guid.NewGuid().ToString("N");
        }

        var intersection = GetIntersection(edge, group);
        var sourceEdge = CreateSplitEdge(edge.GetSourceNode(), group, splitId, color);
        Graph.SetPortLocation(sourceEdge.TargetPort, intersection);
        var targetEdge = CreateSplitEdge(group, edge.GetTargetNode(), splitId, color);
        Graph.SetPortLocation(targetEdge.SourcePort, intersection);
        Graph.Remove(edge);
      }

      await runLayout();
    }

    /// <summary>
    ///   Finds the intersection point of the given edge and group. This is used for placing the edge ports for a smoother
    ///   animation when edges are split.
    /// </summary>
    private PointD GetIntersection(IEdge edge, INode group) {
      var groupingSupport = Graph.GetGroupingSupport();
      var sourceNode = edge.GetSourceNode();
      var inner = groupingSupport.IsDescendant(sourceNode, group)
          ? edge.SourcePort.GetLocation()
          : edge.TargetPort.GetLocation();
      var outer = groupingSupport.IsDescendant(sourceNode, group)
          ? edge.TargetPort.GetLocation()
          : edge.SourcePort.GetLocation();

      // Find the intersecting segment of the edge.
      var foundInner = false;
      var lastBend = inner;
      foreach (var bend in edge.Bends) {
        if (!foundInner) {
          if (!group.Layout.Contains(bend.Location)) {
            foundInner = true;
            outer = bend.Location.ToPointD();
          }
          inner = lastBend;
        }
        lastBend = bend.Location.ToPointD();
      }

      // Find the intersection point.
      return group.Layout.ToRectD().FindLineIntersection(inner, outer) ?? inner;
    }

    /// <summary>
    ///   Joins all edges with the same split IDs that connect to the given group from outside and inside the group.
    /// </summary>
    private async Task JoinInterEdgesAtGroup(INode group) {
      var sourceEdges = new Dictionary<string, IEdge>();
      foreach (var edge in Graph.InEdgesAt(group)) {
        if (GetTag(edge)?.TargetSplitId is { } splitId) {
          sourceEdges[splitId] = edge;
        }
      }
      foreach (var edge in Graph.OutEdgesAt(group).ToArray()) {
        if (GetTag(edge)?.SourceSplitId is { } splitId && sourceEdges.TryGetValue(splitId, out var sourceEdge)) {
          Graph.CreateEdge(sourceEdge.GetSourceNode(), edge.GetTargetNode());
          Graph.Remove(sourceEdge);
          Graph.Remove(edge);
        }
      }

      await runLayout();
    }

    /// <summary>
    ///   Creates an edge that is associated with a specific split ID and color.
    /// </summary>
    private IEdge CreateSplitEdge(INode source, INode target, string splitId, string color) {
      var brush = CreateBrush(color);
      return Graph.CreateEdge(source, target, new PolylineEdgeStyle {
          Pen = new Pen(brush, 3),
          TargetArrow = new Arrow(brush: brush, type: ArrowType.Triangle, widthScale: 1.5, lengthScale: 1.5),
          SmoothingLength = 15
      }, new SplitEdgesDemo.SplitEdgeTag {
          SourceSplitId = splitId,
          TargetSplitId = splitId,
          Color = color
      });
    }

    /// <summary>
    ///   Updates the selection when an item is right-clicked for a context menu.
    /// </summary>
    private void UpdateSelection(IModelItem? item) {
      var selection = graphControl.Selection;
      if (item is null) {
        selection.Clear();
      } else if (!selection.Contains(item)) {
        selection.Clear();
        selection.Add(item);
      } else {
        if (item is IEdge) {
          selection.Nodes.Clear();
        } else {
          selection.Edges.Clear();
        }
        selection.Add(item);
      }
    }

    private bool IsInterEdge(IEdge edge) {
      var sourceNode = edge.GetSourceNode();
      var targetNode = edge.GetTargetNode();
      return Graph.GetParent(sourceNode) != Graph.GetParent(targetNode) && Graph.GetParent(sourceNode) != targetNode &&
          Graph.GetParent(targetNode) != sourceNode;
    }

    private static bool HasSplitIds(SplitEdgesDemo.SplitEdgeTag? tag) =>
        tag?.SourceSplitId is not null || tag?.TargetSplitId is not null;

    private static SplitEdgesDemo.SplitEdgeTag? GetTag(IEdge edge) => edge.Tag as SplitEdgesDemo.SplitEdgeTag;

    private static SplitEdgesDemo.SplitEdgeTag GetOrCreateTag(IEdge edge) {
      if (edge.Tag is SplitEdgesDemo.SplitEdgeTag tag) {
        return tag;
      }
      tag = new SplitEdgesDemo.SplitEdgeTag();
      edge.Tag = tag;
      return tag;
    }

    private static MenuItem CreateMenuItem(string label, Func<Task> action) {
      var item = new MenuItem { Header = label };
      item.Click += async (_, _) => await action();
      return item;
    }

    private static SolidColorBrush CreateBrush(string color) => new((Color) ColorConverter.ConvertFromString(color)!);

    private static readonly string[] Colors = [
        "forestgreen",
        "mediumvioletred",
        "darkcyan",
        "chocolate",
        "limegreen",
        "mediumorchid",
        "royalblue",
        "orangered",
        "crimson",
        "darkturquoise",
        "cornflowerblue",
        "darkslateblue",
        "orange",
        "mediumslateblue"
    ];

    private static int colorIndex;

    /// <summary>
    ///   Returns the next color.
    /// </summary>
    private static string GetColor() {
      var index = colorIndex++ % Colors.Length;
      return Colors[index];
    }
  }
}
