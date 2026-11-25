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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Orthogonal;

namespace yWorks.Demo.Orthogonal
{
  [Demo("layout-orthogonal", "layout-features", "Orthogonal Layout",
      Summary = "Shows common configuration options for the orthogonal layout.", Tags = ["orthogonal"],
      Keywords = ["layout", "directed", "nodemargin", "playground", "margin"])]
  public class OrthogonalDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Initialize the orthogonal layout algorithm
      var orthogonalLayout = new OrthogonalLayout {
          // Use strict mode to always produce orthogonal edge routes and maintain node sizes
          LayoutMode = OrthogonalLayoutMode.Strict,
          // Nodes and edges are laid out on a virtual grid so that nodes are placed on grid points and edges run along grid lines
          // Decrease grid spacing for a more compact graph
          GridSpacing = 5,
          // Disable uniform port assignment to not insert additional bends for more uniform port assignment
          UniformPortAssignment = false,
          // Enable tree substructure placement with a compact style and left-to-right orientation
          TreeSubstructureStyle = TreeSubstructureStyle.Compact,
          TreeSubstructureOrientation = SubstructureOrientation.LeftToRight,
          // Set the minimal size for tree detection, smaller ones are ignored
          TreeSubstructureSize = 6,
          // Set layout orientation to top-to-bottom for directed edges
          LayoutOrientation = LayoutOrientation.TopToBottom
      };

      // Create layout data for item-specific settings
      var orthogonalLayoutData = orthogonalLayout.CreateLayoutData(Graph);
      // Define edges tagged 'directed': they must flow in the main layout orientation
      orthogonalLayoutData.EdgeOrientation.MapperFunction = edge => (edge.Tag as EdgeData)?.Directed == true ? 1 : 0;
      // Increase bend cost for directed edges to prefer straight paths (in favor of bending other edges instead)
      orthogonalLayoutData.EdgeBendCosts.MapperFunction = edge => (edge.Tag as EdgeData)?.Directed == true ? 4 : 1;
      // Add margins around the node with the "Margins" label
      orthogonalLayoutData.NodeMargins.MapperFunction = node =>
          new InsetsD(node.Labels.Count > 0 && node.Labels[0].Text == "Margins" ? 50 : 0);

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", edgeTagType: typeof(EdgeData));

      // Apply the configured orthogonal layout
      Graph.ApplyLayout(orthogonalLayout, orthogonalLayoutData);
      GraphControl.FitGraphBounds();
    }

    private record EdgeData(bool Directed);

    [STAThread]
    private static void Main() => Run<OrthogonalDemo>();
  }
}
