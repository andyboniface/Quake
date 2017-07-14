using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake
{
    public class Quake
    {
        //private static System.Drawing.Color[] QuakeColours = { System.Drawing.Color.LightSlateGray, System.Drawing.Color.Blue, System.Drawing.Color.Cyan, System.Drawing.Color.Green, System.Drawing.Color.Yellow, System.Drawing.Color.Red };
        private static string[] QuakeClassNames = { "Minor", "Light", "Moderate", "Strong", "Major", "Great" };

        public Quake()
        {
        }

        public Quake(String jsonStr)
        {
            JObject json = JObject.Parse(jsonStr);
            Mag = json["mag"].Value<Double>();
            Time = json["time"].Value<long>();
            Tz = json["tz"].Value<long>();
            Place = json["place"].Value<string>();
            Title = json["title"].Value<string>();
            Tsunami = json["tsunami"].Value<int>();
            Id = json["id"].Value<string>();
            DistanceAway = json["away"].Value<double>();
            Coordinates = new Coordinates();
            Coordinates.Depth = json["depth"].Value<double>();
            Coordinates.Latitude = json["latitude"].Value<double>();
            Coordinates.Longitude = json["longitude"].Value<double>();
        }

        public string toJson()
        {
            JObject root = new JObject();
            root.Add("mag", Mag);
            root.Add("time", Time);
            root.Add("tz", Tz);
            root.Add("place", Place);
            root.Add("title", Title);
            root.Add("tsunami", Tsunami);
            root.Add("id", Id);
            root.Add("away", DistanceAway);
            root.Add("depth", Coordinates.Depth);
            root.Add("latitude", Coordinates.Latitude);
            root.Add("longitude", Coordinates.Longitude);

            return root.ToString();
        }

        public double Mag { get; set; }
        public long Time { get; set; }
        public long Tz { get; set; }
        public string Place { get; set; }
        public string Title { get; set; }
        public int Tsunami { get; set; }
        public string Id { get; set; }
        public Coordinates Coordinates { get; set; }
        public double DistanceAway { get; set; }

        public int getQuakeClass()
        {
            if (Mag >= 8)
            {                   // Great
                return 5;
            }
            else if (Mag >= 7)
            {           // Major
                return 4;
            }
            else if (Mag >= 6)
            {           // Strong
                return 3;
            }
            else if (Mag >= 5)
            {           // Moderate
                return 2;
            }
            else if (Mag >= 4)
            {           // Light
                return 1;
            }
            // Everything else is Minor
            return 0;
        }

        public DateTime getUTCTime()
        {
            return DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(Time / 1000)),    // We need time in seconds.
                DateTimeKind.Utc);
        }

        public DateTime getLocalTime()
        {
            return DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(Time / 1000)).AddMinutes(Tz), // We need time in seconds.
                DateTimeKind.Local);
        }


        public string getQuakeClassAsString()
        {
            return QuakeClassNames[getQuakeClass()];
        }

        /*
        public System.Drawing.Color getQuakeColor()
        {
            return QuakeColours[getQuakeClass()];
        }
        */

        public override string ToString()
        {
            return Title + " is " + (DistanceAway / 1000) + "km";
        }

    }
}
