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

namespace yWorks.Demo.ArrowEdgeStyle
{
  /// <summary>
  ///   View model backing the style property editor in the demo's sidebar.
  /// </summary>
  /// <remarks>
  ///   Reflects the values of the currently selected edge's <see cref="ArrowEdgeStyle"/> (see
  ///   <see cref="UpdateFrom"/>) and pushes user edits back to the graph via <see cref="SettingChanged"/> and
  ///   <see cref="ApplyTo"/>.
  /// </remarks>
  public sealed class EdgeStyleSettings : INotifyPropertyChanged
  {
    private bool suppressChangeNotification;

    private ArrowStyleShape shape = ArrowStyleShape.Arrow;
    private double thickness = 18;
    private double angle = 45;
    private double shaftRatio = 0.3;
    private double cropping = 3;
    private bool isEnabled;

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Raised whenever a property has been changed interactively by the user, i.e. not through
    ///   <see cref="UpdateFrom"/>.
    /// </summary>
    public event EventHandler SettingChanged;

    public ArrowStyleShape Shape {
      get => shape;
      set => SetField(ref shape, value);
    }

    public double Thickness {
      get => thickness;
      set => SetField(ref thickness, value);
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

    public double Cropping {
      get => cropping;
      set => SetField(ref cropping, value);
    }

    /// <summary>Whether an edge is currently selected and the property editor should be usable.</summary>
    public bool IsEnabled {
      get => isEnabled;
      set => SetField(ref isEnabled, value);
    }

    /// <summary>
    ///   The shaft ratio has no visual effect for <see cref="ArrowStyleShape.Parallelogram"/> and
    ///   <see cref="ArrowStyleShape.Trapezoid"/>, so the corresponding slider is disabled for those shapes.
    /// </summary>
    public bool IsShaftRatioEnabled =>
        IsEnabled && Shape is not (ArrowStyleShape.Parallelogram or ArrowStyleShape.Trapezoid);

    /// <summary>
    ///   Parallelogram, trapezoid and full-shaft-ratio arrows may point backwards, so the angle slider allows
    ///   negative values in that case.
    /// </summary>
    public double AngleMinimum =>
        Shape is ArrowStyleShape.Parallelogram or ArrowStyleShape.Trapezoid || ShaftRatio == 1 ? -90 : 0;

    /// <summary>
    ///   Copies the values of the given style into this settings instance without raising <see cref="SettingChanged"/>.
    /// </summary>
    public void UpdateFrom(Graph.Styles.ArrowEdgeStyle style, bool enabled) {
      suppressChangeNotification = true;
      try {
        Shape = style.Shape;
        Thickness = style.Thickness;
        Angle = ToDegrees(style.Angle);
        ShaftRatio = style.ShaftRatio;
        Cropping = style.SourceCropping;
        IsEnabled = enabled;
      } finally {
        suppressChangeNotification = false;
      }
    }

    /// <summary>
    ///   Applies the current values to the given style, preserving its brush and pen.
    /// </summary>
    public void ApplyTo(Graph.Styles.ArrowEdgeStyle style) {
      style.Shape = Shape;
      style.Thickness = Thickness;
      style.Angle = ToRadians(Angle);
      style.ShaftRatio = ShaftRatio;
      style.SourceCropping = Cropping;
      style.TargetCropping = Cropping;
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
      if (Equals(field, value)) {
        return;
      }
      field = value;
      OnPropertyChanged(propertyName);

      if (propertyName is nameof(Shape) or nameof(ShaftRatio)) {
        OnPropertyChanged(nameof(AngleMinimum));
      }
      if (propertyName is nameof(Shape) or nameof(IsEnabled)) {
        OnPropertyChanged(nameof(IsShaftRatioEnabled));
      }

      if (!suppressChangeNotification) {
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    private void OnPropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>Returns the given angle in degrees.</summary>
    public static double ToDegrees(double radians) => radians * 180 / Math.PI;

    /// <summary>Returns the given angle in radians.</summary>
    public static double ToRadians(double degrees) => degrees / 180 * Math.PI;
  }
}
