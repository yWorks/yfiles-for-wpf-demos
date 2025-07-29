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
using System.Linq;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat.LabelModels
{
  public enum ExteriorLabelModelPosition
  {
    North = ExteriorNodeLabelModel.Position.Top,
    East = ExteriorNodeLabelModel.Position.Right,
    South = ExteriorNodeLabelModel.Position.Bottom,
    West = ExteriorNodeLabelModel.Position.Left,
    NorthEast = ExteriorNodeLabelModel.Position.TopRight,
    SouthEast = ExteriorNodeLabelModel.Position.BottomRight,
    NorthWest = ExteriorNodeLabelModel.Position.TopLeft,
    SouthWest = ExteriorNodeLabelModel.Position.BottomLeft
  }

  public class ExteriorLabelModelParameterExtension : MarkupExtension
  {
    public ExteriorLabelModelPosition Position { get; set; } = ExteriorLabelModelPosition.North;

    public ILabelModel Model { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var model = this.Model as ExteriorNodeLabelModel;
      if (model == null) {
        model = ExteriorNodeLabelModel.Top.Model;
      }
      return model.CreateParameter((ExteriorNodeLabelModel.Position) Position);
    }
  }

  public class ExteriorLabelModelExtension : MarkupExtension
  {
    public static readonly ILabelModelParameter North;
    public static readonly ILabelModelParameter East;
    public static readonly ILabelModelParameter South;
    public static readonly ILabelModelParameter West;
    public static readonly ILabelModelParameter NorthEast;
    public static readonly ILabelModelParameter SouthEast;
    public static readonly ILabelModelParameter NorthWest;
    public static readonly ILabelModelParameter SouthWest;

    static ExteriorLabelModelExtension() {
      var model = new ExteriorNodeLabelModel {
          Margins = InsetsD.Empty
      };
      North = model.CreateParameter(ExteriorNodeLabelModel.Position.Top);
      East = model.CreateParameter(ExteriorNodeLabelModel.Position.Right);
      South = model.CreateParameter(ExteriorNodeLabelModel.Position.Bottom);
      West = model.CreateParameter(ExteriorNodeLabelModel.Position.Left);
      NorthEast = model.CreateParameter(ExteriorNodeLabelModel.Position.TopRight);
      SouthEast = model.CreateParameter(ExteriorNodeLabelModel.Position.BottomRight);
      NorthWest = model.CreateParameter(ExteriorNodeLabelModel.Position.TopLeft);
      SouthWest = model.CreateParameter(ExteriorNodeLabelModel.Position.BottomLeft);
    }


    public InsetsD Insets { get; set; } = new InsetsD(0);

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new ExteriorNodeLabelModel { Margins = Insets };
    }
  }

  public enum InteriorLabelModelPosition
  {
    North = InteriorNodeLabelModel.Position.Top,
    East = InteriorNodeLabelModel.Position.Right,
    South = InteriorNodeLabelModel.Position.Bottom,
    West = InteriorNodeLabelModel.Position.Left,
    NorthEast = InteriorNodeLabelModel.Position.TopRight,
    SouthEast = InteriorNodeLabelModel.Position.BottomRight,
    NorthWest = InteriorNodeLabelModel.Position.TopLeft,
    SouthWest = InteriorNodeLabelModel.Position.BottomLeft,
    Center = InteriorNodeLabelModel.Position.Center
  }

  public class InteriorLabelModelParameterExtension : MarkupExtension
  {
    public InteriorLabelModelPosition Position { get; set; } = InteriorLabelModelPosition.North;

    public ILabelModel Model { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var model = this.Model as InteriorNodeLabelModel;
      if (model == null) {
        model = InteriorNodeLabelModel.Top.Model;
      }
      return model.CreateParameter((InteriorNodeLabelModel.Position) Position);
    }
  }

  public class InteriorLabelModelExtension : MarkupExtension
  {
    public static readonly ILabelModelParameter North;
    public static readonly ILabelModelParameter East;
    public static readonly ILabelModelParameter South;
    public static readonly ILabelModelParameter West;
    public static readonly ILabelModelParameter NorthEast;
    public static readonly ILabelModelParameter SouthEast;
    public static readonly ILabelModelParameter NorthWest;
    public static readonly ILabelModelParameter SouthWest;
    public static readonly ILabelModelParameter Center;

    static InteriorLabelModelExtension() {
      var model = new InteriorNodeLabelModel {
          Padding = InsetsD.Empty
      };
      North = model.CreateParameter(InteriorNodeLabelModel.Position.Top);
      East = model.CreateParameter(InteriorNodeLabelModel.Position.Right);
      South = model.CreateParameter(InteriorNodeLabelModel.Position.Bottom);
      West = model.CreateParameter(InteriorNodeLabelModel.Position.Left);
      NorthEast = model.CreateParameter(InteriorNodeLabelModel.Position.TopRight);
      SouthEast = model.CreateParameter(InteriorNodeLabelModel.Position.BottomRight);
      NorthWest = model.CreateParameter(InteriorNodeLabelModel.Position.TopLeft);
      SouthWest = model.CreateParameter(InteriorNodeLabelModel.Position.BottomLeft);
      Center = model.CreateParameter(InteriorNodeLabelModel.Position.Center);
    }

    public InsetsD Insets { get; set; } = new InsetsD(0);

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new InteriorNodeLabelModel { Padding = Insets };
    }
  }

  public enum InteriorStretchLabelModelPosition
  {
    North = StretchNodeLabelModel.Position.Top,
    East = StretchNodeLabelModel.Position.Right,
    South = StretchNodeLabelModel.Position.Bottom,
    West = StretchNodeLabelModel.Position.Left,
    Center = StretchNodeLabelModel.Position.Center,
    CenterHorizontal = StretchNodeLabelModel.Position.CenterHorizontal,
    CenterVertical = StretchNodeLabelModel.Position.CenterVertical,
  }

  public class InteriorStretchLabelModelParameterExtension : MarkupExtension
  {
    public InteriorStretchLabelModelPosition Position { get; set; } = InteriorStretchLabelModelPosition.North;

    public ILabelModel Model { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var model = this.Model as StretchNodeLabelModel;
      if (model == null) {
        model = StretchNodeLabelModel.Top.Model;
      }
      return model.CreateParameter((StretchNodeLabelModel.Position) Position);
    }
  }

  public class InteriorStretchLabelModelExtension : MarkupExtension
  {
    public static readonly ILabelModelParameter North;
    public static readonly ILabelModelParameter East;
    public static readonly ILabelModelParameter South;
    public static readonly ILabelModelParameter West;
    public static readonly ILabelModelParameter Center;
    public static readonly ILabelModelParameter CenterHorizontal;
    public static readonly ILabelModelParameter CenterVertical;

    static InteriorStretchLabelModelExtension() {
      var model = new StretchNodeLabelModel {
          Padding = InsetsD.Empty
      };
      North = model.CreateParameter(StretchNodeLabelModel.Position.Top);
      East = model.CreateParameter(StretchNodeLabelModel.Position.Right);
      South = model.CreateParameter(StretchNodeLabelModel.Position.Bottom);
      West = model.CreateParameter(StretchNodeLabelModel.Position.Left);
      Center = model.CreateParameter(StretchNodeLabelModel.Position.Center);
      CenterHorizontal = model.CreateParameter(StretchNodeLabelModel.Position.CenterHorizontal);
      CenterVertical = model.CreateParameter(StretchNodeLabelModel.Position.CenterVertical);
    }

    public InsetsD Insets { get; set; } = new InsetsD(0);

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new StretchNodeLabelModel { Padding = Insets };
    }
  }

  public enum StripeLabelModelPosition
  {
    North = StripeLabelModel.Position.Top,
    East = StripeLabelModel.Position.Right,
    South = StripeLabelModel.Position.Bottom,
    West = StripeLabelModel.Position.Left
  }

  public class StripeLabelModelParameterExtension : MarkupExtension
  {
    public StripeLabelModelPosition Position { get; set; } = StripeLabelModelPosition.North;

    public ILabelModel Model { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var model = this.Model as StripeLabelModel;
      if (model == null) {
        model = StripeLabelModel.Top.Model;
      }
      return model.CreateParameter((StripeLabelModel.Position) Position);
    }
  }

  public class StripeLabelModelExtension : MarkupExtension
  {
    public static readonly ILabelModelParameter North = StripeLabelModel.Top;
    public static readonly ILabelModelParameter East = StripeLabelModel.Right;
    public static readonly ILabelModelParameter South = StripeLabelModel.Bottom;
    public static readonly ILabelModelParameter West = StripeLabelModel.Left;

    public bool UseActualInsets { get; set; } = false;

    public double Ratio { get; set; } = 0.5;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new StripeLabelModel { UseTotalPadding = UseActualInsets, Ratio = Ratio };
    }
  }

  public enum StretchStripeLabelModelPosition
  {
    North = StretchStripeLabelModel.Position.Top,
    East = StretchStripeLabelModel.Position.Right,
    South = StretchStripeLabelModel.Position.Bottom,
    West = StretchStripeLabelModel.Position.Left
  }

  public class StretchStripeLabelModelParameterExtension : MarkupExtension
  {
    public StretchStripeLabelModelPosition Position { get; set; } = StretchStripeLabelModelPosition.North;

    public ILabelModel Model { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var model = this.Model as StretchStripeLabelModel;
      if (model == null) {
        model = StretchStripeLabelModel.Top.Model;
      }
      return model.CreateParameter((StretchStripeLabelModel.Position) Position);
    }
  }

  public class StretchStripeLabelModelExtension : MarkupExtension
  {
    public static readonly ILabelModelParameter North = StretchStripeLabelModel.Top;
    public static readonly ILabelModelParameter East = StretchStripeLabelModel.Right;
    public static readonly ILabelModelParameter South = StretchStripeLabelModel.Bottom;
    public static readonly ILabelModelParameter West = StretchStripeLabelModel.Left;

    public bool UseActualInsets { get; set; } = false;

    public InsetsD Insets { get; set; } = new InsetsD(0);

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new StretchStripeLabelModel { UseTotalPadding = UseActualInsets, Padding = Insets };
    }
  }

  public class CompositeLabelModelParameterExtension : MarkupExtension
  {
    public ILabelModelParameter Parameter { get; set; } = null;

    public ILabelModel Model { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new yWorks.Markup.Common.CompositeLabelModelParameterExtension {
          Model = (CompositeLabelModel) Model, Parameter = Parameter
      }.ProvideValue(serviceProvider);
    }
  }

  public class CompositeLabelModelExtension : MarkupExtension
  {
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public IList<ILabelModel> LabelModels { get; } = new List<ILabelModel>();

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var compositeLabelModel = new CompositeLabelModel();
      foreach (var labelModel in LabelModels) {
        compositeLabelModel.AddModel(labelModel);
      }
      return compositeLabelModel;
    }
  }

  public class GenericLabelModelParameterPair
  {
    public ILabelModelParameter Parameter { get; set; } = null;

    public LabelCandidateDescriptor Descriptor { get; set; } = null;
  }

  public class LabelCandidateDescriptor
  {
    public static readonly LabelCandidateDescriptor ExternalDescriptor = new() { ExternalCandidate = true };

    public static readonly LabelCandidateDescriptor InternalDescriptor = new();

    public bool ExternalCandidate { get; set; } = false;

    public double EdgeOverlapPenalty { get; set; } = 1.0;

    public double NodeOverlapPenalty { get; set; } = 1.0;

    public double Profit { get; set; } = 1.0;
  }

  public class GenericLabelModelParameterExtension : MarkupExtension
  {
    public int Index { get; set; } = 0;

    public ILabelModel Model { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return Model == null
          ? GenericLabelModelExtension.Absolute
          : ((CompositeLabelModel) Model).Parameters.ElementAt(Index);
    }
  }

  [ContentProperty("Parameters")]
  public class GenericLabelModelExtension : MarkupExtension
  {
    internal static readonly ILabelModelParameter Absolute = FreeLabelModel.Instance.CreateAbsolute(PointD.Origin);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public IList<object> Parameters { get; } = new List<object>();

    public int Default { get; set; } = 0;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var compositeLabelModel = new CompositeLabelModel();
      var dParam = Default >= Parameters.Count || Default < 0
          ? (Parameters.First() ?? Absolute)
          : Parameters.ElementAt(Default);
      {
        if (dParam is GenericLabelModelParameterPair pair) {
          compositeLabelModel.AddParameter(pair.Parameter!, pair.Descriptor?.Profit ?? 1.0);
        } else {
          compositeLabelModel.AddParameter((ILabelModelParameter) dParam);
        }
      }

      foreach (var m in Parameters) {
        if (m != dParam) {
          if (m is GenericLabelModelParameterPair pair) {
            compositeLabelModel.AddParameter(pair.Parameter);
          } else {
            compositeLabelModel.AddParameter((ILabelModelParameter) m);
          }
        }
      }
      return compositeLabelModel;
    }
  }

  public class SandwichLabelModelExtension : MarkupExtension
  {
    [GraphML(Name = "North")]
    public static readonly ILabelModelParameter North;

    [GraphML(Name = "South")]
    public static readonly ILabelModelParameter South;

    static SandwichLabelModelExtension() {
      var noMarginsFactory = new ExteriorNodeLabelModel { Margins = new InsetsD(0) };
      var umModel = new CompositeLabelModel();
      North = umModel.AddParameter(noMarginsFactory.CreateParameter(ExteriorNodeLabelModel.Position.Top));
      South = umModel.AddParameter(noMarginsFactory.CreateParameter(ExteriorNodeLabelModel.Position.Bottom));
    }

    public double YOffset { get; set; } = 0;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var wrappedModel = new ExteriorNodeLabelModel { Margins = new InsetsD(YOffset) };
      var model = new CompositeLabelModel();
      model.AddParameter(wrappedModel.CreateParameter(ExteriorNodeLabelModel.Position.Bottom));
      model.AddParameter(wrappedModel.CreateParameter(ExteriorNodeLabelModel.Position.Top));
      return model;
    }
  }

  public class SandwichParameterExtension : MarkupExtension
  {
    public ExteriorLabelModelPosition Position { get; set; } = ExteriorLabelModelPosition.North;

    public ILabelModel Model { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var model = Model as CompositeLabelModel ?? (CompositeLabelModel) SandwichLabelModelExtension.South.Model;
      return model.Parameters.First(p =>
          (int) ((ExteriorNodeLabelModel.Parameter) p.WrappedParameter).Position == (int) Position);
    }
  }

  public class DescriptorWrapperLabelModelParameterExtension : MarkupExtension
  {
    public ILabelModelParameter Parameter { get; set; } = null;

    public ILabelModel Model { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return Parameter;
    }
  }

  public class DescriptorWrapperLabelModelExtension : MarkupExtension
  {
    public ILabelModel InnerModel { get; set; } = null;

    public LabelCandidateDescriptor Descriptor { get; set; } = null;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return InnerModel;
    }
  }

  public class GroupNodeLabelModelExtension : MarkupExtension
  {
    public bool ConsiderTabInset { get; set; } = true;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new GroupNodeLabelModel { ConsiderTabPadding = ConsiderTabInset };
    }
  }

  public abstract class EdgePathBasedLabelModelExtension : MarkupExtension
  {
    public double Angle { get; set; } = 0;

    public bool AutoRotationEnabled { get; set; } = true;

    public double Distance { get; set; } = 0;

    public double Offset { get; set; } = 0;

    public EdgeSides SideOfEdge { get; set; } = EdgeSides.OnEdge | EdgeSides.LeftOfEdge | EdgeSides.RightOfEdge;
  }

  public class EdgePathLabelModelExtension : EdgePathBasedLabelModelExtension
  {
    public override object ProvideValue(IServiceProvider serviceProvider) {
      var _autoRotationEnabled = AutoRotationEnabled;
      return new EdgePathLabelModel {
          Angle = _autoRotationEnabled ? Angle : -Angle,
          AutoRotation = _autoRotationEnabled,
          Distance = Distance,
          Offset = Offset,
          SideOfEdge = SideOfEdge
      };
    }
  }

  public class EdgeSegmentLabelModelExtension : EdgePathBasedLabelModelExtension
  {
    public override object ProvideValue(IServiceProvider serviceProvider) {
      var _autoRotationEnabled = AutoRotationEnabled;
      return new EdgeSegmentLabelModel {
          Angle = _autoRotationEnabled ? Angle : -Angle,
          AutoRotation = _autoRotationEnabled,
          Distance = Distance,
          Offset = Offset,
          SideOfEdge = SideOfEdge
      };
    }
  }

  public class SmartEdgeLabelModelExtension : MarkupExtension
  {
    public double Angle { get; set; } = 0;

    public bool AutoRotation { get; set; } = true;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var _autoRotationEnabled = AutoRotation;
      return new SmartEdgeLabelModel {
          Angle = _autoRotationEnabled ? Angle : -Angle, AutoRotation = _autoRotationEnabled
      };
    }
  }

  public class NinePositionsEdgeLabelModelExtension : MarkupExtension
  {
    public static readonly ILabelModelParameter SourceAbove = NinePositionsEdgeLabelModel.SourceAbove;
    public static readonly ILabelModelParameter CenterAbove = NinePositionsEdgeLabelModel.CenterAbove;
    public static readonly ILabelModelParameter TargetAbove = NinePositionsEdgeLabelModel.TargetAbove;
    public static readonly ILabelModelParameter SourceCentered = NinePositionsEdgeLabelModel.SourceCentered;
    public static readonly ILabelModelParameter CenterCentered = NinePositionsEdgeLabelModel.CenterCentered;
    public static readonly ILabelModelParameter TargetCentered = NinePositionsEdgeLabelModel.TargetCentered;
    public static readonly ILabelModelParameter SourceBelow = NinePositionsEdgeLabelModel.SourceBelow;
    public static readonly ILabelModelParameter CenterBelow = NinePositionsEdgeLabelModel.CenterBelow;
    public static readonly ILabelModelParameter TargetBelow = NinePositionsEdgeLabelModel.TargetBelow;

    public double Angle { get; set; } = 0;

    public double Distance { get; set; } = 10;

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new NinePositionsEdgeLabelModel { Angle = -Angle, Distance = Distance };
    }
  }
}
