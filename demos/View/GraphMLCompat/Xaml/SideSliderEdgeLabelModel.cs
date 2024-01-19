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
using System.Reflection;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml {
  /// <summary>
  /// An edge <see cref="ILabelModel"/> implementation that provides labels
  /// to both sides of the edge's path.
  /// </summary>
  /// <remarks>
  /// This implementation allows for specifying the angle that the label is rotated,
  /// the distance between the label and the edge's path, and how the distance should be interpreted.
  /// </remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class SideSliderEdgeLabelModel : ILabelModel, ILabelModelParameterProvider
  {
    private readonly SliderEdgeLabelModel leftSlider = new SliderEdgeLabelModel(1, 0, true);
    private readonly SliderEdgeLabelModel rightSlider = new SliderEdgeLabelModel(-1, 0, true);

    /// <summary>
    /// Gets or sets the distance between the label and the edge's path.
    /// </summary>
    /// <remarks>
    /// The larger the value, the farther the label will be away from the edge's path.
    /// </remarks>
    /// <seealso cref="EdgeRelativePosition"/>
    public double Distance {
      get { return leftSlider.Distance - 0.01d; }
      set {
        if (value < 0) {
          throw new Exception("Value must be non-negative!");
        }
        leftSlider.Distance = value + 0.01d;
        rightSlider.Distance = -(value + 0.01d);
      }
    }

    /// <summary>
    /// Gets or sets a property that determines if label's are placed left or right
    /// or above or below the edge's path.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="true"/>, the label's position is interpreted
    /// as relative to the edge's path, otherwise it is interpreted as absolute (above or below).
    /// </remarks>
    public bool EdgeRelativePosition {
      get { return leftSlider.EdgeRelativeDistance; }
      set {
        leftSlider.EdgeRelativeDistance = value;
        rightSlider.EdgeRelativeDistance = value;
      }
    }

    /// <summary>
    /// Gets or sets the angle the labels are rotated about.
    /// </summary>
    /// <remarks>
    /// The angle is measured relative to the x-axis.
    /// The default value is <c>0.0</c>.
    /// </remarks>
    /// <value>The angle in radians.</value>
    public double Angle {
      get { return leftSlider.Angle; }
      set {
        leftSlider.Angle = value;
        rightSlider.Angle = value;
      }
    }

    /// <inheritdoc/>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      SideSliderParameter param = parameter as SideSliderParameter;
      if (param != null) {
        return param.parameter.Model.GetGeometry(label, param.parameter);
      } else {
        return new OrientedRectangle();
      }
    }

    /// <inheritdoc/>
    public virtual ILabelModelParameter CreateDefaultParameter() {
      return new SideSliderParameter(this, SliderParameterLocation.FromSource | SliderParameterLocation.Left,
        leftSlider.CreateDefaultParameter());
    }


    /// <inheritdoc/>
    public virtual ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return Lookups.Empty;
    }

    internal class SideSliderParameter : ILabelModelParameter, IMarkupExtensionConverter
    {
      private readonly ILabelModel model;
      private readonly SliderParameterLocation location;
      internal readonly ILabelModelParameter parameter;

      public SideSliderParameter(SideSliderEdgeLabelModel model, SliderParameterLocation location,
                                 ILabelModelParameter parameter) {
        this.model = model;
        this.location = location;
        this.parameter = parameter;
      }

      public ILabelModel Model {
        get { return model; }
      }

      public bool Supports(ILabel label) {
        return parameter.Supports(label);
      }

      public object Clone() {
        return this;
      }

      #region Implementation of IMarkupExtensionConverter

      public bool CanConvert(IWriteContext context, object value) {
        return true;
      }

      public MarkupExtension Convert(IWriteContext context, object value) {
        SliderEdgeLabelModel.SliderParameter parameter = (SliderEdgeLabelModel.SliderParameter) this.parameter;
        if ((location & SliderParameterLocation.FromTarget) == SliderParameterLocation.FromTarget) {
          return new SideSliderLabelModelParameterExtension
          {
            Location = location,
            SegmentIndex = -1 - parameter.segmentIndex,
            SegmentRatio = 1 - parameter.ratio,
            Model = model
          };
        } else {
          return new SideSliderLabelModelParameterExtension
          {
            Location = location,
            SegmentIndex = parameter.segmentIndex,
            SegmentRatio = parameter.ratio,
            Model = model
          };
        }
      }

      #endregion
    }

    /// <summary>
    /// Creates a parameter that describes
    /// a position at the left of the edge at the segment index from the source side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the source side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    public virtual ILabelModelParameter CreateParameterLeftFromSource(int segmentIndex, double segmentRatio) {
      return new SideSliderParameter(this, SliderParameterLocation.Left | SliderParameterLocation.FromSource,
        leftSlider.CreateParameterFromSource(segmentIndex, segmentRatio));
    }

    /// <summary>
    /// Creates a parameter that describes
    /// a position at the right of the edge at the segment index from the source side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the source side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    public virtual ILabelModelParameter CreateParameterRightFromSource(int segmentIndex, double segmentRatio) {
      return new SideSliderParameter(this, SliderParameterLocation.FromSource | SliderParameterLocation.Right,
        rightSlider.CreateParameterFromSource(segmentIndex, segmentRatio));
    }

    /// <summary>
    /// Creates a parameter that describes
    /// a position at the left of the edge at the segment index from the target side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the target side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    public virtual ILabelModelParameter CreateParameterLeftFromTarget(int segmentIndex, double segmentRatio) {
      return new SideSliderParameter(this, SliderParameterLocation.FromTarget | SliderParameterLocation.Left,
        leftSlider.CreateParameterFromTarget(segmentIndex, segmentRatio));
    }

    /// <summary>
    /// Creates a parameter that describes
    /// a position at the right of the edge at the segment index from the target side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the target side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    public virtual ILabelModelParameter CreateParameterRightFromTarget(int segmentIndex, double segmentRatio) {
      return new SideSliderParameter(this, SliderParameterLocation.FromTarget | SliderParameterLocation.Right,
        rightSlider.CreateParameterFromTarget(segmentIndex, segmentRatio));
    }


    /// <inheritdoc/>
    public virtual object Lookup(Type type) {
      if (type == typeof(ILabelModelParameterProvider)) {
        return this;
      }
      if (type == typeof(ILabelModelParameterFinder)) {
        return DefaultLabelModelParameterFinder.Instance;
      }
      return null;
    }

    /// <inheritdoc/>
    public IEnumerable<ILabelModelParameter> GetParameters(ILabel label, ILabelModel model) {
      SideSliderEdgeLabelModel mmodel = (SideSliderEdgeLabelModel) model;
      List<ILabelModelParameter> list = new List<ILabelModelParameter>();
      foreach (
        SliderEdgeLabelModel.SliderParameter parameter in mmodel.leftSlider.GetParameters(label, mmodel.leftSlider)) {
        list.Add(new SideSliderParameter(this,
          SliderParameterLocation.Left |
          (parameter.segmentIndex >= 0 ? SliderParameterLocation.FromSource : SliderParameterLocation.FromTarget),
          parameter));
      }
      foreach (
        SliderEdgeLabelModel.SliderParameter parameter in mmodel.rightSlider.GetParameters(label, mmodel.rightSlider)) {
        list.Add(new SideSliderParameter(this,
          SliderParameterLocation.Right |
          (parameter.segmentIndex >= 0 ? SliderParameterLocation.FromSource : SliderParameterLocation.FromTarget),
          parameter));
      }
      return list;
    }
  }
}