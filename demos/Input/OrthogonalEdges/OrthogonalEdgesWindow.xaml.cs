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
using System.Windows;
using Demo.yFiles.Toolkit;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.Graph.Input.OrthogonalEdges
{
  /// <summary>
  /// Shows how orthogonal edge editing can be customized by implementing the 
  /// <see cref="IOrthogonalEdgeHelper"/> interface.
  /// </summary>
  public partial class OrthogonalEdgesWindow
  {

    public OrthogonalEdgesWindow() {
      InitializeComponent();
    }

    /// <summary>
    /// Creates the custom <see cref="IOrthogonalEdgeHelper"/> of this demo and
    /// registers them with the <see cref="EdgeDecorator"/> of the graph.
    /// Additionally, it sets some other decorators that complete the desired
    /// behavior.
    /// </summary>
    private void RegisterOrthogonalEdgeHelperDecorators() {
      EdgeDecorator edgeDecorator = graphControl.Graph.GetDecorator().EdgeDecorator;
      
      // Add different IOrthogonalEdgeHelpers to demonstrate various custom behaviour.
      edgeDecorator.OrthogonalEdgeHelperDecorator.SetImplementation(
        edge => (edge.Tag == Themes.PaletteRed), new RedOrthogonalEdgeHelper());
      // Green edges have the regular orthogonal editing behavior and therefore, 
      // don't need a custom implementation.
      edgeDecorator.OrthogonalEdgeHelperDecorator.SetImplementation(
        edge => (edge.Tag == Themes.PaletteGreen), new OrthogonalEdgeHelper());
      edgeDecorator.OrthogonalEdgeHelperDecorator.SetImplementation(
        edge => (edge.Tag == Themes.PalettePurple), new PurpleOrthogonalEdgeHelper());
      edgeDecorator.OrthogonalEdgeHelperDecorator.SetImplementation(
        edge => (edge.Tag == Themes.PaletteOrange), new OrangeOrthogonalEdgeHelper());
      edgeDecorator.OrthogonalEdgeHelperDecorator.SetImplementation(
        edge => (edge.Tag == Themes.PaletteLightblue), new BlueOrthogonalEdgeHelper());

      // Disable moving of the complete edge for orthogonal edges since this would create way too many bends.
      edgeDecorator.PositionHandlerDecorator.HideImplementation(
        edge => (edge.Tag == Themes.PaletteOrange) || 
                (edge.Tag == Themes.PaletteGreen) ||
                (edge.Tag == Themes.PalettePurple)
        );

      // Add a custom BendCreator for blue edges that ensures orthogonality 
      // if a bend is added to the first or last (non-orthogonal) segment.
      edgeDecorator.BendCreatorDecorator.SetImplementation(
        edge => (edge.Tag == Themes.PaletteLightblue), new BlueBendCreator());

      // Add custom IEdgePortHandleProviders to make the handles of 
      // purple and orange edge move within the bounds of the node.
      edgeDecorator.EdgePortHandleProviderDecorator.SetImplementationWrapper(
        edge => edge.Tag == Themes.PalettePurple, (edge, provider) => new PurpleEdgePortHandleProvider(provider));
      edgeDecorator.EdgePortHandleProviderDecorator.SetImplementation(
        edge => edge.Tag == Themes.PaletteOrange, new OrangeEdgePortHandleProvider());
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      // Create a default editor input mode
      GraphEditorInputMode graphEditorInputMode = new GraphEditorInputMode();

      // Enable orthogonal edge editing
      graphEditorInputMode.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext();
      
      // Just for user convenience: disable node and edge creation,
      graphEditorInputMode.AllowCreateEdge = false;
      graphEditorInputMode.AllowCreateNode = false;
      graphEditorInputMode.AllowClipboardOperations = false;
      // disable deleting items
      graphEditorInputMode.DeletableItems = GraphItemTypes.None;

      // disable grouping operations
      graphEditorInputMode.AllowGroupingOperations = false;
      graphEditorInputMode.AllowReparentNodes = false;

      // enable snapping for edges only,
      graphEditorInputMode.SnapContext = new GraphSnapContext()
                               {
                                 CollectNodeSnapLines = false, 
                                 CollectNodePairCenterSnapLines = false, 
                                 CollectNodePairSnapLines = false, 
                                 CollectNodePairSegmentSnapLines = false, 
                                 CollectNodeSizes = false, 
                                 SnapNodesToSnapLines = false, 
                                 SnapOrthogonalMovement = false
                               };

      graphControl.Graph.SetUndoEngineEnabled(true);

      // Finally, set the input mode to the graph control.
      graphControl.InputMode = graphEditorInputMode;

      // Disable auto-cleanup of ports since the purple nodes have explicit ports
      graphControl.Graph.NodeDefaults.Ports.AutoCleanUp = false;

      // Create and register the edge decorations
      RegisterOrthogonalEdgeHelperDecorators();

      CreateSampleGraph(graphControl.Graph);
    }

    #region Sample Graph Creation

    /// <summary>
    /// Creates the sample graph of this demo.
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {

      CreateSubgraph(graph, Themes.PaletteRed, 0, false);
      CreateSubgraph(graph, Themes.PaletteGreen, 110, false);
      CreateSubgraph(graph, Themes.PalettePurple, 220, true);
      CreateSubgraph(graph, Themes.PaletteOrange, 330, false);

      // The blue edge has more bends than the other edges
      var blueEdge = CreateSubgraph(graph, Themes.PaletteLightblue, 440, false);
      var blueBends = blueEdge.Bends.ToArray();
      graph.Remove(blueBends[1]);
      graph.Remove(blueBends[0]);
      var sourcePortLocation = blueEdge.SourcePort.GetLocation();
      var targetPortLocation = blueEdge.TargetPort.GetLocation();
      graph.AddBend(blueEdge, new PointD(220, sourcePortLocation.Y - 30));
      graph.AddBend(blueEdge, new PointD(300, sourcePortLocation.Y - 30));
      graph.AddBend(blueEdge, new PointD(300, targetPortLocation.Y + 30));
      graph.AddBend(blueEdge, new PointD(380, targetPortLocation.Y + 30));
    }

    /// <summary>
    /// Creates the sample graph of the given color with two nodes and a single edge.
    /// </summary>
    private static IEdge CreateSubgraph(IGraph graph, Palette palette, double yOffset, bool createPorts) {
      // Create two nodes
      var n1 = graph.CreateNode(new RectD(110, 100 + yOffset, 40, 40), DemoStyles.CreateDemoNodeStyle(palette), palette);
      var n2 = graph.CreateNode(new RectD(450, 130 + yOffset, 40, 40), DemoStyles.CreateDemoNodeStyle(palette), palette);

      // Create an edge, either between the two nodes or between the nodes's ports
      IEdge edge;
      if (!createPorts) {
        edge = graph.CreateEdge(n1, n2, DemoStyles.CreateDemoEdgeStyle(palette, false), palette);
      } else {
        var p1 = CreateSamplePorts(graph, n1, true);
        var p2 = CreateSamplePorts(graph, n2, false);
        edge = graph.CreateEdge(p1[1], p2[2], DemoStyles.CreateDemoEdgeStyle(palette, false), palette);
      }

      // Add bends that create a veredge.SourcePort.Locationtical segment in the middle of the edge
      var sourcePortLocation = edge.SourcePort.GetLocation();
      var targetPortLocation = edge.TargetPort.GetLocation();
      var x = (sourcePortLocation.X + targetPortLocation.X)/2;
      graph.AddBend(edge, new PointD(x, sourcePortLocation.Y));
      graph.AddBend(edge, new PointD(x, targetPortLocation.Y));

      return edge;
    }

    /// <summary>
    /// Adds some ports to the given node.
    /// </summary>
    private static IPort[] CreateSamplePorts(IGraph graph, INode node, bool toEastSide) {
      var model = FreeNodePortLocationModel.Instance;
      var x = toEastSide ? 0.9 : 0.1;
      var ports = new IPort[4];
      ports[0] = graph.AddPort(node, model.CreateParameter(new PointD(x, 0.05)));
      ports[1] = graph.AddPort(node, model.CreateParameter(new PointD(x, 0.35)));
      ports[2] = graph.AddPort(node, model.CreateParameter(new PointD(x, 0.65)));
      ports[3] = graph.AddPort(node, model.CreateParameter(new PointD(x, 0.95)));
      return ports;
    }

    #endregion

  }

  /// <summary>
  /// An <see cref="IOrthogonalEdgeHelper"/> for edges that don't have
  /// orthogonal editing behavior.
  /// </summary>
  public class RedOrthogonalEdgeHelper : IOrthogonalEdgeHelper
  {
    /// <summary>
    /// Returns the non-orthogonal segment orientation.
    /// </summary>
    public SegmentOrientation GetSegmentOrientation(IInputModeContext context, IEdge edge, int segmentIndex) {
      return SegmentOrientation.NonOrthogonal;
    }

    /// <summary>
    /// Returns <see langword="false"/>.
    /// </summary>
    public bool ShouldMoveEndImplicitly(IInputModeContext context, IEdge edge, bool sourceEnd) {
      return false;
    }

    /// <summary>
    /// Returns <see langword="false"/>.
    /// </summary>
    public bool ShouldEditOrthogonally(IInputModeContext context, IEdge edge) {
      return false;
    }

    /// <summary>
    /// Does nothing; no cleanup of bends performed.
    /// </summary>
    public void CleanUpEdge(IInputModeContext context, IGraph graph, IEdge edge) {
      // no cleanup of bends performed
    }
  }

    /// <summary>
  /// An <see cref="OrthogonalEdgeHelper"/> that enables moving the
  /// source/target of the edge to another port, removes bends inside the bounds
  /// of the node and relocates the port to the last bend inside the node.
  /// </summary>
  public class PurpleOrthogonalEdgeHelper : OrthogonalEdgeHelper
  {
    /// <summary>
    /// Enables moving the source and target of the edge to other ports.
    /// </summary>
    public override bool ShouldMoveEndImplicitly(IInputModeContext context, IEdge edge, bool sourceEnd) {
      return true;
    }

    /// <summary>
    /// Removes bends inside of nodes, in addition to the clean-ups provided by
    /// the base implementation.
    /// </summary>
    public override void CleanUpEdge(IInputModeContext context, IGraph graph, IEdge edge) {
      base.CleanUpEdge(context, graph, edge);
      // now check bends which lie inside the node bounds and remove them...
      var sourceNode = edge.GetSourceNode();
      if (sourceNode != null) {
        var sourceContainsTest = sourceNode.SafeLookup<IShapeGeometry>();
        while (edge.Bends.Count > 0 && sourceContainsTest.IsInside(edge.Bends[0].Location.ToPointD())) {
          var bendLocation = edge.Bends[0].Location.ToPointD();
          // we try to move to port to the bend location so that the edge shape stays the same
          graph.SetPortLocation(edge.SourcePort, bendLocation);
          if (edge.SourcePort.GetLocation() != bendLocation) {
            break; // does not work - bail out
          }
          graph.Remove(edge.Bends[0]);
        }
      }
      var targetNode = edge.GetTargetNode();
      if (targetNode != null) {
        var targetContainsTest = targetNode.SafeLookup<IShapeGeometry>();
        while (edge.Bends.Count > 0 && targetContainsTest.IsInside(edge.Bends[edge.Bends.Count-1].Location.ToPointD())) {
          var lastBend = edge.Bends[edge.Bends.Count-1];
          var bendLocation = lastBend.Location.ToPointD();
          // we try to move to port to the bend location so that the edge shape stays the same
          graph.SetPortLocation(edge.TargetPort, bendLocation);
          if (edge.TargetPort.GetLocation() != bendLocation) {
            break; // does not work - bail out
          }
          graph.Remove(lastBend);
        }
      }
    }
  }

  /// <summary>
  /// An <see cref="OrthogonalEdgeHelper"/> that enables moving the
  /// source/target of the edge to another port.
  /// </summary>
  public class OrangeOrthogonalEdgeHelper : OrthogonalEdgeHelper
  {
    /// <summary>
    /// Enables moving the source and target of the edge to other ports.
    /// </summary>
    public override bool ShouldMoveEndImplicitly(IInputModeContext context, IEdge edge, bool sourceEnd) {
      return true;
    }
  }

  /// <summary>
  /// The <see cref="OrthogonalEdgeHelper"/> for blue edges. Orthogonal edge
  /// editing is enabled for the inner segments of this edges but not for the
  /// first and last one.
  /// </summary>
  public class BlueOrthogonalEdgeHelper : OrthogonalEdgeHelper
  {
    /// <summary>
    /// Returns the NonOrthogonal segment orientation for the first and last
    /// segment, and the default for all other segments.
    /// </summary>
    public override SegmentOrientation GetSegmentOrientation(IInputModeContext context, IEdge edge, int segmentIndex) {
      return segmentIndex == 0 || segmentIndex == edge.Bends.Count
               ? SegmentOrientation.NonOrthogonal
               : base.GetSegmentOrientation(context, edge, segmentIndex);
    }
  }

  /// <summary>
  /// This bend creator ensures that inner segments of an edge are always
  /// orthogonal, even if the new bend was created on the non-orthogonal first
  /// or last segment. 
  /// </summary>
  public class BlueBendCreator : IBendCreator
  {
    /// <summary>
    /// Creates a new bend at the given location. If this bend is on the first or last segment,
    /// a second bend is created and placed at a location that ensures that the newly create
    /// inner segment is orthogonal.
    /// </summary>
    public int CreateBend(IInputModeContext context, IGraph graph, IEdge edge, PointD location) {
      var edgePoints = GetEdgePoints(edge);
      var closestSegment = DetermineBendSegmentIndex(edgePoints, location);

      int firstSegment = 0;
      int lastSegment = edge.Bends.Count;

      // if bend wasn't created in first or last segment, call default action
      if (closestSegment != firstSegment && closestSegment != lastSegment) {
        return (new DefaultBendCreator()).CreateBend(context, graph, edge, location);
      }

      // add created bend and another one to make the edge stay orthogonal
      if (closestSegment == -1 || context == null || !(context.ParentInputMode is CreateBendInputMode)) {
        return -1;
      }
      var editingContext = context.Lookup<OrthogonalEdgeEditingContext>();
      if (editingContext == null) {
        return -1;
      }
      if (closestSegment == firstSegment) {
        IPoint nextPoint = edgePoints[1];
        // get orientation of next edge segment to determine second bend location
        SegmentOrientation orientation = editingContext.GetSegmentOrientation(edge, 1);
        graph.AddBend(edge, location, 0);
        if (orientation == SegmentOrientation.Horizontal) {
          graph.AddBend(edge, new PointD(nextPoint.X, location.Y), 1);
        } else if (orientation == SegmentOrientation.Vertical) {
          graph.AddBend(edge, new PointD(location.X, nextPoint.Y), 1);
        }
        return 0;
      }
      if (closestSegment == lastSegment) {
        IPoint prevPoint = edgePoints[edge.Bends.Count];
        // get orientation of next edge segment to determine second bend location
        SegmentOrientation orientation = editingContext.GetSegmentOrientation(edge, edge.Bends.Count - 1);
        graph.AddBend(edge, location, edge.Bends.Count);
        if (orientation == SegmentOrientation.Horizontal) {
          graph.AddBend(edge, new PointD(prevPoint.X, location.Y), edge.Bends.Count - 1);
        } else if (orientation == SegmentOrientation.Vertical) {
          graph.AddBend(edge, new PointD(location.X, prevPoint.Y), edge.Bends.Count - 1);
        }
        return edge.Bends.Count - 1;
      }
      return -1;
    }

    /// <summary>
    /// Determines the index of the segment in which the bend was created. 
    /// </summary>
    private int DetermineBendSegmentIndex(List<PointD> points, PointD location) {
      int closestIndex = -1;
      double minDist = Double.MaxValue;
      for (int i = 0; i < points.Count - 1; i++) {
        double dist = location.DistanceToSegment(points[i], points[i + 1]);
        if (dist < minDist) {
          closestIndex = i;
          minDist = dist;
        }
      }
      return closestIndex;
    }

    /// <summary>
    /// Returns a list containing the source port location, the bend locations,
    /// and the target port location of the given edge.
    /// </summary>
    private List<PointD> GetEdgePoints(IEdge edge) {
      List<PointD> points = new List<PointD>();
      points.Add(edge.SourcePort.GetLocation());
      foreach (IBend bend in edge.Bends) {
        points.Add(bend.Location.ToPointD());
      }
      points.Add(edge.TargetPort.GetLocation());
      return points;
    }
  }

  /// <summary>
  /// An <see cref="IEdgePortHandleProvider" /> that constraints the
  /// port location handle of the wrapped provider to the layout rectangle of the port's owner node.
  /// </summary>
  public class PurpleEdgePortHandleProvider : IEdgePortHandleProvider
  {
    private readonly IEdgePortHandleProvider wrapped;

    public PurpleEdgePortHandleProvider(IEdgePortHandleProvider wrapped) {
      this.wrapped = wrapped;
    }

    /// <summary>
    /// Returns the handle of the wrapped provider constrained to the layout rectangle of the
    /// port's owner node.
    /// </summary>
    public IHandle GetHandle(IInputModeContext context, IEdge edge, bool sourceHandle) {
      var wrappedHandle = wrapped.GetHandle(context, edge, sourceHandle);

      IPort port = sourceHandle ? edge.SourcePort : edge.TargetPort;
      return port.Owner is INode
               ? new NodeLayoutPortLocationHandle((INode)port.Owner, wrappedHandle)
               : wrappedHandle;
    }
  }

  /// <summary>
  /// An <see cref="IEdgePortHandleProvider" /> that constraints the original
  /// port location handle to the layout rectangle of the port's owner node.
  /// </summary>
  public class OrangeEdgePortHandleProvider : IEdgePortHandleProvider
  {
    /// <summary>
    /// Returns a handle that is constrained to the layout rectangle of the port's owner node.
    /// </summary>
    public IHandle GetHandle(IInputModeContext context, IEdge edge, bool sourceHandle) {
      IPort port = sourceHandle ? edge.SourcePort : edge.TargetPort;
      return port.Owner is INode
               ? new NodeLayoutPortLocationHandle((INode) port.Owner, port.Lookup<IHandle>())
               : port.Lookup<IHandle>();
    }
  }

  /// <summary>
  /// A port location handle that is constrained to the layout rectangle of
  /// the port's owner node.
  /// </summary>
  public class NodeLayoutPortLocationHandle : ConstrainedHandle
  {
    private readonly INode node;

    public NodeLayoutPortLocationHandle(INode node, IHandle wrappedHandle) : base(wrappedHandle) {
      this.node = node;
    }

    protected override PointD ConstrainNewLocation(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      return newLocation.GetConstrained(node.Layout.ToRectD());
    }
  }
}
