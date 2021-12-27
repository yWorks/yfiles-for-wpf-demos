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
using yWorks.Graph;

namespace Demo.yFiles.GraphEditor.Support
{
  internal class FilteredMapper<K, V> : IMapper<K, V>
  {
    private Predicate<K> predicate;

    public Predicate<K> Predicate {
      get { return predicate; }
      set { predicate = value; }
    }

    private IMapper<K, V> mapperDelegate;


    public IMapper<K, V> MapperDelegate {
      get { return mapperDelegate; }
      set { mapperDelegate = value; }
    }


    public FilteredMapper(IMapper<K, V> mapperDelegate, Predicate<K> predicate) {
      this.predicate = predicate;
      this.mapperDelegate = mapperDelegate;
    }

    #region IMapper<K,V> Members

   public V this[K key]
    {
      get {
        if (Predicate(key)) {
          return MapperDelegate[key];
        }
        return default(V);
      }
      set {
        if (Predicate(key)) {
          MapperDelegate[key] = value;
        }
      }
    }
   #endregion
  }
}