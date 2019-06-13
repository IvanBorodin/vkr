using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
         pictureBox2.Image = Image.FromFile("t.bmp");
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
                richTextBox.AppendText(s);
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

        private void Button1_Click(object sender, EventArgs e)
        {
            ParameterVisualizer visA = new ParameterVisualizer(pictureBox1, this, "Az", Color.Black);
            visA.mainFontDepth = 10;
            visA.functionDepth = 1;
            // visA.functions.Add(new Function("Ax", Color.Red));
            //  visA.functions.Add(new Function("Ay", Color.Green));
            // visA.functions.Add(new Function("Az", Color.Black));
            visA.lightsOn = true;

            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(textBox5.Text), Convert.ToInt32(textBox4.Text));

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // связываем сокет с локальной точкой, по которой будем принимать данные
            listenSocket.Bind(ipPoint);

            // начинаем прослушивание
            listenSocket.Listen(10);

            Task listen = new Task(() =>
            {
                log("Ожидание подключения БИНС.. ");
                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    log("БИНС подключен!");
                    while (true)
                    {
                        /*  try
                          {*/
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
                        string[] sublines = s.Split(';');
                        if (s[0] == ';')
                        {
                            visA.addPoint(Convert.ToDouble(sublines[1]));

                            textBox2.Invoke(new Action(() => textBox2.Text = sublines[2]));
                            textBox3.Invoke(new Action(() => textBox3.Text = sublines[3]));

                            textBox1.Invoke(new Action(() => textBox1.Text = sublines[4]));
                            double cx = Convert.ToDouble(sublines[4]);
                            if ((cx > 5 & cx < 10) | (cx < -5 & cx > -10))
                            {
                                textBox1.Invoke(new Action(() => textBox1.BackColor = Color.Yellow));
                            }
                            else
                               if (cx > 10 | cx < -10)
                            {
                                textBox1.Invoke(new Action(() => textBox1.BackColor = Color.Red));
                            }
                            else
                                textBox1.Invoke(new Action(() => textBox1.BackColor = Color.White));


                            visA.refresh();
                        }
                        else
                        {
                            string msg = "";
                            if (s.Contains("Волнообразный износ"))
                            {
                                msg = "Волнообразный износ";
                            }
                            if (s.Contains("Стык с дефектом"))
                            {
                                msg = "Стык с дефектом";
                            }
                            if (s.Contains("Стык"))
                            {
                                msg = "Стык";
                            }
                            if (s.Contains("Кривизна пути"))
                            {
                                msg = "Кривизна пути";
                            }
                            Task alert = new Task(() => {
                                pictureBox2.Invoke(new Action(() => pictureBox2.Image = Image.FromFile("red.png")));
                                System.Threading.Thread.Sleep(3000);
                                pictureBox2.Invoke(new Action(() => pictureBox2.Image = Image.FromFile("t.bmp")));
                            });
                            alert.Start();
                            //visA.drawString(msg, Brushes.Red, 14, 50, 50);
                            // visA.functions[0].points[visA.functions[0].points.Count - 1].mark = msg;
                            log(s);
                            visA.refresh();
                        }
                        /* }
                         catch
                         {
                             //  log(socketExc.Message);
                             handler.Shutdown(SocketShutdown.Both);
                             handler.Close();
                             log("Подключение разорвано");
                             break;
                         }*/
                    }
                }
            });
            listen.Start();
        }
    }
}
