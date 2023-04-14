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
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree.Configuration
{
  /// <summary>
  /// Non generic base interface for <see cref="RotatableNodePlacerConfigurationBase{T}"/> that provides the <see cref="ModificationMatrix"/> property
  /// </summary>
  public interface IRotatableNodePlacerConfiguration : INodePlacerConfiguration
  {
    RotatableNodePlacerBase.Matrix ModificationMatrix { get; }

    /// <summary>
    /// Sets the modification matrix
    /// </summary>
    void SetModificationMatrix(RotatableNodePlacerBase.Matrix matrix);
  }

  /// <summary>
  /// Abstract base class for configuration handler for <see cref="AbstractRotatableNodePlacer"/> implementations.
  /// </summary>
  public abstract class RotatableNodePlacerConfigurationBase<T> : IRotatableNodePlacerConfiguration
    where T : RotatableNodePlacerBase, new()
  {
    /// <summary>
    /// Symbolic enumeration for the <see cref="AbstractRotatableNodePlacer.RootAlignment"/> class members.
    /// </summary>
    public enum RotatingRootAlignment
    {
      Leading,
      Center,
      CenterOverChildren,
      Trailing,
      Left,
      Right,
      Median
    }

    protected static readonly IDictionary<RotatingRootAlignment, RotatableNodePlacerBase.RootAlignment> Alignments =
      new Dictionary<RotatingRootAlignment, RotatableNodePlacerBase.RootAlignment>();

    static RotatableNodePlacerConfigurationBase() {
      Alignments[RotatingRootAlignment.Center] = RotatableNodePlacerBase.RootAlignment.Center;
      Alignments[RotatingRootAlignment.CenterOverChildren] =
        RotatableNodePlacerBase.RootAlignment.CenterOverChildren;
      Alignments[RotatingRootAlignment.Leading] = RotatableNodePlacerBase.RootAlignment.Leading;
      Alignments[RotatingRootAlignment.Left] = RotatableNodePlacerBase.RootAlignment.Left;
      Alignments[RotatingRootAlignment.Median] = RotatableNodePlacerBase.RootAlignment.Median;
      Alignments[RotatingRootAlignment.Right] = RotatableNodePlacerBase.RootAlignment.Right;
      Alignments[RotatingRootAlignment.Trailing] = RotatableNodePlacerBase.RootAlignment.Trailing;
    }

    public virtual double Spacing { get; set; }

    public virtual INodePlacer CreateNodePlacer() {
      var placer = CreatePlacerCore();
      ConfigurePlacer(placer);
      return placer;
    }

    /// <summary>
    /// Create the placer - this cannot be done here since we need to specify the <see cref="ModificationMatrix"/> in the placer's constructor.
    /// </summary>
    protected abstract T CreatePlacerCore();

    public virtual void AdoptSettings(INodePlacer nodePlacer) {
      var dnp = (T) nodePlacer;
      Spacing = dnp.Spacing;
      modificationMatrix = dnp.ModificationMatrix;
    }

    private RotatableNodePlacerBase.Matrix modificationMatrix;

    public RotatableNodePlacerBase.Matrix ModificationMatrix {
      get { return modificationMatrix; }
    }

    public void SetModificationMatrix(RotatableNodePlacerBase.Matrix matrix) {
      modificationMatrix = matrix;
    }

    protected virtual void ConfigurePlacer(T placer) {
      placer.Spacing = Spacing;
    }
  }
}