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
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Labeling;

namespace yWorks.Demo.EdgeLabelPlacement
{
  /// <summary>
  ///   Settings bound to the edge label properties panel and the layout selector.
  /// </summary>
  public sealed class Settings : INotifyPropertyChanged
  {
    private bool suppressSettingChanged;
    private string labelText = "Label";
    private string distanceToEdgeText = "5.00";
    private LabelAlongEdgePlacements? placement = LabelAlongEdgePlacements.AtCenter;
    private LabelEdgeSides? side = LabelEdgeSides.OnEdge;
    private LabelSideReferences? sideReference = LabelSideReferences.RelativeToEdgeFlow;
    private string angleText = "0.00";
    private LabelAngleReferences? angleReference = LabelAngleReferences.Absolute;
    private LabelAngleOnRightSideRotations? angleRotation = LabelAngleOnRightSideRotations.Clockwise;
    private bool addHalfRotationOnRightSide;
    private LayoutChoice? selectedLayout;
    private bool isLayoutEnabled = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<SettingsChangedEventArgs>? SettingChanged;

    public IReadOnlyList<LabelAlongEdgePlacements> PlacementOptions { get; } =
      Enum.GetValues<LabelAlongEdgePlacements>();

    public IReadOnlyList<LabelEdgeSides> SideOptions { get; } = Enum.GetValues<LabelEdgeSides>();

    public IReadOnlyList<LabelSideReferences> SideReferenceOptions { get; } = Enum.GetValues<LabelSideReferences>();

    public IReadOnlyList<LabelAngleReferences> AngleReferenceOptions { get; } = Enum.GetValues<LabelAngleReferences>();

    public IReadOnlyList<LabelAngleOnRightSideRotations> AngleRotationOptions { get; } =
      Enum.GetValues<LabelAngleOnRightSideRotations>();

    public IReadOnlyList<LayoutChoice> Layouts { get; } = [
        new("Hierarchical, Top to Bottom",
            EdgeLabelPlacementDemo.CreateHierarchicalLayout(LayoutOrientation.TopToBottom)),
        new("Hierarchical, Left to Right",
            EdgeLabelPlacementDemo.CreateHierarchicalLayout(LayoutOrientation.LeftToRight)),
        new("Generic Edge Labeling", new GenericLabeling()),
        new("Organic", EdgeLabelPlacementDemo.CreateOrganicLayout()),
        new("Generic Tree", EdgeLabelPlacementDemo.CreateTreeLayout()),
        new("Orthogonal", EdgeLabelPlacementDemo.CreateOrthogonalLayout())
    ];

    public Settings() {
      selectedLayout = Layouts[0];
    }

    public string LabelText {
      get => labelText;
      set => SetField(ref labelText, value ?? string.Empty);
    }

    public string DistanceToEdgeText {
      get => distanceToEdgeText;
      set => SetField(ref distanceToEdgeText, value ?? string.Empty);
    }

    public LabelAlongEdgePlacements? Placement {
      get => placement;
      set => SetField(ref placement, value);
    }

    public LabelEdgeSides? Side {
      get => side;
      set => SetField(ref side, value);
    }

    public LabelSideReferences? SideReference {
      get => sideReference;
      set => SetField(ref sideReference, value);
    }

    public string AngleText {
      get => angleText;
      set => SetField(ref angleText, value ?? string.Empty);
    }

    public LabelAngleReferences? AngleReference {
      get => angleReference;
      set => SetField(ref angleReference, value);
    }

    public LabelAngleOnRightSideRotations? AngleRotation {
      get => angleRotation;
      set => SetField(ref angleRotation, value);
    }

    public bool AddHalfRotationOnRightSide {
      get => addHalfRotationOnRightSide;
      set => SetField(ref addHalfRotationOnRightSide, value);
    }

    public LayoutChoice? SelectedLayout {
      get => selectedLayout;
      set => SetField(ref selectedLayout, value);
    }

    public bool IsLayoutEnabled {
      get => isLayoutEnabled;
      set {
        if (isLayoutEnabled == value) {
          return;
        }
        isLayoutEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLayoutEnabled)));
      }
    }

    /// <summary>
    ///   Updates the settings from the selected labels. If values differ, the corresponding setting is cleared.
    /// </summary>
    public void UpdateFrom(IReadOnlyList<ILabel> labels, IReadOnlyList<EdgeLabelPreferredPlacement> descriptors) {
      if (labels.Count == 0) {
        return;
      }

      suppressSettingChanged = true;
      try {
        LabelText = GetCommonValue(labels, label => label.Text) ?? string.Empty;
        Placement = GetCommonValue(descriptors, descriptor => descriptor.PlacementAlongEdge);
        Side = GetCommonValue(descriptors, descriptor => descriptor.EdgeSide);
        SideReference = GetCommonValue(descriptors, descriptor => descriptor.SideReference);
        AngleReference = GetCommonValue(descriptors, descriptor => descriptor.AngleReference);
        AngleRotation = GetCommonValue(descriptors, descriptor => descriptor.AngleRotationOnRightSide);
        AddHalfRotationOnRightSide =
            GetCommonValue(descriptors, descriptor => descriptor.AddHalfRotationOnRightSide) ?? false;
        DistanceToEdgeText =
            GetCommonValue(descriptors, descriptor => descriptor.DistanceToEdge)
                ?.ToString("0.##", CultureInfo.CurrentCulture) ?? string.Empty;
        AngleText = GetCommonValue(descriptors, descriptor => ToDegrees(descriptor.Angle))
            ?.ToString("0.##", CultureInfo.CurrentCulture) ?? string.Empty;
      } finally {
        suppressSettingChanged = false;
      }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return false;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      if (!suppressSettingChanged) {
        SettingChanged?.Invoke(this, new SettingsChangedEventArgs(propertyName));
      }
      return true;
    }

    private static T? GetCommonValue<T, TSource>(IReadOnlyList<TSource> items, Func<TSource, T> getValue)
        where T : struct {
      if (items.Count == 0) {
        return null;
      }
      var first = getValue(items[0]);
      return items.Skip(1).All(item => EqualityComparer<T>.Default.Equals(first, getValue(item))) ? first : null;
    }

    private static string? GetCommonValue<TSource>(IReadOnlyList<TSource> items, Func<TSource, string> getValue) {
      if (items.Count == 0) {
        return null;
      }
      var first = getValue(items[0]);
      return items.Skip(1).All(item => string.Equals(first, getValue(item), StringComparison.Ordinal)) ? first : null;
    }

    private static double ToDegrees(double radians) => radians * 180 / Math.PI;
  }

  public sealed record LayoutChoice(string Name, ILayoutAlgorithm Algorithm);

  public sealed class SettingsChangedEventArgs(string propertyName) : EventArgs
  {
    public string PropertyName { get; } = propertyName;
  }
}
