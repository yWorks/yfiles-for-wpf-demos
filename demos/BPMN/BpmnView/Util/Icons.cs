/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Demo.yFiles.Graph.Bpmn.Util
{

  /// <summary>
  /// An extension of <see cref="IVisualCreator"/> that allows to set bounds for the visualization.
  /// </summary>
  /// <remarks>
  /// To use this interface for the flyweight pattern, <see cref="SetBounds"/> should be called before creating or updating the visuals.
  /// </remarks>
  internal interface IIcon : IVisualCreator {

    /// <summary>
    /// Sets the bounds the visual shall consider.
    /// </summary>
    /// <param name="bounds"></param>
    void SetBounds(IRectangle bounds);
  }

  internal abstract class IconBase : IIcon {

    public IRectangle Bounds { get; protected set; }

    protected IconBase() {
      Bounds = new MutableRectangle(0, 0, 0, 0);
    }

    public virtual void SetBounds(IRectangle bounds) {
      Bounds = bounds;
    }

    public abstract Visual CreateVisual(IRenderContext context);
    public abstract Visual UpdateVisual(IRenderContext context, Visual oldVisual);
  }

  internal class PathIcon : IconBase
  {
    public Brush Brush { get; set; }

    public Pen Pen { get; set; }

    internal GeneralPath Path { get; set; }

    public override Visual CreateVisual(IRenderContext context) {
      VisualGroup container = new VisualGroup();

      var matrix2D = new Matrix2D();
      matrix2D.Scale(Math.Max(0, Bounds.Width), Math.Max(0, Bounds.Height));

      Path visual = Path.CreatePath(Brush, Pen, matrix2D, FillMode.FillClosedFigures);
      container.Add(visual);

      container.SetRenderDataCache(new PathIconState(Bounds.Width, Bounds.Height, Pen, Brush));
      container.SetCanvasArrangeRect(Bounds.ToRectD());
      return container;
    }

    public override Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      VisualGroup container = oldVisual as VisualGroup;
      if (container == null || container.Children.Count != 1) {
        return CreateVisual(context);
      }

      var path = container.Children.ElementAt(0) as Path;
      var lastState = container.GetRenderDataCache<PathIconState>();
      if (path == null || lastState == null || lastState.width != Bounds.Width || lastState.height != Bounds.Height) {
        return CreateVisual(context);
      }

      if (!Equals(lastState.pen, Pen)) {
        path.SetPen(Pen);  
      }
      if (!Equals(lastState.brush, Brush)) {
        path.Fill = Brush;
      }
      
      // arrange visual
      container.SetCanvasArrangeRect(Bounds.ToRectD());
      return container;
    }
  }


  internal class CombinedIcon : IconBase
  {

    private readonly IList<IIcon> icons;
    
    public CombinedIcon(IList<IIcon> icons) {
      this.icons = icons;
    }

    public override Visual CreateVisual(IRenderContext context) {
      if (Bounds == null) {
        return null;
      }
      var container = new VisualGroup();

      var iconBounds = new RectD(PointD.Origin, Bounds.ToSizeD());
      foreach (var icon in icons) {
        icon.SetBounds(iconBounds);
        container.Add(icon.CreateVisual(context));
      }

      container.SetCanvasArrangeRect(Bounds.ToRectD());
      container.SetRenderDataCache(Bounds.ToRectD());

      return container;
    }

    public override Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      var container = oldVisual as VisualGroup;
      if (container == null || container.Children.Count != icons.Count) {
        return CreateVisual(context);
      }
      var cache = container.GetRenderDataCache<RectD>();

      if (cache.Size != Bounds.ToSizeD()) {
        // size changed -> we have to update the icons
        var iconBounds = new RectD(PointD.Origin, Bounds.ToSizeD());
        int index = 0;
        foreach (var pathIcon in icons) {
          pathIcon.SetBounds(iconBounds);
          var oldPathVisual = container.Children[index];
          var newPathVisual = pathIcon.UpdateVisual(context, oldPathVisual);
          if (!oldPathVisual.Equals(newPathVisual)) {
            newPathVisual = newPathVisual ?? new VisualGroup();
            container.Children.Remove(oldPathVisual);
            container.Children.Insert(index, newPathVisual);
          }
          index++;
        }
      } else if (cache.TopLeft == Bounds.GetTopLeft()) {
        // bounds didn't change at all
        return container;
      }
      container.SetCanvasArrangeRect(Bounds.ToRectD());
      container.SetRenderDataCache(Bounds.ToRectD());

      return container;
    }
  }

  internal class LineUpIcon : IconBase
  {
    private readonly IList<IIcon> icons;
    private readonly SizeD innerIconSize;
    private readonly double gap;

    private readonly SizeD combinedSize;

    public LineUpIcon(IList<IIcon> icons, SizeD innerIconSize, double gap) {
      this.icons = icons;
      this.innerIconSize = innerIconSize;
      this.gap = gap;

      double combinedWidth = icons.Count * innerIconSize.Width + (icons.Count - 1) * gap;
      combinedSize = new SizeD(combinedWidth, innerIconSize.Height);
    }

    public override Visual CreateVisual(IRenderContext context) {
      if (Bounds == null) {
        return null;
      }

      var container = new VisualGroup();

      double offset = 0;
      foreach (var pathIcon in icons) {
        pathIcon.SetBounds(new RectD(offset, 0, innerIconSize.Width, innerIconSize.Height));
        container.Add(pathIcon.CreateVisual(context));
        offset += innerIconSize.Width + gap;
      }
      container.SetCanvasArrangeRect(Bounds.ToRectD());
      container.SetRenderDataCache(Bounds.GetTopLeft());

      return container;
    }

    public override Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      var container = oldVisual as VisualGroup;
      if (container == null || container.Children.Count != icons.Count) {
        return CreateVisual(context);
      }

      var cache = container.GetRenderDataCache<PointD>();
      if (cache != Bounds.GetTopLeft()) {
        container.SetCanvasArrangeRect(Bounds.ToRectD());
        container.SetRenderDataCache(Bounds.GetTopLeft());
      }
      return container;
    }

    public override void SetBounds(IRectangle bounds) {
      base.SetBounds(RectD.FromCenter(bounds.GetCenter(), combinedSize));
    }
  }

  internal class PlacedIcon : IIcon
  {
    private readonly SimpleNode dummyNode;
    private readonly SimpleLabel dummyLabel;
    private readonly ILabelModelParameter placementParameter;
    private readonly IIcon innerIcon;

    public PlacedIcon(IIcon innerIcon, ILabelModelParameter placementParameter, SizeD minimumSize) {
      this.innerIcon = innerIcon;
      this.placementParameter = placementParameter;
      dummyNode = new SimpleNode();
      dummyLabel = new SimpleLabel(dummyNode, "", placementParameter) { PreferredSize = minimumSize };
    }

    public Visual CreateVisual(IRenderContext context) {
      return innerIcon.CreateVisual(context);
    }

    public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      return innerIcon.UpdateVisual(context, oldVisual);
    }

    public virtual void SetBounds(IRectangle bounds) {
      dummyNode.Layout = bounds;
      innerIcon.SetBounds(placementParameter.Model.GetGeometry(dummyLabel, placementParameter).GetBounds());
    }
  }

  internal class RectIcon : IconBase
  {
    internal double CornerRadius { get; set; }

    internal Brush Brush { get; set; }

    internal Pen Pen { get; set; }

    public override Visual CreateVisual(IRenderContext context) {
      VisualGroup container = new VisualGroup();

      var rectangle = new Rectangle { RadiusX = CornerRadius, RadiusY = CornerRadius };
      UpdateRectangle(rectangle);
      container.Add(rectangle);

      container.SetRenderDataCache(new PathIconState(Bounds.Width, Bounds.Height, Pen, Brush));
      container.SetCanvasArrangeRect(Bounds.ToRectD());
      return container;
    }

    public override Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      VisualGroup container = oldVisual as VisualGroup;
      if (container == null || container.Children.Count != 1) {
        return CreateVisual(context);
      }
      var rectangle = container.Children.ElementAt(0) as Rectangle;
      var lastState = container.GetRenderDataCache<PathIconState>();

      if (rectangle == null || lastState == null) {
        container.Children.Clear();
        return CreateVisual(context);
      }

      if (!lastState.Equals(Bounds.Width, Bounds.Height, Pen, Brush)) {
        UpdateRectangle(rectangle);
        container.SetRenderDataCache(new PathIconState(Bounds.Width, Bounds.Height, Pen, Brush));
      }

      // arrange
      container.SetCanvasArrangeRect(Bounds.ToRectD());
      return container;
    }

    private void UpdateRectangle(Rectangle rectangle) {
      rectangle.Width = Bounds.Width;
      rectangle.Height = Bounds.Height;
      rectangle.Fill = Brush;
      if (Pen != null) {
        rectangle.SetPen(Pen);
      } else {
        rectangle.Stroke = null;
      }
    }
  }

  internal class VariableRectIcon : IconBase {
    internal double TopLeftRadius { get; set; }
    internal double TopRightRadius { get; set; }
    internal double BottomLeftRadius { get; set; }
    internal double BottomRightRadius { get; set; }

    internal Brush Brush { get; set; }

    internal Pen Pen { get; set; }

    public override Visual CreateVisual(IRenderContext context) {
      VisualGroup container = new VisualGroup();
      var bounds = Bounds;
      var width = bounds.Width;
      var height = bounds.Height;

      var path = new GeneralPath();
      path.MoveTo(0, TopLeftRadius);
      path.QuadTo(0, 0, TopLeftRadius, 0);
      path.LineTo(width - TopRightRadius, 0);
      path.QuadTo(width, 0, width, TopRightRadius);
      path.LineTo(width, height - BottomRightRadius);
      path.QuadTo(width, height, width - BottomRightRadius, height);
      path.LineTo(BottomLeftRadius, height);
      path.QuadTo(0, height, 0, height - BottomRightRadius);
      path.Close();

      var pathVisual = path.CreatePath(Brush, Pen, new Matrix2D(), FillMode.Always);

      container.Add(pathVisual);

      container.SetRenderDataCache(new PathIconState(width, height, Pen, Brush));
      container.SetCanvasArrangeRect(bounds.ToRectD());
      return container;
    }

    public override Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      VisualGroup container = oldVisual as VisualGroup;
      if (container == null || container.Children.Count != 1) {
        return CreateVisual(context);
      }
      var pathVisual = container.Children.ElementAt(0) as Path;
      var lastState = container.GetRenderDataCache<PathIconState>();

      if (pathVisual == null || lastState == null || !lastState.Equals(Bounds.Width, Bounds.Height, Pen, Brush)) {
        container.Children.Clear();
        return CreateVisual(context);
      }

      // arrange
      container.SetCanvasArrangeRect(Bounds.ToRectD());
      return container;
    }
  }

  internal class DataObjectIcon : IconBase
  {

    internal Brush Brush { get; set; }

    internal Pen Pen { get; set; }


    public override Visual CreateVisual(IRenderContext context) {
      VisualGroup container = new VisualGroup();

      var bounds = Bounds;
      var width = bounds.Width;
      var height = bounds.Height;
      var cornerSize = Math.Min(width, height) * 0.4;

      var path = new GeneralPath();
      path.MoveTo(0, 0);
      path.LineTo(width - cornerSize, 0);
      path.LineTo(width, cornerSize);
      path.LineTo(width, height);
      path.LineTo(0, height);
      path.Close();
      container.Add(path.CreatePath(Brush, Pen, new Matrix2D(), FillMode.Always));

      path = new GeneralPath();
      path.MoveTo(width - cornerSize, 0);
      path.LineTo(width - cornerSize, cornerSize);
      path.LineTo(width, cornerSize);
      container.Add(path.CreatePath(null, Pen, new Matrix2D(), FillMode.Never));

      container.SetRenderDataCache(new PathIconState(width, height, Pen, Brush));
      container.SetCanvasArrangeRect(bounds.ToRectD());
      return container;
    }

    public override Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      return CreateVisual(context);
    }
  }

  internal class CollapseButtonIcon : IconBase
  {

    private readonly IIcon collapsedIcon;
    private readonly IIcon expandedIcon;

    private readonly INode node;
    private readonly Brush iconBrush;

    public CollapseButtonIcon(INode node, Brush iconBrush) {
      this.node = node;
      this.iconBrush = iconBrush;
      collapsedIcon = IconFactory.CreateStaticSubState(SubState.Collapsed, iconBrush);
      expandedIcon = IconFactory.CreateStaticSubState(SubState.Expanded, iconBrush);
    }

    public override Visual CreateVisual(IRenderContext context) {
      collapsedIcon.SetBounds(new RectD(PointD.Origin, Bounds.GetSize()));
      expandedIcon.SetBounds(new RectD(PointD.Origin, Bounds.GetSize()));
      var button = CreateButton(context, node, collapsedIcon.CreateVisual(context), expandedIcon.CreateVisual(context));
      button.SetCanvasArrangeRect(Bounds.ToRectD());
      return button;
    }

    public override Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      var button = oldVisual as VisualToggleButton;
      if (button == null) {
        return CreateVisual(context);
      }

      collapsedIcon.SetBounds(new RectD(PointD.Origin, Bounds.GetSize()));
      expandedIcon.SetBounds(new RectD(PointD.Origin, Bounds.GetSize()));

      button.CheckedVisual = collapsedIcon.UpdateVisual(context, button.CheckedVisual);
      button.UncheckedVisual = expandedIcon.UpdateVisual(context, button.UncheckedVisual);

      button.SetCanvasArrangeRect(Bounds.ToRectD());
      return button;
    }

    protected virtual VisualToggleButton CreateButton(IRenderContext context, INode item, Visual collapsedVisual, Visual expandedVisual) {
      var button = new VisualToggleButton {CommandParameter = item, Command = GraphCommands.ToggleExpansionState, CheckedVisual = collapsedVisual, UncheckedVisual = expandedVisual, Background = iconBrush};

      bool expanded = true;
      var canvas = context != null ? context.CanvasControl : null;

      if (canvas != null) {
        button.CommandTarget = canvas;

        IGraph graph = canvas.Lookup(typeof(IGraph)) as IGraph;
        if (graph != null) {
          IFoldingView foldingView = graph.Lookup<IFoldingView>();
          if (foldingView != null && foldingView.Graph.Contains(item)) {
            expanded = foldingView.IsExpanded(item);
          }
        }
      } else {
        button.Content = "+";
      }

      button.IsChecked = !expanded;
      return button;
    }
  }

  internal class PathIconState {
    public double width;
    public double height;
    public Pen pen;
    public Brush brush;

    public PathIconState(double width, double height, Pen pen, Brush brush) {
      this.width = width;
      this.height = height;
      this.pen = pen;
      this.brush = brush;
    }

    public bool Equals(double width, double height, Pen pen, Brush brush) {
      return this.width == width && this.height == height && this.pen == pen && this.brush == brush;
    }
  }
}
