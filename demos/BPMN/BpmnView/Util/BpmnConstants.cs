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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.Bpmn.Util {
  [TypeConverter(typeof(BpmnConstantConverter))]
  internal static class BpmnConstants {

    public static readonly double DoubleLineOffset = 2;

    public static readonly double ChoreographyCornerRadius = 6;
    public static readonly double GroupNodeCornerRadius = 3;


    /// <summary>
    /// The namespace URI for yFiles BPMN extensions to GraphML.
    /// </summary>
    public const string YfilesBpmnNS = "http://www.yworks.com/xml/yfiles-bpmn/2.0";

    /// <summary>
    /// The namespace URI for the older yFiles BPMN extensions to GraphML.
    /// </summary>
    /// <remarks>
    /// This is the version of the styles without changeable colors.
    /// </remarks>
    public const string YfilesBpmnLegacyNS = "http://www.yworks.com/xml/yfiles-bpmn/1.0";

    /// <summary>
    /// The default namespace prefix for <see cref="YfilesBpmnNS"/>.
    /// </summary>
    /// <remarks>This field has the constant value <c>"bpmn"</c></remarks>
    /// <seealso cref="YfilesBpmnNS"/>
    public const string YfilesBpmnPrefix = "bpmn";

    // Shared constants that apply to several different items

    public static readonly Brush DefaultBackground = (Brush) new SolidColorBrush(Color.FromArgb(255,250, 250, 250)).GetAsFrozen();
    public static readonly Brush DefaultIconColor = Brushes.Black;
    public static readonly Brush DefaultEventOutline = null; // null triggers fallback to characteristic-specific colors
    public static readonly Brush DefaultMessageOutline = Brushes.Black;
    public static readonly Brush DefaultInitiatingColor = Brushes.White;
    public static readonly Brush DefaultReceivingColor = Brushes.Gray;

    // Activity
    public static readonly double ActivityCornerRadius = 6;
    public static readonly Brush ActivityDefaultBackground = DefaultBackground;
    public static readonly Brush ActivityDefaultOutline = Brushes.DarkBlue;
    // Gateway
    public static readonly Brush GatewayDefaultBackground = DefaultBackground;
    public static readonly Brush GatewayDefaultOutline = Brushes.DarkOrange;
    // Annotation
    public static readonly Brush AnnotationDefaultBackground = DefaultBackground;
    public static readonly Brush AnnotationDefaultOutline = Brushes.Black;
    // Edges
    public static readonly Brush EdgeDefaultColor = Brushes.Black;
    public static readonly Brush EdgeDefaultInnerColor = Brushes.White;
    // Choreography
    public static readonly Brush ChoreographyDefaultBackground = DefaultBackground;
    public static readonly Brush ChoreographyDefaultOutline = Brushes.DarkGreen;
    public static readonly Brush ChoreographyDefaultIconColor = Brushes.Black;
    public static readonly Brush ChoreographyDefaultMessageOutline = DefaultMessageOutline;
    public static readonly Brush ChoreographyDefaultInitiatingColor = DefaultInitiatingColor;
    public static readonly Brush ChoreographyDefaultResponseColor = DefaultReceivingColor;
    // Conversation
    public static readonly Brush ConversationDefaultOutline = Brushes.DarkGreen;
    public static readonly Brush ConversationDefaultBackground = DefaultBackground;
    // Data object
    public static readonly Brush DataObjectDefaultBackground = Brushes.White;
    public static readonly Brush DataObjectDefaultOutline = Brushes.Black;
    // Data store
    public static readonly Brush DataStoreDefaultOutline = Brushes.Black;
    public static readonly Brush DataStoreDefaultBackground = Brushes.White;
    // Event
    public static readonly Brush DefaultEventBackground = DefaultBackground;
    // Group
    public static readonly Brush GroupDefaultBackground = null;
    public static readonly Brush GroupDefaultOutline = Brushes.Black;
    // Messages
    public static readonly Brush DefaultInitiatingMessageColor = DefaultInitiatingColor;
    public static readonly Brush DefaultReceivingMessageColor = DefaultReceivingColor;
    // Pools
    public static readonly Brush DefaultPoolNodeBackground = (Brush) new SolidColorBrush(Color.FromArgb(255,0xE0, 0xE0, 0xE0)).GetAsFrozen();
    public static readonly Brush DefaultPoolNodeEvenLeafBackground = (Brush) new SolidColorBrush(Color.FromArgb(255, 196, 215, 237)).GetAsFrozen();
    public static readonly Brush DefaultPoolNodeEvenLeafInset = (Brush) new SolidColorBrush(Color.FromArgb(255,0xE0, 0xE0, 0xE0)).GetAsFrozen();
    public static readonly Brush DefaultPoolNodeOddLeafBackground = (Brush) new SolidColorBrush(Color.FromArgb(255, 171, 200, 226)).GetAsFrozen();
    public static readonly Brush DefaultPoolNodeOddLeafInset = (Brush) new SolidColorBrush(Color.FromArgb(255,0xE0, 0xE0, 0xE0)).GetAsFrozen();
    public static readonly Brush DefaultPoolNodeParentBackground = (Brush) new SolidColorBrush(Color.FromArgb(255, 113, 146, 178)).GetAsFrozen();
    public static readonly Brush DefaultPoolNodeParentInset = (Brush) new SolidColorBrush(Color.FromArgb(255,0xE0, 0xE0, 0xE0)).GetAsFrozen();

    // Placement constants for where parts of item visualizations should appear

    private static readonly InteriorLabelModel ilm2 = new InteriorLabelModel { Insets = new InsetsD(2) };
    private static readonly InteriorLabelModel ilm6 = new InteriorLabelModel { Insets = new InsetsD(6) };
    private static readonly InteriorStretchLabelModel islmInsideDoubleLine = new InteriorStretchLabelModel { Insets = new InsetsD(2 * DoubleLineOffset + 1) };
    private static readonly ExteriorLabelModel elm15 = new ExteriorLabelModel { Insets = new InsetsD(15) };
    private static readonly ScalingLabelModel slm = new ScalingLabelModel();
    private static readonly ScalingLabelModel slm3 = new ScalingLabelModel { Insets = new InsetsD(3) };
    public static readonly ILabelModelParameter TaskTypePlacement = ilm6.CreateParameter(InteriorLabelModel.Position.NorthWest);
    public static readonly ILabelModelParameter TaskMarkerPlacement = islmInsideDoubleLine.CreateParameter(InteriorStretchLabelModel.Position.South);
    public static readonly ILabelModelParameter ChoreographyMarkerPlacement = ilm2.CreateParameter(InteriorLabelModel.Position.South);
    public static readonly ILabelModelParameter ChoreographyTopMessagePlacement = elm15.CreateParameter(ExteriorLabelModel.Position.North);
    public static readonly ILabelModelParameter ChoreographyBottomMessagePlacement = elm15.CreateParameter(ExteriorLabelModel.Position.South);
    private static readonly double ratioWidthHeight = 1 / Math.Sin(Math.PI / 3.0);
    public static readonly ILabelModelParameter ConversationPlacement = slm.CreateScaledParameterWithRatio(1, ratioWidthHeight);
    public static readonly ILabelModelParameter ConversationMarkerPlacement = ilm2.CreateParameter(InteriorLabelModel.Position.South);
    public static readonly ILabelModelParameter DataObjectTypePlacement = ilm2.CreateParameter(InteriorLabelModel.Position.NorthWest);
    public static readonly ILabelModelParameter DataObjectMarkerPlacement = ilm2.CreateParameter(InteriorLabelModel.Position.South);
    public static readonly ILabelModelParameter EventPlacement = slm.CreateScaledParameterWithRatio(1, 1);
    public static readonly ILabelModelParameter EventTypePlacement = slm3.CreateScaledParameterWithRatio(0.9, 1);
    public static readonly ILabelModelParameter GatewayPlacement = slm.CreateScaledParameterWithRatio(1, 1);
    public static readonly ILabelModelParameter GatewayTypePlacement = slm.CreateScaledParameterWithRatio(0.6, 1);
    public static readonly ILabelModelParameter EventTypeMessagePlacement = slm.CreateScaledParameterWithRatio(0.8, 1.4);
    public static readonly ILabelModelParameter ActivityTaskTypeMessagePlacement = slm.CreateScaledParameterWithRatio(1, 1.4);
    public static readonly ILabelModelParameter DoubleLinePlacement = new InteriorStretchLabelModel { Insets = new InsetsD(DoubleLineOffset) }.CreateParameter(InteriorStretchLabelModel.Position.Center);
    public static readonly ILabelModelParameter ThickLinePlacement = new InteriorStretchLabelModel { Insets = new InsetsD(DoubleLineOffset / 2) }.CreateParameter(InteriorStretchLabelModel.Position.Center);
    public static readonly ILabelModelParameter InsideDoubleLinePlacement = islmInsideDoubleLine.CreateParameter(InteriorStretchLabelModel.Position.Center);
    public static readonly ILabelModelParameter PoolNodeMarkerPlacement = ilm2.CreateParameter(InteriorLabelModel.Position.South);

    // Default sizes for different items

    public static readonly SizeD MarkerSize = new SizeD(10, 10);
    public static readonly SizeD TaskTypeSize = new SizeD(15, 15);
    public static readonly SizeD MessageSize = new SizeD(20, 14);
    public static readonly double ConversationWidthHeightRatio = Math.Sin(Math.PI / 3.0);
    public static readonly SizeD ConversationSize = new SizeD(20, 20 * ConversationWidthHeightRatio);
    public static readonly SizeD DataObjectTypeSize = new SizeD(10, 8);
    public static readonly SizeD EventPortSize = new SizeD(20, 20);
  }

  internal class BpmnConstantConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
      return true;
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
      return typeof(BpmnConstants).GetField((string) value).GetValue(null);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
      return false;
    }
  }
}
