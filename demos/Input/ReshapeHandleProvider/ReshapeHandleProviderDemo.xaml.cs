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
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace yWorks.Demo.ReshapeHandleProvider
{
  /// <summary>
  ///   Shows how to implement a custom <see cref="IReshapeHandleProvider"/>.
  /// </summary>
  [Demo("reshape-handle-provider", "input", "Reshape Handle Provider",
      Summary = "Shows how to add resize handles to ports.", Tags = ["interaction", "resizing", "port"],
      Keywords = ["handles", "reshape", "size", "scale"])]
  public sealed partial class ReshapeHandleProviderDemo
  {
    private GraphEditorInputMode graphEditorInputMode = null!;

    public ReshapeHandleProviderDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Registers a callback function as a decorator that provides a customized
    ///   <see cref="IReshapeHandleProvider"/> for each port with a <see cref="ShapePortStyle"/>.
    ///   This callback function is called whenever a port in the graph is queried
    ///   for its <see cref="IReshapeHandleProvider"/>. In this case, the <c>port</c>
    ///   parameter will be set to that port.
    /// </summary>
    private void RegisterReshapeHandleProvider(IGraph graph) {
      graph.GetDecorator().Ports.GetDecoratorFor<IReshapeHandleProvider>().AddFactory(
          port => port.Style is ShapePortStyle,
          port => new PortReshapeHandleProvider(port, (ShapePortStyle) port.Style, new SizeD(5, 5)));
    }

    protected override void Initialize() {
      // initialize graph defaults
      InitializeGraphDefaults(graphControl.Graph);

      // create a default editor input mode
      graphEditorInputMode = NewEditorInputMode();
      // set the input mode to the graph control.
      graphControl.InputMode = graphEditorInputMode;

      // PortReshapeHandleProvider considers pressed Shift keys.
      // Whenever Shift is pressed or released, we force GraphEditorInputMode to requery the handles
      // of selected items
      graphControl.KeyDown += OnKeyDown;
      graphControl.KeyUp += OnKeyUp;
      graphControl.LostKeyboardFocus += OnBlur;

      // register the reshape handle provider for ports
      RegisterReshapeHandleProvider(graphControl.Graph);

      // create initial graph
      using var stream = LoadAssemblyResource("sample.graphml");
      new GraphMLIOHandler().Read(graphControl.Graph, stream);
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Configures default behavior and visualizations for the elements of the given graph.
    /// </summary>
    /// <param name="graph">The graph to be configured.</param>
    private static void InitializeGraphDefaults(IGraph graph) {
      var brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#61A044"));
      graph.NodeDefaults.Ports.Style = new ShapePortStyle {
          Brush = brush,
          Pen = new Pen(Brushes.Transparent, 1),
          RenderSize = new SizeD(7, 7)
      };
      // each port needs its own style instance to have its own render size
      graph.NodeDefaults.Ports.ShareStyleInstance = false;
      // disable removing ports when all attached edges have been removed
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x2E, 0x28, 0x2A)), 3),
          OrthogonalEditing = true
      };
    }

    /// <summary>
    ///   Creates a new editor input mode to handle user interaction in this demo.
    /// </summary>
    private static GraphEditorInputMode NewEditorInputMode() {
      // create a default editor input mode
      var mode = new GraphEditorInputMode {
          // ports are preferred for clicks
          ClickHitTestOrder = [
              GraphItemTypes.Port,
              GraphItemTypes.PortLabel,
              GraphItemTypes.Bend,
              GraphItemTypes.EdgeLabel,
              GraphItemTypes.Edge,
              GraphItemTypes.Node,
              GraphItemTypes.NodeLabel
          ]
      };

      return mode;
    }

    /// <summary>
    ///   Triggers <see cref="GraphEditorInputMode.RequeryHandles"/> when the Shift modifier key is pressed.
    /// </summary>
    private void OnKeyDown(object sender, KeyEventArgs e) {
      if (e.Key is Key.LeftShift or Key.RightShift) {
        // update handles if no input gesture is performed
        if (graphEditorInputMode.MutexOwner == null) {
          graphEditorInputMode.RequeryHandles();
        }
      }
    }

    /// <summary>
    ///   Triggers <see cref="GraphEditorInputMode.RequeryHandles"/> when the Shift modifier key is released.
    /// </summary>
    private void OnKeyUp(object sender, KeyEventArgs e) {
      if (e.Key is Key.LeftShift or Key.RightShift) {
        // update handles if no input gesture is performed
        if (graphEditorInputMode.MutexOwner == null) {
          graphEditorInputMode.RequeryHandles();
        }
      }
    }

    /// <summary>
    ///   Triggers <see cref="GraphEditorInputMode.RequeryHandles"/> when the graph control loses focus.
    /// </summary>
    private void OnBlur(object sender, KeyboardFocusChangedEventArgs e) {
      // update handles
      graphEditorInputMode.RequeryHandles();
    }

    [STAThread]
    public static void Main() => Run<ReshapeHandleProviderDemo>();
  }
}
