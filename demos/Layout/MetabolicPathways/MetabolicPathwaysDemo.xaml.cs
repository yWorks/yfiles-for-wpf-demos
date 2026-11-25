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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;

namespace yWorks.Demo.MetabolicPathways
{
  /// <summary>
  ///   Shows how to configure the organic layout for visualizing metabolic pathways.
  /// </summary>
  [Demo("metabolic-pathways", "layout", "Metabolic Pathways",
      Summary = "Shows how to configure organic layout for visualizing metabolic pathways.",
      Tags = ["layout", "organic", "constraint"], Keywords = [
          "layoutfeatures", "align", "horizontal", "vertical", "cycle", "circle",
          "biology", "chemical", "reaction", "margin", "nodeMargin"
      ])]
  public sealed partial class MetabolicPathwaysDemo
  {
    private bool initialized;
    private bool loading;

    public MetabolicPathwaysDemo() {
      InitializeComponent();
    }

    private IGraph Graph => graphControl.Graph;

    protected override void Initialize() {
      // initialize the interaction
      graphControl.InputMode = new GraphViewerInputMode();

      // set some default styles for nodes and edges
      MetabolicPathwayStyles.InitializeDefaultStyles(Graph);

      SampleSelector.SelectedIndex = 0;

      // load the initial data-set and create the graph
      LoadSample("pentose");

      // configure and run the organic layout algorithm
      _ = RunLayoutAsync("pentose");

      initialized = true;
    }

    /// <summary>
    ///   Loads the new graph sample and updates the style and the graph structure.
    /// </summary>
    private void LoadSample(string sample) {
      // read the input data and create the graph
      CreateGraph(sample);
      // update the nodes/edge styles based on the node types
      MetabolicPathwayStyles.UpdateStyles(Graph);
    }

    /// <summary>
    ///   Runs the appropriate layout configuration based on the selected sample.
    /// </summary>
    private async Task RunLayoutAsync(string sample) {
      if (sample == "pentose") {
        var (layout, layoutData) = ConfigurePentoseLayout.Create(Graph);
        await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(500), layoutData);
      } else {
        var (layout, layoutData) = ConfigureKrebsCycleLayout.Create(Graph);
        await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(500), layoutData);
      }

      // update arc edges based on the source/target positions
      UpdateArcEdges();
    }

    /// <summary>
    ///   Loads the input data and builds the graph.
    /// </summary>
    private void CreateGraph(string sample) {
      var resourceName = sample == "pentose" ? "pentose-phosphate-data.json" : "krebs-cycle-data.json";
      using var stream = LoadAssemblyResource(resourceName);
      var data =
          JsonSerializer.Deserialize<MetabolicPathwayData>(stream,
              new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
          throw new InvalidDataException("The embedded metabolic pathway sample is empty.");
      var builder = new GraphBuilder(Graph);

      // create the nodes and update the nodes' tag based on the type mapping
      var nodesSource = builder.CreateNodesSource(data.Nodes, node => node.Id);
      nodesSource.NodeCreator.TagProvider = node => MetabolicPathwayData.CreateTag(node.Tag);
      nodesSource.NodeCreator.CreateLabelBinding(node => MetabolicPathwayStyles.FormatLabel(node.Labels ?? ""));

      // create the edges
      builder.CreateEdgesSource(data.Edges, edge => edge.Source, edge => edge.Target);

      // create the graph
      builder.BuildGraph();
    }

    /// <summary>
    ///   Removes the parallel edges that have already been assigned a bidirectional edge style.
    /// </summary>
    private void UpdateArcEdges() {
      foreach (var edge in Graph.Edges.ToArray()) {
        if (edge.Style is ArcEdgeStyle) {
          Graph.SetStyle(edge, MetabolicPathwayStyles.GetArcEdgeStyle(Graph, edge));
        }
      }
    }

    /// <summary>
    ///   Binds actions to the sample selector in the toolbar.
    /// </summary>
    private async void SampleSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (!initialized || loading || SampleSelector.SelectedItem is not ComboBoxItem item ||
          item.Tag is not string sample) {
        return;
      }

      loading = true;
      SampleSelector.IsEnabled = false;
      try {
        Graph.Clear();
        LoadSample(sample);
        await RunLayoutAsync(sample);
        await graphControl.FitGraphBounds();
      } finally {
        SampleSelector.IsEnabled = true;
        loading = false;
      }
    }

    [STAThread]
    public static void Main() => Run<MetabolicPathwaysDemo>();
  }
}
