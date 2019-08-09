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
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Graph;
using yWorks.Layout;

namespace Demo.yFiles.GraphEditor.UI
{
  internal class EdgeGroupConfigurator : IGraphModule
  {
    public const string NAME = "EdgeGroupConfigurator";
    public const string TOP_LEVEL = "TOP_LEVEL";
    public const string SourceID = "SourceID";
    public const string TargetID = "TargetID";

    public const string ClearAllConstraints = "ClearAllConstraints";

    public const string Scope = "Scope";
    public const string ScopeAllEdges = "ScopeAll";
    public const string ScopeSelectedEdges = "SelectedEdges";
    public const string ScopeEdgesAtSelectedNodes = "SelectedNodes";

    private static readonly IList<string> scopes = new List<string>(3);

    private IMapper<IEdge, object> sourceIDMapper;
    private IMapper<IEdge, object> targetIDMapper;

    static EdgeGroupConfigurator() {
      scopes.Add(ScopeAllEdges);
      scopes.Add(ScopeSelectedEdges);
      scopes.Add(ScopeEdgesAtSelectedNodes);
    }

    public EdgeGroupConfigurator() : base(NAME) { }

    protected override void RunModule() {
      IMapperRegistry registry = CurrentIGraph.MapperRegistry;
      OptionGroup toplevelGroup = Handler.GetGroupByName(TOP_LEVEL);
      if ((bool) toplevelGroup[ClearAllConstraints].Value) {
        //deregistriere den Mapper, d.h. port constraints werden nicht mehr vorgegeben.
        registry.RemoveMapper(PortConstraintKeys.SourceGroupIdDpKey);
        registry.RemoveMapper(PortConstraintKeys.TargetGroupIdDpKey);
        sourceIDMapper = null;
        targetIDMapper = null;
      } else {
        registry.RemoveMapper(PortConstraintKeys.SourceGroupIdDpKey);
        registry.RemoveMapper(PortConstraintKeys.TargetGroupIdDpKey);

        if (sourceIDMapper == null) {
          sourceIDMapper = new DictionaryMapper<IEdge, object>();
        }
        if (targetIDMapper == null) {
          targetIDMapper = new DictionaryMapper<IEdge, object>();
        }

        string scope = (string) toplevelGroup[Scope].Value;

        string sourceId = toplevelGroup[SourceID].Value.ToString();
        string targetId = toplevelGroup[TargetID].Value.ToString();

        foreach (IEdge edge in CurrentIGraph.Edges) {
          bool isSelected = false;
          switch (scope) {
            case ScopeAllEdges:
              isSelected = true;
              break;
            case ScopeSelectedEdges:
              isSelected = IsSelected(Context, edge);
              break;
            case ScopeEdgesAtSelectedNodes:
              IPort sourcePort = edge.SourcePort;
              IPort targetPort = edge.TargetPort;
              isSelected = IsSelected(Context, sourcePort)
                           || IsSelected(Context, targetPort)
                           || IsSelected(Context, sourcePort.Owner)
                           || IsSelected(Context, targetPort.Owner);
              break;
          }
          if (isSelected) {
            sourceIDMapper[edge] = sourceId.Length > 0 ? sourceId : null;
            targetIDMapper[edge] = targetId.Length > 0 ? targetId : null;
          }
        }

        registry.AddMapper(PortConstraintKeys.SourceGroupIdDpKey,
                           sourceIDMapper);
        registry.AddMapper(PortConstraintKeys.TargetGroupIdDpKey,
                           targetIDMapper);
      }
    }

    protected override void SetupHandler() {
      OptionGroup toplevelGroup = Handler.AddGroup(TOP_LEVEL);
      //the toplevel group will show neither in Table view nor in dialog view explicitely
      //it's children will be shown one level above
      toplevelGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      toplevelGroup.Attributes[DefaultEditorFactory.RenderingHintsAttribute] = DefaultEditorFactory.RenderingHints.Invisible;

      var sourceIdItem = toplevelGroup.AddString(SourceID, "");
      var targetIdItem = toplevelGroup.AddString(TargetID, "");

      CollectionOptionItem<string> scopeItem = toplevelGroup.AddList(Scope, scopes, ScopeAllEdges);
      var clearItem = toplevelGroup.AddBool(ClearAllConstraints, false);
      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(clearItem, false, sourceIdItem);
      cm.SetEnabledOnValueEquals(clearItem, false, scopeItem);
      cm.SetEnabledOnValueEquals(clearItem, false, targetIdItem);
    }   
  }
}