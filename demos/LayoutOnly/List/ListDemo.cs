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
using System.Collections.Generic;
using yWorks.Algorithms;

namespace Demo.Base.List
{
  /// <summary>
  /// Demonstrates how to use the linked list data type YList and the ICursor interface.
  /// </summary>
  public class ListDemo
  {
    public static void Main() {
      //create new YList instance
      YList list = new YList();

      //add numbered String elements to list 
      for (int i = 0; i < 20; i++) {
        list.AddLast("" + i);
      }

      //iterate over list from first to last
      Console.WriteLine("List elements from front to back");
      for (ICursor c = list.Cursor(); c.Ok; c.Next()) {
        //output element at cursor position
        Console.WriteLine(c.Current);
      }

      //iterate over list from last to first
      Console.WriteLine("List elements from back to front");
      ICursor rc = list.Cursor();
      for (rc.ToLast(); rc.Ok; rc.Prev()) {
        //output element at cursor position
        Console.WriteLine(rc.Current);
      }

      //sort list lexicografically
      list.Sort(new LexicographicComparer());

      //iterate over list from first to last
      Console.WriteLine("Lexicographically sorted list");
      for (ICursor c = list.Cursor(); c.Ok; c.Next()) {
        //output element at cursor position
        Console.WriteLine(c.Current);
      }

      //low level iteration on list cells (non-const iteration)
      for (ListCell cell = list.FirstCell; cell != null; cell = cell.Succ()) {
        String s = (String) cell.Info;
        //remove all Strings from list that have length == 1 
        if (s.Length == 1) {
          list.RemoveCell(cell);
          //note that cell is still half-valid, i.e it's Succ() and Pred()
          //pointers are still unchanged. therefore cell = cell.Succ() is
          //valid in the for-statement
        }
      }

      Console.WriteLine("list after element removal");
      Console.WriteLine(list);

      //initialize list2 with the elements from list
      YList list2 = new YList(list.Cursor());
      Console.WriteLine("list2 after creation");
      Console.WriteLine(list2);

      //reverse element order in list2
      list2.Reverse();
      Console.WriteLine("list2 after reversal");
      Console.WriteLine(list2);

      //move all elements of list2 to the end of list
      list.Splice(list2);

      Console.WriteLine("list after splicing");
      Console.WriteLine(list);
      Console.WriteLine("list2 after splicing");
      Console.WriteLine(list2);

      Console.WriteLine("\nPress key to end demo.");
      Console.ReadKey();
    }
  }

  internal class LexicographicComparer : IComparer<Object>
  {
    public int Compare(object x, object y) {
      return String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
    }
  }
}