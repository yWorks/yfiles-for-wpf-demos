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

using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ClipboardDeferredCut
{
  /// <summary>
  ///   A special node style wrapper which renders the wrapped style transparent
  ///   if the node is marked as "to be cut" in the <see cref="DeferredCutClipboard"/>.
  /// </summary>
  public sealed class ClipboardNodeStyle : NodeStyleBase<VisualGroup>
  {
    private readonly INodeStyle wrapped;

    /// <summary>
    ///   Wrap the given node style.
    /// </summary>
    /// <param name="wrapped">The node style to wrap.</param>
    public ClipboardNodeStyle(INodeStyle wrapped) {
      this.wrapped = wrapped;
    }

    /// <summary>
    ///   Create the visual which visualizes the given node:
    ///   create the visual of the wrapped style and make it transparent
    ///   if the node is marked as "to be cut".
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="node">The node.</param>
    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      var visual = wrapped.Renderer.GetVisualCreator(node, wrapped).CreateVisual(context);
      var group = new VisualGroup();
      group.Add(visual);
      OpacityHelper.SetOpacity(context, group, node);
      return group;
    }

    /// <summary>
    ///   Update the visual which visualizes the given node:
    ///   update the visual of the wrapped style and make it transparent
    ///   if the node is marked as "to be cut".
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">The visual created by <see cref="CreateVisual"/>.</param>
    /// <param name="node">The node.</param>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      if (oldVisual.Children.Count != 1) {
        return CreateVisual(context, node);
      }

      var visual = wrapped.Renderer.GetVisualCreator(node, wrapped).UpdateVisual(context, oldVisual.Children[0]);
      if (!ReferenceEquals(visual, oldVisual.Children[0])) {
        oldVisual.Children[0] = visual;
      }

      OpacityHelper.SetOpacity(context, oldVisual, node);
      return oldVisual;
    }
  }

  /// <summary>
  ///   A special edge style wrapper which renders the wrapped style transparent
  ///   if the edge is marked as "to be cut" in the <see cref="DeferredCutClipboard"/>.
  /// </summary>
  public sealed class ClipboardEdgeStyle : EdgeStyleBase<VisualGroup>
  {
    private readonly IEdgeStyle wrapped;

    /// <summary>
    ///   Wraps the given style.
    /// </summary>
    /// <param name="wrapped">The edge style to wrap.</param>
    public ClipboardEdgeStyle(IEdgeStyle wrapped) {
      this.wrapped = wrapped;
    }

    /// <summary>
    ///   Create the visual which visualizes the given edge:
    ///   create the visual of the wrapped style and make it transparent
    ///   if the edge is marked as "to be cut".
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="edge">The edge.</param>
    protected override VisualGroup CreateVisual(IRenderContext context, IEdge edge) {
      var visual = wrapped.Renderer.GetVisualCreator(edge, wrapped).CreateVisual(context);
      var group = new VisualGroup();
      group.Add(visual);
      OpacityHelper.SetOpacity(context, group, edge);
      return group;
    }

    /// <summary>
    ///   Update the visual which visualizes the given edge:
    ///   update the visual of the wrapped style and make it transparent
    ///   if the edge is marked as "to be cut".
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">The visual created by <see cref="CreateVisual"/>.</param>
    /// <param name="edge">The edge.</param>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IEdge edge) {
      if (oldVisual.Children.Count != 1) {
        return CreateVisual(context, edge);
      }

      var visual = wrapped.Renderer.GetVisualCreator(edge, wrapped).UpdateVisual(context, oldVisual.Children[0]);
      if (!ReferenceEquals(visual, oldVisual.Children[0])) {
        oldVisual.Children[0] = visual;
      }

      OpacityHelper.SetOpacity(context, oldVisual, edge);
      return oldVisual;
    }
  }

  /// <summary>
  ///   A special label style wrapper which renders the wrapped style transparent
  ///   if the label is marked as "to be cut" in the <see cref="DeferredCutClipboard"/>.
  /// </summary>
  public sealed class ClipboardLabelStyle : LabelStyleBase<VisualGroup>
  {
    private readonly ILabelStyle wrapped;

    /// <summary>
    ///   Wraps the given style.
    /// </summary>
    /// <param name="wrapped">The label style to wrap.</param>
    public ClipboardLabelStyle(ILabelStyle wrapped) {
      this.wrapped = wrapped;
    }

    /// <summary>
    ///   Creates the visual which visualizes the given label:
    ///   creates the visual of the wrapped style and make it transparent
    ///   if the label is marked as "to be cut".
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="label">The label.</param>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      var visual = wrapped.Renderer.GetVisualCreator(label, wrapped).CreateVisual(context);
      var group = new VisualGroup();
      group.Add(visual);
      OpacityHelper.SetOpacity(context, group, label);
      return group;
    }

    /// <summary>
    ///   Updates the visual which visualizes the given label:
    ///   updates the visual of the wrapped style and make it transparent
    ///   if the label is marked as "to be cut".
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">The visual created by <see cref="CreateVisual"/>.</param>
    /// <param name="label">The label.</param>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      if (oldVisual.Children.Count != 1) {
        return CreateVisual(context, label);
      }

      var visual = wrapped.Renderer.GetVisualCreator(label, wrapped).UpdateVisual(context, oldVisual.Children[0]);
      if (!ReferenceEquals(visual, oldVisual.Children[0])) {
        oldVisual.Children[0] = visual;
      }

      OpacityHelper.SetOpacity(context, oldVisual, label);
      return oldVisual;
    }

    /// <summary>
    ///   Delegates to the wrapped style.
    /// </summary>
    protected override SizeD GetPreferredSize(ILabel label) => wrapped.Renderer.GetPreferredSize(label, wrapped);
  }

  public static class ClipboardStyles
  {
    /// <summary>
    ///   Create some default styles and wrap them into
    ///   a style wrapper which indicates whether the item is cut.
    /// </summary>
    /// <param name="graph">The graph.</param>
    public static void SetClipboardStyles(IGraph graph) {
      graph.NodeDefaults.Style = new ClipboardNodeStyle(graph.NodeDefaults.Style);
      graph.EdgeDefaults.Labels.Style = new ClipboardLabelStyle(graph.EdgeDefaults.Labels.Style);
      graph.EdgeDefaults.Style = new ClipboardEdgeStyle(graph.EdgeDefaults.Style);
      graph.NodeDefaults.Labels.Style = new ClipboardLabelStyle(graph.NodeDefaults.Labels.Style);
    }
  }

  public static class OpacityHelper
  {
    /// <summary>
    ///   Sets the opacity on the visual group of an item marked as "to be cut".
    /// </summary>
    /// <param name="context">The render context providing information about the current rendering process.</param>
    /// <param name="group">The visual group whose opacity is set.</param>
    /// <param name="item">The item represented by the visual group.</param>
    public static void SetOpacity(IRenderContext context, VisualGroup group, IModelItem item) {
      if (context.CanvasControl is GraphControl { Clipboard: DeferredCutClipboard deferredCutClipboard }) {
        group.Opacity = deferredCutClipboard.IsToBeCut(item) ? 0.3 : 1;
      }
    }
  }
}
