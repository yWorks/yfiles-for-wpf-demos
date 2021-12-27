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

using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Demo.yFiles.DataBinding.InteractiveEdgesGraphSource
{
  /// <summary>
  /// Interaction logic for CreateNodeDialog.xaml.
  /// </summary>
  public partial class CreateNodeDialog
  {
    private readonly List<string> typeItems;
    private List<string> eventsList;
    private List<string> methodsList;

    public CreateNodeDialog() {
      InitializeComponent();
      nameBox.SelectAll();
      typeItems = new List<string> {"Class", "Interface"};
      typeCombobox.ItemsSource = typeItems;
      typeCombobox.SelectedIndex = 0;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) {
      NodeName = nameBox.Text.Trim();
      Type = GetItemType((string) typeCombobox.SelectedItem);

      // parse the text of the events textbox
      string[] events = eventsBox.Text.Split(new[] { '\n', '\r', ',', ';', ' ' });
      var evts =
        from evt in events
        where evt.Trim().Length > 0
        select evt.Trim();
      eventsList = evts.ToList();

      // parse the text of the methods textbox
      string[] methods = methodsBox.Text.Split(new[] { '\n', '\r', ',', ';', ' ' });
      var mths =
        from method in methods
        where method.Trim().Length > 0
        select method.Trim();
      methodsList = mths.ToList();

      this.DialogResult = !string.IsNullOrWhiteSpace(NodeName);
      this.Close();
    }

    private static ItemType GetItemType(string s) {
      switch(s) {
        case "Class":
          return ItemType.Class;
        case "Interface":
          return ItemType.Interface;
        default:
          return ItemType.Undefined;
      }
    }

    public string NodeName { get; private set; }

    public ItemType Type { get; private set; }

    public List<string> EventsList {
      get { return eventsList; }
    }

    public List<string> MethodsList {
      get { return methodsList; }
    }

    public enum ItemType
    {
      Class,
      Interface,
      Undefined
    } ;
  }
}