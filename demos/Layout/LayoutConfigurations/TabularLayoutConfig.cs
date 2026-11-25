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

using System.ComponentModel;
using System.Reflection;
using yWorks.Controls;
using yWorks.Demo.Toolkit.OptionHandler;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Grid;
using yWorks.Layout.Tabular;

namespace yWorks.Demo.Layout.Configurations
{
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("TabularLayout")]
  public class TabularLayoutConfig : LayoutConfiguration
  {
    public TabularLayoutConfig() {
      var layout = new TabularLayout();

      LayoutPolicyItem = EnumLayoutPolicies.AutoSize;
      RowCountItem = 8;
      ColumnCountItem = 12;
      HorizontalAlignmentItem = EnumHorizontalAlignments.Center;
      VerticalAlignmentItem = EnumVerticalAlignments.Center;
      NodeLabelingItem = layout.NodeLabelPlacement;
      MinimumRowHeightItem = 0;
      MinimumColumnWidthItem = 0;
      CellPaddingItem = 5;
    }

    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new TabularLayout();

      switch (LayoutPolicyItem) {
        case EnumLayoutPolicies.AutoSize:
          layout.LayoutMode = TabularLayoutMode.AutoSize;
          break;
        case EnumLayoutPolicies.FixedTableSize:
        case EnumLayoutPolicies.SingleRow:
        case EnumLayoutPolicies.SingleColumn:
          layout.LayoutMode = TabularLayoutMode.FixedSize;
          break;
        case EnumLayoutPolicies.FromSketch:
          layout.LayoutMode = TabularLayoutMode.FromSketch;
          break;
      }

      layout.NodeLabelPlacement = NodeLabelingItem;

      return layout;
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var layoutData = new TabularLayoutData<INode, IEdge, ILabel, ILabel>();
      var nodeLayoutDescriptor = new TabularLayoutNodeDescriptor();
      switch (HorizontalAlignmentItem) {
        case EnumHorizontalAlignments.Center:
          nodeLayoutDescriptor.HorizontalAlignment = 0.5;
          break;
        case EnumHorizontalAlignments.Left:
          nodeLayoutDescriptor.HorizontalAlignment = 0;
          break;
        case EnumHorizontalAlignments.Right:
          nodeLayoutDescriptor.HorizontalAlignment = 1;
          break;
      }
      switch (VerticalAlignmentItem) {
        case EnumVerticalAlignments.Center:
          nodeLayoutDescriptor.VerticalAlignment = 0.5;
          break;
        case EnumVerticalAlignments.Top:
          nodeLayoutDescriptor.VerticalAlignment = 0;
          break;
        case EnumVerticalAlignments.Bottom:
          nodeLayoutDescriptor.VerticalAlignment = 1;
          break;
      }

      var nodeCount = graphControl.Graph.Nodes.Count;
      LayoutGrid layoutGrid;
      switch (LayoutPolicyItem) {
        case EnumLayoutPolicies.FixedTableSize:
          var rowCount = RowCountItem;
          var columnCount = ColumnCountItem;
          if (rowCount * columnCount >= nodeCount) {
            layoutGrid = new LayoutGrid(rowCount, columnCount);
          } else {
            // make sure layoutGrid has enough cells for all nodes
            layoutGrid = new LayoutGrid(nodeCount / columnCount, columnCount);
          }
          break;
        case EnumLayoutPolicies.SingleRow:
          layoutGrid = new LayoutGrid(1, nodeCount);
          break;
        case EnumLayoutPolicies.SingleColumn:
          layoutGrid = new LayoutGrid(nodeCount, 1);
          break;
        default:
          layoutGrid = new LayoutGrid(1, 1);
          break;
      }

      var minimumRowHeight = MinimumRowHeightItem;
      var minimumColumnWidth = MinimumColumnWidthItem;
      var cellPadding = CellPaddingItem;
      foreach (var row in layoutGrid.Rows) {
        row.MinimumHeight = minimumRowHeight;
        row.TopPadding = cellPadding;
        row.BottomPadding = cellPadding;
      }
      foreach (var column in layoutGrid.Columns) {
        column.MinimumWidth = minimumColumnWidth;
        column.LeftPadding = cellPadding;
        column.RightPadding = cellPadding;
      }

      layoutData.NodeDescriptors.Constant = nodeLayoutDescriptor;
      layoutData.LayoutGridData.Grid = layoutGrid;
      layoutData.LayoutGridData.LayoutGridCellDescriptors.MapperFunction =
          _ => layoutGrid.CreateDynamicCellDescriptor();

      return layoutData;
    }

    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GeneralGroup;

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>The tabular layout style arranges the nodes in rows and columns. This is a" +
        " very simple layout which is useful when nodes should be placed under/next to each other.</Paragraph>" +
        "<Paragraph>Edges are ignored in this layout style. Their bends are removed.</Paragraph>";

    [Label("Layout Mode")]
    [OptionGroup("GeneralGroup", 10)]
    [EnumValue("Automatic Table Size", EnumLayoutPolicies.AutoSize)]
    [EnumValue("Single Row", EnumLayoutPolicies.SingleRow)]
    [EnumValue("Single Column", EnumLayoutPolicies.SingleColumn)]
    [EnumValue("Fixed Table Size", EnumLayoutPolicies.FixedTableSize)]
    [EnumValue("From Sketch", EnumLayoutPolicies.FromSketch)]
    public EnumLayoutPolicies LayoutPolicyItem { get; set; }

    [Label("Row Count")]
    [OptionGroup("GeneralGroup", 20)]
    [MinMax(Min = 1, Max = 200, Step = 1)]
    [ComponentType(ComponentTypes.Slider)]
    public int RowCountItem { get; set; }

    public bool ShouldDisableRowCountItem => LayoutPolicyItem != EnumLayoutPolicies.FixedTableSize;

    [Label("Column Count")]
    [OptionGroup("GeneralGroup", 30)]
    [MinMax(Min = 1, Max = 200, Step = 1)]
    [ComponentType(ComponentTypes.Slider)]
    public int ColumnCountItem { get; set; }

    public bool ShouldDisableColumnCountItem => LayoutPolicyItem != EnumLayoutPolicies.FixedTableSize;

    [Label("Node Labeling")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(NodeLabelPlacement.Consider)]
    [EnumValue("Consider", NodeLabelPlacement.Consider)]
    [EnumValue("Generic", NodeLabelPlacement.Generic)]
    [EnumValue("Ignore", NodeLabelPlacement.Ignore)]
    public NodeLabelPlacement NodeLabelingItem { get; set; }

    [Label("Horizontal Alignment")]
    [OptionGroup("GeneralGroup", 50)]
    [EnumValue("Left", EnumHorizontalAlignments.Left)]
    [EnumValue("Center", EnumHorizontalAlignments.Center)]
    [EnumValue("Right", EnumHorizontalAlignments.Right)]
    public EnumHorizontalAlignments HorizontalAlignmentItem { get; set; }

    [Label("Vertical Alignment")]
    [OptionGroup("GeneralGroup", 60)]
    [EnumValue("Top", EnumVerticalAlignments.Top)]
    [EnumValue("Center", EnumVerticalAlignments.Center)]
    [EnumValue("Bottom", EnumVerticalAlignments.Bottom)]
    public EnumVerticalAlignments VerticalAlignmentItem { get; set; }

    [Label("Cell Padding (all sides)")]
    [OptionGroup("GeneralGroup", 70)]
    [MinMax(Min = 0, Max = 50, Step = 1)]
    [ComponentType(ComponentTypes.Slider)]
    public double CellPaddingItem { get; set; }

    [Label("Minimum Row Height")]
    [OptionGroup("GeneralGroup", 80)]
    [MinMax(Min = 0, Max = 100, Step = 1)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumRowHeightItem { get; set; }

    [Label("Minimum Column Width")]
    [OptionGroup("GeneralGroup", 80)]
    [MinMax(Min = 0, Max = 100, Step = 1)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumColumnWidthItem { get; set; }

    public enum EnumLayoutPolicies
    {
      AutoSize,
      SingleRow,
      SingleColumn,
      FixedTableSize,
      FromSketch
    }

    public enum EnumHorizontalAlignments
    {
      Left,
      Center,
      Right
    }

    public enum EnumVerticalAlignments
    {
      Top,
      Center,
      Bottom
    }
  }
}
