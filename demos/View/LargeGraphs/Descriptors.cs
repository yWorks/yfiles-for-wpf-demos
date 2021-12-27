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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.Graph.LargeGraphs
{
  /// <summary>
  ///   <see cref="NodeStyleDescriptor" /> that delegates its <see cref="IsDirty" /> method to its corresponding canvas
  ///   object.
  /// </summary>
  public class MyNodeStyleDescriptor : ICanvasObjectDescriptor
  {
    /// <summary>
    ///   Determines whether the given canvas object is deemed dirty and needs
    ///   updating.
    /// </summary>
    /// <param name="context">The context that will be used for the update.</param>
    /// <param name="canvasObject">The object to check.</param>
    /// <returns>Always returns <c>canvasObject.Dirty</c>.</returns>
    /// <remarks>
    ///   Since this method always returns <c>canvasObject.Dirty</c>, the return value will in most cases be
    ///   <see langword="false" />. This skips visibility testing and <c>UpdateVisual</c> completely. Especially visibility
    ///   testing can be expensive, e.g. with edge labels.
    /// </remarks>
    public bool IsDirty(ICanvasContext context, ICanvasObject canvasObject) {
      return canvasObject.Dirty;
    }

    public IVisualCreator GetVisualCreator(object forUserObject) {
      var node = (INode) forUserObject;
      return node.Style.Renderer.GetVisualCreator(node, node.Style);
    }

    public IBoundsProvider GetBoundsProvider(object forUserObject) {
      var node = (INode) forUserObject;
      return node.Style.Renderer.GetBoundsProvider(node, node.Style);
    }

    public IVisibilityTestable GetVisibilityTestable(object forUserObject) {
      var node = (INode) forUserObject;
      return node.Style.Renderer.GetVisibilityTestable(node, node.Style);
    }

    public IHitTestable GetHitTestable(object forUserObject) {
      var node = (INode) forUserObject;
      return node.Style.Renderer.GetHitTestable(node, node.Style);
    }
  }

  /// <summary>
  ///   <see cref="EdgeStyleDescriptor" /> that delegates its <see cref="IsDirty" /> method to its corresponding canvas
  ///   object.
  /// </summary>
  public class MyEdgeStyleDescriptor : ICanvasObjectDescriptor
  {
    /// <summary>
    ///   Determines whether the given canvas object is deemed dirty and needs
    ///   updating.
    /// </summary>
    /// <param name="context">The context that will be used for the update.</param>
    /// <param name="canvasObject">The object to check.</param>
    /// <returns>Always returns <c>canvasObject.Dirty</c>.</returns>
    /// <remarks>
    ///   Since this method always returns <c>canvasObject.Dirty</c>, the return value will in most cases be
    ///   <see langword="false" />. This skips visibility testing and <c>UpdateVisual</c> completely. Especially visibility
    ///   testing can be expensive, e.g. with edge labels.
    /// </remarks>
    public bool IsDirty(ICanvasContext context, ICanvasObject canvasObject) {
      return canvasObject.Dirty;
    }

    public IVisualCreator GetVisualCreator(object forUserObject) {
      var edge = (IEdge) forUserObject;
      return edge.Style.Renderer.GetVisualCreator(edge, edge.Style);
    }

    public IBoundsProvider GetBoundsProvider(object forUserObject) {
      var edge = (IEdge) forUserObject;
      return edge.Style.Renderer.GetBoundsProvider(edge, edge.Style);
    }

    public IVisibilityTestable GetVisibilityTestable(object forUserObject) {
      var edge = (IEdge) forUserObject;
      return edge.Style.Renderer.GetVisibilityTestable(edge, edge.Style);
    }

    public IHitTestable GetHitTestable(object forUserObject) {
      var edge = (IEdge) forUserObject;
      return edge.Style.Renderer.GetHitTestable(edge, edge.Style);
    }
  }

  /// <summary>
  ///   <see cref="LabelStyleDescriptor" /> that delegates its <see cref="IsDirty" /> method to its corresponding canvas
  ///   object.
  /// </summary>
  public class MyLabelStyleDescriptor : ICanvasObjectDescriptor
  {
    /// <summary>
    ///   Determines whether the given canvas object is deemed dirty and needs
    ///   updating.
    /// </summary>
    /// <param name="context">The context that will be used for the update.</param>
    /// <param name="canvasObject">The object to check.</param>
    /// <returns>Always returns <c>canvasObject.Dirty</c>.</returns>
    /// <remarks>
    ///   Since this method always returns <c>canvasObject.Dirty</c>, the return value will in most cases be
    ///   <see langword="false" />. This skips visibility testing and <c>UpdateVisual</c> completely. Especially visibility
    ///   testing can be expensive, e.g. with edge labels.
    /// </remarks>
    public bool IsDirty(ICanvasContext context, ICanvasObject canvasObject) {
      return canvasObject.Dirty;
    }

    public IVisualCreator GetVisualCreator(object forUserObject) {
      var label = (ILabel) forUserObject;
      return label.Style.Renderer.GetVisualCreator(label, label.Style);
    }

    public IBoundsProvider GetBoundsProvider(object forUserObject) {
      var label = (ILabel) forUserObject;
      return label.Style.Renderer.GetBoundsProvider(label, label.Style);
    }

    public IVisibilityTestable GetVisibilityTestable(object forUserObject) {
      var label = (ILabel) forUserObject;
      return label.Style.Renderer.GetVisibilityTestable(label, label.Style);
    }

    public IHitTestable GetHitTestable(object forUserObject) {
      var label = (ILabel) forUserObject;
      return label.Style.Renderer.GetHitTestable(label, label.Style);
    }
  }
}