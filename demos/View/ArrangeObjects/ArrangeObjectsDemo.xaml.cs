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
using yWorks.Controls.Input;
using yWorks.Graph;

namespace yWorks.Demo.ArrangeObjects
{
  /// <summary>
  ///   Interaction logic for ArrangeObjectsDemo.xaml
  /// </summary>
  [Demo("arrange-objects", "view", "Arrange Objects",
      Summary = "Shows simple operations for aligning and distributing nodes.", Tags = ["viewer"],
      Keywords = ["arrange", "align nodes", "distribute nodes"])]
  public sealed partial class ArrangeObjectsDemo
  {
    protected override void Initialize() {
      // create the demo's graph component
      var graph = graphControl.Graph;
      // enable interactive editing
      graphControl.InputMode = new GraphEditorInputMode();

      // configure default styles for the demo's graph
      DemoStyles.InitDemoStyles(graph, Themes.Palette31);
      // create the demo's sample graph
      BuildGraphFromJson("graph-data.json", graph);

      // center the demo's graph in the demo's visible area
      graphControl.FitGraphBounds();

      // enable undo and redo
      graph.SetUndoEngineEnabled(true);

      // bind the demo's new node alignment and node distribution operations to the demo's UI controls
      InitializeUI();
    }

    /// <summary>
    ///   Binds actions to the demo's UI controls.
    /// </summary>
    private void InitializeUI() {
      // bind the demo's new node alignment operations to toolbar controls
      alignBottomButton.Click += (_, _) => AlignmentUtils.AlignBottom(Graph, graphControl.Selection.Nodes);
      alignHorizontallyButton.Click += (_, _) => AlignmentUtils.AlignHorizontally(Graph, graphControl.Selection.Nodes);
      alignLeftButton.Click += (_, _) => AlignmentUtils.AlignLeft(Graph, graphControl.Selection.Nodes);
      alignRightButton.Click += (_, _) => AlignmentUtils.AlignRight(Graph, graphControl.Selection.Nodes);
      alignTopButton.Click += (_, _) => AlignmentUtils.AlignTop(Graph, graphControl.Selection.Nodes);
      alignVerticallyButton.Click += (_, _) => AlignmentUtils.AlignVertically(Graph, graphControl.Selection.Nodes);

      // bind the demo's new node distribution operations to toolbar controls
      distributeHorizontallyButton.Click +=
          (_, _) => AlignmentUtils.DistributeHorizontally(Graph, graphControl.Selection.Nodes);
      distributeVerticallyButton.Click +=
          (_, _) => AlignmentUtils.DistributeVertically(Graph, graphControl.Selection.Nodes);
    }

    private IGraph Graph => graphControl.Graph;

    [STAThread]
    private static void Main(string[] args) {
      Run<ArrangeObjectsDemo>();
    }

    public ArrangeObjectsDemo() {
      InitializeComponent();
    }
  }
}
