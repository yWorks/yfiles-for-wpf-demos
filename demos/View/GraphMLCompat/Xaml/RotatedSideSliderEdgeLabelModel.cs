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
using System.ComponentModel;
using System.Reflection;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml {
  /// <summary>
  /// An edge label model that allows placement of labels at a set of continuous positions
  /// along both sides of an edge.
  /// </summary>
  /// <remarks>
  /// The set of positions can be influenced by specifying the density value that controls
  /// the spacing between adjacent label positions.
  /// Furthermore, it's possible to specify distance values that control the distance
  /// between label and edge and between label and nodes.
  /// </remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class RotatedSideSliderEdgeLabelModel : ILabelModel, ILabelModelParameterProvider, ILabelModelParameterFinder
  {
    private readonly RotatedSliderEdgeLabelModel leftModel;
    private readonly RotatedSliderEdgeLabelModel rightModel;

    /// <summary>Returns a new instance of <see cref="RotatedSliderEdgeLabelModel"/>.</summary>
    public RotatedSideSliderEdgeLabelModel() : this(0, 0, true, true) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RotatedSideSliderEdgeLabelModel"/> class.
    /// </summary>
    private RotatedSideSliderEdgeLabelModel(double distance, double angle, bool distanceRelativeToEdge,
                                            bool autoRotationEnabled) {
      double dist = distance;
      if (distance == 0.0) {
        dist += Double.Epsilon;
      }
      leftModel = new RotatedSliderEdgeLabelModel(-dist, angle, distanceRelativeToEdge, autoRotationEnabled);
      rightModel = new RotatedSliderEdgeLabelModel(dist, angle, distanceRelativeToEdge, autoRotationEnabled);
    }

    /// <summary>The distance between the label's box and the edge's path.</summary>
    /// <remarks>
    /// Specifies the distance between the label's box and the edge's path.
    /// The interpretation of positive/negative values depends on property
    /// <see cref="RotatedSliderEdgeLabelModel.DistanceRelativeToEdge"/>
    /// .
    /// </remarks>
    /// <seealso cref="RotatedSliderEdgeLabelModel.DistanceRelativeToEdge"/>
    [DefaultValue(0d)]
    public double Distance {
      get { return rightModel.Distance; }
      set {
        double distance = value;
        if (distance == 0.0) {
          distance += Double.Epsilon;
        }
        leftModel.Distance = -distance;
        rightModel.Distance = distance;
      }
    }

    /// <summary>The angle of the label model.</summary>
    /// <remarks>Specifies the angle of the label model.</remarks>
    [DefaultValue(0.0d)]
    public double Angle {
      get { return rightModel.Angle; }
      set {
        leftModel.Angle = value;
        rightModel.Angle = value;
      }
    }

    /// <summary>
    /// A value indicating whether the distance to the edge is interpreted
    /// relatively to the edge's path.
    /// </summary>
    /// <remarks>
    /// Specifies a value indicating whether the distance to the edge is interpreted
    /// relatively to the edge's path. If this value is set, the label is placed
    /// to the left of the edge segment (relative to the segment direction) if
    /// <see cref="Distance"/> is less than <c>0</c> and to the right of the
    /// edge segment if <see cref="Distance"/> is greater than <c>0</c>.
    /// If this value is not set, the label is placed below the edge segment (in
    /// geometric sense) if <see cref="Distance"/> is less than <c>0</c> and
    /// above the edge segment if <see cref="Distance"/> is greater than
    /// <c>0</c>.
    /// <para>
    /// The default value is <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="RotatedSideSliderEdgeLabelModel.Distance"/>
    [DefaultValue(true)]
    public bool DistanceRelativeToEdge {
      get { return rightModel.DistanceRelativeToEdge; }
      set {
        leftModel.DistanceRelativeToEdge = value;
        rightModel.DistanceRelativeToEdge = value;
      }
    }

    /// <summary>
    /// Specifies whether or not edge labels are automatically rotated according to
    /// the angle of the corresponding reference edge segment.
    /// </summary>
    /// <remarks>
    /// Specifies whether or not edge labels have to be automatically rotated
    /// according to the angle of the corresponding reference edge segment.
    /// <para>
    /// By default, this feature is enabled.
    /// </para>
    /// </remarks>
    [DefaultValue(true)]
    public bool AutoRotationEnabled {
      get { return rightModel.AutoRotationEnabled; }
      set {
        leftModel.AutoRotationEnabled = value;
        rightModel.AutoRotationEnabled = value;
      }
    }

    ///<inheritdoc/>
    public object Lookup(Type type) {
      if (type.IsInstanceOfType(this)) {
        return this;
      } else {
        return null;
      }
    }

    ///<inheritdoc/>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      RotatedSideSliderParameter modelParameter = (RotatedSideSliderParameter) parameter;
      return modelParameter.InnerParameter.Model.GetGeometry(label, modelParameter.InnerParameter);
    }

    /// <summary>
    /// A model parameter that encodes the default position of this model's
    /// allowed edge label positions.
    /// </summary>
    /// <remarks>
    /// This implementation returns a model parameter that encodes the default position of this model's
    /// allowed edge label positions to the right of the edge path.
    /// </remarks>
    public ILabelModelParameter CreateDefaultParameter() {
      return new RotatedSideSliderParameter(rightModel.CreateDefaultParameter(), this);
    }

    ///<inheritdoc/>
    public ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return Lookups.Empty;
    }

    ///<inheritdoc/>
    public IEnumerable<ILabelModelParameter> GetParameters(ILabel label, ILabelModel model) {
      List<ILabelModelParameter> parameters = new List<ILabelModelParameter>();
      RotatedSideSliderEdgeLabelModel rotatedModel;
      var rotatedSideSliderEdgeLabelModel = model as RotatedSideSliderEdgeLabelModel;
      if (rotatedSideSliderEdgeLabelModel != null) {
        rotatedModel = rotatedSideSliderEdgeLabelModel;
      } else {
        rotatedModel = this;
      }
      var rightModel = rotatedModel.rightModel;
      var leftModel = rotatedModel.leftModel;
      if (rightModel != null) {
        var parameterProvider = (ILabelModelParameterProvider) rightModel.Lookup(typeof(ILabelModelParameterProvider));
        if (parameterProvider != null) {
          var innerParameters = parameterProvider.GetParameters(label, rightModel);
          foreach (var innerParameter in innerParameters) {
            parameters.Add(new RotatedSideSliderParameter(innerParameter, rotatedModel));
          }
        }
      }
      if (leftModel != null) {
        var parameterProvider = (ILabelModelParameterProvider) leftModel.Lookup(typeof(ILabelModelParameterProvider));
        if (parameterProvider != null) {
          var innerParameters = parameterProvider.GetParameters(label, leftModel);
          foreach (var innerParameter in innerParameters) {
            parameters.Add(new RotatedSideSliderParameter(innerParameter, rotatedModel));
          }
        }
      }
      return parameters;
    }

    /// <summary>
    /// Creates a parameter that measures the provided segment index from the source side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the source side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment.</param>
    /// <param name="rightOfEdge">Determines whether the label should be placed right or left of the edge.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    [NotNull]
    public ILabelModelParameter CreateParameterFromSource(int segmentIndex, double segmentRatio, bool rightOfEdge) {
      RotatedSliderEdgeLabelModel model = rightOfEdge ? rightModel : leftModel;
      return
          new RotatedSideSliderParameter(model.CreateParameterFromSource(segmentIndex, segmentRatio),
            this);
    }

    /// <summary>
    /// Creates a parameter that measures the provided segment index from the target side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the target side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment.</param>
    /// /// <param name="rightOfEdge">Determines whether the label should be placed right or left of the edge.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    [NotNull]
    public ILabelModelParameter CreateParameterFromTarget(int segmentIndex, double segmentRatio, bool rightOfEdge) {
      RotatedSliderEdgeLabelModel model = rightOfEdge ? rightModel : leftModel;
      return
          new RotatedSideSliderParameter(model.CreateParameterFromTarget(segmentIndex, segmentRatio),
            this);
    }

    ILabelModelParameter ILabelModelParameterFinder.FindBestParameter(ILabel label, ILabelModel model,
                                                                      IOrientedRectangle labelLayout) {
      var leftParam = ((ILabelModelParameterFinder) leftModel).FindBestParameter(label, leftModel, labelLayout);
      var rightParam = ((ILabelModelParameterFinder) rightModel).FindBestParameter(label, rightModel, labelLayout);
      var leftGeom = ((ILabelModel) leftModel).GetGeometry(label, leftParam);
      var rightGeom = ((ILabelModel) rightModel).GetGeometry(label, rightParam);
      var layoutCenter = labelLayout.GetCenter();
      double leftDist = leftGeom.GetCenter().DistanceTo(layoutCenter);
      double rightDist = rightGeom.GetCenter().DistanceTo(layoutCenter);
      return leftDist < rightDist
        ? new RotatedSideSliderParameter(leftParam, this)
        : new RotatedSideSliderParameter(rightParam, this);
    }
  }
}