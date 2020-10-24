using Android.Content;
using ChargerNet.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(SuperEntryRenderer))]
namespace ChargerNet.Droid
{
    public class SuperEntryRenderer : EntryRenderer
    {
        public SuperEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
        }
    }
}