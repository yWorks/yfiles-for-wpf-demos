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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using yWorks.Graph.PortLocationModels;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml {

  [GraphML(Name = "GenericPortLocationModel")]
  [ContentProperty("Parameters")]
  [MarkupExtensionReturnType(typeof(GenericPortLocationModel))]
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class GenericPortLocationModelExtension : MarkupExtension
  {
    private readonly List<IPortLocationModelParameter> parameters = new List<IPortLocationModelParameter>();

    /// <xamlhelper/>
    public GenericPortLocationModelExtension() { }

    /// <xamlhelper/>
    public GenericPortLocationModelExtension(IEnumerable<IPortLocationModelParameter> parameters) {
      this.parameters.AddRange(parameters);
    }

    /// <xamlhelper/>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<IPortLocationModelParameter> Parameters {
      get { return parameters; }
    }

    /// <xamlhelper/>
    public override object ProvideValue(IServiceProvider serviceProvider) {
      var model = new yWorks.Markup.Common.GenericPortLocationModelExtension();
      foreach (var p in parameters) {
        model.LocationParameters.Add(p);
      }
      return model.ProvideValue(serviceProvider);
    }
  }
}
