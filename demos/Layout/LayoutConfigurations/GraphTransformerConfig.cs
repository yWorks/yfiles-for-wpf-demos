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
using System.ComponentModel;
using System.Reflection;
using yWorks.Controls;
using yWorks.Demo.Toolkit.OptionHandler;
using yWorks.Graph;
using yWorks.Layout;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("GraphTransformer")]
  public class GraphTransformerConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public GraphTransformerConfig() {
      //var transformer = new GraphTransformer();

      OperationItem = OperationType.Scale;
      ActOnSelectionOnlyItem = false;
      RotationAngleItem = 0;
      ApplyBestFitRotationItem = false;
      ScaleFactorItem = 1;
      ScaleNodeSizeItem = false;
      TranslateXItem = 0;
      TranslateYItem = 0;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      ILayoutStage stage;
      switch (OperationItem) {
        case OperationType.MirrorXAxis:
          stage = LayoutTransformations.CreateMirroringStage(true);
          break;
        case OperationType.MirrorYAxis:
          stage = LayoutTransformations.CreateMirroringStage(false);
          break;
        case OperationType.Rotate:
          stage = ApplyBestFitRotationItem
              ? LayoutTransformations.CreateBestFitRotationStage(graphControl.InnerSize)
              : LayoutTransformations.CreateRotationStage(RotationAngleItem);
          break;
        case OperationType.Scale:
          stage = LayoutTransformations.CreateScalingStage(ScaleFactorItem, ScaleFactorItem, ScaleNodeSizeItem);
          break;
        case OperationType.Translate:
          stage = LayoutTransformations.CreateTranslationStage(TranslateXItem, TranslateYItem);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return ActOnSelectionOnlyItem ? new SubgraphLayoutStage(stage) : stage;
    }

    /// <inheritdoc/>
    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) =>
        ActOnSelectionOnlyItem ? CreateSubgraphLayoutData(graphControl) : null;

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GeneralGroup;

    [Label("Rotate")]
    [OptionGroup("GeneralGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object RotateGroup;

    [Label("Scale")]
    [OptionGroup("GeneralGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object ScaleGroup;

    [Label("Translate")]
    [OptionGroup("GeneralGroup", 50)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object TranslateGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    [Label("Operation")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(OperationType.Scale)]
    [EnumValue("Mirror on X axis", OperationType.MirrorXAxis)]
    [EnumValue("Mirror on Y axis", OperationType.MirrorYAxis)]
    [EnumValue("Rotate", OperationType.Rotate)]
    [EnumValue("Scale", OperationType.Scale)]
    [EnumValue("Translate", OperationType.Translate)]
    public OperationType OperationItem { get; set; }

    [Label("Act on Selection Only")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(false)]
    public bool ActOnSelectionOnlyItem { get; set; }

    [Label("Rotation Angle")]
    [OptionGroup("RotateGroup", 10)]
    [DefaultValue(0.0d)]
    [MinMax(Min = -360, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public double RotationAngleItem { get; set; }

    public bool ShouldDisableRotationAngleItem => OperationItem != OperationType.Rotate || ApplyBestFitRotationItem;

    [Label("Best Fit Rotation")]
    [OptionGroup("RotateGroup", 20)]
    [DefaultValue(false)]
    public bool ApplyBestFitRotationItem { get; set; }

    public bool ShouldDisableApplyBestFitRotationItem => OperationItem != OperationType.Rotate;

    [Label("Scale Factor")]
    [OptionGroup("ScaleGroup", 10)]
    [DefaultValue(1.0d)]
    [MinMax(Min = 0.1d, Max = 10.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double ScaleFactorItem { get; set; }

    public bool ShouldDisableScaleFactorItem => OperationItem != OperationType.Scale;

    [Label("Scale Node Size")]
    [OptionGroup("ScaleGroup", 20)]
    [DefaultValue(false)]
    public bool ScaleNodeSizeItem { get; set; }

    public bool ShouldDisableScaleNodeSizeItem => OperationItem != OperationType.Scale;

    [Label("Horizontal Distance")]
    [OptionGroup("TranslateGroup", 10)]
    [DefaultValue(0.0d)]
    [MinMax(Min = -999d, Max = 999d, Step = 1)]
    [ComponentType(ComponentTypes.Slider)]
    public double TranslateXItem { get; set; }

    public bool ShouldDisableTranslateXItem => OperationItem != OperationType.Translate;

    [Label("Vertical Distance")]
    [OptionGroup("TranslateGroup", 20)]
    [DefaultValue(0.0d)]
    [MinMax(Min = -999d, Max = 999d, Step = 1)]
    [ComponentType(ComponentTypes.Slider)]
    public double TranslateYItem { get; set; }

    public bool ShouldDisableTranslateYItem => OperationItem != OperationType.Translate;

    public enum OperationType
    {
      MirrorXAxis = 0,
      MirrorYAxis = 1,
      Rotate = 2,
      Scale = 3,
      Translate = 4
    }
  }
}
