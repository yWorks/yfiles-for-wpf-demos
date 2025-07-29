/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Shapes;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Input.SizeConstraintProvider
{
  /// <summary>
  ///   Shows how to customize the resizing behavior of INodes by implementing a
  ///   custom <see cref="INodeSizeConstraintProvider"/>.
  /// </summary>
  public partial class SizeConstraintProviderWindow
  {
    /// <summary>
    ///   Registers a callback function as decorator that provides a custom
    ///   <see cref="INodeSizeConstraintProvider"/> for each node.
    /// </summary>
    /// <remarks>
    ///   This callback function is called whenever a node in the graph is queried
    ///   for its <see cref="INodeSizeConstraintProvider"/>. In this case, the 'node' parameter will be set
    ///   to that node.
    /// </remarks>
    public void RegisterSizeConstraintProvider(MutableRectangle boundaryRectangle) {
      var nodeDecorator = graphControl.Graph.GetDecorator().Nodes;
      nodeDecorator.SizeConstraintProvider.AddFactory(node => {
        // Obtain the tag from the node
        var nodeTag = node.Tag;

        // Check if it is a known tag and choose the respective implementation.
        // Fallback to the default behavior otherwise.
        if (!(nodeTag is Palette)) {
          return null;
        }
        if (Themes.PaletteLightblue.Equals(nodeTag)) {
          return new BlueSizeConstraintProvider(node);
        }
        if (Themes.PaletteGreen.Equals(nodeTag)) {
          return new GreenSizeConstraintProvider(node);
        }
        if (Themes.PaletteOrange.Equals(nodeTag)) {
          return new NodeSizeConstraintProvider(new SizeD(50, 50), new SizeD(300, 300), boundaryRectangle.ToRectD());
        }
        return null;
      });
    }

    #region Initialization

    public SizeConstraintProviderWindow() {
      InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      var graph = graphControl.Graph;

      // Create a default editor input mode
      var graphEditorInputMode = new GraphEditorInputMode();

      // Just for user convenience: disable node and edge creation,
      graphEditorInputMode.AllowCreateEdge = false;
      graphEditorInputMode.AllowCreateNode = false;
      // disable deleting items
      graphEditorInputMode.DeletableItems = GraphItemTypes.None;
      // disable node moving
      graphEditorInputMode.MovableSelectedItems = GraphItemTypes.None;
      graphEditorInputMode.MovableUnselectedItems = GraphItemTypes.None;
      // enable the undo feature
      graph.SetUndoEngineEnabled(true);

      // Finally, set the input mode to the graph control.
      graphControl.InputMode = graphEditorInputMode;

      // Create the rectangle that limits the movement of some nodes
      // and add it to the GraphControl.
      var boundaryRectangle = new MutableRectangle(210, 350, 30, 30);
      var renderTree = graphControl.RenderTree;
      renderTree.CreateElement(renderTree.RootGroup, boundaryRectangle, new LimitingRectangleRenderer());

      RegisterSizeConstraintProvider(boundaryRectangle);

      CreateSampleGraph(graph);

      // reset the Undo queue so the initial graph creation cannot be undone
      graph.GetUndoEngine().Clear();

      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates the visualization for the limiting rectangle.
    /// </summary>
    private sealed class LimitingRectangleRenderer : ObjectRendererBase<MutableRectangle, Rectangle>
    {
      protected override Rectangle CreateVisual(IRenderContext context, MutableRectangle rect) {
        var rectangle = new Rectangle { Stroke = Brushes.Black, StrokeThickness = 2 };
        rectangle.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect.ToRect());
        return rectangle;
      }

      protected override Rectangle UpdateVisual(IRenderContext context, Rectangle oldVisual, MutableRectangle rect) {
        oldVisual.SetValue(CanvasControl.CanvasControlArrangeRectProperty, rect.ToRect());
        return oldVisual;
      }
    }

    #endregion

    /// <summary>
    ///   An <see cref="INodeSizeConstraintProvider"/> that prevents shrinking of
    ///   nodes. Additionally, neither side of the node can become larger than
    ///   three times its initial size in each resizing operation.
    /// </summary>
    public class BlueSizeConstraintProvider : INodeSizeConstraintProvider
    {
      private readonly INode node;

      public BlueSizeConstraintProvider(INode node) {
        this.node = node;
      }

      /// <summary>
      ///   Returns the current node size to prevent the shrinking of nodes.
      /// </summary>
      public SizeD GetMinimumSize() {
        return node.Layout.GetSize();
      }

      /// <summary>
      ///   Returns three times the current node size.
      /// </summary>
      public SizeD GetMaximumSize() {
        return node.Layout.GetSize() * 3;
      }

      /// <summary>
      ///   Returns an empty rectangle since this area is not constrained.
      /// </summary>
      public RectD GetMinimumEnclosedArea() {
        return RectD.Empty;
      }
    }

    /// <summary>
    ///   An <see cref="INodeSizeConstraintProvider"/> that returns the size of the
    ///   first label as minimum size. The maximum size is not limited.
    /// </summary>
    public class GreenSizeConstraintProvider : INodeSizeConstraintProvider
    {
      private readonly INode node;

      public GreenSizeConstraintProvider(INode node) {
        this.node = node;
      }

      /// <summary>
      ///   Returns the label size to prevent the shrinking of nodes beyond their
      ///   label's size.
      /// </summary>
      public SizeD GetMinimumSize() {
        if (node.Labels.Count > 0) {
          foreach (var label in node.Labels) {
            var labelProvider = label.Lookup<INodeSizeConstraintProvider>();
            if (labelProvider != null) {
              return labelProvider.GetMinimumSize();
            }

            if (label.LayoutParameter.Model is InteriorNodeLabelModel) {
              return label.PreferredSize;
            }
          }
        }
        return new SizeD(1, 1);
      }

      /// <summary>
      ///   Returns the infinite size since the maximum size is not limited.
      /// </summary>
      public SizeD GetMaximumSize() {
        return SizeD.Infinite;
      }

      /// <summary>
      ///   Returns an empty rectangle since this area is not constrained.
      /// </summary>
      public RectD GetMinimumEnclosedArea() {
        return RectD.Empty;
      }
    }

    #region Sample Graph Creation

    /// <summary>
    ///   Creates the sample graph of this demo.
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {
      CreateNode(graph, 100, 100, 100, 60, Themes.PaletteLightblue, Colors.WhiteSmoke, "Never Shrink\n(Max 3x)");
      CreateNode(graph, 300, 100, 160, 30, Themes.PaletteLightblue, Colors.WhiteSmoke, "Never Shrink (Max 3x)");
      CreateNode(graph, 100, 215, 100, 30, Themes.PaletteGreen, Colors.WhiteSmoke, "Enclose Label");
      CreateNode(graph, 300, 200, 140, 80, Themes.PaletteGreen, Colors.WhiteSmoke, "Enclose Label,\nEven Large Ones");
      CreateNode(graph, 200, 340, 140, 140, Themes.PaletteOrange, Colors.Black,
          "Encompass Rectangle,\nMin and Max Size");
    }

    /// <summary>
    ///   Creates a sample node for this demo.
    /// </summary>
    private static void CreateNode(
        IGraph graph,
        double x,
        double y,
        double w,
        double h,
        Palette palette,
        Color textColor,
        string labelText) {
      var node = graph.CreateNode(new RectD(x, y, w, h), DemoStyles.CreateDemoNodeStyle(palette), palette);
      graph.SetStyle(graph.AddLabel(node, labelText), DemoStyles.CreateDemoNodeLabelStyle(palette));
    }

    #endregion
  }
}
