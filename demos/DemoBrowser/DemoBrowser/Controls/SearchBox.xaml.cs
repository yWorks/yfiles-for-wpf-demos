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

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DemoBrowser.Controls
{
  /// <summary>
  ///   Combines a search editor, an optional filter selector, and a clear button.
  /// </summary>
  public partial class SearchBox : UserControl
  {
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
        typeof(SearchBox),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, null,
            false, UpdateSourceTrigger.PropertyChanged));

    public static readonly DependencyProperty FilterItemsSourceProperty = DependencyProperty.Register(
        nameof(FilterItemsSource), typeof(IEnumerable), typeof(SearchBox), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedFilterProperty = DependencyProperty.Register(
        nameof(SelectedFilter), typeof(object), typeof(SearchBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty FilterItemTemplateProperty = DependencyProperty.Register(
        nameof(FilterItemTemplate), typeof(DataTemplate), typeof(SearchBox), new PropertyMetadata(null));

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText), typeof(string), typeof(SearchBox), new PropertyMetadata("Search"));

    public SearchBox() {
      InitializeComponent();
    }

    public string Text {
      get => (string) GetValue(TextProperty);
      set => SetValue(TextProperty, value);
    }

    public IEnumerable? FilterItemsSource {
      get => (IEnumerable?) GetValue(FilterItemsSourceProperty);
      set => SetValue(FilterItemsSourceProperty, value);
    }

    public object? SelectedFilter {
      get => GetValue(SelectedFilterProperty);
      set => SetValue(SelectedFilterProperty, value);
    }

    public DataTemplate? FilterItemTemplate {
      get => (DataTemplate?) GetValue(FilterItemTemplateProperty);
      set => SetValue(FilterItemTemplateProperty, value);
    }

    public string PlaceholderText {
      get => (string) GetValue(PlaceholderTextProperty);
      set => SetValue(PlaceholderTextProperty, value);
    }

    private void OnClearCategoryClick(object sender, RoutedEventArgs e) {
      if (FilterItemsSource != null) {
        // Set the first filter item as the active one (assuming it's the equivalent of "no filter" or "all")
        foreach (var item in FilterItemsSource) {
          SetCurrentValue(SelectedFilterProperty, item);
          return;
        }
      }
      SetCurrentValue(SelectedFilterProperty, null);
    }
  }
}
