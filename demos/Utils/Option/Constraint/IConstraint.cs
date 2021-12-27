/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Constraint
{
  /// <summary>
  /// Interface for installable constraints.
  /// </summary>
  /// <remarks>An installable constraint for an <see cref="OptionHandler"/>
  /// can be set by a <see cref="ConstraintManager"/> instance to support
  /// constraints on associated editors for the OptionHandler. A Constraint is defined
  /// for the handler instance, i.e. acts on the OptionItems and Groups defined there.
  /// The <see cref="ConstraintManager"/> ensures by calling <see cref="InstallInView"/>
  /// that the Constraint is correctly installed and disposed
  /// for a <see cref="IModelView"/> instance (which is usually implicitly created
  /// by creating a graphical editor). 
  /// <note>Since correctly implementing <see cref="InstallInView"/> is not trivial,
  /// users are strongly advised to use implementation classes like 
  /// <see cref="ConditionActionConstraint"/> instead.</note>
  /// </remarks>
  /// <seealso cref="OptionHandler"/>
  /// <seealso cref="IModelView"/>
  /// <seealso cref="ConstraintManager"/>
  /// <seealso cref="ConditionActionConstraint"/>
  public interface IConstraint : IDisposable
  {
    /// <summary>
    /// Install the constraint into the given <paramref name="view"/>
    /// </summary>
    /// <remarks>This method creates a copy of the constraint that is correctly
    /// configured to act on the <paramref name="view"/> items that correspond to the
    /// items defined on the <see cref="OptionHandler"/> where the <see cref="ConstraintManager"/>
    /// this instance belongs to is installed.</remarks>
    /// <param name="view">The view where this constraint will be installed.</param>
    /// <returns>A copy of this constraint that is configured to work on the 
    /// given <paramref name="view"/>.</returns>
    IConstraint InstallInView(IModelView view);
  }
}