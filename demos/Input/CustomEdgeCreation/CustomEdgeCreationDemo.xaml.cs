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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CustomEdgeCreation
{
  /// <summary>Bootstraps the custom edge creation demo.</summary>
  [Demo("custom-edge-creation", "input", "Custom Edge Creation",
      Summary = "Shows how to provide directional ports and demonstrates interactive routing during edge creation.",
      Tags = ["layout", "edge routing", "interaction"], Keywords = [
          "layoutfeatures", "edge router", "channel edge router", "orthogonal", "port candidate provider", "styles",
          "ports"
      ])]
  public partial class CustomEdgeCreationDemo
  {
    private static readonly string[] NodeColors = ["#D4B483", "#C1666B", "#48A9A6", "#4357AD"];
    private static readonly Brush NodeFill = BrushFrom("#F0EBE6");
    private readonly Settings settings = new();

    public CustomEdgeCreationDemo() {
      InitializeComponent();
      DataContext = settings;
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;
      // Configure the graph defaults and keep the directional ports available as candidates.
      DemoStyles.InitDemoStyles(graph, orthogonalEdgeEditing: true);
      graph.NodeDefaults.Size = new SizeD(40, 40);

      // configure the port candidate decorator and associated input mode behavior
      InitializePortBehavior(graph);

      // edge creation should be able to finish on an empty canvas
      InitializeInputMode(true);

      // Create and fit the initial sample graph; the toolbar settings are bound to Settings.
      CreateGraph(graph);
      graphControl.FitGraphBounds();

      InitializeUI(graph);
    }

    /// <summary>
    ///   Configures the given <see cref="CreateEdgeInputMode"/> to finish on empty canvas by creating
    ///   a new node at the gesture end.
    /// </summary>
    private void EnableTargetNodeCreation(CreateEdgeInputMode createEdgeInputMode) {
      createEdgeInputMode.PreviewGraph.NodeDefaults.Size = new SizeD(40, 40);
      createEdgeInputMode.GestureStarting += (_, _) => {
        // Give each gesture's dummy target node a new random color.
        var color = RandomColor();
        var style = CreateNodeStyle(color);
        createEdgeInputMode.PreviewGraph.NodeDefaults.Style = style;
        createEdgeInputMode.PreviewGraph.SetStyle(createEdgeInputMode.PreviewEndNode, style);
        createEdgeInputMode.PreviewEndNode.Tag = color;
        // Add ports to the dummy target node.
        AddDirectionalPorts(createEdgeInputMode.PreviewGraph, createEdgeInputMode.PreviewEndNode,
            CreatePortStyle(color, settings.ShowPorts));
      };
      // Allow the gesture to finish at any location so the edge creator can add a node on canvas.
      createEdgeInputMode.PrematureEndHitTestable = HitTestables.Always;
      // Let the gesture finish where released instead of snapping to a port candidate.
      createEdgeInputMode.ForceSnapToCandidate = false;
      var edgeCreator = createEdgeInputMode.EdgeCreator;
      createEdgeInputMode.EdgeCreator = (context, graph, source, target, template) => {
        if (target != null) {
          // An actual graph node was hit; preserve the default edge creation behavior.
          return edgeCreator(context, graph, source, target, template);
        }
        // Use the dummy target node to create a graph node at the gesture's current location.
        var dummy = createEdgeInputMode.PreviewEndNode;
        var color = dummy.Tag as string ?? RandomColor();
        var node = CreateNode(graph, dummy.Layout.GetCenter(), color, settings.ShowPorts);
        var targetCandidate = new PortCandidate(node, FreeNodePortLocationModel.Center);
        return edgeCreator(context, graph, source, targetCandidate, template);
      };
    }

    /// <summary>Initializes the <see cref="GraphEditorInputMode"/> and its custom edge mode.</summary>
    private void InitializeInputMode(bool enableTargetNode) {
      // The current input mode already owns the edge-creation settings. Reinitialize its
      // target-node behavior by replacing it with a freshly configured mode.
      var graph = graphControl.Graph;
      var geim = new GraphEditorInputMode();
      var edgeMode = new RoutingCreateEdgeInputMode { RoutingStrategy = settings.RoutingStrategy };
      // Preserve the original priority when reinstalling the mode after a setting changes.
      edgeMode.Priority = geim.CreateEdgeInputMode.Priority;
      geim.CreateEdgeInputMode = edgeMode;

      // show source port candidates as well
      edgeMode.ShowPortCandidates = ShowPortCandidates.All;

      // allow edge creation for selected nodes on the given source port(s) as well
      edgeMode.StartOverCandidateOnly = true;

      edgeMode.PortCandidateRenderer = new PortCandidateRenderer { Size = 6 };

      // newly created edges should use the same color as their source port color
      edgeMode.EdgeCreationStarted += (_, args) => {
        if (args.Item.SourcePort.Owner.Tag is string color) {
          edgeMode.PreviewGraph.SetStyle(edgeMode.PreviewEdge, CreateEdgeStyle(color));
        }
      };

      if (enableTargetNode) {
        EnableTargetNodeCreation(edgeMode);
        edgeMode.EndPortCandidateChanged += (_, args) => UpdatePreviewTarget(edgeMode, args.Item);
      }

      geim.NodeCreated += (_, args) => {
        // Use a random node color and add directional ports to each created node.
        var color = RandomColor();
        // Assign a random color style.
        graph.SetStyle(args.Item, CreateNodeStyle(color));
        args.Item.Tag = color;
        // Add ports to the new node.
        AddDirectionalPorts(graph, args.Item, CreatePortStyle(color, settings.ShowPorts));
      };
      graphControl.InputMode = geim;
    }

    /// <summary>
    ///   Configures the port behavior in this demo to not discard ports when an edge is disconnected, and
    ///   each node should present its ports as possible candidate.
    /// </summary>
    private static void InitializePortBehavior(IGraph graph) {
      // Keep ports when edges are removed so the node's candidates remain available.
      graph.NodeDefaults.Ports.AutoCleanUp = false;
      // Each node exposes its ports as edge creation candidates.
      graph.GetDecorator().Nodes.PortCandidateProvider
          .AddFactory(node => PortCandidateProviders.FromExistingPorts(node));
      graph.GetDecorator().Ports.Handle.Hide();
    }

    /// <summary>Returns a random color from the <see cref="NodeColors"/> array.</summary>
    private static string RandomColor() => NodeColors[Random.Shared.Next(NodeColors.Length)];

    /// <summary>Creates a two-unit polyline edge style with a matching stealth arrow.</summary>
    private static PolylineEdgeStyle CreateEdgeStyle(string color) =>
        new() {
            Pen = new Pen(BrushFrom(color), 2),
            TargetArrow = new Arrow(ArrowType.Stealth, BrushFrom(color), cropLength: 1)
        };

    /// <summary>Creates a round-rectangle node style with the given outline color.</summary>
    private static ShapeNodeStyle CreateNodeStyle(string color) =>
        new() {
            Shape = ShapeNodeShape.RoundRectangle,
            Brush = NodeFill,
            Pen = new Pen(BrushFrom(color), 4)
        };

    /// <summary>Creates a circular port style, or the void style when visualization is disabled.</summary>
    private static IPortStyle CreatePortStyle(string color, bool showPorts) =>
        showPorts
            ? new ShapePortStyle {
                Shape = ShapeNodeShape.Ellipse,
                Brush = NodeFill,
                Pen = new Pen(BrushFrom(color), 1),
                RenderSize = new SizeD(6, 6)
            }
            : VoidPortStyle.Instance;

    /// <summary>Adds ports at the node center and on its top, right, bottom, and left sides.</summary>
    private static void AddDirectionalPorts(IGraph graph, INode node, IPortStyle style) {
      graph.AddPort(node, FreeNodePortLocationModel.Center, style);
      graph.AddPort(node, FreeNodePortLocationModel.Top, style);
      graph.AddPort(node, FreeNodePortLocationModel.Right, style);
      graph.AddPort(node, FreeNodePortLocationModel.Bottom, style);
      graph.AddPort(node, FreeNodePortLocationModel.Left, style);
    }

    /// <summary>
    ///   Creates a node with directional ports and the given color.
    /// </summary>
    /// <param name="graph">The graph in which the node is created.</param>
    /// <param name="center">The node's center location.</param>
    /// <param name="color">The node's color, or a random color if it is empty.</param>
    private static INode CreateNode(IGraph graph, PointD center, string color, bool showPorts = true) {
      var node = graph.CreateNode(new RectD(center.X - 20, center.Y - 20, 40, 40), CreateNodeStyle(color), color);
      AddDirectionalPorts(graph, node, CreatePortStyle(color, showPorts));
      return node;
    }

    /// <summary>Creates the initial sample graph.</summary>
    private static void CreateGraph(IGraph graph) {
      graph.Clear();
      CreateNode(graph, new PointD(0, 0), NodeColors[0]);
      for (var y = -1; y <= 1; y++) {
        for (var x = -1; x <= 1; x++) {
          if (x != 0 || y != 0) {
            CreateNode(graph, new PointD(x * 80, y * 80), NodeColors[1]);
          }
        }
      }
      for (var y = -2; y <= 2; y += 4) {
        for (var x = -2; x <= 2; x++) {
          CreateNode(graph, new PointD(x * 80, y * 80), NodeColors[2]);
        }
      }
      for (var y = -3; y <= 3; y += 6) {
        for (var x = -3; x <= 3; x++) {
          CreateNode(graph, new PointD(x * 80, y * 80), NodeColors[3]);
        }
      }
    }

    /// <summary>Recreates the initial sample graph.</summary>
    private void OnResetGraph(object sender, RoutedEventArgs e) => CreateGraph(graphControl.Graph);

    private void InitializeUI(IGraph graph) {
      settings.PropertyChanged += (_, args) => {
        if (args.PropertyName == nameof(Settings.RoutingStrategy)) {
          var geim = (GraphEditorInputMode) graphControl.InputMode!;
          ((RoutingCreateEdgeInputMode) geim.CreateEdgeInputMode).RoutingStrategy = settings.RoutingStrategy;
        } else if (args.PropertyName == nameof(Settings.ShowPorts)) {
          // Toggle port visualization without removing ports from the candidate providers.
          foreach (var port in graph.Ports) {
            graph.SetStyle(port, CreatePortStyle(port.Owner.Tag as string ?? NodeColors[0], settings.ShowPorts));
          }
        } else if (args.PropertyName == nameof(Settings.EnableTargetNode)) {
          InitializeInputMode(settings.EnableTargetNode);
        }
      };
    }

    /// <summary>Updates the dummy node visualization as the target candidate changes.</summary>
    private void UpdatePreviewTarget(CreateEdgeInputMode mode, IPortCandidate candidate) {
      if (candidate?.Owner is INode) {
        // The real graph node will be the target when the gesture finishes.
        mode.PreviewGraph.SetStyle(mode.PreviewEndNode, VoidNodeStyle.Instance);
        foreach (var port in mode.PreviewGraph.Ports.ToArray()) {
          mode.PreviewGraph.Remove(port);
        }
      } else {
        var color = mode.PreviewEndNode.Tag as string ?? NodeColors[0];
        mode.PreviewGraph.SetStyle(mode.PreviewEndNode, CreateNodeStyle(color));
        AddDirectionalPorts(mode.PreviewGraph, mode.PreviewEndNode, CreatePortStyle(color, settings.ShowPorts));
      }
    }


    private static SolidColorBrush BrushFrom(string color) => new((Color) ColorConverter.ConvertFromString(color));

    [STAThread]
    public static void Main() => Run<CustomEdgeCreationDemo>();
  }

  /// <summary>The available routing approaches during edge creation.</summary>
  public enum RoutingStrategy
  {
    DefaultOrthogonal,
    EdgeRouterQuality,
    EdgeRouterPerformance
  }

  /// <summary>View model for the demo's toolbar settings.</summary>
  public sealed class Settings : INotifyPropertyChanged
  {
    private bool showPorts = true;

    private bool enableTargetNode = true;

    // Start with the performance-oriented EdgeRouter strategy.
    private RoutingStrategy routingStrategy = RoutingStrategy.EdgeRouterPerformance;

    /// <summary>Notifies the toolbar when a setting changes.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<RoutingStrategy> RoutingStrategies { get; } = [.. Enum.GetValues<RoutingStrategy>()];

    /// <summary>Whether circular port styles are visible in the graph.</summary>
    public bool ShowPorts {
      get => showPorts;
      set => SetField(ref showPorts, value);
    }

    /// <summary>Whether an edge gesture on empty canvas creates a target node.</summary>
    public bool EnableTargetNode {
      get => enableTargetNode;
      set => SetField(ref enableTargetNode, value);
    }

    /// <summary>The routing strategy selected for edge creation gestures.</summary>
    public RoutingStrategy RoutingStrategy {
      get => routingStrategy;
      set => SetField(ref routingStrategy, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
