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
using System.Reflection;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml {
  /// <summary>
  /// A label model for edge labels that uses a ratio on the edge's path
  /// to determine the position of the label.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This model allows for specifying the angle between the edge's path and the
  /// label's baseline.
  /// </para>
  /// <para>
  /// Note that <see cref="RotatedSliderEdgeLabelModel"/> and <see cref="RotatedSideSliderEdgeLabelModel"/>
  /// usually are a better alternative since they provide a continuous set of label candidates.
  /// </para>
  /// </remarks>
  /// <seealso cref="RotatedSliderEdgeLabelModel"/>
  /// <seealso cref="RotatedSideSliderEdgeLabelModel"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class RotatingEdgeLabelModel : ILabelModel, ILabelModelParameterProvider
  {
    #region Distance property

    /// <summary>
    /// Gets or sets a property that determines the distance between the label's center
    /// and the anchor point on the edge's path.
    /// </summary>
    /// <remarks>
    /// A distance of <c>0</c> makes the label appear centered on the edge's path.
    /// Depending on the value of the <see cref="EdgeRelativeDistance"/>
    /// property, the distance is interpreted differently:
    /// If the distance is interpreted relatively, a positive distance
    /// makes the label appear at the left hand of the edge. If
    /// the distance is interpreted absolutely, positive values make the label
    /// appear on top of the edge's path, while negative values make it appear
    /// below the path.
    /// </remarks>
    public double Distance { get; set; }

    #endregion

    #region EdgeRelativeDistance property

    /// <summary>
    /// Gets or sets a property that determines how the
    /// <see cref="Distance"/> value should be interpreted.
    /// </summary>
    /// <remarks>
    /// If the distance is interpreted relatively, a positive distance
    /// makes the label appear at the left hand of the edge. If
    /// the distance is interpreted absolutely, positive values make the label
    /// appear on top of the edge's path, while negative values make it appear
    /// below the path.
    /// </remarks>
    public bool EdgeRelativeDistance { get; set; }

    #endregion

    #region Angle property

    public RotatingEdgeLabelModel() {
      EdgeRelativeDistance = true;
    }

    /// <summary>
    /// Gets or sets the angle of the label relative to the edge's path.
    /// </summary>
    /// <remarks>
    /// An angle of <c>0</c> makes the label appear in parallel to the edge's path,
    /// whereas a positive angle rotates the label counter-clockwise away from the edge's path.
    /// </remarks>
    public double Angle { get; set; }

    #endregion

    /// <inheritdoc/>
    public object Lookup(Type type) {
      if (type == typeof(ILabelModelParameterProvider)) {
        return this;
      }
      if (type == typeof(ILabelModelParameterFinder)) {
        return DefaultLabelModelParameterFinder.Instance;
      }
      return null;
    }

    /// <inheritdoc/>
    public virtual ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return Lookups.Empty;
    }

    /// <summary>
    /// Returns possible parameters for the given label and model.
    /// </summary>
    /// <param name="label">The label for which to retrieve the parameters</param>
    /// <param name="model"> must be <c>this</c> or at least of this type.</param>
    public virtual IEnumerable<ILabelModelParameter> GetParameters(ILabel label, ILabelModel model) {
      RotatingEdgeLabelModel rotatingEdgeLabelModel = (RotatingEdgeLabelModel) model;
      List<ILabelModelParameter> candidates = new List<ILabelModelParameter>();
      for (int i = 0; i <= 10; i++) {
        candidates.Add(new RatioParameter(rotatingEdgeLabelModel, i * 0.1));
      }
      return candidates;
    }

    /// <inheritdoc/>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      OrientedRectangle rect = new OrientedRectangle(0, 0, 10, 10);
      ((RatioParameter) parameter).SetGeometry(this, label, rect);
      return rect;
    }

    /// <summary>
    /// Creates a default parameter for this model.
    /// </summary>
    /// <remarks>
    /// This method creates a parameter that displays the label at the center of the edge path.
    /// </remarks>
    /// <returns>A parameter like in <see cref="CreateRatio"/> with a ratio of <c>0.5d</c>.</returns>
    public ILabelModelParameter CreateDefaultParameter() {
      return CreateRatio(0.5);
    }

    /// <summary>
    /// Creates a parameter for this model using a ratio value between <c>0.0d</c>
    /// and <c>1.0d</c>.
    /// </summary>
    /// <param name="ratio">The ratio where the label should be placed along the edge's path.</param>
    /// <returns>A parameter that uses this model instance.</returns>
    public ILabelModelParameter CreateRatio(double ratio) {
      return new RatioParameter(this, ratio);
    }

    private class RatioParameter : ILabelModelParameter, IMarkupExtensionConverter
    {
      private readonly double ratio;
      private readonly RotatingEdgeLabelModel model;

      public RatioParameter(RotatingEdgeLabelModel model, double r) {
        this.ratio = r;
        this.model = model;
      }

      public bool Supports(ILabel label) {
        return label.Owner is IEdge;
      }

      public object Clone() {
        return this;
      }

      public ILabelModel Model {
        get { return model; }
      }


      public void SetGeometry(RotatingEdgeLabelModel model, ILabel label, IMutableOrientedRectangle rect) {
        IEdge edge = (IEdge) label.Owner;
        double upX;
        double upY;
        double cx;
        double cy;
        if (edge != null && FindAnchorTangent(edge, out upX, out upY, out cx, out cy)) {
          double newAngle = Math.Atan2(upX, -upY) + model.Angle;

          double distance = model.Distance;
          if (distance != 0) {
            double l = Math.Sqrt(upX * upX + upY * upY);
            if (l > 0) {
              upX /= l;
              upY /= l;

              if (!model.EdgeRelativeDistance && upY > 0) {
                distance = -distance;
              }

              cx += upX * distance;
              cy += upY * distance;
            }
          }

          double sin = Math.Sin(newAngle);
          double cos = -Math.Cos(newAngle);
          rect.SetUpVector(sin, cos);

          double w = label.PreferredSize.Width;
          double h = label.PreferredSize.Height;
          rect.Width = w;
          rect.Height = h;
          rect.SetCenter(new PointD(cx, cy));
        } else {
          rect.Width = -1;
          rect.Height = -1;
        }
      }

      private bool FindAnchorTangent(IEdge edge, out double upX, out double upY, out double cx, out double cy) {
        IEdgeStyle style = edge.Style;
        if (style != null) {
          IEdgeStyleRenderer renderer = style.Renderer;
          IPathGeometry geometry = renderer.GetPathGeometry(edge, style);
          if (geometry != null) {
            var t = geometry.GetTangent(ratio);
            if (t != null) {
              var tangent = t.Value;
              upX = -tangent.Vector.Y;
              upY = tangent.Vector.X;
              cx = tangent.Point.X;
              cy = tangent.Point.Y;
              return true;
            }
          }
        }

        double l = 0;

        var spl = edge.SourcePort.GetLocation();
        double x1 = spl.X;
        double y1 = spl.Y;

        var tpl = edge.TargetPort.GetLocation();
        double x2 = tpl.X;
        double y2 = tpl.Y;

        {
          double lx = x1;
          double ly = y1;

          var bends = edge.Bends;
          for (int i = 0; i < bends.Count; i++) {
            IBend bend = bends[i];
            double bx = bend.Location.X;
            double by = bend.Location.Y;
            double dx = bx - lx;
            double dy = by - ly;

            l += Math.Sqrt(dx * dx + dy * dy);
            lx = bx;
            ly = by;
          }

          {
            double dx = x2 - lx;
            double dy = y2 - ly;

            l += Math.Sqrt(dx * dx + dy * dy);
          }
        }
        double tl = ratio * l;

        if (l == 0) {
          // no length, no path, no label
          upX = 0;
          upY = -1;
          cx = x1;
          cy = y1;
          return false;
        }

        l = 0;

        {
          double lx = x1;
          double ly = y1;

          var bends = edge.Bends;
          for (int i = 0; i < bends.Count; i++) {
            IBend bend = bends[i];
            double bx = bend.Location.X;
            double by = bend.Location.Y;
            double dx = bx - lx;
            double dy = by - ly;

            double sl = Math.Sqrt(dx * dx + dy * dy);
            if (sl > 0 && l + sl >= tl) {
              tl -= l;
              cx = lx + tl * dx / sl;
              cy = ly + tl * dy / sl;
              upX = -dy;
              upY = dx;
              return true;
            }
            l += sl;
            lx = bx;
            ly = by;
          }

          {
            double dx = x2 - lx;
            double dy = y2 - ly;

            double sl = Math.Sqrt(dx * dx + dy * dy);
            if (sl > 0) {
              tl -= l;
              cx = lx + tl * dx / sl;
              cy = ly + tl * dy / sl;
              upX = -dy;
              upY = dx;
              return true;
            } else {
              upX = 0;
              upY = -1;
              cx = x1;
              cy = y1;
              return false;
            }
          }
        }
      }

      #region Implementation of IMarkupExtensionConverter

      public bool CanConvert(IWriteContext context, object value) {
        return true;
      }

      public MarkupExtension Convert(IWriteContext context, object value) {
        return new RotatingEdgeLabelModelParameterExtension { Ratio = ratio, Model = Model };
      }

      #endregion
    }
  }
}