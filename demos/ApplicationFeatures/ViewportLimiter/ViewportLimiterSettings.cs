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
using yWorks.Controls;

namespace yWorks.Demo.ViewportLimiter
{
  public sealed class ViewportLimiterSettings : INotifyPropertyChanged
  {
    private ViewportLimitingPolicy policy = ViewportLimitingPolicy.WithinMargins;
    private bool strictBoundsContainment;
    private double marginLeft = 600;
    private double marginTop = 120;
    private double marginRight = 600;
    private double marginBottom = 120;
    private double ratioWidth = 0.8;
    private double ratioHeight = 0.4;
    private bool isometricView;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SettingChanged;

    public ViewportLimitingPolicy Policy {
      get => policy;
      set => SetField(ref policy, value);
    }

    public bool StrictBoundsContainment {
      get => strictBoundsContainment;
      set => SetField(ref strictBoundsContainment, value);
    }

    public double MarginLeft {
      get => marginLeft;
      set => SetField(ref marginLeft, value);
    }

    public double MarginTop {
      get => marginTop;
      set => SetField(ref marginTop, value);
    }

    public double MarginRight {
      get => marginRight;
      set => SetField(ref marginRight, value);
    }

    public double MarginBottom {
      get => marginBottom;
      set => SetField(ref marginBottom, value);
    }

    public double RatioWidth {
      get => ratioWidth;
      set => SetField(ref ratioWidth, value);
    }

    public double RatioHeight {
      get => ratioHeight;
      set => SetField(ref ratioHeight, value);
    }

    public bool IsometricView {
      get => isometricView;
      set => SetField(ref isometricView, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      SettingChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}
