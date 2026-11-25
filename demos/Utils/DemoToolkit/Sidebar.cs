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

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace yWorks.Demo
{
  /// <summary>
  ///   A collapsible sidebar for use in a <see cref="Demo"/>.
  /// </summary>
  public class SideBar : Expander
  {
    /// <summary>
    ///   Gets or sets whether the sidebar can be collapsed by the user.
    /// </summary>
    /// <remarks>
    ///   When <see langword="false"/>, the collapse button is hidden and the sidebar remains expanded.
    /// </remarks>
    public bool IsCollapsible {
      get => (bool) GetValue(IsCollapsibleProperty);
      set => SetValue(IsCollapsibleProperty, value);
    }

    public static readonly DependencyProperty IsCollapsibleProperty = DependencyProperty.Register(nameof(IsCollapsible),
        typeof(bool), typeof(SideBar),
        new FrameworkPropertyMetadata(true,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure,
            OnIsCollapsibleChanged));

    /// <summary>
    ///   Gets or sets the width of the sidebar when expanded.
    /// </summary>
    /// <remarks>
    ///   The width is automatically calculated when the sidebar is collapsed or when the expanded width is set to
    ///   <see cref="double.NaN"/>.
    /// </remarks>
    [TypeConverter(typeof(LengthConverter))]
    public double ExpandedWidth {
      get => (double) GetValue(ExpandedWidthProperty);
      set => SetValue(ExpandedWidthProperty, value);
    }

    public static readonly DependencyProperty ExpandedWidthProperty = DependencyProperty.Register(nameof(ExpandedWidth),
        typeof(double), typeof(SideBar),
        new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

    static SideBar() {
      IsExpandedProperty.OverrideMetadata(typeof(SideBar),
          new FrameworkPropertyMetadata(IsExpandedProperty.DefaultMetadata.DefaultValue,
              FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure,
              null, CoerceIsExpanded));
      DefaultStyleKeyProperty.OverrideMetadata(typeof(SideBar), new FrameworkPropertyMetadata(typeof(SideBar)));
    }

    private static void OnIsCollapsibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      d.CoerceValue(IsExpandedProperty);
    }

    private static object CoerceIsExpanded(DependencyObject d, object baseValue) {
      var sideBar = (SideBar) d;
      return sideBar.IsCollapsible ? baseValue : true;
    }

    protected override Size MeasureOverride(Size constraint) {
      var baseSize = base.MeasureOverride(constraint);
      if (Visibility is Visibility.Collapsed) {
        return Size.Empty;
      }
      if (!IsExpanded || double.IsNaN(ExpandedWidth)) {
        return baseSize;
      }
      return new Size(ExpandedWidth, baseSize.Height);
    }
  }
}
