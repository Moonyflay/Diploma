using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticModulus
{
    class Material
    {
        public double p_ratio; // коэффициент Пуассона
        public double elastic_m; // модуль упругости
        public double compr_strength; // прочность на сжатие
        public Material(double _p_ratio, double _elastic_m, double _compr_strength)
        {

            p_ratio = _p_ratio;
            elastic_m = _elastic_m;
            compr_strength = _compr_strength;
        }

    }
}
