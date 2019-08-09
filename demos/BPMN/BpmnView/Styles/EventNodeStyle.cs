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
using System.Collections.Generic;
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
  /// An <see cref="INodeStyle"/> implementation representing an Event according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class EventNodeStyle  : BpmnNodeStyle {

    #region Properties

    private EventType type;

    /// <summary>
    /// Gets or sets the event type for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EventType.Plain)]
    public EventType Type {
      get { return type; }
      set {
        if (type != value) {
          ModCount++;
          type = value;
          CreateTypeIcon();
        }
      }
    }

    private EventCharacteristic characteristic;

    /// <summary>
    /// Gets or sets the event characteristic for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EventCharacteristic.Start)]
    public EventCharacteristic Characteristic {
      get { return characteristic; }
      set {
        if (characteristic != value || eventIcon == null) {
          ModCount++;
          characteristic = value;
          CreateEventIcon();
        }
      }
    }

    #endregion

    private IIcon eventIcon;
    private IIcon typeIcon;
    private bool fillTypeIcon = false;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public EventNodeStyle() {
      MinimumSize = new SizeD(20, 20);
      Characteristic = EventCharacteristic.Start;
      Type = EventType.Plain;
    }

    private void CreateTypeIcon() {
      typeIcon = IconFactory.CreateEventType(type, fillTypeIcon);
      if (typeIcon != null) {
        typeIcon = IconFactory.CreatePlacedIcon(typeIcon, BpmnConstants.Placements.EventType, SizeD.Empty);
      }
    }

    private void CreateEventIcon() {
      eventIcon = IconFactory.CreateEvent(Characteristic);
      eventIcon = IconFactory.CreatePlacedIcon(eventIcon, BpmnConstants.Placements.Event, MinimumSize);
      bool isFilled = Characteristic == EventCharacteristic.Throwing || Characteristic == EventCharacteristic.End;
      if (isFilled != fillTypeIcon) {
        fillTypeIcon = isFilled;
        CreateTypeIcon();
      }
    }

    /// <inheritdoc/>
    internal override void UpdateIcon(INode node) {
      if (typeIcon != null) {
        Icon = IconFactory.CreateCombinedIcon(new List<IIcon>(new[] {eventIcon, typeIcon}));
      } else {
        Icon = eventIcon;
      }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override GeneralPath GetOutline(INode node) {
      double size = Math.Min(node.Layout.Width, node.Layout.Height);
      RectD bounds = new RectD(node.Layout.GetCenter().X - size / 2, node.Layout.GetCenter().Y - size / 2, size, size);

      var path = new GeneralPath();
      path.AppendEllipse(new RectD(bounds.GetTopLeft(), bounds.GetSize()), false);
      return path;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      double size = Math.Min(node.Layout.Width, node.Layout.Height);
      RectD bounds = new RectD(node.Layout.GetCenter().X - size / 2, node.Layout.GetCenter().Y - size / 2, size, size);
      return GeomUtilities.EllipseContains(bounds, location, context.HitTestRadius);
    }
  }
}
