/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Graph.Bpmn.View;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using VerticalAlignment = System.Windows.VerticalAlignment;
 
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
    
    /// <summary>
    /// Parser for the BPMN 2.0 abstract syntax.
    /// </summary>
    public class BpmnDiParser
    {      
        // Parsing all BPMN elements into this structure
        private Dictionary<string, BpmnElement> Elements { get; set; }

        // List of all diagrams, that are parsed from this document
        private List<BpmnDiagram> Diagrams { get; set; }

        // The currently used diagram
        private BpmnDiagram CurrentDiagram { get; set; }

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
        /// Collection of all warning during program flow
        /// </summary>
        internal List<string> Messages { get; set; }
        
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
        private readonly SizeD bpmnMessageSize = new SizeD(20,14);
        
        #region File Parsing 

        /// <summary>
        /// Constructs a new instance of the parser
        /// </summary>
        public BpmnDiParser() {
            
            // Message Collector for debugging purposes
            Messages = new List<string>();

            // Initialize the GenericLabelModel
            InitGenericLabelModel();
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
            
            // Open document
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            var doc = XDocument.Load(streamReader);
            
            // Go through the XML File
            Elements = new Dictionary<string, BpmnElement>();
            Diagrams = new List<BpmnDiagram>();
            var callingElements = new List<BpmnElement>();
            RecursiveElements(doc.Root, null, callingElements);
            
            // All Elements that are linked to from a plane
            var planeElements = new Dictionary<BpmnElement, BpmnDiagram>();
            foreach (var diagram in Diagrams) {
                try {
                    planeElements.Add(diagram.Plane.Element, diagram);
                } catch (ArgumentException) {
                    Messages.Add("Tried to add a diagram with the already existing id: " + diagram.Id);
                }
            }

            var topLevelDiagrams = new List<BpmnDiagram>();

            // All Diagrams where the Plane corresponds to a Top Level BpmnElement (Process/Choreography/Collaboration)
            foreach (var diagram in Diagrams) {
                BpmnDiagram parent = null;
                var element = diagram.Plane.Element;
                var elementName = element.Name;
                if (elementName == "process" || elementName == "choreography" || elementName == "collaboration") {
                    topLevelDiagrams.Add(diagram);
                    foreach (var callingElement in callingElements) {
                        if (callingElement.CalledElement == element.Id) {
                            parent = callingElement.GetNearestAncestor(planeElements);
                            if (parent != null) {
                                parent.AddChild(diagram, callingElement);
                            }
                        }
                    }
                } else {
                    if (ShowAllDiagrams) {
                        topLevelDiagrams.Add(diagram);
                    }
                    parent = element.GetNearestAncestor(planeElements);
                    if (parent != null) {
                        parent.AddChild(diagram, element);
                    }
                }
            }

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

            var exteriorLabelModel = new ExteriorLabelModel {Insets = new InsetsD(3, 3, 3, 3)};
            GenericLabelModel = new GenericLabelModel(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.South));
            GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.SouthEast));
            GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.SouthWest));
            GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.North));
            GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.NorthEast));
            GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.NorthWest));
            GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.West));
            GenericLabelModel.AddParameter(exteriorLabelModel.CreateParameter(ExteriorLabelModel.Position.East));
            // Big Insets
            exteriorLabelModel = new ExteriorLabelModel {Insets = new InsetsD(18, 18, 18, 18)};
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
        
        /// <summary>
        /// Traverses Depth-First through the bpmn XML file, collecting and linking all Elements
        /// </summary>
        /// <param name="xNode">The XML node to start with</param>
        /// <param name="parent">The parent <see cref="BpmnElement"/></param>
        private void RecursiveElements(XElement xNode, BpmnElement parent, ICollection<BpmnElement> callingElements)
        {
               
            var element = new BpmnElement(xNode);
            if (element.CalledElement != null) {
                callingElements.Add(element);
            } else if (element.Process != null)
            {
                callingElements.Add(element);
            }
        
            // Only xml nodes with an id can be bpmn elements
          var idAttribute = xNode.Attribute("id");
          if (idAttribute != null)
            {
                try {
                    Elements.Add(idAttribute.Value, element);
                } catch (ArgumentException){
                    Messages.Add("Error while trying to add second Element with the same id: " + idAttribute);
                }
            }

            // Double-link bpmn element to the given parent element
            if (parent != null) {
                parent.AddChild(element);
            }
            element.Parent = parent;
            
            // Call all xml children
            foreach (var xChild in xNode.Elements())
            {
                var nameSpace = xChild.Name.Namespace;
                var localName = xChild.Name.LocalName;
                if (nameSpace.Equals(BpmnNM.Bpmn)) {
                    
                    // Add all bpmn elements to the dictionary
                    RecursiveElements(xChild, element, callingElements);
                } else if(nameSpace.Equals(BpmnNM.BpmnDi)){

                    // Parse a diagram as whole
                    if (localName == "BPMNDiagram")
                    {
                        var diagram = BuildDiagram(xChild);
                        if (diagram.Plane != null) {
                            Diagrams.Add(diagram);
                        } else {
                            Messages.Add("The plane for diagram + " + diagram.Id + " was not correctly parsed.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a BpmnDiagramm
        /// </summary>
        /// <param name="xNode">The XML node to start with</param>
        /// <returns>The parsed <see cref="BpmnDiagram"/></returns>
        private BpmnDiagram BuildDiagram(XElement xNode)
        {
            var diagram = new BpmnDiagram(xNode);
            CurrentDiagram = diagram;

            BuildPlane(BpmnNM.GetElement(xNode, BpmnNM.BpmnDi, "BPMNPlane"));

            foreach (var xChild in BpmnNM.GetElements(xNode, BpmnNM.BpmnDi, "BPMNLabelStyle")) {
                var style = new BpmnLabelStyle(xChild);
                diagram.AddStyle(style);
            }
            
            // Setting a default LabelStyle for all labels that do not have their own style.
            diagram.DefaultStyle = BpmnLabelStyle.NewDefaultInstance();
            
            return diagram;
        }

        /// <summary>
        /// Parse all bpmn shapes and bpmn edges and their associations & attributes from one <see cref="BpmnPlane"/> 
        /// </summary>
        /// <param name="xNode">The XML node to start with</param>
        private void BuildPlane(XElement xNode) {
            
            var plane = new BpmnPlane(xNode, Elements);
            if (plane.Element == null) {
                return;
            }
            
            // All Shapes
            foreach (var xChild in BpmnNM.GetElements(xNode, BpmnNM.BpmnDi, "BPMNShape")) {
                
                var shape = new BpmnShape(xChild, Elements);
                if (shape.Element != null) {
                    plane.AddShape(shape); 

                } else {
                    Messages.Add("Error in parsing shape " + (shape.Id) + ", could not find corresponding BPMNElement.");
                    continue;
                }

                // Shapes usually define their bounds
                shape.AddBounds(BpmnNM.GetElement(xChild, BpmnNM.Dc, "Bounds"));  
                
                
                // Shapes can have a BPMNLabel as child
                var bpmnLabel = BpmnNM.GetElement(xChild, BpmnNM.BpmnDi, "BPMNLabel");
                if (bpmnLabel != null) {
                    // Label bounds
                    var bounds = BpmnNM.GetElement(bpmnLabel, BpmnNM.Dc, "Bounds");
                    shape.AddLabel(bounds);
                    // BpmnLabelStyle
                    shape.LabelStyle = BpmnNM.GetAttributeValue(bpmnLabel, BpmnNM.BpmnDi, "labelStyle");
                }
            }

            foreach (var xChild in BpmnNM.GetElements(xNode, BpmnNM.BpmnDi, "BPMNEdge")) {
                
                var edge = new BpmnEdge(xChild, Elements);
                if (edge.Element != null) {
                    plane.AddEdge(edge);
                } else {
                    Messages.Add("Error in parsing edge " + (edge.Id) + ", could not find corresponding BPMNElement.");
                    continue;
                }
                

                // Edges define 2 or more Waypoints
                foreach (var waypoint in BpmnNM.GetElements(xChild, BpmnNM.Di, "waypoint")) {
                    edge.AddWayPoint(waypoint);
                }

                // Edges can have a BPMNLabel as child
                var bpmnLabel = BpmnNM.GetElement(xChild, BpmnNM.BpmnDi, "BPMNLabel");
                if (bpmnLabel != null) {
                    // Label bounds
                    var bounds = BpmnNM.GetElement(bpmnLabel, BpmnNM.Dc, "Bounds");
                    edge.AddLabel(bounds);
                    // BpmnLabelStyle
                    edge.LabelStyle = BpmnNM.GetAttributeValue(bpmnLabel, BpmnNM.BpmnDi, "labelStyle");
                }
            }
            
            CurrentDiagram.AddPlane(plane);
        }
        
        #endregion
        
        #region Building Backbone

        /// <summary>
        /// Builds the first diagram via drawing the individual nodes & edges
        /// </summary>
        /// <param name="diagram">The diagram to draw</param>
        /// <param name="localRoot">The local root node </param>
        private void LoadDiagram(BpmnDiagram diagram, INode localRoot) {

            CurrentDiagram = diagram;
            var plane = diagram.Plane;
            var shapes = plane.ListOfShapes;
            var edges = plane.ListOfEdges;

            // Build all shapes
            foreach (var shape in shapes) {               
                if (shape.Element.Parent.Node == null) {
                    shape.Element.Parent.Node = localRoot;
                }
                BuildShape(shape);
            }

            // Build all Edges
            foreach (var edge in edges)
            {
                if (ParseEdges) {
                    BuildEdge(edge);
                }
            }

            // If we collapse the shape before we add edges, edge labels disappear -> Folding after edge creation
            // But we have to rearrange Labels first, otherwise they are not in sync with the positions after folding.
            Rearrange();
            foreach (var shape in shapes) {
                if (shape.IsExpanded == "false") {
                    view.Collapse(shape.Element.Node);                   
                }
            }

            if (ParseFoldedDiagrams) {
                foreach (var child in diagram.Children) {
                    LoadDiagram(child.Key, child.Value.Node);
                }
            } 
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
        private void BuildShape(BpmnShape shape) {
            
            var bounds = new RectD(shape.X, shape.Y, shape.Width, shape.Height);

            switch (shape.Element.Name)
            {
                // Gateways
                case "exclusiveGateway":
                    if (shape.IsMarkerVisible) {
                        BuildGatewayNode(shape, bounds, GatewayType.ExclusiveWithMarker);
                    } else {
                        BuildGatewayNode(shape, bounds, GatewayType.ExclusiveWithoutMarker);
                    }
                    break;
                case "parallelGateway":
                    BuildGatewayNode(shape, bounds, GatewayType.Parallel);
                    break;
                case "inclusiveGateway":
                    BuildGatewayNode(shape, bounds, GatewayType.Inclusive);
                    break;
                case "eventBasedGateway":
                    if (shape.GetAttribute("eventGatewayType") == "Exclusive") {
                        BuildGatewayNode(shape, bounds, GatewayType.ExclusiveEventBased);
                    } else if (shape.GetAttribute("eventGatewayType") == "Parallel") {
                        BuildGatewayNode(shape, bounds, GatewayType.ParallelEventBased);
                    } else {
                        BuildGatewayNode(shape, bounds, GatewayType.EventBased);
                    }
                    break;
                case "complexGateway":
                    BuildGatewayNode(shape, bounds, GatewayType.Complex);
                    break;
                
                // Activities - Tasks
                case "task":
                    BuildTaskNode(shape, bounds, TaskType.Abstract);
                    break;
                case "userTask":
                    BuildTaskNode(shape, bounds, TaskType.User);
                    break;
                case "manualTask":
                    BuildTaskNode(shape, bounds, TaskType.Manual);
                    break;
                case "serviceTask":
                    BuildTaskNode(shape, bounds, TaskType.Service);
                    break;
                case "scriptTask":
                    BuildTaskNode(shape, bounds, TaskType.Script);
                    break;
                case "sendTask":
                    BuildTaskNode(shape, bounds, TaskType.Send);
                    break;
                case "receiveTask":
                    BuildTaskNode(shape, bounds, TaskType.Receive);
                    break;
                case "businessRuleTask":
                    BuildTaskNode(shape, bounds, TaskType.BusinessRule);
                    break;
                
                // Activities - subProcess
                case "subProcess":
                    if (shape.GetAttribute("triggeredByEvent") == "true") {
                        BuildSubProcessNode(shape, bounds, ActivityType.EventSubProcess);
                    } else {
                        BuildSubProcessNode(shape, bounds, ActivityType.SubProcess);
                    }
                    break;
                
                // Activities - Ad-Hoc Sub-Process 
                case "adHocSubProcess":
                    if (shape.GetAttribute("triggeredByEvent") == "true") {
                        BuildSubProcessNode(shape, bounds, ActivityType.EventSubProcess);
                    } else {
                        BuildSubProcessNode(shape, bounds, ActivityType.SubProcess);
                    }
                    break;
                
                // Activities - Transaction
                case "transaction":
                    BuildSubProcessNode(shape, bounds, ActivityType.Transaction);
                    break;
                
                // Activities - callActivity
                case "callActivity":
                    BuildSubProcessNode(shape, bounds, ActivityType.CallActivity);
                    break;
   
                //Events
                case "startEvent":
                    if (shape.GetAttribute("isInterrupting") == "true") {
                        BuildEventNode(shape, bounds, EventCharacteristic.SubProcessInterrupting);
                    } else if(shape.GetAttribute("isInterrupting") == "false") {
                        BuildEventNode(shape, bounds, EventCharacteristic.SubProcessNonInterrupting);
                    } else {
                        BuildEventNode(shape, bounds, EventCharacteristic.Start);
                    }
                    break;
                case "endEvent":
                    BuildEventNode(shape, bounds, EventCharacteristic.End);
                    break;
                case "boundaryEvent":
                    // Boundary Events are realized as Ports instead of Nodes
                    BuildBoundaryEvent(shape);
                    break;
                case "intermediateThrowEvent":
                    BuildEventNode(shape, bounds, EventCharacteristic.Throwing);
                    break;
                case "intermediateCatchEvent":
                    BuildEventNode(shape, bounds, EventCharacteristic.Catching);
                    break;
                
                // Conversation
                case "conversation":
                    BuildConversationNode(shape, bounds, ConversationType.Conversation, null);
                    break;
                case "callConversation":
                    BpmnElement refElement;
                    if (Elements.TryGetValue(shape.GetAttribute("calledCollaborationRef"), out refElement))
                    {
                        switch (refElement.Name)
                        {
                            case "collaboration":
                                BuildConversationNode(shape, bounds, ConversationType.CallingCollaboration, refElement);
                                break;
                            case "globalConversation":
                                BuildConversationNode(shape, bounds, ConversationType.CallingGlobalConversation, refElement);
                                break;
                            default:
                                // This should not happen under strict conformance
                                BuildConversationNode(shape, bounds, ConversationType.Conversation, refElement);
                                break;
                        }
                    }
                    break;
                case "subConversation":
                    BuildConversationNode(shape, bounds, ConversationType.SubConversation, null);
                    break;
             
                // Choreography
                case "choreographyTask":
                case "subChoreography":
                case "callChoreography":
                    BuildChoreographyNode(shape, bounds);
                    break;

                // Participants 
                case "participant":
                    // If the participant is not part of a choreography node, create a node
                    if (shape.Element.Parent.Name != "choreography") {
                        BuildParticipantNode(shape, bounds);
                    } else {
                        // Else add it to the appropriate choreography
                        BuildParticipantLabel(shape);
                    }
                    break;
                
                // Swimlanes
                case "lane":
                    AddToTable(shape);
                    break;
                
                // Text Annotations 
                case "textAnnotation":
                    BuildTextAnnotationNode(shape, bounds);
                    break;
                
                // Groups
                case "group":            
                    BuildGroupNode(shape, bounds);
                    break;
                
                // DataObject
                case "dataObjectReference":
                    // Find out, if the data Object is a collection
                    var collection = false;
                    BpmnElement dataObject;
                    if (Elements.TryGetValue(shape.GetAttribute("dataObjectRef"), out dataObject)) {
                        string collectionString;
                        if (dataObject.Attributes.TryGetValue("isCollection", out collectionString))
                        {
                            if (collectionString == "true") {
                                collection = true;
                            }
                        }
                    }
                    BuildDataObjectNode(shape, bounds, DataObjectType.None, collection);
                    break;
                case "dataInput":
                    // Find out, if the data Object is a collection
                    collection = shape.GetAttribute("isCollection") == "true";
                    BuildDataObjectNode(shape, bounds, DataObjectType.Input, collection);
                    break;
                case "dataOutput":
                    // Find out, if the data Object is a collection
                    collection = shape.GetAttribute("isCollection") == "true";
                    BuildDataObjectNode(shape, bounds, DataObjectType.Output, collection);
                    break;
                
                // DataStore
                case "dataStoreReference":
                    BuildDataStoreReferenceNode(shape, bounds);
                    break;
            }
        }

        /// <summary>
        /// Creates an <see cref="IEdge"/> on the graph
        /// </summary>
        /// <param name="edge">The <see cref="BpmnEdge"/> to draw. </param>
        private void BuildEdge(BpmnEdge edge) {
            var element = edge.Element;
            var source = edge.Source;
            
            switch (element.Name)
            {
                case "sequenceFlow":
                    if (element.GetChild("conditionExpression") != null && !(source.Name.EndsWith("Gateway"))) {
                        BuildDefaultEdge(edge, EdgeType.ConditionalFlow);
                    } else if (source != null && source.GetValue("default") == element.Id) {
                        BuildDefaultEdge(edge, EdgeType.DefaultFlow);
                    } else {
                        BuildDefaultEdge(edge, EdgeType.SequenceFlow);
                    }
                    break;
                case "association":
                    switch (edge.GetAttribute("associationDirection")) {
                        case "None":
                            BuildDefaultEdge(edge, EdgeType.Association);
                            break;
                        case "One":
                            BuildDefaultEdge(edge, EdgeType.DirectedAssociation);
                            break;
                        case "Both":
                            BuildDefaultEdge(edge, EdgeType.BidirectedAssociation);
                            break;
                        default:
                            // This shouldn't happen under strict conformance
                            BuildDefaultEdge(edge, EdgeType.Association);
                            break;
                    }
                    break;
                case "dataAssociation":
                    BuildDefaultEdge(edge, EdgeType.Association);
                    break;
                case "conversationLink":
                    BuildDefaultEdge(edge, EdgeType.Conversation);
                    break;
                case "messageFlow":
                    BuildMessageFlow(edge);
                    break;
                case "dataInputAssociation":
                    BuildDefaultEdge(edge, EdgeType.DirectedAssociation);
                    break;
                case "dataOutputAssociation":
                    BuildDefaultEdge(edge, EdgeType.DirectedAssociation);
                    break;
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
            var gatewayStyle = new GatewayNodeStyle {Type = type};
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
                if(Elements.TryGetValue(element.CalledElement, out calledElement))
                {
                    calledElement.Node = node;
                }
            }
            
            // dataAssociations point to invisible children of activities, therefore, the INode has to be linked there
            element.SetINodeInputOutput(node);

            var activityStyle = new ActivityNodeStyle {
                    Compensation = shape.GetAttribute("isForCompensation") == "true",
                    // Get Loop Characteristics
                    LoopCharacteristic = element.GetLoopCharacteristics()
            };
            
            // Get, if the subProcess is expanded
            var label = AddNodeLabel(node, shape);
            SetSubProcessLabelStyle(label);
            activityStyle.ActivityType = type;
            activityStyle.TriggerEventType = GetEventType(shape);

            if (shape.GetAttribute("isInterrupting") == "true") {
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
            var eventStyle = new EventNodeStyle {Type = GetEventType(shape), Characteristic = characteristic};
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
            Elements.TryGetValue(shape.GetAttribute("attachedToRef"), out parent);
            var portStyle = new EventPortStyle {
                    Type = GetEventType(shape),
                    Characteristic = shape.GetAttribute("cancelActivity") == "false"
                            ? EventCharacteristic.BoundaryNonInterrupting
                            : EventCharacteristic.BoundaryInterrupting
            };
            if (parent == null) {
              throw new ArgumentException("Shape with no parent", "shape");
            }
            if (parent.Node == null) {
                Messages.Add("The node for boundaryEvent " + shape.Id + " was not (yet) created!");
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
                    var outsideModel = new InsideOutsidePortLabelModel {Distance = 10};
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
            var conversationStyle = new ConversationNodeStyle {Type = type};
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
                    new ChoreographyNodeStyle {LoopCharacteristic = element.GetLoopCharacteristics()};
            
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

            var objectStyle = new DataObjectNodeStyle {Type = type, Collection = isCollection};

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
            var node = MasterGraph.CreateNode(bounds);
            var element = shape.Element;
            SetParent(node, element.Parent.Node);
            element.Node = node;
            
            // dataAssociations point to invisible children of Tasks, therefore, the INode has to be linked there 
            element.SetINodeInputOutput(node);
            
            // participants point to an invisible process, therefore linking the INode there as well
            var processRef = element.Process;
            BpmnElement process;
            if(processRef != null && Elements.TryGetValue(processRef, out process)) {
                process.Node = node;
            }
            
            var partStyle = CreateTable(shape);
            if (element.HasChild("participantMultiplicity")) {
                if (int.Parse(element.GetChildAttribute("participantMultiplicity", "maximum")) > 1) {
                    partStyle.MultipleInstance = true;
                }
            }
            BpmnElement processElement;
            if (element.Process != null && Elements.TryGetValue(element.Process, 
                        out processElement)) {
                processElement.Table = partStyle.TableNodeStyle.Table;
            }
            MasterGraph.SetStyle(node, partStyle);
            if (shape.IsHorizontal) {
                AddTableLabel(element.TableRow, shape);
            }
            else
            {
                AddTableLabel(element.TableColumn, shape); 
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
            
            if (element.HasChild("participantMultiplicity")) {
                if (int.Parse(element.GetChildAttribute("participantMultiplicity", "maximum")) > 1) {
                    multipleInstance = true;
                }
            }
            var participant = new Participant{ MultiInstance = multipleInstance };
            var label = AddParticipantLabel(node, shape);
            switch (shape.PartBandKind)
            {
                case BpmnShape.ParticipantBandKind.TopInitiating:
                    if (shape.IsMessageVisible)
                        choreographyNodeStyle.InitiatingMessage = true;
                    choreographyNodeStyle.InitiatingAtTop = true;
                    choreographyNodeStyle.TopParticipants.Add(participant);
                    top = true;
                    index = choreography.TopParticipants++;
                    break;
                case BpmnShape.ParticipantBandKind.TopNonInitiating:
                    if (shape.IsMessageVisible)
                        choreographyNodeStyle.ResponseMessage = true;
                    choreographyNodeStyle.TopParticipants.Add(participant);
                    top = true;
                    index = choreography.TopParticipants++;
                    break;
                case BpmnShape.ParticipantBandKind.BottomInitiating:
                    if (shape.IsMessageVisible)
                        choreographyNodeStyle.InitiatingMessage = true;
                    choreographyNodeStyle.InitiatingAtTop = false;
                    choreographyNodeStyle.BottomParticipants.Add(participant);
                    index = choreography.BottomParticipants++;
                    break;
                case BpmnShape.ParticipantBandKind.BottomNonInitiating:
                    if (shape.IsMessageVisible)
                        choreographyNodeStyle.ResponseMessage = true;
                    choreographyNodeStyle.BottomParticipants.Add(participant);
                    index = choreography.BottomParticipants++;
                    break;
                case BpmnShape.ParticipantBandKind.MiddleInitiating:
                    // This shouldn't happen under strict conformance
                    if (shape.IsMessageVisible)
                        choreographyNodeStyle.InitiatingMessage = true;
                    if(choreography.TopParticipants < choreography.BottomParticipants) 
                    {
                        top = true;
                        index = choreography.TopParticipants++;
                        choreographyNodeStyle.InitiatingAtTop = true;
                        choreographyNodeStyle.TopParticipants.Add(participant);
                    }
                    else
                    {
                        index = choreography.BottomParticipants++;
                        choreographyNodeStyle.InitiatingAtTop = false;
                        choreographyNodeStyle.BottomParticipants.Add(participant);
                    }
                    break;
                case BpmnShape.ParticipantBandKind.MiddleNonInitiating:
                    if (shape.IsMessageVisible)
                        choreographyNodeStyle.ResponseMessage = true;
                    if(choreography.TopParticipants < choreography.BottomParticipants) 
                    {
                        top = true;
                        index = choreography.TopParticipants++;
                        choreographyNodeStyle.TopParticipants.Add(participant);
                    }
                    else
                    {
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
            if (shape.IsMessageVisible && choreography.HasChild("messageFlowRef"))
            {
                var children = choreography.GetChildren("messageFlowRef");

                foreach (var child in children) {
                    BpmnElement messageFlow;
                    if (Elements.TryGetValue(child.Value, out messageFlow)) {
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
            var node = MasterGraph.CreateNode(bounds);
            var element = shape.Element;
            SetParent(node, element.Parent.Node);
            element.Node = node;
            
            // Add Style
            var groupStyle = new GroupNodeStyle();
            MasterGraph.SetStyle(node, groupStyle);
            
            // Before Adding a Label, we need to get the Label Text, which is located in a categoryValue
            // The id of this category value is located in the Label
            foreach (var childElement in Elements) {
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
            
            if (element.HasChild("messageEventDefinition")) {
                eventType = EventType.Message;
            }
            if (element.HasChild("timerEventDefinition")) {
                eventType = EventType.Timer;
            }
            if (element.HasChild("terminateEventDefinition")) {
                eventType = EventType.Terminate;
            }
            if (element.HasChild("escalationEventDefinition")) {
                eventType = EventType.Escalation;
            }
            if (element.HasChild("errorEventDefinition")) {
                eventType = EventType.Error;
            }
            if (element.HasChild("conditionalEventDefinition")) {
                eventType = EventType.Conditional;
            }
            if (element.HasChild("compensateEventDefinition")) {
                eventType = EventType.Compensation;
            }
            if (element.HasChild("cancelEventDefinition")) {
                eventType = EventType.Cancel;
            }
            if (element.HasChild("linkEventDefinition")) {
                eventType = EventType.Link;
            }
            if (element.HasChild("signalEventDefinition")) {
                eventType = EventType.Signal;
            }
            if (element.HasChild("multipleEventDefinition")) {
                eventType = EventType.Multiple;
            }
            if (element.HasChild("parallelEventDefinition")) {
                eventType = EventType.ParallelMultiple;
            }
            return eventType;
        } 
        
        // Sets the parentNode of a Node, if the parentNode is part of the current Graph
        private void SetParent(INode node, INode parentNode) {
            
            if(MasterGraph.Contains(parentNode))
            {
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
        private void AddTableLabel(IStripe owner, BpmnShape shape)
        {
            var table = shape.Element.Table;
            // blank label, in case we added none
            var name = shape.Element.Label;
            // only has label name, if we added one before
            if (!shape.HasLabel && !ParseAllLabels || name == null) {
                name = "";
            }
            table.AddLabel(owner, name, null, null, null, shape.LabelStyle);

        }
        
        // Adds a label to an edge
        private ILabel AddEdgeLabel(BpmnEdge edge) {
            
            // blank label, in case we added none
            var name = edge.Element.Label;
            // only has label name, if we added one before
            if (!edge.HasLabel && !ParseAllLabels || name == null) {
                name = "";
            }
            var label = MasterGraph.AddLabel(edge.Element.Edge, name, null, null, null, edge.LabelStyle);
            
            return label;
        }

        // Sets label style, if there are fixed bounds for this label
        private void SetFixedBoundsLabelStyle(ILabel label, RectD bounds) {

            var model = new FreeLabelModel();
            MasterGraph.SetLabelLayoutParameter(label, model.CreateAbsolute(bounds.BottomLeft, 0));
            var defaultLabelStyle = SetCustomLabelStyle(label);
            MasterGraph.SetStyle(label, defaultLabelStyle);
            MasterGraph.SetLabelPreferredSize(label, new SizeD(bounds.Width, bounds.Height));
        }

        // Sets label style for tasks (Centered)
        private void SetInternalLabelStyle(ILabel label)
        {       
            var model = new InteriorStretchLabelModel {Insets = new InsetsD(3, 3, 3, 3)};
            MasterGraph.SetLabelLayoutParameter(label, model.CreateParameter(InteriorStretchLabelModel.Position.Center));
            var defaultLabelStyle = SetCustomLabelStyle(label);
            defaultLabelStyle.TextAlignment = TextAlignment.Center;
            defaultLabelStyle.VerticalTextAlignment = VerticalAlignment.Center;
            defaultLabelStyle.TextWrapping = TextWrapping.Wrap;
            MasterGraph.SetStyle(label, defaultLabelStyle);
        }
        
        // Sets label style nodes that have an external label (South of the node)
        private void SetExternalLabelStyle(ILabel label)
        {
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
        private void SetChoreographyLabelStyle(ILabel label)
        {
            MasterGraph.SetLabelLayoutParameter(label, ChoreographyLabelModel.TaskNameBand);
            var defaultLabelStyle = SetCustomLabelStyle(label);
            defaultLabelStyle.TextWrapping = TextWrapping.Wrap;
            defaultLabelStyle.TextAlignment = TextAlignment.Center;
            defaultLabelStyle.VerticalTextAlignment = VerticalAlignment.Center;
            MasterGraph.SetStyle(label, defaultLabelStyle);
        }
        
        // Sets label style for SubProcesses (Upper left corner)
        private void SetSubProcessLabelStyle(ILabel label)
        {
            var model = new InteriorStretchLabelModel {Insets = new InsetsD(3, 3, 3, 3)};
            MasterGraph.SetLabelLayoutParameter(label, model.CreateParameter(InteriorStretchLabelModel.Position.North));
            var defaultLabelStyle = SetCustomLabelStyle(label);
            defaultLabelStyle.TextAlignment = TextAlignment.Left;
            defaultLabelStyle.VerticalTextAlignment = VerticalAlignment.Top;
            MasterGraph.SetStyle(label, defaultLabelStyle);        
        }
        
        // Sets label style for Groups (Upper boundary)
        private void SetGroupLabelStyle(ILabel label)
        {
            var model = new InteriorStretchLabelModel {Insets = new InsetsD(3, 3, 3, 3)};
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
        private void BuildDefaultEdge(BpmnEdge edge, EdgeType type) {

            var sourceVar = edge.Source;
            var targetVar = edge.Target;
            var waypoints = edge.Waypoints;
            var id = edge.Id;
            
            // Check, if source and target were correctly parsed
            if (sourceVar == null) {
                Messages.Add("Edge "  + (id) + " has no valid Source.");
                return;
            }
            if (targetVar == null) {
                Messages.Add("Edge "  + (id) + " has no valid Target.");
                return;
            }
            
            // Get bends & ports from waypoints
            var count = waypoints.Count;
            // First waypoint is source Port
            var source = waypoints[0];
            // Last is target port
            var target = waypoints[count - 1];
            waypoints.Remove(source);
            waypoints.Remove(target);

            // Get source & target node
            var sourceNode = sourceVar.Node;
            var targetNode = targetVar.Node;

            IPort sourcePort = null;
            IPort targetPort = null;

            // Use boundary event port, if source is a boundary event
            if (sourceVar.Name == "boundaryEvent") {
                sourcePort = sourceVar.Port;
                if (sourcePort != null) {
                    sourceNode = (INode) sourcePort.Owner;
                } else {
                    Messages.Add("The source boundary event for edge " + id + " was not (yet) created.");
                    return;
                }

            } else if (sourceNode != null) {
                sourcePort = MasterGraph.AddPort(sourceNode, source);
            }

            // Use boundary event port, if target is a boundary event
            if (targetVar.Name == "boundaryEvent") {
                targetPort = targetVar.Port;
                if (targetPort != null) {
                    targetNode = (INode) targetPort.Owner;
                } else {
                    Messages.Add("The target boundary event for edge " + id + " was not (yet) created.");
                    return;
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
                Messages.Add("Edge "  + (id) + " has no valid Source.");
                return;
            } 
            if (targetPort == null) {
                Messages.Add("Edge "  + (id) + " has no valid Target.");
                return;
            }   
            
            // Create edge on graph
            var iEdge = MasterGraph.CreateEdge(sourcePort, targetPort);
            foreach (var point in waypoints) {
                MasterGraph.AddBend(iEdge, point);
            }

            edge.Element.Edge = iEdge;

            // Set edge style
            var edgeStyle = new BpmnEdgeStyle {Type = type};
            MasterGraph.SetStyle(iEdge, edgeStyle);
 
            // Create label & set style
            var label = AddEdgeLabel(edge);
            if (edge.HasLabelPosition()) {
                SetFixedBoundsLabelStyle(label, edge.LabelBounds);
            } else {
                SetEdgeLabelStyle(label);
                if (edge.HasLabelSize()) {
                    MasterGraph.SetLabelPreferredSize(label, edge.LabelBounds.Size);
                }
            }
        }

        // Builds MessageFlow edges
        private void BuildMessageFlow(BpmnEdge edge) {
            
            var sourceVar = edge.Source;
            var targetVar = edge.Target;
            var waypoints = edge.Waypoints;
            var id = edge.Id;
            
            // Check, if source and target were correctly parsed
            if (sourceVar == null) {
                Messages.Add("Edge "  + (id) + " has no valid Source.");
                return;
            }
            if (targetVar == null) {
                Messages.Add("Edge "  + (id) + " has no valid Target.");
                return;
            }
            
            // Get source & target node
            var sourceNode = sourceVar.Node;
            var targetNode = targetVar.Node;
            
            // Get bends & ports from waypoints
            var count = waypoints.Count;
            // First waypoint is source Port
            var source = waypoints[0];
            // Last is target port
            var target = waypoints[count - 1];
            waypoints.Remove(source);
            waypoints.Remove(target);
            
            // Get source & target port
            var sourcePort = sourceVar.Name == "boundaryEvent" ? sourceVar.Port : MasterGraph.AddPort(sourceNode, source);
            var targetPort = targetVar.Name == "boundaryEvent" ? targetVar.Port : MasterGraph.AddPort(targetNode, target);

            var iEdge = MasterGraph.CreateEdge(sourcePort, targetPort);
            foreach (var point in waypoints) {
                MasterGraph.AddBend(iEdge, point);
            }
            edge.Element.Edge = iEdge;
            
            // If there is a message icon, add a corresponding label
            switch (edge.MessageVisibleK) {
                        
                case BpmnEdge.MessageVisibleKind.Initiating:
                    var messageLabel = MasterGraph.AddLabel(iEdge, "");
                    MasterGraph.SetStyle(messageLabel, MessageLabelStyle.InitiatingStyle());
                    var model = new EdgeSegmentLabelModel() {
                            SideOfEdge = EdgeSides.OnEdge, 
                            AutoRotation = false
                    };
                    MasterGraph.SetLabelPreferredSize(messageLabel, bpmnMessageSize);
                    MasterGraph.SetLabelLayoutParameter(messageLabel, model.CreateParameterFromCenter());
                    break;
                case BpmnEdge.MessageVisibleKind.NonInitiating:
                    messageLabel = MasterGraph.AddLabel(iEdge, "");
                    MasterGraph.SetStyle(messageLabel, MessageLabelStyle.ResponseStyle());
                    model = new EdgeSegmentLabelModel() {
                            SideOfEdge = EdgeSides.OnEdge, 
                            AutoRotation = false
                    };                    
                    MasterGraph.SetLabelPreferredSize(messageLabel, bpmnMessageSize);
                    MasterGraph.SetLabelLayoutParameter(messageLabel, model.CreateParameterFromCenter());
                    break;
                case BpmnEdge.MessageVisibleKind.Unspecified:
                    break;
            }
            
            // Set edge style
            var edgeStyle = new BpmnEdgeStyle {Type = EdgeType.MessageFlow};
            MasterGraph.SetStyle(iEdge, edgeStyle);

            // Create label & set label style
            var label = AddEdgeLabel(edge);
            if (edge.HasLabelPosition()) {
                SetFixedBoundsLabelStyle(label, edge.LabelBounds);
            } else {
                SetEdgeLabelStyle(label);
                if (edge.HasLabelSize()) {
                    MasterGraph.SetLabelPreferredSize(label, edge.LabelBounds.Size);
                }
            }
        }

        #endregion
        
        #region Building Tables
        
        // Adds the given lane to the appropriate table (pool), or creates a new one
        private  void AddToTable(BpmnShape shape) {
            ITable table;
            INode node;
            var element = shape.Element;
            var parent = element.Parent;
            while (parent.Name != "process" && parent.TableRow == null && parent.TableColumn == null) {
                parent = parent.Parent;
            }

            // Creates a new table, if it does not exist yet
            if (parent.Table == null) {
                node = MasterGraph.CreateNode(new RectD(shape.X, shape.Y, shape.Width, shape.Height));
                var poolStyle = CreateTable(shape);
                table = element.Table;
                parent.Table = table;
                parent.Node = node;
                element.Node = node;
                parent.TableRow = table.RootRow;
                parent.TableColumn = table.RootColumn;
                MasterGraph.SetStyle(node, poolStyle);

            } else {
                table = parent.Table;
                var parentCol = parent.TableColumn;
                var parentRow = parent.TableRow;
                node = parent.Node;
                element.Node = node;
                if (shape.IsHorizontal)
                {
                    //getIndex
                    var index = parentRow.ChildRows.Count(siblingRow => 
                            ((PointD) siblingRow.Tag).Y < shape.Y);

                    var row = table.CreateRow(parentRow, shape.Height, null, null, null, null, index);
                    element.TableRow = row;
                    row.Tag = new PointD(shape.X, shape.Y);
                }
                else
                {
                    //getIndex
                    var index = parentCol.ChildColumns.Count(siblingCol => 
                            ((PointD) siblingCol.Tag).X < shape.X);
                    
                    var col = table.CreateColumn(parentCol, shape.Width, null, null, null, null, index);
                    element.TableColumn = col;
                    col.Tag = new PointD(shape.X, shape.Y);
                }
            }

            // Resize the root row/column after adding a colum/row with insets
            if (shape.IsHorizontal) {
                var max = table.RootRow.GetLeaves().Select(s => s.Layout.X - table.Layout.X+ s.Insets.Left).Max();
                table.SetSize(table.RootColumn.ChildColumns.First(), node.Layout.Width - max);
            } else {
                var max = table.RootColumn.GetLeaves().Select(s => s.Layout.Y - table.Layout.Y+ s.Insets.Top).Max();
                table.SetSize(table.RootRow.ChildRows.First(), node.Layout.Height - max);
            }
            
            /*
             * There can be situations, in which the table Layout does not match the node size. In this case, we
             * resize the node
             */
            if (node.Layout.Width != table.Layout.Width)
            {
                MasterGraph.SetNodeLayout(node, new RectD(node.Layout.X, node.Layout.Y, 
                    table.Layout.Width, node.Layout.Height));
            }
            if (node.Layout.Height != table.Layout.Height) 
            {
                MasterGraph.SetNodeLayout(node, new RectD(node.Layout.X, node.Layout.Y, 
                        node.Layout.Width, table.Layout.Height));
            }

            // Link the node and the table to the BpmnElement of the lane
            element.Table = table;
            
            // Add label
            if (shape.IsHorizontal) {
                AddTableLabel(element.TableRow, shape);
            } else {
                AddTableLabel(element.TableColumn, shape); 
            }
        }

        // Creates table (participant/pool)
        private PoolNodeStyle CreateTable(BpmnShape shape) {

            var partStyle = new PoolNodeStyle(!shape.IsHorizontal);
    
            var table = partStyle.TableNodeStyle.Table;
            
            if (shape.IsHorizontal)
            {
                table.ColumnDefaults.Insets = new InsetsD();
            }
            else
            {
                table.RowDefaults.Insets = new InsetsD();
            }
            
            // Set table insets to 0
            table.Insets = new InsetsD();
            // Create first row & column
            var col = table.CreateColumn(shape.Width - table.RowDefaults.Insets.Left);
            var row = table.CreateRow(shape.Height - table.ColumnDefaults.Insets.Top);

            var element = shape.Element;
            
            // Find the root BpmnElement of this pool
            if (element.Process != null) {
                BpmnElement process;
                if (Elements.TryGetValue(element.Process, out process))
                {
                    process.TableRow = row;
                    process.Table = table;
                    process.TableColumn = col;
                }
            }
            element.TableRow = row;
            element.Table = table;
            element.TableColumn = col;
            row.Tag = new PointD(shape.X, shape.Y);
            col.Tag = new PointD(shape.X, shape.Y);
            return partStyle;
        }
        
        #endregion    
    }
}
