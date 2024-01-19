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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Demo.yFiles.Option.Handler;
using yWorks.Annotations;

namespace Demo.yFiles.Option.DataBinding
{
  internal static class PropertyInfoExtensions
  {
    private static IDictionary<KeyValuePair<MemberInfo, Type>, Attribute> attributeCache =
      new Dictionary<KeyValuePair<MemberInfo, Type>, Attribute>();

    public static object GetValue(this PropertyInfo p, object o) {
      return p.GetValue(o, null);
    }

    [CanBeNull]
    public static T GetAttribute<T>([NotNull] this MemberInfo info) where T : Attribute {
      KeyValuePair<MemberInfo, Type> key = new KeyValuePair<MemberInfo, Type>(info, typeof (T));
      Attribute retval;
      if (attributeCache.TryGetValue(key, out retval)) {
        return retval as T;
      } else {
        retval = Attribute.GetCustomAttribute(info, typeof (T), true);
        attributeCache[key] = retval;
        return retval as T;
      }
    }

    [CanBeNull]
    public static T GetAttribute<T>([NotNull] this PropertyInfo info) where T : Attribute {
      return GetAttribute<T>((MemberInfo) info);
      //return (T)Attribute.GetCustomAttribute(info, typeof(T), true);
    }

    public static string GetDisplayName(this PropertyInfo info) {
      var displayNameAttribute = info.GetAttribute<DisplayNameAttribute>();
      if (displayNameAttribute != null) {
        return displayNameAttribute.DisplayName;
      } else {
        return info.Name;
      }
    }
  }


  public class AttributeBasedPropertyMapBuilder<TSubject> : PropertyMapBuilderBase<TSubject> where TSubject : class
  {
    public AttributeBasedPropertyMapBuilder() : base(true) {}

    protected override void BuildPropertyMapImpl(IPropertyBuildContext<TSubject> builder) {
      MethodInfo[] infos =
        typeof (IPropertyBuildContext<>).MakeGenericType(typeof (TSubject)).GetMethods(BindingFlags.DeclaredOnly |
                                                                                       BindingFlags.Instance |
                                                                                       BindingFlags.Public);
      MethodInfo buildChildContextMethod =
        infos.FirstOrDefault(info => info.IsGenericMethod && info.ReturnType.IsGenericType);
      if (buildChildContextMethod == null) {
        throw new Exception("Method not found!");
      }

      TSubject instance = builder.CurrentInstance;

      var properties = instance.GetType().GetProperties();
      foreach (PropertyInfo d  in properties) {
        AddProperty(builder, buildChildContextMethod, d);
      }
    }

    private void AddProperty(IPropertyBuildContext<TSubject> context, MethodInfo buildChildContextMethod, PropertyInfo d) {
      PropertyInfo descriptor = d;
      Type propertyType = descriptor.PropertyType;

      AssignmentPolicyAttribute assignmentPolicyAttribute = descriptor.GetAttribute<AssignmentPolicyAttribute>();

      NullableAttribute nullableAttribute = descriptor.GetAttribute<NullableAttribute>();
      bool nullable = nullableAttribute == null || nullableAttribute.IsNullable;

      AssignmentPolicy policy;

      if (assignmentPolicyAttribute != null && context.Policy == AssignmentPolicy.Default) {
        policy = assignmentPolicyAttribute.Policy;
      } else {
        policy = context.Policy == AssignmentPolicy.Default ? AssignmentPolicy.CreateNewInstance : context.Policy;
      }

      object childbuilder;
      object currentPropertyValue = descriptor.GetValue(context.CurrentInstance, null);

      if (currentPropertyValue == null && nullable) {
        //todo: make this dependent on yet-to-implement nullable attribute
        childbuilder = context.GetPropertyMapBuilder(propertyType, currentPropertyValue);
      } else {
        childbuilder = context.GetPropertyMapBuilder(currentPropertyValue);
      }

      string name = descriptor.GetDisplayName();
      if (childbuilder != null) {
        // Create the SetInstanceDelegate that uses the builders' CurrentInstance for the set operation
        Delegate setMemberInstanceDelegate;
        {
          Type helperSetInstanceDelegateType = typeof (HelperSetInstanceDelegate<,>)
            .MakeGenericType(propertyType, typeof (TSubject));
          ConstructorInfo setterConstructorInfo = helperSetInstanceDelegateType
            .GetConstructor(new[] {typeof (PropertyInfo), typeof (IPropertyBuildContext<TSubject>)});
          object helperSetInstanceDelegateInstance = setterConstructorInfo
            .Invoke(new object[] {descriptor, context});

          MethodInfo method =
            helperSetInstanceDelegateType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public |
                                                     BindingFlags.Instance)[0];
          setMemberInstanceDelegate =
            Delegate.CreateDelegate(typeof (SetInstanceDelegate<>).MakeGenericType(propertyType),
                                    helperSetInstanceDelegateInstance, method);
        }

        // Create the SetInstanceDelegate that uses the builders' CurrentInstance for the set operation
        Delegate getMemberInstanceDelegate;
        {
          Type helperInstanceDelegateType = typeof (HelperGetInstanceDelegate<,>)
            .MakeGenericType(propertyType, typeof (TSubject));
          ConstructorInfo getterConstructorInfo = helperInstanceDelegateType
            .GetConstructor(new[] {typeof (PropertyInfo), typeof (IPropertyBuildContext<TSubject>)});
          object helperGetInstanceDelegateInstance = getterConstructorInfo
            .Invoke(new object[] {descriptor, context});

          MethodInfo method =
            helperInstanceDelegateType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public |
                                                  BindingFlags.Instance)[0];
          getMemberInstanceDelegate =
            Delegate.CreateDelegate(typeof (GetInstanceDelegate<>).MakeGenericType(propertyType),
                                    helperGetInstanceDelegateInstance, method);
        }
        object childContext = buildChildContextMethod.MakeGenericMethod(propertyType).Invoke(context,
                                                                                             new object[]
                                                                                               {
                                                                                                 name,
                                                                                                 getMemberInstanceDelegate
                                                                                                 ,
                                                                                                 setMemberInstanceDelegate
                                                                                                 ,
                                                                                                 policy
                                                                                               });

        MethodInfo buildPropertyMapMethod =
          typeof (IPropertyMapBuilder).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public |
                                                  BindingFlags.Instance)[0];

        buildPropertyMapMethod.MakeGenericMethod(propertyType).Invoke(childbuilder, new[] {childContext});
      } else {
        context.AddEntry(name, new ReflectionGetter<TSubject>(descriptor, context),
                         new ReflectionSetter<TSubject>(descriptor, context));
      }
    }

    private class ReflectionGetter<T> : IValueGetter where T : class
    {
      public ReflectionGetter(PropertyInfo getInstance, IPropertyBuildContext<T> context) {
        this.getInstance = getInstance;
        this.context = context;
      }

      private readonly PropertyInfo getInstance;
      private readonly IPropertyBuildContext<T> context;

      #region IValueGetter Members

      public object GetValue() {
        return getInstance.GetValue(context.CurrentInstance);
      }

      public bool CanGet() {
        return getInstance != null;
      }

      #endregion
    }

    private class ReflectionSetter<T> : IValueSetter where T : class
    {
      public ReflectionSetter(PropertyInfo setInstance, IPropertyBuildContext<T> context) {
        this.setInstance = setInstance;
        this.context = context;
      }

      private readonly PropertyInfo setInstance;
      private readonly IPropertyBuildContext<T> context;

      #region IValueGetter Members

      public bool CanSet() {
        return setInstance != null && !(setInstance.CanRead && !setInstance.CanWrite);
      }

      #endregion

      public void SetValue(object value) {
        setInstance.SetValue(context.CurrentInstance, value, null);
      }
    }
  }

  internal sealed class HelperGetInstanceDelegate<TChild, TSubject> where TSubject : class
  {
    private readonly PropertyInfo descriptor;
    private readonly IPropertyBuildContext<TSubject> context;

    public HelperGetInstanceDelegate(PropertyInfo descriptor, IPropertyBuildContext<TSubject> context) {
      this.descriptor = descriptor;
      this.context = context;
    }

    /// <summary>
    /// DO NOT REMOVE - USED THROUGH REFLECTION ABOVE!!!
    /// </summary>
    public TChild GetInstance() {
      return (TChild) descriptor.GetValue(context.CurrentInstance, null);
    }
  }

  internal sealed class HelperSetInstanceDelegate<TChild, TSubject> where TSubject : class
  {
    private readonly PropertyInfo descriptor;
    private readonly IPropertyBuildContext<TSubject> context;

    public HelperSetInstanceDelegate(PropertyInfo descriptor, IPropertyBuildContext<TSubject> context) {
      this.descriptor = descriptor;
      this.context = context;
    }

    /// <summary>
    /// DO NOT REMOVE - USED THROUGH REFLECTION ABOVE!!!
    /// </summary>
    public void SetValue(TChild value) {
      if (!(descriptor.CanRead && !descriptor.CanWrite)) {
        descriptor.SetValue(context.CurrentInstance, value, null);
      }
    }
  }
}