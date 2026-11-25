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

namespace yWorks.Demo.PortCandidateProvider
{
  /// <summary>
  ///   This port candidate provider only allows connections from green nodes.
  ///   To achieve this, this class returns different port candidates for source
  ///   and target ports.
  /// </summary>
  public sealed class GreenPortCandidateProvider : PortCandidateProviderBase
  {
    private readonly INode node;

    /// <summary>
    ///   Creates a new instance of <see cref="GreenPortCandidateProvider"/>.
    /// </summary>
    /// <param name="node">The given node.</param>
    public GreenPortCandidateProvider(INode node) {
      this.node = node;
    }

    /// <summary>
    ///   Returns a central port candidate if the owner node of the source
    ///   candidate is green, and an empty list otherwise.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided.</param>
    /// <param name="source">The opposite port candidate.</param>
    public override IEnumerable<IPortCandidate> GetTargetPortCandidates(
        IInputModeContext context,
        IPortCandidate source) {
      // Check if the source node is green
      if (source != null && Equals(source.Owner.Tag, "green")) {
        return PortCandidateProviders.FromNodeCenter(node).GetTargetPortCandidates(context, source);
      }
      return [];
    }

    /// <summary>
    ///   Returns a list that contains a port candidate for each of the node's
    ///   ports. Each candidate has the same location as the port. If a port
    ///   already has a connected edge, its port candidate is marked as invalid.
    ///   Note that the variants of <see cref="PortCandidateProviderBase.GetTargetPortCandidates"/>
    ///   are all implemented by this class. Therefore, this method is only used for source ports.
    /// </summary>
    /// <param name="context">The context for which the candidates should be provided.</param>
    protected override IEnumerable<IPortCandidate> GetPortCandidates(IInputModeContext context) {
      var candidates = new List<IPortCandidate>();
      var hasValid = false;
      var graph = context.GetGraph();

      if (graph != null) {
        // Create a port candidate for each free port on the node
        foreach (var port in node.Ports) {
          var portCandidate = new PortCandidate(port);
          var valid = graph.Degree(port) == 0;
          hasValid = hasValid || valid;
          portCandidate.Validity = valid ? PortCandidateValidity.Valid : PortCandidateValidity.Invalid;
          candidates.Add(portCandidate);
        }
      }

      // If no valid candidates have been created so far, use the ShapeGeometryPortCandidateProvider as fallback.
      // This provides a candidate in the middle of each of the four sides of the node.
      if (!hasValid) {
        candidates.AddRange(PortCandidateProviders.FromShapeGeometry(node, 0.5).GetSourcePortCandidates(context));
      }

      return candidates;
    }
  }
}
