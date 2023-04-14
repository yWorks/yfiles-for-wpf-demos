/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Input;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.Input.ReshapeHandleProviderConfiguration
{
  /// <summary>
  /// Shows how to customize the resize behavior of nodes by using customized <see cref="IReshapeHandleProvider"/>.
  /// </summary>
  public partial class ReshapeHandleProviderConfigurationWindow
  {
    /// <summary>
    /// A state object that stores whether resizing cyan nodes should keep their aspect ratio. 
    /// </summary>
    private readonly ApplicationState applicationState = new ApplicationState() { KeepAspectRatio = true};

    /// <summary>
    /// Registers a callback function as decorator that provides customized <see cref="IReshapeHandleProvider"/> for each node.
    /// </summary>
    /// <remarks>
    /// This callback function is called whenever a node in the graph is queried
    /// for its <c>IReshapeHandleProvider</c>. In this case, the 'node'
    /// parameter will be set to that node.
    /// </remarks>
    public void RegisterReshapeHandleProvider(RectD boundaryRectangle) {
      var nodeDecorator = graphControl.Graph.GetDecorator().NodeDecorator;

      // deactivate reshape handling for the red node
      nodeDecorator.ReshapeHandleProviderDecorator.HideImplementation(node => Themes.PaletteRed.Equals(node.Tag));
      
      // return customized reshape handle provider for the orange, blue and green node
      nodeDecorator.ReshapeHandleProviderDecorator.SetFactory(
          node => Themes.PaletteOrange.Equals(node.Tag) 
                  || Themes.PaletteLightblue.Equals(node.Tag)
                  || Themes.PaletteGreen.Equals(node.Tag) 
                  || Themes.PalettePurple.Equals(node.Tag) 
                  || Themes.Palette510.Equals(node.Tag) 
                  || Themes.PaletteBlue.Equals(node.Tag),
          (node) => {
            // Obtain the tag from the node
            object nodeTag = node.Tag;

            // Create a default reshape handle provider for nodes
            var reshapeHandler = node.Lookup<IReshapeHandler>();
            var provider = new NodeReshapeHandleProvider(node, reshapeHandler, HandlePositions.Border);

            // Customize the handle provider depending on the node's color
            if (Themes.PaletteOrange.Equals(nodeTag)) {
              // Restrict the node bounds to the boundaryRectangle
              provider.MaximumBoundingArea = boundaryRectangle;
            } else if (Themes.PaletteGreen.Equals(nodeTag)) {
              // Show only handles at the corners and always use aspect ratio resizing
              provider.HandlePositions = HandlePositions.Corners;
              provider.RatioReshapeRecognizer = EventRecognizers.Always;
            } else if (Themes.PaletteLightblue.Equals(nodeTag)) {
              // Restrict the node bounds to the boundaryRectangle and
              // show only handles at the corners and always use aspect ratio resizing
              provider.MaximumBoundingArea = boundaryRectangle;
              provider.HandlePositions = HandlePositions.Corners;
              provider.RatioReshapeRecognizer = EventRecognizers.Always;
            } else if (Themes.PalettePurple.Equals(nodeTag)) {
              provider = new PurpleNodeReshapeHandleProvider(node, reshapeHandler);
            } else if (Themes.PaletteBlue.Equals(nodeTag)) {
              provider.HandlePositions = HandlePositions.SouthEast;
              provider.CenterReshapeRecognizer = EventRecognizers.Always;
            } else if (Themes.Palette510.Equals(nodeTag)) {
              provider = new CyanNodeReshapeHandleProvider(node, reshapeHandler, applicationState);
            }
            return provider;
          });
    }

    #region Initialization

    public ReshapeHandleProviderConfigurationWindow() {
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

      // store the graphControl in the state so it can requery the handles when necessary
      applicationState.GraphControl = graphControl;

      // Create the rectangle that limits the movement of some nodes
      // and add it to the GraphControl.
      var boundaryRectangle = new RectD(20, 20, 480, 550);
      graphControl.RootGroup.AddChild(boundaryRectangle, new LimitingRectangleDescriptor());

      RegisterReshapeHandleProvider(boundaryRectangle);

      CreateSampleGraph(graph);
    }

    /// <summary>
    /// Creates the visualization for the limiting rectangle.
    /// </summary>
    private class LimitingRectangleDescriptor : ICanvasObjectDescriptor, IVisualCreator
    {
      private IRectangle rect;

      public IVisualCreator GetVisualCreator(object forUserObject) {
        rect = (IRectangle) forUserObject;
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

    #region Sample Graph Creation

    /// <summary>
    /// Creates the sample graph of this demo.
    /// </summary>
    private static void CreateSampleGraph(IGraph graph) {
      CreateNode(graph, 80, 100, 140, 30, Themes.PaletteRed, "Fixed Size");
      CreateNode(graph, 300, 100, 140, 30, Themes.PaletteGreen, "Keep Aspect Ratio");
      CreateNode(graph, 80, 200, 140, 50, Themes.PaletteBlue, "Keep Center");
      CreateNode(graph, 300, 200, 140, 50, Themes.PalettePurple, "Keep Aspect Ratio\nat corners");
      CreateNode(graph, 80, 310, 140, 30, Themes.PaletteOrange, "Limited to Rectangle");
      CreateNode(graph, 300, 300, 140, 50, Themes.PaletteLightblue, "Limited to Rectangle\nand Keep Aspect Ratio");
      CreateNode(graph, 80, 400, 140, 50, Themes.Palette510, "Keep Aspect Ratio\ndepending on State");
    }

    /// <summary>
    /// Creates a sample node for this demo.
    /// </summary>
    private static void CreateNode(IGraph graph, double x, double y, double w, double h, Palette palette, string labelText) {
      INode node = graph.CreateNode(new RectD(x, y, w, h), DemoStyles.CreateDemoNodeStyle(palette), palette);
      graph.SetStyle(graph.AddLabel(node, labelText), DemoStyles.CreateDemoNodeLabelStyle(palette));
    }

    #endregion

    #region Custom NodeReshapeHandleProvider

    /// <summary>
    /// A NodeReshapeHandleProvider for purple nodes that provides different handles for corners and borders.
    /// </summary>
    private class PurpleNodeReshapeHandleProvider : NodeReshapeHandleProvider {
      public PurpleNodeReshapeHandleProvider(INode node, IReshapeHandler reshapeHandler) 
          : base(node, reshapeHandler, HandlePositions.Border) { }

      public override IHandle GetHandle(IInputModeContext inputModeContext, HandlePositions position) {
        var handle = new NodeReshapeHandlerHandle(Node, ReshapeHandler, position);

        var atCorner = (position & HandlePositions.Corners) != HandlePositions.None;
        if (atCorner) {
          // handles at corners shall always keep the aspect ratio
          handle.ReshapePolicy = ReshapePolicy.Projection;
          handle.RatioReshapeRecognizer = EventRecognizers.Always;
          handle.Type = HandleTypes.Resize;
        } else {
          // handles at the sides shall ignore the aspect ratio and use another handle visualization
          handle.ReshapePolicy = ReshapePolicy.None;
          handle.RatioReshapeRecognizer = EventRecognizers.Never;
          handle.Type = HandleTypes.Warp;
        }

        return handle;
      }
    }
    
    /// <summary>
    /// A NodeReshapeHandleProvider for cyan nodes that toggles aspect ratio resizing on and of when clicking on its handles.
    /// </summary>
    private class CyanNodeReshapeHandleProvider : NodeReshapeHandleProvider
    {
      private readonly ApplicationState state;

      public CyanNodeReshapeHandleProvider(INode node, IReshapeHandler reshapeHandler, ApplicationState state) : base(node, reshapeHandler, HandlePositions.Border) {
        this.state = state;
      }

      public override IHandle GetHandle(IInputModeContext inputModeContext, HandlePositions position) {
        var wrapped = base.GetHandle(inputModeContext, position) as NodeReshapeHandlerHandle;
        wrapped.RatioReshapeRecognizer = state.KeepAspectRatio ? EventRecognizers.Always : EventRecognizers.Never;
        return new ClickableNodeReshapeHandlerHandle(state, wrapped);
      }
    }

    /// <summary>
    /// A handle delegating most functionality to a wrapped <see cref="NodeReshapeHandlerHandle"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="IHandle.HandleClick"/> toggles the <see cref="ApplicationState.KeepAspectRatio"/> state.
    /// </remarks>
    private class ClickableNodeReshapeHandlerHandle : IHandle
    {
      private readonly ApplicationState state;
      private readonly NodeReshapeHandlerHandle wrapped;

      public ClickableNodeReshapeHandlerHandle(ApplicationState state, NodeReshapeHandlerHandle wrapped) {
        this.state = state;
        this.wrapped = wrapped;
      }

      /// <summary>
      /// <see cref="ApplicationState.ToggleAspectRatio">Toggles</see> the aspect ratio state of the application.
      /// </summary>
      /// <param name="eventArgs">The arguments describing the click.</param>
      public void HandleClick(ClickEventArgs eventArgs) {
        state.ToggleAspectRatio();
      }

      /// <summary>
      /// Modifies the wrapped <see cref="IHandle.Type"/> by combining it with <see cref="HandleTypes.Variant2"/>.
      /// </summary>
      public HandleTypes Type {
        get { return state.KeepAspectRatio ? wrapped.Type |= HandleTypes.Variant2 : wrapped.Type; }
      }

      #region Delegating to the wrapped handle

      public IPoint Location {
        get { return wrapped.Location; }
      }

      public Cursor Cursor {
        get { return wrapped.Cursor; }
      }

      public void InitializeDrag(IInputModeContext inputModeContext) {
        wrapped.InitializeDrag(inputModeContext);
      }

      public void CancelDrag(IInputModeContext inputModeContext, PointD originalLocation) {
        wrapped.CancelDrag(inputModeContext, originalLocation);
      }

      public void DragFinished(IInputModeContext inputModeContext, PointD originalLocation, PointD newLocation) {
        wrapped.DragFinished(inputModeContext, originalLocation, newLocation);
      }

      public void HandleMove(IInputModeContext inputModeContext, PointD originalLocation, PointD newLocation) {
        wrapped.HandleMove(inputModeContext, originalLocation, newLocation);
      }

      #endregion
    }

    #endregion

    /// <summary>
    /// Some application-wide state.
    /// </summary>
    class ApplicationState
    {
      public bool KeepAspectRatio { get; set; }

      public GraphControl GraphControl { get; set; }

      /// <summary>
      /// Toggles the <see cref="KeepAspectRatio"/> state and <see cref="GraphEditorInputMode.RequeryHandles">requeries</see>
      /// all handles to take the new state into account.
      /// </summary>
      public void ToggleAspectRatio() {
        KeepAspectRatio = !KeepAspectRatio;
        var geim = (GraphControl.InputMode as GraphEditorInputMode);
        if (geim != null) {
          geim.RequeryHandles();
        }
      }
    }
  }
}
