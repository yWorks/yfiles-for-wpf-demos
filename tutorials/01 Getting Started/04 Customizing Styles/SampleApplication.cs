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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.PortLocationModels;


namespace Tutorial.GettingStarted
{

  /// <summary>
  /// Getting Started - 04 Customizing Styles
  /// This demo shows how to configure the visual appearance of graph elements (using so called styles).
  /// </summary>
  /// <remarks>The visual appearance for each type of graph elements (apart from edge bends) can be
  /// customized through implementations of styles. You can either
  /// set a default style through the <see cref="IGraph.NodeDefaults"/> and <see cref="IGraph.EdgeDefaults"/>
  /// properties on <see cref="IGraph"/>, 
  /// which takes effect for all <b>newly created</b> elements. Or, you can set a style for specific graph 
  /// elements at creation time with the various <c>Create/AddXXX</c> methods and extension methods, or for existing elements by calling 
  /// one of the <c>IGraph.SetStyle</c> methods.
  /// <para>yFiles WPF already comes with a number of useful styles for most graph element types.
  /// It is also possible to create custom styles, which are not covered by this tutorial.
  /// Please refer to the Custom Styles tutorial and the yFiles WPF Developer's Guide.</para>
  /// </remarks>
  public partial class SampleApplication
  {

    public void OnLoaded(object source, EventArgs args) {
      ///////////////// New in this Sample /////////////////
      
      // Configures default styles for newly created graph elements
      SetDefaultStyles();

      //////////////////////////////////////////////////////

      // Populates the graph and overrides some styles
      PopulateGraph();

      // Manages the viewport
      UpdateViewport();
    }


    #region Default style setup

    /// <summary>
    /// Sets up default styles for graph elements.
    /// </summary>
    /// <remarks>
    /// Default styles apply only to elements created after the default style has been set,
    /// so typically, you'd set these as early as possible in your application.
    /// </remarks>
    private void SetDefaultStyles() {

      ///////////////// New in this Sample /////////////////

      #region Default Node Style
      // Sets the default style for nodes
      // Creates a nice ShinyPlateNodeStyle instance, using an orange Brush.
      INodeStyle defaultNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)) };

      // Sets this style as the default for all nodes that don't have another
      // style assigned explicitly
      Graph.NodeDefaults.Style = defaultNodeStyle;

      #endregion

      #region Default Edge Style
      // Sets the default style for edges:
      // Creates an edge style that will apply a gray pen with thickness 1
      // to the entire line using PolyLineEdgeStyle,
      // which draws a polyline determined by the edge's control points (bends)
      var defaultEdgeStyle = new PolylineEdgeStyle { Pen = Pens.Gray };

      // Sets the source and target arrows on the edge style instance
      // (Actually: no source arrow)
      // Note that IEdgeStyle itself does not have these properties
      // Also note that by default there are no arrows
      defaultEdgeStyle.TargetArrow = Arrows.Default;

      // Sets the defined edge style as the default for all edges that don't have
      // another style assigned explicitly:
      Graph.EdgeDefaults.Style = defaultEdgeStyle;
      #endregion

      #region Default Label Styles
      // Sets the default style for labels
      // Creates a label style with the label text color set to dark red
      ILabelStyle defaultLabelStyle = new DefaultLabelStyle { Typeface = new Typeface("Tahoma"), TextSize = 12, TextBrush = Brushes.DarkRed };

      // Sets the defined style as the default for both edge and node labels:
      Graph.EdgeDefaults.Labels.Style = Graph.NodeDefaults.Labels.Style = defaultLabelStyle;

      #endregion

      #region Default Node size
      // Sets the default size explicitly to 40x40
      Graph.NodeDefaults.Size = new SizeD(40, 40);

      #endregion

      ///////////////////////////////////////////////////
    }

    #endregion


    /// <summary>
    /// Creates a sample graph and introduces all important graph elements present in
    /// yFiles WPF. Additionally, this method now overrides the label placement for some specific labels.
    /// </summary>
    private void PopulateGraph() {
      #region Sample Graph creation
     
      //////////// Sample node creation ///////////////////

      // Creates two nodes with the default node size
      // The location is specified for the _center_
      INode node1 = Graph.CreateNode(new PointD(50, 50));
      INode node2 = Graph.CreateNode(new PointD(150, 50));
      // Creates a third node with a different size of 80x40
      // In this case, the location of (360,380) describes the _upper left_
      // corner of the node bounds
      INode node3 = Graph.CreateNode(new RectD(360, 380, 80, 40));

      /////////////////////////////////////////////////////

      //////////// Sample edge creation ///////////////////

      // Creates some edges between the nodes
      IEdge edge1 = Graph.CreateEdge(node1, node2);
      IEdge edge2 = Graph.CreateEdge(node2, node3);

      /////////////////////////////////////////////////////

      //////////// Using Bends ////////////////////////////

      // Creates the first bend for edge2 at (400, 50)
      IBend bend1 = Graph.AddBend(edge2, new PointD(400, 50));

      /////////////////////////////////////////////////////

      //////////// Using Ports ////////////////////////////

      // Actually, edges connect "ports", not nodes directly.
      // If necessary, you can manually create ports at nodes
      // and let the edges connect to these.
      // Creates a port in the center of the node layout
      IPort port1AtNode1 = Graph.AddPort(node1, FreeNodePortLocationModel.NodeCenterAnchored);

      // Creates a port at the middle of the left border
      // Note to use absolute locations when placing ports using PointD.
      IPort port1AtNode3 = Graph.AddPort(node3, new PointD(node3.Layout.X, node3.Layout.GetCenter().Y));

      // Creates an edge that connects these specific ports
      IEdge edgeAtPorts = Graph.CreateEdge(port1AtNode1, port1AtNode3);

      /////////////////////////////////////////////////////

      //////////// Sample label creation ///////////////////

      // Adds labels to several graph elements
      Graph.AddLabel(node1, "N 1");
      Graph.AddLabel(node2, "N 2");
      Graph.AddLabel(node3, "N 3");
      var edgeLabel = Graph.AddLabel(edgeAtPorts, "Edge at Ports");

      /////////////////////////////////////////////////////
      /////////////////////////////////////////////////////

      #endregion

      ///////////////// New in this Sample /////////////////
      
      // Override default styles

      // Changes the style for the second node
      // Creates a new node style, this time a ShapeNodeStyle:
      ShapeNodeStyle sns = new ShapeNodeStyle { Shape = ShapeNodeShape.Ellipse, Pen = Pens.Black, Brush = Brushes.OrangeRed };

      // Sets the node's style property to the new style through its owning graph,
      // since you can't set the style property of an INode directly
      // (you'll set most other graph object properties this way as well)
      Graph.SetStyle(node2, sns);

      // Creates a different style for the label with black text and a red border
      // and intransparent white background
      DefaultLabelStyle sls = new DefaultLabelStyle { Typeface = new Typeface("Arial Black"), TextSize = 12, TextBrush = Brushes.Black };
      sls.BackgroundPen = Pens.Red;
      sls.BackgroundBrush = Brushes.White;

      // And sets the style for the edge label, again through its owning graph.
      Graph.SetStyle(edgeLabel, sls);
      
      // Override the style for the "Edge at Ports" edge:
      // Uses a dashed red Pen with thickness 2.
      Pen defaultPen = new Pen(Brushes.Red, 2) { DashStyle = DashStyles.Dash };
      defaultPen.Freeze();
      // Creates an edge style that will apply the new default pen
      // to the entire line using PolyLineEdgeStyle,
      // which draws a polyline determined by the edge's control points (bends)
      var edgeStyle = new PolylineEdgeStyle { Pen = defaultPen };

      // Sets the source and target arrows on the edge style instance
      // Note that IEdgeStyle itself does not have these properties
      // also note: by default the arrows have a default brush and pen
      edgeStyle.SourceArrow = Arrows.Circle;
      // set color and size to match the thick red line
      edgeStyle.TargetArrow = new Arrow(Brushes.Red.Color) { Type = ArrowType.Short, Scale = 2 };

      // Sets the defined edge style as the default for all edges that don't have
      // another style assigned explicitly:
      Graph.SetStyle(edge2, edgeStyle);

      //////////////////////////////////////////////////////
    }


    #region Viewport handling

    /// <summary>
    /// Updates the content rectangle to encompass all existing graph elements.
    /// </summary>
    /// <remarks>If you create your graph elements programmatically, the content rectangle 
    /// (i.e. the rectangle in <b>world coordinates</b>
    /// that encloses the graph) is <b>not</b> updated automatically to enclose these elements. 
    /// Typically, this manifests in wrong/missing scrollbars, incorrect <see cref="GraphOverviewControl"/> 
    /// behavior and the like.
    /// <para>
    /// This method demonstrates several ways to update the content rectangle, with or without adjusting the zoom level 
    /// to show the whole graph in the view.
    /// </para>
    /// <para>
    /// Note that updating the content rectangle only does not change the current Viewport (i.e. the world coordinate rectangle that
    /// corresponds to the currently visible area in view coordinates)
    /// </para>
    /// <para>
    /// Uncomment various combinations of lines in this method and observe the different effects.
    /// </para>
    /// <para>The following demos in this tutorial will assume that you've called <c>graphControl.FitGraphBounds();</c>
    /// in this method.</para>
    /// </remarks>
    private void UpdateViewport() {
      // Uncomment the following line to update the content rectangle 
      // to include all graph elements
      // This should result in correct scrolling behavior:

      //graphControl.UpdateContentRect();

      // Additionally, we can also set the zoom level so that the
      // content rectangle fits exactly into the viewport area:
      // Uncomment this line in addition to UpdateContentRect:
      // Note that this changes the zoom level (i.e. the graph elements will look smaller)

      //graphControl.FitContent();

      // The sequence above is equivalent to just calling:
      graphControl.FitGraphBounds();
    }

    #endregion

    #region Convenience Properties

    public IGraph Graph {
      get { return graphControl.Graph; }
    }

    #endregion

    #region Constructor
    public SampleApplication() {
      InitializeComponent();
    }
    #endregion
  }
}
