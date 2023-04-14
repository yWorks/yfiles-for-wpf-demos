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
using System.Linq;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Grouping;
using yWorks.Layout.Organic;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("CompactDiskLayout")]
  public class CompactDiskLayoutConfig : LayoutConfiguration
  {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public CompactDiskLayoutConfig() {
      var layout = new CompactDiskLayout();

      UseDrawingAsSketchItem = layout.FromSketchMode;
      MinimumNodeDistanceItem = layout.MinimumNodeDistance;
      NodeLabelingStyleItem = EnumNodeLabelingPolicies.None;
      LayoutGroupsItem = GroupLayout.None;
    }

    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      if (LayoutGroupsItem == GroupLayout.Recursive &&
          graphControl.Graph.Nodes.Any(n => graphControl.Graph.IsGroupNode(n))) {
        // if the recursive group layout option is enabled, use RecursiveGroupLayout with organic for
        // the top-level hierarchy - the actual compact disk layout will be specified as layout for
        // each group content in function createConfiguredLayoutData
        return new RecursiveGroupLayout {
            CoreLayout = new OrganicLayout {
                Deterministic = true, NodeOverlapsAllowed = false, MinimumNodeDistance = MinimumNodeDistanceItem
            },
            FromSketchMode = UseDrawingAsSketchItem
        };
      }

      // just use plain CompactDiskLayout
      return this.CreateCompactDiskLayout(graphControl);
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      if (LayoutGroupsItem == GroupLayout.Recursive) {
        var compactDiskLayout = this.CreateCompactDiskLayout(graphControl);
        return new RecursiveGroupLayoutData {
            GroupNodeLayouts = new ItemMapping<INode, ILayoutAlgorithm>(compactDiskLayout)
        };
      }
      return null;
    }

    /**
     * Creates and configures the actual compact disk layout algorithm.
     * @return The configured compact disk layout.
     */
    private CompactDiskLayout CreateCompactDiskLayout(GraphControl graphControl) {
      var layout = new CompactDiskLayout();

      layout.FromSketchMode = UseDrawingAsSketchItem;

      layout.MinimumNodeDistance = MinimumNodeDistanceItem;

      switch (NodeLabelingStyleItem) {
        case EnumNodeLabelingPolicies.None:
          layout.ConsiderNodeLabels = false;
          break;
        case EnumNodeLabelingPolicies.RaylikeLeaves:
          layout.IntegratedNodeLabeling = true;
          layout.NodeLabelingPolicy = NodeLabelingPolicy.RayLikeLeaves;
          break;
        case EnumNodeLabelingPolicies.ConsiderCurrentPosition:
          layout.ConsiderNodeLabels = true;
          break;
        case EnumNodeLabelingPolicies.Horizontal:
          layout.IntegratedNodeLabeling = true;
          layout.NodeLabelingPolicy = NodeLabelingPolicy.Horizontal;
          break;
        default:
          layout.ConsiderNodeLabels = false;
          break;
      }

      if (NodeLabelingStyleItem == EnumNodeLabelingPolicies.RaylikeLeaves ||
          NodeLabelingStyleItem == EnumNodeLabelingPolicies.Horizontal) {
        foreach (var label in graphControl.Graph.Labels) {
          if (label.Owner is INode) {
            graphControl.Graph.SetLabelLayoutParameter(
                label,
                FreeNodeLabelModel.Instance.FindBestParameter(
                    label,
                    FreeNodeLabelModel.Instance,
                    label.GetLayout()
                )
            );
          }
        }
      }

      return layout;
    }


    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GeneralGroup;

    [Label("Labeling")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LabelingGroup;


    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText {
      get {
        return "<Paragraph>The nodes are arranged on a disk such that the disk's radius is minimized.</Paragraph>"
               + "<Paragraph>The layout mostly optimizes the dense placement of the nodes, " +
               "while edges play a minor role. Hence, the compact disk layout is mostly suitable for graphs with " +
               "small components whose loosely connected nodes should be grouped and packed in a small area.</Paragraph>";
      }
    }


    [Label("Use Drawing as Sketch")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(false)]
    public bool UseDrawingAsSketchItem { get; set; }

    [Label("Minimum Node Distance")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(0)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumNodeDistanceItem { get; set; }

    [Label("Layout Groups")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(GroupLayout.None)]
    [EnumValue("Ignore Groups", GroupLayout.None)]
    [EnumValue("Layout Recursively", GroupLayout.Recursive)]
    public GroupLayout LayoutGroupsItem { get; set; }


    [Label("Node Labeling")]
    [OptionGroup("LabelingGroup", 10)]
    [DefaultValue(EnumNodeLabelingPolicies.None)]
    [EnumValue("Ignore Labels", EnumNodeLabelingPolicies.None)]
    [EnumValue("Consider Labels", EnumNodeLabelingPolicies.ConsiderCurrentPosition)]
    [EnumValue("Horizontal", EnumNodeLabelingPolicies.Horizontal)]
    [EnumValue("Ray-like at Leaves", EnumNodeLabelingPolicies.RaylikeLeaves)]
    public EnumNodeLabelingPolicies NodeLabelingStyleItem { get; set; }

    public enum GroupLayout
    {
      None,
      Recursive
    }
  }
}
