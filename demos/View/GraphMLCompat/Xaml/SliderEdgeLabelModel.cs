/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml {
  /// <summary>
  /// An edge label model that can be used for labels along the path of an edge.
  /// </summary>
  /// <remarks>
  /// This model allows for specifying the index of the segment of
  /// the edge and the distance from the edge, as well as the angle
  /// of the label.
  /// </remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class SliderEdgeLabelModel : ILabelModel, ILabelModelParameterProvider
  {
    private double upX;
    private double upY;

    /// <summary>
    /// Creates a new instance with distance and angle set to <c>0</c>.
    /// </summary>
    public SliderEdgeLabelModel() : this(0, 0, true) { }

    /// <summary>
    /// Creates a new instance using the provided values.
    /// </summary>
    public SliderEdgeLabelModel(double distance, double angle, bool edgeRelativeDistance) {
      Distance = distance;
      Angle = angle;
      EdgeRelativeDistance = edgeRelativeDistance;
    }

    /// <summary>
    /// Gets or sets the angle the labels are rotated about.
    /// </summary>
    /// <remarks>
    /// The angle is measured relative to the x-axis.
    /// The default value is <c>0.0</c>.
    /// </remarks>
    /// <value>The angle in radians.</value>
    [DefaultValue(0.0d)]
    public double Angle {
      get { return Math.Atan2(upX, -upY); }
      set {
        upX = Math.Sin(value);
        upY = -Math.Cos(value);
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether distance to the edge is interpreted
    /// relatively to the edge's path.
    /// </summary>
    /// <remarks>
    /// If this is set to <see langword="false"/> positive <see cref="Distance"/> values
    /// will make the label appear above the edge, otherwise they will appear left of the edge.
    /// </remarks>
    [DefaultValue(true)]
    public bool EdgeRelativeDistance { get; set; }

    ///<inheritdoc/>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      OrientedRectangle geometry = new OrientedRectangle(0, 0, 10, 10);
      IEdge edge = (IEdge) label.Owner;

      if (edge == null) {
        geometry.Width = -1;
        geometry.Height = -1;
        return geometry;
      }

      SliderParameter sliderParameter = (SliderParameter) parameter;
      SizeD preferredSize = label.PreferredSize;
      geometry.Width = preferredSize.Width;
      geometry.Height = preferredSize.Height;
      geometry.SetUpVector(upX, upY);

      sliderParameter.SetAnchor(this, edge, geometry);

      return geometry;
    }

    /// <summary>
    /// Gets or sets the distance between the label and the edge's path.
    /// </summary>
    /// <remarks>
    /// A positive value will make the label appear above or right of the edge,
    /// whereas negative values will make it appear on the opposite side of the edge's path.
    /// A value of <c>0</c> will make the label's appear centered on the edge's path.
    /// </remarks>
    /// <seealso cref="EdgeRelativeDistance"/>
    [DefaultValue(0.0d)]
    public double Distance { get; set; }

    ///<inheritdoc/>
    public ILabelModelParameter CreateDefaultParameter() {
      return new SliderParameter(this, 0, 0.5);
    }

    /// <inheritdoc/>
    public virtual ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return Lookups.Empty;
    }

    /// <summary>
    /// Creates a parameter that measures the provided segment index from the source side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the source side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    public ILabelModelParameter CreateParameterFromSource(int segmentIndex, double segmentRatio) {
      return new SliderParameter(this, segmentIndex, segmentRatio);
    }

    /// <summary>
    /// Creates a parameter that measures the provided segment index from the target side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the target side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    public ILabelModelParameter CreateParameterFromTarget(int segmentIndex, double segmentRatio) {
      return new SliderParameter(this, -1 - segmentIndex, 1 - segmentRatio);
    }

    ///<inheritdoc/>
    public virtual object Lookup(Type type) {
      if (type == typeof(ILabelModelParameterProvider)) {
        return this;
      }
      if (type == typeof(ILabelModelParameterFinder)) {
        return DefaultLabelModelParameterFinder.Instance;
      }
      return null;
    }

    ///<inheritdoc/>
    public virtual IEnumerable<ILabelModelParameter> GetParameters(ILabel label, ILabelModel model) {
      SliderEdgeLabelModel sliderEdgeLabelModel = (SliderEdgeLabelModel) model;
      List<ILabelModelParameter> result = new List<ILabelModelParameter>();
      IEdge edge = (IEdge) label.Owner;
      IPathGeometry geometry = GetPathGeometry(edge);
      if (geometry != null) {
        int count = geometry.GetSegmentCount();
        for (int i = 0; i < count; i++) {
          result.Add(sliderEdgeLabelModel.CreateParameterFromSource(i, 0));
          result.Add(sliderEdgeLabelModel.CreateParameterFromSource(i, 0.5));
          result.Add(sliderEdgeLabelModel.CreateParameterFromSource(i, 1));
        }
      }
      return result;
    }

    /// <summary>
    /// Implementation that always returns a path geometry.
    /// </summary>
    private static IPathGeometry GetPathGeometry(IEdge edge) {
      IEdgeStyle style = edge.Style;
      return style.Renderer.GetPathGeometry(edge, style);
    }

    private static void AnchorGeometry(OrientedRectangle geometry, bool edgeRelativeDistance, double labelModelDistance,
                                       double x, double y, double thisRatio, double tx, double ty) {
      // sanitize tangent
      if (tx == 0 && ty == 0) {
        tx = 1;
      }
      // apply edge relative logic
      double distance;
      if (!edgeRelativeDistance && tx < 0) {
        distance = -labelModelDistance;
      } else {
        distance = labelModelDistance;
      }

      // transformation matrix
      double m11 = -geometry.UpY;
      double m12 = geometry.UpX;
      double m21 = -m12;
      double m22 = m11;

      // transform to make labels aligned with x-y axes
      // transform point
      double nx = x * m11 + y * m12;
      double ny = x * m21 + y * m22;

      // transform tangent
      double ntx = tx * m11 + ty * m12;
      double nty = tx * m21 + ty * m22;

      x = nx;
      y = ny;
      tx = ntx;
      ty = nty;

      double width = geometry.Width;
      double height = geometry.Height;

      // see if we should stack vertically or horizontally
      bool verticalStacking;
      if (distance != 0) {
        double atx = Math.Abs(tx);
        double aty = Math.Abs(ty);
        if (atx > 2 * aty) {
          verticalStacking = true;
        } else if (aty > 2 * atx) {
          verticalStacking = false;
        } else {
          if ((tx * ty) > 0) {
            verticalStacking = distance > 0;
          } else {
            verticalStacking = distance < 0;
          }
          if (thisRatio > 0.5) {
            verticalStacking = !verticalStacking;
          }
        }
      } else {
        verticalStacking = Math.Abs(tx) > Math.Abs(ty);
      }

      // calculate the center position using the ratio
      if (verticalStacking) {
        y = -y;
        UpdatePosition(distance, height, width, thisRatio, -ty, tx, ref y, ref x);
        y = -y;
      } else {
        UpdatePosition(distance, width, height, thisRatio, tx, ty, ref x, ref y);
      }

      // go to the anchor
      x -= width * 0.5;
      y += height * 0.5;

      // retransform to original coordinate system and assign as anchor
      geometry.AnchorX = x * m11 + y * m21;
      geometry.AnchorY = x * m12 + y * m22;
    }

    private static void UpdatePosition(double distance, double width, double height, double ratio, double tx, double ty,
                                       ref double x, ref double y) {
      double iratio = 1 - ratio;
      if (distance == 0) {
        // centered on edge
        if (ty > 0) {
          y += height * (iratio - 0.5);
          x += (iratio - 0.5) * height * tx / ty;
        } else {
          // swap ratio
          y += height * (ratio - 0.5);
          x += (ratio - 0.5) * height * tx / ty;
        }
      } else {
        if (ty > 0) {
          // ty > 0
          // 
          // ----------------
          // [IIIII]/[IIIII]
          //       /         
          //      /          
          //
          y += height * (iratio - 0.5);
          double factor = height * tx / ty;
          if (distance > 0) {
            if (tx > 0) {
              x += iratio * factor;
            } else {
              x += -ratio * factor;
            }
            x += width * 0.5 + distance;
          } else {
            if (tx > 0) {
              x += -ratio * factor;
            } else {
              x += iratio * factor;
            }
            x -= width * 0.5 - distance;
          }
        } else if (ty < 0) {
          // ty < 0
          // 
          //          /         
          //         /          
          // [IIIII]/[IIIII]
          // ----------------
          //

          // swap ratio
          double t = ratio;
          ratio = iratio;
          iratio = t;

          y += height * (iratio - 0.5);
          double factor = height * tx / ty;
          if (distance > 0) {
            if (tx > 0) {
              x += iratio * factor;
            } else {
              x += -ratio * factor;
            }
            x -= width * 0.5 + distance;
          } else {
            if (tx > 0) {
              x += -ratio * factor;
            } else {
              x += iratio * factor;
            }
            x += width * 0.5 - distance;
          }
        }
      }
    }

    internal class SliderParameter : ILabelModelParameter, IMarkupExtensionConverter
    {
      private readonly SliderEdgeLabelModel model;
      internal readonly int segmentIndex;
      internal readonly double ratio;

      public SliderParameter(SliderEdgeLabelModel model, int segmentIndex, double ratio) {
        this.model = model;
        this.segmentIndex = segmentIndex;
        this.ratio = ratio;
      }

      public void SetAnchor(SliderEdgeLabelModel labelModel, IEdge edge, OrientedRectangle geometry) {
        IPathGeometry pathGeometry = GetPathGeometry(edge);
        if (pathGeometry != null) {
          int count = pathGeometry.GetSegmentCount();
          int index = segmentIndex;
          if (index >= count) {
            index = count - 1;
          }
          if (index < 0) {
            index = count + index;
          }

          if (index < 0) {
            index = 0;
          } else if (index >= count) {
            index = count - 1;
          }

          double thisRatio = ratio;
          var validTangent = pathGeometry.GetTangent(index, thisRatio);
          if (validTangent.HasValue) {
            var p = validTangent.Value.Point;
            var t = validTangent.Value.Vector;
            AnchorGeometry(geometry, labelModel.EdgeRelativeDistance, labelModel.Distance, p.X, p.Y, thisRatio, t.X, t.Y);
            return;
          }
        }
        geometry.Width = -1;
        geometry.Height = -1;
      }

      public ILabelModel Model {
        get { return model; }
      }

      public bool Supports(ILabel label) {
        return label.Owner is IEdge;
      }

      public object Clone() {
        return MemberwiseClone();
      }

      #region Implementation of IMarkupExtensionConverter

      public bool CanConvert(IWriteContext context, object value) {
        return true;
      }

      public MarkupExtension Convert(IWriteContext context, object value) {
        if (segmentIndex < 0) {
          return new SliderLabelModelParameterExtension
          {
            Location = SliderParameterLocation.FromTarget,
            SegmentIndex = -1 - segmentIndex,
            SegmentRatio = 1 - ratio,
            Model = model
          };
        } else {
          return new SliderLabelModelParameterExtension
          {
            Location = SliderParameterLocation.FromSource,
            SegmentIndex = segmentIndex,
            SegmentRatio = ratio,
            Model = model
          };
        }
      }

      #endregion
    }
  }
}