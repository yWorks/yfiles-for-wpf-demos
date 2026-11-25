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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yWorks.Demo.NodeSelectionResizing
{
  /// <summary>Resize modes available for the node selection.</summary>
  public enum NodeSelectionResizeMode
  {
    Resize,
    Scale
  }

  /// <summary>A resize mode and the text shown for it in the toolbar.</summary>
  public sealed record ResizeModeOption(string Name, NodeSelectionResizeMode Mode);

  /// <summary>Settings bound to the toolbar controls.</summary>
  public sealed class NodeSelectionResizingSettings : INotifyPropertyChanged
  {
    private bool snappingEnabled;
    private bool orthogonalEditingEnabled = true;
    private NodeSelectionResizeMode resizeMode = NodeSelectionResizeMode.Resize;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ResizeModeOption> ResizeModes { get; } = [
        new("Resize Nodes", NodeSelectionResizeMode.Resize),
        new("Scale Locations", NodeSelectionResizeMode.Scale)
    ];

    public bool SnappingEnabled {
      get => snappingEnabled;
      set => SetField(ref snappingEnabled, value);
    }

    public bool OrthogonalEditingEnabled {
      get => orthogonalEditingEnabled;
      set => SetField(ref orthogonalEditingEnabled, value);
    }

    public NodeSelectionResizeMode ResizeMode {
      get => resizeMode;
      set => SetField(ref resizeMode, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
