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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using yWorks.Annotations;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat.CommonMarkup
{
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class NullExtension : MarkupExtension
  {
    public override object ProvideValue(IServiceProvider serviceProvider) {
      return null;
    }
  }

  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class StaticExtension : MarkupExtension
  {
    public string Member { get; set; }

    public StaticExtension() {}

    public StaticExtension(string member) {
      this.Member = member;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new yWorks.Markup.Primitives.StaticExtension(Member).ProvideValue(serviceProvider);
    }
  }

  /// <xamlhelper/>
  [ContentProperty("Items")]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class ListExtension : MarkupExtension
  {

    private readonly List<object> enumerable = new List<object>();

    /// <xamlhelper/>
    public ListExtension() { }

    /// <xamlhelper/>
    public ListExtension(ICollection enumerable) {
      foreach (var item in enumerable) {
        this.enumerable.Add(item);
      }
    }

    /// <xamlhelper/>
    public ListExtension(IEnumerable enumerable) {
      foreach (object o in enumerable) {
        this.enumerable.Add(o);
      }
    }

    /// <xamlhelper/>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public IList Items {
      get { return enumerable; }
    }

    /// <xamlhelper/>
    public override object ProvideValue(IServiceProvider serviceProvider) {
      return enumerable;
    }
  }

  /// <xamlhelper/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [ContentProperty("Items")]
  [GraphML(Name = "Array")]
  public class ArrayExtension : MarkupExtension
  {
    /// <xamlhelper/>
    [CanBeNull, GraphML(Shareable = GraphMLSharingPolicy.Never, Name = "Type")]
    public Type Type { set; get; }

    /// <xamlhelper/>
    [NotNull, DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [GraphML(Name = "Items")]
    public IList Items {
      get {
        return items;
      }
    }

    private readonly ArrayList items = new ArrayList();

    /// <xamlhelper/>
    public ArrayExtension() { }

    /// <xamlhelper/>
    public ArrayExtension([NotNull] Type type) {
      Type = type;
    }

    /// <xamlhelper/>
    public ArrayExtension([NotNull] Array array) {
      Type = array.GetType().GetElementType();
      foreach (var item in array) {
        items.Add(item);
      }
    }

    /// <xamlhelper/>
    [CanBeNull]
    public override object ProvideValue([CanBeNull] IServiceProvider serviceProvider) {
      if (Type == null) {
        throw new InvalidOperationException("Type must not be null");
      }
      return ToArray(items, Type);
    }

    private Array ToArray(ArrayList _this, Type type) {
      Array instance = Array.CreateInstance(type, _this.Count);
      int count = 0;
      foreach (var item in _this) {
        instance.SetValue(item, count++);
      }
      return instance;
    }
  }

  /// <xamlhelper/>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [GraphML(Name = "Type")]
  public class TypeExtension : MarkupExtension
  {
    /// <xamlhelper/>
    [GraphML(Shareable = GraphMLSharingPolicy.Never, Name = "TypeName")]
    public string TypeName { get; set; }

    /// <xamlhelper/>
    public Type Type { get; internal set; }

    /// <xamlhelper/>
    public TypeExtension() { }

    /// <xamlhelper/>
    public TypeExtension(string typeName) {
      this.TypeName = typeName;
    }

    /// <xamlhelper/>
    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new yWorks.Markup.Primitives.TypeExtension(TypeName).ProvideValue(serviceProvider);
    }
  }

}