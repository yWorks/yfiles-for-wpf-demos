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

using System.Collections.Generic;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.RelocateSubtree
{
  /// <summary>
  /// An <see cref="IPositionHandler"/> that moves a node and its subtree.
  /// </summary>
  internal class SubtreePositionHandler : IPositionHandler {

    /// <summary>
    /// Compound handler for all elements of the subgraph.
    /// </summary>
    private IPositionHandler compositeHandler;

    /// <summary>
    /// The node we are currently moving.
    /// </summary>
    private readonly INode node;

    /// <summary>
    /// Performs layout and animation while relocating the subtree.
    /// </summary>
    private RelocateSubtreeLayoutHelper layoutHelper;

    /// <summary>
    /// The original <see cref="IPositionHandler"/>.
    /// </summary>
    private readonly IPositionHandler nodePositionHandler;

    /// <summary>
    /// The nodes and edges of the subgraph the is dragged.
    /// </summary>
    private Subtree subtree;

    public SubtreePositionHandler(INode node, IPositionHandler original) {
      this.node = node;
      nodePositionHandler = original;
    }

    /// <summary>
    /// Returns a view of the location of the item.
    /// </summary>
    public IPoint Location {
      get { return nodePositionHandler.Location; }
    }
    
    #region Drag handling

    /// <summary>
    /// The subtree is upon to be dragged.
    /// </summary>
    public void InitializeDrag(IInputModeContext context) {
      subtree = new Subtree(context.GetGraph(), node);
      layoutHelper = new RelocateSubtreeLayoutHelper((GraphControl)context.CanvasControl, subtree);
      layoutHelper.InitializeLayout();

      compositeHandler = CreateCompositeHandler(subtree);
      compositeHandler.InitializeDrag(context);
    }

    /// <summary>
    /// The subtree is dragged.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      compositeHandler.HandleMove(context, originalLocation, newLocation);
      layoutHelper.RunLayout();
    }

    /// <summary>
    /// The drag is canceled.
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      compositeHandler.CancelDrag(context, originalLocation);
      layoutHelper.CancelLayout();
    }

    /// <summary>
    /// The drag is finished.
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      compositeHandler.DragFinished(context, originalLocation, newLocation);
      layoutHelper.StopLayout();
    }

    #endregion

    #region Position Handler for Subtree 

    /// <summary>
    /// Creates an <see cref="IPositionHandler"/> that moves the whole subtree.
    /// </summary>
    /// <param name="subtree">The nodes and edges of the subtree.</param>
    /// <returns></returns>
    private static IPositionHandler CreateCompositeHandler(Subtree subtree) {
      var positionHandlers = new List<IPositionHandler>();
      foreach (var node in subtree.Nodes) {
        var positionHandler = node.Lookup<IPositionHandler>();
        if (positionHandler != null) {
          var subtreeHandler = positionHandler as SubtreePositionHandler;
          positionHandlers.Add(subtreeHandler != null ? subtreeHandler.nodePositionHandler : positionHandler);
        }
      }
      foreach (var edge in subtree.Edges) {
        var positionHandler = edge.Lookup<IPositionHandler>();
        if (positionHandler != null) {
          positionHandlers.Add(positionHandler);
        }
      }
      return PositionHandlers.Combine(positionHandlers);
    }

    #endregion

  }
}
