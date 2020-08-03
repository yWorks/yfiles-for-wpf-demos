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

using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// Helper class to support clipboard operations for rotatable nodes.
  /// </summary>
  class RotatableNodeClipboardHelper : IClipboardHelper
  {
    /// <summary>
    /// Returns whether or not to copying the given item is possible.
    /// </summary>
    public bool ShouldCopy(IGraphClipboardContext context, IModelItem item) {
      return true;
    }

    /// <summary>
    /// Returns whether or not to cutting the given item is possible.
    /// </summary>
    public bool ShouldCut(IGraphClipboardContext context, IModelItem item) {
      return true;
    }

    /// <summary>
    /// Returns whether or not to pasting of the given item is possible.
    /// </summary>
    public bool ShouldPaste(IGraphClipboardContext context, IModelItem item, object userData) {
      return true;
    }

    /// <summary>
    /// Adds no additional state to the copy-operation.
    /// </summary>
    public object Copy(IGraphClipboardContext context, IModelItem item) {
      return null;
    }

    /// <summary>
    /// Adds no additional state to the cut-operation.
    /// </summary>
    public object Cut(IGraphClipboardContext context, IModelItem item) {
      return null;
    }

    /// <summary>
    /// Copies the node style for the paste-operation because <see cref="RotatableNodeStyleDecorator"/> should not be shared.
    /// </summary>
    public void Paste(IGraphClipboardContext context, IModelItem item, object userData) {
      var node = item as INode;
      if (node == null) {
        return;
      }
      var styleWrapper = node.Style as RotatableNodeStyleDecorator;
      if (styleWrapper != null) {
        if (context.TargetGraph.GetFoldingView() != null) {
          context.TargetGraph.GetFoldingView().Manager.MasterGraph.SetStyle(node, styleWrapper.Clone() as INodeStyle);
        } else {
          context.TargetGraph.SetStyle(node, styleWrapper.Clone() as INodeStyle);
        }
      }
    }
  }
}
