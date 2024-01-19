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
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Styles;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using GroupNodeStyle = Demo.yFiles.Graph.Bpmn.Styles.GroupNodeStyle;

// ReSharper disable UnusedMember.Global

namespace Demo.yFiles.Graph.Bpmn.Legacy
{
  public class ActivityNodeStyleExtension : MarkupExtension
  {
    public ActivityType ActivityType { get; set; }

    public TaskType TaskType { get; set; }

    public EventType TriggerEventType { get; set; }

    public EventCharacteristic TriggerEventCharacteristic { get; set; }

    public LoopCharacteristic LoopCharacteristic { get; set; }

    public SubState SubState { get; set; }

    public bool AdHoc { get; set; }

    public bool Compensation { get; set; }

    public InsetsD Insets { get; set; }

    public SizeD MinimumSize { get; set; }

    public ActivityNodeStyleExtension() {
      ActivityType = ActivityType.Task;
      TaskType = TaskType.Abstract;
      TriggerEventType = EventType.Message;
      TriggerEventCharacteristic = EventCharacteristic.SubProcessInterrupting;
      LoopCharacteristic = LoopCharacteristic.None;
      SubState = SubState.None;
      AdHoc = false;
      Compensation = false;
      Insets = new InsetsD(15);
      MinimumSize = SizeD.Empty;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new ActivityNodeStyle {
          ActivityType = (Bpmn.ActivityType) ActivityType,
          TaskType = (Bpmn.TaskType) TaskType,
          TriggerEventType = (Bpmn.EventType) TriggerEventType,
          TriggerEventCharacteristic = (Bpmn.EventCharacteristic) TriggerEventCharacteristic,
          LoopCharacteristic = (Bpmn.LoopCharacteristic) LoopCharacteristic,
          SubState = (Bpmn.SubState) SubState,
          AdHoc = AdHoc,
          Compensation = Compensation,
          Insets = Insets,
          MinimumSize = MinimumSize
      };
    }
  }

  public class AlternatingLeafStripeStyleExtension : MarkupExtension {
    public StripeDescriptor EvenLeafDescriptor { get; set; }

    public StripeDescriptor ParentDescriptor { get; set; }

    public StripeDescriptor OddLeafDescriptor { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new AlternatingLeafStripeStyle {
          EvenLeafDescriptor = EvenLeafDescriptor,
          ParentDescriptor = ParentDescriptor,
          OddLeafDescriptor = OddLeafDescriptor
      };
    }
  }

  public class AnnotationLabelStyleExtension : MarkupExtension
  {
    public double Insets { get; set; }

    public AnnotationLabelStyleExtension() {
      Insets = 5;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new AnnotationLabelStyle {
          Insets = Insets,
      };
    }
  }

  public class AnnotationNodeStyleExtension : MarkupExtension
  {
    public bool Left { get; set; }

    public SizeD MinimumSize { get; set; }

    public AnnotationNodeStyleExtension() {
      Left = true;
      MinimumSize = SizeD.Empty;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new AnnotationNodeStyle {
          Left = Left,
          MinimumSize = MinimumSize
      };
    }
  }

  public class BpmnEdgeStyleExtension : MarkupExtension
  {
    public EdgeType Type { get; set; }

    public double SmoothingLength { get; set; }
    
    public BpmnEdgeStyleExtension() {
      Type = EdgeType.SequenceFlow;
      SmoothingLength = 20;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new BpmnEdgeStyle {
          Type = (Bpmn.EdgeType) Type,
          SmoothingLength = SmoothingLength
      };
    }
  }

  public class ChoreographyLabelModelExtension : MarkupExtension
  {
    public static readonly ChoreographyLabelModel Instance = ChoreographyLabelModel.Instance;
    public static readonly ILabelModelParameter TaskNameBand = ChoreographyLabelModel.TaskNameBand;
    public static readonly ILabelModelParameter NorthMessage = ChoreographyLabelModel.NorthMessage;
    public static readonly ILabelModelParameter SouthMessage = ChoreographyLabelModel.SouthMessage;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return ChoreographyLabelModel.Instance;
    }
  }

  public class ParticipantLabelModelParameterExtension : MarkupExtension {
    public int Index { get; set; }

    public bool Top { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return ChoreographyLabelModel.Instance.CreateParticipantParameter(Top, Index);
    }
  }

  public class ChoreographyMessageLabelStyleExtension : MarkupExtension
  {
    public ILabelModelParameter TextPlacement { get; set; }

    public ChoreographyMessageLabelStyleExtension() {
      TextPlacement = ChoreographyMessageLabelStyle.defaultTextPlacement;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new ChoreographyMessageLabelStyle {
          TextPlacement = TextPlacement
      };
    }
  }

  public class ChoreographyNodeStyleExtension : MarkupExtension
  {
    public ChoreographyType Type { get; set; }

    public LoopCharacteristic LoopCharacteristic { get; set; }

    public SubState SubState { get; set; }

    public bool InitiatingMessage { get; set; }

    public bool ResponseMessage { get; set; }

    public bool InitiatingAtTop { get; set; }

    public SizeD MinimumSize { get; set; }

    private readonly ChoreographyNodeStyle.ParticipantList topParticipants = new ChoreographyNodeStyle.ParticipantList();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public IList<Participant> TopParticipants {
      get { return topParticipants; }
    }

    private readonly ChoreographyNodeStyle.ParticipantList bottomParticipants = new ChoreographyNodeStyle.ParticipantList();

    /// <summary>
    /// Gets the list of <see cref="Participant"/>s at the bottom of the node, ordered from bottom to top.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public IList<Participant> BottomParticipants {
      get { return bottomParticipants; }
    }

    public InsetsD Insets { get; set; }

    public ChoreographyNodeStyleExtension() {
      Type = ChoreographyType.Task;
      LoopCharacteristic = LoopCharacteristic.None;
      SubState = SubState.None;
      InitiatingMessage = false;
      ResponseMessage = false;
      InitiatingAtTop = true;
      Insets = new InsetsD(5);
      MinimumSize = SizeD.Empty;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var style = new ChoreographyNodeStyle {
          Type = (Bpmn.ChoreographyType) Type,
          LoopCharacteristic = (Bpmn.LoopCharacteristic) LoopCharacteristic,
          SubState = (Bpmn.SubState) SubState,
          InitiatingMessage = InitiatingMessage,
          ResponseMessage = ResponseMessage,
          InitiatingAtTop = InitiatingAtTop,
          Insets = Insets,
          MinimumSize = MinimumSize,
          InitiatingColor = Brushes.LightGray
      };
      foreach (var p in TopParticipants) {
        style.TopParticipants.Add(p);
      }
      foreach (var p in BottomParticipants) {
        style.BottomParticipants.Add(p);
      }
      return style;
    }
  }

  public class ParticipantExtension : MarkupExtension {

    public bool MultiInstance { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new Participant {
          MultiInstance = MultiInstance
      };
    }
  }

  public class ConversationNodeStyleExtension : MarkupExtension {
    public ConversationType Type { get; set; }

    public SizeD MinimumSize { get; set; }

    public ConversationNodeStyleExtension() {
      Type = ConversationType.Conversation;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new ConversationNodeStyle {
          Type = (Bpmn.ConversationType) Type,
          MinimumSize = MinimumSize
      };
    }
  }

  public class DataObjectNodeStyleExtension : MarkupExtension
  {
    public bool Collection { get; set; }

    public DataObjectType Type { get; set; }

    public SizeD MinimumSize { get; set; }

    public DataObjectNodeStyleExtension() {
      Collection = false;
      Type = DataObjectType.None;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new DataObjectNodeStyle {
          Collection = Collection,
          Type = (Bpmn.DataObjectType) Type,
          MinimumSize = MinimumSize
      };
    }
  }

  public class DataStoreNodeStyleExtension : MarkupExtension
  {
    public SizeD MinimumSize { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new DataStoreNodeStyle {
          MinimumSize = MinimumSize
      };
    }
  }

  public class EventNodeStyleExtension : MarkupExtension
  {
    public EventType Type { get; set; }

    public EventCharacteristic Characteristic { get; set; }

    public SizeD MinimumSize { get; set; }

    public EventNodeStyleExtension() {
      Type = EventType.Plain;
      Characteristic = EventCharacteristic.Start;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new EventNodeStyle {
          Type = (Bpmn.EventType) Type,
          Characteristic = (Bpmn.EventCharacteristic) Characteristic,
          MinimumSize = MinimumSize
      };
    }
  }

  public class EventPortStyleExtension : MarkupExtension
  {
    public EventType Type { get; set; }

    public EventCharacteristic Characteristic { get; set; }

    public SizeD RenderSize { get; set; }

    public EventPortStyleExtension() {
      Type = EventType.Compensation;
      Characteristic = EventCharacteristic.BoundaryInterrupting;
      RenderSize = new SizeD(20, 20);
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new EventPortStyle {
          Type = (Bpmn.EventType) Type,
          Characteristic = (Bpmn.EventCharacteristic) Characteristic,
          RenderSize = RenderSize,
      };
    }
  }

  public class GatewayNodeStyleExtension : MarkupExtension
  {
    public GatewayType Type { get; set; }

    public SizeD MinimumSize { get; set; }

    public GatewayNodeStyleExtension() {
      Type = GatewayType.ExclusiveWithoutMarker;
      MinimumSize = SizeD.Empty;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new GatewayNodeStyle {
          Type = (Bpmn.GatewayType) Type,
          MinimumSize = MinimumSize
      };
    }
  }

  public class GroupNodeStyleExtension : MarkupExtension
  {
    public InsetsD Insets { get; set; }

    public GroupNodeStyleExtension() {
      Insets = new InsetsD(15);
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new GroupNodeStyle {
          Insets = Insets
      };
    }
  }

  public class MessageLabelStyleExtension : MarkupExtension
  {
    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new MessageLabelStyle();
    }
  }

  public class PoolHeaderLabelModelExtension : MarkupExtension
  {
    public static readonly PoolHeaderLabelModel Instance = PoolHeaderLabelModel.Instance;
    public static readonly ILabelModelParameter North = PoolHeaderLabelModel.North;
    public static readonly ILabelModelParameter East = PoolHeaderLabelModel.East;
    public static readonly ILabelModelParameter South = PoolHeaderLabelModel.South;
    public static readonly ILabelModelParameter West = PoolHeaderLabelModel.West;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return PoolHeaderLabelModel.Instance;
    }
  }

  [ContentProperty("TableNodeStyle")]
  public class PoolNodeStyleExtension : MarkupExtension {
    public bool MultipleInstance { get; set; }

    private bool Vertical { get; set; }

    public TableNodeStyle TableNodeStyle { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new PoolNodeStyle(Vertical) {
          MultipleInstance = MultipleInstance,
          TableNodeStyle = TableNodeStyle
      };
    }
  }

  public class StripeDescriptorExtension : MarkupExtension {
    public Brush BackgroundBrush { get; set; }

    public Brush InsetBrush { get; set; }

    public Brush BorderBrush { get; set; }

    public Thickness BorderThickness { get; set; }

    public StripeDescriptorExtension() {
      BackgroundBrush = Brushes.Transparent;
      InsetBrush = Brushes.Transparent;
      BorderBrush = Brushes.Black;
      BorderThickness = new Thickness(1);
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new StripeDescriptor {
          BackgroundBrush = BackgroundBrush,
          BorderBrush = BorderBrush,
          BorderThickness = BorderThickness,
          InsetBrush = InsetBrush
      };
    }
  }

  public enum ActivityType
  {
    Task,
    SubProcess,
    Transaction,
    EventSubProcess,
    CallActivity
  }

  public enum TaskType {
    Abstract,
    Send,
    Receive,
    User,
    Manual,
    BusinessRule,
    Service,
    Script,
    EventTriggered
  }

  public enum LoopCharacteristic
  {
    None,
    Loop,
    Parallel,
    Sequential,
  }

  public enum SubState
  {
    None,
    Expanded,
    Collapsed,
    Dynamic
  }

  public enum GatewayType
  {
    ExclusiveWithoutMarker,
    ExclusiveWithMarker,
    Inclusive,
    Parallel,
    Complex,
    EventBased,
    ExclusiveEventBased,
    ParallelEventBased
  }

  public enum EventType
  {
    Plain,
    Message,
    Timer,
    Escalation,
    Conditional,
    Link,
    Error,
    Cancel,
    Compensation,
    Signal,
    Multiple,
    ParallelMultiple,
    Terminate
  }

  public enum EventCharacteristic
  {
    Start,
    SubProcessInterrupting,
    SubProcessNonInterrupting,
    Catching,
    BoundaryInterrupting,
    BoundaryNonInterrupting,
    Throwing,
    End
  }

  public enum ChoreographyType
  {
    Task,
    Call
  }

  public enum ConversationType
  {
    Conversation,
    SubConversation,
    CallingGlobalConversation,
    CallingCollaboration
  }

  public enum DataObjectType
  {
    None,
    Input,
    Output
  }


  public enum EdgeType
  {
    SequenceFlow,
    DefaultFlow,
    ConditionalFlow,
    MessageFlow,
    Association,
    DirectedAssociation,
    BidirectedAssociation,
    Conversation
  }
}
