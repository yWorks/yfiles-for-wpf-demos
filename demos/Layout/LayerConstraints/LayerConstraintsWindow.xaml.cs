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
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchic;
using yWorks.Graph.LabelModels;

[assembly :
  XmlnsDefinition("http://www.yworks.com/yfilesWPF/2.2/demos/LayerConstraintsWindow",
    "Demo.yFiles.Layout.LayerConstraints")]
[assembly : XmlnsPrefix("http://www.yworks.com/yfilesWPF/2.2/demos/LayerConstraintsWindow", "demo")]

namespace Demo.yFiles.Layout.LayerConstraints
{
  /// <summary>
  /// This demo shows how to use layer constraints with the <see cref="HierarchicLayout"/> to
  /// restrict the node layering.
  /// </summary>
  public partial class LayerConstraintsWindow
  {
    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    public LayerConstraintsWindow() {
      InitializeComponent();
    }

    #region Initialization

    /// <summary>
    /// The default style
    /// </summary>
    private readonly NodeControlNodeStyle defaultStyle = new NodeControlNodeStyle("ConstraintNodeControlStyle") { StyleTag = Colors.Blue };

    /// <summary>
    /// Used to create new <see cref="LayerConstraintsInfo"/> objects with random weight that are randomly enabled/disabled.
    /// </summary>
    private readonly Random rand = new Random();

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected async void OnLoaded(object source, RoutedEventArgs e) {
      // initialize the graph
      await InitializeGraph();

      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual async Task InitializeGraph() {
      IGraph graph = graphControl.Graph;

      // set the style as the default for all new nodes
      graph.NodeDefaults.Style = defaultStyle;
      // let the node decide how much space it needs and make sure it doesn't get any smaller.
      graph.NodeDefaults.Size = defaultStyle.GetPreferredSize(graphControl.CreateRenderContext(), new SimpleNode {Tag = new LayerConstraintsInfo()});
      defaultStyle.MinimumSize = graph.NodeDefaults.Size;

      // create a simple label style
      DefaultLabelStyle labelStyle = new DefaultLabelStyle
                                      {
                                        Typeface = new Typeface("Arial"),
                                        BackgroundBrush = Brushes.White,
                                        AutoFlip = true
                                      };

      // set the style as the default for all new node labels
      graph.NodeDefaults.Labels.Style = labelStyle;
      graph.EdgeDefaults.Labels.Style = labelStyle;

      graph.EdgeDefaults.Labels.LayoutParameter = new EdgeSegmentLabelModel().CreateDefaultParameter();

      // create the graph and perform a layout operation
      CreateNewGraph();
      await DoLayout();
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();
    }

    /// <summary>
    /// Creates the default input mode for the <see cref="GraphControl"/>,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <remarks>
    /// The control uses a custom node creation callback that creates business objects for newly
    /// created nodes.
    /// </remarks>
    /// <returns>a new <see cref="GraphEditorInputMode"/> instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      var mode = new GraphEditorInputMode {NodeCreator = CreateNodeCallback};
      // only allow numeric values to be specified as label text
      mode.ValidateLabelText += delegate(object sender, LabelTextValidatingEventArgs labelTextValidatingEventArgs) {
                                  labelTextValidatingEventArgs.NewText = labelTextValidatingEventArgs.NewText.Trim();
                                  if (labelTextValidatingEventArgs.NewText.Length == 0) {
                                    return;
                                  }
                                  double result;
                                  if (
                                    !Double.TryParse(labelTextValidatingEventArgs.NewText, NumberStyles.Float,
                                                     Thread.CurrentThread.CurrentUICulture, out result)) {
                                    // only allow numbers between 0 and 100
                                    if (result <= 100 && result >= 0) {
                                      labelTextValidatingEventArgs.Cancel = true;
                                    }
                                  }
                                };
      return mode;
    }

    /// <summary>
    /// Callback that actually creates the node and its business object
    /// </summary>
    private INode CreateNodeCallback(IInputModeContext context, IGraph graph, PointD location, INode parent) {
      RectD newBounds = RectD.FromCenter(location, graph.NodeDefaults.Size);
      var node = graph.CreateNode(newBounds);
      node.Tag = new LayerConstraintsInfo
      {
        Value = rand.Next(0, 7),
        Constraints = rand.NextDouble() < 0.9
      };
      return node;
    }

    /// <summary>
    /// Calculates the weight of an edge by translating its (first) label into an int.
    /// It will return 0 if the label is not a correctly formatted double.
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    private int GetEdgeWeight(IEdge edge) {
      // if edge has at least one label...
      if (edge.Labels.Count > 0) {
        // return its value
        return (int) Convert.ToDouble(edge.Labels[0].Text, CultureInfo.CurrentUICulture);
      }
      return 1;
    }

    #endregion

    #region Event handler implementation

    /// <summary>
    /// Exits the demo.
    /// </summary>
    private void ExitMenuItemClick(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    /// <summary>
    /// Formats the current graph.
    /// </summary>
    private async void OnLayoutClick(object sender, EventArgs e) {
      await DoLayout();
    }

    /// <summary>
    /// Creates a new graph and formats it.
    /// </summary>
    private async void OnNewGraphClick(object sender, RoutedEventArgs e) {
      CreateNewGraph();
      await DoLayout();
    }

    /// <summary>
    /// Disables all constraints on the nodes.
    /// </summary>
    private void OnDisableConstraints(object sender, RoutedEventArgs e) {
      foreach (var node in graphControl.Graph.Nodes) {
        var data = node.Tag as LayerConstraintsInfo;
        if (data != null) {
          data.Constraints = false;
        }
      }
    }

    /// <summary>
    /// Enables all constraints on the nodes.
    /// </summary>
    private void OnEnableConstraints(object sender, RoutedEventArgs e) {
      foreach (var node in graphControl.Graph.Nodes) {
        var data = node.Tag as LayerConstraintsInfo;
        if (data != null) {
          data.Constraints = true;
        }
      }
    }

    #endregion

    #region Graph creation and layout

    #region Create a random graph

    /// <summary>
    /// Clears the existing graph and creates a new random graph
    /// </summary>
    private void CreateNewGraph() {
      // remove all nodes and edges from the graph
      graphControl.Graph.Clear();

      // create a new random graph
      new RandomGraphGenerator
        {
          AllowCycles = true,
          AllowMultipleEdges = false,
          AllowSelfLoops = false,
          EdgeCount = 25,
          NodeCount = 20,
          NodeCreator = graph => CreateNodeCallback(null, graph, PointD.Origin, null)
        }.Generate(graphControl.Graph);

      // center the graph to prevent the initial layout fading in from the top left corner
      graphControl.FitGraphBounds();
    }

    #endregion

    private async Task DoLayout() {
      // layout starting, disable button
      layoutButton.IsEnabled = false;

      // create a new layout algorithm
      var hl = new HierarchicLayout
                  {
                    OrthogonalRouting = true,
                    FromScratchLayeringStrategy = LayeringStrategy.HierarchicalTopmost,
                    IntegratedEdgeLabeling = true
                  };

      // and layout data for it
      var hlData = new HierarchicLayoutData {
        ConstraintIncrementalLayererAdditionalEdgeWeights = { Delegate = GetEdgeWeight }
      };

      // we provide the LayerConstraintData.Values directly as IComparable instead of using the Add* methods on LayerConstraintData
      var layerConstraintData = hlData.LayerConstraints;
      layerConstraintData.NodeComparables.Delegate = node => {  
        var data = node.Tag as LayerConstraintsInfo;
        if (data != null && data.Constraints) {
          // the node shall be constrained so we use its Value as comparable
          return data.Value;
        }
        // otherwise we don't add constraints for the node
        return null;
      };

      // additionally enforce all nodes with a LayerConstraintInfo.Value of 0 or 7 to be placed at top/bottom
      // and register the value in the NodeComparables Mapper for all other constrained nodes
      foreach (var node in graphControl.Graph.Nodes) {
        var data = node.Tag as LayerConstraintsInfo;
        if (data != null && data.Constraints) {
          if (data.Value == 0) {
            // add constraint to put this node at the top
            layerConstraintData.PlaceAtTop(node);
          } else if (data.Value == 7) {
            // add constraint to put this node at the bottom
            layerConstraintData.PlaceAtBottom(node);
          } else {
            // for every node in between we record it's value with the mapper, assuring that there
            // will be no layer with different values and monotonically growing values per layer
            layerConstraintData.NodeComparables.Mapper[node] = data.Value;
          }
        }
      }

      // perform the layout operation
      await graphControl.MorphLayout(hl, TimeSpan.FromSeconds(1), hlData);
      // code is executed once the layout operation is finished
      // enable button again
      layoutButton.IsEnabled = true;
    }

    #endregion
  }

  #region Business logic

  /// <summary>
  /// A business object that represents the weight (through property "Value") of the node and whether or not
  /// its weight should be taken into account as a layer constraint.
  /// </summary>
  public class LayerConstraintsInfo : INotifyPropertyChanged
  {
    /// <summary>
    /// The weight of the object. An object with a lower number will be layered in a higher layer.
    /// </summary>
    /// <remarks>
    /// The number 0 means the node should be the in the first, 7 means it should be the last layer.
    /// </remarks>
    private int value;

    // property changed support - needed for databinding to the Control Style
    private static readonly PropertyChangedEventArgs ValueChangedArgs = new PropertyChangedEventArgs("Value");

    private static readonly PropertyChangedEventArgs ConstraintsChangedEventArgs =
      new PropertyChangedEventArgs("Constraints");

    /// <summary>
    /// The weight of the object. An object with a lower number will be layered in a higher layer.
    /// </summary>
    /// <remarks>
    /// The number 0 means the node should be the in the first, 7 means it should be the last layer.
    /// </remarks>
    public int Value {
      get { return value; }
      set {
        int oldVal = this.value;
        this.value = value;
        if (oldVal != value && PropertyChanged != null) {
          PropertyChanged(this, ValueChangedArgs);
        }
      }
    }

    private bool constraints;

    /// <summary>
    /// Describes whether or not the constraint is active. If <see langword="true"/>, the constraint will be
    /// taken into account by the layout algorithm.
    /// </summary>
    public bool Constraints {
      get { return constraints; }
      set {
        if (constraints != value) {
          constraints = value;
          if (PropertyChanged != null) {
            PropertyChanged(this, ConstraintsChangedEventArgs);
          }
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }

  #endregion

  // Converters are used as part of the business logic to convert from the integers stored in the
  // LayerConstraintsData objects to represent their weight to more usable data structures
  // required by the view layer. i.e. background and foreground colors

  #region Converter implementations

  /// <summary>
  /// This converter translates an integer between 0 and 7 into a string that represents
  /// an appropriate description of the number.
  /// </summary>
  /// <remarks>
  /// The numbers 0 and 7 are represented by the strings "First" and "Last", every other number is
  /// represented by itself (converted to string).
  /// </remarks>
  [ValueConversion(typeof (int), typeof (string))]
  public class ConstraintsConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      switch ((int) value) {
        case 0:
          return "First";
        case 7:
          return "Last";
        default:
          return value.ToString();
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// This converter translates an integer between 0 and 7 into a Brush that can be used to paint
  /// a background. It works in combination with <see cref="ForegroundColorConverter"/>.
  /// </summary>
  [ValueConversion(typeof (int), typeof (Brush))]
  public class BackgroundColorConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      switch ((int) value) {
        case 0:
          return Brushes.Black;
        case 7:
          return Brushes.White;
        default:
          int v = (int) value;
          return new SolidColorBrush(Color.FromRgb((byte) ((v*255)/7), (byte) ((v*255)/7), 255));
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// This converter translates an integer between 0 and 7 into a Brush that can be used to paint
  /// a text. It works in combination with <see cref="BackgroundColorConverter"/>.
  /// </summary>
  [ValueConversion(typeof (int), typeof (Brush))]
  public class ForegroundColorConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return (int) value < 4 ? Brushes.White : Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// This converter maps a boolean value to a Visibility where <see langword="true"/> is converted to visible
  /// and <see langword="false"/> is converted to hidden (thus the object still reserves the space its needs).
  /// </summary>
  [ValueConversion(typeof (bool), typeof (Visibility))]
  public class BoolToVisibilityConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return (bool) value ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }

  #endregion
}
