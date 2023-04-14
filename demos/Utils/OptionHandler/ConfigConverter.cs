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
using System.Reflection;

namespace Demo.yFiles.Toolkit.OptionHandler
{
  /// <summary>
  /// ConfigConverter is used to convert an input configuration into nested <see cref="Option"/>s usable by an option editor.
  /// </summary>
  /// <remarks>
  /// The output configuration <see cref="Option"/> is later interpreted by an option editor and turned into ui components.
  /// The input configuration is a regular class with additional attributes attached to public member declarations.
  /// These attributes specify how a member is turned into an ui component. 
  ///
  /// The input configuration format
  /// 
  /// Components:
  /// - a default <see cref="Option"/> is created for each public property and field according to the property/field's type.
  /// - components are available for built-in types: enum, bool, int, double, string
  ///
  /// Component labels:
  /// - component labels are defined using the <see cref="LabelAttribute"/>
  /// 
  /// Component minimum, maximum and step:
  /// - component minimum, maximum and step are defined using the <see cref="MinMaxAttribute"/>  
  /// 
  /// Component type:
  /// - component type is one of <see cref="ComponentTypes"/> and defined using the <see cref="ComponentTypeAttribute"/>
  /// 
  /// Component grouping and ordering:
  /// - public fields and properties with an <see cref="ComponentTypeAttribute"/> with value <see cref="ComponentTypes.OptionGroup"/> are interpreted as ui groups
  /// - <see cref="Option"/> or <see cref="OptionGroup"/>s are assigned to another <see cref="OptionGroup"/> using the <see cref="OptionGroupAttribute"/> 
  /// - if a members <see cref="OptionGroupAttribute"/> has name "RootGroup" or there is no <see cref="OptionGroupAttribute"/> the corresponding component is added to the toplevel group.
  /// - display order of components is defined through position parameter of <see cref="OptionGroupAttribute"/> 
  /// 
  /// Conditionally disabling a component:
  /// - a public boolean property named "ShouldDisable[memberName]" defines a condition to disable the component created for the corresponding member.
  /// 
  /// </remarks>
  public class ConfigConverter
  {
    // all members that need to be added to the top-level OptionGroup - including groups
    private List<MemberInfo> toplevelItems = new List<MemberInfo>();
    // the methods - methods aren't used right now
    private List<MethodInfo> methods = new List<MethodInfo>();
    
    // map a group name to a list of its inner members
    private Dictionary<string, List<MemberInfo>> groupMapping = new Dictionary<string, List<MemberInfo>>();

    // map for the ShouldDisable functions for properties
    private Dictionary<string, Func<bool>> isDisabledMapping = new Dictionary<string, Func<bool>>();

    // map for the ShouldHide functions for properties
    private Dictionary<string, Func<bool>> isHiddenMapping = new Dictionary<string, Func<bool>>();

    /// <summary>
    /// Convert the input configuration into an <see cref="OptionGroup"/> usable by an option editor.
    /// </summary>
    /// <remarks>
    /// First, all members are *collected* and stored in their respective maps and lists.
    /// Next, members are sorted by the position information contained in the corresponding <see cref="OptionGroupAttribute"/>.
    /// Finally, all members are visited and the extracted data relevant to build ui components are written to
    /// an <see cref="Option"/>.
    /// </remarks>    
    /// <param name="config">The input configuration written by the developer.</param>
    /// <returns>The output configuration usable by an option editor.</returns>
    public OptionGroup Convert(object config) {
      var type = config.GetType();

      // collect members
      CollectMembers(type, config);

      // sort members
      toplevelItems.Sort(new MemberComparer());
      foreach (KeyValuePair<string, List<MemberInfo>> entry in groupMapping) {
        entry.Value.Sort(new MemberComparer());
      }

      // visit members
      var topLevelGroup = new OptionGroup();
      SetLabel(type, topLevelGroup);
      VisitMembers(topLevelGroup, config);
      return topLevelGroup;
    }

    /// <summary>
    /// Write member information into <see cref="Option"/>s.
    /// <remarks>
    /// The function basically dispatches between different visitor functions.
    /// </remarks>
    /// </summary>
    /// <param name="optionGroup">The OptionGroup to contain the configuration.</param>
    /// <param name="config">The input configuration.</param>
    private void VisitMembers(OptionGroup optionGroup, object config) {
      foreach (var member in toplevelItems) {
        VisitMember(member, optionGroup, config);
      }
      foreach (var method in methods) {
        VisitMethod(method, optionGroup, config);
      }
    }

    /// <summary>
    /// Collect all public members of a given type and store them in the respective field.
    /// </summary>
    /// <param name="type">The type object to collect members for.</param>
    /// <param name="config">The input configuration.</param>
    private void CollectMembers(Type type, object config) {
      ClearMappings();
      var memberInfos = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

      foreach (var member in memberInfos) {
        CollectMember(member, config);
      }
    }

    /// <summary>
    /// Clear members from dictionaries and lists.
    /// </summary>
    private void ClearMappings() {
      groupMapping.Clear();
      toplevelItems.Clear();
      methods.Clear();
      isDisabledMapping.Clear();
      isHiddenMapping.Clear();
    }

    /// <summary>
    /// Collect a member.
    /// </summary>
    /// <remarks>
    /// The function dispatches between different collect functions based on type of member.
    /// </remarks>
    /// <param name="member">The member to be processed.</param>
    /// <param name="config">The input configuration.</param>
    private void CollectMember(MemberInfo member, object config) {
      if (CollectUtilityProperty(member, config)) {
        return;
      }
      if (member is FieldInfo) {
        CollectField((FieldInfo) member);
      } else if (member is PropertyInfo) {
        CollectProperty((PropertyInfo) member);
      } else if (member is MethodInfo) {
        CollectMethod((MethodInfo) member);
      }
    }

    /// <summary>
    /// Collect an utility property that disables a control.
    /// </summary>
    /// <param name="member">The member to be processed.</param>
    /// <param name="config">The input configuration.</param>
    /// <returns><see langword="true"/> if property is an utility property</returns>
    private bool CollectUtilityProperty(MemberInfo member, object config) {
      if (StartsWith(member.Name, "ShouldHide")) {
        var invoker = GetInvoker(member, config);
        if (invoker != null) {
          isHiddenMapping.Add(member.Name.Substring(10).ToLower(), invoker);
          return true;
        }
      }
      if (StartsWith(member.Name, "ShouldDisable")) {
        var invoker = GetInvoker(member, config);
        if (invoker != null) {
          isDisabledMapping.Add(member.Name.Substring(13).ToLower(), invoker);
          return true;
        }
      }
      return false;
    }

    private static bool StartsWith(string text, string pattern) {
      return text.Length > pattern.Length && text.Substring(0, pattern.Length) == pattern;
    }

    private Func<bool> GetInvoker(MemberInfo member, object config) {
      var fieldInfo = member as FieldInfo;
      if (fieldInfo != null) {
        if (fieldInfo.FieldType == typeof (bool)) {
          return (() => (bool) fieldInfo.GetValue(config));
        }
      }
      var propertyInfo = member as PropertyInfo;
      if (propertyInfo != null) {
        if (propertyInfo.PropertyType == typeof (bool)) {
          return (() => (bool) propertyInfo.GetValue(config, null));
        }
      }
      var methodInfo = member as MethodInfo;
      if (methodInfo != null) {
        if (methodInfo.ReturnType == typeof (bool)) {
          return (() => (bool) methodInfo.Invoke(config, null));
        }
      }

      return null;
    }

    /// <summary>
    /// Collect a field.
    /// </summary>
    /// <remarks>
    /// If the field has an <see cref="OptionGroupAttribute"/>, add it to the corresponding <see cref="groupMapping"/>, otherwise add it
    /// to <see cref="toplevelItems"/>.
    /// </remarks>    
    /// <param name="field">The field to process.</param>
    private void CollectField(FieldInfo field) {
      string group = GetGroup(field);
      if (group != null && group != "rootgroup") {
        if (!groupMapping.ContainsKey(group)) {
          groupMapping[group] = new List<MemberInfo>();
        }
        groupMapping[group].Add(field);
      } else {
        toplevelItems.Add(field);
      }
    }

    /// <summary>
    /// Collect a property.
    /// </summary>
    /// <remarks>
    /// If the property has an <see cref="OptionGroupAttribute"/>, add it to the corresponding <see cref="groupMapping"/>, otherwise add it
    /// to <see cref="toplevelItems"/>.
    /// </remarks>
    /// <param name="property">The property to process.</param>
    private void CollectProperty(PropertyInfo property) {
      string group = GetGroup(property);
      if (group != null && group != "rootgroup") {
        if (!groupMapping.ContainsKey(group)) {
          groupMapping[group] = new List<MemberInfo>();
        }
        groupMapping[group].Add(property);
      } else {
        toplevelItems.Add(property);
      }
    }

    /// <summary>
    /// Collect a method.
    /// </summary>
    /// <remarks>
    /// The method is added to the <see cref="methods"/> list.
    /// </remarks>
    /// <param name="method">The method to process.</param>
    private void CollectMethod(MethodInfo method) {
      if (method.Name != "getClass") {
        methods.Add(method);
      }
    }

    /// <summary>
    /// Visit a member.
    /// </summary>
    /// <remarks>
    /// The function dispatches between different visit functions based on type of the member.
    /// </remarks>
    /// <param name="member">The member to visit.</param>
    /// <param name="optionGroup">The output data object.</param>
    /// <param name="config">The input configuration.</param>
    private void VisitMember(MemberInfo member, OptionGroup optionGroup, object config) {
      if (ShouldIgnoreMember(member)) {
        return;
      }
      if (IsOptionGroup(member)) {
        optionGroup.ChildOptions.Add(VisitGroup(member, config));
      } else {
        if (member is FieldInfo) {
          optionGroup.ChildOptions.Add(VisitField((FieldInfo)member, config));
        } else if (member is PropertyInfo) {
          optionGroup.ChildOptions.Add(VisitProperty((PropertyInfo)member, config));
        }
      }
    }

    /// <summary>
    /// Visit a field and return an <see cref="Option"/> with all relevant information to build a component.
    /// </summary>
    /// <remarks>
    /// The following information is written:
    /// - the internally used name
    /// - the value type of the field
    /// - the text label
    /// - the options if it has <see cref="EnumValueAttribute"/>s
    /// - the initial value
    /// - the default value
    /// - the component type
    /// - the utility properties (condition to disable the component)
    /// - all custom attributes
    /// </remarks>
    /// <param name="field">The field to visit.</param>
    /// <param name="config">The input configuration.</param>
    /// <returns>a new <see cref="Option"/> containing all information collected for the field</returns>
    private Option VisitField(FieldInfo field, object config) {
      var f = new Option();

      f.Name = field.Name;
      f.ValueType = field.FieldType;

      SetLabel(field, f);
      SetEnumValues(field, field.FieldType, f);

      f.Getter = () => field.GetValue(config);
      f.Setter = value => field.SetValue(config, value);
      f.DefaultValue = f.Getter();

      SetComponent(field, field.FieldType, f);
      SetUtilityProperties(field, f);
      foreach (var attribute in field.GetCustomAttributes(false)) {
        VisitAttribute((Attribute) attribute, f);
      }
      return f;
    }

    /// <summary>
    /// Visit a property and return an <see cref="Option"/> with all relevant information to build a component.    
    /// </summary>
    /// <remarks>
    /// The following information is written:
    /// - the internally used name
    /// - the value type of the field
    /// - the text label
    /// - the options if it has <see cref="EnumValueAttribute"/>s
    /// - the initial value
    /// - the default value
    /// - the component type
    /// - the utility properties (condition to disable the component)
    /// - all custom attributes
    /// </remarks>
    /// <param name="property">The property to visit.</param>
    /// <param name="config">The input configuration.</param>
    /// <returns>a new <see cref="Option"/> containing all information collected for the property</returns>
    private Option VisitProperty(PropertyInfo property, object config) {
      var p = new Option();

      p.Name = property.Name;
      p.ValueType = property.PropertyType;

      SetLabel(property, p);
      SetEnumValues(property, property.PropertyType, p);

      p.Getter = () => property.GetValue(config, null);
      p.Setter = value => property.SetValue(config, value, null);
      p.DefaultValue = p.Getter();

      SetComponent(property, property.PropertyType, p);
      SetUtilityProperties(property, p);
      foreach (var attribute in property.GetCustomAttributes(false)) {
        VisitAttribute((Attribute) attribute, p);
      }
      return p;
    }

    private static void VisitMethod(MethodInfo method, OptionGroup optionGroup, object config) {
      // methods aren't used right now
    }

    /// <summary>
    /// Visit an attribute and set all relevant information on <paramref name="obj"/> to build a component.
    /// </summary>
    /// <remarks>
    /// The following information is written:
    /// - minimum value
    /// - maximum value
    /// - increment/decrement step
    /// </remarks>    
    /// <param name="attribute">The attribute to process.</param>
    /// <param name="obj">The object containing the extracted information</param>
    private void VisitAttribute(Attribute attribute, Option obj) {
      if (attribute is MinMaxAttribute) {
        obj.MinMax = (MinMaxAttribute) attribute;
      }
    }

    /// <summary>
    /// Visit a group and return an <see cref="OptionGroup"/> with all relevant information to build a component.
    /// </summary>
    /// <remarks>
    /// The following information is written:
    /// - group name
    /// - text label
    /// - custom attributes
    /// <para>
    /// If the <see cref="groupMapping"/> contains child members of this groups, these are <see cref="VisitMember">visited</see> as well.
    /// </para>
    /// </remarks>
    /// <param name="groupMember">The group to process.</param>
    /// <param name="config">The input configuration.</param>
    private OptionGroup VisitGroup(MemberInfo groupMember, object config) {
      var g = new OptionGroup();
      g.Name = groupMember.Name;
      g.ComponentType = ComponentTypes.OptionGroup;
      SetLabel(groupMember, g);
      object[] attrs = groupMember.GetCustomAttributes(false);
      foreach (var attribute in attrs) {
        VisitAttribute((Attribute) attribute, g);
      }
      var name = g.Name.ToLower();
      if (groupMapping.ContainsKey(name)) {
        var memberInfos = groupMapping[name];
        foreach (var memberInfo in memberInfos) {
          VisitMember(memberInfo, g, config);
        }
      }
      return g;
    }

    /// <summary>
    /// Sets the component type to <paramref name="obj"/>.
    /// </summary>
    /// <remarks>
    /// If a <see cref="ComponentTypeAttribute"/> is set, the given component type is set. 
    /// If an <see cref="EnumValueAttribute"/> is present <see cref="ComponentTypes.Combobox"/> is set.
    /// Otherwise a default component is determined based on <paramref name="type"/>.
    /// </remarks>
    /// <param name="member">The member to process.</param>
    /// <param name="type">The type of the member.</param>
    /// <param name="obj">The <see cref="Option"/> containing the extracted information.</param>
    private void SetComponent(MemberInfo member, Type type, Option obj) {
      if (member.GetCustomAttributes(typeof (ComponentTypeAttribute), false).Length > 0) {
        ComponentTypeAttribute attr = (ComponentTypeAttribute) member.GetCustomAttributes(typeof (ComponentTypeAttribute), false)[0];
        obj.ComponentType = attr.Value;
      } else if (member.GetCustomAttributes(typeof (EnumValueAttribute), false).Length > 0) {
        // use combobox for members with EnumValues attribute
        obj.ComponentType = ComponentTypes.Combobox;
      } else {
        obj.ComponentType = GetDefaultComponent(type);
      }
    }

    /// <summary>
    /// Sets the label text on <paramref name="obj"/>.
    /// </summary>
    /// <param name="member">The member to process.</param>
    /// <param name="obj">The <see cref="Option"/> containing the extracted information.</param>
    private void SetLabel(MemberInfo member, Option obj) {
      var attributes = member.GetCustomAttributes(typeof (LabelAttribute), false);
      string label;
      if (attributes.Length > 0) {
        LabelAttribute attr = (LabelAttribute) attributes[0];
        label = attr.Label;
      } else {
        label = member.Name;
      }
      obj.Label = label;
    }

    /// <summary>
    /// Sets the <see cref="Option.EnumValues"/> to <paramref name="obj"/>.
    /// </summary>
    /// <remarks>
    /// If an <see cref="EnumValueAttribute"/> is set, options are extracted from it.
    /// If the member is of type enum, options are extracted from the enum.
    /// </remarks>
    /// <param name="member">The member to process.</param>
    /// <param name="type">The type of the member.</param>
    /// <param name="obj">The <see cref="Option"/> containing the extracted information.</param>
    private void SetEnumValues(MemberInfo member, Type type, Option obj) {
      var attributes = member.GetCustomAttributes(typeof (EnumValueAttribute), false);
      if (attributes.Length > 0) {
        var enumValues = new List<EnumValue>();
        foreach (var attribute in attributes) {
          var enumValue = ((EnumValueAttribute) attribute);
          if (enumValue.Value.GetType() != obj.ValueType) {
            throw new Exception("Type mismatch between optionValue(" + type + ") and enumValue(" + enumValue.Value.GetType() + " for Option " + obj.Name);
          }
          enumValues.Add(new EnumValue(enumValue.Label, enumValue.Value));
        }
        obj.EnumValues = enumValues;
      } else if (type.IsEnum) {
        var enumValues = new List<EnumValue>();
        var values = Enum.GetValues(type);
        foreach (var enumValue in values) {
          enumValues.Add(new EnumValue(Enum.GetName(type, enumValue), enumValue));
        }
        obj.EnumValues = enumValues;
      }
    }

    /// <summary>
    /// Set utility properties on <paramref name="obj"/>.
    /// </summary>
    /// <param name="member">The member to process.</param>
    /// <param name="obj">The <see cref="Option"/> containing the extracted information.</param>
    private void SetUtilityProperties(MemberInfo member, Option obj) {
      var name = member.Name.ToLower();
      if (isDisabledMapping.ContainsKey(name)) {
        obj.CheckDisabled = isDisabledMapping[name];
      }
      if (isHiddenMapping.ContainsKey(name)) {
        obj.CheckHidden = isHiddenMapping[name];
      }
    }

    private static bool ShouldIgnoreMember(MemberInfo member) {
      return StartsWith(member.Name, "ShouldDisable");
    }

    private static bool IsOptionGroup(MemberInfo groupMember) {
      var componentTypeAttributes = groupMember.GetCustomAttributes(typeof (ComponentTypeAttribute), false);
      if (componentTypeAttributes.Length > 0) {
        return ((ComponentTypeAttribute) componentTypeAttributes[0]).Value == ComponentTypes.OptionGroup;
      }
      return false;
    }

    private static string GetGroup(MemberInfo member) {
      var attributes = member.GetCustomAttributes(typeof (OptionGroupAttribute), false);
      if (attributes.Length > 0) {
        return ((OptionGroupAttribute) attributes[0]).Name.ToLower();
      }
      return null;
    }

    private static ComponentTypes GetDefaultComponent(Type type) {
      if (type.IsEnum) {
        return ComponentTypes.Combobox;
      } else if (type == typeof (bool)) {
        return ComponentTypes.Checkbox;
      } else if (type == typeof (int)) {
        return ComponentTypes.Spinner;
      }
      return ComponentTypes.Text;
    }

    /// <summary>
    /// <see cref="IComparer{T}"/> that compares members position information contained in <see cref="OptionGroupAttribute"/>.
    /// </summary>
    public class MemberComparer : IComparer<MemberInfo>
    {
      public int Compare(MemberInfo x, MemberInfo y) {
        int posX = 0, posY = 0;
        var attributesX = x.GetCustomAttributes(typeof (OptionGroupAttribute), false);
        if (attributesX.Length > 0) {
          posX = ((OptionGroupAttribute) attributesX[0]).Position;
        }
        var attributesY = y.GetCustomAttributes(typeof (OptionGroupAttribute), false);
        if (attributesY.Length > 0) {
          posY = ((OptionGroupAttribute) attributesY[0]).Position;
        }
        if (posX > posY) {
          return 1;
        } else if (posX < posY) {
          return -1;
        }
        return 0;
      }
    }
  }
}
