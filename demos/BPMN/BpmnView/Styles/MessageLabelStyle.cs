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

using System.ComponentModel;
using System.Reflection;
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
       public class MessageLabelStyle : ILabelStyle {
     
         #region Initialize static fields
     
         private static readonly ILabelStyleRenderer initiatingRenderer;
         private static readonly ILabelStyleRenderer responseRenderer;
     
         static MessageLabelStyle() {
           
           // Initiate the renderer for the initiating Message
           var messageIcon = IconFactory.CreateMessage(BpmnConstants.Pens.Message, 
                 BpmnConstants.Brushes.InitiatingMessage);
           var bpmnNodeStyle = new BpmnNodeStyle { Icon = messageIcon, MinimumSize = BpmnConstants.Sizes.Message };
           var labelStyle = new DefaultLabelStyle();
           var adapter = new NodeStyleLabelStyleAdapter(bpmnNodeStyle, labelStyle);
           initiatingRenderer = new MessageLabelStyleRenderer(adapter);
           
           // Initiate the renderer for the response Message
           messageIcon = IconFactory.CreateMessage(BpmnConstants.Pens.Message, 
                 BpmnConstants.Brushes.ReceivingMessage);
           bpmnNodeStyle = new BpmnNodeStyle { Icon = messageIcon, MinimumSize = BpmnConstants.Sizes.Message };
           labelStyle = new DefaultLabelStyle();
           adapter = new NodeStyleLabelStyleAdapter(bpmnNodeStyle, labelStyle);
           responseRenderer = new MessageLabelStyleRenderer(adapter);
         }
     
         #endregion
     
         #region Properties
    
         /// <summary>
         /// Gets or sets if this Message is initiating
         /// </summary>
         [DefaultValue(false)]
         public bool IsInitiating { get; set; }

         #endregion
         
         public static ILabelStyle InitiatingStyle() {
           return new MessageLabelStyle() {IsInitiating = true};
         }
         
         public static ILabelStyle ResponseStyle() {
           return new MessageLabelStyle() {IsInitiating = false};
         }
     
         /// <inheritdoc/>
         [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
         public object Clone() {
           return this;
         }
     
         /// <inheritdoc/>
         [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
         public ILabelStyleRenderer Renderer {
           get { return IsInitiating ? initiatingRenderer : responseRenderer; }
         }
     
         /// <summary>
         /// An <see cref="ILabelStyleRenderer"/> implementation used by <see cref="MessageLabelStyle"/>.
         /// </summary>
         internal class MessageLabelStyleRenderer : ILabelStyleRenderer
         {
     
           private ILabelStyle adapter;
     
           public MessageLabelStyleRenderer(ILabelStyle adapter) {
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
