/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.TableNodeStyle.Style
{
  /// <summary>
  /// Simple style that provides alternating visualizations for even and odd stripes
  /// </summary>
  public class AlternatingStripeStyle : StripeStyleBase<VisualGroup>
  {
    public StripeDescriptor EvenStripeDescriptor { get; set; }
    public StripeDescriptor OddStripeDescriptor { get; set; }

    protected override VisualGroup CreateVisual(IRenderContext context, IStripe stripe) {
      var layout = stripe.Layout.ToRectD();
      var cc = new VisualGroup();
      Thickness stripeInsets;

      int index;
      if (stripe is IColumn) {
        var col = (IColumn) stripe;
        stripeInsets = new Thickness(0, col.GetActualInsets().Top, 0, col.GetActualInsets().Bottom);
        index = col.ParentColumn.ChildColumns.ToList().FindIndex((curr) => col == curr);
      } else {
        var row = (IRow) stripe;
        stripeInsets = new Thickness(row.GetActualInsets().Left, 0, row.GetActualInsets().Right, 0);
        index = row.ParentRow.ChildRows.ToList().FindIndex((curr) => row == curr);
      }
      StripeDescriptor descriptor = index % 2 == 0 ? EvenStripeDescriptor : OddStripeDescriptor;
      cc.Add(new Border
      {
        Background = descriptor.BackgroundBrush,
        BorderBrush = descriptor.InsetBrush,
        BorderThickness = stripeInsets,
        Width = layout.Width,
        Height = layout.Height
      });
      cc.Add(new Border
      {
        Background = Brushes.Transparent,
        BorderBrush = descriptor.BorderBrush,
        BorderThickness = descriptor.BorderThickness,
        Width = layout.Width,
        Height = layout.Height
      });
      cc.SetCanvasArrangeRect(layout.ToRectD());
      return cc;
    }


    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IStripe stripe) {
      var layout = stripe.Layout.ToRectD();
      if (oldVisual.Children.Count == 2) {
        Thickness stripeInsets;

        int index;
        if (stripe is IColumn) {
          var col = (IColumn) stripe;
          stripeInsets = new Thickness(0, col.GetActualInsets().Top, 0, col.GetActualInsets().Bottom);
          index = col.ParentColumn.ChildColumns.ToList().FindIndex((curr) => col == curr);
        } else {
          var row = (IRow) stripe;
          stripeInsets = new Thickness(row.GetActualInsets().Left, 0, row.GetActualInsets().Right, 0);
          index = row.ParentRow.ChildRows.ToList().FindIndex((curr) => row == curr);
        }
        StripeDescriptor descriptor = index % 2 == 0 ? EvenStripeDescriptor : OddStripeDescriptor;

        var border = (Border) oldVisual.Children[0];
        border.Background = descriptor.BackgroundBrush;
        border.BorderBrush = descriptor.InsetBrush;
        border.BorderThickness = stripeInsets;
        border.Width = layout.Width;
        border.Height = layout.Height;

        var border2 = (Border) oldVisual.Children[1];
        border2.Background = Brushes.Transparent;
        border2.BorderBrush = descriptor.BorderBrush;
        border2.BorderThickness = descriptor.BorderThickness;
        border2.Width = layout.Width;
        border2.Height = layout.Height;
        oldVisual.SetCanvasArrangeRect(layout.ToRectD());
        return oldVisual;
      } else {
        return CreateVisual(context, stripe);
      }
    }
  }
}
