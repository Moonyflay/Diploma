﻿using System;
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
        Map map;
        int cell_in_row = 81;
        int cell_in_col = 81;
        byte cell_size_pix = 20;
        double cell_size = 2;
        List<double>[] cell_l_d = new List<double>[2];
        List<double> Pressure;

        List<double> Pressure_x;
        List<double> Pressure_y;
        List<double> Angle;
        
        double[] vol_frac;
        Material[] Materials;
        public  static Random sluchai = new Random();

        int sq_in_rad = 40; // Число клеток в радиусе
        double fading = 0.5;
        double transpass = 0.0;
        double transpass_ = 0.0;

        delegate Color color_def_f(double _a, double _b);
        public FormMain()
        {
            InitializeComponent();

            vol_frac = new double[(int)nUD_MaterialsNum.Value+1];
            Materials = new Material[(int)nUD_MaterialsNum.Value];

            sq_in_rad = (int)nUD_SquareNum.Value;
            cell_in_col = sq_in_rad * 2 + 1;
            cell_in_row = cell_in_col;
            
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            vol_frac[0] = 0;
            vol_frac[1] = 1;
            //vol_frac[2] = 1- vol_frac[1];

            double mind = 5;
            double maxd = 30;
            double mx = 8;
            double sigma = 0.15;
            
            Materials[0] = new Material(0.2, 7*Pow(10, 10), 20*Pow(10,8));
           // Materials[1] = new Material(0.5, 1 * Pow(10, 9), 20 * Pow(10, 8));

            Pressure = new List<double>();
            Pressure_x = new List<double>();
            Pressure_y = new List<double>();
            Angle = new List<double>();
            for (int i = 0; i < cell_in_col * cell_in_row; i++)
            {
                Pressure.Add(0);
                Pressure_x.Add(0);
                Pressure_y.Add(0);
                Angle.Add(0);
            }  

            for (int i = 0; i < cell_l_d.Length; i++)
            {
                cell_l_d[i] = new List<double>();
            }
            for (int i = 0; i< cell_in_col*cell_in_row; i++) //при отсутствии пор!
            {
                cell_l_d[0].Add(cell_size * Pow(10, -9));
                cell_l_d[1].Add(cell_size * Pow(10, -9));
            }


            //Map map = new Map(vol_frac, cell_in_row, cell_in_col, mind, maxd, mx, sigma, cell_size);
            map = new Map(sq_in_rad, vol_frac, cell_in_row, cell_in_col, cell_size);
            

            int sch_max = 400;
            for (int sch = 0; sch < sch_max; sch++)
            {
                // switch
                if (rBBoth.Checked == true)
                    Display(map, Pressure, Color_Define_Alt);
                else if (rBsigma_x.Checked == true) Display(map, Pressure_x, Color_Define_Alt);
                else if (rBsigma_y.Checked == true) Display(map, Pressure_y, Color_Define_Alt);
                else Display(map, Angle, Color_Define_Alt);

                //            Count_dl(out double rec_da, out double rec_db, Materials[k-1].elastic_m, Materials[k-1].p_ratio, Pressure[map.Component[k][i]], cell_size * Pow(10, -9));
                //            if (sch == sch_max - 1)
                //            {
                //                cell_l_d[0][map.Component[k][i]] += rec_da;
                //                cell_l_d[1][map.Component[k][i]] += rec_db;
                //            }

                for (int tt = 0; tt < 2; tt++) // tt должно быть четным
                {
                   Interact(map);
                }

                //for (int tt = 0; tt < 1; tt++) 
                //{
                //   Interact_Alt(map);
                //}
               
                Apply_pressure(map, 10*Pow(10, 5));
                Count_Angle(map);
                if(sch < 0.3*sch_max) System.Threading.Thread.Sleep(0);
                else System.Threading.Thread.Sleep(0);

            }
            
        }


        void Display(Map map, List<double> Press,color_def_f Color_Def_F)
        {
            double max_P = Max_P();

            Bitmap bmp;
            bmp = new Bitmap(cell_size_pix * cell_in_row + 5, cell_in_col * cell_size_pix + 15, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            using (Graphics g = Graphics.FromImage(bmp)) 
            {
                Rectangle rect;

                for (int k = 1; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++) 
                    {
                        int x = 0; int y = 0;
                        map.Num_Decrypt(map.Component[k][i], ref x, ref y);

                        Count_dl(out double rec_da, out double rec_db, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[map.Component[k][i]], cell_size * Pow(10, -9));

                        double convert_size = cell_size_pix / cell_size * Pow(10, 9);
                        int rec_x = (int)(x * cell_size_pix + 5/* - Abs(rec_da) * convert_size / 2*/);
                        int rec_y = (int)(y * cell_size_pix + 5/*- Abs(rec_db) * convert_size / 2*/);
                        int rec_a = (int)(cell_size_pix + rec_da * convert_size);
                        int rec_b = (int)(cell_size_pix + rec_db * convert_size); 
                        rect = new Rectangle(rec_x, rec_y, rec_a - 5, rec_b - 5);
                        Color rect_color;
                        if (rBAngle.Checked == false && Abs(Press[map.Component[k][i]]) >= Materials[k - 1].compr_strength) // Abs(Pressure_x[map.Component[k][i]] + Pressure_y[map.Component[k][i]])
                        { 
                            rect_color = Color.Gray;
                        }
                        else rect_color = Color_Def_F(Press[map.Component[k][i]], max_P);  
                        g.FillRectangle(new SolidBrush(rect_color), rect); continue;
                    }
            }
                pictureBoxMain.Image?.Dispose();
                pictureBoxMain.Image = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
                bmp?.Dispose();
                pictureBoxMain.Refresh();
            
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

            // можно переделать под горизонтальный/вертикальный градиент,  убрав/добавив i около y/x
            for (int x = 0; x < maxx; x++)
                for (int i = (int)-maxx; i < maxx; i++) //
                {
                    int y = (int)((maxy - 1) / (maxx - 1) * x);
                    int num;
                    if ((i >= 0 && y + i < maxy) || (i < 0 && y + i >= 0))
                    {
                        num = map.Num_Crypt(x, y + i);
                        if (map.Component[0].Contains(num)) continue;
                        if (Abs(Pressure[num]) < Materials[0].compr_strength) //
                        Pressure[num] += maxP * ((maxx - 2 * x - 1) / (maxx - 1) + (maxy - 2 * (i + y) - 1) / (maxy - 1)) / 2;
                    }
                    else continue;
                }

        }

        private void buttonStart2_Click(object sender, EventArgs e)
        {
            vol_frac[0] = 0.000;
            vol_frac[1] = 0.40;
            vol_frac[2] = 1 - vol_frac[0] - vol_frac[1];

            //double mind = 1;
            //double maxd = 2;
            //double mx = 1;
            //double sigma = 0.15;
            //Map map = new Map(sq_in_rad, vol_frac, cell_in_row, cell_in_col, mind, maxd, mx, sigma, cell_size);

            Map map = new Map(sq_in_rad, vol_frac, cell_in_row, cell_in_col, cell_size);
            Bitmap bmp;
           
                bmp = new Bitmap(cell_size_pix * cell_in_row , cell_in_col * cell_size_pix, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    
                Rectangle rect;
                   
                  
                for (int k = 0; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++)
                    {
                        int x = 0; int y = 0;
                        map.Num_Decrypt(map.Component[k][i], ref x, ref y);
                     //   //if (x - sq_in_rad >= 0 && y - sq_in_rad <= 0)
                      //  //{
                            rect = new Rectangle(x * cell_size_pix, y * cell_size_pix, cell_size_pix, cell_size_pix);
                          //  Color rect_color;
                          //  if ((x - sq_in_rad) * (x - sq_in_rad) + (y - sq_in_rad) * (y - sq_in_rad) > sq_in_rad * sq_in_rad) { rect_color = Color.White; }
                          //  else rect_color = Color.Gray;

                        switch (k)
                        {
                            case 0: { g.FillRectangle(new SolidBrush(Color.LightGray), rect); break; }
                            case 1: { g.FillRectangle(new SolidBrush(Color.BurlyWood), rect); break; }
                            default: { g.FillRectangle(new SolidBrush(Color.Peru), rect); break; }
                        }
                       // g.FillRectangle(new SolidBrush(rect_color), rect); continue;
                      //  //}
                    }
                }
                pictureBoxMain.Image?.Dispose();
                pictureBoxMain.Image = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
                bmp?.Dispose();
                pictureBoxMain.Refresh();
                
               

            
        }

        void Apply_pressure(Map map, double app_press) 
        {
            //!!!!!!!!!!!!!!
            byte affected_cells = 2;//(byte)( cell_in_col*0.5); // Клетки, на которые действует пластина (отсчет ведется с края таблетки)
            //for (int y = 0; y < cell_in_col; y++)
            //    for (int x = 0; x < cell_in_row; x++)
            //    {
            //        if (map.Component[0].Contains(map.Num_Crypt(x, y)) == false)
                    
            //        {
            //            if (y < affected_cells || y > cell_in_col - affected_cells-1)
            //                Pressure[map.Num_Crypt(x, y)] += app_press;
            //            else 
            //            { 
            //                Pressure[map.Num_Crypt(x, y)] += app_press * Pow(transpass, y - affected_cells);
            //                Pressure[map.Num_Crypt(x, y)] += app_press * Pow(transpass, cell_in_col-y - affected_cells-1);
            //            }

            //        }
            //        //if (map.Component[0].Contains(map.Num_Crypt(x, cell_in_col - y - 1)) == false)
            //        //{ Pressure[map.Num_Crypt(x, cell_in_col - y - 1)] += app_press; }
            //    }

            int xmin = cell_in_row; int xmax = 0;
            for (int y = 0; y < affected_cells; y++)
                for (int x = 0; x < cell_in_row; x++)
                {
                    if (map.Component[0].Contains(map.Num_Crypt(x, y)) == false)
                    { 
                        Pressure[map.Num_Crypt(x, y)] += app_press;
                        Pressure_y[map.Num_Crypt(x, y)] -= app_press;
                        if (x > xmax) xmax = x;
                        if (xmin > x) xmin = x;
                    }
                    if (map.Component[0].Contains(map.Num_Crypt(x, cell_in_col - y - 1)) == false)
                    { 
                        Pressure[map.Num_Crypt(x, cell_in_col - y - 1)] += app_press;
                        Pressure_y[map.Num_Crypt(x, cell_in_col - y - 1)] -= app_press;
                        if (x > xmax) xmax = x;
                        if (xmin > x) xmin = x;
                    }
                }
            for (int y = affected_cells; y < cell_in_col - affected_cells; y++)
                for (int x = xmin; x <= xmax; x++)
                {
                    if (map.Component[0].Contains(map.Num_Crypt(x, y)) == false)
                    {                        
                            Pressure[map.Num_Crypt(x, y)] += app_press * Pow(transpass, y - affected_cells+1);
                            Pressure[map.Num_Crypt(x, y)] += app_press * Pow(transpass, cell_in_col - y - affected_cells);
                        Pressure_y[map.Num_Crypt(x, y)] -= app_press * Pow(transpass, y - affected_cells + 1);
                        Pressure_y[map.Num_Crypt(x, y)] -= app_press * Pow(transpass, cell_in_col - y - affected_cells);
                    }

                }



        }

        void Interact(Map map) 
        {
            List<double>[] cell_l_d_new = new List<double>[2];
            for (int i = 0; i < cell_l_d.Length; i++)
            {
                cell_l_d_new[i] = new List<double>();
            }
            for (int i = 0; i < cell_in_col * cell_in_row; i++) //при отсутствии пор!
            {
                cell_l_d_new[0].Add(cell_size * Pow(10, -9)); // -9 заменить на параметр!
                cell_l_d_new[1].Add(cell_size * Pow(10, -9));
            }

            List<double> Pressure_new = new List<double>();
            List<double> Pressure_new_x = new List<double>();
            List<double> Pressure_new_y = new List<double>();

            for (int i = 0; i < cell_in_col * cell_in_row; i++)
            {
                Pressure_new.Add(0);
                Pressure_new_x.Add(0);
                Pressure_new_y.Add(0);
            }

            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int x = 0; int y = 0;
                    int num = map.Component[k][i];
                    map.Num_Decrypt(map.Component[k][i], ref x, ref y);
                    byte neigh = 4;
                    //если раскомментить 4 строки внизу, то все поломается =)
                    //if (x - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x - 1, y)) == true) neigh--;
                    //if (x + 1 < cell_in_row && map.Component[0].Contains(map.Num_Crypt(x + 1, y)) == true) neigh--;
                    //if (y - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x, y - 1)) == true) neigh--;
                    //if (y + 1 < cell_in_col && map.Component[0].Contains(map.Num_Crypt(x, y + 1)) == true) neigh--;

                    if (x - 1 >= 0)
                    {
                        if (map.Component[0].Contains(map.Num_Crypt(x - 1, y)) == true) { Pressure_new[num] -= Pressure[num] / neigh * fading; Pressure_new_x[num] -= Pressure[num] / neigh * fading; }
                        else
                        { Pressure_new[num] -= Pressure[map.Num_Crypt(x - 1, y)] / neigh; Pressure_new_x[num] -= Pressure[map.Num_Crypt(x - 1, y)] / neigh; }
                    }
                    else { Pressure_new[num] -= Pressure[num] / neigh * fading; Pressure_new_x[num] -= Pressure[num] / neigh * fading; }

                    if (x + 1 < cell_in_row)
                    {
                        if (map.Component[0].Contains(map.Num_Crypt(x + 1, y)) == true) { Pressure_new[num] -= Pressure[num] / neigh * fading; Pressure_new_x[num] -= Pressure[num] / neigh * fading; }
                        else { Pressure_new[num] -= Pressure[map.Num_Crypt(x + 1, y)] / neigh; Pressure_new_x[num] -= Pressure[map.Num_Crypt(x + 1, y)] / neigh; }
                    }
                    else { Pressure_new[num] -= Pressure[num] / neigh * fading; Pressure_new_x[num] -= Pressure[num] / neigh * fading; }
                    if (y - 1 >= 0)
                    {
                        if (map.Component[0].Contains(map.Num_Crypt(x, y - 1)) == true) { Pressure_new[num] -= Pressure[num] / neigh * fading; Pressure_new_y[num] -= Pressure[num] / neigh * fading; }
                        else { Pressure_new[num] -= Pressure[map.Num_Crypt(x, y - 1)] / neigh; Pressure_new_y[num] -= Pressure[map.Num_Crypt(x, y - 1)] / neigh; }
                    }
                    else { Pressure_new[num] -= Pressure[num] / neigh * fading; Pressure_new_y[num] -= Pressure[num] / neigh * fading; }

                    if (y + 1 < cell_in_col)
                    {
                        if (map.Component[0].Contains(map.Num_Crypt(x, y + 1)) == true) { Pressure_new[num] -= Pressure[num] / neigh * fading; Pressure_new_y[num] -= Pressure[num] / neigh * fading; }
                        else { Pressure_new[num] -= Pressure[map.Num_Crypt(x, y + 1)] / neigh; Pressure_new_y[num] -= Pressure[map.Num_Crypt(x, y + 1)] / neigh; }
                    }
                    else { Pressure_new[num] -= Pressure[num] / neigh * fading; Pressure_new_y[num] -= Pressure[num] / neigh * fading; }

                    //Pressure_new[num] = Pressure[num]*(1- fading) + Pressure_new[num] * fading;

                    Count_dl(out double rec_da, out double rec_db, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure_new[num], cell_size * Pow(10, -9));

                    cell_l_d_new[0][num] += rec_da;
                    cell_l_d_new[1][num] += rec_db;
                    
                }
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];
                    Pressure[num] = Pressure_new[num];
                    Pressure_x[num] = Pressure_new_x[num];
                    Pressure_y[num] = Pressure_new_y[num];
                    cell_l_d[0][num] = cell_l_d_new[0][num];
                    cell_l_d[1][num] = cell_l_d_new[1][num];

                }


        }

        void Interact_Alt(Map map)
        {
            List<double>[] cell_l_d_new = new List<double>[2];
            List<double> Pressure_new = new List<double>();
            for (int i = 0; i < cell_in_col * cell_in_row; i++) //при отсутствии пор!
            {
                Pressure_new.Add(0);
            }


             for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int x = 0; int y = 0;
                    int num = map.Component[k][i];
                    map.Num_Decrypt(map.Component[k][i], ref x, ref y);

                    bool[] _dir = new bool[4] { true, true, true, true };
                    int[] dir = new int[] { 0, 0, 0, 0 };

                    for (int ss = 0; ss < _dir.Length; ss++)
                        while (_dir[ss] == true)
                        {
                            switch (ss)
                            {
                                case 0:  // влево
                                    {
                                        if (x - 1 - dir[ss] >= 0 && map.Component[0].Contains(map.Num_Crypt(x - 1 - dir[ss], y)) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false;  }
                                        break;
                                    }
                                case 1: // вправо
                                    {
                                        if (x + 1 + dir[ss] < cell_in_row && map.Component[0].Contains(map.Num_Crypt(x + 1 + dir[ss], y)) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false;  }
                                        break;
                                    }
                                case 2: // вниз
                                    {
                                        if (y - 1 - dir[ss] >= 0 && map.Component[0].Contains(map.Num_Crypt(x , y - 1 - dir[ss])) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false;  }
                                        break;
                                    }
                                case 3: // вверх
                                    {
                                        if (y + 1 + dir[ss] < cell_in_col && map.Component[0].Contains(map.Num_Crypt(x , y + 1 + dir[ss])) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false;  }
                                        break;
                                    }
                            } 
                        }
                    byte neigh = 4;
                    for (int ss = 0; ss < _dir.Length; ss++)
                        if (dir[ss] < 1) neigh--;
                    for (int ss = 0; ss < _dir.Length; ss++)
                    {
                        //if (dir[ss] < 1) neigh--;
                        for (int dd = 1; dd <= dir[ss]; dd++)
                        {
                            
                                switch (ss)
                                {
                                    case 0:
                                        {
                                            Pressure_new[map.Num_Crypt(x - dd, y)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                            Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                            break;
                                        }
                                    case 1:
                                        {
                                            Pressure_new[map.Num_Crypt(x + dd, y)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                            Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                            break;
                                        }
                                    case 2:
                                        {
                                            Pressure_new[map.Num_Crypt(x, y - dd)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                            Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                            break;
                                        }
                                    case 3:
                                        {
                                            Pressure_new[map.Num_Crypt(x, y + dd)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                            Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                            break;
                                        }
                                }
                             

                        }
                    }

                    
                    
                }


            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];
                    Pressure[num] = Pressure_new[num];
                    
                }
           

        }


        Color Color_Define(double x, double max) //Для отрицательных и положительных нагрузок
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

        Color Color_Define_Alt(double x, double max) //Для положительных нагрузок
        {
            double ratio = x / max;
            int red = 0;
            int green = 0;
            int blue = 0;
             
            if (ratio <0) return Color.WhiteSmoke;
            
            if (ratio <= 0.25)
            {
                blue = 255;
                green = (int)(235 * ratio / 0.25);
            }
            if (ratio > 0.25 && ratio < 0.5)
            {

                green = 235;
                blue = (int)(255 * (0.5-ratio) / 0.25);
            }
            if (ratio >= 0.5 && ratio < 0.75)
            {
                red = (int)(255 * (ratio-0.5) / 0.25);
                green = 235;

            }
            if (ratio >= 0.75)
            {
                red = 255;
                green = (int)(235 * (1 - ratio)/0.25 );
            }
            return Color.FromArgb(red, green, blue);
            
        }

        void Count_Angle(Map map)
        {
            List<double> Angle_new = new List<double>();
            for (int i = 0; i < cell_in_col * cell_in_row; i++)
            {
                Angle_new.Add(0);
            }
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int x = 0; int y = 0;
                    int num = map.Component[k][i];
                    map.Num_Decrypt(map.Component[k][i], ref x, ref y);
                    double[] theta = new double[4] { 0, 0, 0, 0 };

                    // Учитываются только соседи на углах
                    if (x - 1 >= 0 && y - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x - 1, y - 1)) == false) // слева сверху
                    {
                        int num1 = map.Num_Crypt(x - 1, y - 1);
                        int kk = 1; // индекс материала соседа
                        while (map.Component[kk].Contains(num1) == false)
                        {
                            kk++;
                            if (kk > map.Component.Length - 1) throw new Exception("Невозможно определить материал");
                        }

                        Count_dl(out double a, out double b, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[num], cell_size);
                        double c = 0.5 * Sqrt((cell_size + a) * (cell_size + a) + (cell_size + b) * (cell_size + b));
                        Count_dl(out double a1, out double b1, Materials[kk - 1].elastic_m, Materials[kk - 1].p_ratio, Pressure[num1], cell_size);
                        double d = Sqrt(a1 * a1 + b1 * b1);
                        double f = Sqrt(c * c + d * d); //- 2 * c * d * Cos(PI/2)   // Pi/2?
                        theta[0] = Acos(c / f);
                        if (Pressure[num1] < 0) theta[0] *= -1; 
                    }

                    if (x + 1 < cell_in_row && y - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x + 1, y - 1)) == false) // справа сверху
                    {
                        int num1 = map.Num_Crypt(x + 1, y - 1);
                        int kk = 1; // индекс материала соседа
                        while (map.Component[kk].Contains(num1) == false)
                        {
                            kk++;
                            if (kk > map.Component.Length - 1) throw new Exception("Невозможно определить материал");
                        }

                        Count_dl(out double a, out double b, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[num], cell_size);
                        double c = 0.5 * Sqrt((cell_size + a) * (cell_size + a) + (cell_size + b) * (cell_size + b));
                        Count_dl(out double a1, out double b1, Materials[kk - 1].elastic_m, Materials[kk - 1].p_ratio, Pressure[num1], cell_size);
                        double d = Sqrt(a1 * a1 + b1 * b1);
                        double f = Sqrt(c * c + d * d); //- 2 * c * d * Cos(PI/2)   // Pi/2?
                        theta[1] = Acos(c / f);
                        if (Pressure[num1] > 0) theta[1] *= -1;
                    }
                    if (x - 1 >= 0 && y + 1 < cell_in_col && map.Component[0].Contains(map.Num_Crypt(x - 1, y + 1)) == false) // слева снизу
                    {
                        int num1 = map.Num_Crypt(x - 1, y + 1);
                        int kk = 1; // индекс материала соседа
                        while (map.Component[kk].Contains(num1) == false)
                        {
                            kk++;
                            if (kk > map.Component.Length - 1) throw new Exception("Невозможно определить материал");
                        }

                        Count_dl(out double a, out double b, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[num], cell_size);
                        double c = 0.5 * Sqrt((cell_size + a) * (cell_size + a) + (cell_size + b) * (cell_size + b));
                        Count_dl(out double a1, out double b1, Materials[kk - 1].elastic_m, Materials[kk - 1].p_ratio, Pressure[num1], cell_size);
                        double d = Sqrt(a1 * a1 + b1 * b1);
                        double f = Sqrt(c * c + d * d); //- 2 * c * d * Cos(PI/2)   // Pi/2?
                        theta[2] = Acos(c / f);
                        if (Pressure[num1] > 0) theta[2] *= -1;
                    }

                    if (x + 1 < cell_in_row && y + 1 < cell_in_col && map.Component[0].Contains(map.Num_Crypt(x + 1, y + 1)) == false) // справа снизу
                    {
                        int num1 = map.Num_Crypt(x + 1, y + 1);
                        int kk = 1; // индекс материала соседа
                        while (map.Component[kk].Contains(num1) == false)
                        {
                            kk++;
                            if (kk > map.Component.Length - 1) throw new Exception("Невозможно определить материал");
                        }

                        Count_dl(out double a, out double b, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[num], cell_size);
                        double c = 0.5 * Sqrt((cell_size + a) * (cell_size + a) + (cell_size + b) * (cell_size + b));
                        Count_dl(out double a1, out double b1, Materials[kk - 1].elastic_m, Materials[kk - 1].p_ratio, Pressure[num1], cell_size);
                        double d = Sqrt(a1 * a1 + b1 * b1);
                        double f = Sqrt(c * c + d * d); //- 2 * c * d * Cos(PI/2)   // Pi/2?
                        theta[3] = Acos(c / f);
                        if (Pressure[num1] < 0) theta[3] *= -1;
                    }

                    Angle_new[num] = theta.Average();
                }

            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];
                    Angle[num] = Angle_new[num];
                }
        }

        double Max_P()
        {
            double max_P;
            // заменить на switch
            if (rBBoth.Checked == true)
            {
                if (Pressure.Max() > Abs(Pressure.Min())) max_P = Pressure.Max();
                else max_P = Abs(Pressure.Min());
            }
            else if (rBsigma_x.Checked == true)
            {
                if (Pressure_x.Max() > Abs(Pressure_x.Min())) max_P = Pressure_x.Max();
                else max_P = Abs(Pressure_x.Min());
            }
            else if (rBsigma_y.Checked == true)
            {
                if (Pressure_y.Max() > Abs(Pressure_y.Min())) max_P = Pressure_y.Max();
                else max_P = Abs(Pressure_y.Min());
            }
            else
            {
                if (Angle.Max() > Abs(Angle.Min())) max_P = Angle.Max();
                else max_P = Abs(Angle.Min());
            }
            return max_P;
        }

        private void nUD_SquareNum_ValueChanged(object sender, EventArgs e)
        {
            sq_in_rad = (int)nUD_SquareNum.Value;
            cell_in_col = sq_in_rad*2+1;
            cell_in_row = cell_in_col;

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            vol_frac = new double[(int)nUD_MaterialsNum.Value+1];
            Materials = new Material[(int)nUD_MaterialsNum.Value];

        }

        private void rBBoth_CheckedChanged(object sender, EventArgs e)
        {
            if (rBBoth.Checked == true && Pressure != null && Pressure.Count > 0) Display(map, Pressure, Color_Define_Alt);
        }
        private void rBsigma_x_CheckedChanged(object sender, EventArgs e)
        {
            if (rBsigma_x.Checked==true && Pressure_x != null && Pressure_x.Count > 0) Display(map, Pressure_x, Color_Define_Alt);
        }
        private void rBsigma_y_CheckedChanged(object sender, EventArgs e)
        {
            if (rBsigma_y.Checked == true && Pressure_y!= null && Pressure_y.Count > 0) Display(map, Pressure_y, Color_Define_Alt);
        }
        private void rBAngle_CheckedChanged(object sender, EventArgs e)
        {
            if (rBAngle.Checked == true && Angle != null && Angle.Count > 0 /*&& Angle.Max() > 0*/)
                Display(map, Angle, Color_Define);
        }
    }
}
// Визуальная проверка заполнения cell_l_d
//System.Threading.Thread.Sleep(200);
//bmp = new Bitmap(cell_size_pix * cell_in_row + 5, cell_in_col * cell_size_pix + 15, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
//using (Graphics g = Graphics.FromImage(bmp))
//{
//    Rectangle rect;
//    double max_P;
//    if (Pressure.Max() > Abs(Pressure.Min())) max_P = Pressure.Max();
//    else max_P = Abs(Pressure.Min());
//    for (int k = 1; k < map.Component.Length; k++)
//        for (int i = 0; i < map.Component[k].Count; i++)
//        {
//            int x = 0; int y = 0;
//            map.Num_Decrypt(map.Component[k][i], ref x, ref y);
//            double convert_size = cell_size_pix / cell_size * Pow(10, 9);
//            int rec_x = (int)(x * cell_size_pix + 5/* - Abs(rec_da) * convert_size / 2*/);
//            int rec_y = (int)(y * cell_size_pix + 5/*- Abs(rec_db) * convert_size / 2*/);
//            int rec_a = (int)Abs(cell_l_d[0][map.Component[k][i]] * convert_size);
//            int rec_b = (int)Abs(cell_l_d[1][map.Component[k][i]] * convert_size);
//            rect = new Rectangle(rec_x, rec_y, rec_a - 5, rec_b - 5);
//            //rect = new Rectangle(x* cell_size_pix + 5, y* cell_size_pix+5, cell_size_pix - 10 , cell_size_pix-10);
//            Color rect_color;
//            if (Abs(Pressure[map.Component[k][i]]) >= Materials[k - 1].compr_strength)
//            {
//                rect_color = Color.Gray;
//            }
//            else rect_color = Color_Define(Pressure[map.Component[k][i]], max_P);
//            g.FillRectangle(new SolidBrush(rect_color), rect); continue;
//        }

//}
//pictureBoxMain.Image?.Dispose();
//pictureBoxMain.Image = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
//bmp?.Dispose();
//pictureBoxMain.Refresh();

// Наблюдение за взаимодействием клеток
//sch_max = 40;
//System.Threading.Thread.Sleep(0);
//for (int sch = 0; sch < sch_max; sch++)
//{

//    bmp = new Bitmap(cell_size_pix * cell_in_row + 5, cell_in_col * cell_size_pix + 15, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
//    using (Graphics g = Graphics.FromImage(bmp))
//    {
//        Rectangle rect;
//        double max_P;


//        Interact(map);
//        if (Pressure.Max() > Abs(Pressure.Min())) max_P = Pressure.Max();
//        else max_P = Abs(Pressure.Min());

//        for (int k = 1; k < map.Component.Length; k++)
//            for (int i = 0; i < map.Component[k].Count; i++)
//            {
//                int x = 0; int y = 0;
//                map.Num_Decrypt(map.Component[k][i], ref x, ref y);

//                double convert_size = cell_size_pix / cell_size * Pow(10, 9);
//                int rec_x = (int)(x * cell_size_pix + 5);
//                int rec_y = (int)(y * cell_size_pix + 5);
//                int rec_a = (int)Abs(cell_l_d[0][map.Component[k][i]] * convert_size);
//                int rec_b = (int)Abs(cell_l_d[1][map.Component[k][i]] * convert_size);
//                rect = new Rectangle(rec_x, rec_y, rec_a - 5, rec_b - 5);
//                Color rect_color;
//                if (Abs(Pressure[map.Component[k][i]]) >= Materials[k - 1].compr_strength)
//                {
//                    rect_color = Color.Gray;
//                }
//                else rect_color = Color_Define(Pressure[map.Component[k][i]], max_P);

//                g.FillRectangle(new SolidBrush(rect_color), rect); continue;

//            }

//    }
//    pictureBoxMain.Image?.Dispose();
//    pictureBoxMain.Image = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
//    bmp?.Dispose();
//    pictureBoxMain.Refresh();
//    System.Threading.Thread.Sleep(25);
//}