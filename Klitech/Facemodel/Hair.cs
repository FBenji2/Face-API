using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klitech
{
    /*
     * Haj osztály, eltárolja, hogy mekkora valószínűséggel kopasz az illető és a haja színét
     */

    public class Hair
    {
        public double Bald { get; set; }

        private List<HairColor> _hairColor;
        [JsonProperty("hairColor", NullValueHandling = NullValueHandling.Ignore)]
        public List<HairColor> HairColor
        {
            set //a hajszínek közül kiválasztjuk azt amelyikre a legnagyobb esély van a rendszer szerint
            {
                if (value != null && value.Count > 0)
                {
                    var top = value.OrderByDescending(entry => entry.Confidence).Take(1).First();
                    _hairColor = new List<HairColor>();
                    _hairColor.Add(top);
                }
            }
        }

        public string Color
        {
            get
            {
                return _hairColor?.First().Color;
            }
        }
    }
}
