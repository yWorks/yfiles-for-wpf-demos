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
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.LabelEditing
{
  /// <summary>
  ///   Custom label edit helper.
  ///   This class does the following:
  ///   - Allow at most two labels
  ///   - Disallow editing the first label
  ///   - Use custom placement and style for the first label
  ///   - Change the appearance of the <see cref="TextEditorInputMode"/>.
  ///   For convenience, this implementation inherits from the predefined <see cref="EditLabelHelper"/> class.
  /// </summary>
  public sealed class CustomEditLabelHelper : EditLabelHelper
  {
    private readonly ILabelStyle firstLabelStyle;
    private readonly ILabelModelParameter firstLabelParam;
    private readonly ILabel? label;
    private readonly ILabelOwner? owner;

    /// <summary>
    ///   Creates an instance of <see cref="CustomEditLabelHelper"/>.
    /// </summary>
    public CustomEditLabelHelper(
        ILabelOwner? owner,
        ILabel? label,
        ILabelModelParameter firstLabelParam,
        ILabelStyle firstLabelStyle) {
      this.owner = owner;
      this.label = label;
      this.firstLabelParam = firstLabelParam;
      this.firstLabelStyle = firstLabelStyle;
    }

    /// <summary>
    ///   This method is only called when a label should be added to owner.
    ///   This implementation prevents adding more than two labels.
    /// </summary>
    protected override void OnLabelAdding(IInputModeContext context, LabelEditingEventArgs args) {
      args.TextEditorInputModeConfigurator = ConfigureTextEditor;
      // Prevent adding more than two labels...
      if (owner is null || owner.Labels.Count >= 2) {
        args.Cancel = true;
        return;
      }
      base.OnLabelAdding(context, args);
    }

    /// <summary>
    ///   Provides the label style for newly created labels.
    ///   This implementation returns the special style for the first label if there are no labels yet, and the base
    ///   style otherwise.
    /// </summary>
    protected override ILabelStyle GetLabelStyle(IInputModeContext context, ILabelOwner owner) {
      if (owner.Labels.Count == 0 || !ReferenceEquals(firstLabelStyle, owner.Labels[0].Style)) {
        return firstLabelStyle;
      }
      return base.GetLabelStyle(context, owner)!;
    }

    /// <summary>
    ///   Provides the label model parameter for newly created labels.
    ///   This implementation returns the special parameter for the first label if there are no labels yet, and the base
    ///   label model parameter otherwise.
    /// </summary>
    protected override ILabelModelParameter GetLabelParameter(IInputModeContext context, ILabelOwner owner) {
      if (owner.Labels.Count == 0 || !ReferenceEquals(firstLabelParam, owner.Labels[0].LayoutParameter)) {
        return firstLabelParam;
      }
      return base.GetLabelParameter(context, owner)!;
    }

    /// <summary>
    ///   This method is called when a label should be edited.
    ///   If a label is edited directly, we either return it (if it is the second label) or prevent editing.
    /// </summary>
    protected override void OnLabelEditing(IInputModeContext context, LabelEditingEventArgs args) {
      args.TextEditorInputModeConfigurator = ConfigureTextEditor;
      if (label is not null) {
        // We are directly editing the label.
        if (ReferenceEquals(label.Style, firstLabelStyle)) {
          // The first label is never editable.
          args.Label = null!;
          args.Cancel = true;
          return;
        }

        // We are not the first label.
        // Return the label and disallow editing.
        // If we are editing the first label, the framework will then try to add a label by calling addLabel.
        args.Label = label;
        args.Handled = true;
        return;
      }

      // Implicit editing - this is only reached if we are trying to edit labels for an owner which does not yet have
      // any labels.
      if (owner is null) {
        base.OnLabelEditing(context, args);
        return;
      }
      if (owner.Labels.Count <= 1) {
        // Add a second label instead (since we'll never edit the first one).
        OnLabelAdding(context, args);
        return;
      }

      // If more than one label, edit the second one.
      args.Label = ReferenceEquals(owner.Labels[0].Style, firstLabelStyle) ? owner.Labels[1] : owner.Labels[0];
      args.Handled = true;
    }

    /// <summary>Customize the text editor when we are using our helper.</summary>
    private void ConfigureTextEditor(IInputModeContext context, TextEditorInputMode mode, ILabel labelToEdit) {
      var textBox = mode.TextBox;
      TextBoxProperties.SetCustomHelper(textBox, true);

      EventHandler<InputModeItemEventArgs<ILabel>> resetCustomHelper = null!;
      resetCustomHelper = (sender, _) => {
        var parentMode = (EditLabelInputMode) sender!;
        parentMode.LabelEdited -= resetCustomHelper;
        parentMode.LabelEditingCanceled -= resetCustomHelper;
        TextBoxProperties.SetCustomHelper(textBox, false);
      };

      var parentMode = (EditLabelInputMode) context.InputMode!;
      parentMode.LabelEdited += resetCustomHelper;
      parentMode.LabelEditingCanceled += resetCustomHelper;
    }
  }
}
