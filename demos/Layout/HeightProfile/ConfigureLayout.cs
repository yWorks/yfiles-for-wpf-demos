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

using System.Linq;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Organic;

namespace yWorks.Demo.HeightProfile
{
  internal static class ConfigureLayout
  {
    /// <summary>
    ///   Creates and configures the <see cref="OrganicLayout"/> to produce a height profile visualization.
    ///   In this demo, the layout arranges only the label nodes while the positions of the waypoints
    ///   (the points on the curve) remain fixed. To get the desired visualization, constraints are added
    ///   to the <see cref="OrganicLayout"/>.
    /// </summary>
    public static (OrganicLayout Layout, OrganicLayoutData<INode, IEdge, ILabel, ILabel> LayoutData) Create(
        IGraph graph) {
      // creates the organic layout with the desired distance between nodes
      // and sets the scope to subset so that only the label nodes are placed by the
      // layout algorithm
      var organicLayout = new OrganicLayout {
          Deterministic = true,
          DefaultMinimumNodeDistance = 15,
          DefaultPreferredEdgeLength = 20
      };

      // mark the label nodes as affected so that they are arranged by the layout algorithm
      var organicLayoutData = organicLayout.CreateLayoutData(graph);
      organicLayoutData.Scope.Nodes.Source = graph.Nodes.Where(node => WaypointData.GetWaypoint(node)?.Type == "label");

      // create a constraint for each waypoint-label node pair that forces the algorithm to
      // place the label nodes above their associated waypoints
      foreach (var edge in graph.Edges) {
        var waypoint = edge.GetSourceNode();
        var labelNode = edge.GetTargetNode();

        // create a constraint that will place each label node above its associated waypoint
        var separationConstraint =
            organicLayoutData.Constraints.AddSeparationConstraint(ConstraintOrientation.Vertical, 0);
        separationConstraint.FirstSet.Item = labelNode;
        separationConstraint.SecondSet.Item = waypoint;

        // create a constraint that will vertically align each waypoint with the associated label node
        organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical).Items =
            [labelNode, waypoint];

        // create a constraint that will place the label of the first waypoint (if this coincides with
        // the y-Axis) above the y-Axis
        if (waypoint.Layout.GetCenterX() == 0) {
          organicLayoutData.Constraints.AddMinimumDistance(labelNode, waypoint,
              ScaleData.ScaledMaxY + waypoint.Layout.GetCenterY() + 100);
        }
      }

      return (organicLayout, organicLayoutData);
    }
  }
}
