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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.DataBinding;

namespace yWorks.Demo.GraphAnalysis
{
  public sealed partial class GraphAnalysisDemo
  {
    private Settings settings = null!;
    private Dictionary<string, int[][]> samples = null!;

    /// <summary>
    ///   Returns the algorithm that is currently selected.
    /// </summary>
    private Algorithm? GetCurrentAlgorithm() => settings.Algorithm;

    /// <summary>
    ///   Determines whether the current analysis algorithm should use uniform weight for all edges.
    /// </summary>
    private bool UseUniformEdgeWeights() => settings.WeightIndex == 0;

    /// <summary>
    ///   Determines whether the current analysis algorithm should take the direction of edges into account.
    /// </summary>
    private bool UseDirectedEdges() =>
        settings.Algorithm?.DirectedOnly == true || settings.SupportsDirection && settings.DirectionIndex == 1;

    /// <summary>
    ///   Wires up the combo-boxes and buttons in the toolbar.
    /// </summary>
    private void InitializeToolbar() {
      settingsPanel.DataContext = settings;
      settings.PropertyChanged += OnSettingsChanged;
    }

    private async void OnSettingsChanged(object? sender, PropertyChangedEventArgs e) {
      if (changing) {
        return;
      }

      if (e.PropertyName == nameof(Settings.Sample) && settings.Sample != null) {
        SwitchSample();
      } else if (e.PropertyName == nameof(Settings.Algorithm)) {
        await SwitchAlgorithm();
      } else if (e.PropertyName == nameof(Settings.WeightIndex)) {
        changing = true;
        if (settings.WeightIndex == 1) {
          GenerateWeightLabels();
        } else {
          DeleteWeightLabels();
        }

        changing = false;
        ApplyAlgorithm();
        await RunLayout(null, true);
      } else if (e.PropertyName == nameof(Settings.DirectionIndex)) {
        ApplyAlgorithm();
        await RunLayout(null, true);
      }
    }

    /// <summary>
    ///   Handles a selection change in the sample combo box.
    /// </summary>
    private void SwitchSample() {
      var sample = settings.Sample!;
      changing = true;
      settings.Algorithm = sample;
      settings.DirectionIndex = sample.DefaultDirected ? 1 : 0;
      settings.WeightIndex = 0;
      LoadGraph(sample.Sample);
      changing = false;
      UpdateGraphInformation();
      ApplyAlgorithm();
      var (layout, data) = GetOrganicLayoutConfiguration(null);
      Graph.ApplyLayout(layout, data);
      graphControl.FitGraphBounds();
      Graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Loads a graph from sample data. This demo uses the <see cref="AdjacencyGraphBuilder"/> because the
    ///   samples are provided as adjacency lists.
    /// </summary>
    private void LoadGraph(string name) {
      Graph.Clear();
      var sample = samples[name];
      var graphBuilder = new AdjacencyGraphBuilder(Graph);
      // create the nodes
      var nodesSource = graphBuilder.CreateNodesSource(Enumerable.Range(0, sample.Length), index => index);
      nodesSource.NodeCreator.TagProvider = _ => new NodeTag();

      var edgeCreator = new EdgeCreator<int> { Defaults = Graph.EdgeDefaults };
      nodesSource.AddSuccessorIds(index => sample[index].Cast<object>(), edgeCreator);
      graphBuilder.BuildGraph();
    }

    /// <summary>
    ///   Handles a selection change in the algorithm combo box.
    /// </summary>
    private async Task SwitchAlgorithm() {
      if (GetCurrentAlgorithm()?.DirectedOnly == true) {
        changing = true;
        settings.DirectionIndex = 1;
        changing = false;
      }

      if (UseUniformEdgeWeights()) {
        DeleteWeightLabels();
      }

      ResetTypes();
      ApplyAlgorithm();
      await RunLayout(null, false);
      Graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Disables or enables the controls of the UI.
    ///   When enabling the UI elements, the current algorithm is considered and only suitable elements are enabled.
    /// </summary>
    private void SetUIDisabled(bool disabled) {
      var inputMode = (GraphEditorInputMode) graphControl.InputMode!;
      if (disabled) {
        settingsPanel.IsEnabled = false;
        toolbar.IsEnabled = false;
        inputMode.Waiting = true;
      } else {
        inputMode.Waiting = false;
        settingsPanel.IsEnabled = true;
        toolbar.IsEnabled = true;
      }
    }

    /// <summary>
    ///   Deletes all weight labels.
    /// </summary>
    private void DeleteWeightLabels() {
      foreach (var label in Graph.GetEdgeLabels().Where(label => Equals(label.Tag, "weight")).ToArray()) {
        Graph.Remove(label);
      }
    }

    /// <summary>
    ///   Generates labels for each edge in the graph with a random weight.
    /// </summary>
    private void GenerateWeightLabels() {
      foreach (var edge in Graph.Edges) {
        foreach (var label in edge.Labels.Where(label => Equals(label.Tag, "weight")).ToArray()) {
          Graph.Remove(label);
        }

        // select a weight from 1 to 20
        Graph.AddLabel(edge, Random.Shared.Next(1, 21).ToString(), tag: "weight");
      }
    }

    private void OnNew(object sender, RoutedEventArgs e) {
      using (Graph.BeginEdit("Clear Graph", "Clear Graph")) {
        ResetGraph();
        Graph.Clear();
      }

      ApplyAlgorithm();
    }

    private async void OnLayout(object sender, RoutedEventArgs e) {
      await RunLayout(null, false);
    }
  }
}
