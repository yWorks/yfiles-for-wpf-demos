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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace SankeyLayout
{
  public sealed class HighlightManager : HighlightIndicatorManager<IModelItem>
  {
    private IRenderTreeGroup edgeHighlightGroup;

    public IHighlightRenderer LabelRenderer { get; } = new LabelStyleIndicatorRenderer {
        LabelStyle =
            new NodeStyleLabelStyleAdapter(
                new ShapeNodeStyle {
                    Shape = ShapeNodeShape.RoundRectangle, Pen = new Pen(Brushes.DodgerBlue, 2), Brush = null
                }, VoidLabelStyle.Instance),
        Margins = new InsetsD(3),
        ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
    };

    public IHighlightRenderer EdgeRenderer { get; set; }


    public override void Install(CanvasControl canvas) {
      base.Install(canvas);
      var graphControl = (GraphControl) canvas;
      var graphModelManager = graphControl.GraphModelManager;
      edgeHighlightGroup = graphControl.RenderTree.CreateGroup(graphModelManager.ContentGroup!);
      edgeHighlightGroup.Below(graphModelManager.EdgeLabelGroup);

      EdgeRenderer = new EdgeStyleIndicatorRenderer {
          EdgeStyle = new DemoEdgeStyle(graphControl.Selection, true),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
      };
    }

    public override void Uninstall(CanvasControl canvas) {
      canvas.RenderTree.Remove(edgeHighlightGroup);
      base.Uninstall(canvas);
    }

    protected override IRenderTreeGroup GetRenderTreeGroup(IModelItem item) {
      return item is IEdge ? edgeHighlightGroup : base.GetRenderTreeGroup(item);
    }

    protected override IObjectRenderer GetRenderer(IModelItem item) {
      return item switch {
          ILabel => LabelRenderer,
          IEdge => EdgeRenderer,
          _ => base.GetRenderer(item)
      };
    }
  }
}
