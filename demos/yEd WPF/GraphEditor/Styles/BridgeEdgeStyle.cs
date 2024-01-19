/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
  public sealed class BridgeEdgeStyle : DynamicArrowEdgeStyleBase {

    private yWorks.Graph.Styles.BridgeEdgeStyle delegateStyle;

    public BridgeEdgeStyle() {
      delegateStyle = new yWorks.Graph.Styles.BridgeEdgeStyle {
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
      var clone = (BridgeEdgeStyle)base.MemberwiseClone();
      clone.delegateStyle = (yWorks.Graph.Styles.BridgeEdgeStyle) delegateStyle.Clone();
      return clone;
    }

    protected override void OnArrowsChanged() {
      if (delegateStyle != null) {
        delegateStyle.SourceArrow = SourceArrow;
        delegateStyle.TargetArrow = TargetArrow;
      }
    }

    [DefaultValue(0)]
    public double Height {
      get { return delegateStyle.Height; }
      set {
        if (delegateStyle.Height != value) {
          delegateStyle.Height = value;
        }
      }
    }

    [DefaultValue(0.2)]
    public double FanLength {
      get { return delegateStyle.FanLength; }
      set {
        if (delegateStyle.FanLength != value) {
          delegateStyle.FanLength = value;
        }
      }
    }
  }
}
