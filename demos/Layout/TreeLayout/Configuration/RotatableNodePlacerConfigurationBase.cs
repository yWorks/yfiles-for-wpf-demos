/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree.Configuration
{
  /// <summary>
  ///   Non-generic base interface for <see cref="RotatableSubtreePlacerConfigurationBase{T}"/> that provides the
  ///   <see cref="Transform"/> property
  /// </summary>
  public interface IRotatableSubtreePlacerConfiguration : ISubtreePlacerConfiguration
  {
    SubtreeTransform Transform { get; }

    /// <summary>
    ///   Sets the modification matrix
    /// </summary>
    void SetTransform(SubtreeTransform transform);
  }

  /// <summary>
  ///   Abstract base class for configuration handler for <see cref="ISubtreePlacer"/> implementations that are
  ///   transformable using a <see cref="SubtreeTransform"/>.
  /// </summary>
  public abstract class RotatableSubtreePlacerConfigurationBase<T> : IRotatableSubtreePlacerConfiguration
      where T : ISubtreePlacer
  {
    public virtual double Spacing { get; set; }

    public virtual ISubtreePlacer CreateSubtreePlacer() {
      var placer = CreatePlacerCore();
      ConfigurePlacer(placer);
      return placer;
    }

    public void AdoptSettings(ISubtreePlacer subtreePlacer) {
      AdoptSettings((T) subtreePlacer);
    }

    /// <summary>
    ///   Create the placer - this cannot be done here since we need to specify the
    ///   <see cref="Transform">transformation matrix</see> in the placer's constructor.
    /// </summary>
    protected abstract T CreatePlacerCore();

    /// <summary>
    ///   Adopts the settings from the given subtree placer
    /// </summary>
    public abstract void AdoptSettings(T subtreePlacer);

    public SubtreeTransform Transform { get; set; }

    public void SetTransform(SubtreeTransform transform) {
      Transform = transform;
    }

    /// <summary>
    ///   Transfers the transformation settings to the placer
    /// </summary>
    protected abstract void ConfigurePlacer(T placer);
  }
}
