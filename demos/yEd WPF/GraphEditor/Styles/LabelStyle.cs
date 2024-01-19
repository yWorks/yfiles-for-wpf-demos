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

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.GraphEditor.Styles
{
  public sealed class LabelStyle : ILabelStyle
  {
    private static readonly LabelStyleRenderer renderer = new LabelStyleRenderer();
    private DefaultLabelStyle style;

    public LabelStyle() {
      style = CreateInnerStyle();
    }

    private static DefaultLabelStyle CreateInnerStyle() {
      return new DefaultLabelStyle {
        AutoFlip = true, ClipText = true, VerticalTextAlignment = VerticalAlignment.Center, TextBrush = Brushes.Black,
        Typeface = new Typeface("Arial")
      };
    }

    public object Clone() {
      var clone = (LabelStyle) MemberwiseClone();
      clone.style = CreateInnerStyle();
      return clone;
    }

    ILabelStyleRenderer ILabelStyle.Renderer {
      get { return renderer; }
    }

    public Typeface Typeface {
      get { return style.Typeface; }
    }

    [DisplayName("Font Size")]
    public double FontSize {
      get {return style.TextSize;}
      set {
        if (value != style.TextSize) {
          style.TextSize = value;
        }
      }
    }

    [NotNull,DisplayName("Text Color")]
    [DefaultValue(typeof(SolidColorBrush), "Black")]
    public Brush TextBrush {
      get { return style.TextBrush; }
      set { style.TextBrush = value; }
    }

    [NotNull,DisplayName("Font")]
    [DefaultValue(typeof(FontFamily), "Arial")]
    public FontFamily FontFamily {
      get { return style.Typeface.FontFamily; }
      set {
        if (style.Typeface.FontFamily != value) {
          var typeface = style.Typeface;
          style.Typeface = new Typeface(value, typeface.Style, typeface.Weight, typeface.Stretch);
        }
      }
    }

    [DisplayName("Font Weight")]
    [DefaultValue(typeof(FontWeight), "Normal")]
    public FontWeight FontWeight {
      get { return style.Typeface.Weight; }
      set {
        if (style.Typeface.Weight != value) {
          var typeface = style.Typeface;
          style.Typeface = new Typeface(typeface.FontFamily, typeface.Style, value, typeface.Stretch);
        }
      }
    }

    [DisplayName("Font Style")]
    [DefaultValue(typeof(FontStyle), "Normal")]
    public FontStyle FontStyle {
      get { return style.Typeface.Style; }
      set {
        if (style.Typeface.Style != value) {
          var typeface = style.Typeface;
          style.Typeface = new Typeface(typeface.FontFamily, value, typeface.Weight, typeface.Stretch);
        }
      }
    }

    [CanBeNull, DisplayName("Border")]
    [DefaultValue(null)]
    public Pen BackgroundPen {
      get { return style.BackgroundPen; } 
      set { style.BackgroundPen = value; }
    }

    [CanBeNull, DisplayName("Background")]
    [DefaultValue(null)]
    public Brush BackgroundBrush {
      get { return style.BackgroundBrush; } 
      set { style.BackgroundBrush = value; }
    }

    [DisplayName("V-Alignment")]
    [DefaultValue(VerticalAlignment.Center)]
    public VerticalAlignment VerticalTextAlignment {
      get { return style.VerticalTextAlignment; }
      set { style.VerticalTextAlignment = value; }
    }

    [DisplayName("H-Alignment")]
    [DefaultValue(TextAlignment.Left)]
    public TextAlignment HorizontalTextAlignment {
      get { return style.TextAlignment; }
      set {
        if (value != style.TextAlignment) {
          style.TextAlignment = value;
        }
      }
    }

    public FlowDirection FlowDirection
    {
      get { return style.FlowDirection; }
      set
      {
        if (value != style.FlowDirection) {
          style.FlowDirection = value;
        }
      }
    }

    [DefaultValue(LabelShape.Rectangle)]
    public LabelShape Shape {
      get { return style.Shape; }
      set
      {
        if (value != style.Shape) {
          style.Shape = value;
        }
      }
    }

    [DisplayName("Clip Text")]
    [DefaultValue(true)]
    public bool ClipText { 
      get { return style.ClipText; }
      set { style.ClipText = value; }
    }

    private class LabelStyleRenderer : ILabelStyleRenderer {
      private static readonly DefaultLabelStyleRenderer renderer = new DefaultLabelStyleRenderer();

      public IVisualCreator GetVisualCreator(ILabel label, ILabelStyle style) {
        return renderer.GetVisualCreator(label, ((LabelStyle)style).style);
      }

      public IBoundsProvider GetBoundsProvider(ILabel label, ILabelStyle style) {
        return renderer.GetBoundsProvider(label, ((LabelStyle)style).style);
      }

      public IHitTestable GetHitTestable(ILabel label, ILabelStyle style) {
        return renderer.GetHitTestable(label, ((LabelStyle)style).style);
      }

      public IMarqueeTestable GetMarqueeTestable(ILabel label, ILabelStyle style) {
        return renderer.GetMarqueeTestable(label, ((LabelStyle)style).style);
      }

      public IVisibilityTestable GetVisibilityTestable(ILabel label, ILabelStyle style) {
        return renderer.GetVisibilityTestable(label, ((LabelStyle)style).style);
      }

      public ILookup GetContext(ILabel label, ILabelStyle style) {
        return renderer.GetContext(label, ((LabelStyle)style).style);
      }

      public SizeD GetPreferredSize(ILabel label, ILabelStyle style) {
        return renderer.GetPreferredSize(label, ((LabelStyle)style).style);
      }
    }
  }
}
