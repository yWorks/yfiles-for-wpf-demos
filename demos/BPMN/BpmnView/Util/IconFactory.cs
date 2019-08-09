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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.Bpmn.Util {

  /// <summary>
  /// Factory class providing icons according to the BPMN.
  /// </summary>
  internal class IconFactory
  {
    private static readonly IconBuilder builder = new IconBuilder();
    private static readonly double RadiusToCornerOffset = Math.Sqrt((1.5 - Math.Sqrt(2)));

    public static IIcon CreatePlacedIcon(IIcon icon, ILabelModelParameter placement, SizeD innerSize) {
      return new PlacedIcon(icon, placement, innerSize);
    }

    public static IIcon CreateCombinedIcon(IList<IIcon> icons) {
      return builder.CombineIcons(icons);
    }

    public static IIcon CreateLineUpIcon(IList<IIcon> icons, SizeD innerIconSize, double gap) {
      return builder.CreateLineUpIcon(icons, innerIconSize, gap);
    }

    private static readonly Dictionary<ActivityType, IIcon> activityIcons = new Dictionary<ActivityType, IIcon>();

    public static IIcon CreateActivity(ActivityType type) {
      IIcon result;
      if (!activityIcons.TryGetValue(type, out result))
      {
        builder.Brush = BpmnConstants.Brushes.Activity;

        switch (type) {
          case ActivityType.CallActivity:
            builder.Pen = BpmnConstants.Pens.CallActivity;
            result = builder.CreateRectIcon(BpmnConstants.ActivityCornerRadius);
            break;
          case ActivityType.Transaction:
            builder.Pen = BpmnConstants.Pens.Task;
            List<IIcon> icons = new List<IIcon>(2);
            icons.Add(builder.CreateRectIcon(BpmnConstants.ActivityCornerRadius));

            builder.Pen = BpmnConstants.Pens.Task;
            builder.Brush = BpmnConstants.Brushes.Activity;
            var rectIcon = builder.CreateRectIcon(BpmnConstants.ActivityCornerRadius - BpmnConstants.DoubleLineOffset);
            icons.Add(CreatePlacedIcon(rectIcon, BpmnConstants.Placements.DoubleLine, SizeD.Empty));
            result = builder.CombineIcons(icons);
            break;
          case ActivityType.EventSubProcess:
            builder.Pen = BpmnConstants.Pens.ActivityEventSubProcess;
            result = builder.CreateRectIcon(BpmnConstants.ActivityCornerRadius);
            break;
          default: // Task, SubProcess
            builder.Pen = BpmnConstants.Pens.Task;
            result = builder.CreateRectIcon(BpmnConstants.ActivityCornerRadius);
            break;
        }
        activityIcons.Add(type, result);
      }
      return result;
    }

    private static readonly Dictionary<TaskType, IIcon> taskIcons = new Dictionary<TaskType, IIcon>();

    public static IIcon CreateActivityTaskType(TaskType type) {
      IIcon result;
      if (!taskIcons.TryGetValue(type, out result)) {
        List<IIcon> icons = null;
        switch (type) {
          case TaskType.Send:
            result = CreatePlacedIcon(CreateMessage(BpmnConstants.Pens.MessageInverted, BpmnConstants.Brushes.MessageInverted),
                BpmnConstants.Placements.ActivityTaskTypeMessage, SizeD.Empty);
            break;
          case TaskType.Receive:
            result = CreatePlacedIcon(CreateMessage(BpmnConstants.Pens.Message, BpmnConstants.Brushes.Message),
              BpmnConstants.Placements.ActivityTaskTypeMessage, SizeD.Empty);
            break;
          case TaskType.User:
            builder.Pen = BpmnConstants.Pens.ActivityTaskRound;
            builder.Brush = BpmnConstants.Brushes.ActivityTaskLight;

            // body + head
            icons = new List<IIcon>(3);
            builder.MoveTo(1, 1);
            builder.LineTo(0, 1);
            builder.LineTo(0, 0.701);
            builder.QuadTo(0.13, 0.5, 0.316, 0.443);
            builder.LineTo(0.5 + 0.224*Math.Cos(3.0/4.0*Math.PI), 0.224 + 0.224*Math.Sin(3.0/4.0*Math.PI));
            builder.ArcTo(0.224, 0.5, 0.224, 3.0/4.0*Math.PI, 9.0/4.0*Math.PI);
            builder.LineTo(0.684f, 0.443);
            builder.QuadTo(0.87, 0.5, 1, 0.701);
            builder.Close();
            icons.Add(builder.GetPathIcon());

            // hair
            builder.Pen = BpmnConstants.Pens.ActivityTaskRound;
            builder.Brush = Brushes.Black;
            builder.MoveTo(0.287, 0.229);
            builder.CubicTo(0.48, 0.053, 0.52, 0.253, 0.713, 0.137);
            builder.ArcTo(0.224, 0.5, 0.224, 31.0/16.0*Math.PI, Math.PI);
            builder.Close();
            icons.Add(builder.GetPathIcon());

            builder.Pen = BpmnConstants.Pens.ActivityTaskRound;

            // arms
            builder.MoveTo(0.19, 1);
            builder.LineTo(0.19, 0.816);
            builder.MoveTo(0.810, 1);
            builder.LineTo(0.810, 0.816);

            // collar
            builder.MoveTo(0.316, 0.443);
            builder.CubicTo(0.3, 0.672, 0.7, 0.672, 0.684, 0.443);
            icons.Add(builder.GetPathIcon());

            result = builder.CombineIcons(icons);
            break;
          case TaskType.Manual:
            builder.Pen = BpmnConstants.Pens.ActivityTaskRound;
            builder.MoveTo(0, 0.286);
            builder.QuadTo(0.037, 0.175, 0.147, 0.143);

            // thumb
            builder.LineTo(0.584, 0.143);
            builder.QuadTo(0.602, 0.225, 0.451, 0.286);
            builder.LineTo(0.265, 0.286);

            // index finger
            builder.LineTo(0.95, 0.286);
            builder.QuadTo(1, 0.358, 0.95, 0.429);
            builder.LineTo(0.472, 0.429);

            // middle finger
            builder.LineTo(0.915, 0.429);
            builder.QuadTo(0.965, 0.5, 0.915, 0.571);
            builder.LineTo(0.531, 0.571);

            // ring finger
            builder.LineTo(0.879, 0.571);
            builder.QuadTo(0.929, 0.642, 0.879, 0.714);
            builder.LineTo(0.502, 0.714);

            // pinkie 
            builder.LineTo(0.796, 0.714);
            builder.QuadTo(0.847, 0.786, 0.796, 0.857);
            builder.LineTo(0.088, 0.857);

            builder.QuadTo(0.022, 0.833, 0, 0.759);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case TaskType.BusinessRule:
            const float headHeight = 0.192f;
            const float rowHeight = 0.304f;
            const float column1Width = 0.264f;

            icons = new List<IIcon>(3);
            builder.Brush = BpmnConstants.Brushes.ActivityTaskDark;

            // outline
            builder.MoveTo(0, 0.1);
            builder.LineTo(1, 0.1);
            builder.LineTo(1, 0.9);
            builder.LineTo(0, 0.9);
            builder.Close();
            icons.Add(builder.GetPathIcon());

            // rows outline
            builder.Brush = BpmnConstants.Brushes.ActivityTaskLight;
            builder.MoveTo(0, 0.1 + headHeight);
            builder.LineTo(1, 0.1 + headHeight);
            builder.LineTo(1, 0.9);
            builder.LineTo(0, 0.9);
            builder.Close();
            icons.Add(builder.GetPathIcon());

            // line between second and third row
            builder.MoveTo(0, 0.1 + headHeight + rowHeight);
            builder.LineTo(1f, 0.1 + headHeight + rowHeight);

            // line between first and second column
            builder.MoveTo(column1Width, 0.1 + headHeight);
            builder.LineTo(column1Width, 0.9);
            icons.Add(builder.GetPathIcon());

            result = builder.CombineIcons(icons);
            break;
          case TaskType.Service:
            icons = new List<IIcon>();
            // top gear
            icons.Add(CreateGear(0.4, 0.4, 0.4, BpmnConstants.Pens.TaskTypeService, BpmnConstants.Brushes.ActivityTaskDark));
            icons.Add(CreateGear(0.16, 0.4, 0.4, BpmnConstants.Pens.TaskTypeService, BpmnConstants.Brushes.ActivityTaskLight));

            // bottom gear
            icons.Add(CreateGear(0.4, 0.6, 0.6, BpmnConstants.Pens.TaskTypeService, BpmnConstants.Brushes.ActivityTaskDark));
            icons.Add(CreateGear(0.16, 0.6, 0.6, BpmnConstants.Pens.TaskTypeService, BpmnConstants.Brushes.ActivityTaskLight));

            result = builder.CombineIcons(icons);
            break;
          case TaskType.Script:
            builder.Pen = BpmnConstants.Pens.ActivityTaskRound;

            // outline
            const double size = 0.5;
            const double curveEndX = 0.235;
            const double curveEndY = size;
            const double curveCenterX = curveEndX + (size - curveEndX)*0.5;
            const double curveDeltaX = 0.5;
            const double curveDeltaY = size*0.5;

            builder.MoveTo(0.5 + size, 0.5 - size);
            builder.CubicTo(0.5 + curveCenterX - curveDeltaX, 0.5 - curveDeltaY, 0.5 + curveCenterX + curveDeltaX,
              0.5 + curveDeltaY, 0.5 + curveEndX, 0.5 + curveEndY);
            builder.LineTo(0.5 - size, 0.5 + size);
            builder.CubicTo(0.5 - curveCenterX + curveDeltaX, 0.5 + curveDeltaY, 0.5 - curveCenterX - curveDeltaX,
              0.5 - curveDeltaY, 0.5 - curveEndX, 0.5 - curveEndY);
            builder.Close();

            // inner lines
            const double deltaY2 = size*0.2f;
            const double deltaX1 = 0.045f;
            const double deltaX2 = 0.085f;
            const double length = 0.3f*(size + curveEndX);

            builder.MoveTo(0.5 - length - deltaX2, 0.5 - 3f*deltaY2);
            builder.LineTo(0.5 + length - deltaX2, 0.5 - 3f*deltaY2);
            builder.MoveTo(0.5 - length - deltaX1, 0.5 - 1f*deltaY2);
            builder.LineTo(0.5 + length - deltaX1, 0.5 - 1f*deltaY2);
            builder.MoveTo(0.5 - length + deltaX1, 0.5 + 1f*deltaY2);
            builder.LineTo(0.5 + length + deltaX1, 0.5 + 1f*deltaY2);
            builder.MoveTo(0.5 - length + deltaX2, 0.5 + 3f*deltaY2);
            builder.LineTo(0.5 + length + deltaX2, 0.5 + 3f*deltaY2);
            result = builder.GetPathIcon();
            break;
          case TaskType.EventTriggered:
          default:
            break;
        }
        taskIcons.Add(type, result);
      }
      return result;
    }

    private static IIcon CreateGear(double radius, double centerX, double centerY, Pen pen, Brush brush) {
      builder.Pen = pen;
      builder.Brush = brush;
      var smallR = 0.7*radius;

      var angle = -2* Math.PI/48;
      builder.MoveTo(centerX + radius * Math.Cos(angle), centerY + radius * Math.Sin(angle));
      for (int i = 0; i < 8; i++) {
        builder.ArcTo(radius, centerX, centerY, angle, angle + 4 * Math.PI/48);
        builder.LineTo(centerX + smallR * Math.Cos(angle + 5 * Math.PI / 48), centerY + smallR * Math.Sin(angle + 5 * Math.PI / 48));
        builder.ArcTo(smallR, centerX, centerY, angle + 5 * Math.PI/48, angle + 11 * Math.PI / 48);
        builder.LineTo(centerX + radius * Math.Cos(angle + 12 * Math.PI / 48), centerY + radius * Math.Sin(angle + 12 * Math.PI / 48));
        angle += Math.PI/4;
      }

      builder.Close();
      return builder.GetPathIcon();

    }

    private static readonly Dictionary<LoopCharacteristic, IIcon> loopTypes = new Dictionary<LoopCharacteristic, IIcon>(4); 

    public static IIcon CreateLoopCharacteristic(LoopCharacteristic loopCharacteristic) {
      IIcon result;
      if (!loopTypes.TryGetValue(loopCharacteristic, out result)) {
        switch (loopCharacteristic) {
          case LoopCharacteristic.Loop:
            const double fromAngle = 0.65*Math.PI;
            const double toAngle = 2.4*Math.PI;

            double x = 0.5 + 0.5*Math.Cos(fromAngle);
            double y = 0.5 + 0.5*Math.Sin(fromAngle);
            builder.MoveTo(x, y);
            builder.ArcTo(0.5, 0.5, 0.5, fromAngle, toAngle);
            builder.MoveTo(x - 0.25, y + 0.05);
            builder.LineTo(x, y);
            builder.LineTo(x, y - 0.3);

            result = builder.GetPathIcon();
            break;
          case LoopCharacteristic.Parallel:
            builder.Brush = Brushes.Black;

            for (double xOffset = 0; xOffset < 1; xOffset += 0.4) {
              builder.MoveTo(xOffset, 0);
              builder.LineTo(xOffset + 0.2, 0);
              builder.LineTo(xOffset + 0.2, 1);
              builder.LineTo(xOffset, 1);
              builder.Close();
            }
            result = builder.GetPathIcon();
            break;
          case LoopCharacteristic.Sequential:
            builder.Brush = Brushes.Black;

            for (double yOffset = 0; yOffset < 1; yOffset += 0.4) {
              builder.MoveTo(0, yOffset);
              builder.LineTo(0, yOffset + 0.2);
              builder.LineTo(1, yOffset + 0.2);
              builder.LineTo(1, yOffset);
              builder.Close();
            }
            result = builder.GetPathIcon();
            break;
          case LoopCharacteristic.None:
          default:
            break;
        }
        loopTypes.Add(loopCharacteristic, result);
      }
      return result;
    }

    private static IIcon adHoc;

    public static IIcon CreateAdHoc() {
      if (adHoc == null) {
        builder.Brush = Brushes.Black;

        const double fromAngle1 = 5.0 / 4.0 * Math.PI;
        const double toAngle1 = 7.0 / 4.0 * Math.PI;
        const double fromAngle2 = 1.0 / 4.0 * Math.PI;
        const double toAngle2 = 3.0 / 4.0 * Math.PI;

        var smallR = 0.25 / (1 - Math.Sqrt(1.5 - Math.Sqrt(2)));
        var co = smallR * RadiusToCornerOffset;
        double dy = 0.1;

        var c1x = smallR - co;
        var c1y = 0.35 + smallR;
        double x1 = c1x + smallR * Math.Cos(fromAngle1);
        double y1 = c1y + smallR * Math.Sin(fromAngle1);

        var c2x = c1x + 2 * smallR - 2 * co;
        var c2y = c1y - 2 * smallR + 2 * co;

        double x2 = c2x + smallR * Math.Cos(fromAngle2);
        double y2 = c2y + smallR * Math.Sin(fromAngle2);
        builder.MoveTo(x1, y1 + dy);
        builder.LineTo(x1, y1);
        builder.ArcTo(smallR, c1x, c1y, fromAngle1, toAngle1);
        builder.ArcTo(smallR, c2x, c2y, toAngle2, fromAngle2);
        builder.LineTo(x2, y2 + dy);
        builder.ArcTo(smallR, c2x, c2y + dy, fromAngle2, toAngle2);
        builder.ArcTo(smallR, c1x, c1y + dy, toAngle1, fromAngle1);
        builder.Close();
        adHoc = builder.GetPathIcon();
      }
      return adHoc;
    }

    private static IIcon comparison;
    private static IIcon filledComparison;

    public static IIcon CreateCompensation(bool filled) {
      if ((!filled && comparison == null) || (filled && filledComparison == null)) {
        builder.Brush = filled ? BpmnConstants.Brushes.EventTypeThrowing : BpmnConstants.Brushes.EventTypeCatching;
        var sqrt3inv = 1/Math.Sqrt(3);
        var halfSqurt3 = sqrt3inv/2;
        var xOff = 0.5/(2*sqrt3inv);
        builder.MoveTo(0, 0.5);
        builder.LineTo(xOff, 0.5 - halfSqurt3);
        builder.LineTo(xOff, 0.5);
        builder.LineTo(2*xOff, 0.5 - halfSqurt3);
        builder.LineTo(2*xOff, 0.5 + halfSqurt3);
        builder.LineTo(xOff, 0.5);
        builder.LineTo(xOff, 0.5 + halfSqurt3);
        builder.Close();
        if (filled) {
          filledComparison = builder.GetPathIcon();
        } else {
          comparison = builder.GetPathIcon();
        }
      }
      return filled ? filledComparison : comparison;
    }

    private static readonly Dictionary<SubState, IIcon> subStates = new Dictionary<SubState, IIcon>(3); 

    public static IIcon CreateStaticSubState(SubState subState) {
      IIcon result;
      if (!subStates.TryGetValue(subState, out result)) {
        switch (subState) {
          case SubState.Expanded:
            var icons = new List<IIcon>();
            icons.Add(builder.CreateRectIcon(0));
            builder.MoveTo(0.2, 0.5);
            builder.LineTo(0.8, 0.5);
            icons.Add(builder.GetPathIcon());
            result = builder.CombineIcons(icons);
            break;
          case SubState.Collapsed:
            var icons2 = new List<IIcon>();
            icons2.Add(builder.CreateRectIcon(0));
            builder.MoveTo(0.2, 0.5);
            builder.LineTo(0.8, 0.5);
            builder.MoveTo(0.5, 0.2);
            builder.LineTo(0.5, 0.8);
            icons2.Add(builder.GetPathIcon());
            result = builder.CombineIcons(icons2);
            break;
          case SubState.None:
          default:
            break;
        }
        subStates.Add(subState, result);
      }
      return result;
    }

    public static IIcon CreateDynamicSubState(INode node) {
      return new CollapseButtonIcon(node);
    }

    private static IIcon gateway;

    public static IIcon CreateGateway() {
      if (gateway == null) {
        builder.Pen = BpmnConstants.Pens.Gateway;
        builder.Brush = BpmnConstants.Brushes.Gateway;
        builder.MoveTo(0.5, 0);
        builder.LineTo(1, 0.5);
        builder.LineTo(0.5, 1);
        builder.LineTo(0, 0.5);
        builder.Close();
        gateway = builder.GetPathIcon();
      }
      return gateway;
    }

    private static readonly Dictionary<GatewayType, IIcon> gatewayTypes = new Dictionary<GatewayType, IIcon>(8); 

    public static IIcon CreateGatewayType(GatewayType type) {
      IIcon result;
      if (!gatewayTypes.TryGetValue(type, out result)) {
        List<IIcon> icons;
        switch (type) {
          case GatewayType.ExclusiveWithoutMarker:
            break;
          case GatewayType.ExclusiveWithMarker:
            builder.Brush = Brushes.Black;
            var cornerOffY = 0.5 - 0.5*Math.Sin(Math.PI/4);
            var cornerOffX = cornerOffY + 0.1;
            var xOff = 0.06;

            var x1 = cornerOffX;
            var x2 = cornerOffX + 2*xOff;

            var y1 = cornerOffY;
            var y2 = 0.5 - (0.5*xOff - cornerOffY*xOff)/(0.5 - cornerOffX - xOff);

            builder.MoveTo(x1, y1);
            builder.LineTo(x2, y1);
            builder.LineTo(0.5, y2);
            builder.LineTo(1 - x2, y1);
            builder.LineTo(1 - x1, y1);
            builder.LineTo(0.5 + xOff, 0.5);
            builder.LineTo(1 - x1, 1 - y1);
            builder.LineTo(1 - x2, 1 - y1);
            builder.LineTo(0.5, 1 - y2);
            builder.LineTo(x2, 1 - y1);
            builder.LineTo(x1, 1 - y1);
            builder.LineTo(0.5 - xOff, 0.5);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case GatewayType.Inclusive:
            builder.Pen = BpmnConstants.Pens.GatewayTypeInclusive;
            result = CreatePlacedIcon(builder.CreateEllipseIcon(), BpmnConstants.Placements.ThickLine, SizeD.Empty);
            break;
          case GatewayType.EventBased:
          case GatewayType.ExclusiveEventBased:
            icons = new List<IIcon>(3);
            icons.Add(builder.CreateEllipseIcon());

            if (type == GatewayType.EventBased) {
              var innerCircleIcon = builder.CreateEllipseIcon();
              icons.Add(CreatePlacedIcon(innerCircleIcon, BpmnConstants.Placements.DoubleLine, SizeD.Empty));
            }

            IList<PointD> polygon = CreatePolygon(5, 0.5, 0);
            builder.MoveTo(polygon[0].X, polygon[0].Y);
            for (int i = 1; i < 5; i++) {
              builder.LineTo(polygon[i].X, polygon[i].Y);
            }
            builder.Close();
            var innerIcon = builder.GetPathIcon();
            icons.Add(CreatePlacedIcon(innerIcon, BpmnConstants.Placements.InsideDoubleLine, SizeD.Empty));
            result = builder.CombineIcons(icons);
            break;
          case GatewayType.Parallel:
            result = CreatePlusIcon(0.8, Pens.Black, Brushes.Black);
            break;
          case GatewayType.ParallelEventBased:
            icons = new List<IIcon>(2);
            icons.Add(builder.CreateEllipseIcon());
            icons.Add(CreatePlusIcon(0.6, Pens.Black, null));
            result = builder.CombineIcons(icons);
            break;
          case GatewayType.Complex:
            builder.Brush = Brushes.Black;
            PointD[] outer = CreatePolygon(24, 0.5, Math.PI/24);
            double width = Math.Sqrt(0.5 - (0.5*Math.Cos(Math.PI/12)));
            double rInner = width*Math.Sqrt((1 + Math.Sqrt(2)/2));
            PointD[] inner = CreatePolygon(8, rInner, Math.PI/8);

            builder.MoveTo(outer[0].X, outer[0].Y);
            for (int i = 0; i < 8; i++) {
              builder.LineTo(outer[3*i].X, outer[3*i].Y);
              builder.LineTo(inner[i].X, inner[i].Y);
              builder.LineTo(outer[3*i + 2].X, outer[3*i + 2].Y);
            }
            builder.Close();
            result = builder.GetPathIcon();
            break;
          default:
            break;
        }
        gatewayTypes.Add(type, result);
      }
      return result;
    }

    private static readonly Dictionary<EventCharacteristic, IIcon> eventCharacteristics = new Dictionary<EventCharacteristic, IIcon>(8); 

    public static IIcon CreateEvent(EventCharacteristic characteristic) {
      IIcon result;
      if (!eventCharacteristics.TryGetValue(characteristic, out result)) {
        Pen pen = null;
        switch (characteristic) {
          case EventCharacteristic.Start:
          case EventCharacteristic.SubProcessInterrupting:
            pen = BpmnConstants.Pens.EventStart;
            break;
          case EventCharacteristic.SubProcessNonInterrupting:
            pen = BpmnConstants.Pens.EventSubProcessNonInterrupting;
            break;
          case EventCharacteristic.Catching:
          case EventCharacteristic.BoundaryInterrupting:
          case EventCharacteristic.Throwing:
            pen = BpmnConstants.Pens.EventIntermediate;
            break;
          case EventCharacteristic.BoundaryNonInterrupting:
            pen = BpmnConstants.Pens.EventBoundaryNonInterrupting;
            break;
          case EventCharacteristic.End:
            pen = BpmnConstants.Pens.EventEnd;
            break;
        }

        builder.Pen = pen;
        builder.Brush = BpmnConstants.Brushes.Event;
        var ellipseIcon = builder.CreateEllipseIcon();

        switch (characteristic) {
          case EventCharacteristic.Catching:
          case EventCharacteristic.BoundaryInterrupting:
          case EventCharacteristic.BoundaryNonInterrupting:
          case EventCharacteristic.Throwing:
            List<IIcon> icons = new List<IIcon>();
            icons.Add(ellipseIcon);

            builder.Pen = pen;
            builder.Brush = BpmnConstants.Brushes.Event;
            var innerEllipseIcon = builder.CreateEllipseIcon();
            icons.Add(CreatePlacedIcon(innerEllipseIcon, BpmnConstants.Placements.DoubleLine, SizeD.Empty));
            result = CreateCombinedIcon(icons);
            break;
          default:
            result = ellipseIcon;
            break;
        }
        eventCharacteristics.Add(characteristic, result);
      }
      return result;
    }

    private static readonly Dictionary<EventTypeWithFill, IIcon> eventTypes = new Dictionary<EventTypeWithFill, IIcon>(26); 

    public static IIcon CreateEventType(EventType type, bool filled) {
      IIcon result;
      var eventTypeWithFill = new EventTypeWithFill(type, filled);
      if (!eventTypes.TryGetValue(eventTypeWithFill, out result)) {
        builder.Pen = BpmnConstants.Pens.EventType;
        builder.Brush = filled ? BpmnConstants.Brushes.EventTypeThrowing : BpmnConstants.Brushes.EventTypeCatching;

        List<IIcon> icons;
        switch (type) {
          case EventType.Plain:
            builder.Clear();
            break;
          case EventType.Message:
            var messagePen = filled ? BpmnConstants.Pens.MessageInverted : BpmnConstants.Pens.Message;
            var messageBrush = filled ? BpmnConstants.Brushes.MessageInverted : BpmnConstants.Brushes.Message;
            var combinedIcons = CreateMessage(messagePen, messageBrush);
            result = CreatePlacedIcon(combinedIcons, BpmnConstants.Placements.EventTypeMessage, SizeD.Empty);
            break;
          case EventType.Timer:
            icons = new List<IIcon>();
            icons.Add(builder.CreateEllipseIcon());
            builder.Pen = filled ? BpmnConstants.Pens.EventTypeDetailInverted : BpmnConstants.Pens.EventTypeDetail;
            var outerPoints = CreatePolygon(12, 0.5, 0);
            var innerPoints = CreatePolygon(12, 0.4, 0);
            for (int i = 0; i < 12; i++) {
              builder.MoveTo(outerPoints[i].X, outerPoints[i].Y);
              builder.LineTo(innerPoints[i].X, innerPoints[i].Y);
            }
            builder.MoveTo(0.5, 0.5);
            builder.LineTo(0.8, 0.5);
            builder.MoveTo(0.5, 0.5);
            builder.LineTo(0.6, 0.1);
            icons.Add(builder.GetPathIcon());
            result = CreateCombinedIcon(icons);
            break;
          case EventType.Escalation:
            var cornerOnCircle = 0.5 - 0.5*RadiusToCornerOffset;
            builder.MoveTo(0.5, 0);
            builder.LineTo(0.5 + cornerOnCircle, 0.5 + cornerOnCircle);
            builder.LineTo(0.5, 0.5);
            builder.LineTo(0.5 - cornerOnCircle, 0.5 + cornerOnCircle);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case EventType.Conditional:
            icons = new List<IIcon>();
            builder.MoveTo(0.217, 0.147);
            builder.LineTo(0.783, 0.147);
            builder.LineTo(0.783, 0.853);
            builder.LineTo(0.217, 0.853);
            builder.Close();
            icons.Add(builder.GetPathIcon());

            builder.Pen = filled ? BpmnConstants.Pens.EventTypeDetailInverted : BpmnConstants.Pens.EventTypeDetail;
            for (int i = 0; i < 4; i++) {
              var y = 0.235 + i*0.177;
              builder.MoveTo(0.274, y);
              builder.LineTo(0.726, y);
            }
            icons.Add(builder.GetPathIcon());
            result = builder.CombineIcons(icons);
            break;
          case EventType.Link:
            builder.MoveTo(0.1, 0.38);
            builder.LineTo(0.5, 0.38);
            builder.LineTo(0.5, 0.1);
            builder.LineTo(0.9, 0.5);
            builder.LineTo(0.5, 0.9);
            builder.LineTo(0.5, 0.62);
            builder.LineTo(0.1, 0.62);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case EventType.Error:
            const float x1 = 0.354f;
            const float x2 = 0.084f;
            const float x3 = 0.115f;
            const float y1 = 0.354f;
            const float y2 = 0.049f;
            const float y3 = 0.260f;

            builder.MoveTo(0.5 + x1, 0.5 - y1);
            builder.LineTo(0.5 + x2, 0.5 + y2);
            builder.LineTo(0.5 - x3, 0.5 - y3);
            builder.LineTo(0.5 - x1, 0.5 + y1);
            builder.LineTo(0.5 - x2, 0.5 - y2);
            builder.LineTo(0.5 + x3, 0.5 + y3);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case EventType.Cancel:
            var bigD = 0.5 - 0.5*RadiusToCornerOffset;
            const double smallD = 0.05;
            builder.MoveTo(0.5 - bigD - smallD, 0.5 - bigD + smallD);
            builder.LineTo(0.5 - bigD + smallD, 0.5 - bigD - smallD);
            builder.LineTo(0.5, 0.5 - 2*smallD);
            builder.LineTo(0.5 + bigD - smallD, 0.5 - bigD - smallD);
            builder.LineTo(0.5 + bigD + smallD, 0.5 - bigD + smallD);
            builder.LineTo(0.5 + 2*smallD, 0.5);
            builder.LineTo(0.5 + bigD + smallD, 0.5 + bigD - smallD);
            builder.LineTo(0.5 + bigD - smallD, 0.5 + bigD + smallD);
            builder.LineTo(0.5, 0.5 + 2*smallD);
            builder.LineTo(0.5 - bigD + smallD, 0.5 + bigD + smallD);
            builder.LineTo(0.5 - bigD - smallD, 0.5 + bigD - smallD);
            builder.LineTo(0.5 - 2*smallD, 0.5);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case EventType.Compensation:
            result = CreateCompensation(filled);
            builder.Clear();
            break;
          case EventType.Signal:
            var triangle = CreatePolygon(3, 0.5, 0);
            builder.MoveTo(triangle[0].X, triangle[0].Y);
            builder.LineTo(triangle[1].X, triangle[1].Y);
            builder.LineTo(triangle[2].X, triangle[2].Y);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case EventType.Multiple:
            var pentagram = CreatePolygon(5, 0.5, 0);
            builder.MoveTo(pentagram[0].X, pentagram[0].Y);
            builder.LineTo(pentagram[1].X, pentagram[1].Y);
            builder.LineTo(pentagram[2].X, pentagram[2].Y);
            builder.LineTo(pentagram[3].X, pentagram[3].Y);
            builder.LineTo(pentagram[4].X, pentagram[4].Y);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case EventType.ParallelMultiple:
            var brush = filled ? BpmnConstants.Brushes.EventTypeThrowing : BpmnConstants.Brushes.EventTypeCatching;
            result = CreatePlusIcon(1.0, BpmnConstants.Pens.EventType, brush);
            break;
          case EventType.Terminate:
            result = builder.CreateEllipseIcon();
            break;
          default:
            builder.Clear();
            break;
        }
        eventTypes.Add(eventTypeWithFill, result);
      }
      return result;
    }

    private class EventTypeWithFill
    {
      private readonly EventType type;
      private readonly bool filled;

      public EventTypeWithFill(EventType type, bool filled) {
        this.type = type;
        this.filled = filled;
      }

      public override bool Equals(object other) {
        if (!(other is EventTypeWithFill)) {
          return false;
        }
        return ((EventTypeWithFill)other).type == type && ((EventTypeWithFill)other).filled == filled;
      }

      public override int GetHashCode() {
        unchecked {
          return (type.GetHashCode() * 397) ^ filled.GetHashCode();
        }
      }
    }

    private static readonly Dictionary<PenAndBrush, IIcon> messages = new Dictionary<PenAndBrush, IIcon>(); 

    public static IIcon CreateMessage(Pen pen, Brush brush) {
      IIcon result;
      var penAndBrush = new PenAndBrush(pen, brush);
      if (!messages.TryGetValue(penAndBrush, out result)) {
        List<IIcon> icons = new List<IIcon>();

        builder.Pen = pen;
        builder.Brush = brush;
        builder.MoveTo(0, 0);
        builder.LineTo(1, 0);
        builder.LineTo(1, 1);
        builder.LineTo(0, 1);
        builder.Close();
        icons.Add(builder.GetPathIcon());

        builder.Pen = pen;
        builder.MoveTo(0, 0);
        builder.LineTo(0.5, 0.5);
        builder.LineTo(1, 0);
        icons.Add(builder.GetPathIcon());
        result = builder.CombineIcons(icons);
        messages.Add(penAndBrush, result);
      }
      return result;
    }

    private class PenAndBrush {
      private readonly Pen pen;
      private readonly Brush brush;

      public PenAndBrush(Pen pen, Brush brush) {
        this.pen = pen;
        this.brush = brush;
      }

      public override bool Equals(object other) {
        if (!(other is PenAndBrush)) {
          return false;
        }
        return ((PenAndBrush)other).pen == pen && ((PenAndBrush)other).brush == brush;
      }

      public override int GetHashCode() {
        unchecked {
          return (pen.GetHashCode() * 397) ^ brush.GetHashCode();
        }
      }
    }

    private static readonly Dictionary<PlusData, IIcon> plusIcons = new Dictionary<PlusData, IIcon>(); 

    private static IIcon CreatePlusIcon(double size, Pen pen, Brush brush) {
      IIcon result;
      var plusData = new PlusData(size, pen, brush);
      if (!plusIcons.TryGetValue(plusData, out result)) {
        builder.Pen = pen;
        builder.Brush = brush;
        var d = 0.1*size;
        var dOff = Math.Sqrt(0.25*size*size - d*d);
        builder.MoveTo(0.5 - dOff, 0.5 - d);
        builder.LineTo(0.5 - d, 0.5 - d);
        builder.LineTo(0.5 - d, 0.5 - dOff);
        builder.LineTo(0.5 + d, 0.5 - dOff);
        builder.LineTo(0.5 + d, 0.5 - d);
        builder.LineTo(0.5 + dOff, 0.5 - d);
        builder.LineTo(0.5 + dOff, 0.5 + d);
        builder.LineTo(0.5 + d, 0.5 + d);
        builder.LineTo(0.5 + d, 0.5 + dOff);
        builder.LineTo(0.5 - d, 0.5 + dOff);
        builder.LineTo(0.5 - d, 0.5 + d);
        builder.LineTo(0.5 - dOff, 0.5 + d);
        builder.Close();
        result = builder.GetPathIcon();
        plusIcons.Add(plusData, result);
      }
      return result;
    }

    private class PlusData
    {
      private readonly double size;
      private readonly Pen pen;
      private readonly Brush brush;

      public PlusData(double size, Pen pen, Brush brush) {
        this.size = size;
        this.pen = pen;
        this.brush = brush;
      }

      public override bool Equals(object other) {
        if (!(other is PlusData)) {
          return false;
        }
        PlusData otherData = (PlusData) other;
        return otherData.size == size && otherData.pen == pen && otherData.brush == brush;
      }

      public override int GetHashCode() {
        unchecked {
          var brushHC = brush != null ? brush.GetHashCode() : 1;
          return (((size.GetHashCode() * 397) ^ pen.GetHashCode()) * 397) ^ brushHC;
        }
      }
    }

    private static IIcon choreographyTask;
    private static IIcon choreographyCall;

    public static IIcon CreateChoreography(ChoreographyType type) {
      if (type == ChoreographyType.Task) {
        if (choreographyTask == null) {
          builder.Pen = BpmnConstants.Pens.ChoreographyTask;
          choreographyTask = builder.CreateRectIcon(
            BpmnConstants.ChoreographyCornerRadius, BpmnConstants.ChoreographyCornerRadius,
            BpmnConstants.ChoreographyCornerRadius, BpmnConstants.ChoreographyCornerRadius);
        }
        return choreographyTask;
      } else {
        if (choreographyCall == null) {
          builder.Pen = BpmnConstants.Pens.ChoreographyCall;
          choreographyCall = builder.CreateRectIcon(
            BpmnConstants.ChoreographyCornerRadius, BpmnConstants.ChoreographyCornerRadius,
            BpmnConstants.ChoreographyCornerRadius, BpmnConstants.ChoreographyCornerRadius);
        }
        return choreographyCall;
      }
    }

    private static readonly Dictionary<ParticipantBandType, IIcon> participantBands = new Dictionary<ParticipantBandType, IIcon>(); 

    public static IIcon CreateChoreographyParticipant(bool sending, double topRadius, double bottomRadius) {
      IIcon result;
      var participantBandType = new ParticipantBandType(sending, topRadius, bottomRadius);
      if (!participantBands.TryGetValue(participantBandType, out result)) {
        builder.Pen = BpmnConstants.Pens.ChoreographyTask;
        builder.Brush = sending
          ? BpmnConstants.Brushes.ChoreographyInitializingParticipant
          : BpmnConstants.Brushes.ChoreographyReceivingParticipant;
        result = builder.CreateRectIcon(topRadius, topRadius, bottomRadius, bottomRadius);
        participantBands.Add(participantBandType, result);
      }
      return result;
    }

    private struct ParticipantBandType {
      private readonly bool sending;
      private readonly double topRadius;
      private readonly double bottomRadius;

      public ParticipantBandType(bool sending, double topRadius, double bottomRadius) {
        this.sending = sending;
        this.topRadius = topRadius;
        this.bottomRadius = bottomRadius;
      }

      public bool Equals(ParticipantBandType other) {
        return other.sending == sending && other.topRadius == topRadius && other.bottomRadius == bottomRadius;
      }

      public override bool Equals(object obj) {
        if (obj.GetType() != typeof(ParticipantBandType)) {
          return false;
        }
        return Equals((ParticipantBandType)obj);
      }

      public override int GetHashCode() {
        unchecked {
          return ((sending.GetHashCode() * 397) ^ topRadius.GetHashCode()) * 397 ^ bottomRadius.GetHashCode();
        }
      }
    }

    private static IIcon taskBand;

    public static IIcon CreateChoreographyTaskBand() {
      if (taskBand == null) {
        builder.Pen = null;
        builder.Brush = BpmnConstants.Brushes.ChoreographyTaskBand;
        taskBand = builder.CreateRectIcon(0);
      }
      return taskBand;
    }

    private static readonly Dictionary<ConversationType, IIcon> conversations = new Dictionary<ConversationType, IIcon>(4); 

    public static IIcon CreateConversation(ConversationType type) {
      IIcon result;
      if (!conversations.TryGetValue(type, out result)) {
        switch (type) {
          case ConversationType.Conversation:
          case ConversationType.SubConversation:
            builder.Pen = BpmnConstants.Pens.Conversation;
            break;
          case ConversationType.CallingGlobalConversation:
          case ConversationType.CallingCollaboration:
            builder.Pen = BpmnConstants.Pens.CallingConversation;
            break;
        }
        builder.Brush = BpmnConstants.Brushes.Conversation;

        builder.MoveTo(0, 0.5);
        builder.LineTo(0.25, 0);
        builder.LineTo(0.75, 0);
        builder.LineTo(1, 0.5);
        builder.LineTo(0.75, 1);
        builder.LineTo(0.25, 1);
        builder.Close();
        result = builder.GetPathIcon();
        conversations.Add(type, result);
      }
      return result;
    }

    private static IIcon conversationSubState;

    public static IIcon CreateConversationMarker(ConversationType type) {
      switch (type) {
        case ConversationType.SubConversation:
        case ConversationType.CallingCollaboration:
          return conversationSubState ?? (conversationSubState = CreateStaticSubState(SubState.Collapsed));
        default:
          return null;
      }
    }

    private static IIcon dataObject;

    public static IIcon CreateDataObject() {
      return dataObject ?? (dataObject = new DataObjectIcon() {Pen = BpmnConstants.Pens.DataObject, Brush = BpmnConstants.Brushes.DataObject});
    }

    private static IIcon dataObjectInputType;
    private static IIcon dataObjectOutputType;

    public static IIcon CreateDataObjectType(DataObjectType type) {
      switch (type) {
        case DataObjectType.Input:
          return dataObjectInputType ?? (dataObjectInputType = CreateEventType(EventType.Link, false));
        case DataObjectType.Output:
          return dataObjectOutputType ?? (dataObjectOutputType = CreateEventType(EventType.Link, true));
        case DataObjectType.None:
        default:
          return null;
      }
    }

    private static IIcon leftAnnotation;
    private static IIcon rightAnnotation;

    public static IIcon CreateAnnotation(bool left) {
      if (left) {
        if (leftAnnotation == null) {
          var icons = new List<IIcon>();
          builder.Pen = null;
          builder.Brush = BpmnConstants.Brushes.Annotation;
          icons.Add(builder.CreateRectIcon(0));
          builder.Pen = BpmnConstants.Pens.Annotation;
          builder.MoveTo(0.1, 0);
          builder.LineTo(0, 0);
          builder.LineTo(0, 1);
          builder.LineTo(0.1, 1);
          icons.Add(builder.GetPathIcon());
          leftAnnotation = builder.CombineIcons(icons);
        }
        return leftAnnotation;
      } else {
        if (rightAnnotation == null) {
          var icons = new List<IIcon>();
          builder.Pen = null;
          builder.Brush = BpmnConstants.Brushes.Annotation;
          icons.Add(builder.CreateRectIcon(0));
          builder.Pen = BpmnConstants.Pens.Annotation;
          builder.MoveTo(0.9, 0);
          builder.LineTo(1, 0);
          builder.LineTo(1, 1);
          builder.LineTo(0.9, 1);
          icons.Add(builder.GetPathIcon());
          rightAnnotation = builder.CombineIcons(icons);
        }
        return rightAnnotation;
      }

    }

    private static IIcon dataStore;

    public static IIcon CreateDataStore() {
      if (dataStore == null) {
        const double halfEllipseHeight = 0.125;
        const double ringOffset = 0.07;

        var icons = new List<IIcon>();
        builder.Pen = BpmnConstants.Pens.DataStore;
        builder.Brush = BpmnConstants.Brushes.DataStore;

        builder.MoveTo(0, halfEllipseHeight);
        builder.LineTo(0, 1 - halfEllipseHeight);
        builder.CubicTo(0, 1, 1, 1, 1, 1 - halfEllipseHeight);
        builder.LineTo(1, halfEllipseHeight);
        builder.CubicTo(1, 0, 0, 0, 0, halfEllipseHeight);
        builder.Close();
        icons.Add(builder.GetPathIcon());

        builder.Pen = BpmnConstants.Pens.DataStore;
        double ellipseCenterY = halfEllipseHeight;
        for (int i = 0; i < 3; i++) {
          builder.MoveTo(0, ellipseCenterY);
          builder.CubicTo(0, ellipseCenterY + halfEllipseHeight, 1, ellipseCenterY + halfEllipseHeight, 1,
            ellipseCenterY);
          ellipseCenterY += ringOffset;
        }
        icons.Add(builder.GetPathIcon());
        dataStore = builder.CombineIcons(icons);
      }
      return dataStore;
    }

    private static readonly Dictionary<ArrowType, IIcon> arrows = new Dictionary<ArrowType, IIcon>(8); 

    public static IIcon CreateArrowIcon(ArrowType type) {
      IIcon result;
      if (!arrows.TryGetValue(type, out result)) {
        builder.Pen = BpmnConstants.Pens.Arrow;
        switch (type) {
          case ArrowType.DefaultSource:
            builder.MoveTo(0.1, 0.1);
            builder.LineTo(0.9, 0.9);
            result = builder.GetPathIcon();
            break;
          case ArrowType.Association:
            builder.MoveTo(0.5, 0);
            builder.LineTo(1, 0.5);
            builder.LineTo(0.5, 1);
            result = builder.GetPathIcon();
            break;
          case ArrowType.ConditionalSource:
            builder.MoveTo(0, 0.5);
            builder.LineTo(0.5, 0);
            builder.LineTo(1, 0.5);
            builder.LineTo(0.5, 1);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          case ArrowType.MessageSource:
            result = builder.CreateEllipseIcon();
            break;
          case ArrowType.MessageTarget:
            builder.MoveTo(0, 0);
            builder.LineTo(1, 0.5);
            builder.LineTo(0, 1);
            builder.Close();
            result = builder.GetPathIcon();
            break;
          default:
          case ArrowType.SequenceTarget:
          case ArrowType.DefaultTarget:
          case ArrowType.ConditionalTarget:
            builder.Brush = Brushes.Black;
            builder.MoveTo(0, 0);
            builder.LineTo(1, 0.5);
            builder.LineTo(0, 1);
            builder.Close();
            result = builder.GetPathIcon();
            break;
        }
        arrows.Add(type, result);
      }
      return result;
    }

    public static IIcon CreateLine(Pen pen, double x1, double y1, double x2, double y2) {
      builder.Pen = pen;
      builder.MoveTo(x1, y1);
      builder.LineTo(x2, y2);
      return builder.GetPathIcon();
    }

    protected static PointD[] CreatePolygon(int sideCount, double radius, double rotation) {
      var result = new PointD[sideCount];
      double delta = Math.PI * 2.0 / sideCount;

      for (int i = 0; i < sideCount; i++) {
        var angle = delta * i + rotation;
        result[i] = (new PointD(radius * Math.Sin(angle) + 0.5, -radius * Math.Cos(angle) + 0.5));
      }
      return result;
    }
  }
}
