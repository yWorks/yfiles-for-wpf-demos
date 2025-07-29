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
using System.Linq;
using yWorks.Layout.Graph;
using yWorks.Layout.Grid;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Labeling;
using yWorks.Utils;

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  ///   An automatic layout algorithm for BPMN diagrams.
  /// </summary>
  /// <remarks>
  ///   <p>
  ///     Some elements have to be marked with the data keys
  ///     <see cref="SequenceFlowEdgesDataKey"/> and <see cref="BoundaryInterruptingEdgesDataKey"/>.
  ///   </p>
  /// </remarks>
  public class BpmnLayout : ILayoutAlgorithm
  {
    /// <summary>
    ///   <see cref="IMapper{K,V}"/> key used to store if an edge represents a sequence flow, default flow
    ///   or conditional flow.
    /// </summary>
    public static readonly EdgeDataKey<bool> SequenceFlowEdgesDataKey =
        new("yWorks.Layout.Bpmn.BpmnLayout.SequenceFlowEdgesDataKey");

    /// <summary>
    ///   <see cref="IMapper{K,V}"/> key used to store if an edge starts at a boundary interrupting event.
    /// </summary>
    public static readonly EdgeDataKey<bool> BoundaryInterruptingEdgesDataKey =
        new("yWorks.Layout.Bpmn.BpmnLayout.BoundaryInterruptingEdgesDataKey");

    public BpmnLayout() {
      Scope = Scope.AllElements;
      LanePadding = 10;
      LayoutOrientation = LayoutOrientation.LeftToRight;
      MinimumNodeDistance = 40;
    }

    /// <summary>
    ///   The Scope that is laid out.
    /// </summary>
    /// <remarks>
    ///   Possible values are <see cref="Bpmn.Scope.AllElements"/> and <see cref="Bpmn.Scope.SelectedElements"/>.
    ///   <p>
    ///     Defaults to <see cref="Bpmn.Scope.AllElements"/>.
    ///   </p>
    ///   <p>
    ///     Note, if the scope is set to <see cref="Bpmn.Scope.SelectedElements"/>,
    ///     non-selected elements may also be moved. However, the layout algorithm uses the initial position of
    ///     such elements as a sketch.
    ///   </p>
    /// </remarks>
    public Scope Scope { get; set; }

    /// <summary>
    ///   The padding used for swimlanes.
    /// </summary>
    /// <remarks>
    ///   The padding for swimlanes, that is the distance between a graph element
    ///   and the border of its enclosing swimlane.
    ///   <p>
    ///     Defaults to <c>10.0</c>.
    ///   </p>
    /// </remarks>
    public double LanePadding { get; set; }

    /// <summary>
    ///   The minimum distance between two node elements.
    /// </summary>
    /// <remarks>
    ///   Defaults to <c>40.0</c>
    /// </remarks>
    public double MinimumNodeDistance { get; set; }

    /// <summary>
    ///   The layout orientation.
    /// </summary>
    /// <remarks>
    ///   Defaults to <see cref="yWorks.Layout.Bpmn.LayoutOrientation.LeftToRight"/>.
    /// </remarks>
    public LayoutOrientation LayoutOrientation { get; set; }

    /// <summary>
    ///   The hierarchical layout used as core layout algorithm.
    /// </summary>
    public HierarchicalLayout HierarchicalLayout { get; } = InitializeHierarchicalLayout();

    /// <summary>Lays out the specified graph.</summary>
    public virtual void ApplyLayout(LayoutGraph graph) {
      if (graph.IsEmpty) {
        return;
      }
      // set the lanePadding to all layout grid columns and rows
      ConfigureLayoutGrid(graph);

      // run core layout
      ApplyHierarchicalLayout(graph);

      // apply generic labeling
      ApplyLabeling(graph);

      // adjust endpoints of edges
      new PortLocationAdjuster().ApplyLayout(graph);

      // remove data provider for CriticalEdgePriorityDataKey that was added by BalancingPortOptimizer
      graph.Context.Remove(HierarchicalLayout.CriticalEdgePriorityDataKey);
    }

    private void ConfigureLayoutGrid(LayoutGraph graph) {
      var cellDescriptors = graph.Context.GetItemData(LayoutGrid.LayoutGridCellDescriptorDataKey);
      if (cellDescriptors == null) {
        return;
      }
      var grid = graph.Nodes.Select(node => cellDescriptors[node]).FirstOrDefault(cell => cell != null)?.LayoutGrid;
      if (grid != null) {
        foreach (var column in grid.Columns) {
          column.LeftPadding += LanePadding;
          column.RightPadding += LanePadding;
        }
        foreach (var row in grid.Rows) {
          row.TopPadding += LanePadding;
          row.BottomPadding += LanePadding;
        }
      }
    }

    private void ApplyHierarchicalLayout(LayoutGraph graph) {
      var hl = HierarchicalLayout;
      hl.EdgeLabelPlacement = EdgeLabelPlacement.Ignore;
      hl.MinimumLayerDistance = MinimumNodeDistance;
      hl.NodeDistance = MinimumNodeDistance;
      hl.LayoutOrientation = LayoutOrientation == LayoutOrientation.LeftToRight
          ? Layout.LayoutOrientation.LeftToRight
          : Layout.LayoutOrientation.TopToBottom;
      hl.Core.PortCandidateSelector = new BalancingPortSelector(new PortCandidateSelector());
      if (Scope == Scope.SelectedElements) {
        hl.Core.FixedElementsLayerAssigner = new FromSketchLayerAssigner { MaximumNodeSize = 5 };
        hl.FromSketchMode = true;
      }
      hl.ApplyLayout(graph);
    }

    private static HierarchicalLayout InitializeHierarchicalLayout() {
      var hl = new HierarchicalLayout();
      hl.GroupLayeringPolicy = GroupLayeringPolicy.IgnoreGroups;
      hl.ComponentLayout.Enabled = false;
      hl.Core.FromScratchLayerAssigner = new BackLoopLayerAssigner();
      hl.CoordinateAssigner.SymmetryOptimizationStrategy = SymmetryOptimizationStrategy.Strong;
      hl.CoordinateAssigner.StraightenEdges = true;
      return hl;
    }

    private void ApplyLabeling(LayoutGraph graph) {
      var labeling = new GenericLabeling {
          StopDuration = TimeSpan.Zero,
          Scope = LabelingScope.All,
          DefaultNodeLabelingCosts = { AmbiguousPlacementCost = 1.0 },
          DefaultEdgeLabelingCosts = { AmbiguousPlacementCost = 1.0 }
      };

      graph.Context.AddItemData(GenericLabeling.NodeLabelCandidateProcessorDataKey,
          _ => BpmnLabelWeightModel.NodeLabelWeightDelegate);
      labeling.ApplyLayout(graph);
      graph.Context.Remove(GenericLabeling.NodeLabelCandidateProcessorDataKey);
    }

    /// <summary>
    ///   Returns if the edge represents a sequence flow, default flow, or conditional flow.
    /// </summary>
    /// <seealso cref="SequenceFlowEdgesDataKey"/>
    public static bool IsSequenceFlow(Edge edge, LayoutGraph graph) {
      var flowDP = graph.Context.GetItemData(SequenceFlowEdgesDataKey);
      return flowDP != null && flowDP[edge];
    }

    /// <summary>
    ///   Returns if the edge is attached to a boundary interrupting event.
    /// </summary>
    /// <seealso cref="BoundaryInterruptingEdgesDataKey"/>
    public static bool IsBoundaryInterrupting(Edge edge, LayoutGraph graph) {
      var isInterruptingDP = graph.Context.GetItemData(BoundaryInterruptingEdgesDataKey);
      return isInterruptingDP != null && isInterruptingDP[edge];
    }
  }
}
