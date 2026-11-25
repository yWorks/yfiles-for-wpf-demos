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
using System.Windows;
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ExternalLinks
{
  /// <summary>
  ///   Shows a hand cursor for external links and underlines their text to indicate that they are clickable.
  ///   Highlighting is only active when Ctrl is pressed and the hovered item contains an external link.
  /// </summary>
  public sealed class LinkItemHoverInputMode : ItemHoverInputMode
  {
    private readonly IGraph graph;
    private ILabel? activeLabel;
    private ILabelStyle? originalStyle;

    public LinkItemHoverInputMode(IGraph graph) {
      this.graph = graph;
      Priority = 54;
      HoverCursor = Cursors.Hand;
      HoverItems = GraphItemTypes.Node | GraphItemTypes.Edge | GraphItemTypes.Label;
    }

    /// <summary>
    ///   It is only a valid hover when Ctrl is active and the item has a label that represents a link.
    /// </summary>
    /// <param name="item">The item to check.</param>
    protected override bool IsValidHoverItem(IModelItem item) {
      if (ParentInputModeContext!.CanvasControl.LastPointerEvent.Modifiers != ModifierKeys.Control) {
        return false;
      }

      return GetLabelLink(item) != null;
    }

    /// <summary>
    ///   Toggles the underline text decoration for valid links.
    /// </summary>
    /// <param name="evt">The <see cref="HoveredItemChangedEventArgs"/> instance containing the event data.</param>
    protected override void OnHoveredItemChanged(HoveredItemChangedEventArgs evt) {
      var oldLabelLink = GetLabelLink(evt.OldItem);
      var labelLink = GetLabelLink(evt.Item);

      // the toggle of underlined text should not be added to the undo queue
      var graph = ParentInputModeContext!.GetGraph()!;
      var edit = graph.BeginEdit("LinkDecoration", "LinkDecoration");

      if (oldLabelLink != null) {
        // re-apply the original style
        graph.SetStyle(oldLabelLink, graph.NodeDefaults.Labels.GetStyleInstance(oldLabelLink.Owner));
      }

      if (labelLink != null) {
        // underline the text of the link
        var clone = (labelLink.Style.Clone() as LabelStyle)!;
        clone.TextDecorations = TextDecorations.Underline;
        graph.SetStyle(labelLink, clone);
      }

      // we cancel the edit to not add it to the undo queue
      edit.Cancel();
    }

    /// <summary>
    ///   Returns the external URL represented by the text, or null if the text is not a link.
    /// </summary>
    /// <param name="text">The label text to check.</param>
    /// <returns>The original HTTP URL or an HTTPS URL for a www link.</returns>
    public static string? GetExternalUrl(string? text) {
      if (text == null) {
        return null;
      }
      if (text.StartsWith("http", StringComparison.Ordinal)) {
        return text;
      }
      return text.StartsWith("www.", StringComparison.Ordinal) ? $"https://{text}" : null;
    }

    /// <summary>
    ///   Returns the <see cref="ILabel"/> that represents a link, otherwise null. Nodes and edges are checked for
    ///   any label that represents a link, too.
    /// </summary>
    /// <param name="item">The item that should be checked for an external link.</param>
    /// <returns>The label that represents a link or null.</returns>
    private static ILabel? GetLabelLink(IModelItem? item) {
      ILabel? labelLink = null;
      if (item is ILabel label && (label.Text.StartsWith("www.") || label.Text.StartsWith("http"))) {
        labelLink = label;
      } else if (item is INode node) {
        foreach (var nodeLabel in node.Labels) {
          var text = nodeLabel.Text;
          if (text.StartsWith("www.") || text.StartsWith("http")) {
            labelLink = nodeLabel;
          }
        }
      } else if (item is IEdge edge) {
        foreach (var edgeLabel in edge.Labels) {
          var text = edgeLabel.Text;
          if (text.StartsWith("www.") || text.StartsWith("http")) {
            labelLink = edgeLabel;
          }
        }
      }
      return labelLink;
    }
  }
}
