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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using yWorks.Demo;

namespace DemoBrowser.ViewModels
{
  internal sealed class BrowserViewModel : INotifyPropertyChanged, IDisposable
  {
    private static readonly DemoCategory AllCategories = new("All categories");
    private string searchText = string.Empty;
    private DemoCategory selectedCategory = AllCategories;
    private DemoItem? selectedDemo;
    private UIElement? demoView;
    private readonly List<DemoItem> allDemos;

    public BrowserViewModel() {
      allDemos = [.. DemoRegistry.DemoTypes.Select(DemoItem.Create)];
      EnsureUniqueIds(allDemos);

      Demos = new ListCollectionView(allDemos) { Filter = MatchesFilter };
      Categories = new ReadOnlyCollection<DemoCategory>([
          .. new[] { AllCategories }.Concat(allDemos.Select(demo => demo.Category))
              .DistinctBy(category => category.Value)
      ]); // TODO: Replace with .Add calls

      OpenDemoCommand = new RelayCommand<DemoItem>(OpenDemo);
      OpenDemoIdCommand = new RelayCommand<string>(demoId => OpenDemo(GetDemoItem(demoId)));
      ShowOverviewCommand = new RelayCommand(ShowOverview, () => !IsOverview);
    }

    public ICollectionView Demos { get; }

    public IReadOnlyList<DemoCategory> Categories { get; }

    public ICommand OpenDemoCommand { get; }

    public ICommand OpenDemoIdCommand { get; }

    public ICommand ShowOverviewCommand { get; }

    public event Action<DemoItem>? FocusDemoRequested;

    public string SearchText {
      get => searchText;
      set {
        if (SetProperty(ref searchText, value)) {
          Demos.Refresh();
        }
      }
    }

    public DemoCategory SelectedCategory {
      get => selectedCategory;
      set {
        if (SetProperty(ref selectedCategory, value)) {
          Demos.Refresh();
        }
      }
    }

    public DemoItem? SelectedDemo {
      get => selectedDemo;
      private set => SetProperty(ref selectedDemo, value);
    }

    public UIElement? DemoView {
      get => demoView;
      private set => SetProperty(ref demoView, value);
    }

    public bool IsOverview => DemoView is null;

    private DemoItem? GetDemoItem(string? demoId) {
      return demoId != null ? allDemos.FirstOrDefault(d => d.Id == demoId) : null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose() {
      RemoveCurrentDemo();
    }

    private bool MatchesFilter(object item) {
      var demo = (DemoItem) item;
      var words = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
      return (SelectedCategory == AllCategories || demo.Category.Value == SelectedCategory.Value) &&
          demo.Matches(words);
    }

    private void OpenDemo(DemoItem? demo) {
      if (demo is null) {
        return;
      }

      RemoveCurrentDemo();
      SelectedDemo = demo;
      DemoView = demo.CreateView();
      if (DemoView is Demo demoView) {
        demoView.OpenDemoCommand = OpenDemoIdCommand;
      }
      OnPropertyChanged(nameof(IsOverview));
      CommandManager.InvalidateRequerySuggested();
    }

    private void ShowOverview() {
      var demoToFocus = SelectedDemo;
      RemoveCurrentDemo();
      SelectedDemo = null;
      OnPropertyChanged(nameof(IsOverview));
      CommandManager.InvalidateRequerySuggested();
      if (demoToFocus is not null) {
        FocusDemoRequested?.Invoke(demoToFocus);
      }
    }

    private void RemoveCurrentDemo() {
      var oldView = DemoView;
      DemoView = null;
      (oldView as IDisposable)?.Dispose();
    }

    private static void EnsureUniqueIds(IEnumerable<DemoItem> demos) {
      var duplicate = demos.GroupBy(demo => demo.Id, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
      if (duplicate is not null) {
        throw new InvalidOperationException($"The demo ID '{duplicate.Key}' is registered more than once.");
      }
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return false;
      }

      field = value;
      OnPropertyChanged(propertyName);
      return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
