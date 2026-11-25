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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo
{
  public static class BuildGraphHelper
  {
    public static void BuildGraph(
        IGraph graph,
        JsonDocument graphData,
        Type? nodeTagType = null,
        Type? groupNodeTagType = null,
        Type? edgeTagType = null,
        Type? nodeLabelTagType = null,
        Type? groupNodeLabelTagType = null,
        Type? edgeLabelTagType = null) {
      BuildGraph(graph, graphData.RootElement, nodeTagType, groupNodeTagType, edgeTagType, nodeLabelTagType,
          groupNodeLabelTagType, edgeLabelTagType);
    }

    public static void BuildGraph(
        IGraph graph,
        JsonElement data,
        Type? nodeTagType = null,
        Type? groupNodeTagType = null,
        Type? edgeTagType = null,
        Type? nodeLabelTagType = null,
        Type? groupNodeLabelTagType = null,
        Type? edgeLabelTagType = null) {
      graph.Clear();

      var graphBuilder = new GraphBuilder(graph);

      var nodeSource = graphBuilder.CreateNodesSource(
          GetNodes(data, false), item => GetId(item, $"Node {item} has no ID."));
      nodeSource.ParentIdProvider = item => GetParent(item);
      nodeSource.NodeCreator.TagProvider = item => GetTag(item, nodeTagType);
      nodeSource.NodeCreator.LayoutProvider = item => GetNodeLayout(graph, item);
      nodeSource.NodeCreator.StyleProvider = item =>
          TryGetPalette(item, out var palette)
              ? graph.NodeDefaults.Style is ShapeNodeStyle sns
                  ? DemoStyles.CreateDemoShapeNodeStyle(sns.Shape, palette)
                  : DemoStyles.CreateDemoNodeStyle(palette)
              : graph.NodeDefaults.GetStyleInstance();

      var nodeLabelsSource = nodeSource.NodeCreator.CreateLabelsSource(GetLabels);
      nodeLabelsSource.LabelCreator.TextProvider = GetLabelText;
      nodeLabelsSource.LabelCreator.TagProvider = item => item; // Post-processing below
      nodeLabelsSource.LabelCreator.StyleProvider = item =>
          TryGetPalette(item, out var palette)
              ? DemoStyles.CreateDemoNodeLabelStyle(palette)
              : graph.NodeDefaults.Labels.Style;

      var groupNodeSource = graphBuilder.CreateGroupNodesSource(
          GetNodes(data, true), item => GetId(item, $"Group node {item} has no ID."));
      groupNodeSource.ParentIdProvider = item => GetParent(item);
      groupNodeSource.NodeCreator.TagProvider = item => GetTag(item, groupNodeTagType ?? nodeTagType);
      groupNodeSource.NodeCreator.LayoutProvider = item => GetNodeLayout(graph, item);
      groupNodeSource.NodeCreator.StyleProvider = item =>
          TryGetPalette(item, out var palette)
              ? DemoStyles.CreateDemoGroupStyle(palette)
              : graph.GroupNodeDefaults.GetStyleInstance();

      var groupNodeLabelsSource = groupNodeSource.NodeCreator.CreateLabelsSource(GetLabels);
      groupNodeLabelsSource.LabelCreator.TextProvider = GetLabelText;
      groupNodeLabelsSource.LabelCreator.TagProvider = item => item; // Post-processing below
      groupNodeLabelsSource.LabelCreator.StyleProvider = item =>
          TryGetPalette(item, out var palette)
              ? DemoStyles.CreateDemoGroupLabelStyle(palette)
              : graph.GroupNodeDefaults.Labels.Style;

      var edgeSource = graphBuilder.CreateEdgesSource(
          data.TryGetProperty("edges", out var edges) ? edges.EnumerateArray().AsEnumerable() : [],
          item => GetId(item, $"Edge {item} has no source ID.", "source"),
          item => GetId(item, $"Edge {item} has no target ID.", "target"));
      edgeSource.EdgeCreator.BendsProvider = item => item.TryGetProperty("bends", out var bends) ? GetBends(bends) : [];
      edgeSource.EdgeCreator.TagProvider = item => item; // Post-processing below
      edgeSource.EdgeCreator.StyleProvider = item =>
          TryGetPalette(item, out var palette)
              ? DemoStyles.CreateDemoHighlightEdgeStyle(palette)
              : graph.EdgeDefaults.GetStyleInstance();

      var edgeLabelsSource = edgeSource.EdgeCreator.CreateLabelsSource(GetLabels);
      edgeLabelsSource.LabelCreator.TextProvider = GetLabelText;
      edgeLabelsSource.LabelCreator.TagProvider = item => item; // Post-processing below
      edgeLabelsSource.LabelCreator.StyleProvider = item =>
          TryGetPalette(item, out var palette)
              ? DemoStyles.CreateDemoEdgeLabelStyle(palette)
              : graph.EdgeDefaults.Labels.Style;

      graphBuilder.BuildGraph();

      // set the port locations and then update the tag with the actual tag
      foreach (var edge in graph.Edges) {
        if (edge.Tag is JsonElement el) {
          if (el.TryGetProperty("sourcePort", out var sourcePort)) {
            graph.SetPortLocation(edge.SourcePort, GetPoint(sourcePort));
          }
          if (el.TryGetProperty("targetPort", out var targetPort)) {
            graph.SetPortLocation(edge.TargetPort, GetPoint(targetPort));
          }
          edge.Tag = GetTag(el, edgeTagType);
        }
      }

      // set the label positions
      foreach (var label in graph.Labels) {
        if (label.Tag is JsonElement el) {
          if (TryGetOrientedRectangle(el, out var rect)) {
            var layoutParameter = label.Owner is INode
                ? FreeNodeLabelModel.Instance.FindBestParameter(label, rect)
                : FreeEdgeLabelModel.Instance.FindBestParameter(label, rect);
            graph.SetLabelLayoutParameter(label, layoutParameter);
          }
          label.Tag = GetTag(el,
              label.Owner is INode node
                  ? graph.IsGroupNode(node) ? groupNodeLabelTagType ?? nodeLabelTagType : nodeLabelTagType
                  : edgeLabelTagType);
        }
      }
    }

    private static RectD GetNodeLayout(IGraph graph, JsonElement item) {
      if (item.TryGetProperty("layout", out var layoutElement)) {
        return GetNodeLayout(layoutElement, graph);
      }
      return GetNodeLayout(item, graph);
    }

    private static object? GetParent(JsonElement item) {
      if (!item.TryGetProperty("parent", out var parentElement)) {
        return null;
      }

      return parentElement.ValueKind switch {
          JsonValueKind.Number => parentElement.GetInt32(),
          JsonValueKind.String => parentElement.GetString(),
          _ => null
      };
    }

    private static object GetId(JsonElement item, string message, string idName = "id") {
      if (!item.TryGetProperty(idName, out var idElement)) {
        throw new ArgumentException(message);
      }
      return idElement.ValueKind switch {
          JsonValueKind.Number => idElement.GetInt32(),
          JsonValueKind.String => idElement.GetString(),
          _ => throw new ArgumentException(message)
      };
    }

    private record NodeTag(int Type);

    private static object? GetTag(JsonElement item, Type? type) {
      if (type == null) {
        if (item.ValueKind is JsonValueKind.Object && item.TryGetProperty("tag", out var tagValue)) {
          return TryGetValue(tagValue);
        }
        return null;
      }
      return item.Deserialize(type, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private static bool TryGetPalette(JsonElement item, out Palette palette) {
      if (item.ValueKind is JsonValueKind.Object && item.TryGetProperty("cssClass", out var cssElement) &&
          cssElement.ValueKind is JsonValueKind.String) {
        palette = Themes.ForName(cssElement.GetString());
        return palette != null;
      }

      palette = null!;
      return false;
    }

    private static bool TryGetOrientedRectangle(JsonElement element, [NotNullWhen(true)] out OrientedRectangle? rect) {
      if (element.ValueKind is JsonValueKind.Object && element.TryGetProperty("anchorX", out var anchorX) &&
          element.TryGetProperty("anchorY", out var anchorY) && element.TryGetProperty("width", out var width) &&
          element.TryGetProperty("height", out var height) && element.TryGetProperty("upX", out var upX) &&
          element.TryGetProperty("upY", out var upY)) {
        rect = new OrientedRectangle(anchorX.GetDouble(), anchorY.GetDouble(), width.GetDouble(), height.GetDouble(),
            upX.GetDouble(), upY.GetDouble());
        return true;
      }
      rect = null;
      return false;
    }

    private static IEnumerable<JsonElement> GetLabels(JsonElement item) {
      if (item.TryGetProperty("labels", out var labels)) {
        if (labels.ValueKind is JsonValueKind.String) {
          return [labels];
        }
        return labels.EnumerateArray();
      }
      return [];
    }

    private static string? GetLabelText(JsonElement item) {
      return item switch {
          { ValueKind: JsonValueKind.String } el => el.GetString(),
          { ValueKind: JsonValueKind.Object } el when el.TryGetProperty("text", out var text) &&
              text.ValueKind is JsonValueKind.String => text.GetString(),
          _ => null
      };
    }

    private static IEnumerable<JsonElement> GetNodes(JsonElement data, bool isGroup) {
      foreach (var item in data.GetProperty("nodes").EnumerateArray()) {
        if (item.TryGetProperty("isGroup", out var itemIsgroup)) {
          if (isGroup == itemIsgroup.GetBoolean()) {
            yield return item;
          }
        } else if (!isGroup) {
          yield return item;
        }
      }
    }

    private static PointD GetPoint(JsonElement pointLike) {
      switch (pointLike.ValueKind) {
        case JsonValueKind.Array:
          return new PointD(pointLike[0].GetDouble(), pointLike[1].GetDouble());
        case JsonValueKind.Object:
          return new PointD(pointLike.GetProperty("x").GetDouble(), pointLike.GetProperty("y").GetDouble());
      }
      return default;
    }

    private static IEnumerable<PointD> GetBends(JsonElement bends) {
      foreach (var bend in bends.EnumerateArray()) {
        yield return GetPoint(bend);
      }
    }

    private static RectD GetNodeLayout(JsonElement layoutElement, IGraph graph) {
      switch (layoutElement.ValueKind) {
        case JsonValueKind.Array:
          return new RectD(layoutElement[0].GetDouble(), layoutElement[1].GetDouble(), layoutElement[2].GetDouble(),
              layoutElement[3].GetDouble());
        case JsonValueKind.Object:
          var x = layoutElement.TryGetProperty("x", out var xElement) ? xElement.GetDouble() : 0;
          var y = layoutElement.TryGetProperty("y", out var yElement) ? yElement.GetDouble() : 0;
          var width = layoutElement.TryGetProperty("width", out var widthElement)
              ? widthElement.GetDouble()
              : graph.NodeDefaults.Size.Width;
          var height = layoutElement.TryGetProperty("height", out var heightElement)
              ? heightElement.GetDouble()
              : graph.NodeDefaults.Size.Height;
          return new RectD(x, y, width, height);
      }
      return new RectD(PointD.Origin, graph.NodeDefaults.Size);
    }

    private static object? TryGetValue(JsonElement tagElement) =>
        tagElement.ValueKind switch {
            JsonValueKind.Undefined or JsonValueKind.Null => null,
            JsonValueKind.Object => tagElement,
            JsonValueKind.Array => tagElement.EnumerateArray().ToArray(),
            JsonValueKind.String => tagElement.GetString(),
            JsonValueKind.Number => tagElement.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new ArgumentException("Unknown JsonValueKind.")
        };
  }
}
