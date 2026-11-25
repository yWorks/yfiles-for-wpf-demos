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

using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace yWorks.Demo
{
  public class FloatingButton : Expander
  {
    static FloatingButton() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatingButton),
          new FrameworkPropertyMetadata(typeof(FloatingButton)));
    }

    public object CollapsedHeader {
      get => GetValue(CollapsedHeaderProperty);
      set => SetValue(CollapsedHeaderProperty, value);
    }

    public static readonly DependencyProperty CollapsedHeaderProperty =
        DependencyProperty.Register(nameof(CollapsedHeader), typeof(object), typeof(FloatingButton),
            new FrameworkPropertyMetadata(null));
  }

  internal class FloatingButtonHeaderTemplateSelector : DataTemplateSelector
  {
    public DataTemplate? IconTemplate { get; set; } = null;

    public override DataTemplate? SelectTemplate(object item, DependencyObject container) {
      if (container is FrameworkElement element && item is string { Length: 1 } s &&
          char.GetUnicodeCategory(s[0]) is UnicodeCategory.PrivateUse) {
        return IconTemplate;
      }
      return null;
    }
  }
}
