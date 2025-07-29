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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Markup;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Bpmn.Styles
{
  /// <summary>
  ///   A label model for nodes using a <see cref="ChoreographyNodeStyle"/> that positions labels on the participant or task
  ///   name bands.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class ChoreographyLabelModel : ILabelModel
  {
    #region Initialize static fields

    /// <summary>
    ///   The <see cref="ChoreographyLabelModel"/> singleton.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ChoreographyLabelModel Instance;

    /// <summary>
    ///   A singleton for labels placed centered on the task name band.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly TaskNameBandParameter TaskNameBand;

    /// <summary>
    ///   A singleton for message labels placed at the top of the node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly MessageParameter TopMessage;

    /// <summary>
    ///   A singleton for message labels placed at the bottom of the node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly MessageParameter BottomMessage;

    //private static readonly InteriorNodeLabelModel interiorModel = new InteriorNodeLabelModel();
    private static readonly StretchNodeLabelModel interiorStretchModel = new() { Padding = new InsetsD(3, 3, 3, 3) };

    private static readonly SimpleNode dummyNode = new();
    private static readonly SimpleLabel dummyLabel = new(dummyNode, "", StretchNodeLabelModel.Center);

    static ChoreographyLabelModel() {
      Instance = new ChoreographyLabelModel();
      TaskNameBand = new TaskNameBandParameter();
      TopMessage = new MessageParameter(true);
      BottomMessage = new MessageParameter(false);
    }

    #endregion

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      if (parameter is not ChoreographyParameter choreographyParameter) {
        throw new ArgumentException("The parameter is not supported by this model.", nameof(parameter));
      }
      if (!DefaultGraph.HasOwner(label)) {
        throw new ArgumentException("The label has no valid owner.", nameof(label));
      }
      if (label.Owner is not INode node) {
        throw new ArgumentException("This model only supports node labels.", nameof(label));
      }
      if (node.Style is ChoreographyNodeStyle) {
        return choreographyParameter.GetGeometry(label);
      }
      var layout = node.Layout;
      return new OrientedRectangle(layout.X, layout.Y + layout.Height, layout.Width, layout.Height);
    }

    /// <summary>
    ///   Creates the parameter for the participant at the given position.
    /// </summary>
    /// <param name="top">
    ///   Whether the index refers to <see cref="ChoreographyNodeStyle.TopParticipants"/> or
    ///   <see cref="ChoreographyNodeStyle.BottomParticipants"/>.
    /// </param>
    /// <param name="index">The index of the participant band the label shall be placed in.</param>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ParticipantParameter CreateParticipantParameter(bool top, int index) {
      return new ParticipantParameter(top, index);
    }

    /// <summary>
    ///   Determines if these two parameters are equal.
    /// </summary>
    public static bool GetEqualParameters(ILabelModelParameter parameter1, ILabelModelParameter parameter2) {
      if (parameter1 is ParticipantParameter && parameter2 is ParticipantParameter) {
        if (((ParticipantParameter) parameter1).Index == ((ParticipantParameter) parameter2).Index &&
            ((ParticipantParameter) parameter1).Top == ((ParticipantParameter) parameter2).Top) {
          return true;
        }
      }

      if (parameter1 is TaskNameBandParameter && parameter2 is TaskNameBandParameter) {
        return true;
      }

      if (parameter1 is MessageParameter && parameter2 is MessageParameter) {
        if (((MessageParameter) parameter1).Top == ((MessageParameter) parameter2).Top) {
          return true;
        }
      }

      return false;
    }

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILookup GetContext(ILabel label) {
      return new ChoreographyLabelModelLookup(label, this);
    }

    /// <summary>
    ///   Returns an enumerator over a set of possible <see cref="ILabelModelParameter"/>
    ///   instances that can be used for the given label.
    /// </summary>
    /// <param name="label">The label instance to use.</param>
    /// <returns>A possibly empty enumerator over a set of label model parameters.</returns>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IEnumerable<ILabelModelParameter> GetParameters(ILabel label) {
      var parameters = new List<ILabelModelParameter>();
      var node = label.Owner as INode;
      if (node != null && node.Style is ChoreographyNodeStyle) {
        var nodeStyle = (ChoreographyNodeStyle) node.Style;
        for (var i = 0; i < nodeStyle.TopParticipants.Count; i++) {
          parameters.Add(CreateParticipantParameter(true, i));
        }
        parameters.Add(TaskNameBand);
        for (var i = 0; i < nodeStyle.BottomParticipants.Count; i++) {
          parameters.Add(CreateParticipantParameter(false, i));
        }
        parameters.Add(TopMessage);
        parameters.Add(BottomMessage);
      }

      return parameters;
    }

    [CanBeNull]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILabelModelParameter FindNextParameter([NotNull] INode node) {
      var nodeStyle = node.Style as ChoreographyNodeStyle;
      if (nodeStyle != null) {
        var taskNameBandCount = 1;
        var topParticipantCount = nodeStyle.TopParticipants.Count;
        var bottomParticipantCount = nodeStyle.BottomParticipants.Count;
        var messageCount = 2;

        var parameterTaken = new bool[taskNameBandCount + topParticipantCount + bottomParticipantCount + messageCount];

        // check which label positions are already taken
        foreach (var label in node.Labels) {
          var parameter = label.LayoutParameter as ChoreographyParameter;
          if (parameter != null) {
            var index = 0;
            if (!(parameter is TaskNameBandParameter)) {
              index++;

              if (parameter is ParticipantParameter) {
                var pp = parameter as ParticipantParameter;
                if (!pp.Top) {
                  index += topParticipantCount;
                }
                index += pp.Index;
              } else {
                index += topParticipantCount + bottomParticipantCount;
                if (!((MessageParameter) parameter).Top) {
                  index++;
                }
              }
            }
            parameterTaken[index] = true;
          }
        }

        // get first label position that isn't taken already
        for (var i = 0; i < parameterTaken.Length; i++) {
          if (!parameterTaken[i]) {
            if (i < taskNameBandCount) {
              return TaskNameBand;
            }
            i -= taskNameBandCount;
            if (i < topParticipantCount) {
              return CreateParticipantParameter(true, i);
            }
            i -= topParticipantCount;
            if (i < bottomParticipantCount) {
              return CreateParticipantParameter(false, i);
            }
            i -= bottomParticipantCount;
            return i == 0 ? TopMessage : BottomMessage;
          }
        }
      }
      return null;
    }

    #region Parameters

    public abstract class ChoreographyParameter : ILabelModelParameter
    {
      public ILabelModel Model => Instance;

      public abstract IOrientedRectangle GetGeometry(ILabel label);

      public abstract object Clone();
    }

    [TypeConverter(typeof(ParticipantParameterConverter))]
    public sealed class ParticipantParameter : ChoreographyParameter
    {
      private static readonly InteriorNodeLabelModel ilm = new() { Padding = new InsetsD(3) };
      private static readonly ILabelModelParameter placement = ilm.CreateParameter(InteriorNodeLabelModel.Position.Top);

      public int Index { get; }

      public bool Top { get; }

      public ParticipantParameter(bool top, int index) {
        Top = top;
        Index = index;
      }

      public override IOrientedRectangle GetGeometry(ILabel label) {
        if (!(label.Owner is INode)) {
          return OrientedRectangle.Empty;
        }
        var node = (INode) label.Owner;
        if (!(node.Style is ChoreographyNodeStyle)) {
          return OrientedRectangle.Empty;
        }
        var style = (ChoreographyNodeStyle) node.Style;
        dummyNode.Layout = style.GetParticipantBandBounds(node, Index, Top);
        dummyLabel.PreferredSize = label.PreferredSize;
        return ilm.GetGeometry(dummyLabel, placement);
      }

      public override object Clone() {
        return this;
      }
    }

    /// <xamlhelper/>
    public sealed class ParticipantParameterConverter : TypeConverter
    {
      /// <xamlhelper/>
      public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
        if (destinationType == typeof(MarkupExtension)) {
          return true;
        }
        return base.CanConvertTo(context, destinationType);
      }

      /// <xamlhelper/>
      public override object ConvertTo(
          ITypeDescriptorContext context,
          CultureInfo culture,
          object value,
          Type destinationType) {
        var participantParameter = value as ParticipantParameter;
        if (destinationType == typeof(MarkupExtension) && participantParameter != null) {
          return new ParticipantLabelModelParameterExtension {
              Index = participantParameter.Index, Top = participantParameter.Top
          };
        }
        return base.ConvertTo(context, culture, value, destinationType);
      }
    }

    [GraphML(SingletonContainers = new[] { typeof(ChoreographyLabelModel) })]
    public sealed class TaskNameBandParameter : ChoreographyParameter
    {
      public override IOrientedRectangle GetGeometry(ILabel label) {
        if (!(label.Owner is INode)) {
          return OrientedRectangle.Empty;
        }
        var node = (INode) label.Owner;
        if (!(node.Style is ChoreographyNodeStyle)) {
          return OrientedRectangle.Empty;
        }
        var style = (ChoreographyNodeStyle) node.Style;
        var bandBounds = style.GetTaskNameBandBounds(node);
        dummyNode.Layout = bandBounds;
        dummyLabel.PreferredSize = label.PreferredSize;
        return interiorStretchModel.GetGeometry(dummyLabel, StretchNodeLabelModel.Center);
      }

      public override object Clone() {
        return this;
      }
    }

    [GraphML(SingletonContainers = new[] { typeof(ChoreographyLabelModel) })]
    public sealed class MessageParameter : ChoreographyParameter
    {
      private static readonly ILabelModelParameter topParameter;
      private static readonly ILabelModelParameter bottomParameter;

      static MessageParameter() {
        var sandwichModel = new CompositeLabelModel();
        var exteriorModel = new ExteriorNodeLabelModel { Margins = new InsetsD(32) };
        topParameter = sandwichModel.AddParameter(exteriorModel.CreateParameter(ExteriorNodeLabelModel.Position.Top));
        bottomParameter =
            sandwichModel.AddParameter(exteriorModel.CreateParameter(ExteriorNodeLabelModel.Position.Bottom));
      }

      public MessageParameter(bool top) {
        Top = top;
      }

      public bool Top { get; }

      public override IOrientedRectangle GetGeometry(ILabel label) {
        var parameter = Top ? topParameter : bottomParameter;
        return parameter.Model.GetGeometry(label, parameter);
      }

      public override object Clone() {
        return MemberwiseClone();
      }
    }

    #endregion

    private sealed class ChoreographyLabelModelLookup : ILookup, ILabelModelParameterProvider
    {
      private readonly ILabel label;
      private readonly ChoreographyLabelModel model;

      public ChoreographyLabelModelLookup(ILabel label, ChoreographyLabelModel model) {
        this.label = label;
        this.model = model;
      }

      public object Lookup(Type type) {
        if (type == typeof(ILabelModelParameterProvider)) {
          return this;
        }
        return InteriorNodeLabelModel.Center.Model.GetContext(label).Lookup(type);
      }

      /// <inheritdoc/>
      [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
      public IEnumerable<ILabelModelParameter> GetParameters() {
        return model.GetParameters(label);
      }
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  [MarkupExtensionReturnType(typeof(ILabelModelParameter))]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class ParticipantLabelModelParameterExtension : MarkupExtension
  {
    /// <xamlhelper/>
    public int Index { get; set; }

    /// <xamlhelper/>
    public bool Top { get; set; }

    /// <xamlhelper/>
    public override object ProvideValue(IServiceProvider serviceProvider) {
      return ChoreographyLabelModel.Instance.CreateParticipantParameter(Top, Index);
    }
  }
}
