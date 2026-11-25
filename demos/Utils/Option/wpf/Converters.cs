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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Annotations;
using yWorks.Demo.Option.Editor;
using yWorks.Demo.Option.Handler;
using yWorks.Demo.Option.I18N;
using yWorks.Demo.Option.View;
using yWorks.Geometry;
using ArrayList = System.Collections.Generic.List<object>;
using FrameworkElement = System.Windows.Controls.Control;

namespace yWorks.Demo.Option

{
  //  public class ToStringConverter : IValueConverter
  //  {
  //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
  //      if (value == null) {
  //        return "null";
  //      }
  //      return value.ToString();
  //    }
  //
  //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
  //      throw new NotImplementedException();
  //    }
  //  }

  public class TabConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var source = value as IEnumerable<IOptionItem>;
      if (source != null) {
        var result = new ObservableCollection<TabItem>();
        foreach (var item in source) {
          result.Add(CreateTabItem(item, culture));
        }
        if (source is INotifyCollectionChanged) {
          ((INotifyCollectionChanged) source).CollectionChanged += new Adapter(result, culture).CollectionChanged;
        }
        return result;
      }
      return null;
    }

    internal static TabItem CreateTabItem(IOptionItem item, CultureInfo culture) =>
        new() {
            Header = new XamlLocalizingConverter().Convert(item, typeof(string), null, culture),
            Content = new OptionItemPresenter { Item = item }
        };

    public class Adapter
    {
      private readonly CultureInfo culture;
      private readonly WeakReference result;

      public Adapter(ObservableCollection<TabItem> result, CultureInfo culture) {
        this.culture = culture;
        this.result = new WeakReference(result);
      }

      public void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        var tabs = result.Target as ObservableCollection<TabItem>;
        if (tabs == null) {
          ((INotifyCollectionChanged) sender).CollectionChanged -= CollectionChanged;
        } else {
          switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
              var index = e.NewStartingIndex;
              foreach (var tabItem in e.NewItems) {
                tabs.Insert(index++, CreateTabItem((IOptionItem) tabItem, culture));
              }
              break;
            case NotifyCollectionChangedAction.Remove:
              for (var i = 0; i < e.OldItems.Count; i++) {
                tabs.RemoveAt(e.OldStartingIndex);
              }
              break;
            case NotifyCollectionChangedAction.Replace:
              for (var i = 0; i < e.OldItems.Count; i++) {
                tabs[i + e.NewStartingIndex] = CreateTabItem((IOptionItem) e.NewItems[i], culture);
              }
              break;
            case NotifyCollectionChangedAction.Reset:
              tabs.Clear();
              foreach (var tabItem in (IEnumerable<IOptionItem>) sender) {
                tabs.Add(CreateTabItem(tabItem, culture));
              }
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
      }
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  /// <summary>
  ///   A helper class that is used by the <see cref="ItemsControl"/>s used by the option handler
  ///   implementations.
  /// </summary>
  /// <remarks>
  ///   This class is public due to XAML visibility requirements.
  /// </remarks>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class XamlLocalizingConverter : IValueConverter
  {
    internal static string GetLocalizedString(
        I18NFactory i18NFactory,
        string context,
        string i18nKeyString,
        string name) {
      if (i18NFactory == null) {
        return name;
      }
      var s = i18NFactory.GetString(context, i18nKeyString);
      if (s == i18nKeyString) {
        return name;
      }
      return s;
    }

    /// <xamlhelper/>
    public IOptionItem OptionItem { get; set; }

    /// <xamlhelper/>
    public string PostFix { get; set; }

    /// <xamlhelper/>
    public string PreFix { get; set; }

    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (targetType == typeof(string) || targetType == typeof(object)) {
        var item = value as IOptionItem;
        var stringParam = parameter as string;

        if (item == null && OptionItem != null) {
          item = OptionItem;
          stringParam = value != null ? value.ToString() : null;
        }

        if (item == null && parameter is IOptionItem) {
          item = parameter as IOptionItem;
          stringParam = value != null ? value.ToString() : null;
        }

        if (item != null) {
          var handler = GetHandler(item);
          if (handler != null) {
            var nameOverride = item.Attributes[Handler.OptionItem.DisplaynameAttribute] as string;
            if (nameOverride != null && stringParam == null) {
              return handler.I18nFactory.GetString(handler.Name, nameOverride);
            }

            var i18nName = GetI18nName(item);
            if (stringParam != null) {
              return GetLocalizedString(handler.I18nFactory, handler.Name,
                  (PreFix ?? string.Empty) + i18nName + (PostFix ?? string.Empty) + stringParam, stringParam);
            }
            return GetLocalizedString(handler.I18nFactory, handler.Name,
                (PreFix ?? string.Empty) + i18nName + (PostFix ?? string.Empty), item.Name);
          }
          return item.Name;
        }
      }
      return DependencyProperty.UnsetValue;
    }

    private static string GetI18nName(IOptionItem item) {
      var customI18nPrefix = (string) item.Attributes[Handler.OptionItem.CustomI18NPrefix];
      var i18nName = customI18nPrefix ?? item.Name;
      var group = item.Owner;
      while (group != null) {
        customI18nPrefix = (string) group.Attributes[Handler.OptionItem.CustomI18NPrefix];
        i18nName = (customI18nPrefix ?? group.Name) + "." + i18nName;
        group = group.Owner;
      }
      return i18nName;
    }

    private static OptionHandler GetHandler(IOptionItem item) {
      var handler = item.Lookup(typeof(OptionHandler)) as OptionHandler;
      if (handler != null) {
        return handler;
      }
      var group = item.Owner;
      while (group != null) {
        if (group is OptionHandler) {
          return (OptionHandler) group;
        }
        handler = group.Lookup(typeof(OptionHandler)) as OptionHandler;
        if (handler != null) {
          return handler;
        }
        group = group.Owner;
      }
      return null;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  /// <xamlhelper/>
  [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class ColorBrushConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is Color) {
        return new SolidColorBrush((Color) value);
      }
      return null;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is SolidColorBrush) {
        return ((SolidColorBrush) value).Color;
      }
      return DependencyProperty.UnsetValue;
    }
  }

  /// <xamlhelper/>
  [ValueConversion(typeof(Brush), typeof(Color))]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class SolidColorBrushColorConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is SolidColorBrush) {
        return ((SolidColorBrush) value).Color;
      }
      return DependencyProperty.UnsetValue;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is Color) {
        return new SolidColorBrush((Color) value);
      }
      return DependencyProperty.UnsetValue;
    }
  }

  /// <xamlhelper/>
  [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class StrokeThicknessConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is Pen) {
        return ((Pen) value).Thickness;
      }
      return DependencyProperty.UnsetValue;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class IntConverter : ValidationRule, IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == OptionItem.ValueUndefined) {
        return DependencyProperty.UnsetValue;
      }
      return string.Format(culture, "{0:D}", value);
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      int result;
      var b = int.TryParse(value as string, NumberStyles.Number, culture, out result);
      if (!b) {
        return DependencyProperty.UnsetValue;
      }
      return result;
    }

    /// <xamlhelper/>
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
      int result;
      var b = int.TryParse(value as string, NumberStyles.Number, cultureInfo, out result);
      if (b) {
        return new ValidationResult(true, null);
      }
      return new ValidationResult(false, null);
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class DoubleConverter : ValidationRule, IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == OptionItem.ValueUndefined) {
        return DependencyProperty.UnsetValue;
      }
      return string.Format(culture, "{0:F}", value);
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      double result;
      var b = double.TryParse(value as string, NumberStyles.Float | NumberStyles.AllowThousands, culture, out result);
      if (!b) {
        return DependencyProperty.UnsetValue;
      }
      return result;
    }

    /// <xamlhelper/>
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
      double result;
      var b = double.TryParse(value as string, NumberStyles.Float | NumberStyles.AllowThousands, cultureInfo,
          out result);
      if (b) {
        return new ValidationResult(true, null);
      }
      return new ValidationResult(false, null);
    }
  }

  internal sealed class ColorValueSerializer : ValueSerializer
  {
    private static readonly Dictionary<string, string> colorMap;
    private static readonly Dictionary<string, string> invColorMap;
    private static readonly Dictionary<string, string> ciColorMap;

    static ColorValueSerializer() {
      {
        ciColorMap = new Dictionary<string, string>();
        ciColorMap["aliceblue"] = "#FFF0F8FF";
        ciColorMap["antiquewhite"] = "#FFFAEBD7";
        ciColorMap["aqua"] = "#FF00FFFF";
        ciColorMap["aquamarine"] = "#FF7FFFD4";
        ciColorMap["azure"] = "#FFF0FFFF";
        ciColorMap["beige"] = "#FFF5F5DC";
        ciColorMap["bisque"] = "#FFFFE4C4";
        ciColorMap["black"] = "#FF000000";
        ciColorMap["blanchedalmond"] = "#FFFFEBCD";
        ciColorMap["blue"] = "#FF0000FF";
        ciColorMap["blueviolet"] = "#FF8A2BE2";
        ciColorMap["brown"] = "#FFA52A2A";
        ciColorMap["burlywood"] = "#FFDEB887";
        ciColorMap["cadetblue"] = "#FF5F9EA0";
        ciColorMap["chartreuse"] = "#FF7FFF00";
        ciColorMap["chocolate"] = "#FFD2691E";
        ciColorMap["coral"] = "#FFFF7F50";
        ciColorMap["cornflowerblue"] = "#FF6495ED";
        ciColorMap["cornsilk"] = "#FFFFF8DC";
        ciColorMap["crimson"] = "#FFDC143C";
        ciColorMap["cyan"] = "#FF00FFFF";
        ciColorMap["darkblue"] = "#FF00008B";
        ciColorMap["darkcyan"] = "#FF008B8B";
        ciColorMap["darkgoldenrod"] = "#FFB8860B";
        ciColorMap["darkgray"] = "#FFA9A9A9";
        ciColorMap["darkgreen"] = "#FF006400";
        ciColorMap["darkkhaki"] = "#FFBDB76B";
        ciColorMap["darkmagenta"] = "#FF8B008B";
        ciColorMap["darkolivegreen"] = "#FF556B2F";
        ciColorMap["darkorange"] = "#FFFF8C00";
        ciColorMap["darkorchid"] = "#FF9932CC";
        ciColorMap["darkred"] = "#FF8B0000";
        ciColorMap["darksalmon"] = "#FFE9967A";
        ciColorMap["darkseagreen"] = "#FF8FBC8F";
        ciColorMap["darkslateblue"] = "#FF483D8B";
        ciColorMap["darkslategray"] = "#FF2F4F4F";
        ciColorMap["darkturquoise"] = "#FF00CED1";
        ciColorMap["darkviolet"] = "#FF9400D3";
        ciColorMap["deeppink"] = "#FFFF1493";
        ciColorMap["deepskyblue"] = "#FF00BFFF";
        ciColorMap["dimgray"] = "#FF696969";
        ciColorMap["dodgerblue"] = "#FF1E90FF";
        ciColorMap["firebrick"] = "#FFB22222";
        ciColorMap["floralwhite"] = "#FFFFFAF0";
        ciColorMap["forestgreen"] = "#FF228B22";
        ciColorMap["fuchsia"] = "#FFFF00FF";
        ciColorMap["gainsboro"] = "#FFDCDCDC";
        ciColorMap["ghostwhite"] = "#FFF8F8FF";
        ciColorMap["gold"] = "#FFFFD700";
        ciColorMap["goldenrod"] = "#FFDAA520";
        ciColorMap["gray"] = "#FF808080";
        ciColorMap["green"] = "#FF008000";
        ciColorMap["greenyellow"] = "#FFADFF2F";
        ciColorMap["honeydew"] = "#FFF0FFF0";
        ciColorMap["hotpink"] = "#FFFF69B4";
        ciColorMap["indianred"] = "#FFCD5C5C";
        ciColorMap["indigo"] = "#FF4B0082";
        ciColorMap["ivory"] = "#FFFFFFF0";
        ciColorMap["khaki"] = "#FFF0E68C";
        ciColorMap["lavender"] = "#FFE6E6FA";
        ciColorMap["lavenderblush"] = "#FFFFF0F5";
        ciColorMap["lawngreen"] = "#FF7CFC00";
        ciColorMap["lemonchiffon"] = "#FFFFFACD";
        ciColorMap["lightblue"] = "#FFADD8E6";
        ciColorMap["lightcoral"] = "#FFF08080";
        ciColorMap["lightcyan"] = "#FFE0FFFF";
        ciColorMap["lightgoldenrodyellow"] = "#FFFAFAD2";
        ciColorMap["lightgray"] = "#FFD3D3D3";
        ciColorMap["lightgreen"] = "#FF90EE90";
        ciColorMap["lightpink"] = "#FFFFB6C1";
        ciColorMap["lightsalmon"] = "#FFFFA07A";
        ciColorMap["lightseagreen"] = "#FF20B2AA";
        ciColorMap["lightskyblue"] = "#FF87CEFA";
        ciColorMap["lightslategray"] = "#FF778899";
        ciColorMap["lightsteelblue"] = "#FFB0C4DE";
        ciColorMap["lightyellow"] = "#FFFFFFE0";
        ciColorMap["lime"] = "#FF00FF00";
        ciColorMap["limegreen"] = "#FF32CD32";
        ciColorMap["linen"] = "#FFFAF0E6";
        ciColorMap["magenta"] = "#FFFF00FF";
        ciColorMap["maroon"] = "#FF800000";
        ciColorMap["mediumaquamarine"] = "#FF66CDAA";
        ciColorMap["mediumblue"] = "#FF0000CD";
        ciColorMap["mediumorchid"] = "#FFBA55D3";
        ciColorMap["mediumpurple"] = "#FF9370DB";
        ciColorMap["mediumseagreen"] = "#FF3CB371";
        ciColorMap["mediumslateblue"] = "#FF7B68EE";
        ciColorMap["mediumspringgreen"] = "#FF00FA9A";
        ciColorMap["mediumturquoise"] = "#FF48D1CC";
        ciColorMap["mediumvioletred"] = "#FFC71585";
        ciColorMap["midnightblue"] = "#FF191970";
        ciColorMap["mintcream"] = "#FFF5FFFA";
        ciColorMap["mistyrose"] = "#FFFFE4E1";
        ciColorMap["moccasin"] = "#FFFFE4B5";
        ciColorMap["navajowhite"] = "#FFFFDEAD";
        ciColorMap["navy"] = "#FF000080";
        ciColorMap["oldlace"] = "#FFFDF5E6";
        ciColorMap["olive"] = "#FF808000";
        ciColorMap["olivedrab"] = "#FF6B8E23";
        ciColorMap["orange"] = "#FFFFA500";
        ciColorMap["orangered"] = "#FFFF4500";
        ciColorMap["orchid"] = "#FFDA70D6";
        ciColorMap["palegoldenrod"] = "#FFEEE8AA";
        ciColorMap["palegreen"] = "#FF98FB98";
        ciColorMap["paleturquoise"] = "#FFAFEEEE";
        ciColorMap["palevioletred"] = "#FFDB7093";
        ciColorMap["papayawhip"] = "#FFFFEFD5";
        ciColorMap["peachpuff"] = "#FFFFDAB9";
        ciColorMap["peru"] = "#FFCD853F";
        ciColorMap["pink"] = "#FFFFC0CB";
        ciColorMap["plum"] = "#FFDDA0DD";
        ciColorMap["powderblue"] = "#FFB0E0E6";
        ciColorMap["purple"] = "#FF800080";
        ciColorMap["red"] = "#FFFF0000";
        ciColorMap["rosybrown"] = "#FFBC8F8F";
        ciColorMap["royalblue"] = "#FF4169E1";
        ciColorMap["saddlebrown"] = "#FF8B4513";
        ciColorMap["salmon"] = "#FFFA8072";
        ciColorMap["sandybrown"] = "#FFF4A460";
        ciColorMap["seagreen"] = "#FF2E8B57";
        ciColorMap["seashell"] = "#FFFFF5EE";
        ciColorMap["sienna"] = "#FFA0522D";
        ciColorMap["silver"] = "#FFC0C0C0";
        ciColorMap["skyblue"] = "#FF87CEEB";
        ciColorMap["slateblue"] = "#FF6A5ACD";
        ciColorMap["slategray"] = "#FF708090";
        ciColorMap["snow"] = "#FFFFFAFA";
        ciColorMap["springgreen"] = "#FF00FF7F";
        ciColorMap["steelblue"] = "#FF4682B4";
        ciColorMap["tan"] = "#FFD2B48C";
        ciColorMap["teal"] = "#FF008080";
        ciColorMap["thistle"] = "#FFD8BFD8";
        ciColorMap["tomato"] = "#FFFF6347";
        ciColorMap["transparent"] = "#00FFFFFF";
        ciColorMap["turquoise"] = "#FF40E0D0";
        ciColorMap["violet"] = "#FFEE82EE";
        ciColorMap["wheat"] = "#FFF5DEB3";
        ciColorMap["white"] = "#FFFFFFFF";
        ciColorMap["whitesmoke"] = "#FFF5F5F5";
        ciColorMap["yellow"] = "#FFFFFF00";
        ciColorMap["yellowgreen"] = "#FF9ACD32";
      }
      {
        colorMap = new Dictionary<string, string>();
        invColorMap = new Dictionary<string, string>();
        colorMap["AliceBlue"] = "#FFF0F8FF";
        colorMap["AntiqueWhite"] = "#FFFAEBD7";
        colorMap["Aqua"] = "#FF00FFFF";
        colorMap["Aquamarine"] = "#FF7FFFD4";
        colorMap["Azure"] = "#FFF0FFFF";
        colorMap["Beige"] = "#FFF5F5DC";
        colorMap["Bisque"] = "#FFFFE4C4";
        colorMap["Black"] = "#FF000000";
        colorMap["BlanchedAlmond"] = "#FFFFEBCD";
        colorMap["Blue"] = "#FF0000FF";
        colorMap["BlueViolet"] = "#FF8A2BE2";
        colorMap["Brown"] = "#FFA52A2A";
        colorMap["BurlyWood"] = "#FFDEB887";
        colorMap["CadetBlue"] = "#FF5F9EA0";
        colorMap["Chartreuse"] = "#FF7FFF00";
        colorMap["Chocolate"] = "#FFD2691E";
        colorMap["Coral"] = "#FFFF7F50";
        colorMap["CornflowerBlue"] = "#FF6495ED";
        colorMap["Cornsilk"] = "#FFFFF8DC";
        colorMap["Crimson"] = "#FFDC143C";
        colorMap["Cyan"] = "#FF00FFFF";
        colorMap["DarkBlue"] = "#FF00008B";
        colorMap["DarkCyan"] = "#FF008B8B";
        colorMap["DarkGoldenrod"] = "#FFB8860B";
        colorMap["DarkGray"] = "#FFA9A9A9";
        colorMap["DarkGreen"] = "#FF006400";
        colorMap["DarkKhaki"] = "#FFBDB76B";
        colorMap["DarkMagenta"] = "#FF8B008B";
        colorMap["DarkOliveGreen"] = "#FF556B2F";
        colorMap["DarkOrange"] = "#FFFF8C00";
        colorMap["DarkOrchid"] = "#FF9932CC";
        colorMap["DarkRed"] = "#FF8B0000";
        colorMap["DarkSalmon"] = "#FFE9967A";
        colorMap["DarkSeaGreen"] = "#FF8FBC8F";
        colorMap["DarkSlateBlue"] = "#FF483D8B";
        colorMap["DarkSlateGray"] = "#FF2F4F4F";
        colorMap["DarkTurquoise"] = "#FF00CED1";
        colorMap["DarkViolet"] = "#FF9400D3";
        colorMap["DeepPink"] = "#FFFF1493";
        colorMap["DeepSkyBlue"] = "#FF00BFFF";
        colorMap["DimGray"] = "#FF696969";
        colorMap["DodgerBlue"] = "#FF1E90FF";
        colorMap["Firebrick"] = "#FFB22222";
        colorMap["FloralWhite"] = "#FFFFFAF0";
        colorMap["ForestGreen"] = "#FF228B22";
        colorMap["Fuchsia"] = "#FFFF00FF";
        colorMap["Gainsboro"] = "#FFDCDCDC";
        colorMap["GhostWhite"] = "#FFF8F8FF";
        colorMap["Gold"] = "#FFFFD700";
        colorMap["Goldenrod"] = "#FFDAA520";
        colorMap["Gray"] = "#FF808080";
        colorMap["Green"] = "#FF008000";
        colorMap["GreenYellow"] = "#FFADFF2F";
        colorMap["Honeydew"] = "#FFF0FFF0";
        colorMap["HotPink"] = "#FFFF69B4";
        colorMap["IndianRed"] = "#FFCD5C5C";
        colorMap["Indigo"] = "#FF4B0082";
        colorMap["Ivory"] = "#FFFFFFF0";
        colorMap["Khaki"] = "#FFF0E68C";
        colorMap["Lavender"] = "#FFE6E6FA";
        colorMap["LavenderBlush"] = "#FFFFF0F5";
        colorMap["LawnGreen"] = "#FF7CFC00";
        colorMap["LemonChiffon"] = "#FFFFFACD";
        colorMap["LightBlue"] = "#FFADD8E6";
        colorMap["LightCoral"] = "#FFF08080";
        colorMap["LightCyan"] = "#FFE0FFFF";
        colorMap["LightGoldenrodYellow"] = "#FFFAFAD2";
        colorMap["LightGray"] = "#FFD3D3D3";
        colorMap["LightGreen"] = "#FF90EE90";
        colorMap["LightPink"] = "#FFFFB6C1";
        colorMap["LightSalmon"] = "#FFFFA07A";
        colorMap["LightSeaGreen"] = "#FF20B2AA";
        colorMap["LightSkyBlue"] = "#FF87CEFA";
        colorMap["LightSlateGray"] = "#FF778899";
        colorMap["LightSteelBlue"] = "#FFB0C4DE";
        colorMap["LightYellow"] = "#FFFFFFE0";
        colorMap["Lime"] = "#FF00FF00";
        colorMap["LimeGreen"] = "#FF32CD32";
        colorMap["Linen"] = "#FFFAF0E6";
        colorMap["Magenta"] = "#FFFF00FF";
        colorMap["Maroon"] = "#FF800000";
        colorMap["MediumAquamarine"] = "#FF66CDAA";
        colorMap["MediumBlue"] = "#FF0000CD";
        colorMap["MediumOrchid"] = "#FFBA55D3";
        colorMap["MediumPurple"] = "#FF9370DB";
        colorMap["MediumSeaGreen"] = "#FF3CB371";
        colorMap["MediumSlateBlue"] = "#FF7B68EE";
        colorMap["MediumSpringGreen"] = "#FF00FA9A";
        colorMap["MediumTurquoise"] = "#FF48D1CC";
        colorMap["MediumVioletRed"] = "#FFC71585";
        colorMap["MidnightBlue"] = "#FF191970";
        colorMap["MintCream"] = "#FFF5FFFA";
        colorMap["MistyRose"] = "#FFFFE4E1";
        colorMap["Moccasin"] = "#FFFFE4B5";
        colorMap["NavajoWhite"] = "#FFFFDEAD";
        colorMap["Navy"] = "#FF000080";
        colorMap["OldLace"] = "#FFFDF5E6";
        colorMap["Olive"] = "#FF808000";
        colorMap["OliveDrab"] = "#FF6B8E23";
        colorMap["Orange"] = "#FFFFA500";
        colorMap["OrangeRed"] = "#FFFF4500";
        colorMap["Orchid"] = "#FFDA70D6";
        colorMap["PaleGoldenrod"] = "#FFEEE8AA";
        colorMap["PaleGreen"] = "#FF98FB98";
        colorMap["PaleTurquoise"] = "#FFAFEEEE";
        colorMap["PaleVioletRed"] = "#FFDB7093";
        colorMap["PapayaWhip"] = "#FFFFEFD5";
        colorMap["PeachPuff"] = "#FFFFDAB9";
        colorMap["Peru"] = "#FFCD853F";
        colorMap["Pink"] = "#FFFFC0CB";
        colorMap["Plum"] = "#FFDDA0DD";
        colorMap["PowderBlue"] = "#FFB0E0E6";
        colorMap["Purple"] = "#FF800080";
        colorMap["Red"] = "#FFFF0000";
        colorMap["RosyBrown"] = "#FFBC8F8F";
        colorMap["RoyalBlue"] = "#FF4169E1";
        colorMap["SaddleBrown"] = "#FF8B4513";
        colorMap["Salmon"] = "#FFFA8072";
        colorMap["SandyBrown"] = "#FFF4A460";
        colorMap["SeaGreen"] = "#FF2E8B57";
        colorMap["SeaShell"] = "#FFFFF5EE";
        colorMap["Sienna"] = "#FFA0522D";
        colorMap["Silver"] = "#FFC0C0C0";
        colorMap["SkyBlue"] = "#FF87CEEB";
        colorMap["SlateBlue"] = "#FF6A5ACD";
        colorMap["SlateGray"] = "#FF708090";
        colorMap["Snow"] = "#FFFFFAFA";
        colorMap["SpringGreen"] = "#FF00FF7F";
        colorMap["SteelBlue"] = "#FF4682B4";
        colorMap["Tan"] = "#FFD2B48C";
        colorMap["Teal"] = "#FF008080";
        colorMap["Thistle"] = "#FFD8BFD8";
        colorMap["Tomato"] = "#FFFF6347";
        colorMap["Transparent"] = "#00FFFFFF";
        colorMap["Turquoise"] = "#FF40E0D0";
        colorMap["Violet"] = "#FFEE82EE";
        colorMap["Wheat"] = "#FFF5DEB3";
        colorMap["White"] = "#FFFFFFFF";
        colorMap["WhiteSmoke"] = "#FFF5F5F5";
        colorMap["Yellow"] = "#FFFFFF00";
        colorMap["YellowGreen"] = "#FF9ACD32";
        foreach (var pair in colorMap) {
          invColorMap[pair.Value] = pair.Key;
        }
      }
    }

    #region Overrides of ValueSerializer

    /// <inheritdoc/>
    public override bool CanConvertFromString(string value, IValueSerializerContext context) =>
        true; // Regex.Matches(value, "^#([0-9A-Fa-f]{2}){4}$").Count == 1 || colorMap.ContainsKey(value);

    /// <inheritdoc/>
    public override bool CanConvertToString(object value, IValueSerializerContext context) => value is Color;

    /// <inheritdoc/>
    public override object ConvertFromString(string c, IValueSerializerContext context) {
      string replacement;
      if (ciColorMap.TryGetValue(c.ToLower(CultureInfo.InvariantCulture), out replacement)) {
        c = replacement;
      }
      if (Regex.Matches(c, "^#([0-9A-Fa-f]{2}){4}$").Count == 1) {
        return Color.FromArgb(byte.Parse(c.Substring(1, 2), NumberStyles.HexNumber),
            byte.Parse(c.Substring(3, 2), NumberStyles.HexNumber),
            byte.Parse(c.Substring(5, 2), NumberStyles.HexNumber),
            byte.Parse(c.Substring(7, 2), NumberStyles.HexNumber));
      }
      if (Regex.Matches(c, "^#([0-9A-Fa-f]{2}){3}$").Count == 1) {
        return Color.FromArgb(255, byte.Parse(c.Substring(1, 2), NumberStyles.HexNumber),
            byte.Parse(c.Substring(3, 2), NumberStyles.HexNumber),
            byte.Parse(c.Substring(5, 2), NumberStyles.HexNumber));
      }
      return Colors.Black;
    }

    /// <inheritdoc/>
    public override string ConvertToString(object o, IValueSerializerContext context) {
      var c = (Color) o;
      var colorString = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
      if (c == Colors.Transparent || c.A == 255) {
        string colorName;
        if (invColorMap.TryGetValue(colorString, out colorName)) {
          return colorName;
        }
      }
      return colorString;
    }

    internal string ConvertToHexString(object o, IValueSerializerContext context) {
      var c = (Color) o;
      var colorString = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
      return colorString;
    }

    #endregion
  }


  public class ColorConverter : IValueConverter
  {
    private readonly string NO_COLOR = "No Color";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (IsAssignmentCompatible(targetType, typeof(string))) {
        if (value is Color) {
          return new ColorValueSerializer().ConvertToString(value, null);
        }
        return NO_COLOR;
      }
      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (IsAssignmentCompatible(targetType, typeof(Color)) && value is string) {
        if (value.Equals(NO_COLOR)) {
          return ColorChooser.NullColorObject;
        }
        return new ColorValueSerializer().ConvertFromString((string) value, null);
      }
      return DependencyProperty.UnsetValue;
    }

    private bool IsAssignmentCompatible(Type targetType, Type type) =>
        targetType == type || type.IsSubclassOf(targetType);
  }

  public class GenericConverter<TSourceType, TTargetType> : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      {
        var typeConverter = TypeDescriptor.GetConverter(typeof(TSourceType));
        if (typeConverter != null && typeConverter.CanConvertTo(typeof(TTargetType))) {
          return typeConverter.ConvertTo(null, culture, value, typeof(TTargetType));
        }
      }
      if (targetType == typeof(string)) {
        var valueSerializer = ValueSerializer.GetSerializerFor(typeof(TSourceType));
        if (valueSerializer != null && valueSerializer.CanConvertToString(value, null)) {
          return valueSerializer.ConvertToString(value, null);
        }
      }
      {
        var typeConverter = TypeDescriptor.GetConverter(typeof(TTargetType));
        if (typeConverter != null && typeConverter.CanConvertFrom(typeof(TSourceType))) {
          return typeConverter.ConvertFrom(null, culture, value);
        }
      }
      return DependencyProperty.UnsetValue;
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      {
        var typeConverter = TypeDescriptor.GetConverter(typeof(TSourceType));
        if (typeConverter != null && typeConverter.CanConvertFrom(typeof(TTargetType))) {
          return typeConverter.ConvertFrom(null, culture, value);
        }
      }
      {
        var typeConverter = TypeDescriptor.GetConverter(typeof(TTargetType));
        if (typeConverter != null && typeConverter.CanConvertTo(typeof(TSourceType))) {
          return typeConverter.ConvertTo(null, culture, value, typeof(TSourceType));
        }
      }
      if (value is string) {
        var valueSerializer = ValueSerializer.GetSerializerFor(typeof(TSourceType));
        if (valueSerializer != null && valueSerializer.CanConvertFromString((string) value, null)) {
          return valueSerializer.ConvertFromString((string) value, null);
        }
      }
      return DependencyProperty.UnsetValue;
    }
  }

  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class InsetsDConverter : GenericConverter<InsetsD, string> { }

  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class SizeDConverter : GenericConverter<SizeD, string> { }


  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class PointDConverter : GenericConverter<PointD, string> { }

  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class TypefaceConverter : GenericConverter<Typeface, string> { }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class XamlItemIsUndefinedConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (targetType == typeof(object) || targetType == typeof(bool)) {
        if (value is IOptionItem) {
          return ((IOptionItem) value).Value == OptionItem.ValueUndefined;
        }
        return value == OptionItem.ValueUndefined;
      }
      return DependencyProperty.UnsetValue;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class XamlItemTypeConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (targetType == typeof(object) || targetType == typeof(Type)) {
        if (value is IOptionItem) {
          return ((IOptionItem) value).Type;
        }
        return typeof(void);
      }
      return DependencyProperty.UnsetValue;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class XamlDomainConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is IOptionItem) {
        return ((IOptionItem) value).Attributes[CollectionOptionItem<object>.DOMAIN_ATTRIBUTE];
      }
      return DependencyProperty.UnsetValue;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  /// <xamlhelper/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class EnumDomainConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      Type enumType;
      if (value is IOptionItem) {
        enumType = ((IOptionItem) value).Type;
      } else if (value != null) {
        enumType = value.GetType();
      } else {
        return DependencyProperty.UnsetValue;
      }

      if (enumType.IsEnum) {
        var array = GetValues(enumType);
        ArrayList list = [.. array];
        return list;
      }
      return DependencyProperty.UnsetValue;
    }

    private Array GetValues(Type enumType) =>
        (from field in enumType.GetFields() where field.IsLiteral select field.GetValue(enumType)).ToArray();

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  public class ColorDomain : IEnumerable<object>
  {
    private static IList<object> colors;
    private static IList<object> colorsWithNoColor;

    static ColorDomain() {
      BuildColors();
    }

    public bool NoColorAllowed { get; set; }

    private static void BuildColors() {
      colors = new List<object>();
      colorsWithNoColor = new List<object>();

      colorsWithNoColor.Add(ColorChooser.NullColorObject);

      var fields = typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static);
      foreach (var info in fields) {
        var b = info.GetValue(null, null) as SolidColorBrush;
        if (b != null) {
          colors.Add(b.Color);
          colorsWithNoColor.Add(b.Color);
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<object> GetEnumerator() =>
        NoColorAllowed ? colorsWithNoColor.GetEnumerator() : colors.GetEnumerator();
  }

  /// <xamlhelper/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class ColorDomainConverter : IValueConverter
  {
    private static IList<Color> colors;

    static ColorDomainConverter() {
      BuildColors();
    }

    private static void BuildColors() {
      colors = new List<Color>();
      var fields = typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static);
      foreach (var info in fields) {
        var b = info.GetValue(null, null) as SolidColorBrush;
        if (b != null) {
          colors.Add(b.Color);
        }
      }
    }

    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      Type itemType;
      if (value is IOptionItem) {
        itemType = ((IOptionItem) value).Type;
      } else if (value != null) {
        itemType = value.GetType();
      } else {
        return DependencyProperty.UnsetValue;
      }

      if (itemType == typeof(Color) || typeof(Brush).IsAssignableFrom(itemType)) {
        return colors;
      }
      return DependencyProperty.UnsetValue;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  internal class XamlOptionItemEditorSelector
  {
    internal static T GetAttribute<T>(IOptionItem item, string attribute, FrameworkElement container) where T : class {
      if (item == null) {
        return null;
      }
      var editorAttribute = attribute != null ? item.Attributes[attribute] : null;
      if (editorAttribute != null) {
        if (editorAttribute is T) {
          return editorAttribute as T;
        }
        if (container != null) {
          var resource = container.TryFindResource(editorAttribute) as T;
          if (resource != null) {
            return resource;
          }
        }
        if (editorAttribute is Type) {
          try {
            var o = Activator.CreateInstance((Type) editorAttribute);
            return (T) o;
          } catch (Exception e) {
            Console.WriteLine(e);
          }
        }
        if (editorAttribute is string) {
          try {
            return (T) Activator.CreateInstance(Type.GetType((string) editorAttribute));
          } catch (Exception e) {
            Console.WriteLine(e);
          }
        }
        return null;
      }
      return null;
    }

    internal static object GetAttribute(Type T, IOptionItem item, string attribute, FrameworkElement container) {
      if (item == null) {
        return null;
      }
      var editorAttribute = attribute != null ? item.Attributes[attribute] : null;
      if (editorAttribute != null) {
        if (T.IsInstanceOfType(editorAttribute)) {
          return editorAttribute;
        }
        if (container is Control) {
          var resource = container.TryFindResource(editorAttribute);
          if (T.IsInstanceOfType(resource)) {
            return resource;
          }
        }
        if (editorAttribute is Type) {
          try {
            var o = Activator.CreateInstance((Type) editorAttribute);
            if (T.IsInstanceOfType(o)) {
              return o;
            }
          } catch (Exception e) {
            Console.WriteLine(e);
          }
        }
        if (editorAttribute is string) {
          try {
            throw new NotSupportedException();
            //            object o = Activator.CreateInstance(null, (string) (editorAttribute)).Unwrap();
            //            if (T.IsInstanceOfType(o)) {
            //              return o;
            //            }
          } catch (Exception e) {
            Console.WriteLine(e);
          }
        }
        return null;
      }
      return null;
    }
  }


  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class XamlUndefinedNullStringConverter : ValidationRule, IValueConverter
  {
    /// <xamlhelper/>
    [CanBeNull]
    public IOptionItem Item { get; set; }

    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == OptionItem.ValueUndefined) {
        return null;
      }
      if (value != null) {
        var converter = GetConverter();
        if (converter != null && converter.CanConvertTo(typeof(string))) {
          return converter.ConvertTo(null, culture, value, typeof(string));
        }
      }
      return value;
    }

    private TypeConverter GetConverter() =>
        XamlOptionItemEditorSelector.GetAttribute<TypeConverter>(Item, OptionItem.CustomTypeConverterAttribute, null) ??
        (Item != null ? TypeDescriptor.GetConverter(Item.Type) : null);

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == null) {
        return OptionItem.ValueUndefined;
      }
      var converter = GetConverter();
      if (converter != null && converter.CanConvertFrom(typeof(string))) {
        return converter.ConvertFrom(null, culture, value);
      }
      return DependencyProperty.UnsetValue;
    }

    /// <xamlhelper/>
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
      var converter = GetConverter();
      if (converter != null && converter.CanConvertFrom(typeof(string))) {
        try {
          var result = converter.ConvertFrom(null, cultureInfo, value);
          if (result == null || (Item != null && Item.Type.IsInstanceOfType(result))) {
            return ValidationResult.ValidResult;
          }
        } catch (Exception) { }
      }
      return new ValidationResult(false, null);
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class XamlUndefinedNullConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == OptionItem.ValueUndefined) {
        return null;
      }
      return value;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == null) {
        return OptionItem.ValueUndefined;
      }
      return value;
    }
  }

  public class DynamicTemplateConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var presenter = (OptionItemPresenter) value;
      return presenter.TryFindResource(presenter.Item.Attributes["DynamicContentControl.EditorTemplate"]);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class OptionItemAttributeConverter : IValueConverter
  {
    /// <xamlhelper/>
    public OptionItemAttributeConverter(string attributeName) {
      AttributeName = attributeName;
    }

    /// <xamlhelper/>
    public OptionItemAttributeConverter() { }

    /// <xamlhelper/>
    public string AttributeName { get; set; }

    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (parameter == null) {
        parameter = AttributeName;
      }
      if (parameter != null && value is IOptionItem) {
        var att = parameter is string ? (string) parameter : parameter.ToString();
        var o = XamlOptionItemEditorSelector.GetAttribute(targetType, (IOptionItem) value, att, null);
        if (o != null && targetType.IsInstanceOfType(o)) {
          return o;
        }
        if (targetType == typeof(string)) {
          return o != null ? o.ToString() : null;
        }
        return DependencyProperty.UnsetValue;
      }
      return DependencyProperty.UnsetValue;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class NullFilteringConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var nullAllowed = false;
      if (value == null && !nullAllowed) {
        return DependencyProperty.UnsetValue;
      }
      return value;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      var nullAllowed = false;
      var item = parameter as IOptionItem;
      if (item != null) {
        var attribute = item.Attributes[OptionItem.SupportNullValueAttribute];
        if (attribute != null && attribute is bool) {
          nullAllowed = (bool) attribute;
        }
      }
      if (value == null && !nullAllowed) {
        return DependencyProperty.UnsetValue;
      }
      return value;
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class NullFilteringSolidColorBrushConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var nullAllowed = true;
      if (value == null) {
        if (!nullAllowed) {
          return DependencyProperty.UnsetValue;
        }
        return ColorChooser.NullColorObject;
      }
      if (value is SolidColorBrush) {
        return ((SolidColorBrush) value).Color;
      }
      return ColorChooser.NullColorObject;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      var nullAllowed = true;
      var item = parameter as IOptionItem;
      if (item != null) {
        var attribute = item.Attributes[OptionItem.SupportNullValueAttribute];
        if (attribute != null && attribute is bool) {
          nullAllowed = (bool) attribute;
        }
      }
      if (value == null && !nullAllowed) {
        return DependencyProperty.UnsetValue;
      }
      if (value is Color) {
        return new SolidColorBrush((Color) value);
      }
      return null;
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class NullFilteringPenConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var nullAllowed = true;
      if (value == null) {
        if (!nullAllowed) {
          return DependencyProperty.UnsetValue;
        }
        return ColorChooser.NullColorObject;
      }
      if (value is Pen) {
        var brush = ((Pen) value).Brush as SolidColorBrush;
        return brush != null ? brush.Color : ColorChooser.NullColorObject;
      }
      return ColorChooser.NullColorObject;
    }

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      var nullAllowed = true;
      var item = parameter as IOptionItem;
      if (item != null) {
        var attribute = item.Attributes[OptionItem.SupportNullValueAttribute];
        if (attribute != null && attribute is bool) {
          nullAllowed = (bool) attribute;
        }
      }
      if (value == null && !nullAllowed) {
        return DependencyProperty.UnsetValue;
      }
      if (value is Color) {
        return new Pen(new SolidColorBrush((Color) value), 1);
      }
      return null;
    }
  }

  /// <xamlhelper/>
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class BoolConverter : IValueConverter
  {
    /// <xamlhelper/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(bool) value;

    /// <xamlhelper/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(bool) value;
  }
}
