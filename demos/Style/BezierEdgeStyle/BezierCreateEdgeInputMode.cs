/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Graph.BezierEdgeStyle
{
  /// <summary>
  ///   Custom create edge input mode for bezier edges.
  /// </summary>
  /// <remarks>
  ///   This mode can operate in two different ways:
  ///   If <see cref="CreateSmoothSplines"/> is <see langword="true"/>, you create only the exterior control points and
  ///   the mode interpolates the missing middle control point for each triple.
  ///   Otherwise, you specify each control point exactly as intended.
  ///   During the gesture, the current hull curve is shown.
  /// </remarks>
  public class BezierCreateEdgeInputMode : CreateEdgeInputMode
  {
    /// <summary>
    ///   Reentrancy flag when we are inserting/removing dummy edge bends.
    /// </summary>
    private bool augmenting;

    public BezierCreateEdgeInputMode() {
      CreateSmoothSplines = true;
      // By default, we can't create orthogonal edges with this mode
      // (what would that look like)
      OrthogonalEdgeCreation = OrthogonalEdgeEditingPolicy.Never;

      CancelGestureOnInvalidEnd = false;

      ValidBendHitTestable = HitTestables.Create(delegate(IInputModeContext context, PointD location) {
        if (!(PreviewEdge.Style is yWorks.Graph.Styles.BezierEdgeStyle) || PreviewEdge.Bends.Count <= 2 || !CreateSmoothSplines) {
          return true;
        }
        var nextToLastBend = PreviewEdge.Bends.ElementAt(PreviewEdge.Bends.Count - 2);
        if (nextToLastBend == null) {
          return true;
        }
        // Require a minimum length for the control point triple
        return nextToLastBend.GetIndex() % 3 != 0 || (location - nextToLastBend.Location.ToPointD()).VectorLength > 10;
      });

      ConfigureDummyGraph();
      EdgeCreationStarted += (sender, args) => {
        ConfigureDummyEdge();
      };
    }

    private void ConfigureDummyGraph() {
      var dummyGraph = PreviewGraph;
      // Register to bend creation and removal events
      // in order to insert additional bends in the middle of a line segment
      // or remove them if the defining bend is removed
      dummyGraph.BendAdded += DummyGraph_BendAdded;
      dummyGraph.BendRemoved += DummyGraph_BendRemoved;
    }

    /// <summary>
    ///   If we have a bezier edge style, we decorate it so that we can also show the control points.
    /// </summary>
    /// <remarks>
    ///   A better solution that would however be more involved would be to show the decoration.
    /// </remarks>
    private void ConfigureDummyEdge() {
      var dummyEdge = PreviewEdge;
      var simpleEdge = dummyEdge as SimpleEdge;
      if (simpleEdge != null && dummyEdge.Style is yWorks.Graph.Styles.BezierEdgeStyle) {
        // By default, the BezierEdgeStyle has no bend creator
        // However, we want to be able to create bends here
        // So we insert a BendCreator into the dummy edge's lookup
        simpleEdge.GetDecorator().BendCreator.AddConstant(new DefaultBendCreator(simpleEdge));
      }
    }

    private void DummyGraph_BendRemoved(object sender, BendEventArgs e) {
      if (!augmenting) {
        if (CreateSmoothSplines && PreviewEdge.Style is yWorks.Graph.Styles.BezierEdgeStyle) {
          augmenting = true;
          try {
            if (PreviewEdge.Bends.Any() && PreviewEdge.Bends.Count % 3 == 0) {
              // Undo bend creation that finished a triple
              PreviewGraph.Remove(PreviewEdge.Bends.Last());
            }
          } finally {
            augmenting = false;
          }
        }
      }
    }

    private void DummyGraph_BendAdded(object sender, ItemEventArgs<IBend> e) {
      if (!augmenting) {
        if (CreateSmoothSplines && PreviewEdge.Style is yWorks.Graph.Styles.BezierEdgeStyle) {
          augmenting = true;
          try {
            if (PreviewEdge.Bends.Count % 3 == 0) {
              // Bend creation that finishes a control point line
              // Insert a middle bend
              var cp0 = PreviewEdge.Bends[PreviewEdge.Bends.Count - 2].Location.ToPointD();
              var cp2 = e.Item.Location.ToPointD();
              var cp1 = 0.5 * (cp2 - cp0) + cp0;
              PreviewGraph.AddBend(PreviewEdge, cp1, PreviewEdge.Bends.Count - 1);
            }
          } finally {
            augmenting = false;
          }
        }
      }
    }

    /// <summary>
    ///   Whether we want to create smooth splines.
    /// </summary>
    /// <remarks>
    ///   If <see langword="true"/>, each "bend" creation inserts one of the "exterior" control points for a cubic segment, and the point in the
    ///   middle is created automatically by the mode. Otherwise, each control point must be explicitly created.
    ///   Default value is true.
    /// </remarks>
    public bool CreateSmoothSplines { get; set; }

    /// <summary>
    ///   Overridden to pad the number of bends so that there are always 2 mod 3 by duplicating the last location, if
    ///   necessary.
    /// </summary>
    protected override IEdge CreateEdge(
        IGraph graph,
        IPortCandidate sourcePortCandidate,
        IPortCandidate targetPortCandidate) {
      if (CreateSmoothSplines && PreviewEdge.Style is yWorks.Graph.Styles.BezierEdgeStyle) {
        if (PreviewEdge.Bends.Any()) {
          augmenting = true;
          try {
            var lastLocation = PreviewEdge.Bends.Last().Location.ToPointD();
            if (PreviewEdge.Bends.Count % 3 == 1) {
              // We can reach this branch if we finish the edge creation
              // without having finished a control point triple
              // Just duplicate the last bend
              PreviewGraph.AddBend(PreviewEdge, lastLocation);
            } else if (PreviewEdge.Bends.Count % 3 == 0) {
              // Actually, we shouldn't be able to come here
              // since we always create bend triples and have an initial single control point
              PreviewGraph.AddBend(PreviewEdge, lastLocation);
              PreviewGraph.AddBend(PreviewEdge, lastLocation);
            }
          } finally {
            augmenting = false;
          }
        }
      }
      return base.CreateEdge(graph, sourcePortCandidate, targetPortCandidate);
    }
  }
}
