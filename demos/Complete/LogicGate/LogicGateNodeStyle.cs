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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Layout.LogicGate
{
  /// <summary>
  /// A very simple implementation of an <see cref="INodeStyle"/> that displays logic gates.
  /// </summary>
  public class LogicGateNodeStyle : NodeStyleBase<VisualGroup>
  {
    // node fill
    private static readonly Color FillColor = Colors.WhiteSmoke;
    private static readonly Color OutlineColor = Colors.Black;

    private static readonly GeneralPath AndOutlinePath, OrOutlinePath, NandOutlinePath, NorOutlinePath, NotOutlinePath;

    /// <summary>
    /// Gets or sets the type of the logic gate.
    /// </summary>
    /// <value>
    /// The type of the logic gate this style should represent.
    /// </value>
    public LogicGateType GateType { get; set; }
    
    static LogicGateNodeStyle() {
      // path for AND nodes
      AndOutlinePath = new GeneralPath();
      AndOutlinePath.MoveTo(0.6, 0);
      AndOutlinePath.LineTo(0.1, 0);
      AndOutlinePath.LineTo(0.1, 1);
      AndOutlinePath.LineTo(0.6, 1);
      AndOutlinePath.QuadTo(0.8, 1.0, 0.8, 0.5);
      AndOutlinePath.QuadTo(0.8, 0.0, 0.6, 0);

      // path for OR nodes
      OrOutlinePath = new GeneralPath();
      OrOutlinePath.MoveTo(0.6, 0);
      OrOutlinePath.LineTo(0.1, 0);
      OrOutlinePath.QuadTo(0.3, 0.5, 0.1, 1);
      OrOutlinePath.LineTo(0.6, 1);
      OrOutlinePath.QuadTo(0.8, 1.0, 0.8, 0.5);
      OrOutlinePath.QuadTo(0.8, 0.0, 0.6, 0);

      // path for NAND nodes
      NandOutlinePath = new GeneralPath();
      NandOutlinePath.MoveTo(0.6, 0);
      NandOutlinePath.LineTo(0.1, 0);
      NandOutlinePath.LineTo(0.1, 1);
      NandOutlinePath.LineTo(0.6, 1);
      NandOutlinePath.QuadTo(0.8, 1.0, 0.8, 0.5);
      NandOutlinePath.QuadTo(0.8, 0.0, 0.6, 0);
      NandOutlinePath.AppendEllipse(new RectD(0.8, 0.4, 0.1, 0.2), false);

      // path for NOR nodes
      NorOutlinePath = new GeneralPath();
      NorOutlinePath.MoveTo(0.6, 0);
      NorOutlinePath.LineTo(0.1, 0);
      NorOutlinePath.QuadTo(0.3, 0.5, 0.1, 1);
      NorOutlinePath.LineTo(0.6, 1);
      NorOutlinePath.QuadTo(0.8, 1.0, 0.8, 0.5);
      NorOutlinePath.QuadTo(0.8, 0.0, 0.6, 0);
      NorOutlinePath.AppendEllipse(new RectD(0.8, 0.4, 0.1, 0.2), false);

      // path for NOT nodes
      NotOutlinePath = new GeneralPath();
      NotOutlinePath.MoveTo(0.8, 0.5);
      NotOutlinePath.LineTo(0.1, 0);
      NotOutlinePath.LineTo(0.1, 1);
      NotOutlinePath.LineTo(0.8, 0.5);
      NotOutlinePath.AppendEllipse(new RectD(0.8, 0.4, 0.1, 0.2), false);
    }

    #region Rendering

    /// <summary>
    /// Creates the visual for a node.
    /// </summary>
    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      // This implementation creates a VisualGroup and uses it for the rendering of the node.
      var visual = new VisualGroup();
      visual.SetRenderDataCache(GateType);
      // Render the node
      Render(context, node, visual);
      // set the location
      visual.SetCanvasArrangeRect(node.Layout.ToRectD());
      return visual;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      var oldType = oldVisual.GetRenderDataCache<LogicGateType>();
      if (oldType != GateType) {
        return CreateVisual(context, node);
      } else {
        // update the location
        oldVisual.SetCanvasArrangeRect(node.Layout.ToRectD());
        return oldVisual;
      }
    }

    protected override GeneralPath GetOutline(INode node) {
      var layout = node.Layout;
      return GetNodeOutlinePath().CreateGeneralPath(new Matrix2D(layout.Width, 0,0, layout.Height, layout.X, layout.Y));
    }

    /// <summary>
    /// Actually creates the visual appearance of a node.
    /// </summary>
    /// <remarks>
    /// This renders the node and the edges to the labels and adds the visuals to the <paramref name="container"/>.
    /// All items are arranged as if the node was located at (0,0). <see cref="CreateVisual"/> and <see cref="UpdateVisual"/>
    /// finally arrange the container so that the drawing is translated into the final position.
    /// </remarks>
    private void Render(IRenderContext context, INode node, VisualGroup container) {
      // paint path
      double w = node.Layout.Width;
      double h = node.Layout.Height;
      LogicGateType type = GateType;

      if (type == LogicGateType.Not) {
        var inPortLine = new Line
                           {
                             X1 = 0,
                             X2 = 0.1*w,
                             Y1 = 0.5*h,
                             Y2 = 0.5*h,
                             Stroke = Brushes.Black,
                             StrokeThickness = 3
                           };
        container.Children.Add(inPortLine);
      } else {
        // in port lines
        container.Children.Add(new Line
                                 {
                                   X1 = 0,
                                   X2 = 0.3*w,
                                   Y1 = 5,
                                   Y2 = 5,
                                   Stroke = Brushes.Black,
                                   StrokeThickness = 3
                                 });
        container.Children.Add(new Line
                                 {
                                   X1 = 0,
                                   X2 = 0.3*w,
                                   Y1 = 25,
                                   Y2 = 25,
                                   Stroke = Brushes.Black,
                                   StrokeThickness = 3
                                 });
      }

      var outline = GetNodeOutlinePath();
      var path = outline.CreatePath(
        new SolidColorBrush(FillColor),
        new Pen(new SolidColorBrush(OutlineColor), 2),
        new Matrix2D(w, 0, 0, h, 0, 0), // resize the path to fit our box
        FillMode.FillClosedFigures);

      container.Children.Add(path);


      if (type == LogicGateType.And || type == LogicGateType.Or) {
        var outPortLine = new Line
                            {
                              X1 = 0.8*w,
                              X2 = w,
                              Y1 = 0.5*h,
                              Y2 = 0.5*h,
                              Stroke = Brushes.Black,
                              StrokeThickness = 3
                            };
        container.Children.Add(outPortLine);
      } else {
        var outPortLine = new Line
                            {
                              X1 = 0.9*w,
                              X2 = w,
                              Y1 = 0.5*h,
                              Y2 = 0.5*h,
                              Stroke = Brushes.Black,
                              StrokeThickness = 3
                            };
        container.Children.Add(outPortLine);
      }
    }

    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      return node.Layout.ToRectD().GetEnlarged(context.HitTestRadius).Contains(location);
    }

    #endregion

    private GeneralPath GetNodeOutlinePath() {
      switch (GateType) {
        default:
        case LogicGateType.And:
          return AndOutlinePath;
        case LogicGateType.Nand:
          return NandOutlinePath;
        case LogicGateType.Nor:
          return NorOutlinePath;
        case LogicGateType.Not:
          return NotOutlinePath;
        case LogicGateType.Or:
          return OrOutlinePath;
      }
    }
  }
}
