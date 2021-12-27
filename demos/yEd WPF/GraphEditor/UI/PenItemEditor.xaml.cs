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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Demo.yFiles.Option;
using Demo.yFiles.Option.Editor;
using yWorks.Annotations;

namespace Demo.yFiles.GraphEditor.UI
{
  /// <summary>
  /// Interaction logic for PenItemEditor.xaml
  /// </summary>
  public partial class PenItemEditor : UserControl
  {


    private void OnThicknessChanged(Pen oldThickness, Pen newThickness) {
      if (updatingPen) {
        return;
      }
      Pen oldPen = Pen;
      Pen newPen = oldPen != null ? oldPen.CloneCurrentValue() : new Pen();
      newPen.Thickness = newThickness.Thickness;
      newPen.Freeze();
      Pen = newPen;
    }

    private void OnDashStyleChanged(Pen oldDashStyle, Pen newDashStyle) {
      if (updatingPen) {
        return;
      }
      Pen oldPen = Pen;
      Pen newPen = oldPen != null ? oldPen.CloneCurrentValue() : new Pen();
      newPen.DashStyle = newDashStyle.DashStyle;
      newPen.Freeze();
      Pen = newPen;
    }

    private void OnLineColorChanged(object oldColor, object newColor) {
      if (updatingPen) {
        return;
      }
      
      Pen oldPen = Pen;
      Pen newPen = oldPen != null ? oldPen.CloneCurrentValue() : new Pen();
      if (newColor is Color) {
        newPen.Brush = new SolidColorBrush((Color)newColor);
      } else {
        // no color - 
        if (AllowNull) {
          Pen = null;
          return;
        } 
        newPen.Brush = new SolidColorBrush(Colors.Transparent);
      }
      newPen.Freeze();
      Pen = newPen;
    }

    #region PenProperty

    /// <summary>
    /// Dependency property for the <see cref="Pen"/> property.
    /// </summary>
    public static readonly DependencyProperty AllowNullProperty = DependencyProperty.Register("AllowNull", typeof (bool), typeof (PenItemEditor),
                                                                                      new FrameworkPropertyMetadata(
                                                                                        false));

    public bool AllowNull {
      get { return (bool) GetValue(AllowNullProperty); }
      set { SetValue(AllowNullProperty, value); }
    }


    /// <summary>
    /// Dependency property for the <see cref="Pen"/> property.
    /// </summary>
    public static readonly DependencyProperty PenProperty = DependencyProperty.Register("Pen", typeof (Pen), typeof (PenItemEditor),
                                                                                      new FrameworkPropertyMetadata(
                                                                                        new Pen(Brushes.Transparent, 0),
                                                                                        FrameworkPropertyMetadataOptions
                                                                                          .BindsTwoWayByDefault, OnPenPropertyChanged));

    private bool updatingPen;

    /// <seealso cref="PenProperty"/>
    public Pen Pen {
      get { return (Pen) GetValue(PenProperty); }
      set { SetValue(PenProperty, value); }
    }

    private static void OnPenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ((PenItemEditor) d).OnPenChanged((Pen) e.OldValue, (Pen) e.NewValue);
    }

    private void OnPenChanged([CanBeNull] Pen oldPen, [CanBeNull] Pen newPen) {
      if (updatingPen) {
        return;
      }
      updatingPen = true;
      try {
        IEnumerable<Pen> dashStyles = (IEnumerable<Pen>) TryFindResource("DashStyles");

        if (dashStyles != null) {
          foreach (Pen pen in dashStyles) {
            if (newPen == null || pen.DashStyle.Equals(newPen.DashStyle)) {
              dashPresenter.SelectedValue = pen;
              break;
            }
          }
        }

        IEnumerable<Pen> thicknesses = (IEnumerable<Pen>) TryFindResource("Thicknesses");
        if (thicknesses != null) {
          foreach (Pen pen in thicknesses) {
            if (newPen == null || pen.Thickness == newPen.Thickness) {
              thicknessPresenter.SelectedValue = pen;
              break;
            }
          }
        }

        if (newPen != null && newPen.Brush is SolidColorBrush) {
          colorPresenter.Value = ((SolidColorBrush) newPen.Brush).Color;
        } else {
          colorPresenter.Value=ColorChooser.NullColorObject;
        }
        penPresenter.Content = newPen;
      } finally {
        updatingPen = false;
      }
    }

    #endregion

    public PenItemEditor() {
      InitializeComponent();
      
      ObservableCollection<Pen> thicknesses = new ObservableCollection<Pen>();
      double[] t = new double[]{1,2,3,4,5,6,7,8,9,10,15,20,0};
      foreach (double d in t) {
        Pen item = new Pen(Brushes.Black, d);
        // don't freeze - causes bug in Selector.SetItemIsSelected
//        item.Freeze();
        thicknesses.Add(item);
      }

      DashStyle[] ds = new DashStyle[]{DashStyles.Solid, DashStyles.Dot, DashStyles.Dash, DashStyles.DashDot, DashStyles.DashDotDot};

      ObservableCollection<Pen> dashStyles = new ObservableCollection<Pen>();
      foreach (DashStyle style  in ds) {
        Pen item = new Pen(Brushes.Black, 3);
        item.DashStyle = style;
        // don't freeze - causes bug in Selector.SetItemIsSelected
        //        item.Freeze();
        dashStyles.Add(item);
      }

      Resources.Add("Thicknesses" , thicknesses);
      Resources.Add("DashStyles" , dashStyles);
    }



    private void thicknessChanged(object sender, SelectionChangedEventArgs e) {
      OnThicknessChanged(null, thicknessPresenter.SelectedValue as Pen);
    }

    private void dashChanged(object sender, SelectionChangedEventArgs e) {
      OnDashStyleChanged(null, dashPresenter.SelectedValue as Pen);
    }

    private void valueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
      OnLineColorChanged(e.OldValue, e.NewValue);
    }

    private void PenItemEditor_OnLoaded(object sender, RoutedEventArgs e) {
//      Pen newPen = new Pen(Brushes.Black, 1);
//      newPen.Freeze();
//      Pen = newPen;
    }
  }
}
