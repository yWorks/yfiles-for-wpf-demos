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
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalSubcomponents
{
  public enum LayoutKind
  {
    Tree,
    Organic,
    Orthogonal,
    Hierarchical
  }

  public sealed record Choice<T>(string Label, T Value);

  public sealed class HierarchicalSubcomponentsSettings : INotifyPropertyChanged
  {
    private LayoutKind selectedLayout = LayoutKind.Tree;
    private LayoutOrientation selectedOrientation = LayoutOrientation.TopToBottom;
    private SubcomponentPlacementPolicy selectedPlacementPolicy = SubcomponentPlacementPolicy.Automatic;
    private bool hasSelectedNodes;
    private bool isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<Choice<LayoutKind>> LayoutOptions { get; } = [
        new("Tree Layout", LayoutKind.Tree),
        new("Organic Layout", LayoutKind.Organic),
        new("Orthogonal Layout", LayoutKind.Orthogonal),
        new("Hierarchical Layout", LayoutKind.Hierarchical)
    ];

    public IReadOnlyList<Choice<LayoutOrientation>> OrientationOptions { get; } = [
        new("Top to Bottom", LayoutOrientation.TopToBottom),
        new("Bottom to Top", LayoutOrientation.BottomToTop),
        new("Left to Right", LayoutOrientation.LeftToRight),
        new("Right to Left", LayoutOrientation.RightToLeft)
    ];

    public IReadOnlyList<Choice<SubcomponentPlacementPolicy>> PlacementPolicyOptions { get; } = [
        new("Automatic", SubcomponentPlacementPolicy.Automatic),
        new("Isolated", SubcomponentPlacementPolicy.Isolated),
        new("Always Integrated", SubcomponentPlacementPolicy.AlwaysIntegrated)
    ];

    public LayoutKind SelectedLayout {
      get => selectedLayout;
      set {
        if (selectedLayout == value) {
          return;
        }
        selectedLayout = value;
        OnPropertyChanged(nameof(SelectedLayout));
        OnPropertyChanged(nameof(OrientationEnabled));
      }
    }

    public LayoutOrientation SelectedOrientation {
      get => selectedOrientation;
      set {
        if (selectedOrientation == value) {
          return;
        }
        selectedOrientation = value;
        OnPropertyChanged(nameof(SelectedOrientation));
      }
    }

    public SubcomponentPlacementPolicy SelectedPlacementPolicy {
      get => selectedPlacementPolicy;
      set {
        if (selectedPlacementPolicy == value) {
          return;
        }
        selectedPlacementPolicy = value;
        OnPropertyChanged(nameof(SelectedPlacementPolicy));
      }
    }

    public bool OrientationEnabled => SelectedLayout is LayoutKind.Tree or LayoutKind.Hierarchical;

    public bool CanChangeSubcomponents => hasSelectedNodes && !isBusy;

    public bool CanRunLayout => !isBusy;

    public bool HasSelectedNodes {
      get => hasSelectedNodes;
      set {
        if (hasSelectedNodes == value) {
          return;
        }
        hasSelectedNodes = value;
        OnPropertyChanged(nameof(HasSelectedNodes));
        OnPropertyChanged(nameof(CanChangeSubcomponents));
      }
    }

    public bool IsBusy {
      get => isBusy;
      set {
        if (isBusy == value) {
          return;
        }
        isBusy = value;
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(CanChangeSubcomponents));
        OnPropertyChanged(nameof(CanRunLayout));
      }
    }

    private void OnPropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
