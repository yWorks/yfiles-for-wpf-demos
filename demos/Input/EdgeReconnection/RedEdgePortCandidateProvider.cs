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

using System.Collections.Generic;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace yWorks.Demo.EdgeReconnection
{
  /// <summary>
  ///   An <see cref="IEdgeReconnectionPortCandidateProvider"/> that only allows the current source
  ///   port and therefore disables port relocation.
  /// </summary>
  public sealed class RedEdgePortCandidateProvider : IEdgeReconnectionPortCandidateProvider
  {
    private readonly IEdge edge;

    /// <summary>
    ///   Creates a new instance of <see cref="RedEdgePortCandidateProvider"/>.
    /// </summary>
    /// <param name="edge">The given edge</param>
    public RedEdgePortCandidateProvider(IEdge edge) {
      this.edge = edge;
    }

    /// <summary>
    ///   Returns only the current port as candidate, thus effectively disabling relocation.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided</param>
    /// <returns>The edge's current source port candidate.</returns>
    /// <remarks>Specified by <see cref="IEdgeReconnectionPortCandidateProvider.GetSourcePortCandidates"/>.</remarks>
    public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) =>
        [new PortCandidate(edge.SourcePort)];

    /// <summary>
    ///   Returns no candidates, thus effectively disabling relocation.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided</param>
    /// <returns>No target port candidates.</returns>
    /// <remarks>Specified by <see cref="IEdgeReconnectionPortCandidateProvider.GetTargetPortCandidates"/>.</remarks>
    public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) => [];
  }
}
