/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Router;

[assembly: XmlnsDefinition("http://www.yworks.com/yfilesWPF/2.2/demos/BusRouterWindow", "Demo.yFiles.Layout.BusRouterDemo")]
[assembly: XmlnsPrefix("http://www.yworks.com/yfilesWPF/2.2/demos/BusRouterWindow", "demo")]
namespace Demo.yFiles.Layout.BusRouterDemo
{
  /// <summary>
  /// This Demo shows how to use a <see cref="BusRouter"/> as layout.
  /// </summary>
  public partial class BusRouterWindow
  {
    private readonly BusRouter layout = new BusRouter {
      Scope = Scope.RouteAllEdges,
      MinimumDistanceToNode = 10,
      MinimumDistanceToEdge = 5,
      PreferredBackboneSegmentCount = 1,
      CrossingCost = 1,
    };

    #region Initialization

    public BusRouterWindow() {
      InitializeComponent();
    }

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected async void OnLoaded(object source, RoutedEventArgs e) {
      // initialize the input mode
      InitializeInputModes();

      // initialize the graph
      await InitializeGraph();
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual async Task InitializeGraph() {
      DemoStyles.InitDemoStyles(graphControl.Graph);
      graphControl.Graph.NodeDefaults.Size = new SizeD(50, 50);
      graphControl.ImportFromGraphML(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase), "Resources\\default.graphml"));
      await DoLayout(Scope.RouteAllEdges);
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <remarks>
    /// The control uses a custom node creation callback that creates business objects for newly
    /// created nodes.
    /// </remarks>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      var mode = new GraphEditorInputMode {
        // disable node resizing
        ShowHandleItems = GraphItemTypes.Bend | GraphItemTypes.Edge,
        // don't allow edges to be created by the user
        AllowCreateEdge = false,
        // enable orthogonal edge editing
        OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext(),
        // enable marquee selection for nodes only
        MarqueeSelectableItems = GraphItemTypes.Node,
        // enable context snapping
        SnapContext = new GraphSnapContext()
      };
      return mode;
    }

    #endregion

    #region Event handlers

    /// <summary>
    /// Exits the demo.
    /// </summary>
    private void ExitMenuItemClick(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    /// <summary>
    /// Formats the current graph.
    /// </summary>
    private async void OnLayoutClick(object sender, EventArgs e) {
      await DoLayout(Scope.RouteAllEdges);
    }

    /// <summary>
    /// Creates the edges between all selected nodes, resulting in a complete subgraph.
    /// </summary>
    private async void OnConnectNodesClick(object sender, RoutedEventArgs e) {
      var graph = graphControl.Graph;

      // find the first "Network" node, if any
      var selectedNodes = new HashSet<INode>(graph.Nodes.Where(graphControl.Selection.IsSelected));
      if(selectedNodes.Count == 0) {
        return;
      }

      edgesToRoute.Clear();

      // iterate over all selected nodes to see if we need to create a new edge or modify the old one
      var edgeStyle = new PolylineEdgeStyle {Pen = PenStyles.GetRandomPen()};
      
      
      foreach (var node in selectedNodes) {
        //Create a complete subgraph
        foreach (var otherNode in selectedNodes) {
          if(otherNode != node) {
            edgesToRoute.Add(graph.CreateEdge(node, otherNode, edgeStyle));
          }
        }
      }
      graphControl.Invalidate();
      await DoLayout(Scope.RouteAffectedEdges);
    }

    #endregion

    /// <summary>
    /// mark all edges that should be rerouted
    /// </summary>
    private readonly HashSet<IEdge> edgesToRoute = new HashSet<IEdge>();

    /// <summary>
    /// Perform the layout operation
    /// </summary>
    private async Task DoLayout(Scope scope) {
      // layout starting, disable button
      layoutButton.IsEnabled = false;

      var busRouterData = new BusRouterData
      {
        EdgeDescriptors = { Delegate = edge => new BusDescriptor(((PolylineEdgeStyle) edge.Style).Pen)}
      };

      // layout applies only to selected subset of edges
      if (scope == Scope.RouteAffectedEdges) {
        busRouterData.AffectedEdges.Items = edgesToRoute;
      }

      // tell the layout algorithm about the scope it should operate in
      layout.Scope = scope;

      // do the layout
      await graphControl.MorphLayout(layout, TimeSpan.FromSeconds(1), busRouterData);
      // layout finished, enable layout button again
      layoutButton.IsEnabled = true;
    }

    private async void OnRefreshButtonClicked(object sender, RoutedEventArgs e) {
      await InitializeGraph();
    }
  }

  /// <summary>
  /// Contains predefined <see cref="Pen"/>s.
  /// </summary>
  public static class PenStyles
  {
    /// <summary>
    /// Contains all <see cref="Pen"/>s that can be used.
    /// </summary>
    private static readonly Pen[] Values;
    private static readonly Random Random = new Random();

    static PenStyles() {
      Values = new[] {
        Color.FromRgb(0xAB, 0x23, 0x46), // sample graph edge color
        Color.FromRgb(0x66, 0x2b, 0x00), // sample graph edge color
        Color.FromRgb(0x0B, 0x71, 0x89), // sample graph edge color
        Color.FromRgb(0x11, 0x1D, 0x4A),
        Color.FromRgb(0x17, 0xBE, 0xBB),
        Color.FromRgb(0xFF, 0xC9, 0x14),
        Color.FromRgb(0xFF, 0x6C, 0x00),
        Color.FromRgb(0x2E, 0x28, 0x2A),
        Color.FromRgb(0x76, 0xB0, 0x41),
      }.Select(color => new Pen(new SolidColorBrush(color), 2)).ToArray();
    }

    private static int index;

    /// <summary>
    /// Gets a random <see cref="Pen"/>.
    /// </summary>
    /// <returns>A random <see cref="Pen"/> object</returns>
    public static Pen GetRandomPen() {
      // no more predefined values, generate default (dark) color
      if(Values.Length <= index) {
        return new Pen(new SolidColorBrush(
          Color.FromRgb((byte) Random.Next(150), (byte) Random.Next(150), (byte) Random.Next(150))), 2);
      }
      // use next predefined value
      return Values[index++];
    }
  }
}
