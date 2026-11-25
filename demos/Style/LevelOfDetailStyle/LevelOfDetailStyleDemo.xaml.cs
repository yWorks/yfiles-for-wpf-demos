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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.LevelOfDetailStyle
{
  /// <summary>
  ///   Support three styles for different zoom level.
  /// </summary>
  [Demo("level-of-detail-style", "style", "Level of Detail Style",
      Summary = "Shows a node style that hides details when zooming out.", Tags = ["style", "performance"],
      Keywords = ["data binding", "readability", "hide", "rendering", "sketch"])]
  public sealed partial class LevelOfDetailStyleDemo
  {
    /**
     * Support three styles for different zoom level.
     */
    private LevelOfDetailNodeStyle levelOfDetailNodeStyle = null!;

    /**
     * The Popup text that shows in which level we are now.
     */
    private string? oldZoomState;

    private readonly DispatcherTimer popupHideTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    public LevelOfDetailStyleDemo() {
      // initialize the GraphComponent
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      // initialize input Mode
      GraphControl.InputMode = new GraphViewerInputMode();

      // set default styles for the nodes and the edges
      InitTutorialDefaults(Graph);

      // check the zooming level and update the Popup text that shows in which level we are now
      UpdateDetailLevelIndicator();

      // build a sample graph
      CreateGraph();

      // center the graph
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this tutorial.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private void InitTutorialDefaults(IGraph graph) {
      // set styles that are the same for all tutorials
      DemoStyles.InitDemoStyles(graph);

      // set styles, sizes and locations specific for this tutorial
      levelOfDetailNodeStyle = new LevelOfDetailNodeStyle(new NodeControlNodeStyle("DetailLevelNodeControlStyle"),
          new NodeControlNodeStyle("IntermediateLevelNodeControlStyle"),
          new NodeControlNodeStyle("OverviewLevelNodeControlStyle"));

      graph.NodeDefaults.Style = levelOfDetailNodeStyle;
      graph.NodeDefaults.Size = new SizeD(285, 100);
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 2),
          TargetArrow = Arrow.None
      };
    }

    /// <summary>
    ///   Updates the indicator for the level of details to the current graph.
    /// </summary>
    private void UpdateDetailLevelIndicator() {
      var zoomLevelChangedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
      zoomLevelChangedTimer.Tick += (_, _) => {
        zoomLevelChangedTimer.Stop();
        UpdateIndicator();
      };

      popupHideTimer.Tick += (_, _) => {
        popupHideTimer.Stop();
        detailLevelPopup.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(1)));
      };

      GraphControl.ZoomChanged += (_, _) => {
        // update the zoom level display after 200ms
        if (!zoomLevelChangedTimer.IsEnabled) {
          zoomLevelChangedTimer.Start();
        }
      };
    }

    /// <summary>
    ///   Updates the levels indicator.
    /// </summary>
    private void UpdateIndicator() {
      string zoomState;
      if (GraphControl.Zoom >= levelOfDetailNodeStyle.DetailThreshold) {
        zoomState = "Detail Level";
      } else if (GraphControl.Zoom > levelOfDetailNodeStyle.IntermediateThreshold) {
        zoomState = "Intermediate Level";
      } else {
        zoomState = "Overview Level";
      }

      if (oldZoomState != zoomState) {
        detailLevelPopupText.Text = oldZoomState = zoomState;
        detailLevelPopup.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1)));
        popupHideTimer.Stop();
        popupHideTimer.Start();
      }
    }

    /// <summary>
    ///   Create a sample graph.
    /// </summary>
    private void CreateGraph() {
      var graph = Graph;
      var size = graph.NodeDefaults.Size;
      var n1 = graph.CreateNode(new RectD(-size.Width / 2, -size.Height / 2, size.Width, size.Height),
          tag: new Person("Chief Executive Officer", "Eric Joplin", "ejoplin@yoyodyne.com", "555-0100"));
      var n2 = graph.CreateNode(new RectD(-size.Width / 2, 150 - size.Height / 2, size.Width, size.Height),
          tag: new Person("Chief Executive Assistant", "Gary Roberts", "groberts@yoyodyne.com", "555-0100"));
      var n3 = graph.CreateNode(new RectD(-175 - size.Width / 2, 300 - size.Height / 2, size.Width, size.Height),
          tag: new Person("Senior Executive Assistant", "Alex Burns", "aburns@yoyodyne.com", "555-0102"));
      var n4 = graph.CreateNode(new RectD(175 - size.Width / 2, 300 - size.Height / 2, size.Width, size.Height),
          tag: new Person("Junior Executive Assistant", "Linda Newland", "lnewland@yoyodyne.com", "555-0112"));
      graph.CreateEdge(n1, n2);
      var leftEdge = graph.CreateEdge(n2, n3);
      graph.AddBend(leftEdge, new PointD(0, 230));
      graph.AddBend(leftEdge, new PointD(-175, 230));
      var rightEdge = graph.CreateEdge(n2, n4);
      graph.AddBend(rightEdge, new PointD(0, 230));
      graph.AddBend(rightEdge, new PointD(175, 230));
      GraphControl.FitGraphBounds();
    }

    private void GraphControlSizeChanged(object sender, SizeChangedEventArgs e) {
      detailLevelPopup.Margin = new Thickness(0, 0, 0, GraphControl.ActualHeight * 0.1);
    }

    [STAThread]
    public static void Main() {
      Run<LevelOfDetailStyleDemo>();
    }
  }
}
