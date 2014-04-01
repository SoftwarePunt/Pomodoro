using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Tomato.Properties;

namespace Tomato
{
    public enum Activity
    {
        Idle = 0,
        Working = 1,
        Paused = 2,
        BreakShort = 3,
        BreakLong = 4
    }

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        const int ACTIVITY_DURATION = 60 * 25;

        int activityClock, pauseClock, shorts, totals;
        Timer clockTimer;
        Activity activity;
        string goal;
        NotifyIcon tray;
        bool forceClose;
        
        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
        }

        void Form1_Load(object sender, EventArgs e)
        {
            tray = new NotifyIcon();
            tray.Icon = Resources._1353161231_Tomato;
            tray.Text = "Pomodoro";
            tray.ContextMenuStrip = contextMenuStrip1;
            tray.Visible = true;

            clockTimer = new Timer()
            {
                Enabled = true
            };

            clockTimer.Tick += clockTimer_Tick;
            redata();

            this.FormClosing += Form1_FormClosing;
            tray.DoubleClick += tray_DoubleClick;

            ctxExit.Click += ctxExit_Click;
            ctxShow.Click += ctxShow_Click;
        }

        private void tray_DoubleClick(object sender, EventArgs e)
        {
            RevealTray();
        }

        private void ctxShow_Click(object sender, EventArgs e)
        {
            RevealTray();
        }

        public void RevealTray()
        {
            this.Show();
            this.Activate();
            this.WindowState = FormWindowState.Normal;
        }

        private void ctxExit_Click(object sender, EventArgs e)
        {
            KillIt();
        }

        private void pomoNotif(string notif)
        {
            tray.ShowBalloonTip(5000, "Mr. Pomodoro", notif, ToolTipIcon.Info);
        }

        public void KillIt()
        {
            tray.Visible = false;
            tray.Dispose();

            Application.DoEvents();

            forceClose = true;
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!forceClose)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                Application.DoEvents();
                this.Hide();
                pomoNotif("Pomodoro has been minimized to your tray and is, in fact, still running.");
            }
        }

        private void clockTimer_Tick(object sender, EventArgs e)
        {
            redata(true);
        }

        private bool promptActivity(string pre = null)
        {
            var d = new ActivityDialog(pre);

            if (d.ShowDialog() == DialogResult.Cancel)
            {
                return false;
            }

            goal = d.Input;

            
            addToXml(goal, DateTime.Now);
            pomoNotif("Starting work" + (String.IsNullOrWhiteSpace(goal) ? "" : ": " + goal));
            return true;
        }

        private void redata(bool timer = false)
        {
            this.Invoke(new Action(() =>
            {
                if (activityClock >= ACTIVITY_DURATION)
                {
                    if (shorts >= 3)
                    {
                        Alarm.ActivityEnd();
                        activity = Activity.BreakLong;
                        shorts = 0;

                        pomoNotif("The activity has ended. Time for a long break.");
                    }
                    else
                    {
                        Alarm.PauseNotify();
                        activity = Activity.BreakShort;
                        shorts++;

                        pomoNotif("The activity has ended. Time for a short break.");
                    }

                    totals++;                   

                    RevealTray();
                }

                slbCount.Text = "Pomodoros today: " + totals;
                slbBreakInfo.Text = shorts < 3 ? (4 - shorts) + " Pomodoros to go until long break" : "Next break is long";

                lblDateTime.Text = DateTime.Now.ToString();
                lblTime.ForeColor = Color.Black;
                button2.Enabled = true;

                switch (activity)
                {
                    case Activity.BreakLong:

                        activityClock = 0;
                        lblExplain.Text = "Taking a long break (15-30 min)";
                        button1.Text = "Go back to work";
                        button2.Text = "Reset break timer";

                        if (timer)
                        {
                            pauseClock++;
                        }

                        tray.Text = "Pomodoro is on a short break";
                        setTimer(pauseClock, true);
                        break;

                    case Activity.BreakShort:

                        activityClock = 0;
                        lblExplain.Text = "Taking a short break (3-5 min)";
                        button1.Text = "Go back to work";
                        button2.Text = "Reset break timer";

                        if (timer)
                        {
                            pauseClock++;
                        }

                        tray.Text = "Pomodoro is on a long break";
                        setTimer(pauseClock, true);
                        break;

                    case Activity.Idle:

                        tray.Text = "Pomodoro";
                        lblExplain.Text = "Not started yet";
                        button1.Text = "Start now";
                        button2.Text = "You should start working!";
                        button2.Enabled = false;
                        setTimer(0);
                        break;

                    case Activity.Paused:

                        tray.Text = "Pomodoro is paused";
                        lblTime.ForeColor = Color.Red;
                        lblExplain.Text = "Paused";
                        button1.Text = "Resume";
                        button2.Text = "New activity";
                        setTimer(ACTIVITY_DURATION - activityClock);
                        break;

                    case Activity.Working:

                        lblExplain.Text = String.IsNullOrWhiteSpace(goal) ? "Working" : goal;
                        button1.Text = "Pause";

                        if (timer)
                        {
                            activityClock++;
                        }

                        button2.Text = "Reset activity";
                        tray.Text = "Pomodoro is working ";
                        setTimer(ACTIVITY_DURATION - activityClock, true);
                        break;
                }
            }));
        }

        private void addToXml(String activity, DateTime time)
        {
            XmlDocument document = new XmlDocument();
            String currentDirectory = Environment.CurrentDirectory;
            document.Load(currentDirectory + "\\log.xml");

            XmlElement activityElement = document.CreateElement("Activity");

            XmlElement descriptionElement = document.CreateElement("Description");
            descriptionElement.InnerText = activity;

            XmlElement timeElement = document.CreateElement("StartTime");
            timeElement.InnerText = time.ToString();

            activityElement.AppendChild(timeElement);
            activityElement.AppendChild(descriptionElement);

            XmlNode rootElement = document.GetElementsByTagName("log")[0];
            rootElement.AppendChild(activityElement);

            document.Save("log.xml");
        }

        private void setTimer(int secs, bool traytext = false)
        {
            int mins = (int)Math.Floor(secs / 60d);
            int secsLeft = (secs - (mins * 60));

            var minString = mins.ToString();
            var secString = secsLeft.ToString();

            if (minString.Length == 1)
            {
                minString = "0" + minString;
            }

            if (secString.Length == 1)
            {
                secString = "0" + secString;
            }

            lblTime.Text = minString + ":" + secString;

            if (traytext)
            {
                tray.Text += minString + ":" + secString;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (activity)
            {
                case Activity.BreakLong:
                case Activity.BreakShort:

                    if (promptActivity())
                    {
                        pauseClock = 0;
                        activityClock = 0;
                        activity = Activity.Working;
                    }

                    break;

                case Activity.Paused:

                    activity = Activity.Working;
                    break;

                case Activity.Working:

                    activity = Activity.Paused;
                    break;

                case Activity.Idle:

                    if (promptActivity())
                    {
                        activity = Activity.Working;
                    }

                    break;
            }

            redata();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch (activity)
            {
                case Activity.BreakLong:
                case Activity.BreakShort:

                    pauseClock = 0;
                        redata();
                    break;

                case Activity.Working:
                case Activity.Paused:

                    if (promptActivity(goal))
                    {
                        activityClock = 0;
                        redata();
                    }

                    break;
            }
        }
    }
}
