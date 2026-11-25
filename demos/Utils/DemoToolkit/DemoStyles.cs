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

#nullable disable

using System.Windows;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo
{
  public static class DemoStyles
  {
    /// <summary>
    ///   Initializes graph defaults with nicely configured built-in yFiles styles.
    /// </summary>
    /// <param name="graph">The graph on which the default styles and style-related settings are set.</param>
    /// <param name="nodeTheme">
    ///   Optional color set names for demo node styles. The default is
    ///   <see cref="Themes.PaletteOrange"/>.
    /// </param>
    /// <param name="nodeLabelTheme">Optional color set names for demo node label styles. The default is the node theme.</param>
    /// <param name="edgeTheme">
    ///   Optional color set names for demo edge styles. The default is
    ///   <see cref="Themes.PaletteOrange"/>.
    /// </param>
    /// <param name="edgeLabelTheme">Optional color set names for demo edge label styles. The default is the edge theme.</param>
    /// <param name="groupTheme">
    ///   Optional color set names for demo group node styles. The default is
    ///   <see cref="Themes.Palette12"/>.
    /// </param>
    /// <param name="groupLabelTheme">
    ///   Optional color set names for demo group node label styles. The default is group node
    ///   theme.
    /// </param>
    /// <param name="foldingEnabled">Whether to use collapsible group node style.</param>
    /// <param name="extraCropLength">The extra crop length for the <see cref="EdgePathCropper"/>.</param>
    /// <param name="shape">
    ///   The optional shape of the node style, if <see langword="null"/> a <see cref="RectangleNodeStyle"/>
    ///   is used.
    /// </param>
    /// <param name="orthogonalEdgeEditing">
    ///   Whether orthogonal edge editing should be enabled for the edge style
    ///   if the style supports it.
    /// </param>
    public static void InitDemoStyles(
        IGraph graph,
        Palette nodeTheme = null,
        Palette nodeLabelTheme = null,
        Palette edgeTheme = null,
        Palette edgeLabelTheme = null,
        Palette groupTheme = null,
        Palette groupLabelTheme = null,
        bool foldingEnabled = false,
        double extraCropLength = 2.0,
        ShapeNodeShape? shape = null,
        bool orthogonalEdgeEditing = false) {
      nodeTheme ??= Themes.PaletteOrange;
      nodeLabelTheme ??= nodeTheme;
      edgeTheme ??= Themes.PaletteOrange;
      edgeLabelTheme ??= edgeTheme;
      groupTheme ??= Themes.Palette12;
      groupLabelTheme ??= groupTheme;

      graph.NodeDefaults.Style = shape == null
          ? CreateDemoNodeStyle(nodeTheme)
          : CreateDemoShapeNodeStyle(shape.Value, nodeTheme);
      graph.NodeDefaults.Labels.Style = CreateDemoNodeLabelStyle(nodeLabelTheme);

      graph.GroupNodeDefaults.Style = CreateDemoGroupStyle(groupTheme, foldingEnabled);
      graph.GroupNodeDefaults.Labels.Style = CreateDemoGroupLabelStyle(groupLabelTheme);
      graph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabBackgroundParameter();

      graph.EdgeDefaults.Style = CreateDemoEdgeStyle(edgeTheme, extraCropLength: extraCropLength,
          orthogonalEditing: orthogonalEdgeEditing);
      graph.EdgeDefaults.Labels.Style = CreateDemoEdgeLabelStyle(edgeLabelTheme);
    }

    public static void InitTutorialStyles(IGraph graph) {
      InitializeSimpleDemoStyles(graph);
    }

    public static void InitializeSimpleDemoStyles(IGraph graph) {
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.RoundRectangle,
          Brush = new SolidColorBrush(Color.FromRgb(0x0b, 0x71, 0x89)),
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x04, 0x2d, 0x37)), 1.5)
      };

      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          SmoothingLength = 8,
          Pen = new Pen(Brushes.Black, 1.5),
          TargetArrow = new Arrow(ArrowType.Triangle, new SolidColorBrush(Color.FromRgb(0x04, 0x2d, 0x37)),
              new Pen(new SolidColorBrush(Color.FromRgb(0x04, 0x2d, 0x37)), 1.5), 0.75, 0.75, 0, false)
      };

      graph.NodeDefaults.Labels.Style = new LabelStyle {
          Shape = LabelShape.RoundRectangle,
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0x9d, 0xc6, 0xd0)),
          TextBrush = new SolidColorBrush(Color.FromRgb(0x04, 0x2d, 0x37)),
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Center,
          Padding = new InsetsD(2, 4, 2, 4)
      };

      graph.EdgeDefaults.Labels.Style = new LabelStyle {
          Shape = LabelShape.RoundRectangle,
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0x9b, 0xab, 0xaf)),
          TextBrush = new SolidColorBrush(Color.FromRgb(0x04, 0x2d, 0x37)),
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Center,
          Padding = new InsetsD(2, 4, 2, 4)
      };

      graph.GroupNodeDefaults.Style = new GroupNodeStyle {
          TabPosition = GroupNodeStyleTabPosition.TopTrailing,
          TabSizePolicy = GroupNodeStyleTabSizePolicy.AdjustToLabel,
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x04, 0x2d, 0x37)), 1.5),
          TabBrush = new SolidColorBrush(Color.FromRgb(0x0b, 0x71, 0x89)),
          IconForegroundBrush = new SolidColorBrush(Color.FromRgb(0x9d, 0xc6, 0xd0)),
          IconBackgroundBrush = new SolidColorBrush(Color.FromRgb(0x9d, 0xc6, 0xd0)),
          ContentAreaBrush = new SolidColorBrush(Color.FromRgb(0xf3, 0xf4, 0xf6)),
          CornerRadius = 8,
          TabWidth = 20,
          ContentAreaPadding = new InsetsD(8),
          HitTransparentContentArea = true
      };

      graph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          TextBrush = new SolidColorBrush(Color.FromRgb(0x9d, 0xc6, 0xd0)),
          TextWrapping = TextWrapping.Wrap,
          TextTrimming = TextTrimming.CharacterEllipsis
      };

      graph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabParameter();
    }

    /// <summary>
    ///   Creates a new rectangular node style whose colors match the given palette.
    /// </summary>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.PaletteOrange">default palette</see> should be used.
    /// </param>
    /// <returns>
    ///   A new rectangular node style whose colors match the given palette.
    /// </returns>
    public static RectangleNodeStyle CreateDemoNodeStyle(Palette palette = null) {
      palette ??= Themes.PaletteOrange;
      return new RectangleNodeStyle {
          Brush = palette.Fill,
          Pen = (Pen) new Pen(palette.Stroke, 1.5).GetAsFrozen(),
          CornerStyle = CornerStyle.Round,
          CornerSize = 3.5
      };
    }

    /// <summary>
    ///   Creates a new rectangular node style whose colors match the given palette.
    /// </summary>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.PaletteOrange">default palette</see> should be used.
    /// </param>
    /// <returns>
    ///   A new rectangular node style whose colors match the given palette.
    /// </returns>
    public static RectangleNodeStyle CreateTutorialNodeStyle(Palette palette = null) {
      palette ??= Themes.Palette12;
      return new RectangleNodeStyle {
          Brush = palette.Fill,
          Pen = (Pen) new Pen(palette.Stroke, 1.5).GetAsFrozen(),
          CornerStyle = CornerStyle.Round,
          CornerSize = 10
      };
    }

    /// <summary>
    ///   Creates a new node style with the given shape whose colors match the given palette.
    /// </summary>
    /// <param name="shape">The shape to use.</param>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.PaletteOrange">default palette</see> should be used.
    /// </param>
    /// <returns>
    ///   A new node style with the given shape whose colors match the given palette.
    /// </returns>
    public static ShapeNodeStyle CreateDemoShapeNodeStyle(ShapeNodeShape shape, Palette palette = null) {
      palette ??= Themes.PaletteOrange;
      return new ShapeNodeStyle {
          Shape = shape,
          Brush = palette.Fill,
          Pen = (Pen) new Pen(palette.Stroke, 1.5).GetAsFrozen()
      };
    }

    /// <summary>
    ///   Creates a new polyline edge style whose colors match the given palette.
    /// </summary>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.PaletteOrange">default palette</see> should be used.
    /// </param>
    /// <param name="showTargetArrow">Whether the edge style should have a target arrow.</param>
    /// <param name="extraCropLength">An additional length by which the edge is cropped.</param>
    /// <param name="cropAtPorts">Whether the edge should be cropped at the ports instead of the node bounds.</param>
    /// <param name="orthogonalEditing">Whether orthogonal editing should be active for the edge.</param>
    /// <returns>
    ///   A new polyline edge style whose colors match the given palette.
    /// </returns>
    public static PolylineEdgeStyle CreateDemoEdgeStyle(
        Palette palette = null,
        bool showTargetArrow = true,
        double extraCropLength = 2,
        bool cropAtPorts = false,
        bool orthogonalEditing = false) {
      palette ??= Themes.PaletteOrange;
      return new PolylineEdgeStyle {
          Pen = (Pen) new Pen(palette.Stroke, 1.5).GetAsFrozen(),
          SourceArrow = new Arrow(ArrowType.None, cropLength: extraCropLength),
          TargetArrow =
              showTargetArrow
                  ? new Arrow(ArrowType.Triangle, palette.Stroke, cropLength: extraCropLength, cropAtPort: cropAtPorts)
                  : Arrow.None,
          OrthogonalEditing = orthogonalEditing
      };
    }

    /// <summary>
    ///   Creates a new polyline edge style whose colors match the <see cref="Palette.Fill"/> of the given palette.
    /// </summary>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.PaletteOrange">default palette</see> should be used.
    /// </param>
    /// <param name="showTargetArrow">Whether the edge style should have a target arrow.</param>
    /// <param name="extraCropLength">An additional length by which the edge is cropped.</param>
    /// <param name="cropAtPorts">Whether the edge should be cropped at the ports instead of the node bounds.</param>
    /// <param name="orthogonalEditing">Whether orthogonal editing should be active for the edge.</param>
    /// <returns>
    ///   A new polyline edge style whose colors match the given palette.
    /// </returns>
    public static PolylineEdgeStyle CreateDemoHighlightEdgeStyle(
        Palette palette = null,
        bool showTargetArrow = true,
        double extraCropLength = 2,
        bool cropAtPorts = false,
        bool orthogonalEditing = false) {
      palette ??= Themes.PaletteOrange;
      return new PolylineEdgeStyle {
          Pen = (Pen) new Pen(palette.Fill, 1.5).GetAsFrozen(),
          SourceArrow = new Arrow(ArrowType.None, cropLength: extraCropLength),
          TargetArrow =
              showTargetArrow
                  ? new Arrow(ArrowType.Triangle, palette.Fill, cropLength: extraCropLength, cropAtPort: cropAtPorts)
                  : Arrow.None,
          OrthogonalEditing = orthogonalEditing
      };
    }

    /// <summary>
    ///   Creates a new node label style whose colors match the given palette.
    /// </summary>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.PaletteOrange">default palette</see> should be used.
    /// </param>
    /// <returns>
    ///   A new node label style whose colors match the given palette.
    /// </returns>
    public static LabelStyle CreateDemoNodeLabelStyle(Palette palette = null) {
      palette ??= Themes.PaletteOrange;
      var labelStyle = new LabelStyle();
      labelStyle.Shape = LabelShape.RoundRectangle;
      labelStyle.BackgroundBrush = palette.NodeLabelFill;
      labelStyle.TextBrush = palette.Text;
      labelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      labelStyle.TextAlignment = TextAlignment.Center;
      labelStyle.Padding = new InsetsD(4, 2, 4, 1);
      return labelStyle;
    }

    /// <summary>
    ///   Creates a new edge label style whose colors match the given palette.
    /// </summary>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.PaletteOrange">default palette</see> should be used.
    /// </param>
    /// <returns>
    ///   A new edge label style whose colors match the given palette.
    /// </returns>
    public static LabelStyle CreateDemoEdgeLabelStyle(Palette palette = null) {
      palette ??= Themes.PaletteOrange;
      var labelStyle = new LabelStyle();
      labelStyle.Shape = LabelShape.RoundRectangle;
      labelStyle.BackgroundBrush = palette.EdgeLabelFill;
      labelStyle.TextBrush = palette.Text;
      labelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      labelStyle.TextAlignment = TextAlignment.Center;
      labelStyle.Padding = new InsetsD(4, 2, 4, 1);
      return labelStyle;
    }

    /// <summary>
    ///   Creates a new group label style whose colors match the given palette.
    /// </summary>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.Palette12">default palette for groups</see> should be used.
    /// </param>
    /// <returns>
    ///   A new group label style whose colors match the given palette.
    /// </returns>
    public static ILabelStyle CreateDemoGroupLabelStyle(Palette palette = null) {
      palette ??= Themes.Palette12;
      return new LabelStyle {
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Left,
          ClipText = false,
          TextWrapping = TextWrapping.Wrap,
          TextBrush = palette.NodeLabelFill
      };
    }

    /// <summary>
    ///   Creates a new group node style whose colors match the given palette.
    /// </summary>
    /// <param name="palette">
    ///   The palette to use or <see langword="null"/> if the
    ///   <see cref="Themes.Palette12">default palette for groups</see> should be used.
    /// </param>
    /// <param name="foldingEnabled">Whether the style should be configured for a folding scenario.</param>
    /// <returns>
    ///   A new group node style whose colors match the given palette.
    /// </returns>
    public static GroupNodeStyle CreateDemoGroupStyle(Palette palette = null, bool foldingEnabled = false) {
      palette ??= Themes.Palette12;
      return new GroupNodeStyle {
          GroupIcon = foldingEnabled ? GroupNodeStyleIconType.Minus : GroupNodeStyleIconType.None,
          FolderIcon = GroupNodeStyleIconType.Plus,
          TabBrush = foldingEnabled ? palette.NodeLabelFill : palette.Fill,
          Pen = (Pen) new Pen(palette.Fill, 2.0).GetAsFrozen(),
          TabBackgroundBrush = foldingEnabled ? palette.Fill : null,
          TabPosition = foldingEnabled ? GroupNodeStyleTabPosition.TopTrailing : GroupNodeStyleTabPosition.Top,
          TabWidth = 30.0,
          TabHeight = 20.0,
          TabPadding = 3.0,
          IconOffset = 2.0,
          IconSize = 14.0,
          IconForegroundBrush = palette.Fill,
          HitTransparentContentArea = true
      };
    }
  }
}
