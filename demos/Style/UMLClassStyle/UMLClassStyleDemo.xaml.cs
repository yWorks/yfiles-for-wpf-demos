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
using System.Windows;
using System.Windows.Media;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace Demo.yFiles.Graph.UMLClassStyle
{
  /// <summary>
  ///   A demo that shows how to create a simple class diagram editor similar to the one
  ///   bundled with Visual Studio. The demo does not allow editing the UML features displayed
  ///   in a node, but you will be able to toggle visibility of feature sections by clicking on
  ///   the open/close icons displayed inside a node.
  /// </summary>
  /// <remarks>
  ///   This demo addresses various customization aspects:
  ///   <list type="bullet">
  ///     <item>how to create a simple UML model for classes</item>
  ///     <item>how to create a UML class node representation, i.e. your own <see cref="INodeStyle"/></item>
  ///     <item>how to customize resize behavior and resize handle placement of nodes by decorating the lookup of the nodes</item>
  ///     <item>
  ///       how to use and customize the <see cref="NodeControl.AutoUpdateNodeSize">auto resize feature</see> of
  ///       <see cref="NodeControlNodeStyle"/>
  ///     </item>
  ///   </list>
  /// </remarks>
  public partial class UMLClassStyleDemo
  {
    public UMLClassStyleDemo() {
      InitializeComponent();
    }

    protected virtual void OnLoaded(object src, EventArgs args) {
      InitializeInputModes();

      // Decorate the lookup of the nodes to change the default behavior
      // for moving, selection paint, resizing, etc.
      var graph = graphControl.Graph;
      graph.GetDecorator().Nodes.Add(new UMLNodeLookupChainLink());

      // register a node grid with the canvas' input mode context lookup
      var nodeGrid = new GridConstraintProvider(20);
      graphControl.GetInputModeContextDecoratorFor<INodeGridConstraintProvider>().AddConstant(nodeGrid);

      // remove the highlight indicator manager from the input context - disables highlight hints
      graphControl.GetInputModeContextDecoratorFor<HighlightIndicatorManager<IModelItem>>().Hide();

      // Create a style
      var umlStyle = new NodeControlNodeStyle("ClassInfoNodeStyle");
      graph.NodeDefaults.Style = umlStyle;

      // Add a sample node
      var node = CreateNode(null, graphControl.Graph, new PointD(100, 100), null);

      node.Tag = CreateDefaultClassInfo();

      // enable orthogonal edge editing
      graph.EdgeDefaults.Style =
          new PolylineEdgeStyle { TargetArrow = new Arrow(ArrowType.Stealth), OrthogonalEditing = true };

      // Enable clipboard
      graphControl.Clipboard = new GraphClipboard {
          ToClipboardCopier = { CloneTypes = CloneTypes.Tags, ReferentialIdentityTypes = CloneTypes.All },
          FromClipboardCopier = { CloneTypes = CloneTypes.Tags, ReferentialIdentityTypes = CloneTypes.All },
          DuplicateCopier = { CloneTypes = CloneTypes.Tags, ReferentialIdentityTypes = CloneTypes.All }
      };
    }

    public void InitializeInputModes() {
      // Create a default editor input mode
      var editMode = new UmlGraphEditorInputMode();

      // then customize it to suit our needs

      // snapping
      editMode.SnapContext = new GraphSnapContext {
          SnappableItems = SnappableItems.Node | SnappableItems.Label,
          CollectEdgeSnapReferences = false,
          CollectNodePairSegmentSnapLines = false,
          CollectNodePairSnapLines = false,
          CollectPortSnapReferences = false,
          GridSnapType = GridSnapTypes.All,
          NodeGridConstraintProvider = new GridConstraintProvider(20),
          GridSnapDistance = double.MaxValue,
          VisualizeSnapResults = false
      };

      // tweak the CreateEdgeInputMode
      editMode.CreateEdgeInputMode.ShowPortCandidates = ShowPortCandidates.None;
      editMode.CreateEdgeInputMode.SnapToEndCandidate = false;

      // Enable label editing only for edges and edge labels
      editMode.LabelEditableItems = GraphItemTypes.Edge | GraphItemTypes.EdgeLabel;

      // customize the node creation
      editMode.NodeCreator = CreateNode;

      // and finally register our input mode with the control.
      graphControl.InputMode = editMode;
    }

    // Creates a node and sets its correct size
    private INode CreateNode(IInputModeContext context, IGraph graph, PointD location, INode parent) {
      var node = graph.CreateNode(location);
      node.Tag = CreateDefaultClassInfo();
      var style = node.Style as NodeControlNodeStyle;
      if (style != null) {
        var size = style.GetPreferredSize(graphControl.CreateRenderContext(), node);
        graph.SetNodeLayout(node, RectD.FromCenter(location, new SizeD(120, size.Height)));
      }
      return node;
    }

    /// <summary>
    ///   Creates a default <see cref="ClassInfo"/> instance.
    /// </summary>
    private static ClassInfo CreateDefaultClassInfo() {
      var info = new ClassInfo("Class", "MyNodeStyle");
      info.Fields.Add(new FeatureInfo(FeatureModifier.Private, "fixAspect"));
      info.Fields.Add(new FeatureInfo(FeatureModifier.Private, "pointCount"));
      info.Fields.Add(new FeatureInfo(FeatureModifier.Private, "ratio"));
      info.Fields.Add(new FeatureInfo(FeatureModifier.Private, "renderer"));
      info.Properties.Add(new FeatureInfo(FeatureModifier.Public, "FixAspect"));
      info.Properties.Add(new FeatureInfo(FeatureModifier.Public, "PointCount"));
      info.Properties.Add(new FeatureInfo(FeatureModifier.Public, "Ratio"));
      info.Properties.Add(new FeatureInfo(FeatureModifier.Public, "Renderer"));
      info.Methods.Add(new FeatureInfo(FeatureModifier.Public, "Clone"));
      info.Methods.Add(new FeatureInfo(FeatureModifier.Public, "Install"));
      return info;
    }

    #region Custom node resize behavior

    private class UmlGraphEditorInputMode : GraphEditorInputMode
    {
      protected override void UpdateNodeControlNodeSize(
          [NotNull] NodeControl nodeControl,
          [CanBeNull] object? parameter) {
        // parameter is desired size
        var desiredSize = (Size?) parameter;
        if (desiredSize.HasValue) {
          var node = nodeControl.Item;
          var layout = node.Layout;
          // adjust only height
          SetNodeLayout(node, new RectD(layout.X, layout.Y, layout.Width, desiredSize.Value.Height));
        }
      }
    }

    #endregion

    #region Custom lookup chain link

    /// <summary>
    ///   A <see cref="IContextLookupChainLink"/> for UMLNodeStyles
    /// </summary>
    private class UMLNodeLookupChainLink : IContextLookupChainLink
    {
      private IContextLookup next;

      public void SetNext(IContextLookup next) {
        this.next = next;
      }

      public object Lookup(object item, Type type) {
        var node = (INode) item;
        // see if the node is styled using UMLClassStyle
        if (node.Style is NodeControlNodeStyle) {
          // then customize the behavior

          // for dragging the item using a shadow and grid
          if (type == typeof(IPositionHandler)) {
            var positionHandler = (IPositionHandler) LookupNext(item, type);
            if (positionHandler != null) {
              return new UMLPositionHandler(positionHandler, node);
            }
            return null;
          }
          if (type == typeof(IReshapeHandler)) {
            return new UMLReshapeHandler((IReshapeHandler) LookupNext(item, type));
          }
          // for resizing the item using a shadow and grid
          if (type == typeof(IReshapeHandleProvider)) {
            var result = LookupNext(item, type);
            if (result is ReshapeHandleProviderBase) {
              ((ReshapeHandleProviderBase) result).HandlePositions = HandlePositions.Right | HandlePositions.Left;
            }
            return result;
          }

          if (type == typeof(INodeSnapResultProvider)) {
            return new MyNodeSnapResultProvider();
          }
          // for constraining the size
          if (type == typeof(INodeSizeConstraintProvider)) {
            var height = double.MaxValue;
            return new NodeSizeConstraintProvider(new SizeD(100, 0), new SizeD(800, height));
          }

          // for providing the ClassInfo
          // Allows direct access without having to use ITagOwner.Tag
          if (type == typeof(ClassInfo)) {
            return node.Tag as ClassInfo;
          }
        }
        return LookupNext(item, type);
      }

      private object LookupNext(object item, Type type) {
        return next != null ? next.Lookup(item, type) : null;
      }

      private class MyNodeSnapResultProvider : NodeSnapResultProvider
      {
        protected override void CollectGridSnapResults(
            GraphSnapContext context,
            CollectSnapResultsEventArgs args,
            SnapGrid snapGrid,
            RectD suggestedLayout,
            INode node) {
          CollectGridSnapResult(context, args, snapGrid, suggestedLayout.GetTopLeft(), node);
        }
      }
    }

    #endregion

    #region Customized node reshape/movement handling

    public class UMLReshapeHandler : IReshapeHandler, IRectangle
    {
      private readonly IReshapeHandler originalHandler;
      private MutableRectangle simulationRectangle;
      private IRenderTreeElement shadowElement;
      private RectD originalBounds;

      public UMLReshapeHandler(IReshapeHandler originalHandler) {
        this.originalHandler = originalHandler;
      }

      public IRectangle Bounds => this;

      public void InitializeReshape(IInputModeContext context) {
        simulationRectangle = new MutableRectangle(originalHandler.Bounds);
        var node = new SimpleNode {
            Layout = simulationRectangle,
            Style = new ShapeNodeStyle {
                Shape = ShapeNodeShape.RoundRectangle, Brush = Brushes.Transparent, Pen = new Pen(Brushes.Gray, 2)
            }
        };

        var renderTree = context.CanvasControl.RenderTree;
        shadowElement = renderTree.CreateElement(renderTree.RootGroup, node, GraphModelManager.DefaultNodeRenderer)
            .ToFront();

        originalHandler.InitializeReshape(context);
        originalBounds = originalHandler.Bounds.ToRectD();
      }

      public void HandleReshape(IInputModeContext context, RectD originalBounds, RectD newBounds) {
        simulationRectangle.SetShape(newBounds);
      }

      public void CancelReshape(IInputModeContext context, RectD originalBounds) {
        context.CanvasControl.RenderTree.Remove(shadowElement);
        simulationRectangle = null;
        originalHandler.CancelReshape(context, this.originalBounds);
      }

      public void ReshapeFinished(IInputModeContext context, RectD originalBounds, RectD newBounds) {
        context.CanvasControl.RenderTree.Remove(shadowElement);
        simulationRectangle = null;
        originalHandler.HandleReshape(context, this.originalBounds, newBounds);
        originalHandler.ReshapeFinished(context, this.originalBounds, newBounds);
      }

      double ISize.Width => (simulationRectangle ?? originalHandler.Bounds).Width;

      double ISize.Height => (simulationRectangle ?? originalHandler.Bounds).Height;

      double IRectangle.X => (simulationRectangle ?? originalHandler.Bounds).X;

      double IRectangle.Y => (simulationRectangle ?? originalHandler.Bounds).Y;
    }

    /// <summary>
    ///   A specialized position handler used for dragging ghosts of nodes on a grid.
    /// </summary>
    public class UMLPositionHandler : IPositionHandler
    {
      private readonly IPositionHandler wrappedHandler;
      private readonly INode node;
      private IRenderTreeElement shadowElement;
      private MutablePoint shadowLocation;
      private PointD emulatedOffset;

      public UMLPositionHandler([NotNull] IPositionHandler wrappedHandler, [NotNull] INode node) {
        this.wrappedHandler = wrappedHandler;
        this.node = node;
      }

      public IPoint Location => wrappedHandler.Location.ToPointD() + emulatedOffset;

      public void InitializeDrag(IInputModeContext context) {
        wrappedHandler.InitializeDrag(context);
        shadowLocation = node.Layout.GetTopLeft();
        emulatedOffset = PointD.Origin;
        var dummyNode = new SimpleNode {
            Layout = new DynamicRectangle(shadowLocation, node.Layout),
            Style = new ShapeNodeStyle {
                Shape = ShapeNodeShape.RoundRectangle, Brush = Brushes.Transparent, Pen = new Pen(Brushes.Gray, 2)
            }
        };

        var renderTree = context.CanvasControl.RenderTree;
        shadowElement = renderTree.CreateElement(renderTree.RootGroup, dummyNode, GraphModelManager.DefaultNodeRenderer)
            .ToFront();
      }

      public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
        emulatedOffset = newLocation - originalLocation;
        shadowLocation.SetLocation(node.Layout.GetTopLeft() + emulatedOffset);
      }

      public void CancelDrag(IInputModeContext context, PointD originalLocation) {
        wrappedHandler.CancelDrag(context, originalLocation);
        context.CanvasControl.RenderTree.Remove(shadowElement);
        shadowElement = null;
        shadowLocation = null;
        emulatedOffset = PointD.Origin;
      }

      public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
        wrappedHandler.HandleMove(context, originalLocation, newLocation);
        wrappedHandler.DragFinished(context, originalLocation, newLocation);
        context.CanvasControl.RenderTree.Remove(shadowElement);
        shadowElement = null;
        shadowLocation = null;
        emulatedOffset = PointD.Origin;
      }

      /// <summary>
      ///   A simple rectangle that delegates its properties to a point and size.
      /// </summary>
      private sealed class DynamicRectangle : IRectangle
      {
        private readonly IPoint location;
        private readonly ISize size;

        public DynamicRectangle(IPoint location, ISize size) {
          this.location = location;
          this.size = size;
        }

        public double X => location.X;

        public double Y => location.Y;

        public double Width => size.Width;

        public double Height => size.Height;
      }
    }

    #endregion
  }
}
