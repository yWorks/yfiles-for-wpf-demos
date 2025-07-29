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

using Demo.yFiles.Graph.Bpmn.Styles;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.Bpmn.BpmnDi
{
  public class MultiLabelFolderNodeConverter : IFolderNodeConverter
  {

    // we want to copy all the labels, for the nodes and the ports, but
    // for the node labels, we need to keep the style instance for the ChoreographyStyle nodes.
    private readonly FolderNodeDefaults folderNodeDefaults = new() {
        CopyLabels = true,
        Ports = { CopyLabels = true },
        Labels = {
            LayoutParameter =
                new StretchNodeLabelModel {Padding = new InsetsD(3, 3, 3, 3)}.CreateParameter(
                    StretchNodeLabelModel.Position.Center)
        }
    };

    private readonly FolderNodeDefaults choreographyFolderNodeDefaults = new() {
        CopyLabels = true,
        Ports = { CopyLabels = true },
        Labels =  {
            ShareStyleInstance = true, // we want to keep the instances
            Style = null,
            LayoutParameter = null,
        }
    };

    public FoldingLabelDefaults LabelDefaults => folderNodeDefaults.Labels;

    public void UpdateFolderNodeState(FolderNodeState state, IFoldingView foldingView, INode viewNode, INode masterNode) {
      if (viewNode is { Style: ChoreographyNodeStyle }) {
        choreographyFolderNodeDefaults.UpdateState(state, masterNode);
      } else {
        folderNodeDefaults.UpdateState(state, masterNode);
      }
    }

    public void UpdateGroupNodeState(FolderNodeState state, IFoldingView foldingView, INode viewNode, INode masterNode) {
      // TODO - implement and write back labels!
      if (state.Labels.Count == viewNode.Labels.Count) {
        for (int i = 0; i < state.Labels.Count; i++) {
          foldingView.Graph.SetLabelText(viewNode.Labels[i], state.Labels[i].Text);
        }
      }
    }

    public void InitializeFolderNodeState(FolderNodeState state, IFoldingView foldingView, INode viewNode, INode masterNode) {
      if (viewNode is { Style: ChoreographyNodeStyle }) {
        choreographyFolderNodeDefaults.InitializeState(state, masterNode);
      } else {
        folderNodeDefaults.InitializeState(state, masterNode);
      }
    }
  }
}
