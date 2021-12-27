/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.GraphEditor.Input {
  /// <summary>
  /// A provider that provides different sets, depending on whether it is queried from within
  /// CreateEdgeInputMode or not.
  /// </summary>
  /// <remarks>
  /// This implementation yields dynamic candidates for the edge creation if there is no room left for other
  /// edges connecting to the target port in the same direction, otherwise
  /// the center of the node is provided. If shift is pressed the behavior is inversed.
  /// </remarks>
  public sealed class CustomPortCandidateProvider : PortCandidateProviderBase {
    private readonly INode node;
    private readonly IPortCandidateProvider geometryPortCandidateProvider;

    internal CustomPortCandidateProvider(INode node) {
      this.node = node;
      geometryPortCandidateProvider =
          PortCandidateProviders.Combine(
              PortCandidateProviders.FromNodeCenter(node),
              PortCandidateProviders.FromShapeGeometry(node));
    }

    /// <summary>
    /// Called by the create edge mode to find the port candidates for the start of the edge.
    /// </summary>
    public override IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) {
      var ceim = context.ParentInputMode as CreateEdgeInputMode;
      var canvasControl = context.CanvasControl;
      if (ceim != null && canvasControl != null) {
        return GenerateStartNodePortCandidates(context);
      }
      return base.GetSourcePortCandidates(context);
    }

    /// <summary>
    /// Called by the create edge mode to find the port candidates for the start of the edge if the mode is in reverse
    /// edge creation mode.
    /// </summary>
    /// <seealso cref="CreateEdgeInputMode.CreateReverseEdge"/>
    public override IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) {
      var ceim = context.ParentInputMode as CreateEdgeInputMode;
      var canvasControl = context.CanvasControl;
      if (ceim != null && canvasControl != null) {
        return GenerateStartNodePortCandidates(context);
      }
      return base.GetTargetPortCandidates(context);
    }
    
    /// <summary>
    /// Called by the create edge mode to find the port candidates for the end of the edge given
    /// the source port candidate on the other end of the edge.
    /// </summary>
    public override IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context, IPortCandidate sourcePortCandidate) {
      var ceim = context.ParentInputMode as CreateEdgeInputMode;
      var canvasControl = context.CanvasControl;
      if (ceim != null && canvasControl != null) {
        return GenerateEndNodePortCandidates(context);
      }
      return base.GetTargetPortCandidates(context, sourcePortCandidate);
    }

    /// <summary>
    /// Called by the create edge mode to find the port candidates for the end of the edge in reverse edge creation mode
    /// given the target port candidate on the other end of the edge.
    /// </summary>
    /// <seealso cref="CreateEdgeInputMode.CreateReverseEdge"/>
    public override IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context, IPortCandidate targetPortCandidate) {
      var ceim = context.ParentInputMode as CreateEdgeInputMode;
      var canvasControl = context.CanvasControl;
      if (ceim != null && canvasControl != null) {
        return GenerateEndNodePortCandidates(context);
      }
      return base.GetSourcePortCandidates(context, targetPortCandidate);
    }


    /// <summary>
    /// Creates the list of candidates where the edge creation starts.
    /// </summary>
    private IEnumerable<IPortCandidate> GenerateStartNodePortCandidates(IInputModeContext context) {
      var canvasControl = context.CanvasControl;
      // since we turned on dynamic port candidate resolution without a modifier in the demo, we decide here
      // what to do.
      if (KeyEventRecognizers.ShiftPressed(context.CanvasControl, canvasControl.LastInputEvent)) {
        // yield the center and a dynamic candidate
        return new IPortCandidate[] {
            new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeCenterAnchored),
            new DefaultPortCandidate(node, FreeNodePortLocationModel.Instance),
        };
      } else {
        // yield static candidates, only
        var existingPorts = new List<IPortCandidate>();
        AddExistingPorts(node, existingPorts);
        existingPorts.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeCenterAnchored));
        return existingPorts;
      }
    }

    /// <summary>
    /// Creates the list of candidates where the edge creation ends.
    /// </summary>
    private IEnumerable<IPortCandidate> GenerateEndNodePortCandidates(IInputModeContext context) {
      var canvasControl = context.CanvasControl;

      // check if behavior is toggled.
      var shiftPressed = KeyEventRecognizers.ShiftPressed(context.CanvasControl, canvasControl.LastInputEvent);

      // check if the node is already occupied in that direction
      var targetNodeOccupied = IsTargetNodeOccupied(context, node);

      if (shiftPressed != targetNodeOccupied) {
        // only display dynamic port candidates
        return new IPortCandidate[] {
            new DefaultPortCandidate(node, FreeNodePortLocationModel.Instance),
        };
      } else {
        // only display static port candidates
        var existingPorts = new List<IPortCandidate>();
        AddExistingPorts(node, existingPorts);
        if (node.Lookup<ITable>() == null) {
          // don't provide a default port for table nodes
          existingPorts.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeCenterAnchored));
        }
        return existingPorts;
      }
    }

    private bool IsTargetNodeOccupied(IInputModeContext context, INode node) {
      var ceim = context.ParentInputMode as CreateEdgeInputMode;
      var oeec = context.Lookup<OrthogonalEdgeEditingContext>();
      if (oeec != null && oeec.Enabled) {
        // check whether there are already edges going in the same direction
        var graph = ceim.Graph;
        var dummyEdge = ceim.DummyEdge;
        var direction = dummyEdge.TargetPort.GetLocation() - (dummyEdge.Bends.Count > 0
                          ? dummyEdge.Bends.Last().Location.ToPointD()
                          : dummyEdge.SourcePort.GetLocation());
        var foldingView = graph.GetFoldingView();
        if (foldingView != null && foldingView.Manager.MasterGraph.Contains(node)) {
          node = foldingView.GetViewItem(node);
        }
        if (node != null && graph.Contains(node)) {
          foreach (var edge in graph.EdgesAt(node)) {
            PointD p1, p2;
            if (edge.SourcePort.Owner == node) {
              p1 = edge.SourcePort.GetLocation();
              p2 = edge.Bends.Count > 0 ? edge.Bends.First().Location.ToPointD() : edge.TargetPort.GetLocation();
            } else {
              p1 = edge.TargetPort.GetLocation();
              p2 = edge.Bends.Count > 0 ? edge.Bends.Last().Location.ToPointD() : edge.SourcePort.GetLocation();
            }

            var edgeDirection = p2 - p1;
            // check whether the edge is orthogonal and points into the same direction.
            if ((edgeDirection.X == 0 || edgeDirection.Y == 0) && edgeDirection.ScalarProduct(direction) < 0) {
              return true;
            }
          }
        }
      }
      return false;
    }

    protected override IEnumerable<IPortCandidate> GetPortCandidates(IInputModeContext context) {
      if (context.ParentInputMode is CreateEdgeInputMode) {
        return new IPortCandidate[] {
                                      new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeCenterAnchored),
                                      new DefaultPortCandidate(node, FreeNodePortLocationModel.Instance),
                                    };
      } else {
        var existingPorts = new List<IPortCandidate>();
        AddExistingPorts(node, existingPorts);
        existingPorts.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.Instance));
        return geometryPortCandidateProvider.GetTargetPortCandidates(context).Concat(existingPorts);
      }
    }
  }
}
