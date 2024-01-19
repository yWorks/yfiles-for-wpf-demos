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
using System.Windows.Markup;
using yWorks.Graph.Styles;

[assembly: XmlnsDefinition("http://www.yworks.com/yfiles-wpf/2.1/demos/GraphEditor/Styles", "Demo.yFiles.GraphEditor.Styles")]
[assembly: XmlnsPrefix("http://www.yworks.com/yfiles-wpf/2.1/demos/GraphEditor/Styles", "yed")]

namespace Demo.yFiles.GraphEditor.Styles
{
  public sealed class PolylineEdgeStyle: DynamicArrowEdgeStyleBase {
    private yWorks.Graph.Styles.PolylineEdgeStyle delegateStyle;
    private bool smoothBends;

    public PolylineEdgeStyle() {
      delegateStyle = new yWorks.Graph.Styles.PolylineEdgeStyle {
        SmoothingLength = SmoothBends ? 10 * Pen.Thickness : 0,
        SourceArrow = SourceArrow,
        TargetArrow = TargetArrow,
        Pen = Pen
      };
    }

    public override object Clone() {
      var clone = (PolylineEdgeStyle)base.MemberwiseClone();
      clone.delegateStyle = (yWorks.Graph.Styles.PolylineEdgeStyle) delegateStyle.Clone();
      return clone;
    }

    protected override IEdgeStyle GetDelegateStyle() {
      return delegateStyle;
    }

    public double Smoothing {
      get { return SmoothBends ? 10 * Pen.Thickness: 0; }
    }

    [DisplayName("Round Corners")]
    [DefaultValue(false)]
    public bool SmoothBends {
      get { return smoothBends; }
      set {
        if (smoothBends != value) {
          smoothBends = value;
          delegateStyle.SmoothingLength = value ? 10 * Pen.Thickness : 0;
        }
      }
    }

    protected override void OnPenChanged() {
      base.OnPenChanged();
      if (delegateStyle != null) {
        delegateStyle.Pen = Pen;
      }
    }

    protected override void OnArrowsChanged() {
      if (delegateStyle != null) {
        delegateStyle.SourceArrow = SourceArrow;
        delegateStyle.TargetArrow = TargetArrow;
      }
    }
  }
}