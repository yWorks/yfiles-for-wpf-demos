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

using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.Base.RandomGraphGenerator;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.GraphEditor.UI
{
  internal class RandomGraphGeneratorModule : IGraphModule
  {
    public RandomGraphGeneratorModule() : base(NAME) {}

    public const string NAME = "GENERATOR";
    public const string NumberOfNodes = "NumberOfNodes";
    public const string NumberOfEdges = "NumberOfEdges";
    public const string AllowCycles = "AllowCycles";
    public const string AllowSelfloops = "AllowSelfloops";
    public const string AllowMultipleEdges = "AllowMultipleEdges";

    protected override Task RunModule() {
      RandomGraphGenerator rg = new RandomGraphGenerator
      {
        NodeCount = (int) Handler[NumberOfNodes].Value,
        EdgeCount = (int) Handler[NumberOfEdges].Value,
        AllowCycles = (bool) Handler[AllowCycles].Value,
        AllowMultipleEdges = (bool) Handler[AllowMultipleEdges].Value,
        AllowSelfLoops = (bool) Handler[AllowSelfloops].Value
      };

      rg.Generate(CurrentIGraph);
      int i = 0;
      foreach (INode node in CurrentIGraph.Nodes) {
        CurrentIGraph.AddLabel(node, "" + ++i);
      }
      RandomizeGraph();
      GraphControl.Invalidate();
      return Task.FromResult<object>(null);
    }

    private void RandomizeGraph() {
      // Remove all bends
      var bends = CurrentIGraph.Edges.SelectMany(e => e.Bends).ToList();
      foreach (var bend in bends) {
        CurrentIGraph.Remove(bend);
      }
      var r = new Random();
      // Place nodes in random locations
      foreach (var node in CurrentIGraph.Nodes) {
        CurrentIGraph.SetNodeCenter(node,
            new PointD(r.Next((int) GraphControl.Viewport.MinX, (int) GraphControl.Viewport.MaxX),
                r.Next((int) GraphControl.Viewport.MinY, (int) GraphControl.Viewport.MaxY)));
      }
      GraphControl.UpdateContentRect();
    }


    protected override void SetupHandler() {
      RandomGraphGenerator rg = new RandomGraphGenerator();
      Handler.AddInt(NumberOfNodes, rg.NodeCount);
      Handler.AddInt(NumberOfEdges, rg.EdgeCount);

      Handler.AddBool(AllowCycles, rg.AllowCycles);
      Handler.AddBool(AllowSelfloops, rg.AllowSelfLoops);
      Handler.AddBool(AllowMultipleEdges, rg.AllowMultipleEdges);
    }
  }
}
