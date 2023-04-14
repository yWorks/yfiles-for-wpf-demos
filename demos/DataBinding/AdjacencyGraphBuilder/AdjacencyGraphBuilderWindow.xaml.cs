/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using Demo.yFiles.Toolkit;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;

namespace Demo.yFiles.DataBinding.AdjacencyGraphBuilder
{
  /// <summary>
  /// Interaction logic for AdjacencyGraphBuilderWindow.xaml
  /// </summary>
  public partial class AdjacencyGraphBuilderWindow
  {
    public AdjacencyGraphBuilderWindow() {
      InitializeComponent();
      InitializeGraphDefaults();
    }

    private void AdjacencyGraphBuilderWindow_OnLoaded(object sender, RoutedEventArgs e) {
      graphSourceComboBox.SelectedIndex = 0;
    }

    private async void AdjacencyGraphBuilderModelChanged(object sender, SelectionChangedEventArgs e) {
      BuildGraph(graphSourceComboBox.SelectedIndex);

      // Perform an animated layout of the organization chart graph when the window is loaded.
      await graphControl.MorphLayout(new HierarchicLayout {
          EdgeLayoutDescriptor = new EdgeLayoutDescriptor {MinimumLength = 50},
          LayoutOrientation =
              graphSourceComboBox.SelectedIndex <= 3 ? LayoutOrientation.TopToBottom : LayoutOrientation.BottomToTop,
          IntegratedEdgeLabeling = true //graphSourceComboBox.SelectedIndex == 1
      }, TimeSpan.FromSeconds(2));
    }

    private void BuildGraph(int index) {
      var configurationName = ((ComboBoxItem) graphSourceComboBox.Items[index]).Content as string;
      var dataProvider = ((ComboBoxItem) graphSourceComboBox.Items[index]).Tag as XmlDataProvider;

      if ("Organization with Predecessor"== configurationName) {
        BuildOrganization(dataProvider, false, false);
      } else if ("Organization with Successors" == configurationName) {
        BuildOrganization(dataProvider, true, false);
      } else if ("Organization with Predecessor Id" == configurationName) {
        BuildOrganization(dataProvider, false, true);
      } else if ("Organization with Successors Ids" == configurationName) {
        BuildOrganization(dataProvider, true, true);
      }
    }

    /// <summary>
    /// Extract Data provider from the given XML and create and configure a graph builder and build the graph.
    /// </summary>
    /// <param name="xmlDataProvider">The XML.</param>
    /// <param name="useSuccessor"></param>
    /// <param name="useIds"></param>
    private void BuildOrganization(XmlDataProvider xmlDataProvider, bool useSuccessor, bool useIds) {
      // extract employees, positions, and business units as enumerables
      var employees = xmlDataProvider.Document.DocumentElement.GetElementsByTagName("employee").Cast<XmlElement>();
      var positions = employees.Select(employee => employee.GetAttribute("position")).Distinct();
      var businessunits = xmlDataProvider.Document.DocumentElement.GetElementsByTagName("businessunit").Cast<XmlElement>();
      graphControl.Graph.Clear();
      var adjacentNodesGraphBuilder = new yWorks.Graph.DataBinding.AdjacencyGraphBuilder(graphControl.Graph);

      // first node collection: employees

      // create a nodes source which creates nodes from the given employees
      var nodesSource = adjacentNodesGraphBuilder.CreateNodesSource(employees);
      // nodes are grouped in business units
      nodesSource.ParentIdProvider = employee => employee.GetAttribute("businessUnit");
      // adjust the size so the node labels fit
      nodesSource.NodeCreator.LayoutProvider = element => {
        var width = 5 + 7 * Math.Max(element.GetAttribute("name").Length, element.GetAttribute("position").Length);
        return new RectD(0, 0, width, 40);
      };

      // set label provider
      var nodeNameLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.GetAttribute("name"));
      nodeNameLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(5, 5, 5, 5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      var nodeNameLabelStyle = DemoStyles.CreateDemoNodeLabelStyle();
      nodeNameLabelStyle.Insets = InsetsD.Empty;
      nodeNameLabelStyle.VerticalTextAlignment = VerticalAlignment.Top;
      nodeNameLabels.Defaults.Style = nodeNameLabelStyle;
      var nodePositionLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.GetAttribute("position"));
      nodePositionLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(5, 20, 5, 5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);

      // second nodes collections: positions

      // create nodes source for positions with different style and size
      var positionsSource = adjacentNodesGraphBuilder.CreateNodesSource(positions);
      positionsSource.NodeCreator.Defaults.Size = new SizeD(100, 60);
      positionsSource.NodeCreator.Defaults.Style = DemoStyles.CreateDemoNodeStyle(Themes.PaletteGreen);
      var positionLabelCreator = positionsSource.NodeCreator.CreateLabelBinding(position => position);
      positionLabelCreator.Defaults.Style = DemoStyles.CreateDemoNodeLabelStyle(Themes.PaletteGreen);
      positionLabelCreator.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);

      // group node collections: business units

      var groupNodesSource = adjacentNodesGraphBuilder.CreateGroupNodesSource(businessunits, (businessunit) => businessunit.GetAttribute("name"));
      groupNodesSource.ParentIdProvider = businessUnit => {
        var parentUnit = (businessUnit.ParentNode as XmlElement);
        if ("businessunit".CompareTo(parentUnit.Name) == 0) {
          return parentUnit.GetAttribute("name");
        }
        return null;
      };
      groupNodesSource.NodeCreator.Defaults.Size = new SizeD(50, 50);
      var groupLabels = groupNodesSource.NodeCreator.CreateLabelBinding(element => element.GetAttribute("name"));
      groupLabels.Defaults.LayoutParameter = InteriorLabelModel.NorthWest;

      // prepare edge creation
      EdgeCreator<XmlElement> edgeCreator = new EdgeCreator<XmlElement> { Defaults = graphControl.Graph.EdgeDefaults };
      var edgeLabels = edgeCreator.CreateLabelBinding(element => element.GetAttribute("name"));
      edgeLabels.Defaults = graphControl.Graph.EdgeDefaults.Labels;

      // configure the successor and predecessor sources
      // for this demo this depends on the chosen settings
      // we configure either successors or predecessors and choose whether we use IDs or the elements themselves to resolve the references
      if (useIds) {
        if (useSuccessor) {
          nodesSource.AddSuccessorIds(element => element.ChildNodes.Cast<XmlElement>(), edgeCreator);
        } else {
          nodesSource.AddPredecessorIds(element => {
            var parentElement = element.ParentNode as XmlElement;
            return parentElement != null && parentElement.Name.CompareTo("employee") == 0 ? new []{parentElement} : null;
          },edgeCreator);
        }
      } else {
        if (useSuccessor) {
          nodesSource.AddSuccessorsSource(element => element.ChildNodes.Cast<XmlElement>(), nodesSource, edgeCreator);
        } else {
          nodesSource.AddPredecessorsSource<XmlElement>(element => {
            var parentElement = element.ParentNode as XmlElement;
            return parentElement != null && parentElement.Name.CompareTo("employee") == 0 ? new []{parentElement} : null;
          }, nodesSource, edgeCreator);
        }
      }

      // either way: we create edges between the employee and his/her position
      nodesSource.AddSuccessorIds(employee => new[] { employee.GetAttribute("position") },
          new EdgeCreator<XmlElement> { Defaults = graphControl.Graph.EdgeDefaults });

      adjacentNodesGraphBuilder.BuildGraph();
    }

    private void InitializeGraphDefaults() {
      var graph = graphControl.Graph;
      
      // initialize demo styles
      DemoStyles.InitDemoStyles(graph);
      // remove insets of demo node label styles 
      ((DefaultLabelStyle) graph.NodeDefaults.Labels.Style).Insets = InsetsD.Empty;
      // set insets and bigger text size for demo group node label styles 
      ((DefaultLabelStyle) graph.GroupNodeDefaults.Labels.Style).Insets = new InsetsD(2);
      ((DefaultLabelStyle) graph.GroupNodeDefaults.Labels.Style).TextSize = 24;
      // increase tab height of GroupNodeStyle so the increased group node labels fit into the header
      ((GroupNodeStyle) graph.GroupNodeDefaults.Style).TabHeight = 28;
    }
  }
}
