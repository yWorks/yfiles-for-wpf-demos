/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Collections.Generic;
using System.Resources;
using Demo.yFiles.Option.Handler;

namespace Demo.yFiles.Option.I18N
{
  /// <summary>
  /// Interface for classes that support localization of OptionHandler components.
  /// </summary>
  public interface I18NFactory
  {
    /// <summary>
    /// Return the localized string for the given <paramref name="key"/> in the
    /// specified <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The context where the localization is searched. This allows
    /// for duplicate <paramref name="key"/> values in different contexts</param>
    /// <param name="key">The key value for which a localization is needed.</param>
    /// <returns>A localized string for the given <paramref name="key"/></returns>
    string GetString(string context, string key);
  }

  /// <summary>
  /// Wraps a .NET <see cref="ResourceManager"/> into a <see cref="I18NFactory"/> instance.
  /// </summary>
  /// <remarks><note>
  /// Most support classes like <b>YModule</b> handle the registering of
  /// the resources themselves, please refer to the documentation for these classes for more details.</note></remarks>
  /// <example> This sample shows how associate a standard .NET resource bundle to an option handler.
  /// <code lang="C#">
  ///   class ConstraintDemo 
  ///   {
  ///      public static int Main() 
  ///      {
  ///         oh = new OptionHandler("SETTINGS");
  ///         //add various simple items directly to the handler            
  ///         oh.AddString(null, "STRING", "foo");
  ///         oh.AddInt(null, "INT", 3);
  ///         //add nested items
  ///         OptionGroup g = new OptionGroup("GROUP");
  ///         g.AddOptionItem(new GenericOptionItem&lt;Color>("COLOR"));
  ///         oh.AddOptionItem(g);
  ///         //localization is supported for most aspects, usually you only need to register a .NET
  ///         //ResourceManager to your OptionHandler
  ///         ResourceManager rm =
  ///             new ResourceManager("Demo.Option.OptionHandler.OptionHandlerDemoI18N",
  ///                                  Assembly.GetExecutingAssembly());
  ///         ResourceManagerI18NFactory rmf = new ResourceManagerI18NFactory();
  ///         // bind the resource manager to this specific option handler
  ///         rmf.AddResourceManager(oh.Name, rm);
  ///         //  set the factory
  ///         oh.I18nFactory = rmf;  
  ///       }
  ///    }
  /// </code>
  /// The framework constructs lookup keys from the full path name of an option item, i.e. 
  /// <c>HandlerName.Group1Name....GroupNName.ItemName</c>,
  /// so this example uses a resource file like:
  /// <code>
  /// SETTINGS="Settings"
  /// SETTINGS.STRING="String"
  /// SETTINGS.INT="Int"
  /// SETTINGS.GROUP="Group"
  /// SETTINGS.GROUP.COLOR="Color"
  /// </code>
  /// </example>
  public class ResourceManagerI18NFactory : I18NFactory
  {
    private Dictionary<string, IList<ResourceManager>> resources;

    /// <summary>
    /// Create a new factory.
    /// </summary>
    public ResourceManagerI18NFactory() {
      resources = new Dictionary<string, IList<ResourceManager>>();
    }

    /// <summary>
    /// Add a ResourceManager to the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The context where this ResourceManager is bound to. Typically
    /// the name of an OptionHandler for which the ResourceManaer is responsible</param>
    /// <param name="manager">The new resource manager</param>
    public void AddResourceManager(string context, ResourceManager manager) {
      //retrieve list
      IList<ResourceManager> resList;

      resources.TryGetValue(context, out resList);
      if (resList == null) {
        resList = new List<ResourceManager>(1);
        resources[context] = resList;
      }
      resList.Add(manager);
    }

    /// <summary>
    /// Return the localized string for the given <paramref name="key"/> in the
    /// specified <paramref name="context"/>.
    /// </summary>
    /// <remarks>The factory first tries to get the value from the <see cref="ResourceManager"/>
    /// that is bound to the context <paramref name="context"/>. If this fails, it tries the same lookup without the leading context, if this also fails,
    /// it tries to return values for several well known keys (for button descriptions etc.).
    /// If this also fails, it returns the unmodified <paramref name="key"/>.</remarks>
    /// <param name="context">The context where the localization is searched. This allows
    /// for duplicate <paramref name="key"/> values in different contexts</param>
    /// <param name="key">The key value for which a localization is needed.</param>
    /// <returns>A localized string for the given <paramref name="key"/></returns>
    public string GetString(string context, string key) {
      string retval = null;

      //first try to find string in specific context
      IList<ResourceManager> rmsForContext;
      if (context != null && resources.TryGetValue(context, out rmsForContext)) {
        foreach (ResourceManager manager in rmsForContext) {
          try {
            retval = manager.GetString(key);
            if (retval != null) {
              return retval;
            }
          } catch
            (MissingManifestResourceException) {
            //just skip this exception
          }
        }
      }
      if (retval == null && context != null && context != key && key.StartsWith(context)) {
        string str = key.Substring(context.Length+1);
        //prefix with context...
        if (resources.TryGetValue(context, out rmsForContext)) {
          foreach (ResourceManager manager in rmsForContext) {
            try {
              retval = manager.GetString(str);
              if (retval != null) {
                return retval;
              }
            } catch
              (MissingManifestResourceException) {
              //just skip this exception
            }
          }
        }
      }
      //last resort
//      retval = OptionHandler.FallBackI18NFactory.GetString(context, key);
      return retval == null ? key : retval;
    }
  }
}
