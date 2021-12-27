/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
  /// This class does the following:
  /// <list type="bullet">
  /// <item>Allow at most two labels</item>
  /// <item>Disallow editing the first label</item>
  /// <item>Use custom placement and style for the first label</item>
  /// <item>Change the appearance of the <see cref="TextEditorInputMode" /></item>
  /// </list>
  /// For convenience, this implementation inherits from the predefined <see cref="EditLabelHelper" /> class.
  /// </remarks>
  public class MyEditLabelHelper : EditLabelHelper
  {
    private readonly ILabel label;

    private ILabelOwner owner;

    /// <summary>
    /// Special parameter for the first label.
    /// </summary>
    private readonly ILabelModelParameter firstLabelParam;


    /// <summary>
    /// Special style for the first label.
    /// </summary>
    private readonly ILabelStyle firstLabelStyle;

    public MyEditLabelHelper(ILabelOwner owner, ILabel label, ILabelModelParameter firstLabelParam, ILabelStyle firstLabelStyle) {
      this.owner = owner;
      this.label = label;
      this.firstLabelParam = firstLabelParam;
      this.firstLabelStyle = firstLabelStyle;
      TextBoxBackground = Brushes.LightGray;
      TextBoxForeground = Brushes.DarkSlateGray;
      TextBoxBorderBrush = Brushes.LightGray;
    }

    /// <summary>
    /// The background of the text box during editing.
    /// </summary>
    public Brush TextBoxBackground { get; set; }

    /// <summary>
    /// The foreground of the text box during editing.
    /// </summary>
    public Brush TextBoxForeground { get; set; }

    /// <summary>
    /// The border brush of the text box during editing.
    /// </summary>
    public Brush TextBoxBorderBrush { get; set; }

    /// <summary>
    /// This method is only called when a label should be added to <see cref="owner" />.
    /// </summary>
    /// <remarks>This implementation prevents adding of more than two labels</remarks>
    public override void OnLabelAdding(LabelEditingEventArgs args) {
      args.TextEditorInputModeConfigurator = ConfigureTextEditorInputMode;
      // Prevent adding more than two labels...
      if (owner.Labels.Count >= 2) {
        args.Cancel = true;
        return;
      }
      base.OnLabelAdding(args);
    }

    /// <summary>
    /// Provides the label style for newly created labels.
    /// </summary>
    /// <remarks>
    /// This implementation return the special style for the first if there are no labels yet, and the base style otherwise.
    /// </remarks>
    protected override ILabelStyle GetLabelStyle(IInputModeContext context, ILabelOwner owner) {
      return owner.Labels.Count == 0 ? firstLabelStyle : base.GetLabelStyle(context, owner);
    }

    /// <summary>
    /// Provides the label model parameter for newly created labels.
    /// </summary>
    /// <remarks>
    /// This implementation returns the special parameter for the first label if there are no labels yet, and the base label
    /// model parameter otherwise.
    /// </remarks>
    protected override ILabelModelParameter GetLabelParameter(IInputModeContext context, ILabelOwner owner) {
      return owner.Labels.Count == 0 ? firstLabelParam : base.GetLabelParameter(context, owner);
    }

    /// <summary>This method is called when label should be edited.</summary>
    /// <remarks>
    /// If a label is edited directly, we either return it (if it is the second label) or prevent editing.
    /// </remarks>
    public override void OnLabelEditing(LabelEditingEventArgs args) {
      args.TextEditorInputModeConfigurator = ConfigureTextEditorInputMode;
      if (label != null) {
        // We are directly editing the label
        if (label.Owner != null && label == label.Owner.Labels[0]) {
          // The first label is never editable
          args.Label = null;
          args.Cancel = true;
          return;
        }

        //We are a dummy label or not the first label
        //return the label and disallow editing
        //If we are editing the first label, the framework will then try to add label by calling AddLabel
        args.Label = label;
        args.Handled = true;
        return;
      }

      // Implicit editing - this is only reached if we are trying to edit labels for an owner which does not yet have any labels
      if (owner == null) {
        base.OnLabelEditing(args);
        return;
      }
      if (owner.Labels.Count <= 1) {
        // Add a second label instead (since we'll never edit the first one)
        OnLabelAdding(args);
        return;
      }

      // If more than one label, edit the second one
      args.Label = owner.Labels[1];
      args.Handled = true;
    }

    /// <summary>
    /// Customize the text editor when we are using our helper.
    /// </summary>
    private void ConfigureTextEditorInputMode(IInputModeContext context, TextEditorInputMode mode,
        ILabel labelToEdit) {
      var textBox = mode.TextBox;
      var oldForeground = textBox.Foreground;
      var oldBackground = textBox.Background;
      var oldBorderBrush = textBox.BorderBrush;

      SetStyling(textBox, TextBoxForeground, TextBoxBackground, TextBoxBorderBrush);

      // Restore after editing
      EventHandler<TextEventArgs> afterEditing = null;
      afterEditing = delegate {
        SetStyling(textBox, oldForeground, oldBackground, oldBorderBrush);
        mode.TextEdited -= afterEditing;
        mode.EditingCanceled -= afterEditing;
      };
      mode.TextEdited += afterEditing;
      mode.EditingCanceled += afterEditing;
    }

    private static void SetStyling(TextBox textBox, Brush foreground, Brush background, Brush borderBrush) {
      textBox.Foreground = foreground;
      textBox.Background = background;
      textBox.BorderBrush = borderBrush;
    }
  }
}