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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace yWorks.Demo
{
  /// <summary>
  ///   Abstract base class for yFiles WPF tutorial applications.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Inherit from this class to create a multi-step tutorial.  Override
  ///     <see cref="AddTutorialSteps"/> to register all steps with
  ///     <see cref="AddTutorialStep"/>.
  ///   </para>
  ///   <para>
  ///     HTML description files may contain anchor elements coded as buttons:
  ///     <code>&lt;a href="#" data-type="button" id="undoButton"&gt;Undo&lt;/a&gt;</code>
  ///     A JavaScript shim is injected into the <see cref="Description"/> WebView2 on every
  ///     navigation so that clicking such an anchor posts its <c>id</c> as a WebView2 web
  ///     message, which <see cref="TutorialStepBase.AddButtonListener"/> dispatches to the
  ///     registered C# action.
  ///   </para>
  ///   <para>
  ///     Use the static <see cref="RunTutorial{T}"/> method as the application entry point.
  ///     It launches the tutorial in a maximized window.
  ///   </para>
  /// </remarks>
  public abstract class TutorialBase : Demo
  {
    private const string ButtonShimScript = """
                                            (function() {
                                              document.addEventListener('click', function(e) {
                                                var anchor = e.target.closest('a[data-type="button"]');
                                                if (anchor) {
                                                  e.preventDefault();
                                                  var id = anchor.getAttribute('id');
                                                  if (id && window.chrome && window.chrome.webview) {
                                                    window.chrome.webview.postMessage(id);
                                                  }
                                                }
                                              });
                                            })();
                                            """;

    private readonly List<ITutorialStep> _steps = [];
    private int _currentStep = -1;

    private readonly ContentControl _stepContainer = new();

    private readonly Description _description = new() {
        IsExpanded = true,
        IsCollapsible = false,
        Header = "Description",
        ExpandedWidth = double.NaN
    };

    private Button? _prevButton;
    private Button? _nextButton;
    private ComboBox? _stepComboBox;

    // ── construction ─────────────────────────────────────────────────────────

    protected TutorialBase() {
      var toolBar = BuildToolBar();

      var grid = new Grid {
          ColumnDefinitions = {
              new ColumnDefinition { Width = new(1, GridUnitType.Star) }, // description
              new ColumnDefinition { Width = GridLength.Auto },           // grid splitter
              new ColumnDefinition { Width = new(1, GridUnitType.Star) }  // main content
          },
          RowDefinitions = {
              new RowDefinition { Height = GridLength.Auto },                     // toolbar
              new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } // content
          }
      };

      // Toolbar spans all columns
      Grid.SetRow(toolBar, 0);
      Grid.SetColumnSpan(toolBar, 3);
      grid.Children.Add(toolBar);

      // Description occupies the left column, content row
      var factory = new FrameworkElementFactory(typeof(ContentPresenter));
      factory.SetBinding(ContentPresenter.ContentProperty,
          new Binding("Content") { RelativeSource = RelativeSource.TemplatedParent });
      var plainTemplate = new ControlTemplate(typeof(Description)) { VisualTree = factory };

      _description.Template = plainTemplate;
      Grid.SetColumn(_description, 0);
      Grid.SetRow(_description, 1);
      grid.Children.Add(_description);

      // Step container (GraphControl placeholder) occupies the right column, content row
      Grid.SetColumn(_stepContainer, 2);
      Grid.SetRow(_stepContainer, 1);
      grid.Children.Add(_stepContainer);

      var splitter = new GridSplitter {
          ResizeBehavior = GridResizeBehavior.PreviousAndNext,
          ResizeDirection = GridResizeDirection.Columns
      };
      Grid.SetRow(splitter, 1);
      Grid.SetColumn(splitter, 1);
      grid.Children.Add(splitter);

      var border = new ClippingBorder {
          CornerRadius = new(12),
          Margin = new(8),
          Child = grid,
          BorderThickness = new(1)
      };
      border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
      border.SetResourceReference(Border.BorderBrushProperty, "CardStrokeColorDefaultBrush");

      Content = border;

      // Register the button shim for each WebView created when the description
      // attaches to a presentation source.
      _description.RegisterDocumentCreatedScript(ButtonShimScript);
    }

    /// <summary>
    ///   Called when the tutorial control is loaded.  Registers all steps by calling
    ///   <see cref="AddTutorialSteps"/>, populates the combo-box, and navigates to the first step.
    /// </summary>
    protected override void Initialize() {
      base.Initialize();

      AddTutorialSteps();

      if (_steps.Count == 0) {
        return;
      }

      // Populate combo-box and select the first step.
      // SelectionChanged fires GoToStep(0) automatically when SelectedIndex is set.
      foreach (var step in _steps) {
        _stepComboBox!.Items.Add(step.Title);
      }
      _stepComboBox!.SelectedIndex = 0;
    }

    /// <summary>
    ///   Override to register tutorial steps by calling <see cref="AddTutorialStep"/>.
    /// </summary>
    protected virtual void AddTutorialSteps() { }

    /// <summary>
    ///   Adds a step to the end of the tutorial step list.
    /// </summary>
    protected void AddTutorialStep(ITutorialStep step) {
      _steps.Add(step);
    }

    /// <summary>
    ///   Navigates to the step at <paramref name="index"/> in the step list.
    /// </summary>
    public void GoToStep(int index) {
      if (index < 0 || index >= _steps.Count) {
        return;
      }

      // Clean up the current step first
      if (_currentStep >= 0 && _currentStep < _steps.Count) {
        _steps[_currentStep].CleanUp(_stepContainer, _description);
      }

      _currentStep = index;
      var step = _steps[_currentStep];

      // Update description before calling Start so that CoreWebView2 is navigated
      // and the shim is (re-)injected before the step attaches any button listeners.
      _description.Source = step.DescriptionSource;

      // Start the new step
      step.Start(_stepContainer, _description);

      // Sync navigation controls
      UpdateNavigationControls();
    }

    /// <summary>Navigates to the previous step if one exists.</summary>
    public void PreviousStep() {
      if (HasPreviousStep) {
        GoToStep(_currentStep - 1);
      }
    }

    /// <summary>Navigates to the next step if one exists.</summary>
    public void NextStep() {
      if (HasNextStep) {
        GoToStep(_currentStep + 1);
      }
    }

    /// <summary>Gets whether there is a step before the current one.</summary>
    public bool HasPreviousStep => _currentStep > 0;

    /// <summary>Gets whether there is a step after the current one.</summary>
    public bool HasNextStep => _currentStep < _steps.Count - 1;

    /// <summary>
    ///   Launches a <see cref="TutorialBase"/> subclass as a standalone maximized application.
    /// </summary>
    /// <typeparam name="T">The tutorial's view type.</typeparam>
    protected static void RunTutorial<T>() where T : TutorialBase, new() {
      var attribute = typeof(T).GetCustomAttributes(typeof(DemoAttribute), false) is [DemoAttribute attr, ..]
          ? attr
          : null;

      if (Application.Current != null) {
        throw new InvalidOperationException(
            $"{nameof(RunTutorial)} must be called before another WPF Application is created.");
      }

      var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
      app.Resources.MergedDictionaries.Add(new ResourceDictionary {
          Source = new Uri("pack://application:,,,/DemoToolkit;component/Resources/yFilesDemoStyles.xaml",
              UriKind.Absolute)
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
      var window = new Window {
          Title = attribute?.Title ?? "Tutorial - yFiles WPF",
          Icon = BitmapFrame.Create(new Uri("pack://application:,,,/DemoToolkit;component/Resources/yFiles.ico")),
          Content = view,
          WindowState = WindowState.Maximized,
          MinWidth = 800,
          MinHeight = 600
      };
      window.Closed += TryDispose(view);
      app.MainWindow = window;
      app.Startup += (_, _) => window.Show();
      app.Run();

      static EventHandler TryDispose(T view) {
        return (_, _) => (view as IDisposable)?.Dispose();
      }
    }

    private Grid BuildToolBar() {
      _prevButton = new Button {
          Content = "\xE72B",
          FontSize = 16,
          HorizontalAlignment = HorizontalAlignment.Left
      };
      _prevButton.SetResourceReference(StyleProperty, "SubtleButtonStyle");
      _prevButton.SetBinding(HeightProperty, new Binding("ActualWidth") { RelativeSource = RelativeSource.Self });
      _prevButton.Click += (_, _) => PreviousStep();
      Grid.SetColumn(_prevButton, 0);

      _stepComboBox = new ComboBox {
          MinWidth = 200,
          VerticalAlignment = VerticalAlignment.Center,
          HorizontalAlignment = HorizontalAlignment.Center
      };
      _stepComboBox.SetResourceReference(StyleProperty, "SubtleComboBoxStyle");
      _stepComboBox.SelectionChanged += (_, _) => {
        var idx = _stepComboBox.SelectedIndex;
        if (idx >= 0 && idx != _currentStep) {
          GoToStep(idx);
        }
      };
      Grid.SetColumn(_stepComboBox, 1);

      _nextButton = new Button {
          Content = "\xE72A",
          FontSize = 16,
          HorizontalAlignment = HorizontalAlignment.Right
      };
      _nextButton.SetResourceReference(StyleProperty, "SubtleButtonStyle");
      _nextButton.SetBinding(HeightProperty, new Binding("ActualWidth") { RelativeSource = RelativeSource.Self });
      _nextButton.Click += (_, _) => NextStep();
      Grid.SetColumn(_nextButton, 2);

      return new Grid {
          ColumnDefinitions = {
              new(),
              new(),
              new()
          },
          Children = {
              _prevButton,
              _stepComboBox,
              _nextButton
          }
      };
    }

    private void UpdateNavigationControls() {
      if (_prevButton != null) {
        _prevButton.IsEnabled = HasPreviousStep;
      }
      if (_nextButton != null) {
        _nextButton.IsEnabled = HasNextStep;
      }
      // Sync combo-box without triggering SelectionChanged→GoToStep
      if (_stepComboBox != null && _stepComboBox.SelectedIndex != _currentStep) {
        _stepComboBox.SelectedIndex = _currentStep;
      }
    }
  }
}
