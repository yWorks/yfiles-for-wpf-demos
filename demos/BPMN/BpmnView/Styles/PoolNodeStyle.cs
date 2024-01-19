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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Controls.Input;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.Bpmn.Styles
{
  /// <summary>
  /// An <see cref="INodeStyle"/> implementation representing a Pool according to the BPMN.
  /// </summary>
  /// <remarks>
  /// The main visualization is delegated to <see cref="TableNodeStyle"/>.
  /// </remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  [ContentProperty("TableNodeStyle")]
  [SingletonSerialization]
  public class PoolNodeStyle : NodeStyleBase<VisualGroup> {
    private IIcon multipleInstanceIcon;

    private static TableNodeStyle CreateDefaultTableNodeStyle(bool vertical) {
      // create a new table
      var table = new Table();
      var tns = new TableNodeStyle();

      // we'd like to use a special stripe style
      var alternatingLeafStripeStyle = new AlternatingLeafStripeStyle {
        EvenLeafDescriptor = new StripeDescriptor {
          BackgroundBrush = BpmnConstants.DefaultPoolNodeEvenLeafBackground,
          InsetBrush = BpmnConstants.DefaultPoolNodeEvenLeafInset
        },
        OddLeafDescriptor = new StripeDescriptor {
          BackgroundBrush = BpmnConstants.DefaultPoolNodeOddLeafBackground,
          InsetBrush = BpmnConstants.DefaultPoolNodeOddLeafInset
        },
        ParentDescriptor = new StripeDescriptor {
          BackgroundBrush = BpmnConstants.DefaultPoolNodeParentBackground,
          InsetBrush = BpmnConstants.DefaultPoolNodeParentInset
        }
      };
      if (vertical) {
        table.Insets = new InsetsD(0, 20, 0, 0);

        // set the column defaults
        table.ColumnDefaults.Insets = new InsetsD(0, 20, 0, 0);
        table.ColumnDefaults.Labels.Style = new DefaultLabelStyle {
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Center
        };
        table.ColumnDefaults.Labels.LayoutParameter = StretchStripeLabelModel.North;
        table.ColumnDefaults.Style = alternatingLeafStripeStyle;
        table.ColumnDefaults.MinimumSize = 50;
        tns.TableRenderingOrder = TableRenderingOrder.ColumnsFirst;
      } else {
        table.Insets = new InsetsD(20, 0, 0, 0);

        // set the row defaults
        table.RowDefaults.Insets = new InsetsD(20, 0, 0, 0);
        table.RowDefaults.Labels.Style = new DefaultLabelStyle {
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Center
        };
        table.RowDefaults.Labels.LayoutParameter = StretchStripeLabelModel.West;
        table.RowDefaults.Style = alternatingLeafStripeStyle;
        table.RowDefaults.MinimumSize = 50;
        tns.TableRenderingOrder = TableRenderingOrder.RowsFirst;
      }

      tns.BackgroundStyle = new ShapeNodeStyle {
        Brush = BpmnConstants.DefaultPoolNodeBackground
      };
      tns.Table = table;
      return tns;
    }

    /// <summary>
    /// Gets or sets if this pool represents a multiple instance participant.
    /// </summary>
    [DefaultValue(false)]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public bool MultipleInstance { get; set; }

    [DefaultValue(false)]
    private bool Vertical { get; set; }

    private Brush iconColor = BpmnConstants.DefaultIconColor;

    /// <summary>
    /// Gets or sets the color for the icon.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DefaultIconColor")]
    public Brush IconColor {
      get { return iconColor; }
      set {
        if (iconColor != value) {
          iconColor = value;
          UpdateIcon();
        }
      }
    }

    private void UpdateIcon() {
      var multipleIcon = IconFactory.CreateLoopCharacteristic(LoopCharacteristic.Parallel, IconColor);
      multipleInstanceIcon = new PlacedIcon(multipleIcon, BpmnConstants.PoolNodeMarkerPlacement,
          BpmnConstants.MarkerSize);
    }

    private TableNodeStyle tableNodeStyle;

    /// <summary>
    /// Gets or sets the <see cref="TableNodeStyle"/> the visualization is delegated to.
    /// </summary>
    [NotNull]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public TableNodeStyle TableNodeStyle {
      get {
        return tableNodeStyle ?? (tableNodeStyle = CreateDefaultTableNodeStyle(Vertical));
      }
      set { tableNodeStyle = value; }
    }

    /// <summary>
    /// Creates a new instance for a horizontal pool.
    /// </summary>
    public PoolNodeStyle() : this(false) { }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="vertical">Whether the style represents a vertical pool.</param>
    public PoolNodeStyle(bool vertical) {
      Vertical = vertical;
      UpdateIcon();
    }

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public override object Clone() {
      PoolNodeStyle clone = new PoolNodeStyle {
          MultipleInstance = MultipleInstance,
          TableNodeStyle = (TableNodeStyle) TableNodeStyle.Clone(),
          IconColor = IconColor
      };
      return clone;
    }

    ///<inheritdoc/>
    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      var container = new VisualGroup();
      container.Add(TableNodeStyle.Renderer.GetVisualCreator(node, TableNodeStyle).CreateVisual(context));
      if (MultipleInstance) {
        multipleInstanceIcon.SetBounds(node.Layout);
        container.Add(multipleInstanceIcon.CreateVisual(context));
      }
      return container;
    }

    ///<inheritdoc/>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      VisualGroup container = oldVisual;
      if (container == null || container.Children.Count == 0) {
        return CreateVisual(context, node);
      }

      var oldTableVisual = container.Children[0];
      var newTableVisual = TableNodeStyle.Renderer.GetVisualCreator(node, TableNodeStyle).UpdateVisual(context, oldTableVisual);
      if (oldTableVisual != newTableVisual) {
        container.Children.Remove(oldTableVisual);
        container.Children.Insert(0, newTableVisual);
      }

      var oldMultipleVisual = container.Children.Count > 1 ? container.Children[1] : null;
      if (MultipleInstance) {
        multipleInstanceIcon.SetBounds(node.Layout);
        var newMultipleVisual = multipleInstanceIcon.UpdateVisual(context, oldMultipleVisual);
        if (oldMultipleVisual != newMultipleVisual) {
          if (oldMultipleVisual != null) {
            container.Children.Remove(oldMultipleVisual);
          }
          container.Add(newMultipleVisual);
        }
      } else if (oldMultipleVisual != null) {
        // there has been a multipleInstance icon before
        container.Children.Remove(oldMultipleVisual);
      }
      return container;
    }

    ///<inheritdoc/>
    protected override object Lookup(INode item, Type type) {
      if (type == typeof (IEditLabelHelper)) {
        return new PoolNodeEditLabelHelper(this);
      }
      return TableNodeStyle.Renderer.GetContext(item, TableNodeStyle).Lookup(type);
    }


    private sealed class PoolNodeEditLabelHelper : EditLabelHelper
    {
      private readonly PoolNodeStyle style;

      public PoolNodeEditLabelHelper(PoolNodeStyle style) {
        this.style = style;
      }

      protected override ILabelModelParameter GetLabelParameter(IInputModeContext context, ILabelOwner owner) {
        if (style.TableNodeStyle.TableRenderingOrder == TableRenderingOrder.ColumnsFirst) {
          return PoolHeaderLabelModel.North;
        } else {
          return PoolHeaderLabelModel.West;
        }
      }
    }
  }
}
