using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klitech
{
    /*
     * Arcok attribútumait eltároló osztály
     */

    public class FaceAttributes
    {
        public string Gender { get; set; }
        public double Age { get; set; }
        public string Glasses { get; set; }
        public FacialHair FacialHair { get; set; }
        public Hair Hair { get; set; }

        private Dictionary<string, double> _emotion;
        public Dictionary<string, double> Emotion
        {
            get
            {
                return _emotion;
            }
            set //a setterben kiválasztjuk releváns érzelemnek azt amit a rendszer a legerősebbnek gondolt
            {
                if (value != null && value.Count > 0)
                {
                    var top = value.OrderByDescending(pair => pair.Value).Take(1).First();
                    _emotion = new Dictionary<string, double>();
                    _emotion.Add(top.Key, top.Value);
                }
            }
        }
    }
}
