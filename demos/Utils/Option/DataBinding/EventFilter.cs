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

using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;
using Timer = System.Windows.Threading.DispatcherTimer;

namespace yWorks.Utils
{
  /// <summary>
  /// An event "filter" implementation that can be used to collapse events within a given
  /// time span and fire a final event after the time has elapsed.
  /// </summary>
  /// <typeparam name="T">The type of the event to fire.</typeparam>
  public class EventFilter<T> where T:EventArgs
  {
    private delegate void MethodInvoker();

    private readonly Timer timer;
    private readonly Control control;
    private TimeSpan duration;
    private T lastArgs;
    private MethodInvoker startInvoker;
    private MethodInvoker stopInvoker;
    private object lastSender;
    private MethodInvoker restartInvoker;
    private bool eventRestartsTimer;

    /// <summary>
    /// Whether another arriving event restarts the timer.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="false"/>
    /// </remarks>
    public bool EventRestartsTimer {
      get { return eventRestartsTimer; }
      set { eventRestartsTimer = value; }
    }

    /// <summary>
    /// Gets or sets the duration during which events should be collapsed.
    /// </summary>
    /// <remarks>
    /// The default is half a second.
    /// </remarks>
    public TimeSpan Duration {
      get { return duration; }
      set {
        duration = value;
        int millis = (int)duration.TotalMilliseconds;
        if (millis > 0) {
          timer.Interval = value;
        }
      }
    }

    /// <summary>
    /// Creates a new instance using the container for the construction of the internal timer.
    /// </summary>
    /// <param name="container">The container for the timer or <see langword="null"/>.</param>
    public EventFilter(IContainer container) {
      timer = new Timer();
      init();
    }

    /// <summary>
    /// Creates the filter for the control.
    /// </summary>
    /// <param name="control">The control to use <see cref="Dispatcher.Invoke(DispatcherPriority,Delegate)"/> for the safe
    /// starting and stopping of the Timer instance.</param>
    public EventFilter(Control control) : this(control, TimeSpan.FromMilliseconds(500)) {}

    /// <summary>
    /// Creates the filter for the control.
    /// </summary>
    /// <param name="control">The control to use <see cref="Dispatcher.Invoke(DispatcherPriority,Delegate)"/> for the safe
    /// starting and stopping of the Timer instance.</param>
    /// <param name="duration">The initial value for <see cref="Duration"/>.</param>
    public EventFilter(Control control, TimeSpan duration) {
      timer = new Timer();
      this.control = control;
      init();
      this.Duration = duration;
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public EventFilter() : this((IContainer) null){}

    private void init() {
      timer.Tick += timer_Tick;
      startInvoker = startTimer;
      stopInvoker = stopTimer;
      restartInvoker = restartTimer;
      Duration = TimeSpan.FromMilliseconds(500);
    }

    void timer_Tick(object sender, EventArgs e) {
      timer.Stop();
      OnTick();
    }

    /// <summary>
    /// Cancels any pending events.
    /// </summary>
    public void Cancel() {
      StopTimer();
      lastSender = lastArgs = null;
    }

    /// <summary>
    /// Called once the timer goes off.
    /// </summary>
    protected virtual void OnTick() {
      object sender = lastSender;
      T args = lastArgs;
      lastSender = lastArgs = null;
      OnFilteredEvent(sender, args);
    }

    /// <summary>
    /// Event handler that can be used to trigger the start of the timer.
    /// </summary>
    public void OnEvent(object source, T eventArgs) {
      OnEventCaptured(source, eventArgs);
    }

    /// <summary>
    /// Event handler that can be used to trigger the start of the timer.
    /// </summary>
    public void OnEvent(T eventArgs) {
      OnEventCaptured(null, eventArgs);
    }

    /// <summary>
    /// Event handler that can be used to trigger the start of the timer.
    /// </summary>
    public void OnEvent() {
      OnEventCaptured(null, null);
    }

    /// <summary>
    /// Flushes pending events and immediately fires them.
    /// </summary>
    public virtual void Flush() {
      if (timer.IsEnabled) {
        StopTimer();
        OnTick();
      }
    }

    /// <summary>
    /// Generic Event handler that can be used to trigger the start of the timer.
    /// </summary>
    public void OnEvent<TEventArgs>(object source, TEventArgs eventArgs) {
      if (typeof(TEventArgs) == typeof(T)) {
        OnEventCaptured(source, (T) ((object)eventArgs));
      } else {
        OnEventCaptured(null, null);
      }
    }

    /// <summary>
    /// Called whenever an event is captured.
    /// </summary>
    protected virtual void OnEventCaptured(object lastSender, T lastArgs) {
      if (duration.TotalMilliseconds <= 0) {
        OnFilteredEvent(lastSender, lastArgs);
      } else {
        this.lastSender = lastSender;
        this.lastArgs = lastArgs;
        if (eventRestartsTimer) {
          RestartTimer();
        } else {
          StartTimer();
        }
      }
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    protected virtual void StartTimer() {
      if (control != null && control.Dispatcher.CheckAccess()) {
        Invoke(control.Dispatcher);
      } else {
        startTimer();
      }
    }

    private void Invoke(Dispatcher dispatcher) {
      // WPF
      //dispatcher.Invoke(DispatcherPriority.Normal, startInvoker);
      // SL
      dispatcher.BeginInvoke(startInvoker);
    }

    /// <summary>
    /// Restarts the timer.
    /// </summary>
    protected virtual void RestartTimer() {
      if (control != null && control.Dispatcher.CheckAccess()) {
        Invoke(control.Dispatcher);
      } else {
        restartTimer();
      }
    }

    private void restartTimer() {
      timer.Stop();
      timer.Start();
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    protected virtual void StopTimer() {
      if (control != null && control.Dispatcher.CheckAccess()) {
        Invoke(control.Dispatcher);
      } else {
        stopTimer();
      }
    }

    /// <summary>
    /// Called once the timer went off to trigger the <see cref="Event"/>.
    /// </summary>
    protected virtual void OnFilteredEvent(object lastSender, T lastArgs) {
      Event(lastSender, lastArgs);
    }

    /// <summary>
    /// The event clients can register with.
    /// </summary>
    public event EventHandler<T> Event;

    private void startTimer() {
      if (!timer.IsEnabled) {
        timer.Start();
      } 
    }

    private void stopTimer() {
      if (timer.IsEnabled) {
        timer.Stop();
      }
    }
  }
}
