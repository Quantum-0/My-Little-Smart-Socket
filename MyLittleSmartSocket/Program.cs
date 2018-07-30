using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyLittleSmartSocket
{
    static class Program
    {
        static NotifyIcon notifyIcon;
        static ContextMenuStrip contextMenu;
        static ToolStripMenuItem menuItemExit;
        static ToolStripLabel menuItemHeader;
        static ToolStripSeparator menuSeparator1;
        static ToolStripSeparator menuSeparator2;
        static ToolStripSeparator menuSeparator3;
        static ToolStripSeparator menuSeparator4;
        static ToolStripSeparator menuSeparator5;
        static ToolStripLabel menuItemState;
        static ToolStripLabel menuItemTimer;
        static ToolStripMenuItem menuItemTurnOn;
        static ToolStripMenuItem menuItemTurnOn10;
        static ToolStripMenuItem menuItemTurnOn30;
        static ToolStripMenuItem menuItemTurnOnHour;
        static ToolStripMenuItem menuItemTurnOn2Hours;
        static ToolStripMenuItem menuItemTurnOn3Hours;
        static ToolStripMenuItem menuItemTurnOnNoTimer;
        static ToolStripMenuItem menuItemTurnOn1;
        static ToolStripMenuItem menuItemTurnOff;
        static ToolStripMenuItem menuItemDefault;
        static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        static QuantumSmartSocket SmartSocket;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {

            SmartSocket = new QuantumSmartSocket("192.168.1.75");
            SmartSocket.SmartSocketStateChanged += SmartSocket_SmartSocketStateChanged;
            SmartSocket.SmartSocketConnected += SmartSocket_SmartSocketConnected;
            SmartSocket.SmartSocketDisconnected += SmartSocket_SmartSocketDisconnected;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            notifyIcon = new NotifyIcon();
            contextMenu = new ContextMenuStrip();
            menuItemExit = new ToolStripMenuItem("Выход");
            menuItemHeader = new ToolStripLabel("My Little Smart Socket by Quantum0");
            menuSeparator1 = new ToolStripSeparator();
            menuSeparator2 = new ToolStripSeparator();
            menuSeparator3 = new ToolStripSeparator();
            menuSeparator4 = new ToolStripSeparator();
            menuSeparator5 = new ToolStripSeparator();
            menuItemState = new ToolStripLabel("Состояние: ВЫКЛ");
            menuItemTimer = new ToolStripLabel("Таймер: ВЫКЛ");
            menuItemTurnOn = new ToolStripMenuItem("Включить");
            menuItemTurnOn10 = new ToolStripMenuItem("На 10 минут");
            menuItemTurnOn30 = new ToolStripMenuItem("На 30 минут");
            menuItemTurnOnHour = new ToolStripMenuItem("На час");
            menuItemTurnOn2Hours = new ToolStripMenuItem("На 2 часа");
            menuItemTurnOn3Hours = new ToolStripMenuItem("На 3 часа");
            menuItemTurnOnNoTimer = new ToolStripMenuItem("Без таймера");
            menuItemTurnOn1 = new ToolStripMenuItem("На 1 минуту");
            menuItemTurnOff = new ToolStripMenuItem("Выключить");
            menuItemDefault = new ToolStripMenuItem("Настройки");


            menuItemTurnOn.DropDownItems.AddRange(new ToolStripItem[] { menuItemTurnOn1, menuItemTurnOn10, menuItemTurnOn30, menuItemTurnOnHour, menuItemTurnOn2Hours, menuItemTurnOn3Hours , menuSeparator3, menuItemTurnOnNoTimer});
            contextMenu.Items.AddRange(new ToolStripItem[] { menuItemHeader, menuSeparator1, menuItemState, menuItemTimer, menuSeparator2, menuItemTurnOn, menuItemTurnOff, menuSeparator4, menuItemDefault, menuSeparator5, menuItemExit });

            menuItemExit.Click += (s, e) => { cancelTokenSource.Cancel(); Application.Exit(); };
            menuItemTurnOff.Click += (s, e) => SmartSocket.Off();
            menuItemTurnOn1.Click += (s, e) => SmartSocket.On(1);
            menuItemTurnOn10.Click += (s, e) => SmartSocket.On(10);
            menuItemTurnOn30.Click += (s, e) => SmartSocket.On(30);
            menuItemTurnOnHour.Click += (s, e) => SmartSocket.On(60);
            menuItemTurnOn2Hours.Click += (s, e) => SmartSocket.On(120);
            menuItemTurnOn3Hours.Click += (s, e) => SmartSocket.On(180);
            menuItemTurnOnNoTimer.Click += (s, e) => SmartSocket.On();

            menuItemState.Text = "Состояние: НЕ НАЙДЕНА";
            menuItemTimer.Visible = false;
            menuItemTurnOn.Visible = false;
            menuItemTurnOff.Visible = false;

            MethodInfo methodShowContextMenu = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            methodShowContextMenu.Invoke(notifyIcon, null);
            notifyIcon.Click += (s, e) => methodShowContextMenu.Invoke(notifyIcon, null);
            notifyIcon.Icon = new Icon("icon.ico");
            notifyIcon.Text = "My Little Smart Socket by Quantum0";
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Visible = true;
            Task.Run((Action)(() => SmartSocketLoop(cancelTokenSource.Token)), cancelTokenSource.Token);
            Application.Run();
            cancelTokenSource.Cancel();
            notifyIcon.Visible = false;
        }

        private static void SmartSocket_SmartSocketConnectedInInvoke(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(3000, "Quantum0 Smart Socket", "Умная розетка с таймером подключена", ToolTipIcon.Info);
            menuItemTimer.Visible = true;
            menuItemTurnOn.Visible = true;
            menuItemTurnOff.Visible = true;
        }

        private static void SmartSocket_SmartSocketDisconnectedInInvoke(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(3000, "Quantum0 Smart Socket", "Умная розетка с таймером отключена", ToolTipIcon.Warning);
            menuItemState.Text = "Состояние: НЕ НАЙДЕНА";
            menuItemTimer.Visible = false;
            menuItemTurnOn.Visible = false;
            menuItemTurnOff.Visible = false;
        }

        private static void SmartSocket_SmartSocketConnected(object sender, EventArgs e)
        {
            if (contextMenu.InvokeRequired)
                contextMenu.Invoke((Action)(() => SmartSocket_SmartSocketConnectedInInvoke(sender, e)));
            else
                SmartSocket_SmartSocketConnectedInInvoke(sender, e);
        }

        private static void SmartSocket_SmartSocketDisconnected(object sender, EventArgs e)
        {
            if (contextMenu.InvokeRequired)
                contextMenu.Invoke((Action)(() => SmartSocket_SmartSocketDisconnectedInInvoke(sender, e)));
            else
                SmartSocket_SmartSocketDisconnectedInInvoke(sender, e);
        }

        private static void SmartSocket_SmartSocketStateChangedInInvoke(object sender, SmartSocketStateChangedEventArgs e)
        {
            menuItemState.Text = "Состояние: " + (e.State ? "ВКЛ" : "ВЫКЛ");
            menuItemTimer.Text = "Таймер: " + (e.Timer == -1 ? "ВЫКЛ" : e.Timer.ToString() + " мин.");
            if (e.State)
            {
                menuItemTurnOff.Enabled = true;
                menuItemTurnOn.Enabled = false;
            }
            else
            {
                menuItemTurnOff.Enabled = false;
                menuItemTurnOn.Enabled = true;
            }
        }

        private static void SmartSocket_SmartSocketStateChanged(object sender, SmartSocketStateChangedEventArgs e)
        {
            if (contextMenu.InvokeRequired)
                contextMenu.Invoke((Action)(() => SmartSocket_SmartSocketStateChangedInInvoke(sender, e)));
            else
                SmartSocket_SmartSocketStateChangedInInvoke(sender, e);
        }

        private static void SmartSocketLoop(CancellationToken cancel)
        {
            Task.Delay(500).Wait();
            while (!cancel.IsCancellationRequested)
            {                
                SmartSocket.UpdateStatus();
                Task.Delay(5000).Wait();
            }
        }
    }
}
