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

using System.Windows.Media;

namespace Demo.yFiles.Complete.IsometricDrawing.Model
{
  /// <summary>
  /// Data object that shall be represented by a node.
  /// </summary>
  public class NodeData
  {
    private Color color;

    /// <summary>
    /// The ID used by <see cref="EdgeData"/> to define the <see cref="EdgeData.From">source</see> and
    /// <see cref="EdgeData.To">target</see> node as well as to define the <see cref="Group">parent node</see>.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The base color of the node.
    /// </summary>
    /// <remarks>
    /// The <see cref="Brush"/> is automatically updated to use this color.
    /// </remarks>
    /// <seealso cref="Brush"/>
    public Color Color {
      get { return color; }
      set {
        color = value;
        Brush = color != null ? new SolidColorBrush(color) : null;
      }
    }

    /// <summary>
    /// The brush using the <see cref="Color"/>.
    /// </summary>
    public SolidColorBrush Brush { get; private set; }

    /// <summary>
    /// The pen used for the outline of the faces.
    /// </summary>
    public Pen Pen { get; set; }

    /// <summary>
    /// The geometry of the node.
    /// </summary>
    public Geometry Geometry { get; set; }
    
    /// <summary>
    /// The text that should be used as label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// The <see cref="Id"/> of the parent group node. 
    /// </summary>
    public string Group { get; set; }
  }
}
