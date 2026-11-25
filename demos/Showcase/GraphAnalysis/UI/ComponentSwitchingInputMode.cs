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
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.GraphAnalysis.UI
{
  /// <summary>
  ///   An input mode that shows an overlay and allows to switch between the displayed
  ///   components for nodes that belong to multiple components.
  ///   The input mode will also highlight all nodes and edges belonging to a component
  ///   being hovered over.
  /// </summary>
  internal sealed class ComponentSwitchingInputMode : IVisualCreator
  {
    private readonly GraphControl control;
    private readonly Func<AnalysisResult?> getResult;
    private readonly Func<INode, bool> isEndpoint;
    private readonly Func<int, Brush> color;
    private readonly Action<int?> highlight;
    private readonly IRenderTreeElement overlay;

    /// <summary>
    ///   The node currently hovered over, showing an extended popup for selecting the displayed component.
    /// </summary>
    private INode? hoveredNode;

    private int? hoveredComponent;
    private bool pickerMouseDown;

    internal ComponentSwitchingInputMode(
        GraphControl control,
        Func<AnalysisResult?> getResult,
        Func<INode, bool> isEndpoint,
        Func<int, Brush> color,
        Action<int?> highlight) {
      this.control = control;
      this.getResult = getResult;
      this.isEndpoint = isEndpoint;
      this.color = color;
      this.highlight = highlight;
      var tree = control.RenderTree;
      var group = tree.CreateGroup(tree.RootGroup);
      group.Below(tree.SelectionGroup);
      overlay = tree.CreateElement(group, this);
      control.PointerMove += OnPointerMove;
      control.PointerLeave += (_, _) => {
        pickerMouseDown = false;
        SetHover(null, null);
      };
      control.PointerClick += OnPointerClick;
      // Claim mouse gestures on the expanded ring before the graph editor selects a node.
      control.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
      control.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
      control.Graph.NodeLayoutChanged += (_, _, _) => Invalidate();
    }

    internal void Invalidate() {
      overlay.Invalidate();
      control.Invalidate();
    }

    internal void Reset() {
      SetHover(null, null);
    }

    /// <summary>
    ///   Determines whether to show the multicolored circle overlay for a given node.
    /// </summary>
    private bool HasRing(INode node, out List<int> components) {
      components = null!;
      return !isEndpoint(node) && getResult() is { } result && !result.Gradient.ContainsKey(node) &&
          result.Components.TryGetValue(node, out components!) && components.Count > 1;
    }

    /// <summary>
    ///   Event listener for mouse move events.
    ///   This performs a hit-test to see what item is under the mouse pointer
    ///   and then proceeds accordingly, depending on what has been hit.
    /// </summary>
    private void OnPointerMove(object? sender, PointerEventArgs e) {
      var location = e.Location;
      if (hoveredNode != null && HasRing(hoveredNode, out var previous) && InPopup(hoveredNode, location)) {
        SetHover(hoveredNode, ComponentAt(hoveredNode, previous, location));
        return;
      }

      var item = control.GraphModelManager.HitElementsAt(location, control.InputModeContext).FirstOrDefault();
      if (item is INode node && HasRing(node, out var ids)) {
        SetHover(node, ComponentAt(node, ids, location));
      } else if (item != null && getResult()?.Components.TryGetValue(item, out var itemIds) == true &&
          itemIds.Count > 0) {
        SetHover(null, itemIds[0]);
      } else {
        SetHover(null, null);
      }
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      var view = e.GetPosition(control);
      var location = control.ViewToWorldCoordinates(new PointD(view.X, view.Y));
      if (!TrySelectComponent(location)) {
        return;
      }

      pickerMouseDown = true;
      e.Handled = true;
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      if (!pickerMouseDown) {
        return;
      }

      pickerMouseDown = false;
      e.Handled = true;
    }

    /// <summary>
    ///   Event listener for mouse clicks.
    ///   Clicking on a component color in the popup makes that component display
    ///   in front of others.
    /// </summary>
    private void OnPointerClick(object? sender, PointerEventArgs e) {
      if (e.ChangedButtons == PointerButtons.MouseLeft && TrySelectComponent(e.Location)) {
        e.Handled = true;
      }
    }

    private bool TrySelectComponent(PointD location) {
      if (hoveredNode == null || !HasRing(hoveredNode, out var ids) || !InPopup(hoveredNode, location)) {
        return false;
      }

      var component = ComponentAt(hoveredNode, ids, location);
      // Put the selected component first for each item it shares with other components.
      if (getResult() is { } result) {
        foreach (var memberships in result.Components.Values) {
          if (memberships.Remove(component)) {
            memberships.Insert(0, component);
          }
        }
      }

      SetHover(hoveredNode, component, true);
      return true;
    }

    private void SetHover(INode? node, int? component, bool force = false) {
      if (!force && node == hoveredNode && component == hoveredComponent) {
        return;
      }

      hoveredNode = node;
      hoveredComponent = component;
      highlight(component);
      Invalidate();
    }

    private static bool InPopup(INode node, PointD p) {
      var center = node.Layout.GetCenter();
      var dx = p.X - center.X;
      var dy = p.Y - center.Y;
      var distance = Math.Sqrt(dx * dx + dy * dy);
      return distance > node.Layout.Width * .3 && distance < node.Layout.Width * 1.2;
    }

    private static int ComponentAt(INode node, List<int> ids, PointD p) {
      var center = node.Layout.GetCenter();
      var angle = Math.Atan2(p.X - center.X, center.Y - p.Y);
      if (angle < 0) {
        angle += Math.PI * 2;
      }

      var ordered = ids.OrderBy(id => id).ToArray();
      return ordered[Math.Min((int) (angle / (Math.PI * 2) * ordered.Length), ordered.Length - 1)];
    }

    public Visual CreateVisual(IRenderContext context) => Draw(new DrawingVisual());

    public Visual UpdateVisual(IRenderContext context, Visual? oldVisual) =>
        Draw(oldVisual as DrawingVisual ?? new DrawingVisual());

    private DrawingVisual Draw(DrawingVisual visual) {
      using var dc = visual.RenderOpen();
      foreach (var node in control.Graph.Nodes) {
        if (!HasRing(node, out var ids)) {
          continue;
        }

        var expanded = node == hoveredNode;
        var highlighted = hoveredComponent is { } component && ids.Contains(component);
        var center = node.Layout.GetCenter();
        // Scale the ring and its stroke together so it covers the node's highlight outline.
        var scale = expanded ? 1.8 : highlighted ? 1.25 : 1;
        var radius = (node.Layout.Width * .5 - 2.5) * scale;
        var thickness = (expanded ? 13 : highlighted ? 8 : 6) * scale;
        var ordered = ids.OrderBy(id => id).ToArray();
        for (var i = 0; i < ordered.Length; i++) {
          var a = i * 2 * Math.PI / ordered.Length;
          var b = (i + 1) * 2 * Math.PI / ordered.Length;
          var start = new Point(center.X + Math.Sin(a) * radius, center.Y - Math.Cos(a) * radius);
          var end = new Point(center.X + Math.Sin(b) * radius, center.Y - Math.Cos(b) * radius);
          var path = new StreamGeometry();
          using (var ctx = path.Open()) {
            ctx.BeginFigure(start, false, false);
            ctx.ArcTo(end, new Size(radius, radius), 0, b - a > Math.PI, SweepDirection.Clockwise, true, false);
          }

          dc.DrawGeometry(null, new Pen(color(ordered[i]), thickness), path);
        }

        // Keep the white separator at the node's original size, including while the ring expands.
        var separatorRadius = node.Layout.Width * .5 - 5;
        dc.DrawEllipse(null, new Pen(Brushes.White, 2), new Point(center.X, center.Y), separatorRadius,
            separatorRadius);
      }

      return visual;
    }
  }
}
