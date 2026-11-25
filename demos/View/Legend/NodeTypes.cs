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
using yWorks.Graph.Styles;

namespace yWorks.Demo.Legend;

internal enum NodeTypeId
{
  Employee,
  Organization,
  Department,
  Project,
  Contract,
  Location
}

internal readonly record struct NodeTypeInfo(string Name, string ColorSet, ShapeNodeShape? Shape);

/// <summary>
///   Stores name, color set, and shape for all node types.
/// </summary>
internal static class NodeTypes
{
  public const double LegendNodeWidth = 40;
  public const double LegendNodeHeight = 30;

  public static readonly IReadOnlyDictionary<NodeTypeId, NodeTypeInfo> Infos =
      new Dictionary<NodeTypeId, NodeTypeInfo> {
          [NodeTypeId.Employee] = new("Employee", "demo-blue", null),
          [NodeTypeId.Organization] = new("Organization", "demo-green", ShapeNodeShape.RoundRectangle),
          [NodeTypeId.Department] = new("Department", "demo-purple", ShapeNodeShape.Pill),
          [NodeTypeId.Project] = new("Project", "demo-orange", ShapeNodeShape.Diamond),
          [NodeTypeId.Contract] = new("Contract", "demo-red", ShapeNodeShape.Hexagon),
          [NodeTypeId.Location] = new("Location", "demo-lightblue", ShapeNodeShape.Ellipse)
      };

  /// <summary>
  ///   Returns a node style for the given node type.
  /// </summary>
  public static INodeStyle CreateStyle(NodeTypeId type) {
    var info = Infos[type];
    var palette = Themes.ForName(info.ColorSet);
    return info.Shape is { } shape
        ? DemoStyles.CreateDemoShapeNodeStyle(shape, palette)
        : DemoStyles.CreateDemoNodeStyle(palette);
  }

  public static bool TryParse(string? value, out NodeTypeId type) => Enum.TryParse(value, true, out type);
}
