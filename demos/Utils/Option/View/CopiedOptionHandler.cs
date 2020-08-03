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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Option.Handler;
using yWorks.Graph;

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
    private IOptionGroup group;
    private readonly object initialValue;
    private readonly bool initialEnabled;

    private bool autoAdopt;
    private bool autoCommit;
    private WeakListener listener;
    private DelegatingMapper delegatingMapper;
    private static readonly PropertyChangedEventArgs valuePCEA = new PropertyChangedEventArgs("Value");
    private static readonly PropertyChangedEventArgs enabledPCEA = new PropertyChangedEventArgs("Enabled");

    internal bool AutoAdopt {
      get { return autoAdopt; }
      set { autoAdopt = value; }
    }

    internal bool AutoCommit {
      get { return autoCommit; }
      set { autoCommit = value; }
    }

    public CopiedOptionItem(IOptionItem delegateItem, IOptionGroup group) {
      this.delegateItem = delegateItem;
      dictionaryOverride = new Dictionary<string, object>();
      lookup = delegateItem;
      this.enabled = this.initialEnabled = delegateItem.Enabled;
      this.value = this.initialValue = delegateItem.Value;
      this.group = group;
      listener = new WeakListener(this, delegateItem);
    }

    internal class WeakListener 
    {
      public WeakListener(CopiedOptionItem item, IOptionItem optionItem) {
        this.Target = item;
        optionItem.PropertyChanged += new PropertyChangedEventHandler(delegateItem_PropertyChanged);
      }

      private WeakReference target;
      protected object Target {
        get { return target.Target; }
        set { target = new WeakReference(value); }
      }

      private void delegateItem_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        CopiedOptionItem item = GetItem(sender);
        if (item != null) {
          item.delegateItem_PropertyChanged(sender, e);
        }
      }

      private CopiedOptionItem GetItem(object source) {
        CopiedOptionItem item = Target as CopiedOptionItem;
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
        ((IOptionItem)source).PropertyChanged -= delegateItem_PropertyChanged;
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
      CopiedOptionGroup group = (CopiedOptionGroup) (this as CopiedOptionGroup ?? Owner);
      while (group != null && !(group is CopiedOptionHandler)) {
        group = group.Owner as CopiedOptionGroup;
      }
      return (CopiedOptionHandler)group;
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
        group = null;
      }
    }

    public bool Enabled {
      get { return enabled && (group == null || group.Enabled); }
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

    protected virtual void OnDisabled() {
      
    }

    protected virtual void OnEnabled() {
      
    }

    protected internal virtual void OnParentDisabled() {
      if (this.enabled) {
        if (PropertyChanged != null) {
          OnPropertyChanged(enabledPCEA);
        }
        OnDisabled();
      }
    }

    protected internal virtual void OnParentEnabled() {
      if (this.enabled) {
        if (PropertyChanged != null) {
          OnPropertyChanged(enabledPCEA);
        }
        OnEnabled();
      }
    }

    public string Name {
      get { return delegateItem != null ? delegateItem.Name : string.Empty; }
    }

    public Type Type {
      get { return delegateItem != null ? delegateItem.Type : typeof (object); }
    }

    public object Value {
      get { return value; }
      set {
        if (this.value != value) {
          if (PropertyChanged != null) {
            this.value = value;
            OnPropertyChanged(valuePCEA);
            if (autoCommit && delegateItem != null) {
              delegateItem.Value = this.value;
            }
          } else {
            this.value = value;
            if (autoCommit && delegateItem != null) {
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

    public IOptionGroup Owner {
      get { return group; }
      set { group = value; }
    }

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
          if (copiedOptionItem.dictionaryOverride != null && copiedOptionItem.dictionaryOverride.TryGetValue(key, out value)) {
            return value;
          }
          return copiedOptionItem.delegateItem != null ? copiedOptionItem.delegateItem.Attributes[key] : null;
        }
        set {

          if (copiedOptionItem.dictionaryOverride != null) {
            copiedOptionItem.dictionaryOverride[key] = value;
          } else {
            if (copiedOptionItem.delegateItem != null) copiedOptionItem.delegateItem.Attributes[key] = value;
          }
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual object Lookup(Type type) {
      if (type == typeof (IOptionItem)) {
        return this;
      } else if (type == typeof (IOptionGroup)) {
        return Owner;
      } else {
        return lookup.Lookup(type);
      }
    }

    public void AdoptValue() {
      IOptionItem item = delegateItem;
      if (item != null) {
        this.Value = delegateItem.Value;
        this.Enabled = delegateItem.Enabled;
      }
    }

    public void CommitValue() {
      IOptionItem item = delegateItem;
      if (item != null) {
        item.Value = value;
        item.Enabled = enabled;
      }
    }

    public void Reset() {
      this.Value = initialValue;
      this.Enabled = initialEnabled;
    }
  }

  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  internal class CopiedOptionGroup : CopiedOptionItem, IOptionGroup {
    internal ObservableCollection<IOptionItem> items = new ObservableCollection<IOptionItem>();
    private IOptionGroup groupDelegate;
    private WeakGroupListener listener;

    public CopiedOptionGroup(OptionHandler handler) : base(handler, null) { }

    public CopiedOptionGroup(IOptionGroup delegateItem, IOptionGroup group) : base(delegateItem, group) {
      Init(delegateItem);
    }

    protected void Init(IOptionGroup delegateItem) {
      this.groupDelegate = delegateItem;
      listener = new WeakGroupListener(this, groupDelegate);
      foreach (var item in delegateItem.Items) {
        this.items.Add(CreateCopy(item));
      }
    }

    internal class WeakGroupListener {
      private readonly WeakReference reference;

      public WeakGroupListener(CopiedOptionGroup group, IOptionGroup optionGroup) {
        this.reference = new WeakReference(group);
        optionGroup.Items.CollectionChanged += new NotifyCollectionChangedEventHandler(Items_CollectionChanged);
      }

      protected object Target { get { return reference.Target; } }

      void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        CopiedOptionGroup group = Target as CopiedOptionGroup;
        if (group == null || group.GetOptionHandler() == null) {
          ((INotifyCollectionChanged)sender).CollectionChanged -= Items_CollectionChanged;
        }

        if (group != null) {
          group.Items_CollectionChanged(sender, e);
        }
      }

      public void Disconnect(IOptionGroup item) {
        item.Items.CollectionChanged -= Items_CollectionChanged;
      }
    }


    public ObservableCollection<IOptionItem> Items {
      get { return items; }
    }


    protected override void OnDisabled() {
      base.OnDisabled();
      foreach (IOptionItem item in items) {
        if (item is CopiedOptionItem) {
          ((CopiedOptionItem)item).OnParentDisabled();
        }
      }
    }

    protected override void OnEnabled() {
      base.OnEnabled();
      foreach (IOptionItem item in items) {
        if (item is CopiedOptionItem) {
          ((CopiedOptionItem)item).OnParentEnabled();
        }
      }
    }

    internal void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
          int index = e.NewStartingIndex;
          foreach (var newItem in e.NewItems) {
            items.Insert(index++, CreateCopy((IOptionItem) newItem));
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          for (int i = 0; i < e.OldItems.Count; i++) {
            var optionItem = items[e.OldStartingIndex];
            items.RemoveAt(e.OldStartingIndex);
            ((CopiedOptionItem) optionItem).Dispose();
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          for (int i = 0; i < e.OldItems.Count; i++) {
            var optionItem = items[e.OldStartingIndex + 1];
            ((CopiedOptionItem)optionItem).Dispose();
            items[e.OldStartingIndex + 1] = CreateCopy((IOptionItem)e.NewItems[i]);
          }
          break;
        case NotifyCollectionChangedAction.Reset:
          foreach (var o in items) {
            ((CopiedOptionItem)o).Dispose();
          }
          items.Clear();
          foreach (var i in ((IEnumerable<IOptionItem>)sender)) {
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

  internal class CopiedOptionHandler : CopiedOptionGroup, IModelView, IDisposable {
    private bool isAutoAdopt;
    private bool isAutoCommit;
    private OptionHandler handler;
    private readonly Dictionary<IOptionItem, CopiedOptionItem> mapping;

    public CopiedOptionHandler(OptionHandler handler) : base(handler){
      this.handler = handler;
      mapping = new Dictionary<IOptionItem, CopiedOptionItem>();
      base.Init(handler);
      mapping[handler] = this;
      handler.AddView(this);
    }

    public override void Dispose() {
      if (handler != null) {
        handler.RemoveView(this);
        mapping.Clear();
        handler = null;
        base.Dispose();
      }
    }

    internal CopiedOptionItem CreateCopy(CopiedOptionGroup newParent, IOptionItem itemToCopy) {
      CopiedOptionItem newItem;
      if (itemToCopy is IOptionGroup) {
        CopiedOptionGroup copiedGroup = new CopiedOptionGroup(itemToCopy as IOptionGroup, newParent);
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
      List<CopiedOptionItem> items = GetItems();
      foreach (CopiedOptionItem item in items) {
        item.AdoptValue();
      }
    }

    private List<CopiedOptionItem> GetItems() {
      List<CopiedOptionItem> items = new List<CopiedOptionItem>();
      List<CopiedOptionItem> queue = new List<CopiedOptionItem>();
      queue.Add(mapping[handler]);
      
      while (queue.Count > 0) {
        CopiedOptionItem item = queue[0];
        queue.RemoveAt(0);
        if (item != null) {
          items.Add(item);
          if (item is IOptionGroup) {
            foreach (IOptionItem optionItem in ((IOptionGroup)item).Items) {
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
      foreach (CopiedOptionItem item in GetItems()) {
        item.CommitValue();
      }
    }

    public bool IsAutoAdopt {
      get { return isAutoAdopt; }
      set {
        if(isAutoAdopt != value) {
          isAutoAdopt = value;
          foreach (CopiedOptionItem item in GetItems()) {
            item.AutoAdopt = value;
          }
        }
      }
    }

    public bool IsAutoCommit {
      get { return isAutoCommit; }
      set {
        if (isAutoCommit != value) {
          isAutoCommit = value;
          foreach (CopiedOptionItem item in GetItems()) {
            item.AutoCommit = value;
          }
        }
      }
    }

    public OptionHandler Handler {
      get { return handler; }
    }

    public IOptionItem GetViewItem(IOptionItem item) {
      CopiedOptionItem result;
      mapping.TryGetValue(item, out result);
      return result;
    }

    public void ResetValues() {
      foreach (CopiedOptionItem item in GetItems()) {
        item.Reset();
      }
    }

    public void ResetValue(IOptionItem item) {
      if (GetItems().Contains(item as CopiedOptionItem)) {
        ((CopiedOptionItem)item).Reset();
      }
    }

    public override object Lookup(Type type) {
      if (type == typeof(OptionHandler)) {
        return handler;
      }
      if (handler != null) {
        return handler.Lookup(type);
      }
      return null;
    }

    internal void Dispose(IOptionItem optionItem, CopiedOptionItem copiedOptionItem) {
      mapping.Remove(optionItem);
    }
  }
}
