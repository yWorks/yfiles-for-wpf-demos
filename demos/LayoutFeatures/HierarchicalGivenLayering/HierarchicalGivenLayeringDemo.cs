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

namespace yWorks.Demo.HierarchicalGivenLayering
{
  [Demo("layout-hierarchical-with-given-layering", "layout-features", "Hierarchical Layout with Given Layering",
      Summary = "Shows how to configure hierarchical layout with a given layering.",
      Tags = ["hierarchical", "given", "layers"], Keywords = ["layout", "given", "layering", "layers", "playground"])]
  public class HierarchicalGivenLayeringDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));

      // Run an initial layout with custom layering
      ApplyHierarchicalLayoutWithLayering();

      // Add a button to toggle the custom layering on and off
      ToolBar.AddToggleButton("Custom Layering", pressed => {
        if (pressed) {
          ApplyHierarchicalLayoutWithLayering();
        } else {
          Graph.ApplyLayout(new HierarchicalLayout());
          GraphControl.FitGraphBounds();
        }
      }, true);
    }

    /// <summary>
    ///   Demonstrates how to run a <see cref="HierarchicalLayout"/> with given layering.
    /// </summary>
    private void ApplyHierarchicalLayoutWithLayering() {
      var layout = new HierarchicalLayout {
          // Apply the user-defined layering strategy
          FromScratchLayeringStrategy = LayeringStrategy.UserDefined
      };
      var layoutData = layout.CreateLayoutData(Graph);

      // Specify a layer index (stored in node tags) for each node
      layoutData.GivenLayersIndices.MapperFunction = node => ((NodeData) node.Tag!).Layer;

      Graph.ApplyLayout(layout, layoutData);
      GraphControl.FitGraphBounds();
    }

    private record NodeData(int Layer);

    [STAThread]
    private static void Main() => Run<HierarchicalGivenLayeringDemo>();
  }
}
