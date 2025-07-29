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

using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Graph.HierarchicalGrouping
{
  /// <summary>
  ///   Uses the size constraints provided by the labels to further constrain the size of nodes.
  /// </summary>
  public class LabelSizeConstraintProvider : INodeSizeConstraintProvider
  {
    private readonly INodeSizeConstraintProvider delegateProvider;
    private readonly INode node;

    public LabelSizeConstraintProvider(INode node, INodeSizeConstraintProvider delegateProvider) {
      this.node = node;
      this.delegateProvider = delegateProvider;
    }

    public SizeD GetMinimumSize() {
      var result = delegateProvider?.GetMinimumSize() ?? SizeD.Empty;
      foreach (var label in node.Labels) {
        var context = label.LayoutParameter.Model.GetContext(label);
        var provider = context.Lookup<INodeSizeConstraintProvider>();
        if (provider != null) {
          result = SizeD.Max(result, provider.GetMinimumSize());
        }
      }
      return result;
    }

    public SizeD GetMaximumSize() {
      return delegateProvider?.GetMaximumSize() ?? SizeD.Infinite;
    }

    public RectD GetMinimumEnclosedArea() {
      return delegateProvider?.GetMinimumEnclosedArea() ?? RectD.Empty;
    }
  }
}
