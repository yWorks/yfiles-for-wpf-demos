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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using yWorks.Controls;
using yWorks.Graph;

namespace yWorks.Demo.CircularSubstructures
{
  internal static class TypesPopup
  {
    /// <summary>
    ///   The color sets for the eight different node types.
    /// </summary>
    public static readonly Palette[] NodeTypeColors = [
        Themes.Palette21,
        Themes.Palette22,
        Themes.Palette23,
        Themes.Palette15,
        Themes.Palette25,
        Themes.Palette11,
        Themes.Palette12,
        Themes.Palette14
    ];

    /// <summary>
    ///   Gets the type of the given node by querying it from the node's tag.
    /// </summary>
    public static int GetNodeType(INode node) => (node.Tag as NodeTag)?.Type ?? 0;

    /// <summary>
    ///   Sets the type for the given node by updating its tag and the corresponding style.
    /// </summary>
    private static void SetNodeType(IGraph graph, INode node, int type) {
      // Set a new tag and style so that this change is easily undoable.
      node.Tag = new NodeTag { Type = type };
      graph.SetStyle(node, DemoStyles.CreateDemoNodeStyle(NodeTypeColors[type]));
    }

    /// <summary>
    ///   Initializes the palette for changing the type of selected nodes.
    /// </summary>
    public static void InitializeTypePalette(GraphControl graphControl, Panel palettePanel, Action typeChanged) {
      var graph = graphControl.Graph;
      for (var type = 0; type < NodeTypeColors.Length; type++) {
        var currentType = type;
        var palette = NodeTypeColors[currentType];
        var button = new Button {
            Width = 28,
            Height = 26,
            Margin = new Thickness(2),
            Background = palette.Fill,
            BorderBrush = palette.Stroke,
            BorderThickness = new Thickness(1),
            ToolTip = $"Set selected nodes to type {currentType + 1}"
        };
        button.Click += (_, _) => {
          var selectedNodes = graphControl.Selection.Nodes.ToArray();
          foreach (var node in selectedNodes) {
            SetNodeType(graph, node, currentType);
          }
          if (selectedNodes.Length > 0) {
            typeChanged();
          }
        };
        palettePanel.Children.Add(button);
      }
    }
  }
}
