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

namespace yWorks.Demo.OrthogonalEdgeEditingCustomization
{
  /// <summary>
  ///   An <see cref="IOrthogonalEdgeHelper"/> for edges that don't have orthogonal editing behavior.
  /// </summary>
  public sealed class RedOrthogonalEdgeHelper : IOrthogonalEdgeHelper
  {
    public RedOrthogonalEdgeHelper(IEdge edge) { }

    /// <summary>
    ///   Returns the non-orthogonal segment orientation.
    /// </summary>
    /// <param name="context">The input mode context in which the segment is edited.</param>
    /// <param name="segmentIndex">The index of the segment.</param>
    /// <see cref="IOrthogonalEdgeHelper.GetSegmentOrientation"/>
    public SegmentOrientation GetSegmentOrientation(IInputModeContext context, int segmentIndex) =>
        SegmentOrientation.NonOrthogonal;

    /// <summary>
    ///   Returns <see langword="false"/>.
    /// </summary>
    /// <param name="context">The input mode context in which the segment is edited.</param>
    /// <param name="sourceEnd">
    ///   <see langword="true"/> if the source end of the edge is queried, <see langword="false"/> for
    ///   the target end.
    /// </param>
    /// <see cref="IOrthogonalEdgeHelper.ShouldMoveEndImplicitly"/>
    public bool ShouldMoveEndImplicitly(IInputModeContext context, bool sourceEnd) => false;

    /// <summary>
    ///   Returns <see langword="false"/>.
    /// </summary>
    /// <param name="context">The input mode context in which the segment is edited.</param>
    /// <see cref="IOrthogonalEdgeHelper.ShouldEditOrthogonally"/>
    public bool ShouldEditOrthogonally(IInputModeContext context) => false;

    /// <summary>
    ///   Does nothing; no cleanup of bends performed.
    /// </summary>
    /// <param name="context">The input mode context which edited the edge.</param>
    /// <param name="graph">The graph to use for modifying the edge instance.</param>
    /// <see cref="IOrthogonalEdgeHelper.CleanUpEdge"/>
    public void CleanUpEdge(IInputModeContext context, IGraph graph) { }
  }
}
