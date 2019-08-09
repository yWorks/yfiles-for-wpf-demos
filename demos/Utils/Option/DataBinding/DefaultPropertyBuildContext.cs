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
using yWorks.Graph;

namespace Demo.yFiles.Option.DataBinding
{
  internal class DefaultPropertyBuildContext<TSubject> : IPropertyBuildContext<TSubject> where TSubject : class
  {
    private readonly ILookup innerLookup;
    private readonly IPropertyMap map;
    private readonly string prefix = string.Empty;
    private readonly GetInstanceDelegate<TSubject> getInstance;
    private readonly SetInstanceDelegate<TSubject> setInstance;
    private readonly AssignmentPolicy policy = AssignmentPolicy.Default;
    private readonly IContextLookup contextLookup;

    internal DefaultPropertyBuildContext(ILookup innerLookup, IContextLookup contextLookup, IPropertyMap map, TSubject subject) {
      this.innerLookup = innerLookup;
      this.contextLookup = contextLookup;
      this.map = map;
      this.getInstance = delegate() { return subject; };
      this.setInstance = delegate { };
    }

    protected DefaultPropertyBuildContext(ILookup innerLookup, IContextLookup contextLookup, string prefix, IPropertyMap map, GetInstanceDelegate<TSubject> getInstance, SetInstanceDelegate<TSubject> setInstance, AssignmentPolicy policy) {
      this.innerLookup = innerLookup;
      this.map = map;
      this.prefix = prefix;
      this.getInstance = getInstance;
      this.setInstance = setInstance;
      this.policy = policy;
      this.contextLookup = contextLookup;
    }

    #region IPropertyBuildContext Members

    public AssignmentPolicy Policy {
      get { return policy; }
    }

    #endregion

    #region IPropertyBuildContext Members

    public virtual IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter,
                                          IEqualityComparer comparer) {
      return map.AddEntry(DefaultOptionBuilderContext<object>.CreatePrefix(this.prefix, virtualPropertyName), getter, setter, comparer);
    }


    public IPropertyItem AddEntry<TValue>(string virtualPropertyName, ValueGetterDelegate<TValue> getter,
                                          ValueSetterDelegate<TValue> setter, IEqualityComparer comparer) {
      return AddEntry(virtualPropertyName, new DelegateGetter<TValue>(getter), new DelegateSetter<TValue>(setter), comparer);
    }

    public IPropertyItem AddEntry<TValue>(string virtualPropertyName, ValueGetterDelegate<TValue> getter,
                                          ValueSetterDelegate<TValue> setter) {
      return AddEntry(virtualPropertyName, new DelegateGetter<TValue>(getter), new DelegateSetter<TValue>(setter), null);
    }

    public TSubject CurrentInstance {
      get { return getInstance(); }
    }

    public virtual void SetNewInstance(TSubject newInstance) {
      setInstance(newInstance);
    }

    public IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter) {
      return AddEntry(virtualPropertyName, getter, setter, null);
    }

    #endregion

    public IPropertyMapBuilder GetPropertyMapBuilder(object subject) {
      IPropertyMapBuilder builder = contextLookup.Lookup(subject, typeof (IPropertyMapBuilder)) as IPropertyMapBuilder;
      if (builder != null) {
        return builder;
      }
      if (subject != null) {
        builder = GetBuilder(subject.GetType());
        if (builder != null) {
          return builder;
        }
      }
      return subject != null ? ReflectionHelper.GetPropertyBuilderFromAttribute(subject.GetType()) : null;
    }

    public IPropertyMapBuilder GetPropertyMapBuilder(Type type, object subject) {
      if (subject == null) {
        IPropertyMapBuilder builder = contextLookup.Lookup(type, typeof(IPropertyMapBuilder)) as IPropertyMapBuilder;
        if (builder != null) {
          return builder;
        }
        return GetBuilder(type);
      } else {
        return GetPropertyMapBuilder(subject);
      }
    }

    private IPropertyMapBuilder GetBuilder(Type type) {
//      if (typeof(Brush).IsAssignableFrom(type)) {
//        return new DefaultBrushPropertyMapBuilder();
//      }
//      if (type == typeof(Pen)) {
//        return new DefaultPenPropertyMapBuilder();
//      }
//      if(type == typeof(StringFormat)) {
//        return new StringFormatPropertyMapBuilder();
//      }
      return ReflectionHelper.GetPropertyBuilderFromAttribute(type);
    }

    public IPropertyBuildContext<TChild> CreateChildContext<TChild>(string name, GetInstanceDelegate<TChild> getHandler,
                                                                            SetInstanceDelegate<TChild> setHandler,
                                                                            AssignmentPolicy policy) where TChild:class{
      return new DecoratedBuildContext<TChild>(innerLookup, contextLookup, DefaultOptionBuilderContext<object>.CreatePrefix(this.prefix, name), map, getHandler, setHandler, policy);
    }

    public object Lookup(Type type) {
      return innerLookup.Lookup(type);
    }

    private class DecoratedBuildContext<TInnerSubject> : DefaultPropertyBuildContext<TInnerSubject> where TInnerSubject :class
    {

      internal DecoratedBuildContext(ILookup lookup, IContextLookup innerLookup, string prefix, IPropertyMap map, GetInstanceDelegate<TInnerSubject> getHandler, SetInstanceDelegate<TInnerSubject> setHandler, AssignmentPolicy policy)
        : base(lookup, innerLookup, prefix, map, getHandler, setHandler, policy) {
      }

      public override IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter,
                                             IEqualityComparer comparer) {
        setter = new DecoratedSetter(setter, getInstance, setInstance, null);
        return base.AddEntry(virtualPropertyName, getter, setter, comparer);
      }

      internal class DecoratedSetter : IValueSetter 
      {
        private readonly IValueSetter setter;
        private readonly GetInstanceDelegate<TInnerSubject> getTargetHandler;
        private readonly SetInstanceDelegate<TInnerSubject> newInstanceSetter;
        private readonly Func<TInnerSubject, TInnerSubject> cloner;

        public DecoratedSetter(IValueSetter setter, GetInstanceDelegate<TInnerSubject> getTargetHandler, SetInstanceDelegate<TInnerSubject> newInstanceSetter, Func<TInnerSubject, TInnerSubject> cloner) {
          this.setter = setter;
          this.cloner = cloner;
          this.getTargetHandler = getTargetHandler;
          this.newInstanceSetter = newInstanceSetter;
        }

        public void SetValue(object value) {
          if (cloner != null && newInstanceSetter != null) {
            TInnerSubject currentValue;
            currentValue = getTargetHandler();
            TInnerSubject newSubject = cloner(currentValue);
            if (!ReferenceEquals(newSubject, currentValue)) {
              newInstanceSetter(newSubject);
            }
          }
          {
            setter.SetValue(value);
          }
        }

        public bool CanSet() {
          return setter != null && setter.CanSet();
        }
      }
    }

  }
}