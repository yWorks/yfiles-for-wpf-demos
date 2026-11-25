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
using System.IO;
using System.Text.Json;
using yWorks.Graph;

namespace yWorks.Demo.MetabolicPathways
{
  /// <summary>
  ///   Represents the data-set for this demo for creating the nodes and the edges.
  /// </summary>
  internal sealed class MetabolicPathwayData
  {
    public List<InputNodeData> Nodes { get; set; } = [];

    public List<InputEdgeData> Edges { get; set; } = [];

    /// <summary>
    ///   Represents a node of a metabolic path in the input data-set.
    /// </summary>
    internal sealed class InputNodeData
    {
      public int Id { get; set; }

      public string? Labels { get; set; }

      public InputNodeTag Tag { get; set; } = new();
    }

    internal sealed class InputNodeTag
    {
      public string Type { get; set; } = "";

      public JsonElement VAlign { get; set; }

      public bool Circle { get; set; }
    }

    internal sealed class InputEdgeData
    {
      public int Source { get; set; }

      public int Target { get; set; }
    }

    /// <summary>
    ///   Represents the data associated with a node of a metabolic path after building the graph.
    /// </summary>
    internal sealed record NodeData(NodeType Type, string? AlignmentMarker, int? AlignmentOrder, bool Circle);

    internal enum NodeType
    {
      Enzyme,
      Reaction,
      Reactant,
      CoReactant,
      Product,
      Other
    }

    /// <summary>
    ///   Returns the data associated with the given node.
    /// </summary>
    internal static NodeData GetData(INode node) =>
        node.Tag as NodeData ?? throw new InvalidOperationException("The node has no metabolic pathway data.");

    /// <summary>
    ///   Returns the type of the given node.
    /// </summary>
    internal static NodeType GetType(INode node) => GetData(node).Type;

    /// <summary>
    ///   Returns the alignment of the given node.
    /// </summary>
    internal static int? GetAlignmentOrder(INode node) {
      var data = GetData(node);
      return data.AlignmentOrder ?? data.AlignmentMarker switch {
          "start" => int.MinValue,
          "end" => int.MaxValue,
          _ => null
      };
    }

    internal static bool IsEndAlignment(INode node) => GetData(node).AlignmentMarker == "end";

    /// <summary>
    ///   A mapping between the string values of the node types in the data-set and the NodeType enum.
    /// </summary>
    internal static NodeData CreateTag(InputNodeTag tag) {
      var type = tag.Type switch {
          "ENZYME" => NodeType.Enzyme,
          "REACTION" => NodeType.Reaction,
          "CO_REACTANT" => NodeType.CoReactant,
          "PRODUCT" => NodeType.Product,
          "REACTANT" => NodeType.Reactant,
          "OTHER" => NodeType.Other,
          _ => throw new InvalidDataException($"Unknown metabolic node type '{tag.Type}'.")
      };

      string? marker = null;
      int? order = null;
      if (tag.VAlign.ValueKind == JsonValueKind.String) {
        marker = tag.VAlign.GetString();
      } else if (tag.VAlign.ValueKind == JsonValueKind.Number && tag.VAlign.TryGetInt32(out var value)) {
        order = value;
      }
      return new NodeData(type, marker, order, tag.Circle);
    }
  }
}
