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
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.I18N;
using yWorks.Controls;
using yWorks.Layout.Labeling;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.Layout;

[assembly :
  XmlnsDefinition("http://www.yworks.com/yfilesWPF/2.2/demos/NodeLabelingWindow", "Demo.yFiles.Layout.NodeLabeling")]
[assembly : XmlnsPrefix("http://www.yworks.com/yfilesWPF/2.2/demos/NodeLabelingWindow", "demo")]

namespace Demo.yFiles.Layout.NodeLabeling
{
  /// <summary>
  /// Shows how to use a node labeling algorithm with different label models.
  /// </summary>
  public partial class NodeLabelingWindow
  {
    #region private fields

    // Optionhandler for labeling options
    private OptionHandler handler;

    #endregion

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the window.
    /// </summary>
    public NodeLabelingWindow() {
      InitializeComponent();
    }

    static NodeLabelingWindow() {
      LabelModels.Add(LABEL_MODEL_INTERIOR, new InteriorLabelModel());
      LabelModels.Add(LABEL_MODEL_EXTERIOR, new ExteriorLabelModel() {Insets = new InsetsD(10)});
      LabelModels.Add(LABEL_MODEL_FREENODE, new FreeNodeLabelModel());
      LabelModels.Add(LABEL_MODEL_SANDWICH, new SandwichLabelModel() {YOffset = 10});

      IList<ILabelModelParameter> list = new List<ILabelModelParameter>();
      var model = new ExteriorLabelModel() {Insets = new InsetsD(5)};
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.North));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.NorthEast));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.NorthWest));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.South));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.SouthEast));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.SouthWest));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.East));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.West));
      model = new ExteriorLabelModel() {Insets = new InsetsD(10)};
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.North));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.NorthEast));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.NorthWest));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.South));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.SouthEast));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.SouthWest));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.East));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.West));
      model = new ExteriorLabelModel() {Insets = new InsetsD(15)};
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.North));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.NorthEast));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.NorthWest));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.South));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.SouthEast));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.SouthWest));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.East));
      list.Add(model.CreateParameter(ExteriorLabelModel.Position.West));


      var genericLabelModel = new GenericLabelModel(list[0]);
      foreach (var labelModelParameter in list) {
        // set different profits for various insets
        var insets = ((ExteriorLabelModel) labelModelParameter.Model).Insets.Top;
        double profit = insets < 10 ? 1.0 : insets < 15 ? 0.9 : 0.8;
        genericLabelModel.AddParameter(labelModelParameter, new LabelCandidateDescriptor {Profit = profit});
      }
      LabelModels.Add(LABEL_MODEL_THREE_DISTANCE, genericLabelModel);
    }

    #region Properties

    private OptionHandler Handler {
      get { return handler; }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeGraph"/>
    protected virtual void OnLoad(object src, EventArgs e) {
      // add background
      object userObject = new BackgroundVisualCreator();
      graphControl.BackgroundGroup.AddChild(userObject);
      // initialize the graph
      InitializeGraph();
      InitializeStyles();

      // initialize the input mode
      InitializeInputModes();

      SetupOptions();

      // do initial label placement
      DoLabelPlacement();
      graphControl.FitGraphBounds();
    }

    /// <summary>
    /// Creates a new <see cref="GraphEditorInputMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      var graphEditorInputMode = new GraphEditorInputMode {ShowHandleItems = GraphItemTypes.None};
      //Automatically add a label when a node is created interactively
      graphEditorInputMode.NodeCreated += (o, args) => graphControl.Graph.AddLabel(args.Item, "City");
      graphControl.InputMode = graphEditorInputMode;
    }

    private void InitializeStyles() {
      graphControl.Graph.NodeDefaults.Style = new ShinyPlateNodeStyle {Brush = Brushes.Orange, DrawShadow = false, Radius = 1};
      graphControl.Graph.NodeDefaults.Size = new SizeD(10, 10);

      var innerLabelStyle = new DefaultLabelStyle { TextSize = 8 };
      var labelStyle = new CityLabelStyle(innerLabelStyle) {InnerLabelStyle = innerLabelStyle};

      graphControl.Graph.NodeDefaults.Labels.Style = labelStyle;
      graphControl.Graph.NodeDefaults.Labels.LayoutParameter = ExteriorLabelModel.North;
    }

    /// <summary>
    /// Create the initial sample graph
    /// </summary>
    private void InitializeGraph() {
      graphControl.ImportFromGraphML("Resources/uscities.graphml");
    }

    #endregion

    /// <summary>
    /// Does the label placement using the generic labeling algorithm. Before this, the model and size of the labels is
    /// set according to the option handlers settings.
    /// </summary>
    private async void DoLabelPlacement() {
      if (inLayout) {
        return;
      }
      inLayout = true;

      toolBar.IsEnabled = false;
      editorControl.IsEnabled = false;


      //desired label model
      ILabelModel labelModel = LabelModels[(string) handler[LABEL_MODEL].Value];
      int size = (int) handler[LABEL_SIZE].Value;

      foreach (var label in graphControl.Graph.Labels) {
        if (label.Owner is INode) {
          // only update the label model parameter if the label model changed
          if (labelModel != graphControl.Graph.NodeDefaults.Labels.LayoutParameter.Model) {
            graphControl.Graph.SetLabelLayoutParameter(label, labelModel.CreateDefaultParameter());
          }
          var cityLabelStyle = label.Style as CityLabelStyle;
          if (cityLabelStyle != null && cityLabelStyle.InnerLabelStyle is DefaultLabelStyle) {
            ((DefaultLabelStyle) cityLabelStyle.InnerLabelStyle).TextSize = size;
          }
          graphControl.Graph.AdjustLabelPreferredSize(label);
        }
      }
      {
        // set as default label model parameter
        graphControl.Graph.NodeDefaults.Labels.LayoutParameter = labelModel.CreateDefaultParameter();
        var cityLabelStyle = graphControl.Graph.NodeDefaults.Labels.Style as CityLabelStyle;
        if (cityLabelStyle != null && cityLabelStyle.InnerLabelStyle is DefaultLabelStyle) {
          ((DefaultLabelStyle) cityLabelStyle.InnerLabelStyle).TextSize = size;
        }
      }
      graphControl.Invalidate();

      // configure and run the layout algorithm
      var labelingAlgorithm = new GenericLabeling
                                          {
                                            MaximumDuration = 0,
                                            OptimizationStrategy = OptimizationStrategy.Balanced,
                                            PlaceEdgeLabels = false,
                                            PlaceNodeLabels = true,
                                            ReduceLabelOverlaps = true,
                                            ProfitModel = new ExtendedLabelCandidateProfitModel(),
                                          };

      var layoutExecutor = new LayoutExecutor(graphControl, graphControl.Graph, labelingAlgorithm)
      {
        Duration = TimeSpan.FromMilliseconds(500),
        EasedAnimation = true,
        AnimateViewport = false,
        UpdateContentRect = true
      };

      await layoutExecutor.Start();

      toolBar.IsEnabled = true;
      editorControl.IsEnabled = true;
      inLayout = false;
    }

    #region option handler

    private void SetupOptions() {
      SetupHandler();
      // create the options
      // populate the control to visualize them
      editorControl.OptionHandler = Handler;
      editorControl.IsAutoAdopt = true;
      editorControl.IsAutoCommit = true;
    }


    /// <summary>
    /// Initializes the option handler for the export
    /// </summary>
    private void SetupHandler() {
      handler = new OptionHandler(NODE_LABELING);
      OptionGroup currentGroup = handler;

      //Trigger an automatic relayout whenever an option changes
      currentGroup.AddList(LABEL_MODEL, LabelModels.Keys, "LABEL_MODEL_EXTERIOR").PropertyChanged +=
        HandlerPropertyChanged;
      currentGroup.AddInt(LABEL_SIZE, 8).PropertyChanged += HandlerPropertyChanged;

      // localization
      var rm =
        new ResourceManager("Demo.yFiles.Layout.NodeLabeling.NodeLabeling",
                            Assembly.GetExecutingAssembly());
      var rmf = new ResourceManagerI18NFactory();
      rmf.AddResourceManager(Handler.Name, rm);
      Handler.I18nFactory = rmf;
    }

    private void HandlerPropertyChanged(object sender, PropertyChangedEventArgs e) {
      DoLabelPlacement();
    }

    #endregion

    #region static members

    private const string NODE_LABELING = "NODE_LABELING";
    private const string LABEL_MODEL = "LABEL_MODEL";
    private const string LABEL_SIZE = "LABEL_SIZE";

    private const string LABEL_MODEL_INTERIOR = "LABEL_MODEL_INTERIOR";
    private const string LABEL_MODEL_EXTERIOR = "LABEL_MODEL_EXTERIOR";
    private const string LABEL_MODEL_FREENODE = "LABEL_MODEL_FREENODE";
    private const string LABEL_MODEL_SANDWICH = "LABEL_MODEL_SANDWICH";
    private const string LABEL_MODEL_THREE_DISTANCE = "LABEL_MODEL_THREE_DISTANCE";

    private static readonly Dictionary<string, ILabelModel> LabelModels =
      new Dictionary<string, ILabelModel>();

    private bool inLayout = false;

    #endregion

    #region Standard Actions

    /// <summary>
    /// Callback action that is triggered when the user exits the application.
    /// </summary>
    protected virtual void ExitAction(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    #endregion

    private void PlaceLabelsButton_OnClick(object sender, RoutedEventArgs e) {
      //Remove focus from the editor control
      //This will force an update of all values (and also trigger a layout IF something really has changed)
      var scope = FocusManager.GetFocusScope(editorControl); // elem is the UIElement to unfocus
      FocusManager.SetFocusedElement(scope, null); // remove logical focus
      DoLabelPlacement();
    }
  }
}