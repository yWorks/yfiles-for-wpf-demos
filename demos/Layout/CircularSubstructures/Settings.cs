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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CircularStarSubstructureStyle = yWorks.Layout.Circular.StarSubstructureStyle;

namespace yWorks.Demo.CircularSubstructures
{
  public sealed class Settings : INotifyPropertyChanged
  {
    private CircularStarSubstructureStyle starSubstructureStyle = CircularStarSubstructureStyle.SeparatedRadial;
    private bool considerNodeTypes = true;
    private bool starSubstructureTypeSeparation;
    private bool isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SettingChanged;

    public CircularStarSubstructureStyle StarSubstructureStyle {
      get => starSubstructureStyle;
      set {
        if (starSubstructureStyle == value) {
          return;
        }
        starSubstructureStyle = value;
        OnPropertyChanged();
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    public bool ConsiderNodeTypes {
      get => considerNodeTypes;
      set {
        if (considerNodeTypes == value) {
          return;
        }
        considerNodeTypes = value;
        OnPropertyChanged();
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    public bool StarSubstructureTypeSeparation {
      get => starSubstructureTypeSeparation;
      set {
        if (starSubstructureTypeSeparation == value) {
          return;
        }
        starSubstructureTypeSeparation = value;
        OnPropertyChanged();
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    public bool IsBusy {
      get => isBusy;
      set {
        if (isBusy == value) {
          return;
        }
        isBusy = value;
        OnPropertyChanged();
      }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
