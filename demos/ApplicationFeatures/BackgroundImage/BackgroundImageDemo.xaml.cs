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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.BackgroundImage
{
  [Demo("background-image", "application-features", "Background Image",
      Summary = "Shows how to add a background visualizations to a graph component.",
      Keywords = ["IRenderTreeGroup", "backgroundGroup"], Tags = ["background", "icon"])]
  public sealed partial class BackgroundImageDemo
  {
    private IRenderTreeElement? background;

    public BackgroundImageDemo() {
      InitializeComponent();
      graphControl.ContentBoundsChanged += (_, _) => background?.Invalidate();
    }

    protected override void Initialize() {
      graphControl.InputMode = new GraphEditorInputMode();
      var graph = graphControl.Graph;

      // Configure default styles for newly created graph elements.
      InitializeGraph(graph);

      // Build the graph from the given data set.
      BuildGraph(graph);

      // Layout and center the graph.
      graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // Enable the undo engine now to prevent undoing the graph creation.
      graph.SetUndoEngineEnabled(true);

      // Set the initial background.
      DisplayImage();
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var jsonDocument = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, jsonDocument);
    }

    /// <summary>
    ///   Creates the image and puts it in the background of the graph.
    /// </summary>
    private void DisplayImage() {
      var renderTree = graphControl.RenderTree;
      // Delete what is already displayed in the background.
      if (background != null) {
        renderTree.Remove(background);
      }
      // Create the image and display it.
      background = renderTree.CreateElement(renderTree.BackgroundGroup,
          new ImageVisualCreator((ImageSource) FindResource("yfiles-logo")));
    }

    /// <summary>
    ///   Creates a colored rectangle and puts it in the background of the graph.
    /// </summary>
    private void DisplayRectangle() {
      var renderTree = graphControl.RenderTree;
      // Delete what is already displayed in the background.
      if (background != null) {
        renderTree.Remove(background);
      }
      // Create the rectangle and display it.
      background = renderTree.CreateElement(renderTree.BackgroundGroup, new RectangleVisualCreator(graphControl));
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // Set styles for this demo.
      DemoStyles.InitDemoStyles(graph);

      // Set the style, label style and label parameter for group nodes.
      graph.GroupNodeDefaults.Style = new GroupNodeStyle {
          TabBrush = Brushes.DarkGray,
          TabWidth = 60,
          TabPosition = GroupNodeStyleTabPosition.TopTrailing,
          ContentAreaBrush = Brushes.White
      };
      graph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          TextAlignment = TextAlignment.Right,
          TextBrush = Brushes.Black
      };
      graph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabParameter();

      // Set sizes and locations specific to this demo.
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(sideOfEdge: EdgeSides.BelowEdge);
    }

    /// <summary>
    ///   Displays the image and enables the rectangle button instead of the image button.
    /// </summary>
    private void OnImageClicked(object sender, RoutedEventArgs e) {
      DisplayImage();
      rectangleButton.IsEnabled = true;
      imageButton.IsEnabled = false;
    }

    /// <summary>
    ///   Displays the colored rectangle and enables the image button instead of the rectangle button.
    /// </summary>
    private void OnRectangleClicked(object sender, RoutedEventArgs e) {
      DisplayRectangle();
      rectangleButton.IsEnabled = false;
      imageButton.IsEnabled = true;
    }

    [STAThread]
    public static void Main() {
      Run<BackgroundImageDemo>();
    }
  }
}
