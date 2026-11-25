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

namespace yWorks.Demo.BezierEdgeStyle
{
  public enum SampleKind
  {
    CircularLayout,
    GraphWithLabels
  }

  /// <summary>Configuration object for managing the demo settings.</summary>
  public sealed class BezierSettings : INotifyPropertyChanged
  {
    private bool smoothSegments = true;
    private bool autoRotation = true;
    private bool autoSnapping = true;
    private double angle;
    private SampleKind selectedSample = SampleKind.CircularLayout;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<PropertyChangedEventArgs>? SettingChanged;

    public bool SmoothSegments {
      get => smoothSegments;
      set => SetField(ref smoothSegments, value);
    }

    public bool AutoRotation {
      get => autoRotation;
      set => SetField(ref autoRotation, value);
    }

    public bool AutoSnapping {
      get => autoSnapping;
      set => SetField(ref autoSnapping, value);
    }

    public double Angle {
      get => angle;
      set => SetField(ref angle, value);
    }

    public SampleKind SelectedSample {
      get => selectedSample;
      set => SetField(ref selectedSample, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return;
      }
      field = value;
      var args = new PropertyChangedEventArgs(propertyName);
      PropertyChanged?.Invoke(this, args);
      SettingChanged?.Invoke(this, args);
    }
  }
}
