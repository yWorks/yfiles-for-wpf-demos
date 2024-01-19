/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.Graph.Editor
{
  public abstract class StylePanel<TItemType> : Panel where TItemType : class
  {
    protected readonly GraphControl graphControl;
    private readonly System.Windows.Shapes.Rectangle dragRect;

    protected StylePanel() {
      this.ClipToBounds = true;
      graphControl = new GraphControl();
      graphControl.HorizontalScrollBarPolicy = ScrollBarVisibility.Hidden;
      graphControl.VerticalScrollBarPolicy = ScrollBarVisibility.Hidden;
      graphControl.ContentRect = new RectD(0, 0, 50, 50);
      this.MaxWidth = this.MaxHeight = 50;
      this.MinWidth = this.MinHeight = 10;
      HorizontalAlignment = HorizontalAlignment.Stretch;
      VerticalAlignment = VerticalAlignment.Stretch;

      this.Children.Add(new StackPanel());
      dragRect = new System.Windows.Shapes.Rectangle() { Fill = Brushes.Transparent };

      dragRect.MouseMove += DragRectOnMouseMove;
      this.Children.Add(dragRect);
      var menuItem = new MenuItem(){Command=GraphEditor.GraphEditorWindow.ApplyStyleCommand};
      menuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding() {Source = this, Path = new PropertyPath(ItemProperty.Name)});
      this.ContextMenu = new ContextMenu() {Items = {menuItem}};
      this.SizeChanged += OnSizeChanged;
    }

    protected VisualGroup CreateVisual(GraphControl canvas) {
      RectD rect = canvas.ContentRect;
      ContextConfigurator configurator = new ContextConfigurator(rect);
      configurator.Margins = new InsetsD(0, 0, 0, 0);
      // scale down if necessary
      if (ActualHeight > 0 && ActualHeight > 0) {
        if (ActualHeight < rect.Height || ActualWidth < rect.Width) {
          configurator.Scale = Math.Min(this.ActualWidth / rect.Width, this.ActualHeight / rect.Height);
        }
        RectD bounds = RectD.FromCenter(rect.Center,
                                        new SizeD(this.ActualWidth, this.ActualHeight) * (1 / configurator.Scale));
        configurator.WorldBounds = bounds;
      }

      IRenderContext vc = configurator.CreateRenderContext(canvas);
      Transform transform = configurator.CreateWorldToIntermediateTransform();
      Visual visual = canvas.ExportContent(vc);
      return new VisualGroup() {Children = {visual}, Clip = Clip, Transform = transform};
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
      UpdateChild();
    }

    private void UpdateChild() {
      ApplyItem(Item);
      this.Children.RemoveAt(0);
      graphControl.FlowDirection = this.FlowDirection;
      this.Children.Insert(0, CreateVisual(graphControl));
    }

    protected virtual void DragRectOnMouseMove(object sender, MouseEventArgs mouseEventArgs) {
      if (mouseEventArgs.LeftButton == MouseButtonState.Pressed) {
        DataObject data = new DataObject(typeof(TItemType), Item);
        DragDrop.DoDragDrop(this, data, DragDropEffects.All);
      }
    }

    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item",
                                                                                                typeof(TItemType),
                                                                                                typeof(StylePanel<TItemType>),
                                                                                                new PropertyMetadata(OnItemChanged));



    private static void OnItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) {
      if (args.NewValue is TItemType) {
        var stylePanel = ((StylePanel<TItemType>)dependencyObject);
        stylePanel.UpdateChild();
      }
    }

    protected abstract void ApplyItem(TItemType style);

    public TItemType Item {
      get { return GetValue(ItemProperty) as TItemType; }
      set { SetValue(ItemProperty, value); }
    }

    protected override Size MeasureOverride(Size availableSize) {
      foreach (UIElement child in Children) {
        child.Measure(availableSize);
      }
      return new Size(Math.Max(40, graphControl.DesiredSize.Width), Math.Max(40, graphControl.DesiredSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize) {
      foreach (UIElement child  in Children) {
        child.Arrange(new Rect(0,0, finalSize.Width, finalSize.Height));
      }
      return base.ArrangeOverride(finalSize);
    }
  }

  public class NodeStylePanel : StylePanel<INode>
  {
    private readonly INode dummyNode;

    public NodeStylePanel() {
      this.dummyNode = graphControl.Graph.CreateNode(new RectD(10, 10, 30, 30));
      graphControl.ContentRect = new RectD(5,5,40,40);
    }

    protected override void ApplyItem(INode node) {
      graphControl.Graph.SetStyle(dummyNode, node.Style);
      foreach (var label in new List<ILabel>(dummyNode.Labels)) {
        graphControl.Graph.Remove(label);
      }
      dummyNode.Tag = node.Tag;
      graphControl.Graph.SetNodeLayout(dummyNode, node.Layout.ToRectD());
      foreach (var label in node.Labels) {
        graphControl.Graph.AddLabel(dummyNode, label.Text, label.LayoutParameter,
                                    label.Style, label.PreferredSize, label.Tag);
      }
      graphControl.ContentRect = dummyNode.Layout.ToRectD().GetEnlarged(5);
    }
  }
  
  public class EdgeStylePanel : StylePanel<IEdgeStyle>
  {
    private readonly IEdge dummyEdge;

    public EdgeStylePanel() {
      IGraph graph = graphControl.Graph;
      graph.NodeDefaults.Style = VoidNodeStyle.Instance;
      this.dummyEdge = graph.CreateEdge(
        graph.CreateNode(new RectD(10, 10, 0, 0)),
        graph.CreateNode(new RectD(50, 30, 0, 0)));
      graph.AddBend(dummyEdge, new PointD(30, 10), 0);
      graph.AddBend(dummyEdge, new PointD(30, 30), 1);

      graphControl.ContentRect = new RectD(5, 5, 50, 30);
    }

    protected override void ApplyItem(IEdgeStyle style) {
      graphControl.Graph.SetStyle(dummyEdge, style);
      graphControl.FitContent();
    }
  }

  public class ArrowPanel : StylePanel<IArrow>
  {
    private readonly PolylineEdgeStyle dummyEdgeStyle;

    public ArrowPanel() {
      IGraph graph = graphControl.Graph;
      graphControl.ContentRect = new RectD(0,0, 60, 30);
      graph.NodeDefaults.Style = VoidNodeStyle.Instance;
      dummyEdgeStyle = new PolylineEdgeStyle();
      graph.CreateEdge(
        graph.CreateNode(new RectD(10, 15, 0, 0)),
        graph.CreateNode(new RectD(50, 15, 0, 0)), dummyEdgeStyle);
    }

    protected override void ApplyItem(IArrow style) {
      dummyEdgeStyle.TargetArrow = style;
    }
  }

  public class LabelStylePanel : StylePanel<ILabelStyle>
  {
    private readonly ILabel dummyLabel;

    public LabelStylePanel() {
      var dummyNode = graphControl.Graph.CreateNode(new RectD(10, 10, 50, 30), VoidNodeStyle.Instance);
      this.dummyLabel = graphControl.Graph.AddLabel(dummyNode, "Label", InteriorLabelModel.Center);
      this.graphControl.ContentRect = new RectD(0, 0, 70, 50);
    }

    protected override void ApplyItem(ILabelStyle style) {
      graphControl.Graph.SetStyle(dummyLabel, style);
    }
  }

  public class PortStylePanel : StylePanel<IPortStyle> {
    private readonly IPort dummyPort;

    public PortStylePanel() {
      var dummyNode = graphControl.Graph.CreateNode(new RectD(10, 10, 50, 30), VoidNodeStyle.Instance);
      this.dummyPort = graphControl.Graph.AddPort(dummyNode, FreeNodePortLocationModel.NodeCenterAnchored);
      this.graphControl.ContentRect = new RectD(0, 0, 70, 50);
    }

    protected override void ApplyItem(IPortStyle style) {
      graphControl.Graph.SetStyle(dummyPort, style);
    }

    protected override void DragRectOnMouseMove(object sender, MouseEventArgs mouseEventArgs) {
      if (mouseEventArgs.LeftButton == MouseButtonState.Pressed) {
        DataObject data = new DataObject(typeof(IPort), dummyPort);
        DragDrop.DoDragDrop(this, data, DragDropEffects.All);
      }
    }
  }
}