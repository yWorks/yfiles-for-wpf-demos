/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using Size = System.Windows.Size;

namespace Demo.yFiles.Graph.UMLClassStyle
{
  /// <summary>
  /// A demo that shows how to create a simple class diagram editor similar to the one 
  /// bundled with Visual Studio. The demo does not allow to edit the UML features displayed 
  /// in a node, but you will be able to toggle visibility of feature sections by clicking on
  /// the open/close icons displayed inside a node.
  /// </summary>
  /// <remarks>
  /// This demo addresses various customization aspects:
  /// <list type="bullet">
  /// <item>how to create a simple UML model for classes</item>
  /// <item>how to create a UML class node representation, i.e. your own <see cref="INodeStyle"/></item>
  /// <item>how to customize resize behavior and resize handle placement of nodes by decorating the lookup of the nodes</item>
  /// <item>how to use and customize the <see cref="NodeControl.AutoUpdateNodeSize">auto resize feature</see> of <see cref="NodeControlNodeStyle"/></item>
  /// </list>
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
      IGraph graph = graphControl.Graph;

      ILookupDecorator decorator = graph.Lookup<ILookupDecorator>();
      if (decorator != null && decorator.CanDecorate(typeof(INode))) {
        decorator.AddLookup(typeof (INode), new UMLNodeLookupChainLink());
      }

      // register a node grid with the canvas' input mode context lookup
      GridConstraintProvider<INode> nodeGrid = new GridConstraintProvider<INode>(20);
      IContextLookupChainLink gridLink =
        Lookups.AddingLookupChainLink(typeof (IGridConstraintProvider<INode>), nodeGrid);
      graphControl.InputModeContextLookupChain.Add(gridLink);

      // remove the highlight indicator manager from the input context - disables highlight hints 
      IContextLookupChainLink hidingLink = Lookups.HidingLookupChainLink(typeof (HighlightIndicatorManager<IModelItem>));
      graphControl.InputModeContextLookupChain.Add(hidingLink);

      // Create a style
      var umlStyle = new NodeControlNodeStyle("ClassInfoNodeStyle");
      graph.NodeDefaults.Style = umlStyle;
      
      // Add a sample node
      INode node = CreateNode(null, graphControl.Graph, new PointD(100, 100), null);

      node.Tag = CreateDefaultClassInfo();

      // Enable clipboard
      graphControl.Clipboard = new GraphClipboard
                                 {
                                   ToClipboardCopier = {
                                         Clone = GraphCopier.CloneTypes.Tags,
                                         ReferentialIdentityTypes = GraphCopier.CloneTypes.All
                                       },
                                   FromClipboardCopier = {
                                         Clone = GraphCopier.CloneTypes.Tags,
                                         ReferentialIdentityTypes = GraphCopier.CloneTypes.All
                                       },
                                   DuplicateCopier = {
                                       Clone = GraphCopier.CloneTypes.Tags, 
                                       ReferentialIdentityTypes = GraphCopier.CloneTypes.All
                                   }
                                 };

    }

    public void InitializeInputModes() {
      // Create a default editor input mode
      GraphEditorInputMode editMode = new GraphEditorInputMode();

      // then customize it to suit our needs

      // orthogonal edges
      editMode.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext();

      // snapping
      editMode.SnapContext = new GraphSnapContext()
                               {
                                 CollectEdgeSnapLines = false,
                                 CollectNodePairSegmentSnapLines = false,
                                 CollectNodePairSnapLines = false,
                                 CollectPortSnapLines = false,
                                 SnapBendAdjacentSegments = false,
                                 SnapPortAdjacentSegments = false,
                                 SnapBendsToSnapLines = false,
                                 SnapSegmentsToSnapLines = false,
                                 GridSnapType = GridSnapTypes.All,
                                 NodeGridConstraintProvider = new GridConstraintProvider<INode>(20),
                                 GridSnapDistance = double.MaxValue,
                                 VisualizeSnapResults = false,
                               };

      
      // tweak the CreateEdgeInputMode
      editMode.CreateEdgeInputMode.ShowPortCandidates = ShowPortCandidates.None;
      editMode.CreateEdgeInputMode.SnapToTargetCandidate = false;
      
      //Enable label editing only for edges and edge labels
      editMode.LabelEditableItems = GraphItemTypes.Edge | GraphItemTypes.EdgeLabel;

      // customize the node creation
      editMode.NodeCreator = CreateNode;

      // disable default behavior for auto resize
      editMode.AvailableCommands.Remove(NodeControl.UpdateNodeSizeCommand);

      // and finally register our input mode with the control.
      graphControl.InputMode = editMode;
    }

    // Creates a node and sets it's correct size
    private INode CreateNode(IInputModeContext context, IGraph graph, PointD location, INode parent) {
      INode node = graph.CreateNode(location);
      node.Tag = CreateDefaultClassInfo();
      var style = node.Style as NodeControlNodeStyle;
      if (style != null) {
        SizeD size = style.GetPreferredSize(graphControl.CreateRenderContext(), node);
        graph.SetNodeLayout(node, RectD.FromCenter(location, new SizeD(120, size.Height)));
      }
      return node;
    }

    /// <summary>
    /// Creates a default <see cref="ClassInfo"/> instance.
    /// </summary>
    private static ClassInfo CreateDefaultClassInfo() {
      ClassInfo info = new ClassInfo("Class", "MyNodeStyle");
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

    private void ResizeNodeCanExecute(object sender, CanExecuteRoutedEventArgs e) {
      NodeControl nodeControl = e.OriginalSource as NodeControl;
      e.CanExecute = nodeControl != null;
      e.Handled = true;
    }

    private void ResizeNodeExecuted(object sender, ExecutedRoutedEventArgs e) {
      NodeControl nodeControl = e.OriginalSource as NodeControl;
      if (nodeControl != null) {
        // parameter is desired size
        var desiredSize = (Size?) e.Parameter;
        if (desiredSize.HasValue) {
          INode node = nodeControl.Item;
          GraphEditorInputMode mode = (GraphEditorInputMode) graphControl.InputMode;
          IRectangle layout = node.Layout;
          // adjust only height
          mode.SetNodeLayout(node, new RectD(layout.X, layout.Y, layout.Width, desiredSize.Value.Height));
          e.Handled = true;
        }
      }
    }

    #endregion

    #region Custom lookup chain link

    /// <summary>
    /// A <see cref="IContextLookupChainLink"/> for UMLNodeStyles
    /// </summary>
    private class UMLNodeLookupChainLink : IContextLookupChainLink
    {
      private IContextLookup next;

      public void SetNext(IContextLookup next) {
        this.next = next;
      }

      public object Lookup(object item, Type type) {
        INode node = (INode) item;
        // see if the node is styled using UMLClassStyle
        if (node.Style is NodeControlNodeStyle) {
          // then customize the behavior

          // for dragging the item using a shadow and grid
          if (type == typeof (IPositionHandler)) {
            IPositionHandler positionHandler = (IPositionHandler) LookupNext(item, type);
            if (positionHandler != null) {
              return new UMLPositionHandler(positionHandler, node);
            } else {
              return null;
            }
          }
          if (type == typeof (IReshapeHandler)) {
            return new UMLReshapeHandler((IReshapeHandler)LookupNext(item, type));
          }
          // for resizing the item using a shadow and grid
          if (type == typeof (IReshapeHandleProvider)) {
            var result = LookupNext(item, type);
            if (result is ReshapeHandleProviderBase) {
              ((ReshapeHandleProviderBase) result).HandlePositions = HandlePositions.East | HandlePositions.West;
            }
            return result;
          }

          if (type == typeof (INodeSnapResultProvider)) {
            return new MyNodeSnapResultProvider();
          }
          // for constraining the size
          if (type == typeof (INodeSizeConstraintProvider)) {
            double height = double.MaxValue;
            return new NodeSizeConstraintProvider(new SizeD(100, 0), new SizeD(800, height));
          }

          // for providing the ClassInfo
          // Allows direct access without having to use ITagOwner.Tag
          if (type == typeof (ClassInfo)) {
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
        protected override void CollectGridSnapResults(GraphSnapContext context, CollectSnapResultsEventArgs args, RectD suggestedLayout, INode node) {
          AddGridSnapResult(context, args, suggestedLayout.GetTopLeft(), node);
        }
      }
    }

    #endregion

    #region Customized node reshape/movement handling

    public class UMLReshapeHandler : IReshapeHandler, IRectangle
    {
      private readonly IReshapeHandler originalHandler;
      private MutableRectangle simulationRectangle;
      private ICanvasObject shadowObject;
      private RectD originalBounds;

      public UMLReshapeHandler(IReshapeHandler originalHandler) {
        this.originalHandler = originalHandler;
      }

      public IRectangle Bounds {
        get { return this; }
      }

      public void InitializeReshape(IInputModeContext context) {
        simulationRectangle = new MutableRectangle(originalHandler.Bounds);
        var node = new SimpleNode {
          Layout = simulationRectangle,
          Style =
            new ShapeNodeStyle {
              Shape = ShapeNodeShape.RoundRectangle,
              Brush = Brushes.Transparent,
              Pen = new Pen(Brushes.Gray, 2)
            }
        };

        shadowObject = context.CanvasControl.RootGroup.AddChild(node, GraphModelManager.DefaultNodeDescriptor).ToFront();

        originalHandler.InitializeReshape(context);
        originalBounds = originalHandler.Bounds.ToRectD();
      }

      public void HandleReshape(IInputModeContext context, RectD originalBounds, RectD newBounds) {
        simulationRectangle.Reshape(newBounds);
      }

      public void CancelReshape(IInputModeContext context, RectD originalBounds) {
        shadowObject.Remove();
        simulationRectangle = null;
        originalHandler.CancelReshape(context, this.originalBounds);
      }

      public void ReshapeFinished(IInputModeContext context, RectD originalBounds, RectD newBounds) {
        shadowObject.Remove();
        simulationRectangle = null;
        originalHandler.HandleReshape(context, this.originalBounds, newBounds);
        originalHandler.ReshapeFinished(context, this.originalBounds, newBounds);
      }

      double ISize.Width {
        get { return (simulationRectangle ?? originalHandler.Bounds).Width; }
      }

      double ISize.Height {
        get { return (simulationRectangle ?? originalHandler.Bounds).Height; }
      }

      double IPoint.X {
        get { return (simulationRectangle ?? originalHandler.Bounds).X; }
      }

      double IPoint.Y {
        get { return (simulationRectangle ?? originalHandler.Bounds).Y; }
      }
    }

    /// <summary>
    /// A specialized position handler used for dragging ghosts of nodes on a grid.
    /// </summary>
    public class UMLPositionHandler : IPositionHandler
    {
      private readonly IPositionHandler wrappedHandler;
      private readonly INode node;
      private ICanvasObject shadowObject;
      private MutablePoint shadowLocation;
      private PointD emulatedOffset;

      public UMLPositionHandler([NotNull] IPositionHandler wrappedHandler, [NotNull] INode node) {
        this.wrappedHandler = wrappedHandler;
        this.node = node;
      }

      public IPoint Location {
        get { return wrappedHandler.Location.ToPointD() + emulatedOffset; }
      }

      public void InitializeDrag(IInputModeContext context) {
        wrappedHandler.InitializeDrag(context);
        this.shadowLocation = node.Layout.GetTopLeft();
        this.emulatedOffset = PointD.Origin;
        var dummyNode = new SimpleNode {
          Layout = new DynamicRectangle(shadowLocation, node.Layout),
          Style =
            new ShapeNodeStyle {
              Shape = ShapeNodeShape.RoundRectangle,
              Brush = Brushes.Transparent,
              Pen = new Pen(Brushes.Gray, 2)
            }
        };

        shadowObject = context.CanvasControl.RootGroup.AddChild(dummyNode, GraphModelManager.DefaultNodeDescriptor).ToFront();
      }

      public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
        emulatedOffset = newLocation - originalLocation;
        shadowLocation.Relocate(node.Layout.GetTopLeft() + emulatedOffset);
      }

      public void CancelDrag(IInputModeContext context, PointD originalLocation) {
        wrappedHandler.CancelDrag(context, originalLocation);
        shadowObject.Remove();
        shadowObject = null;
        shadowLocation = null;
        emulatedOffset = PointD.Origin;
      }

      public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
        wrappedHandler.HandleMove(context, originalLocation, newLocation);
        wrappedHandler.DragFinished(context, originalLocation, newLocation);
        shadowObject.Remove();
        shadowObject = null;
        shadowLocation = null;
        emulatedOffset = PointD.Origin;
      }

      /// <summary>
      /// A simple rectangle that delegates its properties to a point and size.
      /// </summary>
      private sealed class DynamicRectangle : IRectangle
      {
        private readonly IPoint location;
        private readonly ISize size;

        public DynamicRectangle(IPoint location, ISize size) {
          this.location = location;
          this.size = size;
        }

        public double X {
          get { return location.X; }
        }

        public double Y {
          get { return location.Y; }
        }

        public double Width {
          get { return size.Width; }
        }

        public double Height {
          get { return size.Height; }
        }
      }
    }
    #endregion
  }
}
