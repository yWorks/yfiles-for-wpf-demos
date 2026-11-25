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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using yWorks.Graph;
using yWorks.GraphML;
using Path = System.Windows.Shapes.Path;

namespace yWorks.Demo
{
  public abstract class Demo : ContentControl
  {
    static Demo() {
      // Fix the wrong culture being used for displaying numbers in the UI.
      LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
          new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
    }

    protected Demo() {
      Loaded += InvokeInitialize;

      void InvokeInitialize(object sender, EventArgs e) {
        Initialize();
        Loaded -= InvokeInitialize;
      }
    }

    /// <summary>
    ///   Command that opens a demo.
    /// </summary>
    /// <remarks>
    ///   This command is used to open a demo linked in a <see cref="Description"/> in the demo browser.
    /// </remarks>
    public ICommand OpenDemoCommand { get; set; }

    /// <summary>
    ///   Called just before showing the demo.
    /// </summary>
    /// <remarks>
    ///   Demo initialization code goes here.
    /// </remarks>
    protected virtual void Initialize() { }

    /// <summary>
    ///   Finds the resource with the given name suffix (file name in the IDE) in the current type's assembly and opens it.
    /// </summary>
    protected internal Stream LoadAssemblyResource(string nameSuffix) => LoadAssemblyResource(GetType(), nameSuffix);

    /// <summary>
    ///   Finds the resource with the given name suffix (file name in the IDE) in the given type's assembly and opens it.
    /// </summary>
    public static Stream LoadAssemblyResource(Type type, string nameSuffix) {
      var assembly = type.Assembly;
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
    ///   Constructs and populates a graph using the provided JSON resource name.
    /// </summary>
    /// <param name="resourceSuffix">
    ///   The name (suffix) of the resource, e.g. <c>graph-data.json</c>, to build the graph
    ///   structure.
    /// </param>
    /// <param name="graph">The graph instance to be populated.</param>
    protected void BuildGraphFromJson(
        string resourceSuffix,
        IGraph graph,
        Type? nodeTagType = null,
        Type? groupNodeTagType = null,
        Type? edgeTagType = null,
        Type? nodeLabelTagType = null,
        Type? groupNodeLabelTagType = null,
        Type? edgeLabelTagType = null) {
      using var stream = LoadAssemblyResource(resourceSuffix);
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json, nodeTagType, groupNodeTagType, edgeTagType, nodeLabelTagType,
          groupNodeLabelTagType, edgeLabelTagType);
    }

    /// <summary>
    ///   Loads a graph from a GraphML resource.
    /// </summary>
    /// <param name="resourceSuffix">The name (suffix) of the resource, e.g. <c>sample-graph.graphml</c>, to load.</param>
    /// <param name="graph">The graph instance to be populated.</param>
    protected void BuildGraphFromGraphML(string resourceSuffix, IGraph graph, bool ignoreStyles = false) {
      using var stream = LoadAssemblyResource(resourceSuffix);
      var graphMLIOHandler = ignoreStyles
          ? new FaultTolerantGraphMLIOHandler { ClearGraphBeforeRead = true }
          : new GraphMLIOHandler { ClearGraphBeforeRead = true };
      graphMLIOHandler.Read(graph, stream);
    }

    /// <summary>
    ///   Launches a <see cref="Demo"/> as a standalone application with a window containing just the main view.
    /// </summary>
    /// <example>
    ///   This will typically be used as the implementation for the assembly entry point:
    ///   <code>
    /// static void Main() => Run&lt;ThisDemo&gt;();
    /// </code>
    /// </example>
    /// <typeparam name="T">The demo's view type.</typeparam>
    protected static void Run<T>() where T : FrameworkElement, new() {
      var attribute = typeof(T).GetCustomAttribute<DemoAttribute>();

      if (Application.Current != null) {
        throw new InvalidOperationException($"{nameof(Run)} must be called before another WPF Application is created.");
      }

      var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
      app.Resources.MergedDictionaries.Add(new ResourceDictionary {
          Source = new Uri("pack://application:,,,/DemoToolkit;component/Resources/yFilesDemoStyles.xaml",
              UriKind.Absolute)
      });
      app.Resources.MergedDictionaries.Add(new ResourceDictionary {
          Source = new Uri("pack://application:,,,/DemoToolkit;component/Icons.xaml", UriKind.Absolute)
      });
#if NET9_0_OR_GREATER
      // Enable Fluent theme
      app.Resources.MergedDictionaries.Add(new ResourceDictionary {
          Source = new Uri("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.Light.xaml",
              UriKind.Absolute)
      });
#endif
      UnhandledExceptionReporter.Install();
      var view = new T();

      var logo = new Path {
          Data = (System.Windows.Media.Geometry) app.FindResource("yFilesWpf"),
          RenderTransform = new TranslateTransform(0, 3),
          Height = 40,
          Stretch = Stretch.Uniform
      };
      logo.SetResourceReference(Shape.FillProperty, "TextFillColorPrimaryBrush");

      var breadcrumbs = new Breadcrumbs {
          Items = {
              new BreadcrumbItem {
                  Content = logo,
                  IsEnabled = false
              },
              new BreadcrumbItem {
                  Content = "Demos",
                  IsEnabled = false
              }
          },
          Margin = new Thickness(24, 16, 24, 16),
          FontSize = 24,
          FontWeight = FontWeights.SemiBold
      };
      if (attribute?.Title is { } title) {
        breadcrumbs.Items.Add(new BreadcrumbItem {
            Content = title,
            IsEnabled = false
        });
      }
      DockPanel.SetDock(breadcrumbs, Dock.Top);
      var panel = new DockPanel {
          Children = {
              breadcrumbs,
              view
          }
      };

      var window = new Window {
          Title = attribute?.Title ?? "yFiles WPF demo",
          Icon = BitmapFrame.Create(new Uri("pack://application:,,,/DemoToolkit;component/Resources/yFiles.ico")),
          Content = panel,
          Width = 1200,
          Height = 800,
          MinWidth = 640,
          MinHeight = 480
      };
      window.Closed += TryDispose(view);
      app.MainWindow = window;
      app.Startup += (_, _) => window.Show();
      app.Run();

      static EventHandler TryDispose(T view) {
        return (_, _) => (view as IDisposable)?.Dispose();
      }
    }
  }
}
