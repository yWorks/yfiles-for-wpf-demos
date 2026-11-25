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
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.ExternalLinks
{
  [Demo("external-links", "application-features", "External Links",
      Summary = "Shows how to add labels that act like external links and open in a new window.",
      Keywords = ["ItemHoverInputMode", "control", "events", "input mode"], Tags = ["interaction"])]
  public sealed partial class ExternalLinksDemo
  {
    private IGraph Graph => graphControl.Graph;

    private readonly LinkItemHoverInputMode linkItemHoverInputMode;

    public ExternalLinksDemo() {
      InitializeComponent();
      linkItemHoverInputMode = new LinkItemHoverInputMode(Graph);
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // Initialize graph component
      var inputMode = new GraphEditorInputMode();
      graphControl.InputMode = inputMode;

      // Configures default styles for newly created graph elements.
      InitializeGraph(Graph);

      // Then build the graph with the given data set.
      BuildGraph();

      Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // Finally, enable the undo engine. This prevents undoing of the graph creation
      Graph.SetUndoEngineEnabled(true);

      // The click listener for labels that represent external links.
      InitializeLinkListener(inputMode);

      // An optional custom ItemHoverInputMode which highlights clickable links by underlining the text.
      inputMode.Add(linkItemHoverInputMode);
    }

    /// <summary>
    ///   Creates nodes and edges from the given data.
    /// </summary>
    private void BuildGraph() {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var graphData = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(Graph, graphData);
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // Set styles for this demo.
      DemoStyles.InitDemoStyles(graph);

      // Set sizes and locations specific to this demo.
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(sideOfEdge: EdgeSides.BelowEdge);
    }

    private void InitializeLinkListener(GraphEditorInputMode inputMode) {
      inputMode.ItemLeftClicked += (_, evt) => {
        if (evt.Modifiers != ModifierKeys.Control) {
          // this listener should only handle CTRL+click to open external links
          return;
        }

        var clickedItem = evt.Item;
        var url = "";

        if (clickedItem is ILabel label) {
          url = label.Text.StartsWith("www.") || label.Text.StartsWith("http") ? label.Text : "";
        } else if (clickedItem is INode node) {
          // if a node or edge was clicked, we see whether it has any label that resembles a link
          foreach (var nodeLabel in node.Labels) {
            var text = nodeLabel.Text;
            if (text.StartsWith("www.") || text.StartsWith("http")) {
              url = text;
            }
          }
        } else if (clickedItem is IEdge edge) {
          foreach (var edgeLabel in edge.Labels) {
            var text = edgeLabel.Text;
            if (text.StartsWith("www.") || text.StartsWith("http")) {
              url = text;
            }
          }
        }

        if (url != "") {
          Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
          evt.Handled = true;
        }
        ;
      };
    }

    [STAThread]
    public static void Main() {
      Run<ExternalLinksDemo>();
    }
  }
}
