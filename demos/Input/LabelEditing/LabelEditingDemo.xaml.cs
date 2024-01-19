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
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Toolkit;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.Input.LabelEditing
{
  /// <summary>Shows customizations of the interactive label editing.</summary>
  /// <remarks>
  /// It shows the following features:
  /// <list type="bullet">
  /// <item><see cref="GraphEditorInputMode" /> properties</item>
  /// <item>Input validation</item>
  /// <item>Instant typing</item>
  /// <item>Custom <see cref="IEditLabelHelper" /></item>
  /// <item>Dummy label creation and editing</item>
  /// </list>
  /// </remarks>
  public partial class LabelEditingDemo
  {
    /// <summary>
    /// The default pattern for label validation.
    /// </summary>
    private const string DefaultValidationPattern = @"^\w+@\w+\.\w+$";

    /// <summary>
    /// Whether label text validation is used at all.
    /// </summary>
    private bool validationEnabled;

    /// <summary>
    /// Precompiled <see cref="Regex"/> matcher from the validation pattern.
    /// </summary>
    private Regex validationPattern = new Regex(DefaultValidationPattern, RegexOptions.Compiled);

    /// <summary>
    /// Whether to use custom label helpers
    /// </summary>
    private bool customHelperEnabled;

    /// <summary>
    /// Whether to automatically start editing the label on key press.
    /// </summary>
    private bool instantTypingEnabled;

    private DemoTextEditorInputMode textEditorInputMode;

    /// <summary>
    /// Simple label for the page header.
    /// </summary>
    private SimpleLabel pageHeader;

    /// <summary>
    /// Initializes the input modes.
    /// </summary> 
    /// <remarks>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </remarks>
    protected virtual void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();

      // Custom label helpers
      RegisterCustomEditLabelHelper();

      // Register custom event handler for the instant typing feater
      GraphControl.PreviewTextInput += HandleInstantTyping;

      // Create a dummy label for the page header
      pageHeader = PageHeaderSupport.CreatePageHeader(GraphControl);
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <remarks>
    /// This implementation configures label text validation
    /// </remarks>
    /// <returns>A new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      var graphEditorInputMode = new GraphEditorInputMode();

      //Configure label text validation
      // Note that by default, no visual feedback is provided, the text is just not changed
      graphEditorInputMode.ValidateLabelText += delegate(object o, LabelTextValidatingEventArgs args) {
        if (args.Label == pageHeader) {
          // Page header may not be empty, regardless of whether validation is enabled or not
          args.Cancel = args.NewText == String.Empty;
        } else {
          // label must match the pattern
          args.Cancel = validationEnabled && !validationPattern.IsMatch(args.NewText);
        }
      };

      // The label size for the dummy label must be updated externally
      graphEditorInputMode.LabelTextChanged += delegate(object o, LabelEventArgs args) {
        if (args.Item == pageHeader) {
          pageHeader.AdoptPreferredSizeFromStyle();
        }
      };

      // Instant typing requires a custom TextEditorInputMode - it's safe to use it globally, though
      graphEditorInputMode.TextEditorInputMode = textEditorInputMode = new DemoTextEditorInputMode();

      return graphEditorInputMode;
    }

    /// <summary>
    /// Register custom label helpers for nodes and node labels
    /// </summary>
    private void RegisterCustomEditLabelHelper() {
      var firstLabelStyle = new DefaultLabelStyle { TextBrush = Brushes.Firebrick };

      // Register the helper for both nodes and edges, but only when the global flag ist set
      // We can use more or less the same implementation for both items, so we just change the item to which the helper is bound
      // The decorator on nodes is called when a label should be added or the label does not provide its own label helper
      GraphControl.Graph.GetDecorator().NodeDecorator.EditLabelHelperDecorator.SetFactory(
          node => customHelperEnabled,
          node => new MyEditLabelHelper(node, null, ExteriorLabelModel.North, firstLabelStyle));
      // The decorator on labels is called when a label is edited
      GraphControl.Graph.GetDecorator().LabelDecorator.EditLabelHelperDecorator.SetFactory(
          label => customHelperEnabled,
          label => new MyEditLabelHelper(null, label, ExteriorLabelModel.North, firstLabelStyle));
    }

    #region Instant typing feature

    /// <summary>
    /// Event handler that implements "instant typing"
    /// </summary>
    private void HandleInstantTyping(object sender, System.Windows.Input.TextCompositionEventArgs e) {
      if (!instantTypingEnabled) {
        return;
      }


      // if nothing is selected, we try using the "current item" of the GraphControl, instead
      var parameter = GraphControl.Selection.Count == 0 ? GraphControl.CurrentItem : null;


      if (!GraphCommands.EditLabel.CanExecute(parameter, GraphControl) && !GraphCommands.AddLabel.CanExecute(parameter, GraphControl)) {
        return;
      }

      // If the command could be executed and instant typing is enabled
      bool oldSelectionState = textEditorInputMode.SelectText;
      EventHandler<TextEventArgs> editingStarted = delegate {
        // Disable automatic text selection temporarily
        textEditorInputMode.SelectText = false;
        // Deregister the handler during the event processing
        // Not strictly needed, since TextEditing has the mutex, anyway
        GraphControl.PreviewTextInput -= HandleInstantTyping;
      };
      // Register ourself on the text editing begin
      textEditorInputMode.EditingStarted += editingStarted;

      // Undo our changes when editing is finished or canceled
      EventHandler<TextEventArgs> editingCanceled = null;
      editingCanceled = delegate {
        // Undo the changes done in the started handler
        textEditorInputMode.EditingStarted -= editingStarted;
        // ReSharper disable AccessToModifiedClosure
        textEditorInputMode.EditingCanceled -= editingCanceled;
        textEditorInputMode.TextEdited -= editingCanceled;
        // ReSharper restore AccessToModifiedClosure
        GraphControl.PreviewTextInput += HandleInstantTyping;
        textEditorInputMode.SelectText = oldSelectionState;
      };
      textEditorInputMode.EditingCanceled += editingCanceled;
      textEditorInputMode.TextEdited += editingCanceled;

      // Now just raise the command
      if (HasLabel()) {
        GraphCommands.EditLabel.Execute(parameter, GraphControl);
      } else {
        GraphCommands.AddLabel.Execute(parameter, GraphControl);
      }
    }
    /// <summary>
    /// Determines whether a label needs to be edited or added, for the purpose of demonstrating
    /// instant typing.
    /// </summary>
    /// <returns>Whether the current Item or selection contains a label</returns>
    private bool HasLabel() {
      if (GraphControl.Selection.Any(item => {
        if (item is ILabelOwner labeledItem) {
          return labeledItem.Labels.Any();
        }
        return false;
      })) {
        return true;
      }
      if (GraphControl.CurrentItem is ILabelOwner current) {
        return current.Labels.Any();
      }
      return false;
    }

    /// <summary>
    /// Custom TextEditorInputMode for instant typing.
    /// </summary>
    /// <remarks>
    /// This implementation allows to disable automatic text selection in the text editor. This is needed for instant typing
    /// because otherwise the first typed letter will be selected and subsequently overwritten.
    /// </remarks>
    public class DemoTextEditorInputMode : TextEditorInputMode
    {
      public DemoTextEditorInputMode() {
        SelectText = true;
      }

      public bool SelectText { get; set; }

      protected override void InstallTextBox() {
        base.InstallTextBox();
        if (SelectText) {
          return;
        }
        RoutedEventHandler handler = null;
        var canvas = InputModeContext.CanvasControl;
        handler = delegate {
          if (canvas != null) {
            // ReSharper disable AccessToModifiedClosure
            canvas.UpdatedVisual -= handler;
            // ReSharper restore AccessToModifiedClosure
          }
          // By default, our text editing mode selects the content of the textbox. This code is undoing this
          // so it works better with the "instant type" gesture.
          TextBox.CaretIndex = TextBox.Text.Length;
        };
        if (canvas != null) {
          canvas.UpdatedVisual += handler;
        }
      }

      #region Overrides of TextEditorInputMode

      protected override void EnsureVisible() {
        if (!ShowInViewCoordinatesProperty) {
          //If we set this property 
          base.EnsureVisible();
        }
      }

      #endregion

      /// <summary>
      /// Overridden to delegate to the new property <see cref="ShowInViewCoordinatesProperty"/>.
      /// </summary>
      protected override bool ShowInViewCoordinates(SizeD textBoxSize) {
        return ShowInViewCoordinatesProperty;
      }

      /// <summary>
      /// Determines whether the text box should be displayed in the view coordinate system instead
      /// of in the world coordinate system.
      /// </summary>
      public bool ShowInViewCoordinatesProperty { get; set; }
    }

    #endregion

    /// <summary>
    /// starts editing of the page header.
    /// </summary>
    private void EditPageHeader(object sender, RoutedEventArgs e) {
      var graphEditorInputMode = ((GraphEditorInputMode) GraphControl.InputMode);
      graphEditorInputMode.EditLabel(pageHeader);
    }

    #region Option handler

    /// <summary>
    /// Initializes the option handler for the label editing properties.
    /// </summary>
    private void InitializeOptionHandler() {
      var optionHandler = CreateOptionHandler();
      editorControl.OptionHandler = optionHandler;
      editorControl.IsAutoAdopt = true;
      editorControl.IsAutoCommit = true;
    }

    /// <summary>
    /// Creates the option handler for the label editing properties.
    /// </summary>
    /// <remarks>
    /// These options either delegate directly to properties of <see cref="GraphEditorInputMode" /> or set some global flag
    /// that is evaluated elsewhere.
    /// </remarks>
    private OptionHandler CreateOptionHandler() {
      var graphEditorInputMode = ((GraphEditorInputMode) GraphControl.InputMode);

      var handler = new OptionHandler("Labeling Options");

      OptionGroup currentGroup = handler.AddGroup("General");
      var labelAddItem = currentGroup.AddBool("Label Creation", true);
      labelAddItem.PropertyChanged += delegate { graphEditorInputMode.AllowAddLabel = (bool) labelAddItem.Value; };

      var labelEditItem = currentGroup.AddBool("Label Editing", true);
      labelEditItem.PropertyChanged +=
          delegate { graphEditorInputMode.AllowEditLabel = (bool) labelEditItem.Value; };

      var hideItem = currentGroup.AddBool("Hide Label during Editing", true);
      hideItem.PropertyChanged += delegate { 
        graphEditorInputMode.HideLabelDuringEditing = (bool) hideItem.Value;
      };

      var instantTypingItem = currentGroup.AddBool("Instant Typing", false);
      instantTypingItem.PropertyChanged += delegate { instantTypingEnabled = (bool) instantTypingItem.Value; };

      var useCustomHelperItem = currentGroup.AddBool("Custom Label Helper", false);
      useCustomHelperItem.PropertyChanged += delegate { customHelperEnabled = (bool) useCustomHelperItem.Value; };

      currentGroup = handler.AddGroup("Editable Items");

      // Disable the whole editable items group if neither label editing or adding allowed
      ConstraintManager cm = new ConstraintManager(handler);
      cm.SetEnabledOnCondition(
          ConstraintManager.LogicalCondition.Or(cm.CreateValueEqualsCondition(labelEditItem, true),
              cm.CreateValueEqualsCondition(labelAddItem, true)), currentGroup);

      currentGroup.AddBool("Nodes", true).PropertyChanged += delegate {
        var editNodes = (bool) handler.GetValue("Editable Items", "Nodes");
        if (editNodes) {
          graphEditorInputMode.LabelEditableItems |= GraphItemTypes.Node | GraphItemTypes.NodeLabel;
        } else {
          graphEditorInputMode.LabelEditableItems &= ~(GraphItemTypes.Node | GraphItemTypes.NodeLabel);
        }
      };

      currentGroup.AddBool("Edges", true).PropertyChanged += delegate {
        var editEdges = (bool) handler.GetValue("Editable Items", "Edges");
        if (editEdges) {
          graphEditorInputMode.LabelEditableItems |= GraphItemTypes.Edge | GraphItemTypes.EdgeLabel;
        } else {
          graphEditorInputMode.LabelEditableItems &= ~(GraphItemTypes.Edge | GraphItemTypes.EdgeLabel);
        }
      };

      currentGroup = handler.AddGroup("Validation");
      var validationItem = currentGroup.AddBool("Enable Validation", false);
      validationItem.PropertyChanged += delegate { validationEnabled = (bool) validationItem.Value; };
      var patternItem = currentGroup.AddString("Pattern", DefaultValidationPattern);
      patternItem.PropertyChanged += delegate { validationPattern = new Regex((string) patternItem.Value, RegexOptions.Compiled); };

      // Editing the pattern doesn't make sense if validation is disabled
      cm.SetEnabledOnValueEquals(validationItem, true, patternItem);

      return handler;
    }

    #endregion

    #region Standard demo code

    /// <summary>
    /// Automatically generated by Visual Studio. Wires up the UI components and adds a
    /// <see cref="GraphControl" /> to the form.
    /// </summary>
    public LabelEditingDemo() {
      InitializeComponent();
    }

    /// <summary>Initializes the graph and the input mode.</summary>
    /// <seealso cref="InitializeInputModes" />
    /// <seealso cref="InitializeGraph" />
    protected virtual void OnLoaded(object source, EventArgs e) {
      // initialize the graph
      InitializeGraph();
      // initialize the input mode
      InitializeInputModes();
      // Setup the option handler
      InitializeOptionHandler();
    }

    /// <summary>
    /// Initializes the graph instance setting default styles and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      var graph = graphControl.Graph;

      // Enable undoability
      graph.SetUndoEngineEnabled(true);

      // Set the default node style
      DemoStyles.InitDemoStyles(graph);
      graph.NodeDefaults.Size = new SizeD(100, 100);
    }

    /// <summary>Callback action that is triggered when the user exits the application.</summary>
    protected virtual void ExitAction(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    /// <summary>
    /// Returns the GraphControl instance used in this demo.
    /// </summary>
    public GraphControl GraphControl {
      get { return graphControl; }
    }

    #endregion
  }
}
