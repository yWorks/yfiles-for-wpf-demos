/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Demo.yFiles.Graph.LargeGraphs.Styles.LevelOfDetail;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using Point = System.Windows.Point;

namespace Demo.yFiles.Graph.LargeGraphs.Styles.Fast
{
  /// <summary>
  ///   A faster label style.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Unlike <see cref="LevelOfDetailLabelStyle" /> this style doesn't keep full fidelity of the visualization. Instead
  ///     it approximates the label's appearance by drawing just a broken line where words are. The intention is to not
  ///     render text at zoom levels where it wouldn't be legible at all. However, using this style for very small zoom
  ///     levels is still not recommended. Using a <see cref="VoidLabelStyle" /> that uses a <see cref="FastEdgeStyle" />
  ///     below a certain point is much more reasonable.
  ///   </para>
  /// </remarks>
  public class FastLabelStyle : LabelStyleBase<Path>
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="FastLabelStyle" /> class with the given auto-flip setting.
    /// </summary>
    /// <param name="autoFlip">Whether to flip the label automatically depending on its orientation.</param>
    public FastLabelStyle(AutoFlipMode autoFlip) {
      AutoFlip = autoFlip == AutoFlipMode.AutoFlip;
      BackgroundBrush = null;
      Stroke = new Pen(Brushes.Black, 5);
      Stroke.Freeze();
    }

    /// <summary>
    ///   Gets or sets a value indicating whether the label automatically flips depending on the orientation so that it stays
    ///   upright even when rotated upside-down.
    /// </summary>
    public bool AutoFlip { get; set; }

    /// <summary>
    ///   Gets or sets the brush to paint the label's background with.
    /// </summary>
    public Brush BackgroundBrush { get; set; }

    /// <summary>
    ///   Gets or sets the foreground brush.
    /// </summary>
    public Pen Stroke { get; set; }

    #region Style

    /// <inheritdoc />
    protected override Path CreateVisual(IRenderContext context, ILabel label) {
      var layout = label.GetLayout();

      // ReSharper disable once UseObjectOrCollectionInitializer
      var path = new Path
      {
        Data = CreateGeometry(layout, label.Text),
        Stroke = Stroke.Brush,
        StrokeThickness = Stroke.Thickness,
        Fill = BackgroundBrush
      };

      // Hack to work around WPF bug. Otherwise the path may not appear at the correct location in some cases.
      path.ClipToBounds = false;
      path.Stretch = Stretch.None;
      path.SetCanvasArrangeRect(new Rect(0, 0, double.MaxValue, double.MaxValue));
      path.MinWidth = path.MinHeight = 1;

      path.SetRenderDataCache(new OrientedRectangle(layout));

      return path;
    }

    /// <inheritdoc />
    protected override Path UpdateVisual(IRenderContext context, Path oldVisual, ILabel label) {
      var layout = label.GetLayout();
      var oldLayout = oldVisual.GetRenderDataCache<OrientedRectangle>();

      // Did the layout change? In that case we have to re-create the geometry
      if (!AreEqual(layout, oldLayout)) {
        oldVisual.Data = CreateGeometry(layout, label.Text);
        oldVisual.SetRenderDataCache(new OrientedRectangle(layout));
      }

      return oldVisual;
    }

    /// <inheritdoc />
    protected override SizeD GetPreferredSize(ILabel label) {
      return label.GetLayout().ToSizeD();
    }

    #endregion

    #region Helper methods

    /// <summary>
    ///   Creates the geometry for the label's path.
    /// </summary>
    /// <param name="layout">The label's layout.</param>
    /// <param name="text">The label text.</param>
    /// <returns>The label path's geometry.</returns>
    /// <remarks>
    ///   The label's visualization is an approximation of its text where each word is represented by a line with a length
    ///   proportional to the word's length. In the vast majority of cases this approximates the position of spaces in the line
    ///   accurately enough that the switch from a text label to a fast label is almost imperceptible.
    /// </remarks>
    private StreamGeometry CreateGeometry(IOrientedRectangle layout, string text) {
      var origin = layout.GetAnchorLocation();
      var up = layout.GetUp();
      var right = new PointD(-up.Y, up.X);

      if (AutoFlip && up.Y > 0) {
        origin = origin + up * layout.Height + right * layout.Width;
        up = -up;
        right = -right;
      }

      var upperLeft = origin + up * layout.Height;

      // This part could be optimized a bit by not allocating new objects and thus reduce pressure on the garbage
      // collector. However, in practice it made not much of an impact.
      var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      var longestLineLength = lines.Max(l => l.Length);
      var sizePerLetter = layout.Width / longestLineLength;

      var geometry = new StreamGeometry();
      using (var ctx = geometry.Open()) {
        // Fill – only drawn when a brush was set
        if (BackgroundBrush != null) {
          ctx.BeginFigure(origin, true, false);
          ctx.PolyLineTo(new Point[]
          {
            origin + right * layout.Width,
            origin + right * layout.Width + up * layout.Height,
            origin + up * layout.Height
          }, false, false);
        }

        // Lines
        for (int i = 0; i < lines.Length; i++) {
          var line = lines[i];
          var words = line.Split(' ', ' ');
          var currentPoint = upperLeft - up * layout.Height / lines.Length * (i + 0.6);
          // Words
          foreach (var word in words) {
            var wordLength = word.Length * sizePerLetter;
            var targetPoint = currentPoint + right * wordLength;

            ctx.BeginFigure(currentPoint, false, false);
            ctx.LineTo(targetPoint, true, false);

            currentPoint = targetPoint + right * sizePerLetter;
          }
        }
      }
      return geometry;
    }

    /// <summary>
    ///   Helper method to determine whether two <see cref="IOrientedRectangle" />s are equal.
    /// </summary>
    /// <param name="rect1">The first rectangle to compare.</param>
    /// <param name="rect2">The second rectangle to compare.</param>
    /// <returns><see langword="true" /> if both rectangles are equal, <see langword="false" /> otherwise.</returns>
    /// <remarks>
    ///   Two oriented rectangles are considered equal if their anchor location, up vector, width, and height are equal.
    /// </remarks>
    private static bool AreEqual(IOrientedRectangle rect1, IOrientedRectangle rect2) {
      // ReSharper disable CompareOfFloatsByEqualityOperator
      return rect1.AnchorX == rect2.AnchorX &&
             rect1.AnchorY == rect2.AnchorY &
             rect1.UpX == rect2.UpX &&
             rect1.UpY == rect2.UpY &&
             rect1.Width == rect2.Width &&
             rect1.Height == rect2.Height;
      // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    #endregion
  }

  /// <summary>
  ///   Enumeration for the auto-flip setting in label styles.
  /// </summary>
  public enum AutoFlipMode
  {
    /// <summary>
    ///   Don't flip the label depending on its orientation.
    /// </summary>
    DontFlip = 0,

    /// <summary>
    ///   Automatically flip the label depending on its orientation so that text is not upside-down.
    /// </summary>
    AutoFlip = 1
  }
}