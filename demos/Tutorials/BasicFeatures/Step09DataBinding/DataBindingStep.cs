/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Controls;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step09DataBinding
{
  /// <summary>
  ///   Tutorial step 09: Data Binding.
  /// </summary>
  /// <remarks>
  ///   Shows how to store arbitrary business data in the <see cref="ITagOwner.Tag"/> of graph
  ///   elements and how to use it in tooltips and context menus.
  /// </remarks>
  public class DataBindingStep : TutorialStepBase
  {
    public override string Title => "09 Data Binding";

    public override string DescriptionSource => "Step09DataBinding.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);

      var inputMode = new GraphEditorInputMode();
      graphControl.InputMode = inputMode;

      EnableDataBinding(graph, inputMode);
      SetupTooltips(inputMode);
      SetupContextMenu(inputMode);

      BasicFeaturesTutorial.CreateSampleGraph(graph);
      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);
    }

    /// <summary>
    ///   Sets up simple data binding by storing the creation date in the node tag.
    /// </summary>
    private static void EnableDataBinding(IGraph graph, GraphEditorInputMode inputMode) {
      // Typically you specify the tag of an item at creation time.
      // Store the current time as the node creation time.
      var n = graph.CreateNode(new RectD(0, 80, 30, 30));
      n.Tag = DateTime.Now;

      // Subscribe to the low-level node creation event to record the node creation time.
      // Note: this event also fires during undo/redo, loading, and clipboard operations.
      graph.NodeCreated += (_, evt) => {
        // Store the current time as node creation time
        var node = evt.Item;
        // if there is no tag associated with the node already, add one
        if (node.Tag == null) {
          node.Tag = DateTime.Now;
        }
      };

      // If we only want to handle events caused by interactive node creation,
      // we register to the node creation event of the input mode instead.
      inputMode.NodeCreated += (_, evt) => {
        // Store the current time as node creation time
        var node = evt.Item;
        node.Tag = DateTime.Now;
      };
    }

    /// <summary>
    ///   Sets up tooltips that return the value stored in the tag of the nodes.
    /// </summary>
    private static void SetupTooltips(GraphEditorInputMode inputMode) {
      inputMode.ToolTipItems = GraphItemTypes.Node;
      inputMode.QueryItemToolTip += (_, evt) => {
        if (evt.Handled) {
          // Tooltip content has already been assigned — nothing to do
          return;
        }
        if (evt.Item is INode node) {
          // Set the tooltip content
          evt.ToolTip = node.Tag is DateTime date ? $"Created: {date}" : "Who knows?";

          // Indicate that the tooltip content has been set
          evt.Handled = true;
        }
      };
    }

    /// <summary>
    ///   Adds a context menu for nodes.
    /// </summary>
    private static void SetupContextMenu(GraphEditorInputMode inputMode) {
      inputMode.ContextMenuItems = GraphItemTypes.Node;

      inputMode.PopulateItemContextMenu += (_, evt) => {
        if (evt.Item is INode node) {
          var menuItem = new MenuItem { Header = "Set to now" };
          menuItem.Click += (_, _) => node.Tag = DateTime.Now;
          evt.Menu.Items.Add(menuItem);
          evt.ShowMenu = true;
          evt.Handled = true;
        }
      };
    }
  }
}
