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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Neo4j.Driver;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Organic;
using yWorks.Layout.Router;
using INode = yWorks.Graph.INode;
using INeo4jNode = Neo4j.Driver.INode;
using Scope = yWorks.Layout.Organic.Scope;
using yWorks.Layout.Labeling;
using yWorks.Controls;
using System.Collections;
using System.Diagnostics;
using System.Windows.Navigation;
using Demo.yFiles.Toolkit;

namespace Neo4JIntegration
{
  /// <summary>
  /// Interaction logic for Neo4JIntegrationDemoWindow.xaml
  /// </summary>
  public partial class Neo4JIntegrationDemo
  {

    private GraphBuilder graphBuilder;
    private NodesSource<INeo4jNode> nodesSource;
    private EdgesSource<IRelationship> edgesSource;
    private List<INeo4jNode> nodes;
    private List<IRelationship> edges;
    /// <summary>Nodes that should be laid out incrementally.</summary>
    private readonly HashSet<INode> incrementalNodes = new HashSet<INode>();

    /// <summary>
    /// Database connection information.
    /// </summary>
    public DBInformation ConnectionInfo { get; } = new DBInformation
    {
      // Set some default values for the database connection
      Url = "neo4j+s://demo.neo4jlabs.com",
      Db = "movies",
      Username = "movies",
      Password = "movies"
    };

    /// <summary>
    /// Database session for queries to the Neo4J database.
    /// </summary>
    private static IAsyncSession Session { get; set; }

    private void OnLoaded(object source, EventArgs args) {
      // Configure interaction and context menu
      var gvim = new GraphViewerInputMode { SelectableItems = GraphItemTypes.None };

      gvim.ContextMenuItems = GraphItemTypes.Node;
      gvim.PopulateItemContextMenu += (sender, e) => {
        if (e.Item is INode node) {
          // Add a context menu entry; it will load all neighbors to the graph
          var showNeighbors = new MenuItem() { Header = "Show neighbors" };
          showNeighbors.Click += async delegate { await LoadNeighbors(node); };

          var shortestPath = new MenuItem() { Header = "Calculate shortest path" };
          shortestPath.Click += delegate { ShowPathViewer(node); };

          e.Menu.Items.Add(showNeighbors);
          e.Menu.Items.Add(shortestPath);
          e.ShowMenu = true;
          e.Handled = true;
        }
      };

      graphControl.InputMode = gvim;

      graphControl.Center = PointD.Origin;
    }

    /// <summary>
    /// Loads neighbors of the given node and shows them in the graph.
    /// </summary>
    private async Task LoadNeighbors(INode n) {
      var id = ((INeo4jNode) n.Tag).Id;
      incrementalNodes.Clear();
      incrementalNodes.Add(n);
      try {
        await LoadGraphAsync(
            $"MATCH (n1)-[r]-(n2) WHERE id(n1) = {id} AND NOT type(r) = 'SIMILAR_JACCARD' RETURN n1,r,n2",
            ConnectionInfo, false, n.Layout.GetCenter());
      } catch (Exception e) {
        MessageBox.Show("Could not load the graph. Please check the database or the query.\n\n" + e.Message);
        ConnectionInfoPanel.Visibility = Visibility.Visible;
      }
    }

    /// <summary>
    /// Shows a separate window that allows to visualize the shortest path between any two nodes.
    /// </summary>
    private async void ShowPathViewer(INode n) {
      // Create a small window to choose the target for the shortest path search
      var dialog = new ShortestPathDialog(n, Graph.Nodes.ToList());
      if (dialog.ShowDialog() == true) {
        try {
          var (nodes, edges) = await LoadDataFromDatabaseAsync(
            $"MATCH (n1),(n2), path = shortestPath((n1)-[*..100]-(n2)) WHERE id(n1) = {dialog.FromItem.Id} AND id(n2) = {dialog.ToItem.Id} RETURN path",
            ConnectionInfo);

          if (nodes.Count == 0) {
            MessageBox.Show("No path found!");
            return;
          }

          var pathViewer = new PathViewer(nodes, edges);
          pathViewer.Show();

        } catch (Exception e) {
          MessageBox.Show("Could not load the graph. Please check the database or the query.\n\n" + e.Message);
          ConnectionInfoPanel.Visibility = Visibility.Visible;
        }
      }
    }

    /// <summary>
    /// Connect to a database and show an initial set of nodes and edges.
    /// </summary>
    private async void Connect_OnClick(object sender, RoutedEventArgs e) {
      try {
        ConnectionInfoPanel.Visibility = Visibility.Collapsed;
        graphControl.Visibility = Visibility.Visible;
        GraphOverviewControl.Visibility = Visibility.Visible;

        StartSession(ConnectionInfo);

        // Load a few nodes and edges – We exclude similarity measures here, since they clutter up the graph fairly quickly
        var startQuery = "MATCH (n1)-[r]->(n2) WHERE NOT type(r) = 'SIMILAR_JACCARD' RETURN n1,r,n2 LIMIT 20";
        await LoadGraphAsync(startQuery, ConnectionInfo, true);
      } catch (Exception ex) {
        MessageBox.Show($"Could not load the graph. Please check the database or the query.\n\n{ex.Message}");
        ConnectionInfoPanel.Visibility = Visibility.Visible;
      }
    }

    /// <summary>
    /// Run a custom query and visualize the result.
    /// </summary>
    private async void RunQuery_OnClick(object sender, RoutedEventArgs e) {
      loadingIndicator.Visibility = Visibility.Visible;
      try {
        await LoadGraphAsync(QueryTextBox.Text, ConnectionInfo, ReplaceGraphCheckbox.IsChecked == true);
      } catch (Exception ex) {
        MessageBox.Show("Could not load the graph. Please check the database or the query.\n\n" + ex.Message);
      }
    }

    /// <summary>
    /// Executes a query and populates the graph with the result.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="dbInformation">The connection information.</param>
    /// <param name="clearGraph">A value indicating whether to replace the graph with the result of the query.</param>
    /// <param name="desiredLocation">An optional location where newly-created nodes should appear.</param>
    private async Task LoadGraphAsync(string query, DBInformation dbInformation, bool clearGraph = false, PointD desiredLocation = default) {
      try {
        loadingIndicator.Visibility = Visibility.Visible;
        var (nodes, edges) = await LoadDataFromDatabaseAsync(query, dbInformation);

        // Finally, with the new data, build the graph
        await BuildGraphAsync(nodes, edges, clearGraph, desiredLocation);
      } finally {
        loadingIndicator.Visibility = Visibility.Collapsed;
      }
    }

    /// <summary>
    /// Fetches nodes and edges from the database
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="dbInformation">The connection information.</param>
    /// <returns>A tuple of nodes and edges returned from the database.</returns>
    public static async Task<(List<INeo4jNode>, List<IRelationship>)> LoadDataFromDatabaseAsync(string query, DBInformation dbInformation) {
      //Connect to database
      if (Session == null) {
        StartSession(dbInformation);
      }

      //Ask for the result
      var result = await Session.RunAsync(query);

      // Collect nodes and edges from the result
      // Items may appear multiple times, so we need to remove duplicates based on their ID
      var nodes = new HashSet<INeo4jNode>(IdEqualityComparer<INeo4jNode>.Instance);
      var edges = new HashSet<IRelationship>(IdEqualityComparer<IRelationship>.Instance);

      foreach (var record in await result.ToListAsync()) {
        foreach (var kv in record.Values) {
          // This may be in a variety of formats (lists, paths, nodes, edges, ...)
          // so it's a bit more complicated to get at the actual elements
          Unpack(kv.Value, nodes, edges);
        }
      }

      return (nodes.ToList(), edges.ToList());
    }

    /// <summary>
    /// Unpack potentially nested values from the result into a set of nodes and edges.
    /// </summary>
    /// <param name="value">The value to recursively unpack.</param>
    /// <param name="nodes">A set of nodes to populate.</param>
    /// <param name="edges">A set of edges to populate.</param>
    private static void Unpack(object value, HashSet<INeo4jNode> nodes, HashSet<IRelationship> edges) {
      switch (value) {
        case INeo4jNode node:
          nodes.Add(node);
          break;
        case IRelationship edge:
          edges.Add(edge);
          break;
        case IEnumerable items:
          foreach (var item in items) {
            Unpack(item, nodes, edges);
          }
          break;
        case IPath path:
          foreach (var node in path.Nodes) {
            nodes.Add(node);
          }
          foreach (var edge in path.Relationships) {
            edges.Add(edge);
          }
          break;
      }
    }

    /// <summary>
    /// Opens the database connection.
    /// </summary>
    /// <param name="dbInformation"></param>
    private static void StartSession(DBInformation dbInformation) {
      var authToken = AuthTokens.Basic(dbInformation.Username, dbInformation.Password);
      var driver = GraphDatabase.Driver(dbInformation.Url, authToken);
      Session =
        dbInformation.Db == "default"
          ? driver.AsyncSession(builder => builder.WithDefaultAccessMode(AccessMode.Read))
          : driver.AsyncSession(builder => builder.WithDefaultAccessMode(AccessMode.Read).WithDatabase(dbInformation.Db));
    }

    /// <summary>
    /// Constructs the yFiles graph from the database results.
    /// </summary>
    /// <param name="nodes">A list of nodes from the database.</param>
    /// <param name="edges">A list of edges from the database.</param>
    /// <param name="clearGraph">A value indicating whether the current graph should be replaced.</param>
    /// <param name="initialLocation">The initial location of newly-created nodes.</param>
    private async Task BuildGraphAsync(List<INeo4jNode> nodes, List<IRelationship> edges, bool clearGraph, PointD initialLocation) {
      if (graphBuilder == null || clearGraph) {
        Graph.Clear();
        this.nodes = nodes;
        this.edges = edges;
        (graphBuilder, nodesSource, edgesSource) = CreateGraphBuilder(graphControl, Graph, nodes, edges);
        // Move new nodes back in z-order to make it look like they show up from underneath the neighboring node
        graphBuilder.NodeCreated += (sender, e) => graphControl.GraphModelManager.GetCanvasObject(e.Item).ToBack();

      } else {
        // Add new data to existing data
        nodes = this.nodes.Concat(nodes).Distinct(IdEqualityComparer<INeo4jNode>.Instance).ToList();
        edges = this.edges.Concat(edges).Distinct(IdEqualityComparer<IRelationship>.Instance).ToList();
        this.nodes.Clear();
        this.nodes.AddRange(nodes);
        this.edges.Clear();
        this.edges.AddRange(edges);
      }

      nodesSource.NodeCreator.LayoutProvider = n => RectD.FromCenter(initialLocation, Graph.NodeDefaults.Size);

      // Update the graph and detect which nodes have been added
      var existingNodes = new HashSet<INode>(Graph.Nodes);
      var existingEdges = new HashSet<IEdge>(Graph.Edges);

      graphBuilder.UpdateGraph();

      var newNodes = Graph.Nodes.Except(existingNodes).ToList();
      var newEdges = Graph.Edges.Except(existingEdges).ToList();
      foreach (var node in newNodes) {
        incrementalNodes.Add(node);
      }
      foreach (var edge in newEdges) {
        incrementalNodes.Add(edge.GetSourceNode());
        incrementalNodes.Add(edge.GetTargetNode());
      }

      loadingIndicator.Visibility = Visibility.Collapsed;

      // Layout the graph if there are new items
      if (newNodes.Count > 0 || newEdges.Count > 0) {
        await ApplyLayout(incrementalNodes);
      }
    }

    /// <summary>
    /// Creates a GraphBuilder instance that's preconfigured with our demo's styles.
    /// </summary>
    internal static (GraphBuilder, NodesSource<INeo4jNode>, EdgesSource<IRelationship>) CreateGraphBuilder(GraphControl graphControl, IGraph graph, List<INeo4jNode> nodes, List<IRelationship> edges) {
      var builder = new GraphBuilder(graph);
      var nodesSource = builder.CreateNodesSource(nodes, n => n.Id);
      nodesSource.NodeCreator.TagProvider = n => n;
      var nodeStyle = new NodeControlNodeStyle("NodeStyle");
      nodesSource.NodeCreator.Defaults.Style = nodeStyle;
      var edgesSource = builder.CreateEdgesSource(edges, e => e.StartNodeId, e => e.EndNodeId, e => e.Id);
      var style58 = Themes.Palette58;
      var demoEdgeStyle = DemoStyles.CreateDemoEdgeStyle(style58);
      edgesSource.EdgeCreator.Defaults.Style = new BezierEdgeStyle { TargetArrow = demoEdgeStyle.TargetArrow, Pen = demoEdgeStyle.Pen };
      var labelBinding = edgesSource.EdgeCreator.CreateLabelBinding(item => item.Type);
      labelBinding.Defaults.LayoutParameter = new EdgeSegmentLabelModel().CreateParameterFromSource(0, 0, EdgeSides.AboveEdge);
      labelBinding.Defaults.Style = DemoStyles.CreateDemoEdgeLabelStyle(style58);

      var context = graphControl.CreateRenderContext();
      builder.NodeCreated +=
        (sender, e) => {
          // Ensure that nodes have the correct size
          e.Graph.SetNodeLayout(e.Item, RectD.FromCenter(e.Item.Layout.GetCenter(), nodeStyle.GetPreferredSize(context, e.Item)));
        };
      return (builder, nodesSource, edgesSource);
    }

    /// <summary>
    /// Lays out the graph with nodes that are laid out incrementally.
    /// </summary>
    /// <param name="incrementalNodes">Nodes that should be inserted into the graph incrementally.</param>
    private async Task ApplyLayout(HashSet<INode> incrementalNodes) {
      toolBar.IsEnabled = false;

      // We'll use an OrganicLayout with OrganicEdgeRouter to get a pleasing image for most databases
      var layout = new OrganicLayout
      {
        NodeEdgeOverlapAvoided = true,
        NodeOverlapsAllowed = false,
        PreferredEdgeLength = 180,
        MinimumNodeDistance = 80,
        StarSubstructureStyle = StarSubstructureStyle.Radial,
        StarSubstructureSize = 3,
        ConsiderNodeSizes = true,
        CycleSubstructureStyle = CycleSubstructureStyle.Circular,
        StarSubstructureTypeSeparation = false,
        LabelingEnabled = true,
        ParallelEdgeRouterEnabled = true,
        Scope = incrementalNodes.Count > 0 ? Scope.MainlySubset : Scope.All
      };

      layout.PrependStage(new OrganicEdgeRouter()
      {
        ConsiderExistingBends = false,
        KeepExistingBends = false,
        EdgeNodeOverlapAllowed = false,
        RouteAllEdges = false,
        MinimumDistance = 10
      });

      // GenericLabeling ensures that labels don't overlap
      layout.PrependStage(new GenericLabeling { RemoveNodeOverlaps = true });

      // CurveFittingLayoutStage adds Bézier control points so we can use BezierEdgeStyle to render the edges
      layout.PrependStage(new CurveFittingLayoutStage());

      var layoutData = new CompositeLayoutData(
        new OrganicLayoutData
        {
          NodeTypes = { Delegate = node => ((INeo4jNode) node.Tag).Labels[0] },
          AffectedNodes = { Source = incrementalNodes },
        },
        new LabelingData
        {
          EdgeLabelPreferredPlacement = {
            Constant = new PreferredPlacementDescriptor
            {
              AngleReference = LabelAngleReferences.RelativeToEdgeFlow,
              PlaceAlongEdge = LabelPlacements.AtSource,
              SideOfEdge = LabelPlacements.RightOfEdge | LabelPlacements.LeftOfEdge
            }
          }
        }
        );

      await graphControl.MorphLayout(layout, TimeSpan.FromMilliseconds(700), layoutData);
      toolBar.IsEnabled = true;
    }

    public IGraph Graph => graphControl.Graph;

    public Neo4JIntegrationDemo() {
      InitializeComponent();
    }

    /// <summary>
    /// Close the database connection again when closing the window.
    /// </summary>
    private void OnUnloaded(object sender, RoutedEventArgs e) {
      //Close the session
      Session?.CloseAsync();
    }

    private void OnLinkClicked(object sender, RequestNavigateEventArgs e) {
      var startInfo = new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true };
      Process.Start(startInfo);
      e.Handled = true;
    }
  }

  /// <summary>
  /// Stores database connection information.
  /// </summary>
  public class DBInformation
  {
    public string Url { get; set; }
    public string Db { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
  }

  /// <summary>
  /// Equality comparer to deduplicate nodes and edges by their ID
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal sealed class IdEqualityComparer<T> : IEqualityComparer<T> where T : IEntity
  {
    public static readonly IdEqualityComparer<T> Instance = new IdEqualityComparer<T>();
    public bool Equals(T x, T y) => EqualityComparer<long>.Default.Equals(x.Id, y.Id);
    public int GetHashCode(T obj) => obj.Id.GetHashCode();
  }
}
