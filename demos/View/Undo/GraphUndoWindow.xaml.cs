/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Utils;

[assembly: XmlnsDefinition("http://www.yworks.com/yfilesWPF/2.1/demos/GraphUndoWindow", "Demo.yFiles.Graph.Undo")]
[assembly: XmlnsPrefix("http://www.yworks.com/yfilesWPF/2.1/demos/GraphUndoWindow", "demo")]

namespace Demo.yFiles.Graph.Undo
{
  /// <summary>
  /// This demo presents Undo/Redo functionality.
  /// </summary>
  /// <remarks>
  /// This demo shows how to use the undo/redo framework in yFiles WPF both for predefined
  /// Undo/Redo operations (e.g. structural modifications) as well as for custom undo functionality.
  /// </remarks>
  public partial class GraphUndoWindow
  {

    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    public GraphUndoWindow() {
      InitializeComponent();
    }

    #region Initialization

    /// <summary>
    /// The default style
    /// </summary>
    private readonly NodeControlNodeStyle defaultStyle = new NodeControlNodeStyle("UndoNodeControlStyle") { StyleTag = Colors.Blue };

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected void OnLoaded(object source, RoutedEventArgs e) {

      //Keep shared instances of copied node styles
      graphControl.Clipboard.FromClipboardCopier.Clone = graphControl.Clipboard.FromClipboardCopier.Clone & ~GraphCopier.CloneTypes.NodeStyle;
      graphControl.Clipboard.ToClipboardCopier.Clone = graphControl.Clipboard.ToClipboardCopier.Clone & ~GraphCopier.CloneTypes.NodeStyle;

      // write symbolic style name in graphml to make the nodes use the default style when imported, not the specific style instance
      // otherwise, 'change color' wouldn't work on imported nodes
      graphControl.GraphMLIOHandler.QueryReferenceId += delegate(object sender, QueryReferenceIdEventArgs args) { if (args.Value == defaultStyle) {
        args.ReferenceId = "defaultStyle";
      } };

      // if node style is string "defaultStyle" in graphml, assign defaultStyle
      graphControl.GraphMLIOHandler.ResolveReference += delegate(object sender, ResolveReferenceEventArgs args) { if (args.ReferenceId == "defaultStyle") {
        args.Value = defaultStyle;
      } };

      graphControl.Graph.SetUndoEngineEnabled(true);
      UndoEngine engine = graphControl.Graph.GetUndoEngine();
      if (engine != null) {
        engine.AutoMergeTime = TimeSpan.FromMilliseconds(100);
        engine.PropertyChanged += delegate {
          UpdateUndoState();
        };
      }
      UpdateUndoState();

      // Add some more interesting port candidates.
      GraphDecorator decorator = graphControl.Graph.GetDecorator();
      decorator.NodeDecorator.PortCandidateProviderDecorator.SetFactory(
        node => PortCandidateProviders.FromShapeGeometry(node));

      graphControl.Graph.NodeCreated += OnNodeCreated;
      graphControl.Graph.NodeRemoved += OnNodeRemoved;

      // initialize the graph
      InitializeGraph();

      // reset the Undo queue so the initial graph creation cannot be undone
      graphControl.Graph.GetUndoEngine().Clear();

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
      graph.NodeDefaults.Size = new SizeD(100, 60);

      DefaultLabelStyle labelStyle = new DefaultLabelStyle();
      labelStyle.Typeface = new Typeface("Arial");
      labelStyle.BackgroundBrush = Brushes.LightBlue;

      // set the style as the default for all new node labels
      graph.NodeDefaults.Labels.Style = labelStyle;

      graph.CreateNode(RectD.FromCenter(new PointD(200, 100), graph.NodeDefaults.Size), graph.NodeDefaults.GetStyleInstance(), new MyBusinessObject(0.3));
    }

    /// <summary>
    /// When a node is created, we add a handler for the custom edit event
    /// </summary>
    private void OnNodeCreated(object source, ItemEventArgs<INode> evt) {
      MyBusinessObject tag = evt.Item.Tag as MyBusinessObject;
      if (tag != null) {
        tag.MyEditEvent += OnUndoEventHandler;
      }
    }

    /// <summary>
    /// When a node is removed, we detach the event handler.
    /// </summary>
    private void OnNodeRemoved(object source, ItemEventArgs<INode> evt) {
      MyBusinessObject tag = evt.Item.Tag as MyBusinessObject;
      if (tag != null) {
        tag.MyEditEvent -= OnUndoEventHandler;
      }
    }

    /// <summary>
    /// The edit event that is dispatched by the business object contains a 
    /// undo unit which is added to the undo engine here.
    /// </summary>
    private void OnUndoEventHandler(object o, MyEditEventHandlerArgs args) {
      UndoEngine engine = graphControl.Graph.GetUndoEngine();
      engine.AddUnit(args.UndoUnit);
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <remarks>
    /// The control uses a custom node creation callback that creates business objects for newly
    /// created nodes.
    /// </remarks>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      GraphEditorInputMode mode = new GraphEditorInputMode
      {
        NodeCreator = CreateNodeCallback,
        AllowGroupingOperations = true
      };
      return mode;
    }

    /// <summary>
    /// Callback that actually creates the node and its business object
    /// </summary>
    private INode CreateNodeCallback(IInputModeContext context, IGraph graph, PointD location, INode parent) {
      RectD newBounds = RectD.FromCenter(location, graph.NodeDefaults.Size);
      var node = graph.CreateNode(newBounds, graph.NodeDefaults.GetStyleInstance(), new MyBusinessObject(0.3));
      return node;
    }

    #endregion

    /// <summary>
    /// Callback method that updates the state of the Undo related UI elements
    /// </summary>
    private void UpdateUndoState() {
      UndoEngine engine = graphControl.Graph.GetUndoEngine();
      undoButton.ToolTip = engine != null ? engine.UndoName : string.Empty;
      redoButton.ToolTip = engine != null ? engine.RedoName : string.Empty;
    }

    /// <summary>
    /// Clears the undo queue.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClearUndoClicked (object sender, EventArgs e) {
      UndoEngine engine = graphControl.Graph.GetUndoEngine();
      if(engine != null) {
        engine.Clear();
      }
    }

    /// <summary>
    /// Exits the demo.
    /// </summary>
    private void ExitMenuItem_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    // Random Generator to create random color
    private readonly Random rnd = new Random();

    /// <summary>
    /// This changes all node's color to a random value.
    /// </summary>
    /// <remarks>The modification is remembered in the undo engine</remarks>
    private void ModifyColorButton_Click(object sender, EventArgs e) {
      var oldColor = (Color) defaultStyle.StyleTag;

      // Generate random color
      Color newColor = Color.FromArgb(255, (byte) rnd.Next(0, 255), (byte) rnd.Next(0, 255), (byte) rnd.Next(0, 255));
      // Set the style's color to new value
      defaultStyle.StyleTag = newColor;

      // Add new undo unit to remember old color in the undo engine
      // As only the tag value change has to be undone, the AddUndoUnit extension method on IGraph is used  
      graphControl.Graph.AddUndoUnit("Change Color", "Change Color",
          () => defaultStyle.StyleTag = oldColor,
          () => defaultStyle.StyleTag = newColor);
      
      // redraw nodes
      graphControl.Invalidate();
    }
  }

  /// <summary>
  /// A custom business object that dispatches an edit event when a property changes, 
  /// in order to enqueue the changes in the graph control's undo engine.
  /// </summary>
  public class MyBusinessObject : INotifyPropertyChanged
  {
    // some value to be displayed
    private double value;
    private static readonly PropertyChangedEventArgs valueChangedArgs = new PropertyChangedEventArgs("Value");

    public MyBusinessObject() {}

    public MyBusinessObject(double value) {
      this.Value = value;
    }

    public double Value {
      get { return value; }
      set {
        // constain value to 0..1
        if (value < 0) {
          value = 0;
        } else if (value > 1) {
          value = 1;
        }
        double oldVal = this.value;
        this.value = value;
        if (MyEditEvent != null) {
          // raise an event to add this change to the undo engine
          MyEditEvent(this, new MyEditEventHandlerArgs(new MyUndoUnit(this, oldVal)));
        }
        if (oldVal != value && PropertyChanged != null) {
          PropertyChanged(this, valueChangedArgs);
        }
      }
    }

    // Event to be raised when a property changes
    public event MyEditEventHandler MyEditEvent;

    /// <summary>
    /// A custom undo unit that allows for undo/redo of changes 
    /// to the business object's name property.
    /// </summary>
    class MyUndoUnit : UndoUnitBase
    {
      private readonly double oldValue;
      private readonly MyBusinessObject businessObject;
      private double newValue;

      public MyUndoUnit(MyBusinessObject businessObject, double oldValue)
        : base("Change Value") {
        this.businessObject = businessObject;
        this.oldValue = oldValue;
        this.newValue = businessObject.Value;
      }

      public override void Undo() {
        businessObject.Value = oldValue;
      }

      public override void Redo() {
        businessObject.Value = newValue;
      }

      /// <summary>
      /// Adds the last undo unit change to the current undo unit.
      /// </summary>
      /// <remarks>This is done to prevent every value change to get undone. 
      /// Only the last value change in a series is remembered.</remarks>
      /// <param name="unit"></param>
      /// <returns></returns>
      public override bool TryMergeUnit(IUndoUnit unit) {
        if (unit is MyUndoUnit) {
          newValue = ((MyUndoUnit) unit).newValue;
          return true;
        } else {
          return false;
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }

  public delegate void MyEditEventHandler(object sender, MyEditEventHandlerArgs args);

  /// <summary>
  /// The edit event contains a undo unit that can be added to the 
  /// graph control's undo engine.
  /// </summary>
  public class MyEditEventHandlerArgs
  {
    private readonly IUndoUnit undoUnit;
    public MyEditEventHandlerArgs(IUndoUnit undoUnit) {
      this.undoUnit = undoUnit;
    }

    public IUndoUnit UndoUnit {
      get { return undoUnit; }
    }
  }

  public class BackgroundColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is Color) {
        if (parameter is Color) {
          return Mix((Color) value, (Color) parameter, 0.5);
        }
        return value;
      }
      return null;
    }

    ///<summary>
    /// Mixes two colors using the provided ratio.
    ///</summary>
    private static Color Mix(Color color0, Color color1, double ratio) {
      double iratio = 1 - ratio;
      double a = color0.A*ratio + color1.A*iratio;
      double r = color0.R*ratio + color1.R*iratio;
      double g = color0.G*ratio + color1.G*iratio;
      double b = color0.B*ratio + color1.B*iratio;
      return Color.FromArgb(System.Convert.ToByte(Math.Round(a)), System.Convert.ToByte(Math.Round(r)), System.Convert.ToByte(Math.Round(g)), System.Convert.ToByte(Math.Round(b)));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
