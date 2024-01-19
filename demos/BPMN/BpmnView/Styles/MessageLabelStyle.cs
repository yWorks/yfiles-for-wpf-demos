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

using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
     using yWorks.Controls;
     using yWorks.Controls.Input;
     using yWorks.Geometry;
     using yWorks.Graph;
     using yWorks.Graph.Styles;
     
     namespace Demo.yFiles.Graph.Bpmn.Styles {
     
       /// <summary>
       /// An <see cref="ILabelStyle"/> implementation representing a Message according to the BPMN.
       /// </summary>
       [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
       public class MessageLabelStyle : ILabelStyle
       {
         private readonly MessageLabelStyleRenderer renderer = new MessageLabelStyleRenderer(
             new NodeStyleLabelStyleAdapter(
                 new BpmnNodeStyle {
                     Icon = IconFactory.CreateMessage((Pen) new Pen(BpmnConstants.DefaultMessageOutline, 1).GetAsFrozen(), BpmnConstants.DefaultReceivingMessageColor),
                     MinimumSize = BpmnConstants.MessageSize
                 },
                 new DefaultLabelStyle()));

         /// <summary>
         /// Gets or sets if this Message is initiating
         /// </summary>
         [DefaultValue(true)]
         public bool IsInitiating {
           get { return isInitiating; }
           set {
             if (isInitiating != value) {
               isInitiating = value;
               UpdateIcon();
             }
           }
         }

         private Brush outline;
         internal Pen messagePen;

         /// <summary>
         /// Gets or sets the outline color of the message.
         /// </summary>
         [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
         [DefaultValue(typeof(BpmnConstants), "DefaultMessageOutline")]
         public Brush Outline {
           get { return outline; }
           set {
             if (outline != value) {
               outline = value;
               messagePen = (Pen) new Pen(outline, 1).GetAsFrozen();
               UpdateIcon();
             }
           }
         }

         private Brush initiatingColor = BpmnConstants.DefaultInitiatingMessageColor;

         /// <summary>
         /// Gets or sets the color for an initiating message.
         /// </summary>
         [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
         [DefaultValue(typeof(BpmnConstants), "DefaultInitiatingMessageColor")]
         public Brush InitiatingColor {
           get { return initiatingColor; }
           set {
             if (initiatingColor != value) {
               initiatingColor = value;
               if (IsInitiating) {
                 UpdateIcon();
               }
             }
           }
         }

         private Brush responseColor = BpmnConstants.DefaultReceivingMessageColor;
         private bool isInitiating = true;

         /// <summary>
         /// Gets or sets the color for a response message.
         /// </summary>
         [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
         [DefaultValue(typeof(BpmnConstants), "DefaultReceivingMessageColor")]
         public Brush ResponseColor {
           get { return responseColor; }
           set {
             if (responseColor != value) {
               responseColor = value;
               if (!IsInitiating) {
                 UpdateIcon();
               }
             }
           }
         }

         public MessageLabelStyle() {
           Outline = BpmnConstants.DefaultMessageOutline;
         }

         private void UpdateIcon() {
           var adapter = renderer.adapter;
           var nodeStyle = (BpmnNodeStyle) adapter.NodeStyle;
           nodeStyle.Icon = IconFactory.CreateMessage(messagePen, IsInitiating ? InitiatingColor : ResponseColor);
           nodeStyle.ModCount++;
         }
    
         /// <inheritdoc/>
         [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
         public object Clone() {
           return new MessageLabelStyle {
               IsInitiating = IsInitiating,
               InitiatingColor = InitiatingColor,
               ResponseColor = ResponseColor,
               Outline = Outline
           };
         }
     
         /// <inheritdoc/>
         [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
         public ILabelStyleRenderer Renderer {
           get { return renderer; }
         }
     
         /// <summary>
         /// An <see cref="ILabelStyleRenderer"/> implementation used by <see cref="MessageLabelStyle"/>.
         /// </summary>
         internal class MessageLabelStyleRenderer : ILabelStyleRenderer
         {
           internal NodeStyleLabelStyleAdapter adapter;
     
           public MessageLabelStyleRenderer(NodeStyleLabelStyleAdapter adapter) {
             this.adapter = adapter;
           }
           
           /// <inheritdoc/>
           public IVisualCreator GetVisualCreator(ILabel item, ILabelStyle style) {
             return adapter.Renderer.GetVisualCreator(item, adapter);
           }
     
           /// <inheritdoc/>
           public IBoundsProvider GetBoundsProvider(ILabel item, ILabelStyle style) {
             return adapter.Renderer.GetBoundsProvider(item, adapter);
           }
     
           /// <inheritdoc/>
           public IVisibilityTestable GetVisibilityTestable(ILabel item, ILabelStyle style) {
             return adapter.Renderer.GetVisibilityTestable(item, adapter);
           }
     
           /// <inheritdoc/>
           public IHitTestable GetHitTestable(ILabel item, ILabelStyle style) {
             return adapter.Renderer.GetHitTestable(item, adapter);
           }
     
           /// <inheritdoc/>
           public IMarqueeTestable GetMarqueeTestable(ILabel item, ILabelStyle style) {
             return adapter.Renderer.GetMarqueeTestable(item, adapter);
           }
     
           /// <inheritdoc/>
           public ILookup GetContext(ILabel item, ILabelStyle style) {
             return adapter.Renderer.GetContext(item, adapter);
           }
     
           /// <inheritdoc/>
           public SizeD GetPreferredSize(ILabel label, ILabelStyle style) {
             return adapter.Renderer.GetPreferredSize(label, adapter);
           }
         }
       }

}
