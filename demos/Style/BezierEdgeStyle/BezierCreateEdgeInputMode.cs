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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.BezierEdgeStyle
{
  /// <summary>
  ///   Custom create edge input mode for Bézier edges. This mode can work in two different ways:
  ///   when <see cref="CreateSmoothSplines"/> is true, the user creates only the exterior control points and the mode
  ///   interpolates the missing middle control point for each triple. Otherwise, each control point is created exactly
  ///   as placed. During the gesture, the current hull curve is shown.
  /// </summary>
  public sealed class BezierCreateEdgeInputMode : CreateEdgeInputMode
  {
    /// <summary>Re-entrance flag used while inserting or removing dummy edge bends.</summary>
    private bool augmenting;

    /// <summary>Additional render tree element that highlights the control point sequence.</summary>
    private IRenderTreeElement? controlPointHighlight;

    /// <summary>Whether we want to create smooth splines.</summary>
    private bool createSmoothSplines = true;

    /// <summary>
    ///   Determines whether to create smooth splines. If true, each bend creation inserts one of the exterior control
    ///   points for a cubic segment and the middle point is created automatically. Otherwise, each control point must be
    ///   specified. The default value is true.
    /// </summary>
    public bool CreateSmoothSplines {
      get => createSmoothSplines;
      set => createSmoothSplines = value;
    }

    public BezierCreateEdgeInputMode() {
      // By default, this mode cannot create orthogonal edges.
      OrthogonalEdgeCreation = OrthogonalEdgeEditingPolicy.Never;
      CancelGestureOnInvalidEnd = false;
      ValidBendHitTestable = new BezierValidBendHitTestable(this);

      ConfigureDummyGraph();
      EdgeCreationStarted += ConfigureDummyEdge;
    }

    /// <summary>
    ///   If the preview edge has a Bézier style, decorate it so the control points can also be shown.
    /// </summary>
    private void ConfigureDummyEdge(object? sender, InputModeItemEventArgs<IEdge> args) {
      if (PreviewEdge is SimpleEdge simpleEdge && simpleEdge.Style is Graph.Styles.BezierEdgeStyle) {
        // By default, BezierEdgeStyle has no bend creator, but we need one to create bends here.
        var defaultBendCreator = new SimpleEdge().Lookup<IBendCreator>();
        if (defaultBendCreator != null) {
          simpleEdge.GetDecorator().BendCreator.AddConstant(defaultBendCreator);
        }
      }
    }

    public override void Uninstall(IInputModeContext context) {
      if (controlPointHighlight != null) {
        context.CanvasControl.RenderTree.Remove(controlPointHighlight);
        controlPointHighlight = null;
      }
      base.Uninstall(context);
    }

    protected override void OnGestureCanceling(InputModeEventArgs inputModeEventArgs) {
      if (controlPointHighlight != null) {
        ParentInputModeContext?.CanvasControl.RenderTree.Remove(controlPointHighlight);
        controlPointHighlight = null;
      }
      base.OnGestureCanceling(inputModeEventArgs);
    }

    protected override void OnGestureFinishing(InputModeEventArgs inputModeEventArgs) {
      if (controlPointHighlight != null) {
        ParentInputModeContext?.CanvasControl.RenderTree.Remove(controlPointHighlight);
        controlPointHighlight = null;
      }
      base.OnGestureFinishing(inputModeEventArgs);
    }

    /// <summary>Registers to bend creation and removal to insert or remove middle bends in the preview edge.</summary>
    private void ConfigureDummyGraph() {
      PreviewGraph.BendAdded += OnBendAdded;
      PreviewGraph.BendRemoved += OnBendRemoved;
    }

    private void OnBendRemoved(object? sender, BendEventArgs args) {
      if (augmenting || !(CreateSmoothSplines && PreviewEdge.Style is Graph.Styles.BezierEdgeStyle)) {
        return;
      }
      augmenting = true;
      try {
        if (PreviewEdge.Bends.Count > 0 && PreviewEdge.Bends.Count % 3 == 0) {
          // Undo bend creation that finished a triple.
          PreviewGraph.Remove(PreviewEdge.Bends[^1]);
        }
      } finally {
        augmenting = false;
      }
    }

    private void OnBendAdded(object? sender, ItemEventArgs<IBend> args) {
      if (augmenting || !(CreateSmoothSplines && PreviewEdge.Style is Graph.Styles.BezierEdgeStyle)) {
        return;
      }
      augmenting = true;
      try {
        if (PreviewEdge.Bends.Count % 3 == 0) {
          // Bend creation finished a control point line, so insert its middle bend.
          var cp0 = PreviewEdge.Bends[PreviewEdge.Bends.Count - 2].Location.ToPointD();
          var cp2 = args.Item.Location.ToPointD();
          var cp1 = cp0 + (cp2 - cp0) * 0.5;
          PreviewGraph.AddBend(PreviewEdge, cp1, PreviewEdge.Bends.Count - 1);
        }
      } finally {
        augmenting = false;
      }
    }

    /// <summary>Pads the number of bends so that it is always 2 mod 3 by duplicating the last location if necessary.</summary>
    protected override IEdge? CreateEdge(
        IGraph graph,
        IPortCandidate sourcePortCandidate,
        IPortCandidate? targetPortCandidate) {
      if (CreateSmoothSplines && PreviewEdge.Style is Graph.Styles.BezierEdgeStyle && PreviewEdge.Bends.Count > 0) {
        augmenting = true;
        try {
          var lastLocation = PreviewEdge.Bends[^1].Location.ToPointD();
          if (PreviewEdge.Bends.Count % 3 == 1) {
            // The edge creation can finish before a control point triple is complete; duplicate the last bend.
            PreviewGraph.AddBend(PreviewEdge, lastLocation);
          } else if (PreviewEdge.Bends.Count % 3 == 0) {
            // A bend count divisible by three should not occur, but complete the control point sequence if it does.
            PreviewGraph.AddBend(PreviewEdge, lastLocation);
            PreviewGraph.AddBend(PreviewEdge, lastLocation);
          }
        } finally {
          augmenting = false;
        }
      }
      return base.CreateEdge(graph, sourcePortCandidate, targetPortCandidate);
    }

    private sealed class BezierValidBendHitTestable : IHitTestable
    {
      private readonly BezierCreateEdgeInputMode inputMode;

      internal BezierValidBendHitTestable(BezierCreateEdgeInputMode inputMode) {
        this.inputMode = inputMode;
      }

      public bool IsHit(IInputModeContext context, PointD location) {
        var previewEdge = inputMode.PreviewEdge;
        if (previewEdge == null || previewEdge.Bends.Count == 0 ||
            previewEdge.Style is not yWorks.Graph.Styles.BezierEdgeStyle || !inputMode.CreateSmoothSplines) {
          return true;
        }
        if (previewEdge.Bends.Count < 2) {
          return true;
        }

        // Require a minimum length for the control point triple.
        var lastBend = previewEdge.Bends[^2];
        return lastBend.GetIndex() % 3 != 0 || (location - lastBend.Location.ToPointD()).VectorLength > 10;
      }
    }
  }
}
