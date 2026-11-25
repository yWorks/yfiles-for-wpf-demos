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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

[assembly: XmlnsDefinition("http://www.yworks.com/yfiles-wpf/2.1/demos/GraphEditor/Styles", "yWorks.Demo.GraphViewer")]
[assembly: XmlnsPrefix("http://www.yworks.com/yfiles-wpf/2.1/demos/GraphEditor/Styles", "yed")]

namespace yWorks.Demo.GraphViewer
{
  public class VectorNodeStyle : NodeStyleBase<VisualGroup>
  {
    public string? TemplateKey { get; set; }

    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      var container = new VisualGroup();
      Render(container, node);
      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      if (oldVisual.Transform is MatrixTransform transform) {
        var layout = node.Layout.ToRectD();
        if (transform.Matrix != Matrix.Identity) {
          transform.Matrix = new Matrix(layout.Width, 0, 0, layout.Height, layout.X, layout.Y);
          return oldVisual;
        }
        oldVisual.Transform = new MatrixTransform(layout.Width, 0, 0, layout.Height, layout.X, layout.Y);
        return oldVisual;
      }
      return CreateVisual(context, node);
    }

    private void Render(VisualGroup container, INode node) {
      var layout = node.Layout.ToRectD();
      if (TemplateKey != null) {
        var template = FindTemplate(TemplateKey);

        if (template != null) {
          if (template.LoadContent() is Canvas child) {
            double oldScaleX = 1, oldScaleY = 1;

            if (child.RenderTransform is ScaleTransform oldTransform) {
              oldScaleX = oldTransform.ScaleX;
              oldScaleY = oldTransform.ScaleY;
            } else {
              child.RenderTransform = oldTransform = new ScaleTransform {
                  ScaleX = 1,
                  ScaleY = 1
              };
            }
            var width = child.Width;
            var height = child.Height;

            var scaleX = 1 / (width / oldScaleX);
            var scaleY = 1 / (height / oldScaleY);

            container.Add(child);

            if (oldTransform.IsFrozen) {
              oldTransform = oldTransform.CloneCurrentValue();
              child.RenderTransform = oldTransform;
            }
            oldTransform.ScaleX = scaleX;
            oldTransform.ScaleY = scaleY;
          }
          container.Transform =
              new MatrixTransform { Matrix = new Matrix(layout.Width, 0, 0, layout.Height, layout.X, layout.Y) };
        }
      }
    }

    private static DataTemplate? FindTemplate(string templateKey) {
      var sources = new[] {
          "pack://application:,,,/GraphViewer;component/Resources/Icons_Computer.xaml",
          "pack://application:,,,/GraphViewer;component/Resources/Icons_Movies.xaml",
          "pack://application:,,,/GraphViewer;component/Resources/Icons_People.xaml"
      };

      foreach (var source in sources) {
        var dictionary = new ResourceDictionary { Source = new Uri(source, UriKind.Absolute) };

        if (dictionary.Contains(templateKey)) {
          return dictionary[templateKey] as DataTemplate;
        }
      }

      return null;
    }
  }
}
