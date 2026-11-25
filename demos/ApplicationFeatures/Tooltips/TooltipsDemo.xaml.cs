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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tooltips
{
  /// <summary>
  ///   Shows how to enable tooltips and popovers for graph items, customize their content, and configure different
  ///   behaviors.
  /// </summary>
  [Demo("tooltips", "application-features", "Tooltips & Popovers",
      Summary = "Shows how to enable tooltips and popovers for graph items.", Tags = ["hover", "interaction"],
      Keywords = ["mouseHoverInputMode", "query-item-tool-tip", "events", "data", "json"])]
  public sealed partial class TooltipsDemo
  {
    public TooltipsDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      // initialize graph component
      var inputMode = new GraphViewerInputMode();
      graphControl.InputMode = inputMode;

      // configure tooltips and popovers
      InitializeTooltips(inputMode);
      InitializePopovers(inputMode);

      // build a sample graph to demonstrate the different tooltips and popovers
      BuildGraph(graphControl);
    }

    /// <summary>
    ///   Initializes tooltips that show when hovering an item.
    /// </summary>
    private static void InitializeTooltips(GraphViewerInputMode inputMode) {
      // the demo shows only node tooltips, but it works the same for any graph item
      inputMode.ToolTipItems = GraphItemTypes.Node;

      // register a listener for when a tooltip should be shown
      inputMode.QueryItemToolTip += (_, evt) => {
        if (evt.Handled) {
          // tooltip content has already been assigned -> nothing to do
          return;
        }

        // this is a node because of the chosen ToolTipItems above
        if (evt.Item is not INode node) {
          return;
        }

        // by default, the tooltip is opened at the pointer and stays open until the element is exited
        evt.ToolTip = PopupConfigurator.CreateTooltipContent(evt.Context.CanvasControl, node);

        // depending on the node, the demo illustrates different tooltip behaviors on different nodes
        var labelTexts = node.Labels.Select(label => label.Text).ToArray();
        if (labelTexts.Contains("Above Node")) {
          PopupConfigurator.ConfigureAboveNodeTooltip(evt);
        } else if (labelTexts.Contains("Follow Pointer")) {
          PopupConfigurator.ConfigureFollowPointer(evt);
        } else if (labelTexts.Contains("Viewport Bottom Center")) {
          PopupConfigurator.ConfigureViewportBottomCenterTooltip(evt);
        }

        // indicate that the tooltip content has been set
        evt.Handled = true;
      };
    }

    /// <summary>
    ///   Initializes popovers that open on node click.
    /// </summary>
    private static void InitializePopovers(GraphViewerInputMode inputMode) {
      inputMode.ItemClicked += async (_, evt) => {
        if (evt.Item is not INode node) {
          return;
        }

        // depending on the node, the demo illustrates different popover behaviors
        var labelTexts = node.Labels.Select(label => label.Text).ToArray();
        if (labelTexts.Contains("Light-dismissable Popover")) {
          await PopupConfigurator.ConfigureLightDismissablePopover(inputMode, evt);
        } else if (labelTexts.Contains("Manual Popover")) {
          await PopupConfigurator.ConfigureManuelPopover(inputMode, evt);
        }
      };
    }

    /// <summary>
    ///   Creates a sample graph.
    /// </summary>
    private static void BuildGraph(GraphControl graphControl) {
      var graph = graphControl.Graph;
      graph.NodeDefaults.Size = new SizeD(100, 100);
      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;
      graph.NodeDefaults.Labels.Style = new LabelStyle {
          TextBrush = Brushes.White,
          TextSize = 12,
          Typeface = new Typeface(new FontFamily("Arial"), FontStyles.Italic, FontWeights.Normal, FontStretches.Normal)
      };

      CreateStyledNode(graph, new PointD(0, 0), "Hover", "Default Behavior", Themes.Palette91);
      CreateStyledNode(graph, new PointD(200, 0), "Hover", "Above Node", Themes.Palette92);
      CreateStyledNode(graph, new PointD(400, 0), "Hover", "Follow Pointer", Themes.Palette93);
      CreateStyledNode(graph, new PointD(600, 0), "Hover", "Viewport Bottom Center", Themes.Palette94);

      // light-dismissable popovers
      CreateStyledNode(graph, new PointD(800, -200), "Click", "Light-dismissable Popover", Themes.Palette95);
      CreateStyledNode(graph, new PointD(800, 0), "Click", "Light-dismissable Popover", Themes.Palette95);
      CreateStyledNode(graph, new PointD(800, 200), "Click", "Light-dismissable Popover", Themes.Palette95);

      // manual popovers
      CreateStyledNode(graph, new PointD(1000, -200), "Click", "Manual Popover", Themes.Palette96);
      CreateStyledNode(graph, new PointD(1000, 0), "Click", "Manual Popover", Themes.Palette96);
      CreateStyledNode(graph, new PointD(1000, 200), "Click", "Manual Popover", Themes.Palette96);

      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates a styled node with given labels.
    /// </summary>
    private static void CreateStyledNode(
        IGraph graph,
        PointD location,
        string centerText,
        string bottomText,
        Palette colorSet) {
      var node = graph.CreateNode(location, DemoStyles.CreateDemoNodeStyle(colorSet));
      graph.AddLabel(node, centerText);
      graph.AddLabel(node, bottomText, ExteriorNodeLabelModel.Bottom, DemoStyles.CreateDemoNodeLabelStyle(colorSet));
    }

    /// <summary>
    ///   Requests focus for the node delete button in a popover.
    /// </summary>
    private void SetFocusOnLoaded(object sender, RoutedEventArgs e) {
      (sender as FrameworkElement)?.Focus();
    }

    /// <summary>
    ///   Gets a command that closes a node's popover and deletes the node.
    /// </summary>
    public ICommand CloseAndDelete => new CloseAndDeleteCommand(() => graphControl.Graph);

    [STAThread]
    public static void Main() {
      Run<TooltipsDemo>();
    }
  }
}
