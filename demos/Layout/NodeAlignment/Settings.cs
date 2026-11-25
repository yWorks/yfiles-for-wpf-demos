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
using yWorks.Layout;

namespace yWorks.Demo.NodeAlignment
{
  /// <summary>
  ///   Represents the layout settings available in the demo.
  /// </summary>
  public sealed class Settings : INotifyPropertyChanged
  {
    private AlignmentPolicy alignmentPolicy;
    private double minimumNodeDistance;
    private double snapDistance;
    private bool separateStripes;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SettingChanged;

    public IReadOnlyList<AlignmentPolicyOption> AlignmentPolicyOptions { get; } = [
        new(AlignmentPolicy.SnapX, "Snap X"),
        new(AlignmentPolicy.SnapY, "Snap Y"),
        new(AlignmentPolicy.SnapXY, "Snap XY")
    ];

    /// <summary>
    ///   The axis, parallel to which the nodes are aligned.
    /// </summary>
    public AlignmentPolicy AlignmentPolicy {
      get => alignmentPolicy;
      set => SetField(ref alignmentPolicy, value);
    }

    /// <summary>
    ///   The minimum distance between two nodes.
    /// </summary>
    public double MinimumNodeDistance {
      get => minimumNodeDistance;
      set => SetField(ref minimumNodeDistance, Math.Max(0, value));
    }

    /// <summary>
    ///   The maximum distance between two nodes when they are aligned on the same line.
    /// </summary>
    public double SnapDistance {
      get => snapDistance;
      set => SetField(ref snapDistance, Math.Max(0, value));
    }

    /// <summary>
    ///   Determines whether nodes are placed in strictly separated rows and columns.
    /// </summary>
    public bool SeparateStripes {
      get => separateStripes;
      set => SetField(ref separateStripes, value);
    }

    public Settings(AlignmentStage defaults) {
      alignmentPolicy = defaults.AlignmentPolicy;
      minimumNodeDistance = defaults.MinimumNodeDistance;
      snapDistance = defaults.SnapDistance;
      separateStripes = defaults.SeparateStripes;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return false;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      SettingChanged?.Invoke(this, EventArgs.Empty);
      return true;
    }
  }

  /// <summary>
  ///   A display label and value for an alignment policy option.
  /// </summary>
  public sealed record AlignmentPolicyOption(AlignmentPolicy Value, string Name);
}
