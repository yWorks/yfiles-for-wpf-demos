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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Legend;

/// <summary>
///   Legend that displays the entries in a separate graph control.
/// </summary>
internal sealed class GraphControlLegend : LegendBase
{
  private readonly Border anchor;
  private readonly Button header;
  private readonly GraphControl legendGraphControl;
  private bool collapsed;

  public GraphControlLegend(GraphControl graphControl, Border anchor, Button header, GraphControl legendGraphControl)
      : base(graphControl) {
    this.anchor = anchor;
    this.header = header;
    this.legendGraphControl = legendGraphControl;
    legendGraphControl.ContentMargins = new InsetsD(10);
    legendGraphControl.InputMode = new GraphViewerInputMode {
        SelectableItems = GraphItemTypes.None,
        FocusableItems = GraphItemTypes.None,
        MarqueeSelectableItems = GraphItemTypes.None
    };
    ShowLegend(true);
  }

  protected override void OnShow() => anchor.Visibility = Visibility.Visible;

  protected override void OnHide() => anchor.Visibility = Visibility.Collapsed;

  protected override void RefreshCore(IReadOnlyList<NodeTypeId> visibleTypes) {
    if (!collapsed) {
      var legendGraph = legendGraphControl.Graph;
      legendGraph.Clear();
      AddLegendEntries(visibleTypes, legendGraph);
      UpdateContainerSize(visibleTypes.Count);
      FitGraphBoundsAfterLayout();
    }
  }

  public void Toggle() => ShowLegend(collapsed);

  private void ShowLegend(bool show) {
    collapsed = !show;
    header.Content = show ? "Legend" : "☷";
    header.ToolTip = show ? "Close legend" : "Show legend";
    legendGraphControl.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    anchor.Width = show ? double.NaN : 56;
    anchor.Height = show ? double.NaN : 56;
    if (show) {
      Refresh(true);
    }
  }

  private void FitGraphBoundsAfterLayout() {
    if (legendGraphControl.Graph.Nodes.Count == 0) {
      return;
    }

    legendGraphControl.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => {
      if (IsVisible && !collapsed && legendGraphControl.ActualWidth > 0 && legendGraphControl.ActualHeight > 0) {
        _ = legendGraphControl.FitGraphBounds();
      }
    }));
  }

  /// <summary>
  ///   Updates the size of the legend graph control based on the number of entries.
  /// </summary>
  private void UpdateContainerSize(int entryCount) {
    const double headerHeight = 40;
    if (Horizontal) {
      anchor.Width = Math.Max(160, entryCount * 120 + 24);
      anchor.Height = headerHeight + 120;
    } else {
      anchor.Width = 300;
      anchor.Height = headerHeight + Math.Max(120, entryCount * 50 + 24);
    }
  }
}
