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
using System.Text.Json;
using System.Windows.Controls;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout.Partial;
using yWorks.Utils;

namespace yWorks.Demo.FillAreaLayout
{
  [Demo("fill-area-layout", "layout", "Fill Area Layout",
      Summary = "Shows how to fill free space after deleting nodes.", Tags = ["layout", "interactive layout"],
      Keywords = ["layoutfeatures", "deletion", "adjustment", "interactive"], RelatedDemos = [])]
  public sealed partial class FillAreaLayoutDemo
  {
    /// <summary>
    ///   Stores the bounding rectangle of the selected nodes that get deleted.
    /// </summary>
    private RectD? selectionRect;

    /// <summary>
    ///   Stores the current component assignment strategy.
    /// </summary>
    private ComponentAssignmentStrategy componentAssignmentStrategy = ComponentAssignmentStrategy.Single;

    protected override void Initialize() {
      InitializeInputModes();

      DemoStyles.InitDemoStyles(graphControl.Graph, orthogonalEdgeEditing: true);
      LoadGraph("hierarchical");

      // Enable undo/redo support.
      graphControl.Graph.SetUndoEngineEnabled(true);

      // Bind the controls to their actions.
      InitializeUI();
    }

    /// <summary>
    ///   Initializes the <see cref="GraphEditorInputMode"/> as the <see cref="GraphControl.InputMode"/>
    ///   and registers handlers which are called when selected nodes are deleted.
    /// </summary>
    private void InitializeInputModes() {
      // create a GraphEditorInputMode instance and install the edit mode into the canvas.
      var editMode = new GraphEditorInputMode();
      editMode.CreateEdgeInputMode.OrthogonalEdgeCreation = OrthogonalEdgeEditingPolicy.Always;
      editMode.OrthogonalBendRemoval = OrthogonalEdgeEditingPolicy.Always;
      graphControl.InputMode = editMode;

      // registers handlers which are called when selected nodes are deleted
      editMode.DeletingSelection += OnDeletingSelection;
      editMode.DeletedSelection += OnDeletedSelection;
    }

    /// <summary>
    ///   Prepares the <see cref="FillAreaLayout"/> that is called after the selection is deleted.
    /// </summary>
    private void OnDeletingSelection(object? sender, SelectionEventArgs<IModelItem> e) {
      // determine the bounds of the selection
      selectionRect = GetBounds(e.Selection);
    }

    /// <summary>
    ///   Calls the prepared <see cref="FillAreaLayout"/>.
    /// </summary>
    private async void OnDeletedSelection(object? sender, ItemsEventArgs e) {
      // configure the layout that will fill free space
      var layout = new Layout.Partial.FillAreaLayout {
          ComponentAssignmentStrategy = componentAssignmentStrategy,
          Area = selectionRect ?? RectD.Empty
      };

      // configure the GraphControl that will perform the layout and morph the result
      await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(150));

      selectionRect = null;
    }

    /// <summary>
    ///   The bounds including the nodes of the selection.
    /// </summary>
    private static RectD GetBounds(IObservableCollection<IModelItem> selection) {
      var bounds = RectD.Empty;
      foreach (var item in selection) {
        if (item is INode node) {
          bounds = RectD.Add(bounds, node.Layout.ToRectD());
        } else if (item is IEdge edge) {
          bounds = RectD.Add(bounds, edge.SourcePort.GetLocation());
          foreach (var bend in edge.Bends) {
            bounds = RectD.Add(bounds, bend.Location.ToPointD());
          }
          bounds = RectD.Add(bounds, edge.TargetPort.GetLocation());
        } else if (item is IBend bend) {
          bounds = RectD.Add(bounds, new RectD(bend.Location.X, bend.Location.Y, 1, 1));
        }
      }
      return bounds;
    }

    /// <summary>
    ///   Loads the sample graph associated with the given name.
    /// </summary>
    private void LoadGraph(string sampleName) {
      using var stream = LoadAssemblyResource("SampleData.json");
      using var jsonDocument = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graphControl.Graph, jsonDocument.RootElement.GetProperty(sampleName));

      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Registers commands and actions for the items in the toolbar.
    /// </summary>
    private void InitializeUI() {
      AddOption(sampleGraphsComboBox, "Hierarchical", "hierarchical");
      AddOption(sampleGraphsComboBox, "Grouping", "grouping");
      AddOption(sampleGraphsComboBox, "Organic", "organic");
      AddOption(sampleGraphsComboBox, "Orthogonal", "orthogonal");
      AddOption(sampleGraphsComboBox, "Circular", "circular");
      AddOption(sampleGraphsComboBox, "Tree", "tree");
      AddOption(sampleGraphsComboBox, "Radial Tree", "radialTree");
      AddOption(sampleGraphsComboBox, "Series-Parallel", "seriesParallel");
      AddOption(sampleGraphsComboBox, "Components", "components");
      sampleGraphsComboBox.SelectedValuePath = nameof(Tag);
      sampleGraphsComboBox.SelectedValue = "hierarchical";
      sampleGraphsComboBox.SelectionChanged += (_, _) => {
        if (sampleGraphsComboBox.SelectedValue is string selectedSample) {
          LoadGraph(selectedSample);
        }
      };

      AddOption(componentAssignmentStrategiesComboBox, "Single", ComponentAssignmentStrategy.Single);
      AddOption(componentAssignmentStrategiesComboBox, "Connected", ComponentAssignmentStrategy.Connected);
      AddOption(componentAssignmentStrategiesComboBox, "Clustering", ComponentAssignmentStrategy.Clustering);
      componentAssignmentStrategiesComboBox.SelectedValuePath = nameof(Tag);
      componentAssignmentStrategiesComboBox.SelectedValue = componentAssignmentStrategy;
      componentAssignmentStrategiesComboBox.SelectionChanged += (_, _) => {
        if (componentAssignmentStrategiesComboBox.SelectedValue is ComponentAssignmentStrategy selectedStrategy) {
          componentAssignmentStrategy = selectedStrategy;
        }
      };
    }

    private static void AddOption<TValue>(ComboBox comboBox, string label, TValue value) {
      comboBox.Items.Add(new ComboBoxItem {
          Content = label,
          Tag = value
      });
    }

    [STAThread]
    private static void Main() => Run<FillAreaLayoutDemo>();

    public FillAreaLayoutDemo() {
      InitializeComponent();
    }
  }
}
