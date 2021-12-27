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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.GraphML;
using yWorks.Graph.LabelModels;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// A label model for nodes using a <see cref="ChoreographyNodeStyle"/> that position labels on the participant or task name bands.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class ChoreographyLabelModel : ILabelModel, ILabelModelParameterProvider {

    #region Initialize static fields

    /// <summary>
    /// The <see cref="ChoreographyLabelModel"/> singleton.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ChoreographyLabelModel Instance;

    /// <summary>
    /// A singleton for labels placed centered on the task name band.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ILabelModelParameter TaskNameBand;

    /// <summary>
    /// A singleton for message labels placed north of the node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ILabelModelParameter NorthMessage;

    /// <summary>
    /// A singleton for message labels placed south of the node.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ILabelModelParameter SouthMessage;

    //private static readonly InteriorLabelModel interiorModel = new InteriorLabelModel();
    private static readonly InteriorStretchLabelModel interiorStretchModel = new InteriorStretchLabelModel {Insets = new InsetsD(3, 3, 3, 3)};
    private static readonly SimpleNode dummyNode = new SimpleNode();
    private static readonly SimpleLabel dummyLabel = new SimpleLabel(dummyNode, "", InteriorStretchLabelModel.Center);

    static ChoreographyLabelModel() {
      Instance = new ChoreographyLabelModel();
      TaskNameBand = new TaskNameBandParameter();
      NorthMessage = new MessageParameter {North = true};
      SouthMessage = new MessageParameter {North = false};
    }

    #endregion

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      if (parameter is ChoreographyParameter && label.Owner is INode && ((INode) label.Owner).Style is ChoreographyNodeStyle) {
        return ((ChoreographyParameter) parameter).GetGeometry(label);
      } else if (label.Owner is INode) {
        var layout = ((INode) label.Owner).Layout;
        return new OrientedRectangle(layout.X, layout.Y + layout.Height, layout.Width, layout.Height);
      }
      return OrientedRectangle.Empty;
    }

    /// <summary>
    /// Returns <see cref="TaskNameBand"/> as default parameter.
    /// </summary>
    /// <returns></returns>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILabelModelParameter CreateDefaultParameter() {
      return TaskNameBand;
    }

    /// <summary>
    /// Creates the parameter for the participant at the given position.
    /// </summary>
    /// <param name="top">Whether the index refers to <see cref="ChoreographyNodeStyle.TopParticipants"/> or <see cref="ChoreographyNodeStyle.BottomParticipants"/>.</param>
    /// <param name="index">The index of the participant band the label shall be placed in.</param>
    /// <returns></returns>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILabelModelParameter CreateParticipantParameter(bool top, int index) {
      return new ParticipantParameter(top, index);
    }

    /// <summary>
    /// Determines, if these two parameters are equal.
    /// </summary>
    /// <returns></returns>
    public static bool GetEqualParameters(ILabelModelParameter parameter1, ILabelModelParameter parameter2) {
      if (parameter1 is ParticipantParameter && parameter2 is ParticipantParameter) {
        if (((ParticipantParameter) parameter1).index == ((ParticipantParameter) parameter2).index && 
            ((ParticipantParameter) parameter1).top == ((ParticipantParameter) parameter2).top) {
          return true;
        }
      }

      if (parameter1 is TaskNameBandParameter && parameter2 is TaskNameBandParameter) {
        return true;
      }

      if (parameter1 is MessageParameter && parameter2 is MessageParameter) {
        if (((MessageParameter) parameter1).North == ((MessageParameter) parameter2).North) {
          return true;
        }
      }
      
      return false;
    }

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return InteriorLabelModel.Center.Model.GetContext(label, parameter);
    }

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Lookup(Type type) {
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

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IEnumerable<ILabelModelParameter> GetParameters(ILabel label, ILabelModel model) {
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
        parameters.Add(NorthMessage);
        parameters.Add(SouthMessage);
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
                if (!pp.top) {
                  index += topParticipantCount;
                }
                index += pp.index;
              } else {
                index += topParticipantCount + bottomParticipantCount;
                if (!((MessageParameter) parameter).North) {
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
            return i == 0 ? NorthMessage : SouthMessage;
          }
        }
      }
      return null;
    }

    #region Parameters

    private abstract class ChoreographyParameter : ILabelModelParameter
    {
      public ILabelModel Model { get { return Instance; } }

      public abstract IOrientedRectangle GetGeometry(ILabel label);

      public bool Supports(ILabel label) {
        return label.Owner is INode;
      }

      public abstract object Clone();
    }

    [TypeConverter(typeof(ParticipantParameterConverter))]
    private sealed class ParticipantParameter : ChoreographyParameter
    {
      private static readonly InteriorLabelModel ilm = new InteriorLabelModel { Insets = new InsetsD(3) };
      private static readonly ILabelModelParameter placement = ilm.CreateParameter(InteriorLabelModel.Position.North);

      internal readonly int index;
      internal readonly bool top;

      public ParticipantParameter(bool top, int index) {
        this.top = top;
        this.index = index;
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
        dummyNode.Layout = style.GetParticipantBandBounds(node, index, top);
        dummyLabel.PreferredSize = label.PreferredSize;
        return ilm.GetGeometry(dummyLabel, placement);
      }
      
    public override object Clone() {
        return this;
      }
    }

    /// <xamlhelper/>
    public sealed class ParticipantParameterConverter : TypeConverter {
      /// <xamlhelper/>
      public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
        if (destinationType == typeof(MarkupExtension)) {
          return true;
        }
        return base.CanConvertTo(context, destinationType);
      }

      /// <xamlhelper/>
      public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                       Type destinationType) {
        var participantParameter = value as ParticipantParameter;
        if (destinationType == typeof(MarkupExtension) && participantParameter != null) {
          return new ParticipantLabelModelParameterExtension
          {
            Index = participantParameter.index,
            Top = participantParameter.top
          };
        }
        return base.ConvertTo(context, culture, value, destinationType);
      }
    }

    [SingletonSerialization(ContainerTypes = new[] { typeof(ChoreographyLabelModel) })]
    private sealed class TaskNameBandParameter : ChoreographyParameter
    {
      public override IOrientedRectangle GetGeometry(ILabel label) {
        if (!(label.Owner is INode)) {
          return OrientedRectangle.Empty;
        }
        var node = (INode)label.Owner;
        if (!(node.Style is ChoreographyNodeStyle)) {
          return OrientedRectangle.Empty;
        }
        var style = (ChoreographyNodeStyle)node.Style;
        var bandBounds = style.GetTaskNameBandBounds(node);
        dummyNode.Layout = bandBounds;
        dummyLabel.PreferredSize = label.PreferredSize;
        return interiorStretchModel.GetGeometry(dummyLabel, InteriorStretchLabelModel.Center);
      }

      public override object Clone() {
        return this;
      }
    }    
    
    [SingletonSerialization(ContainerTypes = new[] { typeof(ChoreographyLabelModel) })]
    private sealed class MessageParameter : ChoreographyParameter
    {
      private static readonly ILabelModelParameter northParameter;
      private static readonly ILabelModelParameter southParameter;

      static MessageParameter() {
        var slm = new SandwichLabelModel { YOffset = 32 };
        northParameter = slm.CreateNorthParameter();
        southParameter = slm.CreateSouthParameter();
      }

      public bool North { get; set; }

      public override IOrientedRectangle GetGeometry(ILabel label) {
        var parameter = North ? northParameter : southParameter;
        return parameter.Model.GetGeometry(label, parameter);
      }

      public override object Clone() {
        return MemberwiseClone();
      }
    }

    #endregion
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  [MarkupExtensionReturnType(typeof(ILabelModelParameter))]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class ParticipantLabelModelParameterExtension : MarkupExtension {

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
