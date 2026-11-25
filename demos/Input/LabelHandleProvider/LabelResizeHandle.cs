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

using System;
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
  ///   A custom <see cref="IHandle"/> implementation that allows resizing a label.
  /// </summary>
  internal sealed class LabelResizeHandle : IHandle, IVisualCreator
  {
    private readonly GraphControl graphControl;
    private readonly bool symmetricResize;
    private readonly ILabel label;
    private IRenderTreeElement? sizeIndicator;
    private bool emulate;
    private SizeD dummyPreferredSize;
    private PointD dummyLocation;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="graphControl">The graph control whose render tree visualizes the size.</param>
    /// <param name="label">The label to resize.</param>
    /// <param name="symmetricResize">A value indicating whether resizing should be symmetric.</param>
    public LabelResizeHandle(GraphControl graphControl, ILabel label, bool symmetricResize) {
      this.graphControl = graphControl;
      this.label = label;
      this.symmetricResize = symmetricResize;
      Location = new LabelResizeHandleLivePoint(this);
    }

    /// <summary>Gets the type of the handle.</summary>
    public HandleType Type => HandleType.Resize;

    /// <summary>Gets an optional tag object associated with the handle.</summary>
    public object? Tag => null;

    /// <summary>Gets the handle's cursor.</summary>
    public Cursor Cursor => Cursors.SizeWE;

    /// <summary>Returns the handle's location.</summary>
    public IPoint Location { get; }

    /// <summary>Invoked when dragging is about to start.</summary>
    public void InitializeDrag(IInputModeContext context) {
      // start using the calculated dummy bounds
      emulate = true;
      dummyPreferredSize = label.PreferredSize;
      var layout = label.GetLayout();
      dummyLocation = layout.GetAnchor();
      sizeIndicator = graphControl.RenderTree.CreateElement(graphControl.RenderTree.SelectionGroup, this);
    }

    /// <summary>
    ///   Invoked when an element has been dragged and its position should be updated.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var layout = label.GetLayout();
      // the normal (orthogonal) vector of the 'up' vector
      var upNormal = new PointD(-layout.UpY, layout.UpX);

      // calculate the total distance the handle has been moved in this drag gesture
      var delta = upNormal.ScalarProduct(newLocation - originalLocation);

      // max with minus half the label size - because the label width can't shrink below zero
      delta = Math.Max(delta, -layout.Width * (symmetricResize ? 0.5 : 1));

      // add one or two times delta to the width to expand the label right and left
      var newWidth = layout.Width + delta * (symmetricResize ? 2 : 1);
      dummyPreferredSize = new SizeD(newWidth, dummyPreferredSize.Height);
      // calculate the new location
      dummyLocation = layout.GetAnchor() - (symmetricResize ? upNormal * delta : PointD.Origin);
    }

    /// <summary>Invoked when dragging has canceled.</summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      // use the normal label bounds if the drag gesture is over
      emulate = false;
      // remove the visual size indicator
      if (sizeIndicator != null) {
        graphControl.RenderTree.Remove(sizeIndicator);
        sizeIndicator = null;
      }
    }

    /// <summary>Invoked when dragging has finished.</summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var graph = context.GetGraph();
      if (graph != null) {
        // assign the new size
        graph.SetLabelPreferredSize(label, dummyPreferredSize);

        // Use the layout of the resize rectangle to find a new labelLayoutParameter. This ensures
        // that the resize rectangle which acts as user feedback is in sync with the actual
        // labelLayoutParameter that is assigned to the label.
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
    private OrientedRectangle GetCurrentLabelLayout() {
      var labelLayout = label.GetLayout();
      if (emulate) {
        return new OrientedRectangle(dummyLocation.X, dummyLocation.Y, dummyPreferredSize.Width,
            dummyPreferredSize.Height, labelLayout.UpX, labelLayout.UpY);
      }
      return new OrientedRectangle(labelLayout.AnchorX, labelLayout.AnchorY, label.PreferredSize.Width,
          label.PreferredSize.Height, labelLayout.UpX, labelLayout.UpY);
    }

    /// <summary>This implementation does nothing special when clicked.</summary>
    public void HandleClick(ClickEventArgs eventArgs) { }

    /// <summary>Creates the indicator that shows the size of the label during drag.</summary>
    public Visual CreateVisual(IRenderContext context) =>
        LabelHandleVisuals.Create(GetCurrentLabelLayout(), false, Color.FromRgb(56, 67, 79), 2);

    /// <summary>Updates the indicator that shows the size of the label during drag.</summary>
    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateVisual(context);

    /// <summary>Represents the new resize point for the given handler.</summary>
    private sealed class LabelResizeHandleLivePoint : IPoint
    {
      private readonly LabelResizeHandle handle;

      /// <summary>Creates a new point for the given handler.</summary>
      public LabelResizeHandleLivePoint(LabelResizeHandle handle) {
        this.handle = handle;
      }

      /// <summary>Returns the x-coordinate of the handle from the anchor, size, and orientation.</summary>
      public double X {
        get {
          var (anchor, up, preferredSize) = GetPositionInfo();
          return anchor.X + (up.X * preferredSize.Height * 0.5 - up.Y * preferredSize.Width);
        }
      }

      /// <summary>Returns the y-coordinate of the handle from the anchor, size, and orientation.</summary>
      public double Y {
        get {
          var (anchor, up, preferredSize) = GetPositionInfo();
          return anchor.Y + (up.Y * preferredSize.Height * 0.5 + up.X * preferredSize.Width);
        }
      }

      /// <summary>Prepares all relevant information needed to calculate the position of the handle.</summary>
      private (PointD Anchor, PointD Up, SizeD PreferredSize) GetPositionInfo() {
        var layout = handle.label.GetLayout();
        var up = layout.GetUpVector();
        var preferredSize = handle.emulate ? handle.dummyPreferredSize : handle.label.PreferredSize;
        var anchor = handle.emulate ? handle.dummyLocation : layout.GetAnchor();
        return (anchor, up, preferredSize);
      }
    }
  }
}
