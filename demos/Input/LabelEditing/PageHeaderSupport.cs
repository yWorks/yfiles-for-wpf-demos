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

using System;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.Input.LabelEditing
{
  public class PageHeaderSupport
  {
    /// <summary>Creates the label for the page header.</summary>
    /// <remarks>
    /// The page header is a dummy labeled that is anchored at a fixed location. FreeLabelModel allows arbitrary positioning.
    /// In this case, we anchor the label to a dynamic point that always corresponds to the upper left corner of the viewport.
    /// We use the ZoomInvariantLabelStyle from the tutorial to ensure we always look the same
    /// </remarks>
    public static SimpleLabel CreatePageHeader(GraphControl graphControl) {
      var innerLabelStyle = new DefaultLabelStyle
      {
        Typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
        TextSize = 20,
        BackgroundPen = Pens.Green,
        TextBrush = Brushes.White,
        BackgroundBrush = Brushes.Green
      };
      var headerLabel = new SimpleLabel(null, "Page Header", FreeLabelModel.Instance.CreateAnchored(
          new DynamicViewPoint(graphControl, 5, 30), 0))
      {
        Style = new ZoomInvariantLabelStyle(innerLabelStyle)
      };
      // Adjust the size so that the text fits
      headerLabel.AdoptPreferredSizeFromStyle();

      // Since we don't have a model item for the label, we add the label's visual creator directly
      // to the scene graph
      var headerCanvasObject = graphControl.RootGroup.AddChild(new PageHeaderVisualCreator(headerLabel));

      var pageHeaderLabelEditHelper = new PageHeaderEditLabelHelper(headerLabel, headerCanvasObject);
      headerLabel.LookupImplementation = Lookups.Single<IEditLabelHelper>(pageHeaderLabelEditHelper);

      return headerLabel;
    }

    /// <summary>
    /// The <see cref="IEditLabelHelper"/> for the page header label.
    /// </summary>
    /// <remarks>
    /// This class sets a special configuration for the zoom-invariant page header label to the
    /// <see cref="TextEditorInputMode"/> and its text box, and restores their previous settings after editing.
    /// </remarks>
    public class PageHeaderEditLabelHelper : IEditLabelHelper
    {
      private readonly ILabel label;
      private readonly ICanvasObject labelCanvasObject;

      public PageHeaderEditLabelHelper(SimpleLabel label, ICanvasObject labelCanvasObject) {
        this.label = label;
        this.labelCanvasObject = labelCanvasObject;
      }

      public void OnLabelEditing(LabelEditingEventArgs args) {
        args.TextEditorInputModeConfigurator = ConfigureTextEditorInputMode;
        args.Label = label;
        args.Handled = true;
      }

      public void OnLabelAdding(LabelEditingEventArgs args) {
        args.Cancel = true;
      }

      private void ConfigureTextEditorInputMode(IInputModeContext context, TextEditorInputMode mode, ILabel label) {
        var oldBackground = mode.TextBox.Background;
        var oldVisual = labelCanvasObject.Visible;

        // We know that this label helper is only used once this demo is properly set up.
        var graphControl = ((GraphControl) context.CanvasControl);
        var graphEditorInputMode = (GraphEditorInputMode) graphControl.InputMode;
        var textEditorInputMode = ((LabelEditingDemo.DemoTextEditorInputMode) graphEditorInputMode.TextEditorInputMode);

        mode.TextBox.Background = Brushes.White;
        mode.TextBox.FontSize = 20;

        // Make sure that the text box location matches the one of the zoom invariant page header label
        textEditorInputMode.ShowInViewCoordinatesProperty = true;
        mode.Location = new DynamicViewPoint(graphControl, 5, 30);
        mode.Anchor = new PointD(1, 0);

        // Hide this label during editing if the cooresponding setting is enabled.
        // This is only necessary since this label is not part of the graph.
        if (graphEditorInputMode.HideLabelDuringEditing) {
          labelCanvasObject.Visible = false;
        }

        // Restore after editing
        EventHandler<TextEventArgs> afterEditing = null;
        afterEditing = delegate {
          textEditorInputMode.ShowInViewCoordinatesProperty = false;
          labelCanvasObject.Visible = oldVisual;
          mode.TextBox.Background = oldBackground;
          mode.TextEdited -= afterEditing;
          mode.EditingCanceled -= afterEditing;
        };
        mode.TextEdited += afterEditing;
        mode.EditingCanceled += afterEditing;
      }
    }

    private class PageHeaderVisualCreator : IVisualCreator
    {
      private readonly ILabel label;

      public PageHeaderVisualCreator(ILabel label) {
        this.label = label;
      }

      public Visual CreateVisual(IRenderContext context) {
        return label.Style.Renderer.GetVisualCreator(label, label.Style).CreateVisual(context);
      }

      public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        return label.Style.Renderer.GetVisualCreator(label, label.Style).UpdateVisual(context, oldVisual);
      }
    }

    /// <summary>
    /// Provides a live view of the upper left corner of the view point.
    /// </summary>
    private class DynamicViewPoint : IPoint
    {
      private readonly GraphControl graphControl;
      private readonly double dx;
      private readonly double dy;

      public DynamicViewPoint(GraphControl graphControl, double dx, double dy) {
        this.graphControl = graphControl;
        this.dx = dx;
        this.dy = dy;
      }

      /// <summary>
      /// Returns the upper left X coordinate of the view plus a fixed zoom invariant offset.
      /// </summary>
      public double X {
        get { return graphControl.ViewPoint.X + dx / graphControl.Zoom; }
      }

      /// <summary>.
      /// Returns the upper left Y coordinate of the view plus a fixed zoom invariant offset.
      /// </summary>
      public double Y {
        get { return graphControl.ViewPoint.Y + dy / graphControl.Zoom; }
      }
    }

  }
}