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
using System.Collections.Generic;
using yWorks.Demo.AbortHandler;
using yWorks.Demo.ArcDiagram;
using yWorks.Demo.ArrangeObjects;
using yWorks.Demo.Arrow;
using yWorks.Demo.ArrowEdgeStyle;
using yWorks.Demo.ArrowNodeStyle;
using yWorks.Demo.BackgroundImage;
using yWorks.Demo.BezierEdgeStyle;
using yWorks.Demo.Biofabrics;
using yWorks.Demo.BorderAlignedLabelModel;
using yWorks.Demo.BoundaryLabeling;
using yWorks.Demo.Bridges;
using yWorks.Demo.BuildingGraphFromData;
using yWorks.Demo.BuildingSwimlanesFromData;
using yWorks.Demo.BundledEdgeRouter;
using yWorks.Demo.BusRouting;
using yWorks.Demo.CircleSnapping;
using yWorks.Demo.CircularSubstructures;
using yWorks.Demo.ClearMarqueeArea;
using yWorks.Demo.ClearRectangleArea;
using yWorks.Demo.ClickableStyleDecorator;
using yWorks.Demo.Clipboard;
using yWorks.Demo.ClipboardDeferredCut;
using yWorks.Demo.Collapse;
using yWorks.Demo.CompactDiskGroups;
using yWorks.Demo.CompactTabularLayout;
using yWorks.Demo.ComplexHighlightDecorator;
using yWorks.Demo.ComponentDragAndDrop;
using yWorks.Demo.CompositeNodeStyle;
using yWorks.Demo.ContextMenu;
using yWorks.Demo.ControlStyles;
using yWorks.Demo.CriticalPaths;
using yWorks.Demo.CustomCopyAndPaste;
using yWorks.Demo.CustomDragAndDrop;
using yWorks.Demo.CustomEdgeCreation;
using yWorks.Demo.CustomGraphML;
using yWorks.Demo.CustomHandleProvider;
using yWorks.Demo.CustomLayoutStage;
using yWorks.Demo.CustomPortModel;
using yWorks.Demo.CustomSnapping;
using yWorks.Demo.DataBinding.AdjacencyGraphBuilder;
using yWorks.Demo.DataBinding.Dynamic;
using yWorks.Demo.DataBinding.GraphBuilder;
using yWorks.Demo.DataBinding.PortAwareAdjacencyGraphBuilder;
using yWorks.Demo.DataBinding.PortAwareGraphBuilder;
using yWorks.Demo.DataBinding.PortAwareTreeBuilder;
using yWorks.Demo.DeepZoom;
using yWorks.Demo.DirectedEdgeLabel;
using yWorks.Demo.DragAndDrop;
using yWorks.Demo.DragFromComponent;
using yWorks.Demo.EdgeBundling;
using yWorks.Demo.EdgeGrouping;
using yWorks.Demo.EdgeLabelPlacement;
using yWorks.Demo.EdgeReconnection;
using yWorks.Demo.EdgeRouter;
using yWorks.Demo.EdgeRouterBuses;
using yWorks.Demo.EdgeRouterBusesCustom;
using yWorks.Demo.EdgeRouterGrouping;
using yWorks.Demo.EdgeRouterIncremental;
using yWorks.Demo.EdgeToEdge;
using yWorks.Demo.EditablePathStyle;
using yWorks.Demo.Events;
using yWorks.Demo.ExternalLinks;
using yWorks.Demo.FillAreaLayout;
using yWorks.Demo.Filtering;
using yWorks.Demo.FilteringWithFolding;
using yWorks.Demo.FlowFiltering;
using yWorks.Demo.Folding;
using yWorks.Demo.FoldingWithLayout;
using yWorks.Demo.FoldingWithMergedEdges;
using yWorks.Demo.GeneralPathNodeStyle;
using yWorks.Demo.GraphAnalysis;
using yWorks.Demo.GraphCopy;
using yWorks.Demo.GraphDecorator;
using yWorks.Demo.GraphDragAndDrop;
using yWorks.Demo.GraphMLCompat;
using yWorks.Demo.GraphSearch;
using yWorks.Demo.GraphSource;
using yWorks.Demo.GraphViewer;
using yWorks.Demo.GridSnapping;
using yWorks.Demo.GroupNodeStyle;
using yWorks.Demo.HeatMap;
using yWorks.Demo.HeightProfile;
using yWorks.Demo.Hierarchical;
using yWorks.Demo.HierarchicalCompactGroups;
using yWorks.Demo.HierarchicalConstraints;
using yWorks.Demo.HierarchicalEdgeGrouping;
using yWorks.Demo.HierarchicalEdgeLabeling;
using yWorks.Demo.HierarchicalExactCoordinates;
using yWorks.Demo.HierarchicalGivenLayering;
using yWorks.Demo.HierarchicalGridComponents;
using yWorks.Demo.HierarchicalIncremental;
using yWorks.Demo.HierarchicalLayerConstraints;
using yWorks.Demo.HierarchicalNesting;
using yWorks.Demo.HierarchicalNestingIncremental;
using yWorks.Demo.HierarchicalNodeAlignment;
using yWorks.Demo.HierarchicalNodePortCandidates;
using yWorks.Demo.HierarchicalSequenceConstraints;
using yWorks.Demo.HierarchicalSubcomponents;
using yWorks.Demo.ImageExport;
using yWorks.Demo.InteractiveAggregation;
using yWorks.Demo.InteractiveEdgeRouting;
using yWorks.Demo.InteractiveGraphRestructuring;
using yWorks.Demo.InteractiveHierarchical;
using yWorks.Demo.InteractiveOrganic;
using yWorks.Demo.InvariantLabelStyle;
using yWorks.Demo.LabelEditing;
using yWorks.Demo.LabelHandleProvider;
using yWorks.Demo.LabelStyle;
using yWorks.Demo.LabelTextWrapping;
using yWorks.Demo.LargeGraphs;
using yWorks.Demo.LassoSelection;
using yWorks.Demo.LayerConstraints;
using yWorks.Demo.LayoutStyles;
using yWorks.Demo.LayoutWithoutView;
using yWorks.Demo.Legend;
using yWorks.Demo.LevelOfDetailStyle;
using yWorks.Demo.ListNode;
using yWorks.Demo.MagnifyingGlass;
using yWorks.Demo.MarqueeNodeCreation;
using yWorks.Demo.MazeRouting;
using yWorks.Demo.MetabolicPathways;
using yWorks.Demo.MouseWheelCustomization;
using yWorks.Demo.MoveUnselectedNodes;
using yWorks.Demo.MultiPage;
using yWorks.Demo.NodeAlignment;
using yWorks.Demo.NodeLabeling;
using yWorks.Demo.NodeOverlapAvoiding;
using yWorks.Demo.NodeSelectionResizing;
using yWorks.Demo.NodeTypes;
using yWorks.Demo.OctilinearEdges;
using yWorks.Demo.OffsetWrapperLabelModel;
using yWorks.Demo.Organic;
using yWorks.Demo.OrganicConstraints;
using yWorks.Demo.OrganicEdgeLabeling;
using yWorks.Demo.OrganicIncremental;
using yWorks.Demo.OrganicSubstructures;
using yWorks.Demo.OrganicWithSubstructures;
using yWorks.Demo.OrgChart;
using yWorks.Demo.Orthogonal;
using yWorks.Demo.OrthogonalEdgeEditingCustomization;
using yWorks.Demo.OrthogonalEdges;
using yWorks.Demo.Overview;
using yWorks.Demo.PartialLayout;
using yWorks.Demo.PortAlignment;
using yWorks.Demo.PortCandidateProvider;
using yWorks.Demo.PositionHandler;
using yWorks.Demo.Printing;
using yWorks.Demo.RadialGroupLayout;
using yWorks.Demo.RectangleNodeStyle;
using yWorks.Demo.RectangularIndicator;
using yWorks.Demo.RecursiveGroupLayout;
using yWorks.Demo.RenderingOrder;
using yWorks.Demo.ReparentHandler;
using yWorks.Demo.ReshapeHandleProvider;
using yWorks.Demo.ReshapeHandleProviderConfiguration;
using yWorks.Demo.RestrictedEditing;
using yWorks.Demo.RotatableNodes;
using yWorks.Demo.SankeyLayout;
using yWorks.Demo.SelectionStyling;
using yWorks.Demo.SequenceConstraints;
using yWorks.Demo.ShapeNodeStyle;
using yWorks.Demo.ShapePortStyle;
using yWorks.Demo.SimpleAdjacencyGraphBuilder;
using yWorks.Demo.SimpleAdjacencyGraphBuilderWithIds;
using yWorks.Demo.SimpleArrowStyle;
using yWorks.Demo.SimpleDragAndDrop;
using yWorks.Demo.SimpleGraphBuilder;
using yWorks.Demo.SimpleGraphBuilderImplicitGroups;
using yWorks.Demo.SimpleGraphEditor;
using yWorks.Demo.SimpleGraphML;
using yWorks.Demo.SimpleHighlightDecorator;
using yWorks.Demo.SimpleLayoutGrid;
using yWorks.Demo.SimpleTreeBuilderArray;
using yWorks.Demo.SimpleTreeBuilderJson;
using yWorks.Demo.SingleSelection;
using yWorks.Demo.SizeConstraintProvider;
using yWorks.Demo.SmartClickNavigation;
using yWorks.Demo.Snapping;
using yWorks.Demo.SplitEdges;
using yWorks.Demo.StyleDecorators;
using yWorks.Demo.SubdivideEdges;
using yWorks.Demo.TableEditor;
using yWorks.Demo.TabularGroups;
using yWorks.Demo.ThemeVariants;
using yWorks.Demo.Theming;
using yWorks.Demo.Tooltips;
using yWorks.Demo.TreeLayout;
using yWorks.Demo.TreeMap;
using yWorks.Demo.TreeNodePlacers;
using yWorks.Demo.Tutorial.BasicFeatures;
using yWorks.Demo.Tutorial.GraphBuilder;
using yWorks.Demo.Tutorial.StyleImplementationEdge;
using yWorks.Demo.Tutorial.StyleImplementationLabel;
using yWorks.Demo.Tutorial.StyleImplementationNode;
using yWorks.Demo.Undo;
using yWorks.Demo.ValidBeginCursors;
using yWorks.Demo.ViewportLimiter;
using yWorks.Demo.XamlPopup;
using yWorks.Demo.ZOrder;

namespace DemoBrowser
{
  internal static class DemoRegistry
  {
    public static IReadOnlyList<Type> DemoTypes { get; } = [
        typeof(LayoutStylesDemo),
        typeof(OrgChartDemo),
        typeof(BiofabricsDemo),
        typeof(SankeyLayoutDemo),
        typeof(TreeMapDemo),
        typeof(GraphAnalysisDemo),
        typeof(HierarchicalNestingDemo),
        typeof(HierarchicalNestingIncrementalDemo),
        typeof(FoldingWithLayoutDemo),
        typeof(LargeGraphsDemo),
        typeof(InteractiveAggregationDemo),
        typeof(CollapseDemo),
        typeof(MagnifyingGlassDemo),
        typeof(LabelStyleDemo),
        typeof(RectangleNodeStyleDemo),
        typeof(ShapeNodeStyleDemo),
        typeof(ShapePortStyleDemo),
        typeof(GroupNodeStyleDemo),
        typeof(ArrowDemo),
        typeof(ArrowNodeStyleDemo),
        typeof(ArrowEdgeStyleDemo),
        typeof(SimpleGraphEditorDemo),
        typeof(ValidBeginCursorsDemo),
        typeof(TableEditorDemo),
        typeof(GraphViewerDemo),
        typeof(XamlPopupDemo),
        typeof(LegendDemo),
        typeof(RenderingOrderDemo),
        typeof(ZOrderDemo),
        typeof(RotatableNodesDemo),
        typeof(DeepZoomDemo),
        typeof(ArrangeObjectsDemo),
        typeof(ControlStylesDemo),
        typeof(ThemeVariantsDemo),
        typeof(EditablePathStyleDemo),
        typeof(ClickableStyleDecoratorDemo),
        typeof(CompositeNodeStyleDemo),
        typeof(LevelOfDetailStyleDemo),
        typeof(ListNodeDemo),
        typeof(BezierEdgeStyleDemo),
        typeof(DirectedEdgeLabelDemo),
        typeof(InvariantLabelStyleDemo),
        typeof(SimpleArrowStyleDemo),
        typeof(SelectionStylingDemo),
        typeof(StyleDecoratorsDemo),
        typeof(GeneralPathNodeStyleDemo),
        typeof(EdgeBundlingDemo),
        typeof(ArcDiagramDemo),
        typeof(MazeRoutingDemo),
        typeof(ComponentDragAndDropDemo),
        typeof(EdgeLabelPlacementDemo),
        typeof(NodeLabelingDemo),
        typeof(NodeTypesDemo),
        typeof(InteractiveHierarchicalLayoutDemo),
        typeof(HierarchicalExactCoordinatesDemo),
        typeof(InteractiveEdgeRoutingDemo),
        typeof(EdgeGroupingDemo),
        typeof(EdgeRouterGroupingDemo),
        typeof(BoundaryLabelingDemo),
        typeof(HeightProfileDemo),
        typeof(MetabolicPathwaysDemo),
        typeof(OrganicSubstructuresDemo),
        typeof(CircularSubstructuresDemo),
        typeof(BusRoutingDemo),
        typeof(FillAreaLayoutDemo),
        typeof(ClearMarqueeAreaDemo),
        typeof(ClearRectangleAreaDemo),
        typeof(NodeOverlapAvoidingDemo),
        typeof(NodeAlignmentDemo),
        typeof(HierarchicalGridComponentsDemo),
        typeof(HierarchicalSubcomponentsDemo),
        typeof(TabularGroupsDemo),
        typeof(CriticalPathsDemo),
        typeof(CustomLayoutStageDemo),
        typeof(SplitEdgesDemo),
        typeof(SimpleLayoutGridDemo),
        typeof(InteractiveGraphRestructuringDemo),
        typeof(LayerConstraintsDemo),
        typeof(SequenceConstraintsDemo),
        typeof(InteractiveOrganicDemo),
        typeof(MultiPageDemo),
        typeof(PartialLayoutDemo),
        typeof(BridgesDemo),
        typeof(CustomEdgeCreationDemo),
        typeof(EdgeToEdgeDemo),
        typeof(GraphBuilderDemo),
        typeof(AdjacencyGraphBuilderDemo),
        typeof(SimpleGraphBuilderDemo),
        typeof(SimpleAdjacencyGraphBuilderDemo),
        typeof(SimpleAdjacencyGraphBuilderWithIdsDemo),
        typeof(SimpleGraphBuilderImplicitGroupsDemo),
        typeof(SimpleTreeBuilderArrayDemo),
        typeof(SimpleTreeBuilderJsonDemo),
        typeof(PortAwareGraphBuilderDemo),
        typeof(PortAwareAdjacencyGraphBuilderDemo),
        typeof(PortAwareTreeBuilderDemo),
        typeof(GraphSourceDemo),
        typeof(DynamicDemo),
        typeof(ImageExportDemo),
        typeof(PrintingDemo),
        typeof(GraphEventsDemo),
        typeof(ClipboardDemo),
        typeof(DeferredCutClipboardDemo),
        typeof(GraphUndoDemo),
        typeof(NodeSelectionResizingDemo),
        typeof(BorderAlignedLabelModelDemo),
        typeof(OffsetWrapperLabelModelDemo),
        typeof(SimpleGraphMLDemo),
        typeof(GraphMLCompatibilityDemo),
        typeof(CustomPortModelDemo),
        typeof(CustomSnappingDemo),
        typeof(DragFromComponentDemo),
        typeof(ContextMenuDemo),
        typeof(SimpleDragAndDropDemo),
        typeof(DragAndDropDemo),
        typeof(GraphDragAndDropDemo),
        typeof(CustomDragAndDropDemo),
        typeof(EdgeReconnectionDemo),
        typeof(LabelEditingDemo),
        typeof(CustomHandleProviderDemo),
        typeof(LabelHandleProviderDemo),
        typeof(MoveUnselectedNodesDemo),
        typeof(OrthogonalEdgeEditingCustomizationDemo),
        typeof(PortCandidateProviderDemo),
        typeof(PositionHandlerDemo),
        typeof(ReparentHandlerDemo),
        typeof(ReshapeHandleProviderConfigurationDemo),
        typeof(ReshapeHandleProviderDemo),
        typeof(RestrictedEditingDemo),
        typeof(LassoSelectionDemo),
        typeof(MarqueeNodeCreationDemo),
        typeof(MouseWheelCustomizationDemo),
        typeof(SingleSelectionDemo),
        typeof(SizeConstraintProviderDemo),
        typeof(CustomCopyAndPasteDemo),
        typeof(CircleSnappingDemo),
        typeof(LayoutWithoutViewDemo),
        typeof(BackgroundImageDemo),
        typeof(BuildingGraphFromDataDemo),
        typeof(BuildingSwimlanesFromDataDemo),
        typeof(ExternalLinksDemo),
        typeof(FilteringDemo),
        typeof(FilteringWithFoldingDemo),
        typeof(FoldingDemo),
        typeof(FoldingWithMergedEdgesDemo),
        typeof(GraphCopyDemo),
        typeof(GraphDecoratorDemo),
        typeof(SimpleHighlightDecoratorDemo),
        typeof(ComplexHighlightDecoratorDemo),
        typeof(GraphSearchDemo),
        typeof(GridSnappingDemo),
        typeof(HeatMapDemo),
        typeof(CustomGraphMLDemo),
        typeof(LabelTextWrappingDemo),
        typeof(OctilinearEdgesDemo),
        typeof(OrthogonalEdgesDemo),
        typeof(RectangularIndicatorDemo),
        typeof(SmartClickNavigationDemo),
        typeof(SnappingDemo),
        typeof(SubdivideEdgesDemo),
        typeof(ThemingDemo),
        typeof(TooltipsDemo),
        typeof(OverviewControlDemo),
        typeof(ViewportLimiterDemo),
        typeof(FlowFilteringDemo),
        typeof(HierarchicalDemo),
        typeof(HierarchicalIncrementalDemo),
        typeof(HierarchicalNodePortCandidatesDemo),
        typeof(HierarchicalEdgeGroupingDemo),
        typeof(HierarchicalGivenLayeringDemo),
        typeof(HierarchicalConstraintsDemo),
        typeof(HierarchicalSequenceConstraintsDemo),
        typeof(HierarchicalLayerConstraintsDemo),
        typeof(HierarchicalNodeAlignmentDemo),
        typeof(HierarchicalEdgeLabelingDemo),
        typeof(HierarchicalCompactGroupsDemo),
        typeof(OrganicDemo),
        typeof(OrganicIncrementalDemo),
        typeof(OrganicEdgeLabelingDemo),
        typeof(OrganicWithSubstructuresDemo),
        typeof(OrganicConstraintsDemo),
        typeof(EdgeRouterDemo),
        typeof(EdgeRouterIncrementalDemo),
        typeof(EdgeRouterBusesDemo),
        typeof(EdgeRouterBusesCustomDemo),
        typeof(TreeLayoutDemo),
        typeof(TreeNodePlacersDemo),
        typeof(OrthogonalDemo),
        typeof(RecursiveGroupLayoutDemo),
        typeof(RadialGroupLayoutDemo),
        typeof(CompactDiskGroupsDemo),
        typeof(CompactTabularLayoutDemo),
        typeof(BundledEdgeRouterDemo),
        typeof(PortAlignmentDemo),
        typeof(AbortHandlerDemo),
        typeof(BasicFeaturesTutorial),
        typeof(GraphBuilderTutorial),
        typeof(CustomNodeStyleTutorial),
        typeof(CustomLabelStyleTutorial),
        typeof(CustomEdgeStyleTutorial)
    ];
  }
}
