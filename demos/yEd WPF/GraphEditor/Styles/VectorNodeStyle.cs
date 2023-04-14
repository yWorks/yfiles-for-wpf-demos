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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.GraphEditor.Styles
{
  public class VectorNodeStyle : NodeStyleBase<VisualGroup>
  {
    public VectorNodeStyle() {
    }

    public string TemplateKey { get; set; }

    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      var container = new VisualGroup();
      Render(context, container, node);
      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      VisualGroup container = oldVisual;
      if (container.Transform is MatrixTransform) {
        RectD layout = node.Layout.ToRectD();
        ((MatrixTransform)container.Transform).Matrix = new Matrix(layout.Width, 0, 0, layout.Height, layout.X, layout.Y);
        return container;
      } else {
        return CreateVisual(context, node);
      }
    }

    private void Render(IRenderContext context, VisualGroup container, INode node) {
      RectD layout = node.Layout.ToRectD();
      DataTemplate template = Application.Current.Resources[TemplateKey] as DataTemplate;

      if (template != null) {
        Canvas child = template.LoadContent() as Canvas;

        if (child != null) {
          double oldScaleX = 1, oldScaleY = 1;
          ScaleTransform oldTransform = child.RenderTransform as ScaleTransform;
          
          if (oldTransform != null) {
            oldScaleX = oldTransform.ScaleX;
            oldScaleY = oldTransform.ScaleY;
          } else {
            child.RenderTransform = oldTransform = new ScaleTransform() {ScaleX = 1, ScaleY = 1};
          }
          double width = child.Width;
          double height = child.Height;

          double scaleX = 1/(width/oldScaleX);
          double scaleY = 1/(height/oldScaleY);

          container.Add(child);

          if (oldTransform.IsFrozen) {
            oldTransform = oldTransform.CloneCurrentValue();
            child.RenderTransform = oldTransform;
          }
          oldTransform.ScaleX = scaleX;
          oldTransform.ScaleY = scaleY;
        }
        container.Transform = new MatrixTransform()
                                {Matrix = new Matrix(layout.Width, 0, 0, layout.Height, layout.X, layout.Y)};
      }
    }
  }
}
