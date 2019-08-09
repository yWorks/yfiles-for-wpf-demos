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
using System.ComponentModel;
using System.Globalization;
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

[assembly: XmlnsDefinition("http://www.yworks.com/yfilesWPF/2.2/demos/SequenceConstraintsWindow", "Demo.yFiles.Layout.SequenceConstraints")]
[assembly: XmlnsPrefix("http://www.yworks.com/yfilesWPF/2.2/demos/SequenceConstraintsWindow", "demo")]

namespace Demo.yFiles.Layout.SequenceConstraints
{
  /// <summary>
  /// This demo shows how to use sequence constraints with the <see cref="HierarchicLayout"/> to
  /// restrict the node sequencing.
  /// </summary>
  public partial class SequenceConstraintsWindow
  {

    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    public SequenceConstraintsWindow() {
      InitializeComponent();
    }

    #region Initialization

    /// <summary>
    /// The default style
    /// </summary>
    private readonly NodeControlNodeStyle defaultStyle = new NodeControlNodeStyle("SequenceConstraintControlStyle") { StyleTag = Colors.Blue };

    /// <summary>
    /// Used to create new <see cref="SequenceConstraintsInfo"/> objects with random weight that are randomly enabled/disabled.
    /// </summary>
    private readonly Random rand = new Random();

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected void OnLoaded(object source, RoutedEventArgs e) {
      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      IGraph graph = graphControl.Graph;
      // set the style as the default for all new nodes
      graph.NodeDefaults.Style = defaultStyle;
      // let the node decide how much space it needs and make sure it doesn't get any smaller.
      graph.NodeDefaults.Size = defaultStyle.GetPreferredSize(graphControl.CreateRenderContext(), new SimpleNode{Tag = new SequenceConstraintsInfo()});
      defaultStyle.MinimumSize = graph.NodeDefaults.Size;

      graph.EdgeDefaults.Labels.LayoutParameter = new EdgeSegmentLabelModel().CreateDefaultParameter();

      // create a simple label style
      DefaultLabelStyle labelStyle = new DefaultLabelStyle
                                      {
                                        Typeface = new Typeface("Arial"),
                                        BackgroundBrush = Brushes.LightBlue
                                      };

      // set the style as the default for all new node labels
      graph.NodeDefaults.Labels.Style = labelStyle;

      // create the graph and perform a layout operation
      CreateNewGraph();
      DoLayout();
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
      GraphEditorInputMode mode = new GraphEditorInputMode {NodeCreator = CreateNodeCallback};
      return mode;
    }

    /// <summary>
    /// Callback that actually creates the node and its business object
    /// </summary>
    private INode CreateNodeCallback(IInputModeContext context, IGraph graph, PointD location, INode parent) {
      RectD newBounds = RectD.FromCenter(location, graph.NodeDefaults.Size);
      // create a new node
      var node = graph.CreateNode(newBounds);
      // set the node tag to a new random sequence constraints
      node.Tag = new SequenceConstraintsInfo {
        Value = rand.Next(0, 7),
        Constraints = rand.NextDouble() < 0.9
      };
      return node;
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
    private void OnLayoutClick(object sender, EventArgs e) {
      DoLayout();
    }

    /// <summary>
    /// Creates a new graph and formats it.
    /// </summary>
    private void OnNewGraphClick(object sender, RoutedEventArgs e) {
      CreateNewGraph();
      DoLayout();
    }

    #endregion


    #region Graph creation and layout

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

    /// <summary>
    /// Performs the layout operation after applying all required constraints
    /// </summary>
    private async void DoLayout() {
      // layout starting, disable button
      layoutButton.IsEnabled = false;

      // we want to use the hl
      var hl = new HierarchicLayout() { OrthogonalRouting = true };

      // we only configure sequence constraints, so we can just use a new SequenceConstraintData
      // if HierarchicLayout should be configured further, we could also use HierarchicLayoutData.SequenceConstraintData
      var scData = new SequenceConstraintData {
        // we provide the item Values directly as IComparable instead of using the Add* methods on SequenceConstraintData
        ItemComparables = {
          Delegate = item => {
            // get the constraints info for the item
            // Note that 'item' can be an INode or IEdge but we only use SequenceConstraintsInfo for nodes
            var data = (item).Tag as SequenceConstraintsInfo;
            if (data != null && data.Constraints) {
              // the item shall be constrained so we use its Value as comparable
              return data.Value;
            }
            // otherwise we don't add constraints for the item
            return null;
          }
        }
      };

      // additionally enforce all nodes with a SequenceConstraintInfo.Value of 0 or 7 to be placed at head/tail
      foreach (var node in graphControl.Graph.Nodes) {
        var data = node.Tag as SequenceConstraintsInfo;
        if (data != null && data.Constraints) {
          if (data.Value == 0) {
            // add constraint to put this node at the head 
            scData.PlaceAtHead(node);
          } else if (data.Value == 7) {
            // add constraint to put this node at the tail
            scData.PlaceAtTail(node);
          }
        }
      }

      // do the layout
      await graphControl.MorphLayout(hl, TimeSpan.FromSeconds(1), scData);

      // enable button again
      layoutButton.IsEnabled = true;
    }

    #endregion
  }

  #region Business logic
  /// <summary>
  /// A business object that represents the weight (through property "Value") of the node and whether or not
  /// its weight should be taken into account as a sequence constraint.
  /// </summary>
  public class SequenceConstraintsInfo : INotifyPropertyChanged
  {
    /// <summary>
    /// Value field for property "Value"
    /// </summary>
    private int value;

    // property changed support
    private static readonly PropertyChangedEventArgs ValueChangedArgs = new PropertyChangedEventArgs("Value");
    private static readonly PropertyChangedEventArgs ConstraintsChangedEventArgs = new PropertyChangedEventArgs("Constraints");

    /// <summary>
    /// The weight of the object. And object with a lower number will be displayed to the left.
    /// </summary>
    /// <remarks>
    /// The number 0 means the node should be the first, 7 means it should be the last.
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
          if(PropertyChanged != null) {
            PropertyChanged(this, ConstraintsChangedEventArgs);
          } 
        }
      }
    }

    /// <summary>
    /// Register to be notified of a property change
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
  }
  #endregion

  // Converters are used as part of the business logic to convert from the integers stored in the
  // SequenceConstraintsInfo objects to represent their weight to more usable data structures
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
  [ValueConversion(typeof(int), typeof(string))]
  public class ConstraintsConverter : IValueConverter
  {
    #region Implementation of IValueConverter
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      switch ((int)value) {
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
  [ValueConversion(typeof(int), typeof(Brush))]
  public class BackgroundColorConverter : IValueConverter
  {
    #region Implementation of IValueConverter
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      switch ((int)value) {
        case 0:
          return Brushes.Black;
        case 7:
          return Brushes.White;
        default:
          int v = (int) value;
          return new SolidColorBrush(Color.FromRgb((byte)((v * 255) / 7), (byte)((v * 255) / 7), 255));
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
  [ValueConversion(typeof(int), typeof(Brush))]
  public class ForegroundColorConverter : IValueConverter
  {
    #region Implementation of IValueConverter
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return (int)value < 4 ? Brushes.White : Brushes.Black;
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
  [ValueConversion(typeof(bool), typeof(Visibility))]
  public class BoolToVisibilityConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return (bool)value ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }
  #endregion
}
