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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Organic;
using yWorks.Utils;

namespace yWorks.Demo.LassoSelection
{
  /// <summary>
  ///   Shows how to use a lasso tool to select graph elements by drawing around them.
  /// </summary>
  [Demo("lasso-selection", "input", "Lasso Selection",
      Summary = "Shows how to configure a lasso tool for freeform selection.", Tags = ["interaction", "selection"],
      Keywords = ["testable", "free", "playground"], RelatedDemos = [])]
  public sealed class LassoSelectionDemo : SimpleDemo
  {
    private GraphEditorInputMode inputMode = null!;

    protected override void Initialize() {
      // Enable lasso selection by default
      inputMode = new GraphEditorInputMode();
      var lassoSelectionInputMode = inputMode.LassoSelectionInputMode;
      lassoSelectionInputMode.FinishRadius = 10;
      lassoSelectionInputMode.Enabled = true;
      GraphControl.InputMode = inputMode;
      SetSelectionStyle("free-hand-selection");

      // Configure UI buttons
      ToolBar.AddComboBox("Selection Style:", [
          ("free-hand-selection", "Free-hand"), ("polyline-selection", "Polyline"), ("marquee-selection", "Marquee")
      ], value => {
        if (value != null) {
          SetSelectionStyle(value);
        }
      }, "free-hand-selection");
      ToolBar.AddComboBox("Select Nodes:", [
          ("nodes-fully-contained", "Fully Contained"), ("nodes-intersected", "Intersected"),
          ("nodes-center-contained", "Center Contained")
      ], value => {
        if (value != null) {
          SetLassoTestables(value);
        }
      }, "nodes-intersected");
      ToolBar.AddSlider("Finish Radius:", 5, 30, SetFinishRadius, 10);
      SetLassoTestables("nodes-intersected");

      // Build and layout the graph
      BuildGraphFromJson("graph-data.json");
      Graph.ApplyLayout(new OrganicLayout {
          DefaultMinimumNodeDistance = 50,
          AutomaticGroupNodeCompaction = true,
          LayoutOrientation = LayoutOrientation.TopToBottom
      });
      GraphControl.FitGraphBounds();
      Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Configures the selection style (free-hand, polyline, or marquee).
    /// </summary>
    private void SetSelectionStyle(string style) {
      var lassoSelectionInputMode = inputMode.LassoSelectionInputMode;

      switch (style) {
        default:
        case "free-hand-selection":
          // Configure free-hand lasso selection
          lassoSelectionInputMode.DragFreeHandRecognizer = EventRecognizers.MouseDrag;
          lassoSelectionInputMode.StartSegmentRecognizer = EventRecognizers.Never;
          lassoSelectionInputMode.EndSegmentRecognizer = EventRecognizers.MouseDown;
          lassoSelectionInputMode.DragSegmentRecognizer = EventRecognizers.MouseMove;
          lassoSelectionInputMode.FinishRecognizer = EventRecognizers.MouseUp;

          // Touch equivalents
          lassoSelectionInputMode.DragFreeHandRecognizerTouch = EventRecognizers.TouchPrimaryDrag;
          lassoSelectionInputMode.StartSegmentRecognizerTouch = EventRecognizers.Never;
          lassoSelectionInputMode.EndSegmentRecognizerTouch = EventRecognizers.TouchPrimaryDown;
          lassoSelectionInputMode.DragSegmentRecognizerTouch = EventRecognizers.TouchPrimaryDrag;
          lassoSelectionInputMode.FinishRecognizerTouch = EventRecognizers.TouchPrimaryUp;

          lassoSelectionInputMode.Enabled = true;
          break;
        case "polyline-selection":
          // Configure polyline lasso selection
          lassoSelectionInputMode.DragFreeHandRecognizer = EventRecognizers.MouseDrag;
          lassoSelectionInputMode.StartSegmentRecognizer = EventRecognizers.Always;
          // End a segment with click
          lassoSelectionInputMode.EndSegmentRecognizer = (source, eventArgs) =>
              EventRecognizers.MouseDown(source, eventArgs) || EventRecognizers.MouseUp(source, eventArgs);
          lassoSelectionInputMode.DragSegmentRecognizer = (source, eventArgs) =>
              EventRecognizers.MouseDrag(source, eventArgs) || EventRecognizers.MouseMove(source, eventArgs);
          // Finish anywhere with a double click
          lassoSelectionInputMode.FinishRecognizer = (_, eventArgs) =>
              eventArgs is PointerEventArgs { ClickCount: > 1 };

          // Touch equivalents
          lassoSelectionInputMode.DragFreeHandRecognizerTouch = EventRecognizers.TouchPrimaryDrag;
          lassoSelectionInputMode.StartSegmentRecognizerTouch = EventRecognizers.Always;
          lassoSelectionInputMode.EndSegmentRecognizerTouch = (source, eventArgs) =>
              EventRecognizers.TouchPrimaryDown(source, eventArgs) ||
              EventRecognizers.TouchPrimaryUp(source, eventArgs);
          lassoSelectionInputMode.DragSegmentRecognizerTouch = EventRecognizers.TouchPrimaryDrag;
          lassoSelectionInputMode.FinishRecognizerTouch = EventRecognizers.TouchPrimaryDoubleTap;

          lassoSelectionInputMode.Enabled = true;
          break;
        case "marquee-selection":
          // Disable lasso selection to allow marquee selection on left drag gestures
          lassoSelectionInputMode.Enabled = false;
          break;
      }
    }

    /// <summary>
    ///   Sets the radius around the starting point of the lasso within which it can be closed.
    ///   When dragging the lasso near its starting point, a circle with corresponding radius is highlighted.
    /// </summary>
    private void SetFinishRadius(double radius) {
      inputMode.LassoSelectionInputMode.FinishRadius = radius;
    }

    private IContextLookupChainLink? decoration;

    /// <summary>
    ///   Configures how nodes are considered selected by the lasso tool.
    /// </summary>
    private void SetLassoTestables(string mode) {
      var nodeDecorator = Graph.GetDecorator().Nodes;
      var lassoTestableDecorator = nodeDecorator.LassoTestable;
      if (decoration != null) {
        nodeDecorator.Remove(decoration);
      }
      if (mode == "nodes-fully-contained") {
        // Nodes are selected only if fully contained within the lasso path
        decoration = lassoTestableDecorator.AddFactory(node => new PredicateLassoTestable((_, lassoPath) =>
            !lassoPath.PathIntersects(node.Layout, 0) && lassoPath.AreaContains(node.Layout.GetCenter())));
      } else if (mode == "nodes-intersected") {
        // Nodes are selected if they intersect or are fully contained within the lasso path
        decoration = lassoTestableDecorator.AddFactory(node => LassoTestables.FromRectangle(node.Layout));
      } else if (mode == "nodes-center-contained") {
        // Nodes are selected only if their center point is contained within the lasso path
        decoration = lassoTestableDecorator.AddFactory(node => LassoTestables.FromPoint(node.Layout.GetCenter()));
      }
    }

    private sealed class PredicateLassoTestable(Func<IInputModeContext, GeneralPath, bool> isInPath) : ILassoTestable
    {
      public bool IsInPath(IInputModeContext context, GeneralPath lassoPath) => isInPath(context, lassoPath);
    }

    [STAThread]
    public static void Main() => Run<LassoSelectionDemo>();
  }
}
