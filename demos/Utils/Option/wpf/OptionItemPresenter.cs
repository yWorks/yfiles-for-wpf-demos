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
using System.Collections.Specialized;
using System.ComponentModel;
#if UseDataAnnotations
using System.ComponentModel.DataAnnotations;
#endif
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Utils;

namespace Demo.yFiles.Option.View
{

#if VSM
  [TemplateVisualState(Name = "ValidDefault", GroupName = "ValueStates")]
  [TemplateVisualState(Name = "ValidCustom", GroupName = "ValueStates")]
  [TemplateVisualState(Name = "Invalid", GroupName = "ValueStates")]
  [TemplateVisualState(Name = "Undefined", GroupName = "ValueStates")]
  [TemplateVisualState(Name = "Enabled", GroupName = "EnabledStates")]
  [TemplateVisualState(Name = "Disabled", GroupName = "EnabledStates")]
#endif
  public class OptionItemPresenter : Control
  {
    public static readonly RoutedCommand ResetValueCommand = new RoutedUICommand("Reset Value", "ResetValue", typeof(OptionItemPresenter));

    static OptionItemPresenter(){
      DefaultStyleKeyProperty.OverrideMetadata(typeof (OptionItemPresenter), new FrameworkPropertyMetadata(typeof(OptionItemPresenter)));
      KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof (OptionItemPresenter), new FrameworkPropertyMetadata(false));
    }

    public OptionItemPresenter() {
      weakEventListener = new MyListener(this);
      ChildItems = new IOptionItem[0];
      this.Loaded += OnLoaded;
      this.LocalizingConverter = new XamlLocalizingConverter();
      CommandManager.RegisterClassCommandBinding(typeof(OptionItemPresenter), new CommandBinding(ResetValueCommand, OnResetValueExecuted, OnCanExecuteResetValue));
    }

    private readonly List<object> validationErrors = new List<object>();
    private readonly Dictionary<object, object> currentViolations = new Dictionary<object, object>();

    private void OnBindingValidationError(object sender, ValidationErrorEventArgs validationErrorEventArgs) {
      if (!validationErrorEventArgs.Handled) {
        switch (validationErrorEventArgs.Action) {
          case ValidationErrorEventAction.Added:
            validationErrors.Add(validationErrorEventArgs.Error.ErrorContent);
            break;
          case ValidationErrorEventAction.Removed:
            validationErrors.Remove(validationErrorEventArgs.Error.ErrorContent);
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
        validationErrorEventArgs.Handled = true;
        UpdateErrorContent();
        if (validationErrors.Count <= 0) {
          // no validationErrors anymore - we can write back the value...
          WriteBackValue(Value);
        }
        UpdateVisualState(true);
      }
    }

    private void WriteBackValue(object value) {
      if (Item != null 
        ) {
        object oldValue = Item.Value;
        try {
          var back = Converter.ConvertBack(value, Item.Type, ConverterParameter, CultureInfo.CurrentUICulture);
          if (back != DependencyProperty.UnsetValue) {
            Item.Value = back;
          } 
        } catch (Exception e) {
          try {
            Item.Value = oldValue;
          } catch (Exception) {} // ignore
          currentViolations[this] = e.Message;
          validationErrors.Add(e.Message);
        }
      }
    }

    private void UpdateErrorContent() {
      if (validationErrors.Count > 0) {
        ErrorContent = validationErrors[validationErrors.Count - 1];
      } else {
        ErrorContent = null;
      }
    }

    public static readonly DependencyProperty ErrorContentProperty = DependencyProperty.Register("ErrorContent",
                                                                                                 typeof (object),
                                                                                                 typeof (
                                                                                                   OptionItemPresenter),
                                                                                                 new PropertyMetadata(
                                                                                                   null));

    public object ErrorContent {
      get { return GetValue(ErrorContentProperty); }
      protected set { SetValue(ErrorContentProperty, value); }
    }

    private void OnCanExecuteResetValue(object sender, CanExecuteRoutedEventArgs canExecuteRoutedEventArgs) {
      canExecuteRoutedEventArgs.Handled = true;
      if (Item != null) {
        object value = Item.Attributes[OptionItem.DefaultValueAttribute];
        if (ItemType.IsInstanceOfType(value) || ItemType.IsClass && value == null) {
          canExecuteRoutedEventArgs.CanExecute = IsUndefined || !Equals(Value, value);
        }
      }
    }

    private void OnResetValueExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs) {
      if (Item != null) {
        executedRoutedEventArgs.Handled = true;
        object value = Item.Attributes[OptionItem.DefaultValueAttribute];
        if (ItemType.IsInstanceOfType(value) || ItemType.IsClass && value == null) {
          Value = value;
        }
      }
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
      this.Loaded -= OnLoaded;
      UpdateStyle(ItemType);
      UpdateVisualState(true);
    }

    public static readonly DependencyProperty ConverterProperty = DependencyProperty.Register("Converter",
                                                                                              typeof (IValueConverter),
                                                                                              typeof (
                                                                                                OptionItemPresenter),
                                                                                              new PropertyMetadata(new VoidConverter(), OnConverterChanged));

    private static void OnConverterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      ((OptionItemPresenter)dependencyObject).OnConverterChanged((IValueConverter)dependencyPropertyChangedEventArgs.OldValue, (IValueConverter)dependencyPropertyChangedEventArgs.NewValue); ;
    }

    protected virtual void OnConverterChanged(IValueConverter oldValue, IValueConverter newValue) {
      AdoptValue();
    }

    public IValueConverter Converter {
      get { return (IValueConverter) GetValue(ConverterProperty); }
      set { SetValue(ConverterProperty, value); }
    }

    public static readonly DependencyProperty LocalizingConverterProperty = DependencyProperty.Register("LocalizingConverter",
                                                                                              typeof (IValueConverter),
                                                                                              typeof (
                                                                                                OptionItemPresenter),
                                                                                              new PropertyMetadata(null));

    public IValueConverter LocalizingConverter {
      get { return (IValueConverter) GetValue(LocalizingConverterProperty); }
      protected set { SetValue(LocalizingConverterProperty, value); }
    }

    public static readonly DependencyProperty ConverterTargetProperty = DependencyProperty.Register("ConverterTarget",
                                                                                              typeof (Type),
                                                                                              typeof (
                                                                                                OptionItemPresenter),
                                                                                              new PropertyMetadata(typeof(object)));

    public Type ConverterTarget {
      get { return (Type) GetValue(ConverterTargetProperty); }
      set { SetValue(ConverterTargetProperty, value); }
    }

    public static readonly DependencyProperty ConverterParameterProperty =
      DependencyProperty.Register("ConverterParameter", typeof (object), typeof (OptionItemPresenter),
                                  new PropertyMetadata(null, OnConverterParameterChanged));

    private static void OnConverterParameterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      ((OptionItemPresenter) dependencyObject).OnConverterParameterChanged(dependencyPropertyChangedEventArgs.OldValue,
                                                                           dependencyPropertyChangedEventArgs.NewValue);
    }

    protected virtual void OnConverterParameterChanged(object oldValue, object newValue)
    {}

    public object ConverterParameter {
      get { return GetValue(ConverterParameterProperty); }
      set { SetValue(ConverterParameterProperty, value); }
    }

    private sealed class VoidConverter : IValueConverter
    {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value;
      }
    } 

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof (object),
                                                                                          typeof (OptionItemPresenter),
                                                                                          new PropertyMetadata(
                                                                                            (object) null,
                                                                                            OnValueChanged));

    private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      ((OptionItemPresenter) dependencyObject).OnValueChanged((object)dependencyPropertyChangedEventArgs.OldValue,
                                                              (object)dependencyPropertyChangedEventArgs.NewValue);
    }

    protected virtual void OnValueChanged(object oldValue, object newValue) {
      if (settingValue) {
        return;
      }
      if (!Equals(oldValue, newValue)) {
        if (Validate(newValue)) {
          if (Item != null) {
            WriteBackValue(newValue);
          }
        }
        UpdateVisualState(true);
      }
    }

    private bool Validate(object newValue) {

      bool continueValidation = true;
      if (!(Converter is VoidConverter)) {
        object outValue;
        if (currentViolations.TryGetValue(this, out outValue)) {
          validationErrors.Remove(outValue);
        }
        try {
          var back = Converter.ConvertBack(newValue, Item.Type, ConverterParameter, CultureInfo.CurrentUICulture);
          if (back != DependencyProperty.UnsetValue) {
            newValue = back;
            currentViolations.Remove(this);
          } else {
            var message = "Could not convert Value!";
            currentViolations[this] = message;
            validationErrors.Add(message);
            continueValidation = false;
          }
        } catch (Exception e) {
          var message = e.Message;
          currentViolations[this] =  message;
          validationErrors.Add(message);
          continueValidation = false;
        }
      }

#if UseDataAnnotations
      if (validationAttributes != null) {
        var validationContext = new ValidationContext(Item, null, null) {DisplayName = (string) new XamlLocalizingConverter().Convert(Item, typeof(string), null, CultureInfo.CurrentUICulture)};
        foreach (var validationAttribute in validationAttributes) {
          if (continueValidation) {
            var validationResult = validationAttribute.GetValidationResult(newValue, validationContext);
            object oldValue;
            if (currentViolations.TryGetValue(validationAttribute, out oldValue)) {
              validationErrors.Remove(oldValue);
            }
            if (validationResult != System.ComponentModel.DataAnnotations.ValidationResult.Success) {
              currentViolations[validationAttribute] = validationResult.ErrorMessage;
              validationErrors.Add(validationResult.ErrorMessage);
            } else {
              currentViolations.Remove(validationAttribute);
            }
          } else {
            object oldValue;
            if (currentViolations.TryGetValue(validationAttribute, out oldValue)) {
              validationErrors.Remove(oldValue);
            }
            currentViolations.Remove(validationAttribute);
          }
        }
      }
#else
      if (validationRules != null) {
        foreach (var r in validationRules) {
          if (continueValidation) {
            var validationResult = r.Validate(newValue, Thread.CurrentThread.CurrentUICulture);
            if (!validationResult.IsValid) {
              var errorContent = validationResult.ErrorContent;
              object value;
              if (currentViolations.TryGetValue(r, out value)) {
                validationErrors.Remove(value);
              } else {
                currentViolations[r] = errorContent;
              }
              validationErrors.Add(errorContent);
            }
          } else {
            object oldValue;
            if (currentViolations.TryGetValue(r, out oldValue)) {
              validationErrors.Remove(oldValue);
            }
            currentViolations.Remove(r);
          }
        }
      }
#endif
      UpdateErrorContent();
      return validationErrors.Count == 0;
    }

    protected virtual bool IsValid(object oldValue, object newValue) {
      return true;
    }

    public object Value {
      get { return (object) GetValue(ValueProperty);}
      set { SetValue(ValueProperty, value);}
    }

    public static readonly DependencyProperty UndefinedValueProperty = DependencyProperty.Register("UndefinedValue", typeof (object),
                                                                                          typeof (OptionItemPresenter),
                                                                                          new PropertyMetadata(
                                                                                            (object) null)
                                                                                            );

    public object UndefinedValue {
      get { return (object) GetValue(UndefinedValueProperty);}
      set { SetValue(UndefinedValueProperty, value); }
    }

    public static readonly DependencyProperty EditorTemplateProperty = DependencyProperty.Register("EditorTemplate", typeof(ControlTemplate),
                                                                                          typeof (OptionItemPresenter),
                                                                                          new PropertyMetadata(
                                                                                            (object) null)
                                                                                            );

    public ControlTemplate EditorTemplate {
      get { return (ControlTemplate) GetValue(EditorTemplateProperty);}
      set { SetValue(EditorTemplateProperty, value); }
    }

    public static readonly DependencyProperty ChildItemsProperty = DependencyProperty.Register("ChildItems", typeof (IEnumerable<IOptionItem>),
                                                                                          typeof (OptionItemPresenter),
                                                                                          new PropertyMetadata(
                                                                                            (object) null, PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      var eventListener = ((OptionItemPresenter)dependencyObject).weakEventListener;
      // TODO - remove memory leak
      if (dependencyPropertyChangedEventArgs.OldValue is INotifyCollectionChanged) {
        CollectionChangedEventManager.RemoveListener((INotifyCollectionChanged) dependencyPropertyChangedEventArgs.OldValue, eventListener);
      }      
      if (dependencyPropertyChangedEventArgs.NewValue is INotifyCollectionChanged) {
        CollectionChangedEventManager.AddListener((INotifyCollectionChanged) dependencyPropertyChangedEventArgs.NewValue, eventListener);
      }      
    }

    private class MyListener 
    : IWeakEventListener
    {
      private readonly OptionItemPresenter optionItemPresenter;

      public MyListener(OptionItemPresenter optionItemPresenter) {
        this.optionItemPresenter = optionItemPresenter;
      }

      public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e) {
        NotifyCollectionChangedEventArgs args = (NotifyCollectionChangedEventArgs) e;
        Event(sender, e);
        return true;
      }

      public void Event(object sender, EventArgs e) {
        optionItemPresenter.ChildItemCollectionChanged(sender, e);
      }
    }

    private void ChildItemCollectionChanged(object sender, EventArgs eventArgs) {
      UpdateStyle(ItemType);
    }

    public IEnumerable<IOptionItem> ChildItems {
      get { return (IEnumerable<IOptionItem>) GetValue(ChildItemsProperty);}
      internal set { SetValue(ChildItemsProperty, value);}
    }

    public static readonly DependencyProperty IsUndefinedProperty = DependencyProperty.Register("IsUndefined",
                                                                                                typeof (bool),
                                                                                                typeof (
                                                                                                  OptionItemPresenter),
                                                                                                new PropertyMetadata(
                                                                                                  (bool) true,
                                                                                                  OnIsUndefinedChanged));

    private static void OnIsUndefinedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      ((OptionItemPresenter) dependencyObject).OnIsUndefinedChanged((bool)dependencyPropertyChangedEventArgs.OldValue,
                                                              (bool)dependencyPropertyChangedEventArgs.NewValue);
    }

    protected virtual void OnIsUndefinedChanged(bool oldValue, bool newValue) {
      UpdateVisualState(true);
    }

    public bool IsUndefined {
      get { return (bool) GetValue(IsUndefinedProperty);}
      set { SetValue(IsUndefinedProperty, value);}
    }    
    
    public static readonly DependencyProperty ItemNameProperty = DependencyProperty.Register("ItemName",
                                                                                                typeof (string),
                                                                                                typeof (
                                                                                                  OptionItemPresenter),
                                                                                                new PropertyMetadata(
                                                                                                  string.Empty,
                                                                                                  OnItemNameChanged));

    private static void OnItemNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      ((OptionItemPresenter)dependencyObject).OnItemNameChanged((string)dependencyPropertyChangedEventArgs.OldValue,
                                                              (string)dependencyPropertyChangedEventArgs.NewValue);
    }

    protected virtual void OnItemNameChanged(string oldValue, string newValue) {

    }

    public string ItemName {
      get { return (string) GetValue(ItemNameProperty);}
      set { SetValue(ItemNameProperty, value);}
    }    

    public static readonly DependencyProperty ItemEnabledProperty = DependencyProperty.Register("ItemEnabled",
                                                                                                typeof (bool),
                                                                                                typeof (
                                                                                                  OptionItemPresenter),
                                                                                                new PropertyMetadata(
                                                                                                  false,
                                                                                                  OnItemEnabledChanged));

    private static void OnItemEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      ((OptionItemPresenter)dependencyObject).OnItemEnabledChanged((bool)dependencyPropertyChangedEventArgs.OldValue,
                                                              (bool)dependencyPropertyChangedEventArgs.NewValue);
    }

    protected virtual void OnItemEnabledChanged(bool oldValue, bool newValue) {
      UpdateVisualState(true);
    }

    public bool ItemEnabled {
      get { return (bool) GetValue(ItemEnabledProperty);}
      set { SetValue(ItemEnabledProperty, value);}
    }    
 
    private void UpdateVisualState(bool useTransitions) {
#if VSM
      if (Item != null && Item.Enabled) {
        VisualStateManager.GoToState(this, "Enabled", useTransitions);
      } else {
        VisualStateManager.GoToState(this, "Disabled", useTransitions);
      }

      if (IsUndefined) {
        VisualStateManager.GoToState(this, "Undefined", useTransitions);
      } else {
        if (validationErrors.Count > 0) {
          VisualStateManager.GoToState(this, "Invalid", useTransitions);
        } else {
          if (Item != null && Equals(Value, Item.Attributes[OptionItem.DefaultValueAttribute])) {
            VisualStateManager.GoToState(this, "ValidDefault", useTransitions);
          } else {
            VisualStateManager.GoToState(this, "ValidCustom", useTransitions);
          }
        }
      }
#else
      if (IsUndefined) {
        ValueState = ValueState.Undefined;
      } else {
        if (validationErrors.Count > 0) {
          ValueState = ValueState.Invalid;
        } else {
          if (Item != null && Equals(Value, Item.Attributes[OptionItem.DefaultValueAttribute])) {
            ValueState = ValueState.ValidDefault;
          } else {
            ValueState = ValueState.ValidCustom;
          }
        }
      }
#endif
    }

#if VSM
#else

    private static readonly DependencyPropertyKey ValueStatePropertyKey = DependencyProperty.RegisterReadOnly("ValueState",
                                                                                               typeof (ValueState),
                                                                                               typeof (
                                                                                                 OptionItemPresenter),
                                                                                               new FrameworkPropertyMetadata
                                                                                                 (ValueState.
                                                                                                    ValidDefault));

    public static readonly DependencyProperty ValueStateProperty = ValueStatePropertyKey.DependencyProperty;

    public ValueState ValueState {
      get { return (ValueState) GetValue(ValueStateProperty); }
      internal set { SetValue(ValueStatePropertyKey, value); }
    }

#endif

    public static readonly DependencyProperty ItemTypeProperty = DependencyProperty.Register("ItemType",
                                                                                                typeof (Type),
                                                                                                typeof (
                                                                                                  OptionItemPresenter),
                                                                                                new PropertyMetadata(
                                                                                                  typeof(object),
                                                                                                  OnItemTypeChanged));

    private static void OnItemTypeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      ((OptionItemPresenter)dependencyObject).OnItemTypeChanged((Type)dependencyPropertyChangedEventArgs.OldValue,
                                                              (Type)dependencyPropertyChangedEventArgs.NewValue);
    }

    protected virtual void OnItemTypeChanged(Type oldValue, Type newValue) {
      UpdateStyle(newValue ?? typeof(object));
    }

    private void UpdateStyle(Type newValue) {
      Style style = null;

      if (Item != null) {
        object o = Item.Attributes[OptionItem.CustomDialogitemEditor];
        if (o != null) {
          if (o is string || o is ResourceKey) {
            var controlTemplate = this.TryFindResource(o) as Style;
            if (controlTemplate != null) {
              style =  controlTemplate;
            }
          } else if (o is Style) {
            style = (Style) o;
          }
        }
      }

      if (style == null) {
        while (style == null && newValue != null) {
          string fullName = GetFullName(newValue);
          if (Item is IOptionGroup) {

            int level = 0;
            IOptionGroup group = (IOptionGroup) Item;
            // determine if group consists of groups, only

            bool justGroups = group.Items.All(optionItem => (optionItem is IOptionGroup));

            while (group != null) {
              group = group.Owner;
              level++;
            }
            if (!justGroups && level == 1) {
              level = 2;
            }
            if (((IOptionGroup) Item).Items.Count == 1) {
              //Discard invisible groups with only one child
              IOptionItem singleChild = ((IOptionGroup) Item).Items[0];
              var hintAttr = singleChild.Attributes[DefaultEditorFactory.RenderingHintsAttribute];
              if (hintAttr is DefaultEditorFactory.RenderingHints &&
                  (DefaultEditorFactory.RenderingHints) hintAttr == DefaultEditorFactory.RenderingHints.Invisible) {
                style =
                  this.TryFindResource("EmptyGroup.OptionItemPresenter") as Style;
              }
            }
            if (style == null) {
              style =
                this.TryFindResource(fullName + ".Level" + level + ".OptionGroup.OptionItemPresenter") as Style;
              if (style == null) {
                style = this.TryFindResource(fullName + ".OptionGroup.OptionItemPresenter") as Style;
              }
              if (style == null) {
                style =
                  this.TryFindResource("OptionGroup.Level" + level + ".OptionItemPresenter") as Style;
                if (style == null) {
                  style = this.TryFindResource("OptionGroup.OptionItemPresenter") as Style;
                }
              }
            }
          } else {
            style = this.TryFindResource(fullName + ".OptionItemPresenter") as Style;
          }
          newValue = newValue.BaseType;
        }
        if (style == null) {
          style = this.TryFindResource(ItemType.FullName + ".OptionItemPresenter") as Style;
          if (style == null) {
            style = this.TryFindResource("OptionItemPresenter") as Style;
          }
        }
      }
      if (style != null) {
        this.Style = style;
      } 
    }

    private string GetFullName(Type newValue) {
      var name = newValue.Name;
      string fullName = newValue.Namespace + "." + name;
      if (newValue.IsGenericType) {
        if (fullName.LastIndexOf('`') > 0) {
          fullName = fullName.Substring(0, fullName.LastIndexOf('`'));
        }
        fullName += "<";
        var arguments = newValue.GetGenericArguments();
        for (int index = 0; index < arguments.Length; index++) {
          var type = arguments[index];
          fullName += GetFullName(type);
          if (index < arguments.Length -1) {
            fullName += ",";
          }
        }
        fullName += ">";
      }
      return fullName;
    }

    public Type ItemType {
      get { return (Type) GetValue(ItemTypeProperty);}
      set { SetValue(ItemTypeProperty, value); }
    }

    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item",
                                                                                                typeof (IOptionItem),
                                                                                                typeof (
                                                                                                  OptionItemPresenter),
                                                                                                new PropertyMetadata(
                                                                                                  null,
                                                                                                  OnItemChanged));

    private static void OnItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      ((OptionItemPresenter)dependencyObject).OnItemChanged((IOptionItem)dependencyPropertyChangedEventArgs.OldValue,
                                                              (IOptionItem)dependencyPropertyChangedEventArgs.NewValue);
    }

    protected virtual void OnItemChanged(IOptionItem oldValue, IOptionItem newValue) {
      if (newValue != null && LocalizingConverter is XamlLocalizingConverter) {
        ((XamlLocalizingConverter)LocalizingConverter).OptionItem = Item;
      }
      if (oldValue != null) {
        oldValue.PropertyChanged -= OnItemPropertyChanged;
      }
      foreach (var violation in currentViolations) {
        validationErrors.Remove(violation.Value);
      }
      currentViolations.Clear();

      if (newValue != null) {

        var valueConverter = XamlOptionItemEditorSelector.GetAttribute<IValueConverter>(newValue, OptionItem.CustomValueConverterAttribute, null);
        if (valueConverter != null) {
          this.Converter = valueConverter;
        } else {
          var typeConverter = XamlOptionItemEditorSelector.GetAttribute<TypeConverter>(newValue, OptionItem.CustomTypeConverterAttribute,
                                                                                       null);
          if (typeConverter != null) {
            this.Converter = new TypeConverterConverter(typeConverter);
          } else {
            this.ClearValue(ConverterProperty);
          }
        }

        newValue.PropertyChanged += OnItemPropertyChanged;
#if UseDataAnnotations
        validationAttributes = Item.Attributes[OptionItem.ValidationAttributesAttribute] as IEnumerable<ValidationAttribute>;
#else
        validationRules = Item.Attributes[OptionItem.ValidationRulesAttribute] as IEnumerable<System.Windows.Controls.ValidationRule>;
#endif
        UpdateValue();
        ItemType = Item.Type;
        ItemName = Item.Name;// (string)new XamlLocalizingConverter() { OptionItem = newValue }.Convert(string.Empty, typeof(string), null, CultureInfo.CurrentUICulture);
        ItemEnabled = Item.Enabled;
        ChildItems = Item is IOptionGroup ? ((IOptionGroup) Item).Items : (IEnumerable<IOptionItem>)new IOptionItem[0];
        UpdateStyle(ItemType);
      } else {
#if UseDataAnnotations
        validationAttributes = EmptyEnumerable<ValidationAttribute>.Instance;
#else
        validationRules = Enumerable.Empty<ValidationRule>();
#endif
        ItemEnabled = false;
        IsUndefined = true;
        ItemType = typeof(object);
        Value = null;
        ItemName = string.Empty;
        UpdateStyle(ItemType);
      }
      UpdateVisualState(true);
    }

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
      if (propertyChangedEventArgs.PropertyName == "Value") {
        UpdateValue();
      } else if (propertyChangedEventArgs.PropertyName == "Enabled") {
        ItemEnabled = Item.Enabled;
      } else if (propertyChangedEventArgs.PropertyName == "Name") {
        ItemName = Item.Name;
      } else if (propertyChangedEventArgs.PropertyName == "Type") {
        ItemType = Item.Type;
      } 
    }

    private void UpdateValue() {
      IsUndefined = Item.Value == OptionItem.ValueUndefined;
      validationErrors.Clear();
      AdoptValue();
    }

    private void AdoptValue() {
      try {
        settingValue = true;
        if (IsUndefined) {
          Value = UndefinedValue;          
        } else {
          var convert = Converter.Convert(Item.Value, ConverterTarget, ConverterParameter, CultureInfo.CurrentUICulture);
          if (convert != DependencyProperty.UnsetValue) {
            Value = convert;
          }
        }
      } catch (Exception) {
        Value = UndefinedValue;
      } finally {
        settingValue = false;
      }
    }

    public IOptionItem Item {
      get { return (IOptionItem) GetValue(ItemProperty);}
      set { SetValue(ItemProperty, value);}
    }

#if UseDataAnnotations
    private IEnumerable<ValidationAttribute> validationAttributes;
#else
    private IEnumerable<System.Windows.Controls.ValidationRule> validationRules;
#endif
    private bool settingValue;
    private readonly MyListener weakEventListener;
  }

  public class TypeConverterConverter : IValueConverter
  {
    private readonly TypeConverter converter;

    public TypeConverterConverter(TypeConverter converter) {
      this.converter = converter;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if(converter.CanConvertTo(targetType)) {
        return converter.ConvertTo(value, targetType);  
      }
      else {
        return converter.ConvertTo(value, typeof(String));  
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return converter.ConvertFrom(value);
    }
  }

  public enum ValueState
  {
    ValidDefault,
    ValidCustom,
    Invalid,
    Undefined,
  }
}
