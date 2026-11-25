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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.DragFromComponent
{
  /// <summary>
  ///   An input mode that starts a native WPF drag operation when a node is dragged out of the graph control.
  /// </summary>
  /// <remarks>
  ///   Register WPF elements that accept dropped nodes with <see cref="AddDropTarget"/>. The mode displays
  ///   a rendered node preview in an adorner while the drag is in progress. Use <see cref="CreateNodePreview"/>
  ///   to create a visual representation of a node for another part of the UI.
  /// </remarks>
  internal sealed class NodeDragInputMode : InputModeBase
  {
    private readonly GraphControl graphControl;
    private readonly FrameworkElement adornerHost;
    private readonly DragEventHandler adornerHostDragOverHandler;
    private readonly List<DropTarget> dropTargets = [];
    private INode? dragCandidate;
    private INode? draggedNode;
    private Point dragStart;
    private NodePreviewAdorner? dragAdorner;
    private AdornerLayer? dragAdornerLayer;
    private DispatcherOperation? dragOperation;
    private bool? oldAutoScrollOnBounds;
    private bool oldAdornerHostAllowDrop;

    /// <summary>
    ///   Creates an input mode that drags nodes from the given graph control to WPF drop targets.
    /// </summary>
    /// <param name="graphControl">The graph control that supplies the draggable nodes.</param>
    /// <param name="adornerHost">The element that hosts the drag preview adorner.</param>
    public NodeDragInputMode(GraphControl graphControl, FrameworkElement adornerHost) {
      this.graphControl = graphControl;
      this.adornerHost = adornerHost;
      adornerHostDragOverHandler = OnAdornerHostDragOver;
    }

    /// <summary>
    ///   Registers a WPF element that accepts nodes dragged from the graph control.
    /// </summary>
    /// <param name="target">The WPF element that accepts dropped nodes.</param>
    /// <param name="dropped">The callback invoked when a node is dropped on the target.</param>
    public void AddDropTarget(FrameworkElement target, Action<INode> dropped) {
      var dropTarget = new DropTarget(target, dropped, this);
      dropTargets.Add(dropTarget);
      if (Installed) {
        InstallDropTarget(dropTarget);
      }
    }

    /// <summary>
    ///   Installs the input mode and registers pointer and WPF drop handlers.
    /// </summary>
    public override void Install(IInputModeContext context, ConcurrencyController controller) {
      base.Install(context, controller);
      graphControl.PointerDown += OnMouseDown;
      graphControl.PointerDrag += OnMouseDrag;
      graphControl.PointerUp += OnMouseUp;
      oldAdornerHostAllowDrop = adornerHost.AllowDrop;
      adornerHost.AllowDrop = true;
      adornerHost.AddHandler(DragDrop.DragOverEvent, adornerHostDragOverHandler, true);
      foreach (var dropTarget in dropTargets) {
        InstallDropTarget(dropTarget);
      }
    }

    /// <summary>
    ///   Uninstalls the input mode and removes its pointer and WPF drop handlers.
    /// </summary>
    public override void Uninstall(IInputModeContext context) {
      graphControl.PointerDown -= OnMouseDown;
      graphControl.PointerDrag -= OnMouseDrag;
      graphControl.PointerUp -= OnMouseUp;
      adornerHost.RemoveHandler(DragDrop.DragOverEvent, adornerHostDragOverHandler);
      adornerHost.AllowDrop = oldAdornerHostAllowDrop;
      foreach (var dropTarget in dropTargets) {
        UninstallDropTarget(dropTarget);
      }
      CleanupDrag();
      base.Uninstall(context);
    }

    /// <summary>
    ///   Cleans up a pending drag when <see cref="InputModeBase.Cancel"/> deactivates this mode.
    /// </summary>
    public override void Cancel() {
      CleanupDrag();
      base.Cancel();
    }

    /// <summary>
    ///   Arms the mode when an unmodified left mouse press hits a node.
    /// </summary>
    private void OnMouseDown(object? sender, PointerEventArgs e) {
      dragCandidate = null;
      if (e.Modifiers != ModifierKeys.None) {
        // To avoid clashes with the CreateEdgeInputMode:
        // Don't arm this mode if shift is held down
        return;
      }

      if (ParentInputModeContext?.Lookup(typeof(IHitTester)) is IHitTester nodeHitTester) {
        var node = nodeHitTester.EnumerateHits(ParentInputModeContext!, e.Location, GraphItemTypes.Node).OfType<INode>()
            .FirstOrDefault();
        if (node != null) {
          dragCandidate = node;
          var viewLocation = graphControl.WorldToViewCoordinates(e.Location);
          dragStart = new Point(viewLocation.X, viewLocation.Y);
        }
      }
    }

    /// <summary>
    ///   Requests the input mutex and starts a native drag after the system drag threshold is crossed.
    /// </summary>
    private void OnMouseDrag(object? sender, PointerEventArgs e) {
      if (draggedNode != null || dragCandidate is not { } node || e.PointerType != PointerType.Mouse ||
          !e.Buttons.HasFlag(PointerButtons.MouseLeft)) {
        return;
      }

      var viewLocation = graphControl.WorldToViewCoordinates(e.Location);
      if (Math.Abs(viewLocation.X - dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
          Math.Abs(viewLocation.Y - dragStart.Y) < SystemParameters.MinimumVerticalDragDistance) {
        return;
      }

      dragCandidate = null;
      if (!graphControl.Graph.Contains(node) || !CanRequestMutex()) {
        return;
      }

      var layer = AdornerLayer.GetAdornerLayer(adornerHost) ??
          throw new InvalidOperationException("The adorner host must be hosted in an AdornerDecorator.");
      var adorner = new NodePreviewAdorner(adornerHost, CreateNodePreview(node)) {
          Position = GetAdornerPosition(e.Location)
      };

      RequestMutex();
      try {
        draggedNode = node;
        dragAdornerLayer = layer;
        dragAdorner = adorner;
        layer.Add(adorner);

        oldAutoScrollOnBounds = graphControl.AutoScrollOnBounds;
        graphControl.AutoScrollOnBounds = false;
        // Let WPF render the adorner before entering the modal OLE drag loop.
        dragOperation = adornerHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(PerformNodeDrag));
      } catch {
        CleanupDrag();
        throw;
      }
    }

    /// <summary>
    ///   Clears a drag candidate when the mouse is released before a drag starts.
    /// </summary>
    private void OnMouseUp(object? sender, PointerEventArgs e) {
      if (draggedNode == null) {
        dragCandidate = null;
      }
    }

    /// <summary>
    ///   Creates a visual which represents the dragged node.
    ///   The exported visual can be used as content in another WPF control.
    /// </summary>
    /// <param name="node">The node to render.</param>
    /// <returns>A WPF visual that represents the node.</returns>
    public static UIElement CreateNodePreview(INode node) {
      // a temporary GraphControl is used to render a copy of the node independently of the live graph
      var exportControl = new GraphControl();
      var exportGraph = exportControl.Graph;

      // we create a node in this GraphControl that should be exported
      CopyNode(node, exportGraph);
      exportControl.UpdateContentBounds(new InsetsD(5));

      // export the rendered node as a WPF visual that can be displayed in another control
      var exporter = new UIElementExporter(exportControl.ContentBounds);
      return exporter.Export(exportControl);
    }

    private static void CopyNode(INode node, IGraph targetGraph) {
      var newNode = targetGraph.CreateNode(node.Layout.ToRectD(), node.Style, node.Tag);
      foreach (var label in node.Labels) {
        targetGraph.AddLabel(newNode, label.Text, label.LayoutParameter, label.Style, label.PreferredSize, label.Tag);
      }

      foreach (var port in node.Ports) {
        var newPort = targetGraph.AddPort(newNode, port.LocationParameter, port.Style, port.Tag);
        foreach (var label in port.Labels) {
          targetGraph.AddLabel(newPort, label.Text, label.LayoutParameter, label.Style, label.PreferredSize, label.Tag);
        }
      }
    }

    /// <summary>
    ///   Performs the modal WPF drag and drop operation for the armed node.
    /// </summary>
    private void PerformNodeDrag() {
      dragOperation = null;
      if (draggedNode is not { } node || !graphControl.Graph.Contains(node) ||
          !Mouse.LeftButton.Equals(MouseButtonState.Pressed)) {
        CleanupDrag();
        return;
      }

      try {
        var data = new DataObject(typeof(INode), node);
        DragDrop.DoDragDrop(graphControl, data, DragDropEffects.Move);
      } finally {
        CleanupDrag();
      }
    }

    /// <summary>
    ///   Keeps the preview adorner centered on the current pointer location.
    /// </summary>
    private void OnAdornerHostDragOver(object sender, DragEventArgs e) {
      if (dragAdorner != null) {
        dragAdorner.Position = e.GetPosition(adornerHost);
        dragAdornerLayer?.Update(adornerHost);
      }
    }

    /// <summary>
    ///   Marks the target as accepting the drag only while its node still belongs to the graph.
    /// </summary>
    private void OnDropTargetDragOver(object? sender, DragEventArgs e) {
      e.Effects = GetDraggedNode(e) is { } node && graphControl.Graph.Contains(node)
          ? DragDropEffects.Move
          : DragDropEffects.None;
      e.Handled = true;
    }

    /// <summary>
    ///   Invokes the registered callback for a valid node drop.
    /// </summary>
    private void OnDropTargetDrop(DropTarget dropTarget, DragEventArgs e) {
      if (GetDraggedNode(e) is { } node && graphControl.Graph.Contains(node)) {
        dropTarget.Dropped(node);
        e.Effects = DragDropEffects.Move;
      } else {
        e.Effects = DragDropEffects.None;
      }
      e.Handled = true;
    }

    /// <summary>
    ///   Converts a graph location to the coordinate space of the adorner host.
    /// </summary>
    private Point GetAdornerPosition(PointD worldLocation) {
      var viewLocation = graphControl.WorldToViewCoordinates(worldLocation);
      return graphControl.TranslatePoint(new Point(viewLocation.X, viewLocation.Y), adornerHost);
    }

    private static INode? GetDraggedNode(DragEventArgs e) =>
        e.Data.GetDataPresent(typeof(INode)) ? e.Data.GetData(typeof(INode)) as INode : null;

    /// <summary>
    ///   Enables WPF drag and drop on a registered target and hooks its event handlers.
    /// </summary>
    private void InstallDropTarget(DropTarget dropTarget) {
      dropTarget.OldAllowDrop = dropTarget.Target.AllowDrop;
      dropTarget.Target.AllowDrop = true;
      dropTarget.Target.AddHandler(DragDrop.DragOverEvent, dropTarget.DragOverHandler, true);
      dropTarget.Target.AddHandler(DragDrop.DropEvent, dropTarget.DropHandler, true);
    }

    /// <summary>
    ///   Removes the event handlers from a registered target and restores its previous drop setting.
    /// </summary>
    private static void UninstallDropTarget(DropTarget dropTarget) {
      dropTarget.Target.RemoveHandler(DragDrop.DragOverEvent, dropTarget.DragOverHandler);
      dropTarget.Target.RemoveHandler(DragDrop.DropEvent, dropTarget.DropHandler);
      dropTarget.Target.AllowDrop = dropTarget.OldAllowDrop;
    }

    /// <summary>
    ///   Removes the preview adorner and restores graph control state after a drag ends or is canceled.
    /// </summary>
    private void CleanupDrag() {
      dragOperation?.Abort();
      dragOperation = null;
      if (dragAdorner != null) {
        dragAdornerLayer?.Remove(dragAdorner);
        dragAdorner = null;
      }
      dragAdornerLayer = null;
      if (oldAutoScrollOnBounds is { } autoScrollOnBounds) {
        graphControl.AutoScrollOnBounds = autoScrollOnBounds;
        oldAutoScrollOnBounds = null;
      }
      dragCandidate = null;
      draggedNode = null;
      if (HasMutex()) {
        ReleaseMutex();
      }
    }

    /// <summary>
    ///   Stores a registered WPF drop target and the handlers and settings needed to install or remove it.
    /// </summary>
    private sealed class DropTarget
    {
      /// <summary>
      ///   Creates a drop target and prepares its event handlers.
      /// </summary>
      /// <param name="target">The WPF element that accepts dropped nodes.</param>
      /// <param name="dropped">The callback invoked when a node is dropped on the target.</param>
      /// <param name="owner">The input mode that handles drag-over and drop events.</param>
      public DropTarget(FrameworkElement target, Action<INode> dropped, NodeDragInputMode owner) {
        Target = target;
        Dropped = dropped;
        DragOverHandler = owner.OnDropTargetDragOver;
        DropHandler = (sender, e) => owner.OnDropTargetDrop(this, e);
      }

      /// <summary>
      ///   Gets the WPF element registered as a drop target.
      /// </summary>
      public FrameworkElement Target { get; }

      /// <summary>
      ///   Gets the callback invoked when a node is dropped on the target.
      /// </summary>
      public Action<INode> Dropped { get; }

      /// <summary>
      ///   Gets or sets the value of <see cref="FrameworkElement.AllowDrop"/> before this target was installed.
      /// </summary>
      public bool OldAllowDrop { get; set; }

      /// <summary>
      ///   Gets the handler that marks valid node drags as accepted by the target.
      /// </summary>
      public DragEventHandler DragOverHandler { get; }

      /// <summary>
      ///   Gets the handler that invokes the drop callback for a valid node drag.
      /// </summary>
      public DragEventHandler DropHandler { get; }
    }

    /// <summary>
    ///   Displays a node preview centered on a position within an adorned element.
    /// </summary>
    private sealed class NodePreviewAdorner : Adorner
    {
      /// <summary>
      ///   Contains the preview presenter, which is the adorner's visual child.
      /// </summary>
      private readonly VisualCollection visualChildren;

      /// <summary>
      ///   Hosts and measures the node preview visual.
      /// </summary>
      private readonly ContentPresenter previewPresenter;

      /// <summary>
      ///   Moves the preview to keep it centered on <see cref="position"/>.
      /// </summary>
      private readonly TranslateTransform previewTranslation = new();

      /// <summary>
      ///   Stores the measured size of the preview.
      /// </summary>
      private Size previewSize;

      /// <summary>
      ///   Stores the preview center in the coordinate space of the adorned element.
      /// </summary>
      private Point position;

      /// <summary>
      ///   Creates an adorner that presents the given visual without intercepting pointer input.
      /// </summary>
      /// <param name="adornedElement">The element over which the preview is displayed.</param>
      /// <param name="preview">The visual to display as the preview.</param>
      public NodePreviewAdorner(UIElement adornedElement, UIElement preview) : base(adornedElement) {
        previewPresenter = new ContentPresenter {
            Content = preview,
            RenderTransform = previewTranslation,
            IsHitTestVisible = false
        };
        visualChildren = new VisualCollection(this);
        visualChildren.Add(previewPresenter);
        IsHitTestVisible = false;
      }

      /// <summary>
      ///   Gets or sets the center position of the preview in the adorned element's coordinates.
      /// </summary>
      public Point Position {
        get => position;
        set {
          position = value;
          UpdatePreviewTranslation();
        }
      }

      /// <summary>
      ///   Gets the number of visual children hosted by this adorner.
      /// </summary>
      protected override int VisualChildrenCount => visualChildren.Count;

      /// <summary>
      ///   Gets the visual child at the specified index.
      /// </summary>
      /// <param name="index">The index of the visual child.</param>
      /// <returns>The visual child at <paramref name="index"/>.</returns>
      protected override Visual GetVisualChild(int index) => visualChildren[index];

      /// <summary>
      ///   Measures the preview within a fixed size and uses the adorned element's render size for the adorner.
      /// </summary>
      /// <param name="constraint">The available size for the adorner.</param>
      /// <returns>The render size of the adorned element.</returns>
      protected override Size MeasureOverride(Size constraint) {
        previewPresenter.Measure(new Size(190, 130));
        previewSize = previewPresenter.DesiredSize;
        UpdatePreviewTranslation();
        return AdornedElement.RenderSize;
      }

      /// <summary>
      ///   Arranges the measured preview at its origin before its render transform is applied.
      /// </summary>
      /// <param name="finalSize">The final size allocated to the adorner.</param>
      /// <returns>The final size of the adorner.</returns>
      protected override Size ArrangeOverride(Size finalSize) {
        if (previewSize.Width > 0 && previewSize.Height > 0) {
          previewPresenter.Arrange(new Rect(previewSize));
        }
        return finalSize;
      }

      /// <summary>
      ///   Updates the translation so the preview is centered on the current position.
      /// </summary>
      private void UpdatePreviewTranslation() {
        previewTranslation.X = position.X - previewSize.Width / 2;
        previewTranslation.Y = position.Y - previewSize.Height / 2;
      }
    }
  }
}
