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
using System.Text.Json.Serialization;

namespace yWorks.Demo.BezierEdgeStyle
{
  internal sealed class GraphData
  {
    [JsonPropertyName("nodes")]
    public List<NodeData> Nodes { get; set; } = [];

    [JsonPropertyName("edges")]
    public List<EdgeData> Edges { get; set; } = [];
  }

  internal sealed class NodeData
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("layout")]
    public LayoutData Layout { get; set; } = new();
  }

  internal sealed class LayoutData
  {
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }
  }

  internal sealed class EdgeData
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("source")]
    public int Source { get; set; }

    [JsonPropertyName("target")]
    public int Target { get; set; }

    [JsonPropertyName("sourcePort")]
    public PointData SourcePort { get; set; } = new();

    [JsonPropertyName("targetPort")]
    public PointData TargetPort { get; set; } = new();

    [JsonPropertyName("bends")]
    public List<PointData> Bends { get; set; } = [];

    [JsonPropertyName("labels")]
    public List<LabelData> Labels { get; set; } = [];
  }

  internal sealed class PointData
  {
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }
  }

  internal sealed class LabelData
  {
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("fromSource")]
    public bool FromSource { get; set; }

    [JsonPropertyName("segmentIndex")]
    public int SegmentIndex { get; set; }

    [JsonPropertyName("segmentRatio")]
    public double SegmentRatio { get; set; }

    [JsonPropertyName("ratio")]
    public double Ratio { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }
  }
}
