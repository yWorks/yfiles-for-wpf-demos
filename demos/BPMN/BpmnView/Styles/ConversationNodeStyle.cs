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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// An <see cref="INodeStyle"/> implementation representing a Conversation according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class ConversationNodeStyle : BpmnNodeStyle {
    private ConversationType type;

    /// <summary>
    /// Gets or sets the conversation type for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(ConversationType.Conversation)]
    public ConversationType Type {
      get { return type; }
      set {
        if (type != value || Icon == null) {
          ModCount++;
          type = value;
          UpdateIcon();
        }
      }
    }

    private Brush background = BpmnConstants.ConversationDefaultBackground;

    /// <summary>
    /// Gets or sets the background color of the conversation.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "ConversationDefaultBackground")]
    public Brush Background {
      get { return background; }
      set {
        if (background != value) {
          ModCount++;
          background = value;
          UpdateIcon();
        }
      }
    }

    private Brush outline = BpmnConstants.ConversationDefaultOutline;

    /// <summary>
    /// Gets or sets the outline color of the conversation.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "ConversationDefaultOutline")]
    public Brush Outline {
      get { return outline; }
      set {
        if (outline != value) {
          ModCount++;
          outline = value;
          UpdateIcon();
        }
      }
    }

    private Brush iconColor = BpmnConstants.DefaultIconColor;

    /// <summary>
    /// Gets or sets the primary color for icons and markers.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DefaultIconColor")]
    public Brush IconColor {
      get { return iconColor; }
      set {
        if (iconColor != value) {
          ModCount++;
          iconColor = value;
          UpdateIcon();
        }
      }
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public ConversationNodeStyle() {
      Type = ConversationType.Conversation;
      MinimumSize = BpmnConstants.ConversationSize;
    }

    private void UpdateIcon() {
      var typeIcon = IconFactory.CreateConversation(type, Background, Outline);
      var markerIcon = IconFactory.CreateConversationMarker(type, IconColor);

      if (markerIcon != null) {
        markerIcon = IconFactory.CreatePlacedIcon(markerIcon, BpmnConstants.ConversationMarkerPlacement,
            BpmnConstants.MarkerSize);
        typeIcon = IconFactory.CreateCombinedIcon(new List<IIcon>(new[] { typeIcon, markerIcon }));
      }

      Icon = IconFactory.CreatePlacedIcon(typeIcon, BpmnConstants.ConversationPlacement,
          BpmnConstants.ConversationSize);
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override GeneralPath GetOutline(INode node) {
      double width = Math.Min(node.Layout.Width, node.Layout.Height / BpmnConstants.ConversationWidthHeightRatio);
      double height = width * BpmnConstants.ConversationWidthHeightRatio;
      RectD bounds = new RectD(node.Layout.GetCenter().X - width/2, node.Layout.GetCenter().Y - height/2, width, height);
      
      var path = new GeneralPath();
      path.MoveTo(0, 0.5);
      path.LineTo(0.25, 0);
      path.LineTo(0.75, 0);
      path.LineTo(1, 0.5);
      path.LineTo(0.75, 1);
      path.LineTo(0.25, 1);
      path.Close();

      var transform = new Matrix2D();
      transform.Translate(bounds.GetTopLeft());
      transform.Scale(bounds.Width, bounds.Height);
      path.Transform(transform);
      return path;
    }
  }
}

