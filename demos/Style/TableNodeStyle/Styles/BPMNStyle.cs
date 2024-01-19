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
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.TableNodeStyle.Style
{
  /// <summary>
  /// Abstract base class for stripe styles that provide a BPMN like visualization.
  /// </summary>
  public abstract class BPMNStyle : StripeStyleBase<VisualGroup>
  {
    public StripeDescriptor StripeDescriptor { get; set; }

    private double wedgeHeight = 10;

    [DefaultValue(10)]
    public double WedgeHeight {
      get { return wedgeHeight; }
      set { wedgeHeight = value; }
    }

    private double wedgeWidth = 10;

    [DefaultValue(10)]
    public double WedgeWidth {
      get { return wedgeWidth; }
      set { wedgeWidth = value; }
    }

    protected override VisualGroup CreateVisual(IRenderContext context, IStripe stripe) {
      IRectangle layout = stripe.Layout.ToRectD();
      GeneralPath outline = CreatePath(stripe, layout);
      var visual = outline.CreatePath(StripeDescriptor.BackgroundBrush,
          new Pen(StripeDescriptor.BorderBrush, StripeDescriptor.BorderThickness.Left),
          null, FillMode.Always);

      var cc = new VisualGroup();
      cc.Add(visual);
      cc.SetCanvasArrangeRect(layout.ToRectD());
      return cc;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IStripe stripe) {
      IRectangle layout = stripe.Layout.ToRectD();
      var rect = CanvasControl.GetCanvasControlArrangeRect(oldVisual);
      var arrangeRect = layout.ToRectD();
      if (rect.Width != arrangeRect.Width || rect.Height != arrangeRect.Height) {
        GeneralPath outline = CreatePath(stripe, layout);
        var oldPath = (Path)oldVisual.Children[0];
        outline.UpdatePath(oldPath, StripeDescriptor.BackgroundBrush,
                                        new Pen(StripeDescriptor.BorderBrush, StripeDescriptor.BorderThickness.Left),
                                        null, FillMode.Always);
      }
      oldVisual.SetCanvasArrangeRect(arrangeRect);
      return oldVisual;
    }

    protected abstract GeneralPath CreatePath(IStripe stripe, IRectangle layout);
  }

  /// <summary>
  /// Custom style that provides a BPMN like visualization for rows
  /// </summary>
  public class BPMNRowStyle : BPMNStyle
  {
    protected override GeneralPath CreatePath(IStripe stripe, IRectangle layout) {
      var row = (IRow)stripe;
      GeneralPath outline = new GeneralPath();
      outline.MoveTo(0, 0);
      outline.LineTo(0, layout.Height);
      outline.LineTo(layout.Width, layout.Height);
      if (IsFirst(row)) {
        outline.LineTo(layout.Width, 2*WedgeHeight);
        outline.LineTo(layout.Width + WedgeWidth, WedgeHeight);
        outline.LineTo(layout.Width, 0);
        outline.Close();
      } else {
        outline.LineTo(layout.Width, 0);
      }
      return outline;
    }

    private bool IsFirst(IRow row) {
      ITable t = row.Table;
      return t != null && t.RootRow.ChildRows.First() == row;
    }
  }

  /// <summary>
  /// Custom style that provides a BPMN like visualization for columns
  /// </summary>
  public class BPMNColumnStyle : BPMNStyle
  {
    protected override GeneralPath CreatePath(IStripe stripe, IRectangle layout) {
      var column = (IColumn)stripe;
      GeneralPath outline = new GeneralPath();
      //Left border:
      outline.MoveTo(0, 0);
      outline.LineTo(layout.Width, 0);
      outline.LineTo(layout.Width + WedgeWidth, WedgeHeight);
      outline.LineTo(layout.Width, 2 * WedgeHeight);
      outline.LineTo(layout.Width, layout.Height);
      outline.LineTo(0, layout.Height);

      if (IsFirst(column)) {
        outline.Close();
      } else {
        outline.LineTo(0, layout.Height);
        outline.LineTo(0, 2 * WedgeHeight);
        outline.LineTo(WedgeWidth, WedgeHeight);
        outline.Close();
      }

      return outline;
    }

    private bool IsFirst(IColumn col) {
      ITable t = col.Table;
      return t != null && t.RootColumn.ChildColumns.First() == col;
    }
  }

}
