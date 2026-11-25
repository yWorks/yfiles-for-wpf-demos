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
using System.Runtime.CompilerServices;

namespace yWorks.Demo
{
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class DemoAttribute : Attribute
  {
    public DemoAttribute(string id, string category, string title, [CallerFilePath] string? path = null) {
      Id = id;
      Category = category;
      Title = title;
      DemoPath = ExtractRelativePath(path);
    }

    private static string? ExtractRelativePath(string? path) {
      if (path == null) {
        return null;
      }
      const string demosPathPart = @"\demos\";
      var demosIndex = path.IndexOf(demosPathPart);
      var lastBackslashIndex = path.LastIndexOf('\\');
      if (demosIndex is -1 || lastBackslashIndex is -1) {
        return null;
      }
      return path.Substring(demosIndex + demosPathPart.Length, lastBackslashIndex - demosIndex - demosPathPart.Length);
    }

    /// <summary>
    ///   Gets or sets the demo's internal ID.
    /// </summary>
    /// <remarks>
    ///   This is reused for <see cref="RelatedDemos"/>.
    /// </remarks>
    public string Id { get; set; }

    /// <summary>
    ///   Gets or sets the demo's category.
    /// </summary>
    /// <remarks>
    ///   Demos in the same category are grouped together.
    /// </remarks>
    public string Category { get; set; }

    /// <summary>
    ///   Gets or sets the title of the demo.
    /// </summary>
    /// <remarks>
    ///   This is the name shown in the demo overview or the window title.
    /// </remarks>
    public string Title { get; set; }

    /// <summary>
    ///   Gets or sets a short summary of the demo.
    /// </summary>
    /// <remarks>
    ///   This is shown in the demo overview.
    /// </remarks>
    public string? Summary { get; set; } = null;

    /// <summary>
    ///   Gets or sets a list of tags for the demo.
    /// </summary>
    /// <remarks>
    ///   Tags can be searched for and are shown in the demo overview.
    /// </remarks>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    ///   Gets or sets a list of keywords for the demo.
    /// </summary>
    /// <remarks>
    ///   Keywords can be searched for but are not displayed in the demo overview.
    /// </remarks>
    public string[] Keywords { get; set; } = Array.Empty<string>();

    /// <summary>
    ///   Gets or sets a list of related demo IDs.
    /// </summary>
    public string[]? RelatedDemos { get; set; } = Array.Empty<string>();

    /// <summary>
    ///   Gets the relative path for the demo within the yFiles WPF package.
    /// </summary>
    public string? DemoPath { get; }
  }
}
