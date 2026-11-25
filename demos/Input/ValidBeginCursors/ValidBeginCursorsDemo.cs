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
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.ValidBeginCursors
{
  [Demo("valid-begin-cursors", "input", "Valid Begin Cursors",
      Summary = "Shows how various cursors can be used to indicate valid gestures at the current location.",
      Tags = ["interaction"], Keywords = ["cursors", "marquee", "lasso", "viewport", "tooltips", "playground"],
      RelatedDemos = [])]
  public sealed class ValidBeginCursorsDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Initialize the editor input mode
      var editorInputMode = new GraphEditorInputMode();

      // --- Lasso Selection Configuration ---
      // Enable lasso selection and configure it to start when the 'Alt' key is pressed
      editorInputMode.LassoSelectionInputMode.Enabled = true;
      editorInputMode.LassoSelectionInputMode.ValidBeginRecognizer = EventRecognizers.AltIsDown;
      // Configures custom cursors for different lasso selection states
      editorInputMode.LassoSelectionInputMode.ValidBeginCursor = CustomCursorIcons.Lasso;
      editorInputMode.LassoSelectionInputMode.LassoCursor = CustomCursorIcons.Lasso;
      editorInputMode.LassoSelectionInputMode.ExtendSelectionCursor = CustomCursorIcons.LassoPlus;
      editorInputMode.LassoSelectionInputMode.SubtractSelectionCursor = CustomCursorIcons.LassoMinus;
      // Set the cursor and radius to indicate where the lasso gesture can be completed
      editorInputMode.LassoSelectionInputMode.ValidEndCursor = Cursors.Cross;
      editorInputMode.LassoSelectionInputMode.FinishRadius = 10;

      // --- Marquee Selection Configuration ---
      // Configure cursors for different marquee selection states
      editorInputMode.MarqueeSelectionInputMode.ValidBeginCursor = Cursors.Cross;
      editorInputMode.MarqueeSelectionInputMode.MarqueeCursor = Cursors.Cross;
      editorInputMode.MarqueeSelectionInputMode.ExtendSelectionCursor = CustomCursorIcons.CrosshairPlus;
      editorInputMode.MarqueeSelectionInputMode.SubtractSelectionCursor = CustomCursorIcons.CrosshairMinus;

      // --- Viewport Movement Configuration ---
      // Configure viewport panning to start with the 'Ctrl' key and use grab/grabbing cursors
      editorInputMode.MoveViewportInputMode.ValidBeginRecognizer = EventRecognizers.CtrlIsDown;
      editorInputMode.MoveViewportInputMode.ValidBeginCursor = Cursors.Hand;
      editorInputMode.MoveViewportInputMode.DragCursor = Cursors.ScrollAll;

      // --- Tooltip Configuration ---
      // Configure tooltips to appear when hovering over an edge, indicated by a help cursor
      editorInputMode.ToolTipInputMode.ValidHoverLocationHitTestable = new EdgeHitTestable(GraphControl);
      editorInputMode.ToolTipInputMode.ValidHoverLocationCursor = Cursors.Help;
      // Assign a higher priority to the tooltip mode to ensure its cursor is displayed when hovering over an edge
      editorInputMode.ToolTipInputMode.Priority = editorInputMode.CreateEdgeInputMode.Priority - 3;
      // Display a simple tooltip containing information about the source and target node
      editorInputMode.QueryItemToolTip += (_, evt) => {
        if (evt.Item is IEdge edge && !evt.Handled) {
          evt.ToolTip = $"{edge.GetSourceNode()} -> {edge.GetTargetNode()}";
          evt.Handled = true;
        }
      };

      // --- Edge Creation Configuration ---
      // Modifies edge creation to require both the 'Ctrl' key to be pressed and hovering over a node to begin.
      // This is achieved by combining the default beginHintTestable (node hover check) with a Ctrl key down check.
      var defaultBeginHitTestable = editorInputMode.CreateEdgeInputMode.BeginHitTestable;
      editorInputMode.CreateEdgeInputMode.BeginHitTestable = HitTestables.Create((context, location) =>
          defaultBeginHitTestable.IsHit(context, location) &&
          GraphControl.LastPointerEvent.Modifiers.HasFlag(ModifierKeys.Control));
      // Set custom cursors to indicate when edge creation is valid to begin and during port dragging.
      editorInputMode.CreateEdgeInputMode.ValidBeginCursor = CustomCursorIcons.CreateEdge;
      editorInputMode.CreateEdgeInputMode.StartOverCandidateOnly = false;
      editorInputMode.CreateEdgeInputMode.StartPortOwnerDraggingCursor = CustomCursorIcons.CreateEdge;
      // Disable enforced bend creation, allowing edge creation to be completed even with 'Ctrl' held down
      editorInputMode.CreateEdgeInputMode.EnforceBendCreationRecognizer = EventRecognizers.Never;
      // Assign a higher priority to the edge creation mode to resolve conflicts with MoveViewportInputMode,
      // as both modes use the 'Ctrl' modifier.
      editorInputMode.CreateEdgeInputMode.Priority = editorInputMode.MoveViewportInputMode.Priority - 2;

      GraphControl.InputMode = editorInputMode;

      // Build and lay out the graph
      BuildGraphFromJson("graph-data.json");
      Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      GraphControl.FitGraphBounds();
      Graph.SetUndoEngineEnabled(true);
    }

    [STAThread]
    public static void Main() => Run<ValidBeginCursorsDemo>();
  }
}
