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
using System.Linq;
using yWorks.Graph;

namespace yWorks.Demo.GraphAnalysis
{
  /// <summary>
  ///   Provides an undo engine that can ignore temporary analysis presentation changes.
  /// </summary>
  internal sealed class AnalysisGraph : DefaultGraph
  {
    protected override UndoEngine CreateUndoEngine() => new AnalysisUndoEngine(this);

    internal IDisposable SuspendRecording() => ((AnalysisUndoEngine) this.GetUndoEngine()!).SuspendRecording();
  }

  /// <summary>
  ///   Analysis results are derived from the graph. They must not replace a user's undo or redo operation.
  /// </summary>
  internal sealed class AnalysisUndoEngine : UndoEngine
  {
    private readonly IGraph graph;
    private int suspensionCount;

    internal AnalysisUndoEngine(IGraph graph) {
      this.graph = graph;
    }

    public override void Undo() {
      if (CanUndo()) {
        RemoveResultLabels();
      }

      base.Undo();
    }

    public override void Redo() {
      if (CanRedo()) {
        RemoveResultLabels();
      }

      base.Redo();
    }

    private void RemoveResultLabels() {
      // History replay can remove owners of labels recreated by a previous analysis.
      using var presentation = SuspendRecording();
      foreach (var label in graph.GetNodeLabels().Concat(graph.GetEdgeLabels().Where(l => !Equals(l.Tag, "weight")))
          .ToArray()) {
        graph.Remove(label);
      }
    }

    internal IDisposable SuspendRecording() {
      suspensionCount++;
      return new RecordingSuspension(this);
    }

    public override void AddUnit(IUndoUnit unit) {
      if (suspensionCount > 0) {
        unit.Dispose();
      } else {
        base.AddUnit(unit);
      }
    }

    private sealed class RecordingSuspension : IDisposable
    {
      private AnalysisUndoEngine? owner;

      internal RecordingSuspension(AnalysisUndoEngine engine) {
        owner = engine;
      }

      public void Dispose() {
        if (owner == null) {
          return;
        }

        owner.suspensionCount--;
        owner = null;
      }
    }
  }
}
