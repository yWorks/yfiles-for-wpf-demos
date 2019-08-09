/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using yWorks.Graph;

namespace Demo.yFiles.Option.Handler
{
  /// <summary>
  /// Option item that can have a list of valid entries for the item's value
  /// </summary>
  /// <typeparam name="T">The type of the entries.</typeparam>
  public class CollectionOptionItem<T> : OptionItem
  {
    private ICollection<T> _domain;

    /// <summary>
    /// If <see langword="true"/>, values that are not in the list of valid values are rejected.
    /// </summary>
    public const string USE_ONLY_DOMAIN_ATTRIBUTE = "CollectionOptionItem.USE_ONLY_DOMAIN_ATTRIBUTE";

    /// <summary>
    /// If <see langword="true"/>, values that are not in the list of valid values are rejected.
    /// </summary>
    public const string DOMAIN_ATTRIBUTE = "CollectionOptionItem.DOMAIN_ATTRIBUTE";

    /// <summary>
    /// If set to an instance, a type, or a fully qualified classname of
    /// a <see cref="DataTemplate"/> this one can be used to display the possible values in the UI.
    /// </summary>
    public const string ITEM_TEMPLATE_DISPLAY_ATTRIBUTE = "CollectionOptionItem.ITEM_TEMPLATE_DISPLAY_ATTRIBUTE";

    /// <summary>
    /// If set to an instance, a type, or a fully qualified classname of
    /// a <see cref="DataTemplate"/> this one can be used to edit a value in the UI.
    /// </summary>
    public const string ITEM_TEMPLATE_EDIT_ATTRIBUTE = "CollectionOptionItem.ITEM_TEMPLATE_EDIT_ATTRIBUTE";

    /// <summary>
    /// Create a new instance with the given name and an undefined initial value.
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="domain">The list of valid values</param>
    public CollectionOptionItem(string name, ICollection<T> domain)
      : base(name) {
      Type = typeof (ICollection<T>);
      _domain = domain;
      IEnumerator<T> enumerator = domain.GetEnumerator();
      if (enumerator.MoveNext()) {
        Value = enumerator.Current;
      }
      //by default, both types are not supported
      Attributes[SupportNullValueAttribute] = false;
      Attributes[SupportUndefinedValueAttribute] = true;
      Attributes[USE_ONLY_DOMAIN_ATTRIBUTE] = true;
      Attributes[DOMAIN_ATTRIBUTE] = _domain;
      AddLocalizingTemplate();
      Attributes[CustomDialogitemEditor] = "Domain.OptionItemPresenter";
    }

    private void AddLocalizingTemplate() {
      XamlLocalizingConverter converter = new XamlLocalizingConverter();
      converter.OptionItem = this;
      Attributes[OptionItem.ItemTemplateAttribute] = CreateTemplate(converter);
      // to keep a strong reference to the converter instance....
      Attributes["ITEM_TEMPLATE_DISPLAY_ATTRIBUTE_ConverterInstance"] = converter;
      converter.PostFix = ".VALUE.";
    }

    private DataTemplate CreateTemplate(IValueConverter converter) {
      DataTemplate dt = new DataTemplate();
      var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
      textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding(){Converter = converter});
      dt.VisualTree = textBlockFactory;
      return dt;
//      var id = DynamicValueConverter.Register(converter);
//      Assembly assembly = typeof(DynamicValueConverter).Assembly;
//      var assemblyName = new AssemblyName(assembly.FullName);
//      string xaml = "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
//                    "xmlns:UI='clr-namespace:yWorks.Option.Handler;assembly=" + assemblyName.Name + "'>" +
//                    "<TextBlock><TextBlock.Text>" +
//                    "<Binding>" +
//                    "<Binding.Converter>" +
//                    "<UI:DynamicValueConverter ConverterId='"+id+"'/>" +
//                    "</Binding.Converter>" +
//                    "</Binding>" +
//                    "</TextBlock.Text></TextBlock>" +
//                    "</DataTemplate>";
//      var result = (DataTemplate)XamlReader.Load(new XmlTextReader(new StringReader(xaml)));
//      return result;
    }


    /// <summary>
    /// Create a new instance with the given name.
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="domain">The list of valid values</param>
    /// <param name="value">The initial value for this item</param>
    public CollectionOptionItem(string name, ICollection<T> domain, object value)
      : base(name) {
      _domain = domain;
      Attributes[SupportNullValueAttribute] = false;
      Attributes[SupportUndefinedValueAttribute] = true;
      Attributes[USE_ONLY_DOMAIN_ATTRIBUTE] = true;
      Attributes[DOMAIN_ATTRIBUTE] = _domain;
      Attributes[CustomDialogitemEditor] = "Domain.OptionItemPresenter";
      AddLocalizingTemplate();
      Value = value;
    }


    /// <summary>
    /// Gets the domain this item is working on.
    /// </summary>
    /// <value>The domain.</value>
    public ICollection<T> Domain {
      get { return _domain; }
    }

    /// <summary>
    /// Gets the type of the entries.
    /// </summary>
    /// <value>The type of the entries.</value>
    public Type EntryType {
      get { return typeof (T); }
    }

    /// <summary>
    /// Get or set the value of this item.
    /// </summary>
    //[TypeConverter(typeof(ListTypeConverter))]
    public override object Value {
      get { return base.Value; }
      set {
        if (value is T) {
          object attr = Attributes[USE_ONLY_DOMAIN_ATTRIBUTE];
          if (attr != null && attr is bool && (bool) attr) {
            if (_domain.Contains((T) value)) {
              base.Value = value;
            } else {
              throw new ArgumentException("Value " + value + " not in value domain");
            }
          } else {
            base.Value = value;
          }
        } else if (value == null || value.Equals(ValueUndefined)) {
          base.Value = value;
        }
          //else if (value == VALUE_UNDEFINED && (bool)GetAttribute(SUPPORT_UNDEFINED_VALUE_ATTRIBUTE))
          //{
          //    base.Value = value;
          //}
        else {
          throw new ArgumentException("Value " + value + " has invalid type");
        }
      }
    }
  }

  [Obfuscation(ApplyToMembers = false, Exclude = true, StripAfterObfuscation = false)]
  public sealed class DynamicValueConverter : IValueConverter
  {
    private int id;

    private static int idCounter;

    private static readonly DictionaryMapper<int, WeakReference> staticLookup = new DictionaryMapper<int, WeakReference>();
    private IValueConverter converter;

    internal static int Register(IValueConverter converter) {
      idCounter++;
      staticLookup[idCounter] = new WeakReference(converter);
      return idCounter;
    }

    [Obfuscation(Exclude = true, StripAfterObfuscation = false)]
    public int ConverterId {
      get { return id; }
      set {
        id = value;
        WeakReference weakReference = staticLookup[id];
        if (weakReference.Target != null) {
          this.converter = weakReference.Target as IValueConverter;
        } else {
          staticLookup.RemoveValue(id);
          this.converter = null;
        }
      }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (converter != null) {
        return converter.Convert(value, targetType, parameter, culture);
      } else {
        throw new NotSupportedException();
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (converter != null) {
        return converter.ConvertBack(value, targetType, parameter, culture);
      } else {
        throw new NotSupportedException();
      }
    }
  }
}
