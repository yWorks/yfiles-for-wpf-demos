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
    private static readonly double radiusToCornerOffset = Math.Sqrt((1.5 - Math.Sqrt(2)));

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

    public static IIcon CreateActivity(ActivityType type, Brush background, Brush outlineBrush) {
      var hasDefaultColors = Equals(background, BpmnConstants.ActivityDefaultBackground) &&
                             Equals(outlineBrush, BpmnConstants.ActivityDefaultOutline);
      IIcon result;
      if (hasDefaultColors && activityIcons.TryGetValue(type, out result)) {
        return result;
      }

      Pen outlinePen;

      switch (type) {
        case ActivityType.EventSubProcess:
          outlinePen = (Pen) new Pen(outlineBrush, 1) { DashStyle = DashStyles.Dot, DashCap = PenLineCap.Round }.GetAsFrozen();
          break;
        case ActivityType.CallActivity:
          outlinePen = (Pen) new Pen(outlineBrush, 3).GetAsFrozen();
          break;
        default:
          outlinePen = (Pen) new Pen(outlineBrush, 1).GetAsFrozen();
          break;
      }

      builder.Pen = outlinePen;
      builder.Brush = background;

      if (type == ActivityType.Transaction) {
        var icons = new List<IIcon>(2);
        icons.Add(builder.CreateRectIcon(BpmnConstants.ActivityCornerRadius));

        builder.Brush = background;
        builder.Pen = outlinePen;
        var rectIcon = builder.CreateRectIcon(BpmnConstants.ActivityCornerRadius - BpmnConstants.DoubleLineOffset);
        icons.Add(CreatePlacedIcon(rectIcon, BpmnConstants.DoubleLinePlacement, SizeD.Empty));
        result = builder.CombineIcons(icons);
      } else {
        result = builder.CreateRectIcon(BpmnConstants.ActivityCornerRadius);
      }

      if (hasDefaultColors) {
        activityIcons[type] = result;
      }

      return result;
    }

    private static readonly Dictionary<TaskType, IIcon> taskIcons = new Dictionary<TaskType, IIcon>();

    public static IIcon CreateActivityTaskType(TaskType type, Brush iconBrush, Brush background) {
      var hasDefaultColor = Equals(iconBrush, BpmnConstants.DefaultIconColor);

      IIcon result;
      if (hasDefaultColor && taskIcons.TryGetValue(type, out result)) {
        return result;
      }

      List<IIcon> icons;
      switch (type) {
        case TaskType.Send:
          result = CreatePlacedIcon(CreateMessage(Pens.Transparent, iconBrush, true),
              BpmnConstants.ActivityTaskTypeMessagePlacement, SizeD.Empty);
          break;
        case TaskType.Receive:
          result = CreatePlacedIcon(CreateMessage((Pen) new Pen(iconBrush, 1).GetAsFrozen(), Brushes.Transparent),
              BpmnConstants.ActivityTaskTypeMessagePlacement, SizeD.Empty);
          break;
        case TaskType.User: {
          var pen = (Pen) new Pen {
              Brush = iconBrush,
              StartLineCap = PenLineCap.Round,
              EndLineCap = PenLineCap.Round,
              LineJoin = PenLineJoin.Round
          }.GetAsFrozen();
          builder.Pen = pen;
          var lightBrush = iconBrush.Clone();
          lightBrush.Opacity = 0.17;
          lightBrush.Freeze();
          builder.Brush = lightBrush;

          // body + head
          icons = new List<IIcon>(3);
          builder.MoveTo(1, 1);
          builder.LineTo(0, 1);
          builder.LineTo(0, 0.701);
          builder.QuadTo(0.13, 0.5, 0.316, 0.443);
          builder.LineTo(0.5 + 0.224 * Math.Cos(3.0 / 4.0 * Math.PI), 0.224 + 0.224 * Math.Sin(3.0 / 4.0 * Math.PI));
          builder.ArcTo(0.224, 0.5, 0.224, 3.0 / 4.0 * Math.PI, 9.0 / 4.0 * Math.PI);
          builder.LineTo(0.684f, 0.443);
          builder.QuadTo(0.87, 0.5, 1, 0.701);
          builder.Close();
          icons.Add(builder.GetPathIcon());

          // hair
          builder.Pen = pen;
          builder.Brush = iconBrush;
          builder.MoveTo(0.287, 0.229);
          builder.CubicTo(0.48, 0.053, 0.52, 0.253, 0.713, 0.137);
          builder.ArcTo(0.224, 0.5, 0.224, 31.0 / 16.0 * Math.PI, Math.PI);
          builder.Close();
          icons.Add(builder.GetPathIcon());

          builder.Pen = pen;

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
        }
        case TaskType.Manual: {
          var pen = (Pen) new Pen {
              Brush = iconBrush,
              StartLineCap = PenLineCap.Round,
              EndLineCap = PenLineCap.Round,
              LineJoin = PenLineJoin.Round
          }.GetAsFrozen();
          builder.Pen = pen;
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
        }
        case TaskType.BusinessRule: {
          const float headHeight = 0.192f;
          const float rowHeight = 0.304f;
          const float column1Width = 0.264f;

          icons = new List<IIcon>(3);
          var darkBrush = iconBrush.Clone();
          darkBrush.Opacity = 0.5;
          darkBrush.Freeze();
          var lightBrush = iconBrush.Clone();
          lightBrush.Opacity = 0.17;
          lightBrush.Freeze();
          var pen = (Pen) new Pen(iconBrush, 1).GetAsFrozen();
          builder.Brush = darkBrush;
          builder.Pen = pen;

          // outline
          builder.MoveTo(0, 0.1);
          builder.LineTo(1, 0.1);
          builder.LineTo(1, headHeight + 0.1);
          builder.LineTo(0, headHeight + 0.1);
          builder.Close();
          icons.Add(builder.GetPathIcon());

          // rows outline
          builder.Brush = lightBrush;
          builder.Pen = pen;
          builder.MoveTo(0, 0.1 + headHeight);
          builder.LineTo(1, 0.1 + headHeight);
          builder.LineTo(1, 0.9);
          builder.LineTo(0, 0.9);
          builder.Close();
          icons.Add(builder.GetPathIcon());

          // line between second and third row
          builder.Pen = pen;
          builder.MoveTo(0, 0.1 + headHeight + rowHeight);
          builder.LineTo(1f, 0.1 + headHeight + rowHeight);

          // line between first and second column
          builder.MoveTo(column1Width, 0.1 + headHeight);
          builder.LineTo(column1Width, 0.9);
          icons.Add(builder.GetPathIcon());

          result = builder.CombineIcons(icons);
          break;
        }
        case TaskType.Service: {
          icons = new List<IIcon>();
          var pen = (Pen) new Pen(iconBrush, 0.3).GetAsFrozen();
          var darkBrush = iconBrush.Clone();
          darkBrush.Opacity = 0.5;
          darkBrush.Freeze();
          var lightBrush = iconBrush.Clone();
          lightBrush.Opacity = 0.17;
          lightBrush.Freeze();

          // top gear
          icons.Add(CreateGear(0.4, 0.4, 0.4, pen, darkBrush));
          icons.Add(CreateGear(0.16, 0.4, 0.4, null, background)); // background-colored gear to make shading work
          icons.Add(CreateGear(0.16, 0.4, 0.4, pen, lightBrush));

          // bottom gear
          icons.Add(CreateGear(0.4, 0.6, 0.6, null, background)); // background-colored gear to make shading work
          icons.Add(CreateGear(0.4, 0.6, 0.6, pen, darkBrush));
          icons.Add(CreateGear(0.16, 0.6, 0.6, null, background)); // background-colored gear to make shading work
          icons.Add(CreateGear(0.16, 0.6, 0.6, pen, lightBrush));

          result = builder.CombineIcons(icons);
          break;
        }
        case TaskType.Script: {
          builder.Pen = (Pen) new Pen {
              Brush = iconBrush,
              StartLineCap = PenLineCap.Round,
              EndLineCap = PenLineCap.Round,
              LineJoin = PenLineJoin.Round
          }.GetAsFrozen();

          // outline
          const double size = 0.5;
          const double curveEndX = 0.235;
          const double curveEndY = size;
          const double curveCenterX = curveEndX + (size - curveEndX) * 0.5;
          const double curveDeltaX = 0.5;
          const double curveDeltaY = size * 0.5;

          builder.MoveTo(0.5 + size, 0.5 - size);
          builder.CubicTo(0.5 + curveCenterX - curveDeltaX, 0.5 - curveDeltaY, 0.5 + curveCenterX + curveDeltaX,
              0.5 + curveDeltaY, 0.5 + curveEndX, 0.5 + curveEndY);
          builder.LineTo(0.5 - size, 0.5 + size);
          builder.CubicTo(0.5 - curveCenterX + curveDeltaX, 0.5 + curveDeltaY, 0.5 - curveCenterX - curveDeltaX,
              0.5 - curveDeltaY, 0.5 - curveEndX, 0.5 - curveEndY);
          builder.Close();

          // inner lines
          const double deltaY2 = size * 0.2f;
          const double deltaX1 = 0.045f;
          const double deltaX2 = 0.085f;
          const double length = 0.3f * (size + curveEndX);

          builder.MoveTo(0.5 - length - deltaX2, 0.5 - 3f * deltaY2);
          builder.LineTo(0.5 + length - deltaX2, 0.5 - 3f * deltaY2);
          builder.MoveTo(0.5 - length - deltaX1, 0.5 - 1f * deltaY2);
          builder.LineTo(0.5 + length - deltaX1, 0.5 - 1f * deltaY2);
          builder.MoveTo(0.5 - length + deltaX1, 0.5 + 1f * deltaY2);
          builder.LineTo(0.5 + length + deltaX1, 0.5 + 1f * deltaY2);
          builder.MoveTo(0.5 - length + deltaX2, 0.5 + 3f * deltaY2);
          builder.LineTo(0.5 + length + deltaX2, 0.5 + 3f * deltaY2);
          result = builder.GetPathIcon();
          break;
        }
        case TaskType.EventTriggered:
        default:
          result = null;
          break;
      }

      if (hasDefaultColor) {
        taskIcons[type] = result;
      }

      return result;
    }

    private static IIcon CreateGear(double radius, double centerX, double centerY, Pen pen, Brush brush, double start = -2* Math.PI/48, int count = 8) {
      builder.Pen = pen;
      builder.Brush = brush;
      var smallR = 0.7*radius;

      var angle = start;
      builder.MoveTo(centerX + radius * Math.Cos(angle), centerY + radius * Math.Sin(angle));
      for (int i = 0; i < count; i++) {
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

    public static IIcon CreateLoopCharacteristic(LoopCharacteristic loopCharacteristic, Brush iconBrush) {
      var hasDefaultColor = Equals(iconBrush, BpmnConstants.DefaultIconColor);

      IIcon result = null;
      if (hasDefaultColor && loopTypes.TryGetValue(loopCharacteristic, out result)) {
        return result;
      }

      builder.Pen = (Pen) new Pen(iconBrush, 1).GetAsFrozen();

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
          builder.Brush = iconBrush;

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
          builder.Brush = iconBrush;

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

      if (hasDefaultColor) {
        loopTypes[loopCharacteristic] = result;
      }
        
      return result;
    }

    private static IIcon adHoc;

    public static IIcon CreateAdHoc(Brush iconBrush) {
      var hasDefaultColor = Equals(iconBrush, BpmnConstants.DefaultIconColor);

      if (hasDefaultColor && adHoc != null) {
        return adHoc;
      }

      builder.Pen = (Pen) new Pen(iconBrush, 1).GetAsFrozen();
      builder.Brush = iconBrush;

      const double fromAngle1 = 5.0 / 4.0 * Math.PI;
      const double toAngle1 = 7.0 / 4.0 * Math.PI;
      const double fromAngle2 = 1.0 / 4.0 * Math.PI;
      const double toAngle2 = 3.0 / 4.0 * Math.PI;

      var smallR = 0.25 / (1 - Math.Sqrt(1.5 - Math.Sqrt(2)));
      var co = smallR * radiusToCornerOffset;
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

      var icon = builder.GetPathIcon();

      if (hasDefaultColor) {
        adHoc = icon;
      }

      return icon;
    }

    private static IIcon comparison;
    private static IIcon filledComparison;

    public static IIcon CreateCompensation(bool filled, Brush iconBrush) {
      var hasDefaultColor = Equals(iconBrush, BpmnConstants.DefaultIconColor);

      if (hasDefaultColor) {
        if (filled && filledComparison != null) {
          return filledComparison;
        }
        if (!filled && comparison != null) {
          return comparison;
        }
      }

      builder.Pen = (Pen) new Pen(iconBrush, 1).GetAsFrozen();
      builder.Brush = filled ? iconBrush : null;

      var sqrt3inv = 1 / Math.Sqrt(3);
      var halfSqurt3 = sqrt3inv / 2;
      var xOff = 0.5 / (2 * sqrt3inv);
      builder.MoveTo(0, 0.5);
      builder.LineTo(xOff, 0.5 - halfSqurt3);
      builder.LineTo(xOff, 0.5);
      builder.LineTo(2 * xOff, 0.5 - halfSqurt3);
      builder.LineTo(2 * xOff, 0.5 + halfSqurt3);
      builder.LineTo(xOff, 0.5);
      builder.LineTo(xOff, 0.5 + halfSqurt3);
      builder.Close();

      var icon = builder.GetPathIcon();

      if (hasDefaultColor) {
        if (filled) {
          filledComparison = icon;
        } else {
          comparison = icon;
        }
      }

      return icon;
    }

    private static readonly Dictionary<SubState, IIcon> subStates = new Dictionary<SubState, IIcon>(3);

    public static IIcon CreateStaticSubState(SubState subState, Brush iconBrush) {
      var hasDefaultColor = Equals(iconBrush, BpmnConstants.DefaultIconColor);

      IIcon result = null;
      if (hasDefaultColor && subStates.TryGetValue(subState, out result)) {
        return result;
      }

      var iconPen = (Pen) new Pen(iconBrush, 1).GetAsFrozen();
      builder.Pen = iconPen;

      switch (subState) {
        case SubState.Expanded:
          var icons = new List<IIcon>();
          icons.Add(builder.CreateRectIcon(0));
          builder.Pen = iconPen;
          builder.MoveTo(0.2, 0.5);
          builder.LineTo(0.8, 0.5);
          icons.Add(builder.GetPathIcon());
          result = builder.CombineIcons(icons);
          break;
        case SubState.Collapsed:
          var icons2 = new List<IIcon>();
          icons2.Add(builder.CreateRectIcon(0));
          builder.Pen = iconPen;
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

      if (hasDefaultColor) {
        subStates[subState] = result;
      }

      return result;
    }

    public static IIcon CreateDynamicSubState(INode node, Brush iconBrush) {
      return new CollapseButtonIcon(node, iconBrush);
    }

    private static IIcon gateway;

    public static IIcon CreateGateway(Brush background, Brush outline) {
      var hasDefaultColors = Equals(background, BpmnConstants.GatewayDefaultBackground) &&
                             Equals(outline, BpmnConstants.GatewayDefaultOutline);

      if (hasDefaultColors && gateway != null) {
        return gateway;
      }

      builder.Pen = (Pen) new Pen(outline, 1).GetAsFrozen();
      builder.Brush = background;
      builder.MoveTo(0.5, 0);
      builder.LineTo(1, 0.5);
      builder.LineTo(0.5, 1);
      builder.LineTo(0, 0.5);
      builder.Close();
      var gatewayIcon = builder.GetPathIcon();
      if (hasDefaultColors) {
        gateway = gatewayIcon;
      }
      return gatewayIcon;
    }

    private static readonly Dictionary<GatewayType, IIcon> gatewayTypes = new Dictionary<GatewayType, IIcon>(8); 

    public static IIcon CreateGatewayType(GatewayType type, Brush brush) {
      var hasDefaultColor = Equals(brush, BpmnConstants.DefaultIconColor);

      IIcon result = null;
      if (hasDefaultColor && gatewayTypes.TryGetValue(type, out result)) {
        return result;
      }

      var pen = (Pen) new Pen(brush, 1).GetAsFrozen();
      var thickPen = (Pen) new Pen(brush, 3).GetAsFrozen();
      List<IIcon> icons;

      PointD[] outer = CreatePolygon(24, 0.5, Math.PI / 24);
      switch (type) {
        case GatewayType.ExclusiveWithoutMarker:
          break;
        case GatewayType.ExclusiveWithMarker:
          builder.Brush = brush;
          builder.Pen = pen;
          var cornerOffY = 0.5 - 0.5 * Math.Sin(Math.PI / 4);
          var cornerOffX = cornerOffY + 0.1;
          var xOff = 0.06;

          var x1 = cornerOffX;
          var x2 = cornerOffX + 2 * xOff;

          var y1 = cornerOffY;
          var y2 = 0.5 - (0.5 * xOff - cornerOffY * xOff) / (0.5 - cornerOffX - xOff);

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
          builder.Pen = thickPen;
          result = CreatePlacedIcon(builder.CreateEllipseIcon(), BpmnConstants.ThickLinePlacement, SizeD.Empty);
          break;
        case GatewayType.EventBased:
        case GatewayType.ExclusiveEventBased:
          icons = new List<IIcon>(3);
          builder.Pen = pen;
          icons.Add(builder.CreateEllipseIcon());

          if (type == GatewayType.EventBased) {
            builder.Pen = pen;
            var innerCircleIcon = builder.CreateEllipseIcon();
            icons.Add(CreatePlacedIcon(innerCircleIcon, BpmnConstants.DoubleLinePlacement, SizeD.Empty));
          }

          builder.Pen = pen;
          IList<PointD> polygon = CreatePolygon(5, 0.5, 0);
          builder.MoveTo(polygon[0].X, polygon[0].Y);
          for (int i = 1; i < 5; i++) {
            builder.LineTo(polygon[i].X, polygon[i].Y);
          }
          builder.Close();
          var innerIcon = builder.GetPathIcon();
          icons.Add(CreatePlacedIcon(innerIcon, BpmnConstants.InsideDoubleLinePlacement, SizeD.Empty));
          result = builder.CombineIcons(icons);
          break;
        case GatewayType.Parallel:
          result = CreatePlusIcon(0.8, pen, brush);
          break;
        case GatewayType.ParallelEventBased:
          icons = new List<IIcon>(2);
          builder.Pen = pen;
          icons.Add(builder.CreateEllipseIcon());
          icons.Add(CreatePlusIcon(0.6, pen, null));
          result = builder.CombineIcons(icons);
          break;
        case GatewayType.Complex:
          builder.Brush = brush;
          builder.Pen = pen;
          double width = Math.Sqrt(0.5 - (0.5 * Math.Cos(Math.PI / 12)));
          double rInner = width * Math.Sqrt((1 + Math.Sqrt(2) / 2));
          var inner = CreatePolygon(8, rInner, Math.PI / 8);

          builder.MoveTo(outer[0].X, outer[0].Y);
          for (int i = 0; i < 8; i++) {
            builder.LineTo(outer[3 * i].X, outer[3 * i].Y);
            builder.LineTo(inner[i].X, inner[i].Y);
            builder.LineTo(outer[3 * i + 2].X, outer[3 * i + 2].Y);
          }
          builder.Close();
          result = builder.GetPathIcon();
          break;
        default:
          break;
      }

      if (hasDefaultColor) {
        gatewayTypes[type] = result;
      }

      return result;
    }

    private static readonly Dictionary<EventCharacteristic, IIcon> eventCharacteristics = new Dictionary<EventCharacteristic, IIcon>(8); 

    public static IIcon CreateEvent(EventCharacteristic characteristic, Brush background, Brush outline) {
      var hasDefaultColors = Equals(background, BpmnConstants.DefaultEventBackground) &&
                             Equals(outline, BpmnConstants.DefaultEventOutline);

      IIcon result;
      if (hasDefaultColors && eventCharacteristics.TryGetValue(characteristic, out result)) {
        return result;
      }

      Pen pen = null;

      switch (characteristic) {
        case EventCharacteristic.Start:
        case EventCharacteristic.SubProcessInterrupting:
          pen = (Pen) new Pen(outline ?? Brushes.Green, 1).GetAsFrozen();
          break;
        case EventCharacteristic.SubProcessNonInterrupting:
          pen = (Pen) new Pen(outline ?? Brushes.Green, 1) { DashStyle = DashStyles.Dash }.GetAsFrozen();
          break;
        case EventCharacteristic.Catching:
        case EventCharacteristic.BoundaryInterrupting:
        case EventCharacteristic.Throwing:
          pen = (Pen) new Pen(outline ?? Brushes.Goldenrod, 1).GetAsFrozen();
          break;
        case EventCharacteristic.BoundaryNonInterrupting:
          pen = (Pen) new Pen(outline ?? Brushes.Goldenrod, 1) { DashStyle = DashStyles.Dash }.GetAsFrozen();
          break;
        case EventCharacteristic.End:
          pen = (Pen) new Pen(outline ?? Brushes.Red, 3).GetAsFrozen();
          break;
      }

      builder.Pen = pen;
      builder.Brush = background;
      var ellipseIcon = builder.CreateEllipseIcon();

      switch (characteristic) {
        case EventCharacteristic.Catching:
        case EventCharacteristic.BoundaryInterrupting:
        case EventCharacteristic.BoundaryNonInterrupting:
        case EventCharacteristic.Throwing:
          List<IIcon> icons = new List<IIcon>();
          icons.Add(ellipseIcon);

          builder.Pen = pen;
          builder.Brush = background;
          var innerEllipseIcon = builder.CreateEllipseIcon();
          icons.Add(CreatePlacedIcon(innerEllipseIcon, BpmnConstants.DoubleLinePlacement, SizeD.Empty));
          result = CreateCombinedIcon(icons);
          break;
        default:
          result = ellipseIcon;
          break;
      }

      if (hasDefaultColors) {
        eventCharacteristics[characteristic] = result;
      }

      return result;
    }

    private static readonly Dictionary<EventTypeWithFill, IIcon> eventTypes = new Dictionary<EventTypeWithFill, IIcon>(26); 

    public static IIcon CreateEventType(EventType type, bool filled, Brush brush, Brush background) {
      var hasDefaultColors = Equals(brush, BpmnConstants.DefaultIconColor) &&
                             Equals(background, BpmnConstants.DefaultEventBackground);

      IIcon result = null;
      var eventTypeWithFill = new EventTypeWithFill(type, filled);
      if (hasDefaultColors && eventTypes.TryGetValue(eventTypeWithFill, out result)) {
        return result;
      }

      var pen = (Pen) new Pen(brush, 1).GetAsFrozen();
      var roundPen = (Pen) new Pen(brush, 1) { LineJoin = PenLineJoin.Round, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }.GetAsFrozen();
      var backgroundRoundPen = (Pen) new Pen(background, 1) { LineJoin = PenLineJoin.Round, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }.GetAsFrozen();
      builder.Pen = pen;
      builder.Brush = filled ? brush : null;

      List<IIcon> icons;
      switch (type) {
        case EventType.Message:
          var combinedIcons = CreateMessage(!filled ? pen : Pens.Transparent, filled ? brush : Brushes.Transparent, filled);
          result = CreatePlacedIcon(combinedIcons, BpmnConstants.EventTypeMessagePlacement, SizeD.Empty);
          break;
        case EventType.Timer:
          icons = new List<IIcon>();
          builder.Pen = filled ? backgroundRoundPen : roundPen;
          icons.Add(builder.CreateEllipseIcon());
          builder.Pen = filled ? backgroundRoundPen : roundPen;
          var outerPoints = CreatePolygon(12, 0.5, 0);
          var innerPoints = CreatePolygon(12, 0.4, 0);
          for (int i = 0; i < 12; i++) {
            builder.MoveTo(outerPoints[i].X, outerPoints[i].Y);
            builder.LineTo(innerPoints[i].X, innerPoints[i].Y);
          }
          builder.MoveTo(0.75, 0.52);
          builder.LineTo(0.5, 0.5);
          builder.LineTo(0.6, 0.15);
          icons.Add(builder.GetPathIcon());
          result = CreateCombinedIcon(icons);
          break;
        case EventType.Escalation:
          var cornerOnCircle = 0.5 - 0.5 * radiusToCornerOffset;
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

          builder.Pen = filled ? backgroundRoundPen : roundPen;
          for (int i = 0; i < 4; i++) {
            var y = 0.235 + i * 0.177;
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
          var bigD = 0.5 - 0.5 * radiusToCornerOffset;
          const double smallD = 0.05;
          builder.MoveTo(0.5 - bigD - smallD, 0.5 - bigD + smallD);
          builder.LineTo(0.5 - bigD + smallD, 0.5 - bigD - smallD);
          builder.LineTo(0.5, 0.5 - 2 * smallD);
          builder.LineTo(0.5 + bigD - smallD, 0.5 - bigD - smallD);
          builder.LineTo(0.5 + bigD + smallD, 0.5 - bigD + smallD);
          builder.LineTo(0.5 + 2 * smallD, 0.5);
          builder.LineTo(0.5 + bigD + smallD, 0.5 + bigD - smallD);
          builder.LineTo(0.5 + bigD - smallD, 0.5 + bigD + smallD);
          builder.LineTo(0.5, 0.5 + 2 * smallD);
          builder.LineTo(0.5 - bigD + smallD, 0.5 + bigD + smallD);
          builder.LineTo(0.5 - bigD - smallD, 0.5 + bigD - smallD);
          builder.LineTo(0.5 - 2 * smallD, 0.5);
          builder.Close();
          result = builder.GetPathIcon();
          break;
        case EventType.Compensation:
          result = CreateCompensation(filled, brush);
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
        case EventType.ParallelMultiple: {
          result = CreatePlusIcon(1.0, pen, filled ? brush : background);
          break;
        }
        case EventType.Terminate:
          result = builder.CreateEllipseIcon();
          break;
        case EventType.Plain:
        default:
          builder.Clear();
          break;
      }
      if (hasDefaultColors) {
        eventTypes[eventTypeWithFill] = result;
      }
      return result;
    }

    private sealed class EventTypeWithFill
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

    public static IIcon CreateMessage(Pen pen, Brush brush, bool inverted = false) {
      var icons = new List<IIcon>();
      if (!inverted) {
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
      } else {
        // Just the two envelope shapes without the pen
        builder.Brush = brush;
        builder.Pen = null;
        builder.MoveTo(0, 0);
        builder.LineTo(1, 0);
        builder.LineTo(0.5, 0.45);
        builder.Close();
        icons.Add(builder.GetPathIcon());

        builder.Brush = brush;
        builder.Pen = null;
        builder.MoveTo(0, 0.1);
        builder.LineTo(0.5, 0.55);
        builder.LineTo(1, 0.1);
        builder.LineTo(1, 1);
        builder.LineTo(0, 1);
        builder.Close();
        icons.Add(builder.GetPathIcon());
      }
      return builder.CombineIcons(icons);
    }

    private sealed class PenAndBrush {
      private readonly Pen pen;
      private readonly Brush brush;
      private readonly bool inverted;

      public PenAndBrush(Pen pen, Brush brush, bool inverted) {
        this.pen = pen;
        this.brush = brush;
        this.inverted = inverted;
      }

      public override bool Equals(object other) {
        if (!(other is PenAndBrush)) {
          return false;
        }
        return ((PenAndBrush) other).pen == pen && ((PenAndBrush) other).brush == brush &&
               ((PenAndBrush) other).inverted == inverted;
      }

      public override int GetHashCode() {
        unchecked {
          return (pen.GetHashCode() * 397) ^ brush.GetHashCode() + inverted.GetHashCode();
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

    private sealed class PlusData
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

    public static IIcon CreateChoreography(ChoreographyType type, Brush outline) {
      var hasDefaultColor = Equals(outline, BpmnConstants.ChoreographyDefaultOutline);

      if (hasDefaultColor) {
        if (type == ChoreographyType.Task && choreographyTask != null) {
          return choreographyTask;
        }
        if (type == ChoreographyType.Call && choreographyCall != null) {
          return choreographyCall;
        }
      }

      builder.Pen = (Pen) new Pen(outline, type == ChoreographyType.Task ? 1 : 3).GetAsFrozen();

      var icon = builder.CreateRectIcon(
          // Needs all four arguments instead of one because the path is drawn differently in both cases on some platforms ...
            BpmnConstants.ChoreographyCornerRadius, BpmnConstants.ChoreographyCornerRadius,
            BpmnConstants.ChoreographyCornerRadius, BpmnConstants.ChoreographyCornerRadius);

      if (hasDefaultColor) {
        if (type == ChoreographyType.Task) {
          choreographyTask = icon;
        }
        if (type == ChoreographyType.Call) {
          choreographyCall = icon;
        }
      }

      return icon;
    }

    private static readonly Dictionary<ParticipantBandType, IIcon> participantBands = new Dictionary<ParticipantBandType, IIcon>(); 

    public static IIcon CreateChoreographyParticipant(Brush outline, Brush background, double topRadius, double bottomRadius) {
      var hasDefaultColors = Equals(outline, BpmnConstants.ChoreographyDefaultOutline) &&
                             (Equals(background, BpmnConstants.ChoreographyDefaultInitiatingColor) ||
                              Equals(background, BpmnConstants.ChoreographyDefaultResponseColor));

      IIcon result;
      var participantBandType = new ParticipantBandType(background, topRadius, bottomRadius);
      if (hasDefaultColors && participantBands.TryGetValue(participantBandType, out result)) {
        return result;
      }

      builder.Pen = (Pen) new Pen(outline, 1).GetAsFrozen();
      builder.Brush = background;
      result = builder.CreateRectIcon(topRadius, topRadius, bottomRadius, bottomRadius);
      
      if (hasDefaultColors) {
        participantBands[participantBandType] = result;
      }

      return result;
    }

    private struct ParticipantBandType {
      private readonly Brush brush;
      private readonly double topRadius;
      private readonly double bottomRadius;

      public ParticipantBandType(Brush brush, double topRadius, double bottomRadius) {
        this.brush = brush;
        this.topRadius = topRadius;
        this.bottomRadius = bottomRadius;
      }

      public bool Equals(ParticipantBandType other) {
        return other.brush == brush && other.topRadius == topRadius && other.bottomRadius == bottomRadius;
      }

      public override bool Equals(object obj) {
        if (!(obj is ParticipantBandType)) {
          return false;
        }
        return Equals((ParticipantBandType)obj);
      }

      public override int GetHashCode() {
        unchecked {
          return ((brush.GetHashCode() * 397) ^ topRadius.GetHashCode()) * 397 ^ bottomRadius.GetHashCode();
        }
      }
    }

    private static IIcon taskBand;

    public static IIcon CreateChoreographyTaskBand(Brush brush) {
      var hasDefaultColor = Equals(brush, BpmnConstants.ChoreographyDefaultBackground);

      if (hasDefaultColor && taskBand != null) {
        return taskBand;
      }

      builder.Pen = null;
      builder.Brush = brush;
      var icon = builder.CreateRectIcon(0);

      if (hasDefaultColor) {
        taskBand = icon;
      }

      return icon;
    }

    private static readonly Dictionary<ConversationType, IIcon> conversations = new Dictionary<ConversationType, IIcon>(4); 

    public static IIcon CreateConversation(ConversationType type, Brush background, Brush outline) {
      var hasDefaultColors = Equals(background, BpmnConstants.ConversationDefaultBackground) &&
                             Equals(outline, BpmnConstants.ConversationDefaultOutline);

      IIcon result;
      if (hasDefaultColors && conversations.TryGetValue(type, out result)) {
        return result;
      }

      switch (type) {
        case ConversationType.Conversation:
        case ConversationType.SubConversation:
          builder.Pen = (Pen) new Pen(outline, 1).GetAsFrozen();
          break;
        case ConversationType.CallingGlobalConversation:
        case ConversationType.CallingCollaboration:
          builder.Pen = (Pen) new Pen(outline, 3).GetAsFrozen();
          break;
      }
      builder.Brush = background;

      builder.MoveTo(0, 0.5);
      builder.LineTo(0.25, 0);
      builder.LineTo(0.75, 0);
      builder.LineTo(1, 0.5);
      builder.LineTo(0.75, 1);
      builder.LineTo(0.25, 1);
      builder.Close();
      result = builder.GetPathIcon();

      if (hasDefaultColors) {
        conversations[type] = result;
      }

      return result;
    }

    private static IIcon conversationSubState;

    public static IIcon CreateConversationMarker(ConversationType type, Brush brush) {
      var hasDefaultColor = Equals(brush, BpmnConstants.DefaultIconColor);

      if (hasDefaultColor && conversationSubState != null &&
          (type == ConversationType.SubConversation || type == ConversationType.CallingCollaboration)) {
        return conversationSubState;
      }

      switch (type) {
        case ConversationType.SubConversation:
        case ConversationType.CallingCollaboration:
          var icon = CreateStaticSubState(SubState.Collapsed, brush);
          if (hasDefaultColor) {
            conversationSubState = icon;
          }
          return icon;
        default:
          return null;
      }
    }

    private static IIcon dataObject;

    public static IIcon CreateDataObject(Brush background, Brush outline) {
      var hasDefaultColors = Equals(background, BpmnConstants.DataObjectDefaultBackground) && Equals(outline, BpmnConstants.DataObjectDefaultOutline);

      if (hasDefaultColors && dataObject != null) {
        return dataObject;
      }

      var icon = new DataObjectIcon { Pen = (Pen) new Pen(outline, 1).GetAsFrozen(), Brush = background };

      if (hasDefaultColors) {
        dataObject = icon;
      }

      return icon;
    }

    private static IIcon dataObjectInputType;
    private static IIcon dataObjectOutputType;

    public static IIcon CreateDataObjectType(DataObjectType type, Brush brush) {
      var hasDefaultColor = Equals(brush, BpmnConstants.DefaultIconColor);

      if (hasDefaultColor) {
        if (type == DataObjectType.Input && dataObjectInputType != null) {
          return dataObjectInputType;
        }
        if (type == DataObjectType.Output && dataObjectOutputType != null) {
          return dataObjectOutputType;
        }
      }

      IIcon icon;
      switch (type) {
        case DataObjectType.Input:
          icon = CreateEventType(EventType.Link, false, brush, Brushes.Transparent);
          if (hasDefaultColor) {
            dataObjectInputType = icon;
          }
          return icon;
        case DataObjectType.Output:
          icon = CreateEventType(EventType.Link, true, brush, brush);
          if (hasDefaultColor) {
            dataObjectOutputType = icon;
          }
          return icon;
        case DataObjectType.None:
        default:
          return null;
      }
    }

    private static IIcon leftAnnotation;
    private static IIcon rightAnnotation;

    public static IIcon CreateAnnotation(bool left, Brush background, Brush outline) {
      var hasDefaultColors = Equals(background, BpmnConstants.AnnotationDefaultBackground) &&
                             Equals(outline, BpmnConstants.AnnotationDefaultOutline);

      if (hasDefaultColors && left && leftAnnotation != null) {
        return leftAnnotation;
      }
      if (hasDefaultColors && !left && rightAnnotation != null) {
        return rightAnnotation;
      }

      var pen = (Pen) new Pen(outline, 1).GetAsFrozen();

      var icons = new List<IIcon>();
      builder.Pen = null;
      builder.Brush = background;
      icons.Add(builder.CreateRectIcon(0));
      builder.Pen = pen;
      if (left) {
        builder.MoveTo(0.1, 0);
        builder.LineTo(0, 0);
        builder.LineTo(0, 1);
        builder.LineTo(0.1, 1);
      } else {
        builder.MoveTo(0.9, 0);
        builder.LineTo(1, 0);
        builder.LineTo(1, 1);
        builder.LineTo(0.9, 1);
      }
      icons.Add(builder.GetPathIcon());
      var icon = builder.CombineIcons(icons);
      if (hasDefaultColors) {
        if (left) {
          leftAnnotation = icon;
        } else {
          rightAnnotation = icon;
        }
      }
      return icon;
    }

    private static IIcon dataStore;

    public static IIcon CreateDataStore(Brush background, Brush outline) {
      var hasDefaultColors = Equals(background, BpmnConstants.DataStoreDefaultBackground) &&
                             Equals(outline, BpmnConstants.DataStoreDefaultOutline);

      if (hasDefaultColors && dataStore != null) {
        return dataStore;
      }

      var pen = (Pen) new Pen(outline, 1).GetAsFrozen();

      const double halfEllipseHeight = 0.125;
      const double ringOffset = 0.07;

      var icons = new List<IIcon>();
      builder.Pen = pen;
      builder.Brush = background;

      builder.MoveTo(0, halfEllipseHeight);
      builder.LineTo(0, 1 - halfEllipseHeight);
      builder.CubicTo(0, 1, 1, 1, 1, 1 - halfEllipseHeight);
      builder.LineTo(1, halfEllipseHeight);
      builder.CubicTo(1, 0, 0, 0, 0, halfEllipseHeight);
      builder.Close();
      icons.Add(builder.GetPathIcon());

      builder.Pen = pen;
      double ellipseCenterY = halfEllipseHeight;
      for (int i = 0; i < 3; i++) {
        builder.MoveTo(0, ellipseCenterY);
        builder.CubicTo(0, ellipseCenterY + halfEllipseHeight, 1, ellipseCenterY + halfEllipseHeight, 1,
            ellipseCenterY);
        ellipseCenterY += ringOffset;
      }
      icons.Add(builder.GetPathIcon());

      var icon = builder.CombineIcons(icons);

      if (hasDefaultColors) {
        dataStore = icon;
      }

      return icon;
    }

    private static readonly Dictionary<ArrowType, IIcon> arrows = new Dictionary<ArrowType, IIcon>(8); 

    public static IIcon CreateArrowIcon(ArrowType type, Brush brush) {
      var hasDefaultColor = Equals(brush, BpmnConstants.DefaultIconColor);

      IIcon result;
      if (hasDefaultColor && arrows.TryGetValue(type, out result)) {
        return result;
      }

      builder.Pen = (Pen) new Pen {
          Brush = brush,
          StartLineCap = PenLineCap.Round,
          EndLineCap = PenLineCap.Round,
          LineJoin = PenLineJoin.Round
      }.GetAsFrozen();
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
          builder.Brush = brush;
          builder.MoveTo(0, 0);
          builder.LineTo(1, 0.5);
          builder.LineTo(0, 1);
          builder.Close();
          result = builder.GetPathIcon();
          break;
      }
      if (hasDefaultColor) {
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
