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
using System.Text.Json;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.ViewportLimiter
{
  [Demo("viewport-limiter", "application-features", "Viewport Limiter",
      Summary = "Shows how to use the viewport limiter to constrain the graph navigation.",
      Tags = ["viewport", "interaction", "navigation"], Keywords = ["zoom", "pan", "viewportlimiter"])]
  public sealed partial class ViewportLimiterDemo
  {
    // private readonly ViewportLimiterSettings settings = new();

    // Visuals for showing the various areas relevant for limiter calculations
    private RectangleVisualCreator contentMarginsVisual = null!;
    private RectangleVisualCreator limiterBoundsVisual = null!;

    private IRenderTreeElement? contentMarginsElement;
    private IRenderTreeElement? limiterBoundsElement;

    public ViewportLimiterDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Runs this demo.
    /// </summary>
    protected override void Initialize() {
      // Enable navigation
      graphControl.InputMode = new GraphViewerInputMode();

      // Configures default styles for newly created graph elements
      InitializeGraph(graphControl.Graph);

      // Build the graph from the given data set
      BuildGraph(graphControl.Graph);

      // Layout and center the graph
      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // Enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);

      AddRectangleVisuals();

      InitializeUI();
    }

    /// <summary>
    ///   Applies the UI settings to the viewport limiter.
    /// </summary>
    private void UpdateViewportLimiterSettings(ViewportLimiterSettings settings) {
      var viewportLimiter = graphControl.ViewportLimiter;
      viewportLimiter.StrictBoundsContainment = settings.StrictBoundsContainment;
      viewportLimiter.ViewportContentMargins = new InsetsD(settings.MarginLeft, settings.MarginTop,
          settings.MarginRight, settings.MarginBottom);
      viewportLimiter.MinimumViewportContentRatio = new SizeD(settings.RatioWidth, settings.RatioHeight);

      viewportLimiter.Policy = settings.Policy;

      UpdateRectangleVisualsRendering(settings.Policy);
      graphControl.Invalidate();
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json);
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    private static void InitializeGraph(IGraph graph) {
      // Set styles for this demo
      DemoStyles.InitDemoStyles(graph);

      // Set sizes and locations specific for this demo
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(0.5, EdgeSides.BelowEdge);
    }

    /// <summary>
    ///   Adds rectangle visuals for content area, limiter ratio margins and ratio.
    /// </summary>
    private void AddRectangleVisuals() {
      var renderTree = graphControl.RenderTree;

      renderTree.CreateElement(renderTree.BackgroundGroup,
          new RectangleVisualCreator("Content Area", true,
              (Brush) new SolidColorBrush(Color.FromRgb(246, 246, 246)).GetAsFrozen(), Pens.Black,
              () => graphControl.ContentBounds));

      contentMarginsVisual = new RectangleVisualCreator("Margins", false, Brushes.Transparent,
          (Pen) new Pen(Brushes.Red, 1) { DashStyle = new DashStyle([3.0, 6.0], 0.0) }.GetAsFrozen(),
          CalculateMarginsRect, true);

      limiterBoundsVisual = new RectangleVisualCreator("Viewport Limiter Bounds", true, Brushes.Transparent, Pens.Blue,
          CalculateEffectiveRect, true);
    }

    /// <summary>
    ///   Calculates the rectangle defined by the GraphControl size and
    ///   ViewportLimiter viewport margins.
    /// </summary>
    private RectD? CalculateMarginsRect() {
      var viewportLimiter = graphControl.ViewportLimiter;
      if (viewportLimiter.Policy == ViewportLimitingPolicy.Unrestricted) {
        return null;
      }

      var viewportContentMargins = viewportLimiter.ViewportContentMargins;
      var left = viewportContentMargins.Left;
      var top = viewportContentMargins.Top;

      var graphControlSize = graphControl.InnerSize;
      var right = graphControlSize.Width - viewportContentMargins.Right;
      var bottom = graphControlSize.Height - viewportContentMargins.Bottom;

      var width = right - left;
      var height = bottom - top;

      var centerView = graphControlSize * 0.5;

      return new RectD(width <= 0 ? centerView.Width : left, height <= 0 ? centerView.Height : top,
          width <= 0 ? 1 : width, height <= 0 ? 1 : height);
    }

    /// <summary>
    ///   Calculates the effective ViewportLimiter area (bounds) as the margins rectangle scaled by the available
    ///   space as defined by the viewportContentRatio.
    /// </summary>
    private RectD? CalculateEffectiveRect() {
      var viewportLimiter = graphControl.ViewportLimiter;
      if (viewportLimiter.Policy == ViewportLimitingPolicy.Unrestricted) {
        return null;
      }

      var margins = viewportLimiter.ViewportContentMargins;
      var ratio = viewportLimiter.MinimumViewportContentRatio;
      var innerSize = graphControl.InnerSize;
      var availableWidth = innerSize.Width - innerSize.Width * ratio.Width;
      var availableHeight = innerSize.Height - innerSize.Height * ratio.Height;
      var effectiveMargins = new InsetsD(
          Math.Min(margins.Left, margins.Left * availableWidth / margins.HorizontalInsets),
          Math.Min(margins.Top, margins.Top * availableHeight / margins.VerticalInsets),
          Math.Min(margins.Right, margins.Right * availableWidth / margins.HorizontalInsets),
          Math.Min(margins.Bottom, margins.Bottom * availableHeight / margins.VerticalInsets));
      var innerWidth = innerSize.Width - Math.Min(effectiveMargins.HorizontalInsets, innerSize.Width);
      var innerHeight = innerSize.Height - Math.Min(effectiveMargins.VerticalInsets, innerSize.Height);

      return new RectD(effectiveMargins.Left, effectiveMargins.Top, innerWidth, innerHeight);
    }

    /// <summary>
    ///   Enables or disables the rendering of the various rectangle visuals,
    ///   depending on the chosen limiter policy.
    /// </summary>
    private void UpdateRectangleVisualsRendering(ViewportLimitingPolicy policy) {
      var renderTree = graphControl.RenderTree;

      if (policy == ViewportLimitingPolicy.Unrestricted && contentMarginsElement != null) {
        renderTree.Remove(contentMarginsElement);
        contentMarginsElement = null;
        renderTree.Remove(limiterBoundsElement!);
        limiterBoundsElement = null;
      } else if (contentMarginsElement == null) {
        contentMarginsElement = renderTree.CreateElement(renderTree.ForegroundGroup, contentMarginsVisual);
        limiterBoundsElement = renderTree.CreateElement(renderTree.ForegroundGroup, limiterBoundsVisual);
      }
    }

    /// <summary>
    ///   Animates the projection between two alpha and scale values.
    /// </summary>
    private void AnimateProjection(double fromAlpha, double toAlpha, double fromScale, double toScale) {
      var projectionAnimation = new ProjectionAnimation(graphControl, fromAlpha, toAlpha, fromScale, toScale);
      new Animator(graphControl).Animate(projectionAnimation.CreateEasedAnimation());
    }

    /// <summary>
    ///   Toggles the isometric view.
    /// </summary>
    private void ToggleProjection(bool isometric) {
      if (isometric) {
        // Set isometric projection
        AnimateProjection(0, -Math.PI / 4, 1, Math.Sqrt(3) / 3);
      } else {
        // Revert isometric projection
        AnimateProjection(-Math.PI / 4, 0, Math.Sqrt(3) / 3, 1);
      }
    }

    /// <summary>
    ///   Initializes the UI elements that are specific to this demo.
    /// </summary>
    private void InitializeUI() {
      var settings = new ViewportLimiterSettings();
      settingsPanel.DataContext = settings;
      isometricViewToggle.DataContext = settings;

      settings.PropertyChanged += (_, args) => {
        if (args.PropertyName == nameof(ViewportLimiterSettings.IsometricView)) {
          ToggleProjection(settings.IsometricView);
        } else {
          UpdateViewportLimiterSettings(settings);
        }
      };

      UpdateViewportLimiterSettings(settings);
    }

    [STAThread]
    private static void Main() {
      Run<ViewportLimiterDemo>();
    }
  }
}
