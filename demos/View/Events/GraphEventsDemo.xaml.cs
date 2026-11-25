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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.Events
{
  /// <summary>
  ///   This demo shows how to register to the various events provided by the <see cref="IGraph">graph</see>,
  ///   the <see cref="GraphControl"/> and the input modes.
  /// </summary>
  [Demo("events-viewer", "view", "Events Viewer",
      Summary = "Shows the multitude of events provided by the classes IGraph, GraphComponent, and the Input Modes.",
      Tags = ["interaction"], Keywords = ["palette", "drag and drop", "dnd"])]
  public partial class GraphEventsDemo
  {
    private GraphEditorInputMode editorMode;
    private FoldingManager manager;
    private GraphViewerInputMode viewerMode;
    private GraphMLSupport graphMLSupport;

    [STAThread]
    private static void Main() => Run<GraphEventsDemo>();

    public GraphEventsDemo() {
      InitializeComponent();
    }

    public void OnLoaded(object source, EventArgs args) {
      InitializePalette();
      EnableFolding();
      InitializeGraph();
      InitializeInputModes();
      InitializeGraphML();
      SetupToolTips();
      SetupContextMenu();
      EnableUndo();

      // load sample graph
      var resource = LoadAssemblyResource("sample.graphml");
      graphMLSupport.ImportFromGraphML(resource);

      logInputModeEvents.IsChecked = true;
      toggleEditingButton.IsChecked = true;
      toggleSingleClickReportingPolicyButton.IsChecked = false;

      eventLog.ItemsSource = messages;
    }

    #region Initialization

    private void EnableFolding() {
      var graph = graphControl.Graph;

      manager = new FoldingManager(graph);
      graphControl.Graph = manager.CreateFoldingView().Graph;
    }

    private void InitializeGraph() {
      var graph = graphControl.Graph;
      DemoStyles.InitDemoStyles(graph, foldingEnabled: true, orthogonalEdgeEditing: true);
    }

    private void InitializeInputModes() {
      editorMode = new GraphEditorInputMode {
          OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext {
              Enabled = false,
              MovePorts = true
          }
      };
      editorMode.ItemHoverInputMode.HoverItems = GraphItemTypes.All;
      editorMode.NodeDropInputMode.Enabled = true;
      editorMode.LabelDropInputMode.Enabled = true;
      editorMode.LabelDropInputMode.UseLocationForParameter = true;
      editorMode.ClickInputMode.SwallowFocusClick = true;

      viewerMode = new GraphViewerInputMode();
      viewerMode.ItemHoverInputMode.HoverItems = GraphItemTypes.All;

      graphControl.InputMode = editorMode;
    }

    /// <summary>
    ///   Initializes the GraphML support and registers command bindings for Open/Save.
    /// </summary>
    private void InitializeGraphML() {
      graphMLSupport = new GraphMLSupport(graphControl);
      graphMLSupport.RegisterOpenSaveCommandBindings();
    }

    private void InitializePalette() {
      var defaultsProvider = new DefaultGraph();
      DemoStyles.InitDemoStyles(defaultsProvider);

      var nodeTemplate = new SimpleNode();
      nodeTemplate.Layout = new RectD(PointD.Origin, defaultsProvider.NodeDefaults.Size);
      nodeTemplate.Style = defaultsProvider.NodeDefaults.Style;

      palette.Items.Add(nodeTemplate);

      var labelTemplate = new SimpleLabel();
      var labelTemplateContainer = new SimpleNode();
      labelTemplateContainer.Layout = new RectD(PointD.Origin, defaultsProvider.NodeDefaults.Size);
      labelTemplateContainer.Style = VoidNodeStyle.Instance;
      labelTemplateContainer.Tag = "Label Container";
      labelTemplateContainer.Labels = new List<ILabel> { labelTemplate };
      labelTemplate.Owner = labelTemplateContainer;
      labelTemplate.Text = "Label";
      labelTemplate.LayoutParameter = InteriorNodeLabelModel.Center;
      labelTemplate.Style = defaultsProvider.NodeDefaults.Labels.Style;
      labelTemplate.PreferredSize = labelTemplate.Style.Renderer.GetPreferredSize(labelTemplate, labelTemplate.Style);

      palette.Items.Add(labelTemplateContainer);
    }

    private void SetupToolTips() {
      editorMode.ToolTipItems = GraphItemTypes.All;
      editorMode.QueryItemToolTip += QueryItemToolTipHandler;

      viewerMode.ToolTipItems = GraphItemTypes.All;
      viewerMode.QueryItemToolTip += QueryItemToolTipHandler;
    }

    private void QueryItemToolTipHandler(object sender, QueryItemToolTipEventArgs<IModelItem> args) {
      args.ToolTip = "ToolTip for " + GraphItemTypesSupport.GetItemType(args.Item);
      args.Handled = true;
    }

    private void SetupContextMenu() {
      editorMode.ContextMenuItems = GraphItemTypes.All;
      editorMode.PopulateItemContextMenu += PopulateContextMenuHandler;

      viewerMode.ContextMenuItems = GraphItemTypes.All;
      viewerMode.PopulateItemContextMenu += PopulateContextMenuHandler;
    }

    private void PopulateContextMenuHandler(object sender, PopulateItemContextMenuEventArgs<IModelItem> args) {
      var type = GraphItemTypesSupport.GetItemType(args.Item);
      args.Menu.Items.Add(new MenuItem { Header = "Menu Item for " + type });
      args.ShowMenu = true;
      args.Handled = true;
    }

    private void EnableUndo() {
      manager.MasterGraph.SetUndoEngineEnabled(true);
    }

    #endregion

    #region Event Registration

    private void RegisterGraphControlKeyEvents(object sender, RoutedEventArgs args) {
      graphControl.CompoundKeyDown += ControlOnCompoundKeyDown;
      graphControl.CompoundKeyUp += ControlOnCompoundKeyUp;
      graphControl.CompoundKeyTextInput += ControlOnCompoundKeyTextInput;
    }

    private void DeregisterGraphControlKeyEvents(object sender, RoutedEventArgs args) {
      graphControl.CompoundKeyDown -= ControlOnCompoundKeyDown;
      graphControl.CompoundKeyUp -= ControlOnCompoundKeyUp;
      graphControl.CompoundKeyTextInput -= ControlOnCompoundKeyTextInput;
    }

    private void RegisterClipboardAndClipboardCopierEvents(object sender, RoutedEventArgs args) {
      RegisterClipboardEvents(sender, args);
      RegisterClipboardCopierEvents(sender, args);
    }

    private void DeregisterClipboardAndClipboardCopierEvents(object sender, RoutedEventArgs args) {
      DeregisterClipboardEvents(sender, args);
      DeregisterClipboardCopierEvents(sender, args);
    }

    private void RegisterClipboardCopierEvents(object sender, RoutedEventArgs args) {
      graphControl.Clipboard.ToClipboardCopier.GraphCopied += ClipboardOnGraphCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.NodeCopied += ClipboardOnNodeCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.EdgeCopied += ClipboardOnEdgeCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.PortCopied += ClipboardOnPortCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.LabelCopied += ClipboardOnLabelCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.ObjectCopied += ClipboardOnObjectCopiedToClipboard;

      graphControl.Clipboard.FromClipboardCopier.GraphCopied += ClipboardOnGraphCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.NodeCopied += ClipboardOnNodeCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.EdgeCopied += ClipboardOnEdgeCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.PortCopied += ClipboardOnPortCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.LabelCopied += ClipboardOnLabelCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.ObjectCopied += ClipboardOnObjectCopiedFromClipboard;

      graphControl.Clipboard.DuplicateCopier.GraphCopied += ClipboardOnGraphDuplicated;
      graphControl.Clipboard.DuplicateCopier.NodeCopied += ClipboardOnNodeDuplicated;
      graphControl.Clipboard.DuplicateCopier.EdgeCopied += ClipboardOnEdgeDuplicated;
      graphControl.Clipboard.DuplicateCopier.PortCopied += ClipboardOnPortDuplicated;
      graphControl.Clipboard.DuplicateCopier.LabelCopied += ClipboardOnLabelDuplicated;
      graphControl.Clipboard.DuplicateCopier.ObjectCopied += ClipboardOnObjectDuplicated;
    }

    private void DeregisterClipboardCopierEvents(object sender, RoutedEventArgs args) {
      graphControl.Clipboard.ToClipboardCopier.GraphCopied -= ClipboardOnGraphCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.NodeCopied -= ClipboardOnNodeCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.EdgeCopied -= ClipboardOnEdgeCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.PortCopied -= ClipboardOnPortCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.LabelCopied -= ClipboardOnLabelCopiedToClipboard;
      graphControl.Clipboard.ToClipboardCopier.ObjectCopied -= ClipboardOnObjectCopiedToClipboard;

      graphControl.Clipboard.FromClipboardCopier.GraphCopied -= ClipboardOnGraphCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.NodeCopied -= ClipboardOnNodeCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.EdgeCopied -= ClipboardOnEdgeCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.PortCopied -= ClipboardOnPortCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.LabelCopied -= ClipboardOnLabelCopiedFromClipboard;
      graphControl.Clipboard.FromClipboardCopier.ObjectCopied -= ClipboardOnObjectCopiedFromClipboard;

      graphControl.Clipboard.DuplicateCopier.GraphCopied -= ClipboardOnGraphDuplicated;
      graphControl.Clipboard.DuplicateCopier.NodeCopied -= ClipboardOnNodeDuplicated;
      graphControl.Clipboard.DuplicateCopier.EdgeCopied -= ClipboardOnEdgeDuplicated;
      graphControl.Clipboard.DuplicateCopier.PortCopied -= ClipboardOnPortDuplicated;
      graphControl.Clipboard.DuplicateCopier.LabelCopied -= ClipboardOnLabelDuplicated;
      graphControl.Clipboard.DuplicateCopier.ObjectCopied -= ClipboardOnObjectDuplicated;
    }

    private void RegisterClipboardEvents(object sender, RoutedEventArgs args) {
      graphControl.Clipboard.ItemsCutting += ClipboardOnItemsCutting;
      graphControl.Clipboard.ItemsCut += ClipboardOnItemsCut;
      graphControl.Clipboard.ItemsCopying += ClipboardOnItemsCopying;
      graphControl.Clipboard.ItemsCopied += ClipboardOnItemsCopied;
      graphControl.Clipboard.ItemsPasting += ClipboardOnItemsPasting;
      graphControl.Clipboard.ItemsPasted += ClipboardOnItemsPasted;
      graphControl.Clipboard.ItemsDuplicating += ClipboardOnItemsDuplicating;
      graphControl.Clipboard.ItemsDuplicated += ClipboardOnItemsDuplicated;
    }

    private void DeregisterClipboardEvents(object sender, RoutedEventArgs args) {
      graphControl.Clipboard.ItemsCut -= ClipboardOnItemsCut;
      graphControl.Clipboard.ItemsCopied -= ClipboardOnItemsCopied;
      graphControl.Clipboard.ItemsPasted -= ClipboardOnItemsPasted;
      graphControl.Clipboard.ItemsDuplicated -= ClipboardOnItemsDuplicated;
    }

    private void RegisterUndoEvents(object sender, RoutedEventArgs args) {
      var undoEngine = graphControl.Graph.GetUndoEngine()!;
      undoEngine.PropertyChanged += UndoEngineOnPropertyChanged;
      undoEngine.UnitUndone += UndoEngineOnUnitUndone;
      undoEngine.UnitRedone += UndoEngineOnUnitRedone;
    }

    private void DeregisterUndoEvents(object sender, RoutedEventArgs args) {
      var undoEngine = graphControl.Graph.GetUndoEngine()!;
      undoEngine.PropertyChanged -= UndoEngineOnPropertyChanged;
      undoEngine.UnitUndone -= UndoEngineOnUnitUndone;
      undoEngine.UnitRedone -= UndoEngineOnUnitRedone;
    }

    private void RegisterGraphControlPointerEvents(object sender, RoutedEventArgs args) {
      graphControl.PointerClick += ControlOnPointerClick;
      graphControl.PointerEnter += ControlOnPointerEntered;
      graphControl.PointerLeave += ControlOnPointerLeave;
      graphControl.LostPointerCapture += ControlOnLostPointerCapture;
      graphControl.PointerDown += ControlOnPointerDown;
      graphControl.PointerUp += ControlOnPointerUp;
      graphControl.Wheel += ControlOnWheel;
      graphControl.PointerDrag += ControlOnPointerDrag;
      graphControl.PointerMove += ControlOnPointerMoved;
      graphControl.PointerLongPress += ControlOnPointerLongPress;
      graphControl.PointerLongRest += ControlOnPointerLongRest;
    }

    private void DeregisterGraphControlPointerEvents(object sender, RoutedEventArgs args) {
      graphControl.PointerClick -= ControlOnPointerClick;
      graphControl.PointerEnter -= ControlOnPointerEntered;
      graphControl.PointerLeave -= ControlOnPointerLeave;
      graphControl.LostPointerCapture -= ControlOnLostPointerCapture;
      graphControl.PointerDown -= ControlOnPointerDown;
      graphControl.PointerUp -= ControlOnPointerUp;
      graphControl.Wheel -= ControlOnWheel;
      graphControl.PointerDrag -= ControlOnPointerDrag;
      graphControl.PointerMove -= ControlOnPointerMoved;
      graphControl.PointerLongPress -= ControlOnPointerLongPress;
      graphControl.PointerLongRest -= ControlOnPointerLongRest;
    }

    private void RegisterGraphControlRenderEvents(object sender, RoutedEventArgs args) {
      graphControl.PrepareRenderContext += ControlOnPrepareRenderContext;
      graphControl.UpdatedVisual += ControlOnUpdatedVisual;
      graphControl.UpdatingVisual += ControlOnUpdatingVisual;
    }

    private void DeregisterGraphControlRenderEvents(object sender, RoutedEventArgs args) {
      graphControl.PrepareRenderContext -= ControlOnPrepareRenderContext;
      graphControl.UpdatedVisual -= ControlOnUpdatedVisual;
      graphControl.UpdatingVisual -= ControlOnUpdatingVisual;
    }

    private void RegisterGraphControlViewportEvents(object sender, RoutedEventArgs args) {
      graphControl.ViewportChanged += ControlOnViewportChanged;
      graphControl.ZoomChanged += ControlOnZoomChanged;
    }

    private void DeregisterGraphControlViewportEvents(object sender, RoutedEventArgs args) {
      graphControl.ViewportChanged -= ControlOnViewportChanged;
      graphControl.ZoomChanged -= ControlOnZoomChanged;
    }

    private void RegisterNodeEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.NodeLayoutChanged += OnNodeLayoutChanged;
      graphControl.Graph.NodeStyleChanged += OnNodeStyleChanged;
      graphControl.Graph.NodeTagChanged += OnNodeTagChanged;
      graphControl.Graph.NodeCreated += OnNodeCreated;
      graphControl.Graph.NodeRemoved += OnNodeRemoved;
    }

    private void DeregisterNodeEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.NodeLayoutChanged -= OnNodeLayoutChanged;
      graphControl.Graph.NodeStyleChanged -= OnNodeStyleChanged;
      graphControl.Graph.NodeTagChanged -= OnNodeTagChanged;
      graphControl.Graph.NodeCreated -= OnNodeCreated;
      graphControl.Graph.NodeRemoved -= OnNodeRemoved;
    }

    private void RegisterEdgeEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.EdgeStyleChanged += OnEdgeStyleChanged;
      graphControl.Graph.EdgePortsChanged += OnEdgePortsChanged;
      graphControl.Graph.EdgeTagChanged += OnEdgeTagChanged;
      graphControl.Graph.EdgeCreated += OnEdgeCreated;
      graphControl.Graph.EdgeRemoved += OnEdgeRemoved;
    }

    private void DeregisterEdgeEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.EdgeStyleChanged -= OnEdgeStyleChanged;
      graphControl.Graph.EdgePortsChanged -= OnEdgePortsChanged;
      graphControl.Graph.EdgeTagChanged -= OnEdgeTagChanged;
      graphControl.Graph.EdgeCreated -= OnEdgeCreated;
      graphControl.Graph.EdgeRemoved -= OnEdgeRemoved;
    }

    private void RegisterBendEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.BendAdded += OnBendAdded;
      graphControl.Graph.BendLocationChanged += OnBendLocationChanged;
      graphControl.Graph.BendTagChanged += OnBendTagChanged;
      graphControl.Graph.BendRemoved += OnBendRemoved;
    }

    private void DeregisterBendEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.BendAdded -= OnBendAdded;
      graphControl.Graph.BendLocationChanged -= OnBendLocationChanged;
      graphControl.Graph.BendTagChanged -= OnBendTagChanged;
      graphControl.Graph.BendRemoved -= OnBendRemoved;
    }

    private void RegisterPortEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.PortAdded += OnPortAdded;
      graphControl.Graph.PortLocationParameterChanged += OnPortLocationParameterChanged;
      graphControl.Graph.PortStyleChanged += OnPortStyleChanged;
      graphControl.Graph.PortTagChanged += OnPortTagChanged;
      graphControl.Graph.PortRemoved += OnPortRemoved;
    }

    private void DeregisterPortEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.PortAdded -= OnPortAdded;
      graphControl.Graph.PortLocationParameterChanged -= OnPortLocationParameterChanged;
      graphControl.Graph.PortStyleChanged -= OnPortStyleChanged;
      graphControl.Graph.PortTagChanged -= OnPortTagChanged;
      graphControl.Graph.PortRemoved -= OnPortRemoved;
    }

    private void RegisterLabelEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.LabelAdded += OnLabelAdded;
      graphControl.Graph.LabelRemoved += OnLabelRemoved;
      graphControl.Graph.LabelLayoutParameterChanged += OnLabelLayoutParameterChanged;
      graphControl.Graph.LabelStyleChanged += OnLabelStyleChanged;
      graphControl.Graph.LabelPreferredSizeChanged += OnLabelPreferredSizeChanged;
      graphControl.Graph.LabelTagChanged += OnLabelTagChanged;
      graphControl.Graph.LabelTextChanged += OnLabelTextChanged;
    }

    private void DeregisterLabelEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.LabelAdded -= OnLabelAdded;
      graphControl.Graph.LabelRemoved -= OnLabelRemoved;
      graphControl.Graph.LabelLayoutParameterChanged -= OnLabelLayoutParameterChanged;
      graphControl.Graph.LabelStyleChanged -= OnLabelStyleChanged;
      graphControl.Graph.LabelPreferredSizeChanged -= OnLabelPreferredSizeChanged;
      graphControl.Graph.LabelTagChanged -= OnLabelTagChanged;
      graphControl.Graph.LabelTextChanged -= OnLabelTextChanged;
    }

    private void RegisterHierarchyEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.IsGroupNodeChanged += OnIsGroupNodeChanged;
      graphControl.Graph.ParentChanged += OnParentChanged;
    }

    private void DeregisterHierarchyEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.IsGroupNodeChanged -= OnIsGroupNodeChanged;
      graphControl.Graph.ParentChanged -= OnParentChanged;
    }

    private void RegisterFoldingEvents(object sender, RoutedEventArgs args) {
      var foldingView = graphControl.Graph.Lookup<IFoldingView>()!;
      foldingView.GroupCollapsed += FoldingOnGroupCollapsed;
      foldingView.GroupExpanded += FoldingOnGroupExpanded;
      foldingView.PropertyChanged += FoldingOnPropertyChanged;
    }

    private void DeregisterFoldingEvents(object sender, RoutedEventArgs args) {
      var foldingView = graphControl.Graph.Lookup<IFoldingView>()!;
      foldingView.GroupCollapsed -= FoldingOnGroupCollapsed;
      foldingView.GroupExpanded -= FoldingOnGroupExpanded;
      foldingView.PropertyChanged -= FoldingOnPropertyChanged;
    }

    private void RegisterGraphRenderEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.DisplaysInvalidated += OnDisplaysInvalidated;
    }

    private void DeregisterGraphRenderEvents(object sender, RoutedEventArgs args) {
      graphControl.Graph.DisplaysInvalidated -= OnDisplaysInvalidated;
    }

    private void RegisterGraphControlEvents(object sender, RoutedEventArgs args) {
      graphControl.CurrentItemChanged += ControlOnCurrentItemChanged;
    }

    private void DeregisterGraphControlEvents(object sender, RoutedEventArgs args) {
      graphControl.CurrentItemChanged -= ControlOnCurrentItemChanged;
    }

    private void RegisterInputModeEvents(object sender, RoutedEventArgs args) {
      editorMode.CanvasClicked += GeimOnCanvasClicked;
      editorMode.DeletedItem += GeimOnDeletedItem;
      editorMode.DeletedSelection += GeimOnDeletedSelection;
      editorMode.DeletingSelection += GeimOnDeletingSelection;
      editorMode.ItemClicked += GeimOnItemClicked;
      editorMode.ItemDoubleClicked += GeimOnItemDoubleClicked;
      editorMode.ItemLeftClicked += GeimOnItemLeftClicked;
      editorMode.ItemLeftDoubleClicked += GeimOnItemLeftDoubleClicked;
      editorMode.ItemRightClicked += GeimOnItemRightClicked;
      editorMode.ItemRightDoubleClicked += GeimOnItemRightDoubleClicked;
      editorMode.ItemsCopied += GeimOnItemsCopied;
      editorMode.ItemsCut += GeimOnItemsCut;
      editorMode.ItemsDuplicated += GeimOnItemsDuplicated;
      editorMode.ItemsPasted += GeimOnItemsPasted;
      editorMode.MultiSelectionFinished += GeimOnMultiSelectionFinished;
      editorMode.MultiSelectionStarted += GeimOnMultiSelectionStarted;
      editorMode.NodeCreated += GeimOnNodeCreated;
      editorMode.NodeReparented += GeimOnNodeReparented;
      editorMode.EdgePortsChanged += GeimOnEdgePortsChanged;
      editorMode.PopulateItemContextMenu += GeimOnPopulateItemContextMenu;
      editorMode.QueryItemToolTip += GeimOnQueryItemToolTip;

      viewerMode.CanvasClicked += GvimOnCanvasClicked;
      viewerMode.ItemClicked += GvimOnItemClicked;
      viewerMode.ItemDoubleClicked += GvimOnItemDoubleClicked;
      viewerMode.ItemLeftClicked += GvimOnItemLeftClicked;
      viewerMode.ItemLeftDoubleClicked += GvimOnItemLeftDoubleClicked;
      viewerMode.ItemRightClicked += GvimOnItemRightClicked;
      viewerMode.ItemRightDoubleClicked += GvimOnItemRightDoubleClicked;
      viewerMode.ItemsCopied += GvimOnItemsCopied;
      viewerMode.MultiSelectionFinished += GvimOnMultiSelectionFinished;
      viewerMode.MultiSelectionStarted += GvimOnMultiSelectionStarted;
      viewerMode.PopulateItemContextMenu += GvimOnPopulateItemContextMenu;
      viewerMode.QueryItemToolTip += GvimOnQueryItemToolTip;
    }

    private void DeregisterInputModeEvents(object sender, RoutedEventArgs args) {
      editorMode.CanvasClicked -= GeimOnCanvasClicked;
      editorMode.DeletedItem -= GeimOnDeletedItem;
      editorMode.DeletedSelection -= GeimOnDeletedSelection;
      editorMode.DeletingSelection -= GeimOnDeletingSelection;
      editorMode.ItemClicked -= GeimOnItemClicked;
      editorMode.ItemDoubleClicked -= GeimOnItemDoubleClicked;
      editorMode.ItemLeftClicked -= GeimOnItemLeftClicked;
      editorMode.ItemLeftDoubleClicked -= GeimOnItemLeftDoubleClicked;
      editorMode.ItemRightClicked -= GeimOnItemRightClicked;
      editorMode.ItemRightDoubleClicked -= GeimOnItemRightDoubleClicked;
      editorMode.ItemsCopied -= GeimOnItemsCopied;
      editorMode.ItemsCut -= GeimOnItemsCut;
      editorMode.ItemsPasted -= GeimOnItemsPasted;
      editorMode.ItemsDuplicated -= GeimOnItemsDuplicated;
      editorMode.MultiSelectionFinished -= GeimOnMultiSelectionFinished;
      editorMode.MultiSelectionStarted -= GeimOnMultiSelectionStarted;
      editorMode.NodeCreated -= GeimOnNodeCreated;
      editorMode.NodeReparented -= GeimOnNodeReparented;
      editorMode.EdgePortsChanged -= GeimOnEdgePortsChanged;
      editorMode.PopulateItemContextMenu -= GeimOnPopulateItemContextMenu;
      editorMode.QueryItemToolTip -= GeimOnQueryItemToolTip;

      viewerMode.CanvasClicked -= GvimOnCanvasClicked;
      viewerMode.ItemClicked -= GvimOnItemClicked;
      viewerMode.ItemDoubleClicked -= GvimOnItemDoubleClicked;
      viewerMode.ItemLeftClicked -= GvimOnItemLeftClicked;
      viewerMode.ItemLeftDoubleClicked -= GvimOnItemLeftDoubleClicked;
      viewerMode.ItemRightClicked -= GvimOnItemRightClicked;
      viewerMode.ItemRightDoubleClicked -= GvimOnItemRightDoubleClicked;
      viewerMode.ItemsCopied -= GvimOnItemsCopied;
      viewerMode.MultiSelectionFinished -= GvimOnMultiSelectionFinished;
      viewerMode.MultiSelectionStarted -= GvimOnMultiSelectionStarted;
      viewerMode.PopulateItemContextMenu -= GvimOnPopulateItemContextMenu;
      viewerMode.QueryItemToolTip -= GvimOnQueryItemToolTip;
    }

    private void RegisterItemDropModeEvents(object sender, RoutedEventArgs args) {
      editorMode.NodeDropInputMode.DragDropped += ItemDropModeDragDropped;
      editorMode.NodeDropInputMode.DragEntered += ItemDropModeDragEntered;
      editorMode.NodeDropInputMode.DragLeft += ItemDropModeDragLeft;
      editorMode.NodeDropInputMode.DragOver += ItemDropModeDragOver;
      editorMode.NodeDropInputMode.ItemCreated += ItemDropModeItemCreated;
      editorMode.LabelDropInputMode.DragDropped += ItemDropModeDragDropped;
      editorMode.LabelDropInputMode.DragEntered += ItemDropModeDragEntered;
      editorMode.LabelDropInputMode.DragLeft += ItemDropModeDragLeft;
      editorMode.LabelDropInputMode.DragOver += ItemDropModeDragOver;
      editorMode.LabelDropInputMode.ItemCreated += ItemDropModeItemCreated;
      // PortDropInputMode inherits DragDropped, DragEntered, DragLeft, DragOver, ItemCreated events from
      // ItemDropInputMode, too
    }

    private void DeregisterItemDropModeEvents(object sender, RoutedEventArgs args) {
      editorMode.NodeDropInputMode.DragDropped -= ItemDropModeDragDropped;
      editorMode.NodeDropInputMode.DragEntered -= ItemDropModeDragEntered;
      editorMode.NodeDropInputMode.DragLeft -= ItemDropModeDragLeft;
      editorMode.NodeDropInputMode.DragOver -= ItemDropModeDragOver;
      editorMode.NodeDropInputMode.ItemCreated -= ItemDropModeItemCreated;
      editorMode.LabelDropInputMode.DragDropped -= ItemDropModeDragDropped;
      editorMode.LabelDropInputMode.DragEntered -= ItemDropModeDragEntered;
      editorMode.LabelDropInputMode.DragLeft -= ItemDropModeDragLeft;
      editorMode.LabelDropInputMode.DragOver -= ItemDropModeDragOver;
      editorMode.LabelDropInputMode.ItemCreated -= ItemDropModeItemCreated;
    }

    private void RegisterEditLabelModeEvents(object sender, RoutedEventArgs args) {
      editorMode.EditLabelInputMode.QueryLabelAdding += EditLabelInputModeOnQueryLabelAdding;
      editorMode.EditLabelInputMode.QueryLabelEditing += EditLabelInputModeOnQueryLabelEditing;
      editorMode.EditLabelInputMode.LabelAdded += EditLabelInputModeOnLabelAdded;
      editorMode.EditLabelInputMode.LabelDeleted += EditLabelInputModeOnLabelDeleted;
      editorMode.EditLabelInputMode.LabelEdited += EditLabelInputModeOnLabelEdited;
      editorMode.EditLabelInputMode.LabelEditingCanceled += EditLabelInputModeOnLabelEditingCanceled;
      editorMode.EditLabelInputMode.LabelEditingStarted += EditLabelInputModeOnLabelEditingStarted;
      editorMode.EditLabelInputMode.ValidateLabelText += EditLabelInputModeOnValidateLabelText;
    }

    private void DeregisterEditLabelModeEvents(object sender, RoutedEventArgs args) {
      editorMode.EditLabelInputMode.QueryLabelAdding -= EditLabelInputModeOnQueryLabelAdding;
      editorMode.EditLabelInputMode.QueryLabelEditing -= EditLabelInputModeOnQueryLabelEditing;
      editorMode.EditLabelInputMode.LabelAdded -= EditLabelInputModeOnLabelAdded;
      editorMode.EditLabelInputMode.LabelDeleted -= EditLabelInputModeOnLabelDeleted;
      editorMode.EditLabelInputMode.LabelEdited -= EditLabelInputModeOnLabelEdited;
      editorMode.EditLabelInputMode.LabelEditingCanceled -= EditLabelInputModeOnLabelEditingCanceled;
      editorMode.EditLabelInputMode.LabelEditingStarted -= EditLabelInputModeOnLabelEditingStarted;
      editorMode.EditLabelInputMode.ValidateLabelText -= EditLabelInputModeOnValidateLabelText;
    }

    private void RegisterMoveModeEvents(object sender, RoutedEventArgs args) {
      editorMode.MoveSelectedItemsInputMode.DragCanceled += InputModeOnDragCanceled;
      editorMode.MoveSelectedItemsInputMode.DragCanceling += InputModeOnDragCanceling;
      editorMode.MoveSelectedItemsInputMode.DragFinished += InputModeOnDragFinished;
      editorMode.MoveSelectedItemsInputMode.DragFinishing += InputModeOnDragFinishing;
      editorMode.MoveSelectedItemsInputMode.DragStarted += InputModeOnDragStarted;
      editorMode.MoveSelectedItemsInputMode.DragStarting += InputModeOnDragStarting;

      editorMode.MoveSelectedItemsInputMode.Dragged += InputModeOnDragged;
      editorMode.MoveSelectedItemsInputMode.Dragging += InputModeOnDragging;
    }

    private void DeregisterMoveModeEvents(object sender, RoutedEventArgs args) {
      editorMode.MoveSelectedItemsInputMode.DragCanceled -= InputModeOnDragCanceled;
      editorMode.MoveSelectedItemsInputMode.DragCanceling -= InputModeOnDragCanceling;
      editorMode.MoveSelectedItemsInputMode.DragFinished -= InputModeOnDragFinished;
      editorMode.MoveSelectedItemsInputMode.DragFinishing -= InputModeOnDragFinishing;
      editorMode.MoveSelectedItemsInputMode.DragStarted -= InputModeOnDragStarted;
      editorMode.MoveSelectedItemsInputMode.DragStarting -= InputModeOnDragStarting;

      editorMode.MoveSelectedItemsInputMode.Dragged -= InputModeOnDragged;
      editorMode.MoveSelectedItemsInputMode.Dragging -= InputModeOnDragging;
    }

    private void RegisterItemHoverModeEvents(object sender, RoutedEventArgs args) {
      editorMode.ItemHoverInputMode.HoveredItemChanged += ItemHoverInputModeOnHoveredItemChanged;
      viewerMode.ItemHoverInputMode.HoveredItemChanged += ItemHoverInputModeOnHoveredItemChanged;
    }

    private void DeregisterItemHoverModeEvents(object sender, RoutedEventArgs args) {
      editorMode.ItemHoverInputMode.HoveredItemChanged -= ItemHoverInputModeOnHoveredItemChanged;
      viewerMode.ItemHoverInputMode.HoveredItemChanged -= ItemHoverInputModeOnHoveredItemChanged;
    }

    private void RegisterCreateBendModeEvents(object sender, RoutedEventArgs args) {
      editorMode.CreateBendInputMode.BendCreated += CreateBendInputModeOnBendCreated;
      editorMode.CreateBendInputMode.DragCanceled += InputModeOnDragCanceled;

      editorMode.CreateBendInputMode.Dragged += InputModeOnDragged;
      editorMode.CreateBendInputMode.Dragging += InputModeOnDragging;
    }

    private void DeregisterCreateBendModeEvents(object sender, RoutedEventArgs args) {
      editorMode.CreateBendInputMode.BendCreated -= CreateBendInputModeOnBendCreated;
      editorMode.CreateBendInputMode.DragCanceled -= InputModeOnDragCanceled;

      editorMode.CreateBendInputMode.Dragged -= InputModeOnDragged;
      editorMode.CreateBendInputMode.Dragging -= InputModeOnDragging;
    }

    private void RegisterContextMenuModeEvents(object sender, RoutedEventArgs args) {
      editorMode.ContextMenuInputMode.PopulateMenu += ContextMenuInputModeOnPopulateMenu;
      viewerMode.ContextMenuInputMode.PopulateMenu += ContextMenuInputModeOnPopulateMenu;
    }

    private void DeregisterContextMenuModeEvents(object sender, RoutedEventArgs args) {
      editorMode.ContextMenuInputMode.PopulateMenu -= ContextMenuInputModeOnPopulateMenu;
      viewerMode.ContextMenuInputMode.PopulateMenu -= ContextMenuInputModeOnPopulateMenu;
    }

    private void RegisterTextEditorModeEvents(object sender, RoutedEventArgs args) {
      editorMode.EditLabelInputMode.TextEditorInputMode.EditingCanceled += TextEditorInputModeOnEditingCanceled;
      editorMode.EditLabelInputMode.TextEditorInputMode.EditingStarted += TextEditorInputModeOnEditingStarted;
      editorMode.EditLabelInputMode.TextEditorInputMode.TextEdited += TextEditorInputModeOnTextEdited;
    }

    private void DeregisterTextEditorModeEvents(object sender, RoutedEventArgs args) {
      editorMode.EditLabelInputMode.TextEditorInputMode.EditingCanceled -= TextEditorInputModeOnEditingCanceled;
      editorMode.EditLabelInputMode.TextEditorInputMode.EditingStarted -= TextEditorInputModeOnEditingStarted;
      editorMode.EditLabelInputMode.TextEditorInputMode.TextEdited -= TextEditorInputModeOnTextEdited;
    }

    private void RegisterNavigationModeEvents(object sender, RoutedEventArgs args) {
      editorMode.NavigationInputMode.GroupCollapsed += NavigationInputModeOnGroupCollapsed;
      editorMode.NavigationInputMode.GroupCollapsing += NavigationInputModeOnGroupCollapsing;
      editorMode.NavigationInputMode.GroupEntered += NavigationInputModeOnGroupEntered;
      editorMode.NavigationInputMode.GroupEntering += NavigationInputModeOnGroupEntering;
      editorMode.NavigationInputMode.GroupExited += NavigationInputModeOnGroupExited;
      editorMode.NavigationInputMode.GroupExiting += NavigationInputModeOnGroupExiting;
      editorMode.NavigationInputMode.GroupExpanded += NavigationInputModeOnGroupExpanded;
      editorMode.NavigationInputMode.GroupExpanding += NavigationInputModeOnGroupExpanding;

      viewerMode.NavigationInputMode.GroupCollapsed += NavigationInputModeOnGroupCollapsed;
      viewerMode.NavigationInputMode.GroupCollapsing += NavigationInputModeOnGroupCollapsing;
      viewerMode.NavigationInputMode.GroupEntered += NavigationInputModeOnGroupEntered;
      viewerMode.NavigationInputMode.GroupEntering += NavigationInputModeOnGroupEntering;
      viewerMode.NavigationInputMode.GroupExited += NavigationInputModeOnGroupExited;
      viewerMode.NavigationInputMode.GroupExiting += NavigationInputModeOnGroupExiting;
      viewerMode.NavigationInputMode.GroupExpanded += NavigationInputModeOnGroupExpanded;
      viewerMode.NavigationInputMode.GroupExpanding += NavigationInputModeOnGroupExpanding;
    }

    private void DeregisterNavigationModeEvents(object sender, RoutedEventArgs args) {
      editorMode.NavigationInputMode.GroupCollapsed -= NavigationInputModeOnGroupCollapsed;
      editorMode.NavigationInputMode.GroupCollapsing -= NavigationInputModeOnGroupCollapsing;
      editorMode.NavigationInputMode.GroupEntered -= NavigationInputModeOnGroupEntered;
      editorMode.NavigationInputMode.GroupEntering -= NavigationInputModeOnGroupEntering;
      editorMode.NavigationInputMode.GroupExited -= NavigationInputModeOnGroupExited;
      editorMode.NavigationInputMode.GroupExiting -= NavigationInputModeOnGroupExiting;
      editorMode.NavigationInputMode.GroupExpanded -= NavigationInputModeOnGroupExpanded;
      editorMode.NavigationInputMode.GroupExpanding -= NavigationInputModeOnGroupExpanding;

      viewerMode.NavigationInputMode.GroupCollapsed -= NavigationInputModeOnGroupCollapsed;
      viewerMode.NavigationInputMode.GroupCollapsing -= NavigationInputModeOnGroupCollapsing;
      viewerMode.NavigationInputMode.GroupEntered -= NavigationInputModeOnGroupEntered;
      viewerMode.NavigationInputMode.GroupEntering -= NavigationInputModeOnGroupEntering;
      viewerMode.NavigationInputMode.GroupExited -= NavigationInputModeOnGroupExited;
      viewerMode.NavigationInputMode.GroupExiting -= NavigationInputModeOnGroupExiting;
      viewerMode.NavigationInputMode.GroupExpanded -= NavigationInputModeOnGroupExpanded;
      viewerMode.NavigationInputMode.GroupExpanding -= NavigationInputModeOnGroupExpanding;
    }

    private void RegisterClickModeEvents(object sender, RoutedEventArgs args) {
      editorMode.ClickInputMode.Clicked += ClickInputModeOnClicked;
      viewerMode.ClickInputMode.Clicked += ClickInputModeOnClicked;
    }

    private void DeregisterClickModeEvents(object sender, RoutedEventArgs args) {
      editorMode.ClickInputMode.Clicked -= ClickInputModeOnClicked;
      viewerMode.ClickInputMode.Clicked -= ClickInputModeOnClicked;
    }

    private void RegisterHandleModeEvents(object sender, RoutedEventArgs args) {
      editorMode.HandleInputMode.DragCanceled += InputModeOnDragCanceled;
      editorMode.HandleInputMode.DragCanceling += InputModeOnDragCanceling;
      editorMode.HandleInputMode.DragFinished += InputModeOnDragFinished;
      editorMode.HandleInputMode.DragFinishing += InputModeOnDragFinishing;
      editorMode.HandleInputMode.DragStarted += InputModeOnDragStarted;
      editorMode.HandleInputMode.DragStarting += InputModeOnDragStarting;

      editorMode.HandleInputMode.Dragged += InputModeOnDragged;
      editorMode.HandleInputMode.Dragging += InputModeOnDragging;
      editorMode.HandleInputMode.Clicked += InputModeOnClick;
    }

    private void DeregisterHandleModeEvents(object sender, RoutedEventArgs args) {
      editorMode.HandleInputMode.DragCanceled -= InputModeOnDragCanceled;
      editorMode.HandleInputMode.DragCanceling -= InputModeOnDragCanceling;
      editorMode.HandleInputMode.DragFinished -= InputModeOnDragFinished;
      editorMode.HandleInputMode.DragFinishing -= InputModeOnDragFinishing;
      editorMode.HandleInputMode.DragStarted -= InputModeOnDragStarted;
      editorMode.HandleInputMode.DragStarting -= InputModeOnDragStarting;

      editorMode.HandleInputMode.Dragged -= InputModeOnDragged;
      editorMode.HandleInputMode.Dragging -= InputModeOnDragging;
      editorMode.HandleInputMode.Clicked -= InputModeOnClick;
    }

    private void RegisterMoveViewportModeEvents(object sender, RoutedEventArgs args) {
      editorMode.MoveViewportInputMode.DragCanceled += InputModeOnDragCanceled;
      editorMode.MoveViewportInputMode.DragCanceling += InputModeOnDragCanceling;
      editorMode.MoveViewportInputMode.DragFinished += InputModeOnDragFinished;
      editorMode.MoveViewportInputMode.DragFinishing += InputModeOnDragFinishing;
      editorMode.MoveViewportInputMode.DragStarted += InputModeOnDragStarted;
      editorMode.MoveViewportInputMode.DragStarting += InputModeOnDragStarting;

      editorMode.MoveViewportInputMode.Dragged += InputModeOnDragged;
      editorMode.MoveViewportInputMode.Dragging += InputModeOnDragging;

      viewerMode.MoveViewportInputMode.DragCanceled += InputModeOnDragCanceled;
      viewerMode.MoveViewportInputMode.DragCanceling += InputModeOnDragCanceling;
      viewerMode.MoveViewportInputMode.DragFinished += InputModeOnDragFinished;
      viewerMode.MoveViewportInputMode.DragFinishing += InputModeOnDragFinishing;
      viewerMode.MoveViewportInputMode.DragStarted += InputModeOnDragStarted;
      viewerMode.MoveViewportInputMode.DragStarting += InputModeOnDragStarting;

      viewerMode.MoveViewportInputMode.Dragged += InputModeOnDragged;
      viewerMode.MoveViewportInputMode.Dragging += InputModeOnDragging;
    }

    private void DeregisterMoveViewportModeEvents(object sender, RoutedEventArgs args) {
      editorMode.MoveViewportInputMode.DragCanceled -= InputModeOnDragCanceled;
      editorMode.MoveViewportInputMode.DragCanceling -= InputModeOnDragCanceling;
      editorMode.MoveViewportInputMode.DragFinished -= InputModeOnDragFinished;
      editorMode.MoveViewportInputMode.DragFinishing -= InputModeOnDragFinishing;
      editorMode.MoveViewportInputMode.DragStarted -= InputModeOnDragStarted;
      editorMode.MoveViewportInputMode.DragStarting -= InputModeOnDragStarting;

      editorMode.MoveViewportInputMode.Dragged -= InputModeOnDragged;
      editorMode.MoveViewportInputMode.Dragging -= InputModeOnDragging;

      viewerMode.MoveViewportInputMode.DragCanceled -= InputModeOnDragCanceled;
      viewerMode.MoveViewportInputMode.DragCanceling -= InputModeOnDragCanceling;
      viewerMode.MoveViewportInputMode.DragFinished -= InputModeOnDragFinished;
      viewerMode.MoveViewportInputMode.DragFinishing -= InputModeOnDragFinishing;
      viewerMode.MoveViewportInputMode.DragStarted -= InputModeOnDragStarted;
      viewerMode.MoveViewportInputMode.DragStarting -= InputModeOnDragStarting;

      viewerMode.MoveViewportInputMode.Dragged -= InputModeOnDragged;
      viewerMode.MoveViewportInputMode.Dragging -= InputModeOnDragging;
    }

    private void RegisterCreateEdgeModeEvents(object sender, RoutedEventArgs args) {
      editorMode.CreateEdgeInputMode.EdgeCreated += CreateEdgeInputModeOnEdgeCreated;
      editorMode.CreateEdgeInputMode.EdgeCreationStarted += CreateEdgeInputModeOnEdgeCreationStarted;
      editorMode.CreateEdgeInputMode.GestureCanceled += CreateEdgeInputModeOnGestureCanceled;
      editorMode.CreateEdgeInputMode.GestureCanceling += CreateEdgeInputModeOnGestureCanceling;
      editorMode.CreateEdgeInputMode.GestureFinished += CreateEdgeInputModeOnGestureFinished;
      editorMode.CreateEdgeInputMode.GestureFinishing += CreateEdgeInputModeOnGestureFinishing;
      editorMode.CreateEdgeInputMode.GestureStarted += CreateEdgeInputModeOnGestureStarted;
      editorMode.CreateEdgeInputMode.GestureStarting += CreateEdgeInputModeOnGestureStarting;
      editorMode.CreateEdgeInputMode.Moved += CreateEdgeInputModeOnMoved;
      editorMode.CreateEdgeInputMode.Moving += CreateEdgeInputModeOnMoving;
      editorMode.CreateEdgeInputMode.PortAdded += CreateEdgeInputModeOnPortAdded;
      editorMode.CreateEdgeInputMode.StartPortCandidateChanged += CreateEdgeInputModeOnStartPortCandidateChanged;
      editorMode.CreateEdgeInputMode.EndPortCandidateChanged += CreateEdgeInputModeOnEndPortCandidateChanged;
      editorMode.CreateEdgeInputMode.EdgeDirectionReversed += CreateEdgeInputModeOnEdgeDirectionReversed;
    }

    private void DeregisterCreateEdgeModeEvents(object sender, RoutedEventArgs args) {
      editorMode.CreateEdgeInputMode.EdgeCreated -= CreateEdgeInputModeOnEdgeCreated;
      editorMode.CreateEdgeInputMode.EdgeCreationStarted -= CreateEdgeInputModeOnEdgeCreationStarted;
      editorMode.CreateEdgeInputMode.GestureCanceled -= CreateEdgeInputModeOnGestureCanceled;
      editorMode.CreateEdgeInputMode.GestureCanceling -= CreateEdgeInputModeOnGestureCanceling;
      editorMode.CreateEdgeInputMode.GestureFinished -= CreateEdgeInputModeOnGestureFinished;
      editorMode.CreateEdgeInputMode.GestureFinishing -= CreateEdgeInputModeOnGestureFinishing;
      editorMode.CreateEdgeInputMode.GestureStarted -= CreateEdgeInputModeOnGestureStarted;
      editorMode.CreateEdgeInputMode.GestureStarting -= CreateEdgeInputModeOnGestureStarting;
      editorMode.CreateEdgeInputMode.Moved -= CreateEdgeInputModeOnMoved;
      editorMode.CreateEdgeInputMode.Moving -= CreateEdgeInputModeOnMoving;
      editorMode.CreateEdgeInputMode.PortAdded -= CreateEdgeInputModeOnPortAdded;
      editorMode.CreateEdgeInputMode.StartPortCandidateChanged -= CreateEdgeInputModeOnStartPortCandidateChanged;
      editorMode.CreateEdgeInputMode.EndPortCandidateChanged -= CreateEdgeInputModeOnEndPortCandidateChanged;
      editorMode.CreateEdgeInputMode.EdgeDirectionReversed -= CreateEdgeInputModeOnEdgeDirectionReversed;
    }

    private void RegisterSelectionEvents(object sender, RoutedEventArgs args) {
      graphControl.Selection.ItemAdded += SelectionOnItemAdded;
      graphControl.Selection.ItemRemoved += SelectionOnItemRemoved;
    }

    private void DeregisterSelectionEvents(object sender, RoutedEventArgs args) {
      graphControl.Selection.ItemAdded -= SelectionOnItemAdded;
      graphControl.Selection.ItemRemoved -= SelectionOnItemRemoved;
    }

    #endregion

    #region InputMode Installing

    private void InstallEditorMode(object sender, RoutedEventArgs args) {
      graphControl.InputMode = editorMode;
    }

    private void InstallViewerMode(object sender, RoutedEventArgs args) {
      graphControl.InputMode = viewerMode;
    }

    private void EnableOrthogonalEdges(object sender, RoutedEventArgs args) {
      editorMode.OrthogonalEdgeEditingContext.Enabled = true;
    }

    private void EnableWaitForSecondClick(object sender, RoutedEventArgs args) {
      editorMode.ClickInputMode.ClickReportingPolicy = ClickReportingPolicy.WaitForDoubleClick;
      viewerMode.ClickInputMode.ClickReportingPolicy = ClickReportingPolicy.WaitForDoubleClick;
    }

    private void DisableWaitForSecondClick(object sender, RoutedEventArgs args) {
      editorMode.ClickInputMode.ClickReportingPolicy = ClickReportingPolicy.Instant;
      viewerMode.ClickInputMode.ClickReportingPolicy = ClickReportingPolicy.Instant;
    }

    private void DisableOrthogonalEdges(object sender, RoutedEventArgs args) {
      editorMode.OrthogonalEdgeEditingContext.Enabled = false;
    }

    #endregion

    #region Graph Events

    private void OnDisplaysInvalidated(object sender, EventArgs args) {
      Log("Displays Invalidated");
    }

    private void OnEdgeStyleChanged(object sender, ItemChangedEventArgs<IEdge, IEdgeStyle> args) {
      Log("Edge Style Changed: " + args.Item, "EdgeStyleChanged");
    }

    private void OnEdgePortsChanged(object sender, EdgeEventArgs args) {
      Log("Edge Ports Changed: " + args.Item, "EdgePortsChanged");
    }

    private void OnEdgeTagChanged(object sender, ItemChangedEventArgs<IEdge, object> args) {
      Log("Edge Tag Changed: " + args.Item, "EdgeTagChanged");
    }

    private void OnEdgeCreated(object sender, ItemEventArgs<IEdge> args) {
      Log("Edge Created: " + args.Item, "EdgeCreated");
    }

    private void OnEdgeRemoved(object sender, ItemEventArgs<IEdge> args) {
      Log("Edge Removed: " + args.Item, "EdgeRemoved");
    }

    private void OnLabelAdded(object sender, ItemEventArgs<ILabel> args) {
      Log("Label Added: " + args.Item, "LabelAdded");
    }

    private void OnLabelLayoutParameterChanged(object sender, ItemChangedEventArgs<ILabel, ILabelModelParameter> args) {
      Log("Label Layout Parameter Changed: " + args.Item, "LabelLayoutParameterChanged");
    }

    private void OnLabelStyleChanged(object sender, ItemChangedEventArgs<ILabel, ILabelStyle> args) {
      Log("Label Style Changed: " + args.Item, "LabelStyleChanged");
    }

    private void OnLabelPreferredSizeChanged(object sender, ItemChangedEventArgs<ILabel, SizeD> args) {
      Log("Label Preferred Size Changed: " + args.Item, "LabelPreferredSizeChanged");
    }

    private void OnLabelTagChanged(object sender, ItemChangedEventArgs<ILabel, object> args) {
      Log("Label Tag Changed: " + args.Item, "LabelTagChanged");
    }

    private void OnLabelTextChanged(object sender, ItemChangedEventArgs<ILabel, string> args) {
      Log("Label Text Changed: " + args.Item, "LabelTextChanged");
    }

    private void OnLabelRemoved(object sender, ItemEventArgs<ILabel> args) {
      Log("Label Removed: " + args.Item, "LabelRemoved");
    }

    private void OnNodeStyleChanged(object sender, ItemChangedEventArgs<INode, INodeStyle> args) {
      Log("Node Style Changed: " + args.Item, "NodeStyleChanged");
    }

    private void OnNodeTagChanged(object sender, ItemChangedEventArgs<INode, object> args) {
      Log("Node Tag Changed: " + args.Item, "NodeTagChanged");
    }

    private void OnNodeCreated(object sender, ItemEventArgs<INode> args) {
      Log("Node Created: " + args.Item, "NodeCreated");
    }

    private void OnNodeRemoved(object sender, ItemEventArgs<INode> args) {
      Log("Node Removed: " + args.Item, "NodeRemoved");
    }

    private void OnIsGroupNodeChanged(object sender, NodeEventArgs args) {
      Log("Group State Changed: " + args.IsGroupNode, "IsGroupNodeChanged");
    }

    private void OnParentChanged(object sender, NodeEventArgs args) {
      var newParent = graphControl.Graph.GetParent(args.Item);
      Log("Parent Changed: " + args.Parent + " -> " + newParent, "ParentChanged");
    }

    private void OnPortAdded(object sender, ItemEventArgs<IPort> args) {
      Log("Port Added: " + args.Item, "PortAdded");
    }

    private void OnPortLocationParameterChanged(
        object sender,
        ItemChangedEventArgs<IPort, IPortLocationModelParameter> args) {
      Log("Port Location Parameter Changed: " + args.Item, "PortLocationParameterChanged");
    }

    private void OnPortStyleChanged(object sender, ItemChangedEventArgs<IPort, IPortStyle> args) {
      Log("Port Style Changed: " + args.Item, "PortStyleChanged");
    }

    private void OnPortTagChanged(object sender, ItemChangedEventArgs<IPort, object> args) {
      Log("Port Tag Changed: " + args.Item, "PortTagChanged");
    }

    private void OnPortRemoved(object sender, ItemEventArgs<IPort> args) {
      Log("Port Removed: " + args.Item, "PortRemoved");
    }

    private void OnBendAdded(object sender, ItemEventArgs<IBend> args) {
      Log("Bend Added: " + args.Item, "BendAdded");
    }

    private void OnBendLocationChanged(object sender, IBend bend, PointD oldLocation) {
      Log("Bend Location Changed: " + bend + "; " + oldLocation, "BendLocationChanged");
    }

    private void OnBendTagChanged(object sender, ItemChangedEventArgs<IBend, object> args) {
      Log("Bend Tag Changed: " + args.Item, "BendTagChanged");
    }

    private void OnBendRemoved(object sender, ItemEventArgs<IBend> args) {
      Log("Bend Removed: " + args.Item, "BendRemoved");
    }

    private void FoldingOnPropertyChanged(object sender, PropertyChangedEventArgs args) {
      Log("PropertyChanged: " + args.PropertyName, "PropertyChanged");
    }

    private void FoldingOnGroupExpanded(object sender, ItemEventArgs<INode> args) {
      Log("Group Expanded: " + args.Item, "GroupExpanded");
    }

    private void FoldingOnGroupCollapsed(object sender, ItemEventArgs<INode> args) {
      Log("Group Collapsed: " + args.Item, "GroupCollapsed");
    }

    #endregion

    #region Clipboard Events

    private void ClipboardOnGraphCopiedToClipboard(object sender, ItemCopiedEventArgs<IGraph> args) {
      Log("Graph copied to Clipboard");
    }

    private void ClipboardOnNodeCopiedToClipboard(object sender, ItemCopiedEventArgs<INode> args) {
      Log("Node copied to Clipboard");
    }

    private void ClipboardOnEdgeCopiedToClipboard(object sender, ItemCopiedEventArgs<IEdge> args) {
      Log("Edge copied to Clipboard");
    }

    private void ClipboardOnPortCopiedToClipboard(object sender, ItemCopiedEventArgs<IPort> args) {
      Log("Port copied to Clipboard");
    }

    private void ClipboardOnLabelCopiedToClipboard(object sender, ItemCopiedEventArgs<ILabel> args) {
      Log("Label copied to Clipboard");
    }

    private void ClipboardOnObjectCopiedToClipboard(object sender, ItemCopiedEventArgs<object> args) {
      Log("Object copied to Clipboard");
    }

    private void ClipboardOnGraphCopiedFromClipboard(object sender, ItemCopiedEventArgs<IGraph> args) {
      Log("Graph copied from Clipboard");
    }

    private void ClipboardOnNodeCopiedFromClipboard(object sender, ItemCopiedEventArgs<INode> args) {
      Log("Node copied from Clipboard");
    }

    private void ClipboardOnEdgeCopiedFromClipboard(object sender, ItemCopiedEventArgs<IEdge> args) {
      Log("Edge  copied from Clipboard");
    }

    private void ClipboardOnPortCopiedFromClipboard(object sender, ItemCopiedEventArgs<IPort> args) {
      Log("Port  copied from Clipboard");
    }

    private void ClipboardOnLabelCopiedFromClipboard(object sender, ItemCopiedEventArgs<ILabel> args) {
      Log("Label copied from Clipboard");
    }

    private void ClipboardOnObjectCopiedFromClipboard(object sender, ItemCopiedEventArgs<object> args) {
      Log("Object copied from Clipboard");
    }

    private void ClipboardOnGraphDuplicated(object sender, ItemCopiedEventArgs<IGraph> args) {
      Log("Graph duplicated.");
    }

    private void ClipboardOnNodeDuplicated(object sender, ItemCopiedEventArgs<INode> args) {
      Log("Node duplicated");
    }

    private void ClipboardOnEdgeDuplicated(object sender, ItemCopiedEventArgs<IEdge> args) {
      Log("Edge duplicated");
    }

    private void ClipboardOnPortDuplicated(object sender, ItemCopiedEventArgs<IPort> args) {
      Log("Port duplicated");
    }

    private void ClipboardOnLabelDuplicated(object sender, ItemCopiedEventArgs<ILabel> args) {
      Log("Label duplicated");
    }

    private void ClipboardOnObjectDuplicated(object sender, ItemCopiedEventArgs<object> args) {
      Log("Object duplicated");
    }

    private void ClipboardOnItemsCutting(object sender, EventArgs args) {
      Log("Clipboard operation: Cutting");
    }

    private void ClipboardOnItemsCut(object sender, EventArgs args) {
      Log("Clipboard operation: Cut");
    }

    private void ClipboardOnItemsCopying(object sender, EventArgs args) {
      Log("Clipboard operation: Copying");
    }

    private void ClipboardOnItemsCopied(object sender, EventArgs args) {
      Log("Clipboard operation: Copied");
    }

    private void ClipboardOnItemsPasting(object sender, EventArgs args) {
      Log("Clipboard operation: Pasting");
    }

    private void ClipboardOnItemsPasted(object sender, EventArgs args) {
      Log("Clipboard operation: Pasted");
    }

    private void ClipboardOnItemsDuplicating(object sender, EventArgs args) {
      Log("Clipboard operation: Duplicating");
    }

    private void ClipboardOnItemsDuplicated(object sender, EventArgs args) {
      Log("Clipboard operation: Duplicated");
    }

    #endregion

    #region Undo Events

    private void UndoEngineOnPropertyChanged(object sender, PropertyChangedEventArgs args) {
      Log("PropertyChanged: " + args.PropertyName, "PropertyChanged");
    }

    private void UndoEngineOnUnitUndone(object sender, EventArgs args) {
      Log("Undo performed");
    }

    private void UndoEngineOnUnitRedone(object sender, EventArgs args) {
      Log("Redo performed");
    }

    #endregion

    #region GraphControl Events

    private void ControlOnCurrentItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<IModelItem> routedPropertyChangedEventArgs) {
      Log("GraphControl CurrentItemChanged");
    }

    #endregion

    #region GraphControl Key Events

    private void ControlOnCompoundKeyDown(object sender, CompoundKeyEventArgs args) {
      Log("GraphControl CompoundKeyPressed: " + args.KeyCode, "GraphControlKeyPressed");
    }

    private void ControlOnCompoundKeyUp(object sender, CompoundKeyEventArgs args) {
      Log("GraphControl CompoundKeyReleased: " + args.KeyCode, "GraphControlKeyReleased");
    }

    private void ControlOnCompoundKeyTextInput(object sender, CompoundKeyEventArgs args) {
      Log("GraphControl CompoundKeyTyped: " + args.KeyCode, "GraphControlKeyTyped");
    }

    #endregion

    #region GraphControl Pointer Events

    private void ControlOnPointerClick(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerClicked ({me.PointerType}, button: {me.ChangedButtons}, ClickCount: {me.ClickCount})");
    }

    private void ControlOnPointerDrag(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerDragged ({me.PointerType})");
    }

    private void ControlOnPointerEntered(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerEntered ({me.PointerType})");
    }

    private void ControlOnPointerLeave(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerExited ({me.PointerType})");
    }

    private void ControlOnLostPointerCapture(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerLostCapture ({me.PointerType})");
    }

    private void ControlOnPointerMoved(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerMoved ({me.PointerType})");
    }

    private void ControlOnPointerDown(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerDown ({me.PointerType}, button: {me.ChangedButtons})");
    }

    private void ControlOnPointerUp(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerUp ({me.PointerType}, button: {me.ChangedButtons})");
    }

    private void ControlOnWheel(object sender, PointerEventArgs me) {
      var direction = me switch {
          { WheelDeltaY: < 0 } => "🞃",
          { WheelDeltaY: > 0 } => "🞁",
          { WheelDeltaX: < 0 } => "🞀",
          { WheelDeltaX: > 0 } => "🞂",
          _ => null
      };
      Log($"GraphControl Wheel ({direction} {(me.WheelDeltaX != 0 ? me.WheelDeltaX : me.WheelDeltaY)})");
    }

    private void ControlOnPointerLongPress(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerLongPressed ({me.PointerType})");
    }

    private void ControlOnPointerLongRest(object sender, PointerEventArgs me) {
      Log($"GraphControl PointerLongRest ({me.PointerType})");
    }

    #endregion

    #region GraphControl Render Events

    private void ControlOnPrepareRenderContext(object src, PrepareRenderContextEventArgs args) {
      Log("GraphControl PrepareRenderContext");
    }

    private void ControlOnUpdatedVisual(object sender, RoutedEventArgs args) {
      Log("GraphControl UpdatedVisual");
    }

    private void ControlOnUpdatingVisual(object sender, RoutedEventArgs args) {
      Log("GraphControl UpdatingVisual");
    }

    #endregion

    #region GraphControl Navigation Events

    private void ControlOnViewportChanged(object sender, PropertyChangedEventArgs args) {
      Log("GraphControl ViewportChanged");
    }

    private void ControlOnZoomChanged(object sender, DependencyPropertyChangedEventArgs args) {
      Log("GraphControl ZoomChanged");
    }

    #endregion

    #region Node Layout Changed Event

    private void OnNodeLayoutChanged(object source, INode node, RectD oldLayout) {
      Log("Node Layout Changed");
    }

    #endregion

    #region ItemDropInputMode Events

    private void ItemDropModeDragDropped(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragDropped", "DragDropped");
    }

    private void ItemDropModeDragEntered(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragEntered", "DragEntered");
    }

    private void ItemDropModeDragLeft(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragLeft", "DragLeft");
    }

    private void ItemDropModeDragOver(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragOver", "DragOver");
    }

    private void ItemDropModeItemCreated(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " ItemCreated", "ItemCreated");
    }

    #endregion

    #region EditLabelInputMode Events

    private void EditLabelInputModeOnQueryLabelAdding(object sender, LabelEditingEventArgs e) {
      Log("EditLabelInputMode QueryLabelAdding");
    }

    private void EditLabelInputModeOnLabelAdded(object sender, InputModeItemEventArgs<ILabel> args) {
      Log("EditLabelInputMode LabelAdded");
    }

    private void EditLabelInputModeOnLabelDeleted(object sender, InputModeItemEventArgs<ILabel> args) {
      Log("EditLabelInputMode LabelDeleted");
    }

    private void EditLabelInputModeOnQueryLabelEditing(object sender, LabelEditingEventArgs e) {
      Log("EditLabelInputMode QueryLabelEditing");
    }

    private void EditLabelInputModeOnLabelEdited(object sender, InputModeItemEventArgs<ILabel> args) {
      Log("EditLabelInputMode LabelEdited");
    }

    private void EditLabelInputModeOnLabelEditingStarted(object sender, InputModeItemEventArgs<ILabel> args) {
      Log("EditLabelInputMode LabelEditingStarted");
    }

    private void EditLabelInputModeOnLabelEditingCanceled(object sender, InputModeItemEventArgs<ILabel> args) {
      Log("EditLabelInputMode LabelEditingCanceled");
    }

    private void EditLabelInputModeOnValidateLabelText(object sender, LabelTextValidatingEventArgs args) {
      Log("EditLabelInputMode ValidateLabelText");
    }

    #endregion

    #region GraphEditorInputMode Events

    private void GeimOnCanvasClicked(object sender, ClickEventArgs args) {
      Log("GraphEditorInputMode CanvasClicked");
    }

    private void GeimOnDeletedItem(object sender, InputModeItemChangedEventArgs<IModelItem, EventArgs> args) {
      Log("GraphEditorInputMode DeletedItem");
    }

    private void GeimOnDeletedSelection(object sender, ItemsEventArgs args) {
      Log("GraphEditorInputMode DeletedSelection");
    }

    private void GeimOnDeletingSelection(object sender, SelectionEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode DeletingSelection");
    }

    private void GeimOnItemClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode ItemClicked" + (args.Handled ? "(Handled)" : "(Unhandled)"));
    }

    private void GeimOnItemDoubleClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode ItemDoubleClicked" + (args.Handled ? "(Handled)" : "(Unhandled)"));
    }

    private void GeimOnItemLeftClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode ItemLeftClicked" + (args.Handled ? "(Handled)" : "(Unhandled)"));
    }

    private void GeimOnItemLeftDoubleClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode ItemLeftDoubleClicked" + (args.Handled ? "(Handled)" : "(Unhandled)"));
    }

    private void GeimOnItemRightClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode ItemRightClicked" + (args.Handled ? "(Handled)" : "(Unhandled)"));
    }

    private void GeimOnItemRightDoubleClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode ItemRightDoubleClicked" + (args.Handled ? "(Handled)" : "(Unhandled)"));
    }

    private void GeimOnMultiSelectionFinished(object sender, SelectionEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode MultiSelectionFinished");
    }

    private void GeimOnMultiSelectionStarted(object sender, SelectionEventArgs<IModelItem> args) {
      Log("GraphEditorInputMode MultiSelectionStarted");
    }

    private void GeimOnNodeCreated(object sender, InputModeItemEventArgs<INode> args) {
      Log("GraphEditorInputMode NodeCreated");
    }

    private void GeimOnNodeReparented(object sender, InputModeItemChangedEventArgs<INode, NodeEventArgs> args) {
      Log("GraphEditorInputMode NodeReparented");
    }

    private void GeimOnEdgePortsChanged(object sender, InputModeItemChangedEventArgs<IEdge, EdgeEventArgs> args) {
      var detail = args.Details;
      Log("GraphEditorInputMode Edge " + args.Item + " Ports Changed from " + detail.SourcePort + "->" +
          detail.TargetPort + " to " + detail.Item.SourcePort + "->" + detail.Item.TargetPort);
    }

    private void GeimOnPopulateItemContextMenu(object sender, PopulateItemContextMenuEventArgs<IModelItem> args) {
      var type = GraphItemTypesSupport.GetItemType(args.Item);
      Log("GraphEditorInputMode PopulateItemContextMenu for " + type + " " +
          (args.Handled ? "(Handled)" : "(Unhandled)"));
    }

    private void GeimOnQueryItemToolTip(object sender, QueryItemToolTipEventArgs<IModelItem> args) {
      var type = GraphItemTypesSupport.GetItemType(args.Item);
      Log("GraphEditorInputMode QueryItemToolTip for " + type + " " + (args.Handled ? "(Handled)" : "(Unhandled)"));
    }

    private void GeimOnItemsCopied(object sender, EventArgs e) {
      Log("GraphEditorInputMode ItemsCopied");
    }

    private void GeimOnItemsCut(object sender, EventArgs e) {
      Log("GraphEditorInputMode ItemsCut");
    }

    private void GeimOnItemsPasted(object sender, EventArgs e) {
      Log("GraphEditorInputMode ItemsPasted");
    }

    private void GeimOnItemsDuplicated(object sender, EventArgs e) {
      Log("GraphEditorInputMode ItemsDuplicated");
    }

    #endregion

    #region GraphViewerInputMode Events

    private void GvimOnCanvasClicked(object sender, ClickEventArgs args) {
      Log("GraphViewerInputMode CanvasClicked");
    }

    private void GvimOnItemClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphViewerInputMode ItemClicked");
    }

    private void GvimOnItemDoubleClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphViewerInputMode ItemDoubleClicked");
    }

    private void GvimOnItemLeftClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphViewerInputMode ItemLeftClicked");
    }

    private void GvimOnItemLeftDoubleClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphViewerInputMode ItemLeftDoubleClicked");
    }

    private void GvimOnItemRightClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphViewerInputMode ItemRightClicked");
    }

    private void GvimOnItemRightDoubleClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      Log("GraphViewerInputMode ItemRightDoubleClicked");
    }

    private void GvimOnMultiSelectionFinished(object sender, SelectionEventArgs<IModelItem> args) {
      Log("GraphViewerInputMode MultiSelectionFinished");
    }

    private void GvimOnMultiSelectionStarted(object sender, SelectionEventArgs<IModelItem> args) {
      Log("GraphViewerInputMode MultiSelectionStarted");
    }

    private void GvimOnPopulateItemContextMenu(
        object sender,
        PopulateItemContextMenuEventArgs<IModelItem> populateItemContextMenuEventArgs) {
      Log("GraphViewerInputMode PopulateItemContextMenu");
    }

    private void GvimOnQueryItemToolTip(
        object sender,
        QueryItemToolTipEventArgs<IModelItem> queryItemToolTipEventArgs) {
      Log("GraphViewerInputMode QueryItemToolTip");
    }

    private void GvimOnItemsCopied(object sender, EventArgs e) {
      Log("GraphViewerInputMode ItemsCopied");
    }

    #endregion

    #region Drag Events

    private void InputModeOnDragCanceled(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragCanceled", "DragCanceled");
    }

    private void InputModeOnDragCanceling(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragCanceling", "DragCanceling");
    }

    private void InputModeOnDragFinished(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragFinished", "DragFinished");
    }

    private void InputModeOnDragFinishing(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragFinishing" + GetAffectedItems(sender), "DragFinishing");
    }

    private void InputModeOnDragged(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " Dragged", "Dragged");
    }

    private void InputModeOnDragging(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " Dragging", "Dragging");
    }

    private void InputModeOnDragStarted(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragStarted" + GetAffectedItems(sender), "DragStarted");
    }

    private void InputModeOnClick(object sender, ClickEventArgs args) {
      Log(sender.GetType().Name + " Clicked Handle", "HandleClicked");
    }

    private static string GetAffectedItems(object sender) {
      IEnumerable<IModelItem> items = null;
      var mim = sender as MoveInputMode;
      if (mim != null) {
        items = mim.AffectedItems;
      }
      var him = sender as HandleInputMode;
      if (him != null) {
        items = him.AffectedItems;
      }
      if (items != null) {
        var nodeCount = items.OfType<INode>().Count();
        var edgeCount = items.OfType<IEdge>().Count();
        var bendCount = items.OfType<IBend>().Count();
        var labelCount = items.OfType<ILabel>().Count();
        var portCount = items.OfType<IPort>().Count();
        return string.Format(" ({0} items: {1} nodes, {2} bends, {3} edges, {4} labels, {5} ports)", items.Count(),
            nodeCount, bendCount, edgeCount, labelCount, portCount);
      }
      return "";
    }

    private void InputModeOnDragStarting(object sender, InputModeEventArgs args) {
      Log(sender.GetType().Name + " DragStarting", "DragStarting");
    }

    #endregion

    #region ItemHoverInputMode Events

    private void ItemHoverInputModeOnHoveredItemChanged(object sender, HoveredItemChangedEventArgs args) {
      var oldType = GraphItemTypesSupport.GetItemType(args.OldItem);
      var newType = GraphItemTypesSupport.GetItemType(args.Item);
      Log("HoverInputMode Item changed from " + oldType + " to " + newType, "HoveredItemChanged");
    }

    #endregion

    #region CreateBendInputMode Events

    private void CreateBendInputModeOnBendCreated(object sender, InputModeItemEventArgs<IBend> args) {
      Log("CreateBendInputMode Bend Created");
    }

    #endregion

    #region ContextMenuInputMode Events

    private void ContextMenuInputModeOnPopulateMenu(object sender, PopulateMenuEventArgs args) {
      Log("ContextMenuInputMode Populate Menu");
    }

    #endregion

    #region TextEditorInputMode Events

    private void TextEditorInputModeOnEditingCanceled(object sender, EventArgs args) {
      Log("TextEditorInputMode Editing Canceled");
    }

    private void TextEditorInputModeOnEditingStarted(object sender, EventArgs args) {
      Log("TextEditorInputMode Editing Started");
    }

    private void TextEditorInputModeOnTextEdited(object sender, EventArgs args) {
      Log("TextEditorInputMode Text Edited");
    }

    #endregion

    #region ToolTipInputMode Events

    private void ToolTipInputModeOnQueryToolTip(object sender, QueryToolTipEventArgs args) {
      Log("ToolTipInputMode QueryToolTip");
    }

    #endregion

    #region ClickInputMode Events

    private void ClickInputModeOnClicked(object sender, ClickEventArgs args) {
      Log($"ClickInputMode Clicked (buttons: {args.PointerButtons.ToString()}, clicks: {args.ClickCount})");
    }

    #endregion

    #region NavigationInputMode Events

    private void NavigationInputModeOnGroupCollapsed(object source, InputModeItemEventArgs<INode> args) {
      Log("NavigationInputMode Group Collapsed: " + args.Item, "GroupCollapsed");
    }

    private void NavigationInputModeOnGroupCollapsing(object source, InputModeItemEventArgs<INode> args) {
      Log("NavigationInputMode Group Collapsing: " + args.Item, "Group Collapsing");
    }

    private void NavigationInputModeOnGroupEntered(object source, InputModeItemEventArgs<INode> args) {
      Log("NavigationInputMode Group Entered: " + args.Item, "Group Entered");
    }

    private void NavigationInputModeOnGroupEntering(object source, InputModeItemEventArgs<INode> args) {
      Log("NavigationInputMode Group Entering: " + args.Item, "Group Entering");
    }

    private void NavigationInputModeOnGroupExited(object source, InputModeItemEventArgs<INode> args) {
      Log("NavigationInputMode Group Exited: " + args.Item, "Group Exited");
    }

    private void NavigationInputModeOnGroupExiting(object source, InputModeItemEventArgs<INode> args) {
      Log("NavigationInputMode Group Exiting: " + args.Item, "Group Exiting");
    }

    private void NavigationInputModeOnGroupExpanded(object source, InputModeItemEventArgs<INode> args) {
      Log("NavigationInputMode Group Expanded: " + args.Item, "Group Expanded");
    }

    private void NavigationInputModeOnGroupExpanding(object source, InputModeItemEventArgs<INode> args) {
      Log("NavigationInputMode Group Expanding: " + args.Item, "Group Expanding");
    }

    #endregion

    #region CreateEdgeInputMode Events

    private void CreateEdgeInputModeOnEdgeCreated(object sender, InputModeItemEventArgs<IEdge> args) {
      Log("CreateEdgeInputMode Edge Created");
    }

    private void CreateEdgeInputModeOnEdgeCreationStarted(object sender, InputModeItemEventArgs<IEdge> args) {
      Log("CreateEdgeInputMode Edge Creation Started");
    }

    private void CreateEdgeInputModeOnGestureCanceled(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Gesture Canceled");
    }

    private void CreateEdgeInputModeOnGestureCanceling(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Gesture Canceling");
    }

    private void CreateEdgeInputModeOnGestureFinished(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Gesture Finished");
    }

    private void CreateEdgeInputModeOnGestureFinishing(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Gesture Finishing");
    }

    private void CreateEdgeInputModeOnGestureStarted(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Gesture Started");
    }

    private void CreateEdgeInputModeOnGestureStarting(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Gesture Starting");
    }

    private void CreateEdgeInputModeOnEdgeDirectionReversed(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Edge Direction Reversed");
    }

    private void CreateEdgeInputModeOnMoved(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Moved");
    }

    private void CreateEdgeInputModeOnMoving(object sender, InputModeEventArgs args) {
      Log("CreateEdgeInputMode Moving");
    }

    private void CreateEdgeInputModeOnPortAdded(object source, InputModeItemEventArgs<IPort> args) {
      Log("CreateEdgeInputMode Port Added");
    }

    private void CreateEdgeInputModeOnStartPortCandidateChanged(
        object sender,
        InputModeItemEventArgs<IPortCandidate> args) {
      Log("CreateEdgeInputMode Start Port Candidate Changed");
    }

    private void CreateEdgeInputModeOnEndPortCandidateChanged(
        object sender,
        InputModeItemEventArgs<IPortCandidate> args) {
      Log("CreateEdgeInputMode End Port Candidate Changed");
    }

    #endregion

    #region Selection Events

    private void SelectionOnItemAdded(object source, ItemEventArgs<IModelItem> args) {
      Log("GraphControl Item Selected: " + args.Item, "ItemSelected");
    }

    private void SelectionOnItemRemoved(object source, ItemEventArgs<IModelItem> args) {
      Log("GraphControl Item Deselected: " + args.Item, "ItemDeselected");
    }

    #endregion

    #region UI event handlers

    private void ClearButtonClick(object sender, RoutedEventArgs args) {
      ClearLog();
    }

    private void OnTemplateMouseDown(object sender, RoutedEventArgs args) {
      if (sender is FrameworkElement { DataContext: INode node } o) {
        if ("Label Container".Equals(node.Tag)) {
          var dao = new DataObject();
          dao.SetData(typeof(ILabel), node.Labels.First());
          DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        } else if ("Port Container".Equals(node.Tag)) {
          var dao = new DataObject();
          dao.SetData(typeof(IPort), node.Ports.First());
          DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        } else {
          var dao = new DataObject();
          dao.SetData(typeof(INode), node);
          DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        }
      }
    }

    #endregion

    #region Logging

    private readonly System.Collections.ObjectModel.ObservableCollection<ILogEntry> messages = [];

    private void Log(string message, string type = null) {
      if (type == null) {
        type = message;
      }
      var msg = new Message {
          Text = message,
          TimeStamp = DateTime.Now,
          Type = type
      };
      messages.Insert(0, msg);

      if (groupEvents.IsChecked == true) {
        MergeEvents();
      }
    }

    private void MergeEvents() {
      MergeWithLatestGroup();
      CreateNewGroup();
    }

    private void MergeWithLatestGroup() {
      var latestGroup = messages.FirstOrDefault(m => m is MessageGroup) as MessageGroup;
      if (latestGroup == null) {
        return;
      }

      var precedingEvents = messages.TakeWhile(m => m is Message).Cast<Message>().ToList();
      var groupCount = latestGroup.RepeatedMessages.Count;
      if (precedingEvents.Count < groupCount) {
        return;
      }

      var precedingTypes = precedingEvents.Select(m => m.Type);
      var groupTypes = latestGroup.RepeatedMessages.Select(m => m.Type);
      // Merge into group
      if (groupTypes.SequenceEqual(precedingTypes)) {
        latestGroup.RepeatedMessages.Clear();
        foreach (var m in precedingEvents) {
          latestGroup.RepeatedMessages.Add(m);
        }
        latestGroup.RepeatCount++;
        for (var i = messages.IndexOf(latestGroup) - 1; i >= 0; i--) {
          messages.RemoveAt(i);
        }
      }
    }

    private void CreateNewGroup() {
      var ungroupedEvents = messages.TakeWhile(m => m is Message).Cast<Message>().ToList();
      for (var start = ungroupedEvents.Count - 1; start >= 1; start--) {
        for (var length = 1; start - 2 * length + 1 >= 0; length++) {
          var types = ungroupedEvents.GetRange(start - length + 1, length).Select(m => m.Type);
          var preceding = ungroupedEvents.GetRange(start - 2 * length + 1, length);
          var precedingTypes = preceding.Select(m => m.Type);
          if (types.SequenceEqual(precedingTypes)) {
            var group = new MessageGroup();
            group.RepeatCount = 2;
            foreach (var m in preceding) {
              group.RepeatedMessages.Add(m);
            }
            messages.Insert(start + 1, group);
            for (var i = start; i >= start - 2 * length + 1; i--) {
              messages.RemoveAt(i);
            }
            return;
          }
        }
      }
    }

    private void ClearLog() {
      messages.Clear();
    }

    #endregion
  }

  #region Logging classes

  public interface ILogEntry { }

  public class Message : ILogEntry
  {
    public DateTime TimeStamp { get; set; }

    public string Text { get; set; }

    public string Type { get; set; }
  }

  public class MessageGroup : ILogEntry, INotifyPropertyChanged
  {
    private int repeatCount;

    public MessageGroup() {
      RepeatedMessages.CollectionChanged += (sender, args) => OnPropertyChanged("RepeatedMessages");
    }

    public System.Collections.ObjectModel.ObservableCollection<Message> RepeatedMessages { get; } = [];

    public int RepeatCount {
      get => repeatCount;
      set {
        repeatCount = value;
        OnPropertyChanged("RepeatCount");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }

  public class EventLogDataTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
      var element = container as FrameworkElement;

      if (element == null || item == null || !(item is ILogEntry)) {
        return null;
      }

      if (item is Message) {
        return element.FindResource("MessageTemplate") as DataTemplate;
      }
      if (item is MessageGroup) {
        return element.FindResource("GroupTemplate") as DataTemplate;
      }
      return null;
    }
  }

  #endregion

  /// <summary>
  ///   Creates image representations from graph nodes for the demo's palette list view.
  /// </summary>
  [ValueConversion(typeof(INode), typeof(UIElement))]
  public class NodeImageConverter : IValueConverter
  {
    private readonly GraphControl graphControl = NewGraphControl();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is INode node ? ConvertImpl(node) : null;

    private object ConvertImpl(INode prototype) {
      var context = graphControl.CanvasContext;
      var graph = graphControl.Graph;
      graph.Clear();

      var node = graph.CreateNode(prototype.Layout.ToRectD(), prototype.Style, prototype.Tag);
      var bounds = node.Style.Renderer.GetBoundsProvider(node, node.Style).GetBounds(context);
      foreach (var label in prototype.Labels) {
        var copy = graph.AddLabel(node, label.Text, label.LayoutParameter, label.Style, label.PreferredSize);
        bounds += copy.Style.Renderer.GetBoundsProvider(copy, copy.Style).GetBounds(context);
      }
      foreach (var port in prototype.Ports) {
        var copy = graph.AddPort(node, port.LocationParameter, port.Style);
        bounds += copy.Style.Renderer.GetBoundsProvider(copy, copy.Style).GetBounds(context);
      }

      graphControl.FitGraphBounds();
      return new UIElementExporter(graphControl.ContentBounds).Export(graphControl);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    private static GraphControl NewGraphControl() {
      var graphControl = new GraphControl();
      graphControl.AnimatedViewportChanges = ViewportChanges.None;
      return graphControl;
    }
  }
}
