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
using System.Collections.Generic;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.RectangleNodeStyle
{
  /// <summary>
  /// An <see cref="IHandleProvider"/> for nodes using a <see cref="RectangleNodeStyle"/>
  /// that provides a single <see cref="CornerSizeHandle"/>
  /// to change the <see cref="RectangleNodeStyle.CornerSize"/> of the node style interactively.
  /// </summary>
  public class CornerSizeHandleProvider : IHandleProvider
  {
    private readonly INode node;
    private readonly IHandleProvider delegateProvider;
    private readonly Action cornerSizeChanged;

    /// <summary>
    /// Creates a new instance of <see cref="CornerSizeHandleProvider"/> with the given <paramref name="cornerSizeChanged"/> action and
    /// an optional <paramref name="delegateProvider"/> whose handles are also returned.
    /// </summary>
    /// <param name="node">The node to provide handles for.</param>
    /// <param name="cornerSizeChanged">An action that is called when the handle is moved.</param>
    /// <param name="delegateProvider">The wrapped <see cref="IHandleProvider"/> implementation.</param>
    public CornerSizeHandleProvider(INode node, Action cornerSizeChanged, IHandleProvider delegateProvider = null) {
      this.node = node;
      this.delegateProvider = delegateProvider;
      this.cornerSizeChanged = cornerSizeChanged;
    }

    public IEnumerable<IHandle> GetHandles(IInputModeContext context) {
      var result = new List<IHandle>();
      if (delegateProvider != null) {
        result.AddRange(delegateProvider.GetHandles(context));
      }
      if (node.Style is yWorks.Graph.Styles.RectangleNodeStyle) {
        result.Add(new CornerSizeHandle(node, cornerSizeChanged));
      }
      return result;
    }
  }
}
