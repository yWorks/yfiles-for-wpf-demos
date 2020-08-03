/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Graph;

namespace Demo.yFiles.Graph.ZOrder {

  /// <summary>
  /// Event argument class used by <see cref="ZOrderSupport.ZOrderChanged"/>. 
  /// </summary>
  public class ZOrderChangedEventArgs : EventArgs {
    /// <summary>
    /// Creates a new event argument for the given node.
    /// </summary>
    /// <param name="node">The node whose z-order has changed.</param>
    /// <param name="newZOrder">The new z-order of the item.</param>
    /// <param name="oldZOrder">The old z-order of the item.</param>
    public ZOrderChangedEventArgs(INode node, int newZOrder, int oldZOrder = 0) {
      this.Node = node;
      this.NewZOrder = newZOrder;
      this.OldZOrder = oldZOrder;
    }

    /// <summary>
    /// Gets the node whose z-order has changed.
    /// </summary>
    public INode Node { get; }

    /// <summary>
    /// The new z-order of <see cref="Node"/>.
    /// </summary>
    public int NewZOrder { get; }

    /// <summary>
    /// The old z-order of <see cref="Node"/>.
    /// </summary>
    public int OldZOrder { get; }
  }
}