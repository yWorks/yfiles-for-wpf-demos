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
using System.Threading.Tasks;
using yWorks.Controls;
using yWorks.Layout.Router;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   Provides event handlers for toolbar controls.
  /// </summary>
  internal static class ToolbarSupport
  {
    internal static void EnableGraphML(GraphControl graphControl) {
      var support = new GraphMLSupport(graphControl);
      support.GraphMLIOHandler.AddXamlNamespaceMapping("http://www.yworks.com/demos/octilinear-edge-style/1.0",
          typeof(OctilinearEdgeStyle));
      ;
      support.RegisterOpenSaveCommandBindings();
    }

    internal static void OnSegmentLengthChanged(GraphControl graphControl, double segmentLength) {
      if (graphControl == null) {
        return;
      }
      foreach (var edge in graphControl.Graph.Edges) {
        if (edge.Style is OctilinearEdgeStyle edgeStyle) {
          // set it in the edge style (for new bends)
          edgeStyle.PreferredOctilinearSegmentLength = segmentLength;
          foreach (var bend in edge.Bends) {
            var maxCuttingLength = Utils.GetMaxCuttingLength(bend);
            // live update the cutting length (for existing bends)
            Utils.UpdateCuttingLength(bend,
                Math.Min(Utils.SegmentLengthToCuttingLength(segmentLength), maxCuttingLength));
          }
        }
      }
      graphControl.Invalidate();
    }

    internal static async Task ApplyLayout(GraphControl graphControl) {
      await ApplyOctilinearLayout.Apply(graphControl, Utils.CreateHierarchicalLayout(true));
    }

    internal static async Task ApplyRouter(GraphControl graphControl) {
      var octilinearRouter = new EdgeRouter();
      octilinearRouter.DefaultEdgeDescriptor.RoutingStyle = RoutingStyle.Octilinear;
      await ApplyOctilinearLayout.Apply(graphControl, octilinearRouter);
    }
  }
}
