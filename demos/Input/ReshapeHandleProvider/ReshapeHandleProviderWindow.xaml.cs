/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Input.ReshapeHandleProvider
{
  /// <summary>
  /// Shows how to customize the resize behavior of nodes by implementing a
  /// custom <see cref="IReshapeHandleProvider"/>.
  /// </summary>
  public partial class ReshapeHandleProviderWindow
  {

    /// <summary>
    /// Registers a callback function as decorator that provides a custom
    /// <see cref="IReshapeHandleProvider"/> for each node.
    /// </summary>
    /// <remarks>
    /// This callback function is called whenever a node in the graph is queried
    /// for its <c>IReshapeHandleProvider</c>. In this case, the 'node'
    /// parameter will be set to that node and the 'delegateHandler' parameter
    /// will be set to the reshape handle provider that would have been returned
    /// without setting this function as decorator.
    /// </remarks>
    public void RegisterReshapeHandleProvider(MutableRectangle boundaryRectangle) {
      var nodeDecorator = graphControl.Graph.GetDecorator().NodeDecorator;
      nodeDecorator.ReshapeHandleProviderDecorator.SetImplementationWrapper(
        (node, delegateProvider) => {
          // Obtain the tag from the node
          object nodeTag = node.Tag;

          // Check if it is a known tag and choose the respective implementation.
          // Fallback to the default behavior otherwise.
          if (!(nodeTag is Color)) {
            return delegateProvider;
          } else if (Colors.Orange.Equals(nodeTag)) {
            // One that delegates certain behavior to the default implementation
            return new OrangeReshapeHandleProvider(boundaryRectangle, delegateProvider);
          } else if (Colors.Firebrick.Equals(nodeTag)) {
            // A simple one that prohibits resizing
            return new RedReshapeHandleProvider();
          } else if (Colors.RoyalBlue.Equals(nodeTag)) {
            // One that uses two levels of delegation to create a combined behavior
            return new OrangeReshapeHandleProvider(boundaryRectangle,
                                                   new GreenReshapeHandleProvider(delegateProvider, node));
          } else if (Colors.Green.Equals(nodeTag)) {
            // Another one that delegates certain behavior to the default implementation
            return new GreenReshapeHandleProvider(delegateProvider, node);
          } else {
            return delegateProvider;
          }
        });
    }

    #region Initialization

    public ReshapeHandleProviderWindow() {
      InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      IGraph graph = graphControl.Graph;

      // Create a default editor input mode
      GraphEditorInputMode graphEditorInputMode = new GraphEditorInputMode();

      // Just for user convenience: disable node and edge creation,
      graphEditorInputMode.AllowCreateEdge = false;
      graphEditorInputMode.AllowCreateNode = false;
      // disable deleting items
      graphEditorInputMode.DeletableItems = GraphItemTypes.None;
      // disable node moving
      graphEditorInputMode.MovableItems = GraphItemTypes.None;
      // enable the undo feature
      graph.SetUndoEngineEnabled(true);

      // Finally, set the input mode to the graph control.
      graphControl.InputMode = graphEditorInputMode;

      // Create the rectangle that limits the movement of some nodes
      // and add it to the GraphControl.
      var boundaryRectangle = new MutableRectangle(20, 20, 480, 400);
      graphControl.RootGroup.AddChild(boundaryRectangle, new LimitingRectangleDescriptor());

      RegisterReshapeHandleProvider(boundaryRectangle);

      CreateSampleGraph(graph);
    }

    /// <summary>
    /// Creates the visualization for the limiting rectangle.
    /// </summary>
    private class LimitingRectangleDescriptor : ICanvasObjectDescriptor, IVisualCreator
    {
      private MutableRectangle rect;

      public IVisualCreator GetVisualCreator(object forUserObject) {
        rect = (MutableRectangle) forUserObject;
        return this;
      }

      public bool IsDirty(ICanvasContext context, ICanvasObject canvasObject) {
        return true;
      }

      public IBoundsProvider GetBoundsProvider(object forUserObject) {
        return BoundsProviders.Unbounded;
      }

      public IVisibilityTestable GetVisibilityTestable(object forUserObject) {
        return VisibilityTestables.Always;
      }

      public IHitTestable GetHitTestable(object forUserObject) {
        return HitTestables.Never;
      }

      public Visual CreateVisual(IRenderContext context) {
        System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
        rectangle.Stroke = Brushes.Black;
        rectangle.StrokeThickness = 2;
        rectangle.SetValue(CanvasControl.CanvasControlArrangeRectProperty, new Rect(this.rect.X, this.rect.Y, this.rect.Width, this.rect.Height));
        return rectangle;
      }

      public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        System.Windows.Shapes.Rectangle rectangle = oldVisual as System.Windows.Shapes.Rectangle;
        if (rectangle == null) {
          return CreateVisual(context);
        } else {
          rectangle.SetValue(CanvasControl.CanvasControlArrangeRectProperty, new Rect(this.rect.X, this.rect.Y, this.rect.Width, this.rect.Height));
          return rectangle;
        }
    }
    }

    #endregion

    /// <summary>
    /// An <see cref="IReshapeHandleProvider"/> that limits the resizing of a
    /// node to be within an enclosing rectangle and delegates for other aspects
    /// to another (the original) handler.
    /// </summary>
    public class OrangeReshapeHandleProvider : IReshapeHandleProvider
    {
      private readonly MutableRectangle boundaryRectangle;
      private readonly IReshapeHandleProvider wrappedHandler;

      public OrangeReshapeHandleProvider(MutableRectangle boundaryRectangle, IReshapeHandleProvider wrappedHandler) {
        this.boundaryRectangle = boundaryRectangle;
        this.wrappedHandler = wrappedHandler;
      }

      /// <summary>
      /// Returns the available handles of the wrapped handler.
      /// </summary>
      public HandlePositions GetAvailableHandles(IInputModeContext context) {
        return wrappedHandler.GetAvailableHandles(context);
      }

      /// <summary>
      /// Returns a handle for the given original position that is limited to
      /// the interior of the boundary rectangle of this class.
      /// </summary>
      public IHandle GetHandle(IInputModeContext context, HandlePositions position) {
        // return handle that is constrained by a box
        IHandle handle = wrappedHandler.GetHandle(context, position);
        return new BoxConstrainedHandle(handle, boundaryRectangle);
      }

      /// <summary>
      /// A <see cref="ConstrainedHandle"/> that is limited to the interior of a
      /// given rectangle.
      /// </summary>
      public class BoxConstrainedHandle : ConstrainedHandle
      {
        private readonly MutableRectangle boundaryRectangle;
        private RectD constraintRect;

        public BoxConstrainedHandle(IHandle handle, MutableRectangle boundaryRectangle) : base(handle) {
          this.boundaryRectangle = boundaryRectangle;
        }

        /// <summary>
        /// Returns for the given new location the constrained location that is
        /// inside the boundary rectangle.
        /// </summary>
        protected override PointD ConstrainNewLocation(IInputModeContext context, PointD originalLocation,
                                                       PointD newLocation) {
          return newLocation.GetConstrained(constraintRect);
        }

        /// <summary>
        /// Makes sure that the constraintRect is set to the current boundary
        /// rectangle and delegates to the base implementation.
        /// </summary>
        protected override void OnInitialized(IInputModeContext context, PointD originalLocation) {
          base.OnInitialized(context, originalLocation);
          constraintRect = boundaryRectangle.ToRectD();
        }
      }
    }

    /// <summary>
    /// An <see cref="IReshapeHandleProvider"/> that doesn't provide any
    /// handles.
    /// </summary>
    public class RedReshapeHandleProvider : IReshapeHandleProvider
    {
      /// <summary>
      /// Returns the indicator for no valid position.
      /// </summary>
      public HandlePositions GetAvailableHandles(IInputModeContext context) {
        return HandlePositions.None;
      }

      public IHandle GetHandle(IInputModeContext context, HandlePositions position) {
        // Never called since getAvailableHandles returns no valid position.
        return null;
      }
    }

    /// <summary>
    /// An <see cref="IReshapeHandleProvider"/> that restricts the available
    /// handles provided by the wrapped handler to the ones in the four corners.
    /// If the wrapped handler doesn't provide all of these handles, this
    /// handler doesn't do this as well. In addition, these handles have a
    /// custom behavior: they maintain the current aspect ratio of the node.
    /// </summary>
    public class GreenReshapeHandleProvider : IReshapeHandleProvider
    {
      private readonly IReshapeHandleProvider wrappedHandler;
      private readonly INode node;

      public GreenReshapeHandleProvider(IReshapeHandleProvider wrappedHandler, INode node) {
        this.wrappedHandler = wrappedHandler;
        this.node = node;
      }

      /// <summary>
      /// Returns the available handles provided by the wrapped handler
      /// restricted to the ones in the four corners.
      /// </summary>
      public HandlePositions GetAvailableHandles(IInputModeContext context) {
        // return only corner handles
        return wrappedHandler.GetAvailableHandles(context)
               & (HandlePositions.NorthEast | HandlePositions.NorthWest |
                  HandlePositions.SouthEast | HandlePositions.SouthWest);
      }

      /// <summary>
      /// Returns a custom handle to maintains the aspect ratio of the node.
      /// </summary>
      public IHandle GetHandle(IInputModeContext context, HandlePositions position) {
        return new AspectRatioHandle(wrappedHandler.GetHandle(context, position), position, node.Layout);
      }

      /// <summary>
      /// A handle that maintains the aspect ratio of the node.
      /// </summary>
      /// <remarks>
      /// Note that the simpler solution for this use case is subclassing 
      /// <see cref="ConstrainedHandle"/>, however the interface is
      /// completely implemented for illustration, here.
      /// </remarks>
      public class AspectRatioHandle : IHandle
      {
        private const int MinSize = 5;
        private readonly IHandle handle;
        private readonly HandlePositions position;
        private readonly IRectangle layout;
        private PointD lastLocation;
        private double ratio;
        private SizeD originalSize;

        public AspectRatioHandle(IHandle handle, HandlePositions position, IRectangle layout) {
          this.handle = handle;
          this.position = position;
          this.layout = layout;
        }

        public IPoint Location {
          get { return handle.Location; }
        }

        /// <summary>
        /// Stores the initial location and aspect ratio for reference, and calls the base method.
        /// </summary>
        public void InitializeDrag(IInputModeContext context) {
          handle.InitializeDrag(context);
          lastLocation = new PointD(handle.Location);
          originalSize = layout.GetSize();
          switch (position) {
            case HandlePositions.NorthWest:
            case HandlePositions.SouthEast:
              ratio = layout.Width/layout.Height;
              break;
            case HandlePositions.NorthEast:
            case HandlePositions.SouthWest:
              ratio = -layout.Width/layout.Height;
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
        }

        /// <summary>
        /// Constrains the movement to maintain the aspect ratio. This is done
        /// by calculating the constrained location for the given new location,
        /// and invoking the original handler with the constrained location.
        /// </summary>
        public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
          const int minSize = 5;
          // For the given new location, the larger node side specifies the actual size change.
          PointD deltaDrag = newLocation - originalLocation;
          // To prevent a ratio from being calculated later on we need to forbid resizing the node to (0, 0) size.
          if (Math.Abs(ratio) > 1) {
            var sign = (position == HandlePositions.SouthEast || position == HandlePositions.SouthWest) ? 1 : -1;
            var newHeight = originalSize.Height + sign * (deltaDrag.X / ratio);
            if (newHeight > minSize) {
              deltaDrag = new PointD(deltaDrag.X, deltaDrag.X / ratio);
            } else {
              var dy = Math.Sign(deltaDrag.X / ratio) * (originalSize.Height - minSize);
              deltaDrag = new PointD(dy * ratio, dy);
            }
          } else {
            var sign = (position == HandlePositions.NorthWest || position == HandlePositions.SouthWest) ? -1 : 1;
            var newWidth = originalSize.Width + sign * (deltaDrag.Y * ratio);
            if (newWidth > minSize) {
              deltaDrag = new PointD(deltaDrag.Y * ratio, deltaDrag.Y);
            } else {
              var dx = Math.Sign(deltaDrag.Y * ratio) * (originalSize.Width - minSize);
              deltaDrag = new PointD(dx, dx / ratio);
            }
          }

          newLocation = originalLocation + deltaDrag;

          if (newLocation != lastLocation) {
            handle.HandleMove(context, originalLocation, newLocation);
            lastLocation = newLocation;
          } 
        }

        public void CancelDrag(IInputModeContext context, PointD originalLocation) {
          handle.CancelDrag(context, originalLocation);
        }

        public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
          handle.DragFinished(context, originalLocation, lastLocation);
        }

        public HandleTypes Type {
          get { return handle.Type; }
        }

        public Cursor Cursor {
          get { return handle.Cursor; }
        }
      }
    }

    #region Sample Graph Creation

    /// <summary>
    /// Creates the sample graph of this demo.
    /// </summary>
    private static void CreateSampleGraph(IGraph graph) {
      CreateNode(graph, 80, 100, 140, 30, Colors.Firebrick, Colors.WhiteSmoke, "Fixed Size");
      CreateNode(graph, 300, 100, 140, 30, Colors.Green, Colors.WhiteSmoke, "Keep Aspect Ratio");
      CreateNode(graph, 80, 260, 140, 30, Colors.Orange, Colors.Black, "Limited to Rectangle");
      CreateNode(graph, 300, 250, 140, 50, Colors.RoyalBlue, Colors.WhiteSmoke, "Limited to Rectangle\nand Keep Aspect Ratio");
    }

    /// <summary>
    /// Creates a sample node for this demo.
    /// </summary>
    private static void CreateNode(IGraph graph, double x, double y, double w, double h, Color fillColor, Color textColor, string labelText) {
      var whiteTextLabelStyle = new DefaultLabelStyle { TextBrush = new SolidColorBrush(textColor) };
      INode node = graph.CreateNode(new RectD(x, y, w, h), new ShinyPlateNodeStyle { Brush = new SolidColorBrush(fillColor) }, fillColor);
      graph.SetStyle(graph.AddLabel(node, labelText), whiteTextLabelStyle);
    }

    #endregion
  }
}
