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
using System.Reflection;
using System.Windows.Markup;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml
{
  public class NinePositionsEdgeLabelModelParameterExtension : MarkupExtension
  {
    public ILabelModel Model { get; set; }
    public NinePositionsEdgeLabelModelPosition Position { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      if ((Position & NinePositionsEdgeLabelModelPosition.SourceCenterTargetMask) == 0) {
        Position |= NinePositionsEdgeLabelModelPosition.Center;
      }
      return new yWorks.Markup.Common.NinePositionsEdgeLabelModelParameterExtension {
        Model = Model,
        Position = (NinePositionsEdgeLabelModel.Position) Position
      }.ProvideValue(serviceProvider);
    }
  }

  [Flags]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public enum NinePositionsEdgeLabelModelPosition
  {
    /// <summary>
    /// Encodes a label position at the source above the edge
    /// </summary>
    SourceAbove = 0x11,

    /// <summary>
    /// Encodes a label position at the center above the edge
    /// </summary>
    CenterAbove = 0x14,

    /// <summary>
    /// Encodes a label position at the target above the edge
    /// </summary>
    TargetAbove = 0x12,

    /// <summary>
    /// Encodes a label position at the source of the edge that lies on the edge
    /// </summary>
    SourceCentered = 0x41,

    /// <summary>
    /// Encodes a label position at the center of the edge that lies on the edge
    /// </summary>
    CenterCentered = 0x44,

    /// <summary>
    /// Encodes a label position at the source of the edge that lies on the edge
    /// </summary>
    TargetCentered = 0x42,

    /// <summary>
    /// Encodes a label position at the source below the edge
    /// </summary>
    SourceBelow = 0x21,

    /// <summary>
    /// Encodes a label position at the center below the edge
    /// </summary>
    CenterBelow = 0x24,

    /// <summary>
    /// Encodes a label position at the target below the edge
    /// </summary>
    TargetBelow = 0x22,

    /// <summary>
    /// Mask value for label positions above, below or directly on the edge
    /// </summary>
    AboveCenteredBelowMask = 0x70,

    /// <summary>
    /// Mask value for label positions at the source, the center or the target of
    /// the edge
    /// </summary>
    SourceCenterTargetMask = 0x07,

    /// <summary>
    /// Encodes a label position above, below or on the edge at the source
    /// </summary>
    Source = 0x01,
    /// <summary>
    /// Encodes a label position above, below or on the edge at the center of the edge
    /// </summary>
    Center = 0x04,
    /// <summary>
    /// Encodes a label position above, below or on the edge at the target
    /// </summary>
    Target = 0x02,

    /// <summary>
    /// Encodes a label position above the edge at the source, center or target
    /// </summary>
    Above = 0x10,
    /// <summary>
    /// Encodes a label position directly on the edge at the source, center or target
    /// </summary>
    Centered = 0x40,
    /// <summary>
    /// Encodes a label position below the edge at the source, center or target
    /// </summary>
    Below = 0x20,
  }

}