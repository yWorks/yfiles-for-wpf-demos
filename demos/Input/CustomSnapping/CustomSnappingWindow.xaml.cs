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
using System.IO;
using Demo.yFiles.Graph.Input.CustomSnapping.UI;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace Demo.yFiles.Graph.Input.CustomSnapping
{
  /// <summary>
  ///   Demo code that shows how to customize the snapping features of the graph control.
  /// </summary>
  public partial class CustomSnappingWindow
  {
    public CustomSnappingWindow() {
      InitializeComponent();
    }

    private void OnLoaded(object source, EventArgs args) {
      DecorateModelItemLookupForCustomSnappingBehaviour();

      var graphSnapContext = CreateGraphSnapContext();

      InitializeGrid(graphSnapContext);

      // Initialize two free snap lines that are also visualized in the GraphCanvasComponent
      AdditionalSnapLineVisualCreators = new List<AdditionalSnapLineVisualCreator>();
      AddAdditionalSnapLineVisualCreator(new PointD(0, -70), new PointD(500, -70));
      AddAdditionalSnapLineVisualCreator(new PointD(-230, -50), new PointD(-230, 400));

      // Initialize the input mode for this demo
      var graphEditorInputMode = new GraphEditorInputMode { SnapContext = graphSnapContext };

      // add an input mode that allows to move the custom AdditionalSnapLines
      graphEditorInputMode.Add(new AdditionalSnapLineMoveInputMode(this) { Priority = -50 });
      GraphControl.InputMode = graphEditorInputMode;

      InitializeGraphDefaults();

      // Initialize the graph by reading it from a file.
      ReadGraph("Resources\\CustomSnapping.graphml");
    }

    /// <summary>
    ///   Creates a pre-configured <see cref="GraphSnapContext"/> for this demo.
    /// </summary>
    protected virtual GraphSnapContext CreateGraphSnapContext() {
      var context = new GraphSnapContext {
          ConsiderInitialLabelLocationSnapping = false, GridSnapType = GridSnapTypes.All, SnapDistance = 10
      };

      // use the free additional snap lines
      context.CollectSnapReferences += CollectAdditionalSnapReferences;

      return context;
    }

    protected virtual void DecorateModelItemLookupForCustomSnappingBehaviour() {
      // add additional snap lines for orthogonal labels of nodes
      GraphControl.Graph.GetDecorator().Nodes.SnapReferenceProvider.AddWrapperFactory((node, wrappedProvider) =>
          new OrthogonalLabelSnapReferenceProviderWrapper(node, wrappedProvider));

      // add additional snap lines for orthogonal labels of edges
      GraphControl.Graph.GetDecorator().Edges.SnapReferenceProvider.AddWrapperFactory((edge, wrappedProvider) =>
          new OrthogonalLabelSnapReferenceProviderWrapper(edge, wrappedProvider));

      // for nodes using ShapeNodeStyle use a customized grid snapping behaviour based on their shape
      GraphControl.Graph.GetDecorator().Nodes.SnapResultProvider.AddConstant(node => node.Style is ShapeNodeStyle,
          new ShapeBasedGridNodeSnapResultProvider());
    }

    // Adds grid to the GraphControl and grid constraint provider to the snap context
    protected virtual void InitializeGrid(GraphSnapContext context) {
      var gridInfo = new GridInfo { HorizontalSpacing = 200, VerticalSpacing = 200 };
      var renderTree = GraphControl.RenderTree;
      renderTree.CreateElement(renderTree.BackgroundGroup, gridInfo, new GridRenderer());

      GraphControl.Invalidate();
      GraphControl.ZoomChanged += delegate { GraphControl.Invalidate(); };
      GraphControl.ViewportChanged += delegate { GraphControl.Invalidate(); };

      context.NodeGridConstraintProvider = new GridConstraintProvider(gridInfo);
      context.BendGridConstraintProvider = new GridConstraintProvider(gridInfo);
    }

    protected virtual void InitializeGraphDefaults() {
      var graph = GraphControl.Graph;
      DemoStyles.InitDemoStyles(graph);
      graph.NodeDefaults.Size = new SizeD(50, 50);

      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(new PointD(0.5, 0.0),
          new PointD(0, -10), new PointD(0.5, 1.0), PointD.Origin);

      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromCenter();
    }

    #region free snap lines

    /// <summary>
    ///   Adds a new <see cref="AdditionalSnapLineVisualCreator"/> to the <see cref="GraphControl"/> that spans between
    ///   <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The start location of the snap line.</param>
    /// <param name="to">The end location of the snap line.</param>
    public void AddAdditionalSnapLineVisualCreator(PointD from, PointD to) {
      var lineVisualCreator = new AdditionalSnapLineVisualCreator(from, to);
      AdditionalSnapLineVisualCreators.Add(lineVisualCreator);
      var renderTree = GraphControl.RenderTree;
      renderTree.CreateElement(renderTree.BackgroundGroup, lineVisualCreator);
    }

    /// <summary>
    ///   Returns a list of the free <see cref="AdditionalSnapLineVisualCreator"/>s used in this demo.
    /// </summary>
    /// <remarks>
    ///   This property is used by the <see cref="AdditionalSnapLineMoveInputMode"/> to access the
    ///   <see cref="AdditionalSnapLineVisualCreator"/>s used in this demo.
    /// </remarks>
    public List<AdditionalSnapLineVisualCreator> AdditionalSnapLineVisualCreators { get; private set; }

    /// <summary>
    ///   Creates and adds <see cref="SnapLine"/>s for the free <see cref="AdditionalSnapLineVisualCreator"/>
    ///   to a <see cref="GraphSnapContext"/>.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="AdditionalSnapLineVisualCreator"/>s are used to visualize and represent free snap lines,
    ///   according <see cref="OrthogonalSnapLine"/>s have to be added to the snapping mechanism to describe their snapping
    ///   behavior.
    /// </remarks>
    /// <param name="sender">The snap context sending this event.</param>
    /// <param name="e">The event arguments to add the snap lines to.</param>
    protected virtual void CollectAdditionalSnapReferences(object sender, CollectSnapReferencesEventArgs e) {
      foreach (var creator in AdditionalSnapLineVisualCreators) {
        foreach (var snapLine in creator.CreateSnapLines()) {
          e.AddSnapReference(snapLine);
        }
      }
    }

    #endregion

    private GraphControl GraphControl => graphControl;

    /// <summary>
    ///   Reads the graph from a file with the given file name.
    /// </summary>
    protected virtual void ReadGraph(string filename) {
      var ioh = new GraphMLIOHandler();
      ioh.Read(GraphControl.Graph, new StreamReader(filename));

      GraphControl.FitGraphBounds();
    }
  }

  #region Class OrthogonalLabelSnapReferenceProviderWrapper

  /// <summary>
  ///   Wraps a given <see cref="ISnapReferenceProvider"/> and adds additional <see cref="OrthogonalSnapLine"/>s for
  ///   orthogonal labels of an <see cref="IModelItem"/>.
  /// </summary>
  /// <remarks>
  ///   For each orthogonal label there are <see cref="OrthogonalSnapLine"/>s added for it's top, bottom, left and right
  ///   side.
  /// </remarks>
  internal class OrthogonalLabelSnapReferenceProviderWrapper : ISnapReferenceProvider
  {
    private readonly ILabelOwner owner;
    private readonly ISnapReferenceProvider wrapped;

    /// <summary>
    ///   Creates a new instance that wraps the given <paramref name="wrapped">snap line provider</paramref>.
    /// </summary>
    /// <param name="owner">The label owner to add orthogonal snap lines for.</param>
    /// <param name="wrapped">The snap line provider that shall be wrapped.</param>
    public OrthogonalLabelSnapReferenceProviderWrapper(ILabelOwner owner, ISnapReferenceProvider wrapped) {
      this.owner = owner;
      this.wrapped = wrapped;
    }

    /// <summary>
    ///   Calls <see cref="ISnapReferenceProvider.AddSnapReferences"/> of the wrapped provider and adds custom
    ///   <see cref="OrthogonalSnapLine"/>s
    ///   for the label owner.
    /// </summary>
    /// <param name="context">The context which holds the settings for the snap lines. </param>
    /// <param name="args">The argument to use for adding snap lines.</param>
    public void AddSnapReferences(GraphSnapContext context, CollectSnapReferencesEventArgs args) {
      wrapped.AddSnapReferences(context, args);

      // add snaplines for orthogonal labels
      foreach (var label in owner.Labels) {
        var layout = label.GetLayout();
        var upX = Math.Round(layout.UpX, 6);     // round UpX to it's first 6 digits
        if (upX == 0 || upX == 1 || upX == -1) { // check if it's orthogonal
          // label is orthogonal
          var bounds = layout.GetBounds();

          // add snaplines to the top, bottom, left and right border of the label
          var topCenter = bounds.GetTopLeft() + new PointD(layout.Width / 2, 0);
          var snapLine = new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Bottom,
              SnapReferenceVisualizationType.FixedLine, topCenter, bounds.X - 10, bounds.GetMaxX() + 10, false, 100,
              GraphItemTypes.Node | GraphItemTypes.Label, label);
          args.AddSnapReference(snapLine);

          var bottomCenter = bounds.GetBottomLeft() + new PointD(layout.Width / 2, 0);
          snapLine = new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Top,
              SnapReferenceVisualizationType.FixedLine, bottomCenter, bounds.X - 10, bounds.GetMaxX() + 10, false, 100,
              GraphItemTypes.Node | GraphItemTypes.Label, label);
          args.AddSnapReference(snapLine);

          var leftCenter = bounds.GetTopLeft() + new PointD(0, layout.Height / 2);
          snapLine = new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Right,
              SnapReferenceVisualizationType.FixedLine, leftCenter, bounds.Y - 10, bounds.GetMaxY() + 10, false, 100,
              GraphItemTypes.Node | GraphItemTypes.Label, label);
          args.AddSnapReference(snapLine);

          var rightCenter = bounds.GetTopRight() + new PointD(0, layout.Height / 2);
          snapLine = new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Left,
              SnapReferenceVisualizationType.FixedLine, rightCenter, bounds.Y - 10, bounds.GetMaxY() + 10, false, 100,
              GraphItemTypes.Node | GraphItemTypes.Label, label);
          args.AddSnapReference(snapLine);
        }
      }
    }
  }

  #endregion

  #region Class ShapeBasedGridNodeSnapResultProvider

  /// <summary>
  ///   Customizes the grid snapping behavior of NodeSnapResultProvider by providing SnapResults for each point of the
  ///   node's shape path instead of the node's center.
  /// </summary>
  internal class ShapeBasedGridNodeSnapResultProvider : NodeSnapResultProvider
  {
    protected override void CollectGridSnapResults(
        GraphSnapContext context,
        CollectSnapResultsEventArgs args,
        SnapGrid snapGrid,
        RectD suggestedLayout,
        INode node) {
      // node.Layout isn't updated, yet, so we have to calculate the delta between the new suggested layout and the current node.Layout
      var delta = suggestedLayout.GetTopLeft() - node.Layout.GetTopLeft();

      // get outline of the shape and iterate over it's path point
      var geometry = node.Style.Renderer.GetShapeGeometry(node, node.Style);
      var outline = geometry.GetOutline();
      if (outline != null) {
        var cursor = outline.CreateCursor();
        while (cursor.MoveNext()) {
          // ignore PathType.Close as we had the path point as first point
          // and cursor.CurrentEndPoint is always (0, 0) for PathType.Close
          if (cursor.PathType != PathType.Close) {
            // adjust path point by the delta calculated above and add an according SnapResult
            var endPoint = cursor.CurrentEndPoint + delta;
            snapGrid.CollectPointSnapResult(args, node, endPoint, context.GridSnapType);
          }
        }
      }
    }
  }

  #endregion
}
