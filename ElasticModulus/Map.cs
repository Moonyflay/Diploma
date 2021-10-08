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

        public Map(double[] _vol_frac, int _cell_in_row, int _cell_in_col, double _cell_size, bool def = true)
            
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

            if (def == true) { Structure_Defenition(FormMain.sluchai); Short_Path(); }

            }
        public Map(double[] _vol_frac, int _cell_in_row, int _cell_in_col, double _mind, double _maxd, double _mx, double _sigma, double _cell_size) :  this(_vol_frac, _cell_in_row, _cell_in_col, _cell_size, false)
        {
            porous = true;
            mind = _mind;
            maxd = _maxd;
            mx = _mx;
            sigma = _sigma;
            Structure_Defenition(FormMain.sluchai);
            Short_Path();
        }

        void Structure_Defenition(Random sluchai)
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
        int Num_Crypt(int x, int y)  // переводит координаты в номер клетки (пока в 2D варианте)
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
    }
}
