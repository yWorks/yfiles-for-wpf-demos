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

using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.CustomLabelModel
{
  public partial class CustomLabelModelWindow
  {
    public CustomLabelModelWindow() {
      InitializeComponent();
    }

    private void OnLoaded(object sender, EventArgs e) {
      // initialize the graph
      InitializeGraph();
      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      return new GraphEditorInputMode();
    }

    /// <summary>
    /// Sets a custom node label model parameter instance for newly created
    /// node labels in the graph, creates an example node with a label using
    /// the default parameter and a nother node with a label without restrictions
    /// on the number of possible placements.
    /// </summary>
    protected void InitializeGraph() {
      DemoStyles.InitDemoStyles(Graph);

      Graph.NodeDefaults.Labels.LayoutParameter = new MyNodeLabelModel().CreateDefaultParameter();

      Graph.AddLabel(Graph.CreateNode(new RectD(90, 90, 100, 100)), "Click and Drag", new MyNodeLabelModel { CandidateCount = 0, Offset = 20 }.CreateDefaultParameter());
      Graph.AddLabel(Graph.CreateNode(new RectD(250, 90, 100, 100)),
                     "Click and Drag To Snap");

    }


    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    public GraphControl GraphControl {
      get { return graphControl; }
    }

    /// <summary>
    /// Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    public IGraph Graph {
      get { return GraphControl.Graph; }
    }

    private void OnExitClicked(object sender, RoutedEventArgs e) {
      Application.Current.Shutdown();
    }
  }
}
