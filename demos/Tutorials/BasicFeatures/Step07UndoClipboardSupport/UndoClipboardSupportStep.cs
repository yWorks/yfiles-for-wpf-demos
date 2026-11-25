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

using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step07UndoClipboardSupport
{
  /// <summary>
  ///   Tutorial step 07: Undo and Clipboard Support.
  /// </summary>
  /// <remarks>
  ///   Shows how to enable undo/redo functionality and how to use clipboard operations
  ///   (cut, copy, paste, duplicate) via <see cref="GraphEditorInputMode"/>.
  /// </remarks>
  public class UndoClipboardSupportStep : TutorialStepBase
  {
    public override string Title => "07 Undo and Clipboard Support";

    public override string DescriptionSource => "Step07UndoClipboardSupport.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      BasicFeaturesTutorial.CreateSampleGraph(graph);
      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);

      // Enable undo functionality
      graph.SetUndoEngineEnabled(true);

      // The clipboard functionality is already enabled by default in GraphEditorInputMode.
      // We only need to register the mode.
      var inputMode = new GraphEditorInputMode();
      graphControl.InputMode = inputMode;

      // Register button listeners for undo/redo and clipboard operations
      AddButtonListener(description, "undoButton", () => {
        if (graph.IsUndoEngineEnabled() && graph.GetUndoEngine() is { } ue && ue.CanUndo()) {
          ue.Undo();
        }
      });

      AddButtonListener(description, "redoButton", () => {
        if (graph.IsUndoEngineEnabled() && graph.GetUndoEngine() is { } ue && ue.CanRedo()) {
          ue.Redo();
        }
      });

      AddButtonListener(description, "clearUndoQueueButton", () => {
        if (graph.IsUndoEngineEnabled()) {
          graph.GetUndoEngine()?.Clear();
        }
      });

      AddButtonListener(description, "cutButton", inputMode.Cut);
      AddButtonListener(description, "copyButton", inputMode.Copy);
      AddButtonListener(description, "pasteButton", inputMode.Paste);
      AddButtonListener(description, "duplicateButton", inputMode.DuplicateSelection);
    }

    /// <summary>
    ///   Demonstrates clipboard low-level functionality.
    ///   This method is not called but exists for documentation purposes.
    /// </summary>
    private static void EnableClipboard(GraphControl graphControl, GraphEditorInputMode graphEditorInputMode) {
      graphEditorInputMode.AllowClipboardOperations = true; // this is the default, already

      // programmatically copy the selected graph items
      if (graphControl.Selection.Nodes.Count > 0) {
        graphEditorInputMode.Copy();
      }

      // programmatically paste and clear the clipboard content
      if (!graphControl.Clipboard.IsEmpty) {
        graphEditorInputMode.Paste();
        graphControl.Clipboard.Clear();
      }
    }
  }
}
