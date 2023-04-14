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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.ZOrder
{
  /// <summary>
  /// An <see cref="IPositionHandler"/> for nodes that tries to keep the z-order of its moved node relative to
  /// other moved nodes.
  /// </summary>
  /// <remarks>
  /// As the z-order doesn't change for normal move gestures, this class customizes only interactive reparent gestures.
  /// </remarks>
  public class ZOrderNodePositionHandler : GroupingNodePositionHandler
  {
    private readonly INode node;
    private INode initialParent;
    private INode currentParent;
    private ZOrderSupport zOrderSupport;
    private bool dragging = false;

    public ZOrderNodePositionHandler(INode node, IPositionHandler wrappedHandler = null) : base(node, wrappedHandler) {
      this.node = node;
    }

    public override void InitializeDrag(IInputModeContext context) {
      var graph = context.GetGraph();
      zOrderSupport = graph.Lookup<ZOrderSupport>();
 
      // store initial parent of the node...
      this.initialParent = graph.GetParent(node);
      this.currentParent = initialParent;
      this.dragging = true;
      base.InitializeDrag(context);
    }

    public override void CancelDrag(IInputModeContext context, PointD originalLocation) {
      base.CancelDrag(context, originalLocation);
      dragging = false;
    }

    public override void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      dragging = false;
      base.DragFinished(context, originalLocation, newLocation);
    }

    /// <summary>
    /// Customizes the temporary new parent of <paramref name="node"/> and its z-order in its new <see cref="ICanvasObjectGroup"/>
    /// </summary>
    protected override void SetCurrentParent(IInputModeContext context, INode node, INode parent) {
      if (!dragging) {
        return;
      }
      if (parent != initialParent) {
        // node is temporarily at a new parent
        var zOrderSupport = context.GetGraph().Lookup<ZOrderSupport>();
        if (zOrderSupport != null) {
          // the ZOrderMoveInputMode knows all moved nodes and therefore can provide the new z-order for this node
          var tempZOrder = zOrderSupport.GetZOrderForNewParent(node, parent);
          // 'parent' is only a temporary new parent so the old z-order should be kept but a new temporary one is set
          this.zOrderSupport.SetTempZOrder(node, parent, tempZOrder);
        }
        base.SetCurrentParent(context, node, parent);
      } else if (parent != currentParent) {
        // node is reset to its initial parent: reset its temporary z-order so the original one is used again
        zOrderSupport.RemoveTempZOrder(node);
        
        base.SetCurrentParent(context, node, parent);
      }
      currentParent = parent;
    }
  }
}
