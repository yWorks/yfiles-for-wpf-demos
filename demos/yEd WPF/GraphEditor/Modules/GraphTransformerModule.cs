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

using System.Collections.Generic;
using System.Windows;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout.Transformer;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for 
  /// <see cref="GraphTransformer"/>.
  /// </summary>
  public class GraphTransformerModule : LayoutModule
  {
    #region configuration constants
    private const string SCALE_FACTOR = "SCALE_FACTOR";
    private const string TOP_LEVEL = "TOP_LEVEL";
    private const string ACT_ON_SELECTION_ONLY = "ACT_ON_SELECTION_ONLY";
    private const string SCALE_NODE_SIZE = "SCALE_NODE_SIZE";
    private const string APPLY_BEST_FIT_ROTATION = "APPLY_BEST_FIT_ROTATION";
    private const string ROTATION_ANGLE = "ROTATION_ANGLE";
    private const string OPERATION = "OPERATION";
    private const string GRAPH_TRANSFORMER = "GRAPH_TRANSFORMER";
    private const string SCALE = "SCALE";
    private const string ROTATE = "ROTATE";
    private const string MIRROR_ON_Y_AXIS = "MIRROR_ON_Y-AXIS";
    private const string MIRROR_ON_X_AXIS = "MIRROR_ON_X-AXIS";
    private const string TRANSLATE_X = "TRANSLATE_X";
    private const string TRANSLATE_Y = "TRANSLATE_Y";
    private const string TRANSLATE = "TRANSLATE";
    private const string GENERAL = "GENERAL";

    private static readonly Dictionary<string, OperationType> operationEnum = new Dictionary<string, OperationType>();

    static GraphTransformerModule() {
      operationEnum.Add(MIRROR_ON_X_AXIS, OperationType.MirrorXAxis);
      operationEnum.Add(MIRROR_ON_Y_AXIS, OperationType.MirrorYAxis);
      operationEnum.Add(ROTATE, OperationType.Rotate);
      operationEnum.Add(SCALE, OperationType.Scale);
      operationEnum.Add(TRANSLATE, OperationType.Translate);
    }
    #endregion

    #region private members
    private GraphTransformer transformer;
    private bool applyBestFitRotation = false;
    #endregion
    
    /// <summary>
    /// Create a new instance.
    /// </summary>
    public GraphTransformerModule() : base(GRAPH_TRANSFORMER) {
      transformer = new GraphTransformer();
    }

    #region LayoutModule interface
    ///<inheritdoc/>
    protected override void SetupHandler() {
      ConstraintManager cm = new ConstraintManager(Handler);
      OptionGroup toplevelGroup = Handler.AddGroup(TOP_LEVEL);
      //the toplevel group will show neither in Table view nor in dialog view explicitely
      //it's children will be shown one level above
      toplevelGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      toplevelGroup.Attributes[DefaultEditorFactory.RenderingHintsAttribute] = DefaultEditorFactory.RenderingHints.Invisible;

      OptionGroup generalGroup = toplevelGroup.AddGroup(GENERAL);
      OptionItem operationItem = generalGroup.AddList(OPERATION, operationEnum.Keys, SCALE);
      generalGroup.AddBool(ACT_ON_SELECTION_ONLY, false);

      OptionGroup rotateGroup = toplevelGroup.AddGroup(ROTATE);
      cm.SetEnabledOnValueEquals(operationItem, ROTATE, rotateGroup);
      rotateGroup.AddInt( ROTATION_ANGLE, (int)transformer.RotationAngle, -360, 360);
      rotateGroup.AddBool( APPLY_BEST_FIT_ROTATION, applyBestFitRotation);

      OptionGroup scaleGroup = toplevelGroup.AddGroup(SCALE);
      cm.SetEnabledOnValueEquals(operationItem, SCALE, scaleGroup);
      scaleGroup.AddDouble(SCALE_FACTOR, transformer.ScaleFactorX, 0.1, 10.0);
      scaleGroup.AddBool( SCALE_NODE_SIZE, transformer.ScaleNodeSize);

      OptionGroup translateGroup = toplevelGroup.AddGroup(TRANSLATE);
      cm.SetEnabledOnValueEquals(operationItem, TRANSLATE, translateGroup);
      translateGroup.AddDouble(TRANSLATE_X, transformer.TranslateX);
      translateGroup.AddDouble(TRANSLATE_Y, transformer.TranslateY);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      OptionGroup toplevelGroup = (OptionGroup)Handler.GetGroupByName(TOP_LEVEL);
      OptionGroup generalGroup = (OptionGroup)toplevelGroup.GetGroupByName(GENERAL);
      string operationChoice = (string)generalGroup[OPERATION].Value;
      transformer.Operation = operationEnum[operationChoice];
      transformer.SubgraphLayoutEnabled = (bool)generalGroup[ACT_ON_SELECTION_ONLY].Value;

      transformer.RotationAngle = (int)toplevelGroup.GetValue(ROTATE, ROTATION_ANGLE);
      if ((bool)toplevelGroup.GetValue(ROTATE, APPLY_BEST_FIT_ROTATION) &&
          ((string)toplevelGroup.GetValue(GENERAL, OPERATION)).Equals(ROTATE)) {
        CanvasControl cv = Context.Lookup<CanvasControl>();
        if (cv != null) {
          Size size;
          size = cv.InnerSize;
          applyBestFitRotation = true;
          transformer.RotationAngle =
            GraphTransformer.FindBestFitRotationAngle(CurrentLayoutGraph, size.Width, size.Height);
        }
      } else {
        applyBestFitRotation = false;
      }

      transformer.ScaleFactor = (double)toplevelGroup.GetValue(SCALE, SCALE_FACTOR);
      transformer.ScaleNodeSize = (bool)toplevelGroup.GetValue(SCALE, SCALE_NODE_SIZE);

      transformer.TranslateX = (double)toplevelGroup.GetValue(TRANSLATE, TRANSLATE_X);
      transformer.TranslateY = (double)toplevelGroup.GetValue(TRANSLATE, TRANSLATE_Y);
      LayoutAlgorithm = transformer;
    }
    #endregion
  }
}
