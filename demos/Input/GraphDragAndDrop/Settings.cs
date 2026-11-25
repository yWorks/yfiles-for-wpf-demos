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

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yWorks.Demo.GraphDragAndDrop
{
  internal sealed class Settings : INotifyPropertyChanged
  {
    private bool showPreview = true;
    private bool snappingEnabled = true;
    private bool allowFolderNodeAsParent;

    public event PropertyChangedEventHandler? PropertyChanged;

    // button to enable or disable the preview shown during the drag and drop operation
    public bool ShowPreview {
      get => showPreview;
      set {
        if (!SetProperty(ref showPreview, value)) {
          return;
        }

        if (!value && SnappingEnabled) {
          SnappingEnabled = false;
        }
      }
    }

    // button to enable or disable the snapping during the drag and drop operation
    public bool SnappingEnabled {
      get => snappingEnabled;
      set {
        if (!SetProperty(ref snappingEnabled, value)) {
          return;
        }

        if (value && !ShowPreview) {
          ShowPreview = true;
        }
      }
    }

    // button to allow or disable dropping of graphs on folder nodes
    public bool AllowFolderNodeAsParent {
      get => allowFolderNodeAsParent;
      set => SetProperty(ref allowFolderNodeAsParent, value);
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return false;
      }

      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      return true;
    }
  }
}
