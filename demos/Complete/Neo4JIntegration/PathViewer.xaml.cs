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
using System.Collections.Generic;
using Neo4j.Driver;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using INeo4jNode = Neo4j.Driver.INode;

namespace Neo4JIntegration
{
  /// <summary>
  ///   Interaction logic for PathViewer.xaml
  /// </summary>
  public partial class PathViewer
  {
    private readonly List<INeo4jNode> nodes;
    private readonly List<IRelationship> edges;

    private async void OnLoaded(object source, EventArgs args) {
      var (graphBuilder, _, _) =
          Neo4JIntegrationDemo.CreateGraphBuilder(graphControl, graphControl.Graph, nodes, edges);

      // Build the graph
      graphBuilder.BuildGraph();

      graphControl.Center = PointD.Origin;

      // Layout the graph
      await graphControl.ApplyLayoutAnimated(
          new HierarchicalLayout {
              EdgeLabelPlacement = EdgeLabelPlacement.Integrated,
              DefaultEdgeDescriptor = {
                  RoutingStyleDescriptor = new RoutingStyleDescriptor(RoutingStyle.Polyline)
              }
          }, TimeSpan.FromSeconds(0.5));
    }

    public PathViewer(List<INeo4jNode> nodes, List<IRelationship> edges) {
      InitializeComponent();
      this.nodes = nodes;
      this.edges = edges;
    }
  }
}
