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
using System.Reflection;
using System.Resources;
using System.Windows;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.I18N;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.GraphEditor.UI
{
  static class DefaultsEditor
  {
    private static readonly ResourceManagerI18NFactory i18NFactory;

    static DefaultsEditor() {
      i18NFactory = new ResourceManagerI18NFactory();
      ResourceManager rm =
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.DefaultsEditorI18N",
                              Assembly.GetExecutingAssembly());
      i18NFactory.AddResourceManager("DEFAULTS", rm);
    }

    public const string NAME = "DEFAULTS";
    public const string GRAPH_SETTINGS = "GRAPH_SETTINGS";
    public const string AutoAdjustPreferredLabelSize = "AutoAdjustPreferredLabelSize";
    public const string AutoCleanupPorts = "AutoCleanupPorts";
    public const string SHARING_SETTINGS = "SHARING_SETTINGS";
    public const string ShareDefaultNodeStyleInstance = "ShareDefaultNodeStyleInstance";
    public const string ShareDefaultEdgeStyleInstance = "ShareDefaultEdgeStyleInstance";
    public const string ShareDefaultNodeLabelStyleInstance = "ShareDefaultNodeLabelStyleInstance";
    public const string ShareDefaultEdgeLabelStyleInstance = "ShareDefaultEdgeLabelStyleInstance";
    public const string ShareDefaultPortStyleInstance = "ShareDefaultPortStyleInstance";
    public const string ShareDefaultNodeLabelModelParameter = "ShareDefaultNodeLabelModelParameter";
    public const string ShareDefaultEdgeLabelModelParameter = "ShareDefaultEdgeLabelModelParameter";

    public const string UI_DEFAULTS = "UI_DEFAULTS";

    public const string HitTestRadius = "HitTestRadius";
    public const string GridEnabled = "GridEnabled";
    public const string GridVisible = "GridVisible";
    public const string GridWidth = "GridWidth";
    public const string GridSnapeType = "GridSnapType";
    public const string GridStyle = "GridStyle";
    public const string AutoAdjustContentRect = "AutoAdjustContentRect";
    public const string AutoRemoveEmptyLabels = "AutoRemoveEmptyLabels";

    public const string BRIDGES = "BRIDGES";
    public const string CrossingDetermination = "CrossingDetermination";
    public const string BridgeSettings = "BridgeSettings";
    public const string BridgesEnabled = "BridgesEnabled";
    public const string DefaultBridgeOrientationStyle = "DefaultBridgeOrientationStyle";
    public const string DefaultCrossingStyle = "DefaultBridgeCrossingStyle";
    public const string DefaultBridgeHeight = "DefaultBridgeHeight";
    public const string DefaultBridgeWidth = "DefaultBridgeWidth";
    public const string ZoomThreshold = "ZoomThreshold";
    public const string ConsiderCurves = "ConsiderCurves";
    
    

    public const string MISC_SETTINGS = "MISC_SETTINGS";
    public const string UndoEngine_Size = "UndoEngine_Size";

    static OptionHandler CreateHandler(GraphEditorWindow form) {
      GraphControl gc = form.GraphControl;
      IGraph g = form.Graph;
      GraphEditorInputMode geim = form.GraphEditorInputMode;
      

      OptionHandler handler = new OptionHandler(NAME);
      OptionGroup controlGroup = handler.AddGroup(UI_DEFAULTS);
      controlGroup.AddDouble(HitTestRadius, gc.HitTestRadius);
      controlGroup.AddBool(AutoRemoveEmptyLabels, geim.AutoRemoveEmptyLabels);

//      var gridEnabledItem = controlGroup.AddBool(GridEnabled, form.Grid.Enabled);
      var gridVisibleItem = controlGroup.AddBool(GridVisible, form.GridVisible);
      var gridWidthItem = controlGroup.AddInt(GridWidth, form.GridWidth);
      var gridSnapTypeItem = controlGroup.AddList(GridSnapeType, new List<GridSnapTypes>()
                                                                   {
                                                                     GridSnapTypes.All, GridSnapTypes.GridPoints, GridSnapTypes.HorizontalLines, GridSnapTypes.Lines, GridSnapTypes.None, GridSnapTypes.VerticalLines
                                                                   }, form.GridSnapType);

      ConstraintManager cm = new ConstraintManager(handler);
      cm.SetEnabledOnCondition(
        ConstraintManager.LogicalCondition.Or(cm.CreateValueEqualsCondition(gridVisibleItem, true),
        cm.CreateValueEqualsCondition(gridVisibleItem, true)),
        gridWidthItem);
      cm.SetEnabledOnValueEquals(gridVisibleItem, true, gridSnapTypeItem);

      if (g != null) {
        OptionGroup graphGroup = handler.AddGroup(GRAPH_SETTINGS);
        graphGroup.AddBool(AutoAdjustPreferredLabelSize, g.NodeDefaults.Labels.AutoAdjustPreferredSize);
        graphGroup.AddBool(AutoCleanupPorts, g.NodeDefaults.Ports.AutoCleanUp);
        OptionGroup sharingGroup = graphGroup.AddGroup(SHARING_SETTINGS);

        sharingGroup.AddBool(ShareDefaultNodeStyleInstance, g.NodeDefaults.ShareStyleInstance);
        sharingGroup.AddBool(ShareDefaultEdgeStyleInstance, g.EdgeDefaults.ShareStyleInstance);
        sharingGroup.AddBool(ShareDefaultNodeLabelStyleInstance, g.NodeDefaults.Labels.ShareStyleInstance);
        sharingGroup.AddBool(ShareDefaultEdgeLabelStyleInstance, g.EdgeDefaults.Labels.ShareStyleInstance);
        sharingGroup.AddBool(ShareDefaultPortStyleInstance, g.NodeDefaults.Ports.ShareStyleInstance);
        sharingGroup.AddBool(ShareDefaultNodeLabelModelParameter, g.NodeDefaults.Labels.ShareLayoutParameterInstance);
        sharingGroup.AddBool(ShareDefaultEdgeLabelModelParameter, g.EdgeDefaults.Labels.ShareLayoutParameterInstance);
      }
      OptionGroup miscGroup = handler.AddGroup(MISC_SETTINGS);
      UndoEngine undoEngine = form.Graph.GetUndoEngine();
      if (undoEngine != null) {
        miscGroup.AddInt(UndoEngine_Size, undoEngine.Size);
      }
      return handler;
    }

    public static void EditDefaults(GraphEditorWindow form) {
      OptionHandler handler = CreateHandler(form);
      handler.I18nFactory = i18NFactory;
      if (new EditorForm { OptionHandler = handler, IsAutoAdopt = true, IsAutoCommit = true, ShowResetButton = true, Owner = form, Title = i18NFactory.GetString(NAME, NAME) }.ShowDialog() == true) {
        CommitValuesToForm(handler, form);
      }
    }

    private static void CommitValuesToForm(OptionHandler handler, GraphEditorWindow form) {
      GraphControl gc = form.GraphControl;
      IGraph g = form.Graph;
      GraphEditorInputMode geim = form.GraphEditorInputMode;
      
      OptionGroup controlGroup = handler.GetGroupByName(UI_DEFAULTS);

      OptionGroup graphGroup = handler.GetGroupByName(GRAPH_SETTINGS);
      OptionGroup sharingGroup = (OptionGroup) graphGroup.GetGroupByName(SHARING_SETTINGS);
      OptionGroup miscGroup = handler.GetGroupByName(MISC_SETTINGS);

      gc.HitTestRadius = (double)controlGroup[HitTestRadius].Value;
      geim.AutoRemoveEmptyLabels = (bool)controlGroup[AutoRemoveEmptyLabels].Value;

      form.GridWidth = (int)controlGroup[GridWidth].Value;
      form.GridSnapType = (GridSnapTypes)controlGroup[GridSnapeType].Value;
      form.GridVisible = (bool)controlGroup[GridVisible].Value;

      if (g != null) {
        g.NodeDefaults.Labels.AutoAdjustPreferredSize = g.EdgeDefaults.Labels.AutoAdjustPreferredSize = (bool)graphGroup[AutoAdjustPreferredLabelSize].Value;
        g.NodeDefaults.Ports.AutoCleanUp = g.EdgeDefaults.Ports.AutoCleanUp = (bool)graphGroup[AutoCleanupPorts].Value;

        g.NodeDefaults.ShareStyleInstance = (bool)sharingGroup[ShareDefaultNodeStyleInstance].Value;
        g.EdgeDefaults.ShareStyleInstance = (bool)sharingGroup[ShareDefaultEdgeStyleInstance].Value;
        g.NodeDefaults.Labels.ShareStyleInstance = (bool)sharingGroup[ShareDefaultNodeLabelStyleInstance].Value;
        g.EdgeDefaults.Labels.ShareStyleInstance = (bool)sharingGroup[ShareDefaultEdgeLabelStyleInstance].Value;
        g.NodeDefaults.Ports.ShareStyleInstance = g.EdgeDefaults.Ports.ShareStyleInstance = (bool)sharingGroup[ShareDefaultPortStyleInstance].Value;
        g.NodeDefaults.Labels.ShareLayoutParameterInstance = (bool)sharingGroup[ShareDefaultNodeLabelModelParameter].Value;
        g.EdgeDefaults.Labels.ShareLayoutParameterInstance = (bool)sharingGroup[ShareDefaultEdgeLabelModelParameter].Value;
      }
      UndoEngine undoEngine = form.Graph.GetUndoEngine();
      if (undoEngine != null) {
        undoEngine.Size = (int)miscGroup[UndoEngine_Size].Value;
      }
    }
  }
}
