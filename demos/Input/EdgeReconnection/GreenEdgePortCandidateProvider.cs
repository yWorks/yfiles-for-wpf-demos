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
using yWorks.Graph.PortLocationModels;

namespace yWorks.Demo.EdgeReconnection
{
  /// <summary>
  ///   An <see cref="IEdgeReconnectionPortCandidateProvider"/> that uses candidates with a
  ///   dynamic FreeNodePortLocationModel. It allows moving ports to any
  ///   location inside a green node.
  /// </summary>
  public sealed class GreenEdgePortCandidateProvider : IEdgeReconnectionPortCandidateProvider
  {
    /// <summary>
    ///   Returns all source port candidates that may be used for all nodes.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided</param>
    /// <returns>All applicable source port candidates.</returns>
    /// <remarks>Specified by <see cref="IEdgeReconnectionPortCandidateProvider.GetSourcePortCandidates"/>.</remarks>
    public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) {
      var graph = context.GetGraph();
      if (graph == null) {
        return [];
      }
      var candidates = new List<IPortCandidate>();
      foreach (var node in graph.Nodes) {
        if (Equals(node.Tag, "green")) {
          candidates.Add(new PortCandidate(node, FreeNodePortLocationModel.Instance));
        }
      }
      return candidates;
    }

    /// <summary>
    ///   Returns all target port candidates that may be used for all nodes.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided</param>
    /// <returns>All applicable target port candidates.</returns>
    /// <remarks>Specified by <see cref="IEdgeReconnectionPortCandidateProvider.GetTargetPortCandidates"/>.</remarks>
    public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) =>
        GetSourcePortCandidates(context);
  }
}
