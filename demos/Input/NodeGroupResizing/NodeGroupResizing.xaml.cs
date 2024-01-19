/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Input.NodeGroupResizing
{
  /// <summary>
  /// Shows how to reshape a group of nodes as one unit.
  /// </summary>
  /// <remarks>
  /// A custom <see cref="IInputMode"/> implementation is used to create handles for the unit and custom
  /// <see cref="IReshapeHandler"/> implementations include the logic to either
  /// <see cref="NodeGroupResizing.ResizeMode.Resize">move and resize</see> or just
  /// <see cref="NodeGroupResizing.ResizeMode.Scale">move</see> the nodes.
  /// </remarks>
  public partial class NodeGroupResizingWindow
  {
    public NodeGroupResizingWindow() {
      InitializeComponent();
    }

    private NodeGroupResizingInputMode nodeGroupResizingInputMode;

    private void OnLoaded(object sender, RoutedEventArgs e) {
      // enable undo engine
      graphControl.Graph.SetUndoEngineEnabled(true);

      // prepare orthogonal edge editing
      graphEditorInputMode.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext { Enabled = false };

      // allow grouping operations
      graphEditorInputMode.AllowGroupingOperations = true;

      // prepare snapping
      graphEditorInputMode.SnapContext = new GraphSnapContext { Enabled = false };

      // set minimum and maximum sizes for all non-group nodes (group nodes should be able to grow larger so they can
      // contain arbitrary numbers of nodes)
      var sizeConstraintProvider = new NodeSizeConstraintProvider(new SizeD(10, 10), new SizeD(100, 100));
      graphControl.Graph
                  .GetDecorator()
                  .NodeDecorator
                  .SizeConstraintProviderDecorator
                  .SetFactory(node => !graphControl.Graph.IsGroupNode(node), node => sizeConstraintProvider);

      // add a custom input mode to the GraphEditorInputMode that shows a single set of handles when multiple nodes are selected
      nodeGroupResizingInputMode = new NodeGroupResizingInputMode {
          Margins = new InsetsD(10), 
          Mode = NodeGroupResizing.ResizeMode.Resize
      };
      graphEditorInputMode.Add(nodeGroupResizingInputMode);

      // set style defaults
      DemoStyles.InitDemoStyles(graphControl.Graph);

      // load sample graph
      graphControl.ImportFromGraphML("Resources/sampleGraph.graphml");
      graphControl.FitContent();
      graphControl.Graph.GetUndoEngine().Clear();
    }

    private void SnappingButtonClick(object sender, RoutedEventArgs e) {
      graphEditorInputMode.SnapContext.Enabled = _snappingButton.IsChecked == true;
    }

    private void OrthogonalEditingButtonClick(object sender, RoutedEventArgs e) {
      graphEditorInputMode.OrthogonalEdgeEditingContext.Enabled = _orthogonalEditingButton.IsChecked == true;
    }

    private void ResizeModeSelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (nodeGroupResizingInputMode != null) {
        nodeGroupResizingInputMode.Mode = _resizeMode.SelectedIndex == 0
            ? NodeGroupResizing.ResizeMode.Resize
            : NodeGroupResizing.ResizeMode.Scale;
      }
    }
  }
}
