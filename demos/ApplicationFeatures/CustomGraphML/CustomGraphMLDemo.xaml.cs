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
using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace yWorks.Demo.CustomGraphML
{
  /// <summary>
  ///   Shows how to read and write additional data from and to GraphML.
  /// </summary>
  [Demo("custom-graphml", "application-features", "Custom Data in GraphML",
      Summary = "Shows how to read and write additional data from and to GraphML.", Tags = ["graphml", "i/o"],
      Keywords = ["IMapper", "GraphMLIOHandler", "data", "json", "read", "write", "files", "io"])]
  public sealed partial class CustomGraphMLDemo
  {
    /// <summary>
    ///   Symbolic name for the mapper that allows transparent access to the correct implementation even across
    ///   wrapped graphs.
    /// </summary>
    private const string DateTimeMapperKey = "DateTimeMapperKey";

    private readonly DictionaryMapper<INode, DateTime> dateMapper = new();

    public CustomGraphMLDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphControl.InputMode = new GraphEditorInputMode();

      // configures default styles for newly created graph elements
      InitializeTutorialDefaults(graphControl.Graph);

      // sets up the data binding that stores the current date when a node is created
      EnableDataBinding(graphControl.Graph);

      // then build the graph from the given data set
      BuildGraph(graphControl.Graph);

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // enable now the undo engine to prevent undoing of the graph creation
      graphControl.Graph.SetUndoEngineEnabled(true);

      // enable GraphML IO
      EnableGraphML();

      // displays tooltips for the stored data items, so that something is visible to the user
      SetupTooltips();
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json);
    }

    /// <summary>
    ///   Sets up simple data binding - creates an IMapper, registers it and subscribes to the node creation event
    ///   on the graph.
    /// </summary>
    private void EnableDataBinding(IGraph graph) {
      // Subscribes to the node creation event to record the node creation time.
      // Note that since this event is triggered after undo/redo, the time will
      // be updated during redo of node creations and undo of node deletions.
      // If this is unwanted behavior, you can customize the node creation itself
      // to associate this data with the element at the time of its initial creation,
      // e.g. by listening to the NodeCreated event of GraphEditorInputMode, see below
      graph.NodeCreated += (_, args) => {
        // Stores the current time as node creation time.
        dateMapper[args.Item] = DateTime.Now;
      };
    }

    /// <summary>
    ///   Enables loading and saving the graph to GraphML.
    /// </summary>
    private void EnableGraphML() {
      var graphMLSupport = new GraphMLSupport(graphControl) {
          // set a custom GraphMLIOHandler
          GraphMLIOHandler = CreateGraphMLIOHandler()
      };
      graphMLSupport.RegisterOpenSaveCommandBindings();
    }

    /// <summary>
    ///   Register input and output handlers that store the data in the mapper as GraphML attributes resp. can read them
    ///   back.
    /// </summary>
    private GraphMLIOHandler CreateGraphMLIOHandler() {
      // create an IOHandler that will be used for all IO operations
      var graphMLIOHandler = new GraphMLIOHandler();

      // The OutputHandler just stores the string value of the attribute
      // We need to provide the symbolic name of the attribute in the GraphML file, the data source as an IMapper and the
      // GraphML type of the attribute
      graphMLIOHandler.AddOutputMapper<INode, DateTime>(DateTimeMapperKey, "demo", dateMapper, (context, item, _) => {
        if (item != default) {
          context.Writer.WriteString(JsonSerializer.Serialize(item));
        }
      }, KeyType.String);

      // To read back a DateTime value from a string GraphML attribute, we have to provide an additional callback method.
      graphMLIOHandler.AddInputMapper(element => GraphMLIOHandler.MatchesName(element, DateTimeMapperKey), dateMapper,
          (_, xmlNode, _) => {
            // The actual value is a text node that can be retrieved from the event
            try {
              return JsonSerializer.Deserialize<DateTime>(((XText) xmlNode).Value);
            } catch (Exception exception) {
              Console.WriteLine(exception);
            }
            return DateTime.Now;
          });
      return graphMLIOHandler;
    }

    /// <summary>
    ///   Setup tooltips that return the value that is stored in the mapper.
    ///   Dynamic tooltips are implemented by adding a tooltip provider as an event handler for the
    ///   <see cref="GraphEditorInputMode.QueryItemToolTip"/> event of the <see cref="GraphEditorInputMode"/>.
    ///   The <see cref="QueryItemToolTipEventArgs{TItem}"/> parameter provides the item, handled, and toolTip properties.
    /// </summary>
    private void SetupTooltips() {
      var graphEditorInputMode = (GraphEditorInputMode) graphControl.InputMode!;
      graphEditorInputMode.ToolTipItems = GraphItemTypes.Node;
      graphEditorInputMode.QueryItemToolTip += (_, args) => {
        if (args.Handled) {
          // Tooltip content has already been assigned -> nothing to do.
          return;
        }
        if (args.Item is INode item) {
          var date = dateMapper[item];
          if (date != default) {
            // Found a suitable mapper. Set the tooltip content.
            args.ToolTip = date.ToLocalTime().ToString("G", CultureInfo.CurrentCulture);
            // Indicate that the tooltip content has been set.
            args.Handled = true;
          }
        }
      };

      // Add a little offset to the tooltip such that it is not obscured by the mouse pointer.
      graphEditorInputMode.ToolTipInputMode.ToolTipLocationOffset = new PointD(20, 20);
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this tutorial.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeTutorialDefaults(IGraph graph) {
      // set styles that are the same for all tutorials
      DemoStyles.InitDemoStyles(graph);

      // set the style, label and label parameter for group nodes
      graph.GroupNodeDefaults.Style = new GroupNodeStyle {
          TabBrush = new SolidColorBrush(Color.FromRgb(0x04, 0x2d, 0x37)),
          TabBackgroundBrush = new SolidColorBrush(Color.FromRgb(0x9d, 0xc6, 0xd0)),
          TabPosition = GroupNodeStyleTabPosition.TopTrailing,
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x9d, 0xc6, 0xd0)), 2),
          CornerRadius = 10
      };
      graph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          TextAlignment = TextAlignment.Left,
          TextBrush = new SolidColorBrush(Color.FromRgb(0x04, 0x2d, 0x37))
      };
      graph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabBackgroundParameter();

      // set sizes and locations specific for this tutorial
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(sideOfEdge: EdgeSides.BelowEdge);
    }

    [STAThread]
    public static void Main() {
      Run<CustomGraphMLDemo>();
    }
  }
}
