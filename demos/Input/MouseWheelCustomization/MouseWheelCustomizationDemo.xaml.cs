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
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.MouseWheelCustomization
{
  /// <summary>
  ///   Interaction logic for MouseWheelCustomizationDemo.xaml
  /// </summary>
  [Demo("mouse-wheel-customization", "input", "Mouse Wheel Customization",
      Summary = "Shows how to customize and enhance the default mouse wheel behavior.", Tags = ["interaction"],
      Keywords = ["wheel", "mousewheel", "scroll", "zoom", "resize"], RelatedDemos = [])]
  public sealed partial class MouseWheelCustomizationDemo
  {
    private readonly MouseWheelCustomizationSettings settings = new();
    private EventRecognizer mouseWheelZoomEventRecognizer = null!;
    private EventRecognizer defaultHorizontalScrollEventRecognizer = null!;

    protected override void Initialize() {
      // store defaults to easily reset it if needed
      mouseWheelZoomEventRecognizer = graphControl.MouseWheelZoomEventRecognizer;
      defaultHorizontalScrollEventRecognizer = graphControl.HorizontalScrollEventRecognizer;

      // Set custom style defaults that will be used for newly created graph items
      DemoStyles.InitDemoStyles(graphControl.Graph);
      // initialize the graph
      CreateSampleGraph(graphControl.Graph);

      // initialize the input mode
      graphControl.InputMode = new GraphEditorInputMode();
      InitializeUI();

      graphControl.Selection.Add(graphControl.Graph.Nodes.First());

      // center the graph
      graphControl.FitGraphBounds();
    }

    private void UpdateModifiers(ModifierScheme value) {
      switch (value) {
        case ModifierScheme.Default:
          graphControl.MouseWheelZoomEventRecognizer = mouseWheelZoomEventRecognizer;
          graphControl.HorizontalScrollEventRecognizer = defaultHorizontalScrollEventRecognizer;
          break;
        case ModifierScheme.Alternative:
          graphControl.MouseWheelZoomEventRecognizer = EventRecognizers.AltIsDown;
          graphControl.HorizontalScrollEventRecognizer = EventRecognizers.CtrlIsDown;
          break;
      }
    }

    // a wheel event listener that resizes the selected nodes when shift and control both are pressed.
    private void CustomWheelListener(object? sender, PointerEventArgs evt) {
      if (sender is not GraphControl graphControl ||
          graphControl.InputMode is not GraphEditorInputMode graphEditorInputMode) {
        return;
      }

      var selectedNodes = graphControl.Selection.Nodes;
      if ((evt.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) ==
          (ModifierKeys.Shift | ModifierKeys.Control)) {
        // calculate the amount that the nodes need to grow, taking the mouse wheel delta into account
        var delta = evt.WheelDeltaY;
        var growthFactor = 1 + Math.Abs(delta) * 0.2;
        if (delta > 0) {
          // shrink the node
          growthFactor = 1 / growthFactor;
        }

        foreach (var node in selectedNodes.ToArray()) {
          var oldLayout = node.Layout;
          // calculate the new layout
          var newLayout = RectD.FromCenter(oldLayout.GetCenter(), oldLayout.GetSize() * growthFactor);
          graphEditorInputMode.SetNodeLayout(node, newLayout);
        }
        // setting Handled on the event prevents the default mouse wheel behavior,
        // i.e., prevents zooming or scrolling the graph
        evt.Handled = true;
      }
    }

    private void UpdateMouseWheelBehavior(WheelBehavior value) {
      graphControl.Wheel -= CustomWheelListener;
      switch (value) {
        case WheelBehavior.None:
          graphControl.MouseWheelBehavior = MouseWheelBehaviors.None;
          break;
        case WheelBehavior.Zoom:
          graphControl.MouseWheelBehavior = MouseWheelBehaviors.Zoom;
          break;
        case WheelBehavior.Scroll:
          graphControl.MouseWheelBehavior = MouseWheelBehaviors.Scroll;
          break;
        case WheelBehavior.Both:
        case WheelBehavior.Custom:
          graphControl.MouseWheelBehavior = MouseWheelBehaviors.Zoom | MouseWheelBehaviors.Scroll;
          if (value == WheelBehavior.Custom) {
            graphControl.Wheel += CustomWheelListener;
          }
          break;
      }
    }

    /// <summary>
    ///   Creates the initial sample graph.
    /// </summary>
    private static void CreateSampleGraph(IGraph graph) {
      graph.CreateNode(new RectD(100, 0, 50, 50));
      graph.CreateNode(new RectD(0, 0, 50, 50));
      graph.CreateNode(new RectD(200, 0, 50, 50));
    }

    /// <summary>
    ///   Wires up the toolbar UI elements and syncs the graphControl with initial values.
    /// </summary>
    private void InitializeUI() {
      SettingsPanel.DataContext = settings;
      settings.SettingChanged += (_, _) => ApplySettings();

      // Sync graphControl with the initial UI element values
      ApplySettings();
    }

    private void ApplySettings() {
      UpdateModifiers(settings.ModifierScheme);
      UpdateMouseWheelBehavior(settings.WheelBehavior);
      graphControl.MouseWheelScrollFactor = settings.ScrollFactor;
      graphControl.MouseWheelZoomFactor = settings.ZoomFactor;
    }

    [STAThread]
    public static void Main(string[] args) => Run<MouseWheelCustomizationDemo>();

    public MouseWheelCustomizationDemo() {
      InitializeComponent();
    }
  }
}
