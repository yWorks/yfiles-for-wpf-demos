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

using System.Windows.Markup;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml {
  /// <summary>
  /// The label model parameter used by <see cref="RotatedSideSliderEdgeLabelModel"/>.
  /// This class wraps the parameter of the model wrapped by 
  /// <see cref="RotatedSideSliderEdgeLabelModel"/>.
  /// </summary>
  internal class RotatedSideSliderParameter : ILabelModelParameter, IMarkupExtensionConverter
  {
    private readonly RotatedSideSliderEdgeLabelModel labelModel;

    /// <summary>
    /// Gets or sets the wrapped parameter.
    /// </summary>
    /// <value>
    /// The inner parameter.
    /// </value>
    internal ILabelModelParameter InnerParameter { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RotatedSideSliderEdgeLabelModel"/> class.
    /// </summary>
    /// <param name="innerParameter">The wrapped parameter.</param>
    /// <param name="labelModel">The label model.</param>
    internal RotatedSideSliderParameter(ILabelModelParameter innerParameter, RotatedSideSliderEdgeLabelModel labelModel) {
      this.InnerParameter = innerParameter;
      this.labelModel = labelModel;
    }

    public ILabelModel Model {
      get { return labelModel; }
    }

    public bool Supports(ILabel label) {
      return InnerParameter.Supports(label);
    }

    public object Clone() {
      return this;
    }

    #region Implementation of IMarkupExtensionConverter

    public bool CanConvert(IWriteContext context, object value) {
      return true;
    }

    public MarkupExtension Convert(IWriteContext context, object value) {
      var parameter = (RotatedSliderEdgeLabelModel.RotatedSliderParameter) InnerParameter;
      var side = labelModel.Distance == ((RotatedSliderEdgeLabelModel) parameter.Model).Distance
        ? SliderParameterLocation.Right
        : SliderParameterLocation.Left;
      if (parameter.Segment < 0) {
        return new RotatedSideSliderLabelModelParameterExtension
        {
          Location = SliderParameterLocation.FromTarget | side,
          SegmentIndex = -1 - parameter.Segment,
          SegmentRatio = 1 - parameter.Ratio,
          Model = Model
        };
      } else {
        return new RotatedSideSliderLabelModelParameterExtension
        {
          Location = SliderParameterLocation.FromSource | side,
          SegmentIndex = parameter.Segment,
          SegmentRatio = parameter.Ratio,
          Model = Model
        };
      }
    }

    #endregion
  }
}