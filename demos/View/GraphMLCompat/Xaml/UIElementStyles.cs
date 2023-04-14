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
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml
{
  [ContentProperty("Template")]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class UIElementNodeStyle : NodeStyleBase<UIElement>, INotifyPropertyChanged
  {
    private object styleTag;
    private DataTemplate template;
    private DataTemplateSelector templateSelector;

    public InsetsD Insets { get; set; }
    public Shape OutlineShape { get; set; }

    public object StyleTag {
      get { return styleTag; }
      set {
        styleTag = value;
        OnPropertyChanged("StyleTag");
      }
    }

    public IContextLookup ContextLookup { get; set; }

    public DataTemplate Template {
      get { return template; }
      set {
        template = value;
        OnPropertyChanged("Template");
      }
    }

    public DataTemplateSelector TemplateSelector {
      get { return templateSelector; }
      set {
        templateSelector = value;
        OnPropertyChanged("TemplateSelector");
      }
    }

    /// <summary>
    /// Not used anymore.
    /// </summary>
    public object UserTagProvider { get; set; }

    public UIElementNodeStyle() {
      Insets = new InsetsD(5);
    }

    protected override UIElement CreateVisual(IRenderContext context, INode node) {
      var style = this;
      var layout = new Rect(node.Layout.X, node.Layout.Y, node.Layout.Width, node.Layout.Height);
      var dataContext = CreateContext(context, node);

      ContentPresenter presenter = new ContentPresenter();
      presenter.SetValue(CanvasControl.CanvasControlArrangeRectProperty, layout);
      if (style.Template != null) {
        presenter.ContentTemplate = style.Template;
      } else if (style.TemplateSelector != null) {
        presenter.ContentTemplate = style.TemplateSelector.SelectTemplate(dataContext, presenter);
      }
      presenter.Content = dataContext;
      presenter.Measure(new Size(layout.Width, layout.Height));
      presenter.Arrange(layout);
      return presenter;
    }

    protected override UIElement UpdateVisual(IRenderContext context, UIElement oldVisual, INode node) {
      var layout = new Rect(node.Layout.X, node.Layout.Y, node.Layout.Width, node.Layout.Height);

      double lWidth = layout.Width;
      double lHeight = layout.Height;
      oldVisual.SetValue(CanvasControl.CanvasControlArrangeRectProperty, layout);
      if (oldVisual is FrameworkElement) {
        FrameworkElement fe = (FrameworkElement) oldVisual;
        if (!fe.IsMeasureValid || (fe.ActualWidth != lWidth || fe.ActualHeight != lHeight)) {
          oldVisual.Measure(new Size(lWidth, lHeight));
        }
      } else {
        oldVisual.Measure(new Size(lWidth, lHeight));
      }
      oldVisual.Arrange(layout);
      return oldVisual;
    }

    private UIElementNodeStyleDataContext CreateContext(IRenderContext context, INode node) {
      return new UIElementNodeStyleDataContext(context, node, this);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  [ContentProperty("Template")]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class UIElementPortStyle : PortStyleBase<UIElement>, INotifyPropertyChanged
  {
    private object styleTag;
    private DataTemplate template;
    private DataTemplateSelector templateSelector;

    public SizeD RenderSize { get; set; }
    public Shape OutlineShape { get; set; }
    public object StyleTag {
      get { return styleTag; }
      set {
        styleTag = value;
        OnPropertyChanged("StyleTag");
      }
    }

    public IContextLookup ContextLookup { get; set; }

    public DataTemplate Template {
      get { return template; }
      set {
        template = value;
        OnPropertyChanged("Template");
      }
    }

    public DataTemplateSelector TemplateSelector {
      get { return templateSelector; }
      set {
        templateSelector = value;
        OnPropertyChanged("TemplateSelector");
      }
    }

    /// <summary>
    /// Not used anymore.
    /// </summary>
    public object UserTagProvider { get; set; }

    public UIElementPortStyle() {
      RenderSize = new SizeD(10, 10);
    }

    protected override UIElement CreateVisual(IRenderContext context, IPort port) {
      var style = this;
      var pl = port.GetLocation();
      var layout = new Rect(pl.X - RenderSize.Width * 0.5, pl.Y - RenderSize.Height * 0.5, RenderSize.Width, RenderSize.Height);
      var dataContext = CreateContext(context, port);

      ContentPresenter presenter = new ContentPresenter();
      presenter.SetValue(CanvasControl.CanvasControlArrangeRectProperty, layout);
      if (style.Template != null) {
        presenter.ContentTemplate = style.Template;
      } else if (style.TemplateSelector != null) {
        presenter.ContentTemplate = style.TemplateSelector.SelectTemplate(dataContext, presenter);
      }
      presenter.Content = dataContext;
      presenter.Measure(new Size(layout.Width, layout.Height));
      presenter.Arrange(layout);
      return presenter;
    }

    protected override UIElement UpdateVisual(IRenderContext context, UIElement oldVisual, IPort port) {
      var pl = port.GetLocation();
      var layout = new Rect(pl.X - RenderSize.Width * 0.5, pl.Y - RenderSize.Height * 0.5, RenderSize.Width, RenderSize.Height);

      double lWidth = layout.Width;
      double lHeight = layout.Height;
      oldVisual.SetValue(CanvasControl.CanvasControlArrangeRectProperty, layout);
      if (oldVisual is FrameworkElement) {
        FrameworkElement fe = (FrameworkElement) oldVisual;
        if (!fe.IsMeasureValid || (fe.ActualWidth != lWidth || fe.ActualHeight != lHeight)) {
          oldVisual.Measure(new Size(lWidth, lHeight));
        }
      } else {
        oldVisual.Measure(new Size(lWidth, lHeight));
      }
      oldVisual.Arrange(layout);
      return oldVisual;
    }

    protected override RectD GetBounds(ICanvasContext context, IPort port) {
      var pl = port.GetLocation();
      return new RectD(pl.X - RenderSize.Width * 0.5, pl.Y - RenderSize.Height * 0.5, RenderSize.Width, RenderSize.Height);
    }

    private UIElementPortStyleDataContext CreateContext(IRenderContext context, IPort port) {
      return new UIElementPortStyleDataContext(context, port, this);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  [ContentProperty("Template")]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class UIElementLabelStyle : LabelStyleBase<UIElement>, INotifyPropertyChanged
  {
    private object styleTag;
    private DataTemplate template;
    private DataTemplateSelector templateSelector;
    private bool autoFlip = true;

    public bool AutoFlip {
      get { return autoFlip; }
      set {
        autoFlip = value;
        OnPropertyChanged("AutoFlip");
      }
    }

    public Shape OutlineShape { get; set; }
    public object StyleTag {
      get { return styleTag; }
      set {
        styleTag = value;
        OnPropertyChanged("StyleTag");
      }
    }

    public IContextLookup ContextLookup { get; set; }

    public DataTemplate Template {
      get { return template; }
      set {
        template = value;
        OnPropertyChanged("Template");
      }
    }

    public DataTemplateSelector TemplateSelector {
      get { return templateSelector; }
      set {
        templateSelector = value;
        OnPropertyChanged("TemplateSelector");
      }
    }

    /// <summary>
    /// Not used anymore.
    /// </summary>
    public object UserTagProvider { get; set; }

    protected override UIElement CreateVisual(IRenderContext context, ILabel label) {
      var layout = label.GetLayout();
      if (layout.Width < 0 || layout.Height < 0) {
        return null;
      }
      var style = this;
      var rect = new Rect(0, 0, layout.Width, layout.Height);
      var dataContext = CreateContext(context, label);

      ContentPresenter presenter = new ContentPresenter();
      presenter.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
      if (style.Template != null) {
        presenter.ContentTemplate = style.Template;
      } else if (style.TemplateSelector != null) {
        presenter.ContentTemplate = style.TemplateSelector.SelectTemplate(dataContext, presenter);
      }
      presenter.Content = dataContext;
      presenter.Measure(new Size(rect.Width, rect.Height));
      presenter.Arrange(rect);

      ArrangeByLayout(presenter, layout, AutoFlip);
      return presenter;
    }

    internal static void ArrangeByLayout(UIElement element, IOrientedRectangle layout, bool autoFlip) {
      if (layout.UpY == -1) {
        Rect rect = new Rect(layout.AnchorX, layout.AnchorY - layout.Height, layout.Width, layout.Height);
        element.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
        element.Arrange(rect);
      } else {
        Matrix matrix;
        if (autoFlip && layout.UpY > 0) {
          Rect rect = new Rect(0, -layout.Height, layout.Width, layout.Height);
          element.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
          element.Arrange(rect);
          matrix =
              new Matrix(-layout.UpY, layout.UpX, -layout.UpX, -layout.UpY, -layout.UpX * layout.Height,
                -layout.UpY * layout.Height);
          matrix.Translate(layout.AnchorX, layout.AnchorY + layout.Height);
          matrix.ScaleAtPrepend(-1, -1, layout.Width * 0.5d, -layout.Height * 0.5d);
        } else {
          Rect rect = new Rect(0, -layout.Height, layout.Width, layout.Height);
          element.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
          element.Arrange(rect);
          matrix =
              new Matrix(-layout.UpY, layout.UpX, -layout.UpX, -layout.UpY, layout.UpX * layout.Height,
                layout.UpY * layout.Height);
          matrix.Translate(layout.AnchorX, layout.AnchorY + layout.Height);
        }
        MatrixTransform transform = element.RenderTransform as MatrixTransform;
        if (transform != null && !transform.IsFrozen) {
          transform.Matrix = matrix;
        } else {
          transform = new MatrixTransform(matrix);
          element.RenderTransform = transform;
        }
      }
    }


    protected override UIElement UpdateVisual(IRenderContext context, UIElement oldVisual, ILabel label) {
      UIElement element = oldVisual;
      var layout = label.GetLayout();
      // triggers some internal state stuff, e.g. in Control.Image
      double lWidth = layout.Width;
      double lHeight = layout.Height;
      if (element is FrameworkElement) {
        FrameworkElement fe = (FrameworkElement) element;
        if (fe.ActualWidth != lWidth || fe.ActualHeight != lHeight) {
          element.Measure(new Size(lWidth, lHeight));
        }
      } else {
        element.Measure(new Size(lWidth, lHeight));
      }
      if (layout.UpY == -1) {
        Rect rect = new Rect(layout.AnchorX, layout.AnchorY - layout.Height, layout.Width, layout.Height);
        element.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
        element.Arrange(rect);
        element.RenderTransform = Transform.Identity;
      } else {
        Matrix matrix;
        Rect rect = new Rect(0, -layout.Height, layout.Width, layout.Height);
        if (AutoFlip && layout.UpY > 0) {
          element.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
          element.Arrange(rect);
          matrix =
              new Matrix(-layout.UpY, layout.UpX, -layout.UpX, -layout.UpY, -layout.UpX * layout.Height,
                -layout.UpY * layout.Height);
          matrix.Translate(layout.AnchorX, layout.AnchorY + layout.Height);
          matrix.ScaleAtPrepend(-1, -1, layout.Width * 0.5d, -layout.Height * 0.5d);
        } else {
          element.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
          element.Arrange(rect);
          matrix =
              new Matrix(-layout.UpY, layout.UpX, -layout.UpX, -layout.UpY, layout.UpX * layout.Height,
                layout.UpY * layout.Height);
          matrix.Translate(layout.AnchorX, layout.AnchorY + layout.Height);
        }
        MatrixTransform transform = element.RenderTransform as MatrixTransform;
        if (transform != null && !transform.IsFrozen) {
          transform.Matrix = matrix;
        } else {
          transform = new MatrixTransform(matrix);
          element.RenderTransform = transform;
        }
      }
      return element;
    }

    protected override SizeD GetPreferredSize(ILabel label) {
      Visual visual;
      var layout = label.GetLayout();
      if (layout.Width < 0 || layout.Height < 0) {
        // do the shortcut instead.
        var rect = new Rect(0, 0, 1000, 1000);
        var dataContext = CreateContext(null, label);
        ContentPresenter presenter = new ContentPresenter();
        presenter.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
        if (Template != null) {
          presenter.ContentTemplate = Template;
        } else if (TemplateSelector != null) {
          presenter.ContentTemplate = TemplateSelector.SelectTemplate(dataContext, presenter);
        }
        presenter.Content = dataContext;
        presenter.Measure(new Size(rect.Width, rect.Height));
        presenter.Arrange(rect);

        visual = presenter;
      } else {
        visual = CreateVisual(null, label);
      }

      if (visual is UIElement) {
        UIElement uiElement = (UIElement) visual;
        uiElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Size ds = uiElement.DesiredSize;
        if (uiElement.IsMeasureValid) {
          return
              new SizeD(Math.Round(ds.Width + 0.5d, MidpointRounding.AwayFromZero),
                Math.Round(ds.Height + 0.5d, MidpointRounding.AwayFromZero));
        }
      }
      return new SizeD(50, 10);
    }

    private UIElementLabelStyleDataContext CreateContext(IRenderContext context, ILabel label) {
      return new UIElementLabelStyleDataContext(context, label, this);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  [ContentProperty("Template")]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class UIElementEdgeStyle : EdgeStyleBase<PathVisual>, INotifyPropertyChanged
  {
    private object styleTag;
    private DataTemplate template;
    private DataTemplateSelector templateSelector;

    public IEdgeStyle PathStyle { get; set; }
    public double SegmentWidth { get; set; }

    public object StyleTag {
      get { return styleTag; }
      set {
        styleTag = value;
        OnPropertyChanged("StyleTag");
      }
    }

    public IContextLookup ContextLookup { get; set; }

    public DataTemplate Template {
      get { return template; }
      set {
        template = value;
        OnPropertyChanged("Template");
      }
    }

    public DataTemplateSelector TemplateSelector {
      get { return templateSelector; }
      set {
        templateSelector = value;
        OnPropertyChanged("TemplateSelector");
      }
    }

    /// <summary>
    /// Not used anymore.
    /// </summary>
    public object UserTagProvider { get; set; }

    public UIElementEdgeStyle() {
      SegmentWidth = 1;
    }

    protected override PathVisual CreateVisual(IRenderContext context, IEdge edge) {
      IPathGeometry geometry = PathStyle.Renderer.GetPathGeometry(edge, PathStyle);
      return new PathVisual(CreateContext(context, edge)).Update(geometry.GetPath());
    }

    protected override PathVisual UpdateVisual(IRenderContext context, PathVisual oldVisual, IEdge edge) {
      PathVisual visual = oldVisual;
      IPathGeometry geometry = PathStyle.Renderer.GetPathGeometry(edge, PathStyle);
      return visual.Update(geometry.GetPath());
    }

    private UIElementEdgeStyleDataContext CreateContext(IRenderContext context, IEdge edge) {
      return new UIElementEdgeStyleDataContext(context, edge, this);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public sealed class PathVisual : UIElement
  {
    private readonly List<SegmentVisual> segments = new List<SegmentVisual>();
    private readonly UIElementEdgeStyleDataContext edgeContext;
    private GeneralPath lastPath = new GeneralPath(0);

    static PathVisual() {
      CanvasControl.CanvasControlArrangeRectProperty.OverrideMetadata(typeof(PathVisual), new FrameworkPropertyMetadata(new Rect(0, 0, 1, 1), FrameworkPropertyMetadataOptions.None));
    }

    public PathVisual(UIElementEdgeStyleDataContext edgeContext) {
      this.edgeContext = edgeContext;
    }

    protected override void ArrangeCore(Rect finalRect) {
      foreach (SegmentVisual child in segments) {
        Rect rect = (Rect) child.GetValue(CanvasControl.CanvasControlArrangeRectProperty);
        if (rect.Width > 0 || rect.Height > 0) {
          child.Arrange(rect);
        }
      }
      base.ArrangeCore(finalRect);
    }

    protected override System.Windows.Size MeasureCore(System.Windows.Size availableSize) {
      foreach (SegmentVisual child in segments) {
        Rect rect = (Rect) child.GetValue(CanvasControl.CanvasControlArrangeRectProperty);
        if (rect.Width > 0 || rect.Height > 0) {
          child.Measure(new System.Windows.Size(rect.Width, rect.Height));
        }
      }
      return base.MeasureCore(availableSize);
    }


    internal PathVisual Update(GeneralPath path) {
      if (path == lastPath || lastPath.IsEquivalentTo(path)) {
        return this;
      } else {
        lastPath = path != null ? (GeneralPath) path.Clone() : null;
      }
      GeneralPath.PathCursor cursor = path.CreateCursor();
      double lastX = 0, lastY = 0, lastMoveX = 0, lastMoveY = 0;
      double[] coordinates = new double[6];
      int segmentIndex = 0;
      while (cursor.MoveNext()) {
        PathType current = cursor.GetCurrent(coordinates);
        switch (current) {
          case PathType.MoveTo:
            lastX = lastMoveX = coordinates[0];
            lastY = lastMoveY = coordinates[1];
            break;
          case PathType.LineTo: {
            double newX = coordinates[0];
            double newY = coordinates[1];
            CreateSegment(segmentIndex, lastX, lastY, newX, newY);
            segmentIndex++;
            lastX = newX;
            lastY = newY;
          }
            break;
          case PathType.QuadTo: {
            double newX = coordinates[2];
            double newY = coordinates[3];
            CreateSegment(segmentIndex, lastX, lastY, newX, newY);
            segmentIndex++;
            lastX = newX;
            lastY = newY;
            break;
          }
          case PathType.CubicTo: {
            double newX = coordinates[4];
            double newY = coordinates[5];
            CreateSegment(segmentIndex, lastX, lastY, newX, newY);
            segmentIndex++;
            lastX = newX;
            lastY = newY;
          }
            break;
          case PathType.Close: {
            double newX = lastMoveX;
            double newY = lastMoveY;
            CreateSegment(segmentIndex, lastX, lastY, newX, newY);
            segmentIndex++;
            lastX = newX;
            lastY = newY;
          }
            break;
          default:
            // ignore
            break;
        }
      }
      if (edgeContext.SegmentCount != segmentIndex) {
        if (edgeContext.SegmentCount > 0) {
          segments[edgeContext.SegmentCount - 1].SetLastSegment(false);
        }
        if (segmentIndex > 0) {
          segments[segmentIndex - 1].SetLastSegment(true);
        }
        edgeContext.SegmentCount = segmentIndex;
      }
      while (segmentIndex < segments.Count) {
        SegmentVisual segment = segments[segmentIndex];
        segments.RemoveAt(segmentIndex);
        RemoveVisualChild(segment);
        OnVisualChildrenChanged(null, segment);
      }
      return this;
    }

    private void CreateSegment(int segmentIndex, double lastX, double lastY, double newX, double newY) {
      SegmentVisual segmentVisual;
      if (segmentIndex >= segments.Count) {
        segmentVisual = new SegmentVisual(edgeContext.CreateSegmentContext(segmentIndex));
        segments.Add(segmentVisual);
        AddVisualChild(segmentVisual);
        OnVisualChildrenChanged(segmentVisual, null);
      } else {
        segmentVisual = segments[segmentIndex];
      }
      segmentVisual.Update(lastX, lastY, newX, newY);
    }

    protected override Visual GetVisualChild(int index) {
      return segments[index];
    }

    protected override int VisualChildrenCount {
      get { return segments.Count; }
    }

    private sealed class SegmentVisual : ContentPresenter
    {
      private readonly UIElementEdgeStyleSegmentDataContext context;

      public SegmentVisual(UIElementEdgeStyleSegmentDataContext context) {
        this.context = context;
        this.RenderTransform = new MatrixTransform();
        this.ContentTemplate = context.EdgeContext.Style.Template;
        this.ContentTemplateSelector = context.EdgeContext.Style.TemplateSelector;
        this.Content = context;
      }

      public void Update(double lastX, double lastY, double newX, double newY) {
        double width = ((UIElementEdgeStyle) context.EdgeContext.Style).SegmentWidth;
        double dx = newX - lastX;
        double dy = newY - lastY;
        double l = Math.Sqrt(dx * dx + dy * dy);
        if (l > 1) {
          Visibility = Visibility.Visible;
          Measure(new Size(l, width));
          Rect rect = new Rect(0, 0, l, width);
          Arrange(rect);
          SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect);
          double UpY = -dx / l;
          double UpX = dy / l;
          Matrix matrix =
              new Matrix(-UpY, UpX, -UpX, -UpY, UpX * width * 0.5,
                UpY * width * 0.5);
          matrix.Translate(lastX, lastY);
          ((MatrixTransform) this.RenderTransform).Matrix = matrix;
        } else {
          SetValue(CanvasControl.CanvasControlArrangeRectProperty, new Rect(0, 0, 0, 0));
          Visibility = Visibility.Collapsed;
        }
      }

      internal void SetLastSegment(bool last) {
        context.IsLastSegment = last;
      }
    }
  }

  /// <summary>
  /// Specializes the <see cref="UIElementStyleDataContext{TModelItem}"/>
  /// and provides a convenience property for the <see cref="Node"/>.
  /// </summary>
  public class UIElementNodeStyleDataContext : INotifyPropertyChanged
  {
    private INode modelItem;
    private bool selected;
    private bool highlighted;
    private bool focused;
    private bool selectionAdapter;
    private bool highlightAdapter;
    private bool focusAdapter;
    private bool selectionQueried;
    private bool highlightQueried;
    private bool focusQueried;
    private CanvasControl canvas;
    private UIElementNodeStyle style;
    private static readonly PropertyChangedEventArgs userTagEA = new PropertyChangedEventArgs("UserTag");
    private static readonly PropertyChangedEventArgs styleTagEA = new PropertyChangedEventArgs("StyleTag");
    private static readonly PropertyChangedEventArgs selectedEA = new PropertyChangedEventArgs("Selected");
    private static readonly PropertyChangedEventArgs highlightEA = new PropertyChangedEventArgs("Highlighted");
    private static readonly PropertyChangedEventArgs focusEA = new PropertyChangedEventArgs("Focused");
    private PropertyChangedEventHandler myHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIElementNodeStyleDataContext"/> class
    /// for use in the given <paramref name="context"/>.
    /// </summary>
    /// <remarks>
    /// This implementation will query the context for the <see cref="CanvasControl"/> to optionally
    /// query the <see cref="ISelectionModel{T}"/> and <see cref="HighlightIndicatorManager{T}"/> instances from
    /// for the <see cref="INode"/> type to satisfy queries to the <see cref="UIElementEdgeStyleDataContext.Selected"/> 
    /// and <see cref="UIElementEdgeStyleDataContext.Highlighted"/> properties.
    /// </remarks>
    /// <param name="context">The context for which the visual has been <see cref="IVisualCreator.CreateVisual">created</see> for.</param>
    /// <param name="modelItem">The node to which the context is bound.</param>
    /// <param name="style">The style instance which has been used to create the visual.</param>
    public UIElementNodeStyleDataContext(IRenderContext context, INode modelItem, UIElementNodeStyle style) {
      this.style = style;
      this.modelItem = modelItem;
      this.canvas = context != null ? context.CanvasControl : null;
      new PropertyChangeAdapter(this, style);
    }

    class PropertyChangeAdapter : WeakReference
    {
      public PropertyChangeAdapter(UIElementNodeStyleDataContext context, UIElementNodeStyle style)
        : base(context) {
        ((INotifyPropertyChanged) style).PropertyChanged += PropertyChangeAdapter_PropertyChanged;
      }

      void PropertyChangeAdapter_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        UIElementNodeStyleDataContext ctx = this.Target as UIElementNodeStyleDataContext;
        if (ctx == null) {
          ((INotifyPropertyChanged) sender).PropertyChanged -= PropertyChangeAdapter_PropertyChanged;
        } else {
          ctx.OnStylePropertyChanged(sender, e);
        }
      }
    }

    /// <summary>
    /// Yields the node that is being visualized.
    /// </summary>
    public INode Node {
      get { return this.ModelItem; }
    }

    /// <summary>
    /// Gets the node style.
    /// </summary>
    /// <value>The node style.</value>
    public UIElementNodeStyle NodeStyle {
      get { return Style as UIElementNodeStyle; }
    }

    /// <summary>
    /// Yields the <see cref="CanvasControl"/> the visual has been 
    /// <see cref="IVisualCreator.CreateVisual">created</see> for.
    /// </summary>
    public CanvasControl Canvas {
      get { return canvas; }
    }

    /// <summary>
    /// Gets or sets the <see cref="ISelectionModel{T}">selection state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="ISelectionModel{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Selected {
      get {
        selectionQueried = true;
        if (selectionAdapter) {
          return selected;
        } else {
          EnsureAdapters();
          if (canvas != null) {
            IGraphSelection selection = canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection;
            if (selection != null) {
              return selected = selection.IsSelected(modelItem);
            }
          }
          return selected;
        }
      }
      set {
        if (selected != value) {
          selected = value;
          if (canvas != null) {
            IGraphSelection selection = canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection;
            if (selection != null) {
              selection.SetSelected(modelItem, selected);
            }
          }
          OnPropertyChanged(selectedEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="HighlightIndicatorManager{T}">highlight state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="HighlightIndicatorManager{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Highlighted {
      get {
        highlightQueried = true;
        if (highlightAdapter) {
          return highlighted;
        } else {
          if (canvas != null) {
            EnsureAdapters();
            HighlightIndicatorManager<INode> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<INode>)) as HighlightIndicatorManager<INode>;
            if (manager != null) {
              return highlighted = manager.SelectionModel.IsSelected(modelItem);
            } else {
              HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
              if (manager2 != null) {
                return highlighted = manager2.SelectionModel.IsSelected(modelItem);
              }
            }
          }
          return highlighted;
        }
      }
      set {
        if (highlighted != value) {
          highlighted = value;
          HighlightIndicatorManager<INode> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<INode>)) as HighlightIndicatorManager<INode>;
          if (manager != null) {
            if (highlighted) {
              manager.AddHighlight(modelItem);
            } else {
              manager.RemoveHighlight(modelItem);
            }
          } else {
            HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
            if (manager2 != null) {
              if (highlighted) {
                manager2.AddHighlight(modelItem);
              } else {
                manager2.RemoveHighlight(modelItem);
              }
            }
          }
          OnPropertyChanged(highlightEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="FocusIndicatorManager{T}">focused state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="FocusIndicatorManager{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Focused {
      get {
        focusQueried = true;
        if (focusAdapter) {
          return focused;
        } else {
          if (canvas is GraphControl) {
            EnsureAdapters();
            FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
            return focused = Equals(modelItem, manager2.FocusedItem);
          }
          return focused;
        }
      }
      set {
        if (focused != value) {
          focused = value;
          if (canvas is GraphControl) {
            FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
            if (focused) {
              manager2.FocusedItem = modelItem;
            } else {
              manager2.FocusedItem = default(INode);
            }
          }
          OnPropertyChanged(focusEA);
        }
      }
    }

    /// <summary>
    /// Yields the style that is associated with this context instance.
    /// </summary>
    public UIElementNodeStyle Style {
      get { return style; }
    }

    /// <summary>
    /// Yields the <see cref="ITaggedStyleBase{TModelItem}.StyleTag">tag of the style</see> that is associated
    /// with this context instance.
    /// </summary>
    /// <value>The style tag.</value>
    public object StyleTag {
      get { return style.StyleTag; }
      set {
        if (style.StyleTag != value) {
          style.StyleTag = value;
          OnPropertyChanged(styleTagEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the user tag associated with this instance.
    /// </summary>
    /// <remarks>
    /// Queries to this property are satisfied by the <see cref="ITaggedStyleBase{TModelItem}.UserTagProvider"/>
    /// instance that is associated with the style that created this context object.
    /// </remarks>
    /// <value>The user tag.</value>
    public object UserTag {
      get { return modelItem.Tag; }
      set {
        if (value != modelItem.Tag) {
          modelItem.Tag = value;
          OnPropertyChanged(userTagEA);
        }
      }
    }

    /// <summary>
    /// Gets the model item for which this context object has been created for.
    /// </summary>
    /// <value>The model item.</value>
    public INode ModelItem {
      get { return modelItem; }
    }

    internal sealed class SelectionAdapter : WeakReference
    {
      public SelectionAdapter(UIElementNodeStyleDataContext context, ISelectionModel<IModelItem> selection)
        : base(context) {
        selection.ItemSelectionChanged += Selection_OnItemSelectionChanged;
      }

      private void Selection_OnItemSelectionChanged(object source, ItemSelectionChangedEventArgs<IModelItem> args) {
        UIElementNodeStyleDataContext context = Target as UIElementNodeStyleDataContext;
        if (context == null) {
          ((IGraphSelection) source).ItemSelectionChanged -= Selection_OnItemSelectionChanged;
        } else {
          if (args.Item == context.modelItem) {
            context.Selected = args.ItemSelected;
          }
        }
      }
    }

    internal sealed class HighlightAdapter<DModelItem> : WeakReference where DModelItem : IModelItem
    {
      private readonly HighlightIndicatorManager<DModelItem> manager;

      public HighlightAdapter(UIElementNodeStyleDataContext context, HighlightIndicatorManager<DModelItem> manager)
        : base(context) {
        this.manager = manager;
        this.manager.SelectionModel.ItemSelectionChanged += Selection_OnItemSelectionChanged;
      }

      private void Selection_OnItemSelectionChanged(object source, ItemSelectionChangedEventArgs<DModelItem> args) {
        UIElementNodeStyleDataContext context = Target as UIElementNodeStyleDataContext;
        if (context == null) {
          manager.SelectionModel.ItemSelectionChanged -= Selection_OnItemSelectionChanged;
        } else {
          if (Equals(args.Item, context.modelItem)) {
            context.Highlighted = args.ItemSelected;
          }
        }
      }
    }

    internal sealed class FocusAdapter<DModelItem> : WeakReference where DModelItem : IModelItem
    {
      private readonly FocusIndicatorManager<DModelItem> manager;

      public FocusAdapter(UIElementNodeStyleDataContext context, FocusIndicatorManager<DModelItem> manager)
        : base(context) {
        this.manager = manager;
        this.manager.PropertyChanged += OnItemFocused;
      }

      private void OnItemFocused(object source, PropertyChangedEventArgs e) {
        UIElementNodeStyleDataContext context = Target as UIElementNodeStyleDataContext;
        if (context == null) {
          manager.PropertyChanged -= OnItemFocused;
        } else {
          if (e.PropertyName == "FocusedItem") {
            context.SetFocused(Equals(manager.FocusedItem, context.modelItem));
          }
        }
      }
    }

    internal void SetFocused(bool value) {
      if (focused != value) {
        focused = value;
        OnPropertyChanged(focusEA);
      }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="args">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (myHandler != null) {
        myHandler(this, args);
      }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    /// <remarks>
    /// This allows for data binding to the properties of this class.
    /// </remarks>
    public event PropertyChangedEventHandler PropertyChanged {
      add {
        if (myHandler == null) {
          myHandler = value;
          EnsureAdapters();
        } else {
          myHandler = (PropertyChangedEventHandler) Delegate.Combine(myHandler, value);
        }
      }
      remove { myHandler = (PropertyChangedEventHandler) Delegate.Remove(myHandler, value); }
    }

    private void EnsureAdapters() {
      if (selectionQueried && !selectionAdapter) {
        IGraphSelection selection = canvas != null ? canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection : null;
        if (selection != null) {
          new SelectionAdapter(this, selection);
          selectionAdapter = true;
          selected = selection.IsSelected(modelItem);
        }
      }
      if (highlightQueried && canvas != null && !highlightAdapter) {
        HighlightIndicatorManager<INode> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<INode>)) as HighlightIndicatorManager<INode>;
        if (manager != null) {
          new HighlightAdapter<INode>(this, manager);
          highlightAdapter = true;
          highlighted = manager.SelectionModel.IsSelected(modelItem);

        } else {
          HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
          if (manager2 != null) {
            new HighlightAdapter<IModelItem>(this, manager2);
            highlightAdapter = true;
            highlighted = manager2.SelectionModel.IsSelected(modelItem);
          }
        }
      }
      if (focusQueried && canvas is GraphControl && !focusAdapter) {
        FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
        new FocusAdapter<IModelItem>(this, manager2);
        focusAdapter = true;
        focused = modelItem.Equals(manager2.FocusedItem);
      }
    }

    /// <summary>
    /// Called when a property on the <see cref="Style"/> has changed.
    /// </summary>
    /// <remarks>
    /// This will only be called if the style instance supports <see cref="INotifyPropertyChanged">property change notification</see>,
    /// which holds true for all implementations in this framework.
    /// </remarks>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnStylePropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "StyleTag") {
        OnPropertyChanged(styleTagEA);
      } else if (e.PropertyName == "UserTagProvider") {
        OnPropertyChanged(userTagEA);
      }
    }
  }

  /// <summary>
  /// Specializes the <see cref="UIElementStyleDataContext{TModelItem}"/>
  /// and provides a convenience property for the <see cref="Port"/>.
  /// </summary>
  public class UIElementPortStyleDataContext : INotifyPropertyChanged
  {
    private IPort modelItem;
    private bool selected;
    private bool highlighted;
    private bool focused;
    private bool selectionAdapter;
    private bool highlightAdapter;
    private bool focusAdapter;
    private bool selectionQueried;
    private bool highlightQueried;
    private bool focusQueried;
    private CanvasControl canvas;
    private UIElementPortStyle style;
    private static readonly PropertyChangedEventArgs userTagEA = new PropertyChangedEventArgs("UserTag");
    private static readonly PropertyChangedEventArgs styleTagEA = new PropertyChangedEventArgs("StyleTag");
    private static readonly PropertyChangedEventArgs selectedEA = new PropertyChangedEventArgs("Selected");
    private static readonly PropertyChangedEventArgs highlightEA = new PropertyChangedEventArgs("Highlighted");
    private static readonly PropertyChangedEventArgs focusEA = new PropertyChangedEventArgs("Focused");
    private PropertyChangedEventHandler myHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIElementPortStyleDataContext"/> class
    /// for use in the given <paramref name="context"/>.
    /// </summary>
    /// <remarks>
    /// This implementation will query the context for the <see cref="CanvasControl"/> to optionally
    /// query the <see cref="ISelectionModel{T}"/> and <see cref="HighlightIndicatorManager{T}"/> instances from
    /// for the <see cref="IPort"/> type to satisfy queries to the <see cref="UIElementEdgeStyleDataContext.Selected"/> 
    /// and <see cref="UIElementEdgeStyleDataContext.Highlighted"/> properties.
    /// </remarks>
    /// <param name="context">The context for which the visual has been <see cref="IVisualCreator.CreateVisual">created</see> for.</param>
    /// <param name="modelItem">The port to which the context is bound.</param>
    /// <param name="style">The style instance which has been used to create the visual.</param>
    public UIElementPortStyleDataContext(IRenderContext context, IPort modelItem, UIElementPortStyle style) {
      this.modelItem = modelItem;
      this.style = style;
      this.canvas = context != null ? context.CanvasControl : null;
      new PropertyChangeAdapter(this, style);
    }

    class PropertyChangeAdapter : WeakReference
    {
      public PropertyChangeAdapter(UIElementPortStyleDataContext context, UIElementPortStyle style)
        : base(context) {
        ((INotifyPropertyChanged) style).PropertyChanged += PropertyChangeAdapter_PropertyChanged;
      }

      void PropertyChangeAdapter_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        var ctx = this.Target as UIElementPortStyleDataContext;
        if (ctx == null) {
          ((INotifyPropertyChanged) sender).PropertyChanged -= PropertyChangeAdapter_PropertyChanged;
        } else {
          ctx.OnStylePropertyChanged(sender, e);
        }
      }
    }

    /// <summary>
    /// Yields the port that is being visualized.
    /// </summary>
    public IPort Port {
      get { return ModelItem; }
    }

    /// <summary>
    /// Gets the port style.
    /// </summary>
    /// <value>The port style.</value>
    public UIElementPortStyle PortStyle {
      get { return Style; }
    }

    /// <summary>
    /// Yields the <see cref="CanvasControl"/> the visual has been 
    /// <see cref="IVisualCreator.CreateVisual">created</see> for.
    /// </summary>
    public CanvasControl Canvas {
      get { return canvas; }
    }

    /// <summary>
    /// Gets or sets the <see cref="ISelectionModel{T}">selection state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="ISelectionModel{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Selected {
      get {
        selectionQueried = true;
        if (selectionAdapter) {
          return selected;
        } else {
          EnsureAdapters();
          if (canvas != null) {
            IGraphSelection selection = canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection;
            if (selection != null) {
              return selected = selection.IsSelected(modelItem);
            }
          }
          return selected;
        }
      }
      set {
        if (selected != value) {
          selected = value;
          if (canvas != null) {
            IGraphSelection selection = canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection;
            if (selection != null) {
              selection.SetSelected(modelItem, selected);
            }
          }
          OnPropertyChanged(selectedEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="HighlightIndicatorManager{T}">highlight state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="HighlightIndicatorManager{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Highlighted {
      get {
        highlightQueried = true;
        if (highlightAdapter) {
          return highlighted;
        } else {
          if (canvas != null) {
            EnsureAdapters();
            HighlightIndicatorManager<IPort> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IPort>)) as HighlightIndicatorManager<IPort>;
            if (manager != null) {
              return highlighted = manager.SelectionModel.IsSelected(modelItem);
            } else {
              HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
              if (manager2 != null) {
                return highlighted = manager2.SelectionModel.IsSelected(modelItem);
              }
            }
          }
          return highlighted;
        }
      }
      set {
        if (highlighted != value) {
          highlighted = value;
          HighlightIndicatorManager<IPort> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IPort>)) as HighlightIndicatorManager<IPort>;
          if (manager != null) {
            if (highlighted) {
              manager.AddHighlight(modelItem);
            } else {
              manager.RemoveHighlight(modelItem);
            }
          } else {
            HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
            if (manager2 != null) {
              if (highlighted) {
                manager2.AddHighlight(modelItem);
              } else {
                manager2.RemoveHighlight(modelItem);
              }
            }
          }
          OnPropertyChanged(highlightEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="FocusIndicatorManager{T}">focused state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="FocusIndicatorManager{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Focused {
      get {
        focusQueried = true;
        if (focusAdapter) {
          return focused;
        } else {
          if (canvas is GraphControl) {
            EnsureAdapters();
            FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
            return focused = Equals(modelItem, manager2.FocusedItem);
          }
          return focused;
        }
      }
      set {
        if (focused != value) {
          focused = value;
          if (canvas is GraphControl) {
            FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
            if (focused) {
              manager2.FocusedItem = modelItem;
            } else {
              manager2.FocusedItem = default(IPort);
            }
          }
          OnPropertyChanged(focusEA);
        }
      }
    }

    /// <summary>
    /// Yields the style that is associated with this context instance.
    /// </summary>
    public UIElementPortStyle Style {
      get { return style; }
    }

    /// <summary>
    /// Yields the <see cref="ITaggedStyleBase{TModelItem}.StyleTag">tag of the style</see> that is associated
    /// with this context instance.
    /// </summary>
    /// <value>The style tag.</value>
    public object StyleTag {
      get { return style.StyleTag; }
      set {
        if (style.StyleTag != value) {
          if (style is UIElementPortStyle) {
            ((UIElementPortStyle) style).StyleTag = value;
            OnPropertyChanged(styleTagEA);
          }
        }
      }
    }

    /// <summary>
    /// Gets or sets the user tag associated with this instance.
    /// </summary>
    /// <remarks>
    /// Queries to this property are satisfied by the <see cref="ITaggedStyleBase{TModelItem}.UserTagProvider"/>
    /// instance that is associated with the style that created this context object.
    /// </remarks>
    /// <value>The user tag.</value>
    public object UserTag {
      get { return modelItem.Tag; }
      set {
        if (value != modelItem.Tag) {
          modelItem.Tag = value;
          OnPropertyChanged(userTagEA);
        }
      }
    }

    /// <summary>
    /// Gets the model item for which this context object has been created for.
    /// </summary>
    /// <value>The model item.</value>
    public IPort ModelItem {
      get { return modelItem; }
    }

    internal sealed class SelectionAdapter : WeakReference
    {
      public SelectionAdapter(UIElementPortStyleDataContext context, ISelectionModel<IModelItem> selection)
        : base(context) {
        selection.ItemSelectionChanged += Selection_OnItemSelectionChanged;
      }

      private void Selection_OnItemSelectionChanged(object source, ItemSelectionChangedEventArgs<IModelItem> args) {
        UIElementPortStyleDataContext context = Target as UIElementPortStyleDataContext;
        if (context == null) {
          ((IGraphSelection) source).ItemSelectionChanged -= Selection_OnItemSelectionChanged;
        } else {
          if (args.Item == context.modelItem) {
            context.Selected = args.ItemSelected;
          }
        }
      }
    }

    internal sealed class HighlightAdapter<DModelItem> : WeakReference where DModelItem : IModelItem
    {
      private readonly HighlightIndicatorManager<DModelItem> manager;

      public HighlightAdapter(UIElementPortStyleDataContext context, HighlightIndicatorManager<DModelItem> manager)
        : base(context) {
        this.manager = manager;
        this.manager.SelectionModel.ItemSelectionChanged += Selection_OnItemSelectionChanged;
      }

      private void Selection_OnItemSelectionChanged(object source, ItemSelectionChangedEventArgs<DModelItem> args) {
        UIElementPortStyleDataContext context = Target as UIElementPortStyleDataContext;
        if (context == null) {
          manager.SelectionModel.ItemSelectionChanged -= Selection_OnItemSelectionChanged;
        } else {
          if (Equals(args.Item, context.modelItem)) {
            context.Highlighted = args.ItemSelected;
          }
        }
      }
    }

    internal sealed class FocusAdapter<DModelItem> : WeakReference where DModelItem : IModelItem
    {
      private readonly FocusIndicatorManager<DModelItem> manager;

      public FocusAdapter(UIElementPortStyleDataContext context, FocusIndicatorManager<DModelItem> manager)
        : base(context) {
        this.manager = manager;
        this.manager.PropertyChanged += OnItemFocused;
      }

      private void OnItemFocused(object source, PropertyChangedEventArgs e) {
        UIElementPortStyleDataContext context = Target as UIElementPortStyleDataContext;
        if (context == null) {
          manager.PropertyChanged -= OnItemFocused;
        } else {
          if (e.PropertyName == "FocusedItem") {
            context.SetFocused(Equals(manager.FocusedItem, context.modelItem));
          }
        }
      }
    }

    internal void SetFocused(bool value) {
      if (focused != value) {
        focused = value;
        OnPropertyChanged(focusEA);
      }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="args">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (myHandler != null) {
        myHandler(this, args);
      }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    /// <remarks>
    /// This allows for data binding to the properties of this class.
    /// </remarks>
    public event PropertyChangedEventHandler PropertyChanged {
      add {
        if (myHandler == null) {
          myHandler = value;
          EnsureAdapters();
        } else {
          myHandler = (PropertyChangedEventHandler) Delegate.Combine(myHandler, value);
        }
      }
      remove { myHandler = (PropertyChangedEventHandler) Delegate.Remove(myHandler, value); }
    }

    private void EnsureAdapters() {
      if (selectionQueried && !selectionAdapter) {
        IGraphSelection selection = canvas != null ? canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection : null;
        if (selection != null) {
          new SelectionAdapter(this, selection);
          selectionAdapter = true;
          selected = selection.IsSelected(modelItem);
        }
      }
      if (highlightQueried && canvas != null && !highlightAdapter) {
        HighlightIndicatorManager<IPort> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IPort>)) as HighlightIndicatorManager<IPort>;
        if (manager != null) {
          new HighlightAdapter<IPort>(this, manager);
          highlightAdapter = true;
          highlighted = manager.SelectionModel.IsSelected(modelItem);

        } else {
          HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
          if (manager2 != null) {
            new HighlightAdapter<IModelItem>(this, manager2);
            highlightAdapter = true;
            highlighted = manager2.SelectionModel.IsSelected(modelItem);
          }
        }
      }
      if (focusQueried && canvas is GraphControl && !focusAdapter) {
        FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
        new FocusAdapter<IModelItem>(this, manager2);
        focusAdapter = true;
        focused = modelItem.Equals(manager2.FocusedItem);
      }
    }

    /// <summary>
    /// Called when a property on the <see cref="Style"/> has changed.
    /// </summary>
    /// <remarks>
    /// This will only be called if the style instance supports <see cref="INotifyPropertyChanged">property change notification</see>,
    /// which holds true for all implementations in this framework.
    /// </remarks>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnStylePropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "StyleTag") {
        OnPropertyChanged(styleTagEA);
      } else if (e.PropertyName == "UserTagProvider") {
        OnPropertyChanged(userTagEA);
      }
    }
  }

  /// <summary>
  /// Specializes the <see cref="UIElementStyleDataContext{TModelItem}"/>
  /// and provides a convenience property for the <see cref="Label"/>.
  /// </summary>
  public class UIElementLabelStyleDataContext : INotifyPropertyChanged
  {
    private ILabel modelItem;
    private bool selected;
    private bool highlighted;
    private bool focused;
    private bool selectionAdapter;
    private bool highlightAdapter;
    private bool focusAdapter;
    private bool selectionQueried;
    private bool highlightQueried;
    private bool focusQueried;
    private CanvasControl canvas;
    private UIElementLabelStyle style;
    private static readonly PropertyChangedEventArgs userTagEA = new PropertyChangedEventArgs("UserTag");
    private static readonly PropertyChangedEventArgs styleTagEA = new PropertyChangedEventArgs("StyleTag");
    private static readonly PropertyChangedEventArgs selectedEA = new PropertyChangedEventArgs("Selected");
    private static readonly PropertyChangedEventArgs highlightEA = new PropertyChangedEventArgs("Highlighted");
    private static readonly PropertyChangedEventArgs focusEA = new PropertyChangedEventArgs("Focused");
    private PropertyChangedEventHandler myHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIElementLabelStyleDataContext"/> class
    /// for use in the given <paramref name="context"/>.
    /// </summary>
    /// <remarks>
    /// This implementation will query the context for the <see cref="CanvasControl"/> to optionally
    /// query the <see cref="ISelectionModel{T}"/> and <see cref="HighlightIndicatorManager{T}"/> instances from
    /// for the <see cref="ILabel"/> type to satisfy queries to the <see cref="UIElementEdgeStyleDataContext.Selected"/> 
    /// and <see cref="UIElementEdgeStyleDataContext.Highlighted"/> properties.
    /// </remarks>
    /// <param name="context">The context for which the visual has been <see cref="IVisualCreator.CreateVisual">created</see> for.</param>
    /// <param name="label">The label to which the context is bound.</param>
    /// <param name="style">The style instance which has been used to create the visual.</param>
    public UIElementLabelStyleDataContext(IRenderContext context, ILabel label, UIElementLabelStyle style) {
      this.style = style;
      this.modelItem = label;
      this.canvas = context != null ? context.CanvasControl : null;
      new PropertyChangeAdapter(this, style);
    }

    class PropertyChangeAdapter : WeakReference
    {
      public PropertyChangeAdapter(UIElementLabelStyleDataContext context, UIElementLabelStyle style)
        : base(context) {
        ((INotifyPropertyChanged) style).PropertyChanged += PropertyChangeAdapter_PropertyChanged;
      }

      void PropertyChangeAdapter_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        UIElementLabelStyleDataContext ctx = this.Target as UIElementLabelStyleDataContext;
        if (ctx == null) {
          ((INotifyPropertyChanged) sender).PropertyChanged -= PropertyChangeAdapter_PropertyChanged;
        } else {
          ctx.OnStylePropertyChanged(sender, e);
        }
      }
    }

    /// <summary>
    /// Yields the label that is being visualized.
    /// </summary>
    public ILabel Label {
      get { return ModelItem; }
    }

    /// <summary>
    /// Gets the label style.
    /// </summary>
    /// <value>The label style.</value>
    public UIElementLabelStyle LabelStyle {
      get { return Style as UIElementLabelStyle; }
    }

    /// <summary>
    /// Yields the <see cref="CanvasControl"/> the visual has been 
    /// <see cref="IVisualCreator.CreateVisual">created</see> for.
    /// </summary>
    public CanvasControl Canvas {
      get { return canvas; }
    }

    /// <summary>
    /// Gets or sets the <see cref="ISelectionModel{T}">selection state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="ISelectionModel{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Selected {
      get {
        selectionQueried = true;
        if (selectionAdapter) {
          return selected;
        } else {
          EnsureAdapters();
          if (canvas != null) {
            IGraphSelection selection = canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection;
            if (selection != null) {
              return selected = selection.IsSelected(modelItem);
            }
          }
          return selected;
        }
      }
      set {
        if (selected != value) {
          selected = value;
          if (canvas != null) {
            IGraphSelection selection = canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection;
            if (selection != null) {
              selection.SetSelected(modelItem, selected);
            }
          }
          OnPropertyChanged(selectedEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="HighlightIndicatorManager{T}">highlight state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="HighlightIndicatorManager{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Highlighted {
      get {
        highlightQueried = true;
        if (highlightAdapter) {
          return highlighted;
        } else {
          if (canvas != null) {
            EnsureAdapters();
            HighlightIndicatorManager<ILabel> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<ILabel>)) as HighlightIndicatorManager<ILabel>;
            if (manager != null) {
              return highlighted = manager.SelectionModel.IsSelected(modelItem);
            } else {
              HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
              if (manager2 != null) {
                return highlighted = manager2.SelectionModel.IsSelected(modelItem);
              }
            }
          }
          return highlighted;
        }
      }
      set {
        if (highlighted != value) {
          highlighted = value;
          HighlightIndicatorManager<ILabel> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<ILabel>)) as HighlightIndicatorManager<ILabel>;
          if (manager != null) {
            if (highlighted) {
              manager.AddHighlight(modelItem);
            } else {
              manager.RemoveHighlight(modelItem);
            }
          } else {
            HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
            if (manager2 != null) {
              if (highlighted) {
                manager2.AddHighlight(modelItem);
              } else {
                manager2.RemoveHighlight(modelItem);
              }
            }
          }
          OnPropertyChanged(highlightEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="FocusIndicatorManager{T}">focused state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="FocusIndicatorManager{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Focused {
      get {
        focusQueried = true;
        if (focusAdapter) {
          return focused;
        } else {
          if (canvas is GraphControl) {
            EnsureAdapters();
            FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
            return focused = Equals(modelItem, manager2.FocusedItem);
          }
          return focused;
        }
      }
      set {
        if (focused != value) {
          focused = value;
          if (canvas is GraphControl) {
            FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
            if (focused) {
              manager2.FocusedItem = modelItem;
            } else {
              manager2.FocusedItem = default(ILabel);
            }
          }
          OnPropertyChanged(focusEA);
        }
      }
    }

    /// <summary>
    /// Yields the style that is associated with this context instance.
    /// </summary>
    public UIElementLabelStyle Style {
      get { return style; }
    }

    /// <summary>
    /// Yields the <see cref="ITaggedStyleBase{TModelItem}.StyleTag">tag of the style</see> that is associated
    /// with this context instance.
    /// </summary>
    /// <value>The style tag.</value>
    public object StyleTag {
      get { return style.StyleTag; }
      set {
        if (style.StyleTag != value) {
          if (style is UIElementLabelStyle) {
            ((UIElementLabelStyle) style).StyleTag = value;
            OnPropertyChanged(styleTagEA);
          }
        }
      }
    }

    /// <summary>
    /// Gets or sets the user tag associated with this instance.
    /// </summary>
    /// <remarks>
    /// Queries to this property are satisfied by the <see cref="ITaggedStyleBase{TModelItem}.UserTagProvider"/>
    /// instance that is associated with the style that created this context object.
    /// </remarks>
    /// <value>The user tag.</value>
    public object UserTag {
      get { return modelItem.Tag; }
      set {
        if (value != modelItem.Tag) {
          modelItem.Tag = value;
          OnPropertyChanged(userTagEA);
        }
      }
    }

    /// <summary>
    /// Gets the model item for which this context object has been created for.
    /// </summary>
    /// <value>The model item.</value>
    public ILabel ModelItem {
      get { return modelItem; }
    }

    internal sealed class SelectionAdapter : WeakReference
    {
      public SelectionAdapter(UIElementLabelStyleDataContext context, ISelectionModel<IModelItem> selection)
        : base(context) {
        selection.ItemSelectionChanged += Selection_OnItemSelectionChanged;
      }

      private void Selection_OnItemSelectionChanged(object source, ItemSelectionChangedEventArgs<IModelItem> args) {
        UIElementLabelStyleDataContext context = Target as UIElementLabelStyleDataContext;
        if (context == null) {
          ((IGraphSelection) source).ItemSelectionChanged -= Selection_OnItemSelectionChanged;
        } else {
          if (args.Item == context.modelItem) {
            context.Selected = args.ItemSelected;
          }
        }
      }
    }

    internal sealed class HighlightAdapter<DModelItem> : WeakReference where DModelItem : IModelItem
    {
      private readonly HighlightIndicatorManager<DModelItem> manager;

      public HighlightAdapter(UIElementLabelStyleDataContext context, HighlightIndicatorManager<DModelItem> manager)
        : base(context) {
        this.manager = manager;
        this.manager.SelectionModel.ItemSelectionChanged += Selection_OnItemSelectionChanged;
      }

      private void Selection_OnItemSelectionChanged(object source, ItemSelectionChangedEventArgs<DModelItem> args) {
        UIElementLabelStyleDataContext context = Target as UIElementLabelStyleDataContext;
        if (context == null) {
          manager.SelectionModel.ItemSelectionChanged -= Selection_OnItemSelectionChanged;
        } else {
          if (Equals(args.Item, context.modelItem)) {
            context.Highlighted = args.ItemSelected;
          }
        }
      }
    }

    internal sealed class FocusAdapter<DModelItem> : WeakReference where DModelItem : IModelItem
    {
      private readonly FocusIndicatorManager<DModelItem> manager;

      public FocusAdapter(UIElementLabelStyleDataContext context, FocusIndicatorManager<DModelItem> manager)
        : base(context) {
        this.manager = manager;
        this.manager.PropertyChanged += OnItemFocused;
      }

      private void OnItemFocused(object source, PropertyChangedEventArgs e) {
        UIElementLabelStyleDataContext context = Target as UIElementLabelStyleDataContext;
        if (context == null) {
          manager.PropertyChanged -= OnItemFocused;
        } else {
          if (e.PropertyName == "FocusedItem") {
            context.SetFocused(Equals(manager.FocusedItem, context.modelItem));
          }
        }
      }
    }

    internal void SetFocused(bool value) {
      if (focused != value) {
        focused = value;
        OnPropertyChanged(focusEA);
      }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="args">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (myHandler != null) {
        myHandler(this, args);
      }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    /// <remarks>
    /// This allows for data binding to the properties of this class.
    /// </remarks>
    public event PropertyChangedEventHandler PropertyChanged {
      add {
        if (myHandler == null) {
          myHandler = value;
          EnsureAdapters();
        } else {
          myHandler = (PropertyChangedEventHandler) Delegate.Combine(myHandler, value);
        }
      }
      remove { myHandler = (PropertyChangedEventHandler) Delegate.Remove(myHandler, value); }
    }

    private void EnsureAdapters() {
      if (selectionQueried && !selectionAdapter) {
        IGraphSelection selection = canvas != null ? canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection : null;
        if (selection != null) {
          new SelectionAdapter(this, selection);
          selectionAdapter = true;
          selected = selection.IsSelected(modelItem);
        }
      }
      if (highlightQueried && canvas != null && !highlightAdapter) {
        HighlightIndicatorManager<ILabel> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<ILabel>)) as HighlightIndicatorManager<ILabel>;
        if (manager != null) {
          new HighlightAdapter<ILabel>(this, manager);
          highlightAdapter = true;
          highlighted = manager.SelectionModel.IsSelected(modelItem);

        } else {
          HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
          if (manager2 != null) {
            new HighlightAdapter<IModelItem>(this, manager2);
            highlightAdapter = true;
            highlighted = manager2.SelectionModel.IsSelected(modelItem);
          }
        }
      }
      if (focusQueried && canvas is GraphControl && !focusAdapter) {
        FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
        new FocusAdapter<IModelItem>(this, manager2);
        focusAdapter = true;
        focused = modelItem.Equals(manager2.FocusedItem);
      }
    }

    /// <summary>
    /// Called when a property on the <see cref="Style"/> has changed.
    /// </summary>
    /// <remarks>
    /// This will only be called if the style instance supports <see cref="INotifyPropertyChanged">property change notification</see>,
    /// which holds true for all implementations in this framework.
    /// </remarks>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnStylePropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "StyleTag") {
        OnPropertyChanged(styleTagEA);
      } else if (e.PropertyName == "UserTagProvider") {
        OnPropertyChanged(userTagEA);
      }
    }
  }

  /// <summary>
  /// Specializes the <see cref="UIElementStyleDataContext{TModelItem}"/>
  /// and provides a convenience property for the <see cref="Edge"/>.
  /// </summary>
  /// <remarks>
  /// This class also yields the total <see cref="SegmentCount"/> of the edge.
  /// Note that this class is not the one that will be assigned the <see cref="FrameworkElement.DataContext"/>
  /// property which belong to the instances that are generated by the <see cref="IUIElementStyle{TModelItem}.Template"/>.
  /// They will be set to instances of <see cref="UIElementEdgeStyleSegmentDataContext"/> instead.
  /// </remarks>
  /// <seealso cref="UIElementEdgeStyleSegmentDataContext"/>
  public class UIElementEdgeStyleDataContext : INotifyPropertyChanged
  {
    private int segmentCount;
    private static readonly PropertyChangedEventArgs segmentCountPCEA = new PropertyChangedEventArgs("SegmentCount");
    private IEdge modelItem;
    private bool selected;
    private bool highlighted;
    private bool focused;
    private bool selectionAdapter;
    private bool highlightAdapter;
    private bool focusAdapter;
    private bool selectionQueried;
    private bool highlightQueried;
    private bool focusQueried;
    private CanvasControl canvas;
    private UIElementEdgeStyle style;
    private static readonly PropertyChangedEventArgs userTagEA = new PropertyChangedEventArgs("UserTag");
    private static readonly PropertyChangedEventArgs styleTagEA = new PropertyChangedEventArgs("StyleTag");
    private static readonly PropertyChangedEventArgs selectedEA = new PropertyChangedEventArgs("Selected");
    private static readonly PropertyChangedEventArgs highlightEA = new PropertyChangedEventArgs("Highlighted");
    private static readonly PropertyChangedEventArgs focusEA = new PropertyChangedEventArgs("Focused");
    private PropertyChangedEventHandler myHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIElementEdgeStyleDataContext"/> class
    /// for use in the given <paramref name="context"/>.
    /// </summary>
    /// <remarks>
    /// This implementation will query the context for the <see cref="CanvasControl"/> to optionally
    /// query the <see cref="ISelectionModel{T}"/> and <see cref="HighlightIndicatorManager{T}"/> instances from
    /// for the <see cref="IEdge"/> type to satisfy queries to the <see cref="Selected"/> 
    /// and <see cref="Highlighted"/> properties.
    /// </remarks>
    /// <param name="context">The context for which the visual has been <see cref="IVisualCreator.CreateVisual">created</see> for.</param>
    /// <param name="edge">The edge to which the context is bound.</param>
    /// <param name="style">The style instance which has been used to create the visual.</param>
    public UIElementEdgeStyleDataContext(IRenderContext context, IEdge edge, UIElementEdgeStyle style) {
      this.style = style;
      this.modelItem = edge;
      this.canvas = context != null ? context.CanvasControl : null;
      new PropertyChangeAdapter(this, style);
    }

    class PropertyChangeAdapter : WeakReference
    {
      public PropertyChangeAdapter(UIElementEdgeStyleDataContext context, UIElementEdgeStyle style)
        : base(context) {
        ((INotifyPropertyChanged) style).PropertyChanged += PropertyChangeAdapter_PropertyChanged;
      }

      void PropertyChangeAdapter_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        var ctx = this.Target as UIElementEdgeStyleDataContext;
        if (ctx == null) {
          ((INotifyPropertyChanged) sender).PropertyChanged -= PropertyChangeAdapter_PropertyChanged;
        } else {
          ctx.OnStylePropertyChanged(sender, e);
        }
      }
    }

    /// <summary>
    /// Yields the edge that is being visualized.
    /// </summary>
    public IEdge Edge {
      get { return this.ModelItem; }
    }

    /// <summary>
    /// Gets the total segment count that is used to visualize this edge.
    /// </summary>
    /// <remarks>
    /// For each segment there will be one <see cref="IUIElementStyle{TModelItem}.Template"/>
    /// instantiated that will be assigned and instance of <see cref="UIElementEdgeStyleSegmentDataContext"/>
    /// to the <see cref="FrameworkElement.DataContext"/> property.
    /// </remarks>
    /// <value>The total segment count.</value>
    /// <seealso cref="UIElementEdgeStyleSegmentDataContext"/>
    /// <seealso cref="UIElementEdgeStyleSegmentDataContext.SegmentIndex"/>
    public int SegmentCount {
      get { return segmentCount; }
      internal set {
        if (segmentCount != value) {
          segmentCount = value;
          OnPropertyChanged(segmentCountPCEA);
        }
      }
    }

    /// <summary>
    /// Gets the edge style.
    /// </summary>
    /// <value>The edge style.</value>
    public UIElementEdgeStyle EdgeStyle {
      get { return Style as UIElementEdgeStyle; }
    }

    /// <summary>
    /// Yields the <see cref="CanvasControl"/> the visual has been 
    /// <see cref="IVisualCreator.CreateVisual">created</see> for.
    /// </summary>
    public CanvasControl Canvas {
      get { return canvas; }
    }

    /// <summary>
    /// Gets or sets the <see cref="ISelectionModel{T}">selection state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="ISelectionModel{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Selected {
      get {
        selectionQueried = true;
        if (selectionAdapter) {
          return selected;
        } else {
          EnsureAdapters();
          if (canvas != null) {
            IGraphSelection selection = canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection;
            if (selection != null) {
              return selected = selection.IsSelected(modelItem);
            }
          }
          return selected;
        }
      }
      set {
        if (selected != value) {
          selected = value;
          if (canvas != null) {
            IGraphSelection selection = canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection;
            if (selection != null) {
              selection.SetSelected(modelItem, selected);
            }
          }
          OnPropertyChanged(selectedEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="HighlightIndicatorManager{T}">highlight state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="HighlightIndicatorManager{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Highlighted {
      get {
        highlightQueried = true;
        if (highlightAdapter) {
          return highlighted;
        } else {
          if (canvas != null) {
            EnsureAdapters();
            HighlightIndicatorManager<IEdge> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IEdge>)) as HighlightIndicatorManager<IEdge>;
            if (manager != null) {
              return highlighted = manager.SelectionModel.IsSelected(modelItem);
            } else {
              HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
              if (manager2 != null) {
                return highlighted = manager2.SelectionModel.IsSelected(modelItem);
              }
            }
          }
          return highlighted;
        }
      }
      set {
        if (highlighted != value) {
          highlighted = value;
          HighlightIndicatorManager<IEdge> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IEdge>)) as HighlightIndicatorManager<IEdge>;
          if (manager != null) {
            if (highlighted) {
              manager.AddHighlight(modelItem);
            } else {
              manager.RemoveHighlight(modelItem);
            }
          } else {
            HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
            if (manager2 != null) {
              if (highlighted) {
                manager2.AddHighlight(modelItem);
              } else {
                manager2.RemoveHighlight(modelItem);
              }
            }
          }
          OnPropertyChanged(highlightEA);
        }
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="FocusIndicatorManager{T}">focused state</see> of the given item 
    /// in the current context.
    /// </summary>
    /// <remarks>
    /// This property is data bindable and will write through to the <see cref="FocusIndicatorManager{T}"/>
    /// of the given type that has been found in the <see cref="Canvas"/>.
    /// </remarks>
    public bool Focused {
      get {
        focusQueried = true;
        if (focusAdapter) {
          return focused;
        } else {
          if (canvas is GraphControl) {
            EnsureAdapters();
            FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
            return focused = Equals(modelItem, manager2.FocusedItem);
          }
          return focused;
        }
      }
      set {
        if (focused != value) {
          focused = value;
          if (canvas is GraphControl) {
            FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
            if (focused) {
              manager2.FocusedItem = modelItem;
            } else {
              manager2.FocusedItem = default(IEdge);
            }
          }
          OnPropertyChanged(focusEA);
        }
      }
    }

    /// <summary>
    /// Yields the style that is associated with this context instance.
    /// </summary>
    public UIElementEdgeStyle Style {
      get { return style; }
    }

    /// <summary>
    /// Yields the <see cref="ITaggedStyleBase{TModelItem}.StyleTag">tag of the style</see> that is associated
    /// with this context instance.
    /// </summary>
    /// <value>The style tag.</value>
    public object StyleTag {
      get { return style.StyleTag; }
      set {
        if (style.StyleTag != value) {
          if (style is UIElementEdgeStyle) {
            ((UIElementEdgeStyle) style).StyleTag = value;
            OnPropertyChanged(styleTagEA);
          }
        }
      }
    }

    /// <summary>
    /// Gets or sets the user tag associated with this instance.
    /// </summary>
    /// <remarks>
    /// Queries to this property are satisfied by the <see cref="ITaggedStyleBase{TModelItem}.UserTagProvider"/>
    /// instance that is associated with the style that created this context object.
    /// </remarks>
    /// <value>The user tag.</value>
    public object UserTag {
      get { return modelItem.Tag; }
      set {
        if (value != modelItem.Tag) {
          modelItem.Tag = value;
          OnPropertyChanged(userTagEA);
        }
      }
    }

    /// <summary>
    /// Gets the model item for which this context object has been created for.
    /// </summary>
    /// <value>The model item.</value>
    public IEdge ModelItem {
      get { return modelItem; }
    }

    internal UIElementEdgeStyleSegmentDataContext CreateSegmentContext(int segmentIndex) {
      return new UIElementEdgeStyleSegmentDataContext(this, segmentIndex);
    }

    internal sealed class SelectionAdapter : WeakReference
    {
      public SelectionAdapter(UIElementEdgeStyleDataContext context, ISelectionModel<IModelItem> selection)
        : base(context) {
        selection.ItemSelectionChanged += Selection_OnItemSelectionChanged;
      }

      private void Selection_OnItemSelectionChanged(object source, ItemSelectionChangedEventArgs<IModelItem> args) {
        UIElementEdgeStyleDataContext context = Target as UIElementEdgeStyleDataContext;
        if (context == null) {
          ((IGraphSelection) source).ItemSelectionChanged -= Selection_OnItemSelectionChanged;
        } else {
          if (args.Item == context.modelItem) {
            context.Selected = args.ItemSelected;
          }
        }
      }
    }

    internal sealed class HighlightAdapter<DModelItem> : WeakReference where DModelItem : IModelItem
    {
      private readonly HighlightIndicatorManager<DModelItem> manager;

      public HighlightAdapter(UIElementEdgeStyleDataContext context, HighlightIndicatorManager<DModelItem> manager)
        : base(context) {
        this.manager = manager;
        this.manager.SelectionModel.ItemSelectionChanged += Selection_OnItemSelectionChanged;
      }

      private void Selection_OnItemSelectionChanged(object source, ItemSelectionChangedEventArgs<DModelItem> args) {
        UIElementEdgeStyleDataContext context = Target as UIElementEdgeStyleDataContext;
        if (context == null) {
          manager.SelectionModel.ItemSelectionChanged -= Selection_OnItemSelectionChanged;
        } else {
          if (Equals(args.Item, context.modelItem)) {
            context.Highlighted = args.ItemSelected;
          }
        }
      }
    }

    internal sealed class FocusAdapter<DModelItem> : WeakReference where DModelItem : IModelItem
    {
      private readonly FocusIndicatorManager<DModelItem> manager;

      public FocusAdapter(UIElementEdgeStyleDataContext context, FocusIndicatorManager<DModelItem> manager)
        : base(context) {
        this.manager = manager;
        this.manager.PropertyChanged += OnItemFocused;
      }

      private void OnItemFocused(object source, PropertyChangedEventArgs e) {
        UIElementEdgeStyleDataContext context = Target as UIElementEdgeStyleDataContext;
        if (context == null) {
          manager.PropertyChanged -= OnItemFocused;
        } else {
          if (e.PropertyName == "FocusedItem") {
            context.SetFocused(Equals(manager.FocusedItem, context.modelItem));
          }
        }
      }
    }

    internal void SetFocused(bool value) {
      if (focused != value) {
        focused = value;
        OnPropertyChanged(focusEA);
      }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="args">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (myHandler != null) {
        myHandler(this, args);
      }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    /// <remarks>
    /// This allows for data binding to the properties of this class.
    /// </remarks>
    public event PropertyChangedEventHandler PropertyChanged {
      add {
        if (myHandler == null) {
          myHandler = value;
          EnsureAdapters();
        } else {
          myHandler = (PropertyChangedEventHandler) Delegate.Combine(myHandler, value);
        }
      }
      remove { myHandler = (PropertyChangedEventHandler) Delegate.Remove(myHandler, value); }
    }

    private void EnsureAdapters() {
      if (selectionQueried && !selectionAdapter) {
        IGraphSelection selection = canvas != null ? canvas.Lookup(typeof(IGraphSelection)) as IGraphSelection : null;
        if (selection != null) {
          new SelectionAdapter(this, selection);
          selectionAdapter = true;
          selected = selection.IsSelected(modelItem);
        }
      }
      if (highlightQueried && canvas != null && !highlightAdapter) {
        HighlightIndicatorManager<IEdge> manager = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IEdge>)) as HighlightIndicatorManager<IEdge>;
        if (manager != null) {
          new HighlightAdapter<IEdge>(this, manager);
          highlightAdapter = true;
          highlighted = manager.SelectionModel.IsSelected(modelItem);

        } else {
          HighlightIndicatorManager<IModelItem> manager2 = canvas.InputModeContext.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as HighlightIndicatorManager<IModelItem>;
          if (manager2 != null) {
            new HighlightAdapter<IModelItem>(this, manager2);
            highlightAdapter = true;
            highlighted = manager2.SelectionModel.IsSelected(modelItem);
          }
        }
      }
      if (focusQueried && canvas is GraphControl && !focusAdapter) {
        FocusIndicatorManager<IModelItem> manager2 = ((GraphControl) canvas).FocusIndicatorManager;
        new FocusAdapter<IModelItem>(this, manager2);
        focusAdapter = true;
        focused = modelItem.Equals(manager2.FocusedItem);
      }
    }

    /// <summary>
    /// Called when a property on the <see cref="Style"/> has changed.
    /// </summary>
    /// <remarks>
    /// This will only be called if the style instance supports <see cref="INotifyPropertyChanged">property change notification</see>,
    /// which holds true for all implementations in this framework.
    /// </remarks>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnStylePropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "StyleTag") {
        OnPropertyChanged(styleTagEA);
      } else if (e.PropertyName == "UserTagProvider") {
        OnPropertyChanged(userTagEA);
      }
    }
  }

  /// <summary>
  /// A data context object for use with the <see cref="UIElementEdgeStyleRenderer"/>
  /// that will be created for each segment in the polygonal representation of
  /// an <see cref="IEdge"/> rendering.
  /// </summary>
  /// <remarks>
  /// This class provides access to the <see cref="SegmentIndex">index of the current segment</see>,
  /// and two convenience properties that indicate whether the current segment is
  /// the <see cref="IsFirstSegment">first</see> and/or <see cref="IsLastSegment">last</see>
  /// segment in the visual representation of the edges. Also it provides access to 
  /// the underlying <see cref="EdgeContext">context object</see> for the edge.
  /// All of the properties support <see cref="INotifyPropertyChanged">property change notification</see>
  /// and can thus be used for data binding.
  /// Instances of this class are created by the framework, only.
  /// </remarks>
  public sealed class UIElementEdgeStyleSegmentDataContext : INotifyPropertyChanged
  {
    private readonly UIElementEdgeStyleDataContext edgeContext;
    private readonly int segmentIndex;
    private bool lastSegment;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIElementEdgeStyleSegmentDataContext"/> class
    /// for the given <see cref="UIElementEdgeStyleDataContext">edge context</see>.
    /// </summary>
    /// <param name="edgeContext">The edge context.</param>
    /// <param name="segmentIndex">The index of the segment.</param>
    internal UIElementEdgeStyleSegmentDataContext(UIElementEdgeStyleDataContext edgeContext, int segmentIndex) {
      this.edgeContext = edgeContext;
      this.segmentIndex = segmentIndex;
    }

    /// <summary>
    /// Gets the edge context.
    /// </summary>
    /// <value>The edge context.</value>
    public UIElementEdgeStyleDataContext EdgeContext {
      get { return edgeContext; }
    }

    /// <summary>
    /// Gets the index of this segment.
    /// </summary>
    /// <value>The index of the segment.</value>
    public int SegmentIndex {
      get { return segmentIndex; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is the last segment
    /// in the edge path.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is the last segment; otherwise, <c>false</c>.
    /// </value>
    public bool IsLastSegment {
      get { return lastSegment; }
      internal set {
        if (lastSegment != value) {
          lastSegment = value;
          OnPropertyChanged(lastSegementPCEA);
        }
      }
    }

    private void OnPropertyChanged(PropertyChangedEventArgs pcea) {
      if (PropertyChanged != null) {
        PropertyChanged(this, pcea);
      }
    }

    private static readonly PropertyChangedEventArgs lastSegementPCEA = new PropertyChangedEventArgs("IsLastSegment");

    /// <summary>
    /// Gets a value indicating whether this instance is the first segment in the edge path.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is the first segment; otherwise, <c>false</c>.
    /// </value>
    public bool IsFirstSegment {
      get { return segmentIndex == 0; }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
  }
}
