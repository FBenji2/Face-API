using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klitech
{
    /*
     * Ebben az osztályban találhatók azok az adatok, amelyekből kinyerhető, hogy a rendszer szerint mekkora valószínűséggel
     * Vannak bizonyos arcszőrzetek az adott személyen
     */

    public class FacialHair
    {
        public double Moustache { get; set; }
        public double Beard { get; set; }
        public double Sideburns { get; set; }
    }
}
