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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Router;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for 
  /// <see cref="CompactOrthogonalLayout"/>.
  /// </summary>
  public class CompactOrthogonalLayoutModule : LayoutModule
  {
    #region configuration constants

    private const string NAME = "COMPACT_ORTHOGONAL";
    private const string TOP_LEVEL = "TOP_LEVEL";
    private const string LAYOUT_OPTIONS = "LAYOUT_OPTIONS";
    private const string ORTHOGONAL_LAYOUT_STYLE = "ORTHOGONAL_LAYOUT_STYLE";
    private const string GRID = "GRID";

    private const string NORMAL = "NORMAL";
    private const string NORMAL_TREE = "NORMAL_TREE";
    private const string FIXED_MIXED = "FIXED_MIXED";
    private const string FIXED_BOX_NODES = "FIXED_BOX_NODES";

    private const string ASPECT_RATIO = "ASPECT_RATIO";
//    private const string USE_VIEW_ASPECT_RATIO = "USE_VIEW_ASPECT_RATIO";

    private const string PLACEMENT_STRATEGY = "PLACEMENT_STRATEGY";
    private const string STYLE_ROWS = "STYLE_ROWS";
    private const string STYLE_PACKED_COMPACT_RECTANGLE = "STYLE_PACKED_COMPACT_RECTANGLE";

    // ChannelInterEdgeRouter stuff
    private const string PATH_FINDER = "PATH_FINDER";
    private const string ORTHOGONAL_PATTERN_PATH_FINDER = "ORTHOGONAL_PATTERN_PATH_FINDER";
    private const string ORTHOGONAL_SHORTESTPATH_PATH_FINDER = "ORTHOGONAL_SHORTESTPATH_PATH_FINDER";
    private const string ROUTE_ALL_EDGES = "ROUTE_ALL_EDGES";

    // ChannelEdgeRouter stuff
    private const string MINIMUM_DISTANCE = "MINIMUM_DISTANCE";
    private const string CENTER_TO_SPACE_RATIO = "SPACE_DRIVEN_VS_CENTER_DRIVEN_SEARCH";
    private const string EDGE_CROSSING_COST = "CROSSING_COST";
    private const string NODE_CROSSING_COST = "NODE_CROSSING_COST";
    private const string BEND_COST = "BEND_COST";

    private static readonly Dictionary<string, LayoutStyle> styles = new Dictionary<string, LayoutStyle>(4);
    private static readonly Dictionary<LayoutStyle, string> str2styles = new Dictionary<LayoutStyle, string>(4);
    private static readonly Dictionary<string, ComponentArrangementStyles> componentStyles = new Dictionary<string, ComponentArrangementStyles>(2);
    private static readonly Dictionary<ComponentArrangementStyles, string> str2componentStyles = new Dictionary<ComponentArrangementStyles, string>(2);
    private readonly ICollection<object> PATH_FINDER_ENUM  = new Collection<object> {
      ORTHOGONAL_PATTERN_PATH_FINDER,
      ORTHOGONAL_SHORTESTPATH_PATH_FINDER
    };

    static CompactOrthogonalLayoutModule() {
      styles.Add(NORMAL, LayoutStyle.Normal);
      styles.Add(FIXED_BOX_NODES, LayoutStyle.FixedBox);
      styles.Add(FIXED_MIXED, LayoutStyle.FixedMixed);

      str2styles.Add(LayoutStyle.Normal, NORMAL);
      str2styles.Add(LayoutStyle.FixedBox, FIXED_BOX_NODES);
      str2styles.Add(LayoutStyle.FixedMixed, FIXED_MIXED);

      componentStyles.Add(STYLE_ROWS, ComponentArrangementStyles.MultiRows);
      componentStyles.Add(STYLE_PACKED_COMPACT_RECTANGLE, ComponentArrangementStyles.PackedCompactRectangle);

      str2componentStyles.Add(ComponentArrangementStyles.MultiRows, STYLE_ROWS);
      str2componentStyles.Add(ComponentArrangementStyles.PackedCompactRectangle, STYLE_PACKED_COMPACT_RECTANGLE);
    }

    #endregion

    /// <summary>
    /// Create a new instance
    /// </summary>
    public CompactOrthogonalLayoutModule()
      : base(NAME) {}

    /// <summary>
    /// sets up the option handler for specifying the layout parameters.
    /// </summary>
    protected override void SetupHandler() {
      // use an instance of the layout as a defaults provider
      CompactOrthogonalLayout layout = new CompactOrthogonalLayout();
      var topLevelGroup = Handler.AddGroup(TOP_LEVEL);

      topLevelGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      topLevelGroup.Attributes[DefaultEditorFactory.RenderingHintsAttribute] = DefaultEditorFactory.RenderingHints.Invisible;

      OrthogonalLayout cl = (OrthogonalLayout) layout.CoreLayout;

      topLevelGroup.AddList(ORTHOGONAL_LAYOUT_STYLE, new []{NORMAL,NORMAL_TREE,FIXED_BOX_NODES,FIXED_MIXED}, str2styles[cl.LayoutStyle]);

      topLevelGroup.AddList(PLACEMENT_STRATEGY, new[] { STYLE_ROWS, STYLE_PACKED_COMPACT_RECTANGLE }, str2componentStyles[ComponentArrangementStyles.PackedCompactRectangle]);

      topLevelGroup.AddDouble(ASPECT_RATIO, layout.AspectRatio);
      topLevelGroup.AddInt(GRID, layout.GridSpacing, 1, int.MaxValue);

      OrthogonalPatternEdgeRouter oper = new OrthogonalPatternEdgeRouter();
      
      IOptionItem bendCostOptionItem = topLevelGroup.AddDouble(BEND_COST, oper.BendCost);
      IOptionItem nodeCrossingCostOptionItem = topLevelGroup.AddDouble(NODE_CROSSING_COST, oper.NodeCrossingCost);
      IOptionItem edgeCrossingCostOptionItem = topLevelGroup.AddDouble(EDGE_CROSSING_COST, oper.EdgeCrossingCost);
      IOptionItem minimumDistanceOptionItem = topLevelGroup.AddDouble(MINIMUM_DISTANCE, oper.MinimumDistance);
      
      IOptionItem pathFinderOptionItem = topLevelGroup.AddList(PATH_FINDER, PATH_FINDER_ENUM, ORTHOGONAL_SHORTESTPATH_PATH_FINDER);

      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(pathFinderOptionItem, ORTHOGONAL_PATTERN_PATH_FINDER, bendCostOptionItem);
      cm.SetEnabledOnValueEquals(pathFinderOptionItem, ORTHOGONAL_PATTERN_PATH_FINDER, nodeCrossingCostOptionItem);
      cm.SetEnabledOnValueEquals(pathFinderOptionItem, ORTHOGONAL_PATTERN_PATH_FINDER, edgeCrossingCostOptionItem);
      cm.SetEnabledOnValueEquals(pathFinderOptionItem, ORTHOGONAL_PATTERN_PATH_FINDER, minimumDistanceOptionItem);
      
      topLevelGroup.AddBool(ROUTE_ALL_EDGES, !layout.InterEdgeRouter.RouteInterEdgesOnly);
    }

    private PartitionLayout.IInterEdgeRouter ConfigureInterEdgeRouter() {
      OptionGroup topLevelGroup = Handler.GetGroupByName(TOP_LEVEL);
      PartitionLayout.IInterEdgeRouter interEdgeRouter;
      if (topLevelGroup[PATH_FINDER].Value.Equals(ORTHOGONAL_PATTERN_PATH_FINDER)) {
        OrthogonalPatternEdgeRouter oper = new OrthogonalPatternEdgeRouter();
        oper.MinimumDistance = (double) topLevelGroup[MINIMUM_DISTANCE].Value;
        oper.EdgeCrossingCost = (double) topLevelGroup[EDGE_CROSSING_COST].Value;
        oper.NodeCrossingCost = (double) topLevelGroup[NODE_CROSSING_COST].Value;
        oper.BendCost = (double) topLevelGroup[BEND_COST].Value;
        var channelEdgeRouter = new ChannelEdgeRouter();
        channelEdgeRouter.PathFinderStrategy = oper;
        interEdgeRouter = PartitionLayout.CreateChannelInterEdgeRouter(channelEdgeRouter);
      } else {
        interEdgeRouter = PartitionLayout.CreateChannelInterEdgeRouter();
      }
      interEdgeRouter.RouteInterEdgesOnly = !(bool)(topLevelGroup.GetItemByName(ROUTE_ALL_EDGES).Value);
      return interEdgeRouter;
    }

    private PartitionLayout.IPartitionPlacer ConfigurePartitionPlacer() {
      OptionGroup topLevelGroup = Handler.GetGroupByName(TOP_LEVEL);
      IOptionItem placementItem = topLevelGroup.GetItemByName(PLACEMENT_STRATEGY);
      return
          PartitionLayout.CreateComponentPartitionPlacer(new ComponentLayout
            {
              Style = componentStyles[(string) placementItem.Value]
            });
    }

    private void ApplyOptions(OrthogonalLayout layout) {
      OptionGroup topLevelGroup = Handler.GetGroupByName(TOP_LEVEL);
      IOptionItem styleItem = topLevelGroup.GetItemByName(ORTHOGONAL_LAYOUT_STYLE);
      layout.LayoutStyle = styles[(string) styleItem.Value];
    }

    private void ApplyOptions(CompactOrthogonalLayout layout) {
      OptionGroup topLevelGroup = Handler.GetGroupByName(TOP_LEVEL);
      IOptionItem gridItem = topLevelGroup.GetItemByName(GRID);
      layout.GridSpacing = (int) gridItem.Value;

      double ar = (double)topLevelGroup.GetItemByName(ASPECT_RATIO).Value;

      // this needs to be done as a final step since it will reconfigure
      // layout stages which support aspect ratio accordingly
      layout.AspectRatio = ar;
    }

    /// <summary>
    /// configures the layout algorithm according to the settings of the option handler.
    /// </summary>
    protected override void ConfigureLayout() {
      CompactOrthogonalLayout compactOrthogonal = new CompactOrthogonalLayout();

      compactOrthogonal.InterEdgeRouter = ConfigureInterEdgeRouter();
      compactOrthogonal.PartitionPlacer = ConfigurePartitionPlacer();

      ApplyOptions((OrthogonalLayout)compactOrthogonal.CoreLayout);

      ApplyOptions(compactOrthogonal);
      LayoutAlgorithm = new HideGroupsStage(compactOrthogonal) { HidingEmptyGroupNodes = false};
    }
  }
}
