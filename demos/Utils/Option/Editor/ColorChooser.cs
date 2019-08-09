/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using yWorks.Utils;

namespace Demo.yFiles.Option.Editor
{
  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class DoubleMultiplyConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double factor;
      if (parameter is string) {
        factor = XmlConvert.ToDouble((string)parameter);
      } else if (parameter is double) {
        factor = (double) parameter;
      } else {
        factor = 1;
      }
      return ((double) value)*factor;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class InverseConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return 1 - (double) value;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return 1 - (double)value;
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class PopupButton : Button
  {
    /// <xamlhelper/>
    protected override void OnClick() {
      base.OnClick();
      System.Windows.Controls.Primitives.Popup p = Popup;
      if (p != null) {
        p.Closed -= p_Closed;
        if (p.PlacementTarget == null) {
          p.PlacementTarget = this;
          p.PlacementRectangle = new Rect(0,0, ActualWidth, ActualHeight);
        }
        p.Closed += p_Closed;
        p.IsOpen = true;
      }
    }

    void p_Closed(object sender, EventArgs e) {
      // thats a hack - well, who told you to use PopupButton anyway?
      try {
        ((ColorChooser)Popup.Child).GetBindingExpression(ColorChooser.ColorProperty).UpdateSource();
      } catch (Exception) {}
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty PopupProperty =
      DependencyProperty.Register("Popup", typeof (Popup), typeof (PopupButton));

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public Popup Popup {
      get { return (Popup) GetValue(PopupProperty); }
      set { SetValue(PopupProperty, value); }
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class ColorHexConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (targetType == typeof(string)) {
        Color c = (Color) value;
        return String.Format(culture, "#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
      } else {
        return null;
      }
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      string s = value.ToString().Trim();
      try {
        return new ColorConverter().Convert(s, typeof(Color), null, culture);
      } catch (NotSupportedException) {
        s = s.ToLower(culture);
        if (s.StartsWith("0x")) {
          s = s.Substring(2);
        }
        if (s.StartsWith("x")) {
          s = s.Substring(1);
        }
        if (s.StartsWith("#")) {
          s = s.Substring(1);
        }
        int result;
        if (Int32.TryParse(s, NumberStyles.AllowHexSpecifier, culture, out result)) {
          return
            Color.FromArgb((byte)((result & 0xff000000) >> 24), (byte)((result & 0x00ff0000) >> 16),
                           (byte)((result & 0x0000ff00) >> 8), (byte)(result & 0x000000ff));
        } else {
          return DependencyProperty.UnsetValue;
        }
      }
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class HsbImageSourceConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double[] coefficients = HsbRangeImageSourceConverter.GetCoefficients(parameter);
      double h = (double) value;
      int iw = 32;
      int ih = 32;
      WriteableBitmap image = new WriteableBitmap(iw, ih, 92, 92, PixelFormats.Pbgra32, null);
      byte[] pixels = new byte[iw*ih*4];
      int i = 0;
      for (int y= 0; y < ih; y++) {
        double dy = y/((double)(ih - 1));
        for (int x = 0; x < iw; x++) {
          double dx = x / ((double)(iw - 1));
          Color color = ColorHelper.FromHSB(dx * coefficients[0] + dy * coefficients[1] + h * coefficients[2] + coefficients[3],
                                             dx * coefficients[4] + dy * coefficients[5] + h * coefficients[6] + coefficients[7],
                                             dx * coefficients[8] + dy * coefficients[9] + h * coefficients[10] + coefficients[11],
                                             dx * coefficients[12] + dy * coefficients[13] + h * coefficients[14] + coefficients[15]);
          pixels[i++] = color.B;
          pixels[i++] = color.G;
          pixels[i++] = color.R;
          pixels[i++] = 255;
        }
      }
      image.WritePixels(new Int32Rect(0, 0, iw, ih), pixels, iw*4 , 0);
      return image;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class HsbRangeImageSourceConverter : IValueConverter
  {

    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double[] coefficients = GetCoefficients(parameter);
      double h = (double) value;
      int iw = 1;
      int ih = 32;
      WriteableBitmap image = new WriteableBitmap(iw, ih, 92, 92, PixelFormats.Pbgra32, null);
      byte[] pixels = new byte[iw*ih*4];
      {
        int i = 0;
        for (int y= 0; y < ih; y++) {
          double dy = 1.0d - (y/((double)(ih - 1)));
          double dx = 1;
          Color color = ColorHelper.FromHSB(dx * coefficients[0] + dy * coefficients[1] + h*coefficients[2] + coefficients[3], 
                                             dx * coefficients[4] + dy * coefficients[5] + h*coefficients[6] + coefficients[7],
                                             dx * coefficients[8] + dy * coefficients[9] + h * coefficients[10] + coefficients[11],
                                             dx * coefficients[12] + dy * coefficients[13] + h*coefficients[14] + coefficients[15]);
          pixels[i++] = color.B;
          pixels[i++] = color.G;
          pixels[i++] = color.R;
          pixels[i++] = color.A;
        }
      }
      image.WritePixels(new Int32Rect(0, 0, iw, ih), pixels, iw*4 , 0);
      return image;
    }

    internal static double[] GetCoefficients(object parameter) {
      string pattern = parameter as string;
      double[] coefficients = new double[16];
      if (pattern != null) {
        string[] items = pattern.Split(new char[]{';'});
        for (int i = 0; i < 4; i++ ) {
          string s = items[i].Trim().ToLower();
          if (s.Equals("x")) {
            coefficients[i*4] = 1;
          } else if (s.Equals("y")) {
            coefficients[i*4 + 1] = 1;
          } else if (s.Equals("v")) {
            coefficients[i*4 + 2] = 1;
          } else if (s.Equals("-x")) {
            coefficients[i*4] = -1;
            coefficients[i*4 + 3] = 1;
          } else if (s.Equals("-y")) {
            coefficients[i*4 + 1] = -1;
            coefficients[i * 4 + 3] = 1;
          } else if (s.Equals("-v")) {
            coefficients[i*4 + 2] = -1;
            coefficients[i * 4 + 3] = 1;
          } else {
            double d = XmlConvert.ToDouble(s);
            coefficients[i*4 + 3] = d;
          }
        }
      } else {
        coefficients[0] = 1;
        coefficients[5] = 1;
        coefficients[10] = 1;
        coefficients[15] = 1;
      }
      return coefficients;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class RgbImageSourceConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double[] coefficients = HsbRangeImageSourceConverter.GetCoefficients(parameter);
      double h = (double) value;
      int iw = 32;
      int ih = 32;
      WriteableBitmap image = new WriteableBitmap(iw, ih, 92, 92, PixelFormats.Pbgra32, null);
      byte[] pixels = new byte[iw*ih*4];
      int i = 0;
      for (int y= 0; y < ih; y++) {
        double dy = y/((double)(ih - 1));
        for (int x = 0; x < iw; x++) {
          double dx = x / ((double)(iw - 1));
          pixels[i++] = (byte)((dx * coefficients[8] + dy * coefficients[9] + h * coefficients[10] + coefficients[11]) * 255);
          pixels[i++] = (byte)((dx * coefficients[4] + dy * coefficients[5] + h * coefficients[6] + coefficients[7]) * 255);
          pixels[i++] = (byte)((dx * coefficients[0] + dy * coefficients[1] + h * coefficients[2] + coefficients[3]) * 255);
          pixels[i++] = (byte)((dx * coefficients[12] + dy * coefficients[13] + h*coefficients[14] + coefficients[15]) * 255);
        }
      }
      image.WritePixels(new Int32Rect(0, 0, iw, ih), pixels, iw*4 , 0);
      return image;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class MyThumb : Thumb
  {
    /// <xamlhelper/>
    public MyThumb() {
      this.DragDelta += new DragDeltaEventHandler(MyThumb_DragDelta);
    }

    void MyThumb_DragDelta(object sender, DragDeltaEventArgs e) {
      System.Windows.Controls.Canvas canvas = Parent as System.Windows.Controls.Canvas;
      if (canvas != null) {
        double w = canvas.ActualWidth;
        double h = canvas.ActualWidth;
        double top = System.Windows.Controls.Canvas.GetTop(this);
        if (double.IsNaN(top)) {
          top = 0;
        }
        top += this.ActualHeight*0.5d;
        double newVal = (top + e.VerticalChange)/h;
        newVal = Math.Max(0, Math.Min(1, newVal));
        SetValue(YProperty, newVal);
        double left = System.Windows.Controls.Canvas.GetLeft(this);
        if (double.IsNaN(left)) {
          left = 0;
        }
        left += this.ActualWidth*0.5d;
        double newX = (left + e.HorizontalChange) / w;
        newX = Math.Max(0, Math.Min(1, newX));
        SetValue(XProperty, newX);
        Focus();
      }
    }

    /// <xamlhelper/>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
      base.OnRenderSizeChanged(sizeInfo);
      System.Windows.Controls.Canvas canvas = this.Parent as System.Windows.Controls.Canvas;
      if (canvas != null) {
        if (handler == null) {
          handler = new System.Windows.Input.MouseButtonEventHandler(canvas_MouseDown);
          canvas.MouseDown += handler;
        }
        System.Windows.Controls.Canvas.SetLeft(this, X * canvas.ActualWidth - this.ActualWidth * 0.5d);
        System.Windows.Controls.Canvas.SetTop(this, Y * canvas.ActualHeight - this.ActualHeight * 0.5d);
      }
    }

    void canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      System.Windows.Controls.Canvas canvas = this.Parent as System.Windows.Controls.Canvas;
      if (canvas != null) {
        Point point = e.GetPosition(canvas);
        SetValue(XProperty, point.X / canvas.ActualWidth);
        SetValue(YProperty, point.Y / canvas.ActualHeight);
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty XProperty =
      DependencyProperty.Register("X", typeof (double), typeof (MyThumb),
                                  new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                XPropertyChangedCallback, CoerceValueCallback));

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty YProperty =
      DependencyProperty.Register("Y", typeof (double), typeof (MyThumb),
                                  new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                YPropertyChangedCallback, CoerceValueCallback));

    private MouseButtonEventHandler handler;

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double X {
      get { return (double) GetValue(XProperty); }
      set { SetValue(XProperty, value); }
    }
    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Y {
      get { return (double) GetValue(YProperty); }
      set { SetValue(YProperty, value); }
    }

    private static object CoerceValueCallback(DependencyObject d, object baseValue) {
      return baseValue;
    }

    private static void XPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      MyThumb thumb = (MyThumb) d;
      System.Windows.Controls.Canvas canvas = thumb.Parent as System.Windows.Controls.Canvas;
      if (canvas != null) {
        double val = (double) e.NewValue;
        System.Windows.Controls.Canvas.SetLeft(thumb, val * canvas.ActualWidth - thumb.ActualWidth*0.5d);
      }

    }
    private static void YPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      MyThumb thumb = (MyThumb) d;
      System.Windows.Controls.Canvas canvas = thumb.Parent as System.Windows.Controls.Canvas;
      if (canvas != null) {
        double val = (double) e.NewValue;
        System.Windows.Controls.Canvas.SetTop(thumb, val * canvas.ActualHeight - thumb.ActualHeight*0.5d);
      }

    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class ColorChooser : Control
  {

    static ColorChooser() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorChooser), new FrameworkPropertyMetadata(typeof(ColorChooser)));
    }

    /// <xamlhelper/>
    public ColorChooser() { }

    /// <summary>
    /// Identifies the ColorChanged routed event.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly RoutedEvent ColorChangedEvent = EventManager.RegisterRoutedEvent(
        "ColorChanged", RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<Color>), typeof(ColorChooser));

    /// <summary>
    /// Identifies the ValueChanged routed event.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        "ValueChanged", RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<object>), typeof(ColorChooser));

    /// <summary>
    /// Occurs when the Color property changes.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public event RoutedPropertyChangedEventHandler<Color> ColorChanged {
      add { AddHandler(ColorChangedEvent, value); }
      remove { RemoveHandler(ColorChangedEvent, value); }
    }

    /// <summary>
    /// Occurs when the Color property changes.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public event RoutedPropertyChangedEventHandler<object> ValueChanged {
      add { AddHandler(ValueChangedEvent, value); }
      remove { RemoveHandler(ValueChangedEvent, value); }
    }

    /// <summary>
    /// Raises the ColorChanged event.
    /// </summary>
    /// <param name="args">Arguments associated with the ValueChanged event.</param>
    protected virtual void OnColorChanged(RoutedPropertyChangedEventArgs<Color> args) {
      RaiseEvent(args);
    }

    /// <summary>
    /// Raises the ValueChanged event.
    /// </summary>
    /// <param name="args">Arguments associated with the ValueChanged event.</param>
    protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<object> args) {
      RaiseEvent(args);
    }


    private static object CoerceColorValue(DependencyObject d0, object baseValue) {
      double d = baseValue is double ? (double)baseValue : 0.0;
      if (d > 1) {
        return 1;
      }
      if (d < 0) {
        return 0;
      }
      return d;
    }

    private void UpdateRGBColor() {
      if (updateInProgress) {
        return;
      }

      Color color = Color.FromArgb((byte)(255 * Alpha), (byte)(255 * Red), (byte)(255 * Green), (byte)(255 * Blue));
      SetValue(ColorProperty, color);
    }

    private void UpdateAlpha() {
      if (updateInProgress) {
        return;
      }
      Color c = Color;
      c.A = (byte) (255*Alpha);
      updateInProgress = true;
      try {
        SetValue(ColorProperty, c);
      } finally {
        updateInProgress = false;
      }
    }

    private void UpdateHSBColor() {
      if (updateInProgress) {
        return;
      }
      updateInProgress = true;
      try {
        Color value = ColorHelper.FromHSB(Hue, Saturation, Brightness, Alpha);
        SetValue(ColorProperty, value);
        Red = value.R / 255.0d;
        Green = value.G / 255.0d;
        Blue = value.B / 255.0d;
      } finally {
        updateInProgress = false;
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty NoColorAllowedProperty =
      DependencyProperty.Register("NoColorAllowed", typeof(bool), typeof(ColorChooser),
                                          new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public bool NoColorAllowed {
      get { return (bool)GetValue(NoColorAllowedProperty); }
      set {
        SetValue(NoColorAllowedProperty, (bool)value);
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)] 
    private static readonly DependencyPropertyKey IsNullPropertyKey =
      DependencyProperty.RegisterReadOnly("IsNull", typeof(bool), typeof(ColorChooser),
                                          new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty IsNullProperty = IsNullPropertyKey.DependencyProperty;

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public bool IsNull {
      get { return (bool)GetValue(IsNullProperty); }
      private set {
        SetValue(IsNullPropertyKey, value);
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register("Value", typeof(object), typeof(ColorChooser),
                                          new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ColorChooser c = d as ColorChooser;
      if (d != null) {
        c.IsNull = e.NewValue is Color ? false : true;
        c.Color = c.IsNull ? Colors.Transparent : (Color)e.NewValue;
      }

      RoutedPropertyChangedEventArgs<object> ea = new RoutedPropertyChangedEventArgs<object>(
        e.OldValue, e.NewValue, ValueChangedEvent);
      c.OnValueChanged(ea);
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Value {
      get { return GetValue(ValueProperty); }
      set {
        SetValue(ValueProperty, value);
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty ColorProperty =
      DependencyProperty.Register("Color", typeof(Color), typeof(ColorChooser),
                                          new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ColorChooser c = d as ColorChooser;
      if (c != null) {
        c.SetColor((Color)e.NewValue);

        RoutedPropertyChangedEventArgs<Color> ea = new RoutedPropertyChangedEventArgs<Color>(
    (Color)e.OldValue, (Color)e.NewValue, ColorChangedEvent);
        c.OnColorChanged(ea);

      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public Color Color {
      get { return (Color)GetValue(ColorProperty); }
      set {
        SetColor(value);
        SetValue(ColorProperty, value);
      }
    }

    private void SetColor(Color value) {
      if (updateInProgress) {
        return;
      } else {
        updateInProgress = true;
        try {
          Red = value.R / 255.0d;
          Green = value.G / 255.0d;
          Blue = value.B / 255.0d;
          Alpha = value.A / 255.0d;
          var colorValues = ColorHelper.ToHSB(value);
          double hue = colorValues[0];
          double saturation = colorValues[1];
          double brightness = colorValues[2];
          Hue = hue;
          Saturation = saturation;
          Brightness = brightness;
        } finally {
          updateInProgress = false;
        }
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty RedProperty =
      DependencyProperty.Register("Red", typeof(double), typeof(ColorChooser), new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, OnRedChanged, CoerceColorValue),
                                  ValidateColorRange);


    private static void OnRedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ColorChooser c = d as ColorChooser;
      if (c != null) {
        c.UpdateRGBColor();
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Red {
      get { return (double)GetValue(RedProperty); }
      set { SetValue(RedProperty, value); }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty GreenProperty =
      DependencyProperty.Register("Green", typeof(double), typeof(ColorChooser),
                                  new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, OnGreenChanged), ValidateColorRange);

    private static void OnGreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ColorChooser c = d as ColorChooser;
      if (c != null) {
        c.UpdateRGBColor();
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Green {
      get { return (double)GetValue(GreenProperty); }
      set { SetValue(GreenProperty, value); }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty BlueProperty =
      DependencyProperty.Register("Blue", typeof(double), typeof(ColorChooser), new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, OnBlueChanged, CoerceColorValue),
                                  ValidateColorRange);

    private static void OnBlueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ColorChooser c = d as ColorChooser;
      if (c != null) {
        c.UpdateRGBColor();
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Blue {
      get { return (double)GetValue(BlueProperty); }
      set { SetValue(BlueProperty, value); }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty HueProperty =
      DependencyProperty.Register("Hue", typeof(double), typeof(ColorChooser), new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, OnHSVChanged, CoerceColorValue),
                                  ValidateColorRange);


    private static void OnHSVChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ColorChooser c = d as ColorChooser;
      if (c != null) {
        c.UpdateHSBColor();
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Hue {
      get { return (double)GetValue(HueProperty); }
      set { SetValue(HueProperty, value); }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty SaturationProperty =
      DependencyProperty.Register("Saturation", typeof(double), typeof(ColorChooser),
                                  new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, OnHSVChanged), ValidateColorRange);

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Saturation {
      get { return (double)GetValue(SaturationProperty); }
      set { SetValue(SaturationProperty, value); }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty BrightnessProperty =
      DependencyProperty.Register("Brightness", typeof(double), typeof(ColorChooser), new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, OnHSVChanged, CoerceColorValue),
                                  ValidateColorRange);

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Brightness {
      get { return (double)GetValue(BrightnessProperty); }
      set { SetValue(BrightnessProperty, value); }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty AlphaProperty =
      DependencyProperty.Register("Alpha", typeof(double), typeof(ColorChooser), new FrameworkPropertyMetadata(1.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, OnAlphaChanged, CoerceColorValue),
                                  ValidateColorRange);

    private bool updateInProgress = false;

    private static void OnAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ColorChooser c = d as ColorChooser;
      if (c != null) {
        c.UpdateAlpha();
      }
    }

    /// <xamlhelper/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Alpha {
      get { return (double)GetValue(AlphaProperty); }
      set { SetValue(AlphaProperty, value); }
    }

    private static bool ValidateColorRange(object value) {
      return value is double && ((double)value) >= 0.0d && ((double)value) <= 1.0d;
    }


    public static readonly object NullColorObject = new object();
  }
}