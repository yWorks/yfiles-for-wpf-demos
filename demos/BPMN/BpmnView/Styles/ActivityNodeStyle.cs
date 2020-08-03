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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// An <see cref="INodeStyle"/> implementation representing an Activity according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class ActivityNodeStyle : BpmnNodeStyle {
    private static readonly ShapeNodeStyle sns = new ShapeNodeStyle(new ShapeNodeStyleRenderer { RoundRectArcRadius = BpmnConstants.ActivityCornerRadius })
    {
        Shape = ShapeNodeShape.RoundRectangle,
        Pen = Pens.Black,
        Brush = null
    };
    private IIcon adHocIcon;
    private IIcon compensationIcon;

    private ActivityType activityType;

    /// <summary>
    /// Gets or sets the activity type for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(ActivityType.Task)]
    public ActivityType ActivityType {
      get { return activityType; }
      set {
        if (activityType != value || activityIcon == null) {
          ModCount++;
          activityType = value;
          UpdateActivityIcon();
        }
      }
    }

    private TaskType taskType;

    /// <summary>
    /// Gets or sets the task type for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(TaskType.Abstract)]
    public TaskType TaskType {
      get { return taskType; }
      set {
        if (taskType != value) {
          ModCount++;
          taskType = value;
          UpdateTaskIcon();
        }
      }
    }

    private EventType triggerEventType;

    /// <summary>
    /// Gets or sets the event type that is used for the task type <see cref="Bpmn.TaskType.EventTriggered"/>.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EventType.Message)]
    public EventType TriggerEventType {
      get { return triggerEventType; }
      set {
        if (triggerEventType != value) {
          triggerEventType = value;
          if (TaskType == TaskType.EventTriggered) {
            ModCount++;
            UpdateTaskIcon();
          }
        }
      }
    }

    private EventCharacteristic triggerEventCharacteristic;

    /// <summary>
    /// Gets or sets the event characteristic that is used for the task type <see cref="Bpmn.TaskType.EventTriggered"/>.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EventCharacteristic.SubProcessInterrupting)]
    public EventCharacteristic TriggerEventCharacteristic {
      get { return triggerEventCharacteristic; }
      set {
        if (triggerEventCharacteristic != value) {
          triggerEventCharacteristic = value;
          if (TaskType == TaskType.EventTriggered) {
            ModCount++;
            UpdateTaskIcon();
          }
        }
      }
    }

    private LoopCharacteristic loopCharacteristic;

    /// <summary>
    /// Gets or sets the loop characteristic of this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(LoopCharacteristic.None)]
    public LoopCharacteristic LoopCharacteristic {
      get { return loopCharacteristic; }
      set {
        if (loopCharacteristic != value) {
          ModCount++;
          loopCharacteristic = value;
          UpdateLoopIcon();
        }
      }
    }

    private SubState subState;

    /// <summary>
    /// Gets or sets the sub state of this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(SubState.None)]
    public SubState SubState {
      get { return subState; }
      set {
        if (subState != value) {
          ModCount++;
          subState = value;
        }
      }
    }

    private bool adHoc;

    /// <summary>
    /// Gets or sets whether this style represents an Ad Hoc Activity. 
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(false)]
    public bool AdHoc {
      get { return adHoc; }
      set {
        if (adHoc != value) {
          ModCount++;
          adHoc = value;
          UpdateAdHocIcon();
        }
      }
    }

    private bool compensation;

    /// <summary>
    /// Gets or sets whether this style represents a Compensation Activity. 
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(false)]
    public bool Compensation {
      get { return compensation; }
      set {
        if (compensation != value) {
          ModCount++;
          compensation = value;
          UpdateCompensationIcon();
        }
      }
    }

    /// <summary>
    /// Gets or sets the insets for the node.
    /// </summary>
    /// <remarks>
    /// These insets are extended at the left and bottom side if markers are active
    /// and returned via an <see cref="INodeInsetsProvider"/> if such an instance is queried through the
    /// <see cref="INodeStyleRenderer.GetContext">context lookup</see>.
    /// </remarks>
    /// <seealso cref="INodeInsetsProvider"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(InsetsD), "15")]
    public InsetsD Insets {
      get { return insets; }
      set { insets = value; }
    }

    private Brush background = BpmnConstants.ActivityDefaultBackground;

    /// <summary>
    /// Gets or sets the background color of the activity.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "ActivityDefaultBackground")]
    public Brush Background {
      get { return background; }
      set {
        if (background != value || activityIcon == null) {
          ModCount++;
          background = value;
          UpdateActivityIcon();
          UpdateTaskIcon();
        }
      }
    }

    private Brush outline = BpmnConstants.ActivityDefaultOutline;

    /// <summary>
    /// Gets or sets the outline color of the activity.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "ActivityDefaultOutline")]
    public Brush Outline {
      get { return outline; }
      set {
        if (outline != value || activityIcon == null) {
          ModCount++;
          outline = value;
          UpdateActivityIcon();
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
          UpdateTaskIcon();
          UpdateLoopIcon();
          UpdateAdHocIcon();
          UpdateCompensationIcon();
        }
      }
    }

    // null is the default value which chooses a default color for the outline depending on the characteristic
    private Brush eventOutline = BpmnConstants.DefaultEventOutline;

    /// <summary>
    /// Gets or sets the outline color for event icons if <see cref="TaskType"/> is <see cref="Bpmn.TaskType.EventTriggered"/>.
    /// </summary>
    /// <remarks>
    /// If this is set to <see langword="null"/>, the outline color is automatic, based on the <see cref="TriggerEventCharacteristic"/>.
    /// </remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(null)]
    public Brush EventOutline {
      get { return eventOutline; }
      set {
        if (eventOutline != value) {
          ModCount++;
          eventOutline = value;
          UpdateTaskIcon();
        }
      }
    }

    private IIcon activityIcon;
    private IIcon taskIcon;
    private IIcon loopIcon;
    private InsetsD insets;

    /// <summary>
    /// Creates a new instance using the default values.
    /// </summary>
    public ActivityNodeStyle() {
      MinimumSize = new SizeD(40, 30);
      ActivityType = ActivityType.Task;
      Insets = new InsetsD(15);
      SubState = SubState.None;
      LoopCharacteristic = LoopCharacteristic.None;
	    TaskType = TaskType.Abstract;
	    TriggerEventType = EventType.Message;
	    TriggerEventCharacteristic = EventCharacteristic.SubProcessInterrupting;
    }

    private void UpdateActivityIcon() {
      activityIcon = IconFactory.CreateActivity(activityType, Background, Outline);
    }

    private void UpdateTaskIcon() {
      if (TaskType == TaskType.EventTriggered) {
        var eventNodeStyle = new EventNodeStyle {
          Characteristic = TriggerEventCharacteristic,
          Type = TriggerEventType,
          Background = Background,
          Outline = EventOutline,
          IconColor = IconColor
        };
        eventNodeStyle.UpdateIcon(null);
        taskIcon = eventNodeStyle.Icon;
      } else {
        taskIcon = IconFactory.CreateActivityTaskType(taskType, IconColor, Background);
      }
      if (taskIcon != null) {
        taskIcon = IconFactory.CreatePlacedIcon(taskIcon, BpmnConstants.TaskTypePlacement, BpmnConstants.TaskTypeSize);
      }
    }

    private void UpdateAdHocIcon() {
      adHocIcon = AdHoc ? IconFactory.CreateAdHoc(IconColor) : null;
    }
    
    private void UpdateCompensationIcon() {
      compensationIcon = Compensation ? IconFactory.CreateCompensation(false, IconColor) : null;
    }

    private void UpdateLoopIcon() {
      loopIcon = IconFactory.CreateLoopCharacteristic(LoopCharacteristic, IconColor);
    }

    internal override void UpdateIcon(INode node) {
      Icon = CreateIcon(node);
    }

    internal IIcon CreateIcon(INode node) {
      var minimumWidth = 10.0;

      var icons = new List<IIcon>();
      if (activityIcon != null) {
        icons.Add(activityIcon);
      }
      if (taskIcon != null) {
        icons.Add(taskIcon);
      }

      var lineUpIcons = new List<IIcon>();
      if (loopIcon != null) {
        minimumWidth += BpmnConstants.MarkerSize.Width + 5;
        lineUpIcons.Add(loopIcon);
      }
      if (AdHoc) {
        minimumWidth += BpmnConstants.MarkerSize.Width + 5;
        lineUpIcons.Add(adHocIcon);
      }
      if (Compensation) {
        minimumWidth += BpmnConstants.MarkerSize.Width + 5;
        lineUpIcons.Add(compensationIcon);
      }
      if (SubState != SubState.None) {
        minimumWidth += BpmnConstants.MarkerSize.Width + 5;
        if (SubState == SubState.Dynamic) {
          lineUpIcons.Add(IconFactory.CreateDynamicSubState(node, IconColor));
        } else {
          lineUpIcons.Add(IconFactory.CreateStaticSubState(SubState, IconColor));
        }
      }
      if (lineUpIcons.Count > 0) {
        IIcon lineUpIcon = IconFactory.CreateLineUpIcon(lineUpIcons, BpmnConstants.MarkerSize, 5);
        icons.Add(IconFactory.CreatePlacedIcon(lineUpIcon, BpmnConstants.TaskMarkerPlacement, BpmnConstants.MarkerSize));
      }

      MinimumSize = new SizeD(Math.Max(minimumWidth, 40), 40);
      if (icons.Count > 1) {
        return IconFactory.CreateCombinedIcon(icons);
      } else if (icons.Count == 1) {
        return icons[0];
      } else {
        return null;
      }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override GeneralPath GetOutline(INode node) {
      // Create a rounded rectangle path
      var layout = node.Layout.ToRectD();
      var path = new GeneralPath(12);
      var x = layout.X;
      var y = layout.Y;
      var w = layout.Width;
      var h = layout.Height;
      var arcX = Math.Min(w * 0.5, 5);
      var arcY = Math.Min(h * 0.5, 5);
      path.MoveTo(x, y + arcY);
      path.QuadTo(x, y, x + arcX, y);
      path.LineTo(x + w - arcX, y);
      path.QuadTo(x + w, y, x + w, y + arcY);
      path.LineTo(x + w, y + h - arcY);
      path.QuadTo(x + w, y + h, x + w - arcX, y + h);
      path.LineTo(x + arcX, y + h);
      path.QuadTo(x, y + h, x, y + h - arcY);
      path.Close();
      return path;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      return sns.Renderer.GetHitTestable(node, sns).IsHit(context, location);
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override object Lookup(INode node, Type type) {
      if (type == typeof(INodeInsetsProvider)) {
        return new ActivityInsetsProvider(this);
      }
      return base.Lookup(node, type);
    }

    /// <summary>
    /// Uses the style insets extended by the size of the participant bands.
    /// </summary>
    private sealed class ActivityInsetsProvider : INodeInsetsProvider {
      private readonly ActivityNodeStyle style;

      internal ActivityInsetsProvider(ActivityNodeStyle style) {
        this.style = style;
      }

      public InsetsD GetInsets(INode node) {
        double left = style.TaskType != TaskType.Abstract 
          ? BpmnConstants.TaskTypeSize.Width + ((InteriorLabelModel)BpmnConstants.TaskTypePlacement.Model).Insets.Left 
          : 0;
        double bottom = style.AdHoc || style.Compensation || style.LoopCharacteristic != LoopCharacteristic.None || style.SubState != SubState.None 
          ? BpmnConstants.MarkerSize.Height + ((InteriorStretchLabelModel)BpmnConstants.TaskMarkerPlacement.Model).Insets.Bottom 
          : 0;
        return new InsetsD(left + style.Insets.Left, style.Insets.Top, style.Insets.Right, bottom + style.Insets.Bottom);
      }
    }
  }
}
