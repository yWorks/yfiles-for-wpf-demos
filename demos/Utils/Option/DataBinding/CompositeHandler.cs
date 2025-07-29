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

using System.Collections.Generic;
using System.ComponentModel;
using Demo.yFiles.Option.Handler;

namespace Demo.yFiles.Option.DataBinding
{
  internal sealed class CompositeHandler<T>
  {
    private readonly ISelectionProvider<T> selection;

    //    private WeakReference optionItem;
    //    private WeakReference propertyItem;
    private readonly string virtualPropertyName;

    //    private string propertyPrefix;
    private bool inUpdate;

    private CompositeHandler(ISelectionProvider<T> selection, string virtualPropertyName, IOptionItem currentItem) {
      this.selection = selection;
      //      this.propertyPrefix = propertyPrefix;
      this.virtualPropertyName = virtualPropertyName;
      //      this.propertyItem = new WeakReference(propertyItem);
      //      this.optionItem = new WeakReference(currentItem);
      UpdateOptionItem(currentItem);
      currentItem.PropertyChanged += currentItem_PropertyChanged;
    }

    private void currentItem_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName.Equals("Value")) {
        UpdateSelection(((IOptionItem) sender).Value);
      }
    }

    public static CompositeHandler<T> Create(
        ISelectionProvider<T> selection,
        string virtualPropertyName,
        IOptionItem currentItem) {
      var handler = new CompositeHandler<T>(selection, virtualPropertyName, currentItem);
      return handler;
    }

    public void UpdateOptionItem(IOptionItem oi) {
      if (inUpdate) {
        return;
      }
      //      if (!optionItem.IsAlive) {
      //        return;
      //      }
      //
      //      IOptionItem oi = (IOptionItem)optionItem.Target;
      //lock...
      inUpdate = true;

      ICollection<IPropertyItemDescriptor<T>> descriptors;
      descriptors = selection.Selection;
      if (descriptors.Count == 0) {
        SetUndefinedValue(oi);
        inUpdate = false;
        return;
      }
      var filled = false;
      object accumulatedValue = null;

      foreach (var descriptor in descriptors) {
        // get the propertyItem from the current lookup

        var map = descriptor.Properties;
        if (map == null) {
          continue;
        }
        var item = map.GetEntry(virtualPropertyName);
        if (item == null) {
          continue;
        }
        // get value from current selection item
        var getter = item.Getter;
        if (getter == null || !getter.CanGet()) {
          continue;
        }

        var value = getter.GetValue();
        if (!filled) {
          // first value
          accumulatedValue = value;
          filled = true;
        } else {
          // check if value is different?
          var comparer = item.EqualityComparer;
          if (comparer != null) {
            if (!comparer.Equals(accumulatedValue, value)) {
              SetUndefinedValue(oi);
              inUpdate = false;
              return;
            }
          } else {
            if (accumulatedValue != null && !accumulatedValue.Equals(value)) {
              SetUndefinedValue(oi);
              inUpdate = false;
              return;
            }
            if (accumulatedValue == null && value != null) {
              SetUndefinedValue(oi);
              inUpdate = false;
              return;
            }
          }
        }
      }
      if (accumulatedValue == null) {
        if ((bool) oi.Attributes[OptionItem.SupportNullValueAttribute]) {
          oi.Value = accumulatedValue;
        }
      } else {
        oi.Value = accumulatedValue;
      }
      inUpdate = false;
    }


    public void SetUndefinedValue(IOptionItem item) {
      if ((bool) item.Attributes[OptionItem.SupportUndefinedValueAttribute]) {
        // this should not be reached
        item.Value = OptionItem.ValueUndefined;
      }
    }

    public void UpdateSelection(object newValue) {
      if (inUpdate) {
        return;
      }
      var value = newValue;
      if (value == OptionItem.ValueUndefined) {
        // no change
        return;
      }
      try {
        inUpdate = true;
        // lock selection for the duration of our update
        selection.BeginValueUpdate();

        ICollection<IPropertyItemDescriptor<T>> descriptors;
        descriptors = selection.Selection;

        var hasChanged = false;
        foreach (var descriptor in descriptors) {
          // get the propertyItem from the current lookup
          var map = descriptor.Properties;
          if (map == null) {
            continue;
          }

          // get value from current selection item
          var item = map.GetEntry(virtualPropertyName);
          var setter = item != null ? item.Setter : null;
          if (setter == null || !setter.CanSet()) {
            continue;
          }
          setter.SetValue(value);
          hasChanged = true;
        }

        if (hasChanged) {
          selection.UpdateSelectedItems();
        }
      } finally {
        selection.EndValueUpdate();
        inUpdate = false;
      }
    }
  }
}
