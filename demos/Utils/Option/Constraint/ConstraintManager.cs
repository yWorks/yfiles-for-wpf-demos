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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Constraint
{
  /// <summary>
  /// This is the main class for constraint handling.
  /// </summary>
  /// <remarks>An instance of this class is always bound to an <see cref="OptionHandler"/> instance.
  /// When new <see cref="IModelView"/>s are (explicitly or implicitly) registered with
  /// the handler, the ConstraintManager installs all registered constraints in the new view. When the view
  /// is disposed, the ConstraintManager automatically disposes all installed constraint copies.
  /// <para>Value sources and action targets are usually defined for the items in the
  /// original option handler.
  /// </para></remarks>
  /// <example> This sample shows how register a constraint manager and create a constraint that
  /// automatically enables the string item on the view iff the int item has value 4.
  /// <code lang="C#">
  ///   class EditorFactoryDemo 
  ///   {
  ///      public static int Main() 
  ///      {
  ///         oh = new OptionHandler("Settings);
  ///         //add various simple items directly to the handler            
  ///         oh.AddString(null, "String", "foo");
  ///         oh.AddInt(null, "Int", 3);
  ///         //Create an standalone editor form
  ///         ConstraintManager cm = new ConstraintManager(oh);
  ///         //this condition is true iff the item has a current value of 4
  ///         ICondition cond = cm.CreateValueEqualsCondition(oh["Int"], 4);
  ///         cm.SetEnabledOnCondition(cond, stringItem);
  ///      }
  ///   }
  /// </code>
  /// </example>
  public sealed class ConstraintManager
  {
    private OptionHandler _handler;
    private List<IConstraint> constraints = new List<IConstraint>();
    private Dictionary<IModelView, List<IConstraint>> viewConstraints = new Dictionary<IModelView, List<IConstraint>>();

    /// <summary>
    /// Get or set whether new constraints should be automatically registered for all active views.
    /// </summary>
    /// <value>Default values is <see langword="true"/></value>
    public bool AutoInstallConstraints {
      get { return autoInstallConstraints; }
      set { autoInstallConstraints = value; }
    }

    private bool autoInstallConstraints = true;

    /// <summary>
    /// Create a new ConstraintManager that is automatically bound to <paramref name="_handler"/>
    /// view change events.
    /// </summary>
    /// <param name="_handler"></param>
    public ConstraintManager(OptionHandler _handler) {
      this._handler = _handler;
      _handler.ViewChanged += handler_ViewChanged;
      foreach (IModelView view in _handler.ActiveViews) {
        viewConstraints[view] = new List<IConstraint>();
      }
    }

//    private void view_PostContentChange(object source, StructureChangeEventArgs e) {
//      List<IConstraint> viewConstraintList;
//      IModelView view = (IModelView) source;
//      viewConstraints.TryGetValue(view, out viewConstraintList);
//      if (viewConstraintList != null) {
//        foreach (IConstraint constraint in viewConstraintList) {
//          constraint.Dispose();
//        }
//        viewConstraintList.Clear();
//      } else {
//        viewConstraintList = new List<IConstraint>();
//        viewConstraints[view] = viewConstraintList;
//      }
      //todo: dispose constraints in this list properly
      //readd all constraints
//      foreach (IConstraint constraint in constraints) {
//        IConstraint viewConstraint = constraint.InstallInView(view);
//        if (viewConstraint != null) {
//          viewConstraintList.Add(viewConstraint);
//        }
//      }
//    }


//    private void view_PreContentChange(object source, StructureChangeEventArgs e) {
//      List<IConstraint> viewConstraintList;
//      IModelView view = (IModelView) source;
//      viewConstraints.TryGetValue(view, out viewConstraintList);
//      if (viewConstraintList != null) {
//        foreach (IConstraint constraint in viewConstraintList) {
//          constraint.Dispose();
//        }
//        viewConstraintList.Clear();
//      }
//   }

    /// <summary>
    /// Clear all registered constraints.
    /// </summary>
    public void Clear() {
      List<IConstraint> viewConstraintList;
      foreach (KeyValuePair<IModelView, List<IConstraint>> pair in viewConstraints) {
        viewConstraintList = pair.Value;
        if (viewConstraintList != null) {
          foreach (IConstraint constraint in viewConstraintList) {
            constraint.Dispose();
          }
          viewConstraintList.Clear();
        }
      }
      viewConstraints.Clear();
      constraints.Clear();
    }

    private void handler_ViewChanged(object source, ViewChangeEventArgs e) {
      IModelView view = e.View;
      if (e.IsAdded) {
        foreach (IConstraint constraint in constraints) {
          if (view != null) {
            IConstraint viewConstraint = constraint.InstallInView(view);
            if (viewConstraint != null) {
              List<IConstraint> viewConstraintList;
              viewConstraints.TryGetValue(view, out viewConstraintList);

              if (viewConstraintList == null) {
                viewConstraints[view] = new List<IConstraint>();
              }
              viewConstraints[view].Add(viewConstraint);
            }
          }
        }
      } else {
        List<IConstraint> viewConstraintList;
        viewConstraints.TryGetValue(view, out viewConstraintList);
        if (viewConstraintList != null) {
          foreach (IConstraint constraint in viewConstraintList) {
            constraint.Dispose();
          }
          viewConstraintList.Clear();
        }

        if (view != null) {
          viewConstraints.Remove(view);
        }
      }
    }

    /// <summary>
    /// Add a new constraint that can be installed onto new views
    /// </summary>
    /// <param name="constraint">The new constraint that is registered.</param>
    public void AddConstraint(IConstraint constraint) {
      constraints.Add(constraint);
      if (AutoInstallConstraints) {
        InstallConstraint(constraint);
      }
    }

    /// <summary>
    /// Force registration of all constraints to all active views
    /// </summary>
    /// <remarks>This is mainly useful in connection with <see cref="AutoInstallConstraints"/></remarks>
    public void ForceInstall() {
      foreach (IConstraint constraint in constraints) {
        InstallConstraint(constraint);
      }
    }

    #region convenience methods for constraint creation

    /// <summary>
    /// Creates and registers a constraint that automatically enables the editor for the item <paramref name="target"/>
    /// iff the value in the editor for <paramref name="source"/> has the value <paramref name="value"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="value"></param>
    /// <param name="target"></param>
    public void SetEnabledOnValueEquals(IOptionItem source, object value, IOptionItem target) {
      IConstraint constraint = new ConditionActionConstraint(CreateValueEqualsCondition(source, value),
                                                             new SetEnabledAction(target));
      AddConstraint(constraint);
    }

    /// <summary>
    /// Creates and registers a constraint that automatically enables the editor for the item <paramref name="target"/>
    /// iff the condition <paramref name="cond"/> becomes true.
    /// </summary>
    /// <param name="cond"></param>
    /// <param name="target">New constraint that automatically enables the editor for the item <paramref name="target"/>
    /// iff the condition <paramref name="cond"/> becomes true</param>
    public void SetEnabledOnCondition(ICondition cond, IOptionItem target) {
      IConstraint constraint = new ConditionActionConstraint(cond, new SetEnabledAction(target));
      AddConstraint(constraint);
    }

    /// <summary>
    /// Create a new Condition that is true iff the value in the editor for <paramref name="source"/> 
    /// has the value <paramref name="triggerValue"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="triggerValue"></param>
    /// <returns>New Condition that is true iff the value in the editor for <paramref name="source"/> 
    /// has the value <paramref name="triggerValue"/></returns>
    public ICondition CreateValueEqualsCondition(IOptionItem source, object triggerValue) {
      return new ValueEqualsCondition(source, triggerValue);
    }

    /// <summary>
    /// Create a new Condition that is true iff the editor for <paramref name="source"/> 
    /// is enabled.
    /// </summary>
    /// <param name="source"></param>
    /// <returns>New Condition that is true iff the editor for <paramref name="source"/> 
    /// is enabled.</returns>
    public ICondition CreateIsEnabledCondition(IOptionItem source) {
      return new IsEnabledCondition(source);
    }

    /// <summary>
    /// Create a new constraint that executes an action when the condition is triggered.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public IConstraint CreateActionConstraint(ICondition trigger, IAction action) {
      return new ConditionActionConstraint(trigger, action);
    }

    /// <summary>
    /// Create a new Condition that is true iff the editor for <paramref name="source"/> 
    /// has a value that appears in <paramref name="triggerValues"/>.
    /// </summary>
    /// <param name="source">The input source of the condition</param>
    /// <param name="triggerValues">List of values for the source which set the condition to true</param>
    /// <returns>New Condition that is true iff the editor for <paramref name="source"/> 
    /// has a value that appears in <paramref name="triggerValues"/></returns>
    public ICondition CreateValueIsOneOfCondition(IOptionItem source, IList triggerValues) {
      return new ValueIsOneOfCondition(source, triggerValues);
    }

    /// <summary>
    /// Create a new Condition that is true iff the editor for <paramref name="source"/> 
    /// has a value that appears in <paramref name="triggerValues"/>.
    /// </summary>
    /// <param name="source">The input source of the condition</param>
    /// <param name="triggerValues">Array of values for the source which set the condition to true</param>
    /// <returns>New Condition that is true iff the editor for <paramref name="source"/> 
    /// has a value that appears in <paramref name="triggerValues"/></returns>
    public ICondition CreateValueIsOneOfCondition(IOptionItem source, params object[] triggerValues) {
      return new ValueIsOneOfCondition(source, triggerValues);
    }

    #endregion

    #region private helper methods

    private void InstallConstraint(IConstraint constraint) {
      foreach (IModelView view in _handler.ActiveViews) {
        if (view != null) {
          IConstraint viewConstraint = constraint.InstallInView(view);
          List<IConstraint> viewConstraintList;
          if (viewConstraint != null) {
            viewConstraints.TryGetValue(view, out viewConstraintList);
            if (viewConstraintList == null) {
              viewConstraintList = viewConstraints[view] = new List<IConstraint>();
            }
            viewConstraintList.Add(viewConstraint);
          }
        }
      }
    }

    #endregion

    #region constraint implementations

    private class ValueEqualsCondition : ConditionBase
    {
      private object triggerValue;

      public ValueEqualsCondition(IOptionItem source, object triggerValue) : base(source) {
        this.triggerValue = triggerValue;
      }

      public override bool IsTrue {
        get {
          object val = ((IOptionItem) Inputs[0]).Value;
          if(val == null) {
            return triggerValue == null;
          }
          return val.Equals(triggerValue);
        }
      }

      protected override ICondition CreateCopy(params INotifyPropertyChanged[] copiedInput) {
        return new ValueEqualsCondition((IOptionItem) copiedInput[0], triggerValue);
      }
    }

    private class IsEnabledCondition : ConditionBase
    {
      public IsEnabledCondition(IOptionItem source)
        : base(source) {}

      public override bool IsTrue {
        get { return ((IOptionItem) Inputs[0]).Enabled; }
      }

      protected override ICondition CreateCopy(params INotifyPropertyChanged[] copiedInput) {
        return new IsEnabledCondition((IOptionItem) copiedInput[0]);
      }
    }

    private class ValueIsOneOfCondition : ConditionBase
    {
      private IList validValues;

      public ValueIsOneOfCondition(IOptionItem source, IList validValues)
        : base(source) {
        this.validValues = validValues;
      }

      public override bool IsTrue {
        get { return validValues.Contains(((IOptionItem) Inputs[0]).Value); }
      }

      protected override ICondition CreateCopy(params INotifyPropertyChanged[] copiedInput) {
        return new ValueIsOneOfCondition((IOptionItem) copiedInput[0], validValues);
      }
    }

    private class SetEnabledAction : IAction
    {
      //todo: turn this into a property that encapsulates a weak reference
      private IOptionItem target;
      private bool invert = false;

      public SetEnabledAction(IOptionItem target) {
        this.target = target;
      }

      public SetEnabledAction(IOptionItem target, bool invert) {
        this.target = target;
        this.invert = invert;
      }

      #region IAction Members

      public void PerformAction(object source, ConditionTriggeredEventArgs statusInformation) {
        target.Enabled = statusInformation.IsTrue && !invert;
      }

      public IAction InstallInView(IModelView view) {
        IAction retval = null;
        IOptionItem viewItem = view.GetViewItem(target);
        if (viewItem != null) {
          retval = new SetEnabledAction(viewItem, invert);
        }
        return retval;
      }

      #endregion

      public void Dispose() {}
    }

    #endregion

    /// <summary>
    /// Factory class for logical conditions that can be used to build more complex conditions
    /// out of existing ones.
    /// </summary>
    public static class LogicalCondition
    {
      /// <summary>
      /// Create a new condition that is <see langword="true"/> iff both <paramref name="cond1"/> and
      /// <paramref name="cond2"/> are <see langword="true"/>.
      /// </summary>
      /// <param name="cond1"></param>
      /// <param name="cond2"></param>
      /// <returns><see langword="true"/> iff both <paramref name="cond1"/> and
      /// <paramref name="cond2"/> are <see langword="true"/></returns>
      public static ICondition And(ICondition cond1, ICondition cond2) {
        return new And(cond1, cond2);
      }

      /// <summary>
      /// Create a new condition that is <see langword="true"/> iff one or both 
      /// of <paramref name="cond1"/> and
      /// <paramref name="cond2"/> are <see langword="true"/>.
      /// </summary>
      /// <param name="cond1"></param>
      /// <param name="cond2"></param>
      /// <returns><see langword="true"/> iff one or both of <paramref name="cond1"/> and
      /// <paramref name="cond2"/> are <see langword="true"/></returns>
      public static ICondition Or(ICondition cond1, ICondition cond2) {
        return new Or(cond1, cond2);
      }

      /// <summary>
      /// Create a new condition that is <see langword="true"/> iff exactly one of 
      /// <paramref name="cond1"/> and
      /// <paramref name="cond2"/> is <see langword="true"/>.
      /// </summary>
      /// <param name="cond1"></param>
      /// <param name="cond2"></param>
      /// <returns><see langword="true"/> iff exactly one of 
      /// <paramref name="cond1"/> and
      /// <paramref name="cond2"/> is <see langword="true"/>.</returns>
      public static ICondition Xor(ICondition cond1, ICondition cond2) {
        return new Xor(cond1, cond2);
      }

      /// <summary>
      /// Create a new condition that is <see langword="true"/> 
      /// iff <paramref name="cond1"/> is <see langword="false"/>.
      /// </summary>
      /// <param name="cond1"></param>
      /// <returns><see langword="true"/> iff <paramref name="cond1"/> is 
      /// <see langword="false"/></returns>
      public static ICondition Not(ICondition cond1) {
        return new Not(cond1);
      }
    }
  }
}
