/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Reflection;
using Demo.yFiles.Option.Handler;
using yWorks.Utils;

namespace Demo.yFiles.Option.View
{
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  internal class CopiedOptionItem : IOptionItem
  {
    private IOptionItem delegateItem;
    private Dictionary<string, object> dictionaryOverride;
    private ILookup lookup;
    private bool enabled;
    private object value;
    private readonly object initialValue;
    private readonly bool initialEnabled;

    private WeakListener listener;
    private DelegatingMapper delegatingMapper;
    private static readonly PropertyChangedEventArgs valuePCEA = new("Value");
    private static readonly PropertyChangedEventArgs enabledPCEA = new("Enabled");

    internal bool AutoAdopt { get; set; }

    internal bool AutoCommit { get; set; }

    public CopiedOptionItem(IOptionItem delegateItem, IOptionGroup group) {
      this.delegateItem = delegateItem;
      dictionaryOverride = new Dictionary<string, object>();
      lookup = delegateItem;
      enabled = initialEnabled = delegateItem.Enabled;
      value = initialValue = delegateItem.Value;
      Owner = group;
      listener = new WeakListener(this, delegateItem);
    }

    internal class WeakListener
    {
      public WeakListener(CopiedOptionItem item, IOptionItem optionItem) {
        Target = item;
        optionItem.PropertyChanged += delegateItem_PropertyChanged;
      }

      private WeakReference target;

      protected object Target {
        get => target.Target;
        set => target = new WeakReference(value);
      }

      private void delegateItem_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        var item = GetItem(sender);
        if (item != null) {
          item.delegateItem_PropertyChanged(sender, e);
        }
      }

      private CopiedOptionItem GetItem(object source) {
        var item = Target as CopiedOptionItem;
        if (item == null) {
          Disconnect(source);
        } else {
          if (item.delegateItem == null) {
            // can happen if Disconnect has been triggered in the same event dispatch
            Disconnect(source);
            return null;
          }
        }
        return item;
      }

      internal void Disconnect(object source) {
        ((IOptionItem) source).PropertyChanged -= delegateItem_PropertyChanged;
      }
    }

    private void delegateItem_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
      var optionItem = delegateItem;
      if (optionItem != null) {
        if (propertyChangedEventArgs.PropertyName == "Value") {
          if (AutoAdopt) {
            Value = optionItem.Value;
          }
        }
        if (propertyChangedEventArgs.PropertyName == "Enabled") {
          if (AutoAdopt) {
            Enabled = optionItem.Enabled;
          }
        }
      }
    }

    protected CopiedOptionHandler GetOptionHandler() {
      var group = (CopiedOptionGroup) (this as CopiedOptionGroup ?? Owner);
      while (group != null && !(group is CopiedOptionHandler)) {
        group = group.Owner as CopiedOptionGroup;
      }
      return (CopiedOptionHandler) group;
    }

    public virtual void Dispose() {
      GetOptionHandler().Dispose(delegateItem, this);
      if (delegateItem != null) {
        if (listener != null) {
          listener.Disconnect(delegateItem);
          listener = null;
        }
        delegateItem = null;
        lookup = null;
        dictionaryOverride = null;
        Owner = null;
      }
    }

    public bool Enabled {
      get => enabled && (Owner == null || Owner.Enabled);
      set {
        if (enabled != value) {
          enabled = value;
          if (PropertyChanged != null) {
            OnPropertyChanged(enabledPCEA);
          }
          if (!enabled) {
            OnDisabled();
          } else {
            OnEnabled();
          }
        }
      }
    }

    protected virtual void OnDisabled() { }

    protected virtual void OnEnabled() { }

    protected internal virtual void OnParentDisabled() {
      if (enabled) {
        if (PropertyChanged != null) {
          OnPropertyChanged(enabledPCEA);
        }
        OnDisabled();
      }
    }

    protected internal virtual void OnParentEnabled() {
      if (enabled) {
        if (PropertyChanged != null) {
          OnPropertyChanged(enabledPCEA);
        }
        OnEnabled();
      }
    }

    public string Name => delegateItem != null ? delegateItem.Name : string.Empty;

    public Type Type => delegateItem != null ? delegateItem.Type : typeof(object);

    public object Value {
      get => value;
      set {
        if (this.value != value) {
          if (PropertyChanged != null) {
            this.value = value;
            OnPropertyChanged(valuePCEA);
            if (AutoCommit && delegateItem != null) {
              delegateItem.Value = this.value;
            }
          } else {
            this.value = value;
            if (AutoCommit && delegateItem != null) {
              delegateItem.Value = this.value;
            }
          }
        }
      }
    }

    protected void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (PropertyChanged != null) {
        PropertyChanged(this, args);
      }
    }

    public IOptionGroup Owner { get; set; }

    public IMapper<string, object> Attributes {
      get {
        if (delegatingMapper == null) {
          delegatingMapper = new DelegatingMapper(this);
        }
        return delegatingMapper;
      }
    }

    internal sealed class DelegatingMapper : IMapper<string, object>
    {
      private readonly CopiedOptionItem copiedOptionItem;

      public DelegatingMapper(CopiedOptionItem copiedOptionItem) {
        this.copiedOptionItem = copiedOptionItem;
      }

      public object this[string key] {
        get {
          object value;
          if (copiedOptionItem.dictionaryOverride != null &&
              copiedOptionItem.dictionaryOverride.TryGetValue(key, out value)) {
            return value;
          }
          return copiedOptionItem.delegateItem != null ? copiedOptionItem.delegateItem.Attributes[key] : null;
        }
        set {
          if (copiedOptionItem.dictionaryOverride != null) {
            copiedOptionItem.dictionaryOverride[key] = value;
          } else {
            if (copiedOptionItem.delegateItem != null) {
              copiedOptionItem.delegateItem.Attributes[key] = value;
            }
          }
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual object Lookup(Type type) {
      if (type == typeof(IOptionItem)) {
        return this;
      }
      if (type == typeof(IOptionGroup)) {
        return Owner;
      }
      return lookup.Lookup(type);
    }

    public void AdoptValue() {
      var item = delegateItem;
      if (item != null) {
        Value = delegateItem.Value;
        Enabled = delegateItem.Enabled;
      }
    }

    public void CommitValue() {
      var item = delegateItem;
      if (item != null) {
        item.Value = value;
        item.Enabled = enabled;
      }
    }

    public void Reset() {
      Value = initialValue;
      Enabled = initialEnabled;
    }
  }

  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  internal class CopiedOptionGroup : CopiedOptionItem, IOptionGroup
  {
    internal System.Collections.ObjectModel.ObservableCollection<IOptionItem> items = new();
    private IOptionGroup groupDelegate;
    private WeakGroupListener listener;

    public CopiedOptionGroup(OptionHandler handler) : base(handler, null) { }

    public CopiedOptionGroup(IOptionGroup delegateItem, IOptionGroup group) : base(delegateItem, group) {
      Init(delegateItem);
    }

    protected void Init(IOptionGroup delegateItem) {
      groupDelegate = delegateItem;
      listener = new WeakGroupListener(this, groupDelegate);
      foreach (var item in delegateItem.Items) {
        items.Add(CreateCopy(item));
      }
    }

    internal class WeakGroupListener
    {
      private readonly WeakReference reference;

      public WeakGroupListener(CopiedOptionGroup group, IOptionGroup optionGroup) {
        reference = new WeakReference(group);
        optionGroup.Items.CollectionChanged += Items_CollectionChanged;
      }

      protected object Target => reference.Target;

      private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        var group = Target as CopiedOptionGroup;
        if (group == null || group.GetOptionHandler() == null) {
          ((INotifyCollectionChanged) sender).CollectionChanged -= Items_CollectionChanged;
        }

        if (group != null) {
          group.Items_CollectionChanged(sender, e);
        }
      }

      public void Disconnect(IOptionGroup item) {
        item.Items.CollectionChanged -= Items_CollectionChanged;
      }
    }


    public System.Collections.ObjectModel.ObservableCollection<IOptionItem> Items => items;


    protected override void OnDisabled() {
      base.OnDisabled();
      foreach (var item in items) {
        if (item is CopiedOptionItem) {
          ((CopiedOptionItem) item).OnParentDisabled();
        }
      }
    }

    protected override void OnEnabled() {
      base.OnEnabled();
      foreach (var item in items) {
        if (item is CopiedOptionItem) {
          ((CopiedOptionItem) item).OnParentEnabled();
        }
      }
    }

    internal void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
          var index = e.NewStartingIndex;
          foreach (var newItem in e.NewItems) {
            items.Insert(index++, CreateCopy((IOptionItem) newItem));
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          for (var i = 0; i < e.OldItems.Count; i++) {
            var optionItem = items[e.OldStartingIndex];
            items.RemoveAt(e.OldStartingIndex);
            ((CopiedOptionItem) optionItem).Dispose();
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          for (var i = 0; i < e.OldItems.Count; i++) {
            var optionItem = items[e.OldStartingIndex + 1];
            ((CopiedOptionItem) optionItem).Dispose();
            items[e.OldStartingIndex + 1] = CreateCopy((IOptionItem) e.NewItems[i]);
          }
          break;
        case NotifyCollectionChangedAction.Reset:
          foreach (var o in items) {
            ((CopiedOptionItem) o).Dispose();
          }
          items.Clear();
          foreach (var i in (IEnumerable<IOptionItem>) sender) {
            items.Add(CreateCopy(i));
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private CopiedOptionItem CreateCopy(IOptionItem item) {
      return GetOptionHandler().CreateCopy(this, item);
    }

    public override void Dispose() {
      base.Dispose();
      if (listener != null) {
        listener.Disconnect(groupDelegate);
        listener = null;
        groupDelegate = null;
      }
    }
  }

  internal class CopiedOptionHandler : CopiedOptionGroup, IModelView, IDisposable
  {
    private bool isAutoAdopt;
    private bool isAutoCommit;
    private readonly Dictionary<IOptionItem, CopiedOptionItem> mapping;

    public CopiedOptionHandler(OptionHandler handler) : base(handler) {
      Handler = handler;
      mapping = new Dictionary<IOptionItem, CopiedOptionItem>();
      Init(handler);
      mapping[handler] = this;
      handler.AddView(this);
    }

    public override void Dispose() {
      if (Handler != null) {
        Handler.RemoveView(this);
        mapping.Clear();
        Handler = null;
        base.Dispose();
      }
    }

    internal CopiedOptionItem CreateCopy(CopiedOptionGroup newParent, IOptionItem itemToCopy) {
      CopiedOptionItem newItem;
      if (itemToCopy is IOptionGroup) {
        var copiedGroup = new CopiedOptionGroup(itemToCopy as IOptionGroup, newParent);
        newItem = copiedGroup;
      } else {
        newItem = new CopiedOptionItem(itemToCopy, newParent);
      }
      newItem.AutoAdopt = IsAutoAdopt;
      newItem.AutoCommit = IsAutoCommit;
      mapping[itemToCopy] = newItem;
      return newItem;
    }

    public void AdoptValues() {
      var items = GetItems();
      foreach (var item in items) {
        item.AdoptValue();
      }
    }

    private List<CopiedOptionItem> GetItems() {
      var items = new List<CopiedOptionItem>();
      var queue = new List<CopiedOptionItem>();
      queue.Add(mapping[Handler]);

      while (queue.Count > 0) {
        var item = queue[0];
        queue.RemoveAt(0);
        if (item != null) {
          items.Add(item);
          if (item is IOptionGroup) {
            foreach (var optionItem in ((IOptionGroup) item).Items) {
              if (optionItem is CopiedOptionItem) {
                queue.Add((CopiedOptionItem) optionItem);
              }
            }
          }
        }
      }
      return items;
    }

    public void CommitValues() {
      foreach (var item in GetItems()) {
        item.CommitValue();
      }
    }

    public bool IsAutoAdopt {
      get => isAutoAdopt;
      set {
        if (isAutoAdopt != value) {
          isAutoAdopt = value;
          foreach (var item in GetItems()) {
            item.AutoAdopt = value;
          }
        }
      }
    }

    public bool IsAutoCommit {
      get => isAutoCommit;
      set {
        if (isAutoCommit != value) {
          isAutoCommit = value;
          foreach (var item in GetItems()) {
            item.AutoCommit = value;
          }
        }
      }
    }

    public OptionHandler Handler { get; private set; }

    public IOptionItem GetViewItem(IOptionItem item) {
      CopiedOptionItem result;
      mapping.TryGetValue(item, out result);
      return result;
    }

    public void ResetValues() {
      foreach (var item in GetItems()) {
        item.Reset();
      }
    }

    public void ResetValue(IOptionItem item) {
      if (GetItems().Contains(item as CopiedOptionItem)) {
        ((CopiedOptionItem) item).Reset();
      }
    }

    public override object Lookup(Type type) {
      if (type == typeof(OptionHandler)) {
        return Handler;
      }
      if (Handler != null) {
        return Handler.Lookup(type);
      }
      return null;
    }

    internal void Dispose(IOptionItem optionItem, CopiedOptionItem copiedOptionItem) {
      mapping.Remove(optionItem);
    }
  }
}
