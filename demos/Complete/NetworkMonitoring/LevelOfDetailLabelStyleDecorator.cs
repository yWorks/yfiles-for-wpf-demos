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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.NetworkMonitoring
{
  /// <summary>
  /// Wraps a label style and hides the label from the view if the zoom level is less than a given threshold.
  /// </summary>
  public class LevelOfDetailLabelStyleDecorator : LabelStyleBase<Visual>
  {
    /// <summary>
    /// Gets or sets the wrapped style instance.
    /// </summary>
    public ILabelStyle WrappedStyle { get; set; }

    /// <summary>
    /// Gets or sets the threshold at which the label should be hidden.
    /// </summary>
    public double HideThreshold { get; set; }

    protected override Visual CreateVisual(IRenderContext context, ILabel label) {
      var zoom = context.CanvasControl.Zoom;
      if (zoom < HideThreshold) {
        return null;
      }

      return WrappedStyle.Renderer.GetVisualCreator(label, WrappedStyle).CreateVisual(context);
    }

    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, ILabel label) {
      var zoom = context.CanvasControl.Zoom;
      if (zoom < HideThreshold) {
        return null;
      }

      return WrappedStyle.Renderer.GetVisualCreator(label, WrappedStyle)
                         .UpdateVisual(context, oldVisual);
    }

    protected override SizeD GetPreferredSize(ILabel label) {
      return WrappedStyle.Renderer.GetPreferredSize(label, WrappedStyle);
    }
  }
}