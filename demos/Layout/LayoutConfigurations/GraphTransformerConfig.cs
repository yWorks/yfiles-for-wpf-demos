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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Layout;
using yWorks.Layout.Transformer;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("GraphTransformer")]
  public class GraphTransformerConfig : LayoutConfiguration {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public GraphTransformerConfig() {
      var transformer = new GraphTransformer();
      
      OperationItem = OperationType.Scale;
      ActOnSelectionOnlyItem = false;
      RotationAngleItem = transformer.RotationAngle;
      ApplyBestFitRotationItem = false;
      ScaleFactorItem = transformer.ScaleFactorX;
      ScaleNodeSizeItem = transformer.ScaleNodeSize;
      TranslateXItem = transformer.TranslateX;
      TranslateYItem = transformer.TranslateY;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var transformer = new GraphTransformer();
      transformer.Operation = OperationItem;
      transformer.SubgraphLayoutEnabled = ActOnSelectionOnlyItem;
      transformer.RotationAngle = RotationAngleItem;
      if (ApplyBestFitRotationItem && OperationItem == OperationType.Rotate) {
        var size = graphControl.InnerSize;
        ApplyBestFitRotationItem = true;

        var layoutGraph = new LayoutGraphAdapter(graphControl.Graph).CreateCopiedLayoutGraph();
        transformer.RotationAngle = GraphTransformer.FindBestFitRotationAngle(layoutGraph, size.Width, size.Height);
      } else {
        ApplyBestFitRotationItem = false;
      }

      transformer.ScaleFactor = ScaleFactorItem;
      transformer.ScaleNodeSize = ScaleNodeSizeItem;
      transformer.TranslateX = TranslateXItem;
      transformer.TranslateY = TranslateYItem;

      return transformer;
    }

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GeneralGroup;

    [Label("Rotate")]
    [OptionGroup("GeneralGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object RotateGroup;

    [Label("Scale")]
    [OptionGroup("GeneralGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object ScaleGroup;

    [Label("Translate")]
    [OptionGroup("GeneralGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object TranslateGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming
    
    [Label("Operation")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(OperationType.Scale)]
    [EnumValue("Mirror on X axis", OperationType.MirrorXAxis)]
    [EnumValue("Mirror on Y axis",OperationType.MirrorYAxis)]
    [EnumValue("Rotate",OperationType.Rotate)]
    [EnumValue("Scale",OperationType.Scale)]
    [EnumValue("Translate",OperationType.Translate)]
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

    public bool ShouldDisableRotationAngleItem {
      get { return OperationItem != OperationType.Rotate || ApplyBestFitRotationItem; }
    }

    [Label("Best Fit Rotation")]
    [OptionGroup("RotateGroup", 20)]
    [DefaultValue(false)]
    public bool ApplyBestFitRotationItem { get; set; }

    public bool ShouldDisableApplyBestFitRotationItem {
      get { return OperationItem != OperationType.Rotate; }
    }

    [Label("Scale Factor")]
    [OptionGroup("ScaleGroup", 10)]
    [DefaultValue(1.0d)]
    [MinMax(Min = 0.1d, Max = 10.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double ScaleFactorItem { get; set; }

    public bool ShouldDisableScaleFactorItem {
      get { return OperationItem != OperationType.Scale; }
    }

    [Label("Scale Node Size")]
    [OptionGroup("ScaleGroup", 20)]
    [DefaultValue(false)]
    public bool ScaleNodeSizeItem { get; set; }

    public bool ShouldDisableScaleNodeSizeItem {
      get { return OperationItem != OperationType.Scale; }
    }

    [Label("Horizontal Distance")]
    [OptionGroup("TranslateGroup", 10)]
    [DefaultValue(0.0d)]
    public double TranslateXItem { get; set; }

    public bool ShouldDisableTranslateXItem {
      get { return OperationItem != OperationType.Translate; }
    }

    [Label("Vertical Distance")]
    [OptionGroup("TranslateGroup", 20)]
    [DefaultValue(0.0d)]
    public double TranslateYItem { get; set; }

    public bool ShouldDisableTranslateYItem {
      get { return OperationItem != OperationType.Translate; }
    }

  }
}
