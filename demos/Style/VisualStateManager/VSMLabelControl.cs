/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.VisualStateManager
{
  [TemplateVisualState(Name = "ItemSelected", GroupName = "ItemSelectionStates")]
  [TemplateVisualStateAttribute(Name = "ItemUnselected", GroupName = "ItemSelectionStates")]
  [TemplateVisualStateAttribute(Name = "ItemHighlighted", GroupName = "ItemHighlightStates")]
  [TemplateVisualStateAttribute(Name = "ItemUnhighlighted", GroupName = "ItemHighlightStates")]
  [TemplateVisualStateAttribute(Name = "ItemFocused", GroupName = "ItemFocusStates")]
  [TemplateVisualStateAttribute(Name = "ItemUnfocused", GroupName = "ItemFocusStates")]
  [TemplateVisualStateAttribute(Name = "MouseOver", GroupName = "MouseStates")]
  [TemplateVisualStateAttribute(Name = "MouseOutside", GroupName = "MouseStates")]
  [TemplateVisualStateAttribute(Name = "NormalOrientation", GroupName = "OrientationStates")]
  [TemplateVisualStateAttribute(Name = "FlippedOrientation", GroupName = "OrientationStates")]
  [TemplateVisualStateAttribute(Name = "UpsideDown", GroupName = "OrientationStates")]
  public class VSMLabelControl : LabelControl
  {
    private bool loaded;
    private bool mouseInside;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphItemControlBase&lt;TModelItem&gt;"/> class.
    /// </summary>
    public VSMLabelControl() {
      this.Loaded += OnLoaded;
    }

    /// <summary>
    /// Overridden to update the <see cref="VisualState">MouseStates</see> accordingly.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected override void OnMouseLeave(MouseEventArgs e) {
      mouseInside = false;
      UpdateMouseState(true);
    }

    /// <summary>
    /// Overridden to update the <see cref="VisualState">MouseStates</see> accordingly.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected override void OnMouseEnter(MouseEventArgs e) {
      mouseInside = true;
      UpdateMouseState(true);
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
      this.Loaded -= OnLoaded;
      loaded = true;
      UpdateSelectionState(ItemSelected, false);
      UpdateFocusState(ItemFocused, false);
      UpdateHighlightState(ItemHighlighted, false);
      UpdateMouseState(false);
    }

    private void UpdateMouseState(bool useTransitions) {
      System.Windows.VisualStateManager.GoToState(this, mouseInside ? "MouseOver" : "MouseOutside", useTransitions && loaded);
    }

    private void UpdateSelectionState(bool selected, bool useTransitions) {
      System.Windows.VisualStateManager.GoToState(this, selected ? "ItemSelected" : "ItemUnselected", useTransitions && loaded);
    }

    private void UpdateFocusState(bool focused, bool useTransitions) {
      System.Windows.VisualStateManager.GoToState(this, focused ? "ItemFocused" : "ItemUnfocused", useTransitions && loaded);
    }

    private void UpdateHighlightState(bool highlighted, bool useTransitions) {
      System.Windows.VisualStateManager.GoToState(this, highlighted ? "ItemHighlighted" : "ItemUnhighlighted", useTransitions && loaded);
    }

    private void UpdateOrientationState(bool useTransitions) {
      System.Windows.VisualStateManager.GoToState(this, IsFlipped ? "FlippedOrientation" : IsUpsideDown ? "UpsideDown" : "NormalOrientation", useTransitions && loaded);
    }

    protected override void OnItemSelectedChanged(bool oldValue, bool newValue) {
      UpdateSelectionState(newValue, true);
      base.OnItemSelectedChanged(oldValue, newValue);
    }

    protected override void OnItemFocusedChanged(bool oldValue, bool newValue) {
      UpdateFocusState(newValue, true);
      base.OnItemFocusedChanged(oldValue, newValue);
    }

    protected override void OnItemHighlightedChanged(bool oldValue, bool newValue) {
      UpdateHighlightState(newValue, true);
      base.OnItemHighlightedChanged(oldValue, newValue);
    }

    public override void InitializeFrom(IRenderContext context, ILabel label, LabelControlLabelStyle style) {
      base.InitializeFrom(context, label, style);
      UpdateOrientationState(false);
    }

    public override void UpdateFor(IRenderContext context, ILabel label, LabelControlLabelStyle style) {
      base.UpdateFor(context, label, style);
      UpdateOrientationState(true);
    }
  }
}
