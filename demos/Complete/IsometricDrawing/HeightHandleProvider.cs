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

using System.Collections.Generic;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.Complete.IsometricDrawing
{
  /// <summary>
  /// An <see cref="IHandleProvider"/> implementation that adds a <see cref="HeightHandle"/> to a node's available handles.
  /// </summary>
  public class HeightHandleProvider : IHandleProvider {

    private readonly INode node;
    private readonly IHandleProvider delegateHandler;
    private readonly double minimumHeight;

    public HeightHandleProvider(INode node, IHandleProvider delegateHandler, double minimumHeight) {
      this.node = node;
      this.delegateHandler = delegateHandler;
      this.minimumHeight = minimumHeight;
    }

    public IEnumerable<IHandle> GetHandles(IInputModeContext context) {
      var result = new List<IHandle>();
      if (this.delegateHandler != null) {
        result.AddRange(this.delegateHandler.GetHandles(context));
      }
      result.Add(new HeightHandle(this.node, context, this.minimumHeight));
      return result;
    }
  }
}
