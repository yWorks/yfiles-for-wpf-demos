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
using System.Linq;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.OverlapAvoidingEditor
{
  /// <summary>
  /// A demo that shows how to interactively edit graphs without creating overlaps.
  /// </summary>
  public partial class OverlapAvoidingEditorDemo
  {
    #region Initialization
    
    public OverlapAvoidingEditorDemo() {
      InitializeComponent();
    }

    private void OnLoad(object sender, EventArgs e) {
      ConfigureModelManager(GraphControl.GraphModelManager);
      InitializeInputModes();
      InitializeGraph();
    }

    /// <summary>
    /// Modifies the <see cref="GraphModelManager"/> so that certain edges are hidden.
    /// </summary>
    /// <param name="manager">The GraphModelManager to modify.</param>
    private void ConfigureModelManager(GraphModelManager manager) {
      manager.EdgeDescriptor = new HidingEdgeDescriptor(manager.EdgeDescriptor);
    }

    /// <summary>
    /// Registers the <see cref="GraphEditorInputMode"/> as the <see cref="CanvasControl.InputMode"/>
    /// and configures input gestures to avoid overlaps.
    /// </summary>
    private void InitializeInputModes() {
      var graph = GraphControl.Graph;
      
      // create a GraphEditorInputMode instance
      var editMode = new GraphEditorInputMode();
      GraphControl.InputMode = editMode;

      // enable interactive re-parenting
      editMode.AllowGroupingOperations = true;
      
      // enable single selection
      new SingleSelectionSupport(editMode).Enable();

      // enable orthogonal edge editing
      editMode.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext();

      // use a position handler that avoids overlapping,
      // but only apply it to the selected node and not to the children of groups
      graph.GetDecorator().NodeDecorator.PositionHandlerDecorator.SetFactory(
          node => node == GraphControl.Selection.SelectedNodes.FirstOrDefault(),
          node => {
            // Lookup the node position handler that only handles the location of the node itself
            var defaultPositionHandler = DefaultGraph.DefaultNodeLookup.Lookup<IPositionHandler>(node);
            // wrap it in a GroupingNodePositionHandler that moves all child nodes but doesn't update the 
            // parent group node bounds
            var handler = new GroupingNodePositionHandler(node, defaultPositionHandler) { AdjustParentNodeLayout = false };
            // wrap it in a NonOverlapPositionHandler that removes overlaps during and after the move gesture
            return new NonOverlapPositionHandler(node, handler);
          });

      // use a reshape handler that avoids overlapping
      graph.GetDecorator().NodeDecorator.ReshapeHandlerDecorator.SetFactory(node => {
        // Lookup the node reshape handler that only reshapes the node itself (and not its parent group node)
        var defaultReshapeHandler = DefaultGraph.DefaultNodeLookup.Lookup<IReshapeHandler>(node);
        // wrap it in a NonOverlapReshapeHandler that removes overlaps during and after the resize gesture
        return new NonOverlapReshapeHandler(node, defaultReshapeHandler);
      });

      // set a size constraint provider for a minimum node size for all nodes not already providing their own one
      var minimumSize = new SizeD(5, 5);
      var sizeConstraintProviderDecorator = graph.GetDecorator().NodeDecorator.SizeConstraintProviderDecorator;
      sizeConstraintProviderDecorator.DecorateNulls = true;
      sizeConstraintProviderDecorator.SetImplementationWrapper((node, provider) => 
          provider ?? new NodeSizeConstraintProvider(minimumSize, SizeD.Infinite));
      
      // avoid overlapping when creating, pasting or duplicating nodes
      editMode.NodeCreated += (sender, args) => MakeSpace(args.Item);
      GraphControl.Clipboard.FromClipboardCopier.NodeCopied += (sender, args) => MakeSpace(args.Copy);
      GraphControl.Clipboard.DuplicateCopier.NodeCopied += (sender, args) => MakeSpace(args.Copy);
    }

    /// <summary>
    /// Makes space for a new node.
    /// </summary>
    private void MakeSpace(INode node) {
      new LayoutHelper(GraphControl, node).LayoutImmediately();
    }

    /// <summary>
    /// Initializes styles and loads a sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      DemoStyles.InitDemoStyles(GraphControl.Graph);
      GraphControl.ImportFromGraphML("Resources\\grouping.graphml");
    }

    #endregion
    
  }
}
