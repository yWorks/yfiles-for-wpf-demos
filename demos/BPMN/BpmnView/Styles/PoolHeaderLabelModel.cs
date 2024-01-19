/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Collections.Generic;
using System.Reflection;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// A label model for nodes using a <see cref="PoolNodeStyle"/> that position labels inside the <see cref="ITable.Insets">table insets</see>.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class PoolHeaderLabelModel : ILabelModel, ILabelModelParameterProvider {

    #region Initialize static fields

    /// <summary>
    /// The <see cref="PoolHeaderLabelModel"/> singleton.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly PoolHeaderLabelModel Instance = new PoolHeaderLabelModel();

    /// <summary>
    /// A parameter instance using the north insets of the pool node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ILabelModelParameter North = new PoolHeaderParameter(0);

    /// <summary>
    /// A parameter instance using the east insets of the pool node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ILabelModelParameter East = new PoolHeaderParameter(1);

    /// <summary>
    /// A parameter instance using the south insets of the pool node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ILabelModelParameter South = new PoolHeaderParameter(2);

    /// <summary>
    /// A parameter instance using the west insets of the pool node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ILabelModelParameter West = new PoolHeaderParameter(3);

    private static readonly IEnumerable<ILabelModelParameter> parameters = new[] { North, East, South, West };

    #endregion


    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public virtual object Lookup(Type type) {
      if (type == typeof(ILabelModelParameterProvider)) {
        return this;
      }
      if (type == typeof(ILabelModelParameterFinder)) {
        return DefaultLabelModelParameterFinder.Instance;
      }
      if (type == typeof(ILabelCandidateDescriptorProvider)) {
        return ConstantLabelCandidateDescriptorProvider.InternalDescriptorProvider;
      }
      return null;
    }

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      var php = parameter as PoolHeaderParameter;
      INode owner = (INode)label.Owner;
      if (php == null || owner == null) {
        return null;
      }

      ITable table = owner.Lookup<ITable>();
      InsetsD insets = table != null && table.Insets != InsetsD.Empty ? table.Insets : new InsetsD(0);

      var orientedRectangle = new OrientedRectangle();
      orientedRectangle.Resize(label.PreferredSize);
      switch (php.Side) {
        case 0: // North
          orientedRectangle.SetUpVector(0, -1);
          orientedRectangle.SetCenter(new PointD(owner.Layout.X + owner.Layout.Width/2, owner.Layout.Y + insets.Top/2));
          break;
        case 1: // East
          orientedRectangle.SetUpVector(1, 0);
          orientedRectangle.SetCenter(new PointD(owner.Layout.GetMaxX() - insets.Right/2, owner.Layout.Y + owner.Layout.Height/2));
          break;
        case 2: // South
          orientedRectangle.SetUpVector(0, -1);
          orientedRectangle.SetCenter(new PointD(owner.Layout.X + owner.Layout.Width/2, owner.Layout.GetMaxY() - insets.Bottom/2));
          break;
        case 3: // West
        default:
          orientedRectangle.SetUpVector(-1, 0);
          orientedRectangle.SetCenter(new PointD(owner.Layout.X + insets.Left/2, owner.Layout.Y + owner.Layout.Height/2));
          break;
      }

      return orientedRectangle;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILabelModelParameter CreateDefaultParameter() {
      return West;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return Lookups.Empty;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IEnumerable<ILabelModelParameter> GetParameters(ILabel label, ILabelModel model) {
      return parameters;
    }

    [SingletonSerialization(ContainerTypes = new[] { typeof(PoolHeaderLabelModel) })]
    internal class PoolHeaderParameter : ILabelModelParameter
    {
      private readonly byte side;

      public byte Side {
        get { return side; }
      }

      public PoolHeaderParameter(byte side) {
        this.side = side;
      }

      public object Clone() {
        return this;
      }

      public ILabelModel Model { get { return Instance; } }

      public bool Supports(ILabel label) {
        return label.Owner.Lookup<ITable>() != null;
      }
    }
  }
}
