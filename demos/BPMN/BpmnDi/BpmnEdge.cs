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
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  /// <summary>
  /// Class for BPMNEdge objects
  /// </summary>
  internal class BpmnEdge
  {
    private static readonly SimpleLabel calculateSizeLabel = new SimpleLabel(new SimpleNode(), "", FreeNodeLabelModel.Instance.CreateDefaultParameter());
    private static readonly DefaultLabelStyle calculateSizeLabelStyle = new DefaultLabelStyle();

    /// <summary>
    /// Calculate the preferred size for <paramref name="text"/> using a <see cref="DefaultLabelStyle"/>.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The preferred Size of the given text.</returns>
    private static SizeD CalculatePreferredSize(string text) {
      calculateSizeLabel.Text = text;
      return calculateSizeLabelStyle.Renderer.GetPreferredSize(calculateSizeLabel, calculateSizeLabelStyle);
    }

        /// <summary>
        /// The <see cref="BpmnElement"/> this edge references to
        /// </summary>
        public BpmnElement Element { get; private set; }

        /// <summary>
        /// True, if this edge has a label
        /// </summary>
        public bool HasLabel { get; set;  }

        /// <summary>
        /// The label bounds of this edge
        /// </summary>
        public RectD LabelBounds { get; set; }

        /// <summary>
        /// Visibility of a message envelope on this edge
        /// </summary>
        public MessageVisibleKind MessageVisibleK { get; private set; }

        /// <summary>
        /// The source element of this edge
        /// </summary>
        public BpmnElement Source { get; private set; }

        /// <summary>
        /// The target element of this edge
        /// </summary>
        public BpmnElement Target { get; private set; }

        /// <summary>
        /// List of all waypoints (ports and bends)
        /// </summary>
        public List<PointD> Waypoints { get; private set; }

        /// <summary>
        /// The id of this edge
        /// </summary>
        public string Id { get; private set; }

    /// <summary>
    /// The custom style of this label
    /// </summary>
    public string LabelStyle { get; set; }

    /// <summary>
    /// Constructs a new edge instance
    /// </summary>
    /// <param name="xEdge">The XML element which represents this edge</param>
    /// <param name="elements">Dictionary of all bpmn elements from this file parsing</param>
    public BpmnEdge(XElement xEdge, IDictionary<string, BpmnElement> elements) {
      Waypoints = new List<PointD>();
      HasLabel = false;
      LabelBounds = new RectD(0, 0, 0, 0);
      Source = null;
      Target = null;
      MessageVisibleK = MessageVisibleKind.Unspecified;

            // Get id
            Id = BpmnNM.GetAttributeValue(xEdge, BpmnNM.BpmnDi, BpmnDiConstants.IdAttribute);
            // Get and link element   
            if (BpmnNM.GetAttributeValue(xEdge, BpmnNM.BpmnDi, BpmnDiConstants.BpmnElementAttribute) != null) {
                BpmnElement element;
                Element = elements.TryGetValue(BpmnNM.GetAttributeValue(xEdge, BpmnNM.BpmnDi, BpmnDiConstants.BpmnElementAttribute), out element) ? element : null;
            }
            
            // If there is no element, skip
            if (Element == null) {
                return;
            }
            
            // Source and target elements can be specified as attribute of the element
            // or as children of the element (in data associations).
            BpmnElement source;
            string sourceRef;
            BpmnElement target;
            string targetRef;
            
            // Getting source element id
            var sourceVar = Element.Source;
            if (sourceVar != null) {
                sourceRef = sourceVar;
            } else {
                sourceRef = Element.LoadSourceFromChild();
            }
            
            // Getting and linking source element
            if (elements.TryGetValue(sourceRef, out source)) {
                Source = source;
            }


            // Getting target element id
            var targetVar = Element.Target;
            if (targetVar != null) {
                targetRef = targetVar;
            } else {
                targetRef = Element.LoadTargetFromChild();
            }
            
            // Getting and linking target element
            if (elements.TryGetValue(targetRef, out target)) {
                Target = target;
            }
            
            switch (BpmnNM.GetAttributeValue(xEdge, BpmnNM.BpmnDi, BpmnDiConstants.MessageVisibleKindAttribute)) {
                case "non_initiating":
                    MessageVisibleK = MessageVisibleKind.NonInitiating;
                    break;
                case "initiating":
                    MessageVisibleK = MessageVisibleKind.Initiating;
                    break;
                default:
                    MessageVisibleK = MessageVisibleKind.Unspecified;
                    break;
            }
        }

        /// <summary>
        /// Add a label and its bounds to the edge
        /// </summary>
        /// <param name="xBounds">The XML element of the label bounds</param>
        public void AddLabel(XElement xBounds)
        {
            HasLabel = true;
            if (xBounds == null) return;
            
            // If there are bounds, set standard values, first.
            double labelX = 0;
            double labelY = 0;
            double labelWidth = 100;
            double labelHeight = 20;

            var attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.XAttribute);
            if (attr != null) {
                labelX = double.Parse(attr, CultureInfo.InvariantCulture);
            }
            
            attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.YAttribute);
            if (attr != null) {
                labelY = double.Parse(attr, CultureInfo.InvariantCulture);
            }
            
            attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.HeightAttribute);
            if (attr != null) {
                labelHeight = double.Parse(attr,
                        CultureInfo.InvariantCulture);
            }
            
            attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.WidthAttribute);
            if (attr != null) {
                labelWidth = double.Parse(attr,
                        CultureInfo.InvariantCulture);
            }

      // In case, the label sizes were set to 0
      if (labelWidth < 1 || labelHeight < 1) {
        var text = Element.Label;
        var preferredSize = CalculatePreferredSize(text);

        labelWidth = preferredSize.Width;
        labelHeight = preferredSize.Height;
      }

      LabelBounds = new RectD(labelX, labelY, labelWidth, labelHeight);
    }

        /// <summary>
        /// Adds a waypoint to the edge
        /// </summary>
        /// <param name="xWaypoint">The waypoint to add</param>
        public void AddWayPoint(XElement xWaypoint)
        {
            double x = 0;
            double y = 0;

            var attr = BpmnNM.GetAttributeValue(xWaypoint, BpmnNM.Di, BpmnDiConstants.XAttribute);
            if (attr != null) {
                x = double.Parse(attr, CultureInfo.InvariantCulture);
            }
            attr = BpmnNM.GetAttributeValue(xWaypoint, BpmnNM.Di, BpmnDiConstants.YAttribute);
            if (attr != null) {
                y = double.Parse(attr, CultureInfo.InvariantCulture);
            }
            
            var tuple = new PointD(x, y);
            Waypoints.Add(tuple);
        }
        
        /// <summary>
        /// Returns true, if the edge has width and height attributes set.
        /// </summary>
        /// <returns>True, if the label has size, false if not</returns>
        public bool HasLabelSize() {
            return LabelBounds.Width > 0 && LabelBounds.Height > 0;
        }

    /// <summary>
    /// Returns true, if the top left point of the bounds is not 0/0 (standard case) 
    /// </summary>
    /// <returns>True, if the label has a given position, false if it is 0/0</returns>
    public bool HasLabelPosition() {
      return LabelBounds.X > 0 && LabelBounds.Y > 0;
    }

    /// <summary>
    /// Returns the value of the given attribute of the linked BpmnElement, or null
    /// </summary>
    /// <param name="attribute">Id (name) of the attribute to get</param>
    /// <returns>value of the attribute or null</returns>
    public string GetAttribute(string attribute) {
      return Element.GetValue(attribute);
    }
  }

  /// <summary>
  /// Visibility of a message envelope on an edge
  /// </summary>
  [DefaultValue(Unspecified)]
  internal enum MessageVisibleKind
  {
    Unspecified,
    Initiating,
    NonInitiating
  }
}
