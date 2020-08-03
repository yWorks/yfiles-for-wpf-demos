/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Diagnostics;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Constraint
{
  /// <summary>
  /// Abstract implementation of <see cref="ICondition"/> that provides
  /// the basic infrastructure for correct installation of this condition in 
  /// a <see cref="IModelView"/>. 
  /// </summary>
  public abstract class ConditionBase : ICondition
  {
    private List<INotifyPropertyChanged> inputs;

    /// <summary>
    /// Create a new instance for the given list of <paramref name="inputs"/>
    /// </summary>
    /// <param name="inputs">The initial inputs for this condition</param>
    protected ConditionBase(List<INotifyPropertyChanged> inputs) {
      foreach (var item in inputs) {
        if (item == null) {
          throw new ArgumentNullException();
        }
      }
      this.inputs = inputs;
    }

    /// <summary>
    /// Create a new instance with an empty input list.
    /// </summary>
    protected ConditionBase() {
      inputs = new List<INotifyPropertyChanged>();
    }

    /// <summary>
    /// Create a new instance for the given array of <paramref name="inputs"/>
    /// </summary>
    /// <param name="inputs">The initial inputs for this condition</param>
    protected ConditionBase(params INotifyPropertyChanged[] inputs) {
      foreach (var item in inputs) {
        if (item == null) {
          throw new ArgumentNullException();
        }
      }
      this.inputs = new List<INotifyPropertyChanged>(inputs);
    }

    /// <inheritdoc/>
    public virtual void AddInput(INotifyPropertyChanged item) {
      if (item != null) {
        inputs.Add(item);
      } else {
        throw new ArgumentNullException();
      }
    }

    /// <inheritdoc/>
    public abstract bool IsTrue { get; }

    /// <inheritdoc/>
    public virtual void RemoveInput(INotifyPropertyChanged item) {
      inputs.Remove(item);
    }

    /// <inheritdoc/>
    public ReadOnlyCollection<INotifyPropertyChanged> Inputs {
      get { return inputs.AsReadOnly(); }
    }

    #region ICondition Members

    /// <summary>
    ///This method provides ensures correct installation of this condition in 
    /// a <see cref="IModelView"/>. 
    /// </summary>
    public virtual ICondition InstallInView(IModelView view) {
      IList<INotifyPropertyChanged> copiedItems = new List<INotifyPropertyChanged>();
      foreach (INotifyPropertyChanged item in inputs) {
        //get corresponding item in view
        INotifyPropertyChanged viewItem = GetItemCopy(view, item);
        if (viewItem != null) {
          copiedItems.Add(viewItem);
        }
      }
      INotifyPropertyChanged[] cp = new INotifyPropertyChanged[copiedItems.Count];
      copiedItems.CopyTo(cp, 0);
      ICondition copy = CreateCopy(cp);
      foreach (INotifyPropertyChanged input in copiedItems) {
        input.PropertyChanged += copy.SourceValueChangedHandler;
      }
      return copy;
    }

    /// <summary>
    /// This method is needed to find the correct item in <paramref name="view"/> that corresponds
    /// to the original input source <paramref name="item"/>. It is called by 
    /// <see cref="InstallInView"/> to correctly determine the new event inputs for the copied
    /// condition.
    /// </summary>
    /// <remarks>This implementation just queries <see cref="IModelView.GetViewItem"/> for
    /// <paramref name="item"/>, and returns the result from this call, or <see langword="null"/>
    /// if <paramref name="item"/> does not implement <see cref="IOptionItem"/>.</remarks>
    /// <param name="view">The view where the condition is currently installed</param>
    /// <param name="item"></param>
    /// <returns>A <see cref="INotifyPropertyChanged"/> instance that corresponds
    /// to <paramref name="item"/> in <paramref name="view"/>.</returns>
    protected virtual INotifyPropertyChanged GetItemCopy(IModelView view, INotifyPropertyChanged item) {
      IOptionItem viewItem = item as IOptionItem;
      if (viewItem != null) {
        return view.GetViewItem(viewItem);
      }
      return null;
    }

    /// <summary>
    /// Called <see cref="ConditionTriggered"/> whenever a source input value
    /// raises a <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <remarks>
    /// This event handler delegates its work to <see cref="OnConditionSuccessful"/> 
    /// or <see cref="OnConditionFailed"/>, depending on whether <see cref="IsTrue"/> 
    /// returns <see langword="true"/>
    /// resp. <see langword="false"/>.
    /// </remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public virtual void SourceValueChangedHandler(object sender, PropertyChangedEventArgs e) {
      if (IsTrue) {
        OnConditionSuccessful();
      } else {
        OnConditionFailed();
      }
    }

    /// <inheritdoc/>
    public event ConditionTriggeredEventHandler ConditionTriggered;

    /// <summary>
    /// Should be called to trigger the successful result event of this condition
    /// </summary>
    /// <remarks>This method raises <see cref="ConditionTriggered"/> with 
    /// status <see langword="true"/>. Additionally, a <see cref="PropertyChanged"/>
    /// event is raised for the property <see cref="IsTrue"/></remarks>
    protected virtual void OnConditionSuccessful() {
      //if there are subscribers to this event
      if (PropertyChanged != null) {
        //fire the event
        PropertyChanged(this, new PropertyChangedEventArgs("IsTrue"));
      }
      if (ConditionTriggered != null) {
        ConditionTriggered(this, new ConditionTriggeredEventArgs(true));
      }
    }

    /// <summary>
    /// Should be called to trigger the failed result event of this condition
    /// </summary>
    /// <remarks>This method raises <see cref="ConditionTriggered"/> with 
    /// status <see langword="false"/>. Additionally, a <see cref="PropertyChanged"/>
    /// event is raised for the property <see cref="IsTrue"/></remarks>
    protected virtual void OnConditionFailed() {
      //if there are subscribers to this event
      if (PropertyChanged != null) {
        //fire the event
        PropertyChanged(this, new PropertyChangedEventArgs("IsTrue"));
      }
      if (ConditionTriggered != null) {
        ConditionTriggered(this, new ConditionTriggeredEventArgs(false));
      }
    }

    #endregion

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public virtual void Dispose() {
//      Trace.WriteLine("Disposing condition");
      //disconnect from source
      foreach (INotifyPropertyChanged item in inputs) {
        item.PropertyChanged -= SourceValueChangedHandler;
      }
      inputs.Clear();
    }

    /// <summary>
    /// Create a new copy of this condition for the correct type of the condition, but
    /// with the copied input list instead of the original one.
    /// </summary>
    /// <remarks>This method is needed since constructors can't be virtual. Usually, it
    /// is sufficient just to return <c>new MyConditionType(copiedInput)</c> here.</remarks>
    /// <param name="copiedInput">array of  copied inputs for this condition.</param>
    /// <returns> a new copy of this condition for the copied input list.</returns>
    protected abstract ICondition CreateCopy(params INotifyPropertyChanged[] copiedInput);

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;
  }
}
