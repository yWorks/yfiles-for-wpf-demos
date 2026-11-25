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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using yWorks.Layout.Router;

namespace yWorks.Demo.MazeRouting
{
  /// <summary>
  ///   Holds the settings available in the maze routing demo.
  /// </summary>
  public sealed class Settings : INotifyPropertyChanged
  {
    private RoutingScope scope = RoutingScope.RouteAllEdges;
    private RoutingStyle selectedRoutingStyle = RoutingStyle.Orthogonal;
    private PortSideSelection portSides = PortSideSelection.Any;
    private int edgeDistance = 6;
    private bool isRouting;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<Option<RoutingScope>> ScopeOptions { get; } = [
        new(RoutingScope.RouteAllEdges, "Route All Edges"),
        new(RoutingScope.RouteSelectedEdges, "Route Selected Edges"),
        new(RoutingScope.RouteEdgesAtSelectedNodes, "Route Edges At Selected Nodes")
    ];

    public IReadOnlyList<Option<RoutingStyle>> RoutingStyleOptions { get; } = [
        new(RoutingStyle.Orthogonal, "Orthogonal"),
        new(RoutingStyle.Octilinear, "Octilinear")
    ];

    public IReadOnlyList<Option<PortSideSelection>> PortSideOptions { get; } = [
        new(PortSideSelection.Any, "Any"),
        new(PortSideSelection.LeftRight, "Left or Right"),
        new(PortSideSelection.TopBottom, "Top or Bottom")
    ];

    /// <summary>
    ///   The edges to include in the routing operation.
    /// </summary>
    public RoutingScope Scope {
      get => scope;
      set => SetField(ref scope, value);
    }

    /// <summary>
    ///   The routing style to use.
    /// </summary>
    public RoutingStyle SelectedRoutingStyle {
      get => selectedRoutingStyle;
      set => SetField(ref selectedRoutingStyle, value);
    }

    /// <summary>
    ///   The allowed sides for the edge ports.
    /// </summary>
    public PortSideSelection PortSides {
      get => portSides;
      set => SetField(ref portSides, value);
    }

    /// <summary>
    ///   The minimum distance between a pair of edges.
    /// </summary>
    public int EdgeDistance {
      get => edgeDistance;
      set => SetField(ref edgeDistance, Math.Clamp(value, 1, 50));
    }

    /// <summary>
    ///   Whether a layout is currently running.
    /// </summary>
    public bool IsRouting {
      get => isRouting;
      set {
        if (SetField(ref isRouting, value)) {
          OnPropertyChanged(nameof(CanRouteEdges));
        }
      }
    }

    /// <summary>
    ///   Whether the route edges button is available.
    /// </summary>
    public bool CanRouteEdges => !IsRouting;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return false;
      }
      field = value;
      OnPropertyChanged(propertyName);
      return true;
    }

    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public enum RoutingScope
  {
    RouteAllEdges,
    RouteSelectedEdges,
    RouteEdgesAtSelectedNodes
  }

  public enum PortSideSelection
  {
    Any,
    LeftRight,
    TopBottom
  }

  public sealed record Option<T>(T Value, string Name);
}
