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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using yWorks.Graph;
using UITypeEditor = System.Windows.DataTemplate;

namespace Demo.yFiles.Option.Handler
{
  /// <summary>
  /// Abstract implementation of interface <see cref="OptionItem"/> that handles general objects.
  /// </summary>
  /// <remarks>For specialized handling/validation etc. of custom types, this class must
  /// be extended by custom implementations</remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class OptionItem : IOptionItem
  {
    #region private members

    private string name;
    private object _value;
    private bool enabled = true;
    private IOptionGroup _owner;
    private readonly IDictionary<string, object> attributes;
    private readonly IMapper<string, object> attributesMapper;

    /// <summary>
    /// Attribute key that controls this option item supports the "no value" concept. 
    /// </summary>
    /// <remarks>If this is set to <see langword="false"/>,
    /// attempts to set a <see langword="null"/> <see cref="Value"/> will be rejected. If set to
    /// <see langword="true"/>, UI editors will automatically contain a wrapper that
    /// allows to specify an "empty" value.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string SupportNullValueAttribute = "OptionItem.SUPPORT_NULL_VALUE_ATTRIBUTE";

    /// <summary>
    /// Attribute key that controls this option item supports the "undefined value" concept. 
    /// </summary>
    /// <remarks>If set to <see langword="true"/>, UI editors will correctly handle
    /// when an undefined value (set with <see cref="SetUndefined"/> or by setting the <see cref="Value"/>
    /// to <see cref="ValueUndefined"/>) on an item. This is mainly useful for
    /// items that encapsulate a multiple selection of actual objects.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string SupportUndefinedValueAttribute = "OptionItem.SUPPORT_UNDEFINED_VALUE_ATTRIBUTE";

    /// <summary>
    /// An optional string that can be used as the description in a 
    /// view.
    /// </summary>
    /// <remarks>This value will not be localized.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string DescriptionAttribute = "OptionItem.DESCRIPTION_ATTRIBUTE";

    /// <summary>
    /// An alternative label for UI editors that will be used instead of the name.
    /// </summary>
    /// <remarks>This value will not be localized.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string DisplaynameAttribute = "OptionItem.DISPLAYNAME_ATTRIBUTE";

    /// <summary>
    /// Holds the default value for the item.
    /// </summary>
    /// <remarks>This value will not be localized.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string DefaultValueAttribute = "OptionItem.DEFAULT_VALUE_ATTRIBUTE";

    /// <summary>
    /// A custom representation string for <see langword="null"/> values in UI editors.
    /// </summary>
    /// <remarks>If not specified, a default value will be used (currently <c>(null))</c></remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string NullValueStringAttribute = "OptionItem.NULL_VALUE_STRING_ATTRIBUTE";

    //add overrides for editors and/or converters

    /// <summary>
    /// Allows to specify an alternative <see cref="TypeConverter"/> for UI editors.
    /// </summary>
    /// <remarks>You can either set an existing <see cref="TypeConverter"/> instance or
    /// the type of your custom converter here.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string CustomTypeConverterAttribute = "OptionItem.CUSTOM_CONVERTER_ATTRIBUTE";
    
    /// <summary>
    /// Allows to specify an alternative <see cref="IValueConverter"/> for UI editors.
    /// </summary>
    /// <remarks>You can either set an existing <see cref="IValueConverter"/> instance or
    /// the type of your custom converter here.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string CustomValueConverterAttribute = "OptionItem.CUSTOM_VALUE_CONVERTER_ATTRIBUTE";

    /// <summary>
    /// Allows to specify an alternative <see cref="DataTemplate"/> for the content
    /// of editors in the views.
    /// </summary>
    /// <remarks>The DataTemplate will be used for the items in a <see cref="IOptionItem"/>
    /// or <see cref="CollectionOptionItem{T}"/> to render the various items to choose from.
    /// The value of this attribute may be either a DataTemplate instance, the 
    /// <see cref="FrameworkElement.FindResource">resource identifier</see> that points to a <see cref="DataTemplate"/>,
    /// a Type that can be instanciated to a DataTemplate or a fully qualified name of a DataTemplate class.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string ItemTemplateAttribute = "OptionItem.ITEM_TEMPLATE_ATTRIBUTE";

    /// <summary>
    /// Allows to specify an alternative <see cref="DataTemplate"/> for UI editors in
    /// a table editor like view.
    /// </summary>
    /// <remarks>The DataTemplate needs to use an <see cref="IOptionItem"/> as the
    /// <see cref="DataTemplate.DataType"/> and needs to bind to the <see cref="IOptionItem.Value"/>
    /// for the value that needs to be edited.
    /// The value of this attribute may be either a DataTemplate instance, the 
    /// <see cref="FrameworkElement.FindResource">resource identifier</see> that points to a <see cref="DataTemplate"/>,
    /// a Type that can be instanciated to a DataTemplate or a fully qualified name of a DataTemplate class.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string CustomTableitemEditor = "OptionItem.CUSTOM_TABLEITEM_EDITOR";

    /// <summary>
    /// Allows to specify an alternative <see cref="DataTemplate"/> for UI editors in
    /// a dialog like view.
    /// </summary>
    /// <remarks>The DataTemplate needs to use an <see cref="IOptionItem"/> as the
    /// <see cref="DataTemplate.DataType"/> and needs to bind to the <see cref="IOptionItem.Value"/>
    /// for the value that needs to be edited.
    /// The value of this attribute may be either a DataTemplate instance, the 
    /// <see cref="FrameworkElement.FindResource">resource identifier</see> that points to a <see cref="DataTemplate"/>,
    /// a Type that can be instanciated to a DataTemplate or a fully qualified name of a DataTemplate class.</remarks>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string CustomDialogitemEditor = "OptionItem.CUSTOM_DIALOGITEM_EDITOR";

    /// <summary>
    /// Allows to specify an alternative internationalization prefix for the localization of this item.
    /// </summary>
    /// <seealso cref="IOptionItem.Attributes"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string CustomI18NPrefix = "OptionItem.I18N_PREFIX";

    /// <summary>
    /// Singleton instance for a object that marks an <c>undefined</c> value.
    /// </summary>
    /// <seealso cref="SetUndefined"/>
    /// <seealso cref="SupportUndefinedValueAttribute"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly object ValueUndefined = new UndefinedValueType();

    /// <summary>
    /// Allows to specify an alternative object that plays the role of <see langword="null"/> e.g. for value types.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public const string NullValueObject = "OptionItem.NULL_VALUE_OBJECT";

    #endregion

    #region constructors

    public OptionItem() : this(String.Empty) { }

    /// <summary>
    /// Create new ObjectOptionItem with given name and no initial value set.
    /// </summary>
    /// <remarks>The option is enabled by default.</remarks>
    /// <param name="name">The canonical (non-localized) name of the option</param>
    public OptionItem(string name) {
      if (name == null) {
        throw new ArgumentNullException("name", "Name must not be null");
      }
      this.name = name;
      attributes = new Dictionary<string, object>();
      attributesMapper = new DictionaryMapper<string, object>(attributes);
      //by default, null values and undefined values are supported
      attributes[SupportNullValueAttribute] = true;
      attributes[SupportUndefinedValueAttribute] = true;
      attributes[NullValueStringAttribute] = "(null)";
      Value = null;
      lookup = Lookups.CreateDictionaryLookup(dictionary);
    }

    /// <summary>
    /// Create new ObjectOptionItem with given name and initial value.
    /// </summary>
    /// <remarks>The option is enabled by default.</remarks>
    /// <param name="name">The canonical (non-localized) name of the option</param>
    /// <param name="initialValue">The initial value for this item.</param>
    protected OptionItem(string name, object initialValue)
      : this(name) {
      Value = initialValue;
    }

    /// <summary>
    /// Create new ObjectOptionItem with given name and no initial value set.
    /// </summary>
    /// <remarks>The option is enabled by default.</remarks>
    /// <param name="name">The canonical (non-localized) name of the option</param>
    /// <param name="attributes">An optional map of attributes for the item</param>
    protected OptionItem(string name, IDictionary<string, object> attributes) {
      if (String.IsNullOrEmpty(name)) {
        throw new ArgumentNullException("name", "Name must not be null or empty");
      }
      this.name = name;
      this.attributes = attributes;
      Value = null;
      lookup = Lookups.CreateDictionaryLookup(dictionary);
    }

    /// <summary>
    /// Create new ObjectOptionItem with given name and initial value.
    /// </summary>
    /// <remarks>The option is enabled by default.</remarks>
    /// <param name="name">The canonical (non-localized) name of the option</param>
    /// <param name="initialValue">The initial value for this item.</param>
    /// <param name="attributes">An optional map of attributes for the item</param>
    protected OptionItem(string name, object initialValue, IDictionary<string, object> attributes)
      : this(name, attributes) {
      Value = initialValue;
    }

    #endregion

    /// <summary>
    /// Get or set the owning group of an item
    /// </summary>
    public virtual IOptionGroup Owner {
      get { return _owner; }
      set { _owner = value; }
    }

    private Type type = typeof(object);

    /// <inheritdoc/>
    public Type Type {
      get { return type; }
      set {
        if (type != value) {
          type = value;
          OnPropertyChanged(new PropertyChangedEventArgs("Type"));
        }
      }
    }

    /// <inheritdoc/>
    public string Name {
      get { return name; }
      set {
        if (name != value) {
          name = value;
          OnPropertyChanged(new PropertyChangedEventArgs("Name"));
        }
      }
    }

    /// <inheritdoc/>
    public virtual bool Enabled {
      get {
        return enabled;
      }
      set {
        if (enabled != value) {
          enabled = value;
          OnPropertyChanged(enabledPCEA);
        }
      }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="args">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (PropertyChanged != null) {
        PropertyChanged(this, args);
      }
    }

//    internal bool SuppressEvents {
//      get {
//        OptionGroup owner = _owner as OptionGroup;
//        if (owner == null) {
//          return suppressEvents;
//        } else {
//          return owner.SuppressEvents && suppressEvents;
//        }
//      }
//      set { suppressEvents = value; }
//    }

    /// <summary>
    /// Get or set the value of this item.
    /// </summary>
    /// <remarks>When a new value is set, a <see cref="PropertyChanged"/> event is fired.</remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public virtual object Value {
      get {
        return _value;
      }
      set {
        //todo: check whether equals is appropriate?
        if (value == ValueUndefined &&
            !(bool) Attributes[SupportUndefinedValueAttribute])  {
          throw new ArgumentException("Undefined value is not supported for this OptionItem");
        }
        //TODO - SL - reenable - combobox.SelectedItem passes a null value the very first time
//        if (value == null && !(bool)GetAttribute(SupportNullValueAttribute)) {
//          throw new ArgumentException("Null value is not supported for this OptionItem");
//        }
        if (!Equals(_value, value)) {
          //          Trace.WriteLine("Setting new value for " + _name + " to: " + value);
          object oldValue = _value;
          _value = value;
          OnValueChanged(this, oldValue, value);
        }
      }
    }

    public IMapper<string, object> Attributes {
      get { return attributesMapper; }
    }

    /// <summary>
    /// Convenience method that allows to check whether an OptionItem has an undefined Value
    /// </summary>
    public bool IsUndefined {
      get { return _value == ValueUndefined; }
    }

    /// <summary>
    /// Convenience method to set the item state to an undefined value.
    /// </summary>
    /// <remarks>This is an atomic operation that overwrites the item's value with an
    /// <see cref="ValueUndefined"/> token</remarks>
    public void SetUndefined() {
      Value = ValueUndefined;
    }

    /// <inheritdoc/>
    public IList<string> GetAttributeKeys() {
      return (new List<string>(attributes.Keys)).AsReadOnly();
    }

    #region IOptionItem Members

    ///<inheritdoc/>
    public void SetLookup(Type t, object impl) {
      dictionary[t] = impl;
    }

    private readonly Dictionary<Type, object> dictionary = new Dictionary<Type, object>();
    private readonly ILookup lookup;

    private static readonly PropertyChangedEventArgs enabledPCEA = new PropertyChangedEventArgs("Enabled");
    private static readonly PropertyChangedEventArgs valuePCEA = new PropertyChangedEventArgs("Value");

    #endregion

    #region INotifyPropertyChanged Members

    /// <summary>
    /// This event gets fired (only) whenever the <see cref="Value"/> property is changed.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <inheritdoc/>
    protected void OnValueChanged(OptionItem source, object oldValue, object newValue) {
      if (PropertyChanged != null) {
        PropertyChanged(source, valuePCEA);
      }
    }

    #endregion

    #region undefined value support

    /// <summary>
    /// Type for objects that can be used as markers for undefined values
    /// </summary>
    internal sealed class UndefinedValueType : IEquatable<UndefinedValueType>
    {
      ///<summary>
      ///Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
      ///</summary>
      ///
      ///<returns>
      ///true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
      ///</returns>
      ///
      ///<param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>. </param><filterpriority>2</filterpriority>
      public override bool Equals(object obj) {
        if (obj == null) {
          return false;
        }
        //there can only be one undefined token in the whole framework
        if (ReferenceEquals(this, null)) {
          return false;
        }
        if (ReferenceEquals(this, obj)) {
          return true;
        }

        return obj.GetType().Equals(typeof(UndefinedValueType));
      }

      /// <summary>
      /// Override of equality operator
      /// </summary>
      /// <param name="left"></param>
      /// <param name="right"></param>
      /// <returns></returns>
      public static bool operator ==(Object left, UndefinedValueType right) {
        //delegate to Equals method
        return !ReferenceEquals(left, null) && left.Equals(right);
      }

      /// <summary>
      /// Override of inequality operator
      /// </summary>
      /// <param name="left"></param>
      /// <param name="right"></param>
      /// <returns></returns>
      public static bool operator !=(Object left, UndefinedValueType right) {
        return !(left == right);
      }

      public static bool operator ==(UndefinedValueType left, Object right) {
        //delegate to Equals method
        return !ReferenceEquals(left, null) && left.Equals(right);
      }

      public static bool operator !=(UndefinedValueType left, Object right) {
        return !(left == right);
      }

      ///<summary>
      ///Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
      ///</summary>
      ///
      ///<returns>
      ///A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
      ///</returns> 
      ///<filterpriority>2</filterpriority>
      public override string ToString() {
        return "Undefined";
      }

      public bool Equals(UndefinedValueType undefinedValueType) {
        if (ReferenceEquals(undefinedValueType, null)) {
          return false;
        }
        return true;
      }

      public override int GetHashCode() {
        return 0;
      }
    }
    #endregion

    #region ILookup Members

    ///<inheritdoc/>
    public virtual object Lookup(Type type) {
      if (type == typeof(IOptionItem)) {
        return this;
      }
      object o = lookup.Lookup(type);
      if (o == null && Owner != null) {
        return Owner.Lookup(type);
      }
      return o;
    }

    #endregion

    public static readonly string ValidationAttributesAttribute = "OptionItem.ValidationAttributes";
    public static readonly string ValidationRulesAttribute = "OptionItem.ValidationRules";
  }
}
