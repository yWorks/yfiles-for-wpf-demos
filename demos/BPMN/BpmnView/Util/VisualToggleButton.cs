/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using yWorks.Controls;

namespace Demo.yFiles.Graph.Bpmn.Util {

  /// <summary>
  /// A toggle button that uses different <see cref="Visual"/>s for the two toggle states.
  /// </summary>
  public class VisualToggleButton : ToggleButton {

    static VisualToggleButton()  {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualToggleButton), new FrameworkPropertyMetadata(typeof(VisualToggleButton)));
    }

    /// <summary>
    /// The visual used if the button is checked.
    /// </summary>
    public Visual CheckedVisual { get; set; }

    /// <summary>
    /// The visual used if the button is not checked.
    /// </summary>
    public Visual UncheckedVisual { get; set; }

    /// <summary>
    /// Called after the template has been applied.
    /// </summary>
    /// <remarks>
    /// This method sets up the necessary bindings with the <see cref="FrameworkElement.GetTemplateChild">named template child</see>
    /// called <c>"visualGroup"</c> which needs to be of type <see cref="VisualGroup"/>.
    /// </remarks>
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      UpdateChildren();
    }

    /// <summary>
    /// Called after the button state has been toggled.
    /// </summary>
    /// <remarks>
    /// This methods updates the <see cref="FrameworkElement.GetTemplateChild">named template child</see>
    /// called <c>"visualGroup"</c> which needs to be of type <see cref="VisualGroup"/> by using 
    /// <see cref="CheckedVisual"/> or <see cref="UncheckedVisual"/> as single child element.
    /// </remarks>
    protected override void OnToggle() {
      base.OnToggle();
      UpdateChildren();
    }

    private void UpdateChildren() {
      VisualGroup contentHost = GetTemplateChild("visualGroup") as VisualGroup;
      if (contentHost != null) {
        var oldVisual = contentHost.Children.Count == 1 ? contentHost.Children.ElementAt(0) : null;
        var newVisual = GetVisual();


        if (newVisual == null) {
          if (oldVisual != null) {
            contentHost.Children.Clear();
          }
        } else {
          if (oldVisual != null) {
            if (!newVisual.Equals(oldVisual)) {
              contentHost.Children[0] = newVisual;
            }
          } else {
            contentHost.Add(newVisual);
          }
        }
      }
    }

    /// <summary>
    /// Returns the visual used by <see cref="UpdateChildren"/>.
    /// </summary>
    /// <remarks>
    /// Depending on the the <see cref="ToggleButton.IsChecked">toggle state</see>, <see cref="CheckedVisual"/>
    /// or <see cref="UncheckedVisual"/> is returned.
    /// </remarks>
    /// <returns>The visual used by <see cref="UpdateChildren"/>.</returns>
    protected virtual Visual GetVisual() {
      return IsChecked.GetValueOrDefault() ? CheckedVisual : UncheckedVisual;
    }
  }
}
