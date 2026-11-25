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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.CustomCopyAndPaste
{
  internal static class ApplyStyle
  {
    /// <summary>
    ///   Shows a toast with the given message.
    /// </summary>
    internal static void ShowToast(Border toast, TextBlock toastText, DispatcherTimer hideTimer, string message) {
      toastText.Text = message;
      toast.Visibility = Visibility.Visible;
      hideTimer.Stop();
      hideTimer.Start();
    }

    /// <summary>
    ///   Applies the style from the clipboard to the selected items in the graph.
    /// </summary>
    internal static void Apply(
        GraphControl graphControl,
        bool pasteStyle,
        bool pasteNodeSize,
        bool pasteLabelPositioning,
        Action<string> showToast) {
      var graph = graphControl.Graph;
      var clipboardGraph = graphControl.Clipboard.ClipboardGraph;
      var selection = graphControl.Selection;
      var skippedTypes = new List<string>();

      // Determine the template items based on what's in the clipboard.

      // Filter out helper nodes. Helper nodes are added automatically to the clipboard graph if one of their dependent
      // items is selected by itself, e.g., if an edge is selected without its source or target node.
      var nonHelperNodes = clipboardGraph.Nodes.Where(node => !graphControl.Clipboard.IsHelper(node)).ToArray();
      var templateNode = nonHelperNodes.Length == 1 ? nonHelperNodes[0] : null;
      if (nonHelperNodes.Length > 1 && selection.Nodes.Count > 0) {
        skippedTypes.Add("nodes");
      }

      // Filter out helper edges. Helper edges are added automatically to the clipboard graph if one of their dependent
      // items is selected by itself, e.g., if an edge label is selected without its owner.
      var nonHelperEdges = clipboardGraph.Edges.Where(edge => !graphControl.Clipboard.IsHelper(edge)).ToArray();
      var templateEdge = nonHelperEdges.Length == 1 ? nonHelperEdges[0] : null;
      if (nonHelperEdges.Length > 1 && selection.Edges.Count > 0) {
        skippedTypes.Add("edges");
      }

      var nodeLabels = clipboardGraph.GetNodeLabels().ToArray();
      var edgeLabels = clipboardGraph.GetEdgeLabels().ToArray();
      var templateNodeLabel = nodeLabels.Length == 1 ? nodeLabels[0] : null;
      var templateEdgeLabel = edgeLabels.Length == 1 ? edgeLabels[0] : null;
      if (nodeLabels.Length > 1 && (selection.Nodes.Count > 0 || selection.Labels.Any(label => label.Owner is INode))) {
        skippedTypes.Add("node labels");
      }
      if (edgeLabels.Length > 1 && (selection.Edges.Count > 0 || selection.Labels.Any(label => label.Owner is IEdge))) {
        skippedTypes.Add("edge labels");
      }

      // When both nodeLabels and edgeLabels are copied, apply the one corresponding to the owner.
      // If only one type is copied, it is pasted to the label irrespective of the owner.
      if (templateNodeLabel == null && templateEdgeLabel != null && templateEdgeLabel.Owner != templateEdge) {
        templateNodeLabel = templateEdgeLabel;
      }
      if (templateEdgeLabel == null && templateNodeLabel != null && templateNodeLabel.Owner != templateNode) {
        templateEdgeLabel = templateNodeLabel;
      }

      if (templateNode != null && (pasteStyle || pasteNodeSize)) {
        // Apply the style to selected nodes.
        foreach (var node in selection.Nodes.ToArray()) {
          if (pasteStyle) {
            graph.SetStyle(node, templateNode.Style);
          }
          if (pasteNodeSize) {
            // Set the node layout through the graph so the change is recorded by its undo engine.
            var geim = graphControl.InputMode as GraphEditorInputMode;
            geim!.SetNodeLayout(node, RectD.FromCenter(node.Layout.GetCenter(), templateNode.Layout.ToSizeD()));
          }
        }
      }
      if (templateEdge != null && pasteStyle) {
        // Apply the style to selected edges.
        foreach (var edge in selection.Edges.ToArray()) {
          graph.SetStyle(edge, templateEdge.Style);
        }
      }
      if (pasteStyle || pasteLabelPositioning) {
        if (templateNodeLabel != null) {
          // Apply the style to selected node labels and labels of selected nodes.
          ApplyLabels(graph, [.. selection.Labels.Where(label => label.Owner is INode)], templateNodeLabel, pasteStyle,
              pasteLabelPositioning);
          ApplyOwnerLabels(graph, selection.Nodes, templateNodeLabel, selection.Labels, pasteStyle,
              pasteLabelPositioning);
        }
        if (templateEdgeLabel != null) {
          // Apply the style to selected edge labels and labels of selected edges.
          ApplyLabels(graph, [.. selection.Labels.Where(label => label.Owner is IEdge)], templateEdgeLabel, pasteStyle,
              pasteLabelPositioning);
          ApplyOwnerLabels(graph, selection.Edges, templateEdgeLabel, selection.Labels, pasteStyle,
              pasteLabelPositioning);
        }
      }
      // note: groups, tables, ports and port labels are ignored in this demo

      // Display a toast when trying to apply style to items for which more than one instance was copied.
      if (skippedTypes.Count > 0) {
        showToast("The style of some items could not be pasted because more than one instance was copied.");
      }
    }

    /// <summary>
    ///   Applies the style of the template label to the labels of the given owners.
    /// </summary>
    private static void ApplyOwnerLabels(
        IGraph graph,
        IEnumerable<ILabelOwner> owners,
        ILabel template,
        IEnumerable<ILabel> selectedLabels,
        bool pasteStyle,
        bool pasteLabelPositioning) {
      foreach (var owner in owners) {
        // Filter directly selected labels to avoid applying the style to them twice.
        ApplyLabels(graph, [.. owner.Labels.Where(label => !selectedLabels.Contains(label))], template, pasteStyle,
            pasteLabelPositioning);
      }
    }

    /// <summary>
    ///   Applies the style of the template label to the given labels.
    /// </summary>
    private static void ApplyLabels(
        IGraph graph,
        IEnumerable<ILabel> labels,
        ILabel template,
        bool pasteStyle,
        bool pasteLabelPositioning) {
      foreach (var label in labels) {
        if (pasteStyle) {
          graph.SetStyle(label, template.Style);
        }
        // Only apply node label parameters to node labels and edge label parameters to edge labels.
        if (pasteLabelPositioning && HaveSameOwnerType(label, template)) {
          graph.SetLabelLayoutParameter(label, template.LayoutParameter);
        }
      }
    }

    /// <summary>
    ///   Determines whether two labels have the same owner type (node or edge).
    /// </summary>
    private static bool HaveSameOwnerType(ILabel label, ILabel templateLabel) =>
        (label.Owner is INode && templateLabel.Owner is INode) ||
        (label.Owner is IEdge && templateLabel.Owner is IEdge);
  }
}
