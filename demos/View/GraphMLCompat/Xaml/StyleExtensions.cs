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
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Demo.GraphViewer.Xaml;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.GraphMLCompat.Styles
{
  [ContentProperty("Wrapped")]
  public class ShadowNodeStyleDecoratorExtension : MarkupExtension
  {
    public INodeStyle Wrapped { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) => new ShadowNodeStyleDecorator(Wrapped);
  }

  public class GroupNodeStyleExtension : MarkupExtension
  {
    private static readonly Brush defaultTabBrush =
        (Brush) new SolidColorBrush(Color.FromRgb(170, 170, 170)).GetAsFrozen();

    public InsetsD ContentAreaInsets { get; set; } = new(4);

    public double CornerRadius { get; set; } = 4;

    public double TabSlope { get; set; } = 0.5;

    public double TabWidth { get; set; } = 50;

    public double TabHeight { get; set; } = 18;

    public double TabInset { get; set; } = 4;

    public GroupNodeStyleTabPosition TabPosition { get; set; } = GroupNodeStyleTabPosition.Top;

    public GroupNodeStyleTabSizePolicy TabSizePolicy { get; set; } = GroupNodeStyleTabSizePolicy.Fixed;

    public GroupNodeStyleIconPosition IconPosition { get; set; } = GroupNodeStyleIconPosition.Trailing;

    public GroupNodeStyleIconType GroupIcon { get; set; } = GroupNodeStyleIconType.None;

    public GroupNodeStyleIconBackgroundShape IconBackgroundShape { get; set; } =
      GroupNodeStyleIconBackgroundShape.RoundRectangleSolid;

    public GroupNodeStyleIconType FolderIcon { get; set; } = GroupNodeStyleIconType.Plus;

    public Brush IconBackgroundBrush { get; set; } = Brushes.White;

    public Brush IconForegroundBrush { get; set; } = defaultTabBrush;

    public double IconSize { get; set; } = 14;

    public double IconOffset { get; set; } = 0;

    public bool DrawShadow { get; set; } = false;

    public SizeD MinimumContentAreaSize { get; set; } = new(0, 0);

    public bool HitTransparentContentArea { get; set; } = false;

    public bool RenderTransparentContentArea { get; set; } = false;

    public bool ShowFolderContentArea { get; set; } = false;

    public Brush TabBackgroundBrush { get; set; } = null;

    public Brush ContentAreaBrush { get; set; } = Brushes.White;

    public Brush TabBrush { get; set; } = defaultTabBrush;

    public Pen Pen { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new GroupNodeStyle {
            ContentAreaBrush = ContentAreaBrush,
            ContentAreaPadding = ContentAreaInsets,
            CornerRadius = CornerRadius,
            DrawShadow = DrawShadow,
            FolderIcon = FolderIcon,
            GroupIcon = GroupIcon,
            HitTransparentContentArea = HitTransparentContentArea,
            IconBackgroundBrush = IconBackgroundBrush,
            IconBackgroundShape = IconBackgroundShape,
            IconForegroundBrush = IconForegroundBrush,
            IconOffset = IconOffset,
            IconPosition = IconPosition,
            IconSize = IconSize,
            MinimumContentAreaSize = MinimumContentAreaSize,
            RenderTransparentContentArea = RenderTransparentContentArea,
            ShowFolderContentArea = ShowFolderContentArea,
            Pen = Pen,
            TabBackgroundBrush = TabBackgroundBrush,
            TabHeight = TabHeight,
            TabPadding = TabInset,
            TabPosition = TabPosition,
            TabSizePolicy = TabSizePolicy,
            TabSlope = TabSlope,
            TabWidth = TabWidth,
            TabBrush = TabBrush
        };
  }

  [ContentProperty("Wrapped")]
  public class CollapsibleNodeStyleDecoratorExtension : MarkupExtension
  {
    public Style ButtonStyle { get; set; }

    public ILabelModelParameter ButtonPlacement { get; set; } =
      new InteriorNodeLabelModel { Padding = InsetsD.Empty }.CreateParameter(InteriorNodeLabelModel.Position.TopLeft);

    public InsetsD Insets { get; set; } = new(5, 16, 5, 5);

    public INodeStyle Wrapped { get; set; }


    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new CollapsibleNodeStyleDecorator {
            Padding = Insets,
            WrappedStyle = Wrapped ?? new ShapeNodeStyle(),
            ButtonPlacement = ButtonPlacement
        };
  }

  public enum ShapeNodeShape
  {
    Rectangle = Graph.Styles.ShapeNodeShape.Rectangle,
    RoundRectangle = Graph.Styles.ShapeNodeShape.RoundRectangle,
    Ellipse = Graph.Styles.ShapeNodeShape.Ellipse,
    Triangle = Graph.Styles.ShapeNodeShape.Triangle,
    Triangle2 = Graph.Styles.ShapeNodeShape.TrianglePointingDown,
    ShearedRectangle = 100,
    ShearedRectangle2 = 101,
    Trapez = 102,
    Trapez2 = 103,
    Star5 = 104,
    Star6 = Graph.Styles.ShapeNodeShape.Star6,
    Star8 = Graph.Styles.ShapeNodeShape.Star8,
    FatArrow = 105,
    FatArrow2 = 106,
    Diamond = Graph.Styles.ShapeNodeShape.Diamond,
    Octagon = Graph.Styles.ShapeNodeShape.Octagon,
    Hexagon = Graph.Styles.ShapeNodeShape.Hexagon,
    Hexagon2 = Graph.Styles.ShapeNodeShape.HexagonStanding,
    Star5Up = 107,
    Pill = Graph.Styles.ShapeNodeShape.Pill
  }

  public class ShapeNodeStyleExtension : MarkupExtension
  {
    public Pen Pen { get; set; } = Pens.Black;

    public Brush Brush { get; set; } = Brushes.White;

    public ShapeNodeShape Shape { get; set; } = ShapeNodeShape.Rectangle;

    public virtual bool KeepIntrinsicAspectRatio { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      switch (Shape) {
        case ShapeNodeShape.ShearedRectangle:
        case ShapeNodeShape.ShearedRectangle2:
        case ShapeNodeShape.Trapez:
        case ShapeNodeShape.Trapez2:
        case ShapeNodeShape.Star5:
        case ShapeNodeShape.FatArrow:
        case ShapeNodeShape.FatArrow2:
        case ShapeNodeShape.Star5Up:
          var path = ShapeNodeStyles.GetPath(Shape, KeepIntrinsicAspectRatio);
          var ar = ShapeNodeStyles.GetIntrinsicAspectRatio(Shape);

          return new GeneralPathNodeStyle {
              Brush = Brush,
              AspectRatio = KeepIntrinsicAspectRatio ? ar : 0,
              Pen = Pen,
              Path = path
          };
        default:
          return new ShapeNodeStyle {
              Brush = Brush,
              KeepIntrinsicAspectRatio = KeepIntrinsicAspectRatio,
              Pen = Pen,
              Shape = (Graph.Styles.ShapeNodeShape) (int) Shape
          };
      }
    }
  }

  public class NodeControlNodeStyleExtension : MarkupExtension
  {
    public string StyleResourceKey { get; set; }

    public InsetsD Insets { get; set; } = new(5);

    public SizeD MinimumSize { get; set; } = SizeD.Empty;

    public Shape OutlineShape { get; set; }

    public object StyleTag { get; set; }

    public IContextLookup ContextLookup { get; set; } = Lookups.EmptyContextLookup;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new NodeControlNodeStyle {
            Padding = Insets,
            ContextLookup = ContextLookup,
            MinimumSize = MinimumSize,
            OutlineShape = OutlineShape,
            StyleTag = StyleTag,
            StyleResourceKey = StyleResourceKey
        };
  }

  public enum ArrowType
  {
    Default = Graph.Styles.ArrowType.Stealth,
    Simple = Graph.Styles.ArrowType.Open,
    Short = Graph.Styles.ArrowType.Stealth + 100,
    Diamond = Graph.Styles.ArrowType.Diamond,
    None = Graph.Styles.ArrowType.None,
    Circle = Graph.Styles.ArrowType.Ellipse,
    Cross = Graph.Styles.ArrowType.Cross,
    Triangle = Graph.Styles.ArrowType.Triangle
  }

  public class Arrows
  {
    public static readonly IArrow Default = ArrowExtension.CreateArrow(ArrowType.Default, Pens.Black, Brushes.Black);
    public static readonly IArrow Simple = ArrowExtension.CreateArrow(ArrowType.Simple, Pens.Black, Brushes.Black);
    public static readonly IArrow Short = ArrowExtension.CreateArrow(ArrowType.Short, Pens.Black, Brushes.Black);
    public static readonly IArrow Diamond = ArrowExtension.CreateArrow(ArrowType.Diamond, Pens.Black, Brushes.Black);
    public static readonly IArrow None = ArrowExtension.CreateArrow(ArrowType.None, Pens.Black, Brushes.Black);
    public static readonly IArrow Circle = ArrowExtension.CreateArrow(ArrowType.Circle, Pens.Black, Brushes.Black);
    public static readonly IArrow Cross = ArrowExtension.CreateArrow(ArrowType.Cross, Pens.Black, Brushes.Black);
    public static readonly IArrow Triangle = ArrowExtension.CreateArrow(ArrowType.Triangle, Pens.Black, Brushes.Black);
  }

  public class ArrowExtension : MarkupExtension
  {
    public Pen Pen { get; set; } = null;

    public Brush Brush { get; set; } = null;

    public double CropLength { get; set; } = 0;

    public double Scale { get; set; } = 1.0;

    public ArrowType Type { get; set; } = ArrowType.Default;

    public bool IsFrozen { get; set; } = false;

    private static double GetWidthScale(ArrowType type) {
      switch (type) {
        case ArrowType.Default:
          return 1;
        case ArrowType.Simple:
          return 1.5;
        case ArrowType.Short:
          return 1.5;
        case ArrowType.Diamond:
          return 1;
        case ArrowType.None:
          return 1;
        case ArrowType.Circle:
          return 1;
        case ArrowType.Cross:
          return 1;
        case ArrowType.Triangle:
          return 1.5;
        default:
          return 1;
      }
    }

    private static double GetLengthScale(ArrowType type) {
      switch (type) {
        case ArrowType.Default:
          return 1;
        case ArrowType.Simple:
          return 0.5;
        case ArrowType.Short:
          return 0.5;
        case ArrowType.Diamond:
          return 1;
        case ArrowType.None:
          return 1;
        case ArrowType.Circle:
          return 1;
        case ArrowType.Cross:
          return 1;
        case ArrowType.Triangle:
          return 0.5;
        default:
          return 1;
      }
    }

    internal static Arrow CreateArrow(ArrowType type, Pen pen, Brush brush, double scale = 1, double cropLength = 0) =>
        new(cropLength: cropLength, brush: brush, lengthScale: scale * GetLengthScale(type), pen: pen,
            type: (Graph.Styles.ArrowType) (type == ArrowType.Short ? type - 100 : type),
            widthScale: scale * GetWidthScale(type));

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var pen = Pen;
      if (Scale != 1 && Pen != null) {
        var newPen = Pen.Clone();
        newPen.Thickness = 2 * Pen.Thickness;
        pen = newPen;
      }
      return CreateArrow(Type, pen, Brush, Scale, CropLength);
    }
  }

  public enum TextWrappingShape
  {
    Rectangle = Graph.Styles.TextWrappingShape.Rectangle,
    RoundRectangle = Graph.Styles.TextWrappingShape.RoundRectangle,
    Ellipse = Graph.Styles.TextWrappingShape.Ellipse,
    Triangle = Graph.Styles.TextWrappingShape.Triangle,
    Triangle2 = Graph.Styles.TextWrappingShape.TrianglePointingDown,
    ShearedRectangle = Graph.Styles.TextWrappingShape.Rectangle,
    ShearedRectangle2 = Graph.Styles.TextWrappingShape.Rectangle,
    Trapez = Graph.Styles.TextWrappingShape.Rectangle,
    Trapez2 = Graph.Styles.TextWrappingShape.Rectangle,
    Diamond = Graph.Styles.TextWrappingShape.Diamond,
    Octagon = Graph.Styles.TextWrappingShape.Octagon,
    Hexagon = Graph.Styles.TextWrappingShape.Hexagon,
    Hexagon2 = Graph.Styles.TextWrappingShape.Hexagon,
    Pill = Graph.Styles.TextWrappingShape.Pill,
    Squircle = Graph.Styles.TextWrappingShape.Squircle,
    LabelShape = Graph.Styles.TextWrappingShape.LabelShape
  }

  public class DefaultLabelStyleExtension : MarkupExtension
  {
    private static readonly Typeface defaultFont = new("Arial");

    public Brush BackgroundBrush { get; set; }

    public VerticalAlignment VerticalTextAlignment { get; set; } = VerticalAlignment.Top;

    public TextDecorationCollection TextDecorations { get; set; }

    public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;

    public TextTrimming TextTrimming { get; set; } = TextTrimming.WordEllipsis;

    public TextWrapping TextWrapping { get; set; } = TextWrapping.Wrap;

    public CultureInfo Culture { get; set; }

    public FlowDirection FlowDirection { get; set; } = FlowDirection.LeftToRight;

    public NumberSubstitution NumberSubstitution { get; set; }

    public bool ClipText { get; set; } = true;

    public Pen BackgroundPen { get; set; }

    public LabelShape Shape { get; set; } = LabelShape.Rectangle;

    public TextWrappingShape TextWrappingShape { get; set; }

    public double TextWrappingPadding { get; set; }

    public Typeface Typeface { get; set; } = defaultFont;

    public bool AutoFlip { get; set; } = true;

    public Brush TextBrush { get; set; } = Brushes.Black;

    public double TextSize { get; set; } = 12;

    public InsetsD Insets { get; set; } = InsetsD.Empty;

    public SizeD MinimumSize { get; set; } = SizeD.Zero;

    public SizeD MaximumSize { get; set; } = SizeD.Infinite;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new LabelStyle {
            AutoFlip = AutoFlip,
            BackgroundBrush = BackgroundBrush,
            BackgroundPen = BackgroundPen,
            Typeface = Typeface,
            TextAlignment = TextAlignment,
            MaximumSize = MaximumSize,
            MinimumSize = MinimumSize,
            Padding = Insets,
            Shape = Shape,
            TextBrush = TextBrush,
            TextSize = TextSize,
            TextWrappingPadding = TextWrappingPadding,
            TextWrappingShape = (Graph.Styles.TextWrappingShape) TextWrappingShape,
            VerticalTextAlignment = VerticalTextAlignment,
            TextWrapping = TextWrapping,
            Culture = Culture,
            ClipText = ClipText,
            FlowDirection = FlowDirection,
            NumberSubstitution = NumberSubstitution,
            TextDecorations = TextDecorations,
            TextTrimming = TextTrimming
        };
  }

  [ContentProperty("Wrapped")]
  public class IconLabelStyleExtension : MarkupExtension
  {
    public ImageSource Icon { get; set; }

    public SizeD IconSize { get; set; } = SizeD.Empty;

    public ILabelModelParameter IconPlacement { get; set; } =
      new ExteriorNodeLabelModel { Margins = InsetsD.Empty }.CreateParameter(ExteriorNodeLabelModel.Position.Left);

    public ILabelStyle Wrapped { get; set; } = new LabelStyle();

    public InsetsD WrappedInsets { get; set; }

    public bool AutoFlip { get; set; } = true;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new IconLabelStyle {
            AutoFlip = AutoFlip,
            IconPlacement = IconPlacement,
            IconSize = IconSize,
            WrappedStyle = Wrapped,
            WrappedStylePadding = WrappedInsets,
            Icon = Icon
        };
  }

  public class NodeStyleLabelStyleAdapterExtension : MarkupExtension
  {
    public INodeStyle NodeStyle { get; set; }

    public ILabelStyle LabelStyle { get; set; }

    public bool AutoFlip { get; set; } = true;

    public InsetsD LabelStyleInsets { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new NodeStyleLabelStyleAdapter {
            AutoFlip = AutoFlip,
            LabelStyle = LabelStyle,
            LabelStylePadding = LabelStyleInsets,
            NodeStyle = NodeStyle
        };
  }
}
