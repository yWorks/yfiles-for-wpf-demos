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
using System.Collections.Generic;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.DataBinding;
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Option.DataBinding
{

  /// <summary>
  /// The default implementation of <see cref="ISelectionProvider{T}"/>
  /// that is used to <see cref="OptionHandler.BuildFromSelection{T}">drive OptionHandler instances.</see>.
  /// </summary>
  /// <remarks>
  /// This implementation automatically <see cref="EventCollapseTimeSpan">collapses multiple requests</see>
  /// into a single request by default. This implies that by default calling <see cref="UpdatePropertyViews()"/>
  /// will <b>not immediately</b> trigger an update.
  /// </remarks>
  /// <seealso cref="EventCollapseTimeSpan"/>
  /// <typeparam name="T">The type of the items to act on.</typeparam>
  public class DefaultSelectionProvider<T> : SelectionProviderBase<T> where T:class 
  {
    private IList<IPropertyItemDescriptor<T>> descriptors = new List<IPropertyItemDescriptor<T>>();

    /// <summary>
    /// Creates a new instance using the provided domain and a filter.
    /// </summary>
    /// <param name="model">The domain to work on.</param>
    public DefaultSelectionProvider(IEnumerable<T> model) : this(model, delegate { return true; }) {}

    /// <summary>
    /// Creates a new instance using the provided domain and a filter.
    /// </summary>
    /// <param name="model">The domain to work on.</param>
    /// <param name="filter">A filter that works on the domain.</param>
    public DefaultSelectionProvider(IEnumerable<T> model, Predicate<T> filter) {
      eventfilter = new EventFilter<EventArgs>();
      eventfilter.EventRestartsTimer = true;
      this.model = model;
      this.filter = filter;
      link = Lookups.CreateContextLookupChainLink(DescriptorContextLookupCallback);
      eventfilter.Event += new EventHandler<EventArgs>(eventfilter_Event);
    }

    void eventfilter_Event(object sender, EventArgs e) {
      if (!InUpdate) {
        UpdateProperties(model, descriptors);
        OnSelectionContentUpdated();
      }
    }

    private EventFilter<EventArgs> eventfilter;
    private IEnumerable<T> model;
    private readonly Predicate<T> filter;

    /// <summary>
    /// Gets or sets the <see cref="IContextLookup"/> implementation that
    /// will be used by <see cref="DescriptorContextLookupCallback"/>
    /// which itself will be used to satisfy calls to the
    /// implementation of <see cref="IPropertyBuildContext{TSubject}.GetPropertyMapBuilder(object)"/>
    /// and <see cref="IPropertyBuildContext{TSubject}.GetPropertyMapBuilder(Type,object)"/>
    /// during calls to <see cref="CreateDescriptor(T)"/>.
    /// </summary>
    public IContextLookup ContextLookup {
      get { return contextLookup; }
      set { contextLookup = value; }
    }

    private IContextLookup contextLookup = Lookups.EmptyContextLookup;
    private IContextLookupChainLink link;

    #region ISelectionProvider Members

    /// <inheritdoc/>
    public override ICollection<IPropertyItemDescriptor<T>> Selection {
      get { return descriptors; }
    }

    /// <summary>
    /// Updates the items, calling <see cref="CreateDescriptor(T)"/>
    /// for each <see cref="IsValid"/> item in <paramref name="sourceItems"/>.
    /// </summary>
    protected virtual void UpdateProperties(IEnumerable<T> sourceItems, ICollection<IPropertyItemDescriptor<T>> targetList) {
      eventfilter.Cancel();
      targetList.Clear();
      foreach (T o in sourceItems) {
        if (IsValid(o)) {
          targetList.Add(CreateDescriptor(o));
        }
      }
    }

    /// <inheritdoc/>
    public override void UpdatePropertyViews() {
      if (!InUpdate) {
        eventfilter.OnEvent();
      }
    }

    /// <summary>
    /// Foreces the update of the properties view, regardless of the <see cref="EventCollapseTimeSpan"/> value
    /// currently being set.
    /// </summary>
    public virtual void UpdatePropertyViewsNow() {
      if (!InUpdate) {
        eventfilter.OnEvent();
        eventfilter.Flush();
      }
    }

    /// <summary>
    /// The timespan during which recurring calls to <see cref="UpdatePropertyViews"/>
    /// are collapsed into a single call.
    /// </summary>
    /// <remarks>
    /// The default is 500 milliseconds.
    /// </remarks>
    public TimeSpan EventCollapseTimeSpan {
      get {
        return eventfilter.Duration;
      }
      set {
        eventfilter.Duration = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a new event restarts the timer.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>.
    /// </remarks>
    /// <value><see langword="true"/> if a new event restarts the timer; <see langword="false"/> otherwise.</value>
    /// 
    public bool EventRestartsTimer {
      get { return eventfilter.EventRestartsTimer; }
      set { eventfilter.EventRestartsTimer = value; }
    }

    /// <summary>
    /// Creates and populates a <see cref="IPropertyItemDescriptor{T}"/>
    /// for the given item.
    /// </summary>
    /// <remarks>
    /// The actual creation of the descriptor is performed by <see cref="CreateDescriptor(T)"/>.
    /// <br/>
    /// This implementation will create the <see cref="IPropertyMap"/>
    /// and retrieve an appropriate <see cref="IPropertyMapBuilder"/> for the item
    /// to <see cref="IPropertyMapBuilder.BuildPropertyMap{T}">build the map</see>.
    /// During the build calls to <see cref="IPropertyBuildContext{TSubject}.GetPropertyMapBuilder(object)"/>
    /// and <see cref="IPropertyBuildContext{TSubject}.GetPropertyMapBuilder(Type,object)"/>
    /// will be delegated to <see cref="DescriptorContextLookupCallback"/>.
    /// Also the <see cref="ILookup"/> functionality of the <see cref="IPropertyBuildContext{TSubject}"/>
    /// will be satisfied by this implementations <see cref="SelectionProviderBase{T}.Lookup"/>
    /// method.
    /// </remarks>
    /// <param name="o">The item to introspect and create a descriptor for.</param>
    /// <returns>The descriptor to put into the <see cref="Selection"/>.</returns>
    protected virtual IPropertyItemDescriptor<T> CreateDescriptor(T o) {
      IPropertyMap map =  new DefaultPropertyMap();
      IPropertyBuildContext<T> propertyBuildContext = 
        new DefaultPropertyBuildContext<T>(this, link, map, o);
      IPropertyMapBuilder builder;
      builder = propertyBuildContext.GetPropertyMapBuilder(o);
      if (builder != null)
      {
        builder.BuildPropertyMap(propertyBuildContext);       
      }
      return CreateDescriptor(o, map);
    }

    /// <summary>
    /// The <see cref="IContextLookup"/> callback that is used during <see cref="CreateDescriptor(T)"/>.
    /// </summary>
    /// <remarks>
    /// This callback is mainly used to retrieve <see cref="IPropertyMapBuilder"/> instances
    /// for a given item.
    /// </remarks>
    /// <see cref="ContextLookup"/>.
    protected virtual object DescriptorContextLookupCallback(object subject, Type type) {
      return contextLookup.Lookup(subject, type);
    }

    /// <summary>
    /// Factory method that actually creates the descriptor instance for the provided parameters.
    /// </summary>
    protected virtual IPropertyItemDescriptor<T> CreateDescriptor(T o, IPropertyMap map) {
      return new DefaultPropertyItemDescriptor<T>(o, map);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the registered predicate does so
    /// </summary>
    /// <param name="o">The item to check.</param>
    /// <returns>Whether to add this item to the selection.</returns>
    protected virtual bool IsValid(T o) {
      return filter(o);
    }

    #endregion

    /// <inheritdoc/>
    protected override void UpdateProperties() {
      UpdateProperties(model, descriptors);
    }
  }
}
