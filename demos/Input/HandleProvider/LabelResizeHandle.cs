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

using System;
using System.Windows;
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.Input.HandleProvider {

  /// <summary>
  /// A custom <see cref="IHandle"/> implementation that implements the functionality 
  /// needed for resizing a label.
  /// </summary>
  public class LabelResizeHandle : IHandle
  {
    private readonly ILabel label;
    // whether to use the temporary dummy bounds or the actual label bounds
    private bool emulate;
    // the dummy bounds for displaying the size indicator during a resize gesture
    private SizeD dummyPreferredSize;
    private PointD dummyLocation;

    private readonly bool symmetricResize = true;
    private ICanvasObject sizeIndicator;

    public LabelResizeHandle(ILabel label, bool symmetricResize) {
      this.label = label;
      this.symmetricResize = symmetricResize;
    }

    public bool SymmetricResize {
      get { return symmetricResize; }
    }

    public HandleTypes Type {
      get { return HandleTypes.Resize; }
    }

    public Cursor Cursor {
      get { return Cursors.SizeWE; }
    }

    public void HandleClick(ClickEventArgs eventArgs) {
      // ignore clicks
    }

    private IPoint location;

    public IPoint Location {
      get {
        if (location == null) {
          location = new LabelResizeHandleLivePoint(this);
        }
        return location;
      }
    }

    public void InitializeDrag(IInputModeContext context) {
      // start using the calculated dummy bounds
      emulate = true;
      dummyPreferredSize = label.PreferredSize;
      dummyLocation = label.GetLayout().GetAnchorLocation();
      var canvasControl = context.CanvasControl;
      if (canvasControl != null) {
        // install the visual size indicator in the SelectionGroup of the canvas
        sizeIndicator = new LabelResizeIndicatorInstaller(this).AddCanvasObject(canvasControl.CanvasContext, ((GraphControl) canvasControl).SelectionGroup, this);
      }
    }

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var layout = label.GetLayout();
      // the normal (orthogonal) vector of the 'up' vector
      var upNormal = new PointD(-layout.UpY, layout.UpX);

      // calculate the total distance the handle has been moved in this drag gesture
      var delta = upNormal.ScalarProduct(newLocation - originalLocation);

      // max with minus half the label size - because the label width can't shrink below zero
      delta = Math.Max(delta, -layout.Width*(symmetricResize?0.5:1));

      // add one or two times delta to the width to expand the label right and left
      var newWidth = layout.Width + delta*(symmetricResize?2:1);
      dummyPreferredSize = new SizeD(newWidth, dummyPreferredSize.Height);
      // calculate the new location
      dummyLocation = layout.GetAnchorLocation() - (symmetricResize?new PointD(upNormal.X * delta, upNormal.Y * delta):new PointD());
    }

    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      // use the normal label bounds if the drag gesture is over
      emulate = false;
      // remove the visual size indicator
      if (sizeIndicator != null) {
        sizeIndicator.Remove();
        sizeIndicator = null;
      }
    }

    public void DragFinished([NotNull] IInputModeContext context, PointD originalLocation, PointD newLocation) {
      CancelDrag(context, originalLocation);
      var graph = context.GetGraph();
      if (graph != null) {
        // assign the new size
        graph.SetLabelPreferredSize(label, dummyPreferredSize);
      }
    }

    private OrientedRectangle GetCurrentLabelLayout() {
      var labelLayout = label.GetLayout();
      if (emulate) {
        return new OrientedRectangle(dummyLocation.X, dummyLocation.Y, dummyPreferredSize.Width, dummyPreferredSize.Height, labelLayout.UpX, labelLayout.UpY);
      } else {
        return new OrientedRectangle(labelLayout.AnchorX, labelLayout.AnchorY, label.PreferredSize.Width, label.PreferredSize.Height, labelLayout.UpX, labelLayout.UpY);
      }
    }

    #region live location

    private class LabelResizeHandleLivePoint : IPoint
    {
      private readonly LabelResizeHandle outerThis;

      public LabelResizeHandleLivePoint(LabelResizeHandle outerThis) {
        this.outerThis = outerThis;
      }

      public double X {
        get {
          var layout = outerThis.label.GetLayout();
          var up = layout.GetUp();
          var preferredSize = outerThis.emulate ? outerThis.dummyPreferredSize : outerThis.label.PreferredSize;
          var anchor = outerThis.emulate ? outerThis.dummyLocation : layout.GetAnchorLocation();
          // calculate the location of the handle from the anchor, the size and the orientation
          return anchor.X + up.X*preferredSize.Height*0.5 + -up.Y*preferredSize.Width;
        }
      }

      public double Y {
        get {
          var layout = outerThis.label.GetLayout();
          var up = layout.GetUp();
          var preferredSize = outerThis.emulate ? outerThis.dummyPreferredSize : outerThis.label.PreferredSize;
          var anchor = outerThis.emulate ? outerThis.dummyLocation : layout.GetAnchorLocation();
          // calculate the location of the handle from the anchor, the size and the orientation
          return anchor.Y + up.Y*preferredSize.Height*0.5 + up.X*preferredSize.Width;
        }
      }
    }

    #endregion

    private class LabelResizeIndicatorInstaller : OrientedRectangleIndicatorInstaller {

      private readonly LabelResizeHandle outerThis;

      public LabelResizeIndicatorInstaller(LabelResizeHandle outerThis) : base(null, SelectionTemplateKey) {
        this.outerThis = outerThis;
      }

      protected override IOrientedRectangle GetRectangle(object item) {
        return outerThis.GetCurrentLabelLayout();
      }
    }
  }
}
