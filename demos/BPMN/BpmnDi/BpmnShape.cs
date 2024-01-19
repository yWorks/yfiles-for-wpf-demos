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
using System.Globalization;
using System.Xml.Linq;
using yWorks.Geometry;
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  /// <summary>
  /// Class for BPMNShape objects
  /// </summary>
  internal class BpmnShape
  {
    /// <summary>
    /// The string id of the choreographyActivityShape if this shape is depicting a participant band
    /// </summary>
    public string ChoreographyActivityShape { get; set; }

    /// <summary>
    /// The <see cref="BpmnElement"/> this shape refers to
    /// </summary>
    public BpmnElement Element { get; set; }

    /// <summary>
    /// String id of the expansion state of this shape
    /// </summary>
    public string IsExpanded { get; set; }

    /// <summary>
    /// Attribute which indicates the orientation if this is a pool or lane
    /// </summary>
    public bool? IsHorizontal { get; set; }

    /// <summary>
    /// Determines, if a marker should be depicted on the shape for exclusive Gateways.
    /// </summary>
    public bool IsMarkerVisible { get; set; }

    /// <summary>
    /// Determines, if a message envelope should be depicted connected to the shape for participant bands
    /// </summary>
    public bool IsMessageVisible { get; set; }

    /// <summary>
    /// Is true, if this shape has a label
    /// </summary>
    public bool HasLabel { get; set; }

    /// <summary>
    /// Bounds for the shapes label, if it has one
    /// </summary>
    public RectD LabelBounds { get; set; }

    /// <summary>
    /// Determines the kind of the participant band, if this participant should be depicted as participant band
    /// instead of beeing depicted as lane
    /// </summary>
    public ParticipantBandKind PartBandKind { get; set; }

    /// <summary>
    /// Height of the shape
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Width of the shape
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// X position of the upper left corner of this shape
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y position of the upper left corner of this shape
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Id of this shape
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Custom <see cref="BpmnLabelStyle"/> for the label of this shape
    /// </summary>
    public String LabelStyle { get; set; }

    /// <summary>
    /// Constructs a new shape instance
    /// </summary>
    /// <param name="xShape">The XML element which represents this shape</param>
    /// <param name="elements">Dictionary of all bpmn elements from this file parsing</param>
    public BpmnShape(XElement xShape, IDictionary<string, BpmnElement> elements) {
      HasLabel = false;
      LabelBounds = new RectD(0, 0, 0, 0);
      X = 0;
      Y = 0;
      Height = 30;
      Width = 30;

      // Get and Link the corresponding element
      BpmnElement element;
      Element = elements.TryGetValue(BpmnNM.GetAttributeValue(xShape, BpmnNM.BpmnDi, BpmnDiConstants.BpmnElementAttribute), out element) ? element : null;

      // If there is no element, skip
      if (Element == null) {
        return;
      }

      // Get the id
      Id = BpmnNM.GetAttributeValue(xShape, BpmnNM.BpmnDi, BpmnDiConstants.IdAttribute);

      // Get all additional Attributes
      var isHorizontalString = BpmnNM.GetAttributeValue(xShape, BpmnNM.BpmnDi, BpmnDiConstants.IsHorizontalAttribute);
      IsHorizontal = isHorizontalString != null ? (bool?) Convert.ToBoolean(isHorizontalString) : null;
      IsExpanded = BpmnNM.GetAttributeValue(xShape, BpmnNM.BpmnDi, BpmnDiConstants.IsExpandedAttribute);
      IsMarkerVisible = Convert.ToBoolean(BpmnNM.GetAttributeValue(xShape, BpmnNM.BpmnDi, BpmnDiConstants.IsMarkerVisibleAttribute));
      IsMessageVisible = Convert.ToBoolean(BpmnNM.GetAttributeValue(xShape, BpmnNM.BpmnDi, BpmnDiConstants.IsMessageVisibleAttribute));
      ChoreographyActivityShape = BpmnNM.GetAttributeValue(xShape, BpmnNM.BpmnDi, BpmnDiConstants.ChoreographyActivityShapeAttribute);

      switch (BpmnNM.GetAttributeValue(xShape, BpmnNM.BpmnDi, BpmnDiConstants.ParticipantBandKindAttribute)) {
        case "top_non_initiating":
          PartBandKind = ParticipantBandKind.TopNonInitiating;
          break;
        case "top_initiating":
          PartBandKind = ParticipantBandKind.TopInitiating;
          break;
        case "middle_non_initiating":
          PartBandKind = ParticipantBandKind.MiddleNonInitiating;
          break;
        case "middle_initiating":
          PartBandKind = ParticipantBandKind.MiddleInitiating;
          break;
        case "bottom_non_initiating":
          PartBandKind = ParticipantBandKind.BottomNonInitiating;
          break;
        case "bottom_initiating":
          PartBandKind = ParticipantBandKind.BottomInitiating;
          break;
      }
    }

        /// <summary>
        /// Adds the bound of this shape
        /// </summary>
        /// <param name="xBounds">XML element representing the bounds</param>
        public void AddBounds(XElement xBounds) {

            var attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.XAttribute);
            if (attr != null) {
                X = double.Parse(attr, CultureInfo.InvariantCulture);
            }
            
            attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.YAttribute);
            if (attr != null) {
                Y = double.Parse(attr, CultureInfo.InvariantCulture);
            }
            
            attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.HeightAttribute);
            if (attr != null) {
                Height = double.Parse(attr, CultureInfo.InvariantCulture);
            }
            
            attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.WidthAttribute);
            if (attr != null) {
                Width = double.Parse(attr, CultureInfo.InvariantCulture);
            }
            
            //Check for size 0
            if (Height == 0) {
                Height = 30;
            }

      if (Width == 0) {
        Width = 30;
      }
    }

    /// <summary>
    /// Add the label bounds for this shapes label
    /// </summary>
    /// <param name="xBounds">The XML element representing this labels bounds</param>
    public void AddLabel(XElement xBounds) {
      HasLabel = true;
      if (xBounds == null) return;

      // If there are bounds, set standard values, first.
      double labelX = 0;
      double labelY = 0;
      double labelWidth = 0;
      double labelHeight = 0;

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
        labelHeight = double.Parse(attr, CultureInfo.InvariantCulture);
      }

      attr = BpmnNM.GetAttributeValue(xBounds, BpmnNM.Dc, BpmnDiConstants.WidthAttribute);
      if (attr != null) {
        labelWidth = double.Parse(attr, CultureInfo.InvariantCulture);
      }

      // In case, the label sizes were set to 0
      if (labelWidth < 1) {
        labelWidth = 100;
      }
      if (labelHeight < 1) {
        labelHeight = 20;
      }

      LabelBounds = new RectD(labelX, labelY, labelWidth, labelHeight);
    }

    /// <summary>
    /// Returns the value of the given attribute of the linked <see cref="BpmnElement"/>, or null
    /// </summary>
    /// <param name="attribute">The attribute to receive</param>
    /// <returns></returns>
    public string GetAttribute(string attribute) {
      return Element.GetValue(attribute);
    }

    /// <summary>
    /// Returns true, if the shape has width and height attributes set.
    /// </summary>
    /// <returns>True, if the label has size, false if not</returns>
    public bool HasLabelSize() {
      return LabelBounds.Width > 0 && LabelBounds.Height > 0;
    }

    /// <summary>
    /// Returns true, if the top left point of the bounds is not 0/0 (standard case) 
    /// </summary>
    /// <returns>True, if the label position is 0/0</returns>
    public bool HasLabelPosition() {
      return LabelBounds.X > 0 && LabelBounds.Y > 0;
    }
  }

  /// <summary>
  /// Enum for the different participant bands
  /// </summary>
  internal enum ParticipantBandKind
  {
    BottomInitiating,
    TopNonInitiating,
    MiddleNonInitiating,
    BottomNonInitiating,
    TopInitiating,

    // This should never happen
    MiddleInitiating
  }
}
