/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.Toolkit.OptionHandler
{  
  /// <summary>
  /// An attribute that determines the ui component's text label.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false, Inherited = true)]
  public class LabelAttribute : Attribute
  {
    /// <summary>
    /// Initialize the LabelAttribute.
    /// </summary>
    /// <param name="label">The text of the generated label.</param>
    public LabelAttribute(string label) {
      this.Label = label;
    }

    /// <summary>
    /// The text of the generated label.
    /// </summary>
    public string Label { get; set; }
  }

  /// <summary>
  /// An attribute that limits the ui component's value to the given range.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false,
    Inherited = true)]
  public class MinMaxAttribute : Attribute
  {
    /// <summary>
    /// Initialize the MinMaxAttribute.
    /// </summary>
    public MinMaxAttribute() {
      this.Min = Int32.MinValue;
      this.Max = Int32.MaxValue;
      this.Step = 1;
    }

    /// <summary>
    /// The ui component's minimum valid value.
    /// </summary>
    public double Min { get; set; }
    /// <summary>
    /// The ui component's maximum valid value.
    /// </summary>
    public double Max { get; set; }
    /// <summary>
    /// The ui component's increment/decrement step (if supported)
    /// </summary>
    public double Step { get; set; }
  }

  /// <summary>
  /// An attribute that determines the <see cref="OptionGroup"/> the ui component belongs to.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false,
    Inherited = true)]
  public class OptionGroupAttribute : Attribute
  {
    /// <summary>
    /// Initialize the new OptionGroupAttribute. 
    /// </summary>
    /// <param name="name">Name of OptionGroup the ui component belongs to.</param>
    /// <param name="position">The ui component's display position in the OptionGroup.</param>
    public OptionGroupAttribute(string name, int position) {
      Name = name;
      Position = position;
    }

    /// <summary>
    /// Name of <see cref="OptionGroup"/> the ui component belongs to.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// The ui component's display position in the <see cref="OptionGroup"/>.
    /// </summary>
    public int Position { get; set; }
  }

  /// <summary>
  /// An attribute that determines an enum value and its label name for the ui component.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
  public class EnumValueAttribute : Attribute
  {
    /// <summary>
    /// Initializes the new EnumValueAttribute.
    /// </summary>
    /// <param name="label">The displayed label of the generated enum value.</param>
    /// <param name="value">The enum value.</param>
    public EnumValueAttribute(string label, object value) {
      Label = label;
      Value = value;
    }

    /// <summary>
    /// The displayed label of the generated enum value.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// The enum value.
    /// </summary>
    public object Value { get; set; }
  }

  /// <summary>
  /// An attribute that determines the type of ui component that will be generated for the attributed member.
  /// </summary>
  public class ComponentTypeAttribute : Attribute
  {
    /// <summary>
    /// Initialize the ComponentTypeAttribute.
    /// </summary>
    /// <param name="value">The type of ui component to be generated.</param>
    public ComponentTypeAttribute(ComponentTypes value) {
      Value = value;
    }

    /// <summary>
    /// >The type of ui component to be generated.
    /// </summary>
    public virtual ComponentTypes Value { get; private set; }
  }

  /// <summary>
  /// The ui components available in the generated UI.
  /// </summary>
  public enum ComponentTypes
  {
    OptionGroup,
    Slider,
    Combobox,
    RadioButton,
    Checkbox,
    Spinner,
    FormattedText,
    Text
  }
}
