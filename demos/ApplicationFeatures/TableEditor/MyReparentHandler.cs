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

using yWorks.Controls.Input;
using yWorks.Graph;

namespace yWorks.Demo.TableEditor
{
  /// <summary>
  ///   Custom <see cref="NodeDropInputMode"/> that disallows to reparent a table node.
  /// </summary>
  internal sealed class MyReparentHandler : IReparentNodeHandler
  {
    private readonly IReparentNodeHandler coreHandler;

    /// <summary>
    ///   Creates a new instance of MyReparentHandler.
    /// </summary>
    public MyReparentHandler(IReparentNodeHandler coreHandler) {
      this.coreHandler = coreHandler;
    }

    public bool IsReparentGesture(IInputModeContext context, INode node) =>
        coreHandler.IsReparentGesture(context, node);

    public bool ShouldReparent(IInputModeContext context, INode node) {
      // Ok, this node has a table associated - disallow dragging into a group node.
      if (node.Lookup(typeof(ITable)) is ITable) {
        return false;
      }
      return coreHandler.ShouldReparent(context, node);
    }

    public bool IsValidParent(IInputModeContext context, INode node, INode? newParent) =>
        coreHandler.IsValidParent(context, node, newParent);

    public void Reparent(IInputModeContext context, INode node, INode? newParent) =>
        coreHandler.Reparent(context, node, newParent);
  }
}
