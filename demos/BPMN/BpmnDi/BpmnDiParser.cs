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
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Demo.yFiles.Graph.Bpmn.Styles;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using GroupNodeStyle = Demo.yFiles.Graph.Bpmn.Styles.GroupNodeStyle;
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  /// <summary>
  /// Parser for the BPMN 2.0 abstract syntax.
  /// </summary>
  public class BpmnDiParser
  {
    // The current parsed BpmnDocument
    private BpmnDocument Document { get; set; }

    // The currently used diagram
    private BpmnDiagram CurrentDiagram { get; set; }

    // maps a process BpmnElement to the BpmnElement that referenced this process in a 'processRef'
    private Dictionary<BpmnElement, BpmnElement> ProcessRefSource { get; set; }

    private List<BpmnShape> delayedBoundaryEvents = new List<BpmnShape>();

    // The master graph
    private IGraph MasterGraph {
      get { return view.Manager.MasterGraph; }
    }

    // The current folding manager and current folding view
    private FoldingManager Manager {
      get { return view.Manager; }
    }

    private IFoldingView view;


    /// <summary>
    /// LabelModel for Nodes with Exterior Label. Provides 32 possible Positions.
    /// </summary>
    private GenericLabelModel GenericLabelModel { get; set; }

    // Flags

    /// <summary>
    /// Flag that sets the rearrangement of Labels. Does not work properly with custom label bounds
    /// </summary>
    private const bool RearrangeLabels = false;

    /// <summary>
    /// Flag that decides if Labels should be parsed, if bpmndi:BPMNLabel XML element is missing
    /// </summary>
    private const bool ParseAllLabels = true;

    /// <summary>
    /// Flag that decides if the folded Diagrams inside a selected diagram should also be parsed
    /// </summary>
    private const bool ParseFoldedDiagrams = true;

    /// <summary>
    /// Flag that decides if only top level diagrams can be selected, or all possible Diagrams in the file
    /// </summary>
    private const bool ShowAllDiagrams = false;

    /// <summary>
    /// Flag, if false, no edges are parsed (Debug)
    /// </summary>
    private const bool ParseEdges = true;

    /// <summary>
    /// Flag to determine, if external node Labels should be single- or multiline
    /// Implementation left unfinished, since the right way to do would be overriding the renderer
    /// </summary>
    private const bool MultiLineExteriorNodeLabels = false;

    // Can't use BPMN-Constants here, so we have to add the standard size of message envelopes.
    private readonly SizeD bpmnMessageSize = new SizeD(20, 14);

    #region File Parsing 

    /// <summary>
    /// Constructs a new instance of the parser
    /// </summary>
    public BpmnDiParser() {
      // Initialize the GenericLabelModel
      InitGenericLabelModel();
      ProcessRefSource = new Dictionary<BpmnElement, BpmnElement>();
    }

    /// <summary>
    /// Parse a BPMN File and build a graph.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="Load(yWorks.Graph.IGraph,System.IO.Stream,System.Func{System.Collections.Generic.IEnumerable{string},string})"/>
    /// </remarks>
    /// <param name="graph">The graph to build the diagram in.</param>
    /// <param name="filePath">The path to read the file from.</param>
    /// <param name="selectDiagramCallback">Callback method which chooses one diagram name from a given list. If no method is provided the first diagram is chosen.</param>
    public void Load(IGraph graph, string filePath, Func<IEnumerable<string>, string> selectDiagramCallback = null) {
      Stream stream = new FileStream(filePath, FileMode.Open);
      Load(graph, stream, selectDiagramCallback);
      stream.Close();
    }

    /// <summary>
    /// Called to parse and build a graph.
    /// </summary>
    /// <param name="graph">The graph Instance build the diagram in.</param>
    /// <param name="stream">FileStream to get the graph from.</param>
    /// <param name="selectDiagramCallback">Callback method which chooses one diagram name from a given list. If no method is provided the first diagram is chosen.</param>
    public void Load(IGraph graph, Stream stream, Func<IEnumerable<string>, string> selectDiagramCallback = null) {
      // Initialize FoldingManager & View for the Graph
      view = graph.GetFoldingView();
      if (view == null) {
        throw new ArgumentException("Folding must be enabled.", "graph");
      }

      // Initialize the default Layout for folded Group Nodes
      Manager.FolderNodeConverter = new MultiLabelFolderNodeConverter {
          CopyFirstLabel = true,
          CopyLabels = true,
          CloneNodeStyle = true,
          CloneLabelLayoutParameter = true,
          LabelStyle = BpmnLabelStyle.NewDefaultInstance()
      };

      // Initialize the Layout for Edges alongside folded Group Nodes
      Manager.FoldingEdgeConverter = new DefaultFoldingEdgeConverter {
          CloneEdgeStyle = true,
          CopyFirstLabel = true,
          CloneLabelStyle = true,
          CloneLabelLayoutParameter = true,
          ReuseMasterPorts = true,
          ReuseFolderNodePorts = true,
          ResetBends = false
      };

      // Clear previous Graph
      MasterGraph.Clear();

      // Create BpmnDocument from XML Stream
      var streamReader = new StreamReader(stream, Encoding.UTF8);
      var doc = XDocument.Load(streamReader);
      Document = new BpmnDocument(doc);

      var topLevelDiagrams = ShowAllDiagrams ? Document.Diagrams : Document.TopLevelDiagrams;

      // Get the Diagram to load
      BpmnDiagram diaToLoad;
      if (selectDiagramCallback != null) {
        var chosenName = selectDiagramCallback(topLevelDiagrams.Select(d => d.Name));
        diaToLoad = topLevelDiagrams.FirstOrDefault(d => d.Name == chosenName);
      } else {
        diaToLoad = topLevelDiagrams.FirstOrDefault();
      }

      // Loads the selected Diagram into the supplied Graph
      if (diaToLoad != null) {
        LoadDiagram(diaToLoad, null);
      }
    }

    /// <summary>
    ///  Initialize the genericLabelModel
    /// Using a model with 32 positions (better than ExteriorLabelModel which only has 8) to enable
    /// more options for customization in the user interface.
    /// </summary>
    private void InitGenericLabelModel() {
      var exteriorLabelModel = new ExteriorLabelModel { Insets = new InsetsD(3, 3, 3, 3) };
      GenericLabelModel = new GenericLabelModel(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.South));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.SouthEast));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.SouthWest));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.North));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.NorthEast));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.NorthWest));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.West));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.East));
      // Big Insets
      exteriorLabelModel = new ExteriorLabelModel { Insets = new InsetsD(18, 18, 18, 18) };
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.South));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.SouthEast));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.SouthWest));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.North));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.NorthEast));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.NorthWest));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.West));
      GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.East));

      // Label Positions between existing exterior positions
      var freeNodeLabelModel = new FreeNodeLabelModel();
      // Small Insets
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0.25, 0), new PointD(-1.5, -3), new PointD(0.75, 1), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0.75, 0), new PointD(1.5, -3), new PointD(0.25, 1), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(1, 0.25), new PointD(3, -1.5), new PointD(0, 0.75), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(1, 0.75), new PointD(3, 1.5), new PointD(0, 0.25), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0.75, 1), new PointD(1.5, 3), new PointD(0.25, 0), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0.25, 1), new PointD(-1.5, 3), new PointD(0.75, 0), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0, 0.75), new PointD(-3, 1.5), new PointD(1, 0.25), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0, 0.25), new PointD(-3, -1.5), new PointD(1, 0.75), new PointD(0, 0), 0));
      // Big Insets
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0.25, 0), new PointD(-9, -18), new PointD(0.75, 1), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0.75, 0), new PointD(9, -18), new PointD(0.25, 1), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(1, 0.25), new PointD(18, -9), new PointD(0, 0.75), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(1, 0.75), new PointD(18, 9), new PointD(0, 0.25), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0.75, 1), new PointD(9, 18), new PointD(0.25, 0), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0.25, 1), new PointD(-9, 18), new PointD(0.75, 0), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0, 0.75), new PointD(-18, 9), new PointD(1, 0.25), new PointD(0, 0), 0));
      GenericLabelModel.AddParameter(freeNodeLabelModel.CreateParameter(new PointD(0, 0.25), new PointD(-18, -9), new PointD(1, 0.75), new PointD(0, 0), 0));
    }

    #endregion

    #region Building Backbone

    /// <summary>
    /// Builds the first diagram via drawing the individual nodes and edges
    /// </summary>
    /// <param name="diagram">The diagram to draw</param>
    /// <param name="localRoot">The local root node </param>
    private void LoadDiagram(BpmnDiagram diagram, INode localRoot) {
      CurrentDiagram = diagram;

      // iterate the BpmnElements of the BpmnPlane and build up all with a BpmnShape first and with a BpmnEdge afterwards
      var bpmnEdges = new List<BpmnEdge>();
      foreach (var child in diagram.Plane.Element.Children) {
        BuildElement(child, diagram.Plane, localRoot, bpmnEdges);
      }
      if (delayedBoundaryEvents.Count > 0) {
        foreach (var delayedBoundaryEvent in delayedBoundaryEvents) {
          BuildBoundaryEvent(delayedBoundaryEvent);
        }
        delayedBoundaryEvents.Clear();
      }
      foreach (var bpmnEdge in bpmnEdges) {
        BuildEdge(bpmnEdge);
      }

      // If we collapse the shape before we add edges, edge labels disappear -> Folding after edge creation
      // But we have to rearrange Labels first, otherwise they are not in sync with the positions after folding.
      Rearrange();
      foreach (var shape in diagram.Plane.ListOfShapes) {
        if (shape.IsExpanded == "false") {
          view.Collapse(shape.Element.Node);
        }
      }

      if (ParseFoldedDiagrams) {
        foreach (var child in diagram.Children) {
          bool collapsed = !view.IsExpanded(child.Value.Node);
          INode lastRoot = view.LocalRoot;
          if (collapsed) {
            view.LocalRoot = child.Value.Node;
          }
          LoadDiagram(child.Key, child.Value.Node);
          if (collapsed) {
            view.LocalRoot = lastRoot;
          }
        }
      }

      var groupNodes = this.MasterGraph.Nodes.Where(node => node.Style is GroupNodeStyle).ToList();
      foreach (var groupNode in groupNodes) {
        if (MasterGraph.GetChildren(groupNode).Count == 0) {
          var newChildren = MasterGraph.GetChildren(MasterGraph.GetParent(groupNode))
                                       .Where(child => child != groupNode 
                                                       && groupNode.Layout.Contains(child.Layout.GetTopLeft()) 
                                                       && groupNode.Layout.Contains(child.Layout.GetBottomRight())).ToList();
          foreach (var newChild in newChildren) {
            MasterGraph.SetParent(newChild, groupNode);
          }
        }
      }
    }

    /// <summary>
    /// Returns the <see cref="BpmnShape"/> for the <paramref name="element"/> in the context of this <paramref name="plane"/>.
    /// </summary>
    /// <param name="element">The element to get the shape for.</param>
    /// <param name="plane">The plane containing the shape for the element.</param>
    /// <returns></returns>
    private BpmnShape GetShape(BpmnElement element, BpmnPlane plane) {
      BpmnElement referencedElement = null;
      if (element.Name == "participantRef") {
        referencedElement = Document.Elements[element.Value];
      }

      // check if there is a valid shape for this element or the referenced one
      foreach (var shape in plane.ListOfShapes) {
        if (IsValidShape(shape, element, referencedElement, plane)) {
          return shape;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns whether the <paramref name="shape"/> belongs to this <paramref name="element"/> or <paramref name="referencedElement"/>
    /// in the context of the <paramref name="plane"/>.
    /// </summary>
    /// <param name="shape">The shape to check validity for.</param>
    /// <param name="element">The element to check if the shape is valid.</param>
    /// <param name="referencedElement">The element referenced by element.</param>
    /// <param name="plane">The plane containing this shape.</param>
    /// <returns></returns>
    private bool IsValidShape(BpmnShape shape, BpmnElement element, BpmnElement referencedElement, BpmnPlane plane) {
      if (shape.Element != element && shape.Element != referencedElement) {
        // shape has to be defined for Element or referenced Element
        return false;
      }
      if (shape.ChoreographyActivityShape == null) {
        // there is no ChoreographyActivityShape, so no further checks needed
        return true;
      }
      if (element.Parent != null && (element.Parent.Name == "choreographyTask" || element.Parent.Name == "subChoreography")) {
        // if a ChoreographyActivityShape is defined, we need to be inside the defined choreographyTask or subChoreography
        var choreoShape = GetShape(element.Parent, plane);
        if (choreoShape != null) {
          return shape.ChoreographyActivityShape == choreoShape.Id;
        }
      }
      return false;
    }

    /// <summary>
    /// Returns the <see cref="BpmnEdge"/> for the <paramref name="element"/> in the context of this <paramref name="plane"/>.
    /// </summary>
    /// <param name="element">The element to get an BpmnEdge for.</param>
    /// <param name="plane">The plane containing the BpmnEdges.</param>
    /// <returns></returns>
    private BpmnEdge GetEdge(BpmnElement element, BpmnPlane plane) {
      foreach (var bpmnEdge in plane.ListOfEdges) {
        if (bpmnEdge.Element == element) {
          return bpmnEdge;
        }
      }
      return null;
    }

    /// <summary>
    /// Recursively builds BPMN items from <paramref name="element"/> and its descendents.
    /// </summary>
    /// <param name="element">The element to build an BPMN item for.</param>
    /// <param name="plane">The plane containing the shapes for the current <see cref="BpmnDiagram"/>.</param>
    /// <param name="localRoot">The current root node.</param>
    /// <param name="bpmnEdges">The Collection to add all found <see cref="BpmnEdge"/> to process later.</param>
    private void BuildElement(BpmnElement element, BpmnPlane plane, INode localRoot, ICollection<BpmnEdge> bpmnEdges) {
      if (element.Name == "laneSet") {
        // build up the Pool structure defined by the laneSet
        BuildPool(element, plane, localRoot);
      } else {
        BpmnShape bpmnShape = GetShape(element, plane);
        BpmnEdge bpmnEdge = GetEdge(element, plane);
        if (bpmnShape != null) {
          if (element.Parent.Node == null) {
            element.Parent.Node = localRoot;
          }
          BuildShape(bpmnShape, element);
        } else if (bpmnEdge != null) {
          bpmnEdges.Add(bpmnEdge);
          return;
        }
        if (element.Process != null) {
          // The element references another Process so build it as well
          BpmnElement process;
          if (TryGetElementForId(element.Process, out process)) {
            ProcessRefSource[process] = element;
            BuildElement(process, plane, localRoot, bpmnEdges);
          }
        }
        // check if all children or only data associations shall be processed
        var parseOnlyDataAssociations = bpmnShape != null && element.Name == "subProcess" && bpmnShape.IsExpanded == "false";
        if (parseOnlyDataAssociations) {
          // this is a collapsed subProcess - check if it is linked to its own Diagram
          if (!Document.ElementToDiagram.ContainsKey(element)) {
            // there is no diagram associated with the subProcess so we parse the children for this diagram
            parseOnlyDataAssociations = false;
          }
        }
        foreach (var child in element.Children) {
          if (!parseOnlyDataAssociations || child.Name == "dataInputAssociation" || child.Name == "dataOutputAssociation") {
            BuildElement(child, plane, localRoot, bpmnEdges);
          }
        }
      }
    }

    /// <summary>
    /// Looks up the <see cref="BpmnElement"/> registered by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The id to look up the element for.</param>
    /// <param name="element">The element to set if one could be found for the given id.</param>
    /// <returns></returns>
    private bool TryGetElementForId(String id, out BpmnElement element) {
      if (Document.Elements.TryGetValue(id, out element)) {
        return true;
      }
      var separatorIndex = id.IndexOf(':');
      if (separatorIndex > 0) {
        // if no element was found for id but the id was prefixed for a namespace, try to find an element for an id without prefix
        var shortId = id.Substring(separatorIndex + 1);
        return Document.Elements.TryGetValue(shortId, out element);
      }
      return false;
    }

    /// <summary>
    /// Uses a labeling algorithm to rearrange the labels to reduce overlaps
    /// </summary>
    private void Rearrange() {
      if (!RearrangeLabels) {
        return;
      }
      var labelingData = new LabelingData();
      labelingData.EdgeLabelPreferredPlacement.Delegate = label => {
        if (string.Equals(label.Text, "yes", StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(label.Text, "no", StringComparison.InvariantCultureIgnoreCase)) {
          return new PreferredPlacementDescriptor {
              PlaceAlongEdge = LabelPlacements.AtSource,
              DistanceToEdge = 5,
              SideOfEdge = LabelPlacements.LeftOfEdge | LabelPlacements.RightOfEdge
          };
        }
        return new PreferredPlacementDescriptor {
            PlaceAlongEdge = LabelPlacements.AtCenter,
            DistanceToEdge = 5,
            SideOfEdge = LabelPlacements.LeftOfEdge | LabelPlacements.RightOfEdge,
        };
      };
      MasterGraph.ApplyLayout(new GenericLabeling {
          PlaceEdgeLabels = true,
          // This can be turned on, but does not give good results (although using a 32 position model)
          PlaceNodeLabels = false,
          // Always keep this false, to keep internal labels in order
          MoveInternalNodeLabels = false,
          OptimizationStrategy = OptimizationStrategy.PreferredPlacement,

          // Set additional preferences here
          ReduceAmbiguity = true,
          AutoFlipping = false,
          EdgeGroupOverlapAllowed = false,
          ReduceLabelOverlaps = true,
          RemoveNodeOverlaps = true,
          RemoveEdgeOverlaps = true
      }, labelingData);
    }

    /// <summary>
    /// Creates an <see cref="INode"/> on the graph
    /// </summary>
    /// <param name="shape">The <see cref="BpmnShape"/> to draw. </param>
    /// <param name="originalElement">The original element the shape shall be applied for.</param>
    private void BuildShape(BpmnShape shape, BpmnElement originalElement) {
      var bounds = new RectD(shape.X, shape.Y, shape.Width, shape.Height);

      switch (shape.Element.Name) {
        // Gateways
        case BpmnDiConstants.ExclusiveGatewayElement:
          if (shape.IsMarkerVisible) {
            BuildGatewayNode(shape, bounds, GatewayType.ExclusiveWithMarker);
          } else {
            BuildGatewayNode(shape, bounds, GatewayType.ExclusiveWithoutMarker);
          }
          break;
        case BpmnDiConstants.ParallelGatewayElement:
          BuildGatewayNode(shape, bounds, GatewayType.Parallel);
          break;
        case BpmnDiConstants.InclusiveGatewayElement:
          BuildGatewayNode(shape, bounds, GatewayType.Inclusive);
          break;
        case BpmnDiConstants.EventBasedGatewayElement:
          if (shape.GetAttribute("eventGatewayType") == "Exclusive") {
            BuildGatewayNode(shape, bounds, GatewayType.ExclusiveEventBased);
          } else if (shape.GetAttribute("eventGatewayType") == "Parallel") {
            BuildGatewayNode(shape, bounds, GatewayType.ParallelEventBased);
          } else {
            BuildGatewayNode(shape, bounds, GatewayType.EventBased);
          }
          break;
        case BpmnDiConstants.ComplexGatewayElement:
          BuildGatewayNode(shape, bounds, GatewayType.Complex);
          break;

        // Activities - Tasks
        case BpmnDiConstants.TaskElement:
          BuildTaskNode(shape, bounds, TaskType.Abstract);
          break;
        case BpmnDiConstants.UserTaskElement:
          BuildTaskNode(shape, bounds, TaskType.User);
          break;
        case BpmnDiConstants.ManualTaskElement:
          BuildTaskNode(shape, bounds, TaskType.Manual);
          break;
        case BpmnDiConstants.ServiceTaskElement:
          BuildTaskNode(shape, bounds, TaskType.Service);
          break;
        case BpmnDiConstants.ScriptTaskElement:
          BuildTaskNode(shape, bounds, TaskType.Script);
          break;
        case BpmnDiConstants.SendTaskElement:
          BuildTaskNode(shape, bounds, TaskType.Send);
          break;
        case BpmnDiConstants.ReceiveTaskElement:
          BuildTaskNode(shape, bounds, TaskType.Receive);
          break;
        case BpmnDiConstants.BusinessRuleTaskElement:
          BuildTaskNode(shape, bounds, TaskType.BusinessRule);
          break;

        // Activities - subProcess
        case BpmnDiConstants.SubProcessElement:
          if (shape.GetAttribute(BpmnDiConstants.TriggeredByEventAttribute) == "true") {
            BuildSubProcessNode(shape, bounds, ActivityType.EventSubProcess);
          } else {
            BuildSubProcessNode(shape, bounds, ActivityType.SubProcess);
          }
          break;

        // Activities - Ad-Hoc Sub-Process 
        case BpmnDiConstants.AdHocSubProcessElement:
          if (shape.GetAttribute(BpmnDiConstants.TriggeredByEventAttribute) == "true") {
            BuildSubProcessNode(shape, bounds, ActivityType.EventSubProcess);
          } else {
            BuildSubProcessNode(shape, bounds, ActivityType.SubProcess);
          }
          break;

        // Activities - Transaction
        case BpmnDiConstants.TransactionElement:
          BuildSubProcessNode(shape, bounds, ActivityType.Transaction);
          break;

        // Activities - callActivity
        case BpmnDiConstants.CallActivityElement:
          BuildSubProcessNode(shape, bounds, ActivityType.CallActivity);
          break;

        //Events
        case BpmnDiConstants.StartEventElement:
          if (shape.GetAttribute(BpmnDiConstants.IsInterruptingAttribute) == "true") {
            BuildEventNode(shape, bounds, EventCharacteristic.SubProcessInterrupting);
          } else if (shape.GetAttribute(BpmnDiConstants.IsInterruptingAttribute) == "false") {
            BuildEventNode(shape, bounds, EventCharacteristic.SubProcessNonInterrupting);
          } else {
            BuildEventNode(shape, bounds, EventCharacteristic.Start);
          }
          break;
        case BpmnDiConstants.EndEventElement:
          BuildEventNode(shape, bounds, EventCharacteristic.End);
          break;
        case BpmnDiConstants.BoundaryEventElement:
          // Boundary Events are realized as Ports instead of Nodes
          BuildBoundaryEvent(shape);
          break;
        case BpmnDiConstants.IntermediateThrowEventElement:
          BuildEventNode(shape, bounds, EventCharacteristic.Throwing);
          break;
        case BpmnDiConstants.IntermediateCatchEventElement:
          BuildEventNode(shape, bounds, EventCharacteristic.Catching);
          break;

        // Conversation
        case BpmnDiConstants.ConversationElement:
          BuildConversationNode(shape, bounds, ConversationType.Conversation, null);
          break;
        case BpmnDiConstants.CallConversationElement:
          BpmnElement refElement;
          if (Document.Elements.TryGetValue(shape.GetAttribute(BpmnDiConstants.CalledCollaborationRefAttribute),
              out refElement)) {
            switch (refElement.Name) {
              case BpmnDiConstants.CollaborationElement:
                BuildConversationNode(shape, bounds, ConversationType.CallingCollaboration, refElement);
                break;
              case BpmnDiConstants.GlobalConversationElement:
                BuildConversationNode(shape, bounds, ConversationType.CallingGlobalConversation, refElement);
                break;
              default:
                // This should not happen under strict conformance
                BuildConversationNode(shape, bounds, ConversationType.Conversation, refElement);
                break;
            }
          }
          break;
        case BpmnDiConstants.SubConversationElement:
          BuildConversationNode(shape, bounds, ConversationType.SubConversation, null);
          break;

        // Choreography
        case BpmnDiConstants.ChoreographyTaskElement:
        case BpmnDiConstants.SubChoreographyElement:
        case BpmnDiConstants.CallChoreographyElement:
          BuildChoreographyNode(shape, bounds);
          break;

        // Participants 
        case BpmnDiConstants.ParticipantElement:
          var parent = originalElement.Parent;
          // If the participant is not part of a choreography node, create a node
          if (parent.Name.ToLower().IndexOf("choreography") == -1) {
            BuildParticipantNode(shape, bounds);
          } else if (parent.Node != null) {
            // Else add it to the appropriate choreography
            BuildParticipantLabel(shape);
          }
          break;
        case "participantRef":

          break;
        // Text Annotations 
        case BpmnDiConstants.TextAnnotationElement:
          BuildTextAnnotationNode(shape, bounds);
          break;

        // Groups
        case BpmnDiConstants.GroupElement:
          BuildGroupNode(shape, bounds);
          break;

        // DataObject
        case BpmnDiConstants.DataObjectReferenceElement:
          // Find out, if the data Object is a collection
          var collection = false;
          BpmnElement dataObject;
          if (TryGetElementForId(shape.GetAttribute(BpmnDiConstants.DataObjectRefAttribute), out dataObject)) {
            string collectionString;
            if (dataObject.Attributes.TryGetValue(BpmnDiConstants.IsCollectionAttribute, out collectionString)) {
              if (collectionString == "true") {
                collection = true;
              }
            }
          }
          BuildDataObjectNode(shape, bounds, DataObjectType.None, collection);
          break;
        case BpmnDiConstants.DataInputElement:
          // Find out, if the data Object is a collection
          collection = shape.GetAttribute(BpmnDiConstants.IsCollectionAttribute) == "true";
          BuildDataObjectNode(shape, bounds, DataObjectType.Input, collection);
          break;
        case BpmnDiConstants.DataOutputElement:
          // Find out, if the data Object is a collection
          collection = shape.GetAttribute(BpmnDiConstants.IsCollectionAttribute) == "true";
          BuildDataObjectNode(shape, bounds, DataObjectType.Output, collection);
          break;

        // DataStore
        case BpmnDiConstants.DataStoreReferenceElement:
          BuildDataStoreReferenceNode(shape, bounds);
          break;
      }
      var iNode = shape.Element.Node;
      if (iNode != null) {
        SetNodeTag(shape, iNode);
      }
    }

    /// <summary>
    /// Creates an <see cref="IEdge"/> on the graph
    /// </summary>
    /// <param name="edge">The <see cref="BpmnEdge"/> to draw. </param>
    private void BuildEdge(BpmnEdge edge) {
      var element = edge.Element;
      var source = edge.Source;
      IEdge iEdge = null;
      switch (element.Name) {
        case BpmnDiConstants.SequenceFlowElement:
          if (element.GetChild(BpmnDiConstants.ConditionExpressionElement) != null && !(source.Name.EndsWith(BpmnDiConstants.GatewaySuffix))) {
            iEdge = BuildDefaultEdge(edge, EdgeType.ConditionalFlow);
          } else if (source != null && source.GetValue("default") == element.Id) {
            iEdge = BuildDefaultEdge(edge, EdgeType.DefaultFlow);
          } else {
            iEdge = BuildDefaultEdge(edge, EdgeType.SequenceFlow);
          }
          break;
        case BpmnDiConstants.AssociationElement:
          switch (edge.GetAttribute(BpmnDiConstants.AssociationDirectionAttribute)) {
            case "None":
              iEdge = BuildDefaultEdge(edge, EdgeType.Association);
              break;
            case "One":
              iEdge = BuildDefaultEdge(edge, EdgeType.DirectedAssociation);
              break;
            case "Both":
              iEdge = BuildDefaultEdge(edge, EdgeType.BidirectedAssociation);
              break;
            default:
              // This shouldn't happen under strict conformance
              iEdge = BuildDefaultEdge(edge, EdgeType.Association);
              break;
          }
          break;
        case BpmnDiConstants.DataAssociationElement:
          iEdge = BuildDefaultEdge(edge, EdgeType.Association);
          break;
        case BpmnDiConstants.ConversationLinkElement:
          iEdge = BuildDefaultEdge(edge, EdgeType.Conversation);
          break;
        case BpmnDiConstants.MessageFlowElement:
          iEdge = BuildMessageFlow(edge);
          break;
        case BpmnDiConstants.DataInputAssociationElement:
          iEdge = BuildDefaultEdge(edge, EdgeType.DirectedAssociation);
          break;
        case BpmnDiConstants.DataOutputAssociationElement:
          iEdge = BuildDefaultEdge(edge, EdgeType.DirectedAssociation);
          break;
      }
      if (iEdge != null) {
        // Create label & set style
        AddEdgeLabel(edge);

        SetEdgeTag(edge, iEdge);
      }
    }

    /// <summary>
    /// Callback to add some of the <see cref="BpmnShape"/> or <see cref="BpmnElement"/>
    /// data to the <see cref="ITagOwner.Tag"/> of <paramref name="iNode"/>.
    /// </summary>
    /// <param name="shape">The bpmn shape used to create the node.</param>
    /// <param name="iNode">The node whose tag shall be filled.</param>
    internal void SetNodeTag(BpmnShape shape, INode iNode) {
      var extensionElements = shape.Element.GetChild("extensionElements");
      if (extensionElements != null) {
        var toolTipText = "";
        foreach (var foreignChild in extensionElements.ForeignChildren) {
          toolTipText += foreignChild + "\n";
        }
        iNode.Tag = toolTipText;
      }
    }

    /// <summary>
    /// Callback to add some of the <see cref="BpmnEdge"/> or <see cref="BpmnElement"/>
    /// data to the <see cref="ITagOwner.Tag"/> of <paramref name="iEdge"/>.
    /// </summary>
    /// <param name="edge">The bpmn edge used to create the edge.</param>
    /// <param name="iEdge">The edge whose tag shall be filled.</param>
    internal void SetEdgeTag(BpmnEdge edge, IEdge iEdge) {
      var elementExtensions = edge.Element.GetChild("elementExtensions");
      if (elementExtensions != null) {
        var toolTipText = "";
        foreach (var foreignChild in elementExtensions.ForeignChildren) {
          toolTipText += foreignChild + "\n";
        }
        iEdge.Tag = toolTipText;
      }
    }

    #endregion

    #region Building Shapes

    // Builds a Gateway node
    private void BuildGatewayNode(BpmnShape shape, RectD bounds, GatewayType type) {
      var node = MasterGraph.CreateNode(bounds);
      var element = shape.Element;
      SetParent(node, element.Parent.Node);
      element.Node = node;

      // dataAssociations point to invisible children of activities, therefore, the INode has to be linked there
      element.SetINodeInputOutput(node);

      //Add Style
      var gatewayStyle = new GatewayNodeStyle { Type = type };
      MasterGraph.SetStyle(node, gatewayStyle);

      //Add Label
      var label = AddNodeLabel(node, shape);
      if (shape.HasLabelPosition()) {
        SetFixedBoundsLabelStyle(label, shape.LabelBounds);
      } else {
        SetExternalLabelStyle(label);
        if (shape.HasLabelSize()) {
          MasterGraph.SetLabelPreferredSize(label, shape.LabelBounds.Size);
        }
      }
    }

    // Builds a Task node
    private void BuildTaskNode(BpmnShape shape, RectD bounds, TaskType type) {
      var node = MasterGraph.CreateNode(bounds);
      var element = shape.Element;
      SetParent(node, element.Parent.Node);
      element.Node = node;

      // dataAssociations point to invisible children of activities, therefore, the INode has to be linked there
      element.SetINodeInputOutput(node);

      //Add Style
      var activityStyle = new ActivityNodeStyle {
          Compensation = shape.GetAttribute("isForCompensation") == "true",
          LoopCharacteristic = element.GetLoopCharacteristics(),
          ActivityType = ActivityType.Task,
          TaskType = type
      };

      MasterGraph.SetStyle(node, activityStyle);

      //Add Label
      var label = AddNodeLabel(node, shape);
      SetInternalLabelStyle(label);
    }

    // Builds a SubProcess node
    private void BuildSubProcessNode(BpmnShape shape, RectD bounds, ActivityType type) {
      var node = MasterGraph.CreateNode(bounds);
      var element = shape.Element;

      // All SubProcess have to be GroupNodes, so they can be collapsed/expanded
      MasterGraph.SetIsGroupNode(node, true);

      SetParent(node, element.Parent.Node);
      element.Node = node;
      BpmnElement calledElement;

      // If this subProcess is a callActivity and calls an existing process, link the Node there aswell
      if (element.CalledElement != null) {
        if (TryGetElementForId(element.CalledElement, out calledElement)) {
          calledElement.Node = node;
        }
      }

      // dataAssociations point to invisible children of activities, therefore, the INode has to be linked there
      element.SetINodeInputOutput(node);

      var activityStyle = new ActivityNodeStyle {
          Compensation = shape.GetAttribute(BpmnDiConstants.IsForCompensationAttribute) == "true",
          // Get Loop Characteristics
          LoopCharacteristic = element.GetLoopCharacteristics()
      };

      // Get, if the subProcess is expanded
      var label = AddNodeLabel(node, shape);
      SetSubProcessLabelStyle(label);
      activityStyle.ActivityType = type;
      activityStyle.TriggerEventType = GetEventType(shape);

      if (shape.GetAttribute(BpmnDiConstants.IsInterruptingAttribute) == "true") {
        activityStyle.TriggerEventCharacteristic = EventCharacteristic.SubProcessInterrupting;
      } else {
        activityStyle.TriggerEventCharacteristic = EventCharacteristic.SubProcessNonInterrupting;
      }
      activityStyle.SubState = SubState.Dynamic;

      MasterGraph.SetStyle(node, activityStyle);
    }

    // Builds an Event node
    private void BuildEventNode(BpmnShape shape, RectD bounds, EventCharacteristic characteristic) {
      var node = MasterGraph.CreateNode(bounds);
      var element = shape.Element;
      SetParent(node, element.Parent.Node);
      element.Node = node;

      // dataAssociations point to invisible children of activities, therefore, the INode has to be linked there
      element.SetINodeInputOutput(node);

      // Add Style
      var eventStyle = new EventNodeStyle { Type = GetEventType(shape), Characteristic = characteristic };
      MasterGraph.SetStyle(node, eventStyle);

      // Add Label
      var label = AddNodeLabel(node, shape);
      if (shape.HasLabelPosition()) {
        SetFixedBoundsLabelStyle(label, shape.LabelBounds);
      } else {
        SetExternalLabelStyle(label);
        if (shape.HasLabelSize()) {
          MasterGraph.SetLabelPreferredSize(label, shape.LabelBounds.Size);
        }
      }
    }

    // Builds a Boundary Event, realized as a port instead of a node
    private void BuildBoundaryEvent(BpmnShape shape) {
      BpmnElement parent;
      TryGetElementForId(shape.GetAttribute(BpmnDiConstants.AttachedToRefAttribute), out parent);
      var portStyle = new EventPortStyle {
          Type = GetEventType(shape),
          Characteristic = shape.GetAttribute(BpmnDiConstants.CancelActivityAttribute) == "false"
              ? EventCharacteristic.BoundaryNonInterrupting
              : EventCharacteristic.BoundaryInterrupting
      };
      if (parent == null) {
        throw new ArgumentException("Shape with no parent", "shape");
      }
      if (parent.Node == null) {
        if (!this.delayedBoundaryEvents.Contains(shape)) {
          this.delayedBoundaryEvents.Add(shape);
        }
        Document.Messages.Add("The node for boundaryEvent " + shape.Id + " was not (yet) created!");
        return;
      }

      var element = shape.Element;

      // dataAssociations point to invisible children of Tasks, therefore, the INode has to be linked there 
      element.SetINodeInputOutput(parent.Node);

      var port = MasterGraph.AddPort(parent.Node,
          new PointD(shape.X + shape.Width / 2, shape.Y + shape.Height / 2), portStyle);
      element.Port = port;
      element.Node = parent.Node;
      var label = AddNodeLabel(port, shape);

      if (shape.HasLabelPosition()) {
        SetFixedBoundsLabelStyle(label, shape.LabelBounds);
      } else {
        MasterGraph.SetStyle(label, new DefaultLabelStyle());
        if (shape.HasLabelSize()) {
          MasterGraph.SetLabelPreferredSize(label, shape.LabelBounds.Size);
        } else {
          var outsideModel = new InsideOutsidePortLabelModel { Distance = 10 };
          MasterGraph.SetLabelLayoutParameter(label, outsideModel.CreateOutsideParameter());
        }
      }
    }

    // Builds a Conversation node
    private void BuildConversationNode(BpmnShape shape, RectD bounds, ConversationType type, BpmnElement refElement) {
      var node = MasterGraph.CreateNode(bounds);
      var element = shape.Element;
      SetParent(node, element.Parent.Node);

      element.Node = node;

      // dataAssociations point to invisible children of Tasks, therefore, the INode has to be linked there 
      element.SetINodeInputOutput(node);

      // Add Style
      var conversationStyle = new ConversationNodeStyle { Type = type };
      MasterGraph.SetStyle(node, conversationStyle);

      // Add Label
      var label = AddNodeLabel(node, shape);
      if (shape.HasLabelPosition()) {
        SetFixedBoundsLabelStyle(label, shape.LabelBounds);
      } else {
        SetExternalLabelStyle(label);
        if (shape.HasLabelSize()) {
          MasterGraph.SetLabelPreferredSize(label, shape.LabelBounds.Size);
        }
      }
    }

    // Builds a Choreography node
    private void BuildChoreographyNode(BpmnShape shape, RectD bounds) {
      var node = MasterGraph.CreateGroupNode(view.LocalRoot, bounds);
      var element = shape.Element;
      SetParent(node, element.Parent.Node);
      element.Node = node;

      // dataAssociations point to invisible children of Tasks, therefore, the INode has to be linked there 
      element.SetINodeInputOutput(node);

      var choreographyStyle =
          new ChoreographyNodeStyle { LoopCharacteristic = element.GetLoopCharacteristics() };

      //Get Loop Characteristics
      element.TopParticipants = 0;
      element.BottomParticipants = 0;
      var label = AddNodeLabel(node, shape);

      // Get SubState
      if (shape.IsExpanded == "true") {
        choreographyStyle.SubState = SubState.None;
        MasterGraph.SetStyle(node, choreographyStyle);
      } else if (shape.IsExpanded == "false") {
        choreographyStyle.SubState = SubState.Dynamic;
        MasterGraph.SetStyle(node, choreographyStyle);
      } else {
        MasterGraph.SetStyle(node, choreographyStyle);
      }

      SetChoreographyLabelStyle(label);
    }

    // Builds a dataObject Node
    private void BuildDataObjectNode(BpmnShape shape, RectD bounds, DataObjectType type, bool isCollection) {
      var node = MasterGraph.CreateNode(bounds);
      var element = shape.Element;
      SetParent(node, element.Parent.Node);
      element.Node = node;

      // dataAssociations point to invisible children of Tasks, therefore, the INode has to be linked there 
      element.SetINodeInputOutput(node);

      var objectStyle = new DataObjectNodeStyle { Type = type, Collection = isCollection };

      MasterGraph.SetStyle(node, objectStyle);

      var label = AddNodeLabel(node, shape);

      if (shape.HasLabelPosition()) {
        SetFixedBoundsLabelStyle(label, shape.LabelBounds);
      } else {
        SetExternalLabelStyle(label);
        if (shape.HasLabelSize()) {
          MasterGraph.SetLabelPreferredSize(label, shape.LabelBounds.Size);
        }
      }
    }

    // Builds a participant node (actually a pool)
    private void BuildParticipantNode(BpmnShape shape, RectD bounds) {
      var element = shape.Element;
      var processRef = element.Process;
      BpmnElement processElement = null;
      if (processRef == null || !TryGetElementForId(processRef, out processElement) || processElement.GetChild("laneSet") == null) {
        // not connected to a process so we need our own node

        var node = MasterGraph.CreateNode(bounds);
        SetParent(node, element.Parent.Node);
        element.Node = node;
        if (processElement != null) {
          processElement.Node = node;
        }

        // dataAssociations point to invisible children of Tasks, therefore, the INode has to be linked there 
        element.SetINodeInputOutput(node);

        var partStyle = CreateTable(shape);
        if (element.HasChild(BpmnDiConstants.ParticipantMultiplicityElement)) {
          if (int.Parse(element.GetChildAttribute(BpmnDiConstants.ParticipantMultiplicityElement, "maximum")) > 1) {
            partStyle.MultipleInstance = true;
          }
        }

        MasterGraph.SetStyle(node, partStyle);

        var table = partStyle.TableNodeStyle.Table;
        if (shape.IsHorizontal ?? false) {
          var row = table.RootRow.ChildRows.First();
          AddTableLabel(table, row, shape);
        } else {
          var column = table.RootColumn.ChildColumns.First();
          AddTableLabel(table, column, shape);
        }
      }
    }

    // Builds a participant label inside a choreography node
    private void BuildParticipantLabel(BpmnShape shape) {
      var choreography = CurrentDiagram.Plane.GetShape(shape.ChoreographyActivityShape).Element;
      var node = choreography.Node;
      var top = false;
      var index = 0;
      var choreographyNodeStyle = ((ChoreographyNodeStyle) node.Style);
      var multipleInstance = false;
      var element = shape.Element;

      if (element.HasChild(BpmnDiConstants.ParticipantMultiplicityElement)) {
        if (int.Parse(element.GetChildAttribute(BpmnDiConstants.ParticipantMultiplicityElement, "maximum")) > 1) {
          multipleInstance = true;
        }
      }
      var participant = new Participant { MultiInstance = multipleInstance };
      var label = AddParticipantLabel(node, shape);
      switch (shape.PartBandKind) {
        case ParticipantBandKind.TopInitiating:
          if (shape.IsMessageVisible)
            choreographyNodeStyle.InitiatingMessage = true;
          choreographyNodeStyle.InitiatingAtTop = true;
          choreographyNodeStyle.TopParticipants.Add(participant);
          top = true;
          index = choreography.TopParticipants++;
          break;
        case ParticipantBandKind.TopNonInitiating:
          if (shape.IsMessageVisible)
            choreographyNodeStyle.ResponseMessage = true;
          choreographyNodeStyle.TopParticipants.Add(participant);
          top = true;
          index = choreography.TopParticipants++;
          break;
        case ParticipantBandKind.BottomInitiating:
          if (shape.IsMessageVisible)
            choreographyNodeStyle.InitiatingMessage = true;
          choreographyNodeStyle.InitiatingAtTop = false;
          choreographyNodeStyle.BottomParticipants.Add(participant);
          index = choreography.BottomParticipants++;
          break;
        case ParticipantBandKind.BottomNonInitiating:
          if (shape.IsMessageVisible)
            choreographyNodeStyle.ResponseMessage = true;
          choreographyNodeStyle.BottomParticipants.Add(participant);
          index = choreography.BottomParticipants++;
          break;
        case ParticipantBandKind.MiddleInitiating:
          // This shouldn't happen under strict conformance
          if (shape.IsMessageVisible)
            choreographyNodeStyle.InitiatingMessage = true;
          if (choreography.TopParticipants < choreography.BottomParticipants) {
            top = true;
            index = choreography.TopParticipants++;
            choreographyNodeStyle.InitiatingAtTop = true;
            choreographyNodeStyle.TopParticipants.Add(participant);
          } else {
            index = choreography.BottomParticipants++;
            choreographyNodeStyle.InitiatingAtTop = false;
            choreographyNodeStyle.BottomParticipants.Add(participant);
          }
          break;
        case ParticipantBandKind.MiddleNonInitiating:
          if (shape.IsMessageVisible)
            choreographyNodeStyle.ResponseMessage = true;
          if (choreography.TopParticipants < choreography.BottomParticipants) {
            top = true;
            index = choreography.TopParticipants++;
            choreographyNodeStyle.TopParticipants.Add(participant);
          } else {
            index = choreography.BottomParticipants++;
            choreographyNodeStyle.BottomParticipants.Add(participant);
          }
          break;
      }
      element.Node = node;

      // Sets the label Style of the new participant
      var parameter = ChoreographyLabelModel.Instance.CreateParticipantParameter(top, index);
      MasterGraph.SetLabelLayoutParameter(label, parameter);
      var defaultLabelStyle = SetCustomLabelStyle(label);
      MasterGraph.SetStyle(label, defaultLabelStyle);

      // checks, if there is a message, if yes, tries to set text label
      if (shape.IsMessageVisible && choreography.HasChild(BpmnDiConstants.MessageFlowRefElement)) {
        var children = choreography.GetChildren(BpmnDiConstants.MessageFlowRefElement);

        foreach (var child in children) {
          BpmnElement messageFlow;
          if (TryGetElementForId(child.Value, out messageFlow)) {
            if (messageFlow.Source == element.Id) {
              var message = messageFlow.Label ?? "";
              label = MasterGraph.AddLabel(node, message, null, null, null, shape.LabelStyle);
              if (top) {
                parameter = ChoreographyLabelModel.NorthMessage;
              } else {
                parameter = ChoreographyLabelModel.SouthMessage;
              }
              MasterGraph.SetLabelLayoutParameter(label, parameter);
              defaultLabelStyle = SetCustomLabelStyle(label);
              MasterGraph.SetStyle(label, defaultLabelStyle);
              break;
            }
          }
        }
      }
    }

    // Builds a TextAnnotationNode
    private void BuildTextAnnotationNode(BpmnShape shape, RectD bounds) {
      var node = MasterGraph.CreateNode(bounds);
      var element = shape.Element;
      SetParent(node, element.Parent.Node);
      element.Node = node;

      // Add Style
      var annotationStyle = new AnnotationNodeStyle();
      MasterGraph.SetStyle(node, annotationStyle);

      // Add Label
      var label = AddNodeLabel(node, shape);
      SetInternalLabelStyle(label);
    }

    // Builds a Group Node
    private void BuildGroupNode(BpmnShape shape, RectD bounds) {
      var element = shape.Element;
      var node = MasterGraph.CreateGroupNode(element.Parent.Node, bounds, new GroupNodeStyle());
      element.Node = node;

      // Before Adding a Label, we need to get the Label Text, which is located in a categoryValue
      // The id of this category value is located in the Label
      foreach (var childElement in Document.Elements) {
        if (childElement.Value.Id == element.Label && childElement.Value.Attributes.ContainsKey("value")) {
          element.Label = childElement.Value.Attributes["value"];
          break;
        }
      }

      // Add Label
      var label = AddNodeLabel(node, shape);
      SetGroupLabelStyle(label);
    }

    // Builds a DataStoreReference Node
    private void BuildDataStoreReferenceNode(BpmnShape shape, RectD bounds) {
      var node = MasterGraph.CreateNode(bounds);
      var element = shape.Element;
      SetParent(node, element.Parent.Node);
      element.Node = node;

      // dataAssociations point to invisible children of Tasks, therefore, the INode has to be linked there 
      element.SetINodeInputOutput(node);

      // Add Style
      var dataStoreStyle = new DataStoreNodeStyle();
      MasterGraph.SetStyle(node, dataStoreStyle);

      // Add Label
      var label = AddNodeLabel(node, shape);
      if (shape.HasLabelPosition()) {
        SetFixedBoundsLabelStyle(label, shape.LabelBounds);
      } else {
        SetExternalLabelStyle(label);
        if (shape.HasLabelSize()) {
          MasterGraph.SetLabelPreferredSize(label, shape.LabelBounds.Size);
        }
      }
    }

    // Retrieves the correct EventType, returns EventNodeStyle with the EventType set accordingly
    private EventType GetEventType(BpmnShape shape) {
      var eventType = EventType.Plain;
      var element = shape.Element;

      if (element.HasChild(BpmnDiConstants.MessageEventDefinitionElement)) {
        eventType = EventType.Message;
      }
      if (element.HasChild(BpmnDiConstants.TimerEventDefinitionElement)) {
        eventType = EventType.Timer;
      }
      if (element.HasChild(BpmnDiConstants.TerminateEventDefinitionElement)) {
        eventType = EventType.Terminate;
      }
      if (element.HasChild(BpmnDiConstants.EscalationEventDefinitionElement)) {
        eventType = EventType.Escalation;
      }
      if (element.HasChild(BpmnDiConstants.ErrorEventDefinitionElement)) {
        eventType = EventType.Error;
      }
      if (element.HasChild(BpmnDiConstants.ConditionalEventDefinitionElement)) {
        eventType = EventType.Conditional;
      }
      if (element.HasChild(BpmnDiConstants.CompensateEventDefinitionElement)) {
        eventType = EventType.Compensation;
      }
      if (element.HasChild(BpmnDiConstants.CancelEventDefinitionElement)) {
        eventType = EventType.Cancel;
      }
      if (element.HasChild(BpmnDiConstants.LinkEventDefinitionElement)) {
        eventType = EventType.Link;
      }
      if (element.HasChild(BpmnDiConstants.SignalEventDefinitionElement)) {
        eventType = EventType.Signal;
      }
      if (element.HasChild(BpmnDiConstants.MultipleEventDefinitionElement)) {
        eventType = EventType.Multiple;
      }
      if (element.HasChild(BpmnDiConstants.ParallelEventDefinitionElement)) {
        eventType = EventType.ParallelMultiple;
      }
      return eventType;
    }

    // Sets the parentNode of a Node, if the parentNode is part of the current Graph
    private void SetParent(INode node, INode parentNode) {
      if (MasterGraph.Contains(parentNode)) {
        MasterGraph.SetParent(node, parentNode);
      }
    }

    #endregion

    #region Building Labels

    // Adds a label to a node
    private ILabel AddNodeLabel(ILabelOwner owner, BpmnShape shape) {
      // blank label, in case we added none
      var name = shape.Element.Label;
      // only has label name, if we added one before
      if (!shape.HasLabel && !ParseAllLabels || name == null) {
        name = "";
      }
      var label = MasterGraph.AddLabel(owner, name, null, null, null, shape.LabelStyle);

      return label;
    }

    // Adds a label to a participant
    private ILabel AddParticipantLabel(ILabelOwner owner, BpmnShape shape) {
      // blank label, in case we added none
      var name = shape.Element.Label;
      // only has label name, if we added one before
      if (!shape.HasLabel && !ParseAllLabels || name == null) {
        name = "";
      }
      var label = MasterGraph.AddLabel(owner, name, null, null, null, shape.LabelStyle);
      return label;
    }

    // Adds a label to a table object (rows/colums)
    private void AddTableLabel(ITable table, IStripe owner, BpmnShape shape) {
      // blank label, in case we added none
      var name = shape.Element.Label;
      // only has label name, if we added one before
      if (!shape.HasLabel && !ParseAllLabels || name == null) {
        name = "";
      }
      table.AddLabel(owner, name, null, null, null, shape.LabelStyle);
    }

    // Adds a label to an edge
    private void AddEdgeLabel(BpmnEdge edge) {
      // blank label, in case we added none
      var name = edge.Element.Label;
      // only has label name, if we added one before
      if (!edge.HasLabel && !ParseAllLabels || name == null) {
        name = "";
      }
      if (name != "") {
        var label = MasterGraph.AddLabel(edge.Element.Edge, name, null, null, null, edge.LabelStyle);

        if (edge.HasLabelPosition()) {
          SetFixedBoundsLabelStyle(label, edge.LabelBounds);
        } else {
          SetEdgeLabelStyle(label);
          if (edge.HasLabelSize()) {
            MasterGraph.SetLabelPreferredSize(label, edge.LabelBounds.Size);
          }
        }
      }
    }

    // Sets label style, if there are fixed bounds for this label
    private void SetFixedBoundsLabelStyle(ILabel label, RectD bounds) {
      ILabelModelParameter parameter = null;
      if (label.Owner is INode) {
        var model = new FreeNodeLabelModel();
        parameter = model.FindBestParameter(label, model, new OrientedRectangle(bounds));
      } else {
        var model = new FreeEdgeLabelModel();
        parameter = model.FindBestParameter(label, model, new OrientedRectangle(bounds));
      }
      MasterGraph.SetLabelLayoutParameter(label, parameter);
      var defaultLabelStyle = SetCustomLabelStyle(label);
      MasterGraph.SetStyle(label, defaultLabelStyle);
      MasterGraph.SetLabelPreferredSize(label, new SizeD(bounds.Width, bounds.Height));
    }

    // Sets label style for tasks (Centered)
    private void SetInternalLabelStyle(ILabel label) {
      var model = new InteriorStretchLabelModel { Insets = new InsetsD(3, 3, 3, 3) };
      MasterGraph.SetLabelLayoutParameter(label, model.CreateParameter(InteriorStretchLabelModel.Position.Center));
      var defaultLabelStyle = SetCustomLabelStyle(label);
      defaultLabelStyle.TextAlignment = TextAlignment.Center;
      defaultLabelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      defaultLabelStyle.TextWrapping = TextWrapping.Wrap;
      MasterGraph.SetStyle(label, defaultLabelStyle);
    }

    // Sets label style nodes that have an external label (South of the node)
    private void SetExternalLabelStyle(ILabel label) {
      MasterGraph.SetLabelLayoutParameter(label, GenericLabelModel.CreateDefaultParameter());
      var defaultLabelStyle = SetCustomLabelStyle(label);
      defaultLabelStyle.TextAlignment = TextAlignment.Center;
      defaultLabelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      MasterGraph.SetStyle(label, defaultLabelStyle);

      if (MultiLineExteriorNodeLabels) {
        // Set some bounds to make labels multi - row
        var maxWidth = Math.Max(((INode) label.Owner).Layout.Width * 1.5, 100);
        var maxHeight = label.PreferredSize.Height;
        var width = maxWidth;
        var height = maxHeight;
        while (width < label.PreferredSize.Width) {
          maxHeight += height;
          width += maxWidth;
        }
        MasterGraph.SetLabelPreferredSize(label, new SizeD(maxWidth, maxHeight));
      }
    }

    // Sets label style for the TaskNameBand in a Choreography
    private void SetChoreographyLabelStyle(ILabel label) {
      MasterGraph.SetLabelLayoutParameter(label, ChoreographyLabelModel.TaskNameBand);
      var defaultLabelStyle = SetCustomLabelStyle(label);
      defaultLabelStyle.TextWrapping = TextWrapping.Wrap;
      defaultLabelStyle.TextAlignment = TextAlignment.Center;
      defaultLabelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      MasterGraph.SetStyle(label, defaultLabelStyle);
    }

    // Sets label style for SubProcesses (Upper left corner)
    private void SetSubProcessLabelStyle(ILabel label) {
      var model = new InteriorStretchLabelModel { Insets = new InsetsD(3, 3, 3, 3) };
      MasterGraph.SetLabelLayoutParameter(label, model.CreateParameter(InteriorStretchLabelModel.Position.North));
      var defaultLabelStyle = SetCustomLabelStyle(label);
      defaultLabelStyle.TextAlignment = TextAlignment.Left;
      defaultLabelStyle.VerticalTextAlignment = VerticalAlignment.Top;
      MasterGraph.SetStyle(label, defaultLabelStyle);
    }

    // Sets label style for Groups (Upper boundary)
    private void SetGroupLabelStyle(ILabel label) {
      var model = new InteriorStretchLabelModel { Insets = new InsetsD(3, 3, 3, 3) };
      MasterGraph.SetLabelLayoutParameter(label, model.CreateParameter(InteriorStretchLabelModel.Position.North));
      var defaultLabelStyle = SetCustomLabelStyle(label);
      defaultLabelStyle.TextAlignment = TextAlignment.Center;
      MasterGraph.SetStyle(label, defaultLabelStyle);
    }

    // Sets edge label style 
    private void SetEdgeLabelStyle(ILabel label) {
      if (label != null) {
        var model = new EdgePathLabelModel {
            Offset = 7,
            SideOfEdge = EdgeSides.AboveEdge,
            AutoRotation = false
        };
        MasterGraph.SetLabelLayoutParameter(label, model.CreateDefaultParameter());
        var defaultLabelStyle = SetCustomLabelStyle(label);
        defaultLabelStyle.TextAlignment = TextAlignment.Center;
        MasterGraph.SetStyle(label, defaultLabelStyle);
      }
    }

    // Sets custom style elements
    private DefaultLabelStyle SetCustomLabelStyle(ILabel label) {
      var styleName = (string) label.Tag;
      return CurrentDiagram.GetStyle(styleName);
    }

    #endregion

    #region Building Edges

    // Builds all edges, except for message flows
    private IEdge BuildDefaultEdge(BpmnEdge edge, EdgeType type) {
      var sourceVar = edge.Source;
      var targetVar = edge.Target;
      var waypoints = edge.Waypoints;
      var id = edge.Id;

      // Check, if source and target were correctly parsed
      if (sourceVar == null) {
        Document.Messages.Add("Edge " + (id) + " has no valid Source.");
        return null;
      }
      if (targetVar == null) {
        Document.Messages.Add("Edge " + (id) + " has no valid Target.");
        return null;
      }

      // Get source & target node
      var sourceNode = sourceVar.Node;
      var targetNode = targetVar.Node;

      // Get bends & ports from waypoints
      var count = waypoints.Count;
      var source = sourceNode.Layout.GetCenter();
      var target = targetNode.Layout.GetCenter();
      if (count > 0) {
        // First waypoint is source Port
        source = waypoints[0];
        // Last is target port
        target = waypoints[count - 1];
        waypoints.Remove(source);
        waypoints.Remove(target);
      }
      
      IPort sourcePort = null;
      IPort targetPort = null;

      // Use boundary event port, if source is a boundary event
      if (sourceVar.Name == BpmnDiConstants.BoundaryEventElement) {
        sourcePort = sourceVar.Port;
        if (sourcePort != null) {
          sourceNode = (INode) sourcePort.Owner;
        } else {
          Document.Messages.Add("The source boundary event for edge " + id + " was not (yet) created.");
          return null;
        }
      } else if (sourceNode != null) {
        sourcePort = MasterGraph.AddPort(sourceNode, source);
      }

      // Use boundary event port, if target is a boundary event
      if (targetVar.Name == BpmnDiConstants.BoundaryEventElement) {
        targetPort = targetVar.Port;
        if (targetPort != null) {
          targetNode = (INode) targetPort.Owner;
        } else {
          Document.Messages.Add("The target boundary event for edge " + id + " was not (yet) created.");
          return null;
        }
      } else if (targetNode != null) {
        targetPort = MasterGraph.AddPort(targetNode, target);
      }

      // Test for textAnnotation, workaround fix for textAnnotations
      if (type == EdgeType.Association) {
        if (targetNode == null) {
          targetPort = MasterGraph.AddPort(sourceNode, target);
        }

        if (sourceNode == null) {
          sourcePort = MasterGraph.AddPort(targetNode, source);
        }
      }

      // Test if one of the ports is still null, notify user and carry on.
      if (sourcePort == null) {
        Document.Messages.Add("Edge " + (id) + " has no valid Source.");
        return null;
      }
      if (targetPort == null) {
        Document.Messages.Add("Edge " + (id) + " has no valid Target.");
        return null;
      }

      // Create edge on graph
      var iEdge = MasterGraph.CreateEdge(sourcePort, targetPort);
      foreach (var point in waypoints) {
        MasterGraph.AddBend(iEdge, point);
      }

      edge.Element.Edge = iEdge;

      // Set edge style
      var edgeStyle = new BpmnEdgeStyle { Type = type };
      MasterGraph.SetStyle(iEdge, edgeStyle);

      return iEdge;
    }

    // Builds MessageFlow edges
    private IEdge BuildMessageFlow(BpmnEdge edge) {
      var sourceVar = edge.Source;
      var targetVar = edge.Target;
      var waypoints = edge.Waypoints;
      var id = edge.Id;

      // Check, if source and target were correctly parsed
      if (sourceVar == null) {
        Document.Messages.Add("Edge " + (id) + " has no valid Source.");
        return null;
      }
      if (targetVar == null) {
        Document.Messages.Add("Edge " + (id) + " has no valid Target.");
        return null;
      }

      // Get source & target node
      var sourceNode = sourceVar.Node;
      var targetNode = targetVar.Node;

      // Get bends & ports from waypoints
      var count = waypoints.Count;
      var source = sourceNode.Layout.GetCenter();
      var target = targetNode.Layout.GetCenter();
      if (count > 0) {
        // First waypoint is source Port
        source = waypoints[0];
        // Last is target port
        target = waypoints[count - 1];
        waypoints.Remove(source);
        waypoints.Remove(target);
      }

      // Get source & target port
      var sourcePort = sourceVar.Name == BpmnDiConstants.BoundaryEventElement ? sourceVar.Port : MasterGraph.AddPort(sourceNode, source);
      var targetPort = targetVar.Name == BpmnDiConstants.BoundaryEventElement ? targetVar.Port : MasterGraph.AddPort(targetNode, target);

      var iEdge = MasterGraph.CreateEdge(sourcePort, targetPort);
      foreach (var point in waypoints) {
        MasterGraph.AddBend(iEdge, point);
      }
      edge.Element.Edge = iEdge;

      // If there is a message icon, add a corresponding label
      switch (edge.MessageVisibleK) {
        case MessageVisibleKind.Initiating:
          var messageLabel = MasterGraph.AddLabel(iEdge, "");
          MasterGraph.SetStyle(messageLabel, new MessageLabelStyle { IsInitiating = true });
          var model = new EdgeSegmentLabelModel() {
              SideOfEdge = EdgeSides.OnEdge,
              AutoRotation = false
          };
          MasterGraph.SetLabelPreferredSize(messageLabel, bpmnMessageSize);
          MasterGraph.SetLabelLayoutParameter(messageLabel, model.CreateParameterFromCenter());
          break;
        case MessageVisibleKind.NonInitiating:
          messageLabel = MasterGraph.AddLabel(iEdge, "");
          MasterGraph.SetStyle(messageLabel, new MessageLabelStyle { IsInitiating = false });
          model = new EdgeSegmentLabelModel() {
              SideOfEdge = EdgeSides.OnEdge,
              AutoRotation = false
          };
          MasterGraph.SetLabelPreferredSize(messageLabel, bpmnMessageSize);
          MasterGraph.SetLabelLayoutParameter(messageLabel, model.CreateParameterFromCenter());
          break;
        case MessageVisibleKind.Unspecified:
          break;
      }

      // Set edge style
      var edgeStyle = new BpmnEdgeStyle { Type = EdgeType.MessageFlow };
      MasterGraph.SetStyle(iEdge, edgeStyle);

      return iEdge;
    }

    #endregion

    #region Building Tables

    private INode BuildPool(BpmnElement element, BpmnPlane plane, INode localRoot) {
      var parent = element.Parent;
      while (parent.Name != BpmnDiConstants.ProcessElement && parent.Name != BpmnDiConstants.SubProcessElement) {
        parent = parent.Parent;
      }

      RectD layout = RectD.Empty;
      bool? isHorizontal = null;
      var multipleInstance = false;

      var tableShape = GetShape(element, plane);
      if (tableShape == null && parent != null) {
        tableShape = GetShape(parent, plane);
        BpmnElement processRefSource;
        if (tableShape == null && ProcessRefSource.TryGetValue(parent, out processRefSource)) {
          tableShape = GetShape(processRefSource, plane);
          if (processRefSource.HasChild(BpmnDiConstants.ParticipantMultiplicityElement)) {
            if (int.Parse(processRefSource.GetChildAttribute(BpmnDiConstants.ParticipantMultiplicityElement, "maximum")) > 1) {
              multipleInstance = true;
            }
          }
        }
      }

      if (tableShape != null) {
        // table has a shape itself so we use its layout to initialize the table
        layout = new RectD(tableShape.X, tableShape.Y, tableShape.Width, tableShape.Height);
        if (tableShape.IsHorizontal != null) {
          isHorizontal = tableShape.IsHorizontal;
        }
      }
      var calculateRect = layout.IsEmpty;
      if (calculateRect || isHorizontal == null) {
        // check the child lanes for their shapes
        foreach (var lane in element.GetChildren("lane")) {
          var laneShape = GetShape(lane, plane);
          if (laneShape != null) {
            if (calculateRect) {
              layout += new RectD(laneShape.X, laneShape.Y, laneShape.Width, laneShape.Height);
            }
            if (isHorizontal == null && laneShape.IsHorizontal != null) {
              isHorizontal = laneShape.IsHorizontal;
            }
          }
        }
      }
      // fallback
      var definedHorizontal = isHorizontal ?? false;
      INode node = null;
      if (!layout.IsEmpty) {
        ITable table;
        if (parent != null && parent.Table != null) {
          table = parent.Table;
          node = parent.Node;
        } else {
          // table was already initialized for the Process due to a Participant element
          node = MasterGraph.CreateNode(localRoot, layout);
          var poolStyle = CreatePoolNodeStyle(definedHorizontal);
          poolStyle.MultipleInstance = multipleInstance;
          MasterGraph.SetStyle(node, poolStyle);
          table = poolStyle.TableNodeStyle.Table;

          // create dummy stripe for the direction where no lanes are defined
          if (definedHorizontal) {
            var col = table.CreateColumn(layout.Width - table.RowDefaults.Insets.Left);
            col.Tag = new PointD(layout.X, layout.Y);
          } else {
            var row = table.CreateRow(layout.Height - table.ColumnDefaults.Insets.Top);
            row.Tag = new PointD(layout.X, layout.Y);
          }
        }

        IStripe parentStripe = definedHorizontal
            ? table.RootRow.ChildRows.Any()
                ? table.RootRow.ChildRows.First()
                : (IStripe) table.RootRow
            : table.RootColumn.ChildColumns.Any()
                ? table.RootColumn.ChildColumns.First()
                : table.RootColumn;
        if (tableShape != null) {
          parentStripe = AddToTable(tableShape, table, node, parentStripe, definedHorizontal);
        }

        element.Node = node;
        if (parent.Node == null) {
          parent.Node = node;
        }

        AddChildLanes(element, table, parentStripe, plane, node, definedHorizontal);

        // Resize the root row/column after adding a colum/row with insets
        if (definedHorizontal) {
          var max = table.RootRow.GetLeaves().Select(s => s.Layout.X - table.Layout.X + s.Insets.Left).Max();
          table.SetSize(table.RootColumn.ChildColumns.First(), node.Layout.Width - max);
        } else {
          var max = table.RootColumn.GetLeaves().Select(s => s.Layout.Y - table.Layout.Y + s.Insets.Top).Max();
          table.SetSize(table.RootRow.ChildRows.First(), node.Layout.Height - max);
        }

        /*
         * There can be situations, in which the table Layout does not match the node size. In this case, we
         * resize the node
         */
        if (node.Layout.Width != table.Layout.Width) {
          MasterGraph.SetNodeLayout(node, new RectD(node.Layout.X, node.Layout.Y,
              table.Layout.Width, node.Layout.Height));
        }
        if (node.Layout.Height != table.Layout.Height) {
          MasterGraph.SetNodeLayout(node, new RectD(node.Layout.X, node.Layout.Y,
              node.Layout.Width, table.Layout.Height));
        }
      }
      return node;
    }

    private void AddChildLanes(BpmnElement element, ITable table, IStripe parentStripe, BpmnPlane plane, INode node,
        bool isHorizontal) {
      foreach (var lane in element.GetChildren(BpmnDiConstants.LaneElement)) {
        var laneShape = GetShape(lane, plane);
        if (laneShape != null) {
          var addedStripe = AddToTable(laneShape, table, node, parentStripe, isHorizontal);
          foreach (var refElement in lane.GetChildren("flowNodeRef")) {
            BpmnElement bpmnElement;
            if (refElement.Value != null && TryGetElementForId(refElement.Value, out bpmnElement)) {
              bpmnElement.Parent.Node = node;
            }
          }
          var childLaneSet = lane.GetChild("childLaneSet");
          if (childLaneSet != null) {
            AddChildLanes(childLaneSet, table, addedStripe, plane, node, isHorizontal);
          }
        }
      }
    }

    // Adds the given lane to the appropriate table (pool), or creates a new one
    private IStripe AddToTable(BpmnShape shape, ITable table, INode node, IStripe parentStripe, bool isHorizontal) {
      // lane element
      var element = shape.Element;

      // Link the node to the BpmnElement of the lane

      element.Node = node;
      if (isHorizontal) {
        var parentRow = parentStripe as IRow;
        //getIndex
        var index = parentRow.ChildRows.Count(siblingRow =>
            ((PointD) siblingRow.Tag).Y < shape.Y);

        var row = table.CreateRow(parentRow, shape.Height, null, null, null, null, index);
        row.Tag = new PointD(shape.X, shape.Y);

        AddTableLabel(table, row, shape);
        return row;
      } else {
        var parentCol = parentStripe as IColumn;
        //getIndex
        var index = parentCol.ChildColumns.Count(siblingCol =>
            ((PointD) siblingCol.Tag).X < shape.X);

        var col = table.CreateColumn(parentCol, shape.Width, null, null, null, null, index);
        col.Tag = new PointD(shape.X, shape.Y);
        AddTableLabel(table, col, shape);
        return col;
      }
    }

    // Creates table (participant/pool)
    private PoolNodeStyle CreateTable(BpmnShape shape) {
      var poolNodeStyle = CreatePoolNodeStyle(shape.IsHorizontal ?? false);
      var table = poolNodeStyle.TableNodeStyle.Table;

      // Create first row & column
      var col = table.CreateColumn(shape.Width - table.RowDefaults.Insets.Left);
      var row = table.CreateRow(shape.Height - table.ColumnDefaults.Insets.Top);

      var location = new PointD(shape.X, shape.Y);
      row.Tag = location;
      col.Tag = location;
      return poolNodeStyle;
    }

    private PoolNodeStyle CreatePoolNodeStyle(bool isHorizontal) {
      var partStyle = new PoolNodeStyle(!isHorizontal);
      var table = partStyle.TableNodeStyle.Table;

      if (isHorizontal) {
        table.ColumnDefaults.Insets = new InsetsD();
      } else {
        table.RowDefaults.Insets = new InsetsD();
      }

      // Set table insets to 0
      table.Insets = new InsetsD();

      return partStyle;
    }

    #endregion
  }
}
