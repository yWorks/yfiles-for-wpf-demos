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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace yWorks.Demo.HeightProfile
{
  /// <summary>
  ///   Interaction logic for HeightProfileDemo.xaml
  /// </summary>
  [Demo("height-profile", "layout", "Height Profile",
      Summary = "Shows how to configure organic layout to create height profile visualization",
      Tags = ["layout", "organic", "constraints"],
      Keywords = ["layoutfeatures", "height", "labeling", "timeline", "time", "time-data", "margin"])]
  public sealed partial class HeightProfileDemo
  {
    private TrekkingData trekkingData = null!;

    public HeightProfileDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps this demo.
    /// </summary>
    protected override async void Initialize() {
      using (var stream = LoadAssemblyResource("TrekkingData.json")) {
        trekkingData =
            JsonSerializer.Deserialize<TrekkingData>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("The trekking data resource is empty.");
      }

      // configure user interaction, disable selection and focus
      graphControl.InputMode = new GraphViewerInputMode {
          SelectableItems = GraphItemTypes.None,
          FocusableItems = GraphItemTypes.None
      };

      // assign the default style and size for the waypoints
      InitializeStyles.Initialize(graphControl.Graph);

      // configure highlights for hovered nodes
      ConfigureHighlight.Initialize(graphControl);

      // scale the data and draw the trekking trail and the axis
      var scaledTrail = ScaleData.ScaleDataPoints(trekkingData.Trail);
      DrawTrekkingTrail.Draw(graphControl, scaledTrail);
      DrawAxis.Draw(graphControl, trekkingData.Trail);

      // read the waypoints from the dataset and create the associated label nodes
      var icons = LoadIcons();
      DrawIconDescription.SetIcons(icons);
      CreateGraph.Initialize(graphControl.Graph, trekkingData, icons);

      // configure and run an OrganicLayout with constraints for aligning waypoints and their associated labels
      await RunLayoutAsync();
    }

    /// <summary>
    ///   Runs the organic layout with the necessary constraints to obtain a height profile visualization.
    /// </summary>
    private async Task RunLayoutAsync() {
      var (layout, layoutData) = ConfigureLayout.Create(graphControl.Graph);
      graphControl.Graph.ApplyLayout(layout, layoutData);
      await graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Loads the icons embedded in the demo assembly.
    /// </summary>
    private IReadOnlyDictionary<string, ImageSource> LoadIcons() {
      var icons = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
      foreach (var waypoint in trekkingData.Waypoints) {
        LoadIcon(waypoint.Icon);
        LoadIcon(InitializeStyles.GetIcon(waypoint));
      }
      return icons;

      void LoadIcon(string? iconPath) {
        if (iconPath is null || icons.ContainsKey(iconPath)) {
          return;
        }

        var resourcePath = iconPath.Replace(".svg", ".png", StringComparison.OrdinalIgnoreCase);
        var resourceSuffix = resourcePath[(resourcePath.LastIndexOf('/') + 1)..];
        using var stream = LoadAssemblyResource(resourceSuffix);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        icons.Add(iconPath, image);
      }
    }

    [STAThread]
    private static void Main() => Run<HeightProfileDemo>();
  }
}
