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
using yWorks.Utils;

namespace yWorks.Demo.EdgeReconnection
{
  /// <summary>
  ///   An <see cref="IEdgeReconnectionPortCandidateProvider"/> that allows moving ports to
  ///   any other orange node, except for the opposite port's node.
  /// </summary>
  public sealed class OrangeEdgePortCandidateProvider : IEdgeReconnectionPortCandidateProvider
  {
    private readonly IEdge edge;

    /// <summary>
    ///   Creates a new instance of <see cref="OrangeEdgePortCandidateProvider"/>.
    /// </summary>
    /// <param name="edge">The given edge</param>
    public OrangeEdgePortCandidateProvider(IEdge edge) {
      this.edge = edge;
    }

    /// <summary>
    ///   Returns candidates for all ports at orange nodes in the graph, except
    ///   for the current target node to avoid the creation of self-loops.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided</param>
    /// <returns>Candidates at orange nodes other than the current target node.</returns>
    /// <remarks>Specified by <see cref="IEdgeReconnectionPortCandidateProvider.GetSourcePortCandidates"/>.</remarks>
    public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) {
      var result = new List<IPortCandidate>();
      // add the current one as the default
      result.Add(new PortCandidate(edge.SourcePort));

      var graph = context.GetGraph();
      if (graph == null) {
        return result;
      }
      foreach (var node in graph.Nodes) {
        if (node != edge.TargetPort.Owner && Equals(node.Tag, "orange")) {
          var provider = node.Lookup<IPortCandidateProvider>();
          // If available, use the candidates from the provider. Otherwise, add a default candidate.
          if (provider != null) {
            result.AddRange(provider.GetTargetPortCandidates(context));
          } else {
            result.Add(new PortCandidate(node, FreeNodePortLocationModel.Center));
          }
        }
      }
      return result;
    }

    /// <summary>
    ///   Returns candidates for all ports at orange nodes in the graph, except
    ///   for the current source node to avoid the creation of self-loops.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided</param>
    /// <returns>Candidates at orange nodes other than the current source node.</returns>
    /// <remarks>Specified by <see cref="IEdgeReconnectionPortCandidateProvider.GetTargetPortCandidates"/>.</remarks>
    public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) {
      var result = new List<IPortCandidate>();
      // add the current one as the default
      result.Add(new PortCandidate(edge.TargetPort));

      var graph = context.GetGraph();
      if (graph == null) {
        return result;
      }
      foreach (var node in graph.Nodes) {
        if (node != edge.SourcePort.Owner && Equals(node.Tag, "orange")) {
          var provider = node.Lookup<IPortCandidateProvider>();
          // If available, use the candidates from the provider. Otherwise, add a default candidate.
          if (provider != null) {
            result.AddRange(provider.GetSourcePortCandidates(context));
          } else {
            result.Add(new PortCandidate(node, FreeNodePortLocationModel.Center));
          }
        }
      }
      return result;
    }
  }
}
