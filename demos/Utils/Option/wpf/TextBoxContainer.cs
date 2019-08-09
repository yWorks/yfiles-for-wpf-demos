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

using System.Windows;
using System.Windows.Controls;

namespace Demo.yFiles.Option.View
{
  public class TextBoxContainer : Decorator
  {

    static TextBoxContainer() {
      FocusableProperty.OverrideMetadata(typeof(TextBoxContainer), new FrameworkPropertyMetadata(false));
    }

    protected override Size MeasureOverride(Size constraint) {
      var child = Child;
      if (child != null) {
        child.Measure(constraint);
        if (child is FrameworkElement) {
          return new Size(((FrameworkElement) child).MinWidth, child.DesiredSize.Height);
        } else {
          return child.DesiredSize;
        }
      } else {
        return new Size(0, 0);
      }
    }

    protected override Size ArrangeOverride(Size arrangeSize) {
      var child = Child;
      if (child != null) {
        if (child is FrameworkElement) {
          ((FrameworkElement) child).Width = arrangeSize.Width;
        }
        child.Arrange(new Rect(0,0,arrangeSize.Width, arrangeSize.Height));
      }
      return arrangeSize;
    }
  }
}