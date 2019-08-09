/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace Tutorial.CustomStyles
{
  /// <summary>
  /// This demo shows how to create and use a relatively simple, non-interactive custom style
  /// for nodes, labels, edges, and ports, as well as a custom arrow.
  /// </summary>
  public partial class SimpleCustomStyleForm
  {

    #region Constructor

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public SimpleCustomStyleForm() {
      InitializeComponent();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="CreateEditorMode"/>
    /// <seealso cref="InitializeGraph"/>
    protected virtual void OnLoaded(object src, RoutedEventArgs e) {
      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      graphControl.InputMode = CreateEditorMode();

      graphControl.FitGraphBounds();
    }

    /// <summary>
    /// Sets a custom NodeStyle instance as a template for newly created
    /// nodes in the graph.
    /// </summary>
    protected void InitializeGraph() {
      IGraph graph = graphControl.Graph;

      // Create a new style and use it as default node style
      graph.NodeDefaults.Style = new MySimpleNodeStyle();
      // Create a new style and use it as default label style
      graph.NodeDefaults.Labels.Style = new MySimpleLabelStyle();
      graph.NodeDefaults.Labels.LayoutParameter = ExteriorLabelModel.North;
      graph.EdgeDefaults.Labels.Style = new MySimpleLabelStyle();

      graph.NodeDefaults.Size = new SizeD(50, 50);

      // Create some graph elements with the above defined styles.
      CreateSampleGraph();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      GraphEditorInputMode mode = new GraphEditorInputMode
                                    {
                                      //We enable label editing
                                      AllowEditLabel = true
                                    };
      return mode;
    }

    #endregion

    #region Graph creation
    /// <summary>
    /// Creates the initial sample graph.
    /// </summary>
    private void CreateSampleGraph() {
      IGraph graph = graphControl.Graph;
      INode node0 = graph.CreateNode(new RectD(180, 40, 30, 30));
      INode node1 = graph.CreateNode(new RectD(260, 50, 30, 30));
      INode node2 = graph.CreateNode(new RectD(284, 200, 30, 30));
      INode node3 = graph.CreateNode(new RectD(350, 40, 30, 30));
      IEdge edge0 = graph.CreateEdge(node1, node2);
      // Add some bends
      graph.AddBend(edge0, new PointD(350, 130));
      graph.AddBend(edge0, new PointD(230, 170));
      graph.CreateEdge(node1, node0);
      graph.CreateEdge(node1, node3);
      ILabel label0 = graph.AddLabel(edge0, "Edge Label");
      ILabel label1 = graph.AddLabel(node1, "Node Label");

    }

    #endregion

  }
}
