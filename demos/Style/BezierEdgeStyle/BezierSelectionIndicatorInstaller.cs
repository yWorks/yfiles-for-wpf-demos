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

using System.Windows.Media;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.BezierEdgeStyle
{
  /// <summary>
  /// Custom decoration decorator that adds a rendering of the curves control point segments
  /// </summary>
  /// <remarks>This implementation adds as a decorator for an existing decorator and just adds the control point rendering on top.</remarks>
  internal class BezierSelectionIndicatorInstaller : ISelectionIndicatorInstaller
  {
    /// <summary>
    /// The style for the control point segments
    /// </summary>
    /// <remarks>We just use a polyline edge style with a custom render so that we can reuse most of the existing rendering implementations.</remarks>
    private static readonly PolylineEdgeStyle SelectionDecoratorStyle =
        new PolylineEdgeStyle(new SelectionRenderer()) {
            Pen = new Pen(Brushes.LightGray, 1) { DashStyle = DashStyles.Dash }
        };

    private readonly ISelectionIndicatorInstaller coreImpl;
    private readonly EdgeStyleDecorationInstaller decorator;

    /// <summary>
    /// Create a new instance that decorates the <paramref name="coreImpl"/>
    /// </summary>
    /// <param name="coreImpl">The core indicator that is again decorated by this instance</param>
    public BezierSelectionIndicatorInstaller([CanBeNull] ISelectionIndicatorInstaller coreImpl) {
      this.coreImpl = coreImpl;
      decorator = new EdgeStyleDecorationInstaller {
          EdgeStyle = SelectionDecoratorStyle, ZoomPolicy = StyleDecorationZoomPolicy.ViewCoordinates
      };
    }

    /// <summary>
    /// Combines the rendering by the wrapped core indicator with our own control segment rendering
    /// </summary>
    /// <returns>A canvas object group combining both renderings</returns>
    public ICanvasObject AddCanvasObject(ICanvasContext context, ICanvasObjectGroup group, object item) {
      var newGroup = group.AddGroup();
      //Add the visualization from the core selection decorator
      if (coreImpl != null) {
        coreImpl.AddCanvasObject(context, newGroup, item);
      }
      //Ad our own decoration on top
      decorator.AddCanvasObject(context, newGroup, item);
      return newGroup;
    }

    /// <summary>
    /// Custom renderer that renders a line segment for collinear control point triples
    /// </summary>
    private sealed class SelectionRenderer : PolylineEdgeStyleRenderer
    {
      protected override GeneralPath CreatePath() {
        var pathPoints = Edge.GetPathPoints();
        var gp = new GeneralPath(pathPoints.Count + 1);
        gp.MoveTo(pathPoints[0]);

        for (int i = 1; i < pathPoints.Count; ++i) {
          if (i % 3 == 2) {
            //Skip to the next triple
            gp.MoveTo(pathPoints[i]);
          } else {
            //Draw a line to the next control pint in the triple
            gp.LineTo(pathPoints[i]);
          }
        }
        return gp;
      }

      protected override GeneralPath CropPath(GeneralPath path) {
        //Don't crop
        return path;
      }
    }
  }
}
