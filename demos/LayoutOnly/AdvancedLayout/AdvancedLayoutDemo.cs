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
using Demo.Layout.LayoutGraphViewer;
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Labeling;

namespace Demo.Layout.AdvancedLayout
{
  /// <summary>
  /// This class shows how to use layout and labeling algorithms. This demo
  /// displays the calculated coordinates in a simple graph viewer.
  /// Additionally it outputs the calculated coordinates of the graph layout to 
  /// the console.
  /// In this demo HierarchicLayout is used to layout a small graph. 
  /// First the edge labels of the graph will be laid out using a general labeling 
  /// approach that effectively positions the labels after the node and edge positions
  /// have already been fixed.
  /// Second, a special edge labeling mechanism will be used that is currently only 
  /// available in conjunction with HierarchicLayout. While laying out the graph the 
  /// edge labels will be considered as well. Therefore the node and edge positions 
  /// can be chosen in such a way that, the labeling does not
  /// introduce overlaps between labels and other entities in the graph.
  /// </summary>
  public class AdvancedLayoutDemo
  {
    [STAThread]
    public static void Main() {
      DefaultLayoutGraph graph = new DefaultLayoutGraph();

      //construct graph. assign sizes to nodes
      Node v1 = graph.CreateNode();
      graph.SetSize(v1, 30, 30);
      Node v2 = graph.CreateNode();
      graph.SetSize(v2, 30, 30);
      Node v3 = graph.CreateNode();
      graph.SetSize(v3, 30, 30);

      Edge e1 = graph.CreateEdge(v1, v2);
      Edge e2 = graph.CreateEdge(v2, v3);
      Edge e3 = graph.CreateEdge(v1, v3);

      //optionally setup some port constraints for HierarchicLayout
      IEdgeMap spc = graph.CreateEdgeMap();
      IEdgeMap tpc = graph.CreateEdgeMap();
      //e1 shall leave and enter the node on the right side
      spc.Set(e1, PortConstraint.Create(PortSide.East));
      //additionally set a strong port constraint on the target side. 
      tpc.Set(e1, PortConstraint.Create(PortSide.East, true));
      //ports with strong port constraints will not be reset by the 
      //layout algorithm.  So we specify the target port right now to connect 
      //to the upper left corner of the node 
      graph.SetTargetPointRel(e1, new YPoint(15, -15));

      //e2 shall leave and enter the node on the top side
      spc.Set(e2, PortConstraint.Create(PortSide.North));
      tpc.Set(e2, PortConstraint.Create(PortSide.North));
      //e3 uses no port constraints, i.e. layout will choose best side
      graph.AddDataProvider(PortConstraintKeys.SourcePortConstraintDpKey, spc);
      graph.AddDataProvider(PortConstraintKeys.TargetPortConstraintDpKey, tpc);

      //setup two edge labels for edge e1. The size of the edge labels will be set to
      //80x20. Usually the size of the labels will be determined by
      //calculaing the bounding box of a piece text that is displayed
      //with a specific font.

      var labelFactory = LayoutGraphUtilities.GetLabelFactory(graph);
      graph.SetLabelLayout(e1, new[]
        {
          CreateEdgeLabelLayout(labelFactory, e1,
              new SliderEdgeLabelLayoutModel(SliderMode.Center),
              PreferredPlacementDescriptor.NewSharedInstance(LabelPlacements.AtCenter)),
          CreateEdgeLabelLayout(labelFactory, e1,
              new SliderEdgeLabelLayoutModel(SliderMode.Side),
              PreferredPlacementDescriptor.NewSharedInstance(LabelPlacements.LeftOfEdge))
        });

      var layout = new HierarchicLayout();
      layout.LabelingEnabled = true;
      layout.Labeling = new GenericLabeling();

      new BufferedLayout(layout).ApplyLayout(graph);

      Console.WriteLine("\n\nGRAPH LAID OUT USING GENERIC EDGE LABELING");
      Console.WriteLine("v1 center position = " + graph.GetCenter(v1));
      Console.WriteLine("v2 center position = " + graph.GetCenter(v2));
      Console.WriteLine("v3 center position = " + graph.GetCenter(v3));
      Console.WriteLine("e1 path = " + graph.GetPath(e1));
      Console.WriteLine("e2 path = " + graph.GetPath(e2));
      Console.WriteLine("e3 path = " + graph.GetPath(e3));
      Console.WriteLine("ell1 upper left location = " + GetEdgeLabelLocation(graph, e1, CreateEdgeLabelLayout(labelFactory, e1,
          new SliderEdgeLabelLayoutModel(SliderMode.Center),
          PreferredPlacementDescriptor.NewSharedInstance(LabelPlacements.AtCenter))));
      Console.WriteLine("ell2 upper left location = " + GetEdgeLabelLocation(graph, e1, graph.GetLabelLayout(e1)[1]));

      GraphViewer gv = new GraphViewer();
      gv.AddLayoutGraph(new CopiedLayoutGraph(graph), "Layout with Generic Labeling");

      var freeModel = new FreeEdgeLabelLayoutModel();
      graph.SetLabelLayout(e1, new[]
        {
          CreateEdgeLabelLayout(labelFactory, e1, freeModel,
              PreferredPlacementDescriptor.NewSharedInstance(LabelPlacements.AtCenter)),
          CreateEdgeLabelLayout(labelFactory, e1, freeModel,
              PreferredPlacementDescriptor.NewSharedInstance(LabelPlacements.LeftOfEdge))
        });

      layout.LabelingEnabled = true;
      layout.Labeling = new LabelLayoutTranslator();

      new BufferedLayout(layout).ApplyLayout(graph);

      Console.WriteLine("\n\nGRAPH LAID OUT USING HIERACHIC LAYOUT SPECIFIC EDGE LABELING");
      Console.WriteLine("v1 center position = " + graph.GetCenter(v1));
      Console.WriteLine("v2 center position = " + graph.GetCenter(v2));
      Console.WriteLine("v3 center position = " + graph.GetCenter(v3));
      Console.WriteLine("e1 path = " + graph.GetPath(e1));
      Console.WriteLine("e2 path = " + graph.GetPath(e2));
      Console.WriteLine("e3 path = " + graph.GetPath(e3));
      Console.WriteLine("ell1 upper left location = " + GetEdgeLabelLocation(graph, e1, CreateEdgeLabelLayout(labelFactory, e1,
          new SliderEdgeLabelLayoutModel(SliderMode.Center),
          PreferredPlacementDescriptor.NewSharedInstance(LabelPlacements.AtCenter))));
      Console.WriteLine("ell2 upper left location = " + GetEdgeLabelLocation(graph, e1, graph.GetLabelLayout(e1)[1]));

      //display the result in a simple viewer
      gv.AddLayoutGraph(graph, "Layout with Integrated Labeling");
      var application = new System.Windows.Application();
      application.Run(gv);
    }

    private static IEdgeLabelLayout CreateEdgeLabelLayout(ILabelLayoutFactory labelFactory, Edge edge,
        IEdgeLabelLayoutModel edgeLabelModel, PreferredPlacementDescriptor preferredPlacementDescriptor) {
      var label = labelFactory.CreateLabelLayout(edge, new YOrientedRectangle(0, 0, 80, 20), edgeLabelModel,
          preferredPlacementDescriptor);
      label.ModelParameter = edgeLabelModel.DefaultParameter;
      return label;
    }

    /// <summary>
    /// Returns the calculated location of the edge label. Note that the labeling
    /// machinery returns the edge labels positions as a parameter
    /// of the model that belongs to the label. This model parameter can be used
    /// to retrieve the actual location of the label as shown in this method.
    /// </summary> 
    private static YPoint GetEdgeLabelLocation(LayoutGraph graph, Edge e, IEdgeLabelLayout ell) {
      var placement = ell.LabelModel.GetLabelPlacement(
        ell.BoundingBox,
        graph.GetLayout(e),
        graph.GetLayout(e.Source),
        graph.GetLayout(e.Target),
        ell.ModelParameter);
      YPoint ellp = new YPoint(placement.Anchor.X, placement.Anchor.Y - placement.Height);
      return ellp;
    }
  }
}
