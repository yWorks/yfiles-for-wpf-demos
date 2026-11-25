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
using System.Text.Json;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.RectangularIndicator
{
  /// <summary>
  ///   This demo shows how to apply a rectangular indicator to a graph.
  /// </summary>
  [Demo("rectangular-indicator", "application-features", "Rectangular Indicator",
      Summary = "Shows how to add an interactive rectangular indicator to the graph control.",
      Tags = ["interaction", "selection"], Keywords = ["PositionHandler", "RectangleHandle", "handles"])]
  public sealed partial class RectangularIndicatorDemo
  {
    public RectangularIndicatorDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   region that will be exported
    /// </summary>
    private MutableRectangle exportRect = null!;

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // create the input Mode and the rectangular indicator
      InitializeInputModes();

      // configures default styles for newly created graph elements
      InitializeGraph(graphControl.Graph);

      // build the graph from the given data set
      BuildGraph(graphControl.Graph);

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json);
    }

    /// <summary>
    ///   Creates the input Mode and the rectangular indicator.
    /// </summary>
    private void InitializeInputModes() {
      // Create a GraphEditorInputMode instance
      var editMode = new GraphEditorInputMode();
      // and install the edit mode into the canvas.
      graphControl.InputMode = editMode;

      // create the model for the export rectangle
      exportRect = new MutableRectangle(-30, -30, 240, 240);

      // ... visualize it in the canvas, ...
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.HighlightGroup, exportRect,
          new RectangleRenderer());

      AddExportRectInputModes(editMode);
    }

    /// <summary>
    ///   Adds the view modes that handle the resizing and movement of the export rectangle.
    /// </summary>
    private void AddExportRectInputModes(GraphInputMode inputMode) {
      // create a mode that deals with the handles
      var exportHandleInputMode = new HandleInputMode { Priority = 1 };
      // add it to the graph editor mode
      inputMode.Add(exportHandleInputMode);

      // now the handles
      exportHandleInputMode.AddHandle(new RectangleHandle(HandlePositions.TopRight, exportRect));
      exportHandleInputMode.AddHandle(new RectangleHandle(HandlePositions.TopLeft, exportRect));
      exportHandleInputMode.AddHandle(new RectangleHandle(HandlePositions.BottomRight, exportRect));
      exportHandleInputMode.AddHandle(new RectangleHandle(HandlePositions.BottomLeft, exportRect));

      // create a mode that allows for dragging the export rectangle at the sides
      var moveInputMode = new MoveInputMode {
          PositionHandler = new PositionHandler(exportRect),
          HitTestable = HitTestables.Create((context, location) => {
            var path = new GeneralPath(5);
            path.AppendRectangle(exportRect, false);
            return path.PathContains(location, context.HitTestRadius + 3 / context.Zoom);
          }),
          Priority = 41
      };

      // add it to the edit mode
      inputMode.Add(moveInputMode);
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph);

      var groupNodeStyle = (GroupNodeStyle) graph.GroupNodeDefaults.Style;
      groupNodeStyle.TabPosition = GroupNodeStyleTabPosition.Left;

      // set sizes and locations specific for this demo
      graph.NodeDefaults.Size = new SizeD(40, 40);

      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(sideOfEdge: EdgeSides.BelowEdge);
    }

    [STAThread]
    public static void Main() {
      Run<RectangularIndicatorDemo>();
    }
  }
}
