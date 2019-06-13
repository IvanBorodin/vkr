using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace Environment
{
    public partial class Form1 : Form
    {
        Random r;
        public Form1()
        {
            InitializeComponent();
        }
        double Ax = 0;
        double Ay = 0;
        double Az = 0;

        double rx = 0;
        double ry = 0;
        double rz = 0;

        double Vx = 0;
        double Vy = 0;
        double Vz = 0;

        double cx = 0;
        double cy = 0;

        double latitude = 39.922600;
        double longitude = 131.368511;

        int dt = 1000;
        private void Form1_Load(object sender, EventArgs e)
        {
            AcceptButton = send;
            Task pocess = new Task(() =>
            {
                Connect();
            });
            pocess.Start();
            r = new Random();
        }
        private void Button1_Click_1(object sender, EventArgs e)
        {
            Task pocess = new Task(() =>
            {
                while (true)
                {

                    for (int i = 1; i < 100; i++)
                    {
                        
                        string s = updateVars();

                        _send_Line(s);
                        System.Threading.Thread.Sleep(dt / 10);
                    }
                }
            });
            pocess.Start();
        }
        Socket _COM = null;
        string IP = "192.168.1.67";
        int PORT = 20500;
        int _TIMEOUT = 10 * 1000;
        //Sends a string of characters with a \\n
        void _send_Line(string line)
        {
            line.Replace('\n', ' ');// one new line at the end only!
            byte[] data = System.Text.Encoding.Default.GetBytes(line);
            _COM.Send(data);
            log("Отправлено " + line);
        }

        string _recv_Line()
        {
            //Receives a string. It reads until if finds LF (\\n)
            byte[] buffer = new byte[1];
            int bytesread = _COM.Receive(buffer, 1, SocketFlags.None);
            string line = "";
            while (bytesread > 0 && buffer[0] != '\n')
            {
                line = line + System.Text.Encoding.Default.GetString(buffer);
                bytesread = _COM.Receive(buffer, 1, SocketFlags.None);
            }
            return line;
        }
        public bool Connect()
        {
            bool connected = false;
            while (true)
            {
                _COM = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                //_COM = new Socket(SocketType.Stream, ProtocolType.IPv4);
                _COM.SendTimeout = 1000;
                _COM.ReceiveTimeout = 1000;
                try
                {
                    _COM.Connect(IP, PORT);
                    connected = is_connected();
                    if (connected)
                    {
                        _COM.SendTimeout = _TIMEOUT;
                        _COM.ReceiveTimeout = _TIMEOUT;
                        log("Подключение установлено..");
                        break;
                    }
                }
                catch (Exception e)
                {
                    log(e.Message);
                }

                if (connected)
                {
                    break;
                }
            }

            return connected;
        }
        public delegate int getTrackBarValueDelegate(TrackBar trackBar, double var);

        int getValue(TrackBar trackBar, double variable)
        {
            variable = trackBar.Value;
            return trackBar.Value;
        }
        string updateVars()
        {
            if (trackBar1.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    updateVars();

                }
          ));
            }
            else
            {


                Ax = getItStochastic(0.1, (double)trackBar1.Value / 10);
                Ay = getItStochastic(0.1, (double)trackBar2.Value / 10);
                double disp = 0.5;
                double stoch = +r.NextDouble() * disp;
                Az = (double)trackBar3.Value / 10 + stoch - (disp / 2) - (double)trackBar3.Maximum / 20;

                rx = getItStochastic(0.05, (double)trackBar4.Value / 10);
                ry = getItStochastic(0.05, (double)trackBar5.Value / 10);
                rz = getItStochastic(0.05, (double)trackBar6.Value / 10);

                Vx = Vx + Ax * (dt / 1000.0);
                Vy = Vy + Ay * (dt / 1000.0);
                Vz = Vz + Az * (dt / 1000.0);

                cx =( getItStochastic(0.005, (double)trackBar10.Value / 10)- (double)trackBar10.Maximum / 20)/2;
                cy =( getItStochastic(0.005, (double)trackBar11.Value / 10) - (double)trackBar11.Maximum / 20)/2;

                latitude += Vx / (1000 * 112);
                longitude -= Vy / (1000 * 112);
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(Ax);
            builder.Append(';');
            builder.Append(Ay);
            builder.Append(';');
            builder.Append(Az);
            builder.Append(';');

            builder.Append(rx);
            builder.Append(';');
            builder.Append(ry);
            builder.Append(';');
            builder.Append(rz);
            builder.Append(';');

            builder.Append(Vx);
            builder.Append(';');
            builder.Append(Vy);
            builder.Append(';');
            builder.Append(Vz);
            builder.Append(';');

            builder.Append(cx);
            builder.Append(';');
            builder.Append(cy);
            builder.Append(';');

            builder.Append(latitude);
            builder.Append(';');
            builder.Append(longitude);
            builder.Append(';');
            return builder.ToString();

        }

        double getItStochastic(double disp, double val)
        {
            double stoch = 1 - disp + r.NextDouble() * disp;
            return val * stoch;
        }
        //Returns 1 if connection is valid, returns 0 if connection is invalid
        bool is_connected()
        {
            return _COM != null && _COM.Connected;
        }

        /// <summary>
        /// Checks if the object is currently linked to RoboDK
        /// </summary>
        /// <returns></returns>
        public bool Connected()
        {
            //return _COM.Connected;//does not work well
            if (_COM == null)
            {
                return false;
            }
            bool part1 = _COM.Poll(1000, SelectMode.SelectRead);
            bool part2 = _COM.Available == 0;
            if (part1 && part2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void log(String s, System.Drawing.Color col)
        {
            this.logDelegate = new Form1.LogDelegate(this.delegatelog);
            this.logBox.Invoke(this.logDelegate, this.logBox, s, col);
            var strings = new string[1];
            strings[0] = s;
            // File.AppendAllLines(logPath, strings);
        }
        public void log(string s)
        {
            this.logDelegate = new Form1.LogDelegate(this.delegatelog);
            this.logBox.Invoke(this.logDelegate, this.logBox, s, Color.Black);
            var strings = new string[1];
            strings[0] = s;
            //  File.AppendAllLines(logPath, strings);
        }

        public void delegatelog(RichTextBox richTextBox, String s, Color col)
        {
            try
            {
                richTextBox.SelectionColor = col;
                richTextBox.AppendText(s + '\n');
                richTextBox.SelectionColor = Color.Black;
                richTextBox.SelectionStart = richTextBox.Text.Length;
                if (richTextBox.Text.Length > 2000)
                    richTextBox.Text = richTextBox.Text.Substring(richTextBox.Text.Length - 1001, 1000);
                var strings = new string[1];
                strings[0] = s;
                // File.AppendAllLines(logPath, strings);
            }
            catch { }
        }
        // public void picBoxRefresh() { picBox.Refresh(); }
        public delegate void LogDelegate(RichTextBox richTextBox, string is_completed, Color col);
        public LogDelegate logDelegate;
        public delegate void StringDelegate(string is_completed);
        public delegate void VoidDelegate();
        public StringDelegate stringDelegate;
        public VoidDelegate voidDelegate;

        private void Button1_Click(object sender, EventArgs e)
        {
            _send_Line(richTextBox1.Text);
            richTextBox1.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _send_Line("Disconnect");
            _COM.Disconnect(false);
        }

        private void GroupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void TrackBar11_Scroll(object sender, EventArgs e)
        {

        }

        private void Label6_Click(object sender, EventArgs e)
        {

        }

    }
}
