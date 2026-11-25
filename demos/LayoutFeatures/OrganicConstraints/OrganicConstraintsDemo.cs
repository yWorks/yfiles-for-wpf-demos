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
using yWorks.Layout.Organic;

namespace yWorks.Demo.OrganicConstraints
{
  /// <summary>
  ///   Demonstrates how to run an <see cref="OrganicLayout"/> with configured constraints.
  /// </summary>
  [Demo("layout-organic-constraints", "layout-features", "Organic Layout with Constraints",
      Summary = "Shows how to configure constraints for the organic layout algorithm.",
      Tags = ["organic", "constraints"],
      Keywords = ["layout", "circle", "constraints", "align", "cycle", "playground"])]
  public class OrganicConstraintsDemo : SimpleDemo
  {
    private readonly OrganicLayout organicLayout = new() {
        Deterministic = true,
        DefaultPreferredEdgeLength = 80,
        DefaultMinimumNodeDistance = 30
    };

    protected override void Initialize() {
      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));

      // Run an initial layout with constraints
      ApplyOrganicLayoutWithConstraints();

      // Add a button to toggle the constraints on and off
      ToolBar.AddToggleButton("Use Constraints", pressed => {
        if (pressed) {
          ApplyOrganicLayoutWithConstraints();
        } else {
          Graph.ApplyLayout(organicLayout);
          GraphControl.FitGraphBounds();
        }
      }, true);
    }

    /// <summary>
    ///   Demonstrates how to run an <see cref="OrganicLayout"/> with configured constraints.
    /// </summary>
    private void ApplyOrganicLayoutWithConstraints() {
      // Initialize layout data to define constraints
      var organicLayoutData = organicLayout.CreateLayoutData(Graph);

      // Force nodes with "placeOnCircle" tags to lie on a circle
      organicLayoutData.Constraints.AddEllipse(false, 1.0).Predicate =
          node => (node.Tag as NodeData)?.PlaceOnCircle == true;

      Graph.ApplyLayout(organicLayout, organicLayoutData);
      GraphControl.FitGraphBounds();
    }

    private record NodeData(bool PlaceOnCircle);

    [STAThread]
    private static void Main() => Run<OrganicConstraintsDemo>();
  }
}
