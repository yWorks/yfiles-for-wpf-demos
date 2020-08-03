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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Demo.yFiles.Option.Handler;
using yWorks.Annotations;
#if UseDataAnnotations
using System.ComponentModel.DataAnnotations;
#else
using DisplayAttribute = System.ComponentModel.DisplayNameAttribute;
#endif

using Trace=System.Diagnostics.Debug;

namespace Demo.yFiles.Option.Handler
{
  public static class OptionItemExtensions
  {
    public static bool IsDefaultAttribute(this Attribute att) {
      return att == null;
    }

    public static object GetValue(this IOptionGroup group, string groupName, string itemName) {
      return ((IOptionGroup)GetItemByName(group, groupName)).GetItemByName(itemName).Value;
    }

    public static IOptionGroup GetGroupByName(this IOptionGroup group, string groupName) {

      return (IOptionGroup) group.Items.FirstOrDefault(item => item is IOptionGroup && item.Name == groupName);
    }

    public static IOptionItem GetItemByName(this IOptionGroup group, string itemName) {
      string[] strings = itemName.Split('.');
      for (int i = 0; i < strings.Length - 1; i++ ) {
        group = group.GetGroupByName(strings[i]);
      }
      return group.Items.FirstOrDefault(item => item.Name == strings[strings.Length - 1]);
    }
  }
}

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// A generic implementation of an <see cref="IOptionBuilder"/> 
  /// that uses reflection and <see cref="Attribute"/>s to build the options
  /// of a given subject and type.
  /// </summary>
  /// <remarks>
  /// This implementation depends on the existence of the <see cref="DisplayNameAttribute"/>
  /// at the public <see cref="Type.GetProperties()">properties</see> of the provided type.
  /// That name is used to create the <see cref="IOptionItem"/>s and create the 
  /// <see cref="IOptionBuilderContext.CreateChildContext">child context for nested properties.</see>
  /// </remarks>
  /// <seealso cref="OptionBuilderAttribute"/>.
  /// <seealso cref="AttributeBasedPropertyMapBuilderAttribute"/>
  public class AttributeBasedOptionBuilder : IOptionBuilder
  {
    /// <inheritdoc/>
    public void AddItems(IOptionBuilderContext context, Type subjectType, object subject) {
      PropertyInfo[] propertyInfos =
        SortProperties(context, FilterProperties(context, subjectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)));

      foreach (PropertyInfo info in propertyInfos) {


        DisplayAttribute descriptionAttribute = GetAttribute<DisplayAttribute>(info);
#if UseDataAnnotations
        string description = descriptionAttribute == null ? info.Name : descriptionAttribute.GetName();
#else
        string description = descriptionAttribute == null ? info.Name : descriptionAttribute.DisplayName;
#endif

        object value = subject == null ? null : info.GetValue(subject, null);
        Type type = value == null ? info.PropertyType : value.GetType();

        IOptionBuilder builder = GetBuilder(context, info, subject, type);

        if (builder != null) {
          IOptionBuilderContext childContext = context.CreateChildContext(description);
          builder.AddItems(childContext, type, value);
          ConfigureItem((IOptionItem) childContext.Lookup(typeof (IOptionGroup)), info);
        } else {
          IOptionItem item = CreateItem(context, info, type, description, value);
          if (item != null) {
            context.BindItem(item, description);
            ConfigureItem(item, info);
          }
        }
      }
    }


    private IOptionBuilder GetBuilder(IOptionBuilderContext context, PropertyInfo info, object subject, Type type) {
      IOptionBuilder builder = null;
      OptionBuilderAttribute builderAttribute =
        (OptionBuilderAttribute) Attribute.GetCustomAttribute(info, typeof (OptionBuilderAttribute));
      if (builderAttribute != null) {
        builder = Activator.CreateInstance(builderAttribute.OptionBuilderType) as IOptionBuilder;
      } else {
        object value = subject != null ? info.GetValue(subject, null) : null;
        builder = context.GetOptionBuilder(type, value);
      }
      return builder;
    }

    [CanBeNull]
    private static T GetAttribute<T>(PropertyInfo info) where T : Attribute {
      return (T) Attribute.GetCustomAttribute(info, typeof (T));
    }

    /// <summary>
    /// Configures additional attributes for an option item or option group, such as collapsed state and group visibility.
    /// </summary>
    /// <param name="item">The item or group to configure.</param>
    /// <param name="propertyInfo">The associated property info.</param>
    protected virtual void ConfigureItem(IOptionItem item, PropertyInfo propertyInfo) {
      NullableAttribute nullableAttribute =
        (NullableAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof (NullableAttribute));
      if (nullableAttribute != null) {
        item.Attributes[OptionItem.SupportNullValueAttribute] = nullableAttribute.IsNullable;
      } else {
        CanBeNullAttribute canBeNull=
          (CanBeNullAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CanBeNullAttribute));
        NotNullAttribute notNull =
          (NotNullAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(NotNullAttribute));
        if (canBeNull != null) {
          item.Attributes[OptionItem.SupportNullValueAttribute] = true;
        } else if (notNull != null) {
          item.Attributes[OptionItem.SupportNullValueAttribute] = false;
        }
      }

      DescriptionAttribute attribute = GetAttribute<DescriptionAttribute>(propertyInfo);
      if (attribute != null && !attribute.IsDefaultAttribute()) {
        item.Attributes[OptionItem.DescriptionAttribute] = attribute.Description;
      }

      var customAttributes = Attribute.GetCustomAttributes(propertyInfo);

      foreach (var attrib in customAttributes.OfType<OptionItemAttributeAttribute>()) {
        item.Attributes[attrib.Name] = attrib.Value;
      }
    }

    /// <summary>
    /// Sorts the list of displayed properties.
    /// </summary>
    /// <remarks>By default, properties are sorted alphabetically by display name.</remarks>
    /// <param name="context"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    protected virtual PropertyInfo[] SortProperties(IOptionBuilderContext context, PropertyInfo[] properties) {
      Array.Sort(properties, new DefaultPropertyInfoComparer(context));
      return properties;
    }

    /// <summary>
    /// Filters the list of properties so that only the desired ones are used.
    /// </summary>
    /// <remarks>By default, all properties are included.</remarks>
    /// <param name="context"></param>
    /// <param name="properties"></param>
    /// <returns></returns>    
    private PropertyInfo[] FilterProperties(IOptionBuilderContext context, PropertyInfo[] properties) {
      List<PropertyInfo> infos = new List<PropertyInfo>();
      foreach (var i in properties) {
        if (i.CanRead && i.CanWrite) {
          var getter = i.GetGetMethod(false);
          var setter = i.GetSetMethod(false);
          if (getter != null && setter != null) {
            if (!setter.IsStatic && !getter.IsStatic) {
              infos.Add(i);
            }
          }
        }
      }
      return infos.ToArray();
    }

    /// <summary>
    /// Factory method that creates the option item using the provided parameters.
    /// </summary>
    protected virtual IOptionItem CreateItem(IOptionBuilderContext context, PropertyInfo propertyInfo, Type type,
                                             string description, object value) {
      IOptionItem item = null;
//      if (type.IsEnum) {
//        Type genericType = typeof (GenericOptionItem<>);
//        Type newType = genericType.MakeGenericType(type);
//        item = newType.GetConstructor(new Type[] {typeof (string)}).Invoke(new object[] {description}) as IOptionItem;
//      } else if (type == typeof (Color)) {
//        item = new ColorOptionItem(description);
      //  } else 
      if (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom((typeof (ICollection<>)))) {
        Type[] types = type.GetGenericArguments();
        if (types.Length == 1) {
          Type collectionItemType = types[0];
          Type collectionBaseType = typeof (ICollection<>);
          Type collectionType = collectionBaseType.MakeGenericType(collectionItemType);
          Type collectionOptionItemType = typeof (CollectionOptionItem<>);
          item = collectionOptionItemType.MakeGenericType(collectionItemType).GetConstructor(
            new Type[] {typeof (string), collectionType}).Invoke(new object[] {description, value}) as
                 IOptionItem;
        }
      }

      if (item == null) {
        if (type.IsValueType) {
          if (IsNullable(type)) {
            item = new OptionItem() {Type = Nullable.GetUnderlyingType(type), Name = description};
            item.Attributes[OptionItem.SupportNullValueAttribute] = true;
          } else if(IsNullable(propertyInfo.PropertyType)) {
            item = new OptionItem() {Type = type, Name = description};
            item.Attributes[OptionItem.SupportNullValueAttribute] = true;
          } else {
            item = new OptionItem() {Type = type, Name = description};
            item.Attributes[OptionItem.SupportNullValueAttribute] = false;
          }
        }
        else {
          item = new OptionItem() { Type = type, Name = description };
        }
      }
      var customAttributes = Attribute.GetCustomAttributes(propertyInfo);

      foreach (var attribute in customAttributes.OfType<OptionItemAttributeAttribute>()) {
        item.Attributes[attribute.Name] = attribute.Value;
      }

      TypeConverterAttribute converter = GetAttribute<TypeConverterAttribute>(propertyInfo);
      if (converter != null && !converter.IsDefaultAttribute()) {
        try {
          Type typeConverter = Type.GetType(converter.ConverterTypeName);
          if (typeConverter != null) {
            item.Attributes[OptionItem.CustomTypeConverterAttribute] = typeConverter;
          }
        } catch (Exception e) {
          Trace.WriteLine("Could not load custom type converter " + e.Message);
        }
      }

      return item;
    }

    private bool IsNullable(Type type) {
      return type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom((typeof(Nullable<>)));
    }


    private class DefaultPropertyInfoComparer : IComparer<PropertyInfo>
    {
      public DefaultPropertyInfoComparer(IOptionBuilderContext context) {
        this.context = context;
      }

      private IOptionBuilderContext context;

      #region IComparer<PropertyInfo> Members

      public int Compare(PropertyInfo x1, PropertyInfo x2) {
        
        DisplayAttribute descriptionAttribute1 = GetAttribute<DisplayAttribute>(x1);
        DisplayAttribute descriptionAttribute2 = GetAttribute<DisplayAttribute>(x2);

#if UseDataAnnotations
        string s1 = descriptionAttribute1 == null ? x1.Name : descriptionAttribute1.GetName();
        string s2 = descriptionAttribute2 == null ? x2.Name : descriptionAttribute2.GetName();
        return s1.CompareTo(s2);
#else
        string s1 = descriptionAttribute1 == null ? x1.Name : descriptionAttribute1.DisplayName;
        string s2 = descriptionAttribute2 == null ? x2.Name : descriptionAttribute2.DisplayName;
        return s1.CompareTo(s2);
#endif
      }

      #endregion
    }
  }

  /// <summary>
  /// The attribute that is used to specify the <see cref="IPropertyMapBuilder"/>
  /// implementation type for a given type or property.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
  public class PropertyMapBuilderAttribute : Attribute
  {
    private readonly Type propertyMapBuilderType;

    /// <summary>
    /// Creates a new instance using the provided type.
    /// </summary>
    /// <param name="propertyMapBuilderType"></param>
    public PropertyMapBuilderAttribute(Type propertyMapBuilderType) {
      this.propertyMapBuilderType = propertyMapBuilderType;
    }

    /// <summary>
    /// Returns the type to use for the <see cref="IPropertyMapBuilder"/> implementation.
    /// </summary>
    public Type PropertyMapBuilderType {
      get { return propertyMapBuilderType; }
    }
  }

  /// <summary>
  /// The attribute that is used to specify that the <see cref="IPropertyMapBuilder"/>
  /// implementation for the annotated type should be created dynamically by
  /// introspecting the type.
  /// </summary>
  /// <seealso cref="AttributeBasedOptionBuilder"/>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
  public class AttributeBasedPropertyMapBuilderAttribute : Attribute
  {
    /// <summary>
    /// Creates the builder for a given type.
    /// </summary>
    /// <param name="t">The type to create an attribute based builder for.</param>
    /// <returns>A builder that is based on the attributes found in the type.</returns>
    public IPropertyMapBuilder CreateBuilder(Type t) {
      while (t != null && t.IsClass && !IsPublic(t)) {
        t = t.BaseType;
      }
      return
        (IPropertyMapBuilder)
        (typeof (AttributeBasedPropertyMapBuilder<>).MakeGenericType(t).GetConstructor(Type.EmptyTypes).Invoke(null));
    }

    private static bool IsPublic(Type type) {
      return type.IsPublic || (type.IsNestedPublic && IsPublic(type.DeclaringType));
    }
  }
 
  
  /// <summary>
  /// The attribute that is used to specify that the <see cref="IPropertyMapBuilder"/>
  /// implementation for the annotated type should be created dynamically by
  /// introspecting the type.
  /// </summary>
  /// <seealso cref="AttributeBasedOptionBuilder"/>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = true)]
  public class OptionItemAttributeAttribute : Attribute
  {
    public string Name { get; set; }
    public object Value { get; set; }
  }

  /// <summary>
  /// Determines whether the annotated property may be <see langword="null"/> during the 
  /// creation of an <see cref="AttributeBasedOptionBuilder">attribute based</see>
  /// <see cref="IPropertyMap"/>.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class NullableAttribute : Attribute
  {
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public NullableAttribute(bool isNullable) {
      this.isNullable = isNullable;
    }

    /// <summary>
    /// Whether the value may be set to <see langword="null"/>.
    /// </summary>
    public bool IsNullable {
      get { return isNullable; }
    }

    private bool isNullable;
  }

  /// <summary>
  /// Attribute that will be evaluated by <see cref="AttributeBasedPropertyMapBuilderAttribute"/>
  /// during the creation of the <see cref="IPropertyMap"/> to determine
  /// the <see cref="IPropertyBuildContext{TSubject}.Policy"/>.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class AssignmentPolicyAttribute : Attribute
  {
    private readonly AssignmentPolicy policy;

    /// <summary>
    /// Creates a new instance using the provided policy.
    /// </summary>
    /// <param name="policy"></param>
    public AssignmentPolicyAttribute(AssignmentPolicy policy) {
      this.policy = policy;
    }

    /// <summary>
    /// Retrieves the policy to use.
    /// </summary>
    public AssignmentPolicy Policy {
      get { return policy; }
    }
  }

  /// <summary>
  /// An enumeration used by <see cref="IPropertyBuildContext{TSubject}.Policy"/>
  /// to determine how the modification of mutable reference values should be performed.
  /// </summary>
  /// <seealso cref="IPropertyBuildContext{TSubject}.SetNewInstance"/>
  public enum AssignmentPolicy
  {
    /// <summary>
    /// The client does not have any preferences, the context itself may decide what to do.
    /// </summary>
    Default,
    /// <summary>
    /// The context may modify the instance, there is no need to 
    /// stay immutable.
    /// </summary>
    ModifyInstance,
    /// <summary>
    /// The context may not be modified, instead a new instance needs to be created that reflects
    /// the modification.
    /// </summary>
    CreateNewInstance
  }
}
