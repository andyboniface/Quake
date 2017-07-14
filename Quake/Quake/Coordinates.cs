using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake
{
    public class Coordinates
    {
        public Coordinates()
        {
        }

        /*
		 * This will return the distance between two coordinates in meters. The multiplier 6378137 is the radius of equator (6378.137km).
		 * 
		*/
        public static double DistanceBetween(Coordinates a, Coordinates b)
        {
            double d = Math.Acos(
                (Math.Sin(ToRadians(a.Latitude)) * Math.Sin(ToRadians(b.Latitude))) +
                (Math.Cos(ToRadians(a.Latitude)) * Math.Cos(ToRadians(b.Latitude)))
                * Math.Cos(ToRadians(b.Longitude) - ToRadians(a.Longitude)));

            return 6378137 * d;
        }

        public static double ToRadians(double val)
        {
            return (Math.PI / 180) * val;
        }

        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Depth { get; set; }

    }
}
