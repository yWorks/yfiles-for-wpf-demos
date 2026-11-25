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

namespace yWorks.Demo.PortCandidateProvider
{
  /// <summary>
  ///   This port candidate provider provides port candidates for the
  ///   ports of a node. If a port already has a connected edge, its
  ///   port candidate is marked as invalid.
  /// </summary>
  public sealed class BluePortCandidateProvider : PortCandidateProviderBase
  {
    private readonly INode node;

    /// <summary>
    ///   Creates a new instance of <see cref="BluePortCandidateProvider"/>.
    /// </summary>
    /// <param name="node">The given node.</param>
    public BluePortCandidateProvider(INode node) {
      this.node = node;
    }

    /// <summary>
    ///   Returns a list that contains a port candidate for each of the node's
    ///   ports. Each candidate has the same location as the port. If a port
    ///   already has a connected edge, its port candidate is marked as invalid.
    ///   Note that the various variants of <see cref="PortCandidateProviderBase.GetPortCandidates"/>
    ///   delegate to this method. This can be used to provide the same candidates for all use-cases.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided.</param>
    protected override IEnumerable<IPortCandidate> GetPortCandidates(IInputModeContext context) {
      var candidates = new List<IPortCandidate>();
      var graph = context.GetGraph();

      // Create the candidate for each port
      if (graph != null) {
        foreach (var port in node.Ports) {
          var portCandidate = new PortCandidate(port) {
              Validity = graph.Degree(port) == 0 ? PortCandidateValidity.Valid : PortCandidateValidity.Invalid
          };
          candidates.Add(portCandidate);
        }
      }

      // If no candidates have been created so far, create a single invalid candidate as fallback
      if (candidates.Count == 0) {
        var item = new PortCandidate(node, FreeNodePortLocationModel.Center) {
            Validity = PortCandidateValidity.Invalid
        };
        candidates.Add(item);
      }

      return candidates;
    }
  }
}
