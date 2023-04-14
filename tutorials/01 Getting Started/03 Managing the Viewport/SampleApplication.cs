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
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;


namespace Tutorial.GettingStarted
{

  /// <summary>
  /// Getting Started - 03 Managing the Viewport
  /// This demo shows how to work with the Viewport. It introduces basic viewport handling such
  /// as zooming and "fit to content".
  /// </summary>
  public partial class SampleApplication
  {

    public void OnLoaded(object source, EventArgs args) {

      // Populates the graph and override some styles and label models
      PopulateGraph();

      ///////////////// New in this Sample /////////////////

      // Manages the viewport
      UpdateViewport();

      //////////////////////////////////////////////////////
    }


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

    /// <summary>
    /// Creates a sample graph and introduces all important graph elements present in
    /// yFiles WPF.
    /// </summary>
    private void PopulateGraph() {
      #region Sample Graph creation

      ///////////////// New in this Sample /////////////////
      
      // Creates a node outside the initial content rectangle
      INode node4 = Graph.CreateNode(new PointD(-100, -100));
      ILabel outsiderLabel = Graph.AddLabel(node4, "Outside Initial Viewport");

      //////////////////////////////////////////////////////

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
      Graph.AddLabel(edgeAtPorts, "Edge at Ports");

      /////////////////////////////////////////////////////
      /////////////////////////////////////////////////////

      #endregion
    }

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
