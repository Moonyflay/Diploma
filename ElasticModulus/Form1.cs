using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElasticModulus
{
    public partial class FormMain : Form
    {
        int cell_in_row = 200;
        int cell_in_col = 200;
        byte cell_size_pix = 10;
        double cell_size = 2;
        double[] vol_frac = new double [3];
        public  static Random sluchai = new Random();
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
           
            vol_frac[0] = 0.20;
            vol_frac[1] = 0.05;
            vol_frac[2] = 1 - vol_frac[0]- vol_frac[1];
            double mind = 5;
            double maxd = 30;
            double mx = 25;
            double sigma = 5;

            Map map = new Map(vol_frac, cell_in_row, cell_in_col, mind, maxd, mx, sigma, cell_size);
            Bitmap bmp;
            bmp = new Bitmap(cell_size_pix * cell_in_row, cell_in_col * cell_size_pix, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle rect;
                
                for(int k = 0; k < map.Component.Length;k++)
                for (int i = 0; i < map.Component[k].Count; i++)
                {
                        int x = 0; int y = 0;
                        map.Num_Decrypt(map.Component[k][i], ref x, ref y);
                        rect = new Rectangle(x* cell_size_pix+1, y* cell_size_pix+1, cell_size_pix-2, cell_size_pix-2);
                        switch (k)
                            {
                            case 0: { g.FillRectangle(new SolidBrush(Color.LightSkyBlue), rect); break; }
                            case 1: { g.FillRectangle(new SolidBrush(Color.BurlyWood), rect); break; }
                            default: { g.FillRectangle(new SolidBrush(Color.Peru), rect); break;}  

                        }
                        //if (k == 0)
                        //g.FillRectangle(new SolidBrush(Color.LightSkyBlue), rect);
                        //else g.FillRectangle(new SolidBrush(Color.Peru), rect);
                    }
            }
            pictureBoxMain.Image?.Dispose();
            pictureBoxMain.Image = bmp;

        }
      
    }
}
