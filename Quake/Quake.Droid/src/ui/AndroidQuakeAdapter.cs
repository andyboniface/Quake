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

namespace Quake.Droid.src.ui
{
    public class AndroidQuakeAdapter : BaseAdapter<string>
    {
        QuakeActivity context;

        public AndroidQuakeAdapter(QuakeActivity context)
        {
            this.context = context;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override string this[int position]
        {
            get
            {
                QuakeDataSource ds = getDataSource();
                if (ds != null)
                {
                    Quake quake = ds.getQuake(position);
                    return quake.Place;
                }
                return "";
            }
        }

        public override int Count
        {
            get
            {
                QuakeDataSource ds = getDataSource();
                if (ds != null)
                {
                    return ds.Count;
                }
                return 0;
            }
        }

        public QuakeDataSource getDataSource()
        {
            return context.getQuakeDataSource();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.QuakeView, null);
            }
            QuakeDataSource ds = getDataSource();
            Quake quake;
            if (ds != null)
            {
                quake = getDataSource().getQuake(position);
                DateTime dtim = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(quake.Time / 1000));  // We need time in seconds.
                view.FindViewById<TextView>(Resource.Id.Place).Text = quake.Place;
                view.FindViewById<TextView>(Resource.Id.Time).Text = dtim.ToString("HH:mm:ss dd-MMM-yyyy UTC");
                view.FindViewById<TextView>(Resource.Id.Away).Text = ((int)(quake.DistanceAway / 1000)) + "km away";
                view.FindViewById<TextView>(Resource.Id.Magnitude).Text = "Mag:" + quake.Mag;
                //view.FindViewById<TextView>(Resource.Id.Magnitude).SetTextColor(new Android.Graphics.Color(quake.getQuakeColor().ToArgb()));
            }
            return view;
        }
    }
}