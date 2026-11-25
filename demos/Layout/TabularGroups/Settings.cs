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

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yWorks.Demo.TabularGroups
{
  /// <summary>
  ///   Settings bound to the toolbar controls.
  /// </summary>
  internal sealed class Settings : INotifyPropertyChanged
  {
    private string selectedSample = "simple";
    private bool tabularGroupsEnabled = true;
    private bool sortingEnabled;
    private double childDistance;
    private bool isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<SampleOption> Samples { get; } = [
        new("simple", "Simple Table"),
        new("database", "Database"),
        new("uml", "UML Diagram")
    ];

    public string SelectedSample {
      get => selectedSample;
      set => SetField(ref selectedSample, value);
    }

    public bool TabularGroupsEnabled {
      get => tabularGroupsEnabled;
      set => SetField(ref tabularGroupsEnabled, value);
    }

    public bool SortingEnabled {
      get => sortingEnabled;
      set => SetField(ref sortingEnabled, value);
    }

    public double ChildDistance {
      get => childDistance;
      set => SetField(ref childDistance, value);
    }

    public bool IsBusy {
      get => isBusy;
      set => SetField(ref isBusy, value);
    }

    public bool CanChangeSample => !IsBusy;

    public bool CanChangeTabularGroups => !IsBusy;

    public bool CanChangeSorting => !IsBusy && TabularGroupsEnabled;

    public bool CanChangeChildDistance => !IsBusy && TabularGroupsEnabled;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

      if (propertyName is nameof(IsBusy)) {
        OnPropertyChanged(nameof(CanChangeSample));
        OnPropertyChanged(nameof(CanChangeTabularGroups));
        OnPropertyChanged(nameof(CanChangeSorting));
        OnPropertyChanged(nameof(CanChangeChildDistance));
      } else if (propertyName is nameof(TabularGroupsEnabled)) {
        OnPropertyChanged(nameof(CanChangeSorting));
        OnPropertyChanged(nameof(CanChangeChildDistance));
      }
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  internal sealed record SampleOption(string Id, string Name);
}
