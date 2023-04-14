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

using System.Collections.Generic;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Algorithms.Geometry;
using yWorks.Layout;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="ComponentLayout"/>.
  /// </summary>
  public class ComponentLayoutModule : LayoutModule
  {
    #region configuration constants

    private const string COMPONENTLAYOUT = "COMPONENTLAYOUTER";
    private const string STYLE = "STYLE";
    private const string STYLE_NONE = "STYLE_NONE";
    private const string STYLE_ROWS = "STYLE_ROWS";
    private const string STYLE_SINGLE_ROW = "STYLE_SINGLE_ROW";
    private const string STYLE_SINGLE_COLUMN = "STYLE_SINGLE_COLUMN";
    private const string STYLE_PACKED_COMPACT_RECTANGLE = "STYLE_PACKED_COMPACT_RECTANGLE";
    private const string STYLE_PACKED_RECTANGLE = "STYLE_PACKED_RECTANGLE";
    private const string STYLE_PACKED_CIRCLE = "STYLE_PACKED_CIRCLE";
    private const string STYLE_PACKED_COMPACT_CIRCLE = "STYLE_PACKED_COMPACT_CIRCLE";
    private const string STYLE_MULTI_ROWS = "STYLE_MULTI_ROWS";
    private const string STYLE_MULTI_ROWS_COMPACT = "STYLE_MULTI_ROWS_COMPACT";
    private const string STYLE_MULTI_ROWS_WIDTH_CONSTRAINED = "STYLE_MULTI_ROWS_WIDTH_CONSTRAINED";
    private const string STYLE_MULTI_ROWS_HEIGHT_CONSTRAINED = "STYLE_MULTI_ROWS_HEIGHT_CONSTRAINED";
    private const string STYLE_MULTI_ROWS_WIDTH_CONSTRAINED_COMPACT = "STYLE_MULTI_ROWS_WIDTH_CONSTRAINED_COMPACT";
    private const string STYLE_MULTI_ROWS_HEIGHT_CONSTRAINED_COMPACT = "STYLE_MULTI_ROWS_HEIGHT_CONSTRAINED_COMPACT";

    private const string FROM_SKETCH = "FROM_SKETCH";
    private const string NO_OVERLAP = "NO_OVERLAP";
    private const string ASPECT_RATIO = "ASPECT_RATIO";
    private const string USE_SCREEN_RATIO = "USE_SCREEN_RATIO";
    private const string COMPONENT_SPACING = "COMPONENT_SPACING";
    private const string GRID_SPACING = "GRID_SPACING";
    private const string GRID_ENABLED = "GRID_ENABLED";

    private static readonly Dictionary<string, ComponentArrangementStyles> styleEnum = new Dictionary<string, ComponentArrangementStyles>();

    static ComponentLayoutModule() {
      styleEnum.Add(STYLE_NONE, ComponentArrangementStyles.None);
      styleEnum.Add(STYLE_ROWS, ComponentArrangementStyles.Rows);
      styleEnum.Add(STYLE_SINGLE_ROW, ComponentArrangementStyles.SingleRow);
      styleEnum.Add(STYLE_SINGLE_COLUMN, ComponentArrangementStyles.SingleColumn);
      styleEnum.Add(STYLE_PACKED_RECTANGLE, ComponentArrangementStyles.PackedRectangle);
      styleEnum.Add(STYLE_PACKED_COMPACT_RECTANGLE, ComponentArrangementStyles.PackedCompactRectangle);
      styleEnum.Add(STYLE_PACKED_CIRCLE, ComponentArrangementStyles.PackedCircle);
      styleEnum.Add(STYLE_PACKED_COMPACT_CIRCLE, ComponentArrangementStyles.PackedCompactCircle);
      styleEnum.Add(STYLE_MULTI_ROWS, ComponentArrangementStyles.MultiRows);
      styleEnum.Add(STYLE_MULTI_ROWS_COMPACT, ComponentArrangementStyles.MultiRowsCompact);
      styleEnum.Add(STYLE_MULTI_ROWS_WIDTH_CONSTRAINED, ComponentArrangementStyles.MultiRowsWidthConstraint);
      styleEnum.Add(STYLE_MULTI_ROWS_HEIGHT_CONSTRAINED, ComponentArrangementStyles.MultiRowsHeightConstraint);
      styleEnum.Add(STYLE_MULTI_ROWS_WIDTH_CONSTRAINED_COMPACT, ComponentArrangementStyles.MultiRowsWidthConstraintCompact);
      styleEnum.Add(STYLE_MULTI_ROWS_HEIGHT_CONSTRAINED_COMPACT, ComponentArrangementStyles.MultiRowsHeightConstraintCompact);
    }

    #endregion

    #region pivate members

    private ComponentLayout componentLayout;

    #endregion

    /// <summary>
    /// Create a new instance
    /// </summary>
    public ComponentLayoutModule() : base(COMPONENTLAYOUT) {}

    #region LayoutModule interface

    ///<inheritdoc/>
    protected override void SetupHandler() {
      CreateLayout();
      ConstraintManager cm = new ConstraintManager(Handler);
      var topLevelGroup = Handler.AddGroup("TOP_LEVEL");
      topLevelGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      topLevelGroup.Attributes[DefaultEditorFactory.RenderingHintsAttribute] = DefaultEditorFactory.RenderingHints.Invisible;

      topLevelGroup.AddList(STYLE, styleEnum.Keys, STYLE_ROWS);
      topLevelGroup.AddBool(NO_OVERLAP,
                      (componentLayout.Style & ComponentArrangementStyles.ModifierNoOverlap) != 0);
      topLevelGroup.AddBool(FROM_SKETCH, (componentLayout.Style & ComponentArrangementStyles.ModifierAsIs) != 0);
      YDimension size = componentLayout.PreferredSize;
      IOptionItem useScreenRationItem = topLevelGroup.AddBool(USE_SCREEN_RATIO, true);
      IOptionItem aspectRationItem = topLevelGroup.AddDouble(ASPECT_RATIO, size.Width / size.Height);
      cm.SetEnabledOnValueEquals(useScreenRationItem, false, aspectRationItem);

      topLevelGroup.AddDouble(COMPONENT_SPACING, componentLayout.ComponentSpacing, 0.0d, double.MaxValue);
      IOptionItem gridEnabledItem = topLevelGroup.AddBool(GRID_ENABLED, componentLayout.GridSpacing > 0);
      IOptionItem gridSpacingItem =
        topLevelGroup.AddDouble(GRID_SPACING,
                          componentLayout.GridSpacing > 0 ? componentLayout.GridSpacing : 20.0d);
      cm.SetEnabledOnValueEquals(gridEnabledItem, true, gridSpacingItem);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      componentLayout.ComponentArrangement = true;
      OptionGroup toplevelGroup = (OptionGroup) Handler.GetGroupByName("TOP_LEVEL");
      string styleChoice = (string)toplevelGroup[STYLE].Value;
      ComponentArrangementStyles style = styleEnum[styleChoice];
      if ((bool)toplevelGroup[NO_OVERLAP].Value) {
        style |= ComponentArrangementStyles.ModifierNoOverlap;
      }
      if ((bool)toplevelGroup[FROM_SKETCH].Value) {
        style |= ComponentArrangementStyles.ModifierAsIs;
      }
      componentLayout.Style = style;

      CanvasControl cv = Context.Lookup<CanvasControl>();
      double w, h;
      if ((cv != null) && (bool)toplevelGroup[USE_SCREEN_RATIO].Value) {
        var canvasSize = cv.InnerSize;
        w = canvasSize.Width;
        h = canvasSize.Height;
      } else {
        w = (double)toplevelGroup[ASPECT_RATIO].Value;
        h = 1.0d / w;
        w *= 400.0d;
        h *= 400.0d;
      }
      componentLayout.PreferredSize = new YDimension(w, h);
      componentLayout.ComponentSpacing = (double)toplevelGroup[COMPONENT_SPACING].Value;
      if ((bool)toplevelGroup[GRID_ENABLED].Value) {
        componentLayout.GridSpacing = (double)toplevelGroup[GRID_SPACING].Value;
      } else {
        componentLayout.GridSpacing = 0;
      }

      LayoutAlgorithm = componentLayout;
    }

    #endregion

    #region private helpers

    private void CreateLayout() {
      if (componentLayout == null) {
        componentLayout = new ComponentLayout();
      }
    }

    #endregion
  }
}
