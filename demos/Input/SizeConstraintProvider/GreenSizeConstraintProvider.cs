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

using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Utils;

namespace yWorks.Demo.SizeConstraintProvider
{
  /// <summary>
  ///   An <see cref="INodeSizeConstraintProvider"/> that returns the size of the
  ///   first label as minimum size. The maximum size is not limited.
  /// </summary>
  public sealed class GreenSizeConstraintProvider : INodeSizeConstraintProvider
  {
    private readonly INode node;

    public GreenSizeConstraintProvider(INode node) {
      this.node = node;
    }

    /// <summary>
    ///   Returns the label size to prevent the shrinking of nodes beyond their
    ///   label's size.
    ///   <see cref="INodeSizeConstraintProvider.GetMinimumSize"/>.
    /// </summary>
    public SizeD GetMinimumSize() {
      foreach (var label in node.Labels) {
        var labelProvider = label.Lookup<INodeSizeConstraintProvider>();
        if (labelProvider != null) {
          return labelProvider.GetMinimumSize();
        }

        if (label.LayoutParameter.Model is InteriorNodeLabelModel) {
          return label.PreferredSize;
        }
      }
      return new SizeD(1, 1);
    }

    /// <summary>
    ///   Returns the infinite size since the maximum size is not limited.
    ///   <see cref="INodeSizeConstraintProvider.GetMaximumSize"/>.
    /// </summary>
    public SizeD GetMaximumSize() => SizeD.Infinite;

    /// <summary>
    ///   Returns an empty rectangle since this area is not constrained.
    ///   <see cref="INodeSizeConstraintProvider.GetMinimumEnclosedArea"/>.
    /// </summary>
    public RectD GetMinimumEnclosedArea() => RectD.Empty;
  }
}
