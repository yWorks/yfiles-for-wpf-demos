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

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Tooltips
{
  public static class PopupConfigurator
  {
    /// <summary>
    ///   Configures an exclusive, light-dismissable, detail popover when a node is clicked, alongside the default tooltip
    ///   behavior.
    ///   Note that the <see cref="PopoverBehavior.Auto"/> causes automatic closing of any other
    ///   <see cref="PopoverBehavior.Auto"/> popover, while still allowing tooltips that use <see cref="PopoverBehavior.Hint"/>
    ///   .
    ///   <see cref="PopoverBehavior.Auto"/> also causes the popover to be light-dismissable. So pressing ESC or clicking
    ///   outside the
    ///   popover also closes it automatically.
    /// </summary>
    /// <param name="sender">The input mode that triggered the click event.</param>
    /// <param name="evt">The item clicked event.</param>
    public static async Task ConfigureLightDismissablePopover(
        GraphInputMode sender,
        ItemClickedEventArgs<IModelItem> evt) {
      if (evt.Item is INode node) {
        // indicate that the event opened a popover, so the event won't cause the selection of the node
        evt.Handled = true;

        var layout = node.Layout;

        // the descriptor manages the popover's behavior and placement
        var popoverDescriptor = new PopoverDescriptor {
            // place it above the node
            Anchor = new PointD(layout.X + layout.Width * 0.5, layout.Y),
            Ratios = new PointD(0.5, 1),
            Offset = new PointD(0, -10),

            // make this popover exclusive and light-dismissable
            Behavior = PopoverBehavior.Auto
        };

        // assign the content to the popover descriptor
        popoverDescriptor.Content = CreatePopoverContent(evt.Context.CanvasControl, node);

        // open the popover
        await sender.PopoverManager.Open(popoverDescriptor);
      }
    }

    /// <summary>
    ///   Configures tooltips to appear centered above the node for which the tooltip is shown.
    /// </summary>
    /// <param name="evt">The tooltip event.</param>
    public static void ConfigureAboveNodeTooltip(QueryItemToolTipEventArgs<IModelItem> evt) {
      // adjust the placement of the tooltip
      if (evt.Item is INode node) {
        // centered above the node bounds
        var layout = node.Layout;
        evt.Popover.Anchor = new PointD(layout.X + layout.Width * 0.5, layout.Y);
        evt.Popover.Offset = new PointD(0, -5);  // five pixel view offset
        evt.Popover.Ratios = new PointD(0.5, 1); // anchored bottom center
      }
    }

    /// <summary>
    ///   Configures tooltips to follow the pointer when moving on the same item.
    /// </summary>
    public static void ConfigureFollowPointer(QueryItemToolTipEventArgs<IModelItem> evt) {
      // show the current location in the tooltip content
      if (evt.Popover.Content is not UIElement element) {
        return;
      }

      var locationNode = LogicalTreeHelper.FindLogicalNode(element, "Location");
      if (locationNode is not TextBlock locationText) {
        return;
      }

      var location = evt.QueryLocation;
      locationText.Text = $"Current Location: {location.X}, {location.Y}";

      // update the tooltip location when the pointer position changes
      evt.Popover.Update += (_, args) => {
        // position the popover at the current location
        args.Popover.Anchor = args.QueryLocation;

        // update the location content in the tooltip
        locationText.Text = $"Current Location: {args.QueryLocation.X}, {args.QueryLocation.Y}";
      };
    }

    /// <summary>
    ///   Configures a developer maintained detail popover when a node is clicked, alongside the default tooltip
    ///   behavior.
    ///   Note that the <see cref="PopoverBehavior.Manual"/> makes the popover never closing automatically, and thus not
    ///   light-dismissable
    ///   either, while still allowing tooltips that use <see cref="PopoverBehavior.Hint"/>.
    ///   So in this case there can be multiple open popovers of this kind. To close the popover, either call
    ///   <see cref="PopoverDescriptor.Close"/> or <see cref="PopoverManager.CloseAll"/> if all popovers should be closed.
    /// </summary>
    /// <param name="sender">The input mode that triggered the click event.</param>
    /// <param name="evt">The item clicked event.</param>
    public static async Task ConfigureManuelPopover(GraphInputMode sender, ItemClickedEventArgs<IModelItem> evt) {
      if (evt.Item is INode node) {
        // indicate that the event opened a popover, so the event won't cause the selection of the node
        evt.Handled = true;

        var layout = node.Layout;

        // the descriptor manages the popover's behavior and placement
        var popoverDescriptor = new PopoverDescriptor {
            // place it above the node
            Anchor = new PointD(layout.X + layout.Width * 0.5, layout.Y),
            Ratios = new PointD(0.5, 1),
            Offset = new PointD(0, -10),

            // make this popover non-exclusive and manually maintained
            Behavior = PopoverBehavior.Manual
        };

        // assign the content to the popover descriptor
        popoverDescriptor.Content = CreatePopoverContent(evt.Context.CanvasControl, node);

        // open the popover
        await sender.PopoverManager.Open(popoverDescriptor);
      }
    }

    /// <summary>
    ///   Configures tooltips to appear centered at the bottom of the viewport.
    /// </summary>
    /// <param name="evt">The tooltip event.</param>
    public static void ConfigureViewportBottomCenterTooltip(QueryItemToolTipEventArgs<IModelItem> evt) {
      var graphControl = (GraphControl) evt.Context.CanvasControl;

      // place the tooltip centered at the bottom of the viewport
      evt.Popover.Anchor =
          graphControl.ViewToWorldCoordinates(new PointD(graphControl.ActualWidth * 0.5, graphControl.ActualHeight));
      // move the reference point on the tooltip content to the bottom center
      evt.Popover.Ratios = new PointD(0.5, 1);
      evt.Popover.Offset = new PointD(0, -30);

      // when the viewport changes (e.g., zooming while the tooltip is opened), reposition the tooltip
      evt.Popover.Update += (_, updateArgs) => {
        if (!updateArgs.Handled && updateArgs.Reason == PopoverUpdateReason.ViewportChanged) {
          evt.Popover.Anchor =
              graphControl.ViewToWorldCoordinates(new PointD(graphControl.ActualWidth * 0.5,
                  graphControl.ActualHeight));
        }
      };
    }

    /// <summary>
    ///   The tooltip may either be a plain string or it can also be a WPF UI element. In this case, we
    ///   show the latter. We extract the last label text from the given node and show it as
    ///   a tooltip.
    /// </summary>
    public static UIElement? CreateTooltipContent(FrameworkElement control, INode node) {
      var content = CreateContent(control, "TooltipContent");
      content?.DataContext = node;
      return content;
    }

    private static FrameworkElement? CreatePopoverContent(FrameworkElement control, INode node) {
      var content = CreateContent(control, "PopoverContent");
      content?.DataContext = node;
      return content;
    }

    private static FrameworkElement? CreateContent(FrameworkElement control, string resourceKey) {
      var resource = control.TryFindResource(resourceKey);
      return resource is DataTemplate template ? template.LoadContent() as FrameworkElement : null;
    }
  }
}
