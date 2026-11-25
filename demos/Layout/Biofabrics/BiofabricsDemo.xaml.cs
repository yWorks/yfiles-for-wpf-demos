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
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   Interaction logic for BiofabricsDemo.xaml
  /// </summary>
  [Demo("biofabrics", "layout", "Biofabrics",
      Summary = "A showcase of and introduction to simple, interactive biofabric visualization.",
      Tags = ["interaction", "style", "layout"], Keywords = [
          "biofabric", "biofabrics", "biofabric visualization", "biofabric visualizations",
          "edge ordering", "node ordering", "node grouping", "edge grouping", "edge color gradients",
          "edge gradients", "style", "styling", "highlight", "highlighting", "expand", "collapse",
          "explore", "animations", "company networks", "social networks", "ego networks",
          "logistics networks", "trading networks", "neighborhood view"
      ], RelatedDemos = ["large-graphs", "organization-chart"])]
  public sealed partial class BiofabricsDemo
  {
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly BiofabricsSettings settings = new();
    private Biofabric biofabric = null!;
    private CircularNodeLink nodeLink = null!;
    private Dictionary<DataSetName, GraphData> dataSets = null!;
    private Dictionary<DataSetName, DatasetDescription> dataDescriptions = null!;
    private DataSetName activeDataset;

    public BiofabricsDemo() {
      InitializeComponent();
    }

    protected override async void Initialize() {
      SettingsPanel.DataContext = settings;
      SettingsPanel.Document.DataContext = settings;
      settings.SettingChanged += OnSettingChanged;

      dataSets = new Dictionary<DataSetName, GraphData> {
          [DataSetName.CompanyNetwork] = LoadGraphData("company-network.json"),
          [DataSetName.LogisticsNetwork] = LoadGraphData("logistics-network.json"),
          [DataSetName.TradingGraph] = LoadGraphData("trading-network.json")
      };
      dataDescriptions =
          JsonSerializer.Deserialize<Dictionary<DataSetName, DatasetDescription>>(
              LoadAssemblyResource("data-descriptions.json"), JsonOptions) ??
          throw new InvalidOperationException("Could not load the dataset descriptions.");

      activeDataset = DataSetName.CompanyNetwork;
      // Apply all defaults stored in the dataDescription object
      var initialDescription = GetDataDescription(activeDataset);
      settings.ApplyDefaults(activeDataset, initialDescription.Defaults);
      // Update the description of the loaded dataset in the sidebar
      UpdateDescriptionText(activeDataset);

      // Setup biofabric and nodelink
      BuildGraph(biofabricGraphControl.Graph, dataSets[activeDataset], true, true);
      BuildGraph(circularGraphControl.Graph, dataSets[activeDataset], true, false);

      ConfigureViewer(biofabricGraphControl);
      ConfigureViewer(circularGraphControl);

      SetupBiofabric(initialDescription.BiofabricParameters);
      SetupNodeLink(initialDescription.CircularParameters);

      // limit Viewport to reasonable margins
      ConfigureViewport(biofabricGraphControl, circularGraphControl);

      // Configure the bidirectional interactivity between biofabric and node-link diagram
      ConfigureBidirectionalInteraction.Configure(biofabric, nodeLink);

      settings.IsEnabled = false;
      try {
        await Task.WhenAll(biofabric.RunLayout(), nodeLink.ApplyLayout());
      } finally {
        settings.IsEnabled = true;
      }
    }

    /// <summary>
    ///   Sets up the Biofabric visualization for the selected dataset.
    /// </summary>
    private void SetupBiofabric(BiofabricParameters parameters) {
      biofabric = new Biofabric(biofabricGraphControl, "label", "label",
          new BiofabricOptions {
              NodeGroupDataKey = settings.NodeGrouping ? "group" : null,
              EdgeGroupDataKey = settings.EdgeGrouping ? "group" : null,
              NodeOrderingKey = settings.NodeOrdering,
              EdgeOrderingKey = settings.EdgeOrdering,
              NodeGroupOrderingKey = settings.NodeGroupOrdering,
              EdgeGroupOrderingKey = settings.EdgeGroupOrdering,
              EdgeColorMode = settings.EdgeColorMode,
              NodeWidthMode = settings.NodeWidthMode,
              BiofabricWidth = 3000,
              BiofabricHeight = 1200,
              HorizontalPadding = 50,
              EdgeSpacing = parameters.EdgeSpacing
          });
    }

    /// <summary>
    ///   Sets up the circular node-link visualization for the selected dataset.
    /// </summary>
    private void SetupNodeLink(CircularParameters parameters) {
      nodeLink = new CircularNodeLink(circularGraphControl, "label", "label",
          new CircularNodeLinkOptions {
              NodeGroupDataKey = settings.NodeGrouping ? "group" : null,
              EdgeGroupDataKey = settings.EdgeGrouping ? "group" : null,
              NodeOrderingKey = settings.NodeOrdering,
              NodeGroupOrderingKey = settings.NodeGroupOrdering,
              EdgeGroupOrderingKey = settings.EdgeGroupOrdering,
              EdgeColorMode = settings.EdgeColorMode,
              EdgeThickness = parameters.EdgeThickness
          });
    }

    /// <summary>
    ///   Configures reasonable navigation limits for the two main visualizations.
    /// </summary>
    private static void ConfigureViewport(GraphControl biofabricGraph, GraphControl nodeLinkGraph) {
      biofabricGraph.ViewportLimiter.ViewportContentMargins = new InsetsD(450, 150, 450, 150);
      biofabricGraph.ViewportLimiter.Policy = ViewportLimitingPolicy.TowardsLimit;
      biofabricGraph.ViewportLimiter.MinimumViewportContentRatio = new SizeD(0.5, 0.5);
      biofabricGraph.MaximumZoom = 1.5;

      nodeLinkGraph.ViewportLimiter.ViewportContentMargins = new InsetsD(150);
      nodeLinkGraph.MaximumZoom = 1.5;
      nodeLinkGraph.ViewportLimiter.MinimumViewportContentRatio = new SizeD(0.5, 0.5);
      nodeLinkGraph.ViewportLimiter.Policy = ViewportLimitingPolicy.TowardsLimit;
    }

    private static void ConfigureViewer(GraphControl graphControl) {
      graphControl.InputMode = new GraphViewerInputMode {
          ClickableItems = GraphItemTypes.None,
          FocusableItems = GraphItemTypes.None,
          SelectableItems = GraphItemTypes.None,
          MarqueeSelectableItems = GraphItemTypes.None
      };
    }

    /// <summary>
    ///   Runs the visualizations again after a sidebar setting has changed.
    /// </summary>
    private async void OnSettingChanged(object? sender, SettingChangedEventArgs args) {
      if (!settings.IsEnabled) {
        return;
      }

      var datasetChanged = args.PropertyName == nameof(BiofabricsSettings.SelectedDataset);
      var colorModeChanged = args.PropertyName == nameof(BiofabricsSettings.EdgeColorMode);
      var resort = args.PropertyName is nameof(BiofabricsSettings.NodeOrdering)
          or nameof(BiofabricsSettings.EdgeOrdering) or nameof(BiofabricsSettings.NodeGrouping)
          or nameof(BiofabricsSettings.EdgeGrouping) or nameof(BiofabricsSettings.NodeGroupOrdering)
          or nameof(BiofabricsSettings.EdgeGroupOrdering) or nameof(BiofabricsSettings.SelectedDataset);

      if (colorModeChanged) {
        ApplySettingsToVisualizations();
        biofabric.UpdateVisualization();
        nodeLink.UpdateVisualization();
        return;
      }

      settings.IsEnabled = false;
      try {
        if (datasetChanged) {
          activeDataset = settings.SelectedDataset;
          var description = GetDataDescription(activeDataset);
          settings.ApplyDefaults(activeDataset, description.Defaults);
          biofabric.ClearHighlights();
          nodeLink.ClearHighlights();
          BuildGraph(biofabricGraphControl.Graph, dataSets[activeDataset], true, true);
          BuildGraph(circularGraphControl.Graph, dataSets[activeDataset], true, false);
          biofabric.SetPortLocationModel();
          biofabric.EdgeSpacing = description.BiofabricParameters.EdgeSpacing;
          nodeLink.EdgeThickness = description.CircularParameters.EdgeThickness;
          UpdateDescriptionText(activeDataset);
        }

        ApplySettingsToVisualizations();
        biofabric.UpdateBiofabric();
        biofabric.UpdateVisualization();
        nodeLink.UpdateVisualization();
        biofabric.DetermineUnitHeightAndWidth(biofabric.FromTargetDimensions);
        biofabric.UpdateLayoutProperties(resort);

        TimeSpan? animationDuration = datasetChanged ? null : TimeSpan.FromSeconds(1);
        await Task.WhenAll(biofabric.RunLayout(animationDuration), nodeLink.ApplyLayout(animationDuration));
        biofabric.UpdateVisualization();
        // nodeLink.UpdateVisualization();
      } finally {
        settings.IsEnabled = true;
      }
    }

    private void ApplySettingsToVisualizations() {
      var nodeGroupDataKey = settings.NodeGrouping ? "group" : null;
      var edgeGroupDataKey = settings.EdgeGrouping ? "group" : null;

      biofabric.EdgeColorMode = nodeLink.EdgeColorMode = settings.EdgeColorMode;
      biofabric.NodeWidthMode = settings.NodeWidthMode;
      biofabric.EdgeOrderingKey = settings.EdgeOrdering;
      biofabric.NodeOrderingKey = nodeLink.NodeOrderingKey = settings.NodeOrdering;
      biofabric.NodeGroupDataKey = nodeLink.NodeGroupDataKey = nodeGroupDataKey;
      biofabric.EdgeGroupDataKey = nodeLink.EdgeGroupDataKey = edgeGroupDataKey;
      biofabric.NodeGroupOrderingKey = nodeLink.NodeGroupOrderingKey = settings.NodeGroupOrdering;
      biofabric.EdgeGroupOrderingKey = nodeLink.EdgeGroupOrderingKey = settings.EdgeGroupOrdering;
    }

    /// <summary>
    ///   Creates a graph from its node and link data and optionally creates node and edge labels.
    /// </summary>
    private static void BuildGraph(
        IGraph graph,
        GraphData graphData,
        bool addNodeLabel = false,
        bool addEdgeLabel = false) {
      graph.Clear();
      var graphBuilder = new GraphBuilder(graph);

      var nodeSource = graphBuilder.CreateNodesSource(graphData.Nodes, node => node.Id);
      nodeSource.NodeCreator.TagProvider = node => new {
          id = node.Id,
          label = node.Label,
          group = node.Group
      };
      if (addNodeLabel) {
        nodeSource.NodeCreator.CreateLabelBinding(node => node.Label);
      }

      var edgeSource = graphBuilder.CreateEdgesSource(graphData.Links, link => link.Source, link => link.Target);
      edgeSource.EdgeCreator.TagProvider = link => new {
          label = link.Label ?? string.Empty,
          group = link.Group,
          source = link.Source,
          target = link.Target
      };
      if (addEdgeLabel) {
        edgeSource.EdgeCreator.CreateLabelBinding(link => string.IsNullOrEmpty(link.Label) ? null : link.Label);
      }

      graphBuilder.BuildGraph();
    }

    private GraphData LoadGraphData(string resourceName) =>
        JsonSerializer.Deserialize<GraphData>(LoadAssemblyResource(resourceName), JsonOptions) ??
        throw new InvalidOperationException($"Could not load graph data from '{resourceName}'.");

    private DatasetDescription GetDataDescription(DataSetName datasetName) => dataDescriptions[datasetName];

    /// <summary>
    ///   Updates the dataset-specific title and description shown in the interaction panel.
    /// </summary>
    private void UpdateDescriptionText(DataSetName datasetName) {
      var description = GetDataDescription(datasetName);
      settings.SetDatasetDescription(description.Title, ConvertHtmlToText(description.Content));
    }

    private static string ConvertHtmlToText(string html) {
      var text = Regex.Replace(html, "<!--[\\s\\S]*?-->", string.Empty);
      text = Regex.Replace(text, "<\\s*li\\b[^>]*>", "• ", RegexOptions.IgnoreCase);
      text = Regex.Replace(text, "<\\s*br\\s*/?\\s*>|<\\s*/\\s*(p|div|li|ul)\\s*>", Environment.NewLine,
          RegexOptions.IgnoreCase);
      text = Regex.Replace(text, "<[^>]+>", string.Empty);
      text = WebUtility.HtmlDecode(text);
      text = Regex.Replace(text, @"[ \t]*\r?\n[ \t]*", Environment.NewLine);
      return text.Trim();
    }

    [STAThread]
    private static void Main(string[] args) {
      Run<BiofabricsDemo>();
    }
  }
}
