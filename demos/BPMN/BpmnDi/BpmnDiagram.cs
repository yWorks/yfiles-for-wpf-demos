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
using yWorks.Graph.Styles;
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  /// <summary>
  /// Class for BPMNDiagram Objects
  /// </summary>
  internal class BpmnDiagram
  {
    /// <summary>
    /// Id of this diagram
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// BPMNPlane of this diagram
    /// </summary>
    public BpmnPlane Plane { get; set; }

    /// <summary>
    /// List of all child diagrams this diagram contains
    /// </summary>
    public Dictionary<BpmnDiagram, BpmnElement> Children { get; private set; }

    /// <summary>
    /// All BPMNLabelStyle instances of this diagram
    /// </summary>
    private Dictionary<string, BpmnLabelStyle> Styles { get; set; }

    /// <summary>
    /// The name of this diagram
    /// </summary>
    public string Name { get; private set; }

    // These parameters are currently unused. They are part of the BPMN Syntax and might be used in the future.
    private string Documentation { get; set; }

    private string Resolution { get; set; }


    /// <summary>
    /// Constructs a new diagram instance
    /// </summary>
    /// <param name="xNode">The XML node which is the root for this diagram instance</param>
    public BpmnDiagram(XElement xNode) {
      Plane = null;
      Children = new Dictionary<BpmnDiagram, BpmnElement>();
      Styles = new Dictionary<string, BpmnLabelStyle>();
      Id = "";
      Documentation = "";
      Resolution = "";

      // Get name, if it exists
      Name = BpmnNM.GetAttributeValue(xNode, BpmnNM.Bpmn, BpmnDiConstants.NameAttribute);

      // Name Diagram "Unnamed", if it has no name (for choosing, if file contains multiple diagrams)
      if (string.IsNullOrEmpty(Name)) {
        Name = "Unnamed Diagram";
      }

      // Get id, if it exists
      Id = BpmnNM.GetAttributeValue(xNode, BpmnNM.Bpmn, BpmnDiConstants.IdAttribute);

      // Get documentation, if it exists
      Documentation = BpmnNM.GetAttributeValue(xNode, BpmnNM.Bpmn, BpmnDiConstants.DocumentationAttribute);

      // Get resolution, if it exists
      Resolution = BpmnNM.GetAttributeValue(xNode, BpmnNM.Bpmn, BpmnDiConstants.ResolutionAttribute);
    }

    /// <summary>
    /// The default label style for this diagram instance
    /// </summary>
    public DefaultLabelStyle DefaultStyle { get; set; }

    /// <summary>
    /// Adds a plane to this diagram. Only happens once, but is caught elsewhere
    /// </summary>
    /// <param name="plane"></param>
    public void AddPlane(BpmnPlane plane) {
      Plane = plane;
    }

    /// <summary>
    /// Adds a LabelStyle to the collection of styles in this diagram
    /// </summary>
    /// <param name="style">The <see cref="BpmnLabelStyle"/> to add</param>
    public void AddStyle(BpmnLabelStyle style) {
      Styles.Add(style.Id, style);
    }

    /// <summary>
    /// Returns the given Style, or the default style, in case it does nor exist
    /// </summary>
    /// <param name="style">The id (name) of the style to get</param>
    public DefaultLabelStyle GetStyle(string style) {
      if (style == null) {
        return (DefaultLabelStyle) DefaultStyle.Clone();
      }
      BpmnLabelStyle retStyle;
      if (Styles.TryGetValue(style, out retStyle)) {
        return (DefaultLabelStyle) retStyle.GetStyle().Clone();
      }
      return (DefaultLabelStyle) DefaultStyle.Clone();
    }

    /// <summary>
    /// Adds a child diagramm to this diagram
    /// </summary>
    /// <param name="diagram">The child diagram</param>
    /// <param name="localRoot">The local root element</param>
    public void AddChild(BpmnDiagram diagram, BpmnElement localRoot) {
      Children.Add(diagram, localRoot);
    }

    /// <returns>The name of the Diagram</returns>
    public override string ToString() {
      return Name;
    }
  }
}
