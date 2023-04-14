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

using System;
using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.Graph.ZOrder
{
  /// <summary>
  ///  Delegates reparenting to an external callback function. 
  /// </summary>
  /// <remarks>
  /// The callback function is intended to provide pre- and post-processing only.
  /// The actual reparenting operation should be performed by the reparent handler
  /// that is passed to the callback function.
  /// </remarks>
  internal class ZOrderReparentHandler : IReparentNodeHandler {
    private readonly IReparentNodeHandler handler;
    private readonly ZOrderSupport zOrderSupport;

    public ZOrderReparentHandler(IReparentNodeHandler handler, ZOrderSupport zOrderSupport) {
      this.handler = handler;
      this.zOrderSupport = zOrderSupport;
    }

    public bool IsReparentGesture(IInputModeContext context, INode node) {
      return this.handler.IsReparentGesture(context, node);
    }

    public bool IsValidParent(IInputModeContext context, INode node, INode newParent) {
      return this.handler.IsValidParent(context, node, newParent);
    }

    public void Reparent(IInputModeContext context, INode node, INode newParent) {
      // Determine the node's and the parent's representatives in the master graph.
      // This has to be done prior to the reparenting operation, because if the new parent is a
      // folder node (i.e. a closed group node), the given node will be removed from the current
      // view graph as part of the reparenting operation. In this case, the corresponding master nodes
      // cannot be determined anymore after reparenting is done.
      // Being able to determine the master nodes right before reparenting is the whole reason
      // for decorating the default reparent handler here.
      var masterNode = zOrderSupport.GetMasterNode(node);
      var masterParent = newParent != null ? zOrderSupport.GetMasterNode(newParent) : null;

      // reparent the node
      this.handler.Reparent(context, node, newParent);

      // update the node's z-order index
      var zIndex = CalculateNewZIndex(zOrderSupport.MasterGraph, masterNode, masterParent);
      zOrderSupport.SetZOrder(masterNode, zIndex);

      var viewNode = zOrderSupport.GetViewNode(masterNode);
      if (viewNode != null) {
        zOrderSupport.GraphControl.GraphModelManager.Update(viewNode);
      }
    }

    public bool ShouldReparent(IInputModeContext context, INode node) {
      return this.handler.ShouldReparent(context, node);
    }

    /// <summary>
    /// Calculates an appropriate z-order index for the given node in the given graph.
    /// </summary>
    private int CalculateNewZIndex(IGraph masterGraph, INode masterNode, INode masterParent) {
      var children = masterGraph.GetChildren(masterParent);
      if (children.Count == 1) {
        // node is the only child
        return 0;
      }
      // increment the maximum z-index of all children by 1 to add node last
      return children.Where(current => current != masterNode).Select(zOrderSupport.GetZOrder).Max() + 1;
    }
  }
}
