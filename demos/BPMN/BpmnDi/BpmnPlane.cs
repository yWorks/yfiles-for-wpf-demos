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

using System.Collections.Generic;
using System.Xml.Linq;
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  /// <summary>
  /// Class for BPMNPlane objects
  /// </summary>
  internal class BpmnPlane
  {
    /// <summary>
    /// The <see cref="BpmnElement"/> this plane refers to
    /// </summary>
    public BpmnElement Element { get; set; }

    /// <summary>
    /// List of all <see cref="BpmnEdge"/>s in this plane
    /// </summary>
    public List<BpmnEdge> ListOfEdges { get; set; }

    /// <summary>
    /// List of all <see cref="BpmnShape"/>s in this plane
    /// </summary>
    public List<BpmnShape> ListOfShapes { get; set; }


    /// <summary>
    /// Constructs a new plane instance
    /// </summary>
    /// <param name="xNode">The XML element which represents this plane</param>
    /// <param name="elements">Dictionary of all bpmn elements from this file parsing</param>
    public BpmnPlane(XElement xNode, IDictionary<string, BpmnElement> elements) {
      ListOfEdges = new List<BpmnEdge>();
      ListOfShapes = new List<BpmnShape>();

      // A BPMNPlane only has one bpmnElement and no further attributes
      BpmnElement element;
      Element = elements.TryGetValue(BpmnNM.GetAttributeValue(xNode, BpmnNM.BpmnDi, BpmnDiConstants.BpmnElementAttribute), out element) ? element : null;
    }

    /// <summary>
    /// Adds a new <see cref="BpmnEdge"/> to this planes list of edges.
    /// </summary>
    /// <param name="edge">Edge to add</param>
    public void AddEdge(BpmnEdge edge) {
      ListOfEdges.Add(edge);
    }

    /// <summary>
    /// Adds a new <see cref="BpmnShape"/> to this planes list of shapes.
    /// </summary>
    /// <param name="shape">Shape to add</param>
    public void AddShape(BpmnShape shape) {
      ListOfShapes.Add(shape);
    }

    /// <summary>
    /// Returns the <see cref="BpmnShape"/> with the given id.
    /// </summary>
    /// <param name="id">Id</param>
    /// <returns><see cref="BpmnShape"/> with the given id, or null if no <see cref="BpmnShape"/>
    /// with this id exists</returns>
    public BpmnShape GetShape(string id) {
      foreach (var shape in ListOfShapes) {
        if (shape.Id == id) {
          return shape;
        }
      }
      return null;
    }
  }
}
