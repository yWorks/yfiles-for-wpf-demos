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
using System.Linq;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CustomHandleProvider
{
  /// <summary>
  ///   Shows how to implement custom handles that allow interactively changing the shape of an
  ///   <see cref="ArrowNodeStyle"/>.
  /// </summary>
  [Demo("custom-handle-provider", "input", "Custom Handle Provider",
      Summary =
          "Shows how to implement custom handles that allow to interactively change the shape of an ArrowNodeStyle.",
      Tags = ["interaction", "style"], Keywords = ["handles", "arrow", "shaft", "IHandleProvider", "IHandle"])]
  public sealed partial class CustomHandleProviderDemo
  {
    public CustomHandleProviderDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      InitializeGraph(graphControl.Graph);
      InitializeInteraction();
      graphControl.FitGraphBounds();
      graphControl.Selection.Add(graphControl.Graph.Nodes.First());
    }

    /// <summary>
    ///   Initializes defaults for the given graph with the ArrowNodeStyle and creates a node.
    /// </summary>
    private void InitializeGraph(IGraph graph) {
      // create an ArrowNodeStyle instance with default angle and shaft ratio pointing to the right
      var palette = Themes.Palette13;
      var arrowStyle = new ArrowNodeStyle {
          Brush = palette.Fill,
          Pen = new Pen(palette.Stroke, 1)
      };

      // initialize the graph defaults
      var labelModel = new FreeNodeLabelModel();
      graph.NodeDefaults.Labels.LayoutParameter = labelModel.CreateParameter(new PointD(0.5, 0), new PointD(0, -50),
          new PointD(0.5, 1), new PointD(0, 0));
      var labelStyle = DemoStyles.CreateDemoNodeLabelStyle(palette);
      labelStyle.TextSize = 16;
      labelStyle.Padding = new InsetsD(8, 10, 8, 10);

      graph.NodeDefaults.Style = arrowStyle;
      graph.NodeDefaults.Size = new SizeD(200, 100);
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Labels.Style = labelStyle;

      // create a node with the default style
      var node = graph.CreateNode(new RectD(0, 0, 400, 200));

      // create a label that shows the current angle and shaft ratio
      graph.AddLabel(node, StyleToText(arrowStyle));
    }

    /// <summary>
    ///   Sets up an input mode for the GraphControl, and adds custom handles to change the angle and
    ///   shaft ratio of the arrow.
    /// </summary>
    private void InitializeInteraction() {
      var inputMode = new GraphEditorInputMode { SelectableItems = GraphItemTypes.Node };
      graphControl.InputMode = inputMode;

      // add a label to newly created node that shows the current style settings
      inputMode.NodeCreated += (_, args) => {
        graphControl.Graph.AddLabel(args.Item, StyleToText((ArrowNodeStyle) args.Item.Style));
      };

      var graph = graphControl.Graph;
      var nodeDecorator = graph.GetDecorator().Nodes;

      // add handles that enable the user to change the angle and shaft ratio of an arrow node style
      nodeDecorator.HandleProvider.AddWrapperFactory(node => node.Style is ArrowNodeStyle,
          (node, delegateProvider) => new ArrowNodeStyleHandleProvider(
              graphControl, node, () => UpdateLabel(graph, node), delegateProvider));

      // only provide reshape handles for the right, bottom and bottom-right sides, so they don't clash with
      // the custom handles
      nodeDecorator.ReshapeHandleProvider.AddFactory(node => new NodeReshapeHandleProvider(node,
          (IReshapeHandler) node.Lookup(typeof(IReshapeHandler))!,
          HandlePositions.Right | HandlePositions.Bottom | HandlePositions.BottomRight));

      // don't show the selection decoration to make the above handles more visible
      nodeDecorator.SelectionRenderer.Hide();
    }

    /// <summary>
    ///   Updates the label's text to show the current style settings.
    /// </summary>
    private static void UpdateLabel(IGraph graph, INode node) {
      if (node.Style is ArrowNodeStyle style) {
        var label = node.Labels.FirstOrDefault();
        var text = StyleToText(style);
        if (label != null) {
          graph.SetLabelText(label, text);
        } else {
          graph.AddLabel(node, text);
        }
      }
    }

    /// <summary>Returns a text description of the style configuration.</summary>
    private static string StyleToText(ArrowNodeStyle style) {
      var angle = (style.Angle * 180 / Math.PI).ToString("F0", CultureInfo.InvariantCulture);
      var shaftRatio = style.ShaftRatio.ToString("F2", CultureInfo.InvariantCulture);
      return $"Angle of the arrow: {angle}°\nRatio of the shaft: {shaftRatio}";
    }

    [STAThread]
    private static void Main(string[] args) {
      Run<CustomHandleProviderDemo>();
    }
  }
}
