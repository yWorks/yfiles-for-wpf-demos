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
using System.Collections.Generic;
using System.Linq;
using yWorks.Geometry;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Grid;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   This stage can be used for automatically assigning
  ///   a <see cref="LayoutGrid"/> instance that treats top-level group nodes as vertical swimlanes/columns.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This stage will modify the hierarchy by temporarily removing the top-level group nodes and assigning
  ///     corresponding grid cells to the remaining nodes. After running the core layout, the top-level group nodes will
  ///     be arranged to function as swimlanes. This class allows using the current coordinates of the group nodes to
  ///     <see cref="OrderSwimlanesFromSketch">arrange the swimlanes from sketch</see>.
  ///     Also, the <see cref="Spacing"/> between swimlanes may be specified.
  ///   </para>
  ///   <para>
  ///     This stage can be used in conjunction with layouts that support <see cref="LayoutGrid"/>,
  ///     like, for example <see cref="HierarchicalLayout"/>.
  ///   </para>
  /// </remarks>
  /// <seealso cref="LayoutGrid"/>
  /// <seealso cref="HierarchicalLayout"/>
  public class TopLevelGroupToSwimlaneStage : LayoutStageBase
  {
    private double spacing;

    /// <summary>
    ///   Creates a new instance of <see cref="TopLevelGroupToSwimlaneStage"/> with the default settings.
    /// </summary>
    public TopLevelGroupToSwimlaneStage() { }

    /// <summary>Gets or sets the spacing between the columns/swimlanes.</summary>
    /// <remarks>The spacing should be greater than <c>0</c>.</remarks>
    /// <exception cref="ArgumentException">if the spacing is negative</exception>
    /// <value>a non-negative spacing value</value>
    public double Spacing {
      get => spacing;
      set {
        if (value < 0) {
          throw new ArgumentException("The spacing should be non-negative: " + value);
        }

        spacing = value;
      }
    }


    /// <summary>
    ///   Gets or sets whether the order of the columns/swimlanes is determined based on the current sketch.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if the order of the swimlanes is determined from
    ///   the current sketch (current item coordinates), <see langword="false"/> otherwise
    /// </value>
    public bool OrderSwimlanesFromSketch { get; set; }


    /// <summary>
    ///   Executes this stage, replacing top-level group by layout grid columns, then executing the core layout.
    /// </summary>
    /// <param name="graph">The input graph</param>
    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      // check if there is anything to do at all.
      if (graph.IsEmpty || !LayoutGraphGrouping.IsGrouped(graph)) {
        // we cannot do anything, simply proceed
        CoreLayout.ApplyLayout(graph);
        return;
      }

      var grouping = LayoutGraphGrouping.CreateReadOnlyView(graph);

      // count all descriptors
      var counter = 0;

      // use available padding and minimum group node sizes
      var paddingProvider = graph.Context.GetItemData(LayoutKeys.GroupNodePaddingDataKey);
      var sizeProvider = graph.Context.GetItemData(LayoutKeys.MinimumGroupNodeSizeDataKey);

      // now find all swimlane nodes by iterating all top-level group nodes
      var topLevelColumns = new YList<TopLevelColumn>();
      var root2MyDescriptor = new Dictionary<Node, TopLevelColumn>();
      foreach (var topLevelGroup in graph.Nodes.Where(n => grouping.IsGroupNode(n) && !grouping.HasParent(n))) {
        // construct column info for this top level group
        var column = new TopLevelColumn(topLevelGroup, counter++);
        topLevelColumns.Add(column);

        column.rightInset = 0;
        column.leftInset = 0;

        var groupPadding = paddingProvider?[topLevelGroup];
        if (groupPadding != null) {
          column.leftInset = groupPadding.Value.Left;
          // spacing is always to the right
          column.rightInset = groupPadding.Value.Right;
        }

        var minimumGroupSize = sizeProvider?[topLevelGroup];
        if (minimumGroupSize != null) {
          column.minimumWidth = minimumGroupSize.Value.Width;
        }

        // remember original spacing - we need it to model empty lanes
        column.originalLeftInset = column.leftInset;
        column.originalRightInset = column.rightInset;

        // add the spacing
        column.rightInset += spacing;

        // mark it as fixed
        column.indexFixed = OrderSwimlanesFromSketch;
        // remember the descriptor
        root2MyDescriptor[topLevelGroup] = column;
      }

      // create a hider that will hide the top-level groups (swimlanes) - edges are not being routed currently
      var hider = new LayoutGraphHider(graph);

      // create a comparator for sorting the swimlanes
      var columnComparer = TopLevelColumn.Comparison;

      // hide roots
      foreach (var topLevelColumn in topLevelColumns) {
        hider.Hide(topLevelColumn.group);
      }

      // at least reset stupid edges between swimlane nodes ....
      foreach (var edge in hider.HiddenEdges) {
        edge.ResetPath();
      }

      // check if this is a non-trivial task, i.e. the graph is not empty for the core layout after hiding top-level groups
      if (!graph.IsEmpty) {
        // create a column for those nodes that don't belong to any group, which has not a fixed index
        var nonGroupedColumn = new TopLevelColumn {
            indexFixed = false,
            leftInset = 0,
            rightInset = spacing
        };

        // remember which roots have been used at all, i.e. have nodes assigned to them
        var usedRootGroups = new HashSet<Node>();
        var allRootGroups = topLevelColumns.Select(column => column.group).ToHashSet();

        // for all nodes, assign them to a top-level column
        var topLevelColumnMapping = graph.CreateNodeDataMap<TopLevelColumn>();
        foreach (var node in graph.Nodes) {
          // find the root group
          var parent = grouping.GetParent(node);
          while (true) {
            if (parent == null) {
              // root node, not contained in any group
              topLevelColumnMapping[node] = nonGroupedColumn;
              break;
            }

            // walk up and find ancestor
            if (allRootGroups.Contains(parent)) {
              // we found the root group, use its descriptor
              topLevelColumnMapping[node] = root2MyDescriptor[parent];
              usedRootGroups.Add(parent);
              break;
            }

            // continue searching our way upwards
            parent = grouping.GetParent(parent);
          }
        }

        // see if there are empty lanes that need to have some space reserved
        if (usedRootGroups.Count != allRootGroups.Count && OrderSwimlanesFromSketch) {
          // sort them the way the code will sort them
          topLevelColumns.Sort(columnComparer);
          // remember the space to reserve on the left side
          double accuX = 0;
          // remember the last non-empty column to add the space to its right side
          TopLevelColumn lastReal = null;
          foreach (var column in topLevelColumns) {
            // if this is empty
            if (!usedRootGroups.Contains(column.group)) {
              // get its width, we will keep that width as is
              var width = column.group.Layout.Width;
              // see if we remember it for later
              if (lastReal == null) {
                accuX += width + spacing;
              } else {
                // or whether we can add it to the last real one...
                lastReal.rightInset = lastReal.rightInset + width + spacing;
              }
            } else {
              // this one is non-empty: add the necessary space to the left
              column.leftInset += accuX;
              accuX = 0;
              lastReal = column;
            }
          }
        }

        // sort descriptors and then add it in that order
        topLevelColumns.Sort(columnComparer);

        // specify layout grid and mapping
        var newGrid = new LayoutGrid { OptimizeColumnOrder = !OrderSwimlanesFromSketch };
        var layoutGridRow = newGrid.AddRow();
        topLevelColumns.AddFirst(nonGroupedColumn);
        foreach (var topLevelColumn in topLevelColumns) {
          var layoutGridColumn = newGrid.AddColumn();
          topLevelColumn.actualGridColumnDescriptor = layoutGridColumn;
          layoutGridColumn.LeftPadding = topLevelColumn.leftInset;
          layoutGridColumn.RightPadding = topLevelColumn.rightInset;
          layoutGridColumn.MinimumWidth = topLevelColumn.minimumWidth;
          layoutGridColumn.IndexFixed = topLevelColumn.indexFixed;
        }
        // remove nonGroupedColumn - it was just temporarily in the list to add an actual column in the above loop
        topLevelColumns.Pop();

        var newGridCellMapper = new DictionaryMapper<Node, LayoutGrid.CellDescriptor>();
        foreach (var node in graph.Nodes) {
          newGridCellMapper[node] =
              newGrid.CreateCellDescriptor(layoutGridRow, topLevelColumnMapping[node].actualGridColumnDescriptor);
        }
        graph.DisposeDataMap(topLevelColumnMapping);

        // Run the core layout
        // register new grid on a new graph context layer to avoid overwriting old, existing stuff
        graph.Context.PushLayer();
        graph.Context.AddItemData(LayoutGrid.LayoutGridCellDescriptorDataKey, newGridCellMapper);
        CoreLayout.ApplyLayout(graph);
        graph.Context.PopLayer();

        // calculate the bounds
        var box = graph.GetBounds();

        // unhide
        hider.UnhideAll();

        var minY = box.Y;
        var maxY = box.Y + box.Height;
        if (paddingProvider != null) {
          foreach (var column in topLevelColumns) {
            var padding = paddingProvider[column.group];
            if (padding != null) {
              minY = Math.Min(box.Y - padding.Value.Top, minY);
              maxY = Math.Max(box.Y + box.Height + padding.Value.Bottom, maxY);
            }
          }
        } else {
          minY -= 10;
          maxY += 10;
        }

        var height = maxY - minY;
        if (sizeProvider != null) {
          foreach (var column in topLevelColumns) {
            var size = sizeProvider[column.group];
            if (size != null) {
              var h = size.Value.Height;
              if (height < h) {
                height = h;
              }
            }
          }
        }

        // now place the top level group nodes that were hidden during the core layout
        var foundFirstReal = false;
        var lastMaxX = double.NaN;
        var maxX = -double.MaxValue;

        if (usedRootGroups.Count == 0) {
          // if only empty descriptors, except for the non-grouped column
          foundFirstReal = true;
          lastMaxX = maxX = nonGroupedColumn.actualGridColumnDescriptor.Position +
              nonGroupedColumn.actualGridColumnDescriptor.Width;
        }

        foreach (var column in topLevelColumns) {
          var group = column.group;
          var columnDescriptor = column.actualGridColumnDescriptor;
          // see if this one has been computed at all, i.e. it was non-empty
          if (columnDescriptor.Index >= 0) {
            // assign the width to the computed one, but remember to subtract the extra padding for the dummy lanes
            double dRight = 0, dLeft = 0;

            if (column != nonGroupedColumn) {
              dRight = columnDescriptor.LeftPadding - column.originalLeftInset;
              dLeft = columnDescriptor.RightPadding - column.originalRightInset;
            }

            group.Layout.Width = columnDescriptor.Width - (dRight + dLeft);
            group.Layout.Height = height;
            group.Layout.X = columnDescriptor.Position + dRight;
            group.Layout.Y = minY;
            // remember last one and the maximum x
            lastMaxX = group.Layout.X + group.Layout.Width;
            maxX = Math.Max(lastMaxX, maxX);
            if (!foundFirstReal && OrderSwimlanesFromSketch) {
              // go back in time and adjust previous empty lanes correspondingly
              var rootCell = topLevelColumns.FirstCell;
              for (; rootCell != null && rootCell.Info != column; rootCell = rootCell.Next) { }

              rootCell = rootCell.Previous;
              var x = group.Layout.X;
              for (; rootCell != null; rootCell = rootCell.Previous) {
                var pNode = rootCell.Info.group;
                pNode.Layout.Height = height;
                pNode.Layout.X = x - pNode.Layout.Width - spacing;
                pNode.Layout.Y = minY;
                x -= pNode.Layout.Width + spacing;
              }
            }

            foundFirstReal = true;
          } else {
            // append empty swimlane
            if (foundFirstReal && OrderSwimlanesFromSketch) {
              group.Layout.Size = new SizeD(group.Layout.Width, height);
              group.Layout.TopLeft = new PointD(lastMaxX + spacing, minY);
              lastMaxX = group.Layout.X + group.Layout.Width;
            }
          }
        }

        // if this was not from sketch append all empty lanes at the back...
        if (!OrderSwimlanesFromSketch) {
          foreach (var column in topLevelColumns) {
            if (column.actualGridColumnDescriptor.Index < 0) {
              var group = column.group;
              group.Layout.Height = height;
              group.Layout.X = maxX + spacing;
              group.Layout.Y = minY;
              maxX = group.Layout.X + group.Layout.Width;
            }
          }
        }
      } else {
        // case that graph is empty after hiding top-level groups, do not actually call core layout
        hider.UnhideAll();
        if (OrderSwimlanesFromSketch) {
          topLevelColumns.Sort(columnComparer);
        }

        var maximumHeight = topLevelColumns.Select(col => col.group.Layout.Height).Max();
        var x = 0.0;
        // make them all the same height... and put them next to each other
        foreach (var column in topLevelColumns) {
          column.group.Layout.Height = maximumHeight;
          column.group.Layout.TopLeft = new PointD(x, 0);
          x += column.group.Layout.Width + spacing;
        }
      }
    }

    private sealed class TopLevelColumn
    {
      internal readonly Node group;
      private readonly int index;

      internal LayoutGrid.Column actualGridColumnDescriptor;
      internal double originalLeftInset;
      internal double originalRightInset;
      internal double rightInset;
      internal double leftInset;
      internal double minimumWidth;
      internal bool indexFixed;

      internal static readonly Comparison<TopLevelColumn> Comparison = (c1, c2) => {
        var x1 = c1.group.Layout.CenterX;
        var x2 = c2.group.Layout.CenterX;
        var dx = x1.CompareTo(x2);
        return dx == 0 ? c1.index.CompareTo(c2.index) : dx;
      };

      public TopLevelColumn() : this(null, -1) { }

      public TopLevelColumn(Node topLevelGroup, int index) {
        group = topLevelGroup;
        this.index = index;
      }
    }
  }
}
