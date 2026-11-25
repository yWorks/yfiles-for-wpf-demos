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

using System.Collections.Generic;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace yWorks.Demo.LabelHandleProvider
{
  /// <summary>
  ///   A custom <see cref="IHandleProvider"/> implementation that returns a <see cref="LabelResizeHandle"/> for each
  ///   label which can be resized and a <see cref="LabelRotateHandle"/> for each label which can be rotated.
  /// </summary>
  internal sealed class LabelHandleProvider : IHandleProvider
  {
    private readonly GraphControl graphControl;
    private readonly ILabel label;

    /// <summary>
    ///   Creates a new instance of <see cref="LabelHandleProvider"/>.
    /// </summary>
    /// <param name="graphControl">The graph control that renders the handle previews.</param>
    /// <param name="label">The given label.</param>
    public LabelHandleProvider(GraphControl graphControl, ILabel label) {
      this.graphControl = graphControl;
      this.label = label;
    }

    /// <summary>
    ///   Returns a list of available handles for the label this instance has been created for.
    /// </summary>
    public IEnumerable<IHandle> GetHandles(IInputModeContext context) {
      // return a list of the available handles
      var handles = new List<IHandle>();
      var labelModel = label.LayoutParameter.Model;
      if (labelModel is StretchNodeLabelModel) {
        // Some label models are not resizable at all - don't provide any handles
      } else if (labelModel is FreeEdgeLabelModel or FreeNodeLabelModel or FreeLabelModel) {
        // These models support resizing in one direction
        handles.Add(new LabelResizeHandle(graphControl, label, false));
        // They also support rotation
        handles.Add(new LabelRotateHandle(graphControl, label, context));
      } else {
        // For all other models, we assume the *center* needs to stay the same
        // This requires that the label must be resized symmetrically in both directions
        handles.Add(new LabelResizeHandle(graphControl, label, true));
      }
      return handles;
    }
  }
}
