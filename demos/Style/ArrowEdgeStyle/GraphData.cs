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

using System.Text.Json.Serialization;

namespace yWorks.Demo.ArrowEdgeStyle
{
  /// <summary>
  ///   The root object of the <c>graph-data.json</c> sample data.
  /// </summary>
  public sealed class GraphData
  {
    [JsonPropertyName("nodes")]
    public NodeData[] Nodes { get; set; }

    [JsonPropertyName("edges")]
    public EdgeData[] Edges { get; set; }
  }

  public sealed class NodeData
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("layout")]
    public NodeLayoutData Layout { get; set; }
  }

  public sealed class NodeLayoutData
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

  public sealed class EdgeData
  {
    [JsonPropertyName("source")]
    public int Source { get; set; }

    [JsonPropertyName("target")]
    public int Target { get; set; }

    [JsonPropertyName("style")]
    public EdgeStyleData Style { get; set; }
  }

  /// <summary>
  ///   The optional per-edge style settings found in the sample data. The <see cref="Shape"/> is stored in
  ///   kebab-case (e.g. <c>"double-arrow"</c>) and the <see cref="Color"/> refers to a named palette
  ///   from <see cref="Themes.ForName"/> (e.g. <c>"demo-green"</c>).
  /// </summary>
  public sealed class EdgeStyleData
  {
    [JsonPropertyName("shape")]
    public string Shape { get; set; }

    [JsonPropertyName("thickness")]
    public double Thickness { get; set; }

    [JsonPropertyName("angle")]
    public double Angle { get; set; }

    [JsonPropertyName("shaftRatio")]
    public double ShaftRatio { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }
  }
}
