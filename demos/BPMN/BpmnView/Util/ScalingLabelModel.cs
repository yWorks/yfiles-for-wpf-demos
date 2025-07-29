/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.Graph.Bpmn.Util
{
  internal class ScalingLabelModel : ILabelModel
  {
    #region Initialize static fields

    private static readonly StretchNodeLabelModel stretchModel;
    private static readonly ILabelModelParameter stretchParameter;

    private static readonly SimpleNode dummyNode;
    private static readonly SimpleLabel dummyLabel;

    static ScalingLabelModel() {
      stretchModel = new StretchNodeLabelModel();
      stretchParameter = stretchModel.CreateParameter(StretchNodeLabelModel.Position.Center);
      dummyNode = new SimpleNode();
      dummyLabel = new SimpleLabel(dummyNode, "", stretchParameter);
    }

    #endregion

    /// <summary>
    ///   Gets or sets the padding to use within the node's <see cref="INode.Layout"/>.
    /// </summary>
    [DefaultValue(typeof(InsetsD), "0")]
    public virtual InsetsD Padding { get; set; }

    ///<inheritdoc/>
    public ILookup GetContext(ILabel label) {
      return stretchModel.GetContext(label);
    }

    ///<inheritdoc/>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      if (parameter is not ScalingParameter scalingParameter) {
        throw new ArgumentException("The parameter is not supported by this label model.", nameof(parameter));
      }
      if (!DefaultGraph.HasOwner(label)) {
        throw new ArgumentException("The label has no valid owner.", nameof(label));
      }
      if (label.Owner is not INode node) {
        throw new ArgumentException("This model only supports node labels.", nameof(label));
      }
      var availableRect = node.Layout.ToRectD();
      var horizontalPadding = Padding.Left + Padding.Right;
      var verticalPadding = Padding.Top + Padding.Bottom;

      // consider fix padding
      var x = availableRect.X + (availableRect.Width > horizontalPadding ? Padding.Left : 0);
      var y = availableRect.Y + (availableRect.Height > verticalPadding ? Padding.Top : 0);
      var width = availableRect.Width - (availableRect.Width > horizontalPadding ? horizontalPadding : 0);
      var height = availableRect.Height - (availableRect.Height > verticalPadding ? verticalPadding : 0);

      // consider scaling padding
      x += scalingParameter.ScalingPadding.Left * width;
      y += scalingParameter.ScalingPadding.Top * height;
      width = width * (1 - scalingParameter.ScalingPadding.Left - scalingParameter.ScalingPadding.Right);
      height = height * (1 - scalingParameter.ScalingPadding.Top - scalingParameter.ScalingPadding.Bottom);

      if (scalingParameter.KeepRatio) {
        var fixRatio = scalingParameter.Ratio;
        var availableRatio = height > 0 && width > 0 ? width / height : 1;

        if (fixRatio > availableRatio) {
          // keep width
          var cy = y + height / 2;
          height *= availableRatio / fixRatio;
          y = cy - height / 2;
        } else {
          var cx = x + width / 2;
          width *= fixRatio / availableRatio;
          x = cx - width / 2;
        }
      }

      dummyNode.Layout = new RectD(x, y, width, height);
      dummyLabel.PreferredSize = label.PreferredSize;
      return stretchModel.GetGeometry(dummyLabel, stretchParameter);
    }

    #region Create Parameter Methods

    public ScalingParameter CreateScaledParameter(double scale) {
      if (scale <= 0 || scale > 1) {
        throw new ArgumentException("Argument '" + scale + "' not allowed. Valid values are in ]0; 1].");
      }
      return new ScalingParameter(this, new InsetsD((1 - scale) / 2));
    }

    public ScalingParameter CreateScaledParameter(
        double leftScale,
        double topScale,
        double rightScale,
        double bottomScale) {
      if (leftScale < 0 || rightScale < 0 || topScale < 0 || bottomScale < 0) {
        throw new ArgumentException("Negative Arguments are not allowed.");
      }
      if (leftScale + rightScale >= 1 || topScale + bottomScale >= 1) {
        throw new ArgumentException(
            "Arguments not allowed. The sum of left and right scale respectively top and bottom scale must be below 1.");
      }
      return new ScalingParameter(this, new InsetsD(leftScale, topScale, rightScale, bottomScale));
    }

    public ScalingParameter CreateScaledParameterWithRatio(double scale, double ratio) {
      if (scale <= 0 || scale > 1) {
        throw new ArgumentException("Argument '" + scale + "' not allowed. Valid values are in ]0; 1].");
      }
      if (ratio <= 0) {
        throw new ArgumentException("Argument '" + ratio + "' not allowed. Ratio must be positive.");
      }
      return new ScalingParameter(this, new InsetsD((1 - scale) / 2), true, ratio);
    }

    public ScalingParameter CreateScaledParameterWithRatio(
        double leftScale,
        double topScale,
        double rightScale,
        double bottomScale,
        double ratio) {
      if (leftScale < 0 || rightScale < 0 || topScale < 0 || bottomScale < 0) {
        throw new ArgumentException("Negative Arguments are not allowed.");
      }
      if (leftScale + rightScale >= 1 || topScale + bottomScale >= 1) {
        throw new ArgumentException(
            "Arguments not allowed. The sum of left and right scale respectively top and bottom scale must be below 1.");
      }
      if (ratio <= 0) {
        throw new ArgumentException("Argument '" + ratio + "' not allowed. Ratio must be positive.");
      }
      return new ScalingParameter(this, new InsetsD(leftScale, topScale, rightScale, bottomScale), true, ratio);
    }

    #endregion

    #region ScalingParameter

    public class ScalingParameter : ILabelModelParameter
    {
      public ScalingParameter(ILabelModel model, InsetsD scalingPadding, bool keepRatio = false, double ratio = 0) {
        Model = model;
        ScalingPadding = scalingPadding;
        KeepRatio = keepRatio;
        Ratio = ratio;
      }

      public ILabelModel Model { get; }

      public InsetsD ScalingPadding { get; }

      public bool KeepRatio { get; }

      public double Ratio { get; }

      public object Clone() {
        return new ScalingParameter(Model, ScalingPadding, KeepRatio, Ratio);
      }
    }

    #endregion
  }
}
