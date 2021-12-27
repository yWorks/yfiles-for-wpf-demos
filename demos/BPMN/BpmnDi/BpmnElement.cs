/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Xml.Linq;
using yWorks.Graph;
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  /// <summary>
  /// Class for Bpmn Element objects
  /// </summary>
  internal class BpmnElement
  {
    /// <summary>
    /// List of all children of this element
    /// </summary>
    public List<BpmnElement> Children { get; set; }
    
    /// <summary>
    /// List of all <see cref="XNode"/> that were children of this element but have a different namespace then
    /// <see cref="BpmnNamespaceManager.Bpmn"/> or <see cref="BpmnNamespaceManager.BpmnDi"/>.
    /// </summary>
    public List<XNode> ForeignChildren { get; set; }

    /// <summary>
    /// Id of this element
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// List of all attributes, that do not have a Property
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; }

    /// <summary>
    /// Number of TopParticipants, if this is a choreography Node
    /// </summary>
    public int TopParticipants { get; set; }

    /// <summary>
    /// Number of BottomParticipants, if this is a choreography Node
    /// </summary>
    public int BottomParticipants { get; set; }

    /// <summary>
    /// The parent BpmnElement
    /// </summary>
    public BpmnElement Parent { get; set; }

    /// <summary>
    /// The corresponding INode, if this element is a BpmnShape
    /// </summary>
    public INode Node { get; set; }

    /// <summary>
    /// The corresponding table, if this element is part of a pool
    /// </summary>
    public ITable Table { get; set; }

    /// <summary>
    /// The label text of this element
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// The name of the element type
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The reference to a process, if this element is a subprocess
    /// </summary>
    public string Process { get; set; }

    /// <summary>
    /// The source element, if this element is an edge
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The target element, if this element is an edge
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// The value (string text between XML tags) of this Element
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The element called by this element, if it is a calling element
    /// </summary>
    public string CalledElement { get; private set; }

    /// <summary>
    /// The corresponding IPort if this element is a BoundaryEvent
    /// </summary>
    public IPort Port { get; set; }

    /// <summary>
    /// The corresponding IEdge, if this element is an Edge
    /// </summary>
    public IEdge Edge { get; set; }

    /// <summary>
    /// Class for BPMNElement objects
    /// </summary>
    /// <param name="xNode">The XML Node to turn into a BpmnElement</param>
    public BpmnElement(XElement xNode) {
      Children = new List<BpmnElement>();
      ForeignChildren = new List<XNode>();
      Attributes = new Dictionary<string, string>();

      //Initialize blank Label
      Label = "";

      // Parsing all Attributes
      foreach (var attribute in BpmnNM.AttributesInNamespace(xNode.Attributes(), BpmnNM.Bpmn)) {
        var localName = attribute.Name.LocalName;
        switch (localName) {
          case BpmnDiConstants.IdAttribute:
            Id = attribute.Value;
            break;
          case BpmnDiConstants.NameAttribute:
            Label = attribute.Value;
            break;
          case BpmnDiConstants.SourceRefAttribute:
            Source = attribute.Value;
            break;
          case BpmnDiConstants.TargetRefAttribute:
            Target = attribute.Value;
            break;
          case BpmnDiConstants.ProcessRefAttribute:
            Process = attribute.Value;
            break;
          case BpmnDiConstants.CalledElementAttribute:
          case BpmnDiConstants.CalledChoreographyRefAttribute:
            CalledElement = attribute.Value;
            break;
          default:
            Attributes.Add(localName, attribute.Value);
            break;
        }
      }

      Value = xNode.Value;
      Name = xNode.Name.LocalName;
      switch (Name) {
        case BpmnDiConstants.GroupElement:
          Label = BpmnNM.GetAttributeValue(xNode, BpmnNM.Bpmn, BpmnDiConstants.CategoryValueRefAttribute);
          break;
        case BpmnDiConstants.TextAnnotationElement:
          var element = BpmnNM.GetElement(xNode, BpmnNM.Bpmn, BpmnDiConstants.TextElement);
          if (element != null) {
            Label = element.Value;
          }
          break;
      }
    }

    /// <summary>
    /// Adds a child to the current BpmnElement
    /// </summary>
    /// <param name="child"> The child to be added</param>
    public void AddChild(BpmnElement child) {
      Children.Add(child);
    }

    /// <summary>
    /// Returns true, if a child with the given name exists
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns></returns>
    public bool HasChild(string name) {
      foreach (var child in Children) {
        if (child.Name == name) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Returns the Value of an Attribute of a given child
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="attribute">The Attribute</param>
    /// <returns></returns>
    public string GetChildAttribute(string name, string attribute) {
      string value = null;
      foreach (var child in Children) {
        if (child.Name == name) {
          child.Attributes.TryGetValue(attribute, out value);
        }
      }
      return value;
    }

    /// <summary>
    /// Returns the first child with the given name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns></returns>
    public BpmnElement GetChild(string name) {
      foreach (var child in Children) {
        if (child.Name == name) {
          return child;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns all children with the given name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns></returns>
    public IEnumerable<BpmnElement> GetChildren(string name) {
      var retChildren = new List<BpmnElement>();
      foreach (var child in Children) {
        if (child.Name == name) {
          retChildren.Add(child);
        }
      }
      return retChildren;
    }

    /// <summary>
    /// Retrieves the sourceRef string of the current element
    /// </summary>
    /// <returns>The sourceRef string</returns>
    public string LoadSourceFromChild() {
      var ret = "";
      foreach (var child in Children) {
        if (child.Name == BpmnDiConstants.SourceRefElement) {
          ret = child.Value;
        }
      }
      return ret;
    }

    /// <summary>
    /// Retrieves the targetRef string of the current element
    /// </summary>
    /// <returns>The targetRef string</returns>
    public string LoadTargetFromChild() {
      var ret = "";
      foreach (var child in Children) {
        if (child.Name == BpmnDiConstants.TargetRefElement) {
          ret = child.Value;
        }
      }
      return ret;
    }

    /// <summary>
    /// Sets the INode of all dataInput and dataOutput hidden children to the given node
    /// </summary>
    /// <param name="node">The node</param>
    public void SetINodeInputOutput(INode node) {
      foreach (var child in Children) {
        var name = child.Name;
        if (name == BpmnDiConstants.IoSpecificationElement) {
          foreach (var childChild in child.Children) {
            var childName = childChild.Name;
            if (childName == BpmnDiConstants.DataOutputElement || childName == BpmnDiConstants.DataInputElement) {
              childChild.Node = node;
            }
          }
        }
        if (name == BpmnDiConstants.DataInputElement) {
          child.Node = node;
        }
        if (name == BpmnDiConstants.DataOutputElement) {
          child.Node = node;
        }
        if (name == BpmnDiConstants.PropertyElement) {
          child.Node = node;
        }
      }
    }

    /// <summary>
    /// Returns the Loop Characteristics of this Element
    /// </summary>
    /// <returns></returns>
    public LoopCharacteristic GetLoopCharacteristics() {
      if (HasChild(BpmnDiConstants.MultiInstanceLoopCharacteristicsElement)) {
        if (GetChildAttribute(BpmnDiConstants.MultiInstanceLoopCharacteristicsElement, BpmnDiConstants.IsSequentialAttribute) == "true") {
          return LoopCharacteristic.Sequential;
        }

        return LoopCharacteristic.Parallel;
      }

      if (HasChild(BpmnDiConstants.StandardLoopCharacteristicsElement)) {
        return LoopCharacteristic.Loop;
      }

      return LoopCharacteristic.None;
    }

    /// <summary>
    /// Returns the value of the given attribute, or null
    /// </summary>
    /// <param name="attribute">The attribute</param>
    /// <returns>The value, or null</returns>
    public string GetValue(string attribute) {
      string value;
      Attributes.TryGetValue(attribute, out value);
      return value;
    }

    /// <summary>
    /// Searches until the nearest Ancestor in a given List of BpmnElements is found, or null if there is no ancestor
    /// in the List (Which means there is something wrong)
    /// </summary>
    /// <param name="planeElements">A list of BpmnElements</param>
    public BpmnDiagram GetNearestAncestor(Dictionary<BpmnElement, BpmnDiagram> planeElements) {
      var parent = Parent;
      while (parent != null) {
        if (planeElements.ContainsKey(parent)) {
          return planeElements[parent];
        }
        parent = parent.Parent;
      }
      return null;
    }
  }
}
