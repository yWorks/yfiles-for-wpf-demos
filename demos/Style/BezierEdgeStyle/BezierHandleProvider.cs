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
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.BezierEdgeStyle
{
  /// <summary>
  ///   Shows the outer control point handles for the given inner control point. This makes it possible to manipulate
  ///   the control points immediately without selecting them first, which is difficult for points off the curve.
  /// </summary>
  public sealed class BezierBendSelectionHandleProvider : IHandleProvider
  {
    private readonly IBend bend;
    private readonly bool ownerIsSelected;

    public BezierBendSelectionHandleProvider(IBend bend, bool ownerIsSelected) {
      this.bend = bend;
      this.ownerIsSelected = ownerIsSelected;
    }

    public IEnumerable<IHandle> GetHandles(IInputModeContext context) {
      // Don't provide handles if outer control point bends are selected; inner control points will provide them.
      var index = bend.GetIndex();
      if (index % 3 != 2) {
        return [];
      }

      var handles = new List<IHandle> { bend.Lookup<IHandle>()! };
      if (ownerIsSelected) {
        // If the edge is selected, it already provides the outer control point handles.
        return handles;
      }

      // Otherwise, provide the whole triplet.
      var edge = bend.Owner;
      handles.Add(edge.Bends[index - 1].Lookup<IHandle>()!);
      handles.Add(edge.Bends[index + 1].Lookup<IHandle>()!);
      return handles;
    }
  }

  /// <summary>Always shows all outer control point handles for the selected Bézier edge.</summary>
  public sealed class BezierEdgeSelectionHandleProvider : IHandleProvider
  {
    private readonly IEdge edge;
    private readonly IHandleProvider? coreImplementation;

    public BezierEdgeSelectionHandleProvider(IEdge edge, IHandleProvider? coreImplementation) {
      this.edge = edge;
      this.coreImplementation = coreImplementation;
    }

    public IEnumerable<IHandle> GetHandles(IInputModeContext context) {
      var handles = new List<IHandle>();
      if (coreImplementation != null) {
        handles.AddRange(coreImplementation.GetHandles(context));
      }
      foreach (var bend in edge.Bends) {
        if (bend.GetIndex() % 3 != 2) {
          handles.Add(bend.Lookup<IHandle>()!);
        }
      }
      return handles;
    }
  }
}
