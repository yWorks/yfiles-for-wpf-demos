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

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  /// Specifies the orientation of the drawing.
  /// </summary>
  /// <seealso cref="BpmnLayout.LayoutOrientation"/>
  public enum LayoutOrientation : sbyte
  {
    /// <summary>
    /// Layout will be oriented from top to bottom.
    /// </summary>
    /// <seealso cref="BpmnLayout.LayoutOrientation"/>
    TopToBottom = 0,

    /// <summary>
    /// Layout will be oriented from left to right.
    /// </summary>
    /// <seealso cref="BpmnLayout.LayoutOrientation"/>
    LeftToRight = 1
  }

  /// <summary>
  /// Specifies the scope of the <see cref="BpmnLayout"/>.
  /// </summary>
  /// <seealso cref="BpmnLayout.Scope"/>
  public enum Scope : sbyte
  {
    /// <summary>
    /// Consider all elements during the layout.
    /// </summary>
    /// <seealso cref="BpmnLayout.Scope"/>
    AllElements = 0,

    /// <summary>
    /// Consider only selected elements.
    /// <p>
    /// The selection state of an edge is determined by a boolean value returned by the data provider associated
    /// with the data provider key <see cref="LayoutKeys.AffectedEdgesDpKey"/>.<br/>
    /// The selection state of a node is determined by a boolean value returned by the data provider associated
    /// with the data provider key <see cref="LayoutKeys.AffectedNodesDpKey"/>.
    /// </p>
    /// <p>
    /// Note, that non-selected elements may also be moved to
    /// produce valid drawings. However the layout algorithm uses the initial position of such elements as sketch.
    /// </p>
    /// </summary>
    /// <seealso cref="BpmnLayout.Scope"/>
    SelectedElements = 1
  }
}