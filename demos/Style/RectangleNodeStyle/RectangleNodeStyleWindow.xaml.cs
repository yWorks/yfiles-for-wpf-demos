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

namespace Demo.yFiles.Graph.RectangleNodeStyle
{
  /// <summary>
  /// A demo that showcases how different properties of <see cref="RectangleNodeStyle"/> change the visualization.
  /// </summary>
  /// <remarks>
  /// Several nodes with different <see cref="RectangleNodeStyle"/> configurations are provided on startup and the
  /// configuration of a style can be changed via an option panel.
  /// <para>
  /// The properties, that can be changed, include:
  /// <list type="bullet">
  ///   <item>whether corners should be rounded or cut by a diagonal line</item>
  ///   <item>whether the corner size should be used as absolute value or be scaled by the node size
  ///         (this is mainly important when resizing the node)</item>
  ///   <item>the (absolute or relative) corner size</item>
  ///   <item>which corners should be affected</item>
  /// </list>
  /// </para>
  /// </remarks>
  public partial class RectangleNodeStyleWindow
  {
    /// <summary>
    /// Wires up the UI components and adds a
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public RectangleNodeStyleWindow() {
      InitializeComponent();
      InitializeStyleDefaults();
      CreateSampleNodes();
      InitializeInputMode();
      InitializeOptionSettings();
    }

    /// <summary>
    /// Initializes defaults for the given graph
    /// </summary>
    private void InitializeStyleDefaults() {
      // Set defaults for new nodes
      var gray = Themes.Palette75;
      Graph.NodeDefaults.Style = new yWorks.Graph.Styles.RectangleNodeStyle {
        Brush = gray.Fill,
        Pen = new Pen(gray.Stroke, 1)
      };
      Graph.NodeDefaults.Size = new SizeD(300, 100);
      Graph.NodeDefaults.ShareStyleInstance = false;
    }

    /// <summary>
    /// Creates a small sample with different node style settings.
    /// </summary>
    private void CreateSampleNodes() {
      var yellow = Themes.Palette71;
      var orange = Themes.Palette72;
      var green = Themes.Palette73;
      var blue = Themes.Palette74;

      // Create nodes with round corners with different resizing behaviors
      CreateNode(new Point(0, 0), yellow, CornerStyle.Round, false, 10);
      CreateNode(new Point(0, 200), orange, CornerStyle.Round, true, 0.2);
      CreateNode(new Point(0, 400), green, CornerStyle.Round, true, 0.5);
      CreateNode(new Point(0, 600), blue, CornerStyle.Round, true, 0.8, Corners.Bottom);

      // Create nodes with cut-off corners with different resizing behaviors
      CreateNode(new Point(400, 0), yellow, CornerStyle.Cut, false, 10);
      CreateNode(new Point(400, 200), orange, CornerStyle.Cut, true, 0.2);
      CreateNode(new Point(400, 400), green, CornerStyle.Cut, true, 0.5);
      CreateNode(new Point(400, 600), blue, CornerStyle.Cut, true, 0.8, Corners.Bottom);
    }

    /// <summary>
    /// Creates a node with a label that describes the configuration of the RectangleNodeStyle.
    /// </summary>
    /// <param name="location">The location of the node.</param>
    /// <param name="color">The color set of the node and label.</param>
    /// <param name="cornerStyle">Whether corners should be round or a line.</param>
    /// <param name="scaleCornerSize">Whether the corner size should be used as absolute value or be scaled with the node size.</param>
    /// <param name="cornerSize">The corner size.</param>
    /// <param name="corners">Which corners are drawn with the given corner style.</param>
    private void CreateNode(PointD location, Palette color, CornerStyle cornerStyle, bool scaleCornerSize,
        double cornerSize, Corners corners = Corners.All) {
      var style = new yWorks.Graph.Styles.RectangleNodeStyle {
          Brush = color.Fill,
          Pen = new Pen(color.Stroke, 1),
          CornerStyle = cornerStyle,
          ScaleCornerSize = scaleCornerSize,
          CornerSize = cornerSize,
          Corners = corners
      };

      var node = Graph.CreateNode(location, style);
      AddLabel(node, color);
    }

    /// <summary>
    /// Adds a label that describes the owner's style configuration.
    /// </summary>
    /// <param name="node">The owner of the label.</param>
    /// <param name="color"> The color set of the label.</param>
    private void AddLabel(INode node, Palette color) {
      Graph.AddLabel(
          node,
          StyleToText(node.Style as yWorks.Graph.Styles.RectangleNodeStyle),
          InteriorLabelModel.Center,
          new DefaultLabelStyle {
              TextBrush = color.Text, BackgroundBrush = color.NodeLabelFill, TextSize = 12, Insets = new InsetsD(5)
          }
      );
    }
    
    /// <summary>
    /// Returns a text description of the style configuration.
    /// </summary>
    private static string StyleToText(yWorks.Graph.Styles.RectangleNodeStyle style) {
      return "Corner Style: " + Enum.GetName(typeof(CornerStyle), style.CornerStyle) + "\n" +
             "Corner Size Scaling: " + Enum.GetName(typeof(CornerSizeScaling), BoolToEnumValue(style.ScaleCornerSize)) + "\n" +
             "Affected Corners: " + CornersToText(style.Corners);
    }

    /// <summary>
    /// Returns a text description of the given corner configuration.
    /// </summary>
    private static string CornersToText(Corners corners) {
      Corners[] all = {
          Corners.All, Corners.Top, Corners.Bottom, Corners.Right, Corners.Left, Corners.TopLeft, Corners.TopRight,
          Corners.BottomLeft, Corners.BottomRight
      };
      string[] affected = all.Where(corner => {
        if ((corners & corner) == corner) {
          corners &= ~corner;
          return true;
        }
        return false;
      }).Select(CornerValueToText).ToArray();
      return affected.Length > 0 ? string.Join(" & ", affected) : "none";
    }

    /// <summary>
    /// Returns the display text for the given corner value.
    ///</summary>
    private static string CornerValueToText(Corners corner) {
      return Enum.GetName(typeof(Corners), corner);
    }

    /// <summary>
    /// Sets up an input mode for the GraphControl, and adds a custom handle
    /// that allows to change the corner size.
    /// </summary>
    private void InitializeInputMode() {
      var inputMode = new GraphEditorInputMode {
        AllowCreateEdge = false, AllowAddLabel = false, AllowEditLabel = false, SelectableItems = GraphItemTypes.Node
      };
      graphControl.InputMode = inputMode;  
      
      // add a label to newly created node that shows the current style settings
      inputMode.NodeCreated += (sender, evt) => {
        var node = evt.Item;
        AddLabel(node, Themes.Palette75);
      };
          
      // listen for selection changes to update the option handler for the style properties
      GraphControl.Selection.ItemSelectionChanged += (sender, e) => {
        if (e.ItemSelected) {
          AdjustOptionPanel((INode) e.Item);
        }
      };

      var nodeDecorator = Graph.GetDecorator().NodeDecorator;

      // add handle that enables the user to change the corner size of a node
      nodeDecorator.HandleProviderDecorator.SetImplementationWrapper(
          n => n.Style is yWorks.Graph.Styles.RectangleNodeStyle,
          (node, delegateProvider) =>
              new CornerSizeHandleProvider(node, () => AdjustOptionPanel(node), delegateProvider)
      );

      // only provide reshape handles for the east, south and south-east sides so they don't clash with the corner size handle
      nodeDecorator.ReshapeHandleProviderDecorator.SetFactory(node =>
          new NodeReshapeHandleProvider(node, node.Lookup<IReshapeHandler>(),
              HandlePositions.East | HandlePositions.South | HandlePositions.SouthEast));

      nodeDecorator.SelectionDecorator.HideImplementation();
    }

    /// <summary>
    /// Center the sample graph in the visible area.
    /// </summary>
    protected virtual void OnLoaded(object source, EventArgs e) {
      graphControl.FitGraphBounds();
    }

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

    private const string RectangleNodeStyleProperties = "RectangleNodeStyle properties";

    private const string CornerStyleName = "Corner Style";
    private const string ScaleCornerSizeName = "Corner Size Scaling";

    private const string CornersName = "Corners";

    private OptionHandler handler;

    private IOptionItem cornerStyleItem;
    private IOptionItem cornerSizeScalingItem;
    private IOptionItem topLeftCornerItem;
    private IOptionItem topRightCornerItem;
    private IOptionItem bottomRightCornerItem;
    private IOptionItem bottomLeftCornerItem;

    /// <summary>
    /// Initializes the OptionHandler for changing the node style properties and assigns it to the option panel.
    /// </summary>
    private void InitializeOptionSettings() {
      SetupHandler();
      AddEditorControlToForm();
    }

    private void SetupHandler() {
      handler = new OptionHandler(RectangleNodeStyleProperties);
      var propertyGroup = handler.AddGroup(RectangleNodeStyleProperties);
      cornerStyleItem = propertyGroup.AddList(
        CornerStyleName,
        new List<CornerStyle> { CornerStyle.Round, CornerStyle.Cut },
        CornerStyle.Round);
      cornerStyleItem.PropertyChanged += OnStylePropertyChanged;
      cornerSizeScalingItem = propertyGroup.AddList(
        ScaleCornerSizeName,
        new List<CornerSizeScaling> { CornerSizeScaling.Absolute, CornerSizeScaling.Relative },
        CornerSizeScaling.Absolute);
      cornerSizeScalingItem.PropertyChanged += OnStylePropertyChanged;

      var cornerGroup = propertyGroup.AddGroup(CornersName);
      topLeftCornerItem = cornerGroup.AddBool(Enum.GetName(typeof(Corners), Corners.TopLeft), true);
      topLeftCornerItem.PropertyChanged += OnStylePropertyChanged;
      topRightCornerItem = cornerGroup.AddBool(Enum.GetName(typeof(Corners), Corners.TopRight), true);
      topRightCornerItem.PropertyChanged += OnStylePropertyChanged;
      bottomRightCornerItem = cornerGroup.AddBool(Enum.GetName(typeof(Corners), Corners.BottomRight), true);
      bottomRightCornerItem.PropertyChanged += OnStylePropertyChanged;
      bottomLeftCornerItem = cornerGroup.AddBool(Enum.GetName(typeof(Corners), Corners.BottomLeft), true);
      bottomLeftCornerItem.PropertyChanged += OnStylePropertyChanged;

      OnStylePropertyChanged();
    }

    private bool optionsUpdating;

    /// <summary>
    /// Applies the style settings from the option panel to the styles of all selected nodes as well as to the
    /// default style used for new nodes.
    /// </summary>
    private void OnStylePropertyChanged(object sender = null, PropertyChangedEventArgs e = null) {
      if (optionsUpdating) {
        return;
      }
      var cornerStyle = (CornerStyle) cornerStyleItem.Value;
      var scaleCornerSize = EnumValueToBool((CornerSizeScaling) cornerSizeScalingItem.Value);

      var affectedCorners = Corners.None;
      if ((bool) topLeftCornerItem.Value) {
        affectedCorners |= Corners.TopLeft;
      }
      if ((bool) topRightCornerItem.Value) {
        affectedCorners |= Corners.TopRight;
      }
      if ((bool) bottomRightCornerItem.Value) {
        affectedCorners |= Corners.BottomRight;
      }
      if ((bool) bottomLeftCornerItem.Value) {
        affectedCorners |= Corners.BottomLeft;
      }

      foreach (var node in GraphControl.Selection.SelectedNodes) {
        if (node.Style is yWorks.Graph.Styles.RectangleNodeStyle style) {
          ApplyStyleSettings(style, cornerStyle, scaleCornerSize, affectedCorners);  
          if (node.Labels.Count == 0) {
            Graph.AddLabel(node, StyleToText(style));
          } else {
            Graph.SetLabelText(node.Labels.First(), StyleToText(style));
          }
        }
      }
      var defaultStyle = (yWorks.Graph.Styles.RectangleNodeStyle) Graph.NodeDefaults.Style;
      ApplyStyleSettings(defaultStyle, cornerStyle, scaleCornerSize, affectedCorners);
      graphControl.Invalidate();
    }

    /// <summary>
    /// Applies new settings to a <see cref="RectangleNodeStyle"/>.
    /// </summary>
    /// <param name="style">The style to apply the settings.</param>
    /// <param name="cornerStyle">The round or cut corner style.</param>
    /// <param name="scaleCornerSize">Whether the corner size should be scaled by the node size.</param>
    /// <param name="corners">The corners to set.</param>
    private static void ApplyStyleSettings(
        yWorks.Graph.Styles.RectangleNodeStyle style,
        CornerStyle cornerStyle,
        bool scaleCornerSize,
        Corners corners
    ) {
      style.CornerStyle = cornerStyle;
      style.ScaleCornerSize = scaleCornerSize;
      style.Corners = corners;
    }

    /// <summary>
    /// Adjusts the option panel to show the style settings of a newly selected node.
    /// </summary>
    private void AdjustOptionPanel(INode node) {
      var style = node.Style as yWorks.Graph.Styles.RectangleNodeStyle;
      if (style == null) {
        return;
      }
      optionsUpdating = true;
      cornerStyleItem.Value = style.CornerStyle;
      cornerSizeScalingItem.Value = BoolToEnumValue(style.ScaleCornerSize);
      var corners = style.Corners;
      topLeftCornerItem.Value = (corners & Corners.TopLeft) == Corners.TopLeft;
      topRightCornerItem.Value = (corners & Corners.TopRight) == Corners.TopRight;
      bottomRightCornerItem.Value = (corners & Corners.BottomRight) == Corners.BottomRight;
      bottomLeftCornerItem.Value = (corners & Corners.BottomLeft) == Corners.BottomLeft;
      optionsUpdating = false;
    }

    private void AddEditorControlToForm() {
      editorControl.OptionHandler = handler;
    }

    private static CornerSizeScaling BoolToEnumValue(bool value) {
      return value ? CornerSizeScaling.Relative : CornerSizeScaling.Absolute;
    }

    private static bool EnumValueToBool(CornerSizeScaling value) {
      return CornerSizeScaling.Relative == value;
    }

    private enum CornerSizeScaling
    {
      Absolute,
      Relative
    }

    #endregion
  }
}
