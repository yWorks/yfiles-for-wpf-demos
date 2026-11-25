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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Clipboard
{
  /// <summary>
  ///   Interaction logic for ClipboardDemo.xaml
  /// </summary>
  [Demo("clipboard", "view", "Clipboard",
      Summary = "Shows different ways of using the class GraphClipboard for Copy & Paste operations.",
      Tags = ["interaction", "copy", "paste"], Keywords = ["labels"])]
  public sealed partial class ClipboardDemo
  {
    private GraphEditorInputMode inputMode = null!;
    private GraphEditorInputMode inputMode2 = null!;

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override void Initialize() {
      // Initialize the graph components. Both components share the clipboard.
      graphControl2.Clipboard = graphControl.Clipboard;

      var nodeStyle = new ShapeNodeStyle {
          Shape = ShapeNodeShape.RoundRectangle,
          Brush = new SolidColorBrush(Color.FromRgb(193, 225, 241)),
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(97, 121, 132)), 1.5)
      };
      InitializeGraphStyling(graphControl.Graph, nodeStyle);
      InitializeGraphStyling(graphControl2.Graph, nodeStyle);

      // Create nodes and an edge.
      CreateSampleGraph(graphControl.Graph);
      // Enable the Undo functionality.
      graphControl.Graph.SetUndoEngineEnabled(true);
      graphControl2.Graph.SetUndoEngineEnabled(true);

      // Create the input modes.
      inputMode = CreateInputMode(graphControl);
      inputMode2 = CreateInputMode(graphControl2);
      graphControl.InputMode = inputMode;
      graphControl2.InputMode = inputMode2;

      InitializeUi();

      graphControl.FitGraphBounds();
      graphControl2.FitGraphBounds();

      // Set the focus to the left GraphControl.
      graphControl.Focus();
    }

    /// <summary>
    ///   Initializes the graph defaults and clipboard helper.
    /// </summary>
    private static void InitializeGraphStyling(IGraph graph, INodeStyle nodeStyle) {
      graph.NodeDefaults.Style = nodeStyle;
      graph.NodeDefaults.Size = new SizeD(120, 60);
      graph.NodeDefaults.Labels.LayoutParameter = ExteriorNodeLabelModel.Top;
      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(Themes.Palette31);
      graph.EdgeDefaults.Labels.LayoutParameter = NinePositionsEdgeLabelModel.CenterAbove;

      graph.NodeTagChanged += (_, args) => {
        if (args.Item.Tag is ClipboardBusinessObject businessObject &&
            args.Item.Labels.FirstOrDefault() is { } nameLabel) {
          graph.SetLabelText(nameLabel, businessObject.Name);
        }
      };

      graph.GetDecorator().Nodes.ClipboardHelper.AddConstant(new TaggedNodeClipboardHelper());
    }

    /// <summary>
    ///   Creates the GraphEditorInputMode for this demo.
    /// </summary>
    private GraphEditorInputMode CreateInputMode(GraphControl component) {
      var inputMode = new GraphEditorInputMode { AllowClipboardOperations = true };

      // For each new node, create a node label and a business object automatically.
      inputMode.NodeCreated += (_, args) => {
        var node = args.Item;
        node.Tag = BusinessDataHandling.CreateNodeBusinessData();
        AddBusinessObjectLabel(component.Graph, node);
        component.Graph.AddLabel(node, $"Label {component.Graph.Nodes.Count}");
      };

      // Discard the first click after getting the focus
      // to prevent node creation when switching between the GraphControls.
      inputMode.ClickInputMode.SwallowFocusClick = true;
      return inputMode;
    }

    /// <summary>
    ///   Wires up the UI.
    /// </summary>
    private void InitializeUi() {
      var clipboard = graphControl.Clipboard;
      clipboard.ItemsCopied += (_, _) => EnablePasteSpecialButtons();
      clipboard.ItemsCut += (_, _) => EnablePasteSpecialButtons();

      inputMode.MultiSelectionFinished += (_, _) => {
        editNameButton.IsEnabled = graphControl.Selection.Nodes.Count > 0;
      };
      inputMode.DeletedSelection += (_, _) => editNameButton.IsEnabled = false;
      inputMode2.MultiSelectionFinished += (_, _) => {
        editNameButton2.IsEnabled = graphControl2.Selection.Nodes.Count > 0;
      };
      inputMode2.DeletedSelection += (_, _) => editNameButton2.IsEnabled = false;
    }

    private void EnablePasteSpecialButtons() {
      pasteSpecialButton.IsEnabled = true;
      pasteSpecialButton2.IsEnabled = true;
    }

    /// <summary>
    ///   Executes Paste Special, which pastes only nodes and labels.
    /// </summary>
    private static void OnPasteSpecialCommand(GraphControl component) {
      var clipboard = component.Clipboard;
      component.Selection.Clear();

      // This is the filter for the Paste call.
      var clipboardGraph = clipboard.ClipboardGraph;
      var itemsToPaste = clipboardGraph.Nodes.Cast<IModelItem>().Concat(clipboardGraph.Labels);
      // This callback is executed for every pasted element. We use it to select the pasted nodes.
      clipboard.Paste(component.Graph, itemsToPaste, (originalItem, pastedItem) => {
        if (pastedItem is INode node) {
          component.Selection.Add(node);
        }
      });

      // Set the different paste delta.
      clipboard.PasteOffset += new PointD(30, 30);
    }

    private void OnPasteSpecial(object sender, RoutedEventArgs e) => OnPasteSpecialCommand(graphControl);

    private void OnPasteSpecial2(object sender, RoutedEventArgs e) => OnPasteSpecialCommand(graphControl2);

    /// <summary>
    ///   Shows a dialog for editing the node name.
    /// </summary>
    private void OnEditName(object sender, RoutedEventArgs e) =>
        ShowEditNameDialog(graphControl, nameDialog, nodeNameInput);

    private void OnEditName2(object sender, RoutedEventArgs e) =>
        ShowEditNameDialog(graphControl2, nameDialog2, nodeNameInput2);

    private static void ShowEditNameDialog(GraphControl component, Popup dialog, TextBox nodeNameInput) {
      nodeNameInput.Text = BusinessDataHandling.GetCommonName(component.Selection.Nodes);
      dialog.IsOpen = true;
      nodeNameInput.Focus();
      nodeNameInput.SelectAll();
    }

    private void OnApplyName(object sender, RoutedEventArgs e) => ApplyName(graphControl, nameDialog, nodeNameInput);

    private void OnApplyName2(object sender, RoutedEventArgs e) =>
        ApplyName(graphControl2, nameDialog2, nodeNameInput2);

    private void OnCancelName(object sender, RoutedEventArgs e) => nameDialog.IsOpen = false;

    private void OnCancelName2(object sender, RoutedEventArgs e) => nameDialog2.IsOpen = false;

    private void ApplyName(GraphControl component, Popup dialog, TextBox nodeNameInput) {
      var name = nodeNameInput.Text;
      foreach (var node in component.Selection.Nodes) {
        if (node.Tag is ClipboardBusinessObject) {
          // Replace the business object on each selected node.
          node.Tag = new ClipboardBusinessObject(name);
        }
      }

      dialog.IsOpen = false;
      component.Focus();
    }

    /// <summary>
    ///   Creates the sample graph.
    /// </summary>
    private static void CreateSampleGraph(IGraph graph) {
      var sharedBusinessObject = BusinessDataHandling.CreateNodeBusinessData();

      var node1 = graph.CreateNode(new RectD(40, 70, 120, 60));
      node1.Tag = sharedBusinessObject;
      AddBusinessObjectLabel(graph, node1);
      graph.AddLabel(node1, "Label 1");

      var node2 = graph.CreateNode(new RectD(290, 70, 120, 60));
      node2.Tag = sharedBusinessObject;
      AddBusinessObjectLabel(graph, node2);
      graph.AddLabel(node2, "Label 2");

      var node3 = graph.CreateNode(new RectD(40, 170, 120, 60));
      node3.Tag = BusinessDataHandling.CreateNodeBusinessData();
      AddBusinessObjectLabel(graph, node3);
      graph.AddLabel(node3, "Label 3");

      graph.AddLabel(graph.CreateEdge(node1, node2), "Shared Object");
    }

    private static void AddBusinessObjectLabel(IGraph graph, INode node) {
      var name = (node.Tag as ClipboardBusinessObject)?.Name ?? string.Empty;
      graph.AddLabel(node, name, InteriorNodeLabelModel.Center);
    }

    [STAThread]
    private static void Main(string[] args) {
      Run<ClipboardDemo>();
    }

    public ClipboardDemo() {
      InitializeComponent();
    }
  }
}
