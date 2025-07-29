/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Grouping;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace Demo.yFiles.Layout.MixedLayout
{
  /// <summary>
  ///   Demonstrates how to realize a table node structure, i.e., each group node in the drawing represents a table
  ///   and the nodes within the groups the table rows. Edges are connected to specific rows.
  ///   The rows are sorted according to their y-coordinate in the initial drawing.
  /// </summary>
  public class TableLayout : LayoutStageBase
  {
    /// <summary>
    ///   Gets the layout data that shall be used for the TableLayout.
    /// </summary>
    public static LayoutData<INode, IEdge, ILabel, ILabel> LayoutData { get; }

    static TableLayout() {
      // set up port candidates for edges (edges should be attached to the left/right side of the corresponding node
      var candidates = new EdgePortCandidates().AddFreeCandidate(PortSides.Left).AddFreeCandidate(PortSides.Right);

      // configure layout algorithms
      var rowLayout = new RowLayout(); // used for laying out the nodes (rows) within the group nodes (tables)

      LayoutData = new RecursiveGroupLayoutData<INode, IEdge, ILabel, ILabel> {
          Ports = {
              SourcePortCandidates = { Constant = candidates }, TargetPortCandidates = { Constant = candidates }
          },

          // map each group node to its corresponding layout algorithm;
          // in this case each group node shall be laid out using the RowLayout
          GroupNodeLayouts = { Constant = rowLayout }
      };
    }

    public TableLayout(bool fromSketch) {
      // incremental hierarchical layout is used for the core layout that connects the table nodes
      var hl = new HierarchicalLayout { LayoutOrientation = LayoutOrientation.LeftToRight, FromSketchMode = fromSketch };

      var rgl = new RecursiveGroupLayout(hl) { AutoAssignPortCandidates = true, FromSketchMode = true };
      CoreLayout = rgl;
    }

    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      CoreLayout?.ApplyLayout(graph);
    }
  }

  internal class RowLayout : ILayoutAlgorithm
  {
    private const int Distance = 5;

    #region ILayoutAlgorithm Members

    public virtual void ApplyLayout(LayoutGraph graph) {
      var rows = graph.Nodes.ToArray();
      // sort the rows by their vertical coordinate
      Array.Sort(rows, (node1, node2) => (int) (node1.Layout.CenterY - node2.Layout.CenterY));

      // layout the nodes in a column
      double currentY = 0;

      foreach (var node in rows) {
        node.Layout.X = 0;
        node.Layout.Y = currentY;
        currentY += node.Layout.Height + Distance;
      }

      // there should be no edges between the 'row' nodes but if there are some, all their bends shall be removed
      foreach (var edge in graph.Edges) {
        edge.ResetPath(false);
      }
    }

    #endregion
  }
}
