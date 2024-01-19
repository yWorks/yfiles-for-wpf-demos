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
using System.ComponentModel;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Constraint
{
  internal abstract class BinaryLogicalCondition : ConditionBase
  {
    protected BinaryLogicalCondition(ICondition condition1, ICondition condition2) : base(condition1, condition2) {}

    public override void AddInput(INotifyPropertyChanged item) {
      throw new NotImplementedException();
    }

    public override void RemoveInput(INotifyPropertyChanged item) {
      throw new NotImplementedException();
    }

    public override void Dispose() {
      //dispose both sources
      ((ICondition) Inputs[0]).Dispose();
      ((ICondition) Inputs[1]).Dispose();
      base.Dispose();
    }

    protected override INotifyPropertyChanged GetItemCopy(IModelView view, INotifyPropertyChanged item) {
      ICondition condCopy = ((ICondition)item).InstallInView(view);
      return condCopy;
    }
  }

  internal sealed class And : BinaryLogicalCondition
  {
    public And(ICondition condition1, ICondition condition2) : base(condition1, condition2) {}

    public override bool IsTrue {
      get { return ((ICondition) Inputs[0]).IsTrue && ((ICondition) Inputs[1]).IsTrue; }
    }

    protected override ICondition CreateCopy(params INotifyPropertyChanged[] copiedInput) {
      return new And((ICondition) copiedInput[0], (ICondition) copiedInput[1]);
    }
  }

  internal sealed class Or : BinaryLogicalCondition
  {
    public Or(ICondition condition1, ICondition condition2) : base(condition1, condition2) {}

    public override bool IsTrue {
      get { return ((ICondition) Inputs[0]).IsTrue || ((ICondition) Inputs[1]).IsTrue; }
    }

    protected override ICondition CreateCopy(params INotifyPropertyChanged[] copiedInput) {
      return new Or((ICondition) copiedInput[0], (ICondition) copiedInput[1]);
    }
  }

  internal sealed class Xor : BinaryLogicalCondition
  {
    public Xor(ICondition condition1, ICondition condition2) : base(condition1, condition2) {}

    public override bool IsTrue {
      get {
        bool v1 = ((ICondition) Inputs[0]).IsTrue;
        bool v2 = ((ICondition) Inputs[1]).IsTrue;
        return (v1 && !v2) || (!v1 && v2);
      }
    }

    protected override ICondition CreateCopy(params INotifyPropertyChanged[] copiedInput) {
      return new Xor((ICondition) copiedInput[0], (ICondition) copiedInput[1]);
    }
  }

  internal sealed class Not : ConditionBase
  {
    public Not(ICondition condition1)
      : base(condition1) {}

    public override bool IsTrue {
      get { return !((ICondition) Inputs[0]).IsTrue; }
    }

    protected override ICondition CreateCopy(params INotifyPropertyChanged[] copiedInput) {
      return new Not((ICondition) copiedInput[0]);
//      throw new NotImplementedException();
    }

    public override void Dispose() {
      ((ICondition) Inputs[0]).Dispose();
      base.Dispose();
    }

    protected override INotifyPropertyChanged GetItemCopy(IModelView view, INotifyPropertyChanged item) {
      ICondition condCopy = ((ICondition)item).InstallInView(view);
      return condCopy;
    }
  }
}