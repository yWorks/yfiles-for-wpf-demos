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

using System;
using System.ComponentModel;
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Handler;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree.Configuration
{
  /// <summary>
  ///   Configuration handler for <see cref="AssistantSubtreePlacer"/>
  /// </summary>
  /// <remarks>This implementation provides the possibility to change and configure the child handler, too</remarks>
  public sealed class AssistantPlacerConfiguration : RotatableSubtreePlacerConfigurationBase<AssistantSubtreePlacer>
  {
    /// <summary>
    ///   Symbolic enumeration for the valid child placer types
    /// </summary>
    /// <remarks>
    ///   This is a subset of all valid placer configurations (especially, <see cref="AssistantPlacerConfiguration"/> is
    ///   excluded).
    /// </remarks>
    public enum ChildSubtreePlacerTypes
    {
      SingleLayerSubtreePlacer,
      BusPlacer,
      DoubleLayerPlacer,
      LeftRightPlacer,
      ARSubtreePlacer
    }

    public AssistantPlacerConfiguration() {
      AdoptSettings(new AssistantSubtreePlacer());
    }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 0)]
    public override double Spacing { get; set; }

    [OptionItemAttribute(Name = OptionItem.CustomDialogitemEditor, Value = "NestedChildSubtreePlacer.OptionItemPresenter")]
    [OptionItemAttribute(Name = "OptionItem.Index", Value = 2)]
    [Description("Child Placer")]
    public ISubtreePlacerConfiguration ChildSubtreePlacer { get; set; }

    private ChildSubtreePlacerTypes type;

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 1)]
    public ChildSubtreePlacerTypes Type {
      get => type;
      set {
        if (type != value) {
          type = value;
          ChildSubtreePlacer = UpdateChildPlacer();
        }
      }
    }

    /// <summary>
    ///   Update the child placer configuration when the child placer type has been changed
    /// </summary>
    private ISubtreePlacerConfiguration UpdateChildPlacer() {
      switch (Type) {
        case ChildSubtreePlacerTypes.SingleLayerSubtreePlacer:
          return SubtreePlacerConfigurations.SingleLayerSubtreePlacer.Configuration;
        case ChildSubtreePlacerTypes.BusPlacer:
          return SubtreePlacerConfigurations.BusPlacer.Configuration;
        case ChildSubtreePlacerTypes.DoubleLayerPlacer:
          return SubtreePlacerConfigurations.DoubleLayerPlacer.Configuration;
        case ChildSubtreePlacerTypes.LeftRightPlacer:
          return SubtreePlacerConfigurations.LeftRightPlacer.Configuration;
        case ChildSubtreePlacerTypes.ARSubtreePlacer:
          return SubtreePlacerConfigurations.ArSubtreePlacer.Configuration;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    #region ISubtreePlacerConfiguration Members

    protected override void ConfigurePlacer(AssistantSubtreePlacer placer) {
      placer.Spacing = Spacing;
      placer.ChildSubtreePlacer = ChildSubtreePlacer.CreateSubtreePlacer();
    }

    protected override AssistantSubtreePlacer CreatePlacerCore() {
      return new AssistantSubtreePlacer { Transformation = Transform };
    }


    public override void AdoptSettings(AssistantSubtreePlacer subtreePlacer) {
      Spacing = subtreePlacer.Spacing;
      Transform = subtreePlacer.Transformation;
      // Configure the child placer configuration handler
      // Create a default configuration
      var childSubtreePlacer = subtreePlacer.ChildSubtreePlacer;
      if (childSubtreePlacer is SingleLayerSubtreePlacer) {
        ChildSubtreePlacer = new SingleLayerSubtreePlacerConfiguration();
        Type = ChildSubtreePlacerTypes.SingleLayerSubtreePlacer;
      }
      if (childSubtreePlacer is BusSubtreePlacer) {
        ChildSubtreePlacer = new BusPlacerConfiguration();
        Type = ChildSubtreePlacerTypes.BusPlacer;
      }
      if (childSubtreePlacer is DoubleLayerSubtreePlacer) {
        ChildSubtreePlacer = new DoubleLayerPlacerConfiguration();
        Type = ChildSubtreePlacerTypes.DoubleLayerPlacer;
      }
      if (childSubtreePlacer is AspectRatioSubtreePlacer) {
        ChildSubtreePlacer = new ARSubtreePlacerConfiguration();
        Type = ChildSubtreePlacerTypes.ARSubtreePlacer;
      }
      if (childSubtreePlacer is LeftRightSubtreePlacer) {
        ChildSubtreePlacer = new LeftRightPlacerConfiguration();
        Type = ChildSubtreePlacerTypes.LeftRightPlacer;
      }
      // And adopt the current values
      ChildSubtreePlacer.AdoptSettings(childSubtreePlacer);
    }

    #endregion
  }
}
