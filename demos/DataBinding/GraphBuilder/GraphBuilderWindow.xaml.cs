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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;

namespace Demo.yFiles.DataBinding.GraphBuilder
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class GraphBuilderWindow
  {
    public GraphBuilderWindow() {
      InitializeComponent();
      InitializeGraphDefaults();
    }

    private void GraphBuilderWindow_OnLoaded(object sender, RoutedEventArgs e) {
      // create the graph
      var dataProvider = Resources["Staff"] as XmlDataProvider;
      var b = CreateOrganizationBuilder(dataProvider);

      // Perform an animated layout of the organization chart graph when the window is loaded.
      graphControl.MorphLayout(new HierarchicLayout
      {
          EdgeLayoutDescriptor = new EdgeLayoutDescriptor { MinimumLength = 50 },
          LayoutOrientation = LayoutOrientation.TopToBottom,
      }, TimeSpan.FromSeconds(2));
    }

    #region graphbuilder
    /// <summary>
    /// Create a GraphBuilder which uses an enumerable as source.
    /// </summary>
    /// <param name="dataProvider">The XML to create the data from.</param>
    /// <returns>A configured graph builder</returns>
    private yWorks.Graph.DataBinding.GraphBuilder CreateOrganizationBuilder(XmlDataProvider dataProvider) {
      // extract the data into enumerables.
      var employees = dataProvider.Document.DocumentElement.GetElementsByTagName("employee").Cast<XmlElement>();
      var businessunits = dataProvider.Document.DocumentElement.GetElementsByTagName("businessunit").Cast<XmlElement>();

      // create the GraphBuilder to configure
      graphControl.Graph.Clear();
      var graphBuilder = new yWorks.Graph.DataBinding.GraphBuilder(graphControl.Graph);

      // configure tne nodes source to use the employees enumerable
      var nodesSource = graphBuilder.CreateNodesSource(employees);
      // group by business units
      nodesSource.ParentIdProvider = employee => employee.GetAttribute("businessUnit");
      // choose the node size so that the labels fit
      nodesSource.NodeCreator.LayoutProvider = element => {
        var width = 7 * Math.Max(element.GetAttribute("name").Length, element.GetAttribute("position").Length);
        return new RectD(0, 0, width, 40);
      };
      // take the name attribute as node name
      var nodeNameLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.GetAttribute("name"));
      nodeNameLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(5, 5, 5, 5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      var nodeNameLabelStyle = DemoStyles.CreateDemoNodeLabelStyle();
      nodeNameLabelStyle.Insets = InsetsD.Empty;
      nodeNameLabelStyle.VerticalTextAlignment = VerticalAlignment.Top;
      nodeNameLabels.Defaults.Style = nodeNameLabelStyle;
      var nodePositionLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.GetAttribute("position"));
      nodePositionLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(5, 20, 5, 5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      
      // create the group nodes from the business unit's enumerable
      var groupNodesSource = graphBuilder.CreateGroupNodesSource(businessunits, (businessunit) => businessunit.GetAttribute("name"));
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

      // create the edges from an element's parent XML node to the element itself
      var edgesSource = graphBuilder.CreateEdgesSource(employees, element => element.ParentNode, element => element);
      edgesSource.EdgeCreator.Defaults.Labels = graphControl.Graph.EdgeDefaults.Labels;
      var edgeLabels = edgesSource.EdgeCreator.CreateLabelBinding(element => element.GetAttribute("position"));
      edgeLabels.Defaults.LayoutParameter = new EdgePathLabelModel() { AutoRotation = false}.CreateDefaultParameter();

      graphBuilder.BuildGraph();
      return graphBuilder;
    }
    #endregion

    #region optional 
    //  The following method demonstrates how to use GraphBuilder with a Dictionary as a source
    
    /*
    /// <summary>
    /// Create a GraphBuilder which uses a dictionary as source.
    /// </summary>
    /// <param name="dataProvider">The XML to create the data from.</param>
    /// <returns>A configured graph builder</returns>
    private yWorks.Graph.DataBinding.GraphBuilder CreateOrganizationBuilderWithDictionary(XmlDataProvider dataProvider) {

      // create the employees dictionary (name to XML element)
      var employees = dataProvider.Document.DocumentElement.GetElementsByTagName("employee").Cast<XmlElement>();
      var employeeDict = new Dictionary<string, XmlElement>();
      foreach (var element in employees) {
        var name = element.GetAttribute("name");
        employeeDict[name] = element;
      }

      // create the business units dictionary (name to XML element)
      var businessunits = dataProvider.Document.DocumentElement.GetElementsByTagName("businessunit").Cast<XmlElement>();
      var unitsDict = new Dictionary<string, XmlElement>();
      foreach (var element in businessunits) {
        var name = element.GetAttribute("name");
        unitsDict[name] = element;
      }
      
      // create the GraphBuilder to configure
      graphControl.Graph.Clear();
      var graphBuilder = new yWorks.Graph.DataBinding.GraphBuilder(graphControl.Graph);

      // create the nodes source for the employees
      var nodesSource = graphBuilder.CreateNodesSource(employeeDict, pair => pair.Key);
      nodesSource.ParentIdProvider = employee => employee.Value.GetAttribute("businessUnit");
      nodesSource.NodeCreator.LayoutProvider = pair => {
        var element = pair.Value;
        var width = 7 * Math.Max(element.GetAttribute("name").Length, element.GetAttribute("position").Length);
        return new RectD(0, 0, width, 40);
      };
      nodesSource.NodeCreator.Defaults.ShareStyleInstance = false;
      nodesSource.NodeCreator.StyleBindings.AddBinding("Pen", pair => {
        var position = pair.Value.GetAttribute("position");
        if (position.Contains("Chief")) {
          return Pens.DarkRed;
        }
        return Pens.DarkOrange;
      });
      nodesSource.NodeCreator.StyleBindings.AddBinding("CornerStyle", pair => {
        var position = pair.Value.GetAttribute("position");
        if (position.Contains("Chief")) {
          return CornerStyle.Round;
        }
        return CornerStyle.Cut;
      });
      
      var nodeNameLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.Key);
      nodeNameLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(5, 5, 5, 5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      var nodeNameLabelStyle = DemoStyles.CreateDemoNodeLabelStyle();
      nodeNameLabelStyle.Insets = InsetsD.Empty;
      nodeNameLabelStyle.VerticalTextAlignment = VerticalAlignment.Top;
      nodeNameLabels.Defaults.Style = nodeNameLabelStyle;
      var nodePositionLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.Value.GetAttribute("position"));
      nodePositionLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(5, 20, 5, 5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      
      // create group nodes from the business units
      var groupNodesSource = graphBuilder.CreateGroupNodesSource(unitsDict, (nameBusinessunitPair) => nameBusinessunitPair.Key);
      groupNodesSource.ParentIdProvider = pair => {
        var parentUnit = (pair.Value.ParentNode as XmlElement);
        if ("businessunit".CompareTo(parentUnit.Name) == 0) {
          return parentUnit.GetAttribute("name");
        }
        return null;
      };
      groupNodesSource.NodeCreator.Defaults.Size = new SizeD(50, 50);
      var groupLabels = groupNodesSource.NodeCreator.CreateLabelBinding(pair => pair.Value.GetAttribute("name"));
      groupLabels.Defaults.LayoutParameter = InteriorLabelModel.NorthWest;

      // configure the edges from an employee's XML parent to the employee himself
      var edgesSource = graphBuilder.CreateEdgesSource(employeeDict, pair => ((XmlElement) pair.Value.ParentNode).GetAttribute("name"), pair => pair.Key);
      var edgeLabels = edgesSource.EdgeCreator.CreateLabelBinding(pair => pair.Key);
      edgeLabels.Defaults = graphControl.Graph.EdgeDefaults.Labels;
      edgeLabels.Defaults.LayoutParameter = new EdgePathLabelModel() { AutoRotation = false}.CreateDefaultParameter();

      graphBuilder.BuildGraph();
      return graphBuilder;
    }*/

    #endregion

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
