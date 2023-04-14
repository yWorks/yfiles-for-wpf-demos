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
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using Demo.yFiles.GraphEditor.Modules.Layout;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.I18N;
using yWorks.Graph;

namespace Demo.yFiles.GraphEditor.Modules
{
  /// <summary>
  /// Base class for modules.
  /// </summary>
  /// <remarks>Modules encapsulate common and often used functionality
  /// into a simple interface that provides easy editing of the
  /// configuration. Typical uses are to provide a convenient interface 
  /// for configuration of layout algorithms.</remarks>
  /// <seealso cref="LayoutModule"/>
  public abstract class YModule : IDisposable
  {
    private OptionHandler handler;
    private string moduleName;
    private ResourceManager resourceManager;
    private bool modal = true;


    public bool Modal {
      get { return modal; }
      set { modal = value; }
    }

    private ILookup context;

    /// <summary>
    /// Get or set the <see cref="ResourceManager"/> for localization of the module
    /// </summary>
    public ResourceManager ResourceManager {
      get {
        if (resourceManager == null) {
          resourceManager =
          new ResourceManager(GetType().FullName,
                              GetType().Assembly);
        }
        return resourceManager;
      }
      //set { resourceManager = value; }
    }

    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <param name="moduleName">The name of the module</param>
    protected YModule(string moduleName) {
      this.moduleName = moduleName;
    }

    /// <summary>
    /// Get the option handler that is used to manage the settings of this module.
    /// </summary>
    public virtual OptionHandler Handler {
      get {
        if (handler == null) {
          CreateHandler();
        }
        return handler;
      }
      internal set { handler = value; }
    }

    /// <summary>
    /// Get the name of the module
    /// </summary>
    public string ModuleName {
      get { return moduleName; }
    }

    /// <summary>
    /// Start execution of the module.
    /// </summary>
    /// <param name="newContext">The context in which the module should execute.</param>
    public virtual Task Start(ILookup newContext) {
      CheckReentrant(newContext);
      try {
        OnModuleEvent(ModuleEventArgs.EventType.BEFORE_CONFIGURATION);
        ConfigureModule();
        OnModuleEvent(ModuleEventArgs.EventType.AFTER_CONFIGURATION);
        OnModuleEvent(ModuleEventArgs.EventType.BEFORE_RUN);
        RunModule();
        OnModuleEvent(ModuleEventArgs.EventType.AFTER_RUN);
        OnModuleEvent(ModuleEventArgs.EventType.BEFORE_DISPOSE);
        Dispose();
        OnModuleEvent(ModuleEventArgs.EventType.AFTER_DISPOSE);
      } finally {
        FreeReentrant();
      }
      return Task.FromResult<object>(null);
    }

    protected void FreeReentrant() {
      this.context = null;
    }

    protected void CheckReentrant(ILookup newContext) {
      if (this.context != null) {
        throw new Exception("Start is not reentrant!");
      }
      CheckContext(newContext);
      context = newContext;
    }

    /// <summary>
    /// Return the current execution context
    /// </summary>
    protected ILookup Context {
      get { return context; }
    }

    ///<inheritdoc/>
    public virtual void Dispose() {
      //does nothing by default
    }

    /// <summary>
    /// This can be used to configure the module (apart from setting up the <see cref="Handler"/>
    /// </summary>
    protected virtual void ConfigureModule() {
    }

    /// <summary>
    /// This method provides the core funtionality.
    /// </summary>
    protected abstract Task RunModule();

    /// <summary>
    /// Raises a <see cref="ModuleEvent"/> event
    /// </summary>
    /// <param name="type">The specific event that should be raised</param>
    protected virtual void OnModuleEvent(ModuleEventArgs.EventType type) {
      if (ModuleEvent != null) {
        ModuleEvent(this, new ModuleEventArgs(type));
      }
    }

    private void CreateHandler() {
      Handler = new OptionHandler(ModuleName);
      SetupHandler();
      ConfigureResources();
    }

    /// <summary>
    /// Sets up i18n resources
    /// </summary>
    internal void ConfigureResources() {
      if (Handler != null) {
        I18NFactory.AddResourceManager(ModuleName, ResourceManager);
      }
    }

    private ResourceManagerI18NFactory I18NFactory {
      get {
        if (Handler.I18nFactory == null || !(Handler.I18nFactory is ResourceManagerI18NFactory)) {
          Handler.I18nFactory = new ResourceManagerI18NFactory();
        }
        return Handler.I18nFactory as ResourceManagerI18NFactory;
      }
    }

    /// <summary>
    /// This method configures the option handler
    /// </summary>
    protected abstract void SetupHandler();

    /// <summary>
    /// This event gets reaised at various points in the module execution path.
    /// </summary>
    public event ModuleEventHandler ModuleEvent;

    /// <summary>
    /// Verify the current execution context.
    /// </summary>
    /// <param name="context"></param>
    internal virtual void CheckContext(ILookup context) {
      Type[] neededTypes = GetRequiredTypes();

      foreach (Type type in neededTypes) {
        if (!type.IsInstanceOfType(context.Lookup(type))) {
          throw new ArgumentException("Expected type " + type + " in context!");
        }
      }
    }

    /// <summary>
    /// Displays the options dialog for this module.
    /// </summary>
    /// <returns>whether or not the settings have been accepted</returns>
    public virtual void ShowModule(Window owner) {
      if (Modal) {
        ShowModal(owner);
      }
      else {
        ShowNonModal(owner);
      }
    }

    private void ShowModal(Window owner) {
      if (Handler != null) {
        var form = new EditorForm
                     {
                       OptionHandler = Handler,
                       IsAutoAdopt = true,
                       IsAutoCommit = true,
                       ShowResetButton = true,
                       Owner = owner,
                     };
        try {
          form.Title = ResourceManager.GetString(moduleName);
        } catch (MissingManifestResourceException) { }
        form.Closed += FormOnClosed;
        form.ShowDialog();
      }
    }

    private void FormOnClosed(object sender, EventArgs eventArgs) {
      var form = (EditorForm)sender;
      form.Closed -= FormOnClosed;
      if (form.DialogResult == true) {
        OnValuesCommitted(this, EventArgs.Empty);
      }
    }

    private bool ShowNonModal(Window owner) {
      if (Handler != null) {
        var form = new EditorForm
        {
          OptionHandler = Handler,
          IsAutoAdopt = true,
          IsAutoCommit = true,
          ShowResetButton = true,
          Owner = owner,
        };
        try {
          form.Title = ResourceManager.GetString(moduleName);
        } catch (MissingManifestResourceException) {}
        form.Closed += FormOnClosed;
        form.ShowApplyButton = true;
        form.ValuesAdopted += OnValuesAdopted;
        form.ValuesCommitted += OnValuesCommitted;
        form.Closed += OnEditorDisposed;
        form.Show();
        return true;
      }
      return false;
    }

    protected virtual void OnEditorDisposed(object sender, EventArgs e) {
      if (EditorDisposed != null) {
        EditorDisposed(this, e);
      }
    }

    protected virtual void OnValuesCommitted(object sender, EventArgs e) {
      if(ValuesCommitted != null) {
        ValuesCommitted(this, e);
      }
    }

     protected virtual void OnValuesAdopted(object sender, EventArgs e) {
      if(ValuesAdopted != null) {
        ValuesAdopted(this, e);
      }
    }

    public event EventHandler ValuesCommitted;
    public event EventHandler ValuesAdopted;
    public event EventHandler EditorDisposed;


    /// <summary>
    /// Calls ShowModule(null);
    /// </summary>
    /// <returns>whether or not the settings have been accepted</returns>
    public virtual void ShowModule() {
      ShowModule(null);
    }

    /// <summary>
    /// Get an array of all types that the <see cref="Context"/> must provide.
    /// </summary>
    /// <returns>an array of all types that the <see cref="Context"/> must provide.</returns>
    protected abstract Type[] GetRequiredTypes();
  }

  /// <summary>
  /// Event handler for <see cref="YModule.ModuleEvent"/>s
  /// </summary>
  /// <param name="source"></param>
  /// <param name="args"></param>
  public delegate void ModuleEventHandler(object source, ModuleEventArgs args);

  /// <summary>
  /// Represents an event in the execution path of a module.
  /// </summary>
  public class ModuleEventArgs : EventArgs
  {
    /// <summary>
    /// The various event types for the module execution points
    /// </summary>
    public enum EventType
    {
      /// <summary>
      /// Fired just before <see cref="YModule.ConfigureModule"/> is called.
      /// </summary>
      BEFORE_CONFIGURATION,
      /// <summary>
      /// Fired directly after <see cref="YModule.ConfigureModule"/> is called.
      /// </summary>
      AFTER_CONFIGURATION,
      /// <summary>
      /// Fired just before <see cref="YModule.RunModule"/> is called.
      /// </summary>
      BEFORE_RUN,
      /// <summary>
      /// Fired directly after <see cref="YModule.RunModule"/> has returned.
      /// </summary>
      AFTER_RUN,
      /// <summary>
      /// Fired just before <see cref="YModule.Dispose"/> is called.
      /// </summary>
      BEFORE_DISPOSE,
      /// <summary>
      /// Fired directly after <see cref="YModule.Dispose"/> has returned.
      /// </summary>
      AFTER_DISPOSE
    } ;

    /// <summary>
    /// Get the event type
    /// </summary>
    public EventType Type {
      get { return type; }
    }

    private EventType type;

    /// <summary>
    /// Create a new instance
    /// </summary>
    /// <param name="type"></param>
    public ModuleEventArgs(EventType type) {
      this.type = type;
    }
  }
}
