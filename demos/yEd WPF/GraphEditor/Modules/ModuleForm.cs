/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Graph;

namespace Demo.yFiles.GraphEditor.Modules
{
  /// <summary>
  /// Convenience class that can show the settings for an <see cref="YModule"/> instance and
  /// start or cancel the module's execution.
  /// </summary>
  public partial class ModuleForm : Window
  {
    private bool isModal = false;
    private YModule module;
    private ILookup context;

    /// <summary>
    /// Create a new Form for the given module.
    /// </summary>
    /// <param name="module">The module</param>
    /// <param name="moduleContext">The execution context for this module.</param>
    /// <param name="isModal">Whether the module form should be modal or not modal when <see cref="ShowWindow"/>
    /// is called.</param>
    /// <param name="parentWindow">The parent formm for this ModuleForm, can be <see langword="null"/>.</param>
    public ModuleForm(YModule module, ILookup moduleContext, bool isModal, Window parentWindow) {
      this.module = module;
      context = moduleContext;
      this.isModal = isModal;
      Owner = parentWindow;

      InitializeComponent();
    }

    protected virtual void OnLoaded(object source, EventArgs args) {
      editorControl.OptionHandler = module.Handler;
      Title = module.ResourceManager.GetString(module.ModuleName);
    }

    #region private helper methods

    /// <summary>
    /// Return whether a modal or non modal dialog should be shown.
    /// </summary>
    public bool IsModal {
      get { return isModal; }
    }

    /// <summary>
    /// Display the form either as modal or non modal dialog, depending on <see cref="IsModal"/>.
    /// </summary>
    public virtual void ShowWindow() {
      if (isModal) {
        ShowDialog();
      } else {
        Show();
      }
    }

    #endregion

    #region event listeners

    private void applyOrOKButton_Click(object sender, EventArgs e) {
      //delegate operation to active view
      editorControl.CommitValues();
      if (IsModal) {
        Close();
      }
      //todo: Start as thread...
      module.Start(context);
    }

    private void resetButton_Click(object sender, EventArgs e) {
      editorControl.ResetValues();
    }

    private void cancelButton_Click(object sender, EventArgs e) {
      //don't commit
      Close();
    }

    #endregion
  }
}