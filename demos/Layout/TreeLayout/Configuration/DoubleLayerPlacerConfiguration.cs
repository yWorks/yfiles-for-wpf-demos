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
  /// Configuration handler for <see cref="DoubleLayerSubtreePlacer"/>
  /// </summary>
  public sealed class DoubleLayerPlacerConfiguration : RotatableSubtreePlacerConfigurationBase<DoubleLayerSubtreePlacer>
  {
    public DoubleLayerPlacerConfiguration() {
      AdoptSettings(new DoubleLayerSubtreePlacer());
    }

    #region ISubtreePlacerConfiguration Members

    protected override void ConfigurePlacer(DoubleLayerSubtreePlacer placer) {
      placer.Spacing = Spacing;
      placer.RootAlignment = RootAlignment;
    }

    protected override DoubleLayerSubtreePlacer CreatePlacerCore() {
      return new DoubleLayerSubtreePlacer { Transformation = Transform };
    }

    public override void AdoptSettings(DoubleLayerSubtreePlacer subtreePlacer) {
      Spacing = subtreePlacer.Spacing;
      Transform = subtreePlacer.Transformation;
      RootAlignment = subtreePlacer.RootAlignment;
    }

    public SubtreeRootAlignment RootAlignment { get; set; }

    #endregion
  }
}
