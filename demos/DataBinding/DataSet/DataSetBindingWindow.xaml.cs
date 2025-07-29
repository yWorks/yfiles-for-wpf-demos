/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Demo.yFiles.DataBinding.DataSetBinding.Properties;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout.Tree;

namespace Demo.yFiles.DataBinding.DataSetBinding
{
  /// <summary>
  ///   Shows how to bind a <see cref="yWorks.Graph.DataBinding.TreeSource"/> to a
  ///   <see cref="System.Data.DataSet"/>.
  /// </summary>
  public partial class DataSetBindingWindow : Window
  {
    public DataSetBindingWindow() {
      InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e) {
      await DoLayout(graphControl);
    }

    /// <summary>
    ///   Does an animated tree layout of the graph displayed by a graph control guided by the aspect ratio
    ///   of the graph control.
    /// </summary>
    /// <param name="graphControl">The graph control</param>
    public static async Task DoLayout(GraphControl graphControl) {
      if (graphControl == null) {
        return;
      }
      var size = graphControl.InnerSize;
      var ratio = size.Width / size.Height;
      var layout = new TreeLayout {
          DefaultSubtreePlacer = new AspectRatioSubtreePlacer { AspectRatio = ratio },
          ComponentLayout = { PreferredSize = new SizeD(graphControl.InnerSize.Width, graphControl.InnerSize.Height) }
      };

      await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(500));
    }
  }

  /// <summary>
  ///   This class constructs a <see cref="System.Data.DataSet"/> containing the business objects from the resources.
  ///   These business objects are the Contacts in this example.
  /// </summary>
  public sealed class EmployeeRepository
  {
    public DataView NodesSource { get; }

    public EmployeeRepository() {
      // Construct a DataSet using the schema and the XML data from the resources.
      var adventureWorksEmployees = new DataSet("AdventureWorksEmployees");
      var schema = Resources.AdventureWorksEmployeesSchema;
      var schemaReader = new StringReader(schema);
      adventureWorksEmployees.ReadXmlSchema(schemaReader);
      var data = Resources.AdventureWorksEmployees;
      var dataReader = new StringReader(data);
      adventureWorksEmployees.ReadXml(dataReader);

      // Set the NodesSource property to a data view on the Contact table.
      var contactTable = adventureWorksEmployees.Tables["Contact"];
      NodesSource = contactTable.AsDataView();
    }
  }

  public sealed class EmployeeContactConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      // Convert a collection of Employees to a collection of the corresponding Contacts.
      if (value is DataView { Count: > 0 } employees) {
        employees.Sort = "ContactID";
        var contactTable = employees.Table.DataSet.Tables["Contact"];
        return contactTable.AsEnumerable().Where(row => employees.Find(row["ContactID"]) > -1).AsDataView();
      }
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  ///   This class is used by the TreeSource to avoid duplicate nodes.
  /// </summary>
  /// <remarks>
  ///   A tree source uses the business objects provided by its NodesSource property
  ///   to construct the nodes of the graph. Moreover, for each business object for which
  ///   a node is constructed, the ChildBinding property is used to look for further
  ///   node sources to add child nodes and to create the edges. In principle, you could
  ///   set the NodeSources property to a collection which contains just the CEO, but you do not
  ///   have to. The tree source keeps track of the node sources for which nodes have already
  ///   been constructed. Therefore, it needs to decide whether two node sources are equal.
  ///   Since the DataSet does not return identical instances of Contact rows, if you
  ///   iterate over the table or follow the relations, the following comparer is in fact needed
  ///   in this example.
  /// </remarks>
  public sealed class ContactComparer : IEqualityComparer<object>
  {
    public new bool Equals(object x, object y) {
      return x is DataRowView view1 && y is DataRowView view2
          ? view1.Row.Field<int>("ContactID").Equals(view2.Row.Field<int>("ContactID"))
          : x == y;

    }

    public int GetHashCode(object o) {
      return o is DataRowView view ? view.Row.Field<int>("ContactID") : 0;
    }
  }
}
