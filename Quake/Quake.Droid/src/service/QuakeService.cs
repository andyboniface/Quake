using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

namespace Quake.Droid.src.service
{
    [Service(Exported = true, Name = "uk.co.fixsolutions.QuakeService") ]
    [IntentFilter(new String[] { "uk.co.fixsolutions.QuakeService" })]
    public class QuakeService : Service, IQuakeDataSourceClient
    {
        LocationManager locMgr;
        QuakeDataSource dataSource;
        QuakeConfig config = null;
        Handler myHandler;
        public const string QuakesUpdatedAction = "QuakesUpdated";
        public const string NewQuakeDetectedAction = "NewQuakeDetected";
        private const string PREFS_NAME = "QuakePrefs";

        public QuakeService()
        {
        }

        //
        // This is called when our service is first created....just do simple initialise here
        //
        public override void OnCreate()
        {
            base.OnCreate();

            myHandler = new Handler();
            locMgr = GetSystemService(Context.LocationService) as LocationManager;
            config = getConfiguration();

            dataSource = new QuakeDataSource(this);
        }

        public void QuakeDataSourceNewQuake(Quake quake)
        {
            var quakeIntent = new Intent(NewQuakeDetectedAction);
            quakeIntent.PutExtra("quake", quake.toJson());

            SendOrderedBroadcast(quakeIntent, null);
        }

        public void QuakeDataSourceChanged()
        {
            var quakeIntent = new Intent(QuakesUpdatedAction);

            SendOrderedBroadcast(quakeIntent, null);

        }

        public void QuakeDataSourceDisplayMessage(string msg)
        {
            myHandler.Post(() => {
                Toast.MakeText(this, msg, ToastLength.Long).Show();
            });
        }

        public QuakeDataSource getQuakeDataSource()
        {
            return dataSource;
        }

        //
        // Called when our service is started.  This is where we start any long running tasks
        //
        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;                   // We return sticky as we are a long running service....
        }

        public override IBinder OnBind(Intent intent)
        {
            return new QuakeServiceBinder(this);
        }

        //
        // Called when our service is being shutdown. We should stop any long running tasks at this point.
        //
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public Coordinates MyLocation()
        {
            Criteria locationCriteria = new Criteria();

            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;

            Location loc = null;
            string locationProvider = locMgr.GetBestProvider(locationCriteria, true);
            if (locationProvider != null)
            {
                loc = locMgr.GetLastKnownLocation(locationProvider);
            }
            Coordinates coords = new Coordinates();
            if (loc != null)
            {
                coords.Longitude = loc.Longitude;
                coords.Latitude = loc.Latitude;
            }
            else
            {
                // Just use TW as a default
                coords.Longitude = 0.0;
                coords.Latitude = 51.0;
            }
            coords.Depth = 0;
            return coords;
        }

        public QuakeConfig getConfiguration()
        {
            if (config == null)
            {
                config = new QuakeConfig();
                ISharedPreferences settings = GetSharedPreferences(PREFS_NAME, 0);
                config.SortOrder = (QuakeSortOrder)(settings.GetInt("sortOrder", (int)(QuakeSortOrder.Nearest)));
            }
            return config;
        }

        public void saveConfiguration()
        {
            if (config != null)
            {
                ISharedPreferences settings = GetSharedPreferences(PREFS_NAME, 0);
                ISharedPreferencesEditor editor = settings.Edit();
                editor.PutInt("sortOrder", (int)config.SortOrder);
                editor.Commit();
            }
        }
    }

    public class QuakeServiceBinder : Binder
    {
        QuakeService service;

        public QuakeServiceBinder(QuakeService service)
        {
            this.service = service;
        }

        public QuakeService getQuakeService()
        {
            return service;
        }
    }
}