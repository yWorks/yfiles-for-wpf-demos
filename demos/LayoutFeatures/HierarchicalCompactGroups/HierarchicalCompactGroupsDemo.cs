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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalCompactGroups
{
  /// <summary>
  ///   Demonstrates how to configure a left-to-right <see cref="HierarchicalLayout"/> with compact group nodes.
  /// </summary>
  [Demo("layout-hierarchical-compact-groups", "layout-features", "Hierarchical Layout with Compact Groups",
      Summary = "Shows how to configure the hierarchical layout such that it yields maximally compact group nodes.",
      Tags = ["hierarchical", "compaction"],
      Keywords = [
          "layout", "recursive layering", "DefaultCoordinateAssignment", "bendReduction", "groupCompactionStrategy",
          "groupCompactionPolicy", "playground"
      ])]
  public class HierarchicalCompactGroupsDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from JSON data
      BuildGraphFromJson("sample.json");

      // Run an initial compact hierarchical layout
      ApplyHierarchicalLayoutWithGroupCompaction();

      // Add a button to toggle the group compaction on and off
      ToolBar.AddToggleButton("Compact Groups", pressed => {
        if (pressed) {
          ApplyHierarchicalLayoutWithGroupCompaction();
        } else {
          var hierarchicalLayout = new HierarchicalLayout { LayoutOrientation = LayoutOrientation.LeftToRight };
          Graph.ApplyLayout(hierarchicalLayout);
          GraphControl.FitGraphBounds();
        }
      }, true);
    }

    /// <summary>
    ///   Configures and applies a left-to-right hierarchical layout with compact group nodes.
    /// </summary>
    private void ApplyHierarchicalLayoutWithGroupCompaction() {
      var hierarchicalLayout = new HierarchicalLayout {
          // Arrange group nodes recursively for minimal width in left-to-right layout
          GroupLayeringPolicy = GroupLayeringPolicy.RecursiveCompact,
          // Decrease horizontal spacing between nodes within the same layer
          NodeDistance = 10,
          LayoutOrientation = LayoutOrientation.LeftToRight
      };
      // Compact groups vertically (reduces group height)
      hierarchicalLayout.CoordinateAssigner.GroupCompaction = true;

      Graph.ApplyLayout(hierarchicalLayout);
      GraphControl.FitGraphBounds();
    }

    [STAThread]
    private static void Main() => Run<HierarchicalCompactGroupsDemo>();
  }
}
