/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Input.LabelEditing
{
  /// <summary>Custom label edit helper.</summary>
  /// <remarks>
  ///   This class does the following:
  ///   <list type="bullet">
  ///     <item>Allow at most two labels</item>
  ///     <item>Disallow editing the first label</item>
  ///     <item>Use custom placement and style for the first label</item>
  ///     <item>Change the appearance of the <see cref="TextEditorInputMode"/></item>
  ///   </list>
  ///   For convenience, this implementation inherits from the predefined <see cref="EditLabelHelper"/> class.
  /// </remarks>
  public sealed class MyEditLabelHelper : EditLabelHelper
  {
    private readonly ILabel label;

    private readonly ILabelOwner owner;

    /// <summary>
    ///   Special parameter for the first label.
    /// </summary>
    private readonly ILabelModelParameter firstLabelParam;


    /// <summary>
    ///   Special style for the first label.
    /// </summary>
    private readonly ILabelStyle firstLabelStyle;

    public MyEditLabelHelper(
        ILabelOwner owner,
        ILabel label,
        ILabelModelParameter firstLabelParam,
        ILabelStyle firstLabelStyle) {
      this.owner = owner;
      this.label = label;
      this.firstLabelParam = firstLabelParam;
      this.firstLabelStyle = firstLabelStyle;
      TextBoxStyle = new Style(typeof(TextBox)) {
        Setters = {
          new Setter(Control.BackgroundProperty, Brushes.LightGray),
          new Setter(Control.ForegroundProperty, Brushes.DarkSlateGray),
          new Setter(Control.BorderBrushProperty, Brushes.LightGray),
        }
      };
    }

    /// <summary>
    ///   The background of the text box during editing.
    /// </summary>
    public Style TextBoxStyle { get; set; }


    /// <summary>
    ///   This method is only called when a label should be added to <see cref="owner"/>.
    /// </summary>
    /// <remarks>This implementation prevents adding more than two labels</remarks>
    protected override void OnLabelAdding(IInputModeContext context, LabelEditingEventArgs args) {
      base.OnLabelAdding(context, args);
      args.TextEditorInputModeConfigurator = ConfigureTextEditorInputMode;
      // Prevent adding more than two labels...
      if (owner.Labels.Count >= 2) {
        args.Cancel = true;
      }
    }

    /// <summary>
    ///   Provides the label style for newly created labels.
    /// </summary>
    /// <remarks>
    ///   This implementation returns the special style for the first if there are no labels yet, and the base style otherwise.
    /// </remarks>
    protected override ILabelStyle GetLabelStyle(IInputModeContext context, ILabelOwner owner) {
      return owner.Labels.Count == 0
          ? firstLabelStyle
          : context.GetGraph()!.GetLabelDefaults(owner).GetStyleInstance(owner);
    }

    /// <summary>
    ///   Provides the label model parameter for newly created labels.
    /// </summary>
    /// <remarks>
    ///   This implementation returns the special parameter for the first label if there are no labels yet, and the base label
    ///   model parameter otherwise.
    /// </remarks>
    protected override ILabelModelParameter GetLabelParameter(IInputModeContext context, ILabelOwner owner) {
      return owner.Labels.Count == 0
          ? firstLabelParam
          : context.GetGraph()!.GetLabelDefaults(owner).GetLayoutParameterInstance(owner);
    }

    /// <summary>This method is called when a label should be edited.</summary>
    /// <remarks>
    ///   If a label is edited directly, we either return it (if it is the second label) or prevent editing.
    /// </remarks>
    protected override void OnLabelEditing(IInputModeContext context, LabelEditingEventArgs args) {
      args.TextEditorInputModeConfigurator = ConfigureTextEditorInputMode;
      if (label != null) {
        // We are directly editing the label
        if (label == label.Owner.Labels[0]) {
          // The first label is never editable
          args.Label = null;
          args.Cancel = true;
          return;
        }

        // We have a label that is not part of the graph structure or not the first label.
        // Return the label and disallow editing.
        // If we are editing the first label, the framework will then try to add label by calling StartLabelAddition.
        args.Label = label;
        args.Handled = true;
        return;
      }

      if (owner.Labels.Count <= 1) {
        // Add a second label instead (since we'll never edit the first one)
        OnLabelAdding(context, args);
        return;
      }

      // If more than one label, edit the second one
      args.Label = owner.Labels[1];
      args.Handled = true;
    }

    /// <summary>
    ///   Customize the text editor when we are using our helper.
    /// </summary>
    private void ConfigureTextEditorInputMode(IInputModeContext context, TextEditorInputMode mode, ILabel labelToEdit) {
      var textBox = mode.TextBox;

      textBox.Style = TextBoxStyle;

      // Restore after editing
      EventHandler<TextEventArgs> afterEditing = null;
      afterEditing = delegate {
        textBox.ClearValue(FrameworkElement.StyleProperty);
        mode.TextEdited -= afterEditing;
        mode.EditingCanceled -= afterEditing;
      };
      mode.TextEdited += afterEditing;
      mode.EditingCanceled += afterEditing;
    }
  }
}
