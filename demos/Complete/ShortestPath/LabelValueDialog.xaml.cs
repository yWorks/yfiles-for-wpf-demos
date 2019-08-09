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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

namespace Demo.yFiles.Algorithms.ShortestPath
{
  /// <summary>
  /// Interaction logic for LabelValueDialog.xaml
  /// </summary>
  public partial class LabelValueDialog : INotifyPropertyChanged
  {
    public LabelValueDialog() {
      InitializeComponent();
      Value = 1;
      weightBox.SelectAll();
    }

    private double value;

    public double Value {
      get { return value; }
      set {
        this.value = value;
        OnPropertyChanged("Value");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(String property) {
      if (PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(property));
      }
    }

    private void Save_Executed(object sender, ExecutedRoutedEventArgs e) {
      this.DialogResult = true;
      this.Close();
    }

    private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = !Validation.GetHasError(weightBox);
      e.Handled = true;
    }
  }

  public class NonNegativeIntegerValidator : ValidationRule
  {
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
      string inputString = (value ?? string.Empty).ToString();
      double d;
      var result = double.TryParse(inputString, NumberStyles.Number, Thread.CurrentThread.CurrentUICulture, out d) &&
                   d >= 0
                     ? new ValidationResult(true, null)
                     : new ValidationResult(false, "Only non-negative numbers are allowed");
      return result;
    }
  }
}