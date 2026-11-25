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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CustomHandleProvider
{
  /// <summary>
  ///   An <see cref="IHandleProvider"/> for nodes using an <see cref="ArrowNodeStyle"/> that provides
  ///   an <see cref="ArrowNodeStyleAngleHandle"/>, an <see cref="ArrowNodeStyleShaftRatioHandle"/>
  ///   and further handles provided by a delegating provider.
  ///   The shaft ratio handle is only provided for arrow, double-arrow, and notched-arrow shapes.
  /// </summary>
  internal sealed class ArrowNodeStyleHandleProvider : IHandleProvider
  {
    private readonly GraphControl graphControl;
    private readonly IHandleProvider? delegateProvider;
    private readonly Action stylePropertyChanged;
    private readonly INode node;

    public ArrowNodeStyleHandleProvider(
        GraphControl graphControl,
        INode node,
        Action stylePropertyChanged,
        IHandleProvider? delegateProvider = null) {
      this.graphControl = graphControl;
      this.node = node;
      this.stylePropertyChanged = stylePropertyChanged;
      this.delegateProvider = delegateProvider;
    }

    public IEnumerable<IHandle> GetHandles(IInputModeContext context) {
      var handles = delegateProvider?.GetHandles(context).ToList() ?? [];
      if (node.Style is ArrowNodeStyle style) {
        handles.Add(new ArrowNodeStyleAngleHandle(graphControl, node, stylePropertyChanged));
        if (style.Shape is ArrowStyleShape.Arrow or ArrowStyleShape.DoubleArrow or ArrowStyleShape.NotchedArrow) {
          handles.Add(new ArrowNodeStyleShaftRatioHandle(node, stylePropertyChanged));
        }
      }
      return handles;
    }
  }
}
