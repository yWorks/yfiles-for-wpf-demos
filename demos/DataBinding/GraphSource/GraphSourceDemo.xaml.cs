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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.GraphSource
{
  [Demo("graph-source", "data-binding", "Data Binding using a GraphSource",
      Summary = "Demonstrates data binding with GraphSource using XML data and assembly types.",
      Tags = ["data binding", "graph source"], Keywords = ["xml", "assembly", "hierarchical"])]
  public sealed partial class GraphSourceDemo
  {
    private static readonly ILabelModelParameter EdgeLabelLayoutParameter =
        FreeEdgeLabelModel.Instance.CreateParameter();

    public GraphSourceDemo() {
      InitializeComponent();
    }

    [STAThread]
    private static void Main() => Run<GraphSourceDemo>();

    protected override void Initialize() {
      // The sources for the "Organization" and "Classes" sample data providers are embedded resources.
      // Due to the way the demo framework loads XAML, embedded resources are not loaded automatically
      // and thus have to be loaded explicitly here.
      UpdateDataProvider("Staff");
      UpdateDataProvider("Classes");

      graphSourceComboBox.SelectedIndex = 0;
    }

    private void UpdateDataProvider(string key) {
      if (TryFindResource(key) is not XmlDataProvider provider) {
        return;
      }

      var source = provider.Source;
      if (source == null) {
        return;
      }

      var resourceName = source.ToString().Replace('\\', '.').Replace('/', '.');

      try {
        using var stream = LoadAssemblyResource(resourceName);
        var document = new XmlDocument();
        document.Load(stream);

        provider.Document = document;
      } catch {
        // do not update the data provider, if the resource cannot be loaded and parsed as an XML document
      }
    }

    private async void GraphSourceModelChanged(object sender, SelectionChangedEventArgs e) {
      var newGraph = GetGraphSource(graphSourceComboBox.SelectedIndex).Graph;

      // add some padding to group nodes
      newGraph.GetDecorator().Nodes.GroupPaddingProvider
          .AddConstant(newGraph.IsGroupNode, new CustomGroupPaddingProvider());

      // use a FreeEdgeLabelModel for the 'Classes' graph
      if (graphSourceComboBox.SelectedIndex == 1) {
        foreach (var edgeLabel in newGraph.GetEdgeLabels()) {
          newGraph.SetLabelLayoutParameter(edgeLabel, EdgeLabelLayoutParameter);
        }
      }

      graphControl.Graph = newGraph;

      // Perform an animated layout of the organization chart graph when the window is loaded.
      await graphControl.ApplyLayoutAnimated(
          new HierarchicalLayout {
              DefaultEdgeDescriptor = new HierarchicalLayoutEdgeDescriptor {
                  MinimumLength = 50,
                  RoutingStyleDescriptor = new RoutingStyleDescriptor(RoutingStyle.Polyline)
              },
              LayoutOrientation =
                  graphSourceComboBox.SelectedIndex == 0
                      ? LayoutOrientation.TopToBottom
                      : LayoutOrientation.BottomToTop,
              EdgeLabelPlacement =
                  graphSourceComboBox.SelectedIndex == 1 ? EdgeLabelPlacement.Integrated : EdgeLabelPlacement.Ignore
          }, TimeSpan.FromSeconds(2));
    }

    private Graph.DataBinding.GraphSource GetGraphSource(int index) {
      if (graphSourceComboBox.Items[index] is not ComboBoxItem { Tag: Graph.DataBinding.GraphSource tag }) {
        throw new ArgumentException("Invalid graph source");
      }
      return tag;
    }
  }

  /// <summary>
  ///   This is a value converter that enables XPath expressions which are not possible using only XAML.
  /// </summary>
  public sealed class XPathConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
      if (value is XmlAttribute attribute) {
        string xpath;
        if (parameter is string s) {
          xpath = s.Replace("{}", "'" + attribute.Value + "'");
        } else {
          xpath = "//[@id='" + attribute.Value + "']";
        }
        var nodes = attribute.OwnerDocument.SelectNodes(xpath);
        return nodes is { Count: > 0 } ? nodes[0] : value;
      }
      return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  /// <summary>
  ///   Converts a <see cref="Type"/> to a <see cref="Color"/>.
  /// </summary>
  public sealed class TypeToColorConverter : IValueConverter
  {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
      var type = value as string;
      var isGroup = bool.Parse(parameter as string ?? "");
      if (type != null) {
        if ("interface" == type) {
          return isGroup ? Colors.MediumSeaGreen : Colors.LightGreen;
        }
        if ("class" == type) {
          return isGroup ? Colors.Goldenrod : Colors.PaleGoldenrod;
        }
      }
      return Colors.White;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  internal sealed class CustomGroupPaddingProvider : IGroupPaddingProvider
  {
    public InsetsD GetPadding() => new(5, 20, 5, 5);
  }

  /// <summary>
  ///   A data holder for a selection of the <see cref="Type"/>s of an <see cref="Assembly"/>
  ///   as well as their inheritance <see cref="Relation"/>.
  /// </summary>
  internal sealed class AssemblyTypeData
  {
    /// <summary>
    ///   The <see cref="Type"/>s that are use as business model for the nodes.
    /// </summary>
    public IEnumerable<Type> Types { get; private set; }

    /// <summary>
    ///   Tuples of <see cref="Type"/>s that are used as business model for the edges.
    /// </summary>
    public IEnumerable<Tuple<Type, Type>> Relation { get; private set; }

    /// <summary>
    ///   Use the yFiles Viewer assembly per default
    /// </summary>
    public AssemblyTypeData() : this(typeof(IGraph).Assembly) { }

    public AssemblyTypeData(Assembly assembly) {
      // collect all public Types
      var types = assembly.GetExportedTypes().ToList();

      // collect all inheritance relations between any two types of the collected types
      var relation = new List<Tuple<Type, Type>>();
      var usedTypes = new HashSet<Type>();
      foreach (var type in types) {
        if (type.DeclaringType != null && types.Contains(type.DeclaringType)) {
          usedTypes.Add(type);
          usedTypes.Add(type.DeclaringType);
        }

        if (type.BaseType != null && types.Contains(type.BaseType)) {
          relation.Add(Tuple.Create(type, type.BaseType));
          usedTypes.Add(type);
          usedTypes.Add(type.BaseType);
        }
        foreach (var @interface in type.GetInterfaces()) {
          if (types.Contains(@interface)) {
            relation.Add(Tuple.Create(type, @interface));
            usedTypes.Add(type);
            usedTypes.Add(@interface);
          }
        }
      }
      // filter the Types in the assembly and only show those that belong to an inheritance or nesting relation
      Types = [.. types.Where(usedTypes.Contains)];
      Relation = relation;
    }
  }
}
