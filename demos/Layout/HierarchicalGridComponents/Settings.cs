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

namespace yWorks.Demo.HierarchicalGridComponents
{
  public enum BusPreset
  {
    Balanced,
    Leaves,
    Squares,
    LeftAligned,
    RightAligned,
    Custom
  }

  public sealed record BusPresetOption(BusPreset Value, string Name);

  public sealed class Settings : INotifyPropertyChanged
  {
    private bool useGridComponents = true;
    private BusPreset preset = BusPreset.Balanced;
    private double beforeBusLimit;
    private double afterBusLimit;
    private bool isLayoutRunning;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SettingChanged;

    public IReadOnlyList<BusPresetOption> BusPresetOptions { get; } = [
        new(BusPreset.Balanced, "Balanced"),
        new(BusPreset.Leaves, "Leaves"),
        new(BusPreset.Squares, "Squares"),
        new(BusPreset.LeftAligned, "Left Aligned"),
        new(BusPreset.RightAligned, "Right Aligned"),
        new(BusPreset.Custom, "Custom")
    ];

    public bool UseGridComponents {
      get => useGridComponents;
      set => SetField(ref useGridComponents, value);
    }

    public BusPreset Preset {
      get => preset;
      set => SetField(ref preset, value);
    }

    public double BeforeBusLimit {
      get => beforeBusLimit;
      set {
        if (SetField(ref beforeBusLimit, value)) {
          OnPropertyChanged(nameof(BeforeBusLabel));
        }
      }
    }

    public double AfterBusLimit {
      get => afterBusLimit;
      set {
        if (SetField(ref afterBusLimit, value)) {
          OnPropertyChanged(nameof(AfterBusLabel));
        }
      }
    }

    public bool IsLayoutRunning {
      get => isLayoutRunning;
      set {
        if (SetField(ref isLayoutRunning, value, notifySettingChanged: false)) {
          OnPropertyChanged(nameof(IsBusSettingsEnabled));
          OnPropertyChanged(nameof(IsToggleEnabled));
          OnPropertyChanged(nameof(IsCustomSettingsEnabled));
        }
      }
    }

    public bool IsBusSettingsEnabled => UseGridComponents && !IsLayoutRunning;

    public bool IsToggleEnabled => !IsLayoutRunning;

    public bool IsCustomSettingsEnabled => IsBusSettingsEnabled && Preset == BusPreset.Custom;

    public string BeforeBusLabel =>
        Preset switch {
            BusPreset.LeftAligned => "0",
            BusPreset.RightAligned => "Infinity",
            _ => BeforeBusLimit.ToString("0")
        };

    public string AfterBusLabel =>
        Preset switch {
            BusPreset.LeftAligned => "Infinity",
            BusPreset.RightAligned => "0",
            _ => AfterBusLimit.ToString("0")
        };

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null,
        bool notifySettingChanged = true) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return false;
      }
      field = value;
      OnPropertyChanged(propertyName);

      if (propertyName is nameof(UseGridComponents) or nameof(Preset)) {
        OnPropertyChanged(nameof(IsBusSettingsEnabled));
        OnPropertyChanged(nameof(IsCustomSettingsEnabled));
      }
      if (propertyName == nameof(Preset)) {
        OnPropertyChanged(nameof(BeforeBusLabel));
        OnPropertyChanged(nameof(AfterBusLabel));
      }
      if (notifySettingChanged) {
        SettingChanged?.Invoke(this, EventArgs.Empty);
      }
      return true;
    }

    private void OnPropertyChanged(string? propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
