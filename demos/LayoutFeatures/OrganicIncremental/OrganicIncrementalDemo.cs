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

namespace yWorks.Demo.OrganicIncremental
{
  /// <summary>
  ///   Demonstrates how to run an organic layout on a predefined subset of nodes.
  /// </summary>
  [Demo("layout-organic-incremental", "layout-features", "Incremental Organic Layout",
      Summary = "Shows how to run the organic layout on a predefined set of nodes.", Tags = ["organic", "incremental"],
      Keywords = ["layout", "subset", "nodes", "set", "node-set", "playground"])]
  public class OrganicIncrementalDemo : SimpleDemo
  {
    protected override void Initialize() {
      var organicLayout = new OrganicLayout();
      var organicLayoutData = organicLayout.CreateLayoutData(Graph);
      organicLayoutData.Scope.ScopeModes.MapperFunction = node =>
          node.Tag is NodeData data && data.IncludeInLayout ? OrganicScope.IncludeCloseNodes : OrganicScope.Fixed;

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));
      GraphControl.FitGraphBounds();

      // Apply the incremental organic layout to the graph
      GraphControl.ApplyLayoutAnimated(organicLayout, TimeSpan.FromSeconds(1), organicLayoutData);
    }

    private record NodeData(bool IncludeInLayout);

    [STAThread]
    private static void Main() => Run<OrganicIncrementalDemo>();
  }
}
