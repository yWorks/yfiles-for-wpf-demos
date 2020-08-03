/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
  public class MultiLabelFolderNodeConverter : DefaultFolderNodeConverter
  {
    
    /// <summary>
    /// Gets or sets a value indicating whether all labels of the <see cref="IFoldingView.GetMasterItem{T}">master group node</see>
    /// should be recreated for the collapsed group node instance.
    /// </summary>
    /// <remarks>
    /// This setting can be used to initially create a copy of all the labels of the master group node (if any) and
    /// subsequently synchronize the <see cref="ILabel.Text"/> property with the master's node label text.
    /// Set it to <see langword="true"/> if all labels should be copied; <see langword="false"/> otherwise.
    /// The default is <see langword="false"/>.
    /// </remarks>
    /// <seealso cref="DefaultFolderNodeConverter.LabelStyle"/>
    /// <seealso cref="DefaultFolderNodeConverter.LabelLayoutParameter"/>
    public bool CopyLabels { get; set; }

    public override void UpdateFolderNodeState(FolderNodeState state, IFoldingView foldingView, INode viewNode, INode masterNode) {
      SynchronizeLabels(state, foldingView, viewNode, masterNode);
      
      // Copys the changed master Style to the state
      state.Style = masterNode.Style;
    }
    
    
    /// <summary>
    /// Called by <see cref="UpdateFolderNodeState" /> to synchronize all labels, if <see cref="CopyLabels" />
    /// is enabled. Also synchronizes all port labels of ports connected to the node.
    /// </summary>
    /// <remarks>This will adjust the label text property.</remarks>
    /// <param name="state">The node view state whose labels should be synchronized.</param>
    /// <param name="foldingView">The folding view.</param>
    /// <param name="viewNode">The local node instance.</param>
    /// <param name="masterNode">The master node.</param>
    protected override void SynchronizeLabels(FolderNodeState state, IFoldingView foldingView, INode viewNode, INode masterNode) {

      if (CopyLabels) {
        if (masterNode.Labels.Count > 0 && state.Labels.Count > 0) {
          for (var i = 0; i < masterNode.Labels.Count; i++) {
            var masterLabel = masterNode.Labels[i];
            var labelViewState = state.Labels[i];
            labelViewState.Text = masterLabel.Text;
            labelViewState.PreferredSize = masterLabel.PreferredSize;
            labelViewState.Tag = masterLabel.Tag;
          }
        }

        if (masterNode.Ports.Count > 0) {
          for (var j = 0; j < masterNode.Ports.Count; j++) {
            var port = masterNode.Ports[j];
            if (port.Labels.Count > 0 ) {
              for (var i = 0; i < port.Labels.Count; i++) {
                var masterLabel = port.Labels[i];
                var labelViewState = state.Ports[j].Labels[i];
                labelViewState.Text = masterLabel.Text;
                labelViewState.PreferredSize = masterLabel.PreferredSize;
                labelViewState.Tag = masterLabel.Tag;
              }
            }
          }
        }  
      }
    }

    /// <inheritdoc />
    protected override void InitializeFolderNodePorts(FolderNodeState state, IFoldingView foldingView, INode viewNode, INode masterNode) {
      foreach (var port in viewNode.Ports) {
        var masterPort = foldingView.GetMasterItem(port);
        var newStyle = CreatePortStyle(foldingView, port, masterPort);
        var portState = state.GetFoldingPortState(masterPort);
        if (newStyle != null) {
          portState.Style = newStyle;
        }
        var newLocationParameter = CreatePortLocationParameter(foldingView, port, masterPort);
        if (newLocationParameter != null) {
          portState.LocationParameter = newLocationParameter;
        }
        
        if (masterPort.Labels.Count > 0) {
          for (var i = 0; i < masterPort.Labels.Count; i++) {
            var label = masterPort.Labels[i];
            var labelStyle = CreateLabelStyle(foldingView, null, label);
            var labelLayoutParameter = CreateLabelLayoutParameter(foldingView, null, label);
            
            portState.AddLabel(label.Text, labelLayoutParameter ?? label.LayoutParameter, labelStyle ?? label.Style, label.PreferredSize, label.Tag); 
          }
        }
        
      }
    }
    
    /// <inheritdoc />
    protected override void InitializeFolderNodeLabels(FolderNodeState state, IFoldingView foldingView, INode viewNode, INode masterNode) {
      state.ClearLabels();
      if (CopyLabels) {
        var labels = masterNode.Labels;
        if (labels.Count > 0) {
          for (var i = 0; i < labels.Count; i++) {
            var label = labels[i];
            // If the node is a choreographyNode, just copy all Labels
            if (masterNode.Style is ChoreographyNodeStyle) {
              state.AddLabel(label.Text, label.LayoutParameter, label.Style, label.PreferredSize, label.Tag);
            } else {
              // if subProcessNode, create new Layout & Style
              var labelStyle = CreateLabelStyle(foldingView, null, label);
              var labelModel = new InteriorStretchLabelModel {Insets = new InsetsD(3, 3, 3, 3)};
              var labelLayoutParameter = labelModel.CreateParameter(InteriorStretchLabelModel.Position.Center);
              state.AddLabel(label.Text, labelLayoutParameter ?? label.LayoutParameter, labelStyle ?? label.Style, label.PreferredSize, label.Tag); 
            }
          }
        }
      }
    }
  }
}
