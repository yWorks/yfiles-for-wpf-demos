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
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>The graph structure and data used by the Biofabrics demo.</summary>
  public sealed class GraphData
  {
    public List<GraphNodeData> Nodes { get; set; } = [];

    public List<GraphLinkData> Links { get; set; } = [];
  }

  public sealed class GraphNodeData
  {
    [JsonConverter(typeof(StringOrNumberConverter))]
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string Group { get; set; } = string.Empty;

    public int? Level { get; set; }
  }

  public sealed class GraphLinkData
  {
    [JsonConverter(typeof(StringOrNumberConverter))]
    public string Source { get; set; } = string.Empty;

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string Target { get; set; } = string.Empty;

    [JsonConverter(typeof(StringOrNumberConverter))]
    public string Group { get; set; } = string.Empty;

    public double? Weight { get; set; }

    public string? Label { get; set; }
  }

  public sealed class StringOrNumberConverter : JsonConverter<string>
  {
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number => reader.GetDecimal().ToString(CultureInfo.InvariantCulture),
            _ => throw new JsonException($"Expected a string or number, got {reader.TokenType}.")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
  }

  public sealed class DatasetDescription
  {
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DatasetDefaults Defaults { get; set; } = new();

    [JsonPropertyName("biofabric_parameters")]
    public BiofabricParameters BiofabricParameters { get; set; } = new();

    [JsonPropertyName("circular_parameters")]
    public CircularParameters CircularParameters { get; set; } = new();
  }

  public sealed class DatasetDefaults
  {
    public string EdgeColorModePicker { get; set; } = "NodeGroups";

    public string NodeWidthPicker { get; set; } = "Full";

    public string NodeSortingPicker { get; set; } = "Degree";

    public string EdgeSortingPicker { get; set; } = "EdgeLength";

    public bool EdgeGroupingPicker { get; set; } = true;

    public bool NodeGroupingPicker { get; set; } = true;

    public string EdgeGroupSortingPicker { get; set; } = "CardinalityDescending";

    public string NodeGroupSortingPicker { get; set; } = "CardinalityDescending";
  }

  public sealed class BiofabricParameters
  {
    public double EdgeSpacing { get; set; }
  }

  public sealed class CircularParameters
  {
    public double EdgeThickness { get; set; }
  }
}
