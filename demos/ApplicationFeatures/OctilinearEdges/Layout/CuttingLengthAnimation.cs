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

using System;
using yWorks.Controls;
using yWorks.Graph;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   An animation that changes the cutting length of a bend of an edge.
  ///   If the bend is not known at the beginning of the animation, it is looked up lazily. The main graph animation
  ///   creates new bends in the first time step, afterward, the cutting length animation for the bend can start.
  /// </summary>
  internal sealed class CuttingLengthAnimation : IAnimation
  {
    /// <summary>
    ///   The <see cref="GraphControl"/> to which the animation is applied.
    /// </summary>
    private readonly GraphControl graphControl;

    /// <summary>
    ///   The owner edge of the bend.
    /// </summary>
    private readonly IEdge edge;

    /// <summary>
    ///   The index of the bend in the edgeCuttingLengths array.
    /// </summary>
    private readonly int bendIndex;

    /// <summary>
    ///   The target cutting lengths for the edge's bends. The array index corresponds to the index of the bend in the edge.
    /// </summary>
    private readonly double[] edgeCuttingLengths;

    /// <summary>
    ///   The bend for which this animation is responsible. May be initialized lazily during the animation.
    /// </summary>
    private IBend? bend;

    /// <summary>
    ///   Holds the initial cutting length of the bend. This is used to calculate the interpolated cutting length.
    /// </summary>
    private double startCuttingLength;

    /// <summary>
    ///   Whether this cutting length needs to be animated.
    /// </summary>
    private bool needsAnimation;

    internal CuttingLengthAnimation(
        GraphControl graphControl,
        IEdge edge,
        IBend? bend,
        int bendIndex,
        double[] edgeCuttingLengths,
        TimeSpan preferredDuration) {
      this.graphControl = graphControl;
      this.edge = edge;
      this.bend = bend;
      this.bendIndex = bendIndex;
      this.edgeCuttingLengths = edgeCuttingLengths;
      PreferredDuration = preferredDuration;
    }

    /// <summary>
    ///   The preferred duration of the cutting length animation.
    /// </summary>
    public TimeSpan PreferredDuration { get; }

    public void Initialize() {
      startCuttingLength = bend == null ? 0 : Utils.GetCuttingLength(bend);
      needsAnimation = startCuttingLength != GetTargetCuttingLength();
    }

    public void Animate(double time) {
      if (!needsAnimation) {
        return;
      }

      // if the bend was not there from the beginning, check again if it is created now
      ConfigureAffectedBend();

      // interpolate and update the cutting length of the bend
      var targetCuttingLength = GetTargetCuttingLength();
      var cuttingLength = time >= 1
          ? targetCuttingLength
          : startCuttingLength + (targetCuttingLength - startCuttingLength) * time;
      Utils.UpdateCuttingLength(bend, cuttingLength);
      graphControl.Invalidate();
    }

    public void CleanUp() {
      // Ensure the final cutting length is correct. For example, if bends have been removed during the
      // animation, the remaining bends can only be updated in the last step because it is unclear
      // during the animation, which bends are eventually removed.
      ConfigureAffectedBend();
      Utils.UpdateCuttingLength(bend, GetTargetCuttingLength());
    }

    /// <summary>
    ///   Returns the target cutting length that this animation should result to.
    /// </summary>
    private double GetTargetCuttingLength() => edgeCuttingLengths[bendIndex];

    /// <summary>
    ///   Lazily determines the actual bend that this animation is responsible for.
    ///   When new bends are created as a result of the main graph animation, the cutting length animation for the new bend
    ///   can start. This is done lazily because the bend is not available at the beginning of the animation.
    /// </summary>
    private void ConfigureAffectedBend() {
      if (bend != null) {
        return;
      }

      // when all bends are created, we can safely map the target cutting lengths
      var currentEdgeBends = edge.Bends;
      if (currentEdgeBends.Count == edgeCuttingLengths.Length) {
        bend = currentEdgeBends[bendIndex];
        startCuttingLength = Utils.GetCuttingLength(bend);
      }
    }
  }
}
