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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Layout.LogicGate
{
  /// <summary>
  /// Convert from a node style to an image that shows the visual representation of the style
  /// </summary>
  [ValueConversion(typeof(INode), typeof(DrawingImage))]
  public class NodeImageConverter : IValueConverter
  {
    private readonly GraphControl graphControl = new GraphControl();
    private readonly SimpleNode dummyNode;

    public NodeImageConverter() {
      dummyNode = new SimpleNode { Layout = new RectD(0, 0, 50, 30) };
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      INode listBoxNode = value as INode;
      if (listBoxNode != null) {
        graphControl.Graph.Clear();

        var node = graphControl.Graph.CreateNode(listBoxNode.Layout.ToRectD(), listBoxNode.Style, listBoxNode.Tag);
        var size = node.Layout.GetSize();
        foreach (var label in listBoxNode.Labels) {
          graphControl.Graph.AddLabel(node, label.Text, label.LayoutParameter, label.Style, label.PreferredSize, label.Tag);
        }
        graphControl.FitGraphBounds();
        ContextConfigurator cc = new ContextConfigurator(graphControl.ContentRect);
        
        var renderContext = cc.CreateRenderContext(graphControl);
        Transform transform = cc.CreateWorldToIntermediateTransform();
        System.Windows.Media.Geometry clip = cc.CreateClip();

        var visualContent = graphControl.ExportContent(renderContext);
        VisualGroup container = new VisualGroup() { Transform = transform, Clip = clip, Children = { visualContent } };
        VisualBrush brush = new VisualBrush(container);
        return new DrawingImage(new GeometryDrawing(brush, null, new RectangleGeometry(new Rect(0, 0, size.Width, size.Height))));
      } else {
        return null;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
