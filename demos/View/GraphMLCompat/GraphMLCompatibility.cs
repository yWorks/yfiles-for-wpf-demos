/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using System.Xml.Linq;
using Demo.yFiles.IO.GraphML.Compat;
using yWorks.Controls;
using yWorks.Graph.PortLocationModels;
using yWorks.GraphML;
using yWorks.Markup.Common;
using yWorks.Utils;

namespace Demo.yFiles.IO.GraphML.Compat
{
  /// <summary>
  ///   Helper class to enable parsing GraphML files from previous versions of yFiles WPF.
  /// </summary>
  /// <remarks>
  ///   For usage in your own applications, you can reference the demo and simply call
  ///   the <see cref="ConfigureGraphMLCompatibility"/> method with your <see cref="GraphMLIOHandler"/>
  ///   (or use the provided extension method on <see cref="GraphMLIOHandler"/>).
  /// </remarks>
  public static class GraphMLCompatibility
  {
    /// <summary>
    ///   Configures a <see cref="GraphMLIOHandler"/> instance to read GraphML files written by
    ///   previous versions of yFiles WPF.
    /// </summary>
    public static void ConfigureGraphMLCompatibility(GraphMLIOHandler handler) {
      // Register our three special namespaces containing MarkupExtensions that can then just be picked up by namespace
      // and name and do not have to be mapped manually (see below).
      // These mappings are akin to the assembly attributes at the top of this file.
      handler.AddXamlNamespaceMapping(YfilesCommonNS30, "Demo.yFiles.IO.GraphML.Compat.LabelModels",
          typeof(GraphMLCompatibility).Assembly);
      handler.AddXamlNamespaceMapping(YfilesCommonNS30, "Demo.yFiles.IO.GraphML.Compat.PortLocationModels",
          typeof(GraphMLCompatibility).Assembly);
      handler.AddXamlNamespaceMapping(YfilesCommonNS30, "Demo.yFiles.IO.GraphML.Compat.Table",
          typeof(GraphMLCompatibility).Assembly);
      handler.AddXamlNamespaceMapping(YfilesWpfXamlNS30, "Demo.yFiles.IO.GraphML.Compat.Styles",
          typeof(GraphMLCompatibility).Assembly);

      // Provides a mapping between XML element names in specific yFiles WPF 3.6 namespaces
      // to either their equivalent in the current library version, or to specially-written
      // MarkupExtensions that surface the old API and map it to the correct instances in the new API.
      var mappings = new Dictionary<XName, Type>() {
          // Label models
          {
              XName.Get("AnchoredLabelModelParameter", YfilesCommonNS30),
              typeof(FreeLabelModelAnchoredParameterExtension)
          },
          { XName.Get("FixedLabelModelParameter", YfilesCommonNS30), typeof(FreeLabelModelAbsoluteParameterExtension) }, {
              XName.Get("RatioAnchoredLabelModelParameter", YfilesCommonNS30),
              typeof(FreeNodeLabelModelParameterExtension)
          }, {
              XName.Get("PortAnchoredLabelModelParameter", YfilesCommonNS30),
              typeof(FreePortLabelModelParameterExtension)
          },
          // Port location models
          { XName.Get("PathRatioParameter", YfilesCommonNS30), typeof(EdgePathPortLocationModelParameterExtension) },
          { XName.Get("SegmentRatioPortLocationModel", YfilesCommonNS30), typeof(EdgeSegmentPortLocationModel) }, {
              XName.Get("BendAnchoredParameter", YfilesCommonNS30),
              typeof(BendAnchoredPortLocationModelParameterExtension)
          },
      };

      // Collect all type names for which we provide MarkupExtensions in this demo:
      var blacklistedCommonNames = new HashSet<string>();
      foreach (var type in typeof(GraphMLCompatibility).Assembly.GetTypes().Where(t =>
          t.IsPublic && (t.Namespace == "Demo.yFiles.IO.GraphML.Compat.LabelModels" ||
              t.Namespace == "Demo.yFiles.IO.GraphML.Compat.PortLocationModels" ||
              t.Namespace == "Demo.yFiles.IO.GraphML.Compat.Table"))) {
        var typeName = type.Name;
        blacklistedCommonNames.Add(typeName);
        if (typeName.EndsWith("Extension")) {
          blacklistedCommonNames.Add(typeName.Substring(0, typeName.Length - "Extension".Length));
        }
      }

      var blacklistedWPFNames = new HashSet<string>();
      foreach (var type in typeof(GraphMLCompatibility).Assembly.GetTypes().Where(t =>
          t.IsPublic && t.Namespace == "Demo.yFiles.IO.GraphML.Compat.Styles")) {
        var typeName = type.Name;
        blacklistedWPFNames.Add(typeName);
        if (typeName.EndsWith("Extension")) {
          blacklistedWPFNames.Add(typeName.Substring(0, typeName.Length - "Extension".Length));
        }
      }

      foreach (var mappingsKey in mappings.Keys.Where(mappingsKey => mappingsKey.NamespaceName == YfilesWpfXamlNS30)) {
        blacklistedWPFNames.Add(mappingsKey.LocalName);
        blacklistedWPFNames.Add(mappingsKey.LocalName + "Extension");
      }

      handler.QueryType += (sender, args) => {
        Type result;
        var argsXmlName = args.XmlName;
        var tagName = argsXmlName.LocalName;
        var namespaceName = argsXmlName.NamespaceName;

        if (mappings.TryGetValue(argsXmlName, out result)) {
          // If we have specified a hard remapping, just use that
          args.Result = result;
        }
        // Intelligent remapping for the common namespace.
        // This is complicated by the fact that some types have remained in the old 3.0 namespace, so we need
        // to query both the 4.0 and the 3.0 namespace
        if (!args.Context.GetDeserializationProperty(RequeryLock) && namespaceName == YfilesCommonNS30 &&
            !blacklistedCommonNames.Contains(tagName)) {
          // If we have not specified a hard remapping and it is not one of the types where we provide compat extensions:
          var nameMapper = args.Context.SafeLookup<IXamlNameMapper>();

          // Make sure we do not ask ourselves again
          var childContext = new ChildParseContext(args.Context);
          childContext.SetDeserializationProperty(RequeryLock, true);

          Type type;
          if (tagName.EndsWith("Extension")) {
            // If we explicitly request an extension under the "old" namespace, first check whether there is a type in
            // the 4.0 and then in the 3.0 namespace
            type = nameMapper.GetType(childContext, XName.Get(tagName, XamlNamespaceConstants.YfilesCommonNS)) ??
                nameMapper.GetType(childContext, argsXmlName);
          } else {
            // Otherwise: Query extension in 4.0, then 3.0, then bare typename in 4.0, then 3.0
            type =
                nameMapper.GetType(childContext,
                    XName.Get(tagName + "Extension", XamlNamespaceConstants.YfilesCommonNS)) ??
                nameMapper.GetType(childContext, XName.Get(tagName + "Extension", YfilesCommonNS30)) ??
                nameMapper.GetType(childContext, XName.Get(tagName, XamlNamespaceConstants.YfilesCommonNS)) ??
                nameMapper.GetType(childContext, argsXmlName);
          }
          args.Result = type;
        }
        // Intelligent remapping for the WPF Xaml namespace.
        if (!args.Context.GetDeserializationProperty(RequeryLock) && namespaceName == YfilesWpfXamlNS30 &&
            !blacklistedWPFNames.Contains(tagName)) {
          // If we have not specified a hard remapping and it is not one of the types where we provide compat extensions:
          var nameMapper = args.Context.SafeLookup<IXamlNameMapper>();

          // Make sure we do not ask ourselves again
          var childContext = new ChildParseContext(args.Context);
          childContext.SetDeserializationProperty(RequeryLock, true);

          Type type;
          if (tagName.EndsWith("Extension")) {
            // If we explicitly request an extension under the "old" namespace, check whether there is a type in
            // the 4.0 namespace
            type = nameMapper.GetType(childContext, XName.Get(tagName, XamlNamespaceConstants.YfilesWpfXamlNS));
          } else {
            // Otherwise: Query extension in 4.0, then bare typename in 4.0
            type = nameMapper.GetType(childContext,
                    XName.Get(tagName + "Extension", XamlNamespaceConstants.YfilesWpfXamlNS)) ??
                nameMapper.GetType(childContext, XName.Get(tagName, XamlNamespaceConstants.YfilesWpfXamlNS));
          }
          args.Result = type;
        }
      };
    }

    public const string YfilesWpfXamlNS30 = "http://www.yworks.com/xml/yfiles-wpf/3.0/xaml";

    public const string YfilesCommonNS30 = "http://www.yworks.com/xml/yfiles-common/3.0";

    public static readonly TypedKey<bool> RequeryLock =
        new TypedKey<bool>(typeof(GraphMLCompatibility).FullName + "RequeryLock", false);
  }
}

namespace yWorks.GraphML
{
  /// <summary>
  ///   Contains an extension method on <see cref="GraphMLIOHandler"/> to allow parsing GraphML files from earlier
  ///   versions of yFiles WPF.
  /// </summary>
  public static class GraphMLCompatibilityExtensions
  {
    /// <summary>
    ///   Configures a <see cref="GraphMLIOHandler"/> instance to read GraphML files written by
    ///   previous versions of yFiles WPF.
    /// </summary>
    public static void ConfigureGraphMLCompatibility(this GraphMLIOHandler handler) {
      GraphMLCompatibility.ConfigureGraphMLCompatibility(handler);
    }
  }
}
