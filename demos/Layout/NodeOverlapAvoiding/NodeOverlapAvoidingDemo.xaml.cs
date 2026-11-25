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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.NodeOverlapAvoiding
{
  /// <summary>
  ///   Shows how to interactively edit graphs without creating overlaps.
  /// </summary>
  [Demo("node-overlap-avoiding", "layout", "Node Overlap Avoiding",
      Summary = "Shows how an automatic layout can remove node overlaps while a user interactively edits a graph.",
      Tags = ["layout", "interactive layout"],
      Keywords = [
          "layoutfeatures", "move", "resize", "resizing", "space", "clear area layout", "fill area layout",
          "adjustment", "interactive"
      ])]
  public sealed partial class NodeOverlapAvoidingDemo
  {
    private GraphEditorInputMode editMode = null!;

    public NodeOverlapAvoidingDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      InitializeInputModes();
      InitializeGraph();
      InitializeUI();
    }

    /// <summary>
    ///   Registers the <see cref="GraphEditorInputMode"/> and initializes the handlers that create
    ///   space for moved, resized, and newly created nodes.
    /// </summary>
    private void InitializeInputModes() {
      var graph = graphControl.Graph;

      // Create a GraphEditorInputMode instance.
      editMode = new GraphEditorInputMode();
      graphControl.InputMode = editMode;

      // Enable single selection.
      SingleSelectionHelper.EnableSingleSelection(graphControl);

      // Enable orthogonal edge editing.
      var orthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext();
      editMode.OrthogonalEdgeEditingContext = orthogonalEdgeEditingContext;

      // Disable orthogonal edge editing during node move or resize gestures.
      void DisableOrthogonalEdgeEditing(object? sender, InputModeEventArgs args) {
        if (editMode.MoveUnselectedItemsInputMode.AffectedItems.Count() > 0) {
          orthogonalEdgeEditingContext.Enabled = false;
        }
      }

      void EnableOrthogonalEdgeEditing(object? sender, InputModeEventArgs args) {
        orthogonalEdgeEditingContext.Enabled = true;
      }

      editMode.HandleInputMode.DragStarting += DisableOrthogonalEdgeEditing;
      editMode.HandleInputMode.DragFinished += EnableOrthogonalEdgeEditing;
      editMode.HandleInputMode.DragCanceled += EnableOrthogonalEdgeEditing;
      editMode.MoveUnselectedItemsInputMode.DragStarting += DisableOrthogonalEdgeEditing;
      editMode.MoveUnselectedItemsInputMode.DragFinished += EnableOrthogonalEdgeEditing;
      editMode.MoveUnselectedItemsInputMode.DragCanceled += EnableOrthogonalEdgeEditing;

      // Use a position handler that avoids overlapping, but only apply it to the moved node and
      // not to the children of groups.
      graph.GetDecorator().Nodes.PositionHandler.AddWrapperFactory((node, originalHandler) => {
        var shouldWrap =
            (!editMode.MoveUnselectedItemsInputMode.IsDragging && !editMode.MoveSelectedItemsInputMode.IsDragging) ||
            (editMode.MoveSelectedItemsInputMode.IsDragging && graphControl.Selection.Nodes.FirstOrDefault() == node);
        if (!shouldWrap || originalHandler == null) {
          return originalHandler;
        }
        // Wrap it in a GroupingNodePositionHandler that moves all child nodes but doesn't
        // update the parent group node bounds.
        var handler = new GroupingNodePositionHandler(node, originalHandler) { AdjustParentNodeLayout = false };
        // Wrap it in a NonOverlapPositionHandler that removes overlaps during and after the
        // move gesture.
        return new NonOverlapPositionHandler(node, handler);
      });

      // Use a reshape handler that avoids overlapping.
      graph.GetDecorator().Nodes.ReshapeHandler.AddWrapperFactory((node, originalHandler) => {
        if (originalHandler == null) {
          return null;
        }
        // Wrap it in a NonOverlapReshapeHandler that removes overlaps during and after the resize
        // gesture.
        return new NonOverlapReshapeHandler(node, originalHandler);
      });

      // Set a size constraint provider for a minimum node size for all nodes not already providing
      // their own one.
      var minimumSize = new SizeD(5, 5);
      graph.GetDecorator().Nodes.SizeConstraintProvider.AddWrapperFactory((_, provider) =>
          provider ?? new NodeSizeConstraintProvider(minimumSize, SizeD.Infinite));

      // Avoid overlapping when creating, pasting, or duplicating nodes.
      editMode.NodeCreated += async (_, args) => await MakeSpace(args.Item);
      graphControl.Clipboard.FromClipboardCopier.NodeCopied += async (_, args) => {
        // Clear the current selection before the layout starts because GraphEditorInputMode cannot
        // do this while the layout is running (the selection is usually cleared after this event).
        graphControl.Selection.Clear();
        await MakeSpace(args.Copy);
      };
      graphControl.Clipboard.DuplicateCopier.NodeCopied += async (_, args) => {
        // Clear the current selection before the layout starts because GraphEditorInputMode cannot
        // do this while the layout is running (the selection is usually cleared after this event).
        graphControl.Selection.Clear();
        await MakeSpace(args.Copy);
      };
    }

    /// <summary>
    ///   Makes space for a new node.
    /// </summary>
    private Task MakeSpace(INode node) => new LayoutHelper(graphControl, node).LayoutImmediatelyAsync();

    /// <summary>
    ///   Initializes styles and loads a sample graph.
    /// </summary>
    private void InitializeGraph() {
      DemoStyles.InitDemoStyles(graphControl.Graph, orthogonalEdgeEditing: true);
      LoadGraph("hierarchical");
    }

    /// <summary>
    ///   Loads the sample graph associated with the given name.
    /// </summary>
    private void LoadGraph(string sampleName) {
      var graph = graphControl.Graph;
      BuildGraphFromJson($"{sampleName}.json", graph);
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Registers the sample graph selector.
    /// </summary>
    private void InitializeUI() {
      AddOption(sampleGraphs, "Hierarchical", "hierarchical");
      AddOption(sampleGraphs, "Grouping", "grouping");
      AddOption(sampleGraphs, "Orthogonal", "orthogonal");
      AddOption(sampleGraphs, "Tree", "tree");
      AddOption(sampleGraphs, "Series-Parallel", "seriesparallel");
      sampleGraphs.SelectedValuePath = nameof(Tag);
      sampleGraphs.SelectedValue = "hierarchical";
      sampleGraphs.SelectionChanged += (_, _) => {
        if (sampleGraphs.SelectedValue is string sampleName) {
          LoadGraph(sampleName);
        }
      };
    }

    private static void AddOption(ComboBox comboBox, string label, string value) {
      comboBox.Items.Add(new ComboBoxItem {
          Content = label,
          Tag = value
      });
    }

    [STAThread]
    private static void Main(string[] args) {
      Run<NodeOverlapAvoidingDemo>();
    }
  }
}
