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

using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using yWorks.Graph.Styles;
// Just for better readability in code
using BpmnNM = Demo.yFiles.Graph.Bpmn.BpmnDi.BpmnNamespaceManager;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  /// <summary>
  /// Class for BPMNLabelStyle objects
  /// </summary>
  public class BpmnLabelStyle
  {
    /// <summary>
    /// Constant that sets the standard Text size of Labels.
    /// yFiles Standard is 12 pt, but the Bpmn Demo files look better with 11pt
    /// </summary>
    private const double LabelTextSize = 11d;

    /// <summary>
    /// The id (name) of this style
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The font used in this style
    /// </summary>
    public string Font { get; set; }

    /// <summary>
    /// The font size used in this style
    /// </summary>
    public double Size { get; set; }

    /// <summary>
    /// True, if this style depicts text in bold
    /// </summary>
    public bool IsBold { get; set; }

    /// <summary>
    /// True, if this style depicts text in italic
    /// </summary>
    public bool IsItalic { get; set; }

    /// <summary>
    /// True, if this style underlines text
    /// </summary>
    public bool IsUnderline { get; set; }

    /// <summary>
    /// True, if this style depicts style with a StrikeThrough
    /// </summary>
    public bool IsStrikeThrough { get; set; }

    /// <summary>
    /// <see cref="DefaultLabelStyle"/> that represents this BpmnLabelStyle
    /// </summary>
    public DefaultLabelStyle LabelStyle { get; set; }

    /// <summary>
    /// The default label Style
    /// </summary>
    /// <returns></returns>
    public static DefaultLabelStyle NewDefaultInstance() {
      var defaultLabelStyle = new DefaultLabelStyle();

      // Set font
      const string font = "Arial";

      // Set text size
      defaultLabelStyle.TextSize = LabelTextSize;

      // Set font style
      var fontStyle = FontStyles.Normal;

      // Set font weight
      var fontWeight = FontWeights.Normal;

      // Set Typeface
      defaultLabelStyle.Typeface = new Typeface(new FontFamily(font), fontStyle, fontWeight, FontStretches.Normal);
      defaultLabelStyle.TextWrapping = TextWrapping.Wrap;
      defaultLabelStyle.TextAlignment = TextAlignment.Center;
      defaultLabelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      return defaultLabelStyle;
    }

    /// <summary>
    /// Constructs an instance of <see cref="DefaultLabelStyle"/> representing this Style
    /// </summary>
    /// <param name="xStyle">The XML Element to be converted into this style</param>
    public BpmnLabelStyle(XElement xStyle) {
      Id = null;
      Font = "Arial";
      Size = 0;
      IsBold = false;
      IsItalic = false;
      IsUnderline = false;
      IsStrikeThrough = false;

      Id = BpmnNM.GetAttributeValue(xStyle, BpmnNM.Dc, BpmnDiConstants.IdAttribute);

      // Parse Values of the Label Style
      var xFont = BpmnNM.GetElement(xStyle, BpmnNM.Dc, BpmnDiConstants.FontElement);
      if (xFont != null) {
        Font = BpmnNM.GetAttributeValue(xFont, BpmnNM.Dc, BpmnDiConstants.NameAttribute);

        var attr = BpmnNM.GetAttributeValue(xFont, BpmnNM.Dc, BpmnDiConstants.SizeAttribute);
        if (attr != null) {
          Size = double.Parse(attr, CultureInfo.InvariantCulture);
        }

        attr = BpmnNM.GetAttributeValue(xFont, BpmnNM.Dc, BpmnDiConstants.IsBoldAttribute);
        if (attr != null) {
          IsBold = bool.Parse(attr);
        }
        attr = BpmnNM.GetAttributeValue(xFont, BpmnNM.Dc, BpmnDiConstants.IsItalicAttribute);
        if (attr != null) {
          IsItalic = bool.Parse(attr);
        }

        attr = BpmnNM.GetAttributeValue(xFont, BpmnNM.Dc, BpmnDiConstants.IsUnderlineAttribute);
        if (attr != null) {
          IsUnderline = bool.Parse(attr);
        }

        attr = BpmnNM.GetAttributeValue(xFont, BpmnNM.Dc, BpmnDiConstants.IsStrikeThroughAttribute);
        if (attr != null) {
          IsStrikeThrough = bool.Parse(attr);
        }
      }

      LabelStyle = new DefaultLabelStyle {
          TextAlignment = TextAlignment.Center, VerticalTextAlignment = VerticalAlignment.Center
      };

      var fontStyle = FontStyles.Normal;
      var fontWeight = FontWeights.Normal;

      // Set text size
      if (Size > 0) {
        LabelStyle.TextSize = Size;
      } else {
        LabelStyle.TextSize = LabelTextSize;
      }

      // Set Boldness
      if (IsBold) {
        fontWeight = FontWeights.Bold;
      }

      // Set Italic
      if (IsItalic) {
        fontStyle = FontStyles.Italic;
      }
      LabelStyle.Typeface = new Typeface(new FontFamily(Font), fontStyle, fontWeight, FontStretches.Normal);
      LabelStyle.TextDecorations = new TextDecorationCollection();

      // Set Underline
      if (IsUnderline) {
        LabelStyle.TextDecorations.Add(TextDecorations.Underline);
      }

      // Set StrikeThrough
      if (IsStrikeThrough) {
        LabelStyle.TextDecorations.Add(TextDecorations.Strikethrough);
      }

      LabelStyle.TextWrapping = TextWrapping.Wrap;
    }

    /// <summary>
    /// Returns the <see cref="DefaultLabelStyle"/> that represents this BpmnLabelStyle instance
    /// </summary>
    /// <returns></returns>
    public DefaultLabelStyle GetStyle() {
      return LabelStyle;
    }
  }
}
