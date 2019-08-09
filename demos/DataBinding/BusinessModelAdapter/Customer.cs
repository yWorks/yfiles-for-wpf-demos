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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Demo.yFiles.Graph.BusinessModelAdapter
{
  /// <summary>
  /// Simple class that models a Customer and uses the DependencyObject
  /// mechanisms for databinding and defaults.
  /// </summary>
  public class Customer : DependencyObject
  {
    private readonly IList<Customer> relatedCustomers = new ObservableCollection<Customer>();

    public Customer() {
    }

    public Customer(string name, int age) {
      Name = name;
      Age = age;
    }

    public static readonly DependencyProperty NameProperty =
      DependencyProperty.Register("Name", typeof (string), typeof (Customer), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
 
    public string Name {
      get { return GetValue(NameProperty) as string; }
      set { SetValue(NameProperty, value);}
    }

    public static readonly DependencyProperty AgeProperty =
      DependencyProperty.Register("Age", typeof(int), typeof(Customer), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
 
    public int Age {
      get { return GetValue(AgeProperty) is int ? (int) GetValue(AgeProperty) : 0; }
      set { SetValue(AgeProperty, value);}
    }


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public IList<Customer> RelatedCustomers {
      get{ return relatedCustomers;}
    } 
  }
}
