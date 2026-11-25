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

using yWorks.Layout;

namespace yWorks.Demo.NodeAlignment
{
  /// <summary>
  ///   Creates and configures the alignment layout algorithm from the demo's settings.
  /// </summary>
  internal static class ConfigureLayout
  {
    /// <summary>
    ///   Creates a new instance of the <see cref="AlignmentStage"/> layout algorithm with the given settings.
    /// </summary>
    public static AlignmentStage CreateConfiguredLayoutAlgorithm(Settings settings) =>
        new() {
            AlignmentPolicy = settings.AlignmentPolicy,
            MinimumNodeDistance = settings.MinimumNodeDistance,
            SnapDistance = settings.SnapDistance,
            SeparateStripes = settings.SeparateStripes
        };

    /// <summary>
    ///   Creates settings with the default values of the <see cref="AlignmentStage"/> layout algorithm.
    /// </summary>
    public static Settings CreateDefaultSettings() => new(new AlignmentStage());
  }
}
