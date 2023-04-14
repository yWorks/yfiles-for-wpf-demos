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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.IO.GraphML.Compat.Common {
  [ContentProperty("Columns")]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class ColumnExtension : MarkupExtension
  {
    private readonly ICollection<IColumn> columns = new List<IColumn>();
    private readonly IList<ILabel> labels = new List<ILabel>();

    public ColumnExtension() {
      MinimumSize = -1.0d;
      Size = -1.0d;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ICollection<IColumn> Columns {
      get { return columns; }
    }

    public INodeStyle Style { get; set; }
    public double MinimumSize { get; set; }
    public double Size { get; set; }
    public InsetsD Insets { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ICollection<ILabel> Labels {
      get { return labels; }
    }

    public object Tag { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var originalExtension = new yWorks.Markup.Common.ColumnExtension
      {
        Size = Size,
        Style = new NodeStyleStripeStyleAdapter(Style),
        Insets = Insets,
        MinimumSize = MinimumSize,
        Tag = Tag
      };
      foreach (var label in Labels) {
        originalExtension.Labels.Add(label);
      }
      foreach (var column in Columns) {
        originalExtension.Columns.Add(column);
      }
      return originalExtension.ProvideValue(serviceProvider);
    }
  }

  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class StripeDefaultsExtension : MarkupExtension
  {
    private double size = 100;
    private double minSize = 10;
    private ILabelDefaults labelDefaults = new LabelDefaults();
    private INodeStyle style = VoidNodeStyle.Instance;
    private bool shareStyleInstance = true;
    private InsetsD insets = InsetsD.Empty;

    public InsetsD Insets {
      get { return insets; }
      set { insets = value; }
    }

    public double Size {
      get { return size; }
      set { size = value; }
    }

    public double MinimumSize {
      get { return minSize; }
      set { minSize = value; }
    }

    public ILabelDefaults Labels {
      get { return labelDefaults; }
      set { labelDefaults = value; }
    }

    public INodeStyle Style {
      get { return style; }
      set { style = value; }
    }

    public bool ShareStyleInstance {
      get { return shareStyleInstance; }
      set { shareStyleInstance = value; }
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new StripeDefaults {
        Insets = Insets,
        Size = Size,
        ShareStyleInstance = ShareStyleInstance,
        MinimumSize = MinimumSize,
        Style = new NodeStyleStripeStyleAdapter(Style),
        Labels = Labels
      };
    }
  }

}