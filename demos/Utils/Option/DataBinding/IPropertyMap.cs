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
using System.Collections;
using System.Collections.Generic;
using Demo.yFiles.Option.DataBinding;
using yWorks.Graph;

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// Interface for classes that manage a list of virtual properties for a specific context.
  /// </summary>
  /// <remarks>
  /// The context is implicitly held by implementations of this interface.
  /// </remarks>
  public interface IPropertyMap
  {
    /// <summary>
    /// Create and add a new virtual property with the given parameters to this map.
    /// </summary>
    /// <param name="virtualPropertyName">The name of the property</param>
    /// <param name="getter">The getter that is used to retrieve the value from the property</param>
    /// <param name="setter">The setter that is used to set the value on the property</param>
    /// <returns>The item that has been added to the property map.</returns>
    IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter);

    /// <summary>
    /// Retrieve the specified virtual property.
    /// </summary>
    /// <param name="virtualPropertyName">The virtual name of the property.</param>
    /// <returns>The property item with the virtual name or <see langword="null"/></returns>
    IPropertyItem GetEntry(string virtualPropertyName);

    /// <summary>
    /// Retrieve an anumeration of the fully qualified property names in this map (prefix+property name)
    /// </summary>
    IEnumerable<string> VirtualProperties { get; }

    /// <summary>
    /// Create and add a new virtual property with the given parameters to this map.
    /// </summary>
    /// <param name="virtualPropertyName">The name of the property</param>
    /// <param name="getter">The getter that is used to retrieve the value from the property</param>
    /// <param name="setter">The setter that is used to set the value on the property</param>
    /// <returns>The item that has been added to the map.</returns>
    /// <param name="comparer">An optional comparer that is used instead of object equality for forming the composites and write back</param>
    IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter,
                           IEqualityComparer comparer);
  }

  /// <summary>
  /// Default implementation of <see cref="IPropertyMap"/>
  /// </summary>
  internal sealed class DefaultPropertyMap : IPropertyMap
  {
    private IDictionary<string, IPropertyItem> internalMap;
    
    public DefaultPropertyMap() {
      internalMap = new Dictionary<string, IPropertyItem>();
    }


    #region IPropertyMap Members
    /// <inheritdoc/>
    public IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter) {
      return AddEntry(virtualPropertyName, getter, setter, null);
    }

    /// <inheritdoc/>
    public IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter,
                                          IValueSetter setter, IEqualityComparer comparer) {
        DefaultPropertyItem item = new DefaultPropertyItem(getter, setter, comparer);
        internalMap[virtualPropertyName] = item;
        return item;     
    }
        
    /// <inheritdoc/>
    public IPropertyItem GetEntry(string virtualPropertyName) {
      IPropertyItem retval;
      internalMap.TryGetValue(virtualPropertyName, out retval);
      return retval;
    }

    /// <inheritdoc/>
    public IEnumerable<string> VirtualProperties {
      get {
        return internalMap.Keys;
      }
    }

    #endregion

    private class DefaultPropertyItem : IPropertyItem
    {
      private IValueGetter getter;
      private IValueSetter setter;
      private readonly IEqualityComparer equalityComparer;
      private ILookup myLookup = Lookups.Empty;

      public DefaultPropertyItem(IValueGetter getter, IValueSetter setter, IEqualityComparer equalityComparer) {
        this.getter = getter;
        this.setter = setter;
        this.equalityComparer = equalityComparer;
      }

      #region IPropertyItem Members

      public IValueGetter Getter {
        get { return getter; }
      }

      public IValueSetter Setter {
        get { return setter; }
      }

      public IEqualityComparer EqualityComparer {
        get { return equalityComparer; }
      }

      #endregion

      #region ILookup Members

      public virtual object Lookup(Type type) {
        return myLookup.Lookup(type);
      }

      #endregion
    }
  }

  /// <summary>
  /// This is the interface that is implemented by classes that create a map of 
  /// named properties for a given 
  /// <see cref="IPropertyBuildContext{TSubject}.CurrentInstance">dynamically retrieved</see>
  /// instance.
  /// </summary>
  /// <remarks>
  /// Note that most implementations should derive from the abstract base class
  /// <see cref="PropertyMapBuilderBase{TSubject}"/> which provides a much more
  /// easily to implement interface.
  /// </remarks>
  /// <seealso cref="PropertyMapBuilderBase{TSubject}"/>
  /// <seealso cref="PropertyMapBuilderBase{TSubject}.BuildPropertyMapImpl"/>
  public interface IPropertyMapBuilder
  {
    /// <summary>
    /// Populates the map of properties via the methods provided by the <paramref name="builder"/>.
    /// </summary>
    /// <remarks>
    /// Implementations need to always use the <see cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
    /// to retrieve the values from and use <see cref="IPropertyBuildContext{TSubject}.SetNewInstance"/>
    /// to propagate referential changes. If the builder demands a <see cref="IPropertyBuildContext{TSubject}.Policy"/>
    /// of type <see cref="AssignmentPolicy.CreateNewInstance"/> the implementation needs to 
    /// ensure that the state of the <see cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
    /// is not changed upon a call to the setters. Instead the setter implementations need to create
    /// a copy of the subject and publish the copy via the <see cref="IPropertyBuildContext{TSubject}.SetNewInstance"/>
    /// method.
    /// </remarks>
    /// <typeparam name="T">The type of the builder context that is passed to this method.</typeparam>
    /// <param name="builder">The builder to use for populating the property map.</param>
    /// <seealso cref="PropertyMapBuilderBase{TSubject}"/>
    /// <seealso cref="PropertyMapBuilderBase{TSubject}.BuildPropertyMapImpl"/>
    void BuildPropertyMap<T>(IPropertyBuildContext<T> builder) where T: class;
  }

  /// <summary>
  /// The delegate that is used by <see cref="IPropertyBuildContext{TSubject}"/>'s
  /// <see cref="IPropertyBuildContext{TSubject}.CreateChildContext{TChild}"/> method.
  /// </summary>
  /// <typeparam name="T">The type of the instance that is retrieved via this getter.</typeparam>
  /// <returns>The value of the instance.</returns>
  /// <seealso cref="IPropertyBuildContext{TSubject}.CreateChildContext{TChild}"/>
  public delegate T GetInstanceDelegate<T>();
  /// <summary>
  /// The delegate that is used by <see cref="IPropertyBuildContext{TSubject}"/>'s
  /// <see cref="IPropertyBuildContext{TSubject}.CreateChildContext{TChild}"/> method.
  /// </summary>
  /// <typeparam name="T">The type of the instance that will be set via this setter.</typeparam>
  /// <param name="newInstance">The new instance or value to set.</param>
  /// <seealso cref="IPropertyBuildContext{TSubject}.CreateChildContext{TChild}"/>
  public delegate void SetInstanceDelegate<T>(T newInstance);
}