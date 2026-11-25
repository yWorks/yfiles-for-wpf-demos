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
using yWorks.Layout.Organic;

namespace yWorks.Demo.OrganicSubstructures
{
  internal sealed record SampleLayoutSettings(
      CycleSubstructureStyle CycleStyle,
      ChainSubstructureStyle ChainStyle,
      StarSubstructureStyle StarStyle,
      ParallelSubstructureStyle ParallelStyle,
      TreeSubstructureStyle TreeStyle,
      GroupSubstructureScope GroupScope,
      bool UseEdgeGrouping,
      bool ConsiderNodeTypes,
      bool ParallelTypeSeparation,
      bool StarTypeSeparation,
      bool AllowTypeChanges);

  /// <summary>
  ///   Settings backing the organic layout interaction panel.
  /// </summary>
  internal sealed class Settings : INotifyPropertyChanged
  {
    private bool suppressChangeNotification;
    private bool isBusy;
    private CycleSubstructureStyle cycleStyle;
    private ChainSubstructureStyle chainStyle;
    private StarSubstructureStyle starStyle;
    private ParallelSubstructureStyle parallelStyle;
    private TreeSubstructureStyle treeStyle;
    private GroupSubstructureScope groupScope;
    private bool useEdgeGrouping;
    private bool considerNodeTypes = true;
    private bool parallelTypeSeparation;
    private bool starTypeSeparation;
    private bool allowTypeChanges = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SettingChanged;

    public CycleSubstructureStyle CycleStyle {
      get => cycleStyle;
      set => SetField(ref cycleStyle, value);
    }

    public ChainSubstructureStyle ChainStyle {
      get => chainStyle;
      set => SetField(ref chainStyle, value);
    }

    public StarSubstructureStyle StarStyle {
      get => starStyle;
      set => SetField(ref starStyle, value);
    }

    public ParallelSubstructureStyle ParallelStyle {
      get => parallelStyle;
      set => SetField(ref parallelStyle, value);
    }

    public TreeSubstructureStyle TreeStyle {
      get => treeStyle;
      set => SetField(ref treeStyle, value);
    }

    public GroupSubstructureScope GroupScope {
      get => groupScope;
      set => SetField(ref groupScope, value);
    }

    public bool UseEdgeGrouping {
      get => useEdgeGrouping;
      set => SetField(ref useEdgeGrouping, value);
    }

    public bool ConsiderNodeTypes {
      get => considerNodeTypes;
      set => SetField(ref considerNodeTypes, value);
    }

    public bool ParallelTypeSeparation {
      get => parallelTypeSeparation;
      set => SetField(ref parallelTypeSeparation, value);
    }

    public bool StarTypeSeparation {
      get => starTypeSeparation;
      set => SetField(ref starTypeSeparation, value);
    }

    public bool AllowTypeChanges {
      get => allowTypeChanges;
      private set => SetField(ref allowTypeChanges, value);
    }

    public bool IsBusy {
      get => isBusy;
      set {
        if (isBusy == value) {
          return;
        }
        isBusy = value;
        OnPropertyChanged();
      }
    }

    public void UpdateFrom(SampleLayoutSettings sample) {
      suppressChangeNotification = true;
      try {
        CycleStyle = sample.CycleStyle;
        ChainStyle = sample.ChainStyle;
        StarStyle = sample.StarStyle;
        ParallelStyle = sample.ParallelStyle;
        TreeStyle = sample.TreeStyle;
        GroupScope = sample.GroupScope;
        UseEdgeGrouping = sample.UseEdgeGrouping;
        ConsiderNodeTypes = sample.ConsiderNodeTypes;
        ParallelTypeSeparation = sample.ParallelTypeSeparation;
        StarTypeSeparation = sample.StarTypeSeparation;
        AllowTypeChanges = sample.AllowTypeChanges;
      } finally {
        suppressChangeNotification = false;
      }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return;
      }
      field = value;
      OnPropertyChanged(propertyName);
      if (!suppressChangeNotification) {
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
