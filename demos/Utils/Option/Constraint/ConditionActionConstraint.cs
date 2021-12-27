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

using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Constraint
{
  /// <summary>
  /// Implementation of interface <see cref="IConstraint"/> that wires a single source <see cref="ICondition"/>
  /// and a single target <see cref="IAction"/> together.
  /// </summary>
  /// <remarks></remarks>
  public class ConditionActionConstraint : IConstraint
  {
    private ICondition sourceCondition;
    private IAction targetAction;

    /// <summary>
    /// Create a new instance for the given source and target objects
    /// </summary>
    /// <param name="sourceCondition">The source condition</param>
    /// <param name="targetAction">The target action</param>
    public ConditionActionConstraint(ICondition sourceCondition, IAction targetAction) {
      this.sourceCondition = sourceCondition;
      this.targetAction = targetAction;
    }

    #region IConstraint Members

    /// <summary>
    /// This implementation creates copies of the source condition and the target action configured
    /// for <paramref name="view"/>, and registeres the target copy as a listener for the source copy's
    /// <see cref="ICondition.ConditionTriggered"/> event.
    /// </summary>
    /// <param name="view">The view where to install this constraint.</param>
    /// <returns>A correctly configured copy of the original constraint</returns>
    public IConstraint InstallInView(IModelView view) {
      ICondition newSrc = sourceCondition.InstallInView(view);
      IAction newAction = targetAction.InstallInView(view);

      if (newSrc != null && newAction != null) {
        //for now, just wire condition and action together
        newSrc.ConditionTriggered += newAction.PerformAction;
        //force value check!
        newSrc.SourceValueChangedHandler(null, null);
        //return new holder instance...
        return new ConditionActionConstraint(newSrc, newAction);
      }
      return null;
    }

    #endregion

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose() {
      //disconnect source and target
      sourceCondition.ConditionTriggered -= targetAction.PerformAction;
      sourceCondition.Dispose();
      targetAction.Dispose();
    }
  }
}