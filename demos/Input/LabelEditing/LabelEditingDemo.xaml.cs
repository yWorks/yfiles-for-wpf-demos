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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.LabelEditing
{
  /// <summary>
  ///   Shows customizations of the interactive label editing.
  /// </summary>
  [Demo("label-editing", "input", "Label Editing", Summary = "Shows customizations of the interactive label editing.",
      Tags = ["interaction", "label"], Keywords = ["texts", "validation"])]
  public sealed partial class LabelEditingDemo
  {
    /// <summary>The default pattern for label validation.</summary>
    private const string DefaultValidationPattern = @"^\w+@\w+\.\w+$";

    /// <summary>Precompiled Regex matcher from the validation pattern.</summary>
    private Regex validationPattern = new(DefaultValidationPattern);

    private readonly LabelEditingSettings settings = new();

    /// <summary>Wires up the settings panel and its change handlers.</summary>
    public LabelEditingDemo() {
      InitializeComponent();
      SettingsPanel.DataContext = settings;
      settings.PropertyChanged += (_, args) => {
        if (args.PropertyName == nameof(LabelEditingSettings.ValidationPattern)) {
          try {
            validationPattern = new Regex(settings.ValidationPattern);
          } catch (ArgumentException) {
            // invalid or unfinished regex, ignore
          }
        }
      };
      settings.SettingChanged += (_, _) => UpdateInputModeSettings((GraphEditorInputMode) GraphControl.InputMode!);
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      DemoStyles.InitDemoStyles(Graph);
      Graph.NodeDefaults.Size = new SizeD(100, 100);

      InitializeInputModes();
      CreateSampleGraph();
      GraphControl.FitGraphBounds();
    }

    /// <summary>Initializes the input modes for this demo.</summary>
    private void InitializeInputModes() {
      GraphControl.InputMode = CreateEditorMode();

      // Custom label helpers
      RegisterCustomEditLabelHelper();
    }

    /// <summary>
    ///   Creates the default input mode for the <see cref="GraphControl"/>, a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <remarks>
    ///   This implementation configures label text validation.
    /// </remarks>
    /// <returns>A new <see cref="GraphEditorInputMode"/> instance.</returns>
    private GraphEditorInputMode CreateEditorMode() {
      var graphEditorInputMode = new GraphEditorInputMode();

      // Configure label text validation
      // Note that by default, no visual feedback is provided, the text is just not changed
      graphEditorInputMode.EditLabelInputMode.ValidateLabelText += (_, args) => {
        // label must match the pattern
        args.Cancel = !IsValidText(args.NewText);
        // alternatively, ValidatedText also accepts asynchronous calls e.g.:
        if (false) {
          args.ValidatedText = ValidateTextAsync(args.NewText)!;
        }
      };
      graphEditorInputMode.EditLabelInputMode.TextEditorInputModeConfigurator = (_, mode, _) =>
          ConfigureTextEditorInputMode(mode);

      UpdateInputModeSettings(graphEditorInputMode);

      return graphEditorInputMode;
    }

    /// <summary>
    ///   Validates the new text.
    /// </summary>
    /// <param name="newText">The new text to validate.</param>
    private bool IsValidText(string newText) => !settings.ValidationEnabled || validationPattern.IsMatch(newText);

    /// <summary>
    ///   Validates the new text asynchronously and returns null, if the text is invalid.
    /// </summary>
    /// <param name="newText">The new text to validate.</param>
    private async Task<string?> ValidateTextAsync(string newText) {
      // simulate awaiting a database access for validation
      await FakeDatabaseAccess(2000);
      // finally validate the text
      return IsValidText(newText) ? newText : null;
    }

    /// <summary>Fakes an asynchronous database access using a timeout.</summary>
    /// <param name="time">The time to set the timeout to.</param>
    private static async Task FakeDatabaseAccess(int time) {
      await Task.Delay(time);
    }

    /// <summary>Registers custom label helpers for nodes and node labels.</summary>
    private void RegisterCustomEditLabelHelper() {
      var firstLabelStyle = new LabelStyle { TextBrush = Brushes.Firebrick };
      var decorator = Graph.GetDecorator();

      // Register the helper for both nodes and labels, but only when the global flag is set.
      // We can use more or less the same implementation for both items, so we just change the item to which the helper
      // is bound. The decorator on nodes is called when a label should be added or the label does not provide its own
      // label helper.
      decorator.Nodes.EditLabelHelper.AddFactory(_ => settings.CustomLabelHelper,
          node => new CustomEditLabelHelper(node, null, ExteriorNodeLabelModel.Top, firstLabelStyle));

      // The decorator on labels is called when a label is edited.
      decorator.Labels.EditLabelHelper.AddFactory(_ => settings.CustomLabelHelper,
          label => new CustomEditLabelHelper(null, label, ExteriorNodeLabelModel.Top, firstLabelStyle));
    }

    /// <summary>Applies the interaction-panel settings to the input mode.</summary>
    private void UpdateInputModeSettings(GraphEditorInputMode graphEditorInputMode) {
      graphEditorInputMode.AllowAddLabel = settings.LabelCreation;
      graphEditorInputMode.AllowEditLabel = settings.LabelEditing;
      graphEditorInputMode.AllowEditLabelOnTyping = settings.InstantTyping;
      graphEditorInputMode.EditLabelInputMode.HideLabelDuringEditing = settings.HideLabel;

      var editableItems = GraphItemTypes.None;
      if (settings.NodesEnabled) {
        editableItems |= GraphItemTypes.Node | GraphItemTypes.NodeLabel;
      }
      if (settings.EdgesEnabled) {
        editableItems |= GraphItemTypes.Edge | GraphItemTypes.EdgeLabel;
      }
      graphEditorInputMode.LabelEditableItems = editableItems;
    }

    /// <summary>Configures the editor and adds visual feedback for invalid label text.</summary>
    private void ConfigureTextEditorInputMode(TextEditorInputMode mode) {
      var textBox = mode.TextBox;

      TextBoxProperties.SetCustomHelper(textBox, false);

      RefreshLabelEditorFeedback(textBox);

      textBox.TextChanged -= OnTextBoxTextChanged;
      textBox.TextChanged += OnTextBoxTextChanged;
    }

    private void OnTextBoxTextChanged(object sender, TextChangedEventArgs args) {
      // Validate the text and apply the invalid background while the editor is open.
      RefreshLabelEditorFeedback((TextBox) sender);
    }

    private void RefreshLabelEditorFeedback(TextBox textBox) {
      TextBoxProperties.SetValidText(textBox, IsValidText(textBox.Text));
    }

    /// <summary>Creates the sample graph.</summary>
    private void CreateSampleGraph() {
      var graph = Graph;
      var n1 = graph.CreateNode(new PointD(0, 0));
      graph.AddLabel(n1, "Node Label");
      var n2 = graph.CreateNode(new PointD(250, 0));
      var edge = graph.CreateEdge(n1, n2);
      graph.AddLabel(edge, "Edge label");
    }

    [STAThread]
    public static void Main() {
      Run<LabelEditingDemo>();
    }
  }
}
