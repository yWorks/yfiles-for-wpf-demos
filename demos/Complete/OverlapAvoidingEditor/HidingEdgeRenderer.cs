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

using System.Collections.Generic;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.Graph.OverlapAvoidingEditor
{
  /// <summary>
  ///   An <see cref="IObjectRenderer"/> implementation that hides certain edges.
  /// </summary>
  public class HidingEdgeRenderer : IObjectRenderer
  {
    // the original renderer that is wrapped
    private readonly IObjectRenderer originalRenderer;

    /// <summary>
    ///   Creates a new instance that wraps the <paramref name="originalRenderer"/>.
    /// </summary>
    /// <param name="originalRenderer">The original renderer.</param>
    public HidingEdgeRenderer(IObjectRenderer originalRenderer) {
      this.originalRenderer = originalRenderer;
      HiddenEdges = new HashSet<IEdge>();
    }

    /// <summary>
    ///   The edges to hide.
    /// </summary>
    public ISet<IEdge> HiddenEdges { get; set; }

    /// <inheritdoc/>
    public IVisualCreator GetVisualCreator(object renderTag) {
      return originalRenderer.GetVisualCreator(renderTag);
    }

    /// <inheritdoc/>
    public IBoundsProvider GetBoundsProvider(object renderTag) {
      return originalRenderer.GetBoundsProvider(renderTag);
    }

    /// <summary>
    ///   Returns <see cref="VisibilityTestables.Never"/> if <paramref name="renderTag"/> is a hidden edge or
    ///   the original implementation otherwise.
    /// </summary>
    public IVisibilityTestable GetVisibilityTestable(object renderTag) {
      return renderTag is IEdge edge && HiddenEdges.Contains(edge)
          ? VisibilityTestables.Never
          : originalRenderer.GetVisibilityTestable(renderTag);
    }

    /// <inheritdoc/>
    public IHitTestable GetHitTestable(object renderTag) {
      return originalRenderer.GetHitTestable(renderTag);
    }
  }
}
