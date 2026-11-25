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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Utils;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   Abstract base class for configurations that can be displayed in an option editor.
  /// </summary>
  /// <remarks>
  ///   Subclasses at least have to implement the method <see cref="CreateConfiguredLayout"/> so
  ///   the <see cref="Apply"/> method can be called to run the returned layout algorithm and apply the
  ///   layout result to the graph in the passed <see cref="GraphControl"/>.
  /// </remarks>
  public abstract class LayoutConfiguration
  {
    /// <summary>
    ///   A guard to prevent running multiple layout calculations at the same time.
    /// </summary>
    private bool layoutRunning;

    /// <summary>
    ///   Applies this configuration to the given <see cref="GraphControl"/>.
    /// </summary>
    /// <remarks>
    ///   This is the main method of this class. Typically, it calls <see cref="CreateConfiguredLayout"/> to create and
    ///   configure a layout and <see cref="CreateConfiguredLayoutData"/> to get a suitable
    ///   <see cref="LayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/> instance
    ///   for the layout.
    /// </remarks>
    /// <param name="graphControl">The <c>GraphControl</c> to apply the configuration on.</param>
    /// <param name="doneHandler">
    ///   A callback that is called after the configuration is applied. Can be <see langword="null"/>
    /// </param>
    /// <returns>The <see cref="Task"/> of this call.</returns>
    public virtual async Task Apply(GraphControl graphControl, Action doneHandler) {
      if (layoutRunning) {
        await graphControl.Dispatcher.BeginInvoke(DispatcherPriority.Background, doneHandler);
        return;
      }

      var layout = CreateConfiguredLayout(graphControl);
      if (layout == null) {
        await graphControl.Dispatcher.BeginInvoke(DispatcherPriority.Background, doneHandler);
        return;
      }

      var layoutData = CreateConfiguredLayoutData(graphControl, layout);

      // configure the LayoutExecutor
      var layoutExecutor = new LayoutExecutor(graphControl, new MinimumNodeSizeStage(layout)) {
          AnimationDuration = TimeSpan.FromSeconds(0.75),
          PortAdjustmentPolicies = { Constant = PortAdjustmentPolicy.Lengthen }
      };

      // set the cancel duration for the layout computation to 20s
      layoutExecutor.LayoutAbortController.CancelDuration = TimeSpan.FromSeconds(100);

      // set the layout data to the LayoutExecutor
      if (layoutData != null) {
        layoutExecutor.LayoutData = layoutData;
      }

      // start the LayoutExecutor
      try {
        await layoutExecutor.Start();
        layoutRunning = false;
      } catch (AlgorithmAbortedException) {
        MessageBox.Show(
            "The layout computation has been canceled because the maximum configured runtime of 20 seconds has been exceeded.");
      } catch (Exception e) {
        MessageBox.Show("Running the layout has encountered an error: " + e.Message);
      } finally {
        PostProcess(graphControl);
        doneHandler();
      }
    }

    /// <summary>
    ///   Creates and configures a layout algorithm.
    /// </summary>
    /// <param name="graphControl">The <see cref="GraphControl"/> to apply the configuration on.</param>
    /// <returns>The configured layout.</returns>
    protected abstract ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl);

    /// <summary>
    ///   Called by <see cref="Apply"/> to create the layout data of the configuration.
    /// </summary>
    /// <remarks>
    ///   This method is typically overridden to provide item-specific configuration of a layout algorithm.
    /// </remarks>
    /// <param name="graphControl">The <see cref="GraphControl"/> to apply the configuration on.</param>
    /// <param name="layout">The layout algorithm to run.</param>
    /// <returns>
    ///   A layout-specific <see cref="LayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/> instance or
    ///   <see langword="null"/>.
    /// </returns>
    protected virtual LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) =>
        null;

    /// <summary>
    ///   Called by <see cref="Apply"/> after the layout animation is done.
    /// </summary>
    /// <param name="graphControl">The <see cref="GraphControl"/> for the post-processing.</param>
    protected virtual void PostProcess(GraphControl graphControl) { }

    /// <summary>
    ///   Creates a layout data for <see cref="SubgraphLayoutStage"/> that is based on the
    ///   <see cref="GraphControl.Selection"/> model of the given graph control.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     If a node, edge, or label is <see cref="IObservableCollection{T}.Contains">selected</see> in the given
    ///     graph control's model, then it will be part of the subgraph items in the respective property
    ///     of <see cref="SubgraphLayoutStageData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/>.
    ///   </para>
    /// </remarks>
    /// <param name="graphControl">The graph control to create the data for</param>
    /// <returns>
    ///   A layout data instance for <see cref="SubgraphLayoutStage"/> where items that are actually
    ///   selected in the given graph control are part of the subgraph
    /// </returns>
    public SubgraphLayoutStageData<INode, IEdge, ILabel, ILabel> CreateSubgraphLayoutData(GraphControl graphControl) {
      return new SubgraphLayoutStageData<INode, IEdge, ILabel, ILabel> {
          SubgraphNodes = { Source = graphControl.Selection.Nodes },
          SubgraphEdges = { Source = graphControl.Selection.Edges },
          SubgraphNodeLabels = { Source = graphControl.Selection.Labels.Where(l => l.Owner is INode) },
          SubgraphEdgeLabels = { Source = graphControl.Selection.Labels.Where(l => l.Owner is IEdge) }
      };
    }

    /// <summary>
    ///   Adds a mapper with an <see cref="EdgeLabelPreferredPlacement"/> that matches the given settings to the mapper
    ///   registry of the given graph.
    /// </summary>
    /// <remarks>
    ///   In addition, sets the label model of all node and edge labels to free since that model
    ///   can realize any label placement calculated by a layout algorithm.
    /// </remarks>
    /// <param name="graph">The graph whose label shall be laid out.</param>
    /// <param name="placeAlongEdge">The edge label placement along the owning edge.</param>
    /// <param name="sideOfEdge">The side of the edge the label shall be placed.</param>
    /// <param name="orientation">The orientation of the edge label.</param>
    /// <param name="distanceToEdge">The distance of the edge label to the owning edge.</param>
    /// <returns>
    ///   A layout data instance for labeling considering the given configuration.
    /// </returns>
    public LayoutData<INode, IEdge, ILabel, ILabel> CreateLabelingLayoutData(
        IGraph graph,
        EnumLabelPlacementAlongEdge placeAlongEdge,
        EnumLabelPlacementSideOfEdge sideOfEdge,
        EnumLabelPlacementOrientation orientation,
        double distanceToEdge) {
      var preferredPlacement =
          CreatePreferredPlacementDescriptor(placeAlongEdge, sideOfEdge, orientation, distanceToEdge);

      // change to a free edge label model to support integrated edge labeling
      var freeEdgeLabelModel = new FreeEdgeLabelModel();
      foreach (var label in graph.GetEdgeLabels()) {
        if (!(label.LayoutParameter.Model is FreeEdgeLabelModel)) {
          graph.SetLabelLayoutParameter(label, freeEdgeLabelModel.FindBestParameter(label, label.GetLayout()));
        }
      }

      // change to a free node label model to support generic node labeling
      var freeNodeLabelModel = new FreeNodeLabelModel();
      foreach (var label in graph.GetNodeLabels()) {
        if (!(label.LayoutParameter.Model is FreeNodeLabelModel)) {
          graph.SetLabelLayoutParameter(label, freeNodeLabelModel.FindBestParameter(label, label.GetLayout()));
        }
      }

      var layoutData = new GenericLayoutData<INode, IEdge, ILabel, ILabel>();
      layoutData.AddItemMapping(EdgeLabelPreferredPlacement.EdgeLabelPreferredPlacementDataKey).Constant =
          preferredPlacement;

      return layoutData;
    }

    /// <summary>
    ///   Creates a new <see cref="EdgeLabelPreferredPlacement"/> that matches the given settings.
    /// </summary>
    /// <param name="placeAlongEdge">The edge label placement along the owning edge.</param>
    /// <param name="sideOfEdge">The side of the edge the label shall be placed.</param>
    /// <param name="orientation">The orientation of the edge label.</param>
    /// <param name="distanceToEdge">The distance of the edge label to the owning edge.</param>
    /// <returns>
    ///   A new <see cref="EdgeLabelPreferredPlacement"/> that matches the given settings.
    /// </returns>
    public EdgeLabelPreferredPlacement CreatePreferredPlacementDescriptor(
        EnumLabelPlacementAlongEdge placeAlongEdge,
        EnumLabelPlacementSideOfEdge sideOfEdge,
        EnumLabelPlacementOrientation orientation,
        double distanceToEdge) {
      var preferredPlacement = new EdgeLabelPreferredPlacement();

      switch (sideOfEdge) {
        case EnumLabelPlacementSideOfEdge.Anywhere:
          preferredPlacement.EdgeSide = LabelEdgeSides.Anywhere;
          break;
        case EnumLabelPlacementSideOfEdge.OnEdge:
          preferredPlacement.EdgeSide = LabelEdgeSides.OnEdge;
          break;
        case EnumLabelPlacementSideOfEdge.Left:
          preferredPlacement.EdgeSide = LabelEdgeSides.LeftOfEdge;
          break;
        case EnumLabelPlacementSideOfEdge.Right:
          preferredPlacement.EdgeSide = LabelEdgeSides.RightOfEdge;
          break;
        case EnumLabelPlacementSideOfEdge.LeftOrRight:
          preferredPlacement.EdgeSide = LabelEdgeSides.LeftOfEdge | LabelEdgeSides.RightOfEdge;
          break;
      }

      switch (placeAlongEdge) {
        case EnumLabelPlacementAlongEdge.Anywhere:
          preferredPlacement.PlacementAlongEdge = LabelAlongEdgePlacements.Anywhere;
          break;
        case EnumLabelPlacementAlongEdge.AtSourcePort:
          preferredPlacement.PlacementAlongEdge = LabelAlongEdgePlacements.AtSourcePort;
          break;
        case EnumLabelPlacementAlongEdge.AtTargetPort:
          preferredPlacement.PlacementAlongEdge = LabelAlongEdgePlacements.AtTargetPort;
          break;
        case EnumLabelPlacementAlongEdge.AtSource:
          preferredPlacement.PlacementAlongEdge = LabelAlongEdgePlacements.AtSource;
          break;
        case EnumLabelPlacementAlongEdge.AtTarget:
          preferredPlacement.PlacementAlongEdge = LabelAlongEdgePlacements.AtTarget;
          break;
        case EnumLabelPlacementAlongEdge.Centered:
          preferredPlacement.PlacementAlongEdge = LabelAlongEdgePlacements.AtCenter;
          break;
      }

      switch (orientation) {
        case EnumLabelPlacementOrientation.Parallel:
          preferredPlacement.Angle = 0.0d;
          preferredPlacement.AngleReference = LabelAngleReferences.RelativeToEdgeFlow;
          break;
        case EnumLabelPlacementOrientation.Orthogonal:
          preferredPlacement.Angle = Math.PI / 2;
          preferredPlacement.AngleReference = LabelAngleReferences.RelativeToEdgeFlow;
          break;
        case EnumLabelPlacementOrientation.Horizontal:
          preferredPlacement.Angle = 0.0d;
          preferredPlacement.AngleReference = LabelAngleReferences.Absolute;
          break;
        case EnumLabelPlacementOrientation.Vertical:
          preferredPlacement.Angle = Math.PI / 2;
          preferredPlacement.AngleReference = LabelAngleReferences.Absolute;
          break;
      }

      preferredPlacement.DistanceToEdge = distanceToEdge;
      return preferredPlacement;
    }

    /// <summary>
    ///   Specifies constants for the preferred placement along an edge used by layout configurations.
    /// </summary>
    public enum EnumLabelPlacementAlongEdge
    {
      Anywhere,
      AtSourcePort,
      AtTargetPort,
      AtSource,
      AtTarget,
      Centered
    }

    /// <summary>
    ///   Specifies constants for the preferred placement at a side of an edge used by layout configurations.
    /// </summary>
    public enum EnumLabelPlacementSideOfEdge
    {
      Anywhere,
      OnEdge,
      Left,
      Right,
      LeftOrRight
    }

    /// <summary>
    ///   Specifies constants for the orientation of an edge label used by layout configurations.
    /// </summary>
    public enum EnumLabelPlacementOrientation
    {
      Parallel,
      Orthogonal,
      Horizontal,
      Vertical
    }
  }
}
