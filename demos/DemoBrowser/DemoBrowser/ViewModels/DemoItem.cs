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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using yWorks.Demo;

namespace DemoBrowser.ViewModels
{
  internal abstract class DemoLabel
  {
    protected DemoLabel(string value) {
      Value = value;
    }

    public string Value { get; }
  }

  internal sealed class DemoCategory : DemoLabel
  {
    public DemoCategory(string value) :
        base(CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(value).Replace("-", " ")) { }
  }

  internal sealed class DemoTag : DemoLabel
  {
    public DemoTag(string value) : base(value) { }
  }

  internal sealed class DemoItem
  {
    private readonly Type viewType;
    private readonly Lazy<BitmapImage?> screenshot;
    private readonly string searchableText;

    private DemoItem(Type viewType, DemoAttribute attribute) {
      this.viewType = viewType;
      Id = attribute.Id;
      Title = attribute.Title;
      Summary = attribute.Summary!;
      Category = new DemoCategory(attribute.Category);
      Keywords = attribute.Keywords;
      Tags = [.. attribute.Tags.Select(tag => new DemoTag(tag))];
      Labels = [Category, .. Tags];
      RelatedDemos = attribute.RelatedDemos ?? [];
      DemoPath = attribute.DemoPath;
      searchableText = string.Join(" ",
          new[] { Title, Category.Value }.Concat(Tags.Select(tag => tag.Value)).Concat(Keywords));
      screenshot = new Lazy<BitmapImage?>(() => LoadScreenshot(Id));
    }

    public string Id { get; }

    public string Title { get; }

    public string Summary { get; }

    public DemoCategory Category { get; }

    public IReadOnlyList<string> Keywords { get; }

    public IReadOnlyList<DemoTag> Tags { get; }

    public IReadOnlyList<DemoLabel> Labels { get; }

    public IReadOnlyList<string> RelatedDemos { get; }

    public string? DemoPath { get; }

    public BitmapImage? Screenshot => screenshot.Value;

    public static DemoItem Create(Type viewType) {
      if (!typeof(UIElement).IsAssignableFrom(viewType)) {
        throw new InvalidOperationException($"Registered demo type '{viewType.FullName}' is not a UIElement.");
      }

      if (viewType.GetConstructor(Type.EmptyTypes) is null) {
        throw new InvalidOperationException(
            $"Registered demo type '{viewType.FullName}' has no public parameterless constructor.");
      }

      var attribute = viewType.GetCustomAttribute<DemoAttribute>() ??
          throw new InvalidOperationException($"Registered demo type '{viewType.FullName}' has no DemoAttribute.");
      if (string.IsNullOrWhiteSpace(attribute.Id) || string.IsNullOrWhiteSpace(attribute.Title) ||
          string.IsNullOrWhiteSpace(attribute.Category) || string.IsNullOrWhiteSpace(attribute.Summary)) {
        throw new InvalidOperationException($"Registered demo type '{viewType.FullName}' has incomplete metadata.");
      }

      return new DemoItem(viewType, attribute);
    }

    public bool Matches(string[] words) {
      foreach (var word in words) {
        if (!searchableText.Contains(word, StringComparison.OrdinalIgnoreCase)) {
          return false;
        }
      }
      return true;
    }

    public UIElement CreateView() => (UIElement) Activator.CreateInstance(viewType)!;

    private static BitmapImage? LoadScreenshot(string id) {
      var uri = new Uri($"/DemoBrowser;component/Screenshots/{id}.webp", UriKind.Relative);
      try {
        var resource = Application.GetResourceStream(uri);
        if (resource is null) {
          return null;
        }

        using (resource.Stream) {
          var image = new BitmapImage();
          image.BeginInit();
          image.DecodePixelWidth = 350;
          image.CacheOption = BitmapCacheOption.OnLoad;
          image.StreamSource = resource.Stream;
          image.EndInit();
          image.Freeze();
          return image;
        }
      } catch (Exception exception) when (exception is IOException or InvalidOperationException) {
        return null;
      }
    }
  }
}
