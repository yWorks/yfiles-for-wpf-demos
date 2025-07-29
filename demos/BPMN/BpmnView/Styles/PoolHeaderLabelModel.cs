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
using System.Collections.Generic;
using System.Reflection;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Bpmn.Styles
{
  /// <summary>
  ///   A label model for nodes using a <see cref="PoolNodeStyle"/> that positions labels inside the
  ///   table <see cref="ITable.Padding"/>.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class PoolHeaderLabelModel : ILabelModel
  {
    #region Initialize static fields

    /// <summary>
    ///   The <see cref="PoolHeaderLabelModel"/> singleton.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly PoolHeaderLabelModel Instance = new();

    /// <summary>
    ///   A parameter instance using the top padding of the pool node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly PoolHeaderParameter Top = new(0);

    /// <summary>
    ///   A parameter instance using the right padding of the pool node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly PoolHeaderParameter Right = new(1);

    /// <summary>
    ///   A parameter instance using the bottom padding of the pool node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly PoolHeaderParameter Bottom = new(2);

    /// <summary>
    ///   A parameter instance using the left padding of the pool node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly PoolHeaderParameter Left = new(3);

    private static readonly IEnumerable<ILabelModelParameter> parameters = new[] { Top, Right, Bottom, Left };

    #endregion


    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      if (parameter is not PoolHeaderParameter php) {
        throw new ArgumentException("The parameter is not supported by this label model.", nameof(parameter));
      }
      if (!DefaultGraph.HasOwner(label)) {
        throw new ArgumentException("The label has no valid owner.", nameof(label));
      }
      if (label.Owner is not INode node) {
        throw new ArgumentException("This model only supports node labels.", nameof(label));
      }

      var table = node.Lookup<ITable>();
      var padding = table != null && table.Padding != InsetsD.Empty ? table.Padding : new InsetsD(0);

      var orientedRectangle = new OrientedRectangle();
      orientedRectangle.Size = label.PreferredSize;
      switch (php.Side) {
        case 0: // Top
          orientedRectangle.SetUpVector(0, -1);
          orientedRectangle.SetCenter(new PointD(node.Layout.X + node.Layout.Width / 2,
              node.Layout.Y + padding.Top / 2));
          break;
        case 1: // Right
          orientedRectangle.SetUpVector(1, 0);
          orientedRectangle.SetCenter(new PointD(node.Layout.GetMaxX() - padding.Right / 2,
              node.Layout.Y + node.Layout.Height / 2));
          break;
        case 2: // Bottom
          orientedRectangle.SetUpVector(0, -1);
          orientedRectangle.SetCenter(new PointD(node.Layout.X + node.Layout.Width / 2,
              node.Layout.GetMaxY() - padding.Bottom / 2));
          break;
        case 3: // Left
        default:
          orientedRectangle.SetUpVector(-1, 0);
          orientedRectangle.SetCenter(new PointD(node.Layout.X + padding.Left / 2,
              node.Layout.Y + node.Layout.Height / 2));
          break;
      }

      return orientedRectangle;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILookup GetContext(ILabel label) {
      return new PoolHeaderLabelModelLookup();
    }

    private sealed class PoolHeaderLabelModelLookup : ILookup, ILabelModelParameterProvider
    {
      public object Lookup(Type type) {
        return type == typeof(ILabelModelParameterProvider) ? this : null;
      }

      /// <inheritdoc/>
      public IEnumerable<ILabelModelParameter> GetParameters() {
        return parameters;
      }
    }

    [GraphML(SingletonContainers = new[] { typeof(PoolHeaderLabelModel) })]
    public class PoolHeaderParameter : ILabelModelParameter
    {
      public byte Side { get; }

      public PoolHeaderParameter(byte side) {
        Side = side;
      }

      public object Clone() {
        return this;
      }

      public ILabelModel Model => Instance;
    }
  }
}
