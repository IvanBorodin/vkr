using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
namespace Sensor
{
    public partial class Form1 : Form
    {
        string logPath;
        public Form1()
        {
            InitializeComponent();
            logPath = "log.txt";
        }
        int window = 3;
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        string buildString(string[] subLines)
        {
            StringBuilder builder = new StringBuilder();
            //  builder.Append("UTC ");
            builder.Append(DateTime.UtcNow.ToShortDateString());
            builder.Append(' ');
            builder.Append(DateTime.UtcNow.ToShortTimeString());
            builder.Append(':');
            builder.Append(DateTime.UtcNow.Second);
            builder.Append(':');
            builder.Append(DateTime.UtcNow.Millisecond);
            builder.Append(';');
            builder.Append("Ax=");
            builder.Append(subLines[0]);
            builder.Append(';');
            builder.Append("Ay=");
            builder.Append(subLines[1]);
            builder.Append(';');
            builder.Append("Az=");
            builder.Append(subLines[2]);
            builder.Append(';');
            builder.Append("rx=");
            builder.Append(subLines[3]);
            builder.Append(';');
            builder.Append("ry=");
            builder.Append(subLines[4]);
            builder.Append(';');
            builder.Append("rz=");
            builder.Append(subLines[5]);
            builder.Append(';');
            builder.Append("Vx=");
            builder.Append(subLines[6]);
            builder.Append(';');
            builder.Append("Vy=");
            builder.Append(subLines[7]);
            builder.Append(';');
            builder.Append("Vz=");
            builder.Append(subLines[8]);
            builder.Append(';');
            builder.Append("cx=");
            builder.Append(subLines[9]);
            builder.Append(';');
            builder.Append("cy=");
            builder.Append(subLines[10]);
            builder.Append(';');
            builder.Append("latitude=");
            builder.Append(subLines[11]);
            builder.Append(';');
            builder.Append("longitude=");
            builder.Append(subLines[12]);
            builder.Append(';');
            return builder.ToString();
        }
        Socket _COM = null;
        int _TIMEOUT = 10 * 1000;
        //Sends a string of characters with a \\n
        void _send_Line(string line)
        {
            line.Replace('\n', ' ');// one new line at the end only!
            byte[] data = System.Text.Encoding.Default.GetBytes(line + "\n");
            _COM.Send(data);
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
            for (int i = 0; i < 2; i++)
            {
                _COM = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                //_COM = new Socket(SocketType.Stream, ProtocolType.IPv4);
                _COM.SendTimeout = 1000;
                _COM.ReceiveTimeout = 1000;
                try
                {
                    _COM.Connect(textBox1.Text, Convert.ToInt32(textBox2.Text));
                    connected = is_connected();
                    if (connected)
                    {
                        _COM.SendTimeout = _TIMEOUT;
                        _COM.ReceiveTimeout = _TIMEOUT;
                        log("УСАВП подключено");
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
        //Returns 1 if connection is valid, returns 0 if connection is invalid
        bool is_connected()
        {
            return _COM != null && _COM.Connected;
        }
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
            //  File.AppendAllLines(logPath, strings);
        }
        public void log(string s)
        {
            this.logDelegate = new Form1.LogDelegate(this.delegatelog);
            this.logBox.Invoke(this.logDelegate, this.logBox, s, Color.Black);
        }
        public void delegatelog(RichTextBox richTextBox, String s, Color col)
        {
            try
            {
                richTextBox.SelectionColor = col;
                richTextBox.AppendText(s + '\n');
                richTextBox.SelectionColor = Color.Black;
                richTextBox.SelectionStart = richTextBox.Text.Length;
                if (richTextBox.Text.Length > 20000)
                    richTextBox.Text = richTextBox.Text.Substring(richTextBox.Text.Length - 10001, 10000);
                //  var strings = new string[1];
                //  strings[0] = s;
                //   File.AppendAllLines(logPath, strings);
            }
            catch { }
        }
        //  public void picBoxRefresh() { picBox.Refresh(); }
        public delegate void LogDelegate(RichTextBox richTextBox, string is_completed, Color col);
        public LogDelegate logDelegate;
        public delegate void StringDelegate(string is_completed);
        public delegate void VoidDelegate();
        public StringDelegate stringDelegate;
        public VoidDelegate voidDelegate;

        private void Send_Click(object sender, EventArgs e)
        {
            try
            {
                Connect();
            }
            catch { }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ParameterVisualizer visA = new ParameterVisualizer(picBox, this, "Acceleration", Color.Yellow);

            visA.mainFontDepth = 12;
            visA.functionDepth = 2;
            // visA.functions.Add(new Function("Ax", Color.Red));
            //  visA.functions.Add(new Function("Ay", Color.Green));
            visA.functions.Add(new Function("Az", Color.Blue));
            visA.lightsOn = true;
            visA.functions.Add(new Function("AvgLowFric Az", Color.Cyan));
            visA.functions.Add(new Function("Avg+StdDevLowFric Az", Color.Pink));
            visA.functions.Add(new Function("Avg-StdDevLowFric Az", Color.Pink));

            visA.functions.Add(new Function("AvgHighFric Az", Color.LimeGreen));
            visA.functions.Add(new Function("Avg+StdDevHighFric Az", Color.Lime));
            visA.functions.Add(new Function("Avg-StdDevHighFric Az", Color.Lime));

            ParameterVisualizer visGPS = new ParameterVisualizer(pictureBox1, this, "GPS", Color.Yellow);
            visGPS.functionDepth = 2;
            visGPS.mainFontDepth = 10;
            visGPS.functions.Add(new Function("latitude", Color.Cyan));
            visGPS.functions.Add(new Function("longitude", Color.Orange));
            visGPS.lightsOn = true;
            ParameterVisualizer visV = new ParameterVisualizer(pictureBox2, this, "Velosity", Color.Yellow);
            visV.functionDepth = 2;
            visV.mainFontDepth = 10;
            visV.functions.Add(new Function("Vx", Color.Pink));
            visV.functions.Add(new Function("Vy", Color.Yellow));
            visV.functions.Add(new Function("Vz", Color.LimeGreen));
            visV.lightsOn = true;

            try
            {
                // получаем адреса для запуска сокета
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(textBox4.Text), Convert.ToInt32(textBox3.Text));

                // создаем сокет
                Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                List<double[]> m = new List<double[]>();
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                log("Сервер запущен. Ожидание подключений...");



                Task listen = new Task(() =>
                {
                    while (true)
                    {
                        Socket handler = listenSocket.Accept();
                        log("Новое одключение");
                        while (true)
                        {
                            try
                            {
                                // получаем сообщение
                                StringBuilder builder = new StringBuilder();
                                int bytes = 0; // количество полученных байтов
                                byte[] data = new byte[256]; // буфер для получаемых данных

                                do
                                {
                                    bytes = handler.Receive(data);
                                    builder.Append(Encoding.Default.GetString(data, 0, bytes));
                                }
                                while (handler.Available > 0);

                                double[] p = new double[13];

                                string s = builder.ToString();
                                if (s == "Disconnect")
                                {
                                    handler.Shutdown(SocketShutdown.Both);
                                    handler.Close();
                                    log("Подключение разорвано");
                                    break;
                                }
                                try
                                {
                                    var subLines = s.Split(';');
                                    if (subLines.Length > 6)
                                    {
                                        s = buildString(subLines);
                                        var strings = new string[1];
                                        strings[0] = s;
                                        File.AppendAllLines(logPath, strings);

                                        double Ax = Convert.ToDouble(subLines[0]);
                                        double Ay = Convert.ToDouble(subLines[1]);
                                        double Az = Convert.ToDouble(subLines[2]);

                                        double lat = Convert.ToDouble(subLines[11]);
                                        double longt = Convert.ToDouble(subLines[12]);

                                        double Vx = Convert.ToDouble(subLines[6]);
                                        double Vy = Convert.ToDouble(subLines[7]);
                                        double Vz = Convert.ToDouble(subLines[8]);

                                        double cx = Convert.ToDouble(subLines[9]);
                                        double cy = Convert.ToDouble(subLines[10]);

                                        p[0] = Ax;
                                        p[1] = Ay;
                                        p[2] = Az;

                                        p[3] = lat;
                                        p[4] = longt;

                                        p[5] = Vx;
                                        p[6] = Vy;
                                        p[7] = Vz;

                                        p[8] = cx;
                                        p[9] = cy;

                                        m.Add(p);

                                        if (m.Count > window * 10)
                                        {
                                            double sum = 0;
                                            for (int i = 0; i < window; i++)
                                            {
                                                sum += m[m.Count - i - 1][2];
                                            }
                                            double AVGlow = sum / window;
                                            sum = 0;
                                            for (int i = 0; i < window; i++)
                                            {
                                                sum += (AVGlow - m[m.Count - i - 1][2]) * (AVGlow - m[m.Count - i - 1][2]);
                                            }
                                            double StdDevHighFric = Math.Sqrt(sum / window);

                                            sum = 0;
                                            for (int i = 0; i < window * 10; i++)
                                            {
                                                sum += m[m.Count - i - 1][2];
                                            }
                                            double AVGhigh = sum / (window * 10);
                                            sum = 0;
                                            for (int i = 0; i < window * 10; i++)
                                            {
                                                sum += (AVGhigh - m[m.Count - i - 1][2]) * (AVGhigh - m[m.Count - i - 1][2]);
                                            }
                                            double StdDevLowFric = Math.Sqrt(sum / (window * 10));


                                            visA.addPoint(AVGlow, "AvgLowFric Az");

                                            visA.addPoint(AVGlow + StdDevHighFric, "Avg+StdDevHighFric Az");
                                            visA.addPoint(AVGlow - StdDevHighFric, "Avg-StdDevHighFric Az");

                                            visA.addPoint(AVGhigh, "AvgHighFric Az");


                                            visA.addPoint(AVGhigh + StdDevLowFric, "Avg+StdDevLowFric Az");
                                            visA.addPoint(AVGhigh - StdDevLowFric, "Avg-StdDevLowFric Az");

                                            m.RemoveAt(0);




                                            double f = 190;

                                            double z_m = (0.5 + (Vz / f)) * 9.81;


                                            if (Math.Abs(Az) > z_m)
                                            {
                                                visA.drawString("Волнообразный износ", Brushes.Red, 24, 150, 150);
                                                _send_Line(DateTime.Now.ToLongTimeString() + "; Ш:" + lat.ToString() + '°' + "; Д:" + longt.ToString() + '°' + ": " + "Волнообразный износ");
                                            }
                                            if (StdDevHighFric > 1.5)
                                            {
                                                visA.drawString("Стык с дефектом", Brushes.Red, 24, 150, 250);
                                                _send_Line(DateTime.Now.ToLongTimeString() + "; Ш:" + lat.ToString() + '°' + "; Д:" + longt.ToString() + '°' + ": " + "Стык с дефектом");
                                            }
                                            else
                                            if (StdDevHighFric > 1)
                                            {
                                                visA.drawString("Стык", Brushes.Red, 24, 150, 200);
                                                _send_Line(DateTime.Now.ToLongTimeString() + "; Ш:" + lat.ToString() + '°' + "; Д:" + longt.ToString() + '°' + ": " + "Стык");
                                            }


                                            sum = 0;
                                            for (int i = 0; i < window * 10; i++)
                                            {
                                                sum += m[m.Count - i - 1][8];
                                            }
                                            AVGlow = sum / (window * 10);
                                            sum = 0;
                                            for (int i = 0; i < window * 10; i++)
                                            {
                                                sum += (AVGlow - m[m.Count - i - 1][8]) * (AVGlow - m[m.Count - i - 1][8]);
                                            }
                                            StdDevLowFric = Math.Sqrt(sum / (window * 10));

                                            if (StdDevLowFric > 10)
                                            {
                                                visA.drawString("Кривизна пути", Brushes.Red, 24, 150, 50);
                                                _send_Line(DateTime.Now.ToLongTimeString() + "; Ш:" + lat.ToString() + '°' + "; Д:" + longt.ToString() + '°' + ": " + "Кривизна пути");
                                            }

                                            sum = 0;
                                            for (int i = 0; i < window * 10; i++)
                                            {
                                                sum += m[m.Count - i - 1][9];
                                            }
                                            AVGlow = sum / (window * 10);
                                            sum = 0;
                                            for (int i = 0; i < window * 10; i++)
                                            {
                                                sum += (AVGlow - m[m.Count - i - 1][9]) * (AVGlow - m[m.Count - i - 1][9]);
                                            }
                                            StdDevLowFric = Math.Sqrt(sum / (window * 10));

                                            if (StdDevLowFric > 10)
                                            {
                                                visA.drawString("Кривизна пути", Brushes.Red, 24, 150, 50);
                                                _send_Line(DateTime.Now.ToLongTimeString() + "; Ш:" + lat.ToString() + '°' + "; Д:" + longt.ToString() + '°' + ": " + "Кривизна пути");
                                            }
                                            //Az+long+lat+cx
                                            _send_Line(';' + subLines[2] + ';' + subLines[11] + ';' + subLines[12] + ';' + subLines[10]);
                                        }

                                        //      visA.addPoint(Ax, "Ax");
                                        //      visA.addPoint(Ay, "Ay");
                                        visA.addPoint(Az, "Az");

                                        visGPS.addPoint(lat, "latitude");
                                        visGPS.addPoint(longt, "longitude");

                                        visV.addPoint(Vx, "Vx");
                                        visV.addPoint(Vy, "Vy");
                                        visV.addPoint(Vz, "Vz");

                                        visA.refresh();
                                        visGPS.refresh();
                                        visV.refresh();



                                        // log(s);
                                    }
                                }
                                catch { }
                            }
                            catch
                            {
                                //  log(socketExc.Message);
                                handler.Shutdown(SocketShutdown.Both);
                                handler.Close();
                                log("Подключение разорвано");
                                break;
                            }
                        }
                    }
                });
                listen.Start();
            }
            catch (Exception ex)
            {
                log(ex.Message);
            }
        }
    }
}





