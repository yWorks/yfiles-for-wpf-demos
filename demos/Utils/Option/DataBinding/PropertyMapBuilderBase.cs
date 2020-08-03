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
using System.Collections;

namespace Demo.yFiles.Option.DataBinding
{

  /// <summary>
  /// Abstract base class implementation of <see cref="IPropertyMapBuilder"/> with sophisticated
  /// default behavior.
  /// </summary>
  /// <remarks>
  /// Since the contract of <see cref="IPropertyMapBuilder"/> is hard to fulfill this class serves
  /// as a convenience base class implementation of custom builder implementations.
  /// Other than in the interface it is not necessary to implement the complicated
  /// <see cref="IPropertyMapBuilder.BuildPropertyMap{T}"/> method. This class will automatically
  /// intercept those classes where the type parameter <c>&lt;T</c> in that method call
  /// matches <typeparamref name="TSubject"/> or where <c>&lt;T</c> <see cref="Type.IsAssignableFrom">is assignable from</see>
  /// <typeparamref name="TSubject"/> and delegate to the <see cref="BuildPropertyMapImpl"/> method
  /// using a cleverly wrapped context.<br/>
  /// Additionally this abstract implementation provides an automatism for easily 
  /// implementing the <see cref="AssignmentPolicy.CreateNewInstance"/> policy:
  /// If the constructor is called with a boolean value of <see langword="true"/> everytime a property in this context
  /// or one of is child contexts is set, the
  /// <see cref="CreateCopy"/> method will be called to create a copy of the <see cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>.
  /// Then the original setter will be executed setting its value on the copy and finally the copy will be 
  /// propagated to the parent of the invoking context via <see cref="IPropertyBuildContext{TSubject}.SetNewInstance"/>.
  /// Implementations may choose to override the <see cref="CreateCopy"/> method or provide a <see cref="Cloner"/>
  /// factory method in for the creation of the clone.
  /// </remarks>
  /// <typeparam name="TSubject">The type of the subject this implementation deals with.</typeparam>
  public abstract class PropertyMapBuilderBase<TSubject> : IPropertyMapBuilder where TSubject : class
  {
    internal readonly bool useClone;

    private Func<TSubject, TSubject> cloner;

    /// <summary>
    /// Sole constructor of this class which has to be called by all subclasses.
    /// </summary>
    /// <param name="useCloneForNewInstance">This setting determines whether the <see cref="CreateCopy"/> mechanism
    /// should be used to fulfill the <see cref="AssignmentPolicy.CreateNewInstance"/> policy. If set to <see langword="false"/>
    /// the implementation needs to obey the <see cref="IPropertyBuildContext{TSubject}.Policy"/> setting and
    /// use the <see cref="IPropertyBuildContext{TSubject}.SetNewInstance"/> if appropriate.</param>
    /// <seealso cref="PropertyMapBuilderBase{TSubject}"/>
    /// <seealso cref="CreateCopy"/>
    /// <seealso cref="Cloner"/>
    protected PropertyMapBuilderBase(bool useCloneForNewInstance) {
      this.useClone = useCloneForNewInstance;
    }

    /// <summary>
    /// Gets or sets the delegate that will perform the cloning of the instance.
    /// </summary>
    /// <seealso cref="CreateCopy"/>
    public Func<TSubject, TSubject> Cloner {
      get { return cloner; }
      set { cloner = value; }
    }

    /// <summary>
    /// Default implementation that wraps the provided context as needed and
    /// delegates to <see cref="BuildPropertyMapImpl"/>, which needs to be implemented by subclasses.
    /// </summary>
    /// <remarks>
    /// See the description of <see cref="PropertyMapBuilderBase{TSubject}">this type</see>
    /// for what this implementation actually does. Normally subclasses need not care about this method but need
    /// to implement <see cref="BuildPropertyMapImpl"/>, only.
    /// </remarks>
    /// <typeparam name="T">The type of the context that is passed in.</typeparam>
    /// <param name="context">The context to use for the building of the property map.</param>
    /// <seealso cref="BuildPropertyMapImpl"/>
    public virtual void BuildPropertyMap<T>(IPropertyBuildContext<T> context) where T : class {
      if (!useClone && typeof(T) == typeof(TSubject)) {
        object ctx = context;
        BuildPropertyMapImpl((IPropertyBuildContext<TSubject>)ctx);
      } else {
        if (typeof(T).IsAssignableFrom(typeof(TSubject))) {
          object o = typeof(WrappingBuildContext<,>).MakeGenericType(typeof(TSubject), typeof(T)).GetConstructor(
            new Type[] { typeof(PropertyMapBuilderBase<TSubject>), typeof(IPropertyBuildContext<T>) }).Invoke(new object[] { this, context });
          BuildPropertyMapImpl((IPropertyBuildContext<TSubject>)o);
        }
      }
    }

    /// <summary>
    /// Creates a copy of the provided item to comply with the <see cref="AssignmentPolicy.CreateNewInstance"/> policy
    /// if that policy is set on the current context.
    /// </summary>
    /// <remarks>
    /// See the description of <see cref="PropertyMapBuilderBase{TSubject}">this type</see>.
    /// </remarks>
    /// <param name="context">The context that the subject needs to be cloned for. Note that this method should not
    /// have side effects on the context.</param>
    /// <param name="item">The item to create a (deep) copy of.</param>
    /// <returns>A clone of this item.</returns>
    protected internal virtual TSubject CreateCopy(IPropertyBuildContext<TSubject> context, TSubject item) {
      if (useClone) {
        if (cloner != null) {
          return cloner(item);
        } else if (item is ICloneable){
          return (TSubject)((ICloneable)item).Clone();
        }
      }
      return item;
    }

    /// <summary>
    /// The core method that needs to be implemented by subclasses.
    /// </summary>
    /// <remarks>
    /// This method actually builds the property map using the provided context.
    /// Note that if this instance has been created using the automatic cloning feature
    /// that can be turned on in the constructor, it is not necessary to take special
    /// action if the <see cref="IPropertyBuildContext{TSubject}.Policy"/> is set to
    /// <see cref="AssignmentPolicy.CreateNewInstance"/>. Instead the <see cref="CreateCopy"/>
    /// method will be used to seemlessly create a copy of the instance before the
    /// instance is modified by calls to the setter implementations that are
    /// registered through the context. 
    /// </remarks>
    /// <param name="context">The context to use.</param>
    protected abstract void BuildPropertyMapImpl(IPropertyBuildContext<TSubject> context);

  }

  /// <summary>
  /// The context that performs the downcasting that is needed by <see cref="PropertyMapBuilderBase{TSubject}"/>
  /// and automatically delegates to the <see cref="PropertyMapBuilderBase{TSubject}.CreateCopy"/>
  /// method if the corresponding policy is set.
  /// </summary>
  /// <typeparam name="T1">The type the context should use.</typeparam>
  /// <typeparam name="T2">The type the original context uses, which must be a super type of <typeparamref name="T1"/></typeparam>
  internal class WrappingBuildContext<T1, T2> : IPropertyBuildContext<T1>
    where T1 : class, T2
    where T2 : class
  {
    private readonly PropertyMapBuilderBase<T1> parent;
    private readonly IPropertyBuildContext<T2> context;
    private T1 tempInstance;
    private bool tempInstanceSet;

    public WrappingBuildContext(PropertyMapBuilderBase<T1> parent, IPropertyBuildContext<T2> context) {
      this.parent = parent;
      this.context = context;
    }

    public IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter, IEqualityComparer comparer) {
      return context.AddEntry(virtualPropertyName, getter, decorate(setter), comparer);
    }

    /// <summary>
    /// Decorates the setters to seemlessly perform the cloning on a set operation.
    /// </summary>
    internal IValueSetter decorate(IValueSetter setter) {
      if (parent.useClone && this.context.Policy == AssignmentPolicy.CreateNewInstance) {
        return new CloneValueSetter(this, setter);
      } else {
        return setter;
      }
    }

    private class CloneValueSetter : IValueSetter
    {
      private readonly WrappingBuildContext<T1, T2> parent;
      private readonly IValueSetter setter;

      public CloneValueSetter(WrappingBuildContext<T1, T2> parent, IValueSetter setter) {
        this.parent = parent;
        this.setter = setter;
      }

      public void SetValue(object value) {
        T1 newInstance = parent.parent.CreateCopy(parent, parent.CurrentInstance);
        parent.SetTempInstance(newInstance);
        try {
          if (setter.CanSet()) {
            setter.SetValue(value);
          } else {
            return; // don't commit it.
          }
        } finally {
          parent.ResetTempInstance();
        }
        parent.SetNewInstance(newInstance);
      }

      public bool CanSet() {
        return setter.CanSet();
      }
    }

    public IPropertyItem AddEntry<TValue>(string virtualPropertyName, ValueGetterDelegate<TValue> getter, ValueSetterDelegate<TValue> setter, IEqualityComparer comparer) {
      return context.AddEntry(virtualPropertyName, getter, decorate(setter) , comparer);
    }

    /// <summary>
    /// Decorates the setters to seemlessly perform the cloning on a set operation.
    /// </summary>
    internal ValueSetterDelegate<TValue> decorate<TValue>(ValueSetterDelegate<TValue> setter) {
      if (parent.useClone && this.context.Policy == AssignmentPolicy.CreateNewInstance) {
        return delegate(TValue value) {
          T1 t1 = parent.CreateCopy(this, CurrentInstance);
          SetTempInstance(t1);
          try {
            setter(value);
          } finally {
            ResetTempInstance();
          }
          SetNewInstance(t1);
        };
      } else {
        return setter;
      }
    }

    public IPropertyItem AddEntry<TValue>(string virtualPropertyName, ValueGetterDelegate<TValue> getter, ValueSetterDelegate<TValue> setter) {
      return context.AddEntry(virtualPropertyName, getter, decorate(setter));
    }

    public IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter) {
      return context.AddEntry(virtualPropertyName, getter, decorate(setter));
    }

    public void SetNewInstance(T1 newInstance) {
      context.SetNewInstance(newInstance);
    }

    public IPropertyMapBuilder GetPropertyMapBuilder(object subject) {
      return this.context.GetPropertyMapBuilder(subject);
    }

    public IPropertyMapBuilder GetPropertyMapBuilder(Type type, object subject) {
      return this.context.GetPropertyMapBuilder(type, subject);
    }

    /// <summary>
    /// Recursively applies the cloning logic to the registered setters.
    /// </summary>
    public IPropertyBuildContext<TChild> CreateChildContext<TChild>(string name, GetInstanceDelegate<TChild> getHandler, SetInstanceDelegate<TChild> setHandler, AssignmentPolicy policy) where TChild : class {
      if (parent.useClone) {
        return new OuterWrappingBuildContext<T1, T2, TChild>(this,
                                                      context.CreateChildContext(name, getHandler, setHandler, policy));
      } else {
        return context.CreateChildContext(name, getHandler, setHandler, policy);
      }
    }

    /// <summary>
    /// Returns the current instance or the temporarily set copy.
    /// </summary>
    public T1 CurrentInstance {
      get {
        if (tempInstanceSet) {
          return tempInstance;
        } else {
          return (T1)context.CurrentInstance;
        }
      }
    }

    public AssignmentPolicy Policy {
      get { return context.Policy; }
    }

    public object Lookup(Type type) {
      return context.Lookup(type);
    }

    /// <summary>
    /// Sets the temporary scratch instance.
    /// </summary>
    public void SetTempInstance(T1 instance) {
      this.tempInstance = instance;
      this.tempInstanceSet = true;
    }

    /// <summary>
    /// Resets the temporary scratch instance.
    /// </summary>
    public void ResetTempInstance() {
      this.tempInstance = null;
      this.tempInstanceSet = false;
    }
  }

  /// <summary>
  /// Recursive wrapping context that applies the cloning logic used by the above class.
  /// </summary>
  internal class OuterWrappingBuildContext<T1, T2, TChild> : IPropertyBuildContext<TChild> where T1:class, T2 where T2:class where TChild:class
  {
    private readonly WrappingBuildContext<T1, T2> wrapper;
    private readonly IPropertyBuildContext<TChild> childContext;

    public OuterWrappingBuildContext(WrappingBuildContext<T1, T2> wrapper, IPropertyBuildContext<TChild> childContext) {
      this.wrapper = wrapper;
      this.childContext = childContext;
    }

    public IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter, IEqualityComparer comparer) {
      return childContext.AddEntry(virtualPropertyName, getter, wrapper.decorate(setter), comparer);
    }

    public IPropertyItem AddEntry<TValue>(string virtualPropertyName, ValueGetterDelegate<TValue> getter, ValueSetterDelegate<TValue> setter, IEqualityComparer comparer) {
      return childContext.AddEntry(virtualPropertyName, getter, wrapper.decorate(setter), comparer);
    }

    public IPropertyItem AddEntry<TValue>(string virtualPropertyName, ValueGetterDelegate<TValue> getter, ValueSetterDelegate<TValue> setter) {
      return childContext.AddEntry(virtualPropertyName, getter, wrapper.decorate(setter));
    }

    public IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter) {
      return childContext.AddEntry(virtualPropertyName, getter, wrapper.decorate(setter));
    }

    public void SetNewInstance(TChild newInstance) {
      childContext.SetNewInstance(newInstance);
    }

    public IPropertyMapBuilder GetPropertyMapBuilder(object subject) {
      return childContext.GetPropertyMapBuilder(subject);
    }

    public IPropertyMapBuilder GetPropertyMapBuilder(Type type, object subject) {
      return childContext.GetPropertyMapBuilder(type, subject);
    }

    public IPropertyBuildContext<TInnerChild> CreateChildContext<TInnerChild>(string name, GetInstanceDelegate<TInnerChild> getHandler, SetInstanceDelegate<TInnerChild> setHandler, AssignmentPolicy policy) where TInnerChild : class {
      return new OuterWrappingBuildContext<T1, T2, TInnerChild>(wrapper, childContext.CreateChildContext(name, getHandler, setHandler, policy));
    }

    public TChild CurrentInstance {
      get { return childContext.CurrentInstance; }
    }

    public AssignmentPolicy Policy {
      get { return childContext.Policy; }
    }

    public object Lookup(Type type) {
      return childContext.Lookup(type);
    }
  }
}
