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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Algorithms.Geometry;
using yWorks.Controls;
using yWorks.Layout;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("ComponentLayout")]
  public class ComponentLayoutConfig : LayoutConfiguration {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public ComponentLayoutConfig() {
      var layout = new ComponentLayout();

      StyleItem = ComponentArrangementStyles.Rows;
      NoOverlapItem = (layout.Style & ComponentArrangementStyles.ModifierNoOverlap) != 0;
      FromSketchItem = (layout.Style & ComponentArrangementStyles.ModifierAsIs) != 0;
      YDimension size = layout.PreferredSize;
      UseScreenRatioItem = true;
      AspectRatioItem = size.Width / size.Height;

      ComponentSpacingItem = layout.ComponentSpacing;
      GridEnabledItem = layout.GridSpacing > 0;
      GridSpacingItem = layout.GridSpacing > 0 ? layout.GridSpacing : 20.0d;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new ComponentLayout();
      layout.ComponentArrangement = true;
      var style = StyleItem;
      if (NoOverlapItem) {
        style |= ComponentArrangementStyles.ModifierNoOverlap;
      }
      if (FromSketchItem) {
        style |= ComponentArrangementStyles.ModifierAsIs;
      }
      layout.Style = style;

      double w, h;
      if (graphControl != null && UseScreenRatioItem) {
        var canvasSize = graphControl.InnerSize;
        w = canvasSize.Width;
        h = canvasSize.Height;
      }
      else {
        w = AspectRatioItem;
        h = 1 / w;
        w *= 400;
        h *= 400;
      }
      layout.PreferredSize = new YDimension(w, h);
      layout.ComponentSpacing = ComponentSpacingItem;
      if (GridEnabledItem) {
        layout.GridSpacing = GridSpacingItem;
      } else {
        layout.GridSpacing = 0;
      }

      return layout;
    }

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;

    [Label("Layout")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LayoutGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming
    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText {
      get {
        return
          "<Paragraph>The component layout algorithm arranges the connected components of a graph. "
          + "It can use any other layout style to arrange each component separately, and then arranges the components as such.</Paragraph>"
          + "<Paragraph>In this demo, the arrangement of each component is just kept as it is.</Paragraph>";
      }
    }

    [Label("Style")]
    [OptionGroup("LayoutGroup", 10)]
    [DefaultValue(ComponentArrangementStyles.Rows)]
    [EnumValue("No Arrangement", ComponentArrangementStyles.None)]
    [EnumValue("Multiple Rows",ComponentArrangementStyles.Rows)]
    [EnumValue("Single Row",ComponentArrangementStyles.SingleRow)]
    [EnumValue("Single Column",ComponentArrangementStyles.SingleColumn)]
    [EnumValue("Packed Rectangle",ComponentArrangementStyles.PackedRectangle)]
    [EnumValue("Compact Rectangle",ComponentArrangementStyles.PackedCompactRectangle)]
    [EnumValue("Packed Circle",ComponentArrangementStyles.PackedCircle)]
    [EnumValue("Compact Circle",ComponentArrangementStyles.PackedCompactCircle)]
    [EnumValue("Nested Rows",ComponentArrangementStyles.MultiRows)]
    [EnumValue("Compact Nested Rows",ComponentArrangementStyles.MultiRowsCompact)]
    [EnumValue("Width-constrained Nested Rows",ComponentArrangementStyles.MultiRowsWidthConstraint)]
    [EnumValue("Height-constrained Nested Rows",ComponentArrangementStyles.MultiRowsHeightConstraint)]
    [EnumValue("Width-constrained Compact Nested Rows",ComponentArrangementStyles.MultiRowsWidthConstraintCompact)]
    [EnumValue("Height-constrained Compact Nested Rows",ComponentArrangementStyles.MultiRowsHeightConstraintCompact)]
    public ComponentArrangementStyles StyleItem { get; set; }

    [Label("Remove Overlaps")]
    [OptionGroup("LayoutGroup", 20)]
    [DefaultValue(false)]
    public bool NoOverlapItem { get; set; }

    [Label("From Sketch")]
    [OptionGroup("LayoutGroup", 30)]
    [DefaultValue(false)]
    public bool FromSketchItem { get; set; }

    [Label("Use Screen Aspect Ratio")]
    [OptionGroup("LayoutGroup", 40)]
    [DefaultValue(true)]
    public bool UseScreenRatioItem { get; set; }

    [Label("Aspect Ratio")]
    [OptionGroup("LayoutGroup", 50)]
    [DefaultValue(1.0d)]
    [MinMax(Min = 0.2d, Max = 5.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double AspectRatioItem { get; set; }

    public bool ShouldDisableAspectRatioItem {
      get { return UseScreenRatioItem; }
    }

    [Label("Minimum Component Distance")]
    [OptionGroup("LayoutGroup", 60)]
    [DefaultValue(45.0d)]
    [MinMax(Min = 0.0d, Max = 400.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double ComponentSpacingItem { get; set; }

    [Label("Route on Grid")]
    [OptionGroup("LayoutGroup", 70)]
    [DefaultValue(false)]
    public bool GridEnabledItem { get; set; }

    [Label("Grid Spacing")]
    [OptionGroup("LayoutGroup", 80)]
    [DefaultValue(20.0d)]
    [MinMax(Min = 2, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double GridSpacingItem { get; set; }

    public bool ShouldDisableGridSpacingItem {
      get { return !GridEnabledItem; }
    }
  }
}