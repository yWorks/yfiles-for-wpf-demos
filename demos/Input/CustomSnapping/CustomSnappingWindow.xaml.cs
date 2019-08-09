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
using System.IO;
using Demo.yFiles.Graph.Input.CustomSnapping.UI;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;

namespace Demo.yFiles.Graph.Input.CustomSnapping
{
  /// <summary>
  /// Demo code that shows how to customize the snapping features of the graph control.
  /// </summary>
  public partial class CustomSnappingWindow
  {

    private GridVisualCreator grid;

    public CustomSnappingWindow() {
      InitializeComponent();
    }

    private void OnLoaded(object source, EventArgs args){

      DecorateModelItemLookupForCustomSnappingBehaviour();

      var graphSnapContext = CreateGraphSnapContext();
      var labelSnapContext = CreateLabelSnapContext();
      
      InitializeGrid(graphSnapContext);

      // Initialize two free snap lines that are also visualized in the GraphCanvasComponent
      AdditionalSnapLineVisualCreators = new List<AdditionalSnapLineVisualCreator>();
      AddAdditionalSnapLineVisualCreator(new PointD(0, -70), new PointD(500, -70));
      AddAdditionalSnapLineVisualCreator(new PointD(-230, -50), new PointD(-230, 400));

      // Initialize the input mode for this demo
      var graphEditorInputMode = new GraphEditorInputMode
      {
        SnapContext = graphSnapContext,
        LabelSnapContext = labelSnapContext
      };

      // add an input mode that allows to move the custom AdditionalSnapLines
      graphEditorInputMode.Add(new AdditionalSnapLineMoveInputMode(this){Priority = -50});
      GraphControl.InputMode = graphEditorInputMode;

      InitializeGraphDefaults();

      // Initialize the graph by reading it from a file.
      ReadGraph("Resources\\CustomSnapping.graphml");
    }

    /// <summary>
    /// Creates a pre-configured <see cref="GraphSnapContext"/> for this demo.
    /// </summary>
    protected virtual GraphSnapContext CreateGraphSnapContext() {
      var context = new GraphSnapContext
      {
        SnapOrthogonalMovement = false,
        SnapBendAdjacentSegments = true,
        GridSnapType = GridSnapTypes.All,
        SnapDistance = 10
      };

      // use the free additional snap lines
      context.CollectSnapLines += CollectAdditionalGraphSnapLines;

      return context;
    }

    /// <summary>
    /// Creates a pre-configured <see cref="LabelSnapContext"/> for this demo.
    /// </summary>
    protected virtual LabelSnapContext CreateLabelSnapContext() {
      var snapContext = new LabelSnapContext
      {
        SnapDistance = 10,
        CollectInitialLocationSnapLines = false
      };
      snapContext.CollectSnapLines += CollectAdditionalLabelSnapLines;

      return snapContext;
    }

    protected virtual void DecorateModelItemLookupForCustomSnappingBehaviour() {
      // add additional snap lines for orthogonal labels of nodes
      GraphControl.Graph.GetDecorator().NodeDecorator.SnapLineProviderDecorator.SetImplementationWrapper(
        (node, wrappedProvider) => new OrthogonalLabelSnapLineProviderWrapper(wrappedProvider));

      // add additional snap lines for orthogonal labels of edges
      GraphControl.Graph.GetDecorator().EdgeDecorator.SnapLineProviderDecorator.SetImplementationWrapper(
        (edge, wrappedProvider) => new OrthogonalLabelSnapLineProviderWrapper(wrappedProvider));

      // for nodes using ShapeNodeStyle use a customized grid snapping behaviour based on their shape
      GraphControl.Graph.GetDecorator().NodeDecorator.NodeSnapResultProviderDecorator.SetImplementation(
        node => node.Style is ShapeNodeStyle, new ShapeBasedGridNodeSnapResultProvider());
    }

    // Adds grid to the GraphControl and grid constraint provider to the snap context
    protected virtual void InitializeGrid(GraphSnapContext context) {
      GridInfo gridInfo = new GridInfo { HorizontalSpacing = 200, VerticalSpacing = 200 };
      grid = new GridVisualCreator(gridInfo);
      GraphControl.BackgroundGroup.AddChild(grid);

      GraphControl.Invalidate();
      GraphControl.ZoomChanged += delegate { GraphControl.Invalidate(); };
      GraphControl.ViewportChanged += delegate { GraphControl.Invalidate(); };

      context.NodeGridConstraintProvider = new GridConstraintProvider<INode>(gridInfo);
      context.BendGridConstraintProvider = new GridConstraintProvider<IBend>(gridInfo);
    }

    protected virtual void InitializeGraphDefaults() {
      var graph = GraphControl.Graph;
      graph.NodeDefaults.Style = new BevelNodeStyle();
      graph.NodeDefaults.Size = new SizeD(50, 50);

      var labelStyle = new DefaultLabelStyle { BackgroundPen = Pens.Black };

      graph.NodeDefaults.Labels.Style = labelStyle;
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(
          new PointD(0.5, 0.0), new PointD(0, -10), new PointD(0.5, 1.0), PointD.Origin, 0.0);

      graph.EdgeDefaults.Labels.Style = labelStyle;
      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0, 0, 0.5);
    }

    #region free snap lines

    /// <summary>
    /// Adds a new <see cref="AdditionalSnapLineVisualCreator"/> to the <see cref="GraphControl"/> that spans between 
    /// <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The start location of the snap line.</param>
    /// <param name="to">The end location of the snap line.</param>
    public void AddAdditionalSnapLineVisualCreator(PointD from, PointD to) {
      var lineVisualCreator = new AdditionalSnapLineVisualCreator(from, to);
      AdditionalSnapLineVisualCreators.Add(lineVisualCreator);
      // Specify the canvas object descriptor for this line. It is responsible for the rendering, amongst others.
      GraphControl.BackgroundGroup.AddChild(lineVisualCreator);
    }

    /// <summary>
    /// Returns a list of the free <see cref="AdditionalSnapLineVisualCreator"/>s used in this demo.
    /// </summary>
    /// <remarks>
    /// This property is used by the <see cref="AdditionalSnapLineMoveInputMode"/> to access the 
    /// <see cref="AdditionalSnapLineVisualCreator"/>s used in this demo.
    /// </remarks>
    public List<AdditionalSnapLineVisualCreator> AdditionalSnapLineVisualCreators { get; private set; }

    /// <summary>
    /// Creates and adds <see cref="SnapLine"/>s for the free <see cref="AdditionalSnapLineVisualCreator"/>
    /// to a <see cref="GraphSnapContext"/>. 
    /// </summary>
    /// <remarks>
    /// While the <see cref="AdditionalSnapLineVisualCreator"/>s are used to visualize and represent free snap lines,
    /// according <see cref="OrthogonalSnapLine"/>s have to be added to the snapping mechanism to describe their snapping behavior.
    /// </remarks>
    /// <param name="sender">The snap context sending this event.</param>
    /// <param name="e">The event arguments to add the snap lines to.</param>
    protected virtual void CollectAdditionalGraphSnapLines(object sender, CollectGraphSnapLinesEventArgs e) {
      foreach (var creator in AdditionalSnapLineVisualCreators) {
        foreach (var snapLine in creator.CreateSnapLines()) {
          e.AddAdditionalSnapLine(snapLine);
        }
      }
    }

    /// <summary>
    /// Creates and adds <see cref="SnapLine"/>s for the free <see cref="AdditionalSnapLineVisualCreator"/>
    /// to a <see cref="LabelSnapContext"/>. 
    /// </summary>
    /// <remarks>
    /// While the <see cref="AdditionalSnapLineVisualCreator"/>s are used to visualize and represent free snap lines,
    /// according <see cref="OrthogonalSnapLine"/>s have to be added to the snapping mechanism to describe their snapping behavior.
    /// </remarks>
    /// <param name="sender">The snap context sending this event.</param>
    /// <param name="e">The event arguments to add the snap lines to.</param>
    protected virtual void CollectAdditionalLabelSnapLines(object sender, CollectLabelSnapLineEventArgs e) {
      foreach (var creator in AdditionalSnapLineVisualCreators) {
        foreach (var snapLine in creator.CreateSnapLines()) {
          e.AddSnapLine(snapLine);
        }
      }
    }

    #endregion

    private GraphControl GraphControl { get { return graphControl; } }

    /// <summary>
    /// Reads the graph from a file with given file name.
    /// </summary>
    virtual protected void ReadGraph(string filename) {
      GraphMLIOHandler ioh = new GraphMLIOHandler();
      ioh.Read(GraphControl.Graph, new StreamReader(filename));

      GraphControl.UpdateContentRect();
      GraphControl.FitContent();
    }

  }

  #region Class OrthogonalLabelSnapLineProviderWrapper

  /// <summary>
  /// Wraps a given <see cref="ISnapLineProvider"/> and adds additional <see cref="OrthogonalSnapLine"/>s for
  /// orthogonal labels of an <see cref="IModelItem"/>.
  /// </summary>
  /// <remarks>
  /// For each orthogonal label there are <see cref="OrthogonalSnapLine"/>s added for it's top, bottom, left and right side.
  /// </remarks>
  class OrthogonalLabelSnapLineProviderWrapper : ISnapLineProvider
  {
    private readonly ISnapLineProvider wrapped;

    /// <summary>
    /// Creates a new instance that wraps the given <paramref name="wrapped">snap line provider</paramref>.
    /// </summary>
    /// <param name="wrapped">The snap line provider that shall be wrapped.</param>
    public OrthogonalLabelSnapLineProviderWrapper(ISnapLineProvider wrapped) {
      this.wrapped = wrapped;
    }

    /// <summary>
    /// Calls <see cref="ISnapLineProvider.AddSnapLines"/> of the wrapped provider and adds custom <see cref="OrthogonalSnapLine"/>s
    /// for the <paramref name="item"/>.
    /// </summary>
    /// <param name="context">The context which holds the settings for the snap lines. </param>
    /// <param name="args">The argument to use for adding snap lines.</param>
    /// <param name="item">The item to add snaplines for.</param>
    public void AddSnapLines(GraphSnapContext context, CollectGraphSnapLinesEventArgs args, IModelItem item) {
      wrapped.AddSnapLines(context, args, item);

      // add snaplines for orthogonal labels
      ILabelOwner labelOwner = item as ILabelOwner;
      if (labelOwner != null) {
        foreach (ILabel label in labelOwner.Labels) {
          var layout = label.GetLayout();
          double upX = Math.Round(layout.UpX, 6); // round UpX to it's first 6 digits 
          if (upX == 0 || upX == 1 || upX == -1) { // check if it's orthogonal
            // label is orthogonal
            RectD bounds = layout.GetBounds();

            // add snaplines to the top, bottom, left and right border of the label
            PointD topCenter = bounds.TopLeft + new PointD(layout.Width/2, 0);
            var snapLine = new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Bottom,
              SnapLine.SnapLineFixedLineKey, topCenter, bounds.MinX - 10, bounds.MaxX + 10, label, 100);
            args.AddAdditionalSnapLine(snapLine);

            PointD bottomCenter = bounds.BottomLeft + new PointD(layout.Width/2, 0);
            snapLine = new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Top,
              SnapLine.SnapLineFixedLineKey, bottomCenter, bounds.MinX - 10, bounds.MaxX + 10, label, 100);
            args.AddAdditionalSnapLine(snapLine);

            PointD leftCenter = bounds.TopLeft + new PointD(0, layout.Height/2);
            snapLine = new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Right,
              SnapLine.SnapLineFixedLineKey, leftCenter, bounds.MinY - 10, bounds.MaxY + 10, label, 100);
            args.AddAdditionalSnapLine(snapLine);

            PointD rightCenter = bounds.TopRight + new PointD(0, layout.Height/2);
            snapLine = new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Left,
              SnapLine.SnapLineFixedLineKey, rightCenter, bounds.MinY - 10, bounds.MaxY + 10, label, 100);
            args.AddAdditionalSnapLine(snapLine);
          }
        }
      }
    }
  }

  #endregion

  #region Class ShapeBasedGridNodeSnapResultProvider

  /// <summary>
  /// Customizes the grid snapping behavior of NodeSnapResultProvider by providing SnapResults for each point of the
  /// node's shape path instead of the node's center.
  /// </summary>
  class ShapeBasedGridNodeSnapResultProvider : NodeSnapResultProvider {

    override protected void CollectGridSnapResults(GraphSnapContext context, CollectSnapResultsEventArgs args, RectD suggestedLayout, INode node) {
      // node.Layout isn't updated, yet, so we have to calculate the delta between the the new suggested layout and the current node.Layout
      PointD delta = suggestedLayout.TopLeft - node.Layout.GetTopLeft();

      // get outline of the shape and iterate over it's path point
      IShapeGeometry geometry = node.Style.Renderer.GetShapeGeometry(node, node.Style);
      GeneralPath outline = geometry.GetOutline();
      if (outline != null) {
        GeneralPath.PathCursor cursor = outline.CreateCursor();
        while (cursor.MoveNext()) {
          // ignore PathType.Close as we had the path point as first point 
          // and cursor.CurrentEndPoint is always (0, 0) for PathType.Close
          if (cursor.PathType != PathType.Close) {
            // adjust path point by the delta calculated above and add an according SnapResult
            PointD endPoint = cursor.CurrentEndPoint + delta;
            AddGridSnapResultCore(context, args, endPoint, node, GridSnapTypes.GridPoints, SnapPolicy.ToNearest, SnapPolicy.ToNearest);
          }
        }
      }
    }
  }

  #endregion

}
