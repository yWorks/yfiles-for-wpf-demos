/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.DataBinding;
using yWorks.Graph;

namespace Demo.yFiles.Option.DataBinding
{
  internal class DefaultOptionBuilderContext<T> : IOptionBuilderContext
  {
    private ILookup innerLookup;
    private IContextLookup contextLookup;
    private ISelectionProvider<T> selectionProvider;
    private string propertyPrefix = string.Empty;
    private IOptionGroup parentGroup;


    public DefaultOptionBuilderContext(ISelectionProvider<T> selectionProvider, IOptionGroup parentGroup) {
      this.innerLookup = Lookups.Empty;
      this.contextLookup = Lookups.EmptyContextLookup;
      this.parentGroup = parentGroup;
      this.selectionProvider = selectionProvider;
    }

    public ILookup InnerLookup {
      get { return innerLookup; }
      set { innerLookup = value; }
    }

    public IContextLookup ContextLookup {
      get { return contextLookup; }
      set { contextLookup = value; }
    }

    public bool BindItem(IOptionItem item, string virtualPropertyName) {
      return BindItem(item, false, virtualPropertyName, true);
    }


    public bool BindItem(IOptionItem item, bool fullyQualifiedId, string id, bool addToOptionHandler) {
      if (fullyQualifiedId) {
        return BindItemHelper(item, id, parentGroup, selectionProvider, string.Empty, addToOptionHandler);
      } else {
        return BindItemHelper(item, id, parentGroup, selectionProvider, propertyPrefix, addToOptionHandler);
      }
    }

    internal static bool BindItemHelper(IOptionItem item, string virtualPropertyName, IOptionGroup group, ISelectionProvider<T> selectionProvider, string prefix, bool addToOptionHandler) {
      bool retval = false;
      IOptionItemFilter<T> filter = item.Lookup<IOptionItemFilter<T>>();
      string finalName = DefaultOptionBuilderContext<object>.CreatePrefix(prefix, virtualPropertyName);
      OptionItemValidities validities;
      if (filter == null) {
        validities = CheckValidity(new DefaultOptionItemFilter<T>(finalName), selectionProvider);
      } else {
        validities = CheckValidity(new DefaultOptionItemFilter<T>(finalName), selectionProvider)
                   & CheckValidity(filter, selectionProvider);
      }
      if (validities != OptionItemValidities.Invalid) { 
        CompositeHandler<T>.Create(selectionProvider, finalName, item);
        if (addToOptionHandler) {
          group.Items.Add(item);
        }
        retval = true;
        if (validities != OptionItemValidities.ReadWrite) {
          item.Enabled = false;
        }
      }
      return retval;
    }

    private static OptionItemValidities CheckValidity(IOptionItemFilter<T> filter, ISelectionProvider<T> selection) {
      if (filter != null) {
        return filter.CheckValidity(selection);
      } else {
        return OptionItemValidities.ReadWrite;
      }
    }

    public virtual IOptionBuilder GetOptionBuilder(object subject) {
      IOptionBuilder builder = contextLookup.Lookup(subject, typeof(IOptionBuilder)) as IOptionBuilder;
      if (builder != null){
        return builder;
      }
      if (subject != null) {
        builder = GetBuilder(subject.GetType());
        if (builder != null) {
          return builder;
        }
      }
      return subject != null ? ReflectionHelper.GetOptionBuilderFromAttribute(subject.GetType()) : null;
    }

    private IOptionBuilder GetBuilder(Type type) {
//      if (typeof(Brush).IsAssignableFrom(type)) {
//        return new DefaultBrushOptionBuilder();
//      }
//      if (type == typeof(Pen)) {
//        return new DefaultPenOptionBuilder();
//      }
      // TODO
//      if(type == typeof(StringFormat)) {
//        return new StringFormatOptionBuilder();
//      }
      return null;
    }

    public virtual IOptionBuilder GetOptionBuilder(Type type, object subject) {
      if (subject != null) {
        return GetOptionBuilder(subject);
      } else {
        IOptionBuilder builder = contextLookup.Lookup(type, typeof (IOptionBuilder)) as IOptionBuilder;
        if (builder == null) {
          builder = GetBuilder(type);
          return builder != null ? builder : ReflectionHelper.GetOptionBuilderFromAttribute(type);
        } else {
          return builder;
        }
      }
    }

    public IOptionBuilderContext CreateChildContext(string prefix) {
      return new ChildBuilderContext(this, this.propertyPrefix, prefix, selectionProvider);
    }

    internal static string CreatePrefix(string prefix, string suffix) {
      string newPrefix;
      if (prefix.Length > 0) {
        newPrefix = suffix.Length > 0 ? prefix + "." + suffix : prefix;
      } else {
        newPrefix = suffix;
      }
      return newPrefix;
    }

    private class ChildBuilderContext : IOptionBuilderContext
    {
      private readonly IOptionBuilderContext parent;
      private readonly string propertyPrefix;
      private readonly ISelectionProvider<T> selectionProvider;
      private IOptionGroup parentGroup;

      internal ChildBuilderContext(IOptionBuilderContext context, string prefix, string groupName, ISelectionProvider<T> selectionProvider) {
        IOptionGroup group = context.Lookup<IOptionGroup>();
        if (group != null && groupName != string.Empty) {
          group.Items.Add(this.parentGroup  = new OptionGroup(groupName));
        } else {
          this.parentGroup = group;
        }
        this.parent = context;
        this.propertyPrefix = CreatePrefix(prefix, groupName);
        this.selectionProvider = selectionProvider;
      }

      public bool BindItem(IOptionItem item, string virtualPropertyName) {
        return BindItem(item, false, virtualPropertyName, true);
      }

      public bool BindItem(IOptionItem item, bool fullyQualifiedId, string id, bool addToOptionHandler) {
        if (fullyQualifiedId) {
          return BindItemHelper(item, id, parentGroup, selectionProvider, string.Empty, addToOptionHandler);
        } else {
          return BindItemHelper(item, id, parentGroup, selectionProvider, propertyPrefix, addToOptionHandler);
        }
      }

      public IOptionBuilder GetOptionBuilder(object subject) {
        return parent.GetOptionBuilder(subject);
      }

      public IOptionBuilder GetOptionBuilder(Type type, object subject) {
        return parent.GetOptionBuilder(type, subject);
      }

      public IOptionBuilderContext CreateChildContext(string prefix) {
        return new ChildBuilderContext(this, this.propertyPrefix,prefix, selectionProvider);
      }

      public object Lookup(Type type) {
        if (type == typeof(IOptionGroup)) {
          return parentGroup;
        } else {
          return parent.Lookup(type);
        }
      }
    }

    public virtual object Lookup(Type type) {
      if (type == typeof(ConstraintManager)) {
        object o = parentGroup.Lookup(type);
        if (o != null) {
          return o;
        }
      }
      if (type == typeof(ISelectionProvider<T>)) {
        return selectionProvider;
      } else if (type == typeof(IOptionGroup)) {
        return parentGroup;
      } else {
        return innerLookup.Lookup(type);
      }
    }
  }
}
