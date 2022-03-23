using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace ElasticModulus
{
    class Map
    {
        int cell_in_row;
        int cell_in_col;
        int cell_num;
        double cell_size;

        bool porous = false;
        double maxd;
        double mind;
        double mx; // Мат. ожидание диаметра
        double sigma ; //ср. кв. отклонение

        public List<List<int>> Path = new List<List<int>>();
        public List<int> [] Component;        
        // Хранит в себе номера клеток, содержащих тот или иной компонент    
        // [0] - Нет вещества (пора) при porous == true                                                   // сделать проверку vmax поры  > V пор => ош
        double[] vol_frac; // Объемные доли компонентов 

        int sq_in_rad;

        public Map(int _sq_in_rad, double[] _vol_frac, int _cell_in_row, int _cell_in_col, double _cell_size, bool def = true)
            
        {        
            int comp_num = _vol_frac.Length;
            Component = new List<int>[comp_num];
            for (int i = 0; i < Component.Length; i++)
                Component[i] = new List<int>();
            vol_frac = new double[comp_num ];
            for (int i = 0; i < comp_num; i++)
            {
                vol_frac[i] = _vol_frac[i];
            }
            cell_in_row = _cell_in_row;
            cell_in_col = _cell_in_col;
            cell_size = _cell_size;
            cell_num = cell_in_col * cell_in_row;

            sq_in_rad = _sq_in_rad;

            if (def == true) { Round_Structure_Definition(FormMain.sluchai); /*Middle_Path(FormMain.sluchai);*/ }

            }
        public Map(int _sq_in_rad, double[] _vol_frac, int _cell_in_row, int _cell_in_col, double _mind, double _maxd, double _mx, double _sigma, double _cell_size) :  this(_sq_in_rad, _vol_frac, _cell_in_row, _cell_in_col, _cell_size, false)
        {
            porous = true;
            mind = _mind;
            maxd = _maxd;
            mx = _mx;
            sigma = _sigma;
            Round_Structure_Definition(FormMain.sluchai);
            
            //Middle_Path(FormMain.sluchai);
        }

        void Round_Structure_Definition(Random sluchai)
        {
            double k;
            double diam;
            int a = 2;
            
            for (int i = 0; i < cell_num; i++) // Выделение клеток за окружностью
            {
                int x = 0; int y = 0;
                Num_Decrypt(i, ref x, ref y);
                if ((x - sq_in_rad) * (x - sq_in_rad) + (y - sq_in_rad) * (y - sq_in_rad) > sq_in_rad * sq_in_rad) { Component[0].Add(i); }
            }

            //for (int i = 0; i < cell_num; i++)
            //{
            //    int x = 0; int y = 0;
            //    Num_Decrypt(i, ref x, ref y);
            //    if ((x - sq_in_rad) * (x - sq_in_rad) + (y - sq_in_rad) * (y - sq_in_rad) > sq_in_rad * sq_in_rad) continue;


            //        k = sluchai.NextDouble();
            //        double vol;
            //        vol = vol_frac[0] + vol_frac[1];

            //        byte type = (byte)(a - 1);
            //        for (byte j = (byte)a; j < vol_frac.Length; j++)
            //        {
            //            if (k >= vol) { vol += vol_frac[j]; type = j; }
            //            else break;
            //        }
            //        Component[type].Add(i);

            //}
            if (vol_frac[0]>0)
            {
                int pore_cell_num = (int)(vol_frac[0] * sq_in_rad * sq_in_rad);
                while (pore_cell_num > PI * Pow(mind / cell_size, 2)) // пока что используем площадь = PI * Pow(mind / cell_size,2)
                {
                    do
                    {
                        k = sluchai.NextDouble();
                        diam = mind + (maxd - mind) * k; // выбор случайного значения диаметра в интервале
                        k = sluchai.NextDouble();
                    }
                    while (k < Gauss_Distribution(diam, mx, sigma));
                    int k2 = sluchai.Next(0, cell_num);
                    if (Component[0].Contains(k2) == true ) continue; // не дает центру поры генерироваться в другой поре или вне круга
                    pore_cell_num -= Circle(k2, diam);

                }
            }
            for (int i = 0; i < cell_num; i++)
            {
                if (Component[0].Contains(i) == true) continue; // можно ли упростить?

                k = sluchai.NextDouble();

                double vol;
                vol = vol_frac[0] + vol_frac[1];

                byte type = (byte)(a - 1);
                for (byte j = (byte)a; j < vol_frac.Length; j++)
                {
                    if (k >= vol) { vol += vol_frac[j]; type = j; }
                    else break;
                }
                // x - случайная величина
                // Пусть имеется 2 вещества. V0 - объемная доля пор, V1 и V2 - веществ
                // При этом V0 + V2 + V3 = 1
                // В зависимости от того, какое значение примет х, выбирается состав клетки
                // 0      <= x <= V0 - пора
                // V0      < x <= V0 + V1 - первое вещество 
                // V0 + V1 < x <= V0 + V1 + V2 -второе вещество

                Component[type].Add(i);

            }




        }

        void Structure_Definition(Random sluchai)
        {
            double k;
            
            double diam;
            
            int a = 1;
            if (porous == true)
            {
                a++;
                int pore_cell_num = (int)(vol_frac[0] * cell_in_col * cell_in_row);
                while (pore_cell_num > PI * Pow(mind / cell_size,2)) // пока что используем площадь = PI * Pow(mind / cell_size,2)
                {
                    do
                    {
                        k = sluchai.NextDouble();
                        diam = mind + (maxd - mind) * k; // выбор случайного значения диаметра в интервале
                        k = sluchai.NextDouble();
                    }
                    while (k < Gauss_Distribution(diam, mx, sigma)); 
                    int k2 = sluchai.Next(0, cell_num);
                    if (Component[0].Contains(k2) == true) continue; // не дает центру поры генерироваться в другой поре
                    pore_cell_num-= Circle(k2,diam);

                }
            }
            for (int i = 0; i < cell_num; i++)
            {
                if (porous == true && Component[0].Contains(i) == true) continue; // можно ли упростить?
                
                k = sluchai.NextDouble();
               
                double vol;
                if (porous == false) // можно сделать красивее?
                    vol = vol_frac[0];
                else vol = vol_frac[0] + vol_frac[1];

                byte type = (byte)(a-1);
                for (byte j = (byte)a; j < vol_frac.Length; j++) 
                {
                    if (k >= vol) { vol += vol_frac[j]; type = j; }
                    else break;
                }
                // x - случайная величина
                // Пусть имеется 2 вещества. V0 - объемная доля пор, V1 и V2 - веществ
                // При этом V0 + V2 + V3 = 1
                // В зависимости от того, какое значение примет х, выбирается состав клетки
                // 0      <= x <= V0 - пора
                // V0      < x <= V0 + V1 - первое вещество 
                // V0 + V1 < x <= V0 + V1 + V2 -второе вещество

                Component[type].Add(i); 
            
            }
        }

        double Gauss_Distribution(double x, double mx, double sigma)
        {
            double probability;
            probability = 1 / (sigma * Sqrt(2 * PI)) * Exp(-0.5 * (x - mx) / sigma * (x - mx) / sigma);
            return probability;
        }
        int Circle(int num, double diam) // упростить, исп симметрию
        {
            int x = 0; int y = 0; int filled_cells = 0; byte a = 0;
            int rad_in_cells = (int)(0.5 * diam / cell_size);
            while (Sqrt(2 * rad_in_cells * rad_in_cells) <= 0.5 * diam / cell_size) 
                rad_in_cells++;
            if (rad_in_cells < 8 && rad_in_cells % 2 == 1) a = 1;
            Num_Decrypt(num, ref x,ref y);
            for (int i = -rad_in_cells-a; i <= rad_in_cells; i++)
                for (int j = -rad_in_cells-a; j <= rad_in_cells; j++)
                {
                    if (x + i < 0 || y + j < 0 || x + i >= cell_in_row || y + j >= cell_in_col) continue;
                    
                    if (Sqrt(i * i + j * j) <= diam*0.5 / cell_size) 
                    {
                        if (Component[0].Contains(Num_Crypt(x + i, y + j)) == true) continue;
                        Component[0].Add(Num_Crypt(x + i, y + j));
                        filled_cells++;                    
                    }
                }
            return filled_cells;        
        }
        public void Num_Decrypt (int num, ref int x, ref int y) // переводит номер клетки в координаты (пока в 2D варианте)
        {
            y = num / cell_in_row;
            x = (num - ( num / cell_in_row)*cell_in_row);
            // Запись (num/cell_in_row)*cell_in_row имеет смысл, тк при делении целых чисел отбрасывается дробная часть. Например:
            // (13/2)*2 = (6)*2 = 12
        }
        public int Num_Crypt(int x, int y)  // переводит координаты в номер клетки (пока в 2D варианте)
        {
            if (y == 0) return x;
            if (x == 0) return y * cell_in_row;
            return y * cell_in_row + x;
        }
        public void Short_Path() //2D 
        {
            if (porous == false) return;
            for (int i = 0; i < cell_in_row; i++)
            {
                if (Component[0].Contains(i)) continue;
                //List<int> path = new List<int>();
                //path.Add(i);
                Path.Add(new List<int>() {i}); 
            }
            for (int i = 0; i < Path.Count; i++)
            {
                List<int> path = Path[i];
                    Go_Down(ref path);


                    //if (Component[0].Contains(index) == false) { Path[i].Add(index); } //Если под клеткой нет поры, воздействие направляется туда
                    //                                                                   // если есть, то проверяется, можно ли обойти пору без подъема вверх. Если можно, то пора обходится. Нельзя - конец пути
                    //else
                    //{
                    //    bool left = index - 1 < 0 || (index - 1) % 3 != 0 || Component[0].Contains(index - 1);
                    //    bool right = index + 1 >= cell_num || (index + 1) % 3 != 0 || Component[0].Contains(index + 1);

                    //    if (left && right == true) { break; }
                    //    if (left == true) { Path[i].Add(index + i); continue; }
                    //    if (right == true) { Path[i].Add(index - i); continue; }

                    //    List<int> path_l = new List<int>();
                    //    List<int> path_r = new List<int>();


                    
            }

            
            void Go_Down(ref List<int> rout, bool from_left = false, bool from_right = false) 
            {
                int index = rout[rout.Count - 1];
                if (index + cell_in_row >= cell_num) { return; }; // условие проверяет, достигли ли мы низа
                if (Component[0].Contains(index + cell_in_row) == false) { rout.Add(index + cell_in_row); Go_Down(ref rout);return; } // ?
                
                //Если под клеткой нет поры, воздействие направляется туда
                // если есть, то проверяется, можно ли обойти пору без подъема вверх. Если можно, то пора обходится. 
                else
                {
                    bool no_left = index - 1 < 0 || (index - 1) % cell_in_row == 0 || Component[0].Contains(index - 1);
                    bool no_right = index + 1 >= cell_num || (index + 1) % cell_in_row == 0 || Component[0].Contains(index + 1);

                    if ((no_left && no_right == true) || (no_left && from_right == true) || (no_right && from_left == true)) { return; } //Нельзя обойти - конец пути
                    // no_left && from_right == true - Клетку нельзя обойти слева и предыдущая клетка была справа - Условие нужно, чтобы направление не металось туда-сюда

                    byte k = (byte)(Convert.ToByte(no_left) + Convert.ToByte(no_right)*2);
                    // 1 - no_left = true  и no_right != true
                    // 2 - no_left != true и no_right = true
                    // 0 - no_left != true  и no_right != true
                    switch (k)
                    {
                        case 1: { rout.Add(index + 1); Go_Down(ref rout, true); break; } //Можно обойти только справа - путь продолжается справа
                        case 2: { rout.Add(index - 1); Go_Down(ref rout, false, true); break;}
                        default:
                            { // Можно обойти с обеих сторон - будут проверяться оба пути
                                List<int> path_l = new List<int>();
                                List<int> path_r = new List<int>();
                                if (from_left == false) { path_l.AddRange(rout); path_l.Add(index - 1); Go_Down(ref path_l, false, true); }
                                if (from_right == false) { path_r.AddRange(rout); path_r.Add(index + 1); Go_Down(ref path_r, true); }
                                // Добавить удаление лишних клеток

                                // Выбор одного из двух путей
                                int x = 0; int y1 = 0 ; int y2 = 0;
                                if(path_l.Count!=0) Num_Decrypt(path_l[path_l.Count - 1], ref x, ref y1);
                                if (path_r.Count != 0) Num_Decrypt(path_r[path_r.Count - 1], ref x, ref y2);
                                if (y2 == y1)
                                { if (path_l.Count > path_r.Count) rout.AddRange(path_r); else rout.AddRange(path_l); }
                                else {if (y2 > y1) rout.AddRange(path_r); else rout.AddRange(path_l); }
                                path_l.Clear();
                                path_r.Clear();
                                break; 
                            }
                    }
                    
                    //if (no_left == true) { rout.Add(index + 1); Go_Down(rout);}  
                    //if (no_right == true) { rout.Add(index - 1); Go_Down(rout);}
                    
                    
                }
            }

        }

        void Middle_Path(Random sluchai)
        {
            if (porous == false) return;
            int last = 0;
            bool last_pore = true;
            for (int i = 0; i < cell_in_row; i++)
            {
                if (Component[0].Contains(i) || i + 1 == cell_in_row)  // строгое или?
                {
                    if (last_pore == true) { last = i; continue; }
                    else
                    {
                        last_pore = true;
                        Path.Add(new List<int>() { (last + i) / 2 });
                        last = i;

                    }
                }
                else last_pore = false;
            }
            for (int i = 0; i < Path.Count; i++)
            {
                
                List<int> path = Path[i];
                //if(i == 2)
                if (Wall_Check(path[0]) == true) //
                {
                    // if (Component[0].Contains(Path[i].Last()+cell_in_row) == false) Path[i].Add(Path[i].Last() + cell_in_row); 
                    // Go_Down(ref path);
                    Go_Down_xy(ref path);
                }

            }


            void Go_Down(ref List<int> path)
            {
                while (path[path.Count - 1] + cell_in_row < cell_num)
                {
                    int index = path.Last();
                    if (Component[0].Contains(index + cell_in_row)) { Go_Side(ref path); return; } //изменить после базы
                    else
                    {

                        //int right_wall = index + cell_in_row; bool right = false;
                        //int left_wall = index + cell_in_row; bool left = false;
                        //int k = 1;
                        //while (right == false || left == false)
                        //{
                        //    int _base = index + cell_in_row;
                        //    if (right == false &&
                        //        ((_base - k - 1) / cell_in_row < _base / cell_in_row || Component[0].Contains(_base - k)))
                        //    { right_wall = _base - k; right = true; }
                        //    if (left == false &&
                        //        ((_base + k + 1) / cell_in_row > _base / cell_in_row || Component[0].Contains(_base + k)))
                        //    { left_wall = _base + k; left = true; }
                        //    k++;
                        //}
                        //int next = (right_wall + left_wall) / 2;
                       
                        
                        path.Add(Wall_Search(index));

                    }
                }
                
            }
            bool Wall_Check(int index /*, ref List<int> path*/) // Жук. Проверяет, разрмкнута ли полость.
            {

                int[] xy = new int[2] { 0, 0 };
                
                int [] xys = new int[2];
                Num_Decrypt(index, ref xy[0], ref xy[1]);
                xys[0] = xy[0]; xys[1] = xy[1];
                byte a = 0;
                sbyte sign = -1; 
                bool clockwise = false;
                do 
                {
                    if (xy[1] >= cell_in_col) return true; // Разомкнута
                    xy[a] += sign;
                    

                    if (xy[a] < 0 || xy[1] >= cell_in_col || xy[0] >= cell_in_row || Component[0].Contains(Num_Crypt(xy[0], xy[1])) == true)
                        clockwise = false;
                    else clockwise = true;
                    //if ((xy[a] < 0 || xy[1] >= cell_in_col || xy[0] >= cell_in_row) == false) path.Add(Num_Crypt(xy[0], xy[1])) ;
                    if (a == 1) a = 0; else a = 1;
                    if ((clockwise == true && a == 0) || (clockwise == false && a == 1)) sign *= -1;
                    
                }
                while ( xys[0] != xy[0] || xys[1] != xy[1]);

                return false; // Замкнута

            }

            int Wall_Search(int index) 
            {
                int right_wall = index + cell_in_row; bool right = false;
                int left_wall = index + cell_in_row; bool left = false;
                int k = 1;
                while (right == false || left == false)
                {
                    int _base = index + cell_in_row;
                    if (right == false &&
                        ((_base - k - 1) / cell_in_row < _base / cell_in_row || Component[0].Contains(_base - k)))
                    { right_wall = _base - k; right = true; }
                    if (left == false &&
                        ((_base + k + 1) / cell_in_row > _base / cell_in_row || Component[0].Contains(_base + k)))
                    { left_wall = _base + k; left = true; }
                    k++;
                }
                int next = (right_wall + left_wall) / 2;
                return next;
            }

            void Go_Side(ref List<int> path) 
            {
                int index = path[path.Count - 1];

                List<int> path_l = new List<int>(); 
                List<int> path_r = new List<int>(); //

                int right_wall = -1; bool right = false;
                int left_wall = -1; bool left = false;
                int k = 1;
                while (right == false || left == false)
                {
                    int _base = index + cell_in_row;

                    if (right == false)
                    {
                        if ((_base - k - 1) / cell_in_row < _base / cell_in_row) right = true;
                        else if (Component[0].Contains(_base - k) == false && Component[0].Contains(Wall_Search(_base - k))== false)
                        { right_wall = Wall_Search(_base - k); right = true; } //
                    }

                    if (left == false)
                    {
                        if ((_base + k + 1) / cell_in_row > _base / cell_in_row) left = true;
                        else if (Component[0].Contains(_base + k) == false && Component[0].Contains(Wall_Search(_base +k)) == false)
                        { left_wall = Wall_Search(_base + k); left = true; } //
                    }                   
                    k++;
                }
                int x = 0; int y1 = 0; int y2 = 0;

                if (left_wall > 0) {  path_l.Add(left_wall); Go_Down(ref path_l); Num_Decrypt(path_l[path_l.Count - 1], ref x, ref y1); }
                if (right_wall > 0) { path_r.Add(right_wall); Go_Down(ref path_r); Num_Decrypt(path_r[path_r.Count - 1], ref x, ref y2); }


                // Выбор одного из двух путей
                if (y2 == y1)
                { if (path_l.Count > path_r.Count) path.AddRange(path_r); else path.AddRange(path_l); }// AddRange(path) n AddRange(path_)?
                else { if (y2 > y1) path.AddRange(path_r); else path.AddRange(path_l); }
                path_l.Clear();
                path_r.Clear();
            }

            void Go_Down_xy(ref List<int> path) 
            {
                int index_1 = path[0];
                double anti_repeat = 0.3;
                while (path.Last() < cell_num)
                {
                    //MARK:
                    int index_2 = path.Last();
                    int x_2 = 0; int y_2 = 0;
                    Num_Decrypt(index_2, ref x_2, ref y_2);
                    int x_1 = x_2; int y_1 = y_2; //
                    if (path.Count > 1) { /*index_1 = path[path.Count - 2];*/ Num_Decrypt(index_1, ref x_1, ref y_1); }
                    // 
                    sbyte dx = 1; sbyte dy = 1;
                    if (Component[0].Contains(Num_Crypt(x_2, y_2 + dy)) == true /*|| sluchai.NextDouble() > anti_repeat*/)
                    {
                        if (x_2 - x_1 == 0 || Abs((y_2 - y_1) / (x_2 - x_1)) >= Tan(3 * PI / 8)) dx = 0; //
                        else if (path.Count > 1 && Abs((y_2 - y_1) / (x_2 - x_1)) <= Tan(PI / 8)) dy = 0;//
                        if (x_2 - x_1 < 0 && x_2 > 0) { dx *= -1; }
                        if (y_2 - y_1 < 0 && y_2 > 0) { dy *= -1; }
                        //if (dy != 0 && y_2 > 0) dy = -1;
                    }
                    else { dx = 0;dy = 1; }
                    bool end = false;
                    bool start = true;
                    while (Component[0].Contains(Num_Crypt(x_2 + dx, y_2 + dy)) == true)
                    {

                        if (start == true)
                            for (sbyte j = 0; j > 2; j++)
                            {
                                if (end == true) break;
                                for (sbyte i = -1; i > 2; i++)
                                {
                                    if ((dx == i && dy == j) || (i == 0 && j == 0) || x_2 + i < 0 || y_2 + j < 0 || x_2 + i >= cell_in_row || y_2 + j >= cell_in_col) continue;
                                    if (Component[0].Contains(Num_Crypt(x_2 + i, y_2 + j)) == false) { dx = i; dy = j; end = true; break; }

                                }
                            }

                        if (end == false)
                        {
                            //return; 
                            // в большинстве случаев работает, но сильно засоряет 
                            //start = false;
                            if (path.Count > 1 && start == true)
                            {
                                double cut = 0.03;
                                Num_Decrypt(path[path.Count - 1 - (int)(path.Count * cut)], ref x_2, ref y_2);
                                path.RemoveRange(path.Count - 1 - (int)(path.Count * cut), (int)(path.Count * cut)+1);
                                index_1 = path[path.Count - 2];
                                //goto MARK;
                                start = false;
                                end = false;

                               // path.RemoveAt(path.Count - 1);
                                //if (!(Abs(x_1 - x_2) < 2 && Abs(y_1 - y_2) < 2))
                                //{
                                    
                                    
                                //    //if (x_2 - x_1 != 0) x_2 -= (x_2 - x_1) / Abs(x_2 - x_1);
                                //    //if (y_2 - y_1 != 0) y_2 -= (y_2 - y_1) / Abs(y_2 - y_1);
                                //}
                                //else return;
                                //x_2 = (x_2 + x_1) / 2;
                                //y_2 = (y_2 + y_1) / 2;
                                //else
                                //{
                                //   Num_Decrypt(path[path.Count - 2], ref x_2, ref y_2);
                                //    path.RemoveAt(path.Count - 1);
                                //}
                                // path.Add(Num_Crypt(x_2, y_2));


                            }
                            else return;
                        }

                    }

                        Wall_Search_xy(out int x_3, out int y_3, dx, dy, x_2, y_2);
                    if (Component[0].Contains(Num_Crypt(x_3, y_3)) == false)
                    {
                        index_1 = path.Last();
                        int dx_23 = 0; int dy_23 = 0;
                        while (Abs(x_3 - x_2 - dx_23) > 1 || Abs(y_3 - y_2 - dy_23) > 1) // для визуализации
                        {
                            if (Abs(x_3 - x_2 - dx_23) > 1)
                            {
                                if (x_3 - x_2 > 0) dx_23++; else dx_23--;
                            }
                            if (Abs(y_3 - y_2 - dy_23) > 1)
                            {
                                if (y_3 - y_2 > 0) dy_23++; else dy_23--;
                            }
                            path.Add(Num_Crypt(x_2 + dx_23, y_2 + dy_23));
                        }
                        path.Add(Num_Crypt(x_3, y_3));
                        
                    }
                    { 
                    //    if (Component[0].Contains(Num_Crypt(x_2 + dx, y_2 + dy)) == false)
                    //{
                    //    Wall_Search_xy(out int x_3, out int y_3, dx, dy, x_2, y_2);
                    //    if (Component[0].Contains(Num_Crypt(x_3, y_3)) == false)
                    //    {
                    //        //int dx_23 = 0; int dy_23 = 0;
                    //        //while (Abs(x_3 - x_2 - dx_23) > 1 || Abs(y_3 - y_2 - dy_23) > 1) // для визуализации
                    //        //{
                    //        //    if (Abs(x_3 - x_2 - dx_23) > 1)
                    //        //    {
                    //        //        if (x_3 - x_2 > 0) dx_23++; else dx_23--;
                    //        //    }
                    //        //    if (Abs(y_3 - y_2 - dy_23) > 1)
                    //        //    {
                    //        //        if (y_3 - y_2 > 0) dy_23++; else dy_23--;
                    //        //    }
                    //        //    path.Add(Num_Crypt(x_2 + dx_23, y_2 + dy_23));
                    //        //}
                    //        path.Add(Num_Crypt(x_3, y_3));

                    //    }

                    //    else return;
                    //}
                    //else
                    //{
                    //    if (path.Count > 1) 
                    //    {
                    //        if ((x_2 == x_1) == false)
                    //            x_2 -= (x_2 - x_1)/Abs(x_2-x_1);
                    //    }
                    //    else return;
                    //    //double k = sluchai.NextDouble();
                    //    //if (k > 1 / 3) dx = 0;
                    //    //if (k < 2 / 3) dy = 0;
                    //}
                }
                }

                void Wall_Search_xy(out int next_x, out int next_y, sbyte dx, sbyte dy, int x_2, int y_2) 
                {
                    sbyte dx_p = 1;sbyte dy_p = 1;
                    if (dx == 0) dy_p = 0;
                    if (dy == 0) dx_p = 0;
                    if (dx * dy > 0) dy_p *= -1;

                    int x_l = x_2 + dx; int y_l = y_2 + dy; bool right = false;
                    int x_r = x_2 + dx; int y_r = y_2 + dy; bool left = false;
                    
                    while (right == false || left == false)
                    {
                        // diag. ch
                        if (left == false ) 
                        {
                            if (x_l - dx_p < 0 || x_l - dx_p >= cell_in_row || y_l - dy_p < 0 || y_l - dy_p >= cell_in_col || Component[0].Contains(Num_Crypt(x_l - dx_p, y_l - dy_p)) == true) left = true;
                            else { x_l -= dx_p; y_l -= dy_p; }
                        }

                        if (right == false)
                        {
                            if (x_r + dx_p < 0 || x_r + dx_p >= cell_in_row || y_r + dy_p < 0 || y_r + dy_p >= cell_in_col || Component[0].Contains(Num_Crypt(x_r + dx_p, y_r + dy_p)) == true) right = true;
                            else { x_r += dx_p; y_r += dy_p; }
                        }

                    }
                    double k;
                    double mnoj;
                    byte sch  = 0;
                    do
                    {
                        if (Max(Abs(x_l - x_r), Abs(y_l - y_r)) < 5)
                        {
                            k = sluchai.NextDouble();
                            next_y = (int)(Abs(y_l - y_r) * k + Min(y_l, y_r));
                            k = sluchai.NextDouble();
                            next_x = (int)(Abs(x_l - x_r) * k + Min(x_l, x_r));
                        }
                        else
                        {

                            do
                            {
                                k = sluchai.NextDouble();
                            }
                            while (sluchai.NextDouble() > Prob(k, 0.3, 0.7));

                            //mnoj = k * (0.90 - 0.40) + 0.40;
                            next_y = (int)(Abs(y_l - y_r) * k + Min(y_l, y_r)); //

                            do
                            {
                                k = sluchai.NextDouble();
                            }
                            while (sluchai.NextDouble() > Prob(k, 0.3, 0.7));
                            //mnoj = k * (0.70 - 0.30) + 0.30;
                            next_x = (int)(Abs(x_l - x_r) * k + Min(x_l, x_r));
                            
                        }
                        sch++;
                    }
                    while (sch < 250 && Component[0].Contains(Num_Crypt(next_x, next_y)) == true);
                   

                }
                double Prob(double x, double min, double max)
                {
                    double p;
                    bool right = x > min;
                    bool left = x < max;
                    if (left == true && right == true) {p = 1; return p; }
                    if (left == true) { p = x / min; return p; }
                    p = (1 - x) / (1 - max);
                    return p;
                }

            }
        }
    }
}
