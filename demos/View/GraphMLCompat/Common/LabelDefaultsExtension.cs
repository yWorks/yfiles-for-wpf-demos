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
using System.Windows.Markup;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.IO.GraphML.Compat.Common
{
  /// 
  public class LabelDefaultsExtension : MarkupExtension
  {
    public ILabelStyle Style { get; set; }
    public bool AutoAdjustPreferredSize { get; set; }
    public bool ShareLabelModelParameterInstance { get; set; }
    public bool ShareStyleInstance { get; set; }
    public ILabelModelParameter LabelModelParameter { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new LabelDefaults {
        Style = Style,
        LayoutParameter = LabelModelParameter,
        AutoAdjustPreferredSize = AutoAdjustPreferredSize,
        ShareLayoutParameterInstance = ShareLabelModelParameterInstance,
        ShareStyleInstance = ShareStyleInstance
      };
    }
  }

}