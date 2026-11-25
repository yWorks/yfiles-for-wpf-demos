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

using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step08Grouping
{
  /// <summary>
  ///   Tutorial step 08: Grouping.
  /// </summary>
  /// <remarks>
  ///   Shows how to configure support for grouped or hierarchically organized graphs,
  ///   including programmatic group node creation and the interactive group/ungroup gestures.
  /// </remarks>
  public class GroupingStep : TutorialStepBase
  {
    public override string Title => "08 Grouping";

    public override string DescriptionSource => "Step08Grouping.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      ConfigureGroupNodeStyles(graph);
      BasicFeaturesTutorial.CreateSampleGraph(graph);
      CreateGroupNodes(graph);

      var inputMode = new GraphEditorInputMode();
      graphControl.InputMode = inputMode;

      // Select all non-group nodes that have labels
      foreach (var node in graph.Nodes.Where(n => !graph.IsGroupNode(n) && n.Labels.Count > 0).ToList()) {
        graphControl.Selection.Nodes.Add(node);
      }

      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);

      AddButtonListener(description, "groupButton", () => inputMode.GroupSelection());
      AddButtonListener(description, "ungroupButton", inputMode.UngroupSelection);
      AddButtonListener(description, "adjustGroupButton", () => AdjustGroupNodeSize(graph));
    }

    /// <summary>Configures the default style for group nodes.</summary>
    private static void ConfigureGroupNodeStyles(IGraph graph) {
      var groupNodeStyle = new GroupNodeStyle {
          TabBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#0b7189"))
      };
      graph.GroupNodeDefaults.Style = groupNodeStyle;

      // Set a label style with right-aligned text
      var labelStyle = new LabelStyle { TextAlignment = TextAlignment.Right, TextBrush = Brushes.White };
      graph.GroupNodeDefaults.Labels.Style = labelStyle;

      // Place the label inside the tab area of the group node.
      // GroupNodeLabelModel is usually the most appropriate label model for GroupNodeStyle.
      graph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabParameter();
    }

    /// <summary>
    ///   Shows how to create group nodes programmatically.
    ///   Creates a couple of nodes and puts them into a group node.
    /// </summary>
    private static void CreateGroupNodes(IGraph graph) {
      var n1 = graph.CreateNode(new PointD(30, -200));
      var n2 = graph.CreateNode(new PointD(170, -200));
      var n3 = graph.CreateNode(new PointD(30, -100));
      graph.CreateEdge(n1, n3);
      // Create an edge that crosses the group node boundary
      graph.CreateEdge(n3, graph.Nodes[0]);

      // Create a group node that encloses n1, n2 and n3
      var groupNode = graph.GroupNodes(n1, n2, n3);

      graph.AddLabel(groupNode, "Group Node");
      // Edges starting from the group node itself are also allowed
      var edgeFromGroup = graph.CreateEdge(groupNode, graph.Nodes[1]);
      graph.AddBend(edgeFromGroup, new PointD(100, -35), 0);
      graph.AddBend(edgeFromGroup, new PointD(170, -35), 1);
    }

    private static void AdjustGroupNodeSize(IGraph graph) {
      // Get a group node
      var groupNode = graph.Nodes.FirstOrDefault(graph.IsGroupNode);

      if (groupNode != null) {
        // Create a child node that's outside the group bounds
        graph.CreateNode(groupNode, new RectD(100, -60, 30, 30));
        // Adjust the group node layout to contain the new child
        graph.AdjustGroupNodeLayout(groupNode);
      }
    }
  }
}
