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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yWorks.Demo.GraphAnalysis
{
  internal sealed record StructureInfo(string Name, bool Matches, string Url);

  internal sealed class Settings : INotifyPropertyChanged
  {
    private Algorithm? sample;
    private Algorithm? algorithm;
    private int weightIndex;
    private int directionIndex;
    private int nodeCount;
    private int edgeCount;
    private List<StructureInfo> structures = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<Algorithm> Algorithms { get; } = AlgorithmCatalog.Algorithms;

    public Algorithm? Sample {
      get => sample;
      set => Set(ref sample, value);
    }

    public Algorithm? Algorithm {
      get => algorithm;
      set {
        if (Set(ref algorithm, value)) {
          Changed(nameof(SupportsWeights));
          Changed(nameof(SupportsDirection));
          Changed(nameof(DescriptionParagraphs));
        }
      }
    }

    public int WeightIndex {
      get => weightIndex;
      set => Set(ref weightIndex, value);
    }

    public int DirectionIndex {
      get => directionIndex;
      set => Set(ref directionIndex, value);
    }

    public bool SupportsWeights => Algorithm?.SupportsEdgeWeights == true;

    public bool SupportsDirection => Algorithm?.SupportsDirectedness == true && !Algorithm.DirectedOnly;

    public int NodeCount {
      get => nodeCount;
      set => Set(ref nodeCount, value);
    }

    public int EdgeCount {
      get => edgeCount;
      set => Set(ref edgeCount, value);
    }

    public List<StructureInfo> Structures {
      get => structures;
      set => Set(ref structures, value);
    }

    public IReadOnlyList<string> DescriptionParagraphs => Algorithm?.Description ?? Array.Empty<string>();

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return false;
      }

      field = value;
      Changed(name);
      return true;
    }

    private void Changed(string? name) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
