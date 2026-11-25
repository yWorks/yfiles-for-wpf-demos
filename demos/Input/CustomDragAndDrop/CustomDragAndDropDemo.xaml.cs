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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CustomDragAndDrop
{
  [Demo("custom-drag-and-drop", "input", "Custom Drag and Drop",
      Summary = "Shows how to change the color of nodes and edges using drag and drop operations.",
      Keywords = ["palette", "colors", "dnd"], Tags = ["interaction", "drag and drop"],
      RelatedDemos = ["drag-and-drop", "graph-drag-and-drop", "drag-from-component", "component-drag-and-drop"])]
  public sealed partial class CustomDragAndDropDemo
  {
    private const int PaletteSize = 15;
    private GraphEditorInputMode graphEditorInputMode = null!;

    public CustomDragAndDropDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      InitializeInputModes(GraphControl);

      InitializeStyles(Graph);

      InitializePalette();

      BuildGraphFromJson("graph-data.json", Graph);

      // center the sample graph in the visible area
      GraphControl.FitGraphBounds();

      // enable undo and redo
      Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Adds support for drop operations and enables interactive editing for the given graph component.
    /// </summary>
    private void InitializeInputModes(GraphControl graphControl) {
      graphEditorInputMode = new GraphEditorInputMode();

      // add the input mode to drop colors
      graphEditorInputMode.Add(new ColorDropInputMode());

      graphControl.InputMode = graphEditorInputMode;
    }

    /// <summary>
    ///   Specifies the default styles for nodes and edges.
    /// </summary>
    private static void InitializeStyles(IGraph graph) {
      graph.NodeDefaults.Size = new SizeD(50, 50);
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = Brushes.DarkGray,
          Pen = new Pen(Brushes.Transparent, 0)
      };

      graph.EdgeDefaults.ShareStyleInstance = false;
      graph.EdgeDefaults.Style = new PolylineEdgeStyle { Pen = new Pen(Brushes.DarkGray, 5) };
    }

    /// <summary>
    ///   Populates the palette with some colors, which can be dragged from the palette and dropped onto
    ///   a node or edge.
    /// </summary>
    private void InitializePalette() {
      for (var i = 0; i < PaletteSize; i++) {
        var color = CreatePaletteColor(i);
        var entry = CreatePaletteEntry(color);
        palette.Children.Add(entry);
      }
    }

    private static Color CreatePaletteColor(int index) {
      var hue = index * 360.0 / PaletteSize;
      var chroma = 1.0;
      var x = chroma * (1 - Math.Abs(hue / 60 % 2 - 1));
      var (red, green, blue) = hue switch {
          < 60 => (chroma, x, 0.0),
          < 120 => (x, chroma, 0.0),
          < 180 => (0.0, chroma, x),
          < 240 => (0.0, x, chroma),
          < 300 => (x, 0.0, chroma),
          _ => (chroma, 0.0, x)
      };
      return Color.FromRgb((byte) Math.Round(red * 255), (byte) Math.Round(green * 255), (byte) Math.Round(blue * 255));
    }

    /// <summary>
    ///   Creates a WPF element that visualizes the given color in the drag and drop panel.
    /// </summary>
    private Border CreatePaletteEntry(Color color) {
      // create an image that visualizes the color
      var paletteImage = new Ellipse {
          Width = 30,
          Height = 30,
          Fill = new SolidColorBrush(color)
      };

      // create a border that holds the image of the color
      var paletteEntry = new Border {
          Width = 60,
          Height = 60,
          Background = Brushes.Transparent,
          Child = paletteImage,
          Cursor = Cursors.Hand,
          Tag = color
      };

      // start the drop input mode when a drag from the palette begins
      paletteEntry.PreviewMouseMove += (sender, evt) => {
        if (evt.LeftButton != MouseButtonState.Pressed || sender is not Border { Tag: Color color } item) {
          return;
        }

        var data = new DataObject();
        data.SetData(typeof(Color), color);
        DragDrop.DoDragDrop(item, data, DragDropEffects.Copy);
        evt.Handled = true;
      };

      return paletteEntry;
    }

    [STAThread]
    public static void Main() => Run<CustomDragAndDropDemo>();
  }
}
