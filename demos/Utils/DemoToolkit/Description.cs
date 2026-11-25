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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace yWorks.Demo
{
  public class Description : SideBar
  {
    private static readonly string ApiPrefix = "https://docs.yworks.com/yfileswpf/#/api/";
    private static readonly string DguidePrefix = "https://docs.yworks.com/yfileswpf/#/dguide/";

    static Description() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(Description), new FrameworkPropertyMetadata(typeof(Description)));
    }

    // ── JavaScript shim injected into the description WebView2 ───────────────
    // Posts the demo ID as a prefixed string so tutorial button messages remain distinct.
    private const string DemoMessagePrefix = "\u001fopen-demo:";

    private const string OpenDemoShimScript = """
                                              (function() {
                                                document.addEventListener('click', function(e) {
                                                  var demoAnchor = e.target.closest('a[data-demo]');
                                                  if (demoAnchor) {
                                                    e.preventDefault();
                                                    var id = demoAnchor.getAttribute('data-demo');
                                                    if (id && window.chrome && window.chrome.webview) {
                                                      window.chrome.webview.postMessage('\u001fopen-demo:' + id);
                                                    }
                                                  }
                                                });
                                              })();
                                              """;

    private readonly string openOnlineDocumentationShimScript = $$"""
                                                                  document.addEventListener('DOMContentLoaded', function() {
                                                                    document.querySelectorAll('a[data-api]').forEach(function(anchor) {
                                                                      var api = anchor.getAttribute('data-api');
                                                                      if (api) {
                                                                        // fix member links with a dot instead of a hash as separator
                                                                        var parts = api.split('.');
                                                                        var path = parts.length === 2
                                                                          ? parts.map(encodeURIComponent).join('#')
                                                                          : encodeURIComponent(api);
                                                                        anchor.href = {{JsonSerializer.Serialize(ApiPrefix)}} + path;
                                                                      }
                                                                    });
                                                                    document.querySelectorAll('a[data-dguide]').forEach(function(anchor) {
                                                                      var dguide = anchor.getAttribute('data-dguide');
                                                                      if (dguide) {
                                                                        anchor.href = {{JsonSerializer.Serialize(DguidePrefix)}} + encodeURI(dguide);
                                                                      }
                                                                    });
                                                                    document.querySelectorAll('a[data-demo]:not([href])').forEach(function(anchor) {
                                                                      anchor.href = '#';
                                                                    });
                                                                  });
                                                                  """;

    private readonly string userDataFolder = Path.Combine(Path.GetTempPath(), "yFiles WPF Demos");
    private readonly List<string> documentCreatedScripts = [];
    private WebView2? webView;
    private bool webViewInitialized;

    private Assembly? resourceLookupAssembly;
    private ICommand? openDemoCommand;

    /// <summary>
    ///   Raised when the active WebView receives a web message.
    /// </summary>
    internal event EventHandler<CoreWebView2WebMessageReceivedEventArgs>? WebMessageReceived;

    public Description() {
      PresentationSource.AddSourceChangedHandler(this, OnPresentationSourceChanged);
    }

    /// <summary>
    ///   Registers a script that should run when a document is created in each WebView instance.
    /// </summary>
    internal void RegisterDocumentCreatedScript(string script) => documentCreatedScripts.Add(script);

    private async void InitializeWebView(WebView2 view) {
      try {
        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        if (!ReferenceEquals(webView, view)) {
          return;
        }

        await view.EnsureCoreWebView2Async(environment);
        if (!ReferenceEquals(webView, view)) {
          return;
        }

        var core = view.CoreWebView2;
        core.Settings.AreDefaultContextMenusEnabled = false;
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = false;
        core.Settings.IsPinchZoomEnabled = false;
        core.Settings.IsBuiltInErrorPageEnabled = false;

        await core.AddScriptToExecuteOnDocumentCreatedAsync(OpenDemoShimScript);
        if (!ReferenceEquals(webView, view)) {
          return;
        }
        await core.AddScriptToExecuteOnDocumentCreatedAsync(openOnlineDocumentationShimScript);
        if (!ReferenceEquals(webView, view)) {
          return;
        }
        foreach (var script in documentCreatedScripts) {
          await core.AddScriptToExecuteOnDocumentCreatedAsync(script);
          if (!ReferenceEquals(webView, view)) {
            return;
          }
        }

        core.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Light;
        core.WebMessageReceived += OnWebViewWebMessageReceived;
        core.NewWindowRequested += OnNewWindowRequested;
        webViewInitialized = true;
        OnSourceChanged(this, new());
      } catch (Exception) when (!ReferenceEquals(webView, view)) {
        // A detach can dispose the control while WebView2 initialization is in flight.
      }
    }

    private void OnWebViewWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e) {
      var message = e.TryGetWebMessageAsString();
      if (message.StartsWith(DemoMessagePrefix, StringComparison.Ordinal)) {
        var demoId = message.Substring(DemoMessagePrefix.Length);
        if (openDemoCommand?.CanExecute(demoId) == true) {
          openDemoCommand.Execute(demoId);
        }
      }
      WebMessageReceived?.Invoke(sender, e);
    }

    private static void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs args) {
      if (args.IsUserInitiated && IsWebAddress(args.Uri)) {
        args.Handled = true;
        OpenInBrowser(args.Uri);
      }
    }

    private void OnWebViewNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e) {
      InjectSystemColors();
    }

    private void OnPresentationSourceChanged(object sender, SourceChangedEventArgs e) {
      // Clean up WndProc hook
      (e.OldSource as HwndSource)?.RemoveHook(WndProc);
      DisposeWebView();

      if (e.NewSource != null) {
        if (e.NewSource is HwndSource source) {
          source.AddHook(WndProc);
        }

        var view = new WebView2();
        webView = view;
        view.NavigationStarting += OnWebViewNavigationStarting;
        view.NavigationCompleted += OnWebViewNavigationCompleted;
        Content = view;
        InitializeWebView(view);
      }
    }

    private void DisposeWebView() {
      if (webView is not { } view) {
        return;
      }

      view.NavigationStarting -= OnWebViewNavigationStarting;
      view.NavigationCompleted -= OnWebViewNavigationCompleted;
      if (view.CoreWebView2 is { } core) {
        core.WebMessageReceived -= OnWebViewWebMessageReceived;
        core.NewWindowRequested -= OnNewWindowRequested;
      }

      webView = null;
      webViewInitialized = false;
      resourceLookupAssembly = null;
      openDemoCommand = null;
      // https://github.com/dotnet/wpf/issues/12045
      Content = null;
      view.Dispose();
    }

    private static void OnWebViewNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e) {
      if (e.IsUserInitiated && IsWebAddress(e.Uri)) {
        e.Cancel = true;
        OpenInBrowser(e.Uri);
      }
    }

    /// <summary>
    ///   Window message hook that updates the CSS color variables according to the system colors.
    ///   This is needed for light/dark mode as well as changing accent colors.
    /// </summary>
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      const int WM_SETTINGCHANGE = 0x001A;
      if (msg == WM_SETTINGCHANGE && wParam == IntPtr.Zero && Marshal.PtrToStringUni(lParam) == "ImmersiveColorSet") {
        // Theme colors have changed (dark/light mode or accent color)
        InjectSystemColors();
      }

      return IntPtr.Zero;
    }

    private static bool IsWebAddress(string address) =>
        Uri.TryCreate(address, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static void OpenInBrowser(string address) {
      try {
        Process.Start(new ProcessStartInfo(address) { UseShellExecute = true });
      } catch (Win32Exception) {
        MessageBox.Show("Unable to open the link in your default browser.", "Open link", MessageBoxButton.OK,
            MessageBoxImage.Error);
      }
    }

    public string Source {
      get => (string) GetValue(SourceProperty);
      set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source),
        typeof(string), typeof(Description), new PropertyMetadata(null, OnSourceChanged));


    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var description = (Description) d;
      if (!description.webViewInitialized || description.webView is not { } webView) {
        return;
      }

      DependencyObject? parent = null;

      if (description.resourceLookupAssembly == null) {
        do {
          parent = VisualTreeHelper.GetParent(parent ?? description);
        } while (parent is not (null or Demo));
        if (parent is Demo demo) {
          description.resourceLookupAssembly = demo.GetType().Assembly;
          description.openDemoCommand = demo.OpenDemoCommand;
        }
      }

      if (description.Source != null && description.resourceLookupAssembly != null) {
        webView.NavigateToString(LoadDescription(description.resourceLookupAssembly, description.Source));
      } else {
        webView.Source = new("about:blank");
      }
    }

    /// <summary>
    ///   Injects the current theme colors into the WebView as CSS variables, so the description can update according to theme
    ///   changes.
    /// </summary>
    private void InjectSystemColors() {
      if (!webViewInitialized || webView is not { } view) {
        return;
      }

      Color FromResourceKey(string key) =>
          TryFindResource(key) is SolidColorBrush { Color: var color } ? color : Colors.Black;

      var colors = new (string Name, Color Color)[] {
          ("accent-text-primary", FromResourceKey("AccentTextFillColorPrimaryBrush")),
          ("accent-text-secondary", FromResourceKey("AccentTextFillColorSecondaryBrush")),
          ("accent-text-tertiary", FromResourceKey("AccentTextFillColorTertiaryBrush")),
          ("text-primary", FromResourceKey("TextFillColorPrimaryBrush"))
      };

      static string ToCss(Color c) => $"rgb({c.R}, {c.G}, {c.B})";

      var lines = colors.Select(c => $"document.documentElement.style.setProperty('--{c.Name}', '{ToCss(c.Color)}');");
      var script = string.Join('\n', lines);

      view.ExecuteScriptAsync(script);
    }

    /// <summary>
    ///   Loads the given HTML file from the application's resources, prepends the common CSS
    /// </summary>
    private static string LoadResourceAsString(Assembly assembly, string resourceSuffix) {
      var name = assembly.GetManifestResourceNames()
          .FirstOrDefault(n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));
      if (name == null) {
        return "";
      }

      using var stream = assembly.GetManifestResourceStream(name);
      using var reader = new StreamReader(stream, Encoding.UTF8);
      return reader.ReadToEnd();
    }

    private static readonly string Css = LoadResourceAsString(typeof(Description).Assembly, "description.css");

    private static string LoadDescription(Assembly assembly, string resource) {
      var html = LoadResourceAsString(assembly, resource);
      if (html.Contains("</head>", StringComparison.OrdinalIgnoreCase)) {
        return html.Replace("</head>", $"<style>{Css}</style></head>", StringComparison.OrdinalIgnoreCase);
      }
      if (html.Contains("</body>", StringComparison.OrdinalIgnoreCase)) {
        return html.Replace("</body>", $"<style>{Css}</style></body>", StringComparison.OrdinalIgnoreCase);
      }
      return html;
    }
  }
}
