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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>Provides drawing resources and geometry helpers shared by Biofabric visuals.</summary>
  internal static class BiofabricRenderUtils
  {
    /// <summary>The background brush used behind group markers.</summary>
    public static readonly Brush DefaultBackground = Brushes.White;

    /// <summary>The default brush used for edges and groups without an assigned color.</summary>
    public static readonly Brush DefaultEdge = CreateBrush("#dfdee3");

    /// <summary>The brush used to highlight graph items and groups.</summary>
    public static readonly Brush DefaultHighlight = CreateBrush("#d9bb7d");

    /// <summary>The brush used for label backgrounds.</summary>
    public static readonly Brush DefaultLabelBackground = CreateBrush("#202739");

    /// <summary>Creates a solid brush from a color string, returning a fallback for invalid or missing colors.</summary>
    /// <param name="color">The color string to parse.</param>
    /// <param name="fallback">The brush to return when the color is missing or invalid.</param>
    /// <returns>A frozen brush for <paramref name="color"/>, or the fallback brush.</returns>
    public static Brush CreateBrush(string? color, Brush? fallback = null) {
      if (string.IsNullOrWhiteSpace(color)) {
        return fallback ?? Brushes.Transparent;
      }
      try {
        var brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(color)!);
        brush.Freeze();
        return brush;
      } catch (FormatException) {
        return fallback ?? Brushes.Transparent;
      }
    }

    /// <summary>Creates a frozen pen with the specified brush and thickness.</summary>
    /// <param name="brush">The pen brush.</param>
    /// <param name="thickness">The pen thickness.</param>
    /// <returns>The frozen pen.</returns>
    public static Pen CreatePen(Brush brush, double thickness) {
      var pen = new Pen(brush, thickness);
      pen.Freeze();
      return pen;
    }

    /// <summary>Creates formatted text with the Biofabric's shared typeface and direction.</summary>
    /// <param name="text">The text to format.</param>
    /// <param name="fontSize">The font size in device-independent units.</param>
    /// <param name="brush">The text brush.</param>
    /// <returns>The formatted text.</returns>
    public static FormattedText FormatText(string text, double fontSize, Brush brush) =>
        new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Sans Serif"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            fontSize, brush, 1.0);

    /// <summary>Calculates the union of the rendered bounds of a set of nodes.</summary>
    /// <param name="nodes">The nodes whose bounds are combined.</param>
    /// <param name="context">The canvas context used by node bounds providers.</param>
    /// <returns>The union bounds, or an empty rectangle when <paramref name="nodes"/> is empty.</returns>
    public static RectD BoundsOfNodes(IEnumerable<INode> nodes, ICanvasContext context) {
      var found = false;
      var minX = double.PositiveInfinity;
      var minY = double.PositiveInfinity;
      var maxX = double.NegativeInfinity;
      var maxY = double.NegativeInfinity;
      foreach (var node in nodes) {
        var boundsProvider = node.Lookup<IBoundsProvider>();
        var bounds = boundsProvider?.GetBounds(context) ?? node.Layout;
        minX = Math.Min(minX, bounds.X);
        minY = Math.Min(minY, bounds.Y);
        maxX = Math.Max(maxX, bounds.GetMaxX());
        maxY = Math.Max(maxY, bounds.GetMaxY());
        found = true;
      }
      return found ? new RectD(minX, minY, maxX - minX, maxY - minY) : new RectD(0, 0, 0, 0);
    }

    /// <summary>
    ///   Creates a semi-transparent version of the given brush.
    /// </summary>
    public static Brush CreateTransparent(Brush color) {
      if (color is SolidColorBrush scb) {
        return new SolidColorBrush(Color.FromArgb(0x20, scb.Color.R, scb.Color.G, scb.Color.B));
      }
      return color;
    }
  }
}
