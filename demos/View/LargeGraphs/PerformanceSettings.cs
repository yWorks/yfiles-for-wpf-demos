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

using System.ComponentModel;
using Demo.yFiles.Graph.LargeGraphs.Styles;
using Demo.yFiles.Graph.LargeGraphs.Styles.Fast;
using Demo.yFiles.Graph.LargeGraphs.Styles.LevelOfDetail;
using Demo.yFiles.Graph.LargeGraphs.Styles.Selection;
using Demo.yFiles.Graph.LargeGraphs.Styles.Virtualization;
using yWorks.Controls;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.LargeGraphs
{
  /// <summary>
  ///   Collection of settings regarding performance optimizations in this demo.
  /// </summary>
  public class PerformanceSettings : INotifyPropertyChanged
  {
    #region Backing fields

    private bool overviewDisabled;
    private bool fastStylesEnabled;
    private double minimumEdgeLength;
    private double edgeBendThreshold;
    private double edgeLabelVisibilityThreshold;
    private double nodeLabelVisibilityThreshold;
    private double edgeLabelTextThreshold;
    private double nodeLabelTextThreshold;
    private double complexNodeStyleThreshold;
    private bool virtualizationDisabled;
    private double edgeVirtualizationThreshold;
    private double nodeVirtualizationThreshold;
    private bool selectionHandlesDisabled;
    private bool customSelectionDecoratorEnabled;
    private bool labelModelBakingEnabled;
    private bool dirtyHandlingOptimizationEnabled;

    #endregion

    #region Overview

    /// <summary>
    ///   Gets or sets a value indicating whether the graph overview should be disabled.
    /// </summary>
    /// <remarks>
    ///   The <see cref="GraphOverviewControl" /> is almost the same as the normal graph control, just drawn
    ///   with less fidelity, not as often and usually smaller. With very large graphs, however, updating the overview can
    ///   incur a significant performance hit, so it's sometimes better to just turn it off.
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateOverviewDisabledSetting"/>
    public bool OverviewDisabled {
      get { return overviewDisabled; }
      set {
        overviewDisabled = value;
        OnPropertyChanged("OverviewDisabled");
      }
    }

    #endregion

    #region Fast styles

    /// <summary>
    ///   Gets or sets a value indicating whether fast styles are enabled.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     A large portion of rendering time is spent with creating and updating visuals of graph items via their styles. Most
    ///     default styles are intended for larger zoom levels and graphs of about a few hundred elements. With very large
    ///     graphs simplifying the appearance (and even drawing nothing at times) at lower zoom levels is a viable strategy to
    ///     improve performance (and also readability of the graph). In this regard, features with a large performance impact
    ///     are:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>Edges are clipped at their end nodes, which is impossible to see at low zoom levels, if at all.</item>
    ///     <item>Edges are drawn with bends, which are also hard to see at low zoom levels.</item>
    ///     <item>Labels are drawn even at low zoom levels where they aren't readable, let alone visible.</item>
    ///   </list>
    ///   <para>
    ///     Enabling fast styles then makes the following changes that improve performance considerably:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>Level-of-detail style for nodes that adapts the graphical fidelity to the zoom level.</item>
    ///     <item>Level-of-detail style for labels that hides labels below a certain zoom level.</item>
    ///     <item>
    ///       A simpler label style in between hiding labels and showing them fully, that just renders a rough shape of the
    ///       text.
    ///     </item>
    ///     <item>A simpler edge style that hides very short edges and doesn't render bends below a configurable zoom level.</item>
    ///   </list>
    /// </remarks>
    /// <seealso cref="LevelOfDetailNodeStyle" />
    /// <seealso cref="LevelOfDetailEdgeStyle" />
    /// <seealso cref="LevelOfDetailLabelStyle" />
    /// <seealso cref="FastEdgeStyle" />
    /// <seealso cref="FastLabelStyle" />
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public bool FastStylesEnabled {
      get { return fastStylesEnabled; }
      set {
        fastStylesEnabled = value;
        OnPropertyChanged("FastStylesEnabled");
      }
    }

    /// <summary>
    ///   Gets or sets the minimum length at which edges are still drawn.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="FastStylesEnabled" /> is <see langword="true" />.</para>
    ///   <para>
    ///     Edges shorter than this length in pixels are hidden.
    ///   </para>
    /// </remarks>
    /// <seealso cref="FastEdgeStyle.MinimumEdgeLength" />
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double MinimumEdgeLength {
      get { return minimumEdgeLength; }
      set {
        minimumEdgeLength = value;
        OnPropertyChanged("MinimumEdgeLength");
      }
    }

    /// <summary>
    ///   Gets or sets the zoom level below which bends are not rendered on edges.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="FastStylesEnabled" /> is <see langword="true" />.</para>
    ///   <para>Below this zoom level edges won't show bends, simplifying their appearance.</para>
    /// </remarks>
    /// <seealso cref="FastEdgeStyle.DrawBendsThreshold" />
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double EdgeBendThreshold {
      get { return edgeBendThreshold; }
      set {
        edgeBendThreshold = value;
        OnPropertyChanged("EdgeBendThreshold");
      }
    }

    /// <summary>
    ///   Gets or sets the zoom level below which to hide edge labels.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="FastStylesEnabled" /> is <see langword="true" />.</para>
    ///   <para>
    ///     Below this zoom level edge labels are hidden. Above it, edge labels will be rendered in a simplified appearance
    ///     (see <see cref="FastLabelStyle" />).
    ///   </para>
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double EdgeLabelVisibilityThreshold {
      get { return edgeLabelVisibilityThreshold; }
      set {
        edgeLabelVisibilityThreshold = value;
        OnPropertyChanged("EdgeLabelVisibilityThreshold");
      }
    }

    /// <summary>
    ///   Gets or sets the zoom level below which to hide node labels.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="FastStylesEnabled" /> is <see langword="true" />.</para>
    ///   <para>
    ///     Below this zoom level node labels are hidden. Above it, node labels will be rendered in a simplified appearance
    ///     (see <see cref="FastLabelStyle" />).
    ///   </para>
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double NodeLabelVisibilityThreshold {
      get { return nodeLabelVisibilityThreshold; }
      set {
        nodeLabelVisibilityThreshold = value;
        OnPropertyChanged("NodeLabelVisibilityThreshold");
      }
    }

    /// <summary>
    ///   Gets or sets the zoom level above which to show edge labels with text.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="FastStylesEnabled" /> is <see langword="true" />.</para>
    ///   <para>
    ///     Below this zoom level edge labels are rendered in a simplified appearance (<see cref="FastLabelStyle" />). Above
    ///     it, edge labels will be rendered in full fidelity and readable.
    ///   </para>
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double EdgeLabelTextThreshold {
      get { return edgeLabelTextThreshold; }
      set {
        edgeLabelTextThreshold = value;
        OnPropertyChanged("EdgeLabelTextThreshold");
      }
    }

    /// <summary>
    ///   Gets or sets the zoom level above which to show node labels with text.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="FastStylesEnabled" /> is <see langword="true" />.</para>
    ///   <para>
    ///     Below this zoom level node labels are rendered in a simplified appearance (<see cref="FastLabelStyle" />). Above
    ///     it, node labels will be rendered in full fidelity and readable.
    ///   </para>
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double NodeLabelTextThreshold {
      get { return nodeLabelTextThreshold; }
      set {
        nodeLabelTextThreshold = value;
        OnPropertyChanged("NodeLabelTextThreshold");
      }
    }

    /// <summary>
    ///   Gets or sets the zoom level above which to display nodes in a visually more complex style.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="FastStylesEnabled" /> is <see langword="true" />.</para>
    ///   <para>
    ///     This setting controls actually two zoom level thresholds between three different node styles:
    ///   </para>
    ///   <list type="number">
    ///     <item>A very simple visualization as a rectangle with no outline.</item>
    ///     <item>A nicer, but still simple visualization as a rounded rectangle with an outline.</item>
    ///     <item>A pretty visualization with highlights and gradients.</item>
    ///   </list>
    ///   <para>The transition between 1 and 2 is this zoom level halved. The transition between 2 and 3 is this zoom level.</para>
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double ComplexNodeStyleThreshold {
      get { return complexNodeStyleThreshold; }
      set {
        complexNodeStyleThreshold = value;
        OnPropertyChanged("ComplexNodeStyleThreshold");
      }
    }

    #endregion

    #region Virtualization

    /// <summary>
    ///   Gets or sets a value indicating whether virtualization should be disabled at low zoom levels.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Virtualization in <see cref="CanvasControl" /> trims the WPF visual tree to the elements that are actually
    ///     visible. This works well when only a few hundred visuals need to be removed or added at a time but results in
    ///     significant stutter if thousands of elements are affected. The later happens typically while panning or zooming in
    ///     large graph at low zoom level.
    ///   </para>
    ///   <para>
    ///     To avoid this, virtualization should be disabled at low zoom levels but kept enabled at higher zoom levels, as
    ///     it will lower memory usage (and improve other performance figures).
    ///   </para>
    ///   <para>
    ///     Note that these values are very sensitive to the displayed graph and its layout as the values depend mostly on
    ///     how many items are visible at once and thus how many items are likely to be virtualized when zooming or panning. In
    ///     this demo labels are always virtualized because we assume that labels are not visible at zoom levels so low that
    ///     virtualization would provide.
    ///   </para>
    ///   <para>
    ///     Disabling virtualization can be implemented by unconditionally returning <see langword="true" /> from a style's
    ///     visibility test. This causes the <see cref="CanvasControl" /> to always assume that the item is visible in the
    ///     current viewport and thus never remove it from the visual tree.
    ///   </para>
    /// </remarks>
    /// <seealso cref="VirtualizationNodeStyleDecorator" />
    /// <seealso cref="VirtualizationEdgeStyleDecorator" />
    /// <seealso cref="VirtualizationLabelStyleDecorator" />
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public bool VirtualizationDisabled {
      get { return virtualizationDisabled; }
      set {
        virtualizationDisabled = value;
        OnPropertyChanged("VirtualizationDisabled");
      }
    }

    /// <summary>
    ///   Gets or sets the zoom level below which virtualization is disabled.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="VirtualizationDisabled" /> is <see langword="true" />.</para>
    /// </remarks>
    /// <seealso cref="VirtualizationNodeStyleDecorator.Threshold" />
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double NodeVirtualizationThreshold {
      get { return nodeVirtualizationThreshold; }
      set {
        nodeVirtualizationThreshold = value;
        OnPropertyChanged("NodeVirtualizationThreshold");
      }
    }

    /// <summary>
    ///   Gets or sets the zoom level below which virtualization is disabled.
    /// </summary>
    /// <remarks>
    ///   <para>This setting has no effect unless <see cref="VirtualizationDisabled" /> is <see langword="true" />.</para>
    /// </remarks>
    /// <seealso cref="VirtualizationEdgeStyleDecorator.Threshold" />
    /// <seealso cref="LargeGraphsWindow.UpdateStyles"/>
    public double EdgeVirtualizationThreshold {
      get { return edgeVirtualizationThreshold; }
      set {
        edgeVirtualizationThreshold = value;
        OnPropertyChanged("EdgeVirtualizationThreshold");
      }
    }

    #endregion

    #region Selection optimizations

    /// <summary>
    ///   Gets or sets a value indicating whether selection handles should be hidden.
    /// </summary>
    /// <remarks>
    ///   Selection handles come in a set of eight per selected node (and two per selected edge). Since performance is very
    ///   dependent on the number of items in the visual tree that can have a large impact if many items are selected.
    ///   Disabling selection handles in large graphs is often a fairly safe choice, especially since showing the handles on
    ///   thousands of selected nodes doesn't gain the user much (apart from confusion ... and wait time).
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateSelectionHandlesSetting"/>
    public bool SelectionHandlesDisabled {
      get { return selectionHandlesDisabled; }
      set {
        selectionHandlesDisabled = value;
        OnPropertyChanged("SelectionHandlesDisabled");
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to use custom selection decorators.
    /// </summary>
    /// <remarks>
    ///   The default selection decorators are somewhat slow to draw. This doesn't matter with selections of a few elements,
    ///   but when you select thousands of items there is a noticeable impact. This setting enables the use of custom-written
    ///   selection decorators (implemented as node, edge and label styles) instead of the default ones.
    /// </remarks>
    /// <seealso cref="FastNodeSelectionStyle" />
    /// <seealso cref="FastEdgeSelectionStyle" />
    /// <seealso cref="FastLabelSelectionStyle" />
    /// <seealso cref="LargeGraphsWindow.SetSelectionDecorators" />
    public bool CustomSelectionDecoratorEnabled {
      get { return customSelectionDecoratorEnabled; }
      set {
        customSelectionDecoratorEnabled = value;
        OnPropertyChanged("CustomSelectionDecoratorEnabled");
      }
    }

    #endregion

    #region Static graph optimizations

    /// <summary>
    ///   Gets or sets a value indicating whether label model baking should be enabled.
    /// </summary>
    /// <remarks>
    ///   Positioning labels can be expensive, since they are usually anchored to their owner. Thus, to determine a label's
    ///   position (which is needed for hit-testing and visibility checks) the owner's position needs to be known, too. If the
    ///   graph is known to be static (or changes to the graph are tightly controlled) we can simply replace all label's models
    ///   with instances of <see cref="FreeLabelModel" /> which records an absolute position and thus is much cheaper to
    ///   calculate.
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateLabelModelBakingSetting"/>
    public bool LabelModelBakingEnabled {
      get { return labelModelBakingEnabled; }
      set {
        labelModelBakingEnabled = value;
        OnPropertyChanged("LabelModelBakingEnabled");
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether a custom descriptor with optimized dirty handling should be used.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     If the graph is known to be static (or changes to the graph are tightly controlled) this represents a
    ///     last-resort optimization at a low level. Every object in a <see cref="CanvasControl" /> is placed in a tree of
    ///     <see cref="ICanvasObject" />s, each of which has an <see cref="ICanvasObjectDescriptor" /> that handles hit-testing,
    ///     visibility checks, creating/updating visuals, etc. For the most part that descriptor looks very similar to a style,
    ///     and by default in fact delegates to an item's style. The default descriptor also always returns
    ///     <see langword="true" /> for its <see cref="ICanvasObjectDescriptor.IsDirty" /> implementation, which means that the
    ///     object is always queried for visibility and updates.
    ///   </para>
    ///   <para>
    ///     By writing a custom descriptor that mostly returns false for <c>IsDirty</c> we can side-step almost all
    ///     visibility checks and calls to <c>UpdateVisual</c>. This greatly increases rendering speed as almost nothing needs
    ///     to be done anymore. The caveat is that we have to manually intervene whenever we want the visuals to change. This
    ///     includes the following situations:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>The level-of-detail style changes from one level to another.</item>
    ///     <item>
    ///       An item (node, edge, label) has changed (moved, resized), e.g. when the user (or code) makes edits to the
    ///       graph.
    ///     </item>
    ///   </list>
    ///   <para>
    ///     Getting all situations right where something changes can be a lot of work. In this demo only the first case is
    ///     handled for simplicity reasons. This optimization should not be performed if the other options already provide
    ///     adequate performance.
    ///   </para>
    /// </remarks>
    /// <seealso cref="LargeGraphsWindow.UpdateDirtyHandlingOptimizationSetting"/>
    public bool DirtyHandlingOptimizationEnabled {
      get { return dirtyHandlingOptimizationEnabled; }
      set {
        dirtyHandlingOptimizationEnabled = value;
        OnPropertyChanged("DirtyHandlingOptimizationEnabled");
      }
    }

    #endregion

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Returns a copy of the given <see cref="PerformanceSettings" /> instance.
    /// </summary>
    /// <param name="p">The <see cref="PerformanceSettings" /> to copy.</param>
    /// <returns>The newly-created copy of <paramref name="p" />.</returns>
    public static PerformanceSettings GetCopy(PerformanceSettings p) {
      return (PerformanceSettings) p.MemberwiseClone();
    }

    /// <summary>
    ///   Raises the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected virtual void OnPropertyChanged(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }
}