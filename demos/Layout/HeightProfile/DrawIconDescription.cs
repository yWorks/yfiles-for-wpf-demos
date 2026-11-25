/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.HeightProfile
{
  internal static class DrawIconDescription
  {
    /// <summary>
    ///   The visual that contains the icon associated with a node that is being hovered.
    /// </summary>
    private static IRenderTreeElement? iconDescriptionVisual;

    /// <summary>
    ///   Images used in labels and waypoint descriptions.
    /// </summary>
    private static IReadOnlyDictionary<string, ImageSource> icons = new Dictionary<string, ImageSource>();

    public static void SetIcons(IReadOnlyDictionary<string, ImageSource> imageSources) => icons = imageSources;

    /// <summary>
    ///   Adds the icon visual associated to the given node to the highlight group of the graph control.
    ///   Called when a node is being hovered.
    /// </summary>
    public static void Add(GraphControl graphControl, INode node) {
      if (WaypointData.GetWaypoint(node)?.Icon is not { } iconPath || !icons.TryGetValue(iconPath, out var image)) {
        return;
      }
      iconDescriptionVisual = graphControl.RenderTree.CreateElement(graphControl.RenderTree.HighlightGroup,
          new IconDescriptionVisual(node.Layout.ToRectD(), image));
    }

    /// <summary>
    ///   Removes the icon visual from the highlight group of the graph control.
    ///   Called whenever the highlighting of a node is being removed.
    /// </summary>
    public static void Remove(GraphControl graphControl) {
      if (iconDescriptionVisual is not null) {
        graphControl.RenderTree.Remove(iconDescriptionVisual);
        iconDescriptionVisual = null;
      }
    }

    /// <summary>
    ///   Creates a background visual that contains an icon for the given node.
    /// </summary>
    private sealed class IconDescriptionVisual(RectD nodeLayout, ImageSource image) : IVisualCreator
    {
      /// <summary>
      ///   Creates an image element with the icon associated with the current node and a line that connects
      ///   the node with the image element.
      /// </summary>
      public Visual CreateVisual(IRenderContext context) {
        var visual = new DrawingVisual();
        using (var drawingContext = visual.RenderOpen()) {
          var centerX = nodeLayout.GetCenterX();
          // create the icon and add it to the container
          const double imageWidth = 300;
          const double imageHeight = 200;
          const double imageY = -250;
          var imageX = Math.Max(5, centerX - imageWidth * 0.5);
          var imageFrame = new Rect(imageX, imageY, imageWidth, imageHeight);
          var imageScale = Math.Min(imageWidth / image.Width, imageHeight / image.Height);
          var scaledImageWidth = image.Width * imageScale;
          var scaledImageHeight = image.Height * imageScale;
          var imageRect = new Rect(imageX + (imageWidth - scaledImageWidth) * 0.5,
              imageY + (imageHeight - scaledImageHeight) * 0.5, scaledImageWidth, scaledImageHeight);
          drawingContext.DrawImage(image, imageRect);

          // create the icon border
          drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb(0x0B, 0x71, 0x89)), 2),
              imageFrame);

          // create the line that connects the node with the icon
          var linePen = new Pen(new SolidColorBrush(Color.FromRgb(0x0B, 0x71, 0x89)), 1) {
              DashStyle = DashStyles.Dash
          };
          drawingContext.DrawLine(linePen, new Point(centerX, nodeLayout.GetMaxY() + nodeLayout.Height),
              new Point(centerX, imageY));
        }
        return visual;
      }

      /// <summary>
      ///   Delegates the call to the <see cref="CreateVisual"/> method.
      /// </summary>
      public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateVisual(context);
    }
  }
}
