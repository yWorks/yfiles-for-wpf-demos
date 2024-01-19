/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Partial;

namespace Demo.yFiles.Graph.FillAreaAfterDeletion
{
  /// <summary>
  /// A demo that shows how to fill free space after deleting nodes.
  /// </summary>
  public partial class FillAreaAfterDeletionDemo
  {
    /// <summary>
    /// Layouts the graph after some nodes have been deleted. 
    /// </summary>
    private LayoutExecutor executor;

    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    private GraphControl GraphControl {
      get { return graphControl; }
    }

    #region Initialization

    /// <summary>
    /// Wires up the UI components and initializes the layout options.
    /// </summary>
    public FillAreaAfterDeletionDemo() {
      InitializeComponent();

      SampleGraphComboBox.ItemsSource = new[] {
          new NamedEntry("Hierarchic", "hierarchic"),
          new NamedEntry("Grouping", "grouping"),
          new NamedEntry("Organic", "organic"),
          new NamedEntry("Orthogonal", "orthogonal"),
          new NamedEntry("Circular", "circular"),
          new NamedEntry("Tree", "tree"),
          new NamedEntry("Balloon", "balloon"),
          new NamedEntry("Series-Parallel", "seriesparallel"),
          new NamedEntry("Components", "components"),
      };

      ComponentAssignmentStrategyComboBox.ItemsSource = new[] {
          new NamedEntry("Single", ComponentAssignmentStrategy.Single),
          new NamedEntry("Connected", ComponentAssignmentStrategy.Connected),
          new NamedEntry("Clustering", ComponentAssignmentStrategy.Clustering),
      };
    }

    /// <summary>
    /// Initializes the graph and the input mode.
    /// </summary>
    private void OnLoad(object sender, EventArgs e) {
      InitializeInputModes();
      InitializeGraph();

      SampleGraphComboBox.SelectedIndex = 0;
      ComponentAssignmentStrategyComboBox.SelectedIndex = 0;
    }

    /// <summary>
    /// Initializes the <see cref="GraphEditorInputMode"/> as the <see cref="CanvasControl.InputMode"/>
    /// and registers handlers which are called when selected nodes are deleted.
    /// </summary>
    private void InitializeInputModes() {
      // create a GraphEditorInputMode instance and install the edit mode into the canvas.
      var editMode = new GraphEditorInputMode();
      GraphControl.InputMode = editMode;

      // registers handlers which are called when selected nodes are deleted
      editMode.DeletingSelection += OnDeletingSelection;
      editMode.DeletedSelection += OnDeletedSelection;
    }

    /// <summary>
    /// Prepares the <see cref="LayoutExecutor"/> that is called after the selection is deleted.
    /// </summary>
    private void OnDeletingSelection(object sender, SelectionEventArgs<IModelItem> e) {
      // configure the layout that will fill free space
      var layout = new FillAreaLayout {
          ComponentAssignmentStrategy = ComponentAssignmentStrategy,
          Area = GetBounds(e.Selection).ToYRectangle()
      };

      // configure the LayoutExecutor that will perform the layout and morph the result
      executor = new LayoutExecutor(GraphControl, layout) {
          Duration = TimeSpan.FromMilliseconds(100)
      };
    }

    /// <summary>
    /// Calls the prepared <see cref="LayoutExecutor"/>.
    /// </summary>
    private void OnDeletedSelection(object sender, SelectionEventArgs<IModelItem> e) {
      executor.Start();
    }

    /// <summary>
    /// The bounds including the nodes of the selection.
    /// </summary>
    public RectD GetBounds(ISelectionModel<IModelItem> selection) {
      var bounds = RectD.Empty;
      foreach (var item in selection) {
        var node = item as INode;
        if (node != null) {
          bounds += node.Layout.ToRectD();
        } else {
          var edge = item as IEdge;
          if (edge != null) {
            if (edge.SourcePort != null) {
              bounds += edge.SourcePort.GetLocation();
            }
            if (edge.TargetPort != null) {
              bounds += edge.TargetPort.GetLocation();
            }
          }
        }
      }
      return bounds;
    }

    /// <summary>
    /// Initializes styles and loads a sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      DemoStyles.InitDemoStyles(GraphControl.Graph);
    }

    #endregion

    #region User interface event handling

    /// <summary>
    /// Gets the <see cref="ComponentAssignmentStrategy"/> selected by the user.
    /// </summary>
    private ComponentAssignmentStrategy ComponentAssignmentStrategy {
      get {
        return (ComponentAssignmentStrategy) ((NamedEntry) ComponentAssignmentStrategyComboBox.SelectedItem).Value;
      }
    }

    /// <summary>
    /// Loads a new sample graph.
    /// </summary>
    private void SampleGraphChanged(object sender, SelectionChangedEventArgs e) {
      var selectedItem = SampleGraphComboBox.SelectedItem as NamedEntry;
      if (selectedItem != null) {
        GraphControl.ImportFromGraphML("Resources\\" + selectedItem.Value + ".graphml");
        CheckSampleButtonStates();
      }
    }

    /// <summary>
    /// Loads the previous sample graph.
    /// </summary>
    private void LoadPreviousSampleGraph(object sender, RoutedEventArgs e) {
      if (SampleGraphComboBox.SelectedIndex <= 0) {
        return;
      }
      SampleGraphComboBox.SelectedIndex--;
    }

    /// <summary>
    /// Loads the next sample graph.
    /// </summary>
    private void LoadNextSampleGraph(object sender, RoutedEventArgs e) {
      if (SampleGraphComboBox.SelectedIndex >= SampleGraphComboBox.Items.Count - 1) {
        return;
      }
      SampleGraphComboBox.SelectedIndex++;
    }

    /// <summary>
    /// Updates the enabled state of the next and previous buttons.
    /// </summary>
    private void CheckSampleButtonStates() {
      var maxReached = SampleGraphComboBox.SelectedIndex == SampleGraphComboBox.Items.Count - 1;
      var minReached = SampleGraphComboBox.SelectedIndex == 0;
      NextSample.IsEnabled = !maxReached;
      PreviousSample.IsEnabled = !minReached;
    }

    #endregion

    /// <summary>
    /// Name-value struct for combo box entries.
    /// </summary>
    private sealed class NamedEntry
    {
      public object Value { get; private set; }
      private string DisplayName { get; set; }

      public NamedEntry(string displayName, object value) {
        DisplayName = displayName;
        Value = value;
      }

      public override string ToString() {
        return DisplayName;
      }
    }
  }
}
