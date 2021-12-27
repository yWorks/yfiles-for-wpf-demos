/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
    }

    private void GraphBuilderWindow_OnLoaded(object sender, RoutedEventArgs e) {
      // create the graph
      var dataProvider = Resources["Staff"] as XmlDataProvider;
      var b = CreateOrganizationBuilder(dataProvider);
      var newGraph = b.Graph;

      // add some insets to group nodes
      newGraph.GetDecorator().NodeDecorator.InsetsProviderDecorator.SetImplementation(newGraph.IsGroupNode, new GroupNodeInsetsProvider());

      graphControl.Graph = newGraph;

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
      var graphBuilder = new yWorks.Graph.DataBinding.GraphBuilder();

      // configure tne nodes source to use the employees enumerable
      var nodesSource = graphBuilder.CreateNodesSource(employees);
      // group by business units
      nodesSource.ParentIdProvider = employee => employee.GetAttribute("businessUnit");
      var nodeBrush = new LinearGradientBrush(Color.FromRgb(255,165,0), Color.FromRgb(255,237,204), new Point(0, 0), new Point(0, 1));
      // choose the node size so that the labels fit
      nodesSource.NodeCreator.LayoutProvider = element => {
        var width = 7 * Math.Max(element.GetAttribute("name").Length, element.GetAttribute("position").Length);
        return new RectD(0, 0, width, 40);
      };
      nodesSource.NodeCreator.Defaults.Style = new ShapeNodeStyle {
          Pen = Pens.DarkOrange,
          Brush = nodeBrush,
          Shape = ShapeNodeShape.RoundRectangle
      };
      // take the name attribute as node name
      var nodeNameLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.GetAttribute("name"));
      nodeNameLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(0, 0, 0, 10)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      var nodePositionLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.GetAttribute("position"));
      nodePositionLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(0, 10, 0, -5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      
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
      var groupNodeBrush = new LinearGradientBrush(Color.FromRgb(225,242,253), Colors.LightSkyBlue, new Point(0.5, 0), new Point(0.5, 1)) { Opacity = 0.5 };
      groupNodesSource.NodeCreator.Defaults.Style = new ShapeNodeStyle() {
          Pen = Pens.LightSkyBlue,
          Brush = groupNodeBrush
      };
      var groupLabels = groupNodesSource.NodeCreator.CreateLabelBinding(element => element.GetAttribute("name"));
      groupLabels.Defaults.Style = new DefaultLabelStyle() {
          TextBrush = Brushes.DarkGray,
          TextSize = 24,
      };
      groupLabels.Defaults.LayoutParameter = InteriorLabelModel.NorthWest;

      // create the edges from an element's parent XML node to the element itself
      var edgesSource = graphBuilder.CreateEdgesSource(employees, element => element.ParentNode, element => element);
      edgesSource.EdgeCreator.Defaults.Style = new PolylineEdgeStyle() {SmoothingLength = 20};
      var edgeLabels = edgesSource.EdgeCreator.CreateLabelBinding(element => element.GetAttribute("position"));
      edgeLabels.Defaults.Style = new DefaultLabelStyle() {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(225,242,253)),
          BackgroundPen = Pens.LightSkyBlue,
          Insets = new InsetsD(2),
          TextSize = 8
      };
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
      var graphBuilder = new yWorks.Graph.DataBinding.GraphBuilder();

      // create the nodes source for the employees
      var nodesSource = graphBuilder.CreateNodesSource(employeeDict, pair => pair.Key);
      nodesSource.ParentIdProvider = employee => employee.Value.GetAttribute("businessUnit");
      var nodeBrush = new LinearGradientBrush(Color.FromRgb(255,165,0), Color.FromRgb(255,237,204), new Point(0, 0), new Point(0, 1));
      nodesSource.NodeCreator.LayoutProvider = pair => {
        var element = pair.Value;
        var width = 7 * Math.Max(element.GetAttribute("name").Length, element.GetAttribute("position").Length);
        return new RectD(0, 0, width, 40);
      };
      nodesSource.NodeCreator.Defaults.ShareStyleInstance = false;
      nodesSource.NodeCreator.Defaults.Style = new ShapeNodeStyle() {
          Pen = Pens.DarkOrange,
          Brush = nodeBrush,
          Shape = ShapeNodeShape.RoundRectangle
      };
      nodesSource.NodeCreator.StyleBindings.AddBinding("Pen", pair => {
        var position = pair.Value.GetAttribute("position");
        if (position.Contains("Chief")) {
          return Pens.DarkRed;
        }
        return Pens.DarkOrange;
      });
      nodesSource.NodeCreator.StyleBindings.AddBinding("Shape", pair => {
        var position = pair.Value.GetAttribute("position");
        if (position.Contains("Chief")) {
          return ShapeNodeShape.Rectangle;
        }
        return ShapeNodeShape.RoundRectangle;
      });
      
      var nodeNameLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.Key);
      nodeNameLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(0, 0, 0, 10)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      var nodePositionLabels = nodesSource.NodeCreator.CreateLabelBinding(element => element.Value.GetAttribute("position"));
      nodePositionLabels.Defaults.LayoutParameter = new InteriorStretchLabelModel() {Insets = new InsetsD(0, 10, 0, -5)}.CreateParameter(InteriorStretchLabelModel.Position.Center);
      
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
      var groupNodeBrush = new LinearGradientBrush(Color.FromRgb(225,242,253), Colors.LightSkyBlue, new Point(0.5, 0), new Point(0.5, 1)) { Opacity = 0.5 };
      groupNodesSource.NodeCreator.Defaults.Style = new ShapeNodeStyle() {
          Pen = Pens.LightSkyBlue,
          Brush = groupNodeBrush
      };
      var groupLabels = groupNodesSource.NodeCreator.CreateLabelBinding(pair => pair.Value.GetAttribute("name"));
      groupLabels.Defaults.Style = new DefaultLabelStyle() {
          TextBrush = Brushes.DarkGray,
          TextSize = 24,
      };
      groupLabels.Defaults.LayoutParameter = InteriorLabelModel.NorthWest;

      // configure the edges from an employee's XML parent to the employee himself
      var edgesSource = graphBuilder.CreateEdgesSource(employeeDict, pair => ((XmlElement) pair.Value.ParentNode).GetAttribute("position"), pair => pair.Key);
      edgesSource.EdgeCreator.Defaults.Style = new PolylineEdgeStyle() {SmoothingLength = 20};
      var edgeLabels = edgesSource.EdgeCreator.CreateLabelBinding(pair => pair.Key);
      edgeLabels.Defaults.Style = new DefaultLabelStyle() {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(225,242,253)),
          BackgroundPen = Pens.LightSkyBlue,
          Insets = new InsetsD(2),
          TextSize = 8
      };
      edgeLabels.Defaults.LayoutParameter = new EdgePathLabelModel() { AutoRotation = false}.CreateDefaultParameter();

      graphBuilder.BuildGraph();
      return graphBuilder;
    }*/
  }
    #endregion
    sealed class GroupNodeInsetsProvider : INodeInsetsProvider {
    public InsetsD GetInsets(INode node) {
      return new InsetsD(5, 20, 5, 5);
    }
  }
}
