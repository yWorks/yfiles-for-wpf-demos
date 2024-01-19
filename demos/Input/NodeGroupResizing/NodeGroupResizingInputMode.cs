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
using System.Collections.Generic;
using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Input.NodeGroupResizing
{
  public enum ResizeMode
  {
    /// <summary>
    /// Scales the locations of the nodes while preserving their sizes.
    /// </summary>
    Scale,

    /// <summary>
    /// Scales the content of the selection rectangle uniformly by scaling both the node locations and the node sizes.
    /// </summary>
    Resize
  }

  /// <summary>
  /// An <see cref="IInputMode"/> implementation for reshape handles for groups of nodes. Can be added as child input
  /// mode of <see cref="GraphEditorInputMode"/> and changes the default node reshape handles when multiple nodes are
  /// selected: instead of one set of handles per node, this input mode only shows a single set of handles around all
  /// selected nodes.
  /// </summary>
  /// <remarks>
  /// Supports two different <see cref="ResizeMode"/>s.
  /// </remarks>
  public class NodeGroupResizingInputMode : IInputMode
  {
    private InsetsD margins = InsetsD.Empty;
    private ResizeMode mode = ResizeMode.Scale;

    private HandleInputMode handleInputMode;

    private readonly OrthogonalEdgeEditingHelper moveHandleOrthogonalHelper = new OrthogonalEdgeEditingHelper();
    private EncompassingRectangle rectangle;
    private ICanvasObject rectCanvasObject;

    #region IInputMode

    /// <summary>
    /// Gets or sets the margins between the handle rectangle and the bounds of the selected nodes.
    /// </summary>
    public InsetsD Margins {
      get { return margins; }
      set { margins = value; }
    }

    /// <summary>
    /// Gets or sets the current <see cref="ResizeMode"/>
    /// </summary>
    public ResizeMode Mode {
      get { return mode; }
      set {
        mode = value;
        if (handleInputMode != null) {
          UpdateHandles();
        }
      }
    }
    
    public IInputModeContext InputModeContext { get; private set; }

    public int Priority { get; set; }

    public void Install(IInputModeContext context, ConcurrencyController controller) {
      InputModeContext = context;
      var geim = context.ParentInputMode as GraphEditorInputMode;
      if (geim == null) {
        throw new InvalidOperationException( "NodeGroupResizingInputMode must be installed as child mode of GraphEditorInputMode");
      }

      // create own HandleInputMode for the handles
      handleInputMode = new HandleInputMode { Priority = 1 };

      // notify the GraphSnapContext which nodes are resized and shouldn't provide SnapLines
      handleInputMode.DragStarted += RegisterReshapedNodes;

      // forward events to OrthogonalEdgeEditingContext so it can handle keeping edges at reshaped nodes orthogonal 
      handleInputMode.DragStarting += moveHandleOrthogonalHelper.Starting;
      handleInputMode.DragStarted += moveHandleOrthogonalHelper.Started;
      handleInputMode.DragFinished += moveHandleOrthogonalHelper.Finished;
      handleInputMode.DragCanceled += moveHandleOrthogonalHelper.Canceled;

      handleInputMode.Install(context, controller);
      handleInputMode.Enabled = false;

      // update handles depending on the changed node selection
      geim.MultiSelectionStarted += MultiSelectionStarted;
      geim.MultiSelectionFinished += MultiSelectionFinished;
      ((GraphControl) context.CanvasControl).Selection.ItemSelectionChanged += ItemSelectionChanged;

      // add a NodeLayoutChanged listener so the reshape rect is updated when the nodes are moved (e.g. through
      // layout animations or MoveInputMode).
      context.GetGraph().NodeLayoutChanged += NodeLayoutChanged;
    }

    /// <summary>
    /// Notifies the current <see cref="GraphSnapContext"/> which nodes are going to be reshaped.
    /// </summary>
    private void RegisterReshapedNodes(object sender, InputModeEventArgs e) {
      // register reshaped nodes
      var snapContext = e.Context.Lookup<GraphSnapContext>();
      if (snapContext != null && snapContext.Enabled) {
        foreach (var node in rectangle.Nodes) {
          snapContext.AddItemToBeReshaped(node);
        }
      }
    }

    /// <summary>
    /// Invalidates the (bounds of the) <see cref="EncompassingRectangle"/> when any node layout is changed
    /// but not by this input mode. 
    /// </summary>
    private void NodeLayoutChanged(object source, INode node, RectD oldLayout) {
      if (rectangle != null && !handleInputMode.IsDragging) {
        rectangle.Invalidate();
      }
    }

    public bool TryStop() {
      RemoveRectangleVisualization();
      return handleInputMode.TryStop();
    }

    public void Cancel() {
      RemoveRectangleVisualization();
      handleInputMode.Cancel();
    }

    public void Uninstall(IInputModeContext context) {
      context.GetGraph().NodeLayoutChanged -= NodeLayoutChanged;
      var geim = context.ParentInputMode as GraphEditorInputMode;
      geim.MultiSelectionStarted -= MultiSelectionStarted;
      geim.MultiSelectionFinished -= MultiSelectionFinished;
      ((GraphControl) context.CanvasControl).Selection.ItemSelectionChanged -= ItemSelectionChanged;

      handleInputMode.DragStarted -= RegisterReshapedNodes;
      handleInputMode.DragStarting -= moveHandleOrthogonalHelper.Starting;
      handleInputMode.DragFinished -= moveHandleOrthogonalHelper.Finished;
      handleInputMode.DragCanceled -= moveHandleOrthogonalHelper.Canceled;
      handleInputMode.DragStarted -= moveHandleOrthogonalHelper.Started;

      RemoveRectangleVisualization();
      handleInputMode.Uninstall(context);
      handleInputMode = null;
      InputModeContext = null;
    }

    private bool ignoreSingleSelectionEvents;

    private void MultiSelectionStarted(object sender, SelectionEventArgs<IModelItem> args) {
      // a multi-selection started so the ItemSelectionChanged events can be ignored until MultiSelectionFinished
      ignoreSingleSelectionEvents = true;
    }

    private void MultiSelectionFinished(object sender, SelectionEventArgs<IModelItem> args) {
      ignoreSingleSelectionEvents = false;
      UpdateHandles();
    }

    private void ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs<IModelItem> e) {
      UpdateHandles();
    }

    private void UpdateHandles() {
      if (ignoreSingleSelectionEvents) {
        // UpdateHandles was called by ItemSelectionChanged by this is a MultiSelection so we wait for MultiSelectionFinished
        return;
      }
      // first, clear any existing handles
      ClearHandles();

      var geim = InputModeContext.ParentInputMode as GraphEditorInputMode;
      var selectedNodesCount = geim.GraphControl.Selection.SelectedNodes.Count;
      // use default behavior only if one node is selected
      geim.HandleInputMode.Enabled = selectedNodesCount <= 1;

      if (selectedNodesCount >= 2) {
        // more then one node is selected so initialize resizing them as a group
        ShowHandles();
      }
    }

    /// <summary>
    /// Clears any existing handles and disables the handleInputMode.
    /// </summary>
    private void ClearHandles() {
      if (!handleInputMode.TryStop()) {
        handleInputMode.Cancel();
      }
      handleInputMode.Enabled = false;
      RemoveRectangleVisualization();
    }

    /// <summary>
    /// Initializes the handles, the reshapeHandler and enables the handleInputMode.
    /// </summary>
    private void ShowHandles() {
      var graphControl = InputModeContext.CanvasControl as GraphControl;
      
      // collect all selected nodes as well as their descendents
      var reshapeNodes = CollectReshapeNodes(graphControl.Graph, graphControl.Selection);

      // create a mutable rectangle, that is updated by the ReshapeHandler
      rectangle = new EncompassingRectangle(reshapeNodes, margins);
      // and visualize it
      var rectangleIndicator = new RectangleIndicatorInstaller(rectangle, RectangleIndicatorInstaller.SelectionTemplateKey);
      rectCanvasObject = rectangleIndicator .AddCanvasObject(graphControl.CanvasContext, graphControl.InputModeGroup, rectangle);
      rectCanvasObject.ToBack();

      // Create a reshape handler factory depending on the current mode
      Func<ReshapeHandlerBase> reshapeHandlerFactory;
      if ((Mode == ResizeMode.Scale)) {
        reshapeHandlerFactory = () => new ScalingReshapeHandler(rectangle);
      } else {
        reshapeHandlerFactory = () => new ResizingReshapeHandler(rectangle);  
      }

      // create and add the handles to our HandleInputMode
      handleInputMode.Handles = new DefaultObservableCollection<IHandle> {
          CreateHandle(HandlePositions.North, reshapeHandlerFactory),
          CreateHandle(HandlePositions.NorthWest, reshapeHandlerFactory),
          CreateHandle(HandlePositions.West, reshapeHandlerFactory),
          CreateHandle(HandlePositions.SouthWest, reshapeHandlerFactory),
          CreateHandle(HandlePositions.South, reshapeHandlerFactory),
          CreateHandle(HandlePositions.SouthEast, reshapeHandlerFactory),
          CreateHandle(HandlePositions.East, reshapeHandlerFactory),
          CreateHandle(HandlePositions.NorthEast, reshapeHandlerFactory)
      };
      handleInputMode.Enabled = true;
    }
    
    /// <summary>
    /// Collect all <see cref="IGraphSelection.SelectedNodes">selected nodes</see> and their descendents.
    /// </summary>
    private IList<INode> CollectReshapeNodes(IGraph graph, IGraphSelection selection) {
      var nodes = new HashSet<INode>();
      foreach (var node in selection.SelectedNodes) {
        if (nodes.Add(node) && graph.IsGroupNode(node)) {
          foreach (var descendant in graph.GetGroupingSupport().GetDescendants(node)) {
            nodes.Add(descendant);
          }
        }
      }
      return nodes.ToList();
    }

    private IHandle CreateHandle(HandlePositions position, Func<ReshapeHandlerBase> reshapeHandlerFactory) {
      var reshapeHandler = reshapeHandlerFactory.Invoke();
      var handle = new NodeGroupHandle(InputModeContext, position, reshapeHandler, Margins);
      reshapeHandler.Handle = handle;
      return handle;
    }

    /// <summary>
    /// Removes the rectCanvasObject.
    /// </summary>
    private void RemoveRectangleVisualization() {
      if (rectCanvasObject != null) {
        rectCanvasObject.Remove();
        rectCanvasObject = null;
      }
      rectangle = null;
    }

    #endregion

    #region EncompassingRectangle Class

    /// <summary>
    /// An <see cref="IRectangle"/> implementation that encompasses a set of <see cref="INode"/> layouts. Can be
    /// <see cref="Invalidate">invalidated</see> to fit the encompassed nodes or explicitly
    /// <see cref="Reshape">reshaped</see>.
    /// </summary>
    sealed class EncompassingRectangle : IRectangle
    {
      private readonly IEnumerable<INode> nodes;
      private readonly InsetsD margins;
      private readonly MutableRectangle rectangle = new MutableRectangle();
      private RectD tightRect;
      private bool invalid;

      public EncompassingRectangle(IEnumerable<INode> nodes, InsetsD margins) {
        this.nodes = nodes;
        this.margins = margins;
        this.invalid = true;
      }

      public void Invalidate() {
        invalid = true;
      }

      public void Reshape(IRectangle newRectangle) {
        tightRect = newRectangle.ToRectD();
        rectangle.Reshape(tightRect.GetEnlarged(Margins));
        invalid = false;
      }

      private void Update() {
        if (!invalid) {
          return;
        }

        rectangle.Width = -1;
        rectangle.Height = -1;
        rectangle.X = 0;
        rectangle.Y = 0;

        foreach (var node in nodes) {
          rectangle.SetToUnion(rectangle, node.Layout);
        }
        tightRect = rectangle.ToRectD();

        rectangle.X -= margins.Left;
        rectangle.Y -= margins.Top;
        rectangle.Width += margins.Left + margins.Right;
        rectangle.Height += margins.Top + margins.Bottom;

        invalid = false;
      }

      public double Width {
        get {
          Update();
          return rectangle.Width;
        }
      }

      public double Height {
        get {
          Update();
          return rectangle.Height;
        }
      }

      public double X {
        get {
          Update();
          return rectangle.X;
        }
      }

      public double Y {
        get {
          Update();
          return rectangle.Y;
        }
      }

      public IEnumerable<INode> Nodes {
        get { return nodes; }
      }

      public InsetsD Margins {
        get { return margins; }
      }

      public RectD TightRectangle {
        get {
          Update();
          return tightRect;
        }
      }
    }
    
    #endregion
    
    #region ReshapeHandler Classes

    /// <summary>
    /// A subclass of <see cref="ReshapeHandlerBase"/> that implements the resize logic for the <see cref="ResizeMode.Scale"/>
    /// resize mode.
    /// </summary>
    private sealed class ScalingReshapeHandler : ReshapeHandlerBase
    {
      public ScalingReshapeHandler(EncompassingRectangle rectangle) : base(rectangle) { }

      /// <summary>
      /// Returns the size of the smallest node (the reshape rect cannot get smaller than this, since the sizes of the
      /// nodes are not modified).
      /// </summary>
      protected override ISize CalculateMinimumSize() {
        var minSize = new MutableSize();
        foreach (var node in ReshapeNodes) {
          minSize.Width = Math.Max(minSize.Width, node.Layout.Width);
          minSize.Height = Math.Max(minSize.Height, node.Layout.Height);
        }
        return minSize;
      }

      protected override ISize CalculateMaximumSize() {
        return SizeD.Infinite;
      }

      protected override PointD GetFactor(double x, double y, RectD originalNodeLayout, bool centered, HandlePositions position) {
        double fx = 0;
        if ((position & HandlePositions.Vertical) == 0) {
          var boundsWidth = (OriginalBounds.Width - originalNodeLayout.Width);
          if (boundsWidth <= 0) {
            fx = centered ? 0 : 0.5;
          } else {
            var xRatio = centered
                ? 2 * (originalNodeLayout.CenterX - OriginalBounds.CenterX) / boundsWidth
                : (originalNodeLayout.MinX - OriginalBounds.X) / boundsWidth;
            if (position.IsAnyWest()) {
              fx = centered ? -xRatio : 1 - xRatio;
            } else if (position.IsAnyEast()) {
              fx = xRatio;
            }
          }
        }
        double fy = 0;
        if ((position & HandlePositions.Horizontal) == 0) {
          var boundsHeight = (OriginalBounds.Height - originalNodeLayout.Height);
          if (boundsHeight <= 0) {
            fy = centered ? 0 : 0.5;
          } else {
            var yRatio = centered
                ? 2 * (originalNodeLayout.CenterY - OriginalBounds.CenterY) / boundsHeight
                : (originalNodeLayout.MinY - OriginalBounds.Y) / boundsHeight;
            if (position.IsAnyNorth()) {
              fy = centered ? -yRatio : 1 - yRatio;
            } else if (position.IsAnySouth()) {
              fy = yRatio;
            }
          }
        }

        return new PointD(fx, fy);
      }

      public override void HandleReshape(IInputModeContext context, RectD originalBounds, RectD newBounds) {
        base.HandleReshape(context, originalBounds, newBounds);
        // some reshaped nodes might got moved outside their parents bounds so enlarge the group node bounds if necessary
        var graph = context.GetGraph();
        foreach (var node in ReshapeNodes) {
          if (graph.IsGroupNode(node)) {
            graph.GetGroupingSupport().EnlargeGroupNode(context, node, true);
          }
        }
      }
    }

    /// <summary>
    /// A subclass of <see cref="ReshapeHandlerBase"/> that implements the resize logic for the <see cref="ResizeMode.Resize"/>
    /// resize mode.
    /// </summary>
    private sealed class ResizingReshapeHandler : ReshapeHandlerBase
    {
      public ResizingReshapeHandler(EncompassingRectangle rectangle) : base(rectangle) { }

      /// <summary>
      /// Considers the minimum scale factors for each node to respect its <see cref="INodeSizeConstraintProvider.GetMinimumSize"/>
      /// and combine them to a general minimum size.
      /// </summary>
      protected override ISize CalculateMinimumSize() {
        double minScaleX = 0;
        double minScaleY = 0;

        foreach (var node in ReshapeNodes) {
          var constraintProvider = node.Lookup<INodeSizeConstraintProvider>();
          if (constraintProvider != null) {
            var minSize = constraintProvider.GetMinimumSize(node);
            if (minSize != SizeD.Empty) {
              var originalLayout = originalNodeLayouts[node];
              minScaleX = Math.Max(minScaleX, minSize.Width / originalLayout.Width);
              minScaleY = Math.Max(minScaleY, minSize.Height / originalLayout.Height);
            }
          }
        }

        double minWidth = this.OriginalBounds.Width * minScaleX;
        double minHeight = this.OriginalBounds.Height * minScaleY;
        return new SizeD(minWidth, minHeight);
      }

      /// <summary>
      /// Considers the maximum scale factors for each node to respect its <see cref="INodeSizeConstraintProvider.GetMaximumSize"/>
      /// and combine them to a general maximum size.
      /// </summary>
      protected override ISize CalculateMaximumSize() {
        double maxScaleX = double.PositiveInfinity;
        double maxScaleY = double.PositiveInfinity;

        foreach (var node in ReshapeNodes) {
          var constraintProvider = node.Lookup<INodeSizeConstraintProvider>();
          if (constraintProvider != null) {
            var maxSize = constraintProvider.GetMaximumSize(node);
            if (maxSize != SizeD.Infinite) {
              var originalLayout = originalNodeLayouts[node];
              maxScaleX = Math.Min(maxScaleX, maxSize.Width / originalLayout.Width);
              maxScaleY = Math.Min(maxScaleY, maxSize.Height / originalLayout.Height);
            }
          }
        }

        double maxWidth = this.OriginalBounds.Width * maxScaleX;
        double maxHeight = this.OriginalBounds.Height * maxScaleY;
        return new SizeD(maxWidth, maxHeight);
      }

      protected override PointD GetFactor(double x, double y, RectD originalNodeLayout, bool centered, HandlePositions position) {
        var xRatio = centered 
            ? 2 * (x - OriginalBounds.CenterX) / OriginalBounds.Width
            : (x - OriginalBounds.X) / OriginalBounds.Width;
        var yRatio = centered 
            ? 2 * (y - OriginalBounds.CenterY) / OriginalBounds.Height
            : (y - OriginalBounds.Y) / OriginalBounds.Height;

        double fx = 0;
        if (position.IsAnyWest()) {
          fx = centered ? -xRatio : 1 - xRatio;
        } else if (position.IsAnyEast()) {
          fx = xRatio;
        }
        double fy = 0;
        if (position.IsAnyNorth()) {
          fy = centered ? -yRatio : 1 - yRatio;
        } else if (position.IsAnySouth()) {
          fy = yRatio;
        }
        return new PointD(fx, fy);
      }
    }

    /// <summary>
    /// The base <see cref="IReshapeHandler"/> class for the two resize modes.
    /// </summary>
    /// <remarks>
    /// This base class implements the interface methods, handles undo/redo support, orthogonal edge editing
    /// and snapping, and contains code common to both modes. 
    /// </remarks>
    private abstract class ReshapeHandlerBase : IReshapeHandler
    {
      // dictionaries storing the original layout, reshape handler and snap result provider of the reshape nodes
      protected readonly IDictionary<INode, RectD> originalNodeLayouts = new Dictionary<INode, RectD>();
      private readonly IDictionary<INode, IReshapeHandler> reshapeHandlers = new Dictionary<INode, IReshapeHandler>();
      private readonly IDictionary<INode, INodeReshapeSnapResultProvider> reshapeSnapResultProviders = new Dictionary<INode, INodeReshapeSnapResultProvider>();
      private readonly IDictionary<INode, OrthogonalEdgeDragHandler> orthogonalEdgeDragHandlers = new Dictionary<INode, OrthogonalEdgeDragHandler>();

      private ICompoundEdit compoundEdit;

      private readonly EncompassingRectangle rectangle;
      
      /// <summary>
      /// Gets a view of the bounds of the item.
      /// </summary>
      public IRectangle Bounds { get { return this.rectangle.TightRectangle; } }

      /// <summary>
      /// Returns the original bounds of the reshaped <see cref="EncompassingRectangle"/> without its margins.
      /// </summary>
      protected RectD OriginalBounds { get; private set; }

      /// <summary>
      /// Returns the nodes to be reshaped.
      /// </summary>
      protected IEnumerable<INode> ReshapeNodes { get { return rectangle.Nodes; } }
      
      /// <summary>
      /// The <see cref="NodeGroupHandle"/> using this <see cref="IReshapeHandler"/>.
      /// </summary>
      public NodeGroupHandle Handle { get; set; }

      protected ReshapeHandlerBase(EncompassingRectangle rectangle) {
        this.rectangle = rectangle;
      }

      public virtual void InitializeReshape(IInputModeContext context) {
        this.OriginalBounds = rectangle.TightRectangle;

        // register our CollectSnapResults callback
        var snapContext = context.Lookup<GraphSnapContext>();
        if (snapContext != null) {
          snapContext.CollectSnapResults += CollectSnapResults;
        }

        // store original node layouts, reshape handlers and reshape snap result providers
        foreach (var node in ReshapeNodes) {
          originalNodeLayouts.Add(node, node.Layout.ToRectD());

          // store reshape handler to change the shape of node
          var reshapeHandler = node.Lookup<IReshapeHandler>();
          if (reshapeHandler != null) {
            reshapeHandler.InitializeReshape(context);
            reshapeHandlers.Add(node, reshapeHandler);
          }
          // store reshape snap result provider to collect snap results where node would snap to snaplines etc.
          var snapResultProvider = node.Lookup<INodeReshapeSnapResultProvider>();
          if (snapContext != null && snapResultProvider != null) {
            reshapeSnapResultProviders.Add(node, snapResultProvider);
          }
          // store orthogonal edge drag handler that keeps edges at node orthogonal
          var orthogonalEdgeDragHandler = OrthogonalEdgeEditingContext.CreateOrthogonalEdgeDragHandler(context, node, false);
          if (orthogonalEdgeDragHandler != null) {
            orthogonalEdgeDragHandlers[node] = orthogonalEdgeDragHandler;
          }
        }

        // update the minimum/maximum size of the handle considering all initial node layouts
        Handle.MinimumSize = CalculateMinimumSize();
        Handle.MaximumSize = CalculateMaximumSize();

        // start a compound undo unit
        this.compoundEdit = context.GetGraph().BeginEdit("Undo Group Resize", "Redo Group Resize");
      }

      private void CollectSnapResults(object sender, CollectSnapResultsEventArgs args) {
        var lastEvent = args.Context.CanvasControl.LastInputEvent;
        var fixedAspectRatio = Handle.RatioReshapeRecognizer.Invoke(this, lastEvent);
        var centered = Handle.CenterReshapeRecognizer.Invoke(this, lastEvent);

        var reshapePolicy = fixedAspectRatio ? Handle.ReshapePolicy : ReshapePolicy.None;
        var ratio = OriginalBounds.Width / OriginalBounds.Height;

        var minScaleX = Handle.MinimumSize.Width / this.OriginalBounds.Width;
        var minScaleY = Handle.MinimumSize.Height / this.OriginalBounds.Height;
        var maxScaleX = Handle.MaximumSize.Width / this.OriginalBounds.Width;
        var maxScaleY = Handle.MaximumSize.Height / this.OriginalBounds.Height;

        foreach (var pair in reshapeSnapResultProviders) {
          // for each selected node that has an INodeReshapeSnapResultProvider we have to create
          // a suiting ReshapeRectangleContext
          var node = pair.Key;
          var layout = originalNodeLayouts[node];

          // get factors that determine how the node layout changes depending on the mouse delta
          var topLeftChangeFactor = FixZero(GetFactor(layout.MinX, layout.MinY, layout, centered, Handle.Position));
          var bottomRightChangeFactor = FixZero(GetFactor(layout.MaxX, layout.MaxY, layout, centered, Handle.Position));
          
          // the SizeChangeFactor can be calculated using those two factors
          var pointDiffFactor = FixZero(bottomRightChangeFactor - topLeftChangeFactor);
          var sizeChangeFactor = new SizeD(pointDiffFactor.X, pointDiffFactor.Y);

          var reshapeRectangleContext = new ReshapeRectangleContext(
              layout,
              new SizeD(layout.Width * minScaleX, layout.Height * minScaleY),
              new SizeD(layout.Width * maxScaleX, layout.Height * maxScaleY),
              RectD.Empty, RectD.Infinite,
              Handle.Position,
              topLeftChangeFactor,
              bottomRightChangeFactor,
              sizeChangeFactor,
              reshapePolicy, 
              ratio);

          // call the INodeReshapeSnapResultProvider
          pair.Value.CollectSnapResults((GraphSnapContext) sender, args, node, reshapeRectangleContext);
        }
      }

      /// <summary>
      /// Calculates the <see cref="ReshapeHandlerHandle.MinimumSize"/> considering all reshaped nodes.
      /// </summary>
      protected abstract ISize CalculateMinimumSize();

      /// <summary>
      /// Calculates the <see cref="ReshapeHandlerHandle.MaximumSize"/> considering all reshaped nodes.
      /// </summary>
      protected abstract ISize CalculateMaximumSize();

      /// <summary>
      /// Calculates the horizontal and vertical factor the mouse movement has to be multiplied with to get the
      /// horizontal and vertical delta for the point (x,y) inside the <paramref name="originalNodeLayout"/>.
      /// </summary>
      /// <param name="x">The horizontal location inside <paramref name="originalNodeLayout"/>.</param>
      /// <param name="y">The vertical location inside <paramref name="originalNodeLayout"/>.</param>
      /// <param name="originalNodeLayout">The original layout of the node to calculate the factors for.</param>
      /// <param name="centered">Whether center resizing is active.</param>
      /// <param name="position">The handle position to calculate the factor for.</param>
      protected abstract PointD GetFactor(double x, double y, RectD originalNodeLayout, bool centered, HandlePositions position);

      /// <summary>
      /// Calculates the vertical and horizontal factor the mouse movement has to be multiplied with to get the
      /// horizontal and vertical delta for the point (x,y) inside the <paramref name="originalNodeLayout"/>.
      /// </summary>
      /// <remarks>
      /// <para>
      /// This factor is only used for <see cref="ReshapeHandlerHandle.RatioReshapeRecognizer">ratio resizing</see>
      /// using either <see cref="ReshapePolicy.Horizontal"/> or <see cref="ReshapePolicy.Vertical"/>.
      /// </para>
      /// <para>
      /// The horizontal delta for point (x,y) is the vertical mouse delta multiplied by the y value of the returned factor.
      /// The vertical delta for point (x,y) is the horizontal mouse delta multiplied by the x value of the returned factor.
      /// </para>
      /// </remarks>
      /// <param name="x">The horizontal location inside <paramref name="originalNodeLayout"/>.</param>
      /// <param name="y">The vertical location inside <paramref name="originalNodeLayout"/>.</param>
      /// <param name="originalNodeLayout">The original layout of the node to calculate the factors for.</param>
      /// <param name="centered">Whether center resizing is active.</param>
      private PointD GetOrthogonalFactor(double x, double y, RectD originalNodeLayout, bool centered) {
        var ratio = OriginalBounds.Width / OriginalBounds.Height;
        if (Handle.ReshapePolicy == ReshapePolicy.Horizontal) {
          var x2y = 1 / (ratio * (centered ? 1 : 2));
          var orthogonalPosition = Handle.Position == HandlePositions.East ? HandlePositions.South : HandlePositions.North;
          var orthoFactor = GetFactor(x, y, originalNodeLayout, true, orthogonalPosition);
          return new PointD(orthoFactor.Y * x2y, 0);
        } else if (Handle.ReshapePolicy == ReshapePolicy.Vertical) {
          var x2y = ratio / (centered ? 1 : 2);
          var orthogonalPosition = Handle.Position == HandlePositions.South ? HandlePositions.East : HandlePositions.West;
          var orthoFactor = GetFactor(x, y, originalNodeLayout, true, orthogonalPosition);
          return new PointD(0, orthoFactor.X * x2y);          
        }
        return PointD.Origin;
      }

      public virtual void HandleReshape(IInputModeContext context, RectD originalBounds, RectD newBounds) {
        // reshape the encompassing rectangle
        rectangle.Reshape(newBounds);
        
        // update node layouts and bend locations
        UpdateNodeLayouts(context, originalBounds, newBounds);
      }

      private void UpdateNodeLayouts(IInputModeContext context, RectD originalBounds, RectD newBounds) {
        var dMinX = newBounds.X - originalBounds.X;
        var dMinY = newBounds.Y - originalBounds.Y;
        var dMaxX = newBounds.MaxX - originalBounds.MaxX;
        var dMaxY = newBounds.MaxY - originalBounds.MaxY;

        // calculate a possible mouse movement that could have led to the newBounds
        double dx = 0;
        double dy = 0;
        if (Handle.Position.IsAnyWest()) {
          dx = dMinX;
        } else if (Handle.Position.IsAnyEast()) {
          dx = dMaxX;
        }
        if (Handle.Position.IsAnyNorth()) {
          dy = dMinY;
        } else if (Handle.Position.IsAnySouth()) {
          dy = dMaxY;
        }

        var centerResize = Handle.CenterReshapeRecognizer(this, context.CanvasControl.LastInputEvent);
        var ratioResize = Handle.RatioReshapeRecognizer(this, context.CanvasControl.LastInputEvent);
        var useOrthogonalFactors = ratioResize && (Handle.ReshapePolicy == ReshapePolicy.Horizontal || Handle.ReshapePolicy == ReshapePolicy.Vertical);
        
        foreach (var pair in originalNodeLayouts) {
          var node = pair.Key;
          var originalLayout = pair.Value;
          if (reshapeHandlers.TryGetValue(node, out var reshapeHandler)) {
            var topLeftFactor = GetFactor(originalLayout.X, originalLayout.Y, originalLayout, centerResize, Handle.Position);
            var bottomRightFactor = GetFactor(originalLayout.MaxX, originalLayout.MaxY, originalLayout, centerResize, Handle.Position);
            var orthogonalTopLeftFactor = PointD.Origin;
            var orthogonalBottomRightFactor = PointD.Origin;
            if (useOrthogonalFactors) {
              orthogonalTopLeftFactor = GetOrthogonalFactor(originalLayout.X, originalLayout.Y, originalLayout, centerResize);
              orthogonalBottomRightFactor = GetOrthogonalFactor(originalLayout.MaxX, originalLayout.MaxY, originalLayout, centerResize);
            }
            
            var newX = originalLayout.X + dx * topLeftFactor.X + dy * orthogonalTopLeftFactor.Y;
            var newY = originalLayout.Y + dy * topLeftFactor.Y + dx * orthogonalTopLeftFactor.X;
            var newMaxX = originalLayout.MaxX + dx * bottomRightFactor.X + dy * orthogonalBottomRightFactor.Y;
            var newMaxY = originalLayout.MaxY + dy * bottomRightFactor.Y + dx * orthogonalBottomRightFactor.X;

            var newLayout = new RectD(newX, newY, newMaxX - newX, newMaxY - newY);
            reshapeHandler.HandleReshape(context, originalLayout, newLayout);
          }
        }
        foreach (var pair in orthogonalEdgeDragHandlers) {
          pair.Value.HandleMove();
        }
        
      }

      public virtual void CancelReshape(IInputModeContext context, RectD originalBounds) {
        rectangle.Reshape(originalBounds);
        foreach (var pair in reshapeHandlers) {
          pair.Value.CancelReshape(context, originalNodeLayouts[pair.Key]);
        }
        foreach (var pair in this.orthogonalEdgeDragHandlers) {
          pair.Value.CancelDrag();
        }
        compoundEdit.Cancel();
        Clear(context);
      }

      public virtual void ReshapeFinished(IInputModeContext context, RectD originalBounds, RectD newBounds) {
        foreach (var pair in reshapeHandlers) {
          pair.Value.ReshapeFinished(context, originalNodeLayouts[pair.Key], pair.Value.Bounds.ToRectD());
        }
        foreach (var pair in orthogonalEdgeDragHandlers) {
          pair.Value.FinishDrag();
        }

        compoundEdit.Commit();
        Clear(context);
      }

      protected virtual void Clear(IInputModeContext context) {
        var snapContext = context.Lookup<GraphSnapContext>();
        if (snapContext != null) {
          snapContext.CollectSnapResults -= CollectSnapResults;
        }
        reshapeSnapResultProviders.Clear();
        originalNodeLayouts.Clear();
        reshapeHandlers.Clear();
        orthogonalEdgeDragHandlers.Clear();
        compoundEdit = null;
      }

      /// <summary>
      /// Sets x or y values that are close to 0 to be 0.
      /// </summary>
      private static PointD FixZero(PointD p) {
        var fixedX = Math.Abs(p.X) < 0.0001 ? 0 : p.X;
        var fixedY = Math.Abs(p.Y) < 0.0001 ? 0 : p.Y;
        return new PointD(fixedX, fixedY);
      }
    }

    #endregion

    #region OrthogonalEdgeEditingHelper Class

    /// <summary>
    /// Simplifies handling the <see cref="OrthogonalEdgeEditingContext"/> by listening to <see cref="HandleInputMode"/>
    /// events.
    /// </summary>
    private sealed class OrthogonalEdgeEditingHelper
    {
      private OrthogonalEdgeEditingContext editingContext;

      public void Starting(object sender, InputModeEventArgs e) {
        IInputModeContext context = e.Context;
        var edgeEditingContext = context.Lookup<OrthogonalEdgeEditingContext>();
        if (edgeEditingContext != null && !edgeEditingContext.IsInitializing && !edgeEditingContext.IsInitialized) {
          editingContext = edgeEditingContext;
          editingContext.InitializeDrag(context);
        } else {
          editingContext = null;
        }
      }

      public void Started(object sender, InputModeEventArgs e) {
        if (editingContext != null) {
          editingContext.DragInitialized();
        }
      }

      public void Finished(object sender, InputModeEventArgs e) {
        if (editingContext != null) {
          editingContext.DragFinished();
          editingContext = null;
        }
      }

      public void Canceled(object sender, InputModeEventArgs e) {
        if (editingContext != null) {
          editingContext.CancelDrag();
          editingContext = null;
        }
      }
    }

    #endregion

    #region NodeGroupHandle Class

    /// <summary>
    /// A <see cref="ReshapeHandlerHandle"/> for an <see cref="EncompassingRectangle"/> that considers the
    /// <see cref="EncompassingRectangle.Margins"/> for the calculation of its <see cref="IDragHandler.Location"/>.
    /// </summary>
    sealed class NodeGroupHandle : ReshapeHandlerHandle, IDragHandler
    {
      private readonly IInputModeContext context;
      private InsetsD margins;

      public NodeGroupHandle(IInputModeContext context, HandlePositions position, IReshapeHandler reshapeHandler, InsetsD margins) 
          : base(position, reshapeHandler) {
        this.margins = margins;
        this.context = context;
        
        if ((position & HandlePositions.Vertical) != 0) {
          ReshapePolicy = ReshapePolicy.Vertical;
        } else if ((position & HandlePositions.Horizontal) != 0) {
          ReshapePolicy = ReshapePolicy.Horizontal;
        } else {
          ReshapePolicy = ReshapePolicy.Projection;
        }
      }

      private HandleLocation location;

      IPoint IDragHandler.Location {
        get {
          if (location == null) {
            location = new HandleLocation(this);
          }
          return location;
        }
      }

      /// <summary>
      /// An <see cref="IPoint"/> implementation that represents the location of a <see cref="NodeGroupHandle"/>.
      /// </summary>
      /// <remarks>
      /// The handle location is calculated considering the position of the handle, the current bounds of the
      /// reshape handler and the margins of the <see cref="EncompassingRectangle"/> as well as an additional
      /// zoom-dependent offset.
      /// </remarks>
      private sealed class HandleLocation : IPoint
      {
        private const int Offset = 5;
        private readonly NodeGroupHandle outerThis;

        public HandleLocation(NodeGroupHandle nodeGroupHandle) {
          outerThis = nodeGroupHandle;
        }

        public double X {
          get {
            var bounds = outerThis.ReshapeHandler.Bounds;
            switch (outerThis.Position) {
              case HandlePositions.NorthWest:
              case HandlePositions.West:
              case HandlePositions.SouthWest:
                return bounds.X - (outerThis.margins.Left + Offset / outerThis.context.Zoom);
              case HandlePositions.North:
              case HandlePositions.Center:
              case HandlePositions.South:
              default:
                return bounds.X + bounds.Width * 0.5d;
              case HandlePositions.NorthEast:
              case HandlePositions.East:
              case HandlePositions.SouthEast:
                return bounds.X + bounds.Width + (outerThis.margins.Right  + Offset / outerThis.context.Zoom);
            }
          }
        }

        public double Y {
          get {
            var bounds = outerThis.ReshapeHandler.Bounds;
            switch (outerThis.Position) {
              case HandlePositions.NorthWest:
              case HandlePositions.North:
              case HandlePositions.NorthEast:
                return bounds.Y - (outerThis.margins.Top + Offset / outerThis.context.Zoom);
              case HandlePositions.West:
              case HandlePositions.Center:
              case HandlePositions.East:
              default:
                return bounds.Y + bounds.Height * 0.5d;
              case HandlePositions.SouthWest:
              case HandlePositions.South:
              case HandlePositions.SouthEast:
                return bounds.Y + bounds.Height + (outerThis.margins.Top + Offset / outerThis.context.Zoom);
            }
          }
        }
      }
    }
    
    #endregion
  }

  #region HandlePositionsExtensions
  
  /// <summary>
  /// Provides extension methods to check the kind of a <see cref="HandlePositions"/>.
  /// </summary>
  static class HandlePositionsExtensions
  {
    /// <summary>
    /// Returns if <paramref name="position"/> is <see cref="HandlePositions.NorthWest"/>,
    /// <see cref="HandlePositions.North"/> or <see cref="HandlePositions.NorthEast"/>
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>If the position is at any of the north sides.</returns>
    public static bool IsAnyNorth(this HandlePositions position) {
      return (position & (HandlePositions.NorthWest | HandlePositions.North | HandlePositions.NorthEast)) != 0;
    }
    
    /// <summary>
    /// Returns if <paramref name="position"/> is <see cref="HandlePositions.SouthWest"/>,
    /// <see cref="HandlePositions.South"/> or <see cref="HandlePositions.SouthEast"/>
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>If the position is at any of the south sides.</returns>
    public static  bool IsAnySouth(this HandlePositions position) {
      return (position & (HandlePositions.SouthWest | HandlePositions.South | HandlePositions.SouthEast)) != 0;
    }

    /// <summary>
    /// Returns if <paramref name="position"/> is <see cref="HandlePositions.NorthWest"/>,
    /// <see cref="HandlePositions.West"/> or <see cref="HandlePositions.SouthWest"/>
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>If the position is at any of the west sides.</returns>
    public static  bool IsAnyWest(this HandlePositions position) {
      return (position & (HandlePositions.NorthWest | HandlePositions.West | HandlePositions.SouthWest)) != 0;
    }
      
    /// <summary>
    /// Returns if <paramref name="position"/> is <see cref="HandlePositions.NorthEast"/>,
    /// <see cref="HandlePositions.East"/> or <see cref="HandlePositions.SouthEast"/>
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>If the position is at any of the east sides.</returns>
    public static  bool IsAnyEast(this HandlePositions position) {
      return (position & (HandlePositions.NorthEast | HandlePositions.East | HandlePositions.SouthEast)) != 0;
    }
  }
  
  #endregion
}
