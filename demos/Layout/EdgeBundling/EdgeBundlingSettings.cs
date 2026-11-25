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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yWorks.Demo.EdgeBundling
{
  /// <summary>Settings bound to the controls in the demo toolbar.</summary>
  public sealed class EdgeBundlingSettings : INotifyPropertyChanged
  {
    private SampleOption? selectedSample;
    private double bundlingStrength = 0.98;
    private bool useDirectionalColors = true;
    private bool isEnabled = true;

    public EdgeBundlingSettings() {
      Samples = [
          new("single-cycle-layout", "Single Cycle Layout", "circular.json", 0.98),
          new("circular-layout", "Circular Layout", "bccCircular.json", 0.98),
          new("spanner", "Spanner-based Routing", "voronoi-spanner.json", 0.85),
          new("voronoi", "Voronoi-based Routing", "voronoi-spanner.json", 0.75),
          new("radial-layout", "Radial Layout", "radial.json", 0.98),
          new("radial-tree-layout", "Radial Tree Layout", "radial-tree.json", 0.95),
          new("tree-layout", "Tree Layout", "tree.json", 0.95),
          new("force-directed", "Force-Directed Routing", "routing.json", 0.75)
      ];
    }

    public ObservableCollection<SampleOption> Samples { get; }

    public SampleOption? SelectedSample {
      get => selectedSample;
      set {
        if (SetField(ref selectedSample, value)) {
          if (value is not null) {
            BundlingStrength = value.DefaultBundlingStrength;
          }
          SampleChanged?.Invoke(this, EventArgs.Empty);
        }
      }
    }

    public double BundlingStrength {
      get => bundlingStrength;
      set {
        if (SetField(ref bundlingStrength, value)) {
          StrengthChanged?.Invoke(this, EventArgs.Empty);
        }
      }
    }

    public bool UseDirectionalColors {
      get => useDirectionalColors;
      set {
        if (SetField(ref useDirectionalColors, value)) {
          ColorSchemeChanged?.Invoke(this, EventArgs.Empty);
        }
      }
    }

    public bool IsEnabled {
      get => isEnabled;
      set => SetField(ref isEnabled, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SampleChanged;

    public event EventHandler? StrengthChanged;

    public event EventHandler? ColorSchemeChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return false;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      return true;
    }
  }

  public sealed record SampleOption(string Id, string Name, string ResourceName, double DefaultBundlingStrength);
}
