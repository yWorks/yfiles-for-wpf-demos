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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Demo.yFiles.Graph.OrgChart
{
  /// <summary>
  /// symbolic zoom levels
  /// </summary>
  public enum ZoomLevel
  {
    Detail, Intermediate, Overview
  }

  /// <summary>
  /// Converter which converts from a double zoom value to a symbolic <see cref="ZoomLevel"/>.
  /// </summary>
  /// <remarks>
  /// Two thresholds can be set (<see cref="DetailThreshold"/> and <see cref="OverviewThreshold"/>),
  /// which define the intervals of the incoming zoom value which are mapped to one of the three
  /// symbolic zoom levels.
  /// </remarks>
  [ValueConversion(typeof(double), typeof(ZoomLevel))]
  public class ZoomToZoomLevelConverter : IValueConverter
  {
    /// <summary>
    /// Gets or sets the detail threshold. A zoom value greater than or equal to this
    /// threshold is mapped to the <see cref="ZoomLevel.Detail"/> zoom level.
    /// </summary>
    public double DetailThreshold { get; set; }

    /// <summary>
    /// Gets or sets the overview threshold. A zoom value less than or equal to this
    /// threshold is mapped to the <see cref="ZoomLevel.Overview"/> zoom level.
    /// </summary>
    public double OverviewThreshold { get; set; }

    public ZoomToZoomLevelConverter() {
      DetailThreshold = 0.7;
      OverviewThreshold = 0.2;
    }

    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value != null) {
        double zoom = (double)value;
        if (zoom >= DetailThreshold) {
          return ZoomLevel.Detail;
        }
        if (zoom <= OverviewThreshold) {
          return ZoomLevel.Overview;
        }

      }
      return ZoomLevel.Intermediate;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }

    #endregion
  }

  /// <summary>
  /// This converter maps the icon attribute of the business data for an employee to an image source.
  /// </summary>
  [ValueConversion(typeof(string), typeof(string))]
  public class NameToImageSourceConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return "pack://application:,,,/Resources/" + value + ".png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// This converter maps a boolean value to a Visibility
  /// </summary>
  [ValueConversion(typeof(bool), typeof(Visibility))]
  public class BoolToVisibilityConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// A converter that converts between zoom levels and DataTemplates
  /// </summary>
  public class ZoomToTemplateConverter : IValueConverter
  {
    /// <summary>
    /// Gets or sets the detail node style template.
    /// </summary>
    /// <value>The detail node style template.</value>
    public DataTemplate DetailTemplate { get; set; }

    /// <summary>
    /// Gets or sets the overview node style template.
    /// </summary>
    /// <value>The overview node style template.</value>
    public DataTemplate OverviewTemplate { get; set; }

    /// <summary>
    /// Gets or sets the normal node style template.
    /// </summary>
    /// <value>The normal node style template.</value>
    public DataTemplate NormalTemplate { get; set; }

    /// <summary>
    /// Gets or sets the detail threshold. A zoom value greater than or equal to this
    /// threshold is mapped to the <see cref="DetailTemplate"/> zoom level.
    /// </summary>
    public double DetailThreshold { get; set; }

    /// <summary>
    /// Gets or sets the overview threshold. A zoom value less than or equal to this
    /// threshold is mapped to the <see cref="OverviewTemplate"/> zoom level.
    /// </summary>
    public double OverviewThreshold { get; set; }

    public ZoomToTemplateConverter() {
      DetailThreshold = 0.7;
      OverviewThreshold = 0.2;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value != null) {
        double zoom = (double)value;
        if (zoom >= DetailThreshold) {
          return DetailTemplate;
        }
        if (zoom <= OverviewThreshold) {
          return OverviewTemplate;
        }
      }
      return NormalTemplate;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

}
