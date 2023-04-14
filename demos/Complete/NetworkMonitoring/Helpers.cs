/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Demo.yFiles.Graph.NetworkMonitoring.Model;

namespace Demo.yFiles.Graph.NetworkMonitoring
{
  /// <summary>
  /// Helper class containing display information about nodes. This is just necessary for easy parsing from
  /// GraphML.
  /// </summary>
  public class NodeInfo
  {
    public string Ip { get; set; }
    public string Name { get; set; }
  }

  /// <summary>
  /// Value converter that takes a <see cref="NodeKind"/> and returns a DataTemplate suitable for a ContentPresenter.
  /// </summary>
  public class NodeKindToTemplateConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var kind = (NodeKind) value;
      var templateName = string.Format("{0}Template", kind);
      var template = Application.Current.Resources[templateName];
      return template;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
  }

  /// <summary>
  /// Value converter to convert a number between 0 and 1 to a color between green and red.
  /// </summary>
  public class LoadToBrushConverter : IValueConverter
  {
    private readonly List<Color> colors;

    public LoadToBrushConverter() {
      colors = new List<Color>
      {
        Colors.LimeGreen,
        Colors.DarkOliveGreen,
        Colors.Orange,
        Colors.Red
      };
    }

    private static Color Interpolate(List<Color> colors, float position) {
      if (position <= 0) {
        return colors[0];
      }
      if (position >= 1) {
        return colors[colors.Count - 1];
      }
      if (colors.Count == 1) {
        return colors[0];
      }

      var index = (colors.Count - 1) * position;
      var left = colors[(int) Math.Floor(index)];
      var right = colors[(int) Math.Ceiling(index)];
      var between = index - (float) Math.Truncate(index);

      Func<float, float, float> adj = (a, b) => a + (b - a) * between;

      return new Color
      {
        ScA = adj(left.ScA, right.ScA),
        ScR = adj(left.ScR, right.ScR),
        ScG = adj(left.ScG, right.ScG),
        ScB = adj(left.ScB, right.ScB)
      };
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (!(value is double)) {
        return null;
      }
      var val = (float) (double) value;
      return new SolidColorBrush(Interpolate(colors, val));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
  }

  /// <summary>
  /// Small class containing extension methods we use in this demo.
  /// </summary>
  public static class NetworkMonitoringExtensions
  {
    /// <summary>
    /// Shuffles an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to shuffle.</param>
    /// <param name="rng">A <see cref="Random"/> instance that is used to shuffle <paramref name="source"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the shuffled items from <paramref name="source"/>.</returns>
    public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source, Random rng) {
      var list = source.ToList();

      for (int i = list.Count; i > 1; i--) {
        var k = rng.Next(i);
        TSource tmp = list[i - 1];
        list[i - 1] = list[k];
        list[k] = tmp;
      }

      return list;
    }
  }
}