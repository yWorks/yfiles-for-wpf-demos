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

namespace Demo.Base.RandomGraphGenerator
{
  internal static class RandomSupport
  {
    ///<summary>
    /// Permutates the positions of the elements within the given array.
    ///</summary>
    public static void Permutate(Random rnd, object[] a) {
      // forth...
      for (int i = 0; i < a.Length; i++) {
        int j = rnd.Next(a.Length);
        Object tmp = a[i];
        a[i] = a[j];
        a[j] = tmp;
      }
      // back...
      for (int i = a.Length - 1; i >= 0; i--) {
        int j = rnd.Next(a.Length);
        Object tmp = a[i];
        a[i] = a[j];
        a[j] = tmp;
      }
    }

    ///<summary>
    /// Returns an array of <paramref name="n"/> unique random integers that 
    /// lie within the range <paramref name="min"/> (inclusive) and
    /// <paramref name="max"/> (exclusive). If <c>max - min &lt; n</c>
    /// then <see langword="null"/> is returned.
    ///</summary>
    public static int[] GetUniqueArray(Random rnd, int n, int min, int max) {
      max--;

      int[] ret = null;
      int l = max - min + 1;
      if (l >= n && n > 0) {
        int[] accu = new int[l];
        ret = new int[n];
        for (int i = 0, j = min; i < l; i++,j++) {
          accu[i] = j;
        }
        for (int j = 0, m = l - 1; j < n; j++,m--) {
          int r = rnd.Next(m + 1);
          ret[j] = accu[r];
          if (r < m) {
            accu[r] = accu[m];
          }
        }
      }
      return ret;
    }

    ///<summary>
    /// Returns an array of <paramref name="n"/> randomly chosen boolean values
    /// of which <paramref name="trueCount"/> of them are <see langword="true"/>.
    /// If the requested numbers of true values is bigger than the number
    /// of requested boolean values, an Exception is raised.
    ///</summary>
    public static bool[] GetBoolArray(Random rnd, int n, int trueCount) {
      if (trueCount > n) {
        throw new ArgumentException("RandomSupport.GetBoolArray( " + n + ", " + trueCount + " )");
      }

      int[] a = GetUniqueArray(rnd, trueCount, 0, n);
      bool[] b = new bool[n];
      if (a != null) {
        for (int i = 0; i < a.Length; i++) {
          b[a[i]] = true;
        }
      }
      return b;
    }
  }
}
