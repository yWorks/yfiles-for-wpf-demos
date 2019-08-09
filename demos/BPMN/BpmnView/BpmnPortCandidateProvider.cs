/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Graph.Bpmn.Styles;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Controls.Input;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.Graph.Bpmn {

  /// <summary>
  /// Provides some existing ports as well as ports on the north, south, east and west center of the visual bounds of a BPMN node.
  /// </summary>
  /// <remarks>
  /// An existing port is provided if it either uses an <see cref="EventPortStyle"/> or is used by at least one edge.
  /// </remarks>
  public class BpmnPortCandidateProvider : PortCandidateProviderBase {
    private readonly INode node;

    public BpmnPortCandidateProvider([NotNull] INode node) {
      this.node = node;
    }

    protected override IEnumerable<IPortCandidate> GetPortCandidates(IInputModeContext context) {
      var portCandidates = new List<IPortCandidate>();

      // provide existing ports as candidates only if they use EventPortStyle and have no edges attached to them.
      foreach (IPort port in node.Ports) {
        if (port.Style is EventPortStyle && context.Lookup<IGraph>().EdgesAt(port).Count == 0) {
          portCandidates.Add(new DefaultPortCandidate(port));
        }
      }
      
      if (node.Style is ActivityNodeStyle || node.Style is ChoreographyNodeStyle 
        || node.Style is DataObjectNodeStyle || node.Style is AnnotationNodeStyle 
        || node.Style is GroupNodeStyle || node.Style is DataStoreNodeStyle) {
        portCandidates.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeTopAnchored));
        portCandidates.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeBottomAnchored));
        portCandidates.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeLeftAnchored));
        portCandidates.Add(new DefaultPortCandidate(node, FreeNodePortLocationModel.NodeRightAnchored));
      } else if (node.Style is EventNodeStyle || node.Style is GatewayNodeStyle) {
        double dmax = Math.Min(node.Layout.Width / 2, node.Layout.Height / 2);
        var model = FreeNodePortLocationModel.Instance;
        portCandidates.Add(new DefaultPortCandidate(node, model.CreateParameter(new PointD(0.5, 0.5), new PointD(0, -dmax))));
        portCandidates.Add(new DefaultPortCandidate(node, model.CreateParameter(new PointD(0.5, 0.5), new PointD(dmax, 0))));
        portCandidates.Add(new DefaultPortCandidate(node, model.CreateParameter(new PointD(0.5, 0.5), new PointD(0, dmax))));
        portCandidates.Add(new DefaultPortCandidate(node, model.CreateParameter(new PointD(0.5, 0.5), new PointD(-dmax, 0))));
      } else if (node.Style is ConversationNodeStyle) {
        double dx = 0.5 * Math.Min(node.Layout.Width, node.Layout.Height / BpmnConstants.Sizes.ConversationWidthHeightRatio);
        double dy = dx * BpmnConstants.Sizes.ConversationWidthHeightRatio;
        var model = FreeNodePortLocationModel.Instance;
        portCandidates.Add(new DefaultPortCandidate(node, model.CreateParameter(new PointD(0.5, 0.5), new PointD(0, -dy))));
        portCandidates.Add(new DefaultPortCandidate(node, model.CreateParameter(new PointD(0.5, 0.5), new PointD(dx, 0))));
        portCandidates.Add(new DefaultPortCandidate(node, model.CreateParameter(new PointD(0.5, 0.5), new PointD(0, dy))));
        portCandidates.Add(new DefaultPortCandidate(node, model.CreateParameter(new PointD(0.5, 0.5), new PointD(-dx, 0))));
      }

      var ceim = context.ParentInputMode as CreateEdgeInputMode;
      var canvasControl = context.CanvasControl;
      if (ceim == null || canvasControl == null || KeyEventRecognizers.ShiftPressed(canvasControl, canvasControl.LastInputEvent)) {
        // add a dynamic candidate
        portCandidates.Add(new DefaultPortCandidate(node, new FreeNodePortLocationModel()));
      }
      return portCandidates;

    }
  }
}
