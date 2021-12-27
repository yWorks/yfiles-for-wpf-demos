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
using System.ComponentModel;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Bpmn.Util {

  internal class ScalingLabelModel : ILabelModel {

    #region Initialize static fields

    private static readonly InteriorStretchLabelModel stretchModel;
    private static readonly ILabelModelParameter stretchParameter;

    private static readonly SimpleNode dummyNode;
    private static readonly SimpleLabel dummyLabel;

    static ScalingLabelModel() {
      stretchModel = new InteriorStretchLabelModel();
      stretchParameter = stretchModel.CreateParameter(InteriorStretchLabelModel.Position.Center);
      dummyNode = new SimpleNode();
      dummyLabel = new SimpleLabel(dummyNode, "", stretchParameter);
    }

    #endregion

    /// <summary>
    /// Gets or sets the insets to use within the node's <see cref="INode.Layout"/>.
    /// </summary>
    [DefaultValue(typeof (InsetsD), "0")]
    public virtual InsetsD Insets { get; set; }

    ///<inheritdoc/>
    public object Lookup(Type type) {
      return stretchModel.Lookup(type);
    }

    ///<inheritdoc/>
    public ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return stretchModel.GetContext(label, parameter);
    }

    ///<inheritdoc/>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      ScalingParameter scalingParameter = (ScalingParameter) parameter;
      if (!(label.Owner is INode)) {
        return OrientedRectangle.Empty;
      }

      var availableRect = ((INode) label.Owner).Layout.ToRectD();
      var horizontalInsets = Insets.Left + Insets.Right;
      var verticalInsets = Insets.Top + Insets.Bottom;

      // consider fix insets
      double x = availableRect.MinX + (availableRect.Width > horizontalInsets ? Insets.Left : 0);
      double y = availableRect.MinY + (availableRect.Height > verticalInsets ? Insets.Top : 0);
      double width = availableRect.Width - (availableRect.Width > horizontalInsets ? horizontalInsets : 0);
      double height = availableRect.Height - (availableRect.Height > verticalInsets ? verticalInsets : 0);

      // consider scaling insets
      x += scalingParameter.ScalingInsets.Left*width;
      y += scalingParameter.ScalingInsets.Top*height;
      width = width*(1 - scalingParameter.ScalingInsets.Left - scalingParameter.ScalingInsets.Right);
      height = height*(1 - scalingParameter.ScalingInsets.Top - scalingParameter.ScalingInsets.Bottom);

      if (scalingParameter.KeepRatio) {
        var fixRatio = scalingParameter.Ratio;
        var availableRatio = height > 0  && width > 0 ? width/height : 1;

        if (fixRatio > availableRatio) {
          // keep width
          double cy = y + height / 2;
          height *= availableRatio / fixRatio;
          y = cy - height/2;
        } else {
          double cx = x + width / 2;
          width *= fixRatio / availableRatio;
          x = cx - width/2;
        }
      }

      dummyNode.Layout = new RectD(x, y, width, height);
      dummyLabel.PreferredSize = label.PreferredSize;
      return stretchModel.GetGeometry(dummyLabel, stretchParameter);
    }

    #region Create Parameter Methods

    ///<inheritdoc/>
    public ILabelModelParameter CreateDefaultParameter() {
      return new ScalingParameter() {Model = this, ScalingInsets = InsetsD.Empty};
    }

    public ILabelModelParameter CreateScaledParameter(double scale) {
      if (scale <= 0 || scale > 1) {
        throw new ArgumentException("Argument '" + scale + "' not allowed. Valid values are in ]0; 1].");
      }
      return new ScalingParameter() {Model = this, ScalingInsets = new InsetsD((1 - scale)/2)};
    }

    public ILabelModelParameter CreateScaledParameter(double leftScale, double topScale, double rightScale, double bottomScale) {
      if (leftScale < 0 || rightScale < 0 || topScale < 0 || bottomScale < 0) {
        throw new ArgumentException("Negative Arguments are not allowed.");
      }
      if (leftScale + rightScale >= 1 || topScale + bottomScale >= 1) {
        throw new ArgumentException("Arguments not allowed. The sum of left and right scale respectively top and bottom scale must be below 1.");
      }
      return new ScalingParameter() { Model = this, ScalingInsets = new InsetsD(leftScale, topScale, rightScale, bottomScale) };
    }

    public ILabelModelParameter CreateScaledParameterWithRatio(double scale, double ratio) {
      if (scale <= 0 || scale > 1) {
        throw new ArgumentException("Argument '" + scale + "' not allowed. Valid values are in ]0; 1].");
      }
      if (ratio <= 0) {
        throw new ArgumentException("Argument '" + ratio + "' not allowed. Ratio must be positive.");
      }
      return new ScalingParameter() {Model = this, ScalingInsets = new InsetsD((1 - scale)/2), KeepRatio = true, Ratio = ratio};
    }

    public ILabelModelParameter CreateScaledParameterWithRatio(double leftScale, double topScale, double rightScale, double bottomScale, double ratio) {
      if (leftScale < 0 || rightScale < 0 || topScale < 0 || bottomScale < 0) {
        throw new ArgumentException("Negative Arguments are not allowed.");
      }
      if (leftScale + rightScale >= 1 || topScale + bottomScale >= 1) {
        throw new ArgumentException("Arguments not allowed. The sum of left and right scale respectively top and bottom scale must be below 1.");
      }
      if (ratio <= 0) {
        throw new ArgumentException("Argument '" + ratio + "' not allowed. Ratio must be positive.");
      }
      return new ScalingParameter() { Model = this, ScalingInsets = new InsetsD(leftScale, topScale, rightScale, bottomScale), KeepRatio = true, Ratio = ratio };
    }

#endregion

    #region ScalingParameter

    private class ScalingParameter : ILabelModelParameter
    {

      public ILabelModel Model { get; set; }

      public InsetsD ScalingInsets { get; set; }

      public bool KeepRatio { get; set; }

      public double Ratio { get; set; }

      public object Clone() {
        return new ScalingParameter() {Model = Model, ScalingInsets = ScalingInsets, KeepRatio = KeepRatio};
      }

      public bool Supports(ILabel label) {
        return label.Owner is INode;
      }
    }

#endregion

  }
}
