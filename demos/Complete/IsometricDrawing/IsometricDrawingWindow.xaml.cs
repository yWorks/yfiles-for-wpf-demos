/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Input;
using System.Windows.Media;
using Demo.yFiles.Complete.IsometricDrawing.Model;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Orthogonal;
using Geometry = Demo.yFiles.Complete.IsometricDrawing.Model.Geometry;

namespace Demo.yFiles.Complete.IsometricDrawing
{
  /// <summary>
  /// This demo displays graphs in an isometric fashion to create an impression of a 3-dimensional view.
  /// </summary>
  public partial class IsometricDrawingWindow
  {
    private const string LayoutHierarchic = "Hierarchic";
    private const string LayoutOrthogonal = "Orthogonal";
    private String layoutType = LayoutHierarchic;

    // A flag that signals whether or not a layout is currently running to prevent re-entrant layout calculations.
    private bool layoutRunning;

    private const double MinimumNodeHeight = 3;
    
    /// <summary>
    /// Called upon loading of the form.
    /// </summary>
    private async void OnLoad(object sender, EventArgs e) {
      // set an isometric projection and a GraphModelManager that considers the isometric render order
      InitializeProjection();
      
      // configure the GraphML loading
      InitializeLoadingFiles();

      // enable and configure folding support
      InitializeFolding();

      // initialize interaction
      InitializeInputMode();

      // add a grid visual as background
      InitializeGridVisual();

      await LoadInitialGraph();
    }

    /// <summary>
    /// Sets an isometric projection and a GraphModelManager that considers the isometric render order.
    /// </summary>
    private void InitializeProjection() {
      // set an isometric projection
      graphControl.Projection = Projections.Isometric;
      // use a graph model manager that renders the nodes in their correct z-order
      graphControl.GraphModelManager = new IsometricGraphModelManager(graphControl) {
          HierarchicNestingPolicy = HierarchicNestingPolicy.GroupNodes,
          LabelLayerPolicy = LabelLayerPolicy.AtOwner,
      };
    }

    /// <summary>
    /// Enables and configures folding.
    /// </summary>
    private void InitializeFolding() {
      var manager = new FoldingManager(graphControl.Graph) {
          FolderNodeConverter = new DefaultFolderNodeConverter {
              CopyFirstLabel = true,
              CloneNodeStyle = true,
              FolderNodeSize = new SizeD(210, 120),
              FolderNodeStyle = new CollapsibleNodeStyleDecorator(new IsometricGroupNodeStyle()) {
                  ButtonPlacement = InteriorLabelModel.SouthWest
              }
          },
          FoldingEdgeConverter = new DefaultFoldingEdgeConverter { CopyFirstLabel = true }
      };

      graphControl.Graph = manager.CreateFoldingView().Graph;
      graphControl.Graph.IsGroupNodeChanged += (sender, args) => AdaptGroupNodes();
    }
    
    /// <summary>
    /// Adapt the group node height and colors: group nodes should be flat and use a pen for the bounds.
    /// </summary>
    private void AdaptGroupNodes() {
      var graph = graphControl.Graph;
      foreach (var groupNode in graph.Nodes.Where(n => graph.IsGroupNode(n))) {
        var nodeData = groupNode.Tag as NodeData;
        nodeData.Geometry.Height = 0;
        nodeData.Color = Color.FromArgb(128, 202, 236, 255);
        nodeData.Pen = IsometricGroupNodeStyle.BorderPen;
      }
      graphControl.Invalidate();
    }

    private readonly Random random = new Random();
    
    /// <summary>
    /// Ensures that the node has geometry and color information present in its tag.
    /// </summary>
    /// <param name="node">The node to check.</param>
    private void EnsureNodeTag(INode node) {
      var nodeData = node.Tag as NodeData;
      if (nodeData == null) {
        nodeData = new NodeData {
            Color = Color.FromArgb(255, (byte) random.Next(256), (byte) random.Next(256), (byte) random.Next(256))
        };
        node.Tag = nodeData;
      }
      if (nodeData.Geometry == null) {
        nodeData.Geometry = new Geometry {
            Height = MinimumNodeHeight + Math.Round(random.NextDouble() * 30)
        };
      }
    }

    /// <summary>
    /// Initialize interaction including snapping and orthogonal edge editing.
    /// </summary>
    private void InitializeInputMode() {
      // initialize interaction
      var geim = new GraphEditorInputMode() {
          NavigationInputMode = {
              FitContentAfterGroupActions = false, 
              AutoGroupNodeAlignmentPolicy = NodeAlignmentPolicy.BottomLeft 
          }
      };

      // we use orthogonal edge editing and snapping, both very helpful for editing in isometric views
      geim.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext();
      geim.SnapContext = new GraphSnapContext();

      // allow expand/collapse for group nodes
      geim.AllowGroupingOperations = true;

      // add listeners to invoke an incremental layout when collapsing/expanding a group
      geim.NavigationInputMode.GroupCollapsed += (source, args) => RunLayout(args.Item);
      geim.NavigationInputMode.GroupExpanded += (source, args) => RunLayout(args.Item);
      
      // ensure that every node has geometry and color information
      geim.NodeCreated += ((sender, evt) => {
        EnsureNodeTag(evt.Item);
        if (graphControl.Graph.IsGroupNode(evt.Item)) {
          AdaptGroupNodes();
        }
      });
      graphControl.InputMode = geim;

      // changing a node's layout may require to adjust its render order
      graphControl.Graph.NodeLayoutChanged += (source, n, layout) => {
        graphControl.GraphModelManager.Update(n);
      };
      
      // add handle that enables the user to change the height of a (non-group) node
      graphControl.Graph.GetDecorator().NodeDecorator.HandleProviderDecorator.SetImplementationWrapper(
          n => {
            var foldingView = graphControl.Graph.GetFoldingView();
            return !foldingView.Manager.MasterGraph.IsGroupNode(foldingView.GetMasterItem(n));
          },
          (node, delegateProvider) => new HeightHandleProvider(node, delegateProvider, MinimumNodeHeight)
      );
    }

    /// <summary>
    /// Initializes the visualization of the grid feature.
    /// </summary>
    private void InitializeGridVisual() {
      var grid = new GridVisualCreator(new GridInfo()) {
          GridStyle = GridStyle.Lines,
          Pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 210, 210, 210)), 0.1),
          VisibilityThreshold = 10,
      };
      graphControl.BackgroundGroup.AddChild(grid, CanvasObjectDescriptors.AlwaysDirtyInstance);
    }

    #region Layout

    /// <summary>
    /// Invokes a layout specified by the current layout type. 
    /// </summary>
    /// <remarks>
    /// If there is a fixed node, the layout is calculated incrementally.
    /// </remarks>
    /// <param name="fixedNode">If defined, the layout will be incrementally and this node remains at its location.</param>
    public async Task RunLayout(INode fixedNode = null) {
      if (layoutRunning) {
        return;
      }
      layoutRunning = true;
      var incremental = fixedNode != null;

      // configure layout
      var layout = layoutType == LayoutHierarchic ? (ILayoutAlgorithm) GetHierarchicLayout(incremental) : GetOrthogonalLayout();
      var layoutData = layoutType == LayoutHierarchic ? (LayoutData) GetHierarchicLayoutData() : GetOrthogonalLayoutData();

      if (incremental) {
        // fixate the location of the given fixed node
        layout = new FixNodeLayoutStage(layout) { FixPointPolicy = FixPointPolicy.LowerLeft };
        layoutData = new CompositeLayoutData(layoutData,
            new FixNodeLayoutData { FixedNodes = new ItemCollection<INode> { Item = fixedNode } }
        );
      }

      EnableUI(false);

      // configure layout execution to not move the view port
      var executor = new LayoutExecutor(graphControl, layout) {
          LayoutData = layoutData, 
          AnimateViewport = !incremental, 
          Duration = TimeSpan.FromMilliseconds(500), 
          UpdateContentRect = true
      };

      // start layout
      await executor.Start();
      layoutRunning = false;
      EnableUI(true);
    }

    /// <summary>
    /// Creates a configured hierarchic layout data.
    /// </summary>
    /// <param name="incremental"><code>true</code> in case the layout should be calculated incrementally.</param>
    public HierarchicLayout GetHierarchicLayout(bool incremental) {
      var layout = new HierarchicLayout {
          OrthogonalRouting = true,
          NodeToEdgeDistance = 50,
          MinimumLayerDistance = 40,
          LabelingEnabled = false,
          IntegratedEdgeLabeling = true,
          ConsiderNodeLabels = true,
          GridSpacing = 10
      };

      if (incremental) {
        layout.LayoutMode = LayoutMode.Incremental;
      }
      return layout;
    }

    /// <summary>
    /// Creates a configured hierarchic layout data.
    /// </summary>
    public HierarchicLayoutData GetHierarchicLayoutData() {
      // use preferred placement descriptors to place the labels vertically on the edges
      var layoutData = new HierarchicLayoutData {
          EdgeLabelPreferredPlacement = new ItemMapping<ILabel, PreferredPlacementDescriptor> { Constant = GetPreferredLabelPlacement() },
          IncrementalHints = new IncrementalHintItemMapping {
              ContextDelegate = (item, hintsFactory) =>
                  item is IEdge ? hintsFactory.CreateSequenceIncrementallyHint(item) : null
          }
      };
      return layoutData;
    }

    /// <summary>
    /// Creates a configured orthogonal layout.
    /// </summary>
    public OrthogonalLayout GetOrthogonalLayout() {
      return new OrthogonalLayout {
          IntegratedEdgeLabeling = true, 
          ConsiderNodeLabels = true,
          GridSpacing = 10
      };
    }

    /// <summary>
    /// Creates a configured orthogonal layout data.
    /// </summary>
    public OrthogonalLayoutData GetOrthogonalLayoutData() {
      return new OrthogonalLayoutData() {
          EdgeLabelPreferredPlacement = new ItemMapping<ILabel, PreferredPlacementDescriptor> {
              Constant = GetPreferredLabelPlacement()
          }
      };
    }

    private PreferredPlacementDescriptor GetPreferredLabelPlacement() {
      return new PreferredPlacementDescriptor {
          Angle = 0,
          AngleReference = LabelAngleReferences.RelativeToEdgeFlow, 
          SideOfEdge = LabelPlacements.LeftOfEdge, 
          SideReference = LabelSideReferences.AbsoluteWithRightInNorth
      };
    }
    
    #endregion

    /// <summary>
    /// Loads a graph from <see cref="IsometricData"/> using a <see cref="MultiGraphBuilder"/> and initializes
    /// all styles and isometric data.
    /// </summary>
    /// <remarks>
    /// The graph also gets an initial layout.
    /// </remarks>
    private async Task LoadInitialGraph() {
      var graph = graphControl.Graph;

      graph.NodeDefaults.Style = new IsometricNodeStyle();
      graph.NodeDefaults.Labels.LayoutParameter = ExteriorLabelModel.South;
      graph.GroupNodeDefaults.Style = new CollapsibleNodeStyleDecorator(new IsometricGroupNodeStyle()) { ButtonPlacement = InteriorLabelModel.SouthWest, };
      graph.GroupNodeDefaults.Labels.Style = new DefaultLabelStyle { TextAlignment = TextAlignment.Right, Insets = new InsetsD(3) };
      graph.GroupNodeDefaults.Labels.LayoutParameter = InteriorLabelModel.SouthEast;
      graph.EdgeDefaults.Labels.Style = new DefaultLabelStyle { Insets = new InsetsD(3) };
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgeSegmentLabelModel() { AutoRotation = true }.CreateDefaultParameter();

      var graphBuilder = new GraphBuilder(graph);
      var nodesSource = graphBuilder.CreateNodesSource(IsometricData.NodesData, item => item.Id);
      nodesSource.ParentIdProvider = item => item.Group;
      nodesSource.NodeCreator.LayoutProvider = item => new RectD(0, 0, item.Geometry.Width, item.Geometry.Depth);
      nodesSource.NodeCreator.CreateLabelBinding(item => item.Label);

      var groupNodesSource = graphBuilder.CreateGroupNodesSource(IsometricData.GroupsData, item => item.Id);
      groupNodesSource.ParentIdProvider = item => item.Group;
      groupNodesSource.NodeCreator.LayoutProvider = item => new RectD(0, 0, item.Geometry.Width, item.Geometry.Depth);
      groupNodesSource.NodeCreator.CreateLabelBinding(item => item.Label);

      var edgesSource = graphBuilder.CreateEdgesSource(IsometricData.EdgesData, item => item.From, item => item.To);
      edgesSource.EdgeCreator.CreateLabelBinding(item => item.Label);

      graphBuilder.BuildGraph();

      await RunLayout();
    }

    /// <summary>
    /// Configures the <see cref="GraphMLIOHandler"/> used to load the graph.
    /// </summary>
    private void InitializeLoadingFiles() {
      // ignore deserialization errors when loading graphs that use different styles
      // the styles will be replaced with isometric styles later
      graphControl.GraphMLIOHandler.DeserializationPropertyOverrides.Set(
          SerializationProperties.IgnoreXamlDeserializationErrors,
          true
      );
      graphControl.GraphMLIOHandler.Parsing += (sender, args) => EnableUI(false);
      graphControl.GraphMLIOHandler.Parsed += (sender, args) => {
        // after loading apply isometric styles and geometry to the nodes and labels
        ApplyIsometricStyles();
        RunLayout();
      };
    }

    private void OnHLLayoutClick(object sender, RoutedEventArgs e) {
      layoutType = LayoutHierarchic;
      RunLayout();
    }

    private void OnOTLayoutClick(object sender, RoutedEventArgs e) {
      layoutType = LayoutOrthogonal;
      RunLayout();
    }

    /// <summary>
    /// Adds isometric styles and geometry data to nodes and labels of the graph.
    /// </summary>
    /// <remarks>
    /// Also free label and port location models are applied to retrieve the correct positions calculated by the
    /// layout algorithm.
    /// </remarks>
    public void ApplyIsometricStyles() {
      var foldingManager = graphControl.Graph.GetFoldingView().Manager;
      var graph = foldingManager.MasterGraph;
      foreach (var node in graph.Nodes) {
        var group = graph.IsGroupNode(node);
        UpdateGeometry(node, node.Layout.GetSize(), group ? 0 : 20);
        if (group) {
          ((NodeData) node.Tag).Color = Color.FromArgb(128, 202, 236, 255);
          ((NodeData) node.Tag).Pen = IsometricGroupNodeStyle.BorderPen;
          graph.SetStyle(node, new CollapsibleNodeStyleDecorator(new IsometricGroupNodeStyle()) { ButtonPlacement = InteriorLabelModel.SouthWest, });
          var folderNodeState = foldingManager.GetFolderNodeState(node);
          folderNodeState.Style = new CollapsibleNodeStyleDecorator(new IsometricGroupNodeStyle()) { ButtonPlacement = InteriorLabelModel.SouthWest, };
          var firstLabel = node.Labels.FirstOrDefault();
          if (firstLabel != null) {
            var layout = firstLabel.GetLayout();
            UpdateGeometry(firstLabel, layout.GetSize(), layout.Height, new InsetsD(3));
            graph.SetStyle(firstLabel, new DefaultLabelStyle() { TextAlignment = TextAlignment.Right, Insets = new InsetsD(3) });
            graph.SetLabelLayoutParameter(firstLabel, graph.GroupNodeDefaults.Labels.LayoutParameter);
          }
          var firstFolderLabel = folderNodeState.Labels.FirstOrDefault();
          if (firstFolderLabel != null) {
            var label = firstFolderLabel.AsLabel();
            var layout = label.GetLayout();
            UpdateGeometry(label, layout.GetSize(), layout.Height, new InsetsD(3));
            firstFolderLabel.Style = new DefaultLabelStyle { TextAlignment = TextAlignment.Right, Insets = new InsetsD(3) };
            firstFolderLabel.LayoutParameter = graph.GroupNodeDefaults.Labels.LayoutParameter;
          }
        } else {
          ((NodeData) node.Tag).Color = Color.FromArgb(255, 255, 153, 0);
          graph.SetStyle(node, new IsometricNodeStyle());
        }
      }
      foreach (var edge in graph.Edges) {
        graph.SetStyle(edge, new PolylineEdgeStyle());
        foreach (var label in edge.Labels) {
          var layout = label.GetLayout();
          UpdateGeometry(label, layout.GetSize(), layout.Height, new InsetsD(3));
          graph.SetStyle(label, new DefaultLabelStyle { Insets = new InsetsD(3) });
          graph.SetLabelLayoutParameter(label, new EdgeSegmentLabelModel { AutoRotation = true }.CreateDefaultParameter());
        }
      }
      foreach (var port in graph.Ports) {
        graph.SetPortLocationParameter(port, FreeNodePortLocationModel.NodeCenterAnchored);
      }
    }

    /// <summary>
    /// Updates the tag of the given item with geometry data. 
    /// </summary>
    /// <remarks>
    /// In case the tag already contains valid geometry data, it stays unchanged.
    /// </remarks>
    /// <param name="item">The item for which the tag is updated.</param>
    /// <param name="layout">The 2D-geometry for the item.</param>
    /// <param name="height">The height of the resulting solid figure.</param>
    /// <param name="insets">Insets that are added to the layout information to create a padding.</param>
    private void UpdateGeometry(IModelItem item, SizeD layout, double height, InsetsD? insets = null) {
      var nodeData = item.Tag as NodeData;
      if (nodeData != null && nodeData.Geometry != null) {
        return;
      }
      var inset = insets ?? InsetsD.Empty;
      var geometry = new Geometry {
          Width = layout.Width + inset.Left + inset.Right, 
          Height = height, 
          Depth = layout.Height + inset.Top + inset.Bottom
      };
      if (nodeData != null) {
        nodeData.Geometry = geometry;
      } else {
        item.Tag = new NodeData { Geometry = geometry };
      }
    }

    /// <summary>
    /// Enables or disables the buttons in the toolbar.
    /// </summary>
    /// <param name="enabled">Whether to enable or disable the buttons.</param>
    private void EnableUI(bool enabled) {
      openBtn.IsEnabled = enabled;
      printBtn.IsEnabled = enabled;
      hlLayoutButton.IsEnabled = enabled;
      otLayoutButton.IsEnabled = enabled;
    }

    private void OnRotationSldChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      graphControl.Projection = new TransformGroup { Children = {
          new RotateTransform(e.NewValue), 
          Projections.Isometric
      } };
      // update z-order of model items according to new rotation
      ((IsometricGraphModelManager) graphControl.GraphModelManager).Update();
      graphControl.Invalidate();
    }
  }
}
