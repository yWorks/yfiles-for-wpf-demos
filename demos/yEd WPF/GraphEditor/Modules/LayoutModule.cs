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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Algorithms;
using yWorks.Layout.Grid;
using yWorks.Layout.Grouping;
using yWorks.Algorithms.Geometry;
using GroupingSupport = yWorks.Layout.Grouping.GroupingSupport;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// Abstract base class for <see cref="YModule"/> instances that can be used to configure 
  /// graph layout algorithm instances.
  /// </summary>
  public abstract class LayoutModule : YModule
  {
    private ILayoutAlgorithm layoutAlgorithm;
    private IGraph graph;
    private CopiedLayoutGraph layoutGraph;
    private TableLayoutConfigurator tableLayoutConfigurator;
    private bool configureTableNodeLayout = false;
    private bool layoutMorphingEnabled = true;
    private bool viewPortMorphingEnabled = true;
    private AbortDialog abortDialog;

    /// <summary>
    /// The table layout configurator that is used if <see cref="ConfigureTableNodeLayout"/> is enabled.
    /// </summary>
    /// <remarks>Upon first access, </remarks>
    public TableLayoutConfigurator TableLayoutConfigurator {
      get {
        if (tableLayoutConfigurator == null) {
          tableLayoutConfigurator = CreateTableLayoutConfigurator();
        }
        return tableLayoutConfigurator;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically perform calls to <see cref="yWorks.Layout.TableLayoutConfigurator.Prepare"/> 
    /// and <see cref="yWorks.Layout.TableLayoutConfigurator.Restore"/> in order to layout table nodes.
    /// </summary>
    /// <value><see langword="true"/> if table layout should be performed automatically; <see langword="false"/> otherwise. Default value is <see langword="true"/></value>
    public bool ConfigureTableNodeLayout {
      get { return configureTableNodeLayout; }
      set { configureTableNodeLayout = value; }
    }

    /// <summary>
    /// Return the <see cref="LayoutGraph"/> that the module should work on.
    /// </summary>
    public LayoutGraph CurrentLayoutGraph {
      get { return Context.Lookup<LayoutGraph>(); }
    }

    /// <summary>
    /// Gets or sets the layout algorithm to configure.
    /// </summary>
    public ILayoutAlgorithm LayoutAlgorithm {
      get { return layoutAlgorithm; }
      set { layoutAlgorithm = value; }
    }

    /// <summary>
    /// Get or set whether layout morphing should be enabled.
    /// </summary>
    public bool LayoutMorphingEnabled
    {
      get { return layoutMorphingEnabled; }
      set { layoutMorphingEnabled = value; }
    }

    /// <summary>
    /// Get or set whether viewport morphing should be enabled. This option 
    /// only has an effect if layout morphing is enabled
    /// </summary>    
    public bool ViewPortMorphingEnabled
    {
      get { return viewPortMorphingEnabled; }
      set { viewPortMorphingEnabled = value; }
    }

    /// <summary>
    /// Whether the layout calculation should be executed in a background thread.
    /// </summary>
    public bool RunInBackground { get; set; }

    /// <summary>
    /// Create a new instance
    /// </summary>
    /// <param name="moduleName"></param>
    protected LayoutModule(string moduleName) : base(moduleName) {
      Done += OnDone;
      RunInBackground = true;
    }

    /// <summary>
    /// Callback method that can be used for actions that take place 
    /// immediately before the actual layout is started
    /// </summary>
    protected virtual void PerformPreLayout() {}

    /// <summary>
    /// Callback method that can be used for actions that take place 
    /// immediately after the actual layout is started
    /// </summary>
    protected virtual void PerformPostLayout() {}

    ///<inheritdoc/>
    protected override void ConfigureModule() {
      base.ConfigureModule();
      ConfigureLayout();
    }

    /// <summary>
    /// This method is used to configure the layout algorithm from the module settings
    /// </summary>
    protected abstract void ConfigureLayout();

    /// <summary>
    /// This method performs the actual layout
    /// </summary>
    protected virtual void LaunchLayout() {
      layoutAlgorithm.ApplyLayout(CurrentLayoutGraph);
    }

    ///<inheritdoc/>
    protected override Task RunModule() {
      try {
        PerformPreLayout();
        LaunchLayout();
      } 
      finally {
        PerformPostLayout();
      }
      return Task.FromResult<object>(null);
    }

    ///<inheritdoc/>
    public override async Task Start(ILookup newContext) {
      LayoutGraph layoutGraph = newContext.Lookup<LayoutGraph>();
      if (layoutGraph == null) {
        IGraph graph = newContext.Lookup<IGraph>();
        if (graph != null) {
          await StartWithIGraph(graph, newContext);
        }
      } else {
        try {
          await base.Start(newContext);
        } catch (InvalidGraphStructureException wex) {
          MessageBox.Show("Exception " + wex.Message + " when launching layout algorithm!",
            "InvalidGraphStructureException", MessageBoxButton.OK, MessageBoxImage.Error);
          Trace.WriteLine("InvalidGraphStructureException when launching layout algorithm!" + wex.StackTrace);
        } catch (AlgorithmAbortedException) {
          //that's ok. do nothing then.
          throw;
        } catch (Exception ex) {
          MessageBox.Show("Exception " + ex.Message + " when launching layout algorithm!\nStackTrace: " + ex.StackTrace,
            "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
          //if we are debugging in the IDE, we also want a trace entry 
          Trace.WriteLine("Exception when launching layout algorithm!" + ex.StackTrace);
          throw;
        }
      }
    }

    /// <summary>
    /// Check whether any nodes have been selected
    /// </summary>
    /// <returns><see langword="true"/> iff no node has been selected</returns>
    public bool IsNodeSelectionEmpty() {
      foreach (Node node in CurrentLayoutGraph.Nodes) {
        if (IsSelected(node)) {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Check whether any edges have been selected
    /// </summary>
    /// <returns><see langword="true"/> iff no edge has been selected</returns>
    public bool IsEdgeSelectionEmpty() {
      foreach (Edge edge in CurrentLayoutGraph.Edges) {
        if (IsSelected(edge)) {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Check whether the specified node has been selected
    /// </summary>
    /// <param name="n">The node to check</param>
    /// <returns><see langword="true"/> iff Node <paramref name="n"/> has been selected</returns>
    public virtual bool IsSelected(Node n) {
      IDataProvider selectedNodes = CurrentLayoutGraph.GetDataProvider(LayoutKeys.AffectedNodesDpKey);
      return (selectedNodes != null) && selectedNodes.GetBool(n);
    }

    /// <summary>
    /// Check whether the specified model item has been selected in the underlying
    /// IGraph.
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns><see langword="true"/> iff ModelItem <paramref name="item"/> has been selected</returns>
    public virtual bool IsSelected(IModelItem item) {
      ISelectionModel<IModelItem> selectionModel = Context.Lookup<ISelectionModel<IModelItem>>();
      if(selectionModel != null) {
        return selectionModel.IsSelected(item);
      }
      return false;
    }

    /// <summary>
    /// Check whether the specified edge has been selected
    /// </summary>
    /// <param name="e">The Edge to check</param>
    /// <returns><see langword="true"/> iff Edge <paramref name="e"/> has been selected</returns>
    public virtual bool IsSelected(Edge e) {
      IDataProvider selectedEdges = CurrentLayoutGraph.GetDataProvider(LayoutKeys.AffectedEdgesDpKey);
      return (selectedEdges != null) && selectedEdges.GetBool(e);
    }

    /// <summary>
    /// Check whether the current graph is flat
    /// </summary>
    /// <returns><see langword="true"/> iff <see cref="CurrentLayoutGraph"/> is flat</returns>
    public virtual bool IsFlat() {
      return GroupingSupport.IsFlat(CurrentLayoutGraph);
    }

    /// <summary>
    /// Check whether the current graph is grouped
    /// </summary>
    /// <returns><see langword="true"/> iff <see cref="CurrentLayoutGraph"/> is grouped</returns>
    public virtual bool IsGrouped() {
      return GroupingSupport.IsGrouped(CurrentLayoutGraph);
    }

    /// <summary>
    /// Check whether the current graph contains group nodes
    /// </summary>
    /// <returns><see langword="true"/> iff <see cref="CurrentLayoutGraph"/> contains group nodes</returns>
    public virtual bool ContainsGroupNodes() {
      IDataProvider grouped = CurrentLayoutGraph.GetDataProvider(GroupingKeys.GroupDpKey);
      if(grouped == null) {
        return false;
      }
      foreach (Node n in CurrentLayoutGraph.Nodes) {
        if(grouped.GetBool(n)) {
          return true;
        }
      }
      return false;
    }

    ///<inheritdoc/>
    protected override Type[] GetRequiredTypes() {
      return new[] {typeof (LayoutGraph)};
    }

    /// <summary>
    /// Executes the module on the given graph using the provided context.
    /// </summary>
    /// <remarks>
    /// The layout will be calculated <see cref="RunInBackground">optionally</see> 
    /// in a separate thread in method <see cref="RunModuleAsync"/>.
    /// </remarks>
    /// <param name="graph">The graph to execute on.</param>
    /// <param name="newContext">The context to use. This method will query a <c>ISelectionModel&lt;IModelItem></c></param>
    /// for the selected nodes and edges and the <c>GraphControl</c> to morph the layout.
    protected virtual async Task StartWithIGraph(IGraph graph, ILookup newContext) {
      this.graph = graph;
      if (ShouldConfigureTableLayout()) {
        PrepareTableLayout();
      }
      ISelectionModel<IModelItem> selectionModel = newContext.Lookup<ISelectionModel<IModelItem>>();
      LayoutGraphAdapter adapter = new LayoutGraphAdapter(graph, selectionModel);
      this.layoutGraph = adapter.CreateCopiedLayoutGraph();
      ILookup additionalLookup = Lookups.Single(layoutGraph, typeof (LayoutGraph));
      ILookup wrappedLookup = Lookups.Wrapped(newContext, additionalLookup);
      try {
        ICompoundEdit compoundEdit = graph.BeginEdit("Layout", "Layout");
        CheckReentrant(wrappedLookup);
        ConfigureModule();

        if (RunInBackground) {
          // without the LayoutExecutor helper class on the layout graph side of things, we register the aborthandler
          // to the layout graph with the utility method provided by AbortHandler
          var abortHandler = AbortHandler.CreateForGraph(layoutGraph);
          // now create the dialog that controls the abort handler
          abortDialog = new AbortDialog { AbortHandler = abortHandler, Owner = Application.Current.MainWindow };
          // start the layout in another thread.
          var layoutThread = new Thread(async () => await RunModuleAsync(wrappedLookup, graph, compoundEdit));
          
          // now if we are not doing a quick layout - and if it takes more than a few seconds, we open the dialog to 
          // enable the user to stop or cancel the execution 
          var showDialogTimer = new DispatcherTimer(DispatcherPriority.Normal, abortDialog.Dispatcher)
          {
            Interval = TimeSpan.FromSeconds(2)
          };

          showDialogTimer.Tick += delegate {
            // it could be that the layout is already done - so check whether we still 
            // need to open the dialog
            var dialogInstance = abortDialog;
            if (dialogInstance != null) {
              // open the abort dialog
              dialogInstance.Show();
            }
            // we only want to let it go off once - so stop the timer
            showDialogTimer.Stop();
          };

          // kick-off the timer and the layout
          showDialogTimer.Start();
          layoutThread.Start();
        } else {
          await RunModuleAsync(wrappedLookup, graph, compoundEdit);
        }
      } catch (Exception e) {
        FreeReentrant();
        TableLayoutConfigurator.CleanUp(graph);
        OnDone(new LayoutEventArgs(e));
        //optionally do something here...
      }
    }

    /// <summary>
    /// Runs the layout and disposes the module afterwards.
    /// </summary>
    /// <remarks>
    /// This method will be called in a separate thread. If the layout has been called successfully
    /// the method <see cref="LayoutDone"/> will be called afterwards to apply the layout.
    /// If the layout has been canceled or an error has happened during layout the <see cref="Done"/>
    /// will be raised with an instance of <see cref="LayoutEventArgs"/>.
    /// </remarks>
    /// <param name="moduleContext">The module context for this operation.</param>
    /// <param name="graph">The graph to apply the layout to.</param>
    /// <param name="compoundEdit">The undo edit which wraps the layout. It has been created in <see cref="StartWithIGraph"/>
    /// and will be closed after a successful layout or canceled otherwise.</param>
    private async Task RunModuleAsync(ILookup moduleContext, IGraph graph, ICompoundEdit compoundEdit)
    {
      GraphControl view = moduleContext.Lookup<GraphControl>();
      try {
        await RunModule();
        Dispose();
        if (view.Dispatcher.CheckAccess()) {
          await LayoutDone(graph, moduleContext, compoundEdit);
        } else {
          Invoke(view.Dispatcher, new Action(async () => await LayoutDone(graph, moduleContext, compoundEdit)));
        }
      } catch (ThreadAbortException tae) {
        compoundEdit.Cancel();
        if (view.Dispatcher.CheckAccess()) {
          OnDone(new LayoutEventArgs(tae));
        } else {
          await view.Dispatcher.BeginInvoke(new Action(() => OnDone(new LayoutEventArgs(tae))));
        }
      } catch (AlgorithmAbortedException aae) {
        // layout was canceled. do nothing then.
        compoundEdit.Cancel();
        if (view.Dispatcher.CheckAccess()) {
          OnDone(new LayoutEventArgs(aae));
        } else {
          Invoke(view.Dispatcher, new Action(() => OnDone(new LayoutEventArgs(aae))));
        }
      } catch (Exception ex) {
        compoundEdit.Cancel();
        if (view.Dispatcher.CheckAccess()) {
          OnDone(new LayoutEventArgs(ex));
        } else {
          Invoke(view.Dispatcher, new Action(() => OnDone(new LayoutEventArgs(ex))));
        }
      } finally {
        FreeReentrant();
        TableLayoutConfigurator.CleanUp(graph);
      }
    }

    /// <summary>
    /// Called after the layout has been calculated.
    /// </summary>
    /// <remarks>
    /// Applies the layout in an animation and cleans up. Calls OnDone to raise the Done event after the animation has been completed.
    /// </remarks>
    /// <param name="graph">The graph to apply the layout to.</param>
    /// <param name="moduleContext">The module context.</param>
    /// <param name="compoundEdit">The undo edit which wraps the layout. It was created in <see cref="StartWithIGraph"/> and will be closed here.</param>
    protected virtual async Task LayoutDone(IGraph graph, ILookup moduleContext, ICompoundEdit compoundEdit) {
      if (abortDialog != null) {
        if (abortDialog.IsVisible) {
          abortDialog.Close();
        }
        abortDialog = null;
      }
      GraphControl view = moduleContext.Lookup<GraphControl>();
      if (LayoutMorphingEnabled && view != null) {
        var morphingAnimation =
            graph.CreateLayoutAnimation(layoutGraph, TimeSpan.FromSeconds(1));

        Rectangle2D box =
            LayoutGraphUtilities.GetBoundingBox(layoutGraph, layoutGraph.GetNodeCursor(), layoutGraph.GetEdgeCursor());
        RectD targetBounds = box.ToRectD();
        ViewportAnimation vpAnim =
            new ViewportAnimation(view,
                targetBounds, TimeSpan.FromSeconds(1)) {
                MaximumTargetZoom = 1.0d, TargetViewMargins = view.FitContentViewMargins
            };

        var animations = new List<IAnimation>();
        animations.Add(morphingAnimation);
        animations.Add(CreateTableAnimations());
        if (ViewPortMorphingEnabled) {
          animations.Add(vpAnim);
        }
        TableLayoutConfigurator.CleanUp(graph);

        Animator animator = new Animator(view);
        await animator.Animate(animations.CreateParallelAnimation().CreateEasedAnimation());
        try {
          compoundEdit.Commit();
          view.UpdateContentRect();
        } finally {
          OnDone(new LayoutEventArgs());
        }
      } else {
        layoutGraph.CommitLayoutToOriginalGraph();
        RestoreTableLayout();
        compoundEdit.Commit();
        if (view != null) {
          view.UpdateContentRect();
        }
        OnDone(new LayoutEventArgs());
      }
    }

    /// <summary>
    /// Set up <see cref="TableLayoutConfigurator"/> for an actual layout run.
    /// </summary>
    /// <remarks>This implementation configures <see cref="yWorks.Layout.TableLayoutConfigurator.HorizontalLayout"/> according to the <see cref="MultiStageLayout.LayoutOrientation"/> and calls 
    /// <see cref="yWorks.Layout.TableLayoutConfigurator.Prepare"/></remarks>
    protected virtual void PrepareTableLayout() {
      var cms = layoutAlgorithm as MultiStageLayout; 
      if (cms != null && cms.OrientationLayout is OrientationLayout && cms.OrientationLayoutEnabled) {
        TableLayoutConfigurator.HorizontalLayout = (((OrientationLayout)cms.OrientationLayout).HorizontalOrientation);
      }
      if (graph != null) {
        TableLayoutConfigurator.Prepare(graph);
      }
    }

    /// <summary>
    /// Writes the table layout information provided through <see cref="TableLayoutConfigurator"/> back to all tables.
    /// </summary>
    /// <remarks>This method is only called when the layout is not animated.</remarks>
    /// <seealso cref="PrepareTableLayout"/>
    protected virtual void RestoreTableLayout() {
      if (graph != null) {
        TableLayoutConfigurator.Restore(graph);
      }
    }

    /// <summary>
    /// Create a new instance of <see cref="TableLayoutConfigurator"/> that is used if <see cref="ConfigureTableNodeLayout"/> is enabled.
    /// </summary>
    /// <remarks>This method is called upon first access to <see cref="TableLayoutConfigurator"/></remarks>
    /// <returns>A new instance of <see cref="TableLayoutConfigurator"/></returns>
    protected virtual TableLayoutConfigurator CreateTableLayoutConfigurator() {
      return new TableLayoutConfigurator();
    }

    private bool ShouldConfigureTableLayout() {
      //If there is already a mapper, do nothing
      return ConfigureTableNodeLayout && graph != null && !graph.MapperRegistry.RegisteredTags.Contains(PartitionGrid.PartitionGridDpKey);
    }

    /// <summary>
    /// Creates an animation that morphs the layout of all <see cref="ITable"/>s in the graph.
    /// </summary>
    /// <seealso cref="TableAnimation"/>
    /// <seealso cref="ConfigureTableNodeLayout"/>
    protected virtual IAnimation CreateTableAnimations() {
      var anims = new List<IAnimation>();
      foreach (var node in graph.Nodes) {
        var table = node.Lookup<ITable>();
        if (table != null) {
          INodeLayout nodeLayout = layoutGraph.GetLayout(layoutGraph.GetCopiedNode(node));

          var columnLayout = TableLayoutConfigurator.GetColumnLayout(table,
            new RectD(nodeLayout.X, nodeLayout.Y, nodeLayout.Width, nodeLayout.Height));
          var rowLayout = TableLayoutConfigurator.GetRowLayout(table,
            new RectD(nodeLayout.X, nodeLayout.Y, nodeLayout.Width, nodeLayout.Height));
          anims.Add(new TableAnimation(table, columnLayout, rowLayout));
        }
      }
      return anims.CreateParallelAnimation();
    }

    private static void Invoke(Dispatcher dispatcher, Delegate d, params object[] args) {
      if (!dispatcher.CheckAccess()) {
        dispatcher.Invoke(d, DispatcherPriority.Normal, args);
      } else {
        d.DynamicInvoke(args);
      }
    }

    /// <summary>
    /// Raises the <see cref="Done"/> event.
    /// </summary>
    /// <param name="args">The <see cref="LayoutEventArgs"/> instance containing the event data.</param>
    protected virtual void OnDone(LayoutEventArgs args) {
      if (Done != null) {
        Done(this, args);
      }
    }

    /// <summary>
    /// Occurs when the layout has been done or an exception has occurred.
    /// </summary>
    public event EventHandler<LayoutEventArgs> Done;

    private void OnDone(object sender, EventArgs eventArgs) {
      if (layoutGraph != null) {
        layoutGraph.RemoveDataProvider(AbortHandler.AbortHandlerDpKey);
        layoutGraph = null;
      }
      graph = null;
      if (abortDialog != null) {
        abortDialog.Close();
        abortDialog = null;
      }
    }

    /// <summary>
    /// Helper method that applies the label model parameter to all edge labels in the graph.
    /// </summary>
    /// <param name="graph">The graph to apply the label model parameter.</param>
    /// <param name="param">The parameter to use.</param>
    protected virtual void ApplyModelToIGraph(IGraph graph, ILabelModelParameter param) {
      if (param != null) {
        foreach (ILabel label in graph.Labels) {
          if (label.Owner is IEdge) {
            graph.SetLabelLayoutParameter(label, param);
          }
        }
      }
    }
  }
}
