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
using System.Collections.Generic;
using System.Xml.Linq;
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  /// <summary>
  /// Utility class holding the information of a BPMN <see cref="XDocument"/>.
  /// </summary>
  internal class BpmnDocument
  {
    /// <summary>
    /// Mapping of all IDs of BPMN elements to these elements.
    /// </summary>
    internal Dictionary<string, BpmnElement> Elements { get; set; }

    /// <summary>
    /// List of all diagrams, that are parsed from this document.
    /// </summary>
    internal List<BpmnDiagram> Diagrams { get; set; }

    /// <summary>
    /// List of diagrams representing a "process", "choreography" or "collaboration"
    /// </summary>
    internal List<BpmnDiagram> TopLevelDiagrams { get; set; }

    /// <summary>
    /// Mapping from a BPMN element to the diagram representing it.
    /// </summary>
    internal Dictionary<BpmnElement, BpmnDiagram> ElementToDiagram { get; private set; }

    /// <summary>
    /// Collection of all warning during program flow
    /// </summary>
    internal List<string> Messages { get; set; }

    /// <summary>
    /// Creates a new instance for the BPMN <see cref="XDocument"/>.
    /// </summary>
    /// <param name="doc">The BPMN document to parse.</param>
    public BpmnDocument(XDocument doc) {
      Elements = new Dictionary<string, BpmnElement>();
      Diagrams = new List<BpmnDiagram>();
      TopLevelDiagrams = new List<BpmnDiagram>();
      ElementToDiagram = new Dictionary<BpmnElement, BpmnDiagram>();
      Messages = new List<string>();

      // parse the XML file
      var callingElements = new List<BpmnElement>();
      RecursiveElements(doc.Root, null, callingElements);

      // collect all elements that are linked to from a plane
      foreach (var diagram in Diagrams) {
        try {
          var planeElement = diagram.Plane.Element;
          ElementToDiagram[planeElement] = diagram;
        } catch (ArgumentException) {
          Messages.Add("Tried to add a diagram with the already existing id: " + diagram.Id);
        }
      }

      // collect all diagrams where the plane corresponds to a Top Level BpmnElement (Process/Choreography/Collaboration)
      foreach (var diagram in Diagrams) {
        BpmnDiagram parent = null;
        var element = diagram.Plane.Element;
        var elementName = element.Name;
        if (elementName == BpmnDiConstants.ProcessElement || elementName == BpmnDiConstants.ChoreographyElement || elementName == BpmnDiConstants.CollaborationElement) {
          TopLevelDiagrams.Add(diagram);
          foreach (var callingElement in callingElements) {
            if (callingElement.CalledElement == element.Id) {
              parent = callingElement.GetNearestAncestor(ElementToDiagram);
              if (parent != null) {
                parent.AddChild(diagram, callingElement);
              }
            }
          }
          foreach (var child in diagram.Plane.Element.Children) {
            CollectChildDiagrams(child, diagram);
          }
        }
      }
    }

    /// <summary>
    /// Collect all <see cref="BpmnDiagram"/> where the plane corresponds to a <see cref="BpmnElement"/> in <paramref name="diagram"/>.
    /// </summary>
    /// <param name="bpmnElement">The element to check.</param>
    /// <param name="diagram">The diagram to collect the child diagrams for.</param>
    private void CollectChildDiagrams(BpmnElement bpmnElement, BpmnDiagram diagram) {
      var currentDiagram = diagram;

      if (ElementToDiagram.ContainsKey(bpmnElement)) {
        var childDiagram = ElementToDiagram[bpmnElement];
        diagram.AddChild(childDiagram, bpmnElement);
        currentDiagram = childDiagram;
      }
      foreach (var child in bpmnElement.Children) {
        CollectChildDiagrams(child, currentDiagram);
      }
      if (bpmnElement.Process != null && Elements.ContainsKey(bpmnElement.Process)) {
        BpmnElement process = Elements[bpmnElement.Process];
        CollectChildDiagrams(process, currentDiagram);
      }
    }

    /// <summary>
    /// Traverses Depth-First through the bpmn XML file, collecting and linking all Elements.
    /// </summary>
    /// <param name="xNode">The XML node to start with</param>
    /// <param name="parent">The parent <see cref="BpmnElement"/></param>
    /// <param name="callingElements">A list to add all <see cref="BpmnElement"/> with a valid 'CalledElement' or 'Process' property.</param>
    private void RecursiveElements(XElement xNode, BpmnElement parent, ICollection<BpmnElement> callingElements) {
      var element = new BpmnElement(xNode);
      if (element.CalledElement != null) {
        callingElements.Add(element);
      } else if (element.Process != null) {
        callingElements.Add(element);
      }

      // Only xml nodes with an id can be bpmn elements
      if (element.Id != null) {
        try {
          Elements.Add(element.Id, element);
          // some tools prefix the ids with a namespace prefix but don't use this namespace to reference it
          // so if the id contains a prefix, it is mapped with and without it
          var prefix = xNode.GetPrefixOfNamespace(xNode.Name.Namespace);
          if (prefix != null) {
            Elements.Add(prefix + ":" + element.Id, element);
          }
        } catch (ArgumentException) {
          Messages.Add("Error while trying to add second Element with the same id: " + element.Id);
        }
      }

      // Double-link bpmn element to the given parent element
      if (parent != null) {
        parent.AddChild(element);
      }
      element.Parent = parent;

      // Call all xml children
      foreach (var xChild in xNode.Elements()) {
        var nameSpace = xChild.Name.Namespace;
        var localName = xChild.Name.LocalName;
        if (nameSpace.Equals(BpmnNM.Bpmn)) {
          // Add all bpmn elements to the dictionary
          RecursiveElements(xChild, element, callingElements);
        } else if (nameSpace.Equals(BpmnNM.BpmnDi)) {
          // Parse a diagram as whole
          if (localName == BpmnDiConstants.BpmnDiagramElement) {
            var diagram = BuildDiagram(xChild);
            if (diagram.Plane != null) {
              Diagrams.Add(diagram);
            } else {
              Messages.Add("The plane for diagram + " + diagram.Id + " was not correctly parsed.");
            }
          }
        } else {
          element.ForeignChildren.Add(xChild);
        }
      }
    }

    /// <summary>
    /// Creates a <see cref="BpmnDiagram"/>.
    /// </summary>
    /// <param name="xNode">The XML node to start with</param>
    /// <returns>The parsed <see cref="BpmnDiagram"/></returns>
    private BpmnDiagram BuildDiagram(XElement xNode) {
      var diagram = new BpmnDiagram(xNode);

      var bpmnPlane = BuildPlane(BpmnNM.GetElement(xNode, BpmnNM.BpmnDi, BpmnDiConstants.BpmnPlaneElement));
      if (bpmnPlane != null) {
        diagram.AddPlane(bpmnPlane);
      }

      foreach (var xChild in BpmnNM.GetElements(xNode, BpmnNM.BpmnDi, BpmnDiConstants.BpmnLabelStyleElement)) {
        var style = new BpmnLabelStyle(xChild);
        diagram.AddStyle(style);
      }

      // Setting a default LabelStyle for all labels that do not have their own style.
      diagram.DefaultStyle = BpmnLabelStyle.NewDefaultInstance();

      return diagram;
    }

    /// <summary>
    /// Parse all bpmn shapes and bpmn edges and their associations and attributes from one <see cref="BpmnPlane"/> 
    /// </summary>
    /// <param name="xNode">The XML node to start with</param>
    private BpmnPlane BuildPlane(XElement xNode) {
      var plane = new BpmnPlane(xNode, Elements);
      if (plane.Element == null) {
        return null;
      }

      // All Shapes
      foreach (var xChild in BpmnNM.GetElements(xNode, BpmnNM.BpmnDi, BpmnDiConstants.BpmnShapeElement)) {
        var shape = new BpmnShape(xChild, Elements);
        if (shape.Element != null) {
          plane.AddShape(shape);
        } else {
          Messages.Add("Error in parsing shape " + (shape.Id) + ", could not find corresponding BPMNElement.");
          continue;
        }

        // Shapes usually define their bounds
        shape.AddBounds(BpmnNM.GetElement(xChild, BpmnNM.Dc, BpmnDiConstants.BoundsElement));

        // Shapes can have a BPMNLabel as child
        var bpmnLabel = BpmnNM.GetElement(xChild, BpmnNM.BpmnDi, BpmnDiConstants.BpmnLabelElement);
        if (bpmnLabel != null) {
          // Label bounds
          var bounds = BpmnNM.GetElement(bpmnLabel, BpmnNM.Dc, BpmnDiConstants.BoundsElement);
          shape.AddLabel(bounds);
          // BpmnLabelStyle
          shape.LabelStyle = BpmnNM.GetAttributeValue(bpmnLabel, BpmnNM.BpmnDi, BpmnDiConstants.LabelStyleAttribute);
        }
      }

      foreach (var xChild in BpmnNM.GetElements(xNode, BpmnNM.BpmnDi, BpmnDiConstants.BpmnEdgeElement)) {
        var edge = new BpmnEdge(xChild, Elements);
        if (edge.Element != null) {
          plane.AddEdge(edge);
        } else {
          Messages.Add("Error in parsing edge " + (edge.Id) + ", could not find corresponding BPMNElement.");
          continue;
        }

        // Edges define 2 or more Waypoints
        foreach (var waypoint in BpmnNM.GetElements(xChild, BpmnNM.Di, BpmnDiConstants.WaypointElement)) {
          edge.AddWayPoint(waypoint);
        }

        // Edges can have a BPMNLabel as child
        var bpmnLabel = BpmnNM.GetElement(xChild, BpmnNM.BpmnDi, BpmnDiConstants.BpmnLabelElement);
        if (bpmnLabel != null) {
          // Label bounds
          var bounds = BpmnNM.GetElement(bpmnLabel, BpmnNM.Dc, BpmnDiConstants.BoundsElement);
          edge.AddLabel(bounds);
          // BpmnLabelStyle
          edge.LabelStyle = BpmnNM.GetAttributeValue(bpmnLabel, BpmnNM.BpmnDi, BpmnDiConstants.LabelStyleAttribute);
        }
      }
      return plane;
    }
  }
}
