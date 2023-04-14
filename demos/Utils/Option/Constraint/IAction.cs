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
using System.ComponentModel;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Constraint
{
  /// <summary>
  /// This interface describes the general contract for an action that can be used as a building block
  /// in <see cref="IConstraint"/>s.
  /// </summary>
  /// <remarks>The basic design pattern that should be follwed when implementing this interface is
  /// to listen to <see cref="INotifyPropertyChanged.PropertyChanged"/> events on the
  /// input objects and perform something on the <see cref="IModelView"/> where
  /// this action is installed. For performance and consistency reasons,
  /// registering to input changes should happen only on copies that are created for 
  /// <see cref="IModelView"/> instances, <b>not</b> on the original instance itself. When the 
  /// action is
  /// used in an <see cref="IConstraint"/> constraint, it is the constraint's responsibility to
  /// ensure that the action is registered and disposed for an actual <see cref="IModelView"/> 
  /// by calling
  /// <see cref="InstallInView"/>.
  /// </remarks>
  /// <seealso cref="ConditionBase"/>
  /// <seealso cref="IConstraint"/>
  /// <seealso cref="ConstraintManager"/>
  /// <seealso cref="INotifyPropertyChanged"/>
  /// <seealso cref="IModelView"/>
  public interface IAction: IDisposable
  {
    /// <summary>
    /// Perform an action on the view where this action is registered.
    /// </summary>
    /// <param name="source">The event source</param>
    /// <param name="statusInformation">The event parameters</param>
    void PerformAction(object source, ConditionTriggeredEventArgs statusInformation);
    
    /// <summary>
    /// Register this action into the given <paramref name="view"/>.
    /// </summary>
    /// <param name="view">The view where the action should be regstered</param>
    /// <returns>A copy of the action that is correctly configured to work on <paramref name="view"/>.
    /// </returns>
    IAction InstallInView(IModelView view);
  }
}