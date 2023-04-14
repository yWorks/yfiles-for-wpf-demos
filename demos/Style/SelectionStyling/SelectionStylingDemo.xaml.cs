/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using Demo.yFiles.Toolkit;

[assembly :
  XmlnsDefinition("http://www.yworks.com/yFilesWPF/demos/SelectionStyling/1.0", "Demo.yFiles.Graph.SelectionStyling")]
[assembly: XmlnsPrefix("http://www.yworks.com/yFilesWPF/demos/SelectionStyling/1.0", "demo")]

namespace Demo.yFiles.Graph.SelectionStyling
{
  /// <summary>
  /// Demonstrates customized selecting painting of nodes, edges and labels by decorating these items with a corresponding style.
  /// </summary>
  public partial class SelectionStylingDemo
  {
    private IContextLookupChainLink nodeDecorationLookupChainLink;
    private IContextLookupChainLink edgeDecorationLookupChainLink;
    private IContextLookupChainLink labelDecorationLookupChainLink;
    private NodeStyleDecorationInstaller nodeDecorationInstaller;
    private EdgeStyleDecorationInstaller edgeDecorationInstaller;
    private LabelStyleDecorationInstaller labelDecorationInstaller;
    private StyleDecorationZoomPolicy[] styleDecorationZoomPolicies;

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public SelectionStylingDemo() {
      InitializeComponent();
    }

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    protected virtual void OnLoaded(object src, RoutedEventArgs e) {
      // initialize default graph styles
      var graph = graphControl.Graph;
      DemoStyles.InitDemoStyles(graph);

      // initialize the graph
      graphControl.ImportFromGraphML("Resources\\SelectionStyling.graphml");

      // initialize the input mode
      // disable label editing, since this would be really confusing in combination with our style.
      var graphEditorInputMode = new GraphEditorInputMode
                                   {
                                     AllowEditLabel = true,
                                     SnapContext = new GraphSnapContext()
                                   };
      graphControl.InputMode = graphEditorInputMode;

      InitializeDecoration();
      UpdateDecoration();

      SelectAllNodes(graphEditorInputMode);
      SelectAllLabels(graphEditorInputMode);
    }

    private void InitializeDecoration() {
      nodeDecorationInstaller = new NodeStyleDecorationInstaller
      {
        NodeStyle = new ShapeNodeStyle {Shape = ShapeNodeShape.Rectangle, Pen = Pens.DeepSkyBlue, Brush = Brushes.Transparent},
        Margins = new InsetsD(10.0)
      };
      edgeDecorationInstaller = new EdgeStyleDecorationInstaller
      {
        EdgeStyle = new PolylineEdgeStyle {Pen = new Pen(Brushes.DeepSkyBlue, 3.0)}
      };
      labelDecorationInstaller = new LabelStyleDecorationInstaller
      {
        LabelStyle = new NodeStyleLabelStyleAdapter(
          new ShapeNodeStyle { Shape = ShapeNodeShape.RoundRectangle, Pen = Pens.DeepSkyBlue, Brush = Brushes.Transparent },
          VoidLabelStyle.Instance),
        Margins = new InsetsD(5.0)
      };

      styleDecorationZoomPolicies = new[]
                              {
                                StyleDecorationZoomPolicy.Mixed,
                                StyleDecorationZoomPolicy.WorldCoordinates,
                                StyleDecorationZoomPolicy.ViewCoordinates
                              };
      ZoomModeComboBox.ItemsSource = styleDecorationZoomPolicies;
      ZoomModeComboBox.SelectedIndex = 0;
    }

    /// <summary>
    /// Sets, removes and updates the custom selection decoration for nodes,
    /// edges, and labels according to the current settings.
    /// </summary>
    public void UpdateDecoration() {
      var selectedZoomMode = (StyleDecorationZoomPolicy) ZoomModeComboBox.SelectedItem;
      nodeDecorationInstaller.ZoomPolicy = selectedZoomMode;
      edgeDecorationInstaller.ZoomPolicy = selectedZoomMode;
      labelDecorationInstaller.ZoomPolicy = selectedZoomMode;

      var graphDecorator = graphControl.Graph.GetDecorator();
      var nodeDecorator = graphDecorator.NodeDecorator;
      var edgeDecorator = graphDecorator.EdgeDecorator;
      var labelDecorator = graphDecorator.LabelDecorator;

      if (!IsChecked(CustomNodeDecoratorButton) && nodeDecorationLookupChainLink != null) {
        nodeDecorator.Remove(nodeDecorationLookupChainLink);
        nodeDecorationLookupChainLink = null;
      } else if (IsChecked(CustomNodeDecoratorButton) && nodeDecorationLookupChainLink == null) {
        nodeDecorationLookupChainLink = nodeDecorator.SelectionDecorator.SetFactory(node => nodeDecorationInstaller);
      }
      if (!IsChecked(CustomEdgeDecoratorButton) && edgeDecorationLookupChainLink != null) {
        edgeDecorator.Remove(edgeDecorationLookupChainLink);
        edgeDecorationLookupChainLink = null;
      } else if (IsChecked(CustomEdgeDecoratorButton) && edgeDecorationLookupChainLink == null) {
        edgeDecorationLookupChainLink = edgeDecorator.SelectionDecorator.SetFactory(edge => edgeDecorationInstaller);
      }
      if (!IsChecked(CustomLabelDecoratorButton) && labelDecorationLookupChainLink != null) {
        labelDecorator.Remove(labelDecorationLookupChainLink);
        labelDecorationLookupChainLink = null;
      } else if (IsChecked(CustomLabelDecoratorButton) && labelDecorationLookupChainLink == null) {
        labelDecorationLookupChainLink = labelDecorator.SelectionDecorator.SetFactory(label => labelDecorationInstaller);
      }
    }

    private static bool IsChecked(ToggleButton button) {
      return button.IsChecked != null && (bool) button.IsChecked;
    }

    private void SelectAllNodes(GraphEditorInputMode inputMode) {
      foreach (var node in graphControl.Graph.Nodes) {
        inputMode.SetSelected(node, true);
      }
    }

    private void SelectAllEdges(GraphEditorInputMode inputMode) {
      foreach (var edge in graphControl.Graph.Edges) {
        inputMode.SetSelected(edge, true);
      }
    }

    private void SelectAllLabels(GraphEditorInputMode inputMode) {
      foreach (var label in graphControl.Graph.Labels) {
        inputMode.SetSelected(label, true);
      }
    }

    #region UI event handler

    private void CustomNodeDecorationChanged(object sender, RoutedEventArgs e) {
      UpdateDecoration();
      var inputMode = ((GraphEditorInputMode) graphControl.InputMode);
      inputMode.ClearSelection();
      SelectAllNodes(inputMode);
    }

    private void CustomEdgeDecorationChanged(object sender, RoutedEventArgs e) {
      UpdateDecoration();
      var inputMode = ((GraphEditorInputMode) graphControl.InputMode);
      inputMode.ClearSelection();
      SelectAllEdges(inputMode);
    }

    private void CustomLabelDecorationChanged(object sender, RoutedEventArgs e) {
      UpdateDecoration();
      var inputMode = ((GraphEditorInputMode) graphControl.InputMode);
      inputMode.ClearSelection();
      SelectAllLabels(inputMode);
    }

    private void ZoomModeChanged(object sender, SelectionChangedEventArgs e) {
      UpdateDecoration();
      graphControl.Invalidate();
    }

    #endregion
  }
}
