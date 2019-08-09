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
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;
using SystemPens = yWorks.Controls.Pens;
using SystemBrushes = System.Windows.Media.Brushes;

namespace Demo.yFiles.Graph.Bpmn.Util {
  internal static class BpmnConstants {

    public static readonly double DoubleLineOffset = 2;

    public static readonly double ActivityCornerRadius = 6;
    public static readonly double ChoreographyCornerRadius = 6;
    public static readonly double GroupNodeCornerRadius = 3;


    /// <summary>
    /// The namespace URI for yFiles bpmn extensions to graphml.
    /// </summary>
    /// <remarks>This field has the constant value <c>http://www.yworks.com/xml/yfiles-bpmn/1.0</c></remarks>
    public const string YfilesBpmnNS = "http://www.yworks.com/xml/yfiles-bpmn/1.0";

    /// <summary>
    /// The default namespace prefix for <see cref="YfilesBpmnNS"/>.
    /// </summary>
    /// <remarks>This field has the constant value <c>"bpmn"</c></remarks>
    /// <seealso cref="YfilesBpmnNS"/>
    public const string YfilesBpmnPrefix = "bpmn";

    internal static class Pens
    {
      public static readonly Pen Task = SystemPens.DarkBlue;
      public static readonly Pen CallActivity = new Pen { Brush = SystemBrushes.DarkBlue, Thickness = 3 };
      public static readonly Pen ActivityEventSubProcess = new Pen { Brush = SystemBrushes.DarkBlue, DashCap = PenLineCap.Round, DashStyle = DashStyles.Dot };

      public static readonly Pen ActivityTaskRound = new Pen { Brush = SystemBrushes.Black, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
      public static readonly Pen TaskTypeService = new Pen() { Brush = SystemBrushes.Black, Thickness = 0.3 };

      public static readonly Pen Annotation = SystemPens.Black;

      public static readonly Pen ChoreographyTask = SystemPens.DarkGreen;
      public static readonly Pen ChoreographyCall = new Pen { Brush = SystemBrushes.DarkGreen, Thickness = 3 };
      public static readonly Pen ChoreographyMessageLink = new Pen { Brush = SystemBrushes.Black, DashStyle = DashStyles.Dot, DashCap = PenLineCap.Round };

      public static readonly Pen Conversation = SystemPens.DarkGreen;
      public static readonly Pen CallingConversation = new Pen { Brush = SystemBrushes.DarkGreen, Thickness = 3 };

      public static readonly Pen DataObject = SystemPens.Black;

      public static readonly Pen DataStore = SystemPens.Black;

      public static readonly Pen EventStart = SystemPens.Green;
      public static readonly Pen EventSubProcessNonInterrupting = new Pen { Brush = SystemBrushes.Green, DashStyle = DashStyles.Dash };
      public static readonly Pen EventIntermediate = SystemPens.Goldenrod;
      public static readonly Pen EventBoundaryNonInterrupting = new Pen { Brush = SystemBrushes.Goldenrod, DashStyle = DashStyles.Dash };
      public static readonly Pen EventEnd = new Pen { Brush = SystemBrushes.Red, Thickness = 3 };
      public static readonly Pen EventType = new Pen { Brush = SystemBrushes.Black, MiterLimit = 3 };
      public static readonly Pen EventTypeDetail = new Pen {
        Brush = SystemBrushes.Black,
        StartLineCap = PenLineCap.Round,
        EndLineCap = PenLineCap.Round
      };
      public static readonly Pen EventTypeDetailInverted = new Pen {
        Brush = SystemBrushes.White,
        StartLineCap = PenLineCap.Round,
        EndLineCap = PenLineCap.Round
      };

      public static readonly Pen Gateway = SystemPens.DarkOrange;
      public static readonly Pen GatewayTypeInclusive = new Pen { Brush = SystemBrushes.Black, Thickness = 3 };

      public static readonly Pen GroupNode = new Pen() { DashStyle = DashStyles.DashDot, DashCap = PenLineCap.Round, Brush = SystemBrushes.Black };


      public static readonly Pen Message = SystemPens.Black;
      public static readonly Pen MessageInverted = SystemPens.White;


      public static readonly Pen Arrow = new Pen { Brush = SystemBrushes.Black, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };

      public static readonly Pen AssociationEdgeStyle = new Pen { Brush = SystemBrushes.Black, DashStyle = DashStyles.Dot, DashCap = PenLineCap.Round };
      public static readonly Pen BpmnEdgeStyle = SystemPens.Black;
      public static readonly Pen ConversationDoubleLine = new Pen { Brush = SystemBrushes.Black, Thickness = 3, LineJoin = PenLineJoin.Round };
      public static readonly Pen ConversationCenterLine = new Pen { Brush = SystemBrushes.White, Thickness = 1, LineJoin = PenLineJoin.Round };
      public static readonly Pen MessageEdgeStyle = new Pen { Brush = SystemBrushes.Black, DashStyle = DashStyles.Dash };
    }

    internal static class Brushes {
      public static readonly Brush Activity = new SolidColorBrush(Color.FromArgb(255,250, 250, 250));
      public static readonly Brush ActivityTaskLight = SystemBrushes.LightGray;
      public static readonly Brush ActivityTaskDark = SystemBrushes.Gray;
      public static readonly Brush Annotation = new SolidColorBrush(Color.FromArgb(255,250, 250, 250));
      public static readonly Brush Conversation = new SolidColorBrush(Color.FromArgb(255,250, 250, 250));
      public static readonly Brush ChoreographyTaskBand = new SolidColorBrush(Color.FromArgb(255,250, 250, 250));
      // BPMN says, this should be white, but looks better this way
      public static readonly Brush ChoreographyInitializingParticipant = SystemBrushes.LightGray;
      // BPMN says, this should be a light fill, but looks better this way
      public static readonly Brush ChoreographyReceivingParticipant = SystemBrushes.Gray;
      public static readonly Brush DataObject = SystemBrushes.White;
      public static readonly Brush DataStore = SystemBrushes.White;
      public static readonly Brush Event = new SolidColorBrush(Color.FromArgb(255,250, 250, 250));
      public static readonly Brush EventTypeCatching = SystemBrushes.Transparent;
      public static readonly Brush EventTypeThrowing = SystemBrushes.Black;
      public static readonly Brush Gateway = new SolidColorBrush(Color.FromArgb(255,250, 250, 250));
      public static readonly Brush GroupNode = null;

      public static readonly Brush Message = SystemBrushes.White;
      // BPMN says, this should be white, but looks better this way
      public static readonly Brush InitiatingMessage = SystemBrushes.LightGray;
      // BPMN says, this should be a light fill, but looks better this way
      public static readonly Brush ReceivingMessage = SystemBrushes.Gray;
      public static readonly Brush MessageInverted = SystemBrushes.Black;
      
      public static readonly Brush PoolNodeBackground = new SolidColorBrush(Color.FromArgb(255,0xE0, 0xE0, 0xE0));
      public static readonly Brush PoolNodeEvenLeafBackground = new SolidColorBrush(Color.FromArgb(255, 196, 215, 237));
      public static readonly Brush PoolNodeEvenLeafInset = new SolidColorBrush(Color.FromArgb(255,0xE0, 0xE0, 0xE0));
      public static readonly Brush PoolNodeOddLeafBackground = new SolidColorBrush(Color.FromArgb(255, 171, 200, 226));
      public static readonly Brush PoolNodeOddLeafInset = new SolidColorBrush(Color.FromArgb(255,0xE0, 0xE0, 0xE0));
      public static readonly Brush PoolNodeParentBackground = new SolidColorBrush(Color.FromArgb(255, 113, 146, 178));
      public static readonly Brush PoolNodeParentInset = new SolidColorBrush(Color.FromArgb(255,0xE0, 0xE0, 0xE0));
    }

    internal static class Placements
    {
      private static readonly InteriorLabelModel ilm2 = new InteriorLabelModel() { Insets = new InsetsD(2) };
      private static readonly InteriorLabelModel ilm6 = new InteriorLabelModel() { Insets = new InsetsD(6) };
      private static readonly InteriorStretchLabelModel islmInsideDoubleLine = new InteriorStretchLabelModel() { Insets = new InsetsD(2 * DoubleLineOffset + 1) };
      private static readonly ExteriorLabelModel elm15 = new ExteriorLabelModel { Insets = new InsetsD(15) };
      private static readonly ScalingLabelModel slm = new ScalingLabelModel();
      private static readonly ScalingLabelModel slm3 = new ScalingLabelModel { Insets = new InsetsD(3) };


      public static readonly ILabelModelParameter TaskType = ilm6.CreateParameter(InteriorLabelModel.Position.NorthWest);
      public static readonly ILabelModelParameter TaskMarker = islmInsideDoubleLine.CreateParameter(InteriorStretchLabelModel.Position.South);

      public static readonly ILabelModelParameter ChoreographyMarker = ilm2.CreateParameter(InteriorLabelModel.Position.South);
      public static readonly ILabelModelParameter ChoreographyTopMessage = elm15.CreateParameter(ExteriorLabelModel.Position.North);
      public static readonly ILabelModelParameter ChoreographyBottomMessage = elm15.CreateParameter(ExteriorLabelModel.Position.South);

      private static readonly double ratioWidthHeight = 1 / Math.Sin(Math.PI / 3.0);
      public static readonly ILabelModelParameter Conversation = slm.CreateScaledParameterWithRatio(1, ratioWidthHeight);
      public static readonly ILabelModelParameter ConversationMarker = ilm2.CreateParameter(InteriorLabelModel.Position.South);

      public static readonly ILabelModelParameter DataObjectType = ilm2.CreateParameter(InteriorLabelModel.Position.NorthWest);
      public static readonly ILabelModelParameter DataObjectMarker = ilm2.CreateParameter(InteriorLabelModel.Position.South);

      public static readonly ILabelModelParameter Event = slm.CreateScaledParameterWithRatio(1, 1);
      public static readonly ILabelModelParameter EventType = slm3.CreateScaledParameterWithRatio(0.9, 1);

      public static readonly ILabelModelParameter Gateway = slm.CreateScaledParameterWithRatio(1, 1);
      public static readonly ILabelModelParameter GatewayType = slm.CreateScaledParameterWithRatio(0.6, 1);

      public static readonly ILabelModelParameter EventTypeMessage = slm.CreateScaledParameterWithRatio(0.8, 1.4);
      public static readonly ILabelModelParameter ActivityTaskTypeMessage = slm.CreateScaledParameterWithRatio(1, 1.4);

      public static readonly ILabelModelParameter DoubleLine = new InteriorStretchLabelModel() { Insets = new InsetsD(DoubleLineOffset) }.CreateParameter(InteriorStretchLabelModel.Position.Center);
      public static readonly ILabelModelParameter ThickLine = new InteriorStretchLabelModel() { Insets = new InsetsD(DoubleLineOffset / 2) }.CreateParameter(InteriorStretchLabelModel.Position.Center);

      public static readonly ILabelModelParameter InsideDoubleLine = islmInsideDoubleLine.CreateParameter(InteriorStretchLabelModel.Position.Center);

      public static readonly ILabelModelParameter PoolNodeMarker = ilm2.CreateParameter(InteriorLabelModel.Position.South);
    }

    internal static class Sizes
    {
      public static readonly SizeD Marker = new SizeD(10, 10);
      public static readonly SizeD TaskType = new SizeD(15, 15);
      public static readonly SizeD Message = new SizeD(20, 14);

      public static readonly double ConversationWidthHeightRatio = Math.Sin(Math.PI / 3.0);
      public static readonly SizeD Conversation = new SizeD(20, 20 * ConversationWidthHeightRatio);

      public static readonly SizeD DataObjectType = new SizeD(10, 8);

      public static readonly SizeD EventPort = new SizeD(20, 20);

    }

  }

}
