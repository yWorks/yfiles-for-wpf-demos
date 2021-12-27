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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Editor
{
  /// <summary>
  /// Convenience class that wraps an <see cref="EditorControl"/> instance and adds some buttons.
  /// </summary>
  public partial class EditorForm 
  {
    public EditorForm() {
      InitializeComponent();
    }

    /// <summary>
    /// Return whether the Editor is in auto commit state
    /// </summary>
    public bool IsAutoCommit {
      get { return editorControl.IsAutoCommit; }
      set { editorControl.IsAutoCommit = value; }
    }

    /// <summary>
    /// Dependency property for the <see cref="ShowAdoptButton"/> property.
    /// </summary>
    public static readonly DependencyProperty ShowAdoptButtonProperty = DependencyProperty.Register("ShowAdoptButton", typeof (bool), typeof (EditorForm),
                                                                                      new PropertyMetadata(
                                                                                        false, AdoptChanged));
    private static void AdoptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ((EditorForm)d).adoptButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void ApplyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ((EditorForm)d).applyButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void ResetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ((EditorForm)d).resetButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Return whether the Editor should show an adopt button.
    /// </summary>
    /// <remarks>Default is <see langword="false"/></remarks>
    /// <seealso cref="ShowAdoptButtonProperty"/>
    public bool ShowAdoptButton {
      get { return (bool) GetValue(ShowAdoptButtonProperty); }
      set { SetValue(ShowAdoptButtonProperty, value); }
    }

    /// <summary>
    /// Dependency property for the <see cref="OptionHandler"/> property.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty OptionHandlerProperty =
      DependencyProperty.Register("OptionHandler", typeof(OptionHandler), typeof(EditorForm),
                                  new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Convenience property that will automatically create a <see cref="View"/> for the given
    /// option handler.
    /// </summary>
    /// <value>The option handler to display in this editor.</value>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public OptionHandler OptionHandler {
      get { return (OptionHandler)GetValue(OptionHandlerProperty); }
      set { SetValue(OptionHandlerProperty, value); }
    }


    /// <summary>
    /// Dependency property for the <see cref="ShowApplyButton"/> property.
    /// </summary>
    public static readonly DependencyProperty ShowApplyButtonProperty = DependencyProperty.Register("ShowApplyButton", typeof (bool), typeof (EditorForm),
                                                                                      new PropertyMetadata(false, ApplyChanged));

    /// <summary>
    /// Return whether the Editor should show an apply button for non-modal dialogs.
    /// </summary>
    /// <remarks>Default is <see langword="false"/></remarks>
    public bool ShowApplyButton {
      get { return (bool) GetValue(ShowApplyButtonProperty); }
      set { SetValue(ShowApplyButtonProperty, value); }
    }


    /// <summary>
    /// Dependency property for the <see cref="ShowResetButton"/> property.
    /// </summary>
    public static readonly DependencyProperty ShowResetButtonProperty =
      DependencyProperty.Register("ShowResetButton", typeof (bool), typeof (EditorForm), new PropertyMetadata(true, ResetChanged));
    
    /// <summary>
    /// Return whether the Editor should show a reset button.
    /// </summary>
    /// <remarks>Default is <see langword="true"/></remarks>
    public bool ShowResetButton {
      get { return (bool) GetValue(ShowResetButtonProperty); }
      set { SetValue(ShowResetButtonProperty, value); }
    }

    /// <summary>
    /// Return whether the Editor is in auto adopt state
    /// </summary>
    public bool IsAutoAdopt {
      get { return editorControl.IsAutoAdopt; }
      set { editorControl.IsAutoAdopt = value; }
    }

    #region event listeners

    private void okButton_Click(object sender, EventArgs e) {
      if (System.Windows.Interop.ComponentDispatcher.IsThreadModal) {
        DialogResult = true;
      }
      OnValuesCommitted(this, EventArgs.Empty);
      this.Close();
    }

    private void applyButton_Click(object sender, EventArgs e) {
      OnValuesCommitted(this, EventArgs.Empty);
    }

    private void adoptButton_Click(object sender, EventArgs e) {
      OnValuesAdopted(this, EventArgs.Empty);
    }

    /// <summary>
    /// This method is called when values have been adopted in the form.
    /// </summary>
    /// <param name="form"></param>
    /// <param name="args"></param>
    protected virtual void OnValuesAdopted(object form, EventArgs args) {
      //delegate operation to active view
      editorControl.AdoptValues();
      if(ValuesAdopted != null) {
        ValuesAdopted(form, args);
      }
    }

    /// <summary>
    /// This method is called when values have been committed by the form.
    /// </summary>
    /// <param name="form"></param>
    /// <param name="args"></param>
    protected virtual void OnValuesCommitted(object form, EventArgs args) {
      //delegate operation to active view
      editorControl.CommitValues();
      if (ValuesCommitted != null) {
        ValuesCommitted(form, args);
      }
    }

    private void cancelButton_Click(object sender, EventArgs e) {
      //don't commit
      if (System.Windows.Interop.ComponentDispatcher.IsThreadModal) {
        DialogResult = false;
      }
      this.Close();
    }

    private void resetButton_Click(object sender, EventArgs e) {
      editorControl.ResetValues();
    }

    #endregion

    /// <summary>
    /// This events is fired when values have been adopted by the form.
    /// </summary>
    public event EventHandler ValuesAdopted;

    /// <summary>
    /// This events is fired when values have been committed by the form.
    /// </summary>
    public event EventHandler ValuesCommitted;
  }
}
