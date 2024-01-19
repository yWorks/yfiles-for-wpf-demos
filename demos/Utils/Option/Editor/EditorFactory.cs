/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.I18N;
using Demo.yFiles.Option.View;
using System.Diagnostics;

namespace Demo.yFiles.Option.Editor
{

  public sealed class TablePanel : Panel
  {

    static TablePanel() {
      UIElement.FocusableProperty.OverrideMetadata(typeof(TablePanel), new FrameworkPropertyMetadata(false));
    }

    private readonly List<double> minWidths = new List<double>(){0,0};
    private readonly List<double> currentWidths = new List<double>(){0,0};

    public TablePanel() {
      HorizontalAlignment = HorizontalAlignment.Stretch;
      VerticalAlignment = VerticalAlignment.Top;
    }

    public IList<double> MinWidths {
      get { return minWidths; }
    }

    public IList<double> CurrentWidths {
      get { return currentWidths; }
    }

    protected override Size MeasureOverride(Size availableSize) {
      var oldWidths = minWidths.ToArray();
      for (int index = 0; index < minWidths.Count; index++) {
        minWidths[index] = -1;
      }

      double minHeight = 0;
      double minWidth = MinWidth;
      foreach (UIElement c in new ArrayList(Children)) {
        c.Measure(availableSize);
        minHeight += c.DesiredSize.Height;
        minWidth = Math.Max(minWidth, c.DesiredSize.Width);
      }

      double columnWidths = 0;

      for (int index = 0; index < minWidths.Count; index++) {
        if (minWidths[index] < 0) {
          minWidths[index] = oldWidths[index];
        }
        columnWidths += minWidths[index];
      }

      if (columnWidths < minWidth) {
        minWidths[minWidths.Count - 1] += (minWidth - columnWidths);
      }
      minWidth = Math.Max(minWidth, columnWidths);

      return new Size(minWidth, Math.Max(MinHeight, minHeight));
    }

    protected override Size ArrangeOverride(Size finalSize) {

      double columnWidths = minWidths.Sum();

      for (int i = 0; i < minWidths.Count; i++) {
        currentWidths[i] = minWidths[i];
      }

      if (columnWidths < finalSize.Width) {
        currentWidths[currentWidths.Count - 1] += finalSize.Width - columnWidths;
      }

      double minHeight = 0;
      foreach (UIElement child in Children) {
        var height = child.DesiredSize.Height;
        child.Arrange(new Rect(0, minHeight, finalSize.Width, height));
        minHeight += height;
      }
      return new Size(finalSize.Width, minHeight);
    }
  }

  public class RowPanel : Panel
  {

    static RowPanel() {
      UIElement.FocusableProperty.OverrideMetadata(typeof(RowPanel), new FrameworkPropertyMetadata(false));
    }


    public RowPanel() {
      HorizontalAlignment = HorizontalAlignment.Stretch;
    }




    protected override Size MeasureOverride(Size availableSize) {
      TablePanel tablePanel = this.GetVisualAncestors().OfType<TablePanel>().FirstOrDefault();

      double minHeight = MinHeight;
      double minWidth = 0;

      int columnCount = 0;
      foreach (UIElement child in Children) {
        child.Measure(availableSize);
        var width = child.DesiredSize.Width;
        if (tablePanel != null && tablePanel.MinWidths.Count > columnCount) {
          tablePanel.MinWidths[columnCount] = Math.Max(tablePanel.MinWidths[columnCount], width);
        }
        minWidth += width;
        minHeight = Math.Max(minHeight, child.DesiredSize.Height);
        columnCount++;
      }
      return new Size(Math.Max(minWidth, MinWidth), minHeight);
    }

    private IEnumerable<Visual> GetVisualAncestors() {
      Visual v = this;
      v = VisualTreeHelper.GetParent(v) as Visual;
      while (v != null) {
        yield return v;
        v = VisualTreeHelper.GetParent(v) as Visual;
      }
    }

    protected override Size ArrangeOverride(Size finalSize) {
      TablePanel tablePanel = this.GetVisualAncestors().OfType<TablePanel>().FirstOrDefault();
      double minWidth = 0;
      // TODO - use ColumnMinWidth / quantify
      int columnCount = 0;
      var widths = tablePanel != null ? tablePanel.CurrentWidths : null;
      foreach (UIElement child in Children) {
        var width = child.DesiredSize.Width;
        if (widths != null && widths.Count > columnCount) {
          width = widths[columnCount];
        }
        var newWidth = Math.Max(0, Math.Min(finalSize.Width - minWidth, width));
        child.Arrange(new Rect(minWidth, 0, newWidth, finalSize.Height));
        minWidth += newWidth;
        columnCount++;
      }
      return new Size(minWidth, finalSize.Height);
    }
  }

  /// <summary>
  /// Abstract base class for factories that can create visual representations of OptionHandlers
  /// </summary>
  /// <remarks>This class can either create a complete standalone editor form together with the
  /// necessary buttons for applying/adopting/canceling, or just the bare core
  /// <see cref="EditorControl"/> for the OptionHandler. An instance of <see cref="IModelView"/>
  /// is created implicitly to control the synchronization and optionally ensure
  /// that the <see cref="ConstraintManager"/> is correctly registered with the editor.</remarks>
  /// <example> This sample shows how create a standalone editor form for a simple option handler
  /// that is automatic synchronization state.
  /// <code lang="C#">
  ///   class ConstraintDemo 
  ///   {
  ///      public static int Main() 
  ///      {
  ///         oh = new OptionHandler("Settings);
  ///         //add various simple items directly to the handler            
  ///         IOptionItem stringItem = oh.AddString(null, "String", "foo");
  ///         oh.AddInt(null, "Int", 3);
  ///         
  ///         //this will show a PropertyGrid like editor
  ///         //synchronization properties can be set
  ///         EditorForm form = (new TableEditorFactory()).CreateEditor(oh, true, true);
  ///         //use the editor as modal dialog
  ///         if (form.ShowDialog() == true){
  ///            MessageBox.Show("Dialog has been accepted");
  ///         }  else {
  ///            MessageBox.Show("Dialog has been canceled");
  ///         }  
  ///       }
  ///    }
  /// </code>
  /// </example>
  public abstract class EditorFactory
  {
  }
}
