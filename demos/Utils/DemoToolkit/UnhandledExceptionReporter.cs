/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Threading;

namespace yWorks.Demo
{
  /// <summary>
  ///   Shows a dialog with details for unhandled WPF dispatcher exceptions.
  /// </summary>
  public static class UnhandledExceptionReporter
  {
    [ThreadStatic]
    private static bool isShowing;

    /// <summary>
    ///   Registers a handler that displays unhandled dispatcher exceptions in a report dialog.
    /// </summary>
    public static void Install() {
      var application = Application.Current ?? throw new InvalidOperationException(
          $"{nameof(Install)} must be called after a WPF Application is created.");
      application.DispatcherUnhandledException -= OnDispatcherUnhandledException;
      application.DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
      if (isShowing) {
        e.Handled = true;
        return;
      }

      // Mark the exception as handled before entering the nested dialog message loop.
      // This lets the WPF dispatcher continue after the dialog is closed.
      e.Handled = true;
      isShowing = true;
      try {
        new ExceptionReportDialog(e.Exception).ShowDialog();
      } finally {
        isShowing = false;
      }
    }
  }
}
