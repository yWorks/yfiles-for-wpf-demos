/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.MagnifyingGlass
{
  /// <summary>
  ///   A specialized input mode that shows a floating magnifying lens that magnifies the cursor's
  ///   surroundings.
  /// </summary>
  public sealed class LensInputMode : InputModeBase
  {
    /// <summary>
    ///   The <see cref="IVisualCreator"/> that displays the magnifying <see cref="GraphControl"/>.
    /// </summary>
    private sealed class LensVisualCreator : IVisualCreator
    {
      private readonly LensInputMode inputMode;

      public LensVisualCreator(LensInputMode inputMode) {
        this.inputMode = inputMode;
      }

      public Visual CreateVisual(IRenderContext context) {
        var canvasControl = (GraphControl) context.CanvasControl;
        if (canvasControl.LastPointerEvent.PointerType == PointerType.None) {
          // During initialization, the LastEventLocation is not set yet, so we don't display the lens.
          return null;
        }

        // Instantiate the GraphControl displaying the zoomed graph.
        var lensGraphControl = new GraphControl {
            // Re-use the same graph, selection, and projection
            Graph = canvasControl.Graph,
            Selection = canvasControl.Selection,
            Projection = canvasControl.Projection,

            // Disable interaction and scrollbars
            MouseWheelBehavior = MouseWheelBehaviors.None,
            AutoScrollOnBounds = false,
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
        var grid = new Grid {
            Children = {
                lensGraphControl,
                new Ellipse {
                    Stroke = Brushes.Gray,
                    StrokeThickness = 2
                }
            }
        };

        // VisualGroup for easier handling
        var visualGroup = new VisualGroup { Children = { grid } };

        // Remove the shared graph when the visual is removed from the view to avoid memory leaks.
        context.SetDisposeCallback(visualGroup, (_, _, _) => {
          lensGraphControl.ClearValue(GraphControl.GraphProperty);
          return null;
        });

        // Update all properties to their current values and ensure that the overlay is rendered where it should
        return UpdateVisual(context, visualGroup);
      }

      public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        var canvasControl = (GraphControl) context.CanvasControl;
        if (canvasControl.LastPointerEvent.PointerType == PointerType.None) {
          context.ChildVisualRemoved(oldVisual);
          return null;
        }
        var lastEventLocation = canvasControl.LastPointerEvent.Location;

        var visualGroup = (VisualGroup) oldVisual;
        var grid = (Grid) visualGroup.Children[0];
        var lensGraphControl = (GraphControl) grid.Children[0];

        // Set where the overlay should appear
        var size = inputMode.Size;
        var arrangeRect = new RectD(context.WorldToViewCoordinates(lastEventLocation), size);
        grid.SetCanvasArrangeRect(arrangeRect.ToRect());

        // Update center, zoom, and projection
        lensGraphControl.Center = lastEventLocation;
        lensGraphControl.Zoom = inputMode.ZoomFactor * context.Zoom;
        lensGraphControl.Projection = context.Projection;

        // Ensure that the overlay is displayed in view coordinates
        visualGroup.Transform = context.ViewTransform;

        // Update the clipping path
        var ellipseGeometry = (EllipseGeometry) lensGraphControl.Clip;
        var halfWidth = size.Width / 2;
        var halfHeight = size.Height / 2;
        ellipseGeometry.RadiusX = halfWidth;
        ellipseGeometry.RadiusY = halfHeight;
        ellipseGeometry.Center = new Point(halfWidth, halfHeight);

        visualGroup.Visibility =
            canvasControl.IsMouseOver && inputMode.Enabled ? Visibility.Visible : Visibility.Collapsed;

        return oldVisual;
      }
    }

    /// <summary>
    ///   The <see cref="IRenderTreeGroup"/> containing the lens graph control.
    /// </summary>
    private IRenderTreeGroup lensGroup;

    private double zoomFactor = 2;
    private SizeD size = new(250, 250);

    /// <summary>
    ///   The size of the "magnifying glass".
    /// </summary>
    public SizeD Size {
      get => size;
      set {
        size = value;
        ParentInputModeContext?.CanvasControl.Invalidate();
      }
    }

    /// <summary>
    ///   Returns the zoom factor of the graphControl of the LensInputMode.
    /// </summary>
    public double ZoomFactor {
      get => zoomFactor;
      set {
        zoomFactor = value;
        ParentInputModeContext?.CanvasControl.Invalidate();
      }
    }

    /// <summary>
    ///   Installs this LensInputMode.
    /// </summary>
    /// <param name="context">The context to install this mode into.</param>
    /// <param name="controller">The controller for this mode.</param>
    public override void Install(IInputModeContext context, ConcurrencyController controller) {
      base.Install(context, controller);

      var graphControl = (GraphControl) context.CanvasControl;

      var renderTree = graphControl.RenderTree;
      lensGroup = renderTree.CreateGroup(renderTree.RootGroup);
      lensGroup.Above(renderTree.InputModeGroup);
      renderTree.CreateElement(lensGroup, new LensVisualCreator(this));

      graphControl.PointerMove += UpdateLensLocation;
      graphControl.PointerDrag += UpdateLensLocation;

      graphControl.PointerEnter += UpdateLensLocation;
      graphControl.PointerLeave += UpdateLensLocation;

      graphControl.ZoomChanged += UpdateLensVisibility;
    }

    /// <summary>
    ///   Uninstalls this LensInputMode.
    /// </summary>
    /// <param name="context">The context to remove this mode from.</param>
    public override void Uninstall(IInputModeContext context) {
      var canvasControl = context.CanvasControl;
      canvasControl.RenderTree.Remove(lensGroup);

      canvasControl.ZoomChanged -= UpdateLensVisibility;

      canvasControl.PointerMove -= UpdateLensLocation;
      canvasControl.PointerDrag -= UpdateLensLocation;

      canvasControl.PointerEnter -= UpdateLensLocation;
      canvasControl.PointerLeave -= UpdateLensLocation;

      base.Uninstall(context);
    }

    /// <summary>
    ///   Determines whether the lens element should be visible or not.
    /// </summary>
    /// <remarks>
    ///   Normally, the magnifying glass should be hidden when the zoom level of the graphComponent is
    ///   larger than 0.7 or if the size of the graphComponent is small.
    /// </remarks>
    private void UpdateLensVisibility(
        object sender,
        DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      var canvasControl = ParentInputModeContext?.CanvasControl;
      if (canvasControl != null && canvasControl.Zoom < 0.7 && canvasControl.ActualWidth > 200 &&
          canvasControl.ActualHeight > 200 && canvasControl.LastPointerEvent != PointerEventArgs.Empty &&
          canvasControl.Viewport.Contains(canvasControl.LastPointerEvent.Location)) {
        lensGroup?.Visible = true;
      } else {
        lensGroup?.Visible = false;
      }
    }

    private void UpdateLensLocation(object? sender, PointerEventArgs args) {
      ParentInputModeContext?.CanvasControl.Invalidate();
    }
  }
}
