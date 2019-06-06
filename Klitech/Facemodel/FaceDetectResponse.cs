using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klitech
{
    /*
     * Ebben az osztályban vannak eltárolva egy adott archoz tartozó adatok
     */

    public class FaceDetectResponse
    {
        public string FaceId { get; set; }
        public FaceAttributes FaceAttributes { get; set; }
        public FaceRectangle FaceRectangle { get; set; }
        public string FaceUrl { get; set; }
    }
}
