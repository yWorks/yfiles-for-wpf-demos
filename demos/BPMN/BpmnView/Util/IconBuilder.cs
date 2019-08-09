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
using System.Collections.Generic;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;

namespace Demo.yFiles.Graph.Bpmn.Util {

  /// <summary>
  /// Builder class to create <see cref="IIcon"/>s.
  /// </summary>
  internal class IconBuilder
  {
    private GeneralPath path;

    private GeneralPath Path {
      get { return path ?? (path = new GeneralPath()); }
      set { path = value; } 
    }

    public Pen Pen { get; set; }
    public Brush Brush { get; set; }

    public IconBuilder() {
      Clear();
    }

    public void MoveTo(double x, double y) {
      Path.MoveTo(x, y);
    }

    public void LineTo(double x, double y) {
      Path.LineTo(x, y);
    }

    public void QuadTo(double cx, double cy, double x, double y) {
      Path.QuadTo(cx, cy, x, y);
    }

    public void CubicTo(double c1x, double c1y, double c2x, double c2y, double x, double y) {
      Path.CubicTo(c1x, c1y, c2x, c2y, x, y);
    }

    public void ArcTo(double r, double cx, double cy, double fromAngle, double toAngle) {
      var a = (toAngle - fromAngle) / 2.0;
      var sgn = a < 0 ? -1 : 1;
      if (Math.Abs(a) > Math.PI/4) {
        // bigger then a quarter circle -> split into multiple arcs
        var start = fromAngle;
        var end = fromAngle + sgn * Math.PI/2;
        while (sgn * end < sgn * toAngle) {
          ArcTo(r, cx, cy, start, end);
          start = end;
          end += sgn * Math.PI/2;
        }
        ArcTo(r, cx, cy, start, toAngle);
        return;
      }

      // calculate unrotated control points
      var x1 = r * Math.Cos(a);
      var y1 = -r * Math.Sin(a);

      var m = (Math.Sqrt(2) - 1) * 4 / 3;
      var mTanA = m * Math.Tan(a);

      var x2 = x1 - mTanA * y1;
      var y2 = y1 + mTanA * x1;
      var x3 = x2;
      var y3 = -y2;

      // rotate the control points by (fromAngle + a)
      var rot = fromAngle + a;
      var sinRot = Math.Sin(rot);
      var cosRot = Math.Cos(rot);

      Path.CubicTo(cx + x2 * cosRot - y2 * sinRot,
                    cy + x2 * sinRot + y2 * cosRot,
                    cx + x3 * cosRot - y3 * sinRot,
                    cy + x3 * sinRot + y3 * cosRot,
                    cx + r * Math.Cos(toAngle),
                    cy + r * Math.Sin(toAngle));
    }

    public IIcon CreateEllipseIcon() {
      Path.AppendEllipse(new RectD(0, 0, 1, 1), false);
      return GetPathIcon();
    }

    public void Close() {
      Path.Close();
    }

    public IIcon CombineIcons(IList<IIcon> icons) {
      var icon = new CombinedIcon(icons);
      Clear();
      return icon;
    }

    public IIcon CreateLineUpIcon(IList<IIcon> icons, SizeD innerIconSize, double gap) {
      var icon = new LineUpIcon(icons, innerIconSize, gap);
      Clear();
      return icon;
    }

    public IIcon GetPathIcon() {
      IIcon icon = new PathIcon { Path = path, Pen = Pen, Brush = Brush};
      Clear();
      return icon;
    }

    public IIcon CreateRectIcon(double cornerRadius) {
      var rectIcon = new RectIcon { Pen = Pen, Brush = Brush, CornerRadius = cornerRadius };
      Clear();
      return rectIcon;
    }

    public IIcon CreateRectIcon(double topLeftRadius, double topRightRadius, double bottomLeftRadius, double bottomRightRadius) {
      var rectIcon = new VariableRectIcon
      {
        Pen = Pen, 
        Brush = Brush, 
        TopLeftRadius = topLeftRadius, 
        TopRightRadius = topRightRadius, 
        BottomLeftRadius = bottomLeftRadius, 
        BottomRightRadius = bottomRightRadius
      };
      Clear();
      return rectIcon;
    }


    public void Clear() {
      Pen = Pens.Black;
      Brush = null;
      Path = null;
    }
  }
}
