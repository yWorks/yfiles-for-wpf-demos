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

using System.Reflection;
using Demo.yFiles.Graph.Bpmn.Styles;
using yWorks.Graph;

namespace Demo.yFiles.Graph.Bpmn
{

  /// <summary>
  /// Specifies the type of an activity according to BPMN.
  /// </summary>
  /// <seealso cref="ActivityNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum ActivityType
  {
    /// <summary>
    /// Specifies the type of an activity to be a Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Task,

    /// <summary>
    /// Specifies the type of an activity to be a Sub-Process according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    SubProcess,

    /// <summary>
    /// Specifies the type of an activity to be a Transaction Sub-Process according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Transaction,

    /// <summary>
    /// Specifies the type of an activity to be an Event Sub-Process according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    EventSubProcess,

    /// <summary>
    /// Specifies the type of an activity to be a Call Activity according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    CallActivity
  }

  /// <summary>
  /// Specifies the type of a task according to BPMN.
  /// </summary>
  /// <seealso cref="ActivityNodeStyle"/>  
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum TaskType {

    /// <summary>
    /// Specifies the type of a task to be an Abstract Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Abstract,

    /// <summary>
    /// Specifies the type of a task to be a Send Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Send,

    /// <summary>
    /// Specifies the type of a task to be a Receive Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Receive,

    /// <summary>
    /// Specifies the type of a task to be a User Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    User,

    /// <summary>
    /// Specifies the type of a task to be a Manual Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Manual,

    /// <summary>
    /// Specifies the type of a task to be a Business Rule Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    BusinessRule,

    /// <summary>
    /// Specifies the type of a task to be a Service Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Service,

    /// <summary>
    /// Specifies the type of a task to be a Script Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Script,

    /// <summary>
    /// Specifies the type of a task to be an Event-Triggered Sub-Task according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    EventTriggered
  }

  /// <summary>
  /// Specifies the Loop Characteristic of an Activity or Choreography according to BPMN.
  /// </summary>
  /// <seealso cref="ActivityNodeStyle"/>
  /// <seealso cref="ChoreographyNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum LoopCharacteristic
  {
    /// <summary>
    /// Specifies that an Activity or Choreography in not looping according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    /// <seealso cref="ChoreographyNodeStyle"/>
    None,

    /// <summary>
    /// Specifies that an Activity or Choreography has a Standard Loop Characteristic according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    /// <seealso cref="ChoreographyNodeStyle"/>
    Loop,

    /// <summary>
    /// Specifies that an Activity or Choreography has a parallel Multi-Instance Loop Characteristic according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    /// <seealso cref="ChoreographyNodeStyle"/>
    Parallel,

    /// <summary>
    /// Specifies that an Activity or Choreography has a sequential Multi-Instance Loop Characteristic according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    /// <seealso cref="ChoreographyNodeStyle"/>
    Sequential,
  }

  /// <summary>
  /// Specifies if an Activity is an expanded or collapsed Sub-Process according to BPMN.
  /// </summary>
  /// <seealso cref="ActivityNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum SubState
  {
    /// <summary>
    /// Specifies that an Activity is either no Sub-Process according to BPMN or should use no Sub-Process marker.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    None,

    /// <summary>
    /// Specifies that an Activity is an expanded Sub-Process according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Expanded,

    /// <summary>
    /// Specifies that an Activity is a collapsed Sub-Process according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    Collapsed,

    /// <summary>
    /// Specifies that the folding state of an <see cref="INode"/> determines if  
    /// an Activity is an expanded or collapsed Sub-Process according to BPMN.
    /// </summary>
    /// <seealso cref="ActivityNodeStyle"/>
    /// <seealso cref="IFoldingView.IsExpanded"/>
    Dynamic
  }

  /// <summary>
  /// Specifies the type of a Gateway according to BPMN.
  /// </summary>
  /// <seealso cref="GatewayNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum GatewayType
  {
    /// <summary>
    /// Specifies that a Gateway has the type Exclusive according to BPMN but should not use a marker.
    /// </summary>
    /// <seealso cref="GatewayNodeStyle"/>
    ExclusiveWithoutMarker,

    /// <summary>
    /// Specifies that a Gateway has the type Exclusive according to BPMN and should use a marker.
    /// </summary>
    /// <seealso cref="GatewayNodeStyle"/>
    ExclusiveWithMarker,

    /// <summary>
    /// Specifies that a Gateway has the type Inclusive according to BPMN.
    /// </summary>
    /// <seealso cref="GatewayNodeStyle"/>
    Inclusive,

    /// <summary>
    /// Specifies that a Gateway has the type Parallel according to BPMN.
    /// </summary>
    /// <seealso cref="GatewayNodeStyle"/>
    Parallel,

    /// <summary>
    /// Specifies that a Gateway has the type Complex according to BPMN.
    /// </summary>
    /// <seealso cref="GatewayNodeStyle"/>
    Complex,

    /// <summary>
    /// Specifies that a Gateway has the type Event-Based according to BPMN.
    /// </summary>
    /// <seealso cref="GatewayNodeStyle"/>
    EventBased,

    /// <summary>
    /// Specifies that a Gateway has the type Exclusive Event-Based according to BPMN.
    /// </summary>
    /// <seealso cref="GatewayNodeStyle"/>
    ExclusiveEventBased,

    /// <summary>
    /// Specifies that a Gateway has the type Parallel Event-Based according to BPMN.
    /// </summary>
    /// <seealso cref="GatewayNodeStyle"/>
    ParallelEventBased
  }

  /// <summary>
  /// Specifies the type of an Event according to BPMN.
  /// </summary>
  /// <seealso cref="EventNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum EventType
  {
    /// <summary>
    /// Specifies that an Event is a Plain Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Plain,

    /// <summary>
    /// Specifies that an Event is a Message Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Message,

    /// <summary>
    /// Specifies that an Event is a Timer Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Timer,

    /// <summary>
    /// Specifies that an Event is an Escalation Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Escalation,

    /// <summary>
    /// Specifies that an Event is a Conditional Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Conditional,

    /// <summary>
    /// Specifies that an Event is a Link Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Link,

    /// <summary>
    /// Specifies that an Event is an Error Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Error,

    /// <summary>
    /// Specifies that an Event is a Cancel Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Cancel,

    /// <summary>
    /// Specifies that an Event is a Compensation Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Compensation,

    /// <summary>
    /// Specifies that an Event is a Signal Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Signal,

    /// <summary>
    /// Specifies that an Event is a Multiple Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Multiple,

    /// <summary>
    /// Specifies that an Event is a Parallel Multiple Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    ParallelMultiple,

    /// <summary>
    /// Specifies that an Event is a Terminate Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Terminate
  }

  /// <summary>
  /// Specifies the characteristic of an event.
  /// </summary>
  /// <seealso cref="EventNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum EventCharacteristic
  {
    /// <summary>
    /// Specifies that an Event is a Start Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Start,

    /// <summary>
    /// Specifies that an Event is a Start Event for a Sub-Process according to BPMN that interrupts the containing Process.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    SubProcessInterrupting,

    /// <summary>
    /// Specifies that an Event is a Start Event for a Sub-Process according to BPMN that doesn`t interrupt the containing Process.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    SubProcessNonInterrupting,

    /// <summary>
    /// Specifies that an Event is an Intermediate Catching Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Catching,

    /// <summary>
    /// Specifies that an Event is an Intermediate Event Attached to an Activity Boundary according to BPMN that interrupts the Activity.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    BoundaryInterrupting,

    /// <summary>
    /// Specifies that an Event is an Intermediate Event Attached to an Activity Boundary according to BPMN that doesn't interrupt the Activity.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    BoundaryNonInterrupting,

    /// <summary>
    /// Specifies that an Event is an Intermediate Throwing Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    Throwing,

    /// <summary>
    /// Specifies that an Event is an End Event according to BPMN.
    /// </summary>
    /// <seealso cref="EventNodeStyle"/>
    End
  }

  /// <summary>
  /// Specifies the type of a Choreography according to BPMN.
  /// </summary>
  /// <seealso cref="ChoreographyNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum ChoreographyType
  {
    /// <summary>
    /// Specifies that a Choreography is a Choreography Task according to BPMN.
    /// </summary>
    /// <seealso cref="ChoreographyNodeStyle"/>
    Task,

    /// <summary>
    /// Specifies that a Choreography is a Call Choreography according to BPMN.
    /// </summary>
    /// <seealso cref="ChoreographyNodeStyle"/>
    Call
  }

  /// <summary>
  /// Specifies the type of a Conversation according to BPMN.
  /// </summary>
  /// <seealso cref="ConversationNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum ConversationType
  {
    /// <summary>
    /// Specifies that a Conversation is a plain Conversation according to BPMN.
    /// </summary>
    /// <seealso cref="ConversationNodeStyle"/>
    Conversation,

    /// <summary>
    /// Specifies that a Conversation is a Sub-Conversation according to BPMN.
    /// </summary>
    /// <seealso cref="ConversationNodeStyle"/>
    SubConversation,

    /// <summary>
    /// Specifies that a Conversation is a Call Conversation according to BPMN where a Global Conversation is called.
    /// </summary>
    /// <seealso cref="ConversationNodeStyle"/>
    CallingGlobalConversation,

    /// <summary>
    /// Specifies that a Conversation is a Call Conversation according to BPMN where a Collaboration is called.
    /// </summary>
    /// <seealso cref="ConversationNodeStyle"/>
    CallingCollaboration
  }

  /// <summary>
  /// Specifies the type of a Data Object according to BPMN.
  /// </summary>
  /// <seealso cref="DataObjectNodeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum DataObjectType
  {

    /// <summary>
    /// Specifies a normal Data Object according to BPMN.
    /// </summary>
    /// <seealso cref="DataObjectNodeStyle"/>
    None,

    /// <summary>
    /// Specifies a Data Input according to BPMN.
    /// </summary>
    /// <seealso cref="DataObjectNodeStyle"/>
    Input,

    /// <summary>
    /// Specifies a Data Output according to BPMN.
    /// </summary>
    /// <seealso cref="DataObjectNodeStyle"/>
    Output
  }


  /// <summary>
  /// Specifies the type of an edge according to BPMN.
  /// </summary>
  /// <seealso cref="BpmnEdgeStyle"/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum EdgeType
  {
    /// <summary>
    /// Specifies an edge to be a Sequence Flow according to BPMN.
    /// </summary>
    /// <seealso cref="BpmnEdgeStyle"/>
    SequenceFlow,

    /// <summary>
    /// Specifies an edge to be a Default Flow according to BPMN.
    /// </summary>
    /// <seealso cref="BpmnEdgeStyle"/>
    DefaultFlow,

    /// <summary>
    /// Specifies an edge to be a Conditional Flow according to BPMN.
    /// </summary>
    /// <seealso cref="BpmnEdgeStyle"/>
    ConditionalFlow,

    /// <summary>
    /// Specifies an edge to be a Message Flow according to BPMN.
    /// </summary>
    /// <seealso cref="BpmnEdgeStyle"/>
    MessageFlow,

    /// <summary>
    /// Specifies an edge to be an undirected Association according to BPMN.
    /// </summary>
    /// <seealso cref="BpmnEdgeStyle"/>
    Association,

    /// <summary>
    /// Specifies an edge to be a directed Association according to BPMN.
    /// </summary>
    /// <seealso cref="BpmnEdgeStyle"/>
    DirectedAssociation,

    /// <summary>
    /// Specifies an edge to be a bidirected Association according to BPMN.
    /// </summary>
    /// <seealso cref="BpmnEdgeStyle"/>
    BidirectedAssociation,

    /// <summary>
    /// Specifies an edge to be a Conversation according to BPMN.
    /// </summary>
    /// <seealso cref="BpmnEdgeStyle"/>
    Conversation
  }

  internal enum ArrowType
  {
    SequenceTarget,

    DefaultSource,

    DefaultTarget,

    ConditionalSource,

    ConditionalTarget,

    MessageSource,

    MessageTarget,

    Association
  }

}