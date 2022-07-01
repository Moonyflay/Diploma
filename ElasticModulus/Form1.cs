using System;
using System.IO;
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
        int sq_in_rad = 20; // Число клеток в радиусе
        int cell_in_row = 41;
        int cell_in_col = 41;
        int cell_in_depth = 16;
        int max_num;
        int display_z = 8;
        int iter_num = 200;
        int pressured_cells = 21;
        byte ttt = 40; // число итераций взаимодействия
        string text;

        double force = 0.78; //Н
        double diameter = 0.01032; //м
        double thickness = 0.00403;  //м
        double app_press = 524472;
        double force_sum;

        double emp_param = 1; 

        List<double> Neigh;
        List<double[]> Neigh2;
        List<bool[]> Neigh3;

        List<bool> Fractured;
        List<bool> Griffith;

        byte cell_size_pix = 20;
        double cell_size;
        List<double>[] cell_l_d = new List<double>[2];

        List<double> Pressure;
        List<double> Pressure_x;
        List<double> Pressure_y;
        List<double> Pressure_z;
        List<double> Shear;
        List<double> Angle;
        List<double> Von_Mises;
        
        
        List<double>[] Stress;
        byte stress_in = 1;

        double[] vol_frac;
        Material[] Materials;
        public  static Random sluchai = new Random();
        double fading = 1.0;
        double transpass = 0.0;
        double transpass_ = 0.0;

        delegate Color color_def_f(double _a, double _b, double _cut = 1);
        color_def_f Color_Function;
        delegate Task Display_function(double cut = 1);
        Display_function Display;

        bool quater = true;

        List<Task> tasks = new List<Task>();

        public FormMain()
        {

            InitializeComponent();

            Color_Function = Color_Define_Log;
            Display = Display_Sym;
            vol_frac = new double[(int)nUD_MaterialsNum.Value+1];
            Materials = new Material[(int)nUD_MaterialsNum.Value];

            sq_in_rad = (int)nUD_SquareNum.Value;
            pressured_cells = sq_in_rad + 1;
            Count_cell_size();
            cell_in_col = sq_in_rad * 2 + 1;
            cell_in_row = cell_in_col;
            Update_Force_Panel();
            
        }
        private async Task Fracture(int sch)
        {

              for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];
                    if (Griffith[num] == true) continue;
                    else
                    {
                        tasks.Add(Count_Griffith(num, k));
                    }
                   
                }
            for (int i = 0; i < tasks.Count;i++)
                await tasks[i];
            await Task.Delay(2);
            for (int i = 0; i < tasks.Count; i++)
            { tasks[i]?.Dispose(); }
            tasks.Clear();

            async Task Count_Griffith(int num,int k)
            {
                Task.Run(() => Count_Griffith_Jr(num, k));
            }
            void Count_Griffith_Jr(int num, int k)
            {
                Griffith[num] = Griffith_Cr(num, k - 1);
                if (Griffith[num] == true)
                {
                    int x = 0; int y = 0; int z = 0;
                    map.Num_Decrypt(num, ref x, ref y, ref z);
                    text += "Sigma_X = " + Round(Pressure_x[num], 0) + $" №{sch - 1}; F = {app_press * cell_in_depth * cell_size * cell_size * sch}; " +
                        $"2F/(Pi*D*t) = {Round(2 * app_press * cell_in_depth * cell_size * cell_size * sch / (PI * diameter * thickness), 0)}; ({x}; {y}; {z})" + Environment.NewLine;

                    Fractured[num] = true; ;
                    //Pressure_x[num] = 0;
                    //Pressure_y[num] = 0;
                    //Pressure_z[num] = 0;
                    //map.Component[k].Remove(num);
                    //map.Component[0].Add(num);
                }
            }


        }

        private void Assign_Materials()
        {
            Materials = new Material[(int)nUD_MaterialsNum.Value];
            Materials[0] = new Material((double)nUD_Poisson.Value, (double)nUD_Young.Value * Pow(10, 9), 20 * Pow(10, 15), 38.06 * Pow(10, 6), (double)nUD_Mises.Value*Pow(10,6));
            //for (int i = 1; i < Materials.Length; i++) ;
            //    Materials[i] = new Material();
        }
        private async void buttonStart_Click(object sender, EventArgs e)
        {
            //textBox1.Clear();
            Interface_Block_Off(false);

            if (quater == false) { max_num = cell_in_row * cell_in_col * cell_in_depth; Display = Display_Asym; }
            else max_num = (sq_in_rad+1) * (sq_in_rad+1) * cell_in_depth;
            app_press = force/(cell_size * cell_size);
          
            force_sum = 0;
            //ttt = (byte)(sq_in_rad*2); 

            vol_frac[0] = 0;
            vol_frac[1] = 1;
            //vol_frac[2] = 1- vol_frac[1];

            //double mind = 5;
            //double maxd = 30;
            //double mx = 8;
            //double sigma = 0.15;
            Assign_Materials();
            //Materials[0] = new Material(0.24, 50*Pow(10, 9), 20*Pow(10,12), 16 * Pow(10, 12));
           // Materials[0] = new Material(0.24, 34 * Pow(10, 9), 38 * Pow(10, 12), 0.02 * 34 * Pow(10, 9), 38.06 * Pow(10, 6));

            Pressure = new List<double>();
            Pressure_x = new List<double>();
            Pressure_y = new List<double>();
            Pressure_z = new List<double>();
            Angle = new List<double>();
            Shear = new List<double>();
            Von_Mises = new List<double>();
            Neigh = new List<double>();
            Neigh2 = new List<double[]>();
            Fractured = new List<bool>();
            Griffith = new List<bool>();

            //for (int i = 0; i < 3; i++)
            //    Neigh3.Add(new bool[]{false,false,false,false,false,false}); ;
            for (int i = 0; i < max_num; i++)
            {
                Pressure.Add(0);
                Pressure_x.Add(0);
                Pressure_y.Add(0);
                Pressure_z.Add(0);
                Shear.Add(0);
                Angle.Add(0);
                Von_Mises.Add(0);
                Neigh.Add(0);
                Neigh2.Add(new double[3]{0,0,0 });
                Griffith.Add(false);
                Fractured.Add(false);
                //Neigh[1].Add(0);
                //Neigh[2].Add(0);

            }
            Stress = new List<double>[] { Pressure, Pressure_x, Pressure_y, Pressure_z, Shear, Angle, Von_Mises };

            for (int i = 0; i < cell_l_d.Length; i++)
            {
                cell_l_d[i] = new List<double>();
            }
            for (int i = 0; i< max_num; i++) //при отсутствии пор!
            {
                cell_l_d[0].Add(cell_size);
                cell_l_d[1].Add(cell_size);
            }

            //Map map = new Map(vol_frac, cell_in_row, cell_in_col, mind, maxd, mx, sigma, cell_size);
            map = new Map(sq_in_rad, vol_frac, cell_in_row, cell_in_col, cell_in_depth, cell_size, quater);

           // Count_app_press(map);

            progressBar1.Maximum = iter_num;
            bool stop;
            force_sum += app_press * cell_in_depth * cell_size * cell_size* iter_num;
            pressured_cells = sq_in_rad + 1;
            int filled_cells = 0;
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    filled_cells++;
                }
            tB_Fracture.Text += filled_cells + Environment.NewLine;
            tB_Fracture.Text += DateTime.Now + Environment.NewLine;
            PP: for (int sch = 0; sch <iter_num; sch++)
             {

                //            Count_dl(out double rec_da, out double rec_db, Materials[k-1].elastic_m, Materials[k-1].p_ratio, Pressure[map.Component[k][i]], cell_size * Pow(10, -9));
                //            if (sch == sch_max - 1)
                //            {
                //                cell_l_d[0][map.Component[k][i]] += rec_da;
                //                cell_l_d[1][map.Component[k][i]] += rec_db;
                //            }
                Define_Empirical(sch);
                Count_Neigh();
                if (checkBox1.Checked==true)Apply_pressure();
                await Fracture(sch + 1);
                Count_Neigh();
                await Interact_Sym_Async(true, sch == 0);
                for (int tt = 0; tt < ttt-1; tt++) // tt должно быть четным
                {
                    await Interact_Sym_Async();
                }
                await Fracture(sch+1);
                 tB_Fracture.Text += text;
                text = "";
                if (stress_in == 0 || stress_in == 6)
                    for (int u = 0; u < max_num; u++)
                        if ((map.Component[0].Contains(u) || Fractured[u])== false) Count_VM_and_Hydro(u);
                Display();
                progressBar1.Value = sch;
                label_progress_bar.Text = progressBar1.Value + " из " + iter_num;
                
            }
            tB_Fracture.Text += DateTime.Now + Environment.NewLine;
            progressBar1.Value = iter_num;
            label_progress_bar.Text = progressBar1.Value + " из " + iter_num;
            Display();
            //stop = false;
            //for (int k = 1; k < map.Component.Length; k++)
            //{
            //    if (stop == true) { break; }
            //    for (int i = 0; i < map.Component[k].Count; i++)
            //    {
            //        if (Mises_Cr(i, k - 1) == true) { stop = true; break; }
            //    }
            //}
            //if (stop == false) goto PP; // ++++++++++++++++++++++++++++++++++++++++++++ Костыль!
            
            Interface_Block_Off(true);
            System.Console.Beep(500, 300);
            System.Console.Beep(500, 250);
            System.Console.Beep(500, 200);
        }

        async Task Display_Asym(double cut = 1)
        {
            double max_P = Max_P();
            label_Max_P.Text = "Max F = " + max_P;
            label_Sigma_at_break.Text = "2*F/(Pi*D*t) = " + Round(2 * force_sum / (PI * thickness * diameter), 2);
            label_Sigma_at_break.Refresh();
            Bitmap bmp;
            bmp = new Bitmap(cell_size_pix * cell_in_row + 5, cell_in_col * cell_size_pix + 15, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            using (Graphics g = Graphics.FromImage(bmp)) 
            {
                Rectangle rect;

                for (int k = 1; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++) 
                    {
                        int x = 0; int y = 0; int z = 0;
                        map.Num_Decrypt(map.Component[k][i], ref x, ref y, ref z);
                        if (z != display_z) continue;
                       
                        // Можно раскомментить, но надо ли?
                        //Count_dl(out double rec_da, out double rec_db, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[map.Component[k][i]], cell_size * Pow(10, -9));
                        //double convert_size = cell_size_pix / cell_size * Pow(10, 9);

                        int rec_x = (int)(x * cell_size_pix + 5);
                        int rec_y = (int)(y * cell_size_pix + 5);
                        int rec_a = cell_size_pix; //(int)(cell_size_pix + rec_da * convert_size);
                        int rec_b = cell_size_pix; //(int)(cell_size_pix + rec_db * convert_size); 


                        rect = new Rectangle(rec_x, rec_y, rec_a - 5, rec_b - 5);
                        Color rect_color;
                        if (stress_in != 4 && stress_in != 5 && (Mises_Cr(map.Component[k][i], k-1) ||Abs(Stress[stress_in][map.Component[k][i]]) >= Materials[k - 1].compr_strength || Fractured[map.Component[k][i]])) // Abs(Pressure_x[map.Component[k][i]] + Pressure_y[map.Component[k][i]])
                        { 
                            rect_color = Color.Gray;
                        }
                        else rect_color = Color_Function(Stress[stress_in][map.Component[k][i]], max_P, cut);  
                        g.FillRectangle(new SolidBrush(rect_color), rect); continue;
                    }
            }
                pictureBoxMain.Image?.Dispose();
                pictureBoxMain.Image = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
                bmp?.Dispose();
                pictureBoxMain.Refresh();
            
        }
        async Task Display_Sym(double cut = 1) 
        {
            double max_P = Max_P();
            Progress_Bar_Update(max_P);
            Bitmap bmp;
            bmp = new Bitmap(cell_size_pix * cell_in_row + 5, cell_in_col * cell_size_pix + 15, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle rect;

                for (int k = 1; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++)
                    {
                        int x = 0; int y = 0; int z = 0;
                        map.Num_Decrypt(map.Component[k][i], ref x, ref y, ref z);
                        if (z != display_z) continue;

                        Color rect_color;
                        if (stress_in != 4 && stress_in != 5 && Materials[k-1]!= null && (/*Mises_Cr(map.Component[k][i], k - 1) ||*/ Abs(Stress[stress_in][map.Component[k][i]]) >= Materials[k - 1].compr_strength || Fractured[map.Component[k][i]])) // Abs(Pressure_x[map.Component[k][i]] + Pressure_y[map.Component[k][i]])
                        {
                            rect_color = Color.Gray;
                        }
                        else rect_color = Color_Function(Stress[stress_in][map.Component[k][i]], max_P, cut);
                        int rec_a = cell_size_pix; 
                        int rec_b = cell_size_pix; 
                        int rec_x;
                        int rec_y;
                        for (int _x = sq_in_rad - x; _x < sq_in_rad + x + 1; _x += 2 * x)
                        {
                            for (int _y = y; _y < cell_in_col - y; _y += cell_in_col-1 - 2 * y)
                            {
                                rec_x = (int)(_x * cell_size_pix + 5);
                                rec_y = (int)(_y * cell_size_pix + 5);
                                rect = new Rectangle(rec_x, rec_y, rec_a - 5, rec_b - 5);
                                g.FillRectangle(new SolidBrush(rect_color), rect);
                                if (y == sq_in_rad) break;
                            }
                            if (x == 0) break;
                        }
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
            for (int z = 0; z< cell_in_depth;z++) 
            {
                for (int x = 0; x < maxx; x++)
                    for (int i = (int)-maxx; i < maxx; i++) //
                    {
                        int y = (int)((maxy - 1) / (maxx - 1) * x);
                        int num;
                        if ((i >= 0 && y + i < maxy) || (i < 0 && y + i >= 0))
                        {
                            num = map.Num_Crypt(x, y + i, z);
                            if (map.Component[0].Contains(num) ) continue;
                            if (Abs(Pressure[num]) < Materials[0].compr_strength) //
                                Pressure[num] += maxP * ((maxx - 2 * x - 1) / (maxx - 1) + (maxy - 2 * (i + y) - 1) / (maxy - 1)) / 2;
                        }
                        else continue;
                    }
            }
            

        }

        private void buttonStart2_Click(object sender, EventArgs e)
        {
            System.Console.Beep(500,300);
            return;
            vol_frac[0] = 0.000;
            vol_frac[1] = 0.40;
            vol_frac[2] = 1 - vol_frac[0] - vol_frac[1];

            //double mind = 1;
            //double maxd = 2;
            //double mx = 1;
            //double sigma = 0.15;
            //Map map = new Map(sq_in_rad, vol_frac, cell_in_row, cell_in_col, mind, maxd, mx, sigma, cell_size);

            Map map = new Map(sq_in_rad, vol_frac, cell_in_row, cell_in_col,cell_in_depth, cell_size);
            Bitmap bmp;
           
                bmp = new Bitmap(cell_size_pix * cell_in_row , cell_in_col * cell_size_pix, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    
                Rectangle rect;
                   
                  
                for (int k = 0; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++)
                    {
                        int x = 0; int y = 0; int z = 0;
                        map.Num_Decrypt(map.Component[k][i], ref x, ref y, ref z);
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

        void Apply_pressure() 
        {
            
            //(byte)( cell_in_col*0.5); // Клетки, на которые действует пластина (отсчет ведется с края таблетки)

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

            //int xmin = cell_in_row; int xmax = 0;

            for (int y = 0; y < pressured_cells; y++)
            {
                for (int z = 0; z < cell_in_depth; z++)
                    for (int x = 0; x < 1; x++)
                    {
                        if (map.Component[0].Contains(map.Num_Crypt(x, y, z)) == false)
                        {
                            int num = map.Num_Crypt(x, y, z);
                            //double delitel = 2.0 / (y + 1);
                            Pressure_y[num] -= app_press / pressured_cells; /**delitel;*/
                            Pressure_x[num] += app_press / pressured_cells; /** Materials[0].p_ratio;*/ //+++++++++++++++++++++++++++++++++++++++++++++++++++++ Костыль
                            Pressure_z[num] += app_press / pressured_cells; /** Materials[0].p_ratio;*/ //+++++++++++++++++++++++++++++++++++++++++++++++++++++ Костыль
                            //if (x > xmax) xmax = x;
                            //if (xmin > x) xmin = x;
                        }
                        if (map.Component[0].Contains(map.Num_Crypt(x, cell_in_col - y - 1, z)) == false && quater == false)
                        {
                            int num = map.Num_Crypt(x, cell_in_col - y - 1, z);
                            Pressure_y[num] -= app_press / pressured_cells;
                            Pressure_x[num] += app_press / pressured_cells * Materials[0].p_ratio; //+++++++++++++++++++++++++++++++++++++++++++++++++++++ Костыль
                            Pressure_z[num] += app_press / pressured_cells * Materials[0].p_ratio; //+++++++++++++++++++++++++++++++++++++++++++++++++++++ Костыль
                           
                            //if (x > xmax) xmax = x;
                            //if (xmin > x) xmin = x;
                        }
                    }
            }


            //for (int y = affected_cells; y < cell_in_col - affected_cells; y++)
            //    for (int z = 0; z < cell_in_depth; z++)
            //        for (int x = xmin; x <= xmax; x++)
            //        {
            //            int num = map.Num_Crypt(x, y, z);
            //            if (map.Component[0].Contains(num) == false)
            //            {
            //                Pressure[num] += app_press * Pow(transpass, y - affected_cells + 1);
            //                Pressure[num] += app_press * Pow(transpass, cell_in_col - y - affected_cells);
            //                Pressure_y[num] -= app_press * Pow(transpass, y - affected_cells + 1);
            //                Pressure_y[num] -= app_press * Pow(transpass, cell_in_col - y - affected_cells);
            //            }
            //        }

        }

        void Count_Neigh()
        {
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int x = 0; int y = 0; int z = 0;
                    int num = map.Component[k][i];
                    map.Num_Decrypt(num, ref x, ref y, ref z);
                    double neigh = 2 + 4 * Materials[k - 1].p_ratio;
                    Neigh2[num][0] = neigh;
                    Neigh2[num][1] = neigh;
                    Neigh2[num][2] = neigh;

                    //if ((quater == false && x - 1 < 0) || (x - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x - 1, y, z)))) Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; /*neigh--;*///neigh-=(1 - 2 * Materials[k - 1].p_ratio);
                    //if (x + 1 >= sq_in_rad+1|| (x + 1 < sq_in_rad + 1 && map.Component[0].Contains(map.Num_Crypt(x + 1, y, z)))) Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; /*neigh--;*///neigh-= (1 - 2 * Materials[k - 1].p_ratio);
                    //// нет у - 1 < 0 т. к. у этой клетки есть сосед - пластина пресса
                    //// нет y + 1 >= cell_in_col по той же причине
                    //if (y - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x, y - 1, z))) Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio; /*neigh--;*///neigh -= (1 - 2 * Materials[k - 1].p_ratio);                     
                    //if (y + 1 < sq_in_rad + 1 && map.Component[0].Contains(map.Num_Crypt(x, y + 1, z))) Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio;/*neigh--;*///neigh -= (1 - 2 * Materials[k - 1].p_ratio); 
                    //if (z - 1 < 0 || (z - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x, y, z - 1)))) Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio; /*neigh--;*/ //neigh -= (1 - 2 * Materials[k - 1].p_ratio);
                    //if (z + 1 >= cell_in_depth || (z + 1 < cell_in_depth && map.Component[0].Contains(map.Num_Crypt(x, y, z + 1)))) Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio;/*neigh--;*///neigh -= (1 - 2 * Materials[k - 1].p_ratio);

                    //if ((x - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x - 1, y, z))) || (x + 1 < sq_in_rad + 1 && map.Component[0].Contains(map.Num_Crypt(x + 1, y, z))) || x + 1 >= sq_in_rad + 1)
                    //    Neigh2[num][0] *= 0.50;
                    //if ((y - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x, y - 1, z))) || (y + 1 < sq_in_rad + 1 && map.Component[0].Contains(map.Num_Crypt(x, y + 1, z))) || y - 1 < 0)
                    //    Neigh2[num][1] *= 0.50;
                    //if ((z - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x, y, z - 1))) || (z + 1 < cell_in_depth && map.Component[0].Contains(map.Num_Crypt(x, y, z + 1))) || z - 1 < 0 || z + 1 >= cell_in_depth)
                    //    Neigh2[num][2] *= 0.50;

                    //if ((x - 1 >= 0 && Fractured.Contains(map.Num_Crypt(x - 1, y, z))) || (x + 1 < sq_in_rad + 1 && Fractured.Contains(map.Num_Crypt(x + 1, y, z))))
                    //{ Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio; }
                    //if ((y - 1 >= 0 && Fractured.Contains(map.Num_Crypt(x, y - 1, z))) || (y + 1 < sq_in_rad + 1 && Fractured.Contains(map.Num_Crypt(x, y + 1, z))))
                    //{ Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio; }
                    //if ((z - 1 >= 0 && Fractured.Contains(map.Num_Crypt(x, y, z - 1))) || (z + 1 < cell_in_depth && Fractured.Contains(map.Num_Crypt(x, y, z + 1))))
                    //{ Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio; }


                    if (x + 1 >= sq_in_rad + 1 || (x + 1 < sq_in_rad + 1 && map.Component[0].Contains(map.Num_Crypt(x + 1, y, z))) || (x - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x - 1, y, z))))
                        Neigh2[num][0] = Neigh2[num][0] - ((1 + 2 * Materials[k - 1].p_ratio) * (sq_in_rad - x) / sq_in_rad)/*+ ((1 + 2 * Materials[k - 1].p_ratio) * (sq_in_rad - y) / sq_in_rad)*/;
                    if (/*y - 1 < 0 ||*/ (y + 1 < sq_in_rad + 1 && map.Component[0].Contains(map.Num_Crypt(x, y + 1, z))) || (y - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x, y - 1, z)))) 
                        Neigh2[num][1] = Neigh2[num][1] - ((1 + 2 * Materials[k - 1].p_ratio) * (/*sq_in_rad -*/ y) / sq_in_rad)/*+ (1 + 2 * Materials[k - 1].p_ratio) * x / sq_in_rad*/;
                    if (y - 1 < 0) { Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; }
                    //if ((z - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x, y, z - 1))) || (z + 1 < cell_in_depth && map.Component[0].Contains(map.Num_Crypt(x, y, z + 1))) || z - 1 < 0 || z + 1 >= cell_in_depth)
                    //    Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio;

                    Neigh[num] = Neigh2[num][0] + Neigh2[num][1] + Neigh2[num][2];
                    Neigh2[num][0] = Neigh[num];
                    Neigh2[num][1] = Neigh[num];
                    Neigh2[num][2] = Neigh[num];

                    if (Fractured[num] == false)
                    {
                        if ((x - 1 >= 0 && Fractured[map.Num_Crypt(x - 1, y, z)]))
                        { Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio; }
                        if ((x + 1 < sq_in_rad + 1 && Fractured[map.Num_Crypt(x + 1, y, z)]))
                        { Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio; }
                        if ((y - 1 >= 0 && Fractured[map.Num_Crypt(x, y - 1, z)]))
                        { Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio; }
                        if ((y + 1 < sq_in_rad + 1 && Fractured[map.Num_Crypt(x, y + 1, z)]))
                        { Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][2] -= 1 + 2 * Materials[k - 1].p_ratio; }
                        if ((z - 1 >= 0 && Fractured[map.Num_Crypt(x, y, z - 1)]))
                        { Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio; }
                        if ((z + 1 < cell_in_depth && Fractured[map.Num_Crypt(x, y, z + 1)]))
                        { Neigh2[num][0] -= 1 + 2 * Materials[k - 1].p_ratio; Neigh2[num][1] -= 1 + 2 * Materials[k - 1].p_ratio; }

                    }

                    if (Fractured[num] == true)
                    {
                        //if ((x - 1 >= 0 && Fractured.Contains(map.Num_Crypt(x - 1, y, z)))||(x + 1 < sq_in_rad + 1 && Fractured.Contains(map.Num_Crypt(x + 1, y, z))))
                        //{ Neigh2[num][0] = neigh;  }
                        //if ((y - 1 >= 0 && Fractured.Contains(map.Num_Crypt(x, y - 1, z)))||(y + 1 < sq_in_rad + 1 && Fractured.Contains(map.Num_Crypt(x, y + 1, z))))
                        //{ Neigh2[num][1] = neigh; }
                        //if ((z - 1 >= 0 && Fractured.Contains(map.Num_Crypt(x, y, z - 1)))||(z + 1 < cell_in_depth && Fractured.Contains(map.Num_Crypt(x, y, z + 1))))
                        //{ Neigh2[num][2] = neigh; }
                        for (int s = 0; s < sq_in_rad + 1; s++)
                        {
                            if (s == x) continue;
                            if (Fractured[map.Num_Crypt(s, y, z)])
                            {
                                Neigh2[num][0] = neigh;
                                break;
                            }
                        }
                        for (int s = 0; s < sq_in_rad + 1; s++)
                        {
                            if (s == y) continue;
                            if (Fractured[map.Num_Crypt(x, s, z)])
                            {
                                Neigh2[num][1] = neigh;
                                break;
                            }
                        }
                        for (int s = 0; s < cell_in_depth; s++)
                        {
                            if (s == z) continue;
                            if (Fractured[map.Num_Crypt(x, y, s)])
                            {
                                Neigh2[num][2] = neigh;
                                break;
                            }
                        }

                    }

                }
        }

        void Interact(/*Map map*/) 
        {
            //List<double>[] cell_l_d_new = new List<double>[2];
            //for (int i = 0; i < cell_l_d.Length; i++)
            //{
            //    cell_l_d_new[i] = new List<double>();
            //}
            //for (int i = 0; i < cell_in_col * cell_in_row * cell_in_depth; i++) //при отсутствии пор!
            //{
            //    cell_l_d_new[0].Add(cell_size);
            //    cell_l_d_new[1].Add(cell_size);
            //}

            List<double> Pressure_new_x = new List<double>();
            List<double> Pressure_new_y = new List<double>();
            List<double> Pressure_new_z = new List<double>();
            //List<double> Shear_new = new List<double>();

            for (int i = 0; i < max_num; i++)
            {
                Pressure_new_x.Add(0);
                Pressure_new_y.Add(0);
                Pressure_new_z.Add(0);
                // Shear_new.Add(0);
            }

            for (int k = 1; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++)
                    {
                        int x = 0; int y = 0; int z = 0;
                        int num = map.Component[k][i];
                        map.Num_Decrypt(num, ref x, ref y, ref z);

                        if (x - 1 >= 0)
                        {
                            int num_1 = map.Num_Crypt(x - 1, y, z);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                //Pressure_new_x[num] -= Pressure_x[num] / neigh * fading;
                                //Pressure_new_y[num] += Pressure_x[num] / neigh * Materials[k - 1].p_ratio * fading;
                                //Pressure_new_z[num] += Pressure_x[num] / neigh * Materials[k - 1].p_ratio * fading;
                                //}
                                //else
                                //{
                                double _new = Pressure_x[num_1] / Neigh[num_1];
                                Pressure_new_x[num] -= _new/* * Cos(Angle[num_1] - Angle[num])*/;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                // Shear_new[num] -= Pressure[num_1] / Neigh[num_1] /** Sin(Angle[num] - Angle[num_1])*/;
                            }
                        }
                        //else 
                        //{ 
                        //    //Pressure_new_x[num] -= Pressure_x[num] / neigh * fading;
                        //    //Pressure_new_y[num] += Pressure_x[num] / neigh * Materials[k - 1].p_ratio * fading;
                        //    //Pressure_new_z[num] += Pressure_x[num] / neigh * Materials[k - 1].p_ratio * fading;
                        //}

                        if (x + 1 < cell_in_row)
                        {
                            int num_1 = map.Num_Crypt(x + 1, y, z);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                //Pressure_new_x[num] -= Pressure_x[num] / neigh * fading;
                                //Pressure_new_y[num] += Pressure_x[num] / neigh * Materials[k - 1].p_ratio * fading;
                                //Pressure_new_z[num] += Pressure_x[num] / neigh * Materials[k - 1].p_ratio * fading;
                                //}
                                //else 
                                //{
                                double _new = Pressure_x[num_1] / Neigh[num_1];
                                Pressure_new_x[num] -= _new /** Cos(Angle[num_1] - Angle[num])*/;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                //Shear_new[num] -= Pressure[num_1] / Neigh[num_1]/* * Sin(Angle[num] - Angle[num_1])*/;
                            }
                        }
                        //else 
                        //{ 
                        //    //Pressure_new_x[num] -= Pressure_x[num] / neigh * fading;
                        //    //Pressure_new_y[num] += Pressure_x[num] / neigh * Materials[k - 1].p_ratio * fading;
                        //    //Pressure_new_z[num] += Pressure_x[num] / neigh * Materials[k - 1].p_ratio * fading;
                        //}
                        if (y - 1 >= 0)
                        {
                            int num_1 = map.Num_Crypt(x, y - 1, z);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                //Pressure_new_y[num] -= Pressure_y[num] / neigh * fading;
                                //Pressure_new_x[num] += Pressure_y[num] / neigh * Materials[k - 1].p_ratio * fading;
                                //Pressure_new_z[num] += Pressure_y[num] / neigh * Materials[k - 1].p_ratio * fading;
                                //}
                                //else 
                                //{
                                double _new = Pressure_y[num_1] / Neigh[num_1];
                                Pressure_new_y[num] -= _new /** Cos(Angle[num_1] - Angle[num])*/;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                //Shear_new[num] -= Pressure[num_1] / Neigh[num_1] /** Sin(Angle[num] - Angle[num_1])*/;
                            }
                        }
                        //else 
                        //{ 
                        //    //Pressure_new_y[num] -= Pressure_y[num] / neigh * fading;
                        //    //Pressure_new_x[num] += Pressure_y[num] / neigh * Materials[k - 1].p_ratio * fading;
                        //    //Pressure_new_z[num] += Pressure_y[num] / neigh * Materials[k - 1].p_ratio * fading;
                        //}

                        if (y + 1 < cell_in_col)
                        {
                            int num_1 = map.Num_Crypt(x, y + 1, z);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                //Pressure_new_y[num] -= Pressure_y[num] / neigh * fading;
                                //Pressure_new_x[num] += Pressure_y[num] / neigh * Materials[k - 1].p_ratio * fading;
                                //Pressure_new_z[num] += Pressure_y[num] / neigh * Materials[k - 1].p_ratio * fading;
                                //}
                                //else 
                                //{
                                double _new = Pressure_y[num_1] / Neigh[num_1];
                                Pressure_new_y[num] -= _new /** Cos(Angle[num_1] - Angle[num])*/;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                //Shear_new[num] -= Pressure[num_1] / Neigh[num_1] /** Sin(Angle[num] - Angle[num_1])*/;
                            }
                        }
                        //else 
                        //{
                        //    //Pressure_new_y[num] -= Pressure_y[num] / neigh * fading;
                        //    //Pressure_new_x[num] += Pressure_y[num] / neigh * fading * Materials[k - 1].p_ratio;
                        //    //Pressure_new_z[num] += Pressure_y[num] / neigh * fading * Materials[k - 1].p_ratio;
                        //}
                        if (z - 1 >= 0)
                        {
                            int num_1 = map.Num_Crypt(x, y, z - 1);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                //Pressure_new_z[num] -= Pressure_z[num] / neigh * fading;
                                //Pressure_new_x[num] += Pressure_z[num] / neigh *fading * Materials[k - 1].p_ratio;
                                //Pressure_new_y[num] += Pressure_z[num] / neigh *fading * Materials[k - 1].p_ratio;
                                //}
                                //else
                                //{
                                double _new = Pressure_z[num_1] / Neigh[num_1];
                                Pressure_new_z[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            }
                        }
                        //else 
                        //{
                        //    //Pressure_new_z[num] -= Pressure_z[num] / neigh * fading;
                        //    //Pressure_new_x[num] += Pressure_z[num] / neigh * fading * Materials[k - 1].p_ratio;
                        //    //Pressure_new_y[num] += Pressure_z[num] / neigh * fading * Materials[k - 1].p_ratio;
                        //}

                        if (z + 1 < cell_in_depth)
                        {
                            int num_1 = map.Num_Crypt(x, y, z + 1);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                //Pressure_new_z[num] -= Pressure_z[num] / neigh * fading;
                                //Pressure_new_x[num] += Pressure_z[num] / neigh * fading * Materials[k - 1].p_ratio;
                                //Pressure_new_y[num] += Pressure_z[num] / neigh * fading * Materials[k - 1].p_ratio;
                                //}
                                //else
                                //{
                                double _new = Pressure_z[num_1] / Neigh[num_1];
                                Pressure_new_z[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            }
                        }
                        //else 
                        //{
                        //    //Pressure_new_z[num] -= Pressure_z[num] / neigh * fading;
                        //    //Pressure_new_x[num] += Pressure_z[num] / neigh * fading * Materials[k - 1].p_ratio;
                        //    //Pressure_new_y[num] += Pressure_z[num] / neigh * fading * Materials[k - 1].p_ratio;
                        //}


                        //Count_dl(out double rec_da, out double rec_db, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, (Pressure_new_x[num] + Pressure_new_y[num] + Pressure_new_z[num]) / 3, cell_size);

                        //cell_l_d_new[0][num] += rec_da;
                        //cell_l_d_new[1][num] += rec_db;
                        
                    }
            
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];
                   
                    Pressure_x[num] = Pressure_new_x[num];
                    Pressure_y[num] = Pressure_new_y[num];
                    Pressure_z[num] = Pressure_new_z[num];
                    //cell_l_d[0][num] = cell_l_d_new[0][num];
                    //cell_l_d[1][num] = cell_l_d_new[1][num];

                }


        }

        void Interact_Sym(bool end = false, bool first = false)
        {
            List<double> Pressure_new_x = new List<double>();
            List<double> Pressure_new_y = new List<double>();
            List<double> Pressure_new_z = new List<double>();

            for (int i = 0; i < max_num; i++)
            {
                if (end == true && (Fractured[i] == false))
                {
                    Pressure_x[i] *= emp_param;
                    Pressure_y[i] *= emp_param;
                    Pressure_z[i] *= emp_param;
                }
                Pressure_new_x.Add(0);
                Pressure_new_y.Add(0);
                Pressure_new_z.Add(0);
            }


            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int x = 0; int y = 0; int z = 0;
                    int num = map.Component[k][i];
                    map.Num_Decrypt(num, ref x, ref y, ref z);

                    if (x - 1 >= 0)
                    {
                        int num_1 = map.Num_Crypt(x - 1, y, z);
                        if (map.Component[0].Contains(num_1) == false)
                        {
                            double _new = Pressure_x[num_1] / Neigh2[num_1][0];
                            Pressure_new_x[num] -= _new;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            if (Fractured[num_1] == false && Fractured[num] == false)
                            {
                                _new = Pressure_y[num_1] / Neigh2[num_1][0];
                                Pressure_new_y[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                _new = Pressure_z[num_1] / Neigh2[num_1][0];
                                Pressure_new_z[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            }
                        }

                    }

                    if (x + 1 < sq_in_rad + 1 || x - 1 < 0)
                    {
                        int num_1 = map.Num_Crypt(x + 1, y, z);
                        if (map.Component[0].Contains(num_1) == false)
                        {
                            double _new = Pressure_x[num_1] / Neigh2[num_1][0];
                            if (x - 1 < 0) _new *= 2;
                            Pressure_new_x[num] -= _new;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            if (Fractured[num_1] == false && Fractured[num] == false)
                            {
                                _new = Pressure_y[num_1] / Neigh2[num_1][0];
                                if (x - 1 < 0) _new *= 2;
                                Pressure_new_y[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                _new = Pressure_z[num_1] / Neigh2[num_1][0];
                                if (x - 1 < 0) _new *= 2;
                                Pressure_new_z[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            }
                        }
                    }

                    if (y - 1 >= 0 || y + 1 >= sq_in_rad + 1)
                    {
                        int num_1 = map.Num_Crypt(x, y - 1, z);
                        if (map.Component[0].Contains(num_1) == false)
                        {
                            double _new = Pressure_y[num_1] / Neigh2[num_1][1];
                            if (y >= sq_in_rad) _new *= 2;
                            Pressure_new_y[num] -= _new;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            if (Fractured[num_1] == false && Fractured[num] == false)
                            {
                                _new = Pressure_x[num_1] / Neigh2[num_1][1];
                                if (y >= sq_in_rad) _new *= 2;
                                Pressure_new_x[num] -= _new;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                _new = Pressure_z[num_1] / Neigh2[num_1][1];
                                if (y >= sq_in_rad) _new *= 2;
                                Pressure_new_z[num] -= _new;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            }
                        }
                    }

                    if (y + 1 < sq_in_rad + 1)
                    {
                        int num_1 = map.Num_Crypt(x, y + 1, z);
                        if (map.Component[0].Contains(num_1) == false)
                        {
                            double _new = Pressure_y[num_1] / Neigh2[num_1][1];
                            Pressure_new_y[num] -= _new;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            if (Fractured[num_1] == false && Fractured[num] == false)
                            {
                                _new = Pressure_x[num_1] / Neigh2[num_1][1];
                                Pressure_new_x[num] -= _new;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                _new = Pressure_z[num_1] / Neigh2[num_1][1];
                                Pressure_new_z[num] -= _new;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            }
                        }
                    }

                    if (z - 1 >= 0)
                    {
                        int num_1 = map.Num_Crypt(x, y, z - 1);
                        if (map.Component[0].Contains(num_1) == false)
                        {
                            double _new = Pressure_z[num_1] / Neigh2[num_1][2];
                            Pressure_new_z[num] -= _new;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            if (Fractured[num_1] == false && Fractured[num] == false)
                            {
                                _new = Pressure_x[num_1] / Neigh2[num_1][2];
                                Pressure_new_x[num] -= _new;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                _new = Pressure_y[num_1] / Neigh2[num_1][2];
                                Pressure_new_y[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            }
                        }
                    }

                    if (z + 1 < cell_in_depth)
                    {
                        int num_1 = map.Num_Crypt(x, y, z + 1);
                        if (map.Component[0].Contains(num_1) == false)
                        {
                            double _new = Pressure_z[num_1] / Neigh2[num_1][2];
                            Pressure_new_z[num] -= _new;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            if (Fractured[num_1] == false && Fractured[num] == false)
                            {
                                _new = Pressure_x[num_1] / Neigh2[num_1][2];
                                Pressure_new_x[num] -= _new;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                                _new = Pressure_y[num_1] / Neigh2[num_1][2];
                                Pressure_new_y[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            }
                        }
                    }

                }

            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];

                    Pressure_x[num] = /*(*/Pressure_new_x[num]  /*+ Pressure_x[num]) * 0.5*/;
                    Pressure_y[num] = /*(*/Pressure_new_y[num] /*+ Pressure_y[num]) * 0.5*/;
                    Pressure_z[num] = /*(*/Pressure_new_z[num] /*+ Pressure_z[num]) * 0.5*/;
                    

                }

        }
        async Task Interact_Sym_Async(bool end = false, bool first = false) 
        {
            List<double> Pressure_new_x = new List<double>();
            List<double> Pressure_new_y = new List<double>();
            List<double> Pressure_new_z = new List<double>();
            
            for (int i = 0; i < max_num; i++)
            {
                if (end == true && (Fractured[i] == false))
                {
                    Pressure_x[i] *= emp_param;
                    Pressure_y[i] *= emp_param;
                    Pressure_z[i] *= emp_param;
                }
                Pressure_new_x.Add(0);
                Pressure_new_y.Add(0);
                Pressure_new_z.Add(0);
            }
            //if(first == false)
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];
                    tasks.Add(Inter(num, k));

                }
            for (int i = 0; i < tasks.Count; i++)
            { await tasks[i]; /*if (i % 5000 == 0) */ }
            

           await Task.Delay(2);
 
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];
                    //if (tasks[i].Status != TaskStatus.RanToCompletion) // !!! [i+k*m.C.Count]
                    //{ tasks[i].Wait(); }
                    Pressure_x[num] = Pressure_new_x[num];
                    Pressure_y[num] = Pressure_new_y[num];
                    Pressure_z[num] = Pressure_new_z[num];

                }
            for (int i = 0; i < tasks.Count; i++)
            { tasks[i]?.Dispose();}
            tasks.Clear();
            // START   

            async Task Inter(int num, int k)
            {
                Task.Run(() => Interact_Sym_Jr(num, k));
            }
            void Interact_Sym_Jr(int num, int k)
            {
                //await Task.Run(() => System.Threading.Thread.Sleep(10));
                int x = 0; int y = 0; int z = 0;
                map.Num_Decrypt(num, ref x, ref y, ref z);

                if (x - 1 >= 0)
                {
                    int num_1 = map.Num_Crypt(x - 1, y, z);
                    if (map.Component[0].Contains(num_1) == false)
                    {
                        double _new = Pressure_x[num_1] / Neigh2[num_1][0];
                        Pressure_new_x[num] -= _new;
                        Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                        if (Fractured[num_1] == false && Fractured[num] == false) 
                        { 
                        _new = Pressure_y[num_1] / Neigh2[num_1][0];
                        Pressure_new_y[num] -= _new;
                        Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                        _new = Pressure_z[num_1] / Neigh2[num_1][0];
                        Pressure_new_z[num] -= _new;
                        Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                        }
                    }
                    
                }

                if (x + 1 < sq_in_rad + 1 || x - 1 < 0)
                {
                    int num_1 = map.Num_Crypt(x + 1, y, z);
                    if (map.Component[0].Contains(num_1) == false)
                    {
                        double _new = Pressure_x[num_1] / Neigh2[num_1][0];
                        if (x - 1 < 0) _new *= 2;
                        Pressure_new_x[num] -= _new;
                        Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                        if (Fractured[num_1] == false && Fractured[num] == false)
                        {
                            _new = Pressure_y[num_1] / Neigh2[num_1][0];
                            if (x - 1 < 0) _new *= 2;
                            Pressure_new_y[num] -= _new;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            _new = Pressure_z[num_1] / Neigh2[num_1][0];
                            if (x - 1 < 0) _new *= 2;
                            Pressure_new_z[num] -= _new;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                        }
                    }
                }

                if (y - 1 >= 0 || y + 1 >= sq_in_rad + 1)
                {
                    int num_1 = map.Num_Crypt(x, y - 1, z);
                    if (map.Component[0].Contains(num_1) == false)
                    {
                        double _new = Pressure_y[num_1] / Neigh2[num_1][1];
                        if (y >= sq_in_rad) _new *= 2;
                        Pressure_new_y[num] -= _new;
                        Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                        if (Fractured[num_1] == false && Fractured[num] == false)
                        { 
                        _new = Pressure_x[num_1] / Neigh2[num_1][1];
                        if (y >= sq_in_rad) _new *= 2;
                        Pressure_new_x[num] -= _new;
                        Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                        _new = Pressure_z[num_1] / Neigh2[num_1][1];
                        if (y >= sq_in_rad) _new *= 2;
                        Pressure_new_z[num] -= _new;
                        Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                        }
                    }
                }

                if (y + 1 < sq_in_rad + 1)
                {
                    int num_1 = map.Num_Crypt(x, y + 1, z);
                    if (map.Component[0].Contains(num_1) == false)
                    {
                        double _new = Pressure_y[num_1] / Neigh2[num_1][1];
                        Pressure_new_y[num] -= _new;
                        Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                        if (Fractured[num_1] == false && Fractured[num] == false)
                        {
                            _new = Pressure_x[num_1] / Neigh2[num_1][1];
                            Pressure_new_x[num] -= _new;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            _new = Pressure_z[num_1] / Neigh2[num_1][1];
                            Pressure_new_z[num] -= _new;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                        }
                    }
                }
                if (z - 1 >= 0)
                {
                    int num_1 = map.Num_Crypt(x, y, z - 1);
                    if (map.Component[0].Contains(num_1)==false)
                    {
                        double _new = Pressure_z[num_1] / Neigh2[num_1][2];
                        Pressure_new_z[num] -= _new;
                        Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                        if (Fractured[num_1] == false && Fractured[num] == false)
                        {
                            _new = Pressure_x[num_1] / Neigh2[num_1][2];
                            Pressure_new_x[num] -= _new;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            _new = Pressure_y[num_1] / Neigh2[num_1][2];
                            Pressure_new_y[num] -= _new;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                        }
                    }
                }

                if (z + 1 < cell_in_depth)
                {
                    int num_1 = map.Num_Crypt(x, y, z + 1);
                    if (map.Component[0].Contains(num_1) == false)
                    {
                        double _new = Pressure_z[num_1] / Neigh2[num_1][2];
                        Pressure_new_z[num] -= _new;
                        Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                        Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                        if (Fractured[num_1] == false && Fractured[num] == false)
                        { 
                            _new = Pressure_x[num_1] / Neigh2[num_1][2];
                            Pressure_new_x[num] -= _new;
                            Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            _new = Pressure_y[num_1] / Neigh2[num_1][2];
                            Pressure_new_y[num] -= _new;
                            Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                            Pressure_new_z[num] += _new * Materials[k - 1].p_ratio; 
                        }
                    }
                }

            }

        }
        
        void Interact_Sym2(int iter)
        {
            List<double> Pressure_new_x = new List<double>();
            List<double> Pressure_new_y = new List<double>();
            List<double> Pressure_new_z = new List<double>();
            List<double> Pressure_l_x = new List<double>();
            List<double> Pressure_l_y = new List<double>();
            List<double> Pressure_l_z = new List<double>();
            for (int i = 0; i < max_num; i++)
            {
                Pressure_new_x.Add(0);
                Pressure_new_y.Add(0);
                Pressure_new_z.Add(0);
                Pressure_l_x.Add(0);
                Pressure_l_y.Add(0);
                Pressure_l_z.Add(0);
            }
            for (int ss =0; ss< iter;ss++)
            {
                for (int k = 1; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++)
                    {
                        int num = map.Component[k][i];

                        Pressure_new_x[num] = Pressure_l_x[num];
                        Pressure_new_y[num] = Pressure_l_y[num];
                        Pressure_new_z[num] = Pressure_l_z[num];
                        Pressure_l_x[num]=0;
                        Pressure_l_y[num]=0;
                        Pressure_l_z[num] =0;
                    }

                for (int k = 1; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++)
                    {
                        int x = 0; int y = 0; int z = 0;
                        int num = map.Component[k][i];
                        map.Num_Decrypt(num, ref x, ref y, ref z);

                        if (x - 1 >= 0)
                        {
                            int num_1 = map.Num_Crypt(x - 1, y, z);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                double _new = Pressure_x[num_1] / Neigh[num_1];
                                Pressure_new_x[num] -= _new;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            }
                            else
                            {
                                double _new = Pressure_x[num] / Neigh[num];
                                Pressure_l_x[num] += _new;
                                Pressure_l_y[num] -= _new * Materials[k - 1].p_ratio;
                                Pressure_l_z[num] -= _new * Materials[k - 1].p_ratio;
                            }
                        }

                        if (x + 1 < sq_in_rad + 1 || x - 1 < 0)
                        {
                            int num_1 = map.Num_Crypt(x + 1, y, z);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                double _new = Pressure_x[num_1] / Neigh[num_1];
                                if (x - 1 < 0) _new *= 2;
                                Pressure_new_x[num] -= _new;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            }
                            else
                            {
                                double _new = Pressure_x[num] / Neigh[num];
                                if (x - 1 < 0) _new *= 2;
                                Pressure_l_x[num] += _new;
                                Pressure_l_y[num] -= _new * Materials[k - 1].p_ratio;
                                Pressure_l_z[num] -= _new * Materials[k - 1].p_ratio;
                            }
                        }
                        if (y - 1 >= 0 || y + 1 >= sq_in_rad + 1)
                        {
                            int num_1 = map.Num_Crypt(x, y - 1, z);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                double _new = Pressure_y[num_1] / Neigh[num_1];
                                if (y >= sq_in_rad) _new *= 2;
                                Pressure_new_y[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            }
                            else
                            {
                                double _new = Pressure_y[num] / Neigh[num];
                                if (y >= sq_in_rad) _new *= 2;
                                Pressure_l_y[num] += _new;
                                Pressure_l_x[num] -= _new * Materials[k - 1].p_ratio;
                                Pressure_l_z[num] -= _new * Materials[k - 1].p_ratio;
                            }
                        }

                        if (y + 1 < sq_in_rad + 1)
                        {
                            int num_1 = map.Num_Crypt(x, y + 1, z);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                double _new = Pressure_y[num_1] / Neigh[num_1];
                                Pressure_new_y[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_z[num] += _new * Materials[k - 1].p_ratio;
                            }
                            else
                            {
                                double _new = Pressure_y[num] / Neigh[num];
                                Pressure_l_y[num] += _new;
                                Pressure_l_x[num] -= _new * Materials[k - 1].p_ratio;
                                Pressure_l_z[num] -= _new * Materials[k - 1].p_ratio;
                            }
                        }
                        if (z - 1 >= 0)
                        {
                            int num_1 = map.Num_Crypt(x, y, z - 1);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                double _new = Pressure_z[num_1] / Neigh[num_1];
                                Pressure_new_z[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            }
                            else
                            {
                                double _new = Pressure_z[num] / Neigh[num];
                                Pressure_l_z[num] += _new;
                                Pressure_l_x[num] -= _new * Materials[k - 1].p_ratio;
                                Pressure_l_y[num] -= _new * Materials[k - 1].p_ratio;
                            }
                        }

                        if (z + 1 < cell_in_depth)
                        {
                            int num_1 = map.Num_Crypt(x, y, z + 1);
                            if (map.Component[0].Contains(num_1) == false)
                            {
                                double _new = Pressure_z[num_1] / Neigh[num_1];
                                Pressure_new_z[num] -= _new;
                                Pressure_new_x[num] += _new * Materials[k - 1].p_ratio;
                                Pressure_new_y[num] += _new * Materials[k - 1].p_ratio;
                            }
                            else
                            {
                                double _new = Pressure_z[num] / Neigh[num];
                                Pressure_l_z[num] += _new;
                                Pressure_l_x[num] -= _new * Materials[k - 1].p_ratio;
                                Pressure_l_y[num] -= _new * Materials[k - 1].p_ratio;
                            }
                        }

                    }

                for (int k = 1; k < map.Component.Length; k++)
                    for (int i = 0; i < map.Component[k].Count; i++)
                    {
                        int num = map.Component[k][i];

                        Pressure_x[num] = Pressure_new_x[num];
                        Pressure_y[num] = Pressure_new_y[num];
                        Pressure_z[num] = Pressure_new_z[num];
                        Pressure_new_x[num]=0;
                        Pressure_new_y[num]=0;
                        Pressure_new_z[num]=0;
                    }
                

            }
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int num = map.Component[k][i];

                    Pressure_x[num] += Pressure_l_x[num];
                    Pressure_y[num] += Pressure_l_y[num];
                    Pressure_z[num] += Pressure_l_z[num];
                }


        }


        void Interact_Alt(Map map)
        {
            List<double>[] cell_l_d_new = new List<double>[2];
            List<double> Pressure_new = new List<double>();
            for (int i = 0; i < max_num; i++) //при отсутствии пор!
            {
                Pressure_new.Add(0);
            }


             for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int x = 0; int y = 0; int z = 0;
                    int num = map.Component[k][i];
                    map.Num_Decrypt(map.Component[k][i], ref x, ref y, ref z);

                    bool[] _dir = new bool[6] { true, true, true, true, true, true };
                    int[] dir = new int[] { 0, 0, 0, 0,0,0 };

                    for (int ss = 0; ss < _dir.Length; ss++)
                        while (_dir[ss] == true)
                        {
                            switch (ss)
                            {
                                case 0:  // влево
                                    {
                                        if (x - 1 - dir[ss] >= 0 && map.Component[0].Contains(map.Num_Crypt(x - 1 - dir[ss], y, z)) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false;  }
                                        break;
                                    }
                                case 1: // вправо
                                    {
                                        if (x + 1 + dir[ss] < cell_in_row && map.Component[0].Contains(map.Num_Crypt(x + 1 + dir[ss], y, z)) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false;  }
                                        break;
                                    }
                                case 2: // вниз
                                    {
                                        if (y - 1 - dir[ss] >= 0 && map.Component[0].Contains(map.Num_Crypt(x , y - 1 - dir[ss], z)) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false;  }
                                        break;
                                    }
                                case 3: // вверх
                                    {
                                        if (y + 1 + dir[ss] < cell_in_col && map.Component[0].Contains(map.Num_Crypt(x , y + 1 + dir[ss], z)) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false;  }
                                        break;
                                    }
                                case 4: // назад
                                    {
                                        if (z - 1 - dir[ss] >= 0 && map.Component[0].Contains(map.Num_Crypt(x, y, z + 1 + dir[ss])) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false; }
                                        break;
                                    }
                                case 5: // вперед
                                    {
                                        if (z + 1 + dir[ss] < cell_in_depth && map.Component[0].Contains(map.Num_Crypt(x, y, z - 1 - dir[ss])) == false)
                                            dir[ss]++;
                                        else { _dir[ss] = false; }
                                        break;
                                    }
                            } 
                        }
                    byte neigh = 6;
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
                                        Pressure_new[map.Num_Crypt(x - dd, y, z)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                        Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                        break;
                                    }
                                case 1:
                                    {
                                        Pressure_new[map.Num_Crypt(x + dd, y, z)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                        Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                        break;
                                    }
                                case 2:
                                    {
                                        Pressure_new[map.Num_Crypt(x, y - dd, z)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                        Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                        break;
                                    }
                                case 3:
                                    {
                                        Pressure_new[map.Num_Crypt(x, y + dd, z)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                        Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                        break;
                                    }
                                case 4:
                                    {
                                        Pressure_new[map.Num_Crypt(x, y, z - dd)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
                                        Pressure_new[num] += Pressure[num] / neigh / dir[ss] * (1 - Pow(transpass_, dd));
                                        break;
                                    }
                                case 5:
                                    {
                                        Pressure_new[map.Num_Crypt(x, y, z + dd)] += Pressure[num] / neigh / dir[ss] * Pow(transpass_, dd);
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

        // Функции, возвращающие цвет клетки в зависимости от нагрузки
        Color Color_Define(double x, double max, double cut = 1) //Для отрицательных и положительных нагрузок
        {
            double ratio = x/max;
            int red = 0;
            int green = 0;
            int blue = 0;

            if (Abs(ratio) > cut) return Color.HotPink;
            else ratio /= cut;

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
        Color Color_Define_Alt(double x, double max, double cut = 1) //Для положительных нагрузок
        {
            double ratio = x / max;
            int red = 0;
            int green = 0;
            int blue = 0;

            if (Abs(ratio) > cut) return Color.HotPink;
            else ratio /= cut;

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
        Color Color_Define_Log(double x, double max, double cut = 1) //Логарифмическая шкала, только +
        {
            if (x <= 0) { return Color.RosyBrown; }
            if (x <  max * 0.000000001) { return Color.SaddleBrown; }
            if (x >= max * 0.000000001  && x < max * 0.00000001) { return Color.Maroon; }
            if (x >= max * 0.00000001   && x < max * 0.0000001) { return Color.Magenta; }
            if (x >= max * 0.0000001    && x < max * 0.000001) { return Color.Purple; }
            if (x >= max * 0.000001     && x < max * 0.00001) { return Color.Blue; }
            if (x >= max * 0.00001      && x < max * 0.0001) { return Color.DodgerBlue; }
            if (x >= max * 0.0001       && x < max * 0.001) { return Color.SeaGreen; }
            if (x >= max * 0.001        && x < max * 0.01) { return Color.Yellow; }
            if (x >= max * 0.01         && x < max * 0.1) { return Color.Orange; }
            return Color.Red; 
        }
        Color Color_Define_Log2(double x, double max, double cut = 1) //Логарифмическая шкала, модуль
        {
            max = -max;
            if (x >= 0) { return Color.RosyBrown; }
            if (x >  max * 0.000000001) { return Color.SaddleBrown; }
            if (x <= max * 0.000000001  && x > max * 0.00000001) { return Color.Maroon; }
            if (x <= max * 0.00000001   && x > max * 0.0000001) { return Color.Magenta; }
            if (x <= max * 0.0000001    && x > max * 0.000001) { return Color.Purple; }
            if (x <= max * 0.000001     && x > max * 0.00001) { return Color.Blue; }
            if (x <= max * 0.00001      && x > max * 0.0001) { return Color.DodgerBlue; }
            if (x <= max * 0.0001       && x > max * 0.001) { return Color.SeaGreen; }
            if (x <= max * 0.001        && x > max * 0.01) { return Color.Yellow; }
            if (x <= max * 0.01         && x > max * 0.1) { return Color.Orange; }
            return Color.Red;
            
        }

        void Count_Angle(Map map) // учитывает вращение только в плоскости ху (по оси z)
        {
            List<double> Angle_new = new List<double>();
            for (int i = 0; i < cell_in_col * cell_in_row; i++)
            {
                Angle_new.Add(0);
            }
            for (int k = 1; k < map.Component.Length; k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                    int x = 0; int y = 0; int z = 0;
                    int num = map.Component[k][i];
                    map.Num_Decrypt(map.Component[k][i], ref x, ref y, ref z);
                    double[] theta = new double[4] { 0, 0, 0, 0 };

                    // Учитываются только соседи на углах
                    if (x - 1 >= 0 && y - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x - 1, y - 1, z)) == false && Pressure[map.Num_Crypt(x - 1, y - 1, z)] != 0) // слева сверху
                    {
                        int num1 = map.Num_Crypt(x - 1, y - 1, z);
                        int kk = 1; // индекс материала соседа
                        while (map.Component[kk].Contains(num1) == false)
                        {
                            kk++;
                            if (kk > map.Component.Length - 1) throw new Exception("Невозможно определить материал");
                        }

                        Count_dl(out double a, out double b, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[num], cell_size);
                        double c = 0.5 * Sqrt((cell_size + a) * (cell_size + a) + (cell_size + b) * (cell_size + b));
                        double alpha = Atan((cell_size + b) / (cell_size + a));
                        Count_dl(out double a1, out double b1, Materials[kk - 1].elastic_m, Materials[kk - 1].p_ratio, Pressure[num1], cell_size);
                        double d = Sqrt(a1 * a1 + b1 * b1) * 0.5;
                        double beta = Acos((a1 * a1 - b1 * b1 + d * d * 4) / (4 * a1 * d));
                        double f = Sqrt(c * c + d * d - 2 * c * d * Cos(PI - alpha - beta + Angle[num1]));// Pi/2?
                        theta[0] = Acos(c / f);
                        if (Pressure[num1] < 0) theta[0] *= -1; 
                    }

                    if (x + 1 < cell_in_row && y - 1 >= 0 && map.Component[0].Contains(map.Num_Crypt(x + 1, y - 1, z)) == false && Pressure[map.Num_Crypt(x + 1, y - 1, z)] != 0) // справа сверху
                    {
                        int num1 = map.Num_Crypt(x + 1, y - 1, z);
                        int kk = 1; // индекс материала соседа
                        while (map.Component[kk].Contains(num1) == false)
                        {
                            kk++;
                            if (kk > map.Component.Length - 1) throw new Exception("Невозможно определить материал");
                        }

                        Count_dl(out double a, out double b, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[num], cell_size);
                        double c = 0.5 * Sqrt((cell_size + a) * (cell_size + a) + (cell_size + b) * (cell_size + b));
                        double alpha = Atan((cell_size + b) / (cell_size + a));
                        Count_dl(out double a1, out double b1, Materials[kk - 1].elastic_m, Materials[kk - 1].p_ratio, Pressure[num1], cell_size);
                        double d = Sqrt(a1 * a1 + b1 * b1) * 0.5;
                        double beta = Acos((a1 * a1 - b1 * b1 + d * d * 4) / (4 * a1 * d));
                        double f = Sqrt(c * c + d * d - 2 * c * d * Cos(PI - alpha - beta + Angle[num1])); //- 2 * c * d * Cos(PI/2)   // Pi/2?
                        theta[1] = Acos(c / f);
                        if (Pressure[num1] > 0) theta[1] *= -1;
                    }
                    if (x - 1 >= 0 && y + 1 < cell_in_col && map.Component[0].Contains(map.Num_Crypt(x - 1, y + 1, z)) == false && Pressure[map.Num_Crypt(x - 1, y + 1, z)] != 0) // слева снизу
                    {
                        int num1 = map.Num_Crypt(x - 1, y + 1, z);
                        int kk = 1; // индекс материала соседа
                        while (map.Component[kk].Contains(num1) == false)
                        {
                            kk++;
                            if (kk > map.Component.Length - 1) throw new Exception("Невозможно определить материал");
                        }

                        Count_dl(out double a, out double b, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[num], cell_size);
                        double c = 0.5 * Sqrt((cell_size + a) * (cell_size + a) + (cell_size + b) * (cell_size + b));
                        double alpha = Atan((cell_size + b) / (cell_size + a));
                        Count_dl(out double a1, out double b1, Materials[kk - 1].elastic_m, Materials[kk - 1].p_ratio, Pressure[num1], cell_size);
                        double d = Sqrt(a1 * a1 + b1 * b1) * 0.5;
                        double beta = Acos((a1 * a1 - b1 * b1 + d * d * 4) / (4 * a1 * d));
                        double f = Sqrt(c * c + d * d - 2 * c * d * Cos(PI - alpha - beta + Angle[num1]));   // Pi/2?
                        theta[2] = Acos(c / f);
                        if (Pressure[num1] > 0) theta[2] *= -1;
                    }

                    if (x + 1 < cell_in_row && y + 1 < cell_in_col && map.Component[0].Contains(map.Num_Crypt(x + 1, y + 1, z)) == false && Pressure[map.Num_Crypt(x + 1, y + 1, z)] != 0 ) // справа снизу
                    {
                        int num1 = map.Num_Crypt(x + 1, y + 1, z);
                        int kk = 1; // индекс материала соседа
                        while (map.Component[kk].Contains(num1) == false)
                        {
                            kk++;
                            if (kk > map.Component.Length - 1) throw new Exception("Невозможно определить материал");
                        }

                        Count_dl(out double a, out double b, Materials[k - 1].elastic_m, Materials[k - 1].p_ratio, Pressure[num], cell_size);
                        double c = 0.5 * Sqrt((cell_size + a) * (cell_size + a) + (cell_size + b) * (cell_size + b)); // определение величины половины диагонали центральной клетки
                        double alpha = Atan((cell_size + b) / (cell_size + a));
                        
                        Count_dl(out double a1, out double b1, Materials[kk - 1].elastic_m, Materials[kk - 1].p_ratio, Pressure[num1], cell_size);
                        double d = Sqrt(a1 * a1 + b1 * b1) * 0.5;
                        double beta = Acos((a1 * a1 - b1 * b1 + d * d*4) / (4 * a1 * d));

                        double f = Sqrt(c * c + d * d - 2 * c * d * Cos(PI - alpha - beta + Angle[num1]));   // Pi/2?
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
            if (Stress[stress_in].Max() > Abs(Stress[stress_in].Min())) max_P = Stress[stress_in].Max();
            else max_P = Abs(Stress[stress_in].Min());
            return max_P;
        }

        bool Mises_Cr(int num, int mat_index) 
        {
            double cr = Von_Mises_Count(num);
            //double x = Pressure_x[num];
            //double y = Pressure_y[num];
            //double z = Pressure_z[num];
            //double cr = Sqrt(0.5 * ((x - y) * (x - y) + (y - z) * (y - z) + (x - z) * (x - z)));
            bool res = cr > Materials[mat_index].yeild_str;
            return res;
        
        }
        double Von_Mises_Count(int num) 
        {
            double x = Pressure_x[num];
            double y = Pressure_y[num];
            double z = Pressure_z[num];
            double cr = Sqrt(0.5 * ((x - y) * (x - y) + (y - z) * (y - z) + (x - z) * (x - z)));
            if (cell_in_depth == 1) cr = Sqrt(0.5 * ((x - y) * (x - y)));
            return cr;
        }
        bool Griffith_Cr(int num, int mat_index, double epsilon = 1) 
        {
            // тк double сравнить с 0 проблематично, ввели эпсилон
            // 8 можно выставить как параметр, тк это теоретическое значение
            double tensile_str = Materials[mat_index].tensile_str;
            double max = Max(Max(Pressure_x[num], Pressure_y[num]), Pressure_z[num]);
            double min = Min(Min(Pressure_x[num], Pressure_y[num]), Pressure_z[num]);
            if (3 * max + min >= 0)
            {
                if (max - tensile_str >= epsilon)
                    return true;
            }
            else if (3 * max + min < 0)
            {
                if ((max - min) * (max - min) + 8 * tensile_str * (max + min) >= epsilon)
                    return true;
            }
                return false;

            }

        void Count_VM_and_Hydro(int num) 
        {
            Von_Mises[num] = Von_Mises_Count(num);
            Pressure[num] = (Pressure_x[num] + Pressure_y[num] + Pressure_z[num]) / 3;
        }
        void Count_cell_size()
        { 
            int sq_in_diam = 2 * sq_in_rad + 1;
            cell_size = diameter / sq_in_diam;
            label1.Text = "Размер клетки = " + Round(cell_size*1000,2) + " мм";
        }
        void Count_app_press(/*Map map*/)
        {
            int affected_cells = 0;
            for (int x =0; x<cell_in_row;x++)
            {
                // Считаем, сколько клеток на поверхности (у = 1) подвергаются воздействию  в одном слое по глубине (z = 0)
                if (map.Component[0].Contains(map.Num_Crypt(x, 1, 0)) == true) continue; 
                else affected_cells++;
            }
            affected_cells *= cell_in_depth; // Считаем, сколько клеток на поверхности (у = 1) подвергаются воздействию на всех слоях
            if (quater == true) affected_cells = (affected_cells - 1) * 2 + 1;
            app_press = force / (affected_cells * cell_size * cell_size); // Рressure = Force/Area
        }

        void Define_Empirical(int iter, bool first = true)
        {
            //double param /*= 1185*/;
            //double a = 1.166;
            //double b = 7.509;
            //double c = a + b / (iter+1);
            //param /= c;
            //double a = 1183;
            //double b = -1200;
            //double c = 0;
            iter++;
            switch (iter)
            {
                //case 1: { emp_param = 8958; return; }
                //case 2: { emp_param = 3293; return; }
                //case 3: { emp_param = 2511; return; }
                //case 4: { emp_param = 2207; return; }
                //case 5: { emp_param = 2050; return; }
                //case 6: { emp_param = 1958; return; }
                //case 7: { emp_param = 1896; return; }
                //case 8: { emp_param = 1855; return; }
                //case 9: { emp_param = 1823; return; }
                //case 10: { emp_param = 1799; return; }
                //case 11: { emp_param = 1781; return; }
                //case 12: { emp_param = 1766; return; }
                //case 13: { emp_param = 1754; return; }
                //case 14: { emp_param = 1744; return; }
                //case 15: { emp_param = 1735; return; }
                //case 16: { emp_param = 1728; return; }
                //case 17: { emp_param = 1722; return; }
                //case 18: { emp_param = 1715; return; }
                //case 19: { emp_param = 1712; return; }
                //case 20: { emp_param = 1707; return; }
                //case 21: { emp_param = 1702; return; }
                //case 22: { emp_param = 1712; return; }
                //case 23: { emp_param = 1696; return; }
                //case 24: { emp_param = 1698; return; }
                //case 25: { emp_param = 1686; return; }
                //case 26: { emp_param = 1688; return; }
                //case 27: { emp_param = 1686; return; }
                //case 28: { emp_param = 1684; return; }
                //case 29: { emp_param = 1682; return; }
                //case 30: { emp_param = 1680; return; }
                //case 31: { emp_param = 1678; return; }
                //case 32: { emp_param = 1677; return; }
                //case 33: { emp_param = 1676; return; }
                //case 34: { emp_param = 1674; return; }
                //case 35: { emp_param = 1672; return; }
                //case 36: { emp_param = 1672; return; }
                //case 37: { emp_param = 1672; return; }
                    //    //case 34: { emp_param = 18631; return; }
                    //    //case 35: { emp_param = 18616; return; }
                    //    //case 39: { emp_param = 18559; return; }
                    //    //case 40: { emp_param = 18583; return; }
                    //    //case 41: { emp_param = 18544; return; }
                    //    //case 42: { emp_param = 18535; return; }
                    //    //case 43: { emp_param = 18525; return; }
                    //    //case 44: { emp_param = 18516; return; }
                    //    //case 55: { emp_param = 18615; return; }
                    //        //case 56: { emp_param = 18434; return; }
                    //        //case 57: { emp_param = 18428; return; }
                    //        //case 58: { emp_param = 18423; return; }
                    //        //case 59: { emp_param = 18418; return; }
                    //        //case 60: { emp_param = 18413; return; }
                    //        //case 61: { emp_param = 18409; return; }
                    //        //case 62: { emp_param = 18405; return; }
                    //        //case 63: { emp_param = 18401; return; }
                    //        //case 64: { emp_param = 18395; return; }
                    //        //case 65: { emp_param = 18393; return; }
                    //        //case 66: { emp_param = 18390; return; }
                    //        //case 67: { emp_param = 18387; return; }
                    //        //case 68: { emp_param = 18383; return; }
                    //        //case 69: { emp_param = 18381; return; }
            }
            //double Dt = (double)(cell_in_depth) / cell_in_row;
            if (iter < 20)
            {
                emp_param = 8958 - 7246 + 1311.2 * Exp(-(iter - 1) / 3.167) + 5934.8 * Exp(-(iter - 1) / 0.44811); //cd = 16
                //emp_param = 104923 - 85877 + 14644 * Exp(-(iter - 1) / 3.181) + 71233 * Exp(-(iter - 1) / 0.4704); //cd =4
                
                //emp_param /= (12.269 - 19.36 * Exp(-(Dt - 0.097561) / 0.084862) + 8.0915 * Exp(-(Dt - 0.097561) / 0.035467));
                //emp_param *= 0.01024 + 0.077347 * Exp(-(iter - 1) / 50.835) - 0.087587 * Exp(-(iter - 1) / 0.89822) + 0.995;

                return;
            }
            if (iter <= 70)
            {
                emp_param = -0.0006 * Pow(iter-20,3) + 0.0731 *Pow(iter - 20, 2) - 3.2801 * (iter - 20) + 1707.3; //cd = 16
                // emp_param = -498.1 * Log(iter) + 20411;//cd =4

                //   emp_param /= (12.269 - 19.36 * Exp(-(Dt - 0.097561) / 0.084862) + 8.0915 * Exp(-(Dt - 0.097561) / 0.035467));
                //emp_param *= 0.01024 + 0.077347 * Exp(-(iter - 1) / 50.835) - 0.087587 * Exp(-(iter - 1) / 0.89822) + 0.995;  
                
                return;
            }
            emp_param = 1650.81 - 17.7 + 13.246 * Exp(-(iter - 71) / 114.87) + 4.4536 * Exp(-(iter - 71) / 23.727); //cd =16
            //emp_param = 18369 - 199 + 148.43 * Exp(-(iter - 71) / 115.64) + 50.567 * Exp(-(iter - 71) / 23.567); //cd =4
            //  emp_param /= (12.269 - 19.36 * Exp(-(Dt - 0.097561) / 0.084862) + 8.0915 * Exp(-(Dt - 0.097561) / 0.035467));

            //emp_param *= 0.01024 + 0.077347 * Exp(-(iter - 1) / 50.835) - 0.087587 * Exp(-(iter - 1) / 0.89822) + 0.995;
            return;

            //if (iter >= 20 && iter < 39) { emp_param = -0.0387 * iter * iter * iter + 4.1585 * iter * iter - 164.23 * iter + 20928; return; }
            //if (iter >= 39 && iter < 55) { emp_param = -0.0014 * iter * iter * iter + 0.3546 * iter * iter - 33.337 * iter + 19410; return; }
            //if (iter >= 56 && iter < 80) { emp_param = -4.824 * iter + 18704; return; }

            //if (iter <= 15) { emp_param = 2195 - 2310 / Pow(iter + 0.8, 0.29); return; }
            //else if (iter <= 200) { emp_param = 1186 - 400 / iter; return; }
            //else emp_param = 1185;

            emp_param = 1657;
        }
        private void nUD_SquareNum_ValueChanged(object sender, EventArgs e)
        {
            sq_in_rad = (int)nUD_SquareNum.Value;
            cell_in_col = sq_in_rad*2+1;
            cell_in_row = cell_in_col;
            Count_cell_size();

            nUD_Thickness_ValueChanged(sender,e);
            if (rB_Force.Checked == true) rB_Force_CheckedChanged(sender, e);
            else rB_App_Press_CheckedChanged(sender, e);
        }

       

        // Блокирование элементов интерфейса во время расчета
        private void Interface_Block_Off(bool block)
        {
            nUD_Depth.Enabled = block;
            nUD_MaterialsNum.Enabled = block;
            nUD_SquareNum.Enabled = block;
            nUD_iter.Enabled = block;
            nUD_Thickness.Enabled = block;
            nUD_Diameter.Enabled = block;
            panel_Force.Enabled = block;

        }

        // Обновление некоторых элементов интерфейса
        async Task Progress_Bar_Update(double max_P)
        {
            label_Max_P.Text = "Max F = " + max_P;
            textBox1.Text += max_P + Environment.NewLine;
            label_Sigma_at_break.Text = "2*F/(Pi*D*t) = " + Round(2 * force_sum / (PI * thickness * diameter), 2);

            label_progress_bar.Refresh();
            textBox1.Refresh();
            label_Sigma_at_break.Refresh();
            label_Max_P.Refresh();
            tB_Fracture.Refresh();
        }

        // Выбор отображаемого поля
        private void rBBoth_CheckedChanged(object sender, EventArgs e)
        {
            if (rBBoth.Checked == true)
            {
                stress_in = 0;
                if (Pressure != null && Pressure.Count > 0) 
                {
                    for (int u = 0; u < max_num; u++)
                        if (map.Component[0].Contains(u) == false) Count_VM_and_Hydro(u);
                    Display(); 
                }
            }
        }
        private void rBsigma_x_CheckedChanged(object sender, EventArgs e)
        {
            if (rBsigma_x.Checked == true)
            {
                stress_in = 1;
                if ( Pressure_x != null && Pressure_x.Count > 0) Display();
            }
        }
        private void rBsigma_y_CheckedChanged(object sender, EventArgs e)
        {
            if (rBsigma_y.Checked == true)
            {
                stress_in = 2;
                if (Pressure_y != null && Pressure_y.Count > 0) Display();
            }
        }
        private void rBsigma_z_CheckedChanged(object sender, EventArgs e)
        {
            if (rBsigma_z.Checked == true)
            { 
                stress_in = 3;
                if(Pressure_z != null && Pressure_z.Count > 0) Display();
            }
        }
        private void rBAngle_CheckedChanged(object sender, EventArgs e)
        {
            if (rBAngle.Checked == true)
            {
                stress_in = 4;
                if ( Angle != null && Angle.Count > 0 /*&& Angle.Max() > 0*/) Display(); 
            }
        }
        private void rBShear_CheckedChanged(object sender, EventArgs e)
        {
            if (rBShear.Checked == true) 
            {
                stress_in = 5; 
                if(Shear != null && Shear.Count > 0 /*&& Angle.Max() > 0*/) Display();
            }
        }
        private void rBMises_CheckedChanged(object sender, EventArgs e)
        {
            if (rBMises.Checked == true)
            {

                stress_in = 6;
                if (Von_Mises != null && Von_Mises.Count > 0)
                {
                    for (int u = 0; u < max_num; u++)
                        if (map.Component[0].Contains(u) == false) Count_VM_and_Hydro(u);
                    Display(); 
                }
            }
        }

        // Выбор цветового отображения
        private void rB_CD_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_CD.Checked == true) 
            { 
                Color_Function = Color_Define; 
                PictureBox_ColorLine_ChangePic(0);
            }
            if (Stress != null && Stress[stress_in] != null) Display();
            
        }
        private void rB_CDA_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_CDA.Checked == true)
            { 
                Color_Function = Color_Define_Alt;
                PictureBox_ColorLine_ChangePic(1);
            }
            if (Stress != null && Stress[stress_in] != null) Display();
            
        }
        private void rB_Log_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_Log.Checked == true) 
            { 
                Color_Function = Color_Define_Log;
                PictureBox_ColorLine_ChangePic(2);
            }
            if (Stress != null && Stress[stress_in] != null) Display();
        }
        private void rB_Log2_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_Log2.Checked == true) 
            { 
                Color_Function = Color_Define_Log2;
                PictureBox_ColorLine_ChangePic(3);
            }
            if (Stress != null && Stress[stress_in] != null) Display();
        }

        private void nUD_Depth_ValueChanged(object sender, EventArgs e)
        {
            Count_cell_size();
            if (nUD_Depth.Value > nUD_Depth.Maximum) cell_in_depth = (int)nUD_Depth.Maximum;
            else cell_in_depth = (int)nUD_Depth.Value;
            trackBar_Depth.Maximum = cell_in_depth - 1;
            thickness = cell_in_depth * cell_size;
            nUD_Thickness.Value = (decimal)(thickness * 1000);

        }

        private void trackBar_Depth_Scroll(object sender, EventArgs e)
        {
            display_z = trackBar_Depth.Value;
            if (Stress != null && Stress[stress_in] != null) Display();
        }
       

        private void nUD_Diameter_ValueChanged(object sender, EventArgs e)
        {
            diameter =(double)nUD_Diameter.Value*0.001;
            Count_cell_size();
            
            if (rB_Force.Checked == true) rB_Force_CheckedChanged(sender, e);
            else rB_App_Press_CheckedChanged(sender, e);
            
        }

        private void nUD_Thickness_ValueChanged(object sender, EventArgs e)
        {
            Count_cell_size();
            thickness = (double)nUD_Thickness.Value*0.001;
            cell_in_depth = (int) Round(thickness / cell_size);
            if (cell_in_depth < 1) cell_in_depth = 1;
            nUD_Depth.Value = cell_in_depth;
            trackBar_Depth.Maximum = cell_in_depth - 1;
        }

        private void nuD_tt_ValueChanged(object sender, EventArgs e)
        {
            ttt = (byte)nuD_tt.Value; 
        }

        private void nUD_iter_ValueChanged(object sender, EventArgs e)
        {
            iter_num = (int)nUD_iter.Value;
            Update_Force_Panel();
        }

        private void PictureBox_ColorLine_ChangePic(byte k) 
        {
            // k == 0 - rB_CD.Checked == true
            // k == 1 - rB_CDA.Checked == true
            // k == 2 - rB_Log.Checked == true
            // k == 3 - rB_Log.Checked == true
            
            for (int i = 0; i < panel_ColorLineHolder.Controls.Count;) 
            {
                if (panel_ColorLineHolder.Controls[i].GetType() == typeof(Label)) panel_ColorLineHolder.Controls.RemoveAt(i);
                else i++;
            }
            Bitmap bmp;
            bmp = new Bitmap(pictureBox_ColorLine.Width, pictureBox_ColorLine.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle rect;
                Color rect_color;
                switch (k)
                {
                    case 0: 
                        {

                            byte iter = 20;
                            for (byte i = 0; i <= iter; i++) 
                            {
                                rect = new Rectangle(0, bmp.Height / iter * i, bmp.Width, bmp.Height / iter);
                                rect_color = Color_Function(i - iter / 2, iter / 2);
                                g.FillRectangle(new SolidBrush(rect_color), rect);
                                
                                Label label = new Label();
                                label.Text = ((double)i*2/iter - 1) * 100 + "%";
                                label.Font = new Font("Microsoft Sans Serif", 7, FontStyle.Underline);
                                label.AutoSize = true;
                                panel_ColorLineHolder.Controls.Add(label);
                                label.Location = new Point(label.Location.X, panel_ColorLine.Location.Y + bmp.Height / iter * i);
                            }
                        
                            break;
                        }
                    case 1: 
                        {
                            Label label;
                            byte iter = 10;

                            //rect = new Rectangle(0, 0, bmp.Width, bmp.Height / (iter + 1));
                            //rect_color = Color_Function(-1, iter);
                            //g.FillRectangle(new SolidBrush(rect_color), rect);

                            //label = new Label();
                            //label.Text = "< 0";
                            //label.Font = new Font("Microsoft Sans Serif", 7, FontStyle.Underline);
                            //label.AutoSize = true;
                            //panel_ColorLineHolder.Controls.Add(label);
                            //label.Location = new Point(label.Location.X, panel_ColorLine.Location.Y);

                            for (byte i = 0; i <= iter /*+ 1*/; i++)
                            {
                                rect = new Rectangle(0, bmp.Height / (iter+1) * i, bmp.Width, bmp.Height / (iter+1));
                                rect_color = Color_Function(i/*-1*/, iter);
                                g.FillRectangle(new SolidBrush(rect_color), rect);

                                label = new Label();
                                //string.Format("{0:F1}", 123.233424224))
                                label.Text =string.Format("{0:F0}", Round((double)(i/*-1*/) / iter*100, 0)) + " %";
                                label.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
                                label.AutoSize = true;
                                panel_ColorLineHolder.Controls.Add(label);
                                label.Location = new Point(label.Location.X, panel_ColorLine.Location.Y + bmp.Height / (iter + 1) * i);
                            }
                           
                            break; 
                        }
                    case 2:
                        {
                            Label label;
                            byte iter = 11;

                            for (byte i = 0; i < iter; i++)
                            {
                                rect = new Rectangle(0, bmp.Height / iter * i, bmp.Width, bmp.Height / iter);
                                rect_color = Color_Function(Pow(10,-i-1), 1);
                                g.FillRectangle(new SolidBrush(rect_color), rect);
                                
                                label = new Label();
                                if(i<iter-2)label.Text =">= 10e-" + (i+1);
                                else if(i==iter-2) label.Text = "< 10e-" + (i+1);
                                label.Font = new Font("Microsoft Sans Serif", 7, FontStyle.Underline);
                                label.AutoSize = true;
                                panel_ColorLineHolder.Controls.Add(label);
                                label.Location = new Point(label.Location.X, panel_ColorLine.Location.Y + bmp.Height / iter * i);

                            }
                            rect = new Rectangle(0, bmp.Height / iter * (iter - 1), bmp.Width, bmp.Height / iter);
                            rect_color = Color_Function(-1, 1);
                            g.FillRectangle(new SolidBrush(rect_color), rect);

                            label = new Label();
                            label.Text = "< 0";
                            label.Font = new Font("Microsoft Sans Serif", 7, FontStyle.Underline);
                            label.AutoSize = true;
                            panel_ColorLineHolder.Controls.Add(label);
                            label.Location = new Point(label.Location.X, panel_ColorLine.Location.Y + bmp.Height/iter*(iter-1));

                            break;
                        }
                    case 3: 
                        {
                            Label label;
                            byte iter = 11;
                            for (byte i = 0; i < iter; i++)
                            {
                                rect = new Rectangle(0, bmp.Height / iter * i, bmp.Width, bmp.Height / iter);
                                rect_color = Color_Function(-Pow(10, -i - 1), 1);
                                g.FillRectangle(new SolidBrush(rect_color), rect);

                                label = new Label();
                                if (i < iter - 2) label.Text = "<= -10e-" + (i + 1);
                                else if (i == iter - 2) label.Text = "> -10e-" + (i + 1);
                                label.Font = new Font("Microsoft Sans Serif", 7, FontStyle.Underline);
                                label.AutoSize = true;
                                panel_ColorLineHolder.Controls.Add(label);
                                label.Location = new Point(label.Location.X, panel_ColorLine.Location.Y + bmp.Height / iter * i);
                            }
                            rect = new Rectangle(0, bmp.Height / iter * (iter - 1), bmp.Width, bmp.Height / iter);
                            rect_color = Color_Function(1, 1);
                            g.FillRectangle(new SolidBrush(rect_color), rect);
                            label = new Label();
                            label.Text = "> 0";
                            label.Font = new Font("Microsoft Sans Serif", 7, FontStyle.Underline);
                            label.AutoSize = true;
                            panel_ColorLineHolder.Controls.Add(label);
                            label.Location = new Point(label.Location.X, panel_ColorLine.Location.Y + bmp.Height / iter * (iter - 1));

                            break; 
                        }
                }

                
               
            }
            pictureBox_ColorLine.Image?.Dispose();
            pictureBox_ColorLine.Image = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
            bmp?.Dispose();
            pictureBox_ColorLine.Refresh();
        }


        // Сохранение и загрузка данных
        private void сохранитьИзображениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("Ошибка: изображение отсутствует");
                return;
            }
            Bitmap bmp = new Bitmap(pictureBoxMain.Image);
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "BMP Files(*.BMP)|*.BMP|JPG Files(*.JPG)|*.JPG|JPEG Files(*.JPEG)|*.JPEG|PNG files (*.PNG)|*.PNG|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    switch (saveFileDialog1.FilterIndex)
                    {
                        case 1:
                            bmp.Save(myStream, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                        case 4:
                            bmp.Save(myStream, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        default:
                            bmp.Save(myStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                    }
                    myStream.Close();
                }
            }
            bmp?.Dispose();
        }
        private void сохранитьСостоянияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Pressure_x == null || Pressure_y == null || Pressure_z == null || Pressure_x.Count == 0 || Pressure_y.Count == 0 || Pressure_z.Count == 0)
            {
                MessageBox.Show("Ошибка: данные о состоянии отсутствуют");
                return;
            }
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "TXT Files(*.TXT)|*.TXT|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    using (StreamWriter writer = new StreamWriter(myStream))
                    {
                        
                        writer.WriteLine(diameter + "; ");
                        writer.WriteLine(thickness + "; ");
                        writer.WriteLine(sq_in_rad + "; ");
                        writer.WriteLine(cell_in_depth + "; ");
                        for (int i = 0; i < max_num; i++)
                        {
                            writer.Write(Pressure_x[i] + "; ");
                        }
                        writer.WriteLine("");
                        for (int i = 0; i < max_num; i++)
                        {
                            writer.Write(Pressure_y[i] + "; ");
                        }
                        writer.WriteLine("");
                        for (int i = 0; i < max_num; i++)
                        {
                            writer.Write(Pressure_z[i] + "; ");
                        }

                    }

                    myStream.Close();
                }
            }

        }
        private void загрузитьСостоянияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TXT Files(*.TXT)|*.TXT|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = openFileDialog.OpenFile()) != null)
                {
                    using (StreamReader reader = new StreamReader(myStream))
                    {

                        string text;
                        text = reader.ReadLine();
                        text = text.Substring(0, text.IndexOf(';'));
                        diameter = Convert.ToDouble(text);
                        
                        text = reader.ReadLine();
                        text = text.Substring(0, text.IndexOf(';'));
                        thickness = Convert.ToDouble(text);

                        text = reader.ReadLine();
                        text = text.Substring(0, text.IndexOf(';'));
                        sq_in_rad = Convert.ToInt32(text);

                        text = reader.ReadLine();
                        text = text.Substring(0, text.IndexOf(';'));
                        cell_in_depth = Convert.ToInt32(text);

                        cell_in_col = sq_in_rad * 2 + 1;
                        cell_in_row = cell_in_col;
                        max_num = (sq_in_rad + 1) * (sq_in_rad + 1) * cell_in_depth;
                        Count_cell_size();

                        Pressure = new List<double>();
                        Pressure_x = new List<double>();
                        Pressure_y = new List<double>();
                        Pressure_z = new List<double>(); 
                        Angle = new List<double>();
                        Shear = new List<double>();
                        Von_Mises = new List<double>();
                        Stress = new List<double>[] { Pressure, Pressure_x, Pressure_y, Pressure_z, Shear, Angle, Von_Mises };

                        map = new Map(sq_in_rad, vol_frac, cell_in_row, cell_in_col, cell_in_depth, cell_size, quater);

                        text = reader.ReadToEnd();
                        string[] words = text.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < max_num; i++)
                        {
                            Pressure_x.Add(Convert.ToDouble(words[i]));
                            Pressure_y.Add(Convert.ToDouble(words[i+max_num]));
                            Pressure_z.Add(Convert.ToDouble(words[i+2*max_num]));
                            Pressure.Add(0);
                            Von_Mises.Add(0);
                            Count_VM_and_Hydro(i);
                        }
                        nUD_Diameter.Value = (decimal)(diameter*1000);
                        nUD_Thickness.Value = (decimal)(thickness*1000);
                        nUD_Depth.Value = cell_in_depth;
                        nUD_SquareNum.Value = sq_in_rad;
                        trackBar_Depth.Maximum = cell_in_depth - 1;
                        trackBar_Depth.Value = cell_in_depth / 2;
                        trackBar_Depth_Scroll(sender,e);
                    }
                    myStream.Close();
                }
            }


        }


        //Задание силы/нагрузки пользователем
        private void rB_App_Press_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_App_Press.Checked == true)  nUD_App_Press.Enabled = true;
            else nUD_App_Press.Enabled = false;
        }
        private void rB_Force_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_Force.Checked == true) nUD_Force.Enabled = true;
            else nUD_Force.Enabled = false;
        }
        private void nUD_App_Press_ValueChanged(object sender, EventArgs e)
        {
            app_press = (double)nUD_App_Press.Value;
            Count_cell_size();
            force = app_press * cell_size * cell_size;
            force_sum = force * iter_num * cell_in_depth;
            Update_Force_Panel();
        }
        private void nUD_Force_ValueChanged(object sender, EventArgs e)
        {
            force = (double)nUD_Force.Value;
            Count_cell_size();
            app_press = force / (cell_size * cell_size);
            force_sum = force * iter_num * cell_in_depth;
            Update_Force_Panel();
        }
        private void Update_Force_Panel()
        {
            tB_App_Press_per_iter.Text = "Нагрузка, прикладываемая за одну итерацию:" + Environment.NewLine + Round(app_press * cell_in_depth, 3);
            tB_App_Press_Sum.Text = "Итого:" + Environment.NewLine + Round(app_press * cell_in_depth * iter_num,3);
            tB_Force_per_iter.Text = "Сила, прикладываемая за одну итерацию:" + Environment.NewLine + Round(force * cell_in_depth,3);
            tB_Force_Sum.Text = "Итого:" + Environment.NewLine + Round(force_sum,3);
        }


        // Настройка таблицы с информацией о материалах
        private void Materials_Add_Col()
        {
            tLP_Materials.ColumnCount++;
            Label lab = new Label();
            lab.Text = $"{tLP_Materials.ColumnCount}";
            lab.Font = new Font(lab.Font.FontFamily, 10);
            lab.Dock = DockStyle.Left;
            lab.TextAlign = ContentAlignment.MiddleLeft;
            tLP_Materials.Controls.Add(lab, tLP_Materials.ColumnCount, 0);

            NumericUpDown num = new NumericUpDown();
            num.Maximum = 500;
            num.Minimum = 0.001M;
            num.DecimalPlaces = 0;
            num.Value = 50;
            num.Increment = 1M;
            tLP_Materials.Controls.Add(num, tLP_Materials.ColumnCount, 1);
            num.Dock = DockStyle.Left;

            num = new NumericUpDown();
            num.Maximum = 1;
            num.Minimum = 0.001M;
            num.DecimalPlaces = 3;
            num.Value = 0.24M;
            num.Increment = 0.001M;
            tLP_Materials.Controls.Add(num, tLP_Materials.ColumnCount, 2);
            num.Dock = DockStyle.Left;

            num = new NumericUpDown();
            num.Maximum = 500;
            num.Minimum = 0.001M;
            num.DecimalPlaces = 0;
            num.Value = 50;
            num.Increment = 1M;
            tLP_Materials.Controls.Add(num, tLP_Materials.ColumnCount, 3);
            num.Dock = DockStyle.Left;

        }
        private void Materials_Delete_Col()
        {
            for (int i = 0; i < 4; i++)
                tLP_Materials.Controls.RemoveAt(tLP_Materials.Controls.Count - 1);
            tLP_Materials.ColumnCount--;
        }
        private void nUD_MaterialsNum_ValueChanged(object sender, EventArgs e)
        {
            while (nUD_MaterialsNum.Value > tLP_Materials.ColumnCount)
                Materials_Add_Col();
            while (nUD_MaterialsNum.Value < tLP_Materials.ColumnCount)
                Materials_Delete_Col();
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