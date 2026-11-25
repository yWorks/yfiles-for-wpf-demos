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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yWorks.Demo.RestrictedEditing
{
  /// <summary>
  ///   Editing operations available in the toolbar.
  /// </summary>
  public enum EditingOperations
  {
    None,
    MovingItems,
    All
  }

  /// <summary>
  ///   An editing operation and the text shown for it in the toolbar.
  /// </summary>
  public sealed record EditingOperationOption(string Name, EditingOperations Value);

  /// <summary>
  ///   Settings bound to the demo's toolbar controls.
  /// </summary>
  public sealed class Settings : INotifyPropertyChanged
  {
    private EditingOperations editingOperation;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Raised whenever a property has been changed interactively by the user.
    /// </summary>
    public event EventHandler? SettingChanged;

    public IReadOnlyList<EditingOperationOption> EditingOperationOptions { get; } = [
        new("None", EditingOperations.None),
        new("Moving Items", EditingOperations.MovingItems),
        new("All", EditingOperations.All)
    ];

    public EditingOperations EditingOperation {
      get => editingOperation;
      set {
        if (editingOperation == value) {
          return;
        }
        editingOperation = value;
        OnPropertyChanged();
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
