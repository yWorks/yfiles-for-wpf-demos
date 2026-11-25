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

using System.Windows.Controls;

namespace yWorks.Demo
{
  /// <summary>
  ///   Represents a single step in a tutorial.
  /// </summary>
  public interface ITutorialStep
  {
    /// <summary>
    ///   Gets the display title of this step (used in the step combo-box).
    /// </summary>
    string Title { get; }

    /// <summary>
    ///   Gets the resource name suffix of the HTML description file for this step,
    ///   e.g. <c>"description.html"</c>. The file must be embedded in the tutorial
    ///   application assembly.
    /// </summary>
    string DescriptionSource { get; }

    /// <summary>
    ///   Called when this step becomes the active step.
    /// </summary>
    /// <param name="container">
    ///   The <see cref="ContentControl"/> that should be filled with this step's main view
    ///   (typically a new <see cref="yWorks.Controls.GraphControl"/>).
    /// </param>
    /// <param name="description">
    ///   The <see cref="Description"/> side-bar whose <see cref="Description.Source"/> will
    ///   already have been updated before this call. Steps that need HTML-button listeners can
    ///   register them through <see cref="TutorialStepBase.AddButtonListener"/>.
    /// </param>
    void Start(ContentControl container, Description description);

    /// <summary>
    ///   Called when this step is about to be replaced by another step.
    ///   All resources acquired in <see cref="Start"/> should be released here.
    /// </summary>
    /// <param name="container">The same container that was passed to <see cref="Start"/>.</param>
    /// <param name="description">The same description that was passed to <see cref="Start"/>.</param>
    void CleanUp(ContentControl container, Description description);
  }
}
