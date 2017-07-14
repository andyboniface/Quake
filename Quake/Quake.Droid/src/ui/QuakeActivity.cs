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
using Quake.Droid.src.service;
using Android.Util;

namespace Quake.Droid.src.ui
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon")]
    public class QuakeActivity : Activity
    {
        ListView lv;
        AndroidQuakeAdapter listAdapter = null;
        IMenuItem sortByNearestCb;
        IMenuItem sortByMagnitudeCb;
        IMenuItem sortByTimeCb;
        Intent quakeServiceIntent;
        QuakeReceiver quakeReceiver;
        QuakeServiceBinder binder;
        QuakeServiceConnection quakeServiceConnection;
        bool isBound = false;

        private static string TAG = "QuakeWatch";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);

            base.OnCreate(savedInstanceState);

            try
            {
                Log.Info(TAG, "Hello world");

                Intent serviceToStart = new Intent(this, typeof(QuakeService));

                StartService(serviceToStart);

                //quakeServiceIntent = new Intent("uk.co.fixsolutions.QuakeService");
                quakeServiceIntent = new Intent(this, typeof(QuakeService));

                quakeReceiver = new QuakeReceiver(this);

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.Main);

                lv = FindViewById<ListView>(Resource.Id.listView1);
                listAdapter = new AndroidQuakeAdapter(this);
                lv.Adapter = listAdapter;
                lv.ItemClick += (sender, e) => {

                    int pos = e.Position;
                    string msg = "item=" + pos;
                    Android.Widget.Toast.MakeText(this, msg, Android.Widget.ToastLength.Short).Show();

                    Quake quake = getQuakeDataSource().getQuake(pos);

                    //var url = "geo:" + quake.Coordinates.Latitude + "," + quake.Coordinates.Longitude + "?z=10";
                    //var geoUri = Android.Net.Uri.Parse (url);
                    //var mapIntent = new Intent (Intent.ActionView, geoUri);
                    //StartActivity (mapIntent);

                    var intent = new Intent(this, typeof(QuakeDetailActivity));
                    intent.PutExtra("quake", quake.toJson());

                    StartActivity(intent);
                };
            }
            catch (Exception e)
            {
                Log.Error(TAG, "Error: " + e.ToString());
            }
        }

        public QuakeDataSource getQuakeDataSource()
        {
            if (isBound)
            {
                return binder.getQuakeService().getQuakeDataSource();
            }
            return null;
        }

        public QuakeConfig getQuakeConfig()
        {
            if (isBound)
            {
                return binder.getQuakeService().getConfiguration();
            }
            return null;
        }

        protected override void OnStart()
        {
            base.OnStart();

            var intentFilter = new IntentFilter(QuakeService.QuakesUpdatedAction) { Priority = (int)IntentFilterPriority.HighPriority };
            RegisterReceiver(quakeReceiver, intentFilter);

            quakeServiceConnection = new QuakeServiceConnection(this);
            BindService(quakeServiceIntent, quakeServiceConnection, Bind.AutoCreate);
        }

        protected override void OnStop()
        {
            base.OnStop();

            if (isBound)
            {
                binder.getQuakeService().saveConfiguration();
                UnbindService(quakeServiceConnection);

                isBound = false;
            }

            UnregisterReceiver(quakeReceiver);
        }

        class QuakeServiceConnection : Java.Lang.Object, IServiceConnection
        {
            QuakeActivity activity;

            public QuakeServiceConnection(QuakeActivity activity)
            {
                this.activity = activity;
            }

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                var quakeServiceBinder = service as QuakeServiceBinder;
                if (quakeServiceBinder != null)
                {
                    var binder = (QuakeServiceBinder)service;
                    activity.binder = binder;
                    activity.isBound = true;
                }
            }
            public void OnServiceDisconnected(ComponentName name)
            {
                activity.isBound = false;
            }
        }
        class QuakeReceiver : BroadcastReceiver
        {
            QuakeActivity activity;

            public QuakeReceiver(QuakeActivity activity)
            {
                this.activity = activity;
            }

            public override void OnReceive(Context context, Android.Content.Intent intent)
            {
                if (intent.Action.Equals(QuakeService.QuakesUpdatedAction))
                {
                    activity.refreshList();                         // Force a list update....
                    InvokeAbortBroadcast();                         // We handled it....don't pass onto anyone else
                }
            }
        }

        //
        // Handle events generated by settings menu
        //
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.byTime:
                    //do something
                    item.SetChecked(true);
                    displayToast("Ordering by time");
                    getQuakeConfig().SortOrder = QuakeSortOrder.ByTime;
                    refreshList();
                    return true;
                case Resource.Id.byMagnitude:
                    //do something
                    item.SetChecked(true);
                    displayToast("Ordering by magnitude");
                    getQuakeConfig().SortOrder = QuakeSortOrder.ByMagnitude;
                    refreshList();
                    return true;
                case Resource.Id.byNearest:
                    //do something
                    item.SetChecked(true);
                    displayToast("Ordering by nearest");
                    getQuakeConfig().SortOrder = QuakeSortOrder.Nearest;
                    refreshList();
                    return true;
                case Resource.Id.preferences:
                    //do something
                    displayToast("Preferences");
                    return true;
                case Resource.Id.help:
                    //do something
                    displayToast("Help");
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        //
        // Define our settings menu.
        //
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MyMenu, menu);

            if (sortByNearestCb == null)
            {
                sortByNearestCb = menu.FindItem(Resource.Id.byNearest);
                sortByMagnitudeCb = menu.FindItem(Resource.Id.byMagnitude);
                sortByTimeCb = menu.FindItem(Resource.Id.byTime);

                if (sortByNearestCb != null)
                {
                    switch (getQuakeConfig().SortOrder)
                    {
                        case QuakeSortOrder.ByMagnitude:
                            sortByMagnitudeCb.SetChecked(true);
                            break;
                        case QuakeSortOrder.ByTime:
                            sortByTimeCb.SetChecked(true);
                            break;
                        case QuakeSortOrder.Nearest:
                            sortByNearestCb.SetChecked(true);
                            break;
                    }
                }
            }

            bool state = base.OnPrepareOptionsMenu(menu);

            return state;
        }

        public void displayToast(string msg)
        {
            RunOnUiThread(() => {
                Android.Widget.Toast.MakeText(this, msg, Android.Widget.ToastLength.Long).Show();
            });
        }

        public void refreshList()
        {
            RunOnUiThread(() => {
                listAdapter.NotifyDataSetChanged();
            });
        }

    }
}