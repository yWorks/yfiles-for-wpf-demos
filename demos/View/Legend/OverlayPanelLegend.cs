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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;

namespace yWorks.Demo.Legend;

/// <summary>
///   Legend that consists of simple WPF elements. The icons are created using a UIElement export from the node style.
/// </summary>
internal sealed class OverlayPanelLegend : LegendBase
{
  private readonly Border container;
  private readonly StackPanel listElement;

  public OverlayPanelLegend(GraphControl graphControl, Border container, StackPanel listElement) : base(graphControl) {
    this.container = container;
    this.listElement = listElement;
  }

  protected override void OnShow() => container.Visibility = Visibility.Visible;

  protected override void OnHide() => container.Visibility = Visibility.Collapsed;

  protected override void RefreshCore(IReadOnlyList<NodeTypeId> visibleTypes) {
    container.HorizontalAlignment = Horizontal ? HorizontalAlignment.Center : HorizontalAlignment.Right;
    container.VerticalAlignment = Horizontal ? VerticalAlignment.Bottom : VerticalAlignment.Center;
    container.Margin = Horizontal ? new Thickness(16) : new Thickness(0, 70, 16, 16);
    listElement.Orientation = Horizontal ? Orientation.Horizontal : Orientation.Vertical;
    listElement.Children.Clear();
    foreach (var type in visibleTypes) {
      var entry = new StackPanel {
          Orientation = Orientation.Horizontal,
          Margin = new Thickness(2, 4, 2, 4),
          Tag = type,
          Cursor = Cursors.Arrow
      };
      entry.Children.Add(CreateNodeTypeVisual(NodeTypes.CreateStyle(type)));
      entry.Children.Add(new TextBlock {
          Text = NodeTypes.Infos[type].Name,
          Foreground = Brushes.White,
          VerticalAlignment = VerticalAlignment.Center,
          Margin = new Thickness(10, 0, 0, 0)
      });
      entry.MouseEnter += OnEntryMouseEnter;
      entry.MouseLeave += OnEntryMouseLeave;
      listElement.Children.Add(entry);
    }
  }

  private void OnEntryMouseEnter(object sender, MouseEventArgs args) {
    if (sender is FrameworkElement { Tag: NodeTypeId type }) {
      graphControl.Highlights.Clear();
      foreach (var node in graphControl.Graph.Nodes) {
        if (node.Tag is NodeTypeId nodeType && nodeType == type) {
          graphControl.Highlights.Add(node);
        }
      }
    }
  }

  private void OnEntryMouseLeave(object sender, MouseEventArgs args) => graphControl.Highlights.Clear();
}
