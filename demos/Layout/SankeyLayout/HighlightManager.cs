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

using System.Windows.Media;
using Demo.yFiles.Layout.Sankey;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace SankeyLayout
{
  public class HighlightManager : HighlightIndicatorManager<IModelItem>
  {
    private readonly ICanvasObjectGroup edgeHighlightGroup;

    public HighlightManager(GraphControl canvas,
      [CanBeNull] ISelectionModel<IModelItem> selectionModel = null) : base(canvas, selectionModel) {
      var graphModelManager = canvas.GraphModelManager;
      edgeHighlightGroup = graphModelManager.ContentGroup.AddGroup();
      edgeHighlightGroup.Below(graphModelManager.EdgeLabelGroup);
    }

    protected override ICanvasObjectGroup GetCanvasObjectGroup(IModelItem item) {
      return item is IEdge ? edgeHighlightGroup : base.GetCanvasObjectGroup(item);
    }

    protected override ICanvasObjectInstaller GetInstaller(IModelItem item) {
      if (item is IEdge) {
        return new EdgeStyleDecorationInstaller
        {
          EdgeStyle = new BezierEdgeStyle(new DemoEdgeStyleRenderer(((GraphControl) this.Canvas).Selection, true)),
          ZoomPolicy = StyleDecorationZoomPolicy.WorldCoordinates
        };
      }
      if (item is ILabel) {
        return new LabelStyleDecorationInstaller
        {
          LabelStyle = new NodeStyleLabelStyleAdapter(new ShapeNodeStyle
          {
            Shape = ShapeNodeShape.RoundRectangle,
            Pen = new Pen(Brushes.DodgerBlue, 2),
            Brush = null
          }, VoidLabelStyle.Instance),
          Margins = new InsetsD(3),
          ZoomPolicy = StyleDecorationZoomPolicy.WorldCoordinates
        };
      }
      return base.GetInstaller(item);
    }
  }
}
