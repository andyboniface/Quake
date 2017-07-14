
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace Quake
{
    public class QuakeDataSource
    {
        private IQuakeDataSourceClient myInterface;
        private IDictionary<string, Quake> quakes = new Dictionary<string, Quake>();
        private bool checkForNew = false;                        // Don't check for new on the first pass

        //private System.Timers.Timer aTimer;

        public QuakeDataSource(IQuakeDataSourceClient myInterface)
        {
            this.myInterface = myInterface;

            TimeScheduler.GetTimeScheduler().AddTask(this.GetType().FullName, TimeSpan.FromSeconds(1), () => OnTimedEvent());

            /*
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;             // Fetch in a second on the first pass
            aTimer.Enabled = true;
            */
        }

        public int Count
        {
            get { return quakes.Count; }
        }

        public Quake getQuake(int position)
        {
            switch (myInterface.getConfiguration().SortOrder)
            {
                case QuakeSortOrder.ByMagnitude:
                    return quakes.Select(value => value.Value).OrderByDescending(quake => quake.Mag).ToList()[position];
                case QuakeSortOrder.ByTime:
                    return quakes.Select(value => value.Value).OrderByDescending(quake => quake.Time).ToList()[position];
                case QuakeSortOrder.Nearest:
                default:
                    return quakes.Select(value => value.Value).OrderBy(quake => quake.DistanceAway).ToList()[position];
            }
        }

        private async Task<TimeSpan> OnTimedEvent()
        {
            try
            {
                Task<string> httpContent = getContent(checkForNew);

                checkForNew = true;                             // We want to just check for new ones from now on...
                var contents = await httpContent;
            }
            catch (Exception ex)
            {
                myInterface.QuakeDataSourceDisplayMessage("error " + ex.ToString());
            }
            return TimeSpan.FromSeconds(60 * 5);                // Try again in 5 minutes.
        }

        private async Task<string> getContent(bool checkForNew)
        {
            var httpClient = new HttpClient();

            try
            {
                //
                // Only get quakes of 2.5 or above.....
                //
                Task<string> contentsTask = httpClient.GetStringAsync("http://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/2.5_day.geojson");

                string contents = await contentsTask;

                Coordinates myLocation = myInterface.MyLocation();

                JObject json = JObject.Parse(contents);
                JArray features = (JArray) json["features"];

                bool foundNewQuakes = false;

                foreach (var quakeInfo in features)
                {
                    Quake quake = new Quake();

                    var props = quakeInfo["properties"];
                    var geo = quakeInfo["geometry"];
                    var coords = geo["coordinates"];

                    
                    if (props["mag"] != null)
                    {
                        quake.Mag = props["mag"].Value<Double>();
                    }
                    else
                    {
                        quake.Mag = 0;
                    }
                    if (props["time"] != null)
                    {
                        quake.Time = props["time"].Value<long>();
                    }
                    else
                    {
                        quake.Time = 0;
                    }
                    if (props["tz"]!= null)
                    {
                        quake.Tz = props["tz"].Value<long>();
                    }
                    else
                    {
                        quake.Tz = 0;
                    }
                    quake.Place = props["place"].Value<String>();
                    quake.Title = props["title"].Value<String>();
                    if (props["tsumami"] != null)
                    {
                        quake.Tsunami = props["tsunami"].Value<int>();
                    }
                    else
                    {
                        quake.Tsunami = 0;
                    }

                    Coordinates quakeCoords = new Coordinates();
                    quakeCoords.Longitude = coords[0].Value<Double>();
                    quakeCoords.Latitude = coords[1].Value<Double>();
                    quakeCoords.Depth = coords[2].Value<Double>();
                    quake.Coordinates = quakeCoords;
                    quake.Id = quakeInfo["id"].Value<String>();

                    quake.DistanceAway = Coordinates.DistanceBetween(quakeCoords, myLocation);

                    //
                    // Remember this quake
                    //
                    if (quakes.ContainsKey(quake.Id))
                    {
                        quakes.Remove(quake.Id);                // We are replacing this value.
                        quakes.Add(quake.Id, quake);
                    }
                    else
                    {
                        quakes.Add(quake.Id, quake);            // Its a new one...
                        if (checkForNew)
                        {
                            foundNewQuakes = true;
                            myInterface.QuakeDataSourceNewQuake(quake);
                        }
                    }
                }

                if (checkForNew)
                {
                    if (foundNewQuakes)
                    {
                        // Only tell the world if there is new one...
                        myInterface.QuakeDataSourceChanged();
                    }
                }
                else
                {
                    myInterface.QuakeDataSourceChanged();
                }
                return contents;
            }
            catch (Exception e)
            {
                myInterface.QuakeDataSourceDisplayMessage("error " + e);
                return null;
            }
        }

    }
}
