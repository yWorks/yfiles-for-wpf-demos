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

namespace Demo.yFiles.Graph.Bpmn.BpmnDi {
  public static class BpmnDiConstants
  {
    public const string ProcessElement = "process";
    public const string BpmnDiagramElement = "BPMNDiagram";
    public const string BpmnPlaneElement = "BPMNPlane";
    public const string BpmnEdgeElement = "BPMNEdge";
    public const string BpmnLabelElement = "BPMNLabel";
    public const string BpmnShapeElement = "BPMNShape";
    public const string BpmnLabelStyleElement = "BPMNLabelStyle";
    public const string BoundsElement = "Bounds";
    public const string WaypointElement = "waypoint";
    public const string GatewaySuffix = "Gateway";
    public const string ExclusiveGatewayElement = "exclusiveGateway";
    public const string InclusiveGatewayElement = "inclusiveGateway";
    public const string ParallelGatewayElement = "parallelGateway";
    public const string EventBasedGatewayElement = "eventBasedGateway";
    public const string ComplexGatewayElement = "complexGateway";
    public const string UserTaskElement = "userTask";
    public const string ManualTaskElement = "manualTask";
    public const string SendTaskElement = "sendTask";
    public const string ReceiveTaskElement = "receiveTask";
    public const string ScriptTaskElement = "scriptTask";
    public const string ServiceTaskElement = "serviceTask";
    public const string BusinessRuleTaskElement = "businessRuleTask";
    public const string TaskElement = "task";
    public const string SubProcessElement = "subProcess";
    public const string AdHocSubProcessElement = "adHocSubProcess";
    public const string TransactionElement = "transaction";
    public const string ConversationElement = "conversation";
    public const string ConversationLinkElement = "conversationLink";
    public const string SubConversationElement = "subConversation";
    public const string GlobalConversationElement = "globalConversation";
    public const string CallConversationElement = "callConversation";
    public const string CollaborationElement = "collaboration";
    public const string CallActivityElement = "callActivity";
    public const string StartEventElement = "startEvent";
    public const string MessageEventDefinitionElement = "messageEventDefinition";
    public const string TimerEventDefinitionElement = "timerEventDefinition";
    public const string TerminateEventDefinitionElement = "terminateEventDefinition";
    public const string ErrorEventDefinitionElement = "errorEventDefinition";
    public const string ConditionalEventDefinitionElement = "conditionalEventDefinition";
    public const string CompensateEventDefinitionElement = "compensateEventDefinition";
    public const string CancelEventDefinitionElement = "cancelEventDefinition";
    public const string SignalEventDefinitionElement = "signalEventDefinition";
    public const string MultipleEventDefinitionElement = "multipleEventDefinition";
    public const string ParallelEventDefinitionElement = "parallelEventDefinition";
    public const string EscalationEventDefinitionElement = "escalationEventDefinition";
    public const string LinkEventDefinitionElement = "linkEventDefinition";
    public const string EndEventElement = "endEvent";
    public const string IntermediateThrowEventElement = "intermediateThrowEvent";
    public const string IntermediateCatchEventElement = "intermediateCatchEvent";
    public const string BoundaryEventElement = "boundaryEvent";
    public const string LaneElement = "lane";
    public const string ChoreographyElement = "choreography";
    public const string SubChoreographyElement = "subChoreography";
    public const string CallChoreographyElement = "callChoreography";
    public const string ChoreographyTaskElement = "choreographyTask";
    public const string ParticipantElement = "participant";
    public const string ParticipantMultiplicityElement = "participantMultiplicity";
    public const string TextAnnotationElement = "textAnnotation";
    public const string TextElement = "text";
    public const string FontElement = "Font";
    public const string StandardLoopCharacteristicsElement = "standardLoopCharacteristics";
    public const string MultiInstanceLoopCharacteristicsElement = "multiInstanceLoopCharacteristics";
    public const string PropertyElement = "property";
    public const string IoSpecificationElement = "ioSpecification";
    public const string SourceRefElement = "sourceRef";
    public const string TargetRefElement = "targetRef";
    public const string GroupElement = "group";
    public const string DataInputElement = "dataInput";
    public const string DataOutputElement = "dataOutput";
    public const string SequenceFlowElement = "sequenceFlow";
    public const string MessageFlowElement = "messageFlow";
    public const string MessageFlowRefElement = "messageFlowRef";
    public const string DataObjectReferenceElement = "dataObjectReference";
    public const string DataStoreReferenceElement = "dataStoreReference";
    public const string ConditionExpressionElement = "conditionExpression";
    public const string AssociationElement = "association";
    public const string DataAssociationElement = "dataAssociation";
    public const string DataInputAssociationElement = "dataInputAssociation";
    public const string DataOutputAssociationElement = "dataOutputAssociation";

    public const string IdAttribute = "id";
    public const string NameAttribute = "name";
    public const string DocumentationAttribute = "documentation";
    public const string ResolutionAttribute = "resolution";
    public const string XAttribute = "x";
    public const string YAttribute = "y";
    public const string WidthAttribute = "width";
    public const string HeightAttribute = "height";
    public const string BpmnElementAttribute = "bpmnElement";
    public const string SourceRefAttribute = "sourceRef";
    public const string TargetRefAttribute = "targetRef";
    public const string ProcessRefAttribute = "processRef";
    public const string CategoryValueRefAttribute = "categoryValueRef";
    public const string CalledChoreographyRefAttribute = "calledChoreographyRef";
    public const string CalledElementAttribute = "calledElement";
    public const string MessageVisibleKindAttribute = "messageVisibleKind";
    public const string IsSequentialAttribute = "isSequential";
    public const string SizeAttribute = "size";
    public const string IsBoldAttribute = "isBold";
    public const string IsItalicAttribute = "isItalic";
    public const string IsUnderlineAttribute = "isUnderline";
    public const string IsStrikeThroughAttribute = "isStrikeThrough";
    public const string IsHorizontalAttribute = "isHorizontal";
    public const string IsExpandedAttribute = "isExpanded";
    public const string IsMarkerVisibleAttribute = "isMarkerVisible";
    public const string IsMessageVisibleAttribute = "isMessageVisible";
    public const string ChoreographyActivityShapeAttribute = "choreographyActivityShape";
    public const string ParticipantBandKindAttribute = "participantBandKind";
    public const string LabelStyleAttribute = "labelStyle";
    public const string TriggeredByEventAttribute = "triggeredByEvent";
    public const string IsInterruptingAttribute = "isInterrupting";
    public const string CalledCollaborationRefAttribute = "calledCollaborationRef";
    public const string IsCollectionAttribute = "isCollection";
    public const string DataObjectRefAttribute = "dataObjectRef";
    public const string AssociationDirectionAttribute = "associationDirection";
    public const string IsForCompensationAttribute = "associationDirection";
    public const string AttachedToRefAttribute = "attachedToRef";
    public const string CancelActivityAttribute = "cancelActivity";
  }
}
