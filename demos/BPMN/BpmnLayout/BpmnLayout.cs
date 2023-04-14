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

using yWorks.Algorithms;
using yWorks.Layout.Grid;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Labeling;

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  /// An automatic layout algorithm for BPMN diagrams.
  /// </summary>
  /// <remarks>
  /// <p>
  /// Some elements have to be marked with the DataProvider keys
  /// <see cref="SequenceFlowEdgesDpKey"/> and <see cref="BoundaryInterruptingEdgesDpKey"/>.
  /// </p>
  /// </remarks>
  public class BpmnLayout : ILayoutAlgorithm
  {
    /// <summary>
    /// <see cref="yWorks.Algorithms.IDataProvider"/> key used to store if an edge represents a sequence flow, default flow or conditional flow.
    /// </summary>
    public static readonly EdgeDpKey<bool> SequenceFlowEdgesDpKey = new EdgeDpKey<bool>(typeof(BpmnLayout), "com.yworks.yfiles.bpmn.layout.BpmnLayout.SequenceFlowEdgesDpKey");

    /// <summary>
    /// <see cref="yWorks.Algorithms.IDataProvider"/> key used to store if an edge starts at a boundary interrupting event.
    /// </summary>
    public static readonly EdgeDpKey<bool> BoundaryInterruptingEdgesDpKey = new EdgeDpKey<bool>(typeof (BpmnLayout), "com.yworks.yfiles.bpmn.layout.BpmnLayout.BoundaryInterruptingEdgesDpKey");

    /// <summary>
    /// <see cref="yWorks.Algorithms.IDataProvider"/> key used to store which labels shall be positioned by the labeling algorithm.
    /// </summary>
    public static readonly ILabelLayoutDpKey<bool> AffectedLabelsDpKey = new ILabelLayoutDpKey<bool>(typeof(BpmnLayout), "com.yworks.yfiles.bpmn.layout.BpmnLayout.AffectedLabelsDpKey");

    public BpmnLayout() {
      Scope = Scope.AllElements;
      LaneInsets = 10;
      LayoutOrientation = LayoutOrientation.LeftToRight;
      MinimumNodeDistance = 40;
    }

    /// <summary>
    /// The Scope that is laid out.
    /// </summary>
    /// <remarks>
    /// Possible values are <see cref="Bpmn.Scope.AllElements"/>
    /// and <see cref="Bpmn.Scope.SelectedElements"/>.
    /// <p>
    /// Defaults to <see cref="Bpmn.Scope.AllElements"/>.
    /// </p>
    /// <p>
    /// Note, if the scope is set to <see cref="Bpmn.Scope.SelectedElements"/>,
    /// non-selected elements may also be moved. However the layout algorithm uses the initial position of
    /// such elements as sketch.
    /// </p>
    /// </remarks>
    public Scope Scope { get; set; }

    /// <summary>
    /// The insets used for swim lanes.
    /// </summary>
    /// <remarks>
    /// The insets for swim lanes, that is the distance between a graph element
    /// and the border of its enclosing swim lane.
    /// <p>
    /// Defaults to <c>10.0</c>.
    /// </p>
    /// </remarks>
    public double LaneInsets { get; set; }

    /// <summary>
    /// The minimum distance between two node elements.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>40.0</c>
    /// </remarks>
    public double MinimumNodeDistance { get; set; }

    /// <summary>
    /// The layout orientation.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="yWorks.Layout.Bpmn.LayoutOrientation.LeftToRight"/>.
    /// </remarks>
    public LayoutOrientation LayoutOrientation { get; set; }


    /// <summary>Lays out the specified graph.</summary>
    public virtual void ApplyLayout(LayoutGraph graph) {
      if (graph.Empty) {
        return;
      }
      // set the laneInsets to all partition grid columns and rows
      ConfigurePartitionGrid(graph);

      // run core layout
      ApplyHierarchicLayout(graph);

      // apply generic labeling
      ApplyLabeling(graph);
      
      // adjust endpoints of edges
      new PortLocationAdjuster().ApplyLayout(graph);

      //remove data provider for CriticalEdgePriorityDpKey that was added by BalancingPortOptimizer
      graph.RemoveDataProvider(HierarchicLayout.CriticalEdgePriorityDpKey);
    }

    private void ConfigurePartitionGrid(LayoutGraph graph) {
      PartitionGrid grid = PartitionGrid.GetPartitionGrid(graph);
      if (grid != null) {
        foreach (var column in grid.Columns) {
          column.LeftInset = column.LeftInset + LaneInsets;
          column.RightInset = column.RightInset + LaneInsets;
        }
        foreach (var row in grid.Rows) {
          row.TopInset = row.TopInset + LaneInsets;
          row.BottomInset = row.BottomInset + LaneInsets;
        }
      }
    }

    private void ApplyHierarchicLayout(LayoutGraph graph) {
      HierarchicLayout hl = new HierarchicLayout();
      hl.OrthogonalRouting = true;
      hl.RecursiveGroupLayering = false;
      hl.ComponentLayoutEnabled = false;
      hl.FromScratchLayerer = new BackLoopLayerer();
      hl.MinimumLayerDistance = MinimumNodeDistance;
      hl.NodeToNodeDistance = MinimumNodeDistance;
      ((SimplexNodePlacer)hl.NodePlacer).BarycenterMode = true;
      ((SimplexNodePlacer)hl.NodePlacer).StraightenEdges = true;
      hl.LayoutOrientation = LayoutOrientation == LayoutOrientation.LeftToRight
        ? Layout.LayoutOrientation.LeftToRight
        : Layout.LayoutOrientation.TopToBottom;
      hl.HierarchicLayoutCore.PortConstraintOptimizer = new BalancingPortOptimizer(new PortCandidateOptimizer());
      if (Scope == Scope.SelectedElements) {
        hl.FixedElementsLayerer = new AsIsLayerer {MaximumNodeSize = 5};
        hl.LayoutMode = LayoutMode.Incremental;
      }
      hl.ApplyLayout(graph);
    }

    private void ApplyLabeling(LayoutGraph graph) {
      GenericLabeling labeling = new GenericLabeling();
      labeling.MaximumDuration = 0;
      labeling.ReduceAmbiguity = true;
      labeling.PlaceNodeLabels = true;
      labeling.PlaceEdgeLabels = true;
      labeling.AffectedLabelsDpKey = AffectedLabelsDpKey;
      labeling.ProfitModel = new BpmnLabelProfitModel(graph);
      labeling.CustomProfitModelRatio = 0.15;
      labeling.ApplyLayout(graph);
    }

    /// <summary>
    /// Returns if the edge represents a sequence flow, default flow or conditional flow.
    /// </summary>
    /// <seealso cref="SequenceFlowEdgesDpKey"/>
    public static bool IsSequenceFlow(Edge edge, Graph graph) {
      var flowDP = graph.GetDataProvider(SequenceFlowEdgesDpKey);
      return flowDP != null && flowDP.GetBool(edge);
    }

    /// <summary>
    /// Returns if the edge is attached to a boundary interrupting event.
    /// </summary>
    /// <seealso cref="BoundaryInterruptingEdgesDpKey"/>.
    public static bool IsBoundaryInterrupting(Edge edge, LayoutGraph graph) {
      var isInterruptingDP = graph.GetDataProvider(BoundaryInterruptingEdgesDpKey);
      return isInterruptingDP != null && isInterruptingDP.GetBool(edge);
    }
  }
}
