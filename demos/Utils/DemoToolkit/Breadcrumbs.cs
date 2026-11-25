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

using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace yWorks.Demo
{
  public class Breadcrumbs : ItemsControl
  {
    static Breadcrumbs() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(Breadcrumbs), new FrameworkPropertyMetadata(typeof(Breadcrumbs)));
    }

    public Breadcrumbs() {
      Loaded += (_, __) => { UpdateLastItemFlags(); };
      ItemContainerGenerator.StatusChanged += (_, __) => {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
          UpdateLastItemFlags();
        }
      };

      // Bubble clicks from internal buttons as a single routed event.
      AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnAnyButtonClick), true);
    }

    // Routed event fired when a breadcrumb item is invoked.
    public static readonly RoutedEvent ItemInvokedEvent = EventManager.RegisterRoutedEvent(nameof(ItemInvoked),
        RoutingStrategy.Bubble, typeof(EventHandler<BreadcrumbItemInvokedEventArgs>), typeof(Breadcrumbs));

    public event EventHandler<BreadcrumbItemInvokedEventArgs> ItemInvoked {
      add => AddHandler(ItemInvokedEvent, value);
      remove => RemoveHandler(ItemInvokedEvent, value);
    }

    // DataTemplate for the separator drawn between items (e.g., a chevron).
    public DataTemplate SeparatorTemplate {
      get => (DataTemplate) GetValue(SeparatorTemplateProperty);
      set => SetValue(SeparatorTemplateProperty, value);
    }

    public static readonly DependencyProperty SeparatorTemplateProperty =
        DependencyProperty.Register(nameof(SeparatorTemplate), typeof(DataTemplate), typeof(Breadcrumbs),
            new PropertyMetadata(null));

    // By default, the last breadcrumb is not clickable.
    public bool IsLastItemClickable {
      get => (bool) GetValue(IsLastItemClickableProperty);
      set => SetValue(IsLastItemClickableProperty, value);
    }

    public static readonly DependencyProperty IsLastItemClickableProperty =
        DependencyProperty.Register(nameof(IsLastItemClickable), typeof(bool), typeof(Breadcrumbs),
            new PropertyMetadata(false));

    // Spacing around the separator visual.
    public Thickness SeparatorMargin {
      get => (Thickness) GetValue(SeparatorMarginProperty);
      set => SetValue(SeparatorMarginProperty, value);
    }

    public static readonly DependencyProperty SeparatorMarginProperty =
        DependencyProperty.Register(nameof(SeparatorMargin), typeof(Thickness), typeof(Breadcrumbs),
            new PropertyMetadata(new Thickness(8, 0, 8, 0)));

    protected override DependencyObject GetContainerForItemOverride() => new BreadcrumbItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is BreadcrumbItem;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
      base.PrepareContainerForItemOverride(element, item);
      if (element is BreadcrumbItem bi) {
        bi.IsLast = Items.IndexOf(item) == Items.Count - 1;
        bi.Owner = this;
      }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
      base.ClearContainerForItemOverride(element, item);
      if (element is BreadcrumbItem bi) {
        bi.IsLast = false;
        bi.Owner = null;
      }
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e) {
      base.OnItemsChanged(e);
      UpdateLastItemFlags();
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      UpdateLastItemFlags();
    }

    private void UpdateLastItemFlags() {
      var count = Items.Count;
      for (var i = 0; i < count; i++) {
        if (ItemContainerGenerator.ContainerFromIndex(i) is BreadcrumbItem bi) {
          bi.Index = i;
          bi.IsLast = i == count - 1;
        }
      }
    }


    private void OnAnyButtonClick(object sender, RoutedEventArgs e) {
      if (e.OriginalSource is DependencyObject d) {
        var container = FindAncestor<BreadcrumbItem>(d);
        if (container != null && IsContainerInvokable(container)) {
          RaiseItemInvoked(container);
          e.Handled = true;
        }
      }
    }

    internal void RaiseItemInvoked(BreadcrumbItem container) {
      var args = new BreadcrumbItemInvokedEventArgs(ItemInvokedEvent, this) {
          Item = ItemContainerGenerator.ItemFromContainer(container),
          Index = container.Index
      };
      RaiseEvent(args);
    }

    internal bool IsContainerInvokable(BreadcrumbItem bi) => bi.IsEnabled && (!bi.IsLast || IsLastItemClickable);

    private static T FindAncestor<T>(DependencyObject d) where T : DependencyObject {
      while (d != null) {
        if (d is T t) {
          return t;
        }
        d = VisualTreeHelper.GetParent(d);
      }
      return null;
    }
  }

  public class BreadcrumbItemInvokedEventArgs : RoutedEventArgs
  {
    public BreadcrumbItemInvokedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }

    public object Item { get; set; }

    public int Index { get; set; }
  }

  public class BreadcrumbItem : ContentControl
  {
    static BreadcrumbItem() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbItem),
          new FrameworkPropertyMetadata(typeof(BreadcrumbItem)));
    }

    internal Breadcrumbs Owner { get; set; }

    internal int Index { get; set; }

    public bool IsLast {
      get => (bool) GetValue(IsLastProperty);
      internal set => SetValue(IsLastPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsLastPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLast), typeof(bool), typeof(BreadcrumbItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsLastProperty = IsLastPropertyKey.DependencyProperty;

    public ICommand Command {
      get => (ICommand) GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command),
        typeof(ICommand), typeof(BreadcrumbItem), new PropertyMetadata(null));

    public object CommandParameter {
      get => GetValue(CommandParameterProperty);
      set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(BreadcrumbItem),
            new PropertyMetadata(null));

    public IInputElement CommandTarget {
      get => (IInputElement) GetValue(CommandTargetProperty);
      set => SetValue(CommandTargetProperty, value);
    }

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(nameof(CommandTarget),
        typeof(IInputElement), typeof(BreadcrumbItem), new PropertyMetadata(null));
  }
}
