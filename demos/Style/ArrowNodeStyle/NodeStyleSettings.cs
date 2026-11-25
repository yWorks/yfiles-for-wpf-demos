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

namespace yWorks.Demo.ArrowNodeStyle
{
  /// <summary>
  ///   View model backing the style property editor in the demo's sidebar.
  /// </summary>
  public sealed class NodeStyleSettings : INotifyPropertyChanged
  {
    private bool suppressChangeNotification;
    private ArrowStyleShape shape = ArrowStyleShape.Arrow;
    private ArrowNodeDirection direction = ArrowNodeDirection.Right;
    private double angle = 90;
    private double shaftRatio = 0.3;
    private bool isEnabled;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Raised whenever a property has been changed interactively by the user, i.e. not through
    ///   <see cref="UpdateFrom"/>.
    /// </summary>
    public event EventHandler? SettingChanged;

    public ArrowStyleShape Shape {
      get => shape;
      set => SetField(ref shape, value);
    }

    public ArrowNodeDirection Direction {
      get => direction;
      set => SetField(ref direction, value);
    }

    /// <summary>The tip angle, in degrees, as shown in the property editor.</summary>
    public double Angle {
      get => angle;
      set => SetField(ref angle, value);
    }

    public double ShaftRatio {
      get => shaftRatio;
      set => SetField(ref shaftRatio, value);
    }

    /// <summary>Whether a node is selected and the property editor should be usable.</summary>
    public bool IsEnabled {
      get => isEnabled;
      set => SetField(ref isEnabled, value);
    }

    /// <summary>
    ///   Copies the values of the given style into this settings instance without raising
    ///   <see cref="SettingChanged"/>.
    /// </summary>
    public void UpdateFrom(Graph.Styles.ArrowNodeStyle style, bool enabled) {
      suppressChangeNotification = true;
      try {
        Shape = style.Shape;
        Direction = style.Direction;
        Angle = ToDegrees(style.Angle);
        ShaftRatio = style.ShaftRatio;
        IsEnabled = enabled;
      } finally {
        suppressChangeNotification = false;
      }
    }

    /// <summary>Applies the current values to the given style.</summary>
    public void ApplyTo(Graph.Styles.ArrowNodeStyle style) {
      style.Shape = Shape;
      style.Direction = Direction;
      style.Angle = ToRadians(Angle);
      style.ShaftRatio = ShaftRatio;
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
      if (Equals(field, value)) {
        return;
      }

      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      if (!suppressChangeNotification) {
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    /// <summary>Returns the given angle in degrees.</summary>
    private static double ToDegrees(double radians) => radians * 180 / Math.PI;

    /// <summary>Returns the given angle in radians.</summary>
    private static double ToRadians(double degrees) => degrees / 180 * Math.PI;
  }
}
