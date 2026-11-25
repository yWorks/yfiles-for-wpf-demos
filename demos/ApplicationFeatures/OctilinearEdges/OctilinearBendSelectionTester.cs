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
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   A custom bend selection tester for octilinear edges that considers the translated bend handle location, so
  ///   that clicking the center of an octilinear segment actually selects the bend's handle.
  /// </summary>
  internal sealed class OctilinearBendSelectionTester : IBendSelectionTester
  {
    private readonly IEdge edge;

    internal OctilinearBendSelectionTester(IEdge edge) {
      this.edge = edge;
    }

    /// <summary>
    ///   Returns the translated location of the given bend, taking into account the handle offset.
    /// </summary>
    /// <param name="bend">The bend for which to get the translated location.</param>
    private static PointD GetTranslatedBendLocation(IBend bend) =>
        Utils.GetLocation(bend) + Utils.GetHandleOffset(bend);

    public IEnumerable<IBend> GetBendsInBox(IInputModeContext context, RectD rectangle) {
      foreach (var bend in edge.Bends) {
        var location = GetTranslatedBendLocation(bend);
        var radius = context.HitTestRadius;
        if (location.X >= rectangle.X - radius && location.X <= rectangle.X + rectangle.Width + radius &&
            location.Y >= rectangle.Y - radius && location.Y <= rectangle.Y + rectangle.Height + radius) {
          yield return bend;
        }
      }
    }

    public IEnumerable<IBend> GetBendsInPath(IInputModeContext context, GeneralPath lassoPath) {
      var result = new List<IBend>();
      foreach (var bend in edge.Bends) {
        var bendLocation = GetTranslatedBendLocation(bend);
        if (lassoPath.AreaOrPathContains(bendLocation, context.HitTestRadius)) {
          result.Add(bend);
        }
      }

      return result;
    }

    public IBend? GetHitBend(IInputModeContext context, PointD location) {
      var minimumDistanceSquared = double.MaxValue;
      var radiusSquared = context.HitTestRadius * context.HitTestRadius * 1.5;
      IBend? nearest = null;
      foreach (var bend in edge.Bends) {
        var bendLocation = GetTranslatedBendLocation(bend);
        var dx = bendLocation.X - location.X;
        var dy = bendLocation.Y - location.Y;
        var distanceSquared = dx * dx + dy * dy;
        if (distanceSquared < radiusSquared && distanceSquared < minimumDistanceSquared) {
          minimumDistanceSquared = distanceSquared;
          nearest = bend;
        }
      }
      return nearest;
    }
  }
}
