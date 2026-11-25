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
using yWorks.Geometry;
using yWorks.Graph.Styles;
using ShapeNodeShape = yWorks.Demo.GraphMLCompat.Styles.ShapeNodeShape;

namespace yWorks.Demo.GraphViewer.Xaml
{
  internal static class ShapeNodeStyles
  {
    private static readonly double Star5IntrinsicAspectRatio = 1.0514622;
    private static readonly GeneralPath Trapez = CreateShearedShape(0.2, false, false, true, true);
    private static readonly GeneralPath Trapez2 = CreateShearedShape(0.2, true, true, false, false);
    private static readonly GeneralPath FatArrow = CreateFatArrow();
    private static readonly GeneralPath FatArrow2 = CreateFatArrow2();
    private static readonly GeneralPath ShearedRectangle = CreateShearedShape(0.2, true, false, true, false);
    private static readonly GeneralPath ShearedRectangle2 = CreateShearedShape(0.2, false, true, false, true);
    private static readonly GeneralPath Star5 = CreateStarPath(5, Math.PI, false);
    private static readonly GeneralPath IntrinsicStar5 = CreateStarPath(5, Math.PI, true);
    private static readonly GeneralPath Star5Up = CreateStarPath(5, 0, false);
    private static readonly GeneralPath IntrinsicStar5Up = CreateStarPath(5, 0, true);
    private static readonly GeneralPath Thing = CreateThing();

    internal static GeneralPath GetPath(ShapeNodeShape shape, bool keepIntrinsicAspectRatio) {
      switch (shape) {
        case ShapeNodeShape.Trapez:
          return Trapez;
        case ShapeNodeShape.Trapez2:
          return Trapez2;
        case ShapeNodeShape.FatArrow:
          return FatArrow;
        case ShapeNodeShape.FatArrow2:
          return FatArrow2;
        case ShapeNodeShape.ShearedRectangle:
          return ShearedRectangle;
        case ShapeNodeShape.ShearedRectangle2:
          return ShearedRectangle2;
        case ShapeNodeShape.Star5:
          return keepIntrinsicAspectRatio ? IntrinsicStar5 : Star5;
        case ShapeNodeShape.Star5Up:
          return keepIntrinsicAspectRatio ? IntrinsicStar5Up : Star5Up;
        default:
          return Thing;
      }
    }

    public static double GetIntrinsicAspectRatio(ShapeNodeShape shape) {
      switch (shape) {
        case ShapeNodeShape.Trapez:
        case ShapeNodeShape.Trapez2:
        case ShapeNodeShape.FatArrow:
        case ShapeNodeShape.FatArrow2:
        case ShapeNodeShape.ShearedRectangle:
        case ShapeNodeShape.ShearedRectangle2:
          return 2;
        case ShapeNodeShape.Star5:
        case ShapeNodeShape.Star5Up:
          return Star5IntrinsicAspectRatio;
        default:
          return 1;
      }
    }

    private static GeneralPath CreateThing() {
      var path = new GeneralPath(6);
      path.MoveTo(0, 0.2);
      path.QuadTo(0.5, -0.2, 1, 0.2);
      path.LineTo(1, 0.8);
      path.QuadTo(0.5, 1.2, 0, 0.8);
      path.Close();

      return path;
    }

    private static GeneralPath CreateShearedShape(
        double xRatio,
        bool useXRatioTopLeft,
        bool useXRatioTopRight,
        bool useXRatioBottomRight,
        bool useXRatioBottomLeft) {
      var x = 0;
      var y = 0;
      var width = 1;
      var height = 1;
      var maxX = x + width;
      var maxY = y + height;
      var xLeftRatio = x + xRatio * width;
      var xRightRatio = maxX - xRatio * width;
      var slope = height / (width * xRatio);

      var topLeftX = useXRatioTopLeft ? xLeftRatio : x;
      var topRightX = useXRatioTopRight ? xRightRatio : maxX;
      var bottomRightX = useXRatioBottomRight ? xRightRatio : maxX;
      var bottomLeftX = useXRatioBottomLeft ? xLeftRatio : x;

      var noHorizontalLineTop = topLeftX >= topRightX;
      var noHorizontalLineBottom = bottomLeftX >= bottomRightX;

      var path = new GeneralPath(4);
      if (noHorizontalLineTop && noHorizontalLineBottom) {
        // use some empty path as returning null would switch unexpectedly to non-shape wrapping code
        path.MoveTo((topLeftX + topRightX) * 0.5, y);
        path.LineTo((bottomLeftX + bottomRightX) * 0.5, maxY);
      } else if (noHorizontalLineTop) {
        // triangle with valid bottom line
        var dx = (bottomRightX - bottomLeftX) * 0.5;
        var dy = dx * slope;
        path.MoveTo(bottomLeftX + dx, maxY - dy);
        path.LineTo(bottomRightX, maxY);
        path.LineTo(bottomLeftX, maxY);
      } else if (noHorizontalLineBottom) {
        // triangle with valid top line
        var dx = (topRightX - topLeftX) * 0.5;
        var dy = dx * slope;
        path.MoveTo(topLeftX, y);
        path.LineTo(topRightX, y);
        path.LineTo(topLeftX + dx, y + dy);
      } else {
        path.MoveTo(topLeftX, y);
        path.LineTo(topRightX, y);
        path.LineTo(bottomRightX, maxY);
        path.LineTo(bottomLeftX, maxY);
      }

      path.Close();
      return path;
    }

    private static GeneralPath CreateFatArrow2() {
      var path = new GeneralPath(6);
      path.MoveTo(0.1, 0);
      path.LineTo(1, 0);
      path.LineTo(0.9, 0.5);
      path.LineTo(1, 1);
      path.LineTo(0.1, 1);
      path.LineTo(0, 0.5);
      path.Close();

      return path;
    }

    private static GeneralPath CreateFatArrow() {
      var path = new GeneralPath(6);
      path.MoveTo(0, 0);
      path.LineTo(0.9, 0);
      path.LineTo(1, 0.5);
      path.LineTo(0.9, 1);
      path.LineTo(0, 1);
      path.LineTo(0.1, 0.5);
      path.Close();

      return path;
    }

    private static GeneralPath CreateStarPath(int pointCount, double initialAngle, bool keepIntrinsicAspectRatio) {
      var path = GeneralPathNodeStyle.CreateStarPath(pointCount, 0.6, initialAngle);
      return keepIntrinsicAspectRatio ? GeneralPathNodeStyle.CreateAspectRatioInstance(path).Path : path;
    }
  }
}
