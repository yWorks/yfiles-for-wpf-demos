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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace Demo.yFiles.Graph.Input.LensInputMode
{
  /// <summary>
  /// A specialized <see cref="IInputMode"/> that displays the currently hovered-over
  /// part of the graph in some kind of magnifying glass.
  /// </summary>
  public class LensInputMode : InputModeBase
  {
    /// <summary>
    /// The <see cref="IVisualCreator"/> that displays the magnifying <see cref="GraphControl"/>.
    /// </summary>
    private sealed class LensVisualCreator : IVisualCreator
    {
      private LensInputMode inputMode;

      public LensVisualCreator(LensInputMode inputMode) {
        this.inputMode = inputMode;
      }

      public Visual CreateVisual(IRenderContext context) {
        var canvasControl = context.CanvasControl as GraphControl;

        // Instantiate the GraphControl displaying the zoomed graph.
        var lensGraphControl = new GraphControl
        {
          // Re-use the same graph, selection, and projection
          Graph = canvasControl.Graph,
          Selection = canvasControl.Selection,
          Projection = canvasControl.Projection,

          // Disable interaction and scrollbars
          MouseWheelBehavior = MouseWheelBehaviors.None,
          AutoDrag = false,
          IsHitTestVisible = false,
          HorizontalScrollBarPolicy = ScrollBarVisibility.Hidden,
          VerticalScrollBarPolicy = ScrollBarVisibility.Hidden,
          // This is only necessary to show handles in the zoomed graph. Remove if not needed
          InputMode = new GraphEditorInputMode(),

          // Clip the overlay to circular
          Clip = new EllipseGeometry(),
          Background = Brushes.White
        };

        // Arrange both the GraphControl and an outline
        var grid = new Grid
        {
          Children =
          {
            lensGraphControl,
            new Ellipse { Stroke = Brushes.Gray, StrokeThickness = 2 }
          }
        };

        // VisualGroup for easier handling 
        var visualGroup = new VisualGroup {
          Children = { grid }
        };

        // Update all properties to their current values and ensure that the overlay is rendered where it should
        return UpdateVisual(context, visualGroup);
      }

      public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        var canvasControl = context.CanvasControl as GraphControl;

        var visualGroup = (VisualGroup) oldVisual;
        var grid = (Grid) visualGroup.Children[0];
        var lensGraphControl = (GraphControl) grid.Children[0];

        // Set where the overlay should appear
        var arrangeRect = new RectD(context.ToViewCoordinates(canvasControl.LastEventLocation), inputMode.Size);
        grid.SetCanvasArrangeRect(arrangeRect);

        // Update center, zoom, and projection
        lensGraphControl.Center = canvasControl.LastEventLocation;
        lensGraphControl.Zoom = inputMode.ZoomFactor * context.Zoom;
        lensGraphControl.Projection = context.Projection;

        // Ensure that the overlay is displayed in view coordinates
        visualGroup.Transform = context.ViewTransform;

        // Update the clipping path
        var ellipseGeometry = (EllipseGeometry) lensGraphControl.Clip;
        var halfWidth = inputMode.Size.Width / 2;
        var halfHeight = inputMode.Size.Height / 2;
        ellipseGeometry.RadiusX = halfWidth;
        ellipseGeometry.RadiusY = halfHeight;
        ellipseGeometry.Center = new Point(halfWidth, halfHeight);

        visualGroup.Visibility = canvasControl.IsMouseOver && inputMode.Enabled ? Visibility.Visible : Visibility.Collapsed;

        return oldVisual;
      }
    }

    /// <summary>
    /// The <see cref="ICanvasObjectGroup"/> containing the lens graph control.
    /// </summary>
    private ICanvasObjectGroup lensGroup;

    private Size size = new Size(250, 250);
    private double zoomFactor = 3;

    /// <summary>
    /// The size of the "magnifying glass".
    /// </summary>
    public Size Size {
      get { return size; }
      set {
        size = value;
        InputModeContext?.CanvasControl?.Invalidate();
      }
    }

    /// <summary>
    /// The zoom factor used for magnifying the graph.
    /// </summary>
    public double ZoomFactor {
      get { return zoomFactor; }
      set {
        zoomFactor = value;
        InputModeContext?.CanvasControl?.Invalidate();
      }
    }

    /// <summary>
    /// Installs the <see cref="LensInputMode"/> by adding the <see cref="LensVisualCreator"/>
    /// to the <see cref="lensGroup"/> and registering the necessary mouse event handlers.
    /// </summary>
    public override void Install(IInputModeContext context, ConcurrencyController controller) {
      base.Install(context, controller);

      var canvasControl = context.CanvasControl as GraphControl;

      lensGroup = canvasControl.RootGroup.AddGroup();
      lensGroup.Above(canvasControl.InputModeGroup);
      lensGroup.AddChild(new LensVisualCreator(this));

      canvasControl.Mouse2DMoved += UpdateLensLocation;
      canvasControl.Mouse2DDragged += UpdateLensLocation;

      canvasControl.Mouse2DEntered += UpdateLensLocation;
      canvasControl.Mouse2DExited += UpdateLensLocation;
    }

    /// <summary>
    /// Uninstalls the <see cref="LensInputMode"/> by removing the <see cref="lensGroup"/>
    /// and unregistering the various mouse event handlers.
    /// </summary>
    /// <param name="context"></param>
    public override void Uninstall(IInputModeContext context) {
      var canvasControl = context.CanvasControl;

      lensGroup.Remove();

      canvasControl.Mouse2DMoved -= UpdateLensLocation;
      canvasControl.Mouse2DDragged -= UpdateLensLocation;

      canvasControl.Mouse2DEntered -= UpdateLensLocation;
      canvasControl.Mouse2DExited -= UpdateLensLocation;

      base.Uninstall(context);
    }

    private void UpdateLensLocation(object sender, Mouse2DEventArgs args) {
      InputModeContext.CanvasControl.Invalidate();
    }
  }
}
