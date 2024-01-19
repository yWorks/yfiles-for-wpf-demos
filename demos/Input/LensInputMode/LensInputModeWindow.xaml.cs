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
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.Input.LensInputMode
{
  /// <summary>
  /// Shows how to use a special LensInputMode to magnify the currently hovered over part of the graph.
  /// </summary>
  public partial class LensInputModeWindow
  {
    /// <summary>
    /// The <see cref="LensInputMode"/> displaying the "magnifying glass".
    /// </summary>
    private readonly LensInputMode lensInputMode = new LensInputMode();

    #region Initialization

    public LensInputModeWindow() {
      InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      // Set a nicer node style and create the sample graph
      DemoStyles.InitDemoStyles(GraphControl.Graph);

      GraphEditorInputMode.Add(lensInputMode);

      InitializeGraph(GraphControl.Graph);
    }

    #endregion

    #region Sample Graph Creation

    /// <summary>
    /// Creates the sample graph.
    /// </summary>
    private void InitializeGraph(IGraph graph) {
      INode[] nodes = new INode[16];
      int count = 0;
      for (int i = 1; i < 5; i++) {
        nodes[count++] = graph.CreateNode(new PointD(50 + 40*i, 260));
        nodes[count++] = graph.CreateNode(new PointD(50 + 40*i, 40));
        nodes[count++] = graph.CreateNode(new PointD(40, 50 + 40*i));
        nodes[count++] = graph.CreateNode(new PointD(260, 50 + 40*i));
      }

      for (int i = 0; i < nodes.Length; i++) {
        graph.AddLabel(nodes[i], "" + i);
      }

      graph.CreateEdge(nodes[0], nodes[1]);

      graph.CreateEdge(nodes[5], nodes[4]);

      graph.CreateEdge(nodes[2], nodes[3]);

      graph.CreateEdge(nodes[7], nodes[6]);

      graph.CreateEdge(nodes[2 + 8], nodes[3 + 8]);
      graph.CreateEdge(nodes[7 + 8], nodes[6 + 8]);

      graph.CreateEdge(nodes[0 + 8], nodes[1 + 8]);
      graph.CreateEdge(nodes[5 + 8], nodes[4 + 8]);

      // enable undo...
      graph.SetUndoEngineEnabled(true);

      GraphControl.FitGraphBounds();
    }

    #endregion

    #region Event Handlers

    private void ZoomPresetChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      lensInputMode.ZoomFactor = e.NewValue;
    }

    private void SizePresetChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      lensInputMode.Size = new Size(e.NewValue, e.NewValue);
    }

    private void DisableProjection(object sender, RoutedEventArgs e) {
      GraphControl.Projection = Projections.Identity;
    }

    private void EnableProjection(object sender, RoutedEventArgs e) {
      GraphControl.Projection = Projections.Isometric;
    }

    #endregion
  }
}
