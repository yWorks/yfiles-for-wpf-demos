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
  ///   The <see cref="OrthogonalEdgeHelper"/> for blue edges. Orthogonal edge
  ///   editing is enabled for the inner segments of this edge but not for the
  ///   first and last one.
  /// </summary>
  public sealed class BlueOrthogonalEdgeHelper(IEdge edge) : OrthogonalEdgeHelper(edge)
  {
    /// <summary>
    ///   Returns the NonOrthogonal segment orientation for the first and last
    ///   segment, and the default for all other segments.
    /// </summary>
    /// <param name="inputModeContext">The input mode context in which the orientation is needed.</param>
    /// <param name="segmentIndex">The index of the segment.</param>
    /// <see cref="OrthogonalEdgeHelper.GetSegmentOrientation"/>
    /// <see cref="IOrthogonalEdgeHelper.GetSegmentOrientation"/>
    public override SegmentOrientation GetSegmentOrientation(IInputModeContext inputModeContext, int segmentIndex) {
      var isFirstOrLastSegment = segmentIndex == 0 || segmentIndex == Edge.Bends.Count;
      return isFirstOrLastSegment
          ? SegmentOrientation.NonOrthogonal
          : base.GetSegmentOrientation(inputModeContext, segmentIndex);
    }
  }
}
