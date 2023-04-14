/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.MarqueeClearArea
{
  /// <summary>
  /// A demo that shows how to interactively move graph elements within a marquee rectangle in a given graph
  /// layout so that the modifications in the graph are minimal.
  /// </summary>
  public partial class MarqueeClearAreaDemo
  {
    /// <summary>
    /// Performs layout and animation while dragging the marquee.
    /// </summary>
    private ClearAreaLayoutHelper layoutHelper;

    /// <summary>
    /// A <see cref="ResourceKey"/> that will be used to find the <see cref="DataTemplate"/> for
    /// drawing the marquee.
    /// </summary>
    public static readonly ResourceKey ClearRectTemplateKey =
        new ComponentResourceKey(typeof(RectangleIndicatorInstaller), "ClearRectTemplateKey");

    #region Initialization
    
    public MarqueeClearAreaDemo() {
      InitializeComponent();
    }

    private void OnLoad(object sender, EventArgs e) {
      InitializeInputModes();
      InitializeGraph();
    }

    /// <summary>
    /// Registers the <see cref="GraphEditorInputMode"/> as the <see cref="CanvasControl.InputMode"/>
    /// and initializes the marquee input mode that clears the area of the marquee rectangle.
    /// </summary>
    private void InitializeInputModes() {
      // enable undo/redo support
      GraphControl.Graph.SetUndoEngineEnabled(true);
      
      // create an input mode to edit graphs
      var editMode = new GraphEditorInputMode();

      // create an input mode to clear the area of a marquee rectangle
      // using the right mouse button
      var marqueeClearInputMode = new MarqueeSelectionInputMode {
          PressedRecognizer = MouseEventRecognizers.RightPressed,
          DraggedRecognizer = MouseEventRecognizers.RightDragged,
          ReleasedRecognizer = MouseEventRecognizers.RightReleased,
          CancelRecognizer = KeyEventRecognizers.EscapePressed.Or(MouseEventRecognizers.LostCaptureDuringDrag),
          Template = GraphControl.FindResource(ClearRectTemplateKey) as DataTemplate
      };
      // handle dragging the marquee
      marqueeClearInputMode.DragStarting += OnDragStarting;
      marqueeClearInputMode.Dragged += OnDragged;
      marqueeClearInputMode.DragCanceled += OnDragCanceled;
      marqueeClearInputMode.DragFinished += OnDragFinished;
      // add this mode to the edit mode 
      editMode.Add(marqueeClearInputMode);

      // and install the edit mode into the canvas
      GraphControl.InputMode = editMode;
    }

    /// <summary>
    /// Initializes styles and loads a sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      DemoStyles.InitDemoStyles(GraphControl.Graph);
      // The content area of the group nodes should not be hit transparent in this demo,
      // since we also want to allow the marquee rectangle within group nodes.
      ((GroupNodeStyle) GraphControl.Graph.GroupNodeDefaults.Style).HitTransparentContentArea = false;
      
      GraphControl.ImportFromGraphML("Resources\\grouping.graphml");
    }

    #endregion

    #region Dragging the marquee rectangle

    /// <summary>
    /// The marquee rectangle is upon to be dragged.
    /// </summary>
    private void OnDragStarting(object sender, MarqueeSelectionEventArgs e) {
      var hitGroupNode = GetHitGroupNode(e.Context, e.Context.CanvasControl.LastEventLocation);
      layoutHelper = new ClearAreaLayoutHelper(GraphControl) {
          ClearRectangle = e.Rectangle,
          GroupNode = hitGroupNode
      };
      layoutHelper.InitializeLayout();
    }

    /// <summary>
    /// The marquee rectangle is currently dragged.
    /// </summary>
    /// <remarks>
    /// For each drag a new layout is calculated and applied if the previous one is completed.
    /// </remarks>
    private void OnDragged(object sender, MarqueeSelectionEventArgs e) {
      layoutHelper.ClearRectangle = e.Rectangle;
      layoutHelper.RunLayout();
    }

    /// <summary>
    /// Dragging the marquee rectangle has been canceled so
    /// the state before the gesture must be restored.
    /// </summary>
    private void OnDragCanceled(object sender, MarqueeSelectionEventArgs e) {
      layoutHelper.ClearRectangle = e.Rectangle;
      layoutHelper.CancelLayout();
    }

    /// <summary>
    /// Dragging the marquee rectangle has been finished so
    /// we execute the layout with the final rectangle.
    /// </summary>
    private void OnDragFinished(object sender, MarqueeSelectionEventArgs e) {
      layoutHelper.ClearRectangle = e.Rectangle;
      layoutHelper.StopLayout();
    }
    
    /// <summary>
    /// Returns the group node at the given location.
    /// </summary>
    /// <remarks>
    /// If there is no group node, <code>null</code> is returned.
    /// </remarks>
    private INode GetHitGroupNode(IInputModeContext context, PointD location) {
      return context.Lookup<IHitTester<INode>>()
                    .EnumerateHits(context, location)
                    .FirstOrDefault(n => GraphControl.Graph.IsGroupNode(n));
    }

    #endregion

  }
}
