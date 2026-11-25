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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.GraphMLCompat.Table
{
  public class TableExtension : MarkupExtension
  {
    public IStripeDefaults ColumnDefaults { get; set; }

    public IStripeDefaults RowDefaults { get; set; }

    public PointD RelativeLocation { set; get; } = PointD.Origin;

    public InsetsD Insets { get; set; } = new(0);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ICollection<IRow> Rows { get; } = new List<IRow>();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ICollection<IColumn> Columns { get; } = new List<IColumn>();


    public override object ProvideValue(IServiceProvider serviceProvider) {
      var extension = new Markup.Common.TableExtension {
          Padding = Insets,
          RelativeLocation = RelativeLocation,
          ColumnDefaults = ColumnDefaults,
          RowDefaults = RowDefaults
      };
      foreach (var column in Columns) {
        extension.Columns.Add(column);
      }
      foreach (var row in Rows) {
        extension.Rows.Add(row);
      }
      return extension.ProvideValue(serviceProvider);
    }
  }

  public class RowExtension : MarkupExtension
  {
    public object Tag { get; set; }

    public IStripeStyle Style { get; set; }

    public double Size { get; set; }

    public double MinimumSize { get; set; }

    public InsetsD Insets { get; set; } = new(0);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ICollection<IRow> Rows { get; } = new List<IRow>();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ICollection<ILabel> Labels { get; } = new List<ILabel>();

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var extension = new Markup.Common.RowExtension {
          Padding = Insets,
          Size = Size,
          Style = Style,
          MinimumSize = MinimumSize,
          Tag = Tag
      };
      foreach (var label in Labels) {
        extension.Labels.Add(label);
      }
      foreach (var row in Rows) {
        extension.Rows.Add(row);
      }
      return extension.ProvideValue(serviceProvider);
    }
  }

  public class ColumnExtension : MarkupExtension
  {
    public object Tag { get; set; }

    public IStripeStyle Style { get; set; }

    public double Size { get; set; }

    public double MinimumSize { get; set; }

    public InsetsD Insets { get; set; } = new(0);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ICollection<IColumn> Columns { get; } = new List<IColumn>();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ICollection<ILabel> Labels { get; } = new List<ILabel>();

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var extension = new Markup.Common.ColumnExtension {
          Padding = Insets,
          Size = Size,
          Style = Style,
          MinimumSize = MinimumSize,
          Tag = Tag
      };
      foreach (var label in Labels) {
        extension.Labels.Add(label);
      }
      foreach (var column in Columns) {
        extension.Columns.Add(column);
      }
      return extension.ProvideValue(serviceProvider);
    }
  }

  public class StripeDefaultsExtension : MarkupExtension
  {
    public InsetsD Insets { get; set; } = new(0);

    public bool ShareStyleInstance { get; set; } = true;

    public double Size { get; set; } = 100;

    public double MinimumSize { get; set; } = 10;

    public ILabelDefaults Labels { get; set; } = new LabelDefaults();

    public IStripeStyle Style { get; set; } = VoidStripeStyle.Instance;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new StripeDefaults {
            Padding = Insets,
            ShareStyleInstance = ShareStyleInstance,
            Size = Size,
            MinimumSize = MinimumSize,
            Labels = Labels,
            Style = Style
        };
  }
}
