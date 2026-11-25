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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace yWorks.Demo.CriticalPaths
{
  /// <summary>
  ///   A panel over the graph control that displays priority settings for a selected graph item.
  ///   The panel is placed on the canvas overlay and uses an <see cref="ILabelModelParameter"/>
  ///   to determine its position relative to the selected item.
  /// </summary>
  internal sealed class PriorityPanel : Border
  {
    private static readonly Color[] PriorityColors = [
        Color.FromRgb(0xc7, 0xc7, 0xa6),
        Color.FromRgb(0x33, 0x66, 0x99),
        Color.FromRgb(0x56, 0x92, 0x6e),
        Color.FromRgb(0xf0, 0xc8, 0x08),
        Color.FromRgb(0xff, 0x6c, 0x00),
        Color.FromRgb(0xdb, 0x3a, 0x34)
    ];

    private readonly GraphControl graphControl;
    private readonly Button[] priorityButtons = new Button[6];
    private IReadOnlyList<IModelItem>? currentItems;

    public PriorityPanel(GraphControl graphControl, Canvas canvas) {
      this.graphControl = graphControl;
      BorderBrush = Brushes.LightGray;
      BorderThickness = new Thickness(1);
      Background = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255));
      Padding = new Thickness(6);
      Opacity = 0.9;
      // make the popup invisible
      Visibility = Visibility.Collapsed;

      var content = new StackPanel();
      content.Children.Add(new TextBlock {
          Text = "Priority",
          FontSize = 24,
          Margin = new Thickness(0, 0, 0, 2)
      });
      var buttonRow = new StackPanel { Orientation = Orientation.Horizontal };
      for (var priority = 0; priority < priorityButtons.Length; priority++) {
        var buttonPriority = priority;
        var button = new Button {
            Content = priority.ToString(),
            Width = 35,
            Height = 35,
            Margin = new Thickness(1),
            Padding = new Thickness(0),
            FontSize = 22,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(PriorityColors[priority]),
            BorderThickness = new Thickness(0)
        };
        button.Click += (_, _) => ApplyPriority(buttonPriority);
        priorityButtons[priority] = button;
        buttonRow.Children.Add(button);
      }
      content.Children.Add(buttonRow);
      Child = content;
      canvas.Children.Add(this);

      // Adds listeners for viewport changes and updates of the visual tree.
      graphControl.ViewportChanged += (_, _) => UpdateLocation();
      graphControl.UpdatedVisual += (_, _) => UpdateLocation();
    }

    public Action<IModelItem, int, int>? ItemPriorityChanged { get; set; }

    public Action? PriorityChanged { get; set; }

    public IReadOnlyList<IModelItem>? CurrentItems {
      get => currentItems;
      set {
        if (value is { Count: > 0 }) {
          if (currentItems is null || !currentItems.SequenceEqual(value)) {
            currentItems = value;
            ShowPanel();
          }
        } else {
          currentItems = null;
          HidePanel();
        }
      }
    }

    /// <summary>
    ///   Applies the selected priority to the current items and invokes the callbacks.
    /// </summary>
    private void ApplyPriority(int priority) {
      if (currentItems is not null) {
        foreach (var item in currentItems) {
          var oldPriority = GetPriority(item);
          if (oldPriority != priority) {
            ItemPriorityChanged?.Invoke(item, priority, oldPriority);
          }
        }
        PriorityChanged?.Invoke();
        CurrentItems = null;
      }
    }

    private void ShowPanel() {
      Visibility = Visibility.Visible;
      UpdateCurrentPriority();
      UpdateLocation();
    }

    private void HidePanel() {
      Visibility = Visibility.Collapsed;
    }

    private void UpdateCurrentPriority() {
      var priority = currentItems?.Count > 0 ? GetPriority(currentItems[0]) : 0;
      for (var i = 0; i < priorityButtons.Length; i++) {
        priorityButtons[i].BorderBrush = i == priority ? Brushes.DarkSeaGreen : Brushes.Transparent;
        priorityButtons[i].BorderThickness = i == priority ? new Thickness(2) : new Thickness(0);
      }
    }

    private void UpdateLocation() {
      if (currentItems is not { Count: > 0 } || graphControl.Zoom <= 0) {
        return;
      }

      Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      var panelSize = DesiredSize;
      var preferredSize = new SizeD(panelSize.Width / graphControl.Zoom, panelSize.Height / graphControl.Zoom);
      ILabelModelParameter? parameter = null;
      ILabel? dummyLabel = null;

      switch (currentItems[0]) {
        case IEdge edge:
          var edgeLabelModel = new EdgePathLabelModel { AutoRotation = false };
          parameter = edgeLabelModel.CreateRatioParameter();
          dummyLabel = new SimpleLabel(edge, string.Empty, parameter) { PreferredSize = preferredSize };
          break;
        case INode node:
          parameter = ExteriorNodeLabelModel.Top;
          dummyLabel = new SimpleLabel(node, string.Empty, parameter) { PreferredSize = preferredSize };
          break;
      }

      if (parameter is not null && dummyLabel is not null) {
        var geometry = parameter.Model.GetGeometry(dummyLabel, parameter);
        // Calculate view coordinates for the overlay, which shares the graph control's coordinate space.
        var viewPoint = graphControl.WorldToViewCoordinates(new PointD(geometry.AnchorX, geometry.AnchorY));
        Canvas.SetLeft(this, viewPoint.X);
        Canvas.SetTop(this, viewPoint.Y - panelSize.Height);
      }
    }

    private static int GetPriority(IModelItem item) =>
        item.Tag switch {
            EdgeData edge => edge.Priority,
            NodeData node => node.Priority,
            _ => 0
        };
  }
}
