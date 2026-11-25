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
using System.Collections.Generic;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Router;

namespace yWorks.Demo.BundledEdgeRouter
{
  /// <summary>
  ///   Demonstrates how to configure the <see cref="BundledEdgeRouter"/> with a given strategy.
  /// </summary>
  [Demo("layout-bundled-edge-router", "layout-features", "Bundled Edge Router",
      Summary = "Shows common configuration options for the bundled edge routing algorithm.",
      Tags = ["bundle", "router"],
      Keywords = [
          "layout", "router", "bundling", "bundle", "routing-strategy", "EdgeBundleDescriptor", "curved", "curves",
          "splines", "routing", "playground"
      ])]
  public class BundledEdgeRouterDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Define the node and label styles.
      Graph.NodeDefaults.Style = new ShapeNodeStyle {
          Brush = new SolidColorBrush(Color.FromRgb(0x01, 0x2c, 0x69)),
          Shape = ShapeNodeShape.Ellipse,
          Pen = null
      };
      Graph.NodeDefaults.Labels.Style = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xdc, 0xeb, 0xf7)),
          Shape = LabelShape.RoundRectangle,
          Padding = new InsetsD(2, 2, 2, 2)
      };
      Graph.NodeDefaults.Labels.LayoutParameter = ExteriorNodeLabelModel.Top;

      // Because Bézier fitting is enabled, applying BezierEdgeStyle yields better visual results
      Graph.EdgeDefaults.Style = new BezierEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x32, 0x6b, 0xbb)), 0.2),
          SourceArrow = Arrow.None,
          TargetArrow = Arrow.None
      };

      BuildGraphFromJson("sample.json");

      ToolBar.AddComboBox("Routing Strategy: ",
          new List<(BundledEdgeRouterStrategy value, string label)> {
              (BundledEdgeRouterStrategy.Spanner, "Spanner-based"),
              (BundledEdgeRouterStrategy.Voronoi, "Voronoi-based")
          }.AsReadOnly(), strategy => ApplyBundledEdgeRouter(strategy), BundledEdgeRouterStrategy.Spanner);

      // Run an initial layout using the spanner-based strategy
      ApplyBundledEdgeRouter(BundledEdgeRouterStrategy.Spanner);
    }

    /// <summary>
    ///   Configures the bundled edge router with a given strategy.
    /// </summary>
    private void ApplyBundledEdgeRouter(BundledEdgeRouterStrategy? strategy = null) {
      // Configure the bundled edge router with the specified strategy
      var bundledEdgeRouter = new Layout.Router.BundledEdgeRouter();
      if (strategy.HasValue) {
        bundledEdgeRouter.Strategy = strategy.Value;
      }

      // Set the desired bundling strength
      bundledEdgeRouter.EdgeBundling.BundlingStrength = 0.85;

      // Enable bundling and Bézier fitting so a Bézier curve is fitted to every edge.
      // In this case, we use the BezierEdgeStyle for a better visual result.
      bundledEdgeRouter.EdgeBundling.DefaultBundleDescriptor = new EdgeBundleDescriptor {
          Bundled = true,
          BezierFitting = true
      };

      // Apply a labeling algorithm and set the bundled edge router as core layout
      var layout = new GenericLabeling {
          CoreLayout = bundledEdgeRouter,
          Scope = LabelingScope.NodeLabels
      };
      // Apply the layout algorithm
      Graph.ApplyLayout(layout);

      // If no labeling is desired
      // Graph.ApplyLayout(bundledEdgeRouter);
      GraphControl.FitGraphBounds();
    }

    [STAThread]
    private static void Main() => Run<BundledEdgeRouterDemo>();
  }
}
