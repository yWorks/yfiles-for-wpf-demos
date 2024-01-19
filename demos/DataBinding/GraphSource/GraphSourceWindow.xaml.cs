/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;

namespace Demo.yFiles.DataBinding.GraphSource
{
  public partial class GraphSourceWindow
  {
    private static readonly ILabelModelParameter edgeLabelLayoutParameter = FreeEdgeLabelModel.Instance.CreateDefaultParameter();

    public GraphSourceWindow() {
      InitializeComponent();
    }

    private void GraphSourceWindow_OnLoaded(object sender, RoutedEventArgs e) {
      graphSourceComboBox.SelectedIndex = 0;
    }

    private async void GraphSourceModelChanged(object sender, SelectionChangedEventArgs e) {
      var newGraph = GetGraphSource(graphSourceComboBox.SelectedIndex).Graph;

      // add some insets to group nodes
      newGraph.GetDecorator().NodeDecorator.InsetsProviderDecorator.SetImplementation(newGraph.IsGroupNode, new GroupNodeInsetsProvider());

      // use a FreeEdgeLabelModel for the 'Classes' graph
      if (graphSourceComboBox.SelectedIndex == 1) {
        foreach (var edgeLabel in newGraph.GetEdgeLabels()) {
          newGraph.SetLabelLayoutParameter(edgeLabel, edgeLabelLayoutParameter);
        }
      }

      graphControl.Graph = newGraph;

      // Perform an animated layout of the organization chart graph when the window is loaded.
      await graphControl.MorphLayout(new HierarchicLayout {
        EdgeLayoutDescriptor = new EdgeLayoutDescriptor {MinimumLength = 50},
        LayoutOrientation =
          graphSourceComboBox.SelectedIndex == 0 ? LayoutOrientation.TopToBottom : LayoutOrientation.BottomToTop,
        IntegratedEdgeLabeling = graphSourceComboBox.SelectedIndex == 1
      }, TimeSpan.FromSeconds(2));
    }

    private yWorks.Graph.DataBinding.GraphSource GetGraphSource(int index) {
      return (((ComboBoxItem) graphSourceComboBox.Items[index]).Tag as yWorks.Graph.DataBinding.GraphSource);
    }
  }

  /// <summary>
  /// This is a value converter which enables XPath expressions which are not possible using only XAML.
  /// </summary>
  public sealed class XPathConverter: IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      XmlAttribute attribute = value as XmlAttribute;
      if (attribute != null) {
        string xpath;
        if (parameter is string) {
          xpath = ((string) parameter).Replace("{}", "'"+attribute.Value+"'");
        } else {
          xpath = "//[@id='" + attribute.Value + "']";
        }
        var nodes = attribute.OwnerDocument.SelectNodes(xpath);
        return nodes[0];
      }
      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new System.NotImplementedException();
    }
  }

  /// <summary>
  /// Converts a <see cref="Type"/> to a <see cref="Color"/>.
  /// </summary>
  public sealed class TypeToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      String type = value as String;
      var isGroup = Boolean.Parse(parameter as String);
      if (type != null) {
        if ("interface" == type) {
          return isGroup ? Colors.MediumSeaGreen : Colors.LightGreen;
        } else if ("class" == type) {
          return isGroup ? Colors.Goldenrod : Colors.PaleGoldenrod;
        }
      }
      return Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  sealed class GroupNodeInsetsProvider : INodeInsetsProvider {
    public InsetsD GetInsets(INode node) {
      return new InsetsD(5, 20, 5, 5);
    }
  }

  /// <summary>
  /// A data holder for a selection of the <see cref="Type"/>s of an <see cref="Assembly"/>
  /// as well as their inheritance <see cref="Relation"/>.
  /// </summary>
  sealed class AssemblyTypeData
  {
    /// <summary>
    /// The <see cref="Type"/>s that are use as business model for the nodes.
    /// </summary>
    public IEnumerable<Type> Types { get; private set; }

    /// <summary>
    /// Tuples of <see cref="Type"/>s that are used as business model for the edges.
    /// </summary>
    public IEnumerable<Tuple<Type, Type>> Relation { get; private set; }

    /// <summary>
    /// Use the yFiles Viewer assembly per default
    /// </summary>
    public AssemblyTypeData(): this(typeof(IGraph).Assembly){}

    public AssemblyTypeData(Assembly assembly) {
      // collect all public Types
      var types = assembly.GetExportedTypes().ToList();

      // collect all inheritance relations between any two types of the collected types
      var relation = new List<Tuple<Type, Type>>();
      var usedTypes = new HashSet<Type>();
      foreach (var type in types) {
        if (types.Contains(type.DeclaringType)) {
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
      Types = types.Where(usedTypes.Contains).ToList();
      Relation = relation;
    }
  }
}
