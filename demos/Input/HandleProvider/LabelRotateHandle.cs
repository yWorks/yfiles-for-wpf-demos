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

using System.Windows.Input;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Input.HandleProvider
{
  /// <summary>
  ///   A custom <see cref="IHandle"/> implementation that implements the functionality
  ///   needed for rotating a label.
  /// </summary>
  public class LabelRotateHandle : IHandle
  {
    private readonly ILabel label;

    // whether to use the temporary dummy bounds or the actual label bounds
    private bool emulate;

    // the dummy bounds for displaying the size indicator during a resize gesture
    private PointD dummyLocation;

    private PointD up;

    // the render tree element representing the rotation indicator
    private IRenderTreeElement rotationIndicator;
    private PointD rotationCenter;

    private IInputModeContext inputModeContext;

    public LabelRotateHandle(ILabel label, IInputModeContext context) {
      this.label = label;
      inputModeContext = context;
    }

    public HandleType Type => HandleType.Move;

    public object Tag => null;

    public Cursor Cursor => Cursors.Cross;

    public void HandleClick(ClickEventArgs eventArgs) {
      // ignore clicks
    }

    private IPoint location;

    public IPoint Location => location ??= new LabelRotateHandleLivePoint(this);

    public void InitializeDrag(IInputModeContext context) {
      inputModeContext = context;
      // start using the calculated dummy bounds
      emulate = true;
      var labelLayout = label.GetLayout();
      dummyLocation = labelLayout.GetAnchor();
      up = labelLayout.GetUpVector();

      rotationCenter = labelLayout.GetCenter();
      if (context.CanvasControl is GraphControl graphControl) {
        // install the visual rotation indicator in the SelectionGroup of the canvas
        var renderer = new OrientedRectangleRenderer<LabelRotateHandle>(handle => handle.GetCurrentLabelLayout()) {
            Pen = new Pen(Brushes.Gray, 2)
        };
        var renderTree = graphControl.RenderTree;
        rotationIndicator = renderTree.CreateElement(renderTree.SelectionGroup, this, renderer);
      }
    }

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // calculate the up vector
      up = (newLocation - rotationCenter).Normalized;

      // and the anchor point
      var preferredSize = label.PreferredSize;

      var p2X = -preferredSize.Width * 0.5;
      var p2Y = preferredSize.Height * 0.5;

      var anchorX = rotationCenter.X - p2X * up.Y - p2Y * up.X;
      var anchorY = rotationCenter.Y - p2Y * up.Y + p2X * up.X;

      // calculate the new location
      dummyLocation = new PointD(anchorX, anchorY);
    }

    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      // use the normal label bounds if the drag gesture is over
      emulate = false;
      // remove the visual rotation indicator
      if (rotationIndicator != null) {
        context.CanvasControl!.RenderTree.Remove(rotationIndicator);
        rotationIndicator = null;
      }
    }

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

    private OrientedRectangle GetCurrentLabelLayout() {
      var preferredSize = label.PreferredSize;
      var labelLayout = label.GetLayout();
      return new OrientedRectangle(emulate ? dummyLocation.X : labelLayout.AnchorX,
          emulate ? dummyLocation.Y : labelLayout.AnchorY, preferredSize.Width, preferredSize.Height, up.X, up.Y);
    }

    #region live location

    private class LabelRotateHandleLivePoint : IPoint
    {
      private readonly LabelRotateHandle outerThis;

      public LabelRotateHandleLivePoint(LabelRotateHandle outerThis) {
        this.outerThis = outerThis;
      }

      public double X {
        get {
          var preferredSize = outerThis.label.PreferredSize;
          var labelLayout = outerThis.label.GetLayout();
          var anchor = outerThis.emulate ? outerThis.dummyLocation : labelLayout.GetAnchor();
          var up = outerThis.emulate ? outerThis.up : labelLayout.GetUpVector();
          // calculate the location of the handle from the anchor, the size and the orientation
          var offset = outerThis.inputModeContext != null ? 27 / outerThis.inputModeContext.CanvasControl.Zoom : 27;
          return anchor.X + up.X * (preferredSize.Height + offset) + -up.Y * (preferredSize.Width * 0.5);
        }
      }

      public double Y {
        get {
          var preferredSize = outerThis.label.PreferredSize;
          var labelLayout = outerThis.label.GetLayout();
          var anchor = outerThis.emulate ? outerThis.dummyLocation : labelLayout.GetAnchor();
          var up = outerThis.emulate ? outerThis.up : labelLayout.GetUpVector();
          // calculate the location of the handle from the anchor, the size and the orientation
          var offset = outerThis.inputModeContext != null ? 27 / outerThis.inputModeContext.CanvasControl.Zoom : 27;
          return anchor.Y + up.Y * (preferredSize.Height + offset) + up.X * (preferredSize.Width * 0.5);
        }
      }
    }

    #endregion
  }
}
