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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// An <see cref="INodeStyle"/> implementation representing a Gateway according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class GatewayNodeStyle : BpmnNodeStyle {
    private IIcon gatewayIcon;

    private GatewayType type;

    /// <summary>
    /// Gets or sets the gateway type for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(GatewayType.ExclusiveWithoutMarker)]
    public GatewayType Type {
      get { return type; }
      set {
        if (type != value) {
          ModCount++;
          type = value;
          UpdateTypeIcon();
        }
      }
    }

    private Brush background = BpmnConstants.GatewayDefaultBackground;

    /// <summary>
    /// Gets or sets the background color of the gateway.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "GatewayDefaultBackground")]
    public Brush Background {
      get { return background; }
      set {
        if (background != value) {
          ModCount++;
          background = value;
          UpdateGatewayIcon();
        }
      }
    }

    private Brush outline = BpmnConstants.GatewayDefaultOutline;

    /// <summary>
    /// Gets or sets the outline color of the gateway.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "GatewayDefaultOutline")]
    public Brush Outline {
      get { return outline; }
      set {
        if (outline != value) {
          ModCount++;
          outline = value;
          UpdateGatewayIcon();
        }
      }
    }

    private Brush iconColor = BpmnConstants.DefaultIconColor;

    /// <summary>
    /// Gets or sets the color for the icon.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DefaultIconColor")]
    public Brush IconColor {
      get { return iconColor; }
      set {
        if (iconColor != value) {
          ModCount++;
          iconColor = value;
          UpdateTypeIcon();
        }
      }
    }

    private IIcon typeIcon;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public GatewayNodeStyle() {
      MinimumSize = new SizeD(20, 20);
      Type = GatewayType.ExclusiveWithoutMarker;
    }

    private void UpdateGatewayIcon() {
      gatewayIcon = IconFactory.CreatePlacedIcon(IconFactory.CreateGateway(Background, Outline), BpmnConstants.GatewayPlacement, SizeD.Empty);
    }

    private void UpdateTypeIcon() {
      typeIcon = IconFactory.CreateGatewayType(type, IconColor);
      if (typeIcon != null) {
        typeIcon = IconFactory.CreatePlacedIcon(typeIcon, BpmnConstants.GatewayTypePlacement, SizeD.Empty);
      }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    internal override void UpdateIcon(INode node) {
      if (gatewayIcon == null) {
        UpdateGatewayIcon();
      }
      Icon = typeIcon != null ? IconFactory.CreateCombinedIcon(new[] { gatewayIcon, typeIcon }) : gatewayIcon;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override GeneralPath GetOutline(INode node) {
      double size = Math.Min(node.Layout.Width, node.Layout.Height);
      RectD bounds = new RectD(node.Layout.X + node.Layout.Width/2 - size / 2, node.Layout.Y + node.Layout.Height/2 - size / 2, size, size);

      var path = new GeneralPath();
      path.MoveTo(bounds.X, bounds.CenterY); // <
      path.LineTo(bounds.CenterX, bounds.Y); // ^
      path.LineTo(bounds.MaxX, bounds.CenterY); // >
      path.LineTo(bounds.CenterX, bounds.MaxY); // v
      path.Close();
      return path;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      if (!node.Layout.ToRectD().GetEnlarged(context.HitTestRadius).Contains(location)) {
        return false;
      }
      double size = Math.Min(node.Layout.Width, node.Layout.Height);

      var distVector = node.Layout.GetCenter() - location;
      var dist = Math.Abs(distVector.X) + Math.Abs(distVector.Y);
      return dist < size/2 + context.HitTestRadius;
    }
  }
}

