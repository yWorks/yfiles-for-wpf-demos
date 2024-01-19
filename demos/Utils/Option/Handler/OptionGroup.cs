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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;

namespace Demo.yFiles.Option.Handler
{
  
  /// <summary>
  /// Used to classify option items into groups.
  /// </summary>
  /// <remarks>OptionGroups can be nested.</remarks>
  public class OptionGroup : OptionItem, IOptionGroup, IEnumerable<IOptionItem>
  {
    private readonly ObservableCollection<IOptionItem> children;
    //used for direct access by item name
    private readonly IDictionary<string, IOptionItem> nameMap = new Dictionary<string, IOptionItem>();

    /// <summary>
    /// Get or set the value of this item.
    /// </summary>
    /// <remarks>Getting this property's value returns just a readonly list of all children, while
    /// setting the property's  value has no effect and is silently ignored.</remarks>
//    public override object Value {
//      get { return children; }
//      set {}
//    }



    IEnumerator<IOptionItem> IEnumerable<IOptionItem>.GetEnumerator() {
      return children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return ((IEnumerable<IOptionItem>)this).GetEnumerator();
    }


    /// <inheritdoc/>
    public override object Lookup(Type type) {
      if (type == typeof(IOptionGroup)) {
        return this;
      } else {
        return base.Lookup(type);
      }
    }

    /// <summary>
    /// Return a readonly list of all children
    /// </summary>
    public ObservableCollection<IOptionItem> Items {
      get { return children; }
    }

    /// <summary>
    /// Retrieve the child item with name <paramref name="index"/>
    /// </summary>
    /// <param name="index">The name of the item to find</param>
    /// <returns>the child item with name <paramref name="index"/>, or <see langword="null"/>
    /// if nno such child exists.</returns>
    public IOptionItem this[string index] {
      get {
        IOptionItem retval;
        nameMap.TryGetValue(index, out retval);
        return retval;
      }
    }

    /// <summary>
    /// Return the number of children.
    /// </summary>
    public int Count {
      get { return children.Count; }
    }

    public OptionGroup() : this(string.Empty){}

    /// <summary>
    /// Create new instance with the given <paramref name="name"/> and no children.
    /// </summary>
    /// <param name="name">The name of the group</param>
    public OptionGroup(string name)
      : base(name) {
      children = new ObservableCollection<IOptionItem>();
      children.CollectionChanged += ChildrenOnCollectionChanged;
      Type = typeof (object);
      //Option groups don't support both elements
      Attributes[OptionItem.SupportNullValueAttribute] = true;
      Attributes[OptionItem.SupportUndefinedValueAttribute] = true;
    }

    private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
      switch (notifyCollectionChangedEventArgs.Action) {
        case NotifyCollectionChangedAction.Add:
          foreach (var i in notifyCollectionChangedEventArgs.NewItems) {
            IOptionItem item = (IOptionItem) i;
            item.Owner = this;
            nameMap.Add(item.Name, item);
            //register for child notification events
            item.PropertyChanged += item_PropertyChanged;
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (var i in notifyCollectionChangedEventArgs.OldItems) {
            IOptionItem item = (IOptionItem)i;
            item.Owner = null;
            nameMap.Remove(item.Name);
            item.PropertyChanged -= item_PropertyChanged;
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          foreach (var i in notifyCollectionChangedEventArgs.NewItems) {
            IOptionItem item = (IOptionItem)i;
            item.Owner = this;
            nameMap.Add(item.Name, item);
            //register for child notification events
            item.PropertyChanged += item_PropertyChanged;
          }
          foreach (var i in notifyCollectionChangedEventArgs.OldItems) {
            IOptionItem item = (IOptionItem)i;
            item.Owner = null;
            nameMap.Remove(item.Name);
            item.PropertyChanged -= item_PropertyChanged;
          }
          break;
        case NotifyCollectionChangedAction.Reset:
          foreach (var o in children) {
            o.Owner = this;
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    /// <summary>
    /// Add a new <see cref="IOptionItem"/> to this group
    /// </summary>
    /// <param name="item"></param>
    public virtual IOptionItem AddOptionItem(IOptionItem item) {
      children.Add(item);
//      if (item is IOwnerSettable) {
//        ((IOwnerSettable)item).Owner = this;
//      }

      return item;
    }

    void item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
//      if (e is OptionValueChangedEventArgs) {
//        OnValueChanged((OptionItem) sender,
//                       ((OptionValueChangedEventArgs) e).OldValue, ((OptionValueChangedEventArgs) e).NewValue);
//      }
    }

    /// <summary>
    /// Remove the specified child.
    /// </summary>
    /// <param name="item">The child to remove</param>
    public virtual void RemoveOptionItem(IOptionItem item) {
      children.Remove(item);
      nameMap.Remove(item.Name); 
//      if (item is IOwnerSettable) {
//        ((IOwnerSettable)item).Owner = this;
//      }

    }

    /// <summary>
    /// Clear the list of all children
    /// </summary>
    public void Clear() {
      IList<IOptionItem> tmpList = new List<IOptionItem>(children);
      foreach (IOptionItem item in tmpList) {
        if (item is IOptionGroup) {
          ((IOptionGroup) item).Items.Clear();
        }
        RemoveOptionItem(item);
      }
    }

    /// <summary>
    /// Convenience method to add an <see cref="OptionItem"/> to this group
    /// </summary>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddInt(string itemName, int initialValue) {
      OptionItem newItem = new OptionItem(itemName) {Value = initialValue, Type = typeof(int)};
      AddOptionItem(newItem);
      return newItem;
    }

    /// <summary>
    /// Convenience method to add an <see cref="OptionItem"/> to this group
    /// </summary>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <param name="minValue">The minimum allowed value.</param>
    /// <param name="maxValue">The maximum allowed value.</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddInt(string itemName, int initialValue, int minValue, int maxValue) {
      OptionItem newItem = new OptionItem(itemName) {Value = initialValue, Type = typeof(int)};
#if UseDataAnnotations
      newItem.Attributes[OptionItem.ValidationAttributesAttribute] = new List<ValidationAttribute>()
                                                                  {new RangeAttribute(minValue, maxValue)};
#else
      newItem.Attributes[OptionItem.ValidationRulesAttribute] = new List<ValidationRule>()
                                                                  {new RangeValidationRule<int>(minValue, maxValue)};
#endif
      AddOptionItem(newItem);
      return newItem;
    }

    /// <summary>
    /// Convenience method to add an <see cref="OptionItem"/> to this group
    /// </summary>
    /// <remarks>The values of this item are constrained to be non negative, with no upper bound.</remarks>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddNonNegativeInt(string itemName, int initialValue) {
      return AddInt(itemName, initialValue, 0, int.MaxValue);
    }

    /// <summary>
    /// Convenience method to add a <see cref="OptionItem"/> to this group
    /// </summary>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddBool(string itemName, bool initialValue) {
      OptionItem newItem = new OptionItem(itemName) {Value = initialValue, Type = typeof (bool)};
      AddOptionItem(newItem);
      return newItem;
    }

    /// <summary>
    /// Convenience method to add a <see cref="OptionItem"/> to this group
    /// </summary>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddDouble(string itemName, double initialValue) {
      OptionItem newItem = new OptionItem(itemName) { Value = initialValue, Type = typeof(double) };
      AddOptionItem(newItem);
      return newItem;
    }
    
    /// <summary>
    /// Convenience method to add a <see cref="OptionItem"/> to this group
    /// </summary>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <param name="minValue">The minimum allowed value.</param>
    /// <param name="maxValue">The maximum allowed value.</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddDouble(string itemName, double initialValue, double minValue, double maxValue) {
      OptionItem newItem = new OptionItem(itemName) { Value = initialValue, Type = typeof(double) };
#if UseDataAnnotations
      newItem.Attributes[OptionItem.ValidationAttributesAttribute] = new List<ValidationAttribute>()
                                                                  {new RangeAttribute(minValue, maxValue)};
#else
      newItem.Attributes[OptionItem.ValidationRulesAttribute] = new List<ValidationRule>() { new RangeValidationRule<double>(minValue, maxValue) };
#endif
      AddOptionItem(newItem);
      return newItem;
    }

    /// <summary>
    /// Convenience method to add an <see cref="OptionItem"/> to this group
    /// </summary>
    /// <remarks>The values of this item are constrained to be non negative, with no upper bound.</remarks>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddNonNegativeDouble(string itemName, int initialValue) {
      return AddDouble(itemName, initialValue, 0, double.MaxValue);
    }

    /// <summary>
    /// Convenience method to add a <see cref="OptionItem"/> to this group
    /// </summary>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddString(string itemName, string initialValue) {
      OptionItem newItem = new OptionItem(itemName) { Value = initialValue, Type = typeof(string) };
      AddOptionItem(newItem);
      return newItem;
    }

    /// <summary>
    /// Convenience method to add a <see cref="CollectionOptionItem{T}"/> to this group
    /// </summary>
    /// <param name="itemName">The name of the item</param>
    /// <param name="domain">List of initial values for this item.</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <returns>A new instance of <see cref="CollectionOptionItem{T}"/></returns>
    public CollectionOptionItem<T> AddList<T>(string itemName, ICollection<T> domain, T initialValue) {
      CollectionOptionItem<T> newItem = new CollectionOptionItem<T>(itemName, domain, initialValue);
      AddOptionItem(newItem);
      return newItem;
    }

    /// <summary>
    /// Convenience method to add a OptionItem for <see cref="Color"/> values to this handler
    /// </summary>
    /// <param name="itemName">The name of the item</param>
    /// <param name="initialValue">The initial value of the item</param>
    /// <returns>A new instance of <see cref="OptionItem"/></returns>
    public IOptionItem AddColor(string itemName, Color initialValue) {
      OptionItem newItem = new OptionItem(itemName) { Value = initialValue, Type = typeof(Color) };
      AddOptionItem(newItem);
      return newItem;
    }

    /// <summary>
    /// Convenience method to add a <see cref="OptionGroup"/> to this handler
    /// </summary>
    /// <param name="name">The name of the group where this item belongs to. If 
    /// <see langword="null"/>, the item is added directly to the handler itself.</param>
    /// <returns>A new instance of <see cref="OptionGroup"/></returns>
    public OptionGroup AddGroup(string name) {
      OptionGroup retval = new OptionGroup(name);
      AddOptionItem(retval);
      return retval;
    }

    public IOptionItem Add<T>(string name, T value) {
      return Add(new OptionItem(name) {Value = value, Type = typeof (T)});
    }

    public IOptionItem Add(IOptionItem optionItem) {
      AddOptionItem(optionItem);
      return optionItem;
    }
  }
}
