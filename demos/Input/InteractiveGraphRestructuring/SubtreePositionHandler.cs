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

using System.Collections.Generic;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.InteractiveGraphRestructuring
{
  /// <summary>
  ///   An IPositionHandler that moves a node and its subtree.
  /// </summary>
  internal sealed class SubtreePositionHandler : IPositionHandler
  {
    private readonly INode? node;
    private readonly IPositionHandler? nodePositionHandler;
    private readonly INodeStyle movingNodeStyle;
    private readonly Dictionary<INode, INodeStyle> nodeToNormalStyle = [];
    private Subtree? subtree;
    private RelocateSubtreeLayoutHelper? layoutHelper;
    private IPositionHandler? compositeHandler;

    /// <summary>
    ///   Returns the location of the selected node.
    /// </summary>
    public IPoint Location => nodePositionHandler?.Location ?? PointD.Origin;

    /// <summary>
    ///   Creates a new instance of a <see cref="SubtreePositionHandler"/>.
    /// </summary>
    /// <param name="node">The selected node</param>
    /// <param name="nodePositionHandler">The original position handler</param>
    /// <param name="movingNodeStyle">The node style that is set while the node is moving</param>
    public SubtreePositionHandler(INode? node, IPositionHandler? nodePositionHandler, INodeStyle movingNodeStyle) {
      this.node = node;
      this.nodePositionHandler = nodePositionHandler;
      this.movingNodeStyle = movingNodeStyle;
    }

    /// <summary>
    ///   The subtree is about to be dragged.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from</param>
    public void InitializeDrag(IInputModeContext context) {
      var graph = context.GetGraph()!;
      subtree = new Subtree(graph, node!);
      nodeToNormalStyle.Clear();
      foreach (var currentNode in subtree.Nodes) {
        // store normal style of the node and set the moving node style while dragging
        nodeToNormalStyle[currentNode] = currentNode.Style;
        graph.SetStyle(currentNode, movingNodeStyle);
      }

      layoutHelper = new RelocateSubtreeLayoutHelper((GraphControl) context.CanvasControl, subtree);
      layoutHelper.InitializeLayout();
      compositeHandler = CreateCompositeHandler(subtree);
      compositeHandler.InitializeDrag(context);
    }

    /// <summary>
    ///   The subtree is dragged.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from</param>
    /// <param name="originalLocation">The value of the location property at the time of InitializeDrag</param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      compositeHandler!.HandleMove(context, originalLocation, newLocation);
      layoutHelper!.RunLayout();
    }

    /// <summary>
    ///   The drag is canceled.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from</param>
    /// <param name="originalLocation">The value of the coordinate of the location property at the time of InitializeDrag</param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      compositeHandler!.CancelDrag(context, originalLocation);
      layoutHelper!.CancelLayout();
      ResetStyles(((GraphControl) context.CanvasControl).Graph);
    }

    /// <summary>
    ///   The drag is finished.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from</param>
    /// <param name="originalLocation">The value of the location property at the time of InitializeDrag</param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      compositeHandler!.DragFinished(context, originalLocation, newLocation);
      layoutHelper!.StopLayout();
      ResetStyles(context.GetGraph()!);
    }

    /// <summary>
    ///   Replaces the temporary styles used while moving nodes with the original styles.
    /// </summary>
    private void ResetStyles(IGraph graph) {
      foreach (var (currentNode, style) in nodeToNormalStyle) {
        // reset style to the normal node style of this node
        graph.SetStyle(currentNode, style);
      }
      nodeToNormalStyle.Clear();
    }

    /// <summary>
    ///   Creates an <see cref="IPositionHandler"/> that moves the whole subtree.
    /// </summary>
    /// <param name="subtree">The nodes and edges of the subtree</param>
    /// <returns>An <see cref="IPositionHandler"/> that moves the whole subtree</returns>
    private static IPositionHandler CreateCompositeHandler(Subtree subtree) {
      var positionHandlers = new List<IPositionHandler>();
      foreach (var currentNode in subtree.Nodes) {
        var positionHandler = currentNode.Lookup<IPositionHandler>();
        if (positionHandler != null) {
          positionHandlers.Add(positionHandler is SubtreePositionHandler wrapper
              ? wrapper.nodePositionHandler!
              : positionHandler);
        }
      }
      foreach (var edge in subtree.Edges) {
        positionHandlers.Add(new EdgePositionHandler(edge));
      }
      return PositionHandlers.Combine(positionHandlers);
    }
  }
}
