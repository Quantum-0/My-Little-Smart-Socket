using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MyLittleSmartSocket
{
    class QuantumSmartSocket
    {
        public bool Found { get; private set; }
        public bool State { get; private set; }
        public int Timer { get; private set; }
        public int Uptime { get; private set; }

        private String host;
        private Ping ping = new Ping();

        public event EventHandler<SmartSocketStateChangedEventArgs> SmartSocketStateChanged;
        public event EventHandler<EventArgs> SmartSocketDisconnected;
        public event EventHandler<EventArgs> SmartSocketConnected;

        public QuantumSmartSocket(string ip)
        {
            host = ip;
        }

        private bool Ping()
        {
            lock (ping)
            {
                var result = ping.Send(host);
                if (result.Status == IPStatus.Success)
                    return true;
                else
                {
                    Task.Delay(500).Wait();
                    return ping.Send(host).Status == IPStatus.Success;
                }
            }
        }

        public void On(int timer = 0)
        {
            if (timer == 0)
                MakeRequest("on");
            else
                MakeRequest("on?timer=" + timer.ToString());
            Task.Delay(100).Wait();
            UpdateStatus();
        }

        public void SetDefaults(bool state, int timer = 0)
        {
            if (timer == 0)
                MakeRequest("defaults?state=" + (state?'1':'0'));
            else
                MakeRequest("defaults?timer=" + timer.ToString() + "state=" + (state ? '1' : '0'));
        }

        /*public <bool, int> GetDefaults()
        {
            Tuple.Create()
        }*/

        public void Off()
        {
            MakeRequest("off");
            Task.Delay(100).Wait();
            UpdateStatus();
        }

        public void UpdateStatus()
        {
            bool changed = false;
            var resp = MakeRequest("status");
            if (resp.Success)
            {
                if (Found != true)
                {
                    SmartSocketConnected?.Invoke(this, EventArgs.Empty);
                    changed = true;
                }
                Found = true;
                var lines = resp.Data.Split('\n');
                foreach (var line in lines)
                {
                    if (line == "NO TIMER")
                    {
                        if (Timer != -1)
                            changed = true;
                        Timer = -1;
                    }
                    else
                    {
                        if (line.StartsWith("TIMER:"))
                        {
                            int value = int.Parse(line.Split(':').Last());
                            if (Timer != value)
                                changed = true;
                            Timer = value;
                        }
                        else if (line.StartsWith("UPTIME:"))
                        {
                            int value = int.Parse(line.Split(':').Last());
                            if (Uptime != value)
                                changed = true;
                            Uptime = value;
                        }
                        else if (line.StartsWith("STATE:"))
                        {
                            if (State != line.Contains("ON"))
                                changed = true;
                            State = line.Contains("ON");
                        }
                    }
                }
            }
            else
            {
                if (Found != false)
                    SmartSocketDisconnected?.Invoke(this, EventArgs.Empty);
                Found = false;
            }

            if (changed)
                SmartSocketStateChanged?.Invoke(this, new SmartSocketStateChangedEventArgs(State, Timer));
        }

        public SmartSocketResponse MakeRequest(string method)
        {
            if (!Ping())
                return SmartSocketResponse.NoResponse;

            var resp = (HttpWebRequest.Create("http://" + host + '/' + method).GetResponse() as HttpWebResponse);
            var code = resp.StatusCode;
            var stream = resp.GetResponseStream();
            if (code != HttpStatusCode.OK)
                return new SmartSocketResponse(code);

            string data;
            using (var sr = new StreamReader(stream))
            {
                data = sr.ReadToEnd();
            }
            return new SmartSocketResponse(data);
        }
    }
}
