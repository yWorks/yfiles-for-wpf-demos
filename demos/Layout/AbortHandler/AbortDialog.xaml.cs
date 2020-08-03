/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Windows;

namespace Demo.yFiles.Layout.AbortHandler
{


  /// <summary>
  /// Interaction logic for AbortDialog.xaml 
  /// </summary>
  public partial class AbortDialog : Window
  {

    // Whether a stop is already requested.
    private bool stopRequested;

    public AbortDialog()
    {
      InitializeComponent();
    }

    /// <summary>
    /// The AbortHandler to use.
    /// </summary>
    public yWorks.Algorithms.AbortHandler AbortHandler { get; set; }


    /// <summary>
    /// Handles Clicks on the Stop/Abort button.
    /// </summary>
    /// <remarks>
    /// The first click will schedule a stop request, the second click will cancel the layout immediately.
    /// </remarks>
    private void OnStopClicked(object sender, RoutedEventArgs e) {
      if (this.AbortHandler == null) {
        return;
      }
      
      if (stopRequested) {
        // stop was already requested: abort the layout.
        // Calling Cancel() will cause the layout algorithm to throw an AlgorithmAbortedException -
        // this stops the layout immediately. The exception is caught by the LayoutExecutor.
        // The original graph stays unchanged.
        this.AbortHandler.Cancel();
        // close this window
        Close();
      } else {
        // set stopRequested to true, so the next call will be to cancel the layout
        stopRequested = true;
        // change button and label text
        stopButton.Content = "Abort Immediately";
        label.Content = "Trying to stop layout calculation.";
        // schedule a stop request
        // calling Stop() will not stop the layout algorithm immediately
        // instead, a request is scheduled for the layout algorithm to stop
        // when the graph is in a defined state again
        this.AbortHandler.Stop();
      }
    }
  }
}
