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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Constraint
{
  /// <summary>
  /// This interface describes the general contract for a condition that can be used as a building block
  /// in <see cref="IConstraint"/>s.
  /// </summary>
  /// <remarks>The basic design pattern that should be follwed when implementing this interface is
  /// to listen to <see cref="INotifyPropertyChanged.PropertyChanged"/> events on the
  /// input objects and fire a <see cref="ConditionTriggered"/> event when needed. 
  /// For performance and consistency reasons,
  /// registering to input changes should happen only on copies that are created for 
  /// <see cref="IModelView"/> instances, <b>not</b> on the original instance itself.
  /// <para>An actual object is always defined on the <see cref="OptionHandler"/> where
  /// the <see cref="ConstraintManager"/> for this Condition is registered. When the condition is
  /// used in an <see cref="IConstraint"/> contraint, it is the constraint's responsibility to
  /// ensure that the condition is registered and disposed for an actual <see cref="IModelView"/> by calling
  /// <see cref="InstallInView"/>.</para>
  /// <note>Since correctly implementing <see cref="InstallInView"/> is not trivial,
  /// users are strongly advised to use implementation classes like 
  /// <see cref="ConditionBase"/> instead.</note>
  /// </remarks>
  /// <seealso cref="ConditionBase"/>
  /// <seealso cref="IConstraint"/>
  /// <seealso cref="ConstraintManager"/>
  /// <seealso cref="INotifyPropertyChanged"/>
  /// <seealso cref="IModelView"/>
  public interface ICondition : IDisposable, INotifyPropertyChanged
  {
    /// <summary>
    /// Read-Only list of all inputs that this condition should listen to.
    /// </summary>
    ReadOnlyCollection<INotifyPropertyChanged> Inputs { get; }

    /// <summary>
    /// Add a new event source, but do not register any listeners. 
    /// </summary>
    /// <param name="item">The new input source</param>
    void AddInput(INotifyPropertyChanged item);
    
    /// <summary>
    /// Remove a given input source.
    /// </summary>
    /// <param name="item">The input source that is to be removed</param>
    void RemoveInput(INotifyPropertyChanged item);
    
    /// <summary>
    /// Install the condition into the given <paramref name="view"/>
    /// </summary>
    /// <remarks>This method creates a copy of the condition that is correctly
    /// configured to act on the <paramref name="view"/> items that correspond to the
    /// items defined on the <see cref="OptionHandler"/> where the <see cref="ConstraintManager"/>
    /// this instance belongs to is installed. Here all needed listeners on input
    /// events should be set up for the copy.</remarks>
    /// <param name="view">The view where this constraint will be installed.</param>
    /// <returns>A copy of this condition that is configured to work on the 
    /// given <paramref name="view"/>.</returns>
    ICondition InstallInView(IModelView view);
    
    /// <summary>
    /// Returns <see langword="true"/> iff the condition is logically true wrt to it's input values.
    /// </summary>   
    bool IsTrue { get; }
    
    /// <summary>
    /// Called when an input property has been changed.
    /// </summary>
    /// <remarks>This should evaluate <see cref="IsTrue"/> and fire a <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// to notify dependencies of the status change.</remarks>
    /// <param name="sender">The origin of the event</param>
    /// <param name="e">The parameters for the event</param>
    void SourceValueChangedHandler(object sender, PropertyChangedEventArgs e);

    /// <summary>
    /// This event is fired whenever the condition decides it is necessary to inform subscribers.
    /// </summary>
    event ConditionTriggeredEventHandler ConditionTriggered;
  }


  /// <summary>
  /// Specialized event handler for condition status changes.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  public delegate void ConditionTriggeredEventHandler(object sender, ConditionTriggeredEventArgs e);

  /// <summary>
  /// Extension of <see cref="PropertyChangedEventArgs"/> that also indicates
  /// whether the condition has executed succesfully.
  /// </summary>
  /// <remarks>The property name is always fixed to <see cref="ICondition.IsTrue"/>,
  /// clients only should check the status value embedded in this event.</remarks>
  public class ConditionTriggeredEventArgs : PropertyChangedEventArgs
  {
    private bool isTrue;

    /// <summary>
    /// Create new parameter
    /// </summary>
    /// <param name="isTrue">used to track status changes without the need
    /// to query the sender again</param>
    public ConditionTriggeredEventArgs(bool isTrue)
      : base("IsTrue") {
      this.isTrue = isTrue;
    }

    /// <summary>
    /// Returns the status value of the event
    /// </summary>
    public bool IsTrue {
      get { return isTrue; }
    }
  }
}