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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.ArrowNodeStyle
{
  /// <summary>
  /// A demo that showcases how different properties of <see cref="ArrowNodeStyle"/> change the visualization.
  /// </summary>
  /// <remarks>
  /// Several nodes with different <see cref="ArrowNodeStyle"/> configurations are provided on startup and the
  /// configuration of a style can be changed via an option panel.
  /// <para>
  /// The properties, that can be changed, include:
  /// <list type="bullet">
  ///   <item>the basic shape of the style</item>
  ///   <item>the direction the shape is pointing to</item>
  ///   <item>the angle of the arrow tip</item>
  ///   <item>the thickness of the arrow shaft as ratio of the node size</item>
  /// </list>
  /// </para>
  /// </remarks>
  public partial class ArrowNodeStyleWindow
  {

    #region Initialization

    /// <summary>
    /// Wires up the UI components and adds a
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public ArrowNodeStyleWindow() {
      InitializeComponent();
      InitializeStyleDefaults();
      CreateSampleGraph();
      InitializeInteraction();
      InitializeOptionSettings();
    }

    /// <summary>
    /// Center the sample graph in the visible area.
    /// </summary>
    /// <seealso cref="InitializeStyleDefaults"/>
    protected virtual void OnLoaded(object source, EventArgs e) {
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    /// Initializes the default styles.
    /// </summary>
    private void InitializeStyleDefaults() {
      var orange = Themes.PaletteOrange;
      Graph.NodeDefaults.Style =
          new yWorks.Graph.Styles.ArrowNodeStyle { Brush = orange.Fill, Pen = new Pen(orange.Stroke, 1) };
      Graph.NodeDefaults.Size = new SizeD(200, 100);
      Graph.NodeDefaults.ShareStyleInstance = false;
      Graph.NodeDefaults.Labels.Style = CreateLabelStyle(orange);
      Graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorLabelModel { Insets = new InsetsD(30) }.CreateParameter(ExteriorLabelModel.Position.South);
    }

    /// <summary>
    /// Creates a new node label style with colors from the given palette.
    /// </summary>
    private static ILabelStyle CreateLabelStyle(Palette palette) {
      return new DefaultLabelStyle {
          Shape = LabelShape.RoundRectangle,
          BackgroundBrush = palette.NodeLabelFill,
          TextBrush = palette.Text,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Left,
          Insets = new InsetsD(8, 4, 8, 4),
          TextSize = 14
      };
    }

    /// <summary>
    /// Creates the initial sample graph.
    /// </summary>
    private void CreateSampleGraph() {
      // create nodes with different shapes, angles and shaft ratios
      CreateNodes(0, ArrowStyleShape.Arrow, Themes.PaletteOrange);
      CreateNodes(300, ArrowStyleShape.DoubleArrow, Themes.PaletteBlue);
      CreateNodes(600, ArrowStyleShape.NotchedArrow, Themes.PaletteRed);
      CreateNodes(900, ArrowStyleShape.Parallelogram, Themes.PaletteGreen);
      CreateNodes(1200, ArrowStyleShape.Trapezoid, Themes.PalettePurple);
    }

    /// <summary>
    /// Creates several nodes with the given shape and different angles and shaft ratios.
    /// <param name="xOffset">The x-location where to place the nodes.</param>
    /// <param name="shape">The shape to use for the arrow.</param>
    /// <param name="palette">The colors to use for nodes and labels.</param>
    /// </summary>
    private void CreateNodes(double xOffset, ArrowStyleShape shape, Palette palette) {
      var angleFactor = shape == ArrowStyleShape.Parallelogram || shape == ArrowStyleShape.Trapezoid ? 0.5 : 1;

      yWorks.Graph.Styles.ArrowNodeStyle[] styles = {
          // small angle and shaft ratio pointing left
          new yWorks.Graph.Styles.ArrowNodeStyle {
              Shape = shape,
              Direction = ArrowNodeDirection.Left,
              Angle = (angleFactor * Math.PI) / 8,
              ShaftRatio = 0.25,
              Brush = palette.Fill,
              Pen = new Pen(palette.Stroke, 1)
          },
          // default angle and shaft ratio pointing up
          new yWorks.Graph.Styles.ArrowNodeStyle {
              Shape = shape,
              Direction = ArrowNodeDirection.Up,
              Angle = (angleFactor * Math.PI) / 4,
              ShaftRatio = 1.0 / 3,
              Brush = palette.Fill,
              Pen = new Pen(palette.Stroke, 1)
          },
          // bigger angle and shaft ratio pointing right
          new yWorks.Graph.Styles.ArrowNodeStyle {
              Shape = shape,
              Direction = ArrowNodeDirection.Right,
              Angle = (angleFactor * Math.PI * 3) / 8,
              ShaftRatio = 0.75,
              Brush = palette.Fill,
              Pen = new Pen(palette.Stroke, 1)
          },
          // negative angle and max shaft ratio pointing right
          new yWorks.Graph.Styles.ArrowNodeStyle {
              Shape = shape,
              Direction = ArrowNodeDirection.Right,
              Angle = (angleFactor * -Math.PI) / 8,
              ShaftRatio = 1,
              Brush = palette.Fill,
              Pen = new Pen(palette.Stroke, 1)
          }
      };

      // create a sample node for each sample style instance
      var y = 0;
      for (var i = 0; i < styles.Length; ++i) {
        var x = xOffset + (i == 1 ? 50 : 0);
        var width = i == 1 ? 100 : 200;
        var height = i == 1 ? 200 : 100;
        var style = styles[i];
        Graph.AddLabel(
            Graph.CreateNode(new Rect(x, y, width, height), style),
            StyleToText(style),
            new ExteriorLabelModel { Insets = new InsetsD(30) }.CreateParameter(ExteriorLabelModel.Position.South),
            CreateLabelStyle(palette)
        );
        y += height + 250;
      }
    }

    private void InitializeInteraction() {
      // reserve some space for the angle adjustment handle
      GraphControl.ContentMargins = new InsetsD(20);
      
      var inputMode = new GraphEditorInputMode {
        AllowCreateEdge = false, AllowAddLabel = false, AllowEditLabel = false, SelectableItems = GraphItemTypes.Node
      };
      GraphControl.InputMode = inputMode;

      // add a label to newly created node that shows the current style settings
      inputMode.NodeCreated += ((sender, evt) => {
        var node = evt.Item;
        Graph.AddLabel(node, StyleToText(node.Style as yWorks.Graph.Styles.ArrowNodeStyle));
      });
      
      // listen for selection changes to update the option handler for the style properties
      GraphControl.Selection.ItemSelectionChanged += (sender, e) => {
        if (e.ItemSelected) {
          AdjustOptionPanel((INode) e.Item);
        }
      };

      var nodeDecorator = graphControl.Graph.GetDecorator().NodeDecorator;

      // add handle that enables the user to change the angle and shaft ratio of a node style
      nodeDecorator.HandleProviderDecorator.SetImplementationWrapper(
          n => n.Style is yWorks.Graph.Styles.ArrowNodeStyle,
          (node, delegateProvider) =>
              new ArrowNodeStyleHandleProvider(node, () => {
                AdjustOptionPanel(node);
                var style = (yWorks.Graph.Styles.ArrowNodeStyle) node.Style;
                if (node.Labels.Count == 0) {
                  Graph.AddLabel(node, StyleToText(style));
                } else {
                  Graph.SetLabelText(node.Labels.First(), StyleToText(style));
                }
              }, delegateProvider)
      );

      // only provide reshape handles for the east, south and south-east sides so they don't clash with the custom handles
      nodeDecorator.ReshapeHandleProviderDecorator.SetFactory(node =>
          new NodeReshapeHandleProvider(node, node.Lookup<IReshapeHandler>(),
              HandlePositions.East | HandlePositions.South | HandlePositions.SouthEast));

      nodeDecorator.SelectionDecorator.HideImplementation();
    }

    private static string StyleToText(yWorks.Graph.Styles.ArrowNodeStyle style) {
      return
          "Shape: " + Enum.GetName(typeof(ArrowStyleShape), style.Shape) + "\n" +
          "Direction: " + Enum.GetName(typeof(ArrowNodeDirection), style.Direction) + "\n" +
          "Angle: " + Math.Round(ToDegrees(style.Angle), 0) + "\n" +
          "Shaft Ratio: " + Math.Round(style.ShaftRatio, 1);
    }

    /// <summary>
    /// Returns the given angle in degrees.
    /// </summary>
    private static double ToDegrees(double radians) {
      return (radians * 180) / Math.PI;
    }

    /// <summary>
    /// Returns the given angle in radians.
    /// </summary>
    private static double ToRadians(double degrees) {
      return (degrees / 180) * Math.PI;
    }

    #endregion

    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    public GraphControl GraphControl {
      get { return graphControl; }
    }

    /// <summary>
    /// Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    public IGraph Graph {
      get { return GraphControl.Graph; }
    }

    #region Option Handler For Node Style Properties

    private const string ArrowNodeStyleProperties = "ArrowNodeStyle properties";

    private const string ArrowStyleShapeName = "Basic Shape";
    private const string DirectionName = "Shape Direction";
    private const string AngleName = "Angle";
    private const string ShaftRatioName = "Shaft Ratio";

    private OptionHandler handler;

    private IOptionItem arrowStyleShapeItem;
    private IOptionItem directionItem;
    private IOptionItem angleItem;
    private IOptionItem shaftRatioItem;

    /// <summary>
    /// Initializes the OptionHandler for changing the node style properties and assigns it to the option panel.
    /// </summary>
    private void InitializeOptionSettings() {
      SetupHandler();
      AddEditorControlToForm();
    }

    private void SetupHandler() {
      handler = new OptionHandler(ArrowNodeStyleProperties);
      var propertyGroup = handler.AddGroup(ArrowNodeStyleProperties);

      arrowStyleShapeItem = propertyGroup.AddOptionItem(
          new CollectionOptionItem<ArrowStyleShape>(ArrowStyleShapeName,
              new List<ArrowStyleShape> {
                  ArrowStyleShape.Arrow,
                  ArrowStyleShape.DoubleArrow,
                  ArrowStyleShape.NotchedArrow,
                  ArrowStyleShape.Parallelogram,
                  ArrowStyleShape.Trapezoid
              },
              ArrowStyleShape.Arrow
          ));
      arrowStyleShapeItem.PropertyChanged += OnArrowShapeChanged;
      directionItem = propertyGroup.AddOptionItem(
          new CollectionOptionItem<ArrowNodeDirection>(DirectionName,
              new List<ArrowNodeDirection> {
                  ArrowNodeDirection.Right, ArrowNodeDirection.Down, ArrowNodeDirection.Left, ArrowNodeDirection.Up,
              },
              ArrowNodeDirection.Right
          ));
      directionItem.PropertyChanged += OnDirectionChanged;
      angleItem = propertyGroup.AddDouble(AngleName, 90, -180, 180);
      angleItem.PropertyChanged += OnAngleChanged;
      shaftRatioItem = propertyGroup.AddDouble(ShaftRatioName, 0.3, 0, 1);
      shaftRatioItem.PropertyChanged += OnShaftRatioChanged;
    }

    private bool optionsUpdating;

    /// <summary>
    /// Applies the arrow shape setting from the option panel to the styles of all selected nodes as well as to the
    /// default style used for new nodes.
    /// </summary>
    private void OnArrowShapeChanged(object sender = null, PropertyChangedEventArgs e = null) {
      var newShape = (ArrowStyleShape) arrowStyleShapeItem.Value;
      ApplyStyleSetting(style => style.Shape = newShape);
    }

    /// <summary>
    /// Applies the direction setting from the option panel to the styles of all selected nodes as well as to the
    /// default style used for new nodes.
    /// </summary>
    private void OnDirectionChanged(object sender = null, PropertyChangedEventArgs e = null) {
      var newDirection = (ArrowNodeDirection) directionItem.Value;
      ApplyStyleSetting(style => style.Direction = newDirection);
    }

    /// <summary>
    /// Applies the angle setting from the option panel to the styles of all selected nodes as well as to the
    /// default style used for new nodes.
    /// </summary>
    private void OnAngleChanged(object sender = null, PropertyChangedEventArgs e = null) {
      var newAngle = Math.Round((double) angleItem.Value, 0);
      angleItem.Value = newAngle;
      ApplyStyleSetting(style => style.Angle = ToRadians(newAngle));
    }

    /// <summary>
    /// Applies the shaft ratio setting from the option panel to the styles of all selected nodes as well as to the
    /// default style used for new nodes.
    /// </summary>
    private void OnShaftRatioChanged(object sender = null, PropertyChangedEventArgs e = null) {
      var newShaftRatio = Math.Round((double) shaftRatioItem.Value, 1);
      shaftRatioItem.Value = newShaftRatio;
      ApplyStyleSetting(style => style.ShaftRatio = newShaftRatio);
    }

    private void ApplyStyleSetting(Action<yWorks.Graph.Styles.ArrowNodeStyle> adjustStyle) {
      if (optionsUpdating) {
        return;
      }
      foreach (var node in GraphControl.Selection.SelectedNodes) {
        if (node.Style is yWorks.Graph.Styles.ArrowNodeStyle style) {
          adjustStyle(style);
          if (node.Labels.Count == 0) {
            Graph.AddLabel(node, StyleToText(style));
          } else {
            Graph.SetLabelText(node.Labels.First(), StyleToText(style));
          }
        }
      }

      // adjust also the default style applied to newly created nodes
      adjustStyle(Graph.NodeDefaults.Style as yWorks.Graph.Styles.ArrowNodeStyle);

      GraphControl.Invalidate();
    }

    /// <summary>
    /// Adjusts the option panel to show the style settings of a newly selected node.
    /// </summary>
    private void AdjustOptionPanel(INode node) {
      var style = node.Style as yWorks.Graph.Styles.ArrowNodeStyle;
      if (style == null) {
        return;
      }
      optionsUpdating = true;

      arrowStyleShapeItem.Value = style.Shape;
      directionItem.Value = style.Direction;
      angleItem.Value = Math.Round(ToDegrees(style.Angle), 0);
      shaftRatioItem.Value = Math.Round(style.ShaftRatio, 1);

      // update defaultArrowNodeStyle to correspond to the option panel
      if (Graph.NodeDefaults.Style is yWorks.Graph.Styles.ArrowNodeStyle defaultArrowNodeStyle) {
        defaultArrowNodeStyle.Shape = style.Shape;
        defaultArrowNodeStyle.Direction = style.Direction;
        defaultArrowNodeStyle.Angle = style.Angle;
        defaultArrowNodeStyle.ShaftRatio = style.ShaftRatio;
        Graph.NodeDefaults.Size = node.Layout.GetSize();
      }

      optionsUpdating = false;
    }

    private void AddEditorControlToForm() {
      editorControl.OptionHandler = handler;
    }

    #endregion
  }
}
