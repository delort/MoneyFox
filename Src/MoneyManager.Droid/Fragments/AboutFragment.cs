using Android.OS;
using Android.Views;
using MoneyManager.Core.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.FullFragging.Fragments;

namespace MoneyManager.Droid.Fragments
{
    public class AboutFragment : MvxFragment
    {
        public new AboutViewModel ViewModel
        {
            get { return (AboutViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.AboutLayout, null);
        }
    }
}