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

using Demo.yFiles.Option.DataBinding;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree.Configuration
{
  /// <summary>
  ///   Configuration handler for <see cref="SimpleSubtreePlacer"/>
  /// </summary>
  /// <remarks>This implementation maps directly to <see cref="SingleLayerSubtreePlacer"/> properties.</remarks>
  public sealed class
      SingleLayerSubtreePlacerConfiguration : RotatableSubtreePlacerConfigurationBase<SingleLayerSubtreePlacer>
  {
    public SingleLayerSubtreePlacerConfiguration() {
      AdoptSettings(new SingleLayerSubtreePlacer());
    }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 1)]
    public SingleLayerSubtreePlacerRootAlignment RootAlignment { get; set; }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 2)]
    public SingleLayerSubtreePlacerRoutingStyle RoutingStyle { get; set; }

    #region ISubtreePlacerConfiguration Members

    public override void AdoptSettings(SingleLayerSubtreePlacer nodePlacer) {
      Transform = nodePlacer.Transformation;
      RoutingStyle = nodePlacer.EdgeRoutingStyle;
      Spacing = nodePlacer.HorizontalDistance;
      RootAlignment = nodePlacer.RootAlignment;
    }

    protected override SingleLayerSubtreePlacer CreatePlacerCore() {
      return new SingleLayerSubtreePlacer { Transformation = Transform };
    }

    protected override void ConfigurePlacer(SingleLayerSubtreePlacer placer) {
      placer.EdgeRoutingStyle = RoutingStyle;
      placer.HorizontalDistance = Spacing;
      placer.VerticalDistance = Spacing;
      placer.RootAlignment = RootAlignment;
    }

    #endregion
  }
}
