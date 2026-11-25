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

using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.DragAndDrop
{
  /// <summary>
  ///   Initializes the drag and drop panel.
  /// </summary>
  internal static class DragAndDrop
  {
    /// <summary>
    ///   Initializes the drag and drop panel.
    /// </summary>
    public static void InitializeDnDPanel(ContentControl panel, ImageSource yFilesLogo) {
      var dragAndDropPanel = new DragAndDropPanel(panel) { MaxItemWidth = 160 };
      dragAndDropPanel.PopulatePanel(CreateDnDPanelItems(yFilesLogo));
    }

    /// <summary>
    ///   Creates the items that provide the visualizations for the drag and drop panel.
    /// </summary>
    private static IEnumerable<DragAndDropPanelItem> CreateDnDPanelItems(ImageSource yFilesLogo) {
      var itemContainer = new List<DragAndDropPanelItem>();

      // Create some nodes
      var groupNodeStyle = DemoStyles.CreateDemoGroupStyle();

      // A label model with insets for the expand/collapse button
      var groupLabelStyle = new LabelStyle { TextBrush = Brushes.White };

      var groupNode = new SimpleNode {
          Layout = new RectD(0, 0, 80, 80),
          Style = groupNodeStyle
      };

      var groupLabel = new SimpleLabel(
          groupNode, "Group Node",
          new GroupNodeLabelModel().CreateTabBackgroundParameter()) { Style = groupLabelStyle };
      groupNode.Labels = [groupLabel];
      itemContainer.Add(new DragAndDropPanelItem(groupNode, "Group Node"));

      var demoStyleNode = new SimpleNode {
          Layout = new RectD(0, 0, 60, 40),
          Style = DemoStyles.CreateDemoNodeStyle()
      };
      itemContainer.Add(new DragAndDropPanelItem(demoStyleNode, "Demo Node"));

      var shapeStyleNode = new SimpleNode {
          Layout = new RectD(0, 0, 60, 40),
          Style = new ShapeNodeStyle {
              Shape = ShapeNodeShape.RoundRectangle,
              Pen = new Pen(Brushes.DarkOrange, 1),
              Brush = Brushes.DarkOrange
          }
      };
      itemContainer.Add(new DragAndDropPanelItem(shapeStyleNode, "Shape Node"));

      var arrowNode = new SimpleNode {
          Layout = new RectD(0, 0, 60, 40),
          Style = new ArrowNodeStyle { Brush = Brushes.DarkOrange }
      };
      itemContainer.Add(new DragAndDropPanelItem(arrowNode, "Arrow Node"));

      var imageStyleNode = new SimpleNode {
          Layout = new RectD(0, 0, 60, 60),
          Style = new ImageNodeStyle(yFilesLogo) { KeepAspectRatio = true }
      };
      itemContainer.Add(new DragAndDropPanelItem(imageStyleNode, "Image Node"));

      var port = new SimplePort(demoStyleNode, FreeNodePortLocationModel.Center) {
          Style = new ShapePortStyle {
              Brush = Brushes.LightGray,
              Pen = new Pen(Brushes.Gray, 1),
              Shape = ShapeNodeShape.Ellipse
          }
      };
      itemContainer.Add(new DragAndDropPanelItem(port, "Port"));

      var nodeLabelStyle = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xff, 0xc4, 0x99)),
          TextBrush = new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)),
          Padding = new InsetsD(5),
          Shape = LabelShape.RoundRectangle
      };

      var nodeLabel = new SimpleLabel(demoStyleNode, "Node Label", FreeNodeLabelModel.Center) {
          Style = nodeLabelStyle
      };
      nodeLabel.AdoptPreferredSizeFromStyle();
      itemContainer.Add(new DragAndDropPanelItem(nodeLabel, "Node Label"));

      var edge0 = new SimpleEdge(port, port) { Style = VoidEdgeStyle.Instance };

      var edgeLabelStyle = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xc2, 0xaa, 0x99)),
          TextBrush = new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)),
          Padding = new InsetsD(5),
          Shape = LabelShape.RoundRectangle
      };

      var edgeLabel =
          new SimpleLabel(edge0, "Edge Label",
              FreeEdgeLabelModel.Instance.CreateParameter()) { Style = edgeLabelStyle };
      edgeLabel.AdoptPreferredSizeFromStyle();
      itemContainer.Add(new DragAndDropPanelItem(edgeLabel, "Edge Label"));

      var portLabelStyle = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xd5, 0xcc)),
          TextBrush = new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)),
          Padding = new InsetsD(5),
          Shape = LabelShape.RoundRectangle
      };

      var portLabel = new SimpleLabel(port, "Port Label", FreePortLabelModel.Center) { Style = portLabelStyle };
      portLabel.AdoptPreferredSizeFromStyle();
      itemContainer.Add(new DragAndDropPanelItem(portLabel, "Port Label"));

      var edge1 = new SimpleEdge {
          Style = new PolylineEdgeStyle {
              SmoothingLength = 100,
              TargetArrow = new Arrow(ArrowType.Triangle, Brushes.Gray),
              Pen = CreateDashedGrayPen()
          }
      };
      var edge2 = new SimpleEdge {
          Style = new PolylineEdgeStyle {
              SourceArrow = new Arrow(ArrowType.Triangle, Brushes.Gray),
              TargetArrow = new Arrow(ArrowType.Triangle, Brushes.Gray),
              Pen = new Pen(Brushes.Gray, 2)
          }
      };
      var edge3 = new SimpleEdge { Style = new PolylineEdgeStyle { Pen = CreateDashedGrayPen() } };

      itemContainer.Add(new DragAndDropPanelItem(edge1, "Default"));
      itemContainer.Add(new DragAndDropPanelItem(edge2, "Bidirectional"));
      itemContainer.Add(new DragAndDropPanelItem(edge3, "Dashed"));

      return itemContainer;
    }

    private static Pen CreateDashedGrayPen() => new(Brushes.Gray, 2) { DashStyle = DashStyles.Dash };
  }
}
