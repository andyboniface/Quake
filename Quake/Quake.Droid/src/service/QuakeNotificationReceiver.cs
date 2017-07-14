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
using Quake.Droid.src.ui;

namespace Quake.Droid.src.service
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { QuakeService.NewQuakeDetectedAction }, Priority = (int)IntentFilterPriority.LowPriority)]
    public class QuakeNotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(QuakeService.NewQuakeDetectedAction))
            {
                var json = intent.GetStringExtra("quake");
                Quake quake = new Quake(json);

                var nMgr = (NotificationManager)context.GetSystemService(Context.NotificationService);
                var detailIntent = new Intent(context, typeof(QuakeDetailActivity));
                detailIntent.PutExtra("quake", quake.toJson());
                var pendingIntent = PendingIntent.GetActivity(context, 0, detailIntent, 0);

                Android.App.Notification.Builder builder = new Notification.Builder(context);

                builder.SetContentTitle("New Earthquake");
                builder.SetContentText(quake.ToString());
                builder.SetContentIntent(pendingIntent);
                builder.SetSmallIcon(Resource.Drawable.Icon);
                builder.SetAutoCancel(true);                    // This means it will cancel when user clicks on it.

                var notification = builder.Build();
                nMgr.Notify(0, notification);
            }
        }
    }
}