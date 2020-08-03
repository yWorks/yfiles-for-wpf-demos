/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Bpmn.Editor
{
  /// <summary>Custom panel that serves as drag source for drag and drop</summary>
  public class NodeStylePanel : Panel
  {
    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item",
        typeof (INode),
        typeof (NodeStylePanel),
        new PropertyMetadata(
            OnItemChanged));

    private readonly INode dummyNode;
    private readonly GraphControl graphControl;

    public NodeStylePanel() {
      ClipToBounds = true;
      // Create a dummy graph control that holds the item representation as an INode
      // We just hard code the necessary sizes for simplicity
      graphControl = new GraphControl
      {
        HorizontalScrollBarPolicy = ScrollBarVisibility.Hidden,
        VerticalScrollBarPolicy = ScrollBarVisibility.Hidden,
        ContentRect = new RectD(0, 0, 150, 150)
      };
      MaxWidth = MaxHeight = 150;
      MinWidth = 150;
      MinHeight = 80;
      HorizontalAlignment = HorizontalAlignment.Stretch;
      VerticalAlignment = VerticalAlignment.Stretch;
      Children.Add(new StackPanel());


      var dragRect = new Rectangle { Fill = Brushes.Transparent };
      dragRect.MouseMove += DragRectOnMouseMove;
      Children.Add(dragRect);
      SizeChanged += OnSizeChanged;
      dummyNode = graphControl.Graph.CreateNode(new RectD(10, 10, 30, 30));
      graphControl.ContentRect = new RectD(5, 5, 150, 150);
    }

    public INode Item {
      get { return GetValue(ItemProperty) as INode; }
      set { SetValue(ItemProperty, value); }
    }

    private VisualGroup CreateVisual(CanvasControl canvas) {
      RectD rect = canvas.ContentRect;
      ContextConfigurator configurator = new ContextConfigurator(rect) { Margins = new InsetsD(0, 0, 0, 0) };
      // scale down if necessary
      if (ActualHeight > 0 && ActualHeight > 0) {
        if (ActualHeight < rect.Height || ActualWidth < rect.Width) {
          configurator.Scale = Math.Min(ActualWidth / rect.Width, ActualHeight / rect.Height);
        }
        RectD bounds = RectD.FromCenter(rect.Center,
            new SizeD(ActualWidth, ActualHeight) * (1 / configurator.Scale));
        configurator.WorldBounds = bounds;
      }

      IRenderContext vc = configurator.CreateRenderContext(canvas);
      Transform transform = configurator.CreateWorldToIntermediateTransform();
      Visual visual = canvas.ExportContent(vc);
      return new VisualGroup { Children = { visual }, Clip = Clip, Transform = transform };
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
      UpdateChild();
    }

    private void UpdateChild() {
      ApplyItem();
      Children.RemoveAt(0);
      graphControl.FlowDirection = FlowDirection;
      Children.Insert(0, CreateVisual(graphControl));
    }

    /// <summary>
    /// Populate the Drag 'n' Drop data structures when a drag is actually started.
    /// </summary>
    private void DragRectOnMouseMove(object sender, MouseEventArgs mouseEventArgs) {
      if (mouseEventArgs.LeftButton == MouseButtonState.Pressed) {
        if (Item.Tag is IStripe) {
          //If the dummy node has a stripe as its tag, we use the stripe directly
          //This allows StripeDropInputMode to take over
          var dao = new DataObject();
          dao.SetData(typeof (IStripe), Item.Tag);
          DragDrop.DoDragDrop(this, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        } else {
          //Otherwise, we just use the node itself and let (hopefully) NodeDropInputMode take over
          var dao = new DataObject();
          dao.SetData(typeof (INode),
              new SimpleNode { Layout = Item.Layout, Style = (INodeStyle) Item.Style.Clone(), Tag = Item.Tag });
          DragDrop.DoDragDrop(this, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        }
      }
    }


    private static void OnItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) {
      if (args.NewValue is IModelItem) {
        var stylePanel = ((NodeStylePanel) dependencyObject);
        stylePanel.UpdateChild();
      }
    }

    private void ApplyItem() {
      graphControl.Graph.SetStyle(dummyNode, Item.Style);
      foreach (var label in new List<ILabel>(dummyNode.Labels)) {
        graphControl.Graph.Remove(label);
      }
      dummyNode.Tag = Item.Tag;
      graphControl.Graph.SetNodeLayout(dummyNode, Item.Layout.ToRectD());
      foreach (var label in Item.Labels) {
        graphControl.Graph.AddLabel(dummyNode, label.Text, label.LayoutParameter,
            label.Style, label.PreferredSize, label.Tag);
      }
      graphControl.ContentRect = dummyNode.Layout.ToRectD().GetEnlarged(5);
    }

    protected override Size MeasureOverride(Size availableSize) {
      foreach (UIElement child in Children) {
        child.Measure(availableSize);
      }
      return new Size(MinWidth, MinHeight);
    }

    protected override Size ArrangeOverride(Size finalSize) {
      foreach (UIElement child  in Children) {
        child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
      }
      return base.ArrangeOverride(finalSize);
    }
  }
}