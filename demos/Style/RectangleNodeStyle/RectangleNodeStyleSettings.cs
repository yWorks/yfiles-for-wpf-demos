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
using yWorks.Graph.Styles;

namespace yWorks.Demo.RectangleNodeStyle
{
  public enum CornerSizeScaling
  {
    Absolute,
    Relative
  }

  internal sealed class RectangleNodeStyleSettings : INotifyPropertyChanged
  {
    private CornerStyle cornerStyle = CornerStyle.Round;
    private CornerSizeScaling cornerSizeScaling = CornerSizeScaling.Absolute;
    private bool topLeftCornerAffected = true;
    private bool topRightCornerAffected = true;
    private bool bottomLeftCornerAffected = true;
    private bool bottomRightCornerAffected = true;
    private bool isEnabled;
    private bool isUpdating;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SettingChanged;

    public CornerStyle CornerStyle {
      get => cornerStyle;
      set => SetProperty(ref cornerStyle, value);
    }

    public CornerSizeScaling CornerSizeScaling {
      get => cornerSizeScaling;
      set => SetProperty(ref cornerSizeScaling, value);
    }

    public bool TopLeftCornerAffected {
      get => topLeftCornerAffected;
      set => SetProperty(ref topLeftCornerAffected, value);
    }

    public bool TopRightCornerAffected {
      get => topRightCornerAffected;
      set => SetProperty(ref topRightCornerAffected, value);
    }

    public bool BottomLeftCornerAffected {
      get => bottomLeftCornerAffected;
      set => SetProperty(ref bottomLeftCornerAffected, value);
    }

    public bool BottomRightCornerAffected {
      get => bottomRightCornerAffected;
      set => SetProperty(ref bottomRightCornerAffected, value);
    }

    public bool IsEnabled {
      get => isEnabled;
      set => SetProperty(ref isEnabled, value);
    }

    /// <summary>
    ///   Updates the options panel from the selected node's style.
    /// </summary>
    /// <param name="style">The style whose settings should be displayed.</param>
    /// <param name="enabled">Whether the settings can currently be edited.</param>
    public void UpdateFrom(Graph.Styles.RectangleNodeStyle style, bool enabled) {
      isUpdating = true;
      try {
        CornerStyle = style.CornerStyle;
        CornerSizeScaling = style.ScaleCornerSize ? CornerSizeScaling.Relative : CornerSizeScaling.Absolute;
        TopLeftCornerAffected = (style.Corners & Corners.TopLeft) != 0;
        TopRightCornerAffected = (style.Corners & Corners.TopRight) != 0;
        BottomLeftCornerAffected = (style.Corners & Corners.BottomLeft) != 0;
        BottomRightCornerAffected = (style.Corners & Corners.BottomRight) != 0;
        IsEnabled = enabled;
      } finally {
        isUpdating = false;
      }
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return false;
      }

      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      if (!isUpdating && propertyName != nameof(IsEnabled)) {
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }

      return true;
    }
  }
}
