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
using System.Collections.Generic;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.I18N;
using Demo.yFiles.Option.View;
using yWorks.Graph;

namespace Demo.yFiles.Option.Handler
{
  /// <summary>
  /// This class can be used to manage settings and options that belong together
  /// with a consistent interface.
  /// </summary>
  /// <remarks>Each item is represented by an instance of <see cref="IOptionItem"/>. 
  /// To edit values in the handler, you can create editors with subclasses of 
  /// <see cref="EditorFactory"/>.</remarks>
  public class OptionHandler : OptionGroup
  {
    //todo: make more specific
    private List<WeakReference> views = new List<WeakReference>();

    //not sure if this should be serialized...
    private I18NFactory i18nFactory;

    /// <summary>
    /// Gets or sets an <see cref="I18NFactory"/> instance that can be
    /// used for localization of various string values such as item names, tooltips,
    /// button labels, etc.
    /// </summary>
    /// <remarks>If no instance has been set, retrieving this property
    /// returns a default implementation.</remarks>
    public I18NFactory I18nFactory {
      get {
        return i18nFactory;
      }
      set { i18nFactory = value; }
    }

    public OptionGroup GetGroupByName(string name) {
      return (OptionGroup) OptionItemExtensions.GetGroupByName(this, name);
    }

    /// <inheritdoc/>
    public override object Lookup(Type type) {
      if (type == typeof(IOptionGroup)) {
        return this;
      } else if (type == typeof(OptionHandler)) {
        return this;
      } else if (type == typeof(I18NFactory)) {
        return I18nFactory;
      } else {
        return base.Lookup(type);
      }
    }

    /// <summary>
    /// Creates a new option handler.
    /// </summary>
    /// <remarks>This option handler always contains one OptionGroup by default, 
    /// with the same name as the title of the handler.</remarks>
    public OptionHandler(string title)
      : base(title) {
      //_title = title;
      //OptionGroup g = new OptionGroup(title);
      //AddOptionGroup(g);
      }

    internal void AddView(IModelView view) {
      if (views == null) {
        views = new List<WeakReference>();
      }
      //todo: add notification
      views.Add(new WeakReference(view));
      OnViewChanged(view, true);
    }

    internal void RemoveView(IModelView view) {
      foreach (WeakReference reference in views) {
        if (reference.Target == view) {
          views.Remove(reference);
          break;
        }
      }
      OnViewChanged(view, false);
      //todo: add notification
    }

    #region internal methods

    internal IList<IModelView> ActiveViews {
      get {
        List<IModelView> list = new List<IModelView>();

        List<WeakReference> deletable = new List<WeakReference>();
        foreach (WeakReference view in views) {
          IModelView mv = view.Target as IModelView;
          if (mv != null) {
            list.Add(mv);
          } else {
            deletable.Add(view);
          }
        }
        foreach (WeakReference reference in deletable) {
          views.Remove(reference);
        }
        return list.AsReadOnly();
      }
    }

    #endregion

    /// <summary>
    /// EventBracketing method for content changes.
    /// </summary>
    /// <remarks>When this method is called, all <see cref="ViewChanged"/> events
    /// from this handler are temporarily suppressed, until <see cref="EndContentChange"/>
    /// is called. This can be used for complex changes of the handler's content, when it is not necessary
    /// to raise these events, e.g. when views will be recreated anyway.</remarks>
    public void StartContentChange() {
      isUpdating = true;
      SuppressEvents = true;
    }

    protected bool SuppressEvents { get; set; }

    /// <summary>
    /// EventBracketing method for content changes.
    /// </summary>
    /// <remarks>When this method is called, <see cref="ViewChanged"/> events
    /// from this handler are not suppressed anymore. Together with <see cref="StartContentChange"/>,
    /// this can be used for complex changes of the handler's content, when it is not necessary
    /// to raise these events, e.g. when views will be recreated anyway.</remarks>
    public void EndContentChange() {
      SuppressEvents = false;
      isUpdating = false;
    }

    private void OnViewChanged(IModelView view, bool isAdded) {
      if (ViewChanged != null) {
        ViewChanged(this, new ViewChangeEventArgs(isAdded, view));
      }
    }

    /// <summary>
    /// Returns whether the handler is currently updating its structure.
    /// </summary>
    /// <remarks>When this returns true, querying the handler for items is unsafe.</remarks>
    public bool IsUpdating {
      get { return isUpdating; }
    }

    private bool isUpdating = false;

    /// <inheritdoc/>
    internal event ViewChangeHandler ViewChanged;

    /// <summary>
    /// Populates this instance from scratch using a provided selection provider.
    /// </summary>
    /// <remarks>This instance will be cleared, and all constraints on it will be reset. 
    /// The builder inspects the first <see cref="IPropertyItemDescriptor{T}"/> from
    /// <paramref name="selectionProvider"/> and creates an <see cref="IOptionBuilder"/> instance 
    /// that will <see cref="IOptionBuilder.AddItems"/> to this instance via the builder.
    /// </remarks>
    /// <param name="selectionProvider"></param>
    /// <param name="contextLookup">The lookup tunnel through to the created
    /// <see cref="IOptionBuilderContext"/> that will be used to query the <see cref="IOptionBuilder"/>
    /// instances for recursive sets of properties.</param>
    public virtual void BuildFromSelection<T>(ISelectionProvider<T> selectionProvider, IContextLookup contextLookup) {
      StartContentChange();
      try {
        Clear();
        DefaultOptionBuilderContext<T> context;
        context = new DefaultOptionBuilderContext<T>(selectionProvider, this);
        context.ContextLookup = contextLookup;

        ConstraintManager constraintManager = this.Lookup(typeof (ConstraintManager)) as ConstraintManager;
        if (constraintManager == null) {
          constraintManager = new ConstraintManager(this);
          this.SetLookup(typeof(ConstraintManager), constraintManager);
        }
        constraintManager.Clear();
        IEnumerator<IPropertyItemDescriptor<T>> enumerator = selectionProvider.Selection.GetEnumerator();
        if (enumerator.MoveNext()) {
          IPropertyItemDescriptor<T> descriptor = enumerator.Current;
          T item;
          item = descriptor.Item;
          IOptionBuilder builder = context.GetOptionBuilder(item);
          if (builder != null) {
            builder.AddItems(context, item.GetType(), item);
          }
        }
      } finally {
        EndContentChange();
      }
    }
  }


  internal delegate void ViewChangeHandler(object source, ViewChangeEventArgs e);

  internal class ViewChangeEventArgs : EventArgs
  {
    private bool isAdded;
    private IModelView view;

    public ViewChangeEventArgs(bool isAdded, IModelView view) {
      this.isAdded = isAdded;
      this.view = view;
    }

    public bool IsAdded {
      get { return isAdded; }
    }

    public IModelView View {
      get { return view; }
    }
  }
}
