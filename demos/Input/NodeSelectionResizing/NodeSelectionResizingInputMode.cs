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
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.NodeSelectionResizing
{
  /// <summary>
  ///   An input mode for reshape handles for groups of nodes. Add it as a child mode of
  ///   <see cref="GraphEditorInputMode"/> to replace the default node reshape handles when multiple nodes are
  ///   selected. Instead of one set of handles per node, this mode shows one set around all selected nodes.
  ///   It supports two resize modes: <see cref="NodeSelectionResizeMode.Resize"/> and
  ///   <see cref="NodeSelectionResizeMode.Scale"/>, which scales node locations while preserving their sizes.
  /// </summary>
  public sealed class NodeSelectionResizingInputMode : InputModeBase
  {
    private readonly OrthogonalEdgeEditingHelper orthogonalEdgeEditingHelper = new();
    private HandleInputMode? handleInputMode;
    private GraphEditorInputMode? graphEditorInputMode;
    private GraphControl? graphControl;
    private EncompassingRectangle? rectangle;
    private IRenderTreeElement? rectRenderTreeElement;
    private InsetsD margins;
    private bool ignoreSingleSelectionEvents;
    private NodeSelectionResizeMode mode;

    /// <summary>Gets or sets the margins between the handle rectangle and the bounds of the selected nodes.</summary>
    /// <value>The space between the selected node layouts and the outer handle rectangle.</value>
    /// <remarks>Setting this property invalidates the encompassing rectangle and its visualization.</remarks>
    public InsetsD Margins {
      get => margins;
      set {
        margins = value;
        rectangle?.Invalidate();
        rectRenderTreeElement?.Invalidate();
      }
    }

    /// <summary>Gets or sets the current resize mode, either resizing node layouts or scaling their locations.</summary>
    /// <value>The resize behavior to use for the selected nodes.</value>
    /// <remarks>Changing the mode recreates the handles when this input mode is active.</remarks>
    public NodeSelectionResizeMode Mode {
      get => mode;
      set {
        mode = value;
        if (handleInputMode != null && handleInputMode.Enabled) {
          UpdateHandles();
        }
      }
    }

    /// <summary>Creates an input mode that resizes multiple selected nodes as a group.</summary>
    /// <param name="mode">The resize mode to use. The default is <see cref="NodeSelectionResizeMode.Scale"/>.</param>
    /// <param name="margins">The space to leave between the selected nodes and the handles.</param>
    public NodeSelectionResizingInputMode(
        NodeSelectionResizeMode mode = NodeSelectionResizeMode.Scale,
        InsetsD? margins = null) {
      this.mode = mode;
      this.margins = margins ?? InsetsD.Empty;
    }

    public override void Install(IInputModeContext context, ConcurrencyController controller) {
      base.Install(context, controller);
      graphEditorInputMode = context.InputMode as GraphEditorInputMode ??
          throw new InvalidOperationException(
              "NodeSelectionResizingInputMode must be installed as a child mode of GraphEditorInputMode.");
      graphControl = context.CanvasControl as GraphControl ??
          throw new InvalidOperationException("NodeSelectionResizingInputMode requires a GraphControl.");

      // Create a dedicated HandleInputMode for the shared resize handles.
      handleInputMode = new HandleInputMode {
          Priority = 1,
          Enabled = false
      };
      handleInputMode.DragStarted += RegisterReshapedNodes;
      handleInputMode.DragStarting += orthogonalEdgeEditingHelper.Starting;
      handleInputMode.DragStarted += orthogonalEdgeEditingHelper.Started;
      handleInputMode.DragFinished += orthogonalEdgeEditingHelper.Finished;
      handleInputMode.DragCanceled += orthogonalEdgeEditingHelper.Canceled;
      handleInputMode.Install(context, controller);

      graphEditorInputMode.MultiSelectionStarted += MultiSelectionStarted;
      graphEditorInputMode.MultiSelectionFinished += MultiSelectionFinished;
      graphControl.Selection.ItemAdded += ItemSelectionChanged;
      graphControl.Selection.ItemRemoved += ItemSelectionChanged;

      // add a NodeLayoutChanged listener so the reshape rect is updated when the nodes are moved (e.g. through
      // layout animations or MoveInputMode).
      graphControl.Graph.NodeLayoutChanged += NodeLayoutChanged;
    }

    /// <summary>Notifies the current GraphSnapContext which nodes are going to be reshaped.</summary>
    private void RegisterReshapedNodes(object? sender, InputModeEventArgs args) {
      // Register the nodes so that they do not provide snap lines for themselves.
      var snapContext = args.Context.Lookup<GraphSnapContext>();
      if (snapContext?.Enabled == true && rectangle != null) {
        foreach (var node in rectangle.Nodes) {
          snapContext.AddItemToBeReshaped(node);
        }
      }
    }

    /// <summary>Invalidates the encompassing rectangle when nodes move outside this input mode.</summary>
    private void NodeLayoutChanged(object source, INode node, RectD oldLayout) {
      if (rectangle != null && handleInputMode?.IsDragging != true) {
        rectangle.Invalidate();
        rectRenderTreeElement?.Invalidate();
      }
    }

    public override bool TryStop() {
      RemoveRectangleVisualization();
      return handleInputMode?.TryStop() ?? true;
    }

    public override void Cancel() {
      RemoveRectangleVisualization();
      handleInputMode?.Cancel();
    }

    public override void Uninstall(IInputModeContext context) {
      if (graphControl != null) {
        graphControl.Graph.NodeLayoutChanged -= NodeLayoutChanged;
      }
      if (graphEditorInputMode != null) {
        graphEditorInputMode.MultiSelectionStarted -= MultiSelectionStarted;
        graphEditorInputMode.MultiSelectionFinished -= MultiSelectionFinished;
      }
      if (graphControl != null) {
        graphControl.Selection.ItemAdded -= ItemSelectionChanged;
        graphControl.Selection.ItemRemoved -= ItemSelectionChanged;
      }
      if (handleInputMode != null) {
        handleInputMode.DragStarted -= RegisterReshapedNodes;
        handleInputMode.DragStarting -= orthogonalEdgeEditingHelper.Starting;
        handleInputMode.DragStarted -= orthogonalEdgeEditingHelper.Started;
        handleInputMode.DragFinished -= orthogonalEdgeEditingHelper.Finished;
        handleInputMode.DragCanceled -= orthogonalEdgeEditingHelper.Canceled;
        RemoveRectangleVisualization();
        handleInputMode.Uninstall(context);
        handleInputMode = null;
      }
      base.Uninstall(context);
    }

    private void MultiSelectionStarted(object? sender, SelectionEventArgs<IModelItem> args) {
      // Ignore individual item events until the complete multi-selection has been applied.
      ignoreSingleSelectionEvents = true;
    }

    private void MultiSelectionFinished(object? sender, SelectionEventArgs<IModelItem> args) {
      ignoreSingleSelectionEvents = false;
      UpdateHandles();
    }

    private void ItemSelectionChanged(object? sender, ItemEventArgs<IModelItem> args) => UpdateHandles();

    private void UpdateHandles() {
      if (ignoreSingleSelectionEvents || graphEditorInputMode == null || graphControl == null ||
          handleInputMode == null) {
        return;
      }
      // first, clear any existing handles
      ClearHandles();

      var selectedNodesCount = graphControl.Selection.Nodes.Count;
      // Use the default behavior only if zero or one node is selected.
      graphEditorInputMode.HandleInputMode.Enabled = selectedNodesCount <= 1;
      if (selectedNodesCount >= 2) {
        ShowHandles();
      }
    }

    /// <summary>Clears any existing handles and disables the dedicated HandleInputMode.</summary>
    private void ClearHandles() {
      if (handleInputMode == null) {
        return;
      }
      if (!handleInputMode.TryStop()) {
        handleInputMode.Cancel();
      }
      handleInputMode.Enabled = false;
      handleInputMode.Handles.Clear();
      RemoveRectangleVisualization();
    }

    /// <summary>Initializes the shared handles, reshape handlers, and selection rectangle.</summary>
    private void ShowHandles() {
      if (graphControl == null || graphEditorInputMode == null || handleInputMode == null) {
        return;
      }

      // collect all selected nodes as well as their descendents
      var reshapeNodes = CollectReshapeNodes(graphControl.Graph, graphControl.Selection);

      // create a mutable rectangle that is updated by the ReshapeHandler
      rectangle = new EncompassingRectangle(reshapeNodes, Margins);

      // and visualize it
      rectRenderTreeElement = graphControl.RenderTree.CreateElement(graphControl.RenderTree.InputModeGroup,
          new RectangleVisualCreator(rectangle));
      rectRenderTreeElement.ToBack();

      // create a reshape handler factory depending on the current mode
      ReshapeHandlerBase ReshapeHandlerFactory() =>
          Mode == NodeSelectionResizeMode.Scale
              ? new ScalingReshapeHandler(rectangle)
              : new ResizingReshapeHandler(rectangle);

      // create and add the handles to our HandleInputMode
      handleInputMode.AddHandle(CreateHandle(HandlePositions.Top, ReshapeHandlerFactory));
      handleInputMode.AddHandle(CreateHandle(HandlePositions.TopLeft, ReshapeHandlerFactory));
      handleInputMode.AddHandle(CreateHandle(HandlePositions.Left, ReshapeHandlerFactory));
      handleInputMode.AddHandle(CreateHandle(HandlePositions.BottomLeft, ReshapeHandlerFactory));
      handleInputMode.AddHandle(CreateHandle(HandlePositions.Bottom, ReshapeHandlerFactory));
      handleInputMode.AddHandle(CreateHandle(HandlePositions.BottomRight, ReshapeHandlerFactory));
      handleInputMode.AddHandle(CreateHandle(HandlePositions.Right, ReshapeHandlerFactory));
      handleInputMode.AddHandle(CreateHandle(HandlePositions.TopRight, ReshapeHandlerFactory));

      handleInputMode.Enabled = true;
    }

    /// <summary>Collects selected nodes and all descendants of selected groups.</summary>
    private static List<INode> CollectReshapeNodes(IGraph graph, IGraphSelection selection) {
      var nodes = new List<INode>();
      var seen = new HashSet<INode>();

      void Add(INode node) {
        if (!seen.Add(node)) {
          return;
        }
        nodes.Add(node);
        if (graph.IsGroupNode(node)) {
          foreach (var child in graph.GetChildren(node)) {
            Add(child);
          }
        }
      }

      foreach (var node in selection.Nodes) {
        Add(node);
      }
      return nodes;
    }

    private IHandle CreateHandle(HandlePositions position, Func<ReshapeHandlerBase> reshapeHandlerFactory) {
      var reshapeHandler = reshapeHandlerFactory();
      var handle = new NodeSelectionReshapeHandle(ParentInputModeContext!, position, reshapeHandler, Margins);
      reshapeHandler.Handle = handle;
      return handle;
    }

    private void RemoveRectangleVisualization() {
      if (rectRenderTreeElement != null && graphControl != null) {
        graphControl.RenderTree.Remove(rectRenderTreeElement);
      }
      rectRenderTreeElement = null;
      rectangle = null;
    }
  }

  /// <summary>
  ///   Simplifies handling <see cref="OrthogonalEdgeEditingContext"/> by forwarding
  ///   <see cref="HandleInputMode"/> drag events to it.
  /// </summary>
  internal sealed class OrthogonalEdgeEditingHelper
  {
    private OrthogonalEdgeEditingContext? editingContext;

    /// <summary>Initializes orthogonal edge editing when a handle drag starts.</summary>
    public void Starting(object? sender, InputModeEventArgs args) {
      var context = args.Context;
      var edgeEditingContext = context.Lookup<OrthogonalEdgeEditingContext>();
      if (edgeEditingContext != null && !edgeEditingContext.IsInitializing && !edgeEditingContext.IsInitialized) {
        editingContext = edgeEditingContext;
        editingContext.InitializeDrag(context);
      } else {
        editingContext = null;
      }
    }

    /// <summary>Notifies orthogonal edge editing that the handle drag has been initialized.</summary>
    public void Started(object? sender, InputModeEventArgs args) => editingContext?.DragInitialized();

    /// <summary>Completes orthogonal edge editing when the handle drag finishes.</summary>
    public void Finished(object? sender, InputModeEventArgs args) {
      if (editingContext != null) {
        editingContext.DragFinished();
        editingContext = null;
      }
    }

    /// <summary>Cancels orthogonal edge editing when the handle drag is canceled.</summary>
    public void Canceled(object? sender, InputModeEventArgs args) {
      if (editingContext != null) {
        editingContext.CancelDrag();
        editingContext = null;
      }
    }
  }

  internal static class HandlePositionExtensions
  {
    /// <summary>Returns whether <paramref name="position"/> is a top, top-left, or top-right handle position.</summary>
    /// <param name="position">The handle position to check.</param>
    /// <returns><see langword="true"/> if the position is at any of the top sides.</returns>
    public static bool IsAnyTop(this HandlePositions position) =>
        (position & (HandlePositions.TopLeft | HandlePositions.Top | HandlePositions.TopRight)) != 0;

    /// <summary>Returns whether <paramref name="position"/> is a bottom, bottom-left, or bottom-right handle position.</summary>
    /// <param name="position">The handle position to check.</param>
    /// <returns><see langword="true"/> if the position is at any of the bottom sides.</returns>
    public static bool IsAnyBottom(this HandlePositions position) =>
        (position & (HandlePositions.BottomLeft | HandlePositions.Bottom | HandlePositions.BottomRight)) != 0;

    /// <summary>Returns whether <paramref name="position"/> is a top-left, left, or bottom-left handle position.</summary>
    /// <param name="position">The handle position to check.</param>
    /// <returns><see langword="true"/> if the position is at any of the left sides.</returns>
    public static bool IsAnyLeft(this HandlePositions position) =>
        (position & (HandlePositions.TopLeft | HandlePositions.Left | HandlePositions.BottomLeft)) != 0;

    /// <summary>Returns whether <paramref name="position"/> is a top-right, right, or bottom-right handle position.</summary>
    /// <param name="position">The handle position to check.</param>
    /// <returns><see langword="true"/> if the position is at any of the right sides.</returns>
    public static bool IsAnyRight(this HandlePositions position) =>
        (position & (HandlePositions.TopRight | HandlePositions.Right | HandlePositions.BottomRight)) != 0;
  }

  /// <summary>
  ///   An <see cref="IRectangle"/> implementation that encompasses a set of <see cref="INode"/> layouts.
  ///   It can be invalidated to fit the current node layouts or explicitly reshaped.
  /// </summary>
  internal sealed class EncompassingRectangle
  {
    private RectD tightRectangle;
    private RectD outerRectangle;
    private bool invalid = true;

    /// <summary>Gets the nodes whose layouts are encompassed by this rectangle.</summary>
    public IReadOnlyList<INode> Nodes { get; }

    /// <summary>Gets the margins between the tight rectangle and the outer rectangle.</summary>
    public InsetsD Margins { get; }

    /// <summary>Gets the bounds of the nodes without margins.</summary>
    public RectD TightRectangle {
      get {
        Update();
        return tightRectangle;
      }
    }

    /// <summary>Gets the bounds of the nodes enlarged by <see cref="Margins"/>.</summary>
    public RectD OuterRectangle {
      get {
        Update();
        return outerRectangle;
      }
    }

    /// <summary>Initializes an encompassing rectangle for the specified nodes and margins.</summary>
    /// <param name="nodes">The nodes whose layouts are encompassed.</param>
    /// <param name="margins">The space to add around the node layouts.</param>
    public EncompassingRectangle(IReadOnlyList<INode> nodes, InsetsD margins) {
      Nodes = nodes;
      Margins = margins;
    }

    /// <summary>Marks the bounds as invalid so they are recalculated from the current node layouts on access.</summary>
    public void Invalidate() => invalid = true;

    /// <summary>Sets the tight bounds explicitly and updates the outer bounds using the configured margins.</summary>
    /// <param name="newRectangle">The new bounds without margins.</param>
    public void Reshape(RectD newRectangle) {
      tightRectangle = newRectangle;
      outerRectangle = newRectangle.GetEnlarged(Margins);
      invalid = false;
    }

    private void Update() {
      if (!invalid) {
        return;
      }

      var rectangle = new MutableRectangle();
      foreach (var node in Nodes) {
        rectangle.Add(node.Layout);
      }
      tightRectangle = rectangle.ToRectD();

      outerRectangle = tightRectangle.GetEnlarged(Margins);

      invalid = false;
    }
  }

  /// <summary>Draws the dashed outline around the selected nodes.</summary>
  internal sealed class RectangleVisualCreator : IVisualCreator
  {
    private readonly EncompassingRectangle rectangle;

    public RectangleVisualCreator(EncompassingRectangle rectangle) {
      this.rectangle = rectangle;
    }

    public Visual CreateVisual(IRenderContext context) {
      var bounds = rectangle.OuterRectangle;
      var visual = new Rectangle {
          Fill = null,
          Width = bounds.Width,
          Height = bounds.Height
      };
      visual.SetCanvasArrangeRect(bounds.ToRect());
      visual.SetPen((Pen) new Pen(Brushes.DodgerBlue, 1) { DashStyle = DashStyles.Dash }.GetAsFrozen());
      return visual;
    }

    public Visual UpdateVisual(IRenderContext context, Visual? oldVisual) {
      if (oldVisual is not Rectangle visual) {
        return CreateVisual(context);
      }

      var bounds = rectangle.OuterRectangle;
      visual.Width = bounds.Width;
      visual.Height = bounds.Height;
      visual.SetCanvasArrangeRect(bounds.ToRect());
      return visual;
    }
  }

  /// <summary>
  ///   The base <see cref="IReshapeHandler"/> for the two resize modes. It implements shared reshape behavior,
  ///   undo support, orthogonal edge editing, and snapping.
  /// </summary>
  internal abstract class ReshapeHandlerBase : IReshapeHandler
  {
    protected readonly Dictionary<INode, RectD> OriginalNodeLayouts = new();
    private readonly Dictionary<INode, IReshapeHandler> reshapeHandlers = new();
    private readonly Dictionary<INode, INodeReshapeSnapResultProvider> reshapeSnapResultProviders = new();
    private readonly Dictionary<INode, OrthogonalEdgeDragHandler> orthogonalEdgeDragHandlers = new();
    private ICompoundEdit? compoundEdit;
    private GraphSnapContext? snapContext;
    private readonly EncompassingRectangle rectangle;

    /// <summary>Gets the original bounds of the encompassing rectangle without its margins.</summary>
    protected RectD OriginalBounds { get; private set; }

    /// <summary>Gets the handle that uses this reshape handler.</summary>
    public NodeSelectionReshapeHandle? Handle { get; set; }

    /// <summary>Gets the bounds of the item being reshaped.</summary>
    public IRectangle Bounds => rectangle.TightRectangle;

    /// <summary>Gets the nodes whose layouts are reshaped.</summary>
    protected IReadOnlyList<INode> ReshapeNodes => rectangle.Nodes;

    protected ReshapeHandlerBase(EncompassingRectangle rectangle) {
      this.rectangle = rectangle;
    }

    public void InitializeReshape(IInputModeContext context) {
      OriginalBounds = rectangle.TightRectangle;
      snapContext = context.Lookup<GraphSnapContext>();
      if (snapContext != null) {
        // Register the callback that supplies snap results for each selected node.
        snapContext.CollectSnapResults += CollectSnapResults;
      }

      // Store the original layout, reshape handler, snap result provider, and orthogonal edge handler for each node.
      foreach (var node in ReshapeNodes) {
        OriginalNodeLayouts[node] = node.Layout.ToRectD();
        // Store the handler that will change this node's shape.
        if (node.Lookup(typeof(IReshapeHandler)) is IReshapeHandler reshapeHandler) {
          reshapeHandler.InitializeReshape(context);
          reshapeHandlers[node] = reshapeHandler;
        }
        // Store the provider that contributes snap results for this node (for example, snap lines).
        if (snapContext != null &&
            node.Lookup(typeof(INodeReshapeSnapResultProvider)) is INodeReshapeSnapResultProvider snapProvider) {
          reshapeSnapResultProviders[node] = snapProvider;
        }
        // Store a handler that keeps edges at this node orthogonal while it moves.
        var orthogonalHandler = OrthogonalEdgeEditingContext.CreateOrthogonalEdgeDragHandler(context, node, false);
        if (orthogonalHandler != null) {
          orthogonalEdgeDragHandlers[node] = orthogonalHandler;
        }
      }

      // Update the handle's minimum and maximum size to respect all selected nodes.
      Handle!.MinimumSize = CalculateMinimumSize();
      Handle.MaximumSize = CalculateMaximumSize();
      // Combine the group resize into a single undo unit.
      compoundEdit = (context.CanvasControl as GraphControl)!.Graph.BeginEdit("Undo Group Resize", "Redo Group Resize");
    }

    private void CollectSnapResults(object? sender, CollectSnapResultsEventArgs args) {
      if (snapContext == null || Handle == null || OriginalBounds.Width == 0 || OriginalBounds.Height == 0) {
        return;
      }
      var lastEvent = args.Context.CanvasControl.LastPointerEvent;
      var fixedRatio = Handle.RatioReshapeRecognizer(lastEvent, this);
      var centered = Handle.CenterReshapeRecognizer(lastEvent, this);
      var policy = fixedRatio ? Handle.ReshapePolicy : ReshapePolicy.None;
      var ratio = OriginalBounds.Width / OriginalBounds.Height;
      var minScaleX = Handle.MinimumSize.Width / OriginalBounds.Width;
      var minScaleY = Handle.MinimumSize.Height / OriginalBounds.Height;
      var maxScaleX = Handle.MaximumSize.Width / OriginalBounds.Width;
      var maxScaleY = Handle.MaximumSize.Height / OriginalBounds.Height;

      foreach (var pair in reshapeSnapResultProviders) {
        // Create a suitable ReshapeRectangleContext for each selected node with a snap result provider.
        var node = pair.Key;
        var layout = OriginalNodeLayouts[node];
        // Calculate how each point in the node layout changes for the mouse movement.
        var topLeft = FixZero(GetFactor(layout.X, layout.Y, layout, centered, Handle.Position));
        var bottomRight = FixZero(GetFactor(layout.GetMaxX(), layout.GetMaxY(), layout, centered, Handle.Position));
        var pointDiffFactor = FixZero(bottomRight - topLeft);
        // The size change factor is the difference between the bottom-right and top-left point factors.
        var sizeChangeFactor = new SizeD(pointDiffFactor.X, pointDiffFactor.Y);

        var reshapeContext = new ReshapeRectangleContext(layout,
            new SizeD(layout.Width * minScaleX, layout.Height * minScaleY),
            new SizeD(layout.Width * maxScaleX, layout.Height * maxScaleY), RectD.Empty, RectD.Infinite,
            Handle.Position, topLeft, bottomRight, sizeChangeFactor, policy, ratio);
        // Let the node's provider contribute its snap results.
        pair.Value.CollectSnapResults(snapContext, args, node, reshapeContext);
      }
    }

    /// <summary>Calculates the minimum size of the handle, considering all nodes being reshaped.</summary>
    protected abstract SizeD CalculateMinimumSize();

    /// <summary>Calculates the maximum size of the handle, considering all nodes being reshaped.</summary>
    protected abstract SizeD CalculateMaximumSize();

    /// <summary>
    ///   Calculates the horizontal and vertical factor the mouse movement has to be multiplied with to get the
    ///   horizontal and vertical delta for the point (x,y) inside the `originalNodeLayout`.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the point in <paramref name="originalNodeLayout"/>.</param>
    /// <param name="y">The vertical coordinate of the point in <paramref name="originalNodeLayout"/>.</param>
    /// <param name="originalNodeLayout">The original layout of the node to calculate the factors for.</param>
    /// <param name="centered">Whether center resizing is active.</param>
    /// <param name="position">The handle position to calculate the factor for.</param>
    protected abstract PointD GetFactor(
        double x,
        double y,
        RectD originalNodeLayout,
        bool centered,
        HandlePositions position);

    /// <summary>
    ///   Calculates the factors that convert mouse movement into point movement for ratio resizing when a side
    ///   handle is constrained to move in the orthogonal direction. The horizontal delta for a point is the
    ///   vertical mouse delta multiplied by the returned factor's Y value; the vertical delta is the horizontal
    ///   mouse delta multiplied by its X value. This is used only for ratio resizing with a horizontal or
    ///   vertical <see cref="ReshapePolicy"/>.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the point in <paramref name="originalNodeLayout"/>.</param>
    /// <param name="y">The vertical coordinate of the point in <paramref name="originalNodeLayout"/>.</param>
    /// <param name="originalNodeLayout">The original layout of the node to calculate the factors for.</param>
    /// <param name="centered">Whether center resizing is active.</param>
    private PointD GetOrthogonalFactor(double x, double y, RectD originalNodeLayout, bool centered) {
      var ratio = OriginalBounds.Width / OriginalBounds.Height;
      if (Handle!.ReshapePolicy == ReshapePolicy.Horizontal) {
        var xToY = 1 / (ratio * (centered ? 1 : 2));
        var orthogonalPosition =
            Handle.Position == HandlePositions.Right ? HandlePositions.Bottom : HandlePositions.Top;
        var factor = GetFactor(x, y, originalNodeLayout, true, orthogonalPosition);
        return new PointD(factor.Y * xToY, 0);
      }
      if (Handle.ReshapePolicy == ReshapePolicy.Vertical) {
        var xToY = ratio / (centered ? 1 : 2);
        var orthogonalPosition =
            Handle.Position == HandlePositions.Bottom ? HandlePositions.Right : HandlePositions.Left;
        var factor = GetFactor(x, y, originalNodeLayout, true, orthogonalPosition);
        return new PointD(0, factor.X * xToY);
      }
      return PointD.Origin;
    }

    public virtual void HandleReshape(IInputModeContext context, RectD originalBounds, RectD newBounds) {
      // Reshape the encompassing rectangle.
      rectangle.Reshape(newBounds);
      // Update the node layouts and bend locations.
      UpdateNodeLayouts(context, originalBounds, newBounds);
    }

    private void UpdateNodeLayouts(IInputModeContext context, RectD originalBounds, RectD newBounds) {
      var dMinX = newBounds.X - originalBounds.X;
      var dMinY = newBounds.Y - originalBounds.Y;
      var dMaxX = newBounds.GetMaxX() - originalBounds.GetMaxX();
      var dMaxY = newBounds.GetMaxY() - originalBounds.GetMaxY();
      // Calculate a possible mouse movement that could have produced the new bounds.
      var dx = Handle!.Position.IsAnyLeft() ? dMinX : Handle.Position.IsAnyRight() ? dMaxX : 0;
      var dy = Handle.Position.IsAnyTop() ? dMinY : Handle.Position.IsAnyBottom() ? dMaxY : 0;

      var pointerEvent = context.CanvasControl.LastPointerEvent;
      var centered = Handle.CenterReshapeRecognizer(pointerEvent, this);
      var fixedRatio = Handle.RatioReshapeRecognizer(pointerEvent, this);
      var useOrthogonalFactors =
          fixedRatio && Handle.ReshapePolicy is ReshapePolicy.Horizontal or ReshapePolicy.Vertical;

      foreach (var pair in OriginalNodeLayouts) {
        if (!reshapeHandlers.TryGetValue(pair.Key, out var reshapeHandler)) {
          continue;
        }
        var originalLayout = pair.Value;
        var topLeft = GetFactor(originalLayout.X, originalLayout.Y, originalLayout, centered, Handle.Position);
        var bottomRight = GetFactor(originalLayout.GetMaxX(), originalLayout.GetMaxY(), originalLayout, centered,
            Handle.Position);
        var orthogonalTopLeft = PointD.Origin;
        var orthogonalBottomRight = PointD.Origin;
        if (useOrthogonalFactors) {
          orthogonalTopLeft = GetOrthogonalFactor(originalLayout.X, originalLayout.Y, originalLayout, centered);
          orthogonalBottomRight =
              GetOrthogonalFactor(originalLayout.GetMaxX(), originalLayout.GetMaxY(), originalLayout, centered);
        }

        var newX = originalLayout.X + dx * topLeft.X + dy * orthogonalTopLeft.Y;
        var newY = originalLayout.Y + dy * topLeft.Y + dx * orthogonalTopLeft.X;
        var newMaxX = originalLayout.GetMaxX() + dx * bottomRight.X + dy * orthogonalBottomRight.Y;
        var newMaxY = originalLayout.GetMaxY() + dy * bottomRight.Y + dx * orthogonalBottomRight.X;
        reshapeHandler.HandleReshape(context, originalLayout, new RectD(newX, newY, newMaxX - newX, newMaxY - newY));
      }
      foreach (var handler in orthogonalEdgeDragHandlers.Values) {
        handler.HandleMove();
      }
    }

    public void CancelReshape(IInputModeContext context, RectD originalBounds) {
      rectangle.Reshape(originalBounds);
      foreach (var pair in reshapeHandlers) {
        pair.Value.CancelReshape(context, OriginalNodeLayouts[pair.Key]);
      }
      foreach (var handler in orthogonalEdgeDragHandlers.Values) {
        handler.CancelDrag();
      }
      compoundEdit?.Cancel();
      Clear();
    }

    public void ReshapeFinished(IInputModeContext context, RectD originalBounds, RectD newBounds) {
      foreach (var pair in reshapeHandlers) {
        var bounds = pair.Value.Bounds;
        pair.Value.ReshapeFinished(context, OriginalNodeLayouts[pair.Key], bounds.ToRectD());
      }
      foreach (var handler in orthogonalEdgeDragHandlers.Values) {
        handler.FinishDrag();
      }
      compoundEdit?.Commit();
      Clear();
    }

    private void Clear() {
      if (snapContext != null) {
        snapContext.CollectSnapResults -= CollectSnapResults;
      }
      reshapeSnapResultProviders.Clear();
      OriginalNodeLayouts.Clear();
      reshapeHandlers.Clear();
      orthogonalEdgeDragHandlers.Clear();
      compoundEdit = null;
      snapContext = null;
    }

    /// <summary>Sets x or y values close to zero to zero.</summary>
    private static PointD FixZero(PointD point) =>
        new(Math.Abs(point.X) < 0.0001 ? 0 : point.X, Math.Abs(point.Y) < 0.0001 ? 0 : point.Y);
  }

  /// <summary>A <see cref="ReshapeHandlerBase"/> that implements the 'scale' resize mode.</summary>
  internal sealed class ScalingReshapeHandler : ReshapeHandlerBase
  {
    public ScalingReshapeHandler(EncompassingRectangle rectangle) : base(rectangle) { }

    /// <summary>
    ///   Returns the minimum size of the selection. Since this mode preserves node sizes, the encompassing
    ///   rectangle cannot be narrower or shorter than the maximum node width or height, respectively.
    /// </summary>
    protected override SizeD CalculateMinimumSize() {
      var width = 0d;
      var height = 0d;
      foreach (var node in ReshapeNodes) {
        width = Math.Max(width, node.Layout.Width);
        height = Math.Max(height, node.Layout.Height);
      }
      return new SizeD(width, height);
    }

    /// <summary>Returns an unlimited maximum size because this mode does not constrain node sizes.</summary>
    protected override SizeD CalculateMaximumSize() => SizeD.Infinite;

    /// <summary>
    ///   Calculates how the specified point in a node layout moves with the handle. In this mode the node size is
    ///   preserved, so the factors account for the relative position of the node within the selection.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the point in <paramref name="layout"/>.</param>
    /// <param name="y">The vertical coordinate of the point in <paramref name="layout"/>.</param>
    /// <param name="layout">The original node layout.</param>
    /// <param name="centered">Whether resizing is centered.</param>
    /// <param name="position">The position of the dragged handle.</param>
    /// <returns>The horizontal and vertical movement factors for the point.</returns>
    protected override PointD GetFactor(double x, double y, RectD layout, bool centered, HandlePositions position) {
      var fx = 0d;
      if ((position & HandlePositions.Vertical) == 0) {
        var boundsWidth = OriginalBounds.Width - layout.Width;
        if (boundsWidth <= 0) {
          fx = centered ? 0 : 0.5;
        } else {
          var xRatio = centered
              ? 2 * (layout.X + layout.Width / 2 - (OriginalBounds.X + OriginalBounds.Width / 2)) / boundsWidth
              : (layout.X - OriginalBounds.X) / boundsWidth;
          if (position.IsAnyLeft()) {
            fx = centered ? -xRatio : 1 - xRatio;
          } else if (position.IsAnyRight()) {
            fx = xRatio;
          }
        }
      }

      var fy = 0d;
      if ((position & HandlePositions.Horizontal) == 0) {
        var boundsHeight = OriginalBounds.Height - layout.Height;
        if (boundsHeight <= 0) {
          fy = centered ? 0 : 0.5;
        } else {
          var yRatio = centered
              ? 2 * (layout.Y + layout.Height / 2 - (OriginalBounds.Y + OriginalBounds.Height / 2)) / boundsHeight
              : (layout.Y - OriginalBounds.Y) / boundsHeight;
          if (position.IsAnyTop()) {
            fy = centered ? -yRatio : 1 - yRatio;
          } else if (position.IsAnyBottom()) {
            fy = yRatio;
          }
        }
      }
      return new PointD(fx, fy);
    }

    public override void HandleReshape(IInputModeContext context, RectD originalBounds, RectD newBounds) {
      base.HandleReshape(context, originalBounds, newBounds);
      var graph = (context.CanvasControl as GraphControl)?.Graph;
      if (graph == null) {
        return;
      }
      var groupingSupport = graph.GetGroupingSupport();
      foreach (var node in ReshapeNodes) {
        if (graph.IsGroupNode(node)) {
          // Keep each group large enough to contain its descendants after their locations have been scaled.
          groupingSupport.EnlargeGroupNode(context, node, true);
        }
      }
    }
  }

  /// <summary>A <see cref="ReshapeHandlerBase"/> that implements the 'resize' mode.</summary>
  internal sealed class ResizingReshapeHandler : ReshapeHandlerBase
  {
    public ResizingReshapeHandler(EncompassingRectangle rectangle) : base(rectangle) { }

    /// <summary>
    ///   Considers each node's minimum size constraint and combines the resulting scale factors into a minimum
    ///   size for the selection.
    /// </summary>
    protected override SizeD CalculateMinimumSize() {
      var minScaleX = 0d;
      var minScaleY = 0d;
      foreach (var node in ReshapeNodes) {
        if (node.Lookup(typeof(INodeSizeConstraintProvider)) is not INodeSizeConstraintProvider provider) {
          continue;
        }
        var minSize = provider.GetMinimumSize();
        if (minSize.Width <= 0 && minSize.Height <= 0) {
          continue;
        }
        var layout = OriginalNodeLayouts[node];
        minScaleX = Math.Max(minScaleX, minSize.Width / layout.Width);
        minScaleY = Math.Max(minScaleY, minSize.Height / layout.Height);
      }
      return new SizeD(OriginalBounds.Width * minScaleX, OriginalBounds.Height * minScaleY);
    }

    /// <summary>
    ///   Considers each node's maximum size constraint and combines the resulting scale factors into a maximum
    ///   size for the selection.
    /// </summary>
    protected override SizeD CalculateMaximumSize() {
      var maxScaleX = double.PositiveInfinity;
      var maxScaleY = double.PositiveInfinity;
      foreach (var node in ReshapeNodes) {
        if (node.Lookup(typeof(INodeSizeConstraintProvider)) is not INodeSizeConstraintProvider provider) {
          continue;
        }
        var maxSize = provider.GetMaximumSize();
        if (double.IsPositiveInfinity(maxSize.Width) && double.IsPositiveInfinity(maxSize.Height)) {
          continue;
        }
        var layout = OriginalNodeLayouts[node];
        maxScaleX = Math.Min(maxScaleX, maxSize.Width / layout.Width);
        maxScaleY = Math.Min(maxScaleY, maxSize.Height / layout.Height);
      }
      return new SizeD(OriginalBounds.Width * maxScaleX, OriginalBounds.Height * maxScaleY);
    }

    /// <summary>Calculates how a point in a node layout moves as the selection bounds are resized.</summary>
    /// <param name="x">The horizontal coordinate of the point in <paramref name="layout"/>.</param>
    /// <param name="y">The vertical coordinate of the point in <paramref name="layout"/>.</param>
    /// <param name="layout">The original node layout.</param>
    /// <param name="centered">Whether resizing is centered.</param>
    /// <param name="position">The position of the dragged handle.</param>
    /// <returns>The horizontal and vertical movement factors for the point.</returns>
    protected override PointD GetFactor(double x, double y, RectD layout, bool centered, HandlePositions position) {
      var xRatio = centered
          ? 2 * (x - (OriginalBounds.X + OriginalBounds.Width / 2)) / OriginalBounds.Width
          : (x - OriginalBounds.X) / OriginalBounds.Width;
      var yRatio = centered
          ? 2 * (y - (OriginalBounds.Y + OriginalBounds.Height / 2)) / OriginalBounds.Height
          : (y - OriginalBounds.Y) / OriginalBounds.Height;
      var fx = position.IsAnyLeft() ? centered ? -xRatio : 1 - xRatio : position.IsAnyRight() ? xRatio : 0;
      var fy = position.IsAnyTop() ? centered ? -yRatio : 1 - yRatio : position.IsAnyBottom() ? yRatio : 0;
      return new PointD(fx, fy);
    }
  }

  /// <summary>
  ///   A <see cref="ReshapeHandlerHandle"/> for an <see cref="EncompassingRectangle"/>. It accounts for the
  ///   rectangle's margins when calculating the handle location.
  /// </summary>
  internal sealed class NodeSelectionReshapeHandle : IHandle
  {
    private readonly ReshapeHandlerHandle reshapeHandlerHandle;
    private readonly IInputModeContext context;
    private readonly InsetsD margins;

    /// <summary>Creates a reshape handle at the specified position around the encompassing rectangle.</summary>
    /// <param name="context">The input mode context used to calculate the handle location.</param>
    /// <param name="position">The position of the handle around the rectangle.</param>
    /// <param name="reshapeHandler">The handler that applies changes to the rectangle and its nodes.</param>
    /// <param name="margins">The margins between the node bounds and the handle rectangle.</param>
    public NodeSelectionReshapeHandle(
        IInputModeContext context,
        HandlePositions position,
        IReshapeHandler reshapeHandler,
        InsetsD margins) {
      this.context = context;
      this.margins = margins;
      reshapeHandlerHandle = new ReshapeHandlerHandle(position, reshapeHandler);
      Location = new HandleLocation(this);

      if ((position & HandlePositions.Vertical) != 0) {
        reshapeHandlerHandle.ReshapePolicy = ReshapePolicy.Vertical;
      } else if ((position & HandlePositions.Horizontal) != 0) {
        reshapeHandlerHandle.ReshapePolicy = ReshapePolicy.Horizontal;
      } else {
        reshapeHandlerHandle.ReshapePolicy = ReshapePolicy.Projection;
      }
      reshapeHandlerHandle.RatioReshapeRecognizer = ((GraphEditorInputMode) context.InputMode!).HandleInputMode
          .DirectionalConstraintRecognizer;
    }

    /// <summary>Gets the location of this handle.</summary>
    public IPoint Location { get; }

    /// <summary>Gets the reshape handler associated with this handle.</summary>
    public IReshapeHandler ReshapeHandler => reshapeHandlerHandle.ReshapeHandler;

    /// <summary>Gets the position of this handle around the encompassing rectangle.</summary>
    public HandlePositions Position => reshapeHandlerHandle.Position;

    /// <summary>Gets or sets the reshape policy used when this handle is dragged.</summary>
    public ReshapePolicy ReshapePolicy {
      get => reshapeHandlerHandle.ReshapePolicy;
      set => reshapeHandlerHandle.ReshapePolicy = value;
    }

    /// <summary>Gets or sets the maximum size allowed for the encompassing rectangle.</summary>
    public ISize MaximumSize {
      get => reshapeHandlerHandle.MaximumSize;
      set => reshapeHandlerHandle.MaximumSize = value;
    }

    /// <summary>Gets or sets the minimum size allowed for the encompassing rectangle.</summary>
    public ISize MinimumSize {
      get => reshapeHandlerHandle.MinimumSize;
      set => reshapeHandlerHandle.MinimumSize = value;
    }

    /// <summary>Determines whether the current gesture should resize around the center.</summary>
    public bool CenterReshapeRecognizer(EventArgs args, object eventSource) =>
        reshapeHandlerHandle.CenterReshapeRecognizer(eventSource, args);

    /// <summary>Determines whether the current gesture should preserve the aspect ratio.</summary>
    public bool RatioReshapeRecognizer(EventArgs args, object eventSource) =>
        reshapeHandlerHandle.RatioReshapeRecognizer!(eventSource, args);

    /// <summary>Cancels the current handle drag.</summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) =>
        reshapeHandlerHandle.CancelDrag(context, originalLocation);

    /// <summary>Finishes the current handle drag.</summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) =>
        reshapeHandlerHandle.DragFinished(context, originalLocation, newLocation);

    /// <summary>Applies a handle movement.</summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) =>
        reshapeHandlerHandle.HandleMove(context, originalLocation, newLocation);

    /// <summary>Initializes a handle drag.</summary>
    public void InitializeDrag(IInputModeContext context) => reshapeHandlerHandle.InitializeDrag(context);

    /// <summary>Gets the cursor displayed for this handle.</summary>
    public Cursor Cursor => reshapeHandlerHandle.Cursor;

    /// <summary>Gets the type of this handle.</summary>
    public HandleType Type => reshapeHandlerHandle.Type;

    /// <summary>Gets the tag associated with this handle; this implementation does not use a tag.</summary>
    public object? Tag => null;

    /// <summary>This implementation does nothing special when clicked.</summary>
    public void HandleClick(ClickEventArgs eventArgs) { }

    /// <summary>
    ///   An <see cref="IPoint"/> implementation that represents a <see cref="NodeSelectionReshapeHandle"/> location.
    ///   It calculates the location from the handle position, the current reshape bounds, the encompassing
    ///   rectangle's margins, and an additional zoom-dependent offset.
    /// </summary>
    private sealed class HandleLocation : IPoint
    {
      private readonly NodeSelectionReshapeHandle handle;
      private const double Offset = 5;

      public HandleLocation(NodeSelectionReshapeHandle handle) {
        this.handle = handle;
      }

      /// <summary>Gets the horizontal coordinate of the handle based on its position and the current bounds.</summary>
      public double X {
        get {
          var bounds = handle.ReshapeHandler.Bounds;
          return handle.Position switch {
              HandlePositions.TopLeft or HandlePositions.Left or HandlePositions.BottomLeft => bounds.X -
                  (handle.margins.Left + Offset / handle.context.CanvasControl.Zoom),
              HandlePositions.TopRight or HandlePositions.Right or HandlePositions.BottomRight => bounds.X +
                  bounds.Width + handle.margins.Right + Offset / handle.context.CanvasControl.Zoom,
              _ => bounds.X + bounds.Width * 0.5
          };
        }
      }

      /// <summary>Gets the vertical coordinate of the handle based on its position and the current bounds.</summary>
      public double Y {
        get {
          var bounds = handle.ReshapeHandler.Bounds;
          return handle.Position switch {
              HandlePositions.TopLeft or HandlePositions.Top or HandlePositions.TopRight => bounds.Y -
                  (handle.margins.Top + Offset / handle.context.CanvasControl.Zoom),
              HandlePositions.BottomLeft or HandlePositions.Bottom or HandlePositions.BottomRight => bounds.Y +
                  bounds.Height + handle.margins.Bottom + Offset / handle.context.CanvasControl.Zoom,
              _ => bounds.Y + bounds.Height * 0.5
          };
        }
      }
    }
  }
}
