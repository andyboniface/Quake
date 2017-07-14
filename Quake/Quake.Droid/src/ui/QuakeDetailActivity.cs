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
using Android.Webkit;

namespace Quake.Droid.src.ui
{
    [Activity(Label = "@string/app_detail_name")]
    public class QuakeDetailActivity : Activity
    {
        private WebView wv;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.QuakeDetailView);

            this.ActionBar.SetDisplayHomeAsUpEnabled(true);
            this.ActionBar.SetHomeButtonEnabled(true);

            //var quake = JsonConvert.DeserializeObject<Quake>(Intent.GetStringExtra("quake"));
            var json = Intent.GetStringExtra("quake");
            Quake quake = new Quake(json);

            DateTime utcTime = quake.getUTCTime();
            DateTime localTime = quake.getLocalTime();

            //			DateTime dtim = new DateTime (1970, 1, 1, 0, 0, 0).AddSeconds (Convert.ToDouble (quake.Time / 1000));	// We need time in seconds.

            FindViewById<TextView>(Resource.Id.detailPlace).Text = quake.Place;
            FindViewById<TextView>(Resource.Id.detailWhen).Text = utcTime.ToString("HH:mm:ss dd-MMM-yyyy UTC");
            FindViewById<TextView>(Resource.Id.detailWhenLocal).Text = localTime.ToString("HH:mm:ss dd-MMM-yyyy (Local)");
            FindViewById<TextView>(Resource.Id.detailDistance).Text = ((int)(quake.DistanceAway / 1000)) + "km away";
            FindViewById<TextView>(Resource.Id.detailMagnitude).Text = "Mag:" + quake.Mag + " (" + quake.getQuakeClassAsString() + ")";
            //FindViewById<TextView>(Resource.Id.detailMagnitude).SetTextColor(new Android.Graphics.Color(quake.getQuakeColor().ToArgb()));
            FindViewById<TextView>(Resource.Id.detailWhere).Text = quake.Coordinates.Latitude + "/" + quake.Coordinates.Longitude + " (latitude/longitude)";
            FindViewById<TextView>(Resource.Id.detailDepth).Text = quake.Coordinates.Depth + " km deep";

            wv = FindViewById<WebView>(Resource.Id.quakeMap);
            if (wv != null)
            {
                wv.Settings.JavaScriptEnabled = true;
                wv.SetWebViewClient(new MyWebViewClient());
                wv.LoadUrl("http://www.bing.com/mapspreview?cp=" + quake.Coordinates.Latitude + "~" + quake.Coordinates.Longitude + "&style=a");
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            this.Finish();
            return true;
        }

        public override bool OnKeyDown(Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
        {
            if (wv != null)
            {
                if (keyCode == Keycode.Back && wv.CanGoBack())
                {
                    wv.GoBack();
                    return true;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }

        public class MyWebViewClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                view.LoadUrl(url);
                return true;
            }
        }
    }
}