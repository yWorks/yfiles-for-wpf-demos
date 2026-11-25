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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.EditablePathStyle
{
  [Demo("editable-path-style", "style", "Editable Path Node Style",
      Summary = "Shows a path-based node style whose control points can be moved by users.",
      Tags = ["style", "path", "editing"], Keywords = ["handles", "general paths", "interaction", "editing"])]
  public sealed partial class EditablePathStyleDemo
  {
    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public EditablePathStyleDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      GraphControl.InputMode = CreateEditorMode();

      GraphControl.FitGraphBounds();
      Graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Sets a custom node style instance as a template for newly created
    ///   nodes in the graph.
    /// </summary>
    private void InitializeGraph() {
      var graph = Graph;

      graph.SetUndoEngineEnabled(true);

      var selectionStroke = new Pen(Brushes.Black, 2) { DashStyle = DashStyles.Dash };
      selectionStroke.Freeze();

      // Highlight the selected nodes along their outline
      graph.GetDecorator().Nodes.SelectionRenderer.AddFactory(node => node.Style is EditablePathNodeStyle,
          node => new NodeStyleIndicatorRenderer {
              ZoomPolicy = StyleIndicatorZoomPolicy.ViewCoordinates,
              Margins = InsetsD.Empty,
              NodeStyle = new EditablePathNodeStyle(((EditablePathNodeStyle) node.Style).Path, pen: selectionStroke)
          });

      // Create a new style and use it as default node style
      graph.NodeDefaults.Style = new EditablePathNodeStyle(brush: new SolidColorBrush(Color.FromRgb(0xFF, 0x6C, 0x00)),
          pen: new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2B, 0x00)), 1.5));
      graph.NodeDefaults.Size = new SizeD(200, 200);

      // Set some defaults for the edges
      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle();

      // Create some graph elements with the above defined styles.
      CreateSampleGraph();
    }

    /// <summary>
    ///   Creates the default input mode for the GraphControl,
    ///   a <see cref="GraphEditorInputMode"/> configured to show and create path handles.
    /// </summary>
    private GraphEditorInputMode CreateEditorMode() {
      var graphEditorInputMode = new GraphEditorInputMode();

      // add handle input to handle change shape handles
      var changeShapeHandleInputMode = new HandleInputMode {
          Priority = graphEditorInputMode.HandleInputMode.Priority - 1,
          Exclusive = true
      };
      graphEditorInputMode.Add(changeShapeHandleInputMode);

      var locations = new List<PointD>();
      INode? currentNode = null;
      var snapContext = new GraphSnapContext();
      changeShapeHandleInputMode.SnapContext = snapContext;
      snapContext.CreateSnapResultsModelManager(GraphControl).RenderTreeGroup = GraphControl.RenderTree.InputModeGroup;
      snapContext.Initializing += (_, _) => {
        locations.Clear();
        if (currentNode is not null) {
          var outline = currentNode.Style.Renderer.GetShapeGeometry(currentNode, currentNode.Style).GetOutline();
          if (outline is not null) {
            var coords = new double[6];
            var cursor = outline.CreateCursor();
            while (cursor.MoveNext()) {
              switch (cursor.GetCurrent(coords)) {
                case PathType.LineTo:
                case PathType.MoveTo:
                  locations.Add(cursor.CurrentEndPoint);
                  break;
              }
            }
          }
        }
      };

      snapContext.CollectSnapResults += (_, evt) => {
        locations.Where(point => Math.Abs(point.X - evt.NewLocation.X) < evt.SnapDistance).ToList().ForEach(point => {
          var location = new PointD(point.X, evt.NewLocation.Y);
          var snapLine = new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Center,
              SnapReferenceVisualizationType.BlankVariableLine, point, false,
              1 / (1 + Math.Abs(point.X - evt.NewLocation.X)));
          var constraint = SnapConstraint.CreateLineConstraint(
              location, evt.NewLocation, evt.SnapDistance, new PointD(0, 1));
          if (constraint is not null) {
            evt.AddSnapResult(SnapResults.CreateLineSnapResult(constraint,
                1 / (1 + Math.Abs(point.X - evt.NewLocation.X)), null, null, location, snapLine));
          }
        });
        locations.Where(point => Math.Abs(point.Y - evt.NewLocation.Y) < evt.SnapDistance).ToList().ForEach(point => {
          var location = new PointD(evt.NewLocation.X, point.Y);
          var snapLine = new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Center,
              SnapReferenceVisualizationType.BlankVariableLine, point, false,
              1 / (1 + Math.Abs(point.Y - evt.NewLocation.Y)));
          var constraint = SnapConstraint.CreateLineConstraint(
              location, evt.NewLocation, evt.SnapDistance, new PointD(1, 0));
          if (constraint is not null) {
            evt.AddSnapResult(SnapResults.CreateLineSnapResult(constraint,
                1 / (1 + Math.Abs(point.Y - evt.NewLocation.Y)), null, null, location, snapLine));
          }
        });
      };

      // hide handles when a node is removed
      GraphControl.Graph.NodeRemoved += (_, evt) => {
        if (evt.Item == currentNode) {
          changeShapeHandleInputMode.Cancel();
          changeShapeHandleInputMode.Handles.Clear();
          currentNode = null;
        }
      };

      // deselect the control point handles using right-click on canvas
      graphEditorInputMode.CanvasClicked += (_, evt) => {
        if (evt.PointerButtons == PointerButtons.MouseRight) {
          changeShapeHandleInputMode.Handles.Clear();
        }
      };

      // select existing or add new handles on double-click on path
      graphEditorInputMode.ItemLeftDoubleClicked += (_, evt) => {
        if (evt.Item is INode node && node.Style is EditablePathNodeStyle style) {
          var existingHandle = style.GetHandles(node)
              .FirstOrDefault(handle => handle.Location.DistanceTo(evt.Location) < evt.Context.HitTestRadius);
          if (existingHandle is not null) {
            changeShapeHandleInputMode.Handles.Clear();
            // if we were clicked with the modifier presses, remove the corresponding segment instead
            if (evt.Modifiers.HasFlag(ModifierKeys.Shift) && existingHandle is PathHandle pathHandle) {
              pathHandle.RemoveSegment();
              currentNode = null;
            } else {
              changeShapeHandleInputMode.Handles.Add(existingHandle);
              currentNode = node;
            }
            evt.Handled = true;
          } else if (style.SplitPath(node, evt.Location)) {
            changeShapeHandleInputMode.Handles.Clear();
            // find the closest handle, which should be the new handle...
            var createdHandle = style.GetHandles(node).OrderBy(handle => handle.Location.DistanceTo(evt.Location))
                .FirstOrDefault();
            if (createdHandle is not null) {
              changeShapeHandleInputMode.Handles.Clear();
              changeShapeHandleInputMode.Handles.Add(createdHandle);
              currentNode = node;
              evt.Handled = true;
            }
          } else {
            EditablePathNodeStyleHelpers.UpdateHandles(node, changeShapeHandleInputMode);
            currentNode = node;
            evt.Handled = true;
          }
        }
      };

      // remove a handle and control point on right click
      graphEditorInputMode.ItemRightClicked += (_, evt) => {
        if (currentNode is not null && changeShapeHandleInputMode.Handles.Count > 3) {
          var handle = changeShapeHandleInputMode.Handles.OfType<PathHandle>()
              .FirstOrDefault(candidate => candidate.Location.DistanceTo(evt.Location) < 10);
          if (handle is not null) {
            handle.RemoveSegment();
            ((EditablePathNodeStyle) currentNode.Style).NormalizePath(currentNode, Graph);

            // reset handles to update their locations and order
            changeShapeHandleInputMode.Handles.Clear();
            foreach (var currentHandle in ((EditablePathNodeStyle) currentNode.Style).GetHandles(currentNode)) {
              changeShapeHandleInputMode.Handles.Add(currentHandle);
            }
            Graph.InvalidateDisplays();
          }
        }
      };

      var undoEngine = Graph.GetUndoEngine();
      if (undoEngine is not null) {
        undoEngine.UnitUndone += (_, _) =>
            EditablePathNodeStyleHelpers.UpdateHandles(currentNode, changeShapeHandleInputMode);
        undoEngine.UnitRedone += (_, _) =>
            EditablePathNodeStyleHelpers.UpdateHandles(currentNode, changeShapeHandleInputMode);
      }

      return graphEditorInputMode;
    }

    /// <summary>
    ///   Creates the initial sample graph.
    /// </summary>
    private void CreateSampleGraph() {
      var graph = Graph;
      graph.NodeDefaults.ShareStyleInstance = false;

      var n1 = graph.CreateNode();

      var path = new GeneralPath(4);
      path.MoveTo(0, 0);
      path.LineTo(0.5, 0.25);
      path.LineTo(1, 0);
      path.LineTo(1, 1);
      path.LineTo(0, 1);
      path.Close();

      var n2 = graph.CreateNode(new RectD(300, 0, 200, 100),
          new EditablePathNodeStyle(path, new SolidColorBrush(Color.FromRgb(0xFF, 0x6C, 0x00)),
              new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2B, 0x00)), 1.5)));

      graph.CreateEdge(n1, n2);
    }

    [STAThread]
    private static void Main() => Run<EditablePathStyleDemo>();
  }
}
