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
using yWorks.Layout.Partial;

namespace yWorks.Demo.PartialLayout
{
  internal enum SubgraphLayoutKind
  {
    Hierarchical,
    Orthogonal,
    Organic,
    Circular
  }

  internal sealed record EnumChoice<T>(T Value, string Label);

  /// <summary>
  ///   Settings backing the partial layout interaction panel.
  /// </summary>
  internal sealed class Settings : INotifyPropertyChanged
  {
    private ComponentAssignmentStrategy componentAssignment = ComponentAssignmentStrategy.Single;
    private SubgraphLayoutKind subgraphLayout = SubgraphLayoutKind.Hierarchical;
    private SubgraphPlacement subgraphPlacement = SubgraphPlacement.Barycenter;
    private RoutingStyle edgeRoutingStyle = RoutingStyle.Automatic;
    private PartialLayoutOrientation layoutOrientation = PartialLayoutOrientation.None;
    private double minimumNodeDistance = 20;
    private bool allowMirroring;
    private bool nodeSnapping;
    private bool isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<EnumChoice<ComponentAssignmentStrategy>> ComponentAssignments { get; } = [
        new(ComponentAssignmentStrategy.Single, "Single"),
        new(ComponentAssignmentStrategy.Connected, "Connected")
    ];

    public IReadOnlyList<EnumChoice<SubgraphLayoutKind>> SubgraphLayouts { get; } = [
        new(SubgraphLayoutKind.Hierarchical, "Hierarchical"),
        new(SubgraphLayoutKind.Orthogonal, "Orthogonal"),
        new(SubgraphLayoutKind.Organic, "Organic"),
        new(SubgraphLayoutKind.Circular, "Circular")
    ];

    public IReadOnlyList<EnumChoice<SubgraphPlacement>> SubgraphPlacements { get; } = [
        new(SubgraphPlacement.Barycenter, "Barycenter"),
        new(SubgraphPlacement.FromSketch, "From Sketch")
    ];

    public IReadOnlyList<EnumChoice<RoutingStyle>> EdgeRoutingStyles { get; } = [
        new(RoutingStyle.Automatic, "Automatic"),
        new(RoutingStyle.Orthogonal, "Orthogonal"),
        new(RoutingStyle.StraightLine, "Straight-line"),
        new(RoutingStyle.Organic, "Organic"),
        new(RoutingStyle.Octilinear, "Octilinear")
    ];

    public IReadOnlyList<EnumChoice<PartialLayoutOrientation>> LayoutOrientations { get; } = [
        new(PartialLayoutOrientation.None, "None"),
        new(PartialLayoutOrientation.AutoDetect, "Auto-detect"),
        new(PartialLayoutOrientation.TopToBottom, "Top to Bottom"),
        new(PartialLayoutOrientation.BottomToTop, "Bottom to Top"),
        new(PartialLayoutOrientation.LeftToRight, "Left to Right"),
        new(PartialLayoutOrientation.RightToLeft, "Right to Left")
    ];

    public ComponentAssignmentStrategy ComponentAssignment {
      get => componentAssignment;
      set => SetField(ref componentAssignment, value);
    }

    public SubgraphLayoutKind SubgraphLayout {
      get => subgraphLayout;
      set => SetField(ref subgraphLayout, value);
    }

    public SubgraphPlacement SubgraphPlacement {
      get => subgraphPlacement;
      set => SetField(ref subgraphPlacement, value);
    }

    public RoutingStyle EdgeRoutingStyle {
      get => edgeRoutingStyle;
      set => SetField(ref edgeRoutingStyle, value);
    }

    public PartialLayoutOrientation LayoutOrientation {
      get => layoutOrientation;
      set => SetField(ref layoutOrientation, value);
    }

    public double MinimumNodeDistance {
      get => minimumNodeDistance;
      set => SetField(ref minimumNodeDistance, double.IsNaN(value) ? 0 : Math.Clamp(value, 0, 200));
    }

    public bool AllowMirroring {
      get => allowMirroring;
      set => SetField(ref allowMirroring, value);
    }

    public bool NodeSnapping {
      get => nodeSnapping;
      set => SetField(ref nodeSnapping, value);
    }

    public bool IsBusy {
      get => isBusy;
      set => SetField(ref isBusy, value);
    }

    public void SetOptions(
        SubgraphLayoutKind selectedSubgraphLayout,
        ComponentAssignmentStrategy selectedComponentAssignment,
        SubgraphPlacement selectedSubgraphPlacement,
        RoutingStyle selectedEdgeRoutingStyle,
        PartialLayoutOrientation selectedLayoutOrientation,
        double selectedMinimumNodeDistance,
        bool selectedAllowMirroring,
        bool selectedNodeSnapping) {
      SubgraphLayout = selectedSubgraphLayout;
      ComponentAssignment = selectedComponentAssignment;
      SubgraphPlacement = selectedSubgraphPlacement;
      EdgeRoutingStyle = selectedEdgeRoutingStyle;
      LayoutOrientation = selectedLayoutOrientation;
      MinimumNodeDistance = selectedMinimumNodeDistance;
      AllowMirroring = selectedAllowMirroring;
      NodeSnapping = selectedNodeSnapping;
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
