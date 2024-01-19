/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Layout;
using yWorks.Algorithms;
using yWorks.Layout.Grouping;
using yWorks.Layout.Hierarchic;

namespace Demo.yFiles.Layout.MixedLayout
{
  ///<summary>
  /// Demonstrates how to realize a table node structure, i.e., each group node in the drawing represents a table
  /// and the nodes within the groups the table rows. Edges are connected to specific rows.
  /// The rows are sorted according to their y-coordinate in the initial drawing.
  ///</summary>
  public class TableLayout : LayoutStageBase
  {
    private static readonly LayoutData layoutData;

    /// <summary>
    /// Gets the layout data that shall be used for the TableLayout.
    /// </summary>
    public static LayoutData LayoutData { get { return layoutData; } }

    static TableLayout() {

      // set up port candidates for edges (edges should be attached to the left/right side of the corresponding node
      var candidates = new List<PortCandidate>
        {
        PortCandidate.CreateCandidate(PortDirections.West),
        PortCandidate.CreateCandidate(PortDirections.East)
      };

      // configure layout algorithms
      var rowLayout = new RowLayout(); // used for laying out the nodes (rows) within the group nodes (tables)

      layoutData = new RecursiveGroupLayoutData
      {
        SourcePortCandidates = {Constant = candidates},
        TargetPortCandidates = {Constant = candidates},

        // map each group node to its corresponding layout algorithm;
        // in this case each group node shall be laid out using the RowLayout
        GroupNodeLayouts = {Constant = rowLayout}
      };
    }

    public TableLayout(bool fromSketch) {
      // incremental hierarchic layout is used for the core layout that connects the table nodes
      var hl = new HierarchicLayout
                  {
                    LayoutOrientation = LayoutOrientation.LeftToRight,
                    LayoutMode = fromSketch ? LayoutMode.Incremental : LayoutMode.FromScratch,
                    OrthogonalRouting = true
                  };

      var rgl = new RecursiveGroupLayout(hl) {AutoAssignPortCandidates = true, FromSketchMode = true};
      CoreLayout = rgl;
    }

    public override void ApplyLayout(LayoutGraph graph) {
      ApplyLayoutCore(graph);
    }
  }

  internal class RowLayout : ILayoutAlgorithm
  {
    private const int Distance = 5;

    #region ILayoutAlgorithm Members

    public void ApplyLayout(LayoutGraph graph) {
      var rows = graph.GetNodeArray();
      // sort the rows by their vertical coordinate
      Array.Sort(rows, (node1, node2) => (int) (graph.GetCenterY(node1) - graph.GetCenterY(node2)));

      // layout the nodes in a column
      double currentY = 0;

      foreach (var node in rows) {
        graph.SetLocation(node, 0, currentY);
        currentY += graph.GetHeight(node) + Distance;
      }

      // there should be no edges between the 'row' nodes but if there are some, all their bends shall be removed
      foreach (var edge in graph.Edges) {
        graph.SetPoints(edge, new YList());
      }
    }

    #endregion
  }
}