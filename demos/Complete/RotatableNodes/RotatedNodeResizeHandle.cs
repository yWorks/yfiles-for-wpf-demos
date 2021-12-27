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

using System;
using System.Collections.Generic;
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// A node reshape handle that adjusts its position according to the node rotation.
  /// </summary>
  public class RotatedNodeResizeHandle : IHandle, IPoint
  {
    private readonly HandlePositions position;
    private readonly INode node;
    private readonly IReshapeHandler reshapeHandler;
    private PointD dummyLocation;
    private SizeD dummySize;
    private readonly bool symmetricResize;
    private readonly List<IHandle> portHandles;
    private readonly OrientedRectangle initialLayout;
    private RectD initialRect;

    public RotatedNodeResizeHandle(HandlePositions position, INode node, IReshapeHandler reshapeHandler, bool symmetricResize) {
      this.position = position;
      this.node = node;
      this.reshapeHandler = reshapeHandler;
      this.symmetricResize = symmetricResize;
      portHandles = new List<IHandle>();
      initialLayout = new OrientedRectangle(GetNodeBasedOrientedRectangle());
    }

    /// <summary>
    /// Returns the node rotation information.
    /// </summary>
    private CachingOrientedRectangle GetNodeBasedOrientedRectangle() {
      var wrapper = node.Style as RotatableNodeStyleDecorator;
      return wrapper != null ? wrapper.GetRotatedLayout(node) : new CachingOrientedRectangle();
    }

    /// <summary>
    /// Sets the original node bounds according to the given anchor location and size.
    /// </summary>
    private RectD SetNodeLocationAndSize(IInputModeContext inputModeContext, PointD anchor, SizeD size) {
      var graph = inputModeContext.GetGraph();
      if (graph == null) {
        return RectD.Empty;
      }
      var orientedRectangle = new OrientedRectangle(anchor.X, anchor.Y, size.Width, size.Height,
        initialLayout.UpX, initialLayout.UpY);
      var center = orientedRectangle.GetCenter();

      var layout = RectD.FromCenter(center, size);
      graph.SetNodeLayout(node, layout);
      return layout;
    }

    /// <summary>
    /// Whether or not the node is symmetrically resized.
    /// </summary>
    public bool SymmetricResize {
      get { return symmetricResize; }
    }

    /// <summary>
    /// Defines the visualization of the handle. In this case a dot that rotates nicely.
    /// </summary>
    public HandleTypes Type {
      get { return HandleTypes.Resize; }
    }

    /// <summary>
    /// The cursor visualization according to the handle position.
    /// </summary>
    public Cursor Cursor {
      get {
        var layout = GetNodeBasedOrientedRectangle();
        var angle = layout.Angle;
        var cursors = new[] { Cursors.SizeNESW, Cursors.SizeNS, Cursors.SizeNWSE, Cursors.SizeWE };
        int index;
        // Pick the right array index for the respective handle location
        switch (position) {
          case HandlePositions.NorthWest:
          case HandlePositions.SouthEast:
            index = 2;
            break;
          case HandlePositions.North:
          case HandlePositions.South:
            index = 1;
            break;
          case HandlePositions.NorthEast:
          case HandlePositions.SouthWest:
            index = 0;
            break;
          case HandlePositions.East:
          case HandlePositions.West:
            index = 3;
            break;
          default:
            return Cursors.Hand;
        }
        // Then shift the array position according to the rotation angle
        index += (int)Math.Round(angle / 45);
        index %= cursors.Length;
        if (index < 0) {
          index += cursors.Length;
        }
        return cursors[index % cursors.Length];
      }
    }

    /// <summary>
    /// The location of this handle considering the node rotation.
    /// </summary>
    public IPoint Location {
      get {
        return GetLocation(GetNodeBasedOrientedRectangle(), position);
      }
    }
    
    /// <summary>
    /// Stores the initial layout of the node in case the user cancels the resizing.
    /// </summary>
    /// <param name="inputModeContext"></param>
    public void InitializeDrag(IInputModeContext inputModeContext) {
      if (reshapeHandler != null) {
        // if there is a reshape handler: initialize to 
        // ensure proper handling of a parent group node
        reshapeHandler.InitializeReshape(inputModeContext);
      }
      initialLayout.Reshape(GetNodeBasedOrientedRectangle());
      dummyLocation = initialLayout.GetAnchorLocation();
      dummySize = initialLayout.GetSize();
      initialRect = node.Layout.ToRectD();

      portHandles.Clear();
      var portContext = new DelegatingContext(inputModeContext);
      foreach (var port in node.Ports) {
        var portHandle = new DummyPortLocationModelParameterHandle(port);
        portHandle.InitializeDrag(portContext);
        portHandles.Add(portHandle);
      }
    }

    /// <summary>
    /// Adjusts the node location and size according to the new handle location.
    /// </summary>
    public void HandleMove(IInputModeContext inputModeContext, PointD originalLocation, PointD newLocation) {
      // calculate how much the handle was moved
      var upNormal = new PointD(-initialLayout.UpY, initialLayout.UpX);
      var deltaW = GetWidthDelta(originalLocation, newLocation, upNormal);
      var up = initialLayout.GetUp();
      var deltaH = GetHeightDelta(originalLocation, newLocation, up);

      // add one or two times delta to the width to expand the node right and left
      dummySize = new SizeD(
          initialLayout.Width + deltaW * (symmetricResize ? 2 : 1),
          initialLayout.Height + deltaH * (symmetricResize ? 2 : 1));

      // Calculate the new location.
      // Depending on our handle position, a different corner of the node should stay fixed.
      if (symmetricResize) {
        var dx = upNormal.X * deltaW + up.X * deltaH;
        var dy = upNormal.Y * deltaW + up.Y * deltaH;
        dummyLocation = initialLayout.GetAnchorLocation() - new PointD(dx, dy);
      } else {
        var w = dummySize.Width - initialLayout.Width;
        var h = dummySize.Height - initialLayout.Height;
        switch (position) {
          case HandlePositions.NorthWest:
            dummyLocation = initialLayout.GetAnchorLocation()
                            - new PointD(-up.Y * w, up.X * w);
            break;
          case HandlePositions.South:
          case HandlePositions.SouthWest:
          case HandlePositions.West:
            dummyLocation = initialLayout.GetAnchorLocation()
                            - new PointD(up.X * h - up.Y * w, up.Y * h + up.X * w);
            break;
          case HandlePositions.SouthEast:
            dummyLocation = initialLayout.GetAnchorLocation()
                            - new PointD(up.X * h, up.Y * h);
            break;
          // case HandlePositions.North:
          // case HandlePositions.NorthEast:
          // case HandlePositions.East:
          default:
            dummyLocation = initialLayout.GetAnchorLocation();
            break;
        }
      }

      var newLayout = SetNodeLocationAndSize(inputModeContext, dummyLocation, dummySize);

      var portContext = new DelegatingContext(inputModeContext);
      foreach (var portHandle in portHandles) {
        portHandle.HandleMove(portContext, dummyLocation, newLocation);
      }
      if (reshapeHandler != null) {
        // if there is a reshape handler: 
        // ensure proper handling of a parent group node
        reshapeHandler.HandleReshape(inputModeContext, initialRect, newLayout);
      }
    }

    /// <summary>
    /// Returns the delta by which the width of the node was changed.
    /// </summary>
    private double GetWidthDelta(PointD originalLocation, PointD newLocation, PointD vector) {
      switch (position) {
        case HandlePositions.NorthWest:
        case HandlePositions.West:
        case HandlePositions.SouthWest:
          // calculate the total distance the handle has been moved in this drag gesture
          // max with minus half the node size - because the node can't shrink below zero
          return Math.Max(vector.ScalarProduct(originalLocation - newLocation),
              -initialLayout.Width * (symmetricResize ? 0.5 : 1));
        case HandlePositions.NorthEast:
        case HandlePositions.East:
        case HandlePositions.SouthEast:
          return Math.Max(vector.ScalarProduct(newLocation - originalLocation),
              -initialLayout.Width * (symmetricResize ? 0.5 : 1));
        default:
          return 0.0;
      }
    }

    /// <summary>
    /// Returns the delta by which the height of the node was changed.
    /// </summary>
    private double GetHeightDelta(PointD originalLocation, PointD newLocation, PointD vector) {
      switch (position) {
        case HandlePositions.NorthWest:
        case HandlePositions.North:
        case HandlePositions.NorthEast:
          return Math.Max(vector.ScalarProduct(newLocation - originalLocation),
              -initialLayout.Height * (symmetricResize ? 0.5 : 1));
        case HandlePositions.SouthWest:
        case HandlePositions.South:
        case HandlePositions.SouthEast:
          return Math.Max(vector.ScalarProduct(originalLocation - newLocation),
              -initialLayout.Height * (symmetricResize ? 0.5 : 1));
        default:
          return 0.0;
      }
    }

    /// <summary>
    /// Restores the original node layout.
    /// </summary>
    public void CancelDrag(IInputModeContext inputModeContext, PointD originalLocation) {
      SetNodeLocationAndSize(inputModeContext, initialLayout.GetAnchorLocation(), initialLayout.GetSize());
      var portContext = new DelegatingContext(inputModeContext);
      foreach (var portHandle in portHandles) {
        portHandle.CancelDrag(portContext, originalLocation);
      }
      portHandles.Clear();
      if (reshapeHandler != null) {
        // if there is a reshape handler: 
        // ensure proper handling of a parent group node
        reshapeHandler.CancelReshape(inputModeContext, initialRect);
      }
    }

    /// <summary>
    /// Applies the new node layout.
    /// </summary>
    public void DragFinished(IInputModeContext inputModeContext, PointD originalLocation, PointD newLocation) {
      var newLayout = SetNodeLocationAndSize(inputModeContext, dummyLocation, dummySize);
      var portContext = new DelegatingContext(inputModeContext);
      foreach (var portHandle in portHandles) {
        portHandle.DragFinished(portContext, originalLocation, newLocation);
      }
      portHandles.Clear();
      if (reshapeHandler != null) {
        // if there is a reshape handler: 
        // ensure proper handling of a parent group node
        reshapeHandler.ReshapeFinished(inputModeContext, initialRect, newLayout);
      }
    }

    /// <summary>
    /// Gets the location that is specified by the given ratios.
    /// </summary>
    private static PointD GetLocation(IOrientedRectangle rectangle, double ratioWidth, double ratioHeight) {
      var x1 = rectangle.AnchorX;
      var y1 = rectangle.AnchorY;

      var upX = rectangle.UpX;
      var upY = rectangle.UpY;

      var w = rectangle.Width * ratioWidth;
      var h = rectangle.Height * ratioHeight;
      var x2 = x1 + upX * h - upY * w;
      var y2 = y1 + upY * h + upX * w;
      return new PointD(x2, y2);
    }

    #region IPoint members

    /// <summary>
    /// Returns the x-coordinate of the rotated bounds.
    /// </summary>
    public double X {
      get { return GetLocation(GetNodeBasedOrientedRectangle(), position).X; }
    }

    /// <summary>
    /// Returns the y-coordinate of the rotated bounds.
    /// </summary>
    public double Y {
      get { return GetLocation(GetNodeBasedOrientedRectangle(), position).Y; }
    }

    /// <summary>
    /// Returns the location of the specified position on the border of the oriented rectangle.
    /// </summary>
    private PointD GetLocation(IOrientedRectangle layout, HandlePositions position) {
      if (layout == null) {
        return node.Layout.ToPointD();
      }
      switch (position) {
        case HandlePositions.NorthWest:
          return GetLocation(layout, 0.0, 1.0);
        case HandlePositions.North:
          return GetLocation(layout, 0.5, 1.0);
        case HandlePositions.NorthEast:
          return GetLocation(layout, 1.0, 1.0);
        case HandlePositions.East:
          return GetLocation(layout, 1.0, 0.5);
        case HandlePositions.SouthEast:
          return GetLocation(layout, 1.0, 0.0);
        case HandlePositions.South:
          return GetLocation(layout, 0.5, 0.0);
        case HandlePositions.SouthWest:
          return layout.GetAnchorLocation();
        case HandlePositions.West:
          return GetLocation(layout, 0.0, 0.5);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    #endregion
  }

  /// <summary>
  /// A context that returns no SnapContext in its lookup and delegates its other methods to an inner context.
  /// </summary>
  internal class DelegatingContext : IInputModeContext
  {
    private readonly IInputModeContext context;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    internal DelegatingContext(IInputModeContext context) {
      this.context = context;
    }

    /// <summary>
    /// The wrapped context's zoom.
    /// </summary>
    public double Zoom {
      get { return context.Zoom; }
    }

    /// <summary>
    /// The wrapped context's hit test radius.
    /// </summary>
    public double HitTestRadius {
      get { return context.HitTestRadius; }
    }

    /// <summary>
    /// The wrapped context's canvas component.
    /// </summary>
    public CanvasControl CanvasControl {
      get { return context.CanvasControl; }
    }

    /// <summary>
    /// The wrapped context's parent input mode.
    /// </summary>
    public IInputMode ParentInputMode {
      get { return context.ParentInputMode; }
    }

    /// <summary>
    /// Delegates to the wrapped context's lookup but cancels the snap context.
    /// </summary>
    public object Lookup(Type type) {
      return type == typeof(SnapContext) ? null : context.Lookup(type);
    }
  }

  /// <summary>
  /// This port handle is used only to trigger the updates of the orthogonal edge editing facility of yFiles.
  /// </summary>
  /// <remarks>
  /// In yFiles, all code related to updates of the orthogonal edge editing facility is internal. As a workaround,
  /// we explicitly call internal port handles from our custom node handles.
  /// </remarks>
  internal class DummyPortLocationModelParameterHandle : PortLocationModelParameterHandle
  {
    public DummyPortLocationModelParameterHandle(IPort port) : base(port) { }

    /// <summary>
    /// Does nothing since we don't want to change the port location.
    /// </summary>
    protected override void SetParameter(IGraph graph, IPort port, IPortLocationModelParameter newParameter) {
      // do nothing
    }

    /// <summary>
    /// Returns the current port location since we don't want to change the port location.
    /// </summary>
    protected override IPortLocationModelParameter GetNewParameter(IPort port, IPortLocationModel model,
        PointD newLocation) {
      return port.LocationParameter;
    }
  }
}
