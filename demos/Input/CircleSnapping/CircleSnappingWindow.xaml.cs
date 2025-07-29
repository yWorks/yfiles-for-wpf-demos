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

using System;
using System.IO;
using Demo.yFiles.Graph.Input.CircleSnapping.Snapping;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace Demo.yFiles.Graph.Input.CircleSnapping
{
  /// <summary>
  ///   Demo code that shows how to add circle snapping to edit a tree-like layout.
  /// </summary>
  public partial class CircleSnappingWindow
  {
    private GraphControl GraphControl => graphControl;

    public CircleSnappingWindow() {
      InitializeComponent();
    }

    private void OnLoaded(object source, EventArgs args) {
      // configures default styles for newly created graph elements
      InitializeGraph();

      // initialize input mode for graph control
      GraphControl.InputMode = new GraphEditorInputMode();

      // enable circular snapping for nodes
      InitializeCircularSnapping();

      // Initialize the graph by reading it from a file.
      ReadGraph("Resources\\CircleSnapping.graphml");
    }

    /// <summary>
    ///   Configures snapping in a way that is suited to circular/radial arrangements of child nodes around parent nodes.
    /// </summary>
    private void InitializeCircularSnapping() {
      var inputMode = ((GraphEditorInputMode) GraphControl.InputMode)!;
      inputMode.SnapContext = new GraphSnapContext {
          SnapDistance = 20,
          SnappableItems = SnappableItems.Node,
          ConsiderNodeLabelAllNodeLabelsDistanceSnapping = false,
          CollectNodePairCenterSnapLines = false,
          CollectNodePairSegmentSnapLines = false,
          ConsiderNodeLabelOwnerDistanceSnapping = false,
          ConsiderNodeLabelOwnerLabelsDistanceSnapping = false,
          CollectNodePairSnapLines = false,
          ConsiderEdgeLabelOwnerDistanceSnapping = false,
          ConsiderEdgeLabelOwnerLabelsDistanceSnapping = false,
          CollectNodeSizes = false,
          CollectEdgeSnapReferences = false,
          CollectPortSnapReferences = false,
          ConsiderEdgeLabelOwnerPathSnapping = false,
          ConsiderEdgeLabelAllEdgeLabelsDistanceSnapping = false,
          ConsiderInitialLabelLocationSnapping = false,
          ConsiderPortLabelOwnerLocationSnapping = false
      };

      var nodeDecorator = graphControl.Graph.GetDecorator().Nodes;
      nodeDecorator.SnapReferenceProvider.AddFactory(node => new SnapCircleProvider(node));
      nodeDecorator.SnapResultProvider.AddConstant(new CircleSnapResultProvider());
    }

    /// <summary>
    ///   Reads the graph from a file with the given file name.
    /// </summary>
    protected virtual void ReadGraph(string filename) {
      var ioh = new GraphMLIOHandler();
      ioh.Read(GraphControl.Graph, new StreamReader(filename));

      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Configures the graph item style defaults for this demo.
    /// </summary>
    private void InitializeGraph() {
      var graph = GraphControl.Graph;
      DemoStyles.InitDemoStyles(graph);
      graph.NodeDefaults.Style = DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse);
    }
  }
}
