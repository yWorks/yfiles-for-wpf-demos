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
using yWorks.Utils;

namespace yWorks.Demo.OrthogonalEdgeEditingCustomization
{
  /// <summary>
  ///   Creates an <see cref="IEdgePortHandleProvider"/> that constrains the original
  ///   port location handle to the layout rectangle of the port's owner node.
  /// </summary>
  public sealed class PortLookupEdgePortHandleProvider(IEdge edge) : IEdgePortHandleProvider
  {
    /// <summary>
    ///   Returns a handle that is constrained to the layout rectangle of the port's owner node.
    /// </summary>
    /// <param name="context">The context in which the handle will be used.</param>
    /// <param name="sourceHandle">
    ///   <see langword="true"/> if the handle for the source side/port should be returned,
    ///   <see langword="false"/> for the target side/port.
    /// </param>
    /// <see cref="IEdgePortHandleProvider.GetHandle"/>
    public IHandle GetHandle(IInputModeContext context, bool sourceHandle) {
      var port = sourceHandle ? edge.SourcePort : edge.TargetPort;
      var handle = port.Lookup<IHandle>()!;
      return port.Owner is INode node ? new NodeLayoutPortLocationHandle(node, handle) : handle;
    }
  }
}
