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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.PortLocationModels;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Input.EdgeReconnection
{
  /// <summary>
  ///   Shows how to customize the reconnection behavior for existing edges in the graph
  ///   by implementing a custom <see cref="IEdgeReconnectionPortCandidateProvider"/>.
  /// </summary>
  public partial class EdgeReconnectionWindow
  {
    
    /// <summary>
    ///   Registers a callback function as decorator that provides a custom
    ///   <see cref="IEdgeReconnectionPortCandidateProvider"/> for each node.
    /// </summary>
    /// <remarks>
    ///   This callback function is called whenever a node in the graph is queried
    ///   for its <c>IEdgeReconnectionPortCandidateProvider</c>. In this case, the 'node'
    ///   parameter will be set to that node.
    /// </remarks>
    private void RegisterEdgeReconnectionPortCandidateProvider() {
      // register our customized link with the chain for the IEdges to provide
      // the custom provider instances dynamically
      graphControl.Graph.GetDecorator().EdgeDecorator.EdgeReconnectionPortCandidateProviderDecorator.SetFactory(
        edge => {
          // obtain the tag from the node
          object edgeTag = edge.Tag;
      
          // check if it is a known tag and choose the respective implementation
          if (!(edgeTag is Color)) {
            return null;
          } else if (Colors.Firebrick.Equals(edgeTag)) {
            return new RedEdgeReconnectionPortCandidateProvider(edge);
          } else if (Colors.Orange.Equals(edgeTag)) {
            return new OrangeEdgeReconnectionPortCandidateProvider(edge);
          } else if (Colors.RoyalBlue.Equals(edgeTag)) {
            return new BlueEdgeReconnectionPortCandidateProvider();
          } else if (Colors.Green.Equals(edgeTag)) {
            return new GreenEdgeReconnectionPortCandidateProvider();
          } else {
            // otherwise revert to default behavior
            return null;
          }
        });
    }

    /// <summary>
    ///   An <see cref="IEdgeReconnectionPortCandidateProvider"/> that prevents relocation of the ports.
    /// </summary>
    public class RedEdgeReconnectionPortCandidateProvider : IEdgeReconnectionPortCandidateProvider {
      private readonly IEdge edge;

      public RedEdgeReconnectionPortCandidateProvider(IEdge edge) {
        this.edge = edge;
      }

      /// <summary>
      ///   Returns only the current port as candidate, thus effectively disabling relocation.
      /// </summary>
      public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) {
        // add the current port only to the candidates - effectively disabling reconnection
        return new List<IPortCandidate> { new DefaultPortCandidate(edge.SourcePort) };
      }

      /// <summary>
      ///   Returns no candidates, thus effectively disabling relocation.
      /// </summary>
      public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) {
        return Enumerable.Empty<IPortCandidate>();
      }
    }

    /// <summary>
    ///   An <see cref="IEdgeReconnectionPortCandidateProvider"/> that uses candidates with a
    ///   dynamic <see cref="FreeNodePortLocationModel"/>. It allows moving ports to any
    ///   location inside a green node.
    /// </summary>
    public class GreenEdgeReconnectionPortCandidateProvider : IEdgeReconnectionPortCandidateProvider {

      /// <summary>
      ///   Returns for each green node a candidate with a dynamic <see cref="FreeNodePortLocationModel"/>. 
      ///   When the Shift key is pressed, a port can be placed
      ///   anywhere inside that node.
      /// </summary>
      public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) {
        var graph = context.GetGraph();
        if (graph == null) {
          return Enumerable.Empty<IPortCandidate>();
        } else {
          return 
            from node in graph.Nodes 
            where Equals(node.Tag, Colors.Green) 
            select (IPortCandidate)new DefaultPortCandidate(node, FreeNodePortLocationModel.Instance);
        }
      }

      /// <summary>
      ///   The same as <see cref="GetSourcePortCandidates"/>.
      /// </summary>
      public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) {
        return GetSourcePortCandidates(context);
      }
    }

    /// <summary>
    ///   An <see cref="IEdgeReconnectionPortCandidateProvider"/> that allows moving ports to
    ///   any other orange node, except for the opposite port's node. 
    /// </summary>
    public class OrangeEdgeReconnectionPortCandidateProvider : IEdgeReconnectionPortCandidateProvider
    {
      private readonly IEdge edge;

      public OrangeEdgeReconnectionPortCandidateProvider(IEdge edge) {
        this.edge = edge;
      }

      /// <summary>
      ///   Returns candidates for all ports at orange nodes in the graph, except
      ///   for the current target node to avoid the creation of self-loops.
      /// </summary>
      public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) {
        List<IPortCandidate> result = new List<IPortCandidate>();
        // add the current one as the default
        result.Add(new DefaultPortCandidate(edge.SourcePort));
        var graph = context.GetGraph();
        if (graph == null) {
          return result;
        } 
        foreach (INode node in graph.Nodes) {
          if (node != edge.TargetPort.Owner && Colors.Orange.Equals(node.Tag)) {
            // use the candidates from the provider - if available
            IPortCandidateProvider provider = node.Lookup<IPortCandidateProvider>();
            // If available, use the candidates from the provider. Otherwise, add a default candidate.
            if (provider != null) {
              result.AddRange(provider.GetTargetPortCandidates(context));
            } else {
              result.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeCenterAnchored));
            }
          }
        }
        return result;
      }

      /// <summary>
      ///   Returns candidates for all ports at orange nodes in the graph, except
      ///   for the current source node to avoid the creation of self-loops.
      /// </summary>
      public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) {
        List<IPortCandidate> result = new List<IPortCandidate>();
        // add the current one as the default
        result.Add(new DefaultPortCandidate(edge.TargetPort));
        var graph = context.GetGraph();
        if (graph == null) {
          return result;
        }
        foreach (INode node in graph.Nodes) {
          if (node != edge.SourcePort.Owner && Colors.Orange.Equals(node.Tag)) {
            // If available, use the candidates from the provider. Otherwise, add a default candidate.
            IPortCandidateProvider provider = node.Lookup<IPortCandidateProvider>();
            if (provider != null) {
              result.AddRange(provider.GetTargetPortCandidates(context));
            } else {
              result.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeCenterAnchored));
            }
          }
        }
        return result;
      }
    }

    /// <summary>
    ///   An <see cref="IEdgeReconnectionPortCandidateProvider"/> that allows moving ports to
    ///   any other existing port on any node.
    /// </summary>
    private class BlueEdgeReconnectionPortCandidateProvider : IEdgeReconnectionPortCandidateProvider
    {
      /// <summary>
      /// Returns candidates for the locations of all existing ports at all nodes.
      /// </summary>
      public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) {
        return GetPortCandidates(context);
      }

      /// <summary>
      /// Returns candidates for the locations of all existing ports at all nodes.
      /// </summary>
      public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) {
        return GetPortCandidates(context);
      }

      /// <summary>
      ///   Returns port candidates for all existing ports at all nodes in the graph as alternatives.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <returns>A sequence of all possible port candidates in <paramref name="context" />'s graph.</returns>
      private IEnumerable<IPortCandidate> GetPortCandidates(IInputModeContext context) {
        var graph = context.GetGraph();

        if (graph == null) {
          return Enumerable.Empty<IPortCandidate>();
        }

        return
          from node in graph.Nodes
          from port in node.Ports
          select CreatePortCandidate(node, port);
      }

      /// <summary>
      ///   Adds a port candidate dependent on the color tag of the given node.
      /// </summary>
      /// <param name="node">
      ///   The node that the port belongs to (for convenience, it could also be retrieved directly from the
      ///   port).
      /// </param>
      /// <param name="port">The port to add the candidate for.</param>
      /// <returns></returns>
      /// <remarks>
      ///   <para>
      ///     If the node is blue, then the existing port instances will be reused for the port candidates. This has the effect
      ///     that a port can potentially have multiple edges connected to it.
      ///   </para>
      ///   <para>
      ///     If the node is not blue, then a new port will be created at the same location as the original port for the port
      ///     candidate. This means that there are potentially multiple ports at the same location and no edge shares a port with
      ///     another edge.
      ///   </para>
      /// </remarks>
      private IPortCandidate CreatePortCandidate(INode node, IPort port) {
        if (Colors.RoyalBlue.Equals(node.Tag)) {
          // reuse the existing port - the edge will be connected to the very same port after reconnection
          return new DefaultPortCandidate(port);
        } else {
          // don't reuse the existing ports, but create new ones at the same location
          return new DefaultPortCandidate(node,
              FreeNodePortLocationModel.Instance.CreateParameter(node, port.GetLocation()));
        }
      }
    }

    #region Initialization

    public EdgeReconnectionWindow() {
      InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      IGraph graph = graphControl.Graph;

      // Disable automatic cleanup of unconnected ports since some nodes have a predfined set of ports
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      // Create a default editor input mode
      var graphEditorInputMode = new GraphEditorInputMode();

      // Just for user convenience: disable node and edge creation,
      graphEditorInputMode.AllowCreateEdge = false;
      graphEditorInputMode.AllowCreateNode = false;
      // disable deleting items
      graphEditorInputMode.DeletableItems = GraphItemTypes.None;
      // disable the clipboard
      graphEditorInputMode.AllowClipboardOperations = false;
      // and enable the undo feature.
      graph.SetUndoEngineEnabled(true);

      // Finally, set the input mode to the graph control.
      graphControl.InputMode = graphEditorInputMode;

      // Set a port style that makes the pre-defined ports visible
      graph.NodeDefaults.Ports.Style =
          new NodeStylePortStyleAdapter(new ShapeNodeStyle
          {
            Shape = ShapeNodeShape.Ellipse,
            Brush = Brushes.Black,
            Pen = null
          }) { RenderSize = new SizeD(3, 3) };

      RegisterEdgeReconnectionPortCandidateProvider();

      CreateSampleGraph(graphControl);
    }

    #endregion

    #region Sample Graph Creation

    /// <summary>
    ///   Creates the sample graph of this demo.
    /// </summary>
    private void CreateSampleGraph(GraphControl graphControl) {
      var graph = graphControl.Graph;
      var blackPortStyle =
          new NodeStylePortStyleAdapter(new ShapeNodeStyle
          {
            Shape = ShapeNodeShape.Ellipse,
            Brush = Brushes.Black,
            Pen = null
          }) { RenderSize = new SizeD(3, 3) };
      graph.SetUndoEngineEnabled(true);
      
      CreateSubgraph(graph, Colors.Firebrick, 0);
      CreateSubgraph(graph, Colors.Orange, 200);
      CreateSubgraph(graph, Colors.Green, 600);

      // the blue nodes have some additional ports besides the ones used by the edge
      var nodes = CreateSubgraph(graph, Colors.RoyalBlue, 400);
      graph.AddPort(nodes[0], FreeNodePortLocationModel.Instance.CreateParameter(new PointD(1, 0.2)), blackPortStyle);
      graph.AddPort(nodes[0], FreeNodePortLocationModel.Instance.CreateParameter(new PointD(1, 0.8)), blackPortStyle);

      var candidateProvider = PortCandidateProviders.FromShapeGeometry(nodes[2], 0, 0.25, 0.5, 0.75);
      candidateProvider.Style = blackPortStyle;
      IEnumerable<IPortCandidate> candidates = candidateProvider.GetSourcePortCandidates(graphControl.InputModeContext);
      foreach (IPortCandidate portCandidate in candidates) {
        if (portCandidate.Validity != PortCandidateValidity.Dynamic) {
          portCandidate.CreatePort(graphControl.InputModeContext);
        }
      }
      graph.GetUndoEngine().Clear();
    }

    /// <summary>
    /// Creates the sample graph of the given color.
    /// </summary>
    private static INode[] CreateSubgraph(IGraph graph, Color color, double yOffset) {
      var brush = new SolidColorBrush(color);
      INode n1 = graph.CreateNode(new RectD(100, 100 + yOffset, 60, 60), new ShinyPlateNodeStyle { Brush = brush }, color);
      INode n2 = graph.CreateNode(new RectD(500, 100 + yOffset, 60, 60), new ShinyPlateNodeStyle { Brush = brush }, color);
      INode n3 = graph.CreateNode(new RectD(300, 160 + yOffset, 60, 60), new ShinyPlateNodeStyle { Brush = brush }, color);

      graph.CreateEdge(n1, n2, new PolylineEdgeStyle { Pen = new Pen(brush, 1) }, color);
      return new[]{n1, n2, n3};
    }

    #endregion    
  }
}
