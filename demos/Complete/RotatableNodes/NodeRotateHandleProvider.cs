/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.ComponentModel;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// Provides a rotate handle for a given node
  /// </summary>
  public class NodeRotateHandleProvider : IHandleProvider
  {
    private readonly INode node;
    private readonly IReshapeHandler reshapeHandler;

    /// <summary>
    /// Creates a new instance for the given node.
    /// </summary>
    /// <param name="node">The node to handle.</param>
    public NodeRotateHandleProvider(INode node) {
      this.node = node;
      reshapeHandler = node.Lookup<IReshapeHandler>();
      SnapStep = 45;
      SnapDelta = 10;
      SnapToSameAngleDelta = 5;
    }

    /// <summary>
    /// Gets or sets the angular step size to which rotation should snap (in degrees).
    /// </summary>
    /// <remarks>
    /// Default is 45. Setting this to zero will disable snapping to predefined steps.
    /// </remarks>
    [DefaultValue(45.0)]
    public double SnapStep { get; set; }

    /// <summary>
    /// Gets or sets the snapping distance when rotation should snap (in degrees).
    /// </summary>
    /// <remarks>
    /// The rotation will snap if the angle is less than this distance from a <see cref="SnapStep">snapping angle</see>.
    /// Default is 10.
    /// <para>
    /// Setting this to a non-positive value will disable snapping to predefined steps.
    /// </para>
    /// </remarks>
    [DefaultValue(10.0)]
    public double SnapDelta { get; set; }

    /// <summary>
    /// Gets or sets the snapping distance (in degrees) for snapping to the same angle as other visible nodes.
    /// </summary>
    /// <remarks>
    /// Rotation will snap to another node's rotation angle if the current angle differs from the other one by less than this.
    /// The default is 5.
    /// <para>
    /// Setting this to a non-positive value will disable same angle snapping.
    /// </para>
    /// </remarks>
    [DefaultValue(5.0)]
    public double SnapToSameAngleDelta { get; set; }

    /// <summary>
    /// Returns a set of handles for the rotated node.
    /// </summary>
    public IEnumerable<IHandle> GetHandles(IInputModeContext inputModeContext) {
      return new IHandle[] {
        new NodeRotateHandle(node, reshapeHandler, inputModeContext) {
          SnapDelta = SnapDelta,
          SnapStep = SnapStep,
          SnapToSameAngleDelta = SnapToSameAngleDelta
        }
      };
    }
  }
}
