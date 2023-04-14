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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Demo.yFiles.Complete.IsometricDrawing.Model;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Complete.IsometricDrawing
{
  /// <summary>
  /// A node style that visualizes the node as block in an isometric fashion.
  /// </summary>
  public class IsometricNodeStyle : NodeStyleBase<VisualGroup>
  {
    /// <summary>
    /// Returns a darker shade fill of the given fill.
    /// </summary>
    /// <param name="fill">The base fill.</param>
    /// <returns>A darker shade fill of the <paramref name="fill"/>.</returns>
    public static SolidColorBrush Darker(SolidColorBrush fill) {
      var oldColor = fill.Color;
      var factor = 0.7f;
      var newColor = oldColor * factor;
      return new SolidColorBrush(Color.FromArgb(oldColor.A, newColor.R, newColor.G, newColor.B));
    }
    
    /// <summary>
    /// Calculates a vector in world coordinates whose transformation by the <paramref name="projection"/> results
    /// in the vector (0, -1).
    /// </summary>
    /// <param name="projection">The projection to consider.</param>
    /// <returns>The vector in world coordinates that gets transformed to the vector (0, -1).</returns>
    public static PointD CalculateHeightVector(Transform projection) {
      return projection.Inverse.Transform(new Point(0d, -1d));
    }
    
    #region Create/UpdateVisual

    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      var faces = new VisualGroup();
      faces.Children.Add(new Path());
      faces.Children.Add(new Path());
      faces.Children.Add(new Path());
      return UpdateVisual(context, faces, node);
    }

    /// <summary>
    /// The render data class used to check whether the visuals have to be updated.
    /// </summary>
    private sealed class NodeRenderData
    {
      public readonly double X;
      public readonly double Y;
      public readonly double Width;
      public readonly double Depth;
      public readonly double Height;
      public readonly Color Color;
      public readonly Transform Projection;

      public NodeRenderData(double x, double y, double width, double depth, double height, Color color, Transform projection) {
        X = x;
        Y = y;
        Width = width;
        Depth = depth;
        Height = height;
        Color = color;
        Projection = projection;
      }

      private bool Equals(NodeRenderData other) {
        return other.X == X && other.Y == Y && other.Width == Width && other.Height == Height &&
               other.Depth == Depth && other.Color == Color && other.Projection == Projection;
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
          return false;
        }
        if (obj.GetType() != typeof(NodeRenderData)) {
          return false;
        }
        return Equals((NodeRenderData)obj);
      }
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup faces, INode node) {
      // create faces
      var nodeData = (NodeData) node.Tag;
      var height = nodeData.Geometry.Height;
      var layout = node.Layout;

      var oldCache = faces.GetRenderDataCache<NodeRenderData>();
      var newCache = new NodeRenderData(layout.X, layout.Y, layout.Width, layout.Height, height, nodeData.Color, context.Projection);
      
      if (!newCache.Equals(oldCache)) {
        faces.SetRenderDataCache(newCache);
        
        var corners = CalculateCorners(context.Projection, layout.X, layout.Y, layout.Width, layout.Height, height);
        SolidColorBrush topBrush = null;
        SolidColorBrush leftBrush = null;
        SolidColorBrush rightBrush = null;
        var pen = nodeData.Pen;
        var brush = nodeData.Brush;
        if (brush != null) {
          topBrush = brush;
          if (height > 0) {
            leftBrush = Darker(brush);
            rightBrush = Darker(leftBrush);
          }
        }
        
        if (height == 0 && faces.Children.Count > 1) {
          while (faces.Children.Count > 1) {
            faces.Children.RemoveAt(faces.Children.Count - 1);
          }
        } else if (height > 0) {
          // check which of the left, right, back and front faces are visible using the current projection
          var upVector = CalculateHeightVector(context.Projection);
          var useLeft = upVector.X > 0; 
          var useBack = upVector.Y > 0; 

          var leftFacePath = useLeft ? GetLeftFacePath(corners) : GetRightFacePath(corners);
          var rightFacePath = useBack ? GetBackFacePath(corners) : GetFrontFacePath(corners);
          if (faces.Children.Count == 1) {
            var faceLeft = leftFacePath.CreatePath(leftBrush, pen, null, FillMode.Always);
            faces.Children.Insert(0, faceLeft);
            var faceRight = rightFacePath.CreatePath(rightBrush, pen, null, FillMode.Always);
            faces.Children.Insert(1, faceRight);
          } else {
            var faceLeft = faces.Children[0] as Path;
            leftFacePath.UpdatePath(faceLeft, leftBrush, pen, null, FillMode.Always);
            var faceRight = faces.Children[1] as Path;
            rightFacePath.UpdatePath(faceRight, rightBrush, pen, null, FillMode.Always);
          }
        }
        var topFacePath = GetTopFacePath(corners);
        var faceTop = faces.Children.Last() as Path;
        topFacePath.UpdatePath(faceTop, topBrush, pen, null, FillMode.Always);
      }
      return faces;
    }
    
    #endregion
    
    #region Face Path Factories

    /// <summary>
    /// Creates a <see cref="GeneralPath"/> that describes the face on the top of the block.
    /// </summary>
    /// <param name="corners">The coordinates of the corners of the block.</param>
    /// <returns>A <see cref="GeneralPath"/> that describes the face on the top of the block.</returns>
    static GeneralPath GetTopFacePath(double[] corners) {
      var path = new GeneralPath();
      path.MoveTo(
          corners[UpTopLeftX],
          corners[UpTopLeftY]
      );
      path.LineTo(
          corners[UpTopRightX],
          corners[UpTopRightY]
      );
      path.LineTo(
          corners[UpBottomRightX],
          corners[UpBottomRightY]
      );
      path.LineTo(
          corners[UpBottomLeftX],
          corners[UpBottomLeftY]
      );
      path.Close();
      return path;
    }

    /// <summary>
    /// Creates a <see cref="GeneralPath"/> that describes the face on the left side of the block.
    /// </summary>
    /// <param name="corners">The coordinates of the corners of the block.</param>
    /// <returns>A <see cref="GeneralPath"/> that describes the face on the left side of the block.</returns>
    static GeneralPath GetLeftFacePath(double[] corners) {
      var path = new GeneralPath();
      path.MoveTo(
          corners[LowTopLeftX],
          corners[LowTopLeftY]
      );
      path.LineTo(
          corners[UpTopLeftX],
          corners[UpTopLeftY]
      );
      path.LineTo(
          corners[UpBottomLeftX],
          corners[UpBottomLeftY]
      );
      path.LineTo(
          corners[LowBottomLeftX],
          corners[LowBottomLeftY]
      );
      path.Close();
      return path;
    }
    /// <summary>
    /// Creates a <see cref="GeneralPath"/> that describes the face on the right side of the block.
    /// </summary>
    /// <param name="corners">The coordinates of the corners of the block.</param>
    /// <returns>A <see cref="GeneralPath"/> that describes the face on the right side of the block.</returns>
    static GeneralPath GetRightFacePath(double[] corners) {
      var path = new GeneralPath();
      path.MoveTo(
          corners[LowTopRightX],
          corners[LowTopRightY]
      );
      path.LineTo(
          corners[UpTopRightX],
          corners[UpTopRightY]
      );
      path.LineTo(
          corners[UpBottomRightX],
          corners[UpBottomRightY]
      );
      path.LineTo(
          corners[LowBottomRightX],
          corners[LowBottomRightY]
      );
      path.Close();
      return path;
    }

    /// <summary>
    /// Creates a <see cref="GeneralPath"/> that describes the face on the front side of the block.
    /// </summary>
    /// <param name="corners">The coordinates of the corners of the block.</param>
    /// <returns>A <see cref="GeneralPath"/> that describes the face on the front side of the block.</returns>
    static GeneralPath GetFrontFacePath(double[] corners) {
      var path = new GeneralPath();
      path.MoveTo(
          corners[LowBottomLeftX],
          corners[LowBottomLeftY]
      );
      path.LineTo(
          corners[UpBottomLeftX],
          corners[UpBottomLeftY]
      );
      path.LineTo(
          corners[UpBottomRightX],
          corners[UpBottomRightY]
      );
      path.LineTo(
          corners[LowBottomRightX],
          corners[LowBottomRightY]
      );
      path.Close();
      return path;
    }
    
    /// <summary>
    /// Creates a <see cref="GeneralPath"/> that describes the face on the back side of the block.
    /// </summary>
    /// <param name="corners">The coordinates of the corners of the block.</param>
    /// <returns>A <see cref="GeneralPath"/> that describes the face on the back side of the block.</returns>
    static GeneralPath GetBackFacePath(double[] corners) {
      var path = new GeneralPath();
      path.MoveTo(
          corners[LowTopLeftX],
          corners[LowTopLeftY]
      );
      path.LineTo(
          corners[UpTopLeftX],
          corners[UpTopLeftY]
      );
      path.LineTo(
          corners[UpTopRightX],
          corners[UpTopRightY]
      );
      path.LineTo(
          corners[LowTopRightX],
          corners[LowTopRightY]
      );
      path.Close();
      return path;
    }
    
    // Indices for the corners of the bounding box.
    private const int LowTopLeftX = 0;
    private const int LowTopLeftY = 1;
    private const int LowTopRightX = 2;
    private const int LowTopRightY = 3;
    private const int LowBottomRightX = 4;
    private const int LowBottomRightY = 5;
    private const int LowBottomLeftX = 6;
    private const int LowBottomLeftY = 7;
    private const int UpTopLeftX = 8;
    private const int UpTopLeftY = 9;
    private const int UpTopRightX = 10;
    private const int UpTopRightY = 11;
    private const int UpBottomRightX = 12;
    private const int UpBottomRightY = 13;
    private const int UpBottomLeftX = 14;
    private const int UpBottomLeftY = 15;

    private static double[] CalculateCorners(Transform projection, double x, double y, double width, double depth, double height) {
      var heightVec = height * CalculateHeightVector(projection);

      var corners = new double[16];
      corners[LowTopLeftX] = x;
      corners[LowTopLeftY] = y;

      corners[LowTopRightX] = x + width;
      corners[LowTopRightY] = y;

      corners[LowBottomRightX] = x + width;
      corners[LowBottomRightY] = y + depth;

      corners[LowBottomLeftX] = x;
      corners[LowBottomLeftY] = y + depth;

      for (var i = 0; i < 8; i += 2) {
        corners[i + 8] = corners[i] + heightVec.X;
        corners[i + 9] = corners[i + 1] + heightVec.Y;
      }
      return corners;
    }

    #endregion

    protected override RectD GetBounds(ICanvasContext context, INode node) {
      var layout = node.Layout;
      var nodeData = node.Tag as NodeData;
      if (nodeData == null) {
        return layout.ToRectD();
      }
      var corners = CalculateCorners(context.CanvasControl.Projection, layout.X, layout.Y, layout.Width, layout.Height, nodeData.Geometry.Height);

      double minX = Double.MaxValue;
      double minY = Double.MaxValue;
      double maxX = Double.MinValue;
      double maxY = Double.MinValue;
      for (var i = 0; i < corners.Length; i += 2) {
        minX = Math.Min(minX, corners[i]);
        maxX = Math.Max(maxX, corners[i]);
        minY = Math.Min(minY, corners[i + 1]);
        maxY = Math.Max(maxY, corners[i + 1]);
      }
      return new RectD(minX, minY, maxX - minX, maxY - minY);
    }
  }
}
