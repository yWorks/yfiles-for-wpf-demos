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

using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Utils;

namespace yWorks.Demo.LabelHandleProvider
{
  /// <summary>
  ///   A custom <see cref="IHandle"/> implementation that implements the functionality needed for rotating a label.
  /// </summary>
  internal sealed class LabelRotateHandle : IHandle, IVisualCreator
  {
    private readonly GraphControl graphControl;
    private readonly ILabel label;
    private IInputModeContext inputModeContext;
    private PointD dummyLocation;
    private PointD up;
    private PointD rotationCenter;
    private bool emulate;
    private IRenderTreeElement? rotationIndicator;

    /// <summary>
    ///   Creates a rotate handle for the given label.
    /// </summary>
    /// <param name="graphControl">The graph control whose render tree visualizes the rotation.</param>
    /// <param name="label">The given label.</param>
    /// <param name="context">The context to retrieve information.</param>
    public LabelRotateHandle(GraphControl graphControl, ILabel label, IInputModeContext context) {
      this.graphControl = graphControl;
      this.label = label;
      inputModeContext = context;
      Location = new LabelRotateHandleLivePoint(this);
    }

    /// <summary>Gets the type of the handle.</summary>
    public HandleType Type => HandleType.Custom3;

    /// <summary>Returns the cursor object.</summary>
    public Cursor Cursor => Cursors.Cross;

    /// <summary>Gets an optional tag object associated with the handle.</summary>
    public object? Tag => null;

    /// <summary>Returns the handle's location.</summary>
    public IPoint Location { get; }

    /// <summary>Invoked when dragging is about to start.</summary>
    public void InitializeDrag(IInputModeContext context) {
      inputModeContext = context;
      // start using the calculated dummy bounds
      emulate = true;
      var labelLayout = label.GetLayout();
      dummyLocation = labelLayout.GetAnchor();
      up = labelLayout.GetUpVector();
      rotationCenter = labelLayout.GetCenter();
      rotationIndicator = graphControl.RenderTree.CreateElement(graphControl.RenderTree.SelectionGroup, this);
    }

    /// <summary>
    ///   Invoked when an element has been dragged and its position should be updated.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // calculate the up vector
      up = (newLocation - rotationCenter).Normalized;

      // and the anchor point
      var preferredSize = label.PreferredSize;
      var p2X = -preferredSize.Width * 0.5;
      var p2Y = preferredSize.Height * 0.5;
      var anchorX = rotationCenter.X - p2X * up.Y - p2Y * up.X;
      var anchorY = rotationCenter.Y - (p2Y * up.Y - p2X * up.X);

      // calculate the new location
      dummyLocation = new PointD(anchorX, anchorY);
    }

    /// <summary>Invoked when dragging has canceled.</summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      // use the normal label bounds if the drag gesture is over
      emulate = false;
      // remove the visual size indicator
      if (rotationIndicator != null) {
        graphControl.RenderTree.Remove(rotationIndicator);
        rotationIndicator = null;
      }
    }

    /// <summary>Invoked when dragging has finished.</summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var graph = context.GetGraph();
      if (graph != null) {
        var model = label.LayoutParameter.Model;
        var finder = model.GetContext(label).Lookup<ILabelModelParameterFinder>();
        if (finder != null) {
          var param = finder.FindBestParameter(GetCurrentLabelLayout());
          graph.SetLabelLayoutParameter(label, param);
        }
      }
      CancelDrag(context, originalLocation);
    }

    /// <summary>Returns the current label layout.</summary>
    public OrientedRectangle GetCurrentLabelLayout() {
      var preferredSize = label.PreferredSize;
      var labelLayout = label.GetLayout();
      return new OrientedRectangle(emulate ? dummyLocation.X : labelLayout.AnchorX,
          emulate ? dummyLocation.Y : labelLayout.AnchorY, preferredSize.Width, preferredSize.Height, up.X, up.Y);
    }

    /// <summary>This implementation does nothing special when clicked.</summary>
    public void HandleClick(ClickEventArgs eventArgs) { }

    /// <summary>Creates the indicator that shows the rotation angle of the label during drag.</summary>
    public Visual CreateVisual(IRenderContext context) =>
        LabelHandleVisuals.Create(GetCurrentLabelLayout(), true, Color.FromRgb(56, 67, 79), 2);

    /// <summary>Updates the indicator that shows the rotation angle of the label during drag.</summary>
    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateVisual(context);

    /// <summary>Represents the new rotate point for the given handler.</summary>
    private sealed class LabelRotateHandleLivePoint : IPoint
    {
      private readonly LabelRotateHandle handle;

      /// <summary>Creates a new point for the given handler.</summary>
      public LabelRotateHandleLivePoint(LabelRotateHandle handle) {
        this.handle = handle;
      }

      /// <summary>Returns the x-coordinate of the handle from the anchor, size, and orientation.</summary>
      public double X {
        get {
          var (anchor, up, preferredSize, offset) = GetPositionInfo();
          return anchor.X + up.X * (preferredSize.Height + offset) - up.Y * (preferredSize.Width * 0.5);
        }
      }

      /// <summary>Returns the y-coordinate of the handle from the anchor, size, and orientation.</summary>
      public double Y {
        get {
          var (anchor, up, preferredSize, offset) = GetPositionInfo();
          return anchor.Y + up.Y * (preferredSize.Height + offset) + up.X * (preferredSize.Width * 0.5);
        }
      }

      /// <summary>Prepares all relevant information needed to calculate the position of the handle.</summary>
      private (PointD Anchor, PointD Up, SizeD PreferredSize, double Offset) GetPositionInfo() {
        var preferredSize = handle.label.PreferredSize;
        var labelLayout = handle.label.GetLayout();
        var anchor = handle.emulate ? handle.dummyLocation : labelLayout.GetAnchor();
        var up = handle.emulate ? handle.up : labelLayout.GetUpVector();
        return (anchor, up, preferredSize, 20);
      }
    }
  }
}
