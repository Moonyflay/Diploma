using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Math;

namespace ElasticModulus
{
    public partial class FormMain : Form
    {
        int cell_in_row = 30;
        int cell_in_col = 30;
        byte cell_size_pix = 20;
        double cell_size = 2;
        List<double>[] cell_l_d = new List<double>[2];
        List<double> Pressure;
        double[] vol_frac = new double [1];
        Material[] Materials;
        public  static Random sluchai = new Random();
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
           
            vol_frac[0] = 1.00;
            //vol_frac[1] = 1 - vol_frac[0];
           
            double mind = 5;
            double maxd = 30;
            double mx = 8;
            double sigma = 0.15;
            Materials = new Material[1];
            Materials[0] = new Material(0.5, 7*Pow(10, 10), 200*Pow(10,8));

            Pressure = new List<double>();
            for (int i = 0; i < cell_in_col * cell_in_row; i++)
            {
                Pressure.Add(0);
            }  

            for (int i = 0; i < cell_l_d.Length; i++)
            {
                cell_l_d[i] = new List<double>();
            }
            for (int i = 0; i< cell_in_col*cell_in_row; i++) //при отсутствии пор!
            {
                cell_l_d[0].Add(0);
                cell_l_d[1].Add(0);
            }


            //Map map = new Map(vol_frac, cell_in_row, cell_in_col, mind, maxd, mx, sigma, cell_size);
            Map map = new Map(vol_frac, cell_in_row, cell_in_col, cell_size);
            

            Bitmap bmp;
            byte sch_max = 38;
            for (int sch = 0; sch < sch_max; sch++)
            {
                
                bmp = new Bitmap(cell_size_pix * cell_in_row + 5, cell_in_col * cell_size_pix + 15, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    Rectangle rect;
                    double max_P;
                    if (Pressure.Max() > Abs(Pressure.Min())) max_P = Pressure.Max();
                    else max_P = Abs(Pressure.Min());
                    for (int k = 0; k < map.Component.Length; k++)
                        for (int i = 0; i < map.Component[k].Count; i++)
                        {
                            int x = 0; int y = 0;
                            map.Num_Decrypt(map.Component[k][i], ref x, ref y);

                            Count_dl(out double rec_da, out double rec_db, Materials[k].elastic_m, Materials[k].p_ratio, Pressure[map.Component[k][i]], cell_size * Pow(10, -9));
                            double convert_size = cell_size_pix / cell_size * Pow(10, 9);
                            int rec_x = (int)(x * cell_size_pix + 5/* - Abs(rec_da) * convert_size / 2*/);
                            int rec_y = (int)(y * cell_size_pix + 5/*- Abs(rec_db) * convert_size / 2*/);
                            int rec_a = (int)(cell_size_pix + rec_da * convert_size);
                            int rec_b = (int)(cell_size_pix + rec_db * convert_size);
                            rect = new Rectangle(rec_x, rec_y, rec_a - 5, rec_b - 5);
                            //rect = new Rectangle(x* cell_size_pix + 5, y* cell_size_pix+5, cell_size_pix - 10 , cell_size_pix-10);
                            Color rect_color;
                            if (Abs(Pressure[map.Component[k][i]]) >= Materials[k].compr_strength) 
                            {
                                //double alpha = 187 / 255.0;
                                //int red = (int)(alpha * rect_color.R);
                                //int green =(int)( alpha * rect_color.G);
                                //int blue = (int)(alpha * rect_color.B);
                                //rect_color = Color.FromArgb(red, green, blue);
                                rect_color = Color.Gray;
                            }
                            else rect_color = Color_Define(Pressure[map.Component[k][i]], max_P);

                            g.FillRectangle(new SolidBrush(rect_color), rect); continue;

                            switch (k)
                            {
                                case 0: { g.FillRectangle(new SolidBrush(Color.LightSkyBlue), rect); break; }
                                case 1: { g.FillRectangle(new SolidBrush(Color.BurlyWood), rect); break; }
                                default: { g.FillRectangle(new SolidBrush(Color.Peru), rect); break; }
                            }


                        }

                    //// Функция отображения путей. Не забыть убрать!
                    //for (int i = 0; i < map.Path.Count; i++)
                    //{
                    //    Color col = Color.FromArgb(sluchai.Next(256), sluchai.Next(256), sluchai.Next(256));
                    //    for (int j = 0; j < map.Path[i].Count; j++)
                    //    {
                    //        int x = 0; int y = 0;
                    //        map.Num_Decrypt(map.Path[i][j], ref x, ref y);
                    //        rect = new Rectangle(x * cell_size_pix/*+1, y * cell_size_pix/*+1*/, cell_size_pix/*-2*/, cell_size_pix/*-2*/);

                    //        g.FillRectangle(new SolidBrush(col), rect);
                    //    }
                    //}

                }
                pictureBoxMain.Image?.Dispose();
                pictureBoxMain.Image = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height ), bmp.PixelFormat);
                bmp?.Dispose();
                pictureBoxMain.Refresh();
                Set_P(map);
                if(sch < 0.3*sch_max) System.Threading.Thread.Sleep(25);
                else System.Threading.Thread.Sleep(10);

            }

        }


        void Count_dl(out double dl, out double dd, double E, double mu, double P, double l, double d = -1) 
        {
            if (d <= 0) d = l; // d = l (клетки квадратные)
            dl = P * l / E; // из Е = Р * l/dl
            dd = -mu * d * dl/l; // mu = - dd/d * l/dl        
            if (P < 0)
            {
                dd = -P * d / E; // из Е = Р * l/dl
                dl = -mu * l * dd / d;
            }
        }

        void Set_P(Map map) // временная функция. Задает напряженияпо диагонали: сверху слева идет большее давление в горизонтальнойплоскости, справа снизу - в вертикальной 
        { 
            double maxP = 5*Pow(10, 9);
            double maxx = cell_in_row;
            double maxy = cell_in_col;
            // только для диагонали
            //for (int x = 0; x < maxx; x++) 
            //    for (int y = 0; y < maxy; y++ ) //
            //    {
            //            int num = map.Num_Crypt(x, y);
            //            Pressure[num] = maxP * (maxx + maxy-2*(x+y+1))/(maxx+maxy-2);
            //    }

            // можно еределать под горизонтальный/вертикальный градиент,  убрав/добавив i около y/x
            for (int x = 0; x < maxx; x++)
                for (int i = (int)-maxx; i < maxx; i++) //
                {
                    int y = (int)((maxy - 1) / (maxx - 1) * x);
                    int num;
                    if ((i >= 0 && y + i < maxy) || (i < 0 && y + i >= 0))
                    {
                        num = map.Num_Crypt(x, y + i);
                        if (Abs(Pressure[num]) < Materials[0].compr_strength) //
                        Pressure[num] += maxP * ((maxx - 2 * x - 1) / (maxx - 1) + (maxy - 2 * (i + y) - 1) / (maxy - 1)) / 2;
                    }
                    else continue;
                }

        }

        Color Color_Define(double x, double max)
        {
            double ratio = x/max;
            int red = 0;
            int green = 0;
            int blue = 0;
            if (ratio <= -0.5) 
            {
                blue = 255;
                green = (int)(235 * (1 - (Abs(ratio)-0.5) / 0.5));
            }
            if (ratio > -0.5 && ratio < 0) 
            {
                
                green = 235;
                blue = (int)(255 * (Abs(ratio)/0.5));
            }
            if (ratio >= 0 && ratio < 0.5)
            {
                red = (int)(255 * ratio/0.5);
                green = 235;
                
            }
            if (ratio >= 0.5)
            {
                red = 255;
                green = (int)(235 *(1 - (ratio-0.5) / 0.5));
            }

            return Color.FromArgb(red, green, blue);
        }
    
    }
}
