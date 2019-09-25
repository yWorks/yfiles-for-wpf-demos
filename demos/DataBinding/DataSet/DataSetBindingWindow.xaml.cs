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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using yWorks.Algorithms.Geometry;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Tree;

namespace Demo.yFiles.DataBinding.DataSetBinding
{
  /// <summary>
  /// Interaction logic for GraphSourceDemoWindow.xaml
  /// </summary>
  public partial class DataSetBindingWindow : Window
  {
    #region LayoutCommand triggered by the "Fit Contents" button

    static DataSetBindingWindow()
    {
      CommandManager.RegisterClassCommandBinding(
        typeof(DataSetBindingWindow), new CommandBinding(LayoutCommand, OnLayoutCommandExecuted));
    }

    public static readonly RoutedCommand LayoutCommand = new RoutedCommand();

    private static async void OnLayoutCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
      DataSetBindingWindow graphSourceDemoWindow = sender as DataSetBindingWindow;
      if (graphSourceDemoWindow != null) {
        await DoLayout(graphSourceDemoWindow.graphControl);
        e.Handled = true;
      }
    }

    #endregion

    public DataSetBindingWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) {
      await DoLayout(graphControl);
    }

    /// <summary>
    /// Does an animated tree layout of the graph displayed by a graph control guided by the aspect ratio
    /// of the graph control.
    /// </summary>
    /// <param name="graphControl">the graph control</param>
    public static async Task DoLayout(GraphControl graphControl)
    {
      if (graphControl == null || graphControl.Graph == null)
      {
        return;
      }
      var layout = new AspectRatioTreeLayout();
      System.Windows.Size size = graphControl.InnerSize;
      double ratio = size.Width / (1.0 * size.Height);
      layout.AspectRatio = ratio;
      ((ComponentLayout)layout.ComponentLayout).PreferredSize =
        new YDimension(graphControl.InnerSize.Width, graphControl.InnerSize.Height);

      await graphControl.MorphLayout(layout, TimeSpan.FromMilliseconds(500), null);
    }
  }

  /// <summary>
  /// This class constructs a DataSet containing the business objects from the resources.
  /// These business objects are the Contacts in this example.
  /// </summary>
  public class EmployeeRepository
  {
    private readonly DataView nodesSource;
    public DataView NodesSource
    {
      get { return nodesSource; }
    }

    public EmployeeRepository()
    {
      // Construct a DataSet using the schema and the XML data from the resources.
      DataSet adventureWorksEmployees = new DataSet("AdventureWorksEmployees");
      string schema = Properties.Resources.AdventureWorksEmployeesSchema;
      StringReader schemaReader = new StringReader(schema);
      adventureWorksEmployees.ReadXmlSchema(schemaReader);
      string data = Properties.Resources.AdventureWorksEmployees;
      StringReader dataReader = new StringReader(data);
      adventureWorksEmployees.ReadXml(dataReader);

      // Set the NodesSource property to a data view on the Contact table.
      DataTable contactTable = adventureWorksEmployees.Tables["Contact"];
      nodesSource = contactTable.AsDataView();
    }
  }

  public class EmployeeContactConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      // Convert a collection of Employees to a collection of the corresponding Contacts.
      DataView employees = value as DataView;
      if (employees != null && employees.Count > 0)
      {
        employees.Sort = "ContactID";
        DataTable contactTable = employees.Table.DataSet.Tables["Contact"];
        return contactTable.AsEnumerable().Where(row => employees.Find(row["ContactID"]) > -1).AsDataView();
      }
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }

  /// <summary>
  /// This class is used by the TreeSource to avoid duplicate nodes.
  /// </summary>
  /// <remarks>
  /// A tree source uses the business objects provided by its NodesSource property
  /// to construct the nodes of the graph. Moreover, for each business object for which
  /// a node is constructed the ChildBinding property is used to look for further
  /// node sources to add child nodes and to create the edges. In principle, you could
  /// set the NodeSources property to a collection which contains just the CEO, but you do not
  /// have to. The tree source keeps track of the node sources for which nodes have already
  /// been constructed. Therefore, it needs to decide whether two node sources are equal.
  /// Since the DataSet does not return identical instances of Contact rows, if you
  /// iterate over the table or follow the relations, the following comparer is in fact needed
  /// in this example.
  /// </remarks>
  public class ContactComparer : IEqualityComparer<object>
  {
    public new bool Equals(object x, object y)
    {
      DataRowView xobj = x as DataRowView;
      DataRowView yobj = y as DataRowView;
      if (xobj == null || yobj == null)
      {
        return xobj == null && yobj == null;
      }
      return xobj.Row.Field<int>("ContactID").Equals(yobj.Row.Field<int>("ContactID"));
    }

    public int GetHashCode(object x)
    {
      DataRowView xobj = x as DataRowView;
      if (xobj == null)
      {
        return 0;
      }
      return xobj.Row.Field<int>("ContactID");
    }
  }
}
