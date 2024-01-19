/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Neo4JIntegration
{
  /// <summary>
  /// Converts a string to Titlecase.
  /// </summary>
  public class TitleCaseConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => CultureInfo.CurrentUICulture.TextInfo.ToTitleCase((string) value);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }

  /// <summary>
  /// Converts the first item of a list to a suitable brush.
  /// </summary>
  public class HeaderBrushConverter : IValueConverter
  {
    static readonly Brush[] brushes = new[] { Brushes.DodgerBlue, Brushes.Orange, Brushes.Crimson, Brushes.Green, Brushes.DarkSlateBlue };

    private Dictionary<string, Brush> seenLabels = new Dictionary<string, Brush>();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      // Somewhat arbitrarily, use the first label as the "type" of the node and assign the same color to identical types. 
      var label = ((IReadOnlyList<string>) value)[0];
      Brush brush;
      if (!seenLabels.ContainsKey(label)) {
        brush = brushes[seenLabels.Count % brushes.Length];
        seenLabels[label] = brush;
      } else {
        brush = seenLabels[label];
      }
      return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }

  /// <summary>
  /// Retrieves the first item of a list.
  /// </summary>
  public class FirstLabelConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((IReadOnlyList<string>) value)[0];
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
  }
}
