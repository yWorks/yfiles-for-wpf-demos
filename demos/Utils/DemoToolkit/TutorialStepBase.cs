/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using yWorks.Controls;
using yWorks.Graph;

namespace yWorks.Demo
{
  /// <summary>
  ///   Abstract base class for individual tutorial steps.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Subclasses must implement <see cref="Title"/> and override <see cref="OnStart"/> to set up
  ///     the graph, styles, and input mode for the step.
  ///   </para>
  ///   <para>
  ///     The default <see cref="DescriptionSource"/> returns <c>"description.html"</c>.  Steps whose
  ///     description resource has a different name (or a namespace-qualified name) must override this
  ///     property.
  ///   </para>
  ///   <para>
  ///     Steps that need buttons inside the HTML description (like undo/redo triggers) should call
  ///     <see cref="AddButtonListener"/> in <see cref="OnStart"/>.  All listeners registered this
  ///     way are automatically removed in <see cref="CleanUp"/>.
  ///   </para>
  /// </remarks>
  public abstract class TutorialStepBase : ITutorialStep
  {
    // ── button-listener infrastructure ───────────────────────────────────────

    private readonly Dictionary<string, Action> _buttonListeners = new();
    private EventHandler<CoreWebView2WebMessageReceivedEventArgs>? _webMessageHandler;
    private Description? _listeningDescription;

    // ── ITutorialStep ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public abstract string Title { get; }

    /// <inheritdoc/>
    /// <remarks>
    ///   The default implementation returns <c>"description.html"</c>.
    ///   Override to return a more specific resource name suffix when the assembly contains
    ///   multiple description files (e.g. <c>"Step01CreatingTheView.description.html"</c>).
    /// </remarks>
    public virtual string DescriptionSource => "description.html";

    /// <inheritdoc/>
    /// <remarks>
    ///   The default implementation creates a new <see cref="GraphControl"/>, places it in
    ///   <paramref name="container"/>, then calls <see cref="OnStart"/>.
    ///   Override this method when a step needs to demonstrate the <see cref="GraphControl"/>
    ///   creation and placement itself (e.g. step 01 "Creating the View").
    ///   When overriding, assign the created <see cref="GraphControl"/> to the
    ///   <see cref="GraphControl"/> property so that <see cref="CleanUp"/> can release it.
    /// </remarks>
    public virtual void Start(ContentControl container, Description description) {
      GraphControl = new GraphControl();
      container.Content = GraphControl;
      OnStart(GraphControl, description);
    }

    /// <inheritdoc/>
    /// <remarks>
    ///   Removes the <see cref="GraphControl"/> from <paramref name="container"/>,
    ///   unregisters all button listeners, and calls <see cref="OnCleanUp"/>.
    /// </remarks>
    public void CleanUp(ContentControl container, Description description) {
      OnCleanUp(GraphControl!, description);

      // Unsubscribe HTML-button bridge
      if (_webMessageHandler != null) {
        if (_listeningDescription != null) {
          _listeningDescription.WebMessageReceived -= _webMessageHandler;
        }
        _webMessageHandler = null;
        _listeningDescription = null;
      }
      _buttonListeners.Clear();

      // Clean up GraphControl
      if (GraphControl != null) {
        GraphControl.InputMode = null;
        container.Content = null;
        GraphControl = null;
      }
    }

    // ── overridable hooks ─────────────────────────────────────────────────────

    /// <summary>
    ///   Called after the <see cref="GraphControl"/> has been created and placed in the container.
    ///   Override this to configure the graph, styles, and input mode for the step.
    /// </summary>
    /// <param name="graphControl">The freshly created <see cref="GraphControl"/> for this step.</param>
    /// <param name="description">
    ///   The <see cref="Description"/> side-bar.  Pass it to <see cref="AddButtonListener"/>
    ///   to register HTML-button callbacks.
    /// </param>
    protected virtual void OnStart(GraphControl graphControl, Description description) { }

    /// <summary>
    ///   Called just before the <see cref="GraphControl"/> is removed from the container.
    ///   Override to perform additional cleanup beyond what <see cref="CleanUp"/> handles automatically.
    /// </summary>
    /// <param name="graphControl">This step's <see cref="GraphControl"/> (still attached).</param>
    /// <param name="description">The description side-bar.</param>
    protected virtual void OnCleanUp(GraphControl graphControl, Description description) { }

    // ── protected helpers ─────────────────────────────────────────────────────

    /// <summary>
    ///   Gets the <see cref="GraphControl"/> for this step, or <see langword="null"/> when the step
    ///   is not active.
    /// </summary>
    protected GraphControl? GraphControl { get; private set; }

    /// <summary>
    ///   Sets the <see cref="GraphControl"/> for this step.
    ///   Call this when overriding <see cref="Start"/> to ensure <see cref="CleanUp"/> can release
    ///   the control correctly.
    /// </summary>
    protected void SetGraphControl(GraphControl graphControl) => GraphControl = graphControl;

    /// <summary>
    ///   Registers an action to be invoked when an HTML button with the given <paramref name="id"/>
    ///   is clicked inside the description pane.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     In the HTML description files, buttons are coded as anchor elements with a
    ///     <c>data-type="button"</c> attribute and an <c>id</c> attribute identifying the button,
    ///     e.g.:
    ///     <code>
    ///       &lt;a href="#" data-type="button" id="undoButton"&gt;Undo&lt;/a&gt;
    ///     </code>
    ///     A JavaScript shim injected by <see cref="TutorialBase"/> converts these clicks into
    ///     WebView2 web messages, which are dispatched here by button id.
    ///   </para>
    ///   <para>
    ///     All listeners registered via this method are automatically unsubscribed when
    ///     <see cref="CleanUp"/> is called.
    ///   </para>
    /// </remarks>
    /// <param name="description">The active <see cref="Description"/> side-bar.</param>
    /// <param name="id">The <c>id</c> attribute of the HTML button element.</param>
    /// <param name="action">The action to invoke when the button is clicked.</param>
    protected void AddButtonListener(Description description, string id, Action action) {
      _buttonListeners[id] = action;

      // Lazily subscribe a single handler that dispatches by id
      if (_webMessageHandler == null) {
        _webMessageHandler = (_, args) => {
          var buttonId = args.TryGetWebMessageAsString();
          if (_buttonListeners.TryGetValue(buttonId, out var handler)) {
            handler();
          }
        };
        _listeningDescription = description;
        description.WebMessageReceived += _webMessageHandler;
      }
    }

    /// <summary>
    ///   Loads a resource embedded in the entry assembly by its name suffix and returns it as a
    ///   <see cref="Stream"/>.
    /// </summary>
    /// <param name="nameSuffix">
    ///   Suffix of the embedded resource name, e.g. <c>"graph-data.json"</c>.
    /// </param>
    protected Stream LoadResource(string nameSuffix) => LoadResource(nameSuffix, GetType().Assembly);

    public static Stream LoadResource(string nameSuffix, Assembly assembly) {
      var resourceName = assembly.GetManifestResourceNames()
          .FirstOrDefault(n => n.EndsWith(nameSuffix, StringComparison.OrdinalIgnoreCase));
      if (resourceName != null) {
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is not null) {
          return stream;
        }
      }
      throw new ArgumentException($"Resource ending in {nameSuffix} not found.");
    }

    /// <summary>
    ///   Loads an image resource embedded in the entry assembly by its name suffix and returns it as a
    ///   <see cref="BitmapImage"/>.
    /// </summary>
    /// <param name="nameSuffix">
    ///   Suffix of the embedded resource name, e.g. <c>"person.png"</c>.
    /// </param>
    protected BitmapImage LoadImageResource(string nameSuffix) => LoadImageResource(nameSuffix, GetType().Assembly);

    public static BitmapImage LoadImageResource(string nameSuffix, Assembly assembly) {
      using var stream = LoadResource(nameSuffix, assembly);
      var image = new BitmapImage();
      image.BeginInit();
      image.StreamSource = stream;
      image.CacheOption = BitmapCacheOption.OnLoad;
      image.EndInit();
      image.Freeze();
      return image;
    }

    /// <summary>
    ///   Constructs and populates a graph using the provided JSON data.
    /// </summary>
    protected void BuildGraphFromJson(IGraph graph, string resourceName) {
      using var stream = LoadResource(resourceName);
      using var graphData = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, graphData);
    }
  }
}
