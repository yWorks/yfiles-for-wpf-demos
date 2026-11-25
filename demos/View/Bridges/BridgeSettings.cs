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
using yWorks.Controls;

namespace yWorks.Demo.Bridges
{
  internal sealed class BridgeSettings : INotifyPropertyChanged
  {
    private BridgeCrossingStyle crossingStyle;
    private BridgeCrossingPolicy crossingPolicy;
    private BridgeOrientationStyle orientationStyle;
    private double bridgeWidth;
    private double bridgeHeight;
    private double maximumBridgeWidth;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SettingChanged;

    public BridgeCrossingStyle CrossingStyle {
      get => crossingStyle;
      set => SetProperty(ref crossingStyle, value);
    }

    public BridgeCrossingPolicy CrossingPolicy {
      get => crossingPolicy;
      set => SetProperty(ref crossingPolicy, value);
    }

    public BridgeOrientationStyle OrientationStyle {
      get => orientationStyle;
      set => SetProperty(ref orientationStyle, value);
    }

    public double BridgeWidth {
      get => bridgeWidth;
      set => SetProperty(ref bridgeWidth, value);
    }

    public double BridgeHeight {
      get => bridgeHeight;
      set => SetProperty(ref bridgeHeight, value);
    }

    public double MaximumBridgeWidth {
      get => maximumBridgeWidth;
      set => SetProperty(ref maximumBridgeWidth, value);
    }

    public BridgeSettings(BridgeManager bridgeManager) {
      crossingStyle = bridgeManager.DefaultBridgeCrossingStyle;
      crossingPolicy = bridgeManager.BridgeCrossingPolicy;
      orientationStyle = bridgeManager.DefaultBridgeOrientationStyle;
      bridgeWidth = bridgeManager.DefaultBridgeWidth;
      bridgeHeight = bridgeManager.DefaultBridgeHeight;
      maximumBridgeWidth = bridgeManager.MaximumBridgeWidth;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return false;
      }

      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      SettingChanged?.Invoke(this, EventArgs.Empty);
      return true;
    }
  }
}
