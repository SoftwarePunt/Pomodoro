using System;
using System.Threading;

namespace Tomato
{
    public delegate void TimerTick(object sender, EventArgs e);

    public class Timer
    {
        const int SLEEP_TIME = 1000;

        private Thread innerThread;
        public Boolean Enabled;
        public TimerTick Tick;

        public Timer()
        {
            innerThread = new Thread(DoEvents);
            innerThread.Priority = ThreadPriority.Normal;
            innerThread.Start();
        }

        private void DoEvents()
        {
            try
            {
                while (true)
                {
                    var start = DateTime.Now.Ticks;

                    if (Enabled)
                    {
                        if (Tick != null)
                        {
                            Tick.Invoke(null, null);
                        }
                    }

                    var end = DateTime.Now.Ticks;
                    var spent = new TimeSpan(end - start).Ticks;

                    var sleepTime = (int)(SLEEP_TIME - spent / TimeSpan.TicksPerMillisecond);

                    if (sleepTime <= 0)
                    {
                        continue;
                    }

                    Thread.Sleep(sleepTime);
                }
            }
            catch (ThreadAbortException) { }
        }

        public void Abort()
        {
            this.Enabled = false;

            if (innerThread != null)
            {
                innerThread.Abort();
                innerThread = null;
            }
        }
    }
}
