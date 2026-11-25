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
using yWorks.Layout.Router;

namespace yWorks.Demo.InteractiveEdgeRouting
{
  /// <summary>
  ///   Stores the edge routing scope selected in the toolbar.
  /// </summary>
  public sealed class RoutingSettings : INotifyPropertyChanged
  {
    private EdgeRouterScope selectedScope = EdgeRouterScope.SegmentsAsNeeded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<RoutingScopeOption> RoutingScopes { get; } = [
        new("Segment as Needed", EdgeRouterScope.SegmentsAsNeeded),
        new("Path as Needed", EdgeRouterScope.PathAsNeeded)
    ];

    public EdgeRouterScope SelectedScope {
      get => selectedScope;
      set {
        if (selectedScope == value) {
          return;
        }
        selectedScope = value;
        OnPropertyChanged();
      }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public sealed record RoutingScopeOption(string Label, EdgeRouterScope Value);
}
