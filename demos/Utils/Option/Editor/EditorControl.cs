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

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.I18N;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Editor
{
  /// <summary>
  /// Base class for <see cref="Control"/>s that create a GUI for
  /// an <see cref="OptionHandler"/>.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class EditorControl : Control
  {
    private IModelView view;

    /// <summary>
    /// The routed command that performs the <see cref="CommitValues"/> action.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly RoutedCommand CommitValuesCommand = new RoutedCommand("CommitValues", typeof(EditorControl));
    /// <summary>
    /// The routed command that performs the <see cref="ResetValues"/> action.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly RoutedCommand ResetValuesCommand = new RoutedCommand("ResetValues", typeof(EditorControl));
    /// <summary>
    /// The routed command that performs the <see cref="AdoptValues"/> action.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly RoutedCommand AdoptValuesCommand = new RoutedCommand("AdoptValues", typeof(EditorControl));

    /// <summary>
    /// The routed command that performs the <see cref="ResetItem"/> action.
    /// </summary>
    /// <remarks>
    /// This command is tailored for single <see cref="IOptionItem"/>s, which need to be passed
    /// as the <see cref="ExecutedRoutedEventArgs.Parameter">commmand parameter</see>.
    /// </remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly RoutedCommand ResetItemCommand = new RoutedCommand("ResetItem", typeof(EditorControl));

    static EditorControl () {
      CommandManager.RegisterClassCommandBinding(typeof(EditorControl), new CommandBinding(CommitValuesCommand, OnCommitValuesCommandExecuted));
      CommandManager.RegisterClassCommandBinding(typeof(EditorControl), new CommandBinding(ResetValuesCommand, OnResetValuesCommandExecuted));
      CommandManager.RegisterClassCommandBinding(typeof(EditorControl), new CommandBinding(AdoptValuesCommand, OnAdoptValuesCommandExecuted));
      CommandManager.RegisterClassCommandBinding(typeof(EditorControl), new CommandBinding(ResetItemCommand, OnResetItemCommandExecuted, CanExecuteResetItem));
      DefaultStyleKeyProperty.OverrideMetadata(typeof(EditorControl), new FrameworkPropertyMetadata(typeof(EditorControl)));
    }

    private static void CanExecuteResetItem(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = e.Parameter is IOptionItem;
      e.Handled = true;
    }

    private static void OnCommitValuesCommandExecuted(object source, ExecutedRoutedEventArgs args) {
      EditorControl control = (EditorControl) source;
      if (control != null) {
        control.CommitValues();
        args.Handled = true;
      }
    }

    private static void OnResetItemCommandExecuted(object source, ExecutedRoutedEventArgs args) {
      EditorControl control = (EditorControl) source;
      if (control != null) {
        control.ResetItem((IOptionItem)args.Parameter);
        args.Handled = true;
      }
    }
    private static void OnAdoptValuesCommandExecuted(object source, ExecutedRoutedEventArgs args) {
      EditorControl control = (EditorControl) source;
      if (control != null) {
        control.AdoptValues();
        args.Handled = true;
      }
    }
    private static void OnResetValuesCommandExecuted(object source, ExecutedRoutedEventArgs args) {
      EditorControl control = (EditorControl) source;
      if (control != null) {
        control.ResetValues();
        args.Handled = true;
      }
    }

    /// <summary>
    /// Create a new instance that is bound to an <see cref="IModelView"/> abstraction
    /// of an option handler.
    /// </summary>
    public EditorControl() {
      DefaultStyleKey = typeof (EditorControl);
    }

    /// <summary>
    /// Dependency property for the <see cref="IsAutoCommit"/> property.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty IsAutoCommitProperty =
      DependencyProperty.Register("IsAutoCommit", typeof (bool), typeof (EditorControl),
                                  new FrameworkPropertyMetadata(false, OnIsAutoCommitChanged));

    private static void OnIsAutoCommitChanged(DependencyObject source, DependencyPropertyChangedEventArgs args) {
      EditorControl control = source as EditorControl;
      if (control != null && control.view != null) {
        control.view.IsAutoCommit = (bool) args.NewValue;
      }
    }

    /// <summary>
    /// Dependency property for the <see cref="View"/> property.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty ViewProperty =
      DependencyProperty.Register("View", typeof (IModelView), typeof (EditorControl),
                                  new FrameworkPropertyMetadata(null, OnViewChanged));

    private static void OnViewChanged(DependencyObject source, DependencyPropertyChangedEventArgs args) {
      EditorControl control = source as EditorControl;
      if (control != null) {
        control.OnViewChanged((IModelView) args.OldValue, (IModelView) args.NewValue);
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="IModelView">view</see> of an <see cref="OptionHandler"/>.
    /// </summary>
    /// <remarks>
    /// The editor will use the view to store temporary edits in the option handler.
    /// </remarks>
    /// <value>The view to use.</value>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IModelView View {
      get { return (IModelView) GetValue(ViewProperty); }
      set { SetValue(ViewProperty, value);}
    }

    /// <summary>
    /// Dependency property for the <see cref="OptionHandler"/> property.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty OptionHandlerProperty =
      DependencyProperty.Register("OptionHandler", typeof (OptionHandler), typeof (EditorControl),
                                  new FrameworkPropertyMetadata(null, OnOptionHandlerChanged));

    private static void OnOptionHandlerChanged(DependencyObject source, DependencyPropertyChangedEventArgs args) {
      EditorControl control = source as EditorControl;
      if (control != null) {
        control.OnOptionHandlerChanged((OptionHandler) args.OldValue, (OptionHandler) args.NewValue);
      }
    }

    /// <summary>
    /// Convenience property that will automatically create a <see cref="View"/> for the given
    /// option handler.
    /// </summary>
    /// <value>The option handler to display in this editor.</value>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public OptionHandler OptionHandler {
      get { return (OptionHandler) GetValue(OptionHandlerProperty); }
      set { SetValue(OptionHandlerProperty, value); }
    }

    /// <summary>
    /// Callback that is triggered once the <see cref="OptionHandler"/> property changes.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnOptionHandlerChanged(OptionHandler oldValue, OptionHandler newValue) {
      if (oldValue != null) {
        IModelView view = View;
        if (view is CopiedOptionHandler && view.Handler == oldValue) {
          CopiedOptionHandler oldView = (CopiedOptionHandler)view;
          if (newValue != null) {
            View = CreateView(newValue);
          } else {
            View = null;
          }
          oldView.Dispose();
        } else {
          if (newValue != null) {
            View = CreateView(newValue);
          } else {
            View = null;
          }
        }
      } else {
        if (newValue != null) {
          View = CreateView(newValue);
        } else {
          View = null;
        }
      }

      if (newValue != null) {
        if (I18NFactory != null) {
          Title = I18NFactory.GetString(newValue.Name, newValue.Name);
        } else {
          Title = newValue.Name;
        }
      } else {
        Title = string.Empty;
      }
    }

    /// <summary>
    /// Creates the <see cref="View"/> for the provided <see cref="OptionHandler"/>.
    /// </summary>
    /// <remarks>
    /// This callback method is used if the <see cref="OptionHandler"/> property is set to 
    /// actually create the <see cref="View"/>.
    /// </remarks>
    /// <param name="forHandler">The handler to create the view for.</param>
    /// <returns>A view instance.</returns>
    /// <seealso cref="CreateView(Handler.OptionHandler, bool, bool)"/>.
    protected virtual IModelView CreateView(OptionHandler forHandler) {
      return CreateView(forHandler, IsAutoAdopt, IsAutoCommit);
    }

    /// <summary>
    /// Helper method that creates a view for a specifice <see cref="Handler.OptionHandler"/>.
    /// </summary>
    /// <param name="forHandler">The handler to create a view for.</param>
    /// <param name="autoAdopt">if set to <see langword="true"/> value changes in 
    /// the handler will automatically be adopted by the view.</param>
    /// <param name="autoCommit">if set to <see langword="true"/> value changes in 
    /// the view will automatically be committed to the handler.</param>
    /// <returns>A new view.</returns>
    public static IModelView CreateView(OptionHandler forHandler, bool autoAdopt, bool autoCommit) {
      CopiedOptionHandler view2 = new CopiedOptionHandler(forHandler);
      view2.IsAutoAdopt = autoAdopt;
      view2.IsAutoCommit = autoCommit;
      return view2;
    }

    /// <summary>
    /// Called when the <see cref="View">view</see> changed.
    /// </summary>
    /// <param name="oldView">The old view.</param>
    /// <param name="newView">The new view.</param>
    protected virtual void OnViewChanged(IModelView oldView, IModelView newView) {
      if (view != null) {
        SetValue(RootItemProperty, null);
      }
      this.view = newView;
      if (this.view != null) {
        view.IsAutoAdopt = IsAutoAdopt;
        view.IsAutoCommit = IsAutoCommit;
        IOptionGroup item = view.GetViewItem(view.Handler) as IOptionGroup;
        SetValue(RootItemProperty, item);
      } else {
        SetValue(RootItemProperty, null);
      }
    }

    /// <summary>
    /// Dependency property for the <see cref="RootItem"/> property.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty RootItemProperty = DependencyProperty.Register("RootItem", typeof(IOptionGroup), typeof(EditorControl), new PropertyMetadata(null));

    /// <summary>
    /// Gets the root item that represents the option handler root.
    /// </summary>
    /// <remarks>
    /// These are both <see cref="IOptionItem"/> and <see cref="IOptionGroup"/>. <see cref="HierarchicalDataTemplate"/>s
    /// can be used to bind to these properties.
    /// </remarks>
    /// <value>An observable collection of the items in this editor.</value>
    public IOptionGroup RootItem {
      get {
        return (IOptionGroup) GetValue(RootItemProperty);
      }
    }

    /// <summary>
    /// Controls the synchronization mode of this control for
    /// writing back the values to the OptionHandler.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public virtual bool IsAutoCommit {
      get { return (bool) GetValue(IsAutoCommitProperty); }
      set { SetValue(IsAutoCommitProperty, value); }
    }

    /// <summary>
    /// Dependency property for the <see cref="IsAutoAdopt"/> property.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly DependencyProperty IsAutoAdoptProperty =
      DependencyProperty.Register("IsAutoAdopt", typeof(bool), typeof(EditorControl),
                                  new FrameworkPropertyMetadata(false, OnIsAutoAdoptChanged));

    private static void OnIsAutoAdoptChanged(DependencyObject source, DependencyPropertyChangedEventArgs args) {
      EditorControl control = source as EditorControl;
      if (control != null && control.view != null) {
        control.view.IsAutoAdopt = (bool)args.NewValue;
      }
    }

    /// <summary>
    /// Controls the synchronization mode of this control for
    /// external changes.
    /// </summary>
    /// <value>
    /// 	<see langword="true"/> if this instance should automatically adopt changes of the values in the underlying model; <see langword="false"/> otherwise.
    /// </value>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public virtual bool IsAutoAdopt {
      get { return (bool) GetValue(IsAutoAdoptProperty); }
      set { SetValue(IsAutoAdoptProperty, value); }
    }    

    /// <summary>
    /// Write back all values to the underlying OptionHandler
    /// </summary>
    /// <remarks>
    /// Calls <see cref="IModelView.CommitValues"/>.
    /// </remarks>
    public virtual void CommitValues() {
      if (view != null) {
        view.CommitValues();
      }
    }

    /// <summary>
    /// Reset all values.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="IModelView.ResetValues"/>.
    /// </remarks>
    public virtual void ResetValues() {
      if (view != null) {
        view.ResetValues();
      }
    }
    
    /// <summary>
    /// Get all values from the underlying OptionHandler
    /// </summary>
    /// <remarks>
    /// Calls <see cref="IModelView.AdoptValues"/>.
    /// </remarks>
    public virtual void AdoptValues() {
      if (view != null) {
        view.AdoptValues();
      }
    }

    /// <summary>
    /// Returns the <see cref="I18N.I18NFactory"/> that is currently
    /// in effect for the underlying handler
    /// </summary>
    public I18NFactory I18NFactory {
      get { return view != null ? view.Lookup(typeof(I18NFactory)) as I18NFactory : null; }
    }

    /// <summary>
    /// Gets the title for this Control, which may appear as the window title
    /// </summary>
    /// <remarks>This returns the (possibly localized) name of the underlying OptionHandler.</remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public virtual string Title {
      get {
        return (string) GetValue(TitleProperty);
      }
      set {
        SetValue(TitleProperty, value);
      }
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("TitleProperty",
                                                                                          typeof (string),
                                                                                          typeof (EditorControl),
                                                                                          new PropertyMetadata(string.Empty));

    /// <summary>
    /// Calls <see cref="IModelView.ResetValue"/> on the given item.
    /// </summary>
    /// <param name="item">The item to reset the value of.</param>
    public virtual void ResetItem(IOptionItem item) {
      view.ResetValue(item);
    }
  }
}
