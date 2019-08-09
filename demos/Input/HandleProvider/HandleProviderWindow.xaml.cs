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

using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.Input.HandleProvider
{
  /// <summary>
  /// Shows how to achieve interactive resize behavior for labels
  /// by implementing a custom <see cref="IHandleProvider"/> and <see cref="IHandle"/>.
  /// </summary>
  /// <remarks>
  /// See the XAML or run the application to find out about what this application demonstrates.
  /// </remarks>
  public partial class HandleProviderWindow
  {
    
    #region Initialization

    public HandleProviderWindow() {
      InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      IGraph graph = graphControl.Graph;

      // add a custom handle provider for labels
      graph.GetDecorator().LabelDecorator.HandleProviderDecorator.SetFactory(GetLabelHandleProvider);
      // for rotatable labels: modify the selection visualization to indicate that this label can be rotated
      var template = graphControl.TryFindResource(LabelRotateHandle.LabelSelectionTemplateKey) as DataTemplate;
      graph.GetDecorator().LabelDecorator.SelectionDecorator.SetImplementationWrapper((label, wrapped) => {
        if (label.LayoutParameter.Model is FreeNodeLabelModel 
          || label.LayoutParameter.Model is FreeEdgeLabelModel
          || label.LayoutParameter.Model is FreeLabelModel) {
          return new LabelSelectionIndicatorInstaller {Template = template};
        }
        return wrapped;
      });

      InitializeGraphDefaults(graph);
      InitializeInputMode();
      CreateSampleGraph(graph);

      graphControl.FitGraphBounds();
    }

    private IHandleProvider GetLabelHandleProvider(ILabel label) {
      return new LabelHandleProvider(label);
    }

    private void InitializeGraphDefaults(IGraph graph) {
      graph.NodeDefaults.Style = new ShinyPlateNodeStyle {Brush = Brushes.Orange };
      graph.NodeDefaults.Size = new SizeD(100, 50);

      // create a label style that shows the labels bounds
      DefaultLabelStyle labelStyle = new DefaultLabelStyle {
        BackgroundPen = Pens.LightGray,
        BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x77, 0xCC, 0xFF, 0xFF)),
        TextAlignment = TextAlignment.Center
      };
      graph.NodeDefaults.Labels.Style = labelStyle;
      //Our resize logic does not work together with all label models resp. label model parameters
      //for simplicity, we just use a centered label for nodes
      graph.NodeDefaults.Labels.LayoutParameter =
        new GenericLabelModel(InteriorLabelModel.Center).CreateDefaultParameter();
      graph.EdgeDefaults.Labels.Style = labelStyle;

      var labelModel = new EdgeSegmentLabelModel { Distance = 10 };
      graph.EdgeDefaults.Labels.LayoutParameter = labelModel.CreateParameterFromSource(0, 0.5, EdgeSides.RightOfEdge);

      graph.SetUndoEngineEnabled(true);
    }

    private void InitializeInputMode() {
      GraphEditorInputMode mode = new GraphEditorInputMode();

      // add a label to each created node
      mode.NodeCreated += (sender, args) => {
        var graph = mode.GraphControl.Graph;
        graph.AddLabel(args.Item, "Node " + graph.Nodes.Count);
      };
      // customize hit test order to simplify click selecting labels
      mode.ClickHitTestOrder = new[]
                                 {
                                   GraphItemTypes.EdgeLabel, GraphItemTypes.NodeLabel, GraphItemTypes.Bend,
                                   GraphItemTypes.Edge, GraphItemTypes.Node, GraphItemTypes.Port, GraphItemTypes.All
                                 };
      graphControl.InputMode = mode;
    }

    #endregion

    #region Sample Graph Creation

    private static void CreateSampleGraph(IGraph graph) {
      INode n1 = graph.CreateNode(new PointD(100, 100));
      INode n2 = graph.CreateNode(new PointD(500, 0));
      graph.AddLabel(n1, "Centered Node Label. Resizes symmetrically.");
      var label2 = graph.AddLabel(n2, "Free Node Label.\nSupports rotation and asymmetric resizing");
      var label2Layout = label2.GetLayout();
      graph.SetLabelLayoutParameter(label2,
                                   new FreeNodeLabelModel().CreateParameter(new PointD(0.5, 0.5), new PointD(-label2Layout.Width/2, -label2Layout.Height/2), PointD.Origin, PointD.Origin, 0));

      var edge = graph.CreateEdge(n2, n1);
      graph.AddLabel(edge, "Rotated Edge Label");
    }

    #endregion
  }

  class LabelSelectionIndicatorInstaller : OrientedRectangleIndicatorInstaller {
    protected override IOrientedRectangle GetRectangle(object item) {
      return ((ILabel) item).GetLayout();
    }
  }
}
