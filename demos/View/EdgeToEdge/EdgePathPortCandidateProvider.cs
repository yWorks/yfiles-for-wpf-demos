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

namespace yWorks.Demo.EdgeToEdge
{
  /// <summary>
  ///   A port candidate provider that aggregates different <see cref="IPortLocationModel"/>s
  ///   to provide a number of port candidates along the path of the edge.
  /// </summary>
  public sealed class EdgePathPortCandidateProvider : PortCandidateProviderBase
  {
    private readonly IEdge edge;

    /// <summary>
    ///   Create a new instance of this type.
    /// </summary>
    public EdgePathPortCandidateProvider(IEdge edge) {
      this.edge = edge;
    }

    /// <summary>
    ///   Creates an enumeration of possible port candidates.
    /// </summary>
    protected override IEnumerable<IPortCandidate> GetPortCandidates(IInputModeContext context) {
      var candidates = new List<IPortCandidate>();

      // add equally distributed port candidates along the edge
      for (var i = 1; i < 10; ++i) {
        candidates.Add(new PortCandidate(edge, EdgePathPortLocationModel.Instance.CreateRatioParameter(0.1 * i)));
      }

      // add a dynamic candidate that can be used if shift is pressed to assign the exact location.
      candidates.Add(new PortCandidate(edge, EdgePathPortLocationModel.Instance));

      return candidates;
    }
  }
}
