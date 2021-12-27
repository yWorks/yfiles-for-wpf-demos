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
using System.Reflection;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph.PortLocationModels;
using yWorks.Markup.Common;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml
{
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class NodeScaledParameterExtension : MarkupExtension
  {
    private PointD offset;


    public PointD Offset {
      get { return offset; }
      set { offset = value; }
    }


    public override object ProvideValue(IServiceProvider serviceProvider) {
      var coreExtension = new FreeNodePortLocationModelParameterExtension
      {
        Ratio = new PointD(0.5, 0.5) + Offset,
        Offset = PointD.Origin
      };
      return coreExtension.ProvideValue(serviceProvider);
    }
  }


  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public sealed class NodeScaledPortLocationModelExtension : MarkupExtension
  {

    public static FreeNodePortLocationModel Instance {
      get { return FreeNodePortLocationModel.Instance; }
    }


    public static IPortLocationModelParameter NodeCenterAnchored {
      get { return FreeNodePortLocationModel.NodeCenterAnchored; }
    }


    public static IPortLocationModelParameter NodeLeftAnchored {
      get { return FreeNodePortLocationModel.NodeLeftAnchored; }
    }


    public static IPortLocationModelParameter NodeRightAnchored {
      get { return FreeNodePortLocationModel.NodeRightAnchored; }
    }


    public static IPortLocationModelParameter NodeTopAnchored {
      get { return FreeNodePortLocationModel.NodeTopAnchored; }
    }


    public static IPortLocationModelParameter NodeBottomAnchored {
      get { return FreeNodePortLocationModel.NodeBottomAnchored; }
    }


    public static IPortLocationModelParameter NodeTopLeftAnchored {
      get { return FreeNodePortLocationModel.NodeTopLeftAnchored; }
    }


    public static IPortLocationModelParameter NodeTopRightAnchored {
      get { return FreeNodePortLocationModel.NodeTopRightAnchored; }
    }


    public static IPortLocationModelParameter NodeBottomRightAnchored {
      get { return FreeNodePortLocationModel.NodeBottomRightAnchored; }
    }


    public static IPortLocationModelParameter NodeBottomLeftAnchored {
      get { return FreeNodePortLocationModel.NodeBottomLeftAnchored; }
    }

    #region Overrides of MarkupExtension

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new FreeNodePortLocationModel();
    }

    #endregion
  }
}