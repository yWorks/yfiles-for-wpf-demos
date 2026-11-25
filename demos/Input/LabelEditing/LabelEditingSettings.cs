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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yWorks.Demo.LabelEditing
{
  /// <summary>
  ///   Settings for the label editing interaction panel.
  /// </summary>
  public sealed class LabelEditingSettings : INotifyPropertyChanged
  {
    private bool labelCreation = true;
    private bool labelEditing = true;
    private bool hideLabel = true;
    private bool instantTyping = true;

    /// <summary>Whether to use custom label helpers.</summary>
    private bool customLabelHelper;

    private bool nodesEnabled = true;
    private bool edgesEnabled = true;

    /// <summary>Whether label text validation is used at all.</summary>
    private bool validationEnabled;

    private string validationPattern = @"^\w+@\w+\.\w+$";

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Raised whenever a setting has been changed interactively by the user.
    /// </summary>
    public event EventHandler? SettingChanged;

    public bool LabelCreation {
      get => labelCreation;
      set => SetField(ref labelCreation, value);
    }

    public bool LabelEditing {
      get => labelEditing;
      set => SetField(ref labelEditing, value);
    }

    public bool HideLabel {
      get => hideLabel;
      set => SetField(ref hideLabel, value);
    }

    public bool InstantTyping {
      get => instantTyping;
      set => SetField(ref instantTyping, value);
    }

    public bool CustomLabelHelper {
      get => customLabelHelper;
      set => SetField(ref customLabelHelper, value);
    }

    public bool NodesEnabled {
      get => nodesEnabled;
      set => SetField(ref nodesEnabled, value);
    }

    public bool EdgesEnabled {
      get => edgesEnabled;
      set => SetField(ref edgesEnabled, value);
    }

    public bool ValidationEnabled {
      get => validationEnabled;
      set => SetField(ref validationEnabled, value);
    }

    public string ValidationPattern {
      get => validationPattern;
      set => SetField(ref validationPattern, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return;
      }

      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      SettingChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}
