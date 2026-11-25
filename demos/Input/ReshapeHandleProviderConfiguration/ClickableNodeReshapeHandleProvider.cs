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
using System.Text.RegularExpressions;
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.ReshapeHandleProviderConfiguration
{
  /// <summary>
  ///   A <see cref="NodeReshapeHandleProvider"/> for gold nodes that toggles aspect ratio resizing on and off when clicking
  ///   one of its handles.
  /// </summary>
  public sealed class ClickableNodeReshapeHandleProvider : NodeReshapeHandleProvider
  {
    private readonly ApplicationState state;

    public ClickableNodeReshapeHandleProvider(INode node, IReshapeHandler reshapeHandler, ApplicationState state) :
        base(node, reshapeHandler, HandlePositions.Border) {
      this.state = state;
    }

    public override IHandle GetHandle(IInputModeContext inputModeContext, HandlePositions position) {
      var wrapped = (NodeReshapeHandlerHandle) base.GetHandle(inputModeContext, position);
      wrapped.RatioReshapeRecognizer = state.KeepAspectRatio ? EventRecognizers.Always : EventRecognizers.Never;
      return new ClickableNodeReshapeHandlerHandle(state, wrapped);
    }
  }

  internal sealed class ClickableNodeReshapeHandlerHandle : IHandle
  {
    private readonly ApplicationState state;
    private readonly NodeReshapeHandlerHandle wrapped;

    public ClickableNodeReshapeHandlerHandle(ApplicationState state, NodeReshapeHandlerHandle wrapped) {
      this.state = state;
      this.wrapped = wrapped;
    }

    public Cursor Cursor => wrapped.Cursor;

    public IPoint Location => wrapped.Location;

    /// <summary>
    ///   Changes the wrapped round handles to quadratic ones to visualize the state.
    /// </summary>
    public HandleType Type => state.KeepAspectRatio ? HandleType.Custom2 : wrapped.Type;

    public object? Tag => wrapped.Tag;

    /// <summary>
    ///   Toggles the aspect ratio state of the application.
    /// </summary>
    public void HandleClick(ClickEventArgs eventArgs) {
      state.ToggleAspectRatio();
    }

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      wrapped.HandleMove(context, originalLocation, newLocation);
    }

    public void InitializeDrag(IInputModeContext context) {
      wrapped.InitializeDrag(context);
    }

    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      wrapped.DragFinished(context, originalLocation, newLocation);
    }

    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      wrapped.CancelDrag(context, originalLocation);
    }
  }

  public sealed class ApplicationState
  {
    private readonly GraphEditorInputMode graphEditorInputMode;
    private readonly IGraph graph;

    public bool KeepAspectRatio { get; private set; }

    public ApplicationState(GraphEditorInputMode graphEditorInputMode, IGraph graph, bool keepAspectRatio) {
      this.graphEditorInputMode = graphEditorInputMode;
      this.graph = graph;
      this.KeepAspectRatio = keepAspectRatio;
    }

    public void ToggleAspectRatio() {
      KeepAspectRatio = !KeepAspectRatio;
      graphEditorInputMode.RequeryHandles();
      UpdateLabel();
    }

    private void UpdateLabel() {
      var goldNode = graph.Nodes.FirstOrDefault(node => Equals(node.Tag, "gold"));
      if (goldNode != null) {
        var label = goldNode.Labels.FirstOrDefault();
        if (label != null) {
          var stateString = KeepAspectRatio ? "keep" : "don't keep";
          graph.SetLabelText(label, Regex.Replace(label.Text, @":\n.*", $":\n {stateString}"));
        }
      }
    }
  }
}
