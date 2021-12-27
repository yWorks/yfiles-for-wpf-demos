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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Graph.Styles;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml
{
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class SimpleLabelStyleExtension : MarkupExtension
  {
    public SimpleLabelStyleExtension() {
      BackgroundBrush = null;
      VerticalTextAlignment = VerticalAlignment.Top;
      TypefaceFormat = new TypefaceFormat();
      ClipText = true;
      BackgroundPen = null;
      Typeface = null;
      AutoFlip = true;
      TextBrush = Brushes.Black;
    }


    public Brush BackgroundBrush { get; set; }
    public VerticalAlignment VerticalTextAlignment { get; set; }
    public bool ClipText { get; set; }
    public TypefaceFormat TypefaceFormat { get; set; }
    public Pen BackgroundPen { get; set; }
    public Typeface Typeface { get; set; }
    public bool AutoFlip { get; set; }
    public Brush TextBrush { get; set; }

    #region Overrides of MarkupExtension

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new DefaultLabelStyle
      {
        BackgroundBrush = BackgroundBrush,
        VerticalTextAlignment = VerticalTextAlignment,
        TextDecorations = TypefaceFormat.TextDecorations,
        TextAlignment = TypefaceFormat.TextAlignment,
        TextTrimming = TypefaceFormat.TextTrimming,
        TextWrapping = TypefaceFormat.TextWrapping,
        Culture = TypefaceFormat.Culture,
        FlowDirection = TypefaceFormat.FlowDirection,
        NumberSubstitution = TypefaceFormat.NumberSubstitution,
        ClipText=ClipText,
        BackgroundPen = BackgroundPen,
        Typeface = Typeface,
        AutoFlip = AutoFlip,
        TextBrush = TextBrush,
        TextSize = TypefaceFormat.Size
      };
    }

    #endregion
  }

  /// <summary>
  /// A helper class that holds values for creating
  /// <see cref="FormattedText"/> instances from <see cref="Typeface"/>s 
  /// and strings.
  /// </summary>
  /// <seealso cref="CreateFormattedText"/>
  [Obfuscation(ApplyToMembers = false, StripAfterObfuscation = false, Exclude = true)]
  public class TypefaceFormat : ICloneable, IEquatable<TypefaceFormat>
  {

    private double size = 12;

    private CultureInfo culture;
    private FlowDirection flowDirection = FlowDirection.LeftToRight;
    private NumberSubstitution numberSubstitution;
    private TextDecorationCollection textDecorations;
    private TextAlignment textAlignment = TextAlignment.Left;
    private TextTrimming textTrimming = TextTrimming.WordEllipsis;

    /// <summary>
    /// Gets or sets the text decorations.
    /// </summary>
    /// This value will be used during <see cref="CreateFormattedText"/> for the construction of the instance.
    /// The default value is <see langword="null"/>.
    /// <value>The text decorations.</value>
    [DefaultValue(null)]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public TextDecorationCollection TextDecorations {
      get { return textDecorations; }
      set { textDecorations = value; }
    }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    /// This value will be used during <see cref="CreateFormattedText"/> to 
    /// initialize the <see cref="FormattedText.TextAlignment"/> property.
    /// The default value is <see cref="System.Windows.TextAlignment.Left"/>.
    /// <value>The text decorations.</value>
    [DefaultValue(TextAlignment.Left)]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public TextAlignment TextAlignment {
      get { return textAlignment; }
      set { textAlignment = value; }
    }

    /// <summary>
    /// Gets or sets the text trimming.
    /// </summary>
    /// This value will be used during <see cref="CreateFormattedText"/> to 
    /// initialize the <see cref="FormattedText.Trimming"/> property.
    /// The default value is <see cref="System.Windows.TextTrimming.WordEllipsis"/>.
    /// <value>The text decorations.</value>
    [DefaultValue(TextTrimming.WordEllipsis)]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public TextTrimming TextTrimming {
      get { return textTrimming; }
      set { textTrimming = value; }
    }

    /// <summary>
    /// Gets or sets the size of the text.
    /// </summary>
    /// <remarks>
    /// This value will be used during <see cref="CreateFormattedText"/> for the construction of the instance.
    /// The default value is <c>12.0d</c>.
    /// </remarks>
    /// <value>The size.</value>
    [DefaultValue(12d)]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public double Size {
      get { return size; }
      set { size = value; }
    }

    /// <summary>
    /// Gets or sets the culture.
    /// </summary>
    /// <remarks>
    /// This value will be used during <see cref="CreateFormattedText"/> for the construction of the instance.
    /// The default value is <see langword="null"/>, which will result in the
    /// <see cref="CultureInfo.CurrentCulture"/> being used.
    /// </remarks>
    /// <value>The culture.</value>
    [DefaultValue(null)]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public CultureInfo Culture {
      get { return culture; }
      set { culture = value; }
    }

    /// <summary>
    /// Gets or sets the flow direction.
    /// </summary>
    /// <remarks>
    /// This value will be used during <see cref="CreateFormattedText"/> for the construction of the instance.
    /// The default is <see cref="System.Windows.FlowDirection.LeftToRight"/>.</remarks>
    /// <value>The flow direction.</value>
    [DefaultValue(FlowDirection.LeftToRight)]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public FlowDirection FlowDirection {
      get { return flowDirection; }
      set { flowDirection = value; }
    }

    /// <summary>
    /// Gets or sets the number substitution.
    /// </summary>
    /// <remarks>
    /// This value will be used during <see cref="CreateFormattedText"/> for the construction of the instance.
    /// The default is <see langword="null"/>.</remarks>
    /// <value>The number substitution.</value>
    [DefaultValue(null)]
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public NumberSubstitution NumberSubstitution {
      get { return numberSubstitution; }
      set { numberSubstitution = value; }
    }

    /// <summary>
    /// Factory method that actually creates the <see cref="FormattedText"/>
    /// instance that will be used to render the label's text.
    /// </summary>
    /// <param name="typeface">The typeface to use.</param>
    /// <param name="text">The text to apply.</param>
    /// <param name="foreground">The foreground brush.</param>
    /// <returns>A readily configured <see cref="FormattedText"/> instance.</returns>
    public virtual FormattedText CreateFormattedText(Typeface typeface, string text, Brush foreground) {
      FormattedText result = new FormattedText(text, culture ?? CultureInfo.CurrentCulture, flowDirection, typeface, size, foreground, numberSubstitution);
      if (textDecorations != null) {
        result.SetTextDecorations(textDecorations);
      }
      result.Trimming = textTrimming;
      result.TextAlignment = textAlignment;
      return result;
    }

    private TextWrapping textWrapping = TextWrapping.Wrap;
    /// <summary>
    /// Gets or sets the text wrapping mode.
    /// </summary>
    /// <remarks>The default is <see cref="System.Windows.TextWrapping.Wrap"/>.</remarks>
    /// <value>The text wrapping mode.</value>
    [DefaultValue(TextWrapping.Wrap)]
    public TextWrapping TextWrapping {
      get { return textWrapping; }
      set { textWrapping = value; }
    }


    /// <inheritdoc/>
    public object Clone() {
      TypefaceFormat clone = (TypefaceFormat) base.MemberwiseClone();
      if (culture != null) {
        if (!culture.IsReadOnly) {
          clone.culture = (CultureInfo) culture.Clone();
        }
      }
      if (numberSubstitution is ICloneable) {
        clone.numberSubstitution = (NumberSubstitution) ((ICloneable) numberSubstitution).Clone();
      }
      return clone;
    }

    /// <inheritdoc/>
    public bool Equals(TypefaceFormat typefaceFormat) {
      if (typefaceFormat == null) {
        return false;
      }
      if (size != typefaceFormat.size) {
        return false;
      }
      if (textTrimming != typefaceFormat.textTrimming) {
        return false;
      }
      if (textAlignment != typefaceFormat.textAlignment) {
        return false;
      }
      if (!Equals(culture, typefaceFormat.culture)) {
        return false;
      }
      if (!Equals(flowDirection, typefaceFormat.flowDirection)) {
        return false;
      }
      if (!Equals(numberSubstitution, typefaceFormat.numberSubstitution)) {
        return false;
      }
      if (!Equals(textDecorations, typefaceFormat.textDecorations)) {
        return false;
      }
      return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) {
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      return Equals(obj as TypefaceFormat);
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
      int result = size.GetHashCode();
      result = 29 * result + (culture != null ? culture.GetHashCode() : 0);
      result = 29 * result + flowDirection.GetHashCode();
      result = 29 * result + size.GetHashCode();
      result = 29 * result + textAlignment.GetHashCode();
      result = 29 * result + (numberSubstitution != null ? numberSubstitution.GetHashCode() : 0);
      result = 29 * result + (textDecorations != null ? textDecorations.GetHashCode() : 0);
      result = 29 * result + textTrimming.GetHashCode();
      result = 29 * result + textAlignment.GetHashCode();
      return result;
    }
  }

}