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
using System.Windows;
using System.Windows.Controls;

namespace yWorks.Demo
{
  /// <summary>
  ///   A panel that implements the general layout for yFiles WPF demos:
  ///   Side bars on the right (description, interaction), a floating toolbar,
  ///   a main view filling the remaining space.
  /// </summary>
  public class DemoView : Panel
  {
    static DemoView() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(DemoView), new FrameworkPropertyMetadata(typeof(DemoView)));
    }

    /// <summary>
    ///   Gets or sets the gap width to leave between adjacent controls (e.g. <see cref="SideBar"/> and the main content).
    /// </summary>
    public double Gap {
      get => (double) GetValue(GapProperty);
      set => SetValue(GapProperty, value);
    }

    public static readonly DependencyProperty GapProperty = DependencyProperty.Register(nameof(Gap), typeof(double),
        typeof(DemoView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    protected override Size MeasureOverride(Size availableSize) {
      double collapsedSidebarCumulativeHeight = 0,
          collapsedSidebarMaximumWidth = 0,
          expandedSidebarCumulativeWidth = 0,
          maximumHeight = 0,
          mainContentWidth = 0,
          toolBarHeight = 0;
      var children = InternalChildren;

      // Measure sidebars and toolbar first, as they reduce the available size for the main content
      foreach (var child in children) {
        if (child is SideBar sidebar) {
          sidebar.Measure(availableSize);
          var desiredSize = sidebar.DesiredSize;
          if (sidebar.IsExpanded) {
            expandedSidebarCumulativeWidth += desiredSize.Width;
            if (sidebar.Visibility is not Visibility.Collapsed) {
              expandedSidebarCumulativeWidth += Gap;
            }
          } else {
            collapsedSidebarMaximumWidth = Math.Max(collapsedSidebarMaximumWidth, desiredSize.Width);
            collapsedSidebarCumulativeHeight += desiredSize.Height;
          }
        }
        if (child is DemoToolBar toolBar) {
          toolBar.Measure(availableSize);
          toolBarHeight = toolBar.DesiredSize.Height;
        }
      }

      if (collapsedSidebarMaximumWidth > 0) {
        collapsedSidebarMaximumWidth += Gap;
      }

      var remainingSize =
          new Size(Math.Max(0, availableSize.Width - expandedSidebarCumulativeWidth - collapsedSidebarMaximumWidth),
              Math.Max(0, availableSize.Height - toolBarHeight));

      for (var i = 0; i < children.Count; i++) {
        var child = children[i];
        if (child is not SideBar) {
          child.Measure(remainingSize);
        }
        var desiredSize = child.DesiredSize;

        maximumHeight = Math.Max(maximumHeight, desiredSize.Height);

        if (child is not SideBar) {
          mainContentWidth = Math.Max(mainContentWidth, desiredSize.Width);
        }
      }
      return new Size(mainContentWidth + expandedSidebarCumulativeWidth + collapsedSidebarMaximumWidth, maximumHeight);
    }

    protected override Size ArrangeOverride(Size finalSize) {
      // General layout idea:

      // ┌────────────────────┬──────┬─┐
      // │   ╔═════════════╗  │      │ │
      // │   ╚═════════════╝  │  S   │C│
      // │      Toolbar       │  i   │o│
      // │                    │  d   │l│
      // │                    │  e   │l│
      // │  Main content      │  b   │a│
      // │                    │  a   │p│
      // │              ╔══╗  │  r   │s│
      // │              ║  ║  │  s   │e│
      // │              ╚══╝  │      │d│
      // │           Overview │      │ │
      // └────────────────────┴──────┴─┘

      double collapsedSidebarMaximumWidth = 0, expandedSidebarCumulativeWidth = 0, toolBarHeight = 0;

      // Tally up sidebars – collapsed ones go to the far right, vertically stacked; the others stack horizontally
      foreach (var child in InternalChildren) {
        if (child is SideBar { DesiredSize: { Width: var desiredWidth } } sidebar) {
          if (sidebar.Visibility is not Visibility.Collapsed) {
            desiredWidth += Gap;
          }
          if (sidebar.IsExpanded) {
            expandedSidebarCumulativeWidth += desiredWidth;
          } else {
            collapsedSidebarMaximumWidth = Math.Max(collapsedSidebarMaximumWidth, desiredWidth);
          }
        }
        if (child is DemoToolBar toolBar) {
          toolBarHeight = toolBar.DesiredSize.Height;
        }
      }

      // Size remaining for the main content
      var mainContentSize =
          new Size(Math.Max(0, finalSize.Width - collapsedSidebarMaximumWidth - expandedSidebarCumulativeWidth),
              Math.Max(0, finalSize.Height - toolBarHeight));

      double collapsedSidebarCurrentY = 0, expandedSidebarCurrentX = mainContentSize.Width + Gap;
      var children = InternalChildren;
      for (var i = 0; i < children.Count; i++) {
        var child = children[i];
        var desiredSize = child.DesiredSize;
        switch (child) {
          case SideBar { IsExpanded: false }:
            child.Arrange(new Rect(finalSize.Width - desiredSize.Width, collapsedSidebarCurrentY, desiredSize.Width,
                desiredSize.Height));
            collapsedSidebarCurrentY += desiredSize.Height;
            break;
          case SideBar { IsExpanded: true, Visibility: not Visibility.Collapsed }:
            child.Arrange(new Rect(expandedSidebarCurrentX, 0, desiredSize.Width, finalSize.Height));
            expandedSidebarCurrentX += desiredSize.Width + Gap;
            break;
          case DemoToolBar:
            // Docked above the main content
            child.Arrange(new Rect(0, 0, mainContentSize.Width, desiredSize.Height));
            break;
          case FloatingButton:
            // Lower right of the main content
            child.Arrange(new Rect(mainContentSize.Width - 16 - desiredSize.Width,
                toolBarHeight + mainContentSize.Height - 16 - desiredSize.Height, desiredSize.Width,
                desiredSize.Height));
            break;
          default:
            child.Arrange(new Rect(new Point(0, toolBarHeight), mainContentSize));
            break;
        }
      }

      return finalSize;
    }
  }
}
