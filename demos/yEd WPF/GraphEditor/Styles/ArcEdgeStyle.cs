/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.ComponentModel;
using yWorks.Graph.Styles;

namespace Demo.yFiles.GraphEditor.Styles
{
  public sealed class ArcEdgeStyle : DynamicArrowEdgeStyleBase {

    private yWorks.Graph.Styles.ArcEdgeStyle delegateStyle;

    public ArcEdgeStyle() {
      delegateStyle = new yWorks.Graph.Styles.ArcEdgeStyle {
        Height = 20d,
        SourceArrow = SourceArrow,
        TargetArrow = TargetArrow,
        Pen = Pen
      };
    }

    protected override IEdgeStyle GetDelegateStyle() {
      return delegateStyle;
    }

    protected override void OnPenChanged() {
      base.OnPenChanged();
      if (delegateStyle != null) {
        delegateStyle.Pen = Pen;
      }
    }

    public override object Clone() {
      var clone = (ArcEdgeStyle)base.MemberwiseClone();
      clone.delegateStyle = (yWorks.Graph.Styles.ArcEdgeStyle) delegateStyle.Clone();
      return clone;
    }

    protected override void OnArrowsChanged() {
      if (delegateStyle != null) {
        delegateStyle.SourceArrow = SourceArrow;
        delegateStyle.TargetArrow = TargetArrow;
      }
    }

    [DefaultValue(false)]
    public bool FixedHeight {
      get { return delegateStyle.FixedHeight; }
      set {
        if (delegateStyle.FixedHeight != value) {
          delegateStyle.FixedHeight = value;
          delegateStyle.Height = value ? 20d : 0.5d;
        }
      }
    }
  }
}