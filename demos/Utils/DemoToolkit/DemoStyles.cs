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

using System.Windows;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Toolkit
{
  public static class DemoStyles
  {
    /// <summary>
    /// Initializes graph defaults with nicely configured built-in yFiles styles.
    /// </summary>
    /// <param name="graph">The graph on which the default styles and style-related setting are set.</param>
    /// <param name="nodeTheme">Optional color set names for demo node styles. The default is <see cref="Themes.PaletteOrange"/>.</param>
    /// <param name="nodeLabelTheme">Optional color set names for demo node label styles. The default is the node theme.</param>
    /// <param name="edgeTheme">Optional color set names for demo edge styles. The default is <see cref="Themes.PaletteOrange"/>.</param>
    /// <param name="edgeLabelTheme">Optional color set names for demo edge label styles. The default is the edge theme.</param>
    /// <param name="groupTheme">Optional color set names for demo group node styles. The default is <see cref="Themes.Palette12"/>.</param>
    /// <param name="groupLabelTheme">Optional color set names for demo group node label styles. The default is group node theme.</param>
    /// <param name="foldingEnabled">whether to use collapsable group node style.</param>
    /// <param name="extraCropLength">the extra crop length for the <see cref="DefaultEdgePathCropper"/>.</param>
    /// <param name="shape">the optional shape of the node style, if <see langword="null"/> a <see cref="RectangleNodeStyle"/> is used.</param>
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
      ShapeNodeShape? shape = null
    ) {
      nodeTheme = nodeTheme ?? Themes.PaletteOrange;
      nodeLabelTheme = nodeLabelTheme ?? nodeTheme;
      edgeTheme = edgeTheme ?? Themes.PaletteOrange;
      edgeLabelTheme = edgeLabelTheme ?? edgeTheme;
      groupTheme = groupTheme ?? Themes.Palette12;
      groupLabelTheme = groupLabelTheme ?? groupTheme;

      graph.NodeDefaults.Style = shape == null
          ? (INodeStyle) CreateDemoNodeStyle(nodeTheme)
          : CreateDemoShapeNodeStyle(shape.Value, nodeTheme);
      graph.NodeDefaults.Labels.Style = CreateDemoNodeLabelStyle(nodeLabelTheme);

      graph.GroupNodeDefaults.Style = CreateDemoGroupStyle(groupTheme, foldingEnabled);
      graph.GroupNodeDefaults.Labels.Style = CreateDemoGroupLabelStyle(groupLabelTheme);
      graph.GroupNodeDefaults.Labels.LayoutParameter =
          new GroupNodeLabelModel().CreateTabBackgroundParameter();

      graph.EdgeDefaults.Style = CreateDemoEdgeStyle(edgeTheme);
      graph.GetDecorator().PortDecorator.EdgePathCropperDecorator.SetImplementation(
        new DefaultEdgePathCropper { CropAtPort = false, ExtraCropLength = extraCropLength });
      graph.EdgeDefaults.Labels.Style = CreateDemoEdgeLabelStyle(edgeLabelTheme);
    }

    /// <summary>
    /// Creates a new rectangular node style whose colors match the given palette.
    /// </summary>
    public static RectangleNodeStyle CreateDemoNodeStyle(Palette palette = null) {
      palette = palette ?? Themes.PaletteOrange;
      return new RectangleNodeStyle {
        Brush = palette.Fill,
        Pen = (Pen) new Pen(palette.Stroke, 1.5).GetAsFrozen(),
        CornerStyle = CornerStyle.Round,
        CornerSize = 3.5
      };
    }

    /// <summary>
    /// Creates a new node style with the given shape whose colors match the given palette.
    /// </summary>
    public static ShapeNodeStyle CreateDemoShapeNodeStyle( ShapeNodeShape shape, Palette palette = null ) {
      palette = palette ?? Themes.PaletteOrange;
      return new ShapeNodeStyle {
        Shape = shape,
        Brush = palette.Fill,
        Pen = (Pen) new Pen(palette.Stroke, 1.5).GetAsFrozen()
      };
    }

    /// <summary>
    /// Creates a new polyline edge style whose colors match the given palette.
    /// </summary>
    public static PolylineEdgeStyle CreateDemoEdgeStyle(Palette palette = null, bool showTargetArrow = true) {
      palette = palette ?? Themes.PaletteOrange;
      return new PolylineEdgeStyle {
        Pen = (Pen) new Pen(palette.Stroke, 1.5).GetAsFrozen(),
        TargetArrow = showTargetArrow
          ? new Arrow() { Brush = palette.Stroke, Type = ArrowType.Triangle }
          : Arrows.None
      };
    }

    /// <summary>
    /// Creates a new node label style whose colors match the given palette.
    /// </summary>
    public static DefaultLabelStyle CreateDemoNodeLabelStyle(Palette palette = null) {
      palette = palette ?? Themes.PaletteOrange;
      var labelStyle = new DefaultLabelStyle();
      labelStyle.Shape = LabelShape.RoundRectangle;
      labelStyle.BackgroundBrush = palette.NodeLabelFill;
      labelStyle.TextBrush = palette.Text;
      labelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      labelStyle.TextAlignment = TextAlignment.Center;
      labelStyle.Insets = new InsetsD(4, 2, 4, 1);
      return labelStyle;
    }

    /// <summary>
    /// Creates a new edge label style whose colors match the given palette.
    /// </summary>
    public static DefaultLabelStyle CreateDemoEdgeLabelStyle(Palette palette = null) {
      palette = palette ?? Themes.PaletteOrange;
      var labelStyle = new DefaultLabelStyle();
      labelStyle.Shape = LabelShape.RoundRectangle;
      labelStyle.BackgroundBrush = palette.EdgeLabelFill;
      labelStyle.TextBrush = palette.Text;
      labelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      labelStyle.TextAlignment = TextAlignment.Center;
      labelStyle.Insets = new InsetsD(4, 2, 4, 1);
      return labelStyle;
    }

    /// <summary>
    /// Creates a new group label style whose colors match the given palette.
    /// </summary>
    public static ILabelStyle CreateDemoGroupLabelStyle(Palette palette = null) {
      palette = palette ?? Themes.Palette12;
      return new DefaultLabelStyle {
        VerticalTextAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Left,
        ClipText = false,
        TextWrapping = TextWrapping.Wrap,
        TextBrush = palette.NodeLabelFill
      };
    }

    /// <summary>
    /// Creates a new group node style whose colors match the given palette.
    /// </summary>
    public static GroupNodeStyle CreateDemoGroupStyle(Palette palette = null, bool foldingEnabled = false) {
      palette = palette ?? Themes.Palette12;
      return new GroupNodeStyle {
        GroupIcon = foldingEnabled ? GroupNodeStyleIconType.Minus : GroupNodeStyleIconType.None,
        FolderIcon = GroupNodeStyleIconType.Plus,
        TabBrush = foldingEnabled ? palette.NodeLabelFill : palette.Fill,
        Pen = (Pen) new Pen(palette.Fill, 2.0).GetAsFrozen(),
        TabBackgroundBrush = foldingEnabled ? palette.Fill : null,
        TabPosition = foldingEnabled ? GroupNodeStyleTabPosition.TopTrailing : GroupNodeStyleTabPosition.Top,
        TabWidth = 30.0,
        TabHeight = 20.0,
        TabInset = 3.0,
        IconOffset = 2.0,
        IconSize = 14.0,
        IconForegroundBrush = palette.Fill,
        HitTransparentContentArea = true
      };
    }
  }
}
