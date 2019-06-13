﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
namespace Sensor
{

    public class ParameterVisualizer
    {
        public PictureBox picBox;
        public Graphics g;
        public Bitmap bitmap;
        public Form1 form1;
        public double mainFontDepth;
        public double functionDepth = 1;
        public int Xmax;
        public int Ymax;
        public int Ymin = 0;
        public int xZeroGap = 80;
        public double minY;
        public double maxY;
        public double dx;
        public List<Function> functions;
        public string label;
        public double zeroY;
        public double yUpGap;
        public double yDownGap;
        public bool multy;
        public bool enableGrid;
        public System.Timers.Timer needToRefresh;
        //высота диаграммы в пикселах
        public int H;

        public bool lightsOn = false;
        public ParameterVisualizer(PictureBox target_picBox, Form1 form1, string label, Color color)
        {
            multy = false;
            this.label = label;
            lastCount = 0;
            minY = double.MaxValue;
            maxY = double.MinValue;
            functions = new List<Function>();
            functions.Add(new Function(label, color));

            this.form1 = form1;
            this.picBox = target_picBox;
            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            Xmax = picBox.Width;
            Ymax = picBox.Height;
            functions[0].label = label;

            if ((Ymax / 10) < 14)
                mainFontDepth = Ymax / 10;
            else
                mainFontDepth = 14;

            yUpGap = (Ymax - Ymin) / 10;
            yDownGap = (Ymax - Ymin) / 20;
            zeroY = (Ymax - Ymin) / 2;
            needToRefresh = new System.Timers.Timer();
            needToRefresh.Elapsed += new System.Timers.ElapsedEventHandler(checkForNewPoints);
            needToRefresh.AutoReset = true;
            needToRefresh.Interval = 250;
           // needToRefresh.Start();
            refresh();
        }
        public void addPoint(double y)
        {
            point point = new point();
            point.x = functions[0].points.Count + 1;
            point.y = y;
            functions[0].points.Add(point);
        }
        public void addPoint(double y, string name, string mark)
        {
            // if (functions.Count == 1 && name != functions[0].label && functions[0].label == label)
            //     functions[0].label = name;
            for (int i = 0; i < functions.Count; i++)
            {
                if (functions[i].label == name)
                {
                    point point = new point();
                    point.x = functions[i].points.Count + 1;
                    point.y = y;
                    point.mark = mark;
                    functions[i].points.Add(point);
                    goto addPointOver;
                }
            }

            Random r = new Random();
        addFunction:
            try
            {
                functions.Add(new Function(name, Color.FromArgb(255, functions[0].color.R + r.Next(-255, 255), functions[0].color.G + r.Next(-255, 255), functions[0].color.B + r.Next(-255, 255))));
                point point = new point();
                point.x = functions[functions.Count - 1].points.Count + 1;
                point.y = y;
                point.mark = mark;
                functions[functions.Count - 1].points.Add(point);
            }
            catch { goto addFunction; }
        addPointOver:;
        }
        public void addPoint(double y, string name)
        {
            addPoint(y, name, "");
        }
        public void addPoint(double inc, double y)
        {
            point point = new point();
            point.x = inc;
            point.y = y;
            functions[0].points.Add(point);
        }

        public Bitmap multyRefresh(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            g = Graphics.FromImage(bitmap);

            if (functions[0].points != null)
            {
                int maxPointsCount = 0;
                int maxPointsCountI = 0;
                for (int i = 0; i < functions.Count; i++)
                {
                    if (functions[i].points.Count > maxPointsCount)
                    {
                        maxPointsCount = functions[i].points.Count;
                        maxPointsCountI = i;
                    }
                }
                foreach (Function function in functions)
                {
                    for (int i = 0; i < function.points.Count; i++)
                    {
                        if (function.points[i].y > maxY)
                            maxY = function.points[i].y;
                        if (function.points[i].y < minY)
                            minY = function.points[i].y;
                    }
                }
                dx = Convert.ToDouble(Xmax - (Xmax / 10)) / Convert.ToDouble(functions[maxPointsCountI].points.Count + 1);
                drawStatic(dx);


                foreach (Function function in functions)
                {
                    if (function.points.Count > 1)
                    {

                        for (int i = 1; i < function.points.Count; i++)
                        {
                            if (minY == maxY)
                            {
                                drawLine(function.color, functionDepth,
                                xZeroGap + dx * (i - 1), Ymin + (Ymax / 2),
                                xZeroGap + dx * i, Ymin + (Ymax / 2));
                                if (function.points[i].mark != function.points[i - 1].mark || i == 2)
                                    drawStringVertical(function.points[i].mark, mainFontDepth, xZeroGap + dx * i, Ymin + (Ymax / 2));
                            }
                            else
                            {
                                drawLine(function.color, functionDepth,
                                  xZeroGap + dx * (i - 1), Ymin + Ymax - (((Ymax - yUpGap) * (function.points[i - 1].y - minY)) / (maxY - minY) + yDownGap),
                                  xZeroGap + dx * i, Ymin + Ymax - (((Ymax - yUpGap) * (function.points[i].y - minY)) / (maxY - minY) + yDownGap));

                                if (function.points[i].mark != function.points[i - 1].mark || i == 2)
                                    drawStringVertical(function.points[i].mark, mainFontDepth,
                                    xZeroGap + dx * i, Ymin + Ymax - (((Ymax - yUpGap) * (function.points[i].y - minY)) / (maxY - minY) + yDownGap));
                            }
                        }
                    }
                }
            }
            return bitmap;
        }
        public void refresh()
        {

            if (functions[0].points != null)
            {
                int maxPointsCount = 0;
                int maxPointsCountI = 0;
                for (int i = 0; i < functions.Count; i++)
                {
                    if (functions[i].points.Count > maxPointsCount)
                    {
                        maxPointsCount = functions[i].points.Count;
                        maxPointsCountI = i;
                    }
                }
                foreach (Function function in functions)
                {
                    for (int i = 0; i < function.points.Count; i++)
                    {
                        if (function.points[i].y > maxY)
                            maxY = function.points[i].y;
                        if (function.points[i].y < minY)
                            minY = function.points[i].y;
                    }
                }
                dx = Convert.ToDouble(Xmax - xZeroGap) / Convert.ToDouble(functions[maxPointsCountI].points.Count + 1);
                drawStatic(dx);


                foreach (Function function in functions)
                {
                    if (function.points.Count > 1)
                    {

                        for (int i = 1; i < function.points.Count; i++)
                        {
                            if (minY == maxY)
                            {
                                drawLine(function.color, functionDepth,
                                xZeroGap + dx * (i - 1), Ymin + (Ymax / 2),
                                xZeroGap + dx * i, Ymin + (Ymax / 2));
                                if (function.points[i].mark != function.points[i - 1].mark || i == 2)
                                    drawStringVertical(function.points[i].mark, mainFontDepth, xZeroGap + dx * i, Ymin + (Ymax / 2));
                            }
                            else
                            {
                                drawLine(function.color, functionDepth,
                                  xZeroGap + dx * (i - 1), Ymin + Ymax - (((Ymax - yUpGap) * (function.points[i - 1].y - minY)) / (maxY - minY) + yDownGap),
                                  xZeroGap + dx * i, Ymin + Ymax - (((Ymax - yUpGap) * (function.points[i].y - minY)) / (maxY - minY) + yDownGap));

                                if (function.points[i].mark != function.points[i - 1].mark || i == 2)
                                    drawStringVertical(function.points[i].mark, mainFontDepth,
                                    xZeroGap + dx * i, Ymin + Ymax - (((Ymax - yUpGap) * (function.points[i].y - minY)) / (maxY - minY) + yDownGap));
                            }

                        }

                        if (function.points.Count > 50)
                        {
                            function.points.RemoveAt(0);
                        }
                    }
                }
            }


            picBox.Image = bitmap;
            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
        }


        private void drawStatic(double dx)
        {

            if (functions.Count > 1)
                for (int i = 1; i < functions.Count; i++)
                {
                    if (Ymin + i * (mainFontDepth * 0.8) + (i * mainFontDepth * 0.3) < Ymin + Ymax)
                    {
                        drawLine(functions[i].color, mainFontDepth * 0.8, Xmax - Xmax / 30, Ymin + i * (mainFontDepth * 0.8) + (i * mainFontDepth * 0.3), Xmax, Ymin + i * (mainFontDepth * 0.8) + (i * mainFontDepth * 0.3));
                        drawString(functions[i].label, (mainFontDepth * 0.8), Xmax - Xmax / 30 - functions[i].label.Length * (mainFontDepth * 0.8), Ymin + i * (mainFontDepth * 0.8) - (mainFontDepth * 0.8) + (i * mainFontDepth * 0.3));
                    }
                }

            drawString(label, mainFontDepth, Xmax / 2 - (label.Length * mainFontDepth / 2), Ymin);


            //   drawLine(Color.White, 2, xZeroGap, 0, xZeroGap, Ymax);


            if (minY != maxY)
            {
                for (double i = minY; i <= maxY; i = i + Math.Abs(((maxY - minY) / 6)))
                {
                    if (i > 0)
                    {
                        if (i.ToString().Length > Convert.ToInt16(xZeroGap / mainFontDepth))
                            drawString(" " + i.ToString().Substring(0, Convert.ToInt16(xZeroGap / mainFontDepth)), mainFontDepth, 0, Ymin + Ymax - (((Ymax - yUpGap) * (i - minY)) / (maxY - minY) + yDownGap) - mainFontDepth);
                        else
                            drawString(" " + i.ToString(), mainFontDepth, 0, Ymin + Ymax - (((Ymax - yUpGap) * (i - minY)) / (maxY - minY) + yDownGap) - mainFontDepth);
                    }
                    else
                    {
                        if (i.ToString().Length > Convert.ToInt16(xZeroGap / mainFontDepth) + 1)
                            drawString(i.ToString().Substring(0, Convert.ToInt16(xZeroGap / mainFontDepth) + 1), mainFontDepth, 0, Ymin + Ymax - (((Ymax - yUpGap) * (i - minY)) / (maxY - minY) + yDownGap) - mainFontDepth);
                        else
                            drawString(i.ToString(), mainFontDepth, 0, Ymin + Ymax - (((Ymax - yUpGap) * (i - minY)) / (maxY - minY) + yDownGap) - mainFontDepth);
                    }
                    drawLine(Color.White, functionDepth,
                             xZeroGap - 5, Ymin + Ymax - (((Ymax - yUpGap) * (i - minY)) / (maxY - minY) + yDownGap),
                             xZeroGap + 5, Ymin + Ymax - (((Ymax - yUpGap) * (i - minY)) / (maxY - minY) + yDownGap));
                    drawLine(Color.FromArgb(50, Math.Abs(picBox.BackColor.R - 255), Math.Abs(picBox.BackColor.R - 255), Math.Abs(picBox.BackColor.R - 255)), 1,
                           xZeroGap, Ymin + Ymax - (((Ymax - yUpGap) * (i - minY)) / (maxY - minY) + yDownGap),
                           Xmax, Ymin + Ymax - (((Ymax - yUpGap) * (i - minY)) / (maxY - minY) + yDownGap));

                }
                if (minY <= 0)
                {
                    drawLine(Color.White, 1,
                             xZeroGap, Ymin + Ymax - (((Ymax - yUpGap) * (0 - minY)) / (maxY - minY) + yDownGap),
                             Xmax, Ymin + Ymax - (((Ymax - yUpGap) * (0 - minY)) / (maxY - minY) + yDownGap));
                    drawString("    0".ToString(), mainFontDepth, 0, Ymin + Ymax - (((Ymax - yUpGap) * (0 - minY)) / (maxY - minY) + yDownGap) - mainFontDepth);
                }

            }
            else
            {

                if (maxY.ToString().Length > Convert.ToInt16(xZeroGap / mainFontDepth))
                    drawString(" " + maxY.ToString().Substring(0, Convert.ToInt16(xZeroGap / mainFontDepth)), mainFontDepth, 0, Ymin + (Ymax / 2) - mainFontDepth);
                else
                    drawString(" " + maxY.ToString(), mainFontDepth, 0, Ymin + (Ymax / 2) - mainFontDepth);
            }

            if (enableGrid)
            {
                for (int i = 0; i < Xmax / dx; i++)
                {
                    drawLine(Color.FromArgb(50, Math.Abs(picBox.BackColor.R - 255), Math.Abs(picBox.BackColor.R - 255), Math.Abs(picBox.BackColor.R - 255)), 1, xZeroGap + dx * i, Ymin, xZeroGap + dx * i, Ymin + Ymax - (mainFontDepth * 2));

                    if (!multy)
                    {
                        if (dx < 10)
                            drawString(i.ToString(), (dx - 1), xZeroGap + dx * i - (mainFontDepth / 2), Ymin + Ymax - mainFontDepth * 2);
                        else
                            drawString(i.ToString(), 10, xZeroGap + dx * i - (mainFontDepth / 2), Ymin + Ymax - mainFontDepth * 2);

                    }
                }
            }
            drawLine(Color.FromArgb(250, 0, 0, 255), 1, 0, Ymin + Ymax, Xmax, Ymin + Ymax);
            //   drawLine(Color.FromArgb(155, 55, 55, 55), 3, 0, Ymin + Ymax, Xmax, Ymin + Ymax);
        }

        public delegate void DrawStringDelegate(string s, double depth, double x, double y);
        public void drawString(string s, double depth, double x, double y)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new DrawStringDelegate(drawString), new Object[] { s, depth, x, y }); // вызываем эту же функцию обновления состояния, но уже в UI-потоке
            }
            else
            {
                if (y > picBox.Height)
                    picBox.Height = Convert.ToInt16(y);
                else
                    try
                    {
                        if (lightsOn)
                        {
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.Black, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                        }
                        else
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                    }
                    catch { }
            }
        }
        public void drawStringVertical(string s, double depth, double x, double y)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new DrawStringDelegate(drawString), new Object[] { s, depth, x, y }); // вызываем эту же функцию обновления состояния, но уже в UI-потоке
            }
            else
            {
                if (y > picBox.Height)
                    picBox.Height = Convert.ToInt16(y);
                else
                    try
                    {
                        if (lightsOn)
                        {
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.Black, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))), new StringFormat(StringFormatFlags.DirectionVertical));
                        }
                        else
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))), new StringFormat(StringFormatFlags.DirectionVertical));
                    }
                    catch { }
            }
        }
        public delegate void DrawStringDelegate2(string s, Brush brush, double depth, double x, double y);
        /////////////////////////////////Brushes.[Color]
        public void drawString(string s, Brush brush, double depth, double x, double y)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new DrawStringDelegate2(drawString), new Object[] { s, brush, depth, x, y }); // вызываем эту же функцию обновления состояния, но уже в UI-потоке
            }
            else
            {

                if (y > picBox.Height)
                    picBox.Height = Convert.ToInt16(y);
                else
                    try
                    {
                        if (lightsOn)
                        {
                            if (brush == Brushes.White)
                                g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.Black, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                            else
                                g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), brush, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));


                        }
                        else
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), brush, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                    }
                    catch { }
            }
        }

        public delegate void DrawLineDelegate(Color col, double depth, double x1, double y1, double x2, double y2);
        public void drawLine(Color col, double depth, double x1, double y1, double x2, double y2)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new DrawLineDelegate(drawLine), new Object[] { col, depth, x1, y1, x2, y2 }); // вызываем эту же функцию обновления состояния, но уже в UI-потоке
            }
            else
            {

                if (y1 > picBox.Height)
                    picBox.Height = Convert.ToInt16(y1);
                else
                if (y2 > picBox.Height)
                    picBox.Height = Convert.ToInt16(y2);
                else
                    g.DrawLine(new Pen(col, Convert.ToInt16(depth)), Convert.ToInt16(Math.Round(x1)), Convert.ToInt16(Math.Round(y1)), Convert.ToInt16(Math.Round(x2)), Convert.ToInt16(Math.Round(y2)));
            }
        }

        private int lastCount;
        public void checkForNewPoints(object sender, EventArgs e)
        {
            int count = 0;
            foreach (Function function in functions)
            {
                if (function.points != null)
                {
                    count = count + function.points.Count;
                }
            }

            if (count > lastCount)
            {
                refresh();
                lastCount = count;
            }
        }

    }
    public class Function
    {
        public Function(string label, Color color)
        {
            this.label = label;
            this.color = color;
            points = new List<point>();
        }
        public string label;
        public Color color;
        public List<point> points;
    }
    public class point
    {
        public string mark;

        public double x;
        public double y;
    }
}

