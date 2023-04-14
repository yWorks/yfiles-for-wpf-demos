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
using System.Threading.Tasks;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;

namespace Demo.yFiles.GraphEditor.UI
{
  internal class PortConstraintsConfigurator : IGraphModule
  {
    public const string NAME = "PortConstraintConfigurator";
    public const string TOP_LEVEL = "TOP_LEVEL";
    public const string SourcePortConstraints = "SourcePortConstraints";
    public const string TargetPortConstraints = "TargetPortConstraints";

    public const string PortConstraintStr = "PortConstraint";
    public const string StrongPortConstraint = "StrongPortConstraint";

    public const string ClearAllConstraints = "ClearAllConstraints";

    public const string Scope = "Scope";
    public const string ScopeAllEdges = "ScopeAll";
    public const string ScopeSelectedEdges = "SelectedEdges";
    public const string ScopeEdgesAtSelectedNodes = "SelectedNodes";

    private static readonly IList<string> scopes = new List<string>(3);

    private IMapper<IEdge, PortConstraint> sourcePCMapper;
    private IMapper<IEdge, PortConstraint> targetPCMapper;

    static PortConstraintsConfigurator() {
      scopes.Add(ScopeAllEdges);
      scopes.Add(ScopeSelectedEdges);
      scopes.Add(ScopeEdgesAtSelectedNodes);
    }

    public PortConstraintsConfigurator() : base(NAME) {}

    protected override Task RunModule() {
      IMapperRegistry registry = CurrentIGraph.MapperRegistry;
      OptionGroup toplevelGroup = Handler.GetGroupByName(TOP_LEVEL);
      if ((bool) toplevelGroup[ClearAllConstraints].Value) {
        //deregistriere den Mapper, d.h. port constraints werden nicht mehr vorgegeben.
        registry.RemoveMapper(PortConstraintKeys.TargetPortConstraintDpKey);
        registry.RemoveMapper(PortConstraintKeys.SourcePortConstraintDpKey);
        sourcePCMapper = null;
        targetPCMapper = null;
      } else {
        registry.RemoveMapper(PortConstraintKeys.TargetPortConstraintDpKey);
        registry.RemoveMapper(PortConstraintKeys.SourcePortConstraintDpKey);

        if (sourcePCMapper == null) {
          sourcePCMapper = new DictionaryMapper<IEdge, PortConstraint>();
        }
        if (targetPCMapper == null) {
          targetPCMapper = new DictionaryMapper<IEdge, PortConstraint>();
        }

        string scope = (string) toplevelGroup[Scope].Value;
        OptionGroup spcg = (OptionGroup) toplevelGroup.GetGroupByName(SourcePortConstraints);
        OptionGroup tpcg = (OptionGroup) toplevelGroup.GetGroupByName(TargetPortConstraints);
        PortConstraintType sourceType = (PortConstraintType) spcg[PortConstraintStr].Value;
        bool strongSource = (bool) spcg[StrongPortConstraint].Value;
        PortConstraintType targetType = (PortConstraintType) tpcg[PortConstraintStr].Value;
        bool strongTarget = (bool) spcg[StrongPortConstraint].Value;

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
            sourcePCMapper[edge] = CreatePortConstraint(edge, sourceType, true, strongSource);
            targetPCMapper[edge] = CreatePortConstraint(edge, targetType, false, strongTarget);
          }
        }

        registry.AddMapper(PortConstraintKeys.SourcePortConstraintDpKey,
                           sourcePCMapper);
        registry.AddMapper(PortConstraintKeys.TargetPortConstraintDpKey,
                           targetPCMapper);
      }
      return Task.FromResult<object>(null);
    }

    protected override void SetupHandler() {
      OptionGroup toplevelGroup = Handler.AddGroup(TOP_LEVEL);
      //the toplevel group will show neither in Table view nor in dialog view explicitely
      //it's children will be shown one level above
      ((IOptionItem) toplevelGroup).Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      ((IOptionItem) toplevelGroup).Attributes[DefaultEditorFactory.RenderingHintsAttribute] = DefaultEditorFactory.RenderingHints.Invisible;

      OptionGroup spcg = toplevelGroup.AddGroup(SourcePortConstraints);
      spcg.Add(PortConstraintStr, PortConstraintType.Any).Attributes[OptionItem.SupportNullValueAttribute] = false;
      spcg.AddBool(StrongPortConstraint, false);
      OptionGroup tpcg = toplevelGroup.AddGroup(TargetPortConstraints);
      tpcg.Add(PortConstraintStr, PortConstraintType.Any).Attributes[OptionItem.SupportNullValueAttribute] = false;
      tpcg.AddBool(StrongPortConstraint, false);
      CollectionOptionItem<string> scopeItem = toplevelGroup.AddList(Scope, scopes, ScopeAllEdges);
      var clearItem = toplevelGroup.AddBool(ClearAllConstraints, false);
      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(clearItem, false, spcg);
      cm.SetEnabledOnValueEquals(clearItem, false, scopeItem);
      cm.SetEnabledOnValueEquals(clearItem, false, tpcg);
    }

    public enum PortConstraintType: sbyte
    {
      Any = PortSide.Any,
      North = PortSide.North,
      East = PortSide.East,
      South = PortSide.South,
      West = PortSide.West,
      FromSketch
    }

    public PortConstraint CreatePortConstraint(IEdge key, PortConstraintType type, bool atSource, bool strong) {
      if (type == PortConstraintType.FromSketch) {
        return CreatePortConstraintFromSketch(key, atSource, strong);
      }
      return PortConstraint.Create((PortSide) type, strong);
    }

    private static PortConstraint CreatePortConstraintFromSketch(IEdge e, bool source, bool strong) {
      //Get connection port and owner
      IPort port = source ? e.SourcePort : e.TargetPort;
      INode portOwner = port.Owner as INode;
      if (portOwner != null) {
        //Einfachste Loesung:
        //Erzeugt einen strong PortConstraint genau an der port location
        //anschluesse in alle richtungen moeglich
        //        return PortConstraint.create(PortConstraint.ANY_SIDE, strong);
        //alternativ: z.B. Kantenpfad bestimmen und einen PortConstraint 
        //erzeugen, dessen Richtung durch den Schnittpunkt zwischen Pfad und Knotenrand gegeben ist.
        //hier nur geradlinige Verbindung zwischen Bends
        PointD portLocation = port.GetLocation();
        PointD seg = new PointD();
        var bends = e.Bends;
        if (bends.Count == 0) {
          // no bends, instead take the endpoint
          seg = source ? e.TargetPort.GetLocation() : e.SourcePort.GetLocation();
        } else {
          IPoint p1 = bends[0].Location;
          IPoint p2 = bends[bends.Count - 1].Location;
          seg = source ? new PointD(p1) : new PointD(p2);
        }
        // Some offset for ports, which lie exactly on the border
        RectD enlarged = portOwner.Layout.ToRectD().GetEnlarged(5);

        var generalPath = new GeneralPath(2);
        generalPath.MoveTo(portLocation);
        generalPath.LineTo(seg);

        if (generalPath.FindLineIntersection(enlarged.TopLeft, enlarged.TopRight) < 1) { 
          //Erstes Segment verlaesst den Knoten auf der Nordseite
          //Die tatsaechliche Position des Constraints ergibt sich aus dem Startpunkt der Kante, muss
          //hier also nicht noch mal angegeben werden, dafuer aber, dass es sich wirklich um einen STRONG constraint
          //handelt.
          return PortConstraint.Create(PortSide.North, strong);
        }
        if (generalPath.FindLineIntersection(enlarged.TopLeft, enlarged.BottomLeft) < 1) { 
          //first segment leaves at west...
          return PortConstraint.Create(PortSide.West, strong);
        }
        if (generalPath.FindLineIntersection(enlarged.TopRight, enlarged.BottomRight) < 1) { 
          //first segment leaves at east...
          return PortConstraint.Create(PortSide.East, strong);
        }
        if (generalPath.FindLineIntersection(enlarged.BottomLeft, enlarged.BottomRight) < 1) { 
          //first segment leaves at south...
          return PortConstraint.Create(PortSide.South, strong);
        }
        //keine intersection mit dem ersten segment, hier waehlen wir den einfachen Weg...
        return PortConstraint.Create(PortSide.Any, strong);
      }
      return null;
    }
  }
}
