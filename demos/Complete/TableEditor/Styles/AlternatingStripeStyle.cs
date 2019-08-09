/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.Graph.TableEditor.Style
{
  /// <summary>
  /// Custom stripe style that alternates the visualizations for the leaf nodes and uses a different style for all parent stripes.
  /// </summary>
  public class AlternatingLeafStripeStyle : StripeStyleBase<VisualGroup>
  {
    /// <summary>
    /// Visualization for all leaf stripes that have an even index
    /// </summary>
    public StripeDescriptor EvenLeafDescriptor { get; set; }

    /// <summary>
    /// Visualization for all stripes that are not leafs
    /// </summary>
    public StripeDescriptor ParentDescriptor { get; set; }

    /// <summary>
    /// Visualization for all leaf stripes that have an odd index
    /// </summary>
    public StripeDescriptor OddLeafDescriptor { get; set; }

    protected override VisualGroup CreateVisual(IRenderContext context, IStripe stripe) {
      var layout = stripe.Layout.ToRectD();
      var cc = new VisualGroup() { AllowDrop = true };
      Thickness stripeInsets;

      StripeDescriptor descriptor;
      //Depending on the stripe type, we need to consider horizontal or vertical insets
      if (stripe is IColumn) {
        var col = (IColumn) stripe;
        stripeInsets = new Thickness(0, col.GetActualInsets().Top, 0, col.GetActualInsets().Bottom);
      } else {
        var row = (IRow) stripe;
        stripeInsets = new Thickness(row.GetActualInsets().Left, 0, row.GetActualInsets().Right, 0);
      }

      Thickness actualBorderThickness;

      if (stripe.GetChildStripes().Any()) {
        //Parent stripe - use the parent descriptor
        descriptor = ParentDescriptor;
        actualBorderThickness = descriptor.BorderThickness;
      } else {
        int index;
        if (stripe is IColumn) {
          var col = (IColumn) stripe;
          //Get all leaf columns
          var leafs = col.Table.RootColumn.GetLeaves().ToList();
          //Determine the index
          index = leafs.FindIndex((curr) => col == curr);
          //Use the correct descriptor
          descriptor = index % 2 == 0 ? EvenLeafDescriptor : OddLeafDescriptor;
          actualBorderThickness = descriptor.BorderThickness;
        } else {
          var row = (IRow) stripe;
          var leafs = row.Table.RootRow.GetLeaves().ToList();
          index = leafs.FindIndex((curr) => row == curr);
          descriptor = index % 2 == 0 ? EvenLeafDescriptor : OddLeafDescriptor;
          actualBorderThickness = descriptor.BorderThickness;
        }
      }

      {
        cc.Add(new Border
        {
          Background = descriptor.BackgroundBrush,
          BorderBrush = descriptor.InsetBrush,
          BorderThickness = stripeInsets,
          Width = layout.Width,
          Height = layout.Height,
          AllowDrop = true
        });
        cc.Add(new Border
        {
          Background = Brushes.Transparent,
          BorderBrush = descriptor.BorderBrush,
          BorderThickness = actualBorderThickness,
          Width = layout.Width,
          Height = layout.Height,
          AllowDrop = true
        });
      }
      cc.SetCanvasArrangeRect(layout.ToRectD().ToRect());
      var renderData = CreateRenderDataCache(context, descriptor, stripe, stripeInsets);
      cc.SetRenderDataCache(renderData);
      return cc;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IStripe stripe) {
      var layout = stripe.Layout.ToRectD();
      Thickness stripeInsets;
      //Check if values have changed - then update everything
      StripeDescriptor descriptor;
      if (stripe is IColumn) {
        var col = (IColumn) stripe;
        stripeInsets = new Thickness(0, col.GetActualInsets().Top, 0, col.GetActualInsets().Bottom);
      } else {
        var row = (IRow) stripe;
        stripeInsets = new Thickness(row.GetActualInsets().Left, 0, row.GetActualInsets().Right, 0);
      }

      Thickness actualBorderThickness;

      if (stripe.GetChildStripes().Any()) {
        descriptor = ParentDescriptor;
        actualBorderThickness = descriptor.BorderThickness;
      } else {
        int index;
        if (stripe is IColumn) {
          var col = (IColumn) stripe;
          var leafs = col.Table.RootColumn.GetLeaves().ToList();
          index = leafs.FindIndex((curr) => col == curr);
          descriptor = index % 2 == 0 ? EvenLeafDescriptor : OddLeafDescriptor;
          actualBorderThickness = descriptor.BorderThickness;
        } else {
          var row = (IRow) stripe;
          var leafs = row.Table.RootRow.GetLeaves().ToList();
          index = leafs.FindIndex((curr) => row == curr);
          descriptor = index % 2 == 0 ? EvenLeafDescriptor : OddLeafDescriptor;
          actualBorderThickness = descriptor.BorderThickness;
        }
      }

      // get the data with which the oldvisual was created
      var oldCache = oldVisual.GetRenderDataCache<RenderDataCache>();
      // get the data for the new visual
      RenderDataCache newCache = CreateRenderDataCache(context, descriptor, stripe, stripeInsets);

      // check if something changed except for the location of the node
      if (!newCache.Equals(oldCache)) {
        // something changed - just re-render the visual
        return CreateVisual(context, stripe);
      }

      Border borderVisual = (Border) oldVisual.Children[0];
      borderVisual.Width = layout.Width;
      borderVisual.Height = layout.Height;
      borderVisual.BorderThickness = stripeInsets;

      Border stripeVisual = (Border) oldVisual.Children[1];
      stripeVisual.Width = layout.Width;
      stripeVisual.Height = layout.Height;
      stripeVisual.BorderThickness = actualBorderThickness;
      oldVisual.SetCanvasArrangeRect(layout.ToRectD().ToRect());
      return oldVisual;
    }

#pragma warning disable 659
    /// <summary>
    /// Helper class to cache rendering related data
    /// </summary>
    private sealed class RenderDataCache
    {
      private readonly StripeDescriptor descriptor;
      private readonly Thickness insets;
      private readonly IStripe stripe;

      public StripeDescriptor Descriptor {
        get { return descriptor; }
      }

      public Thickness Insets {
        get { return insets; }
      }

      public IStripe Stripe {
        get { return stripe; }
      }

      public RenderDataCache(StripeDescriptor descriptor, IStripe stripe, Thickness insets) {
        this.descriptor = descriptor;
        this.stripe = stripe;
        this.insets = insets;
      }

      public bool Equals(RenderDataCache other) {
        return other.Descriptor == Descriptor && other.Insets == Insets && other.Stripe == Stripe;
      }


      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
          return false;
        }
        if (obj.GetType() != typeof(RenderDataCache)) {
          return false;
        }
        return Equals((RenderDataCache)obj);
      }
    }
#pragma warning restore 659

    private static RenderDataCache CreateRenderDataCache(IRenderContext context, StripeDescriptor descriptor, IStripe stripe, Thickness insets) {
      return new RenderDataCache(descriptor, stripe, insets);
    }
  }
}
