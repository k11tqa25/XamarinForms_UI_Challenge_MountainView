using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MountainView.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrientationContentPage : ContentPage
    {
        private double width = 0;
        private double height = 0;

        protected Type LandscapeLayoutType;
        protected Type PortraitLayoutType;


        public OrientationContentPage(): base()
        {
            Initi();
        }

        private void Initi()
        {
            width = this.Width;
            height = this.Height;
            UpdateLayout();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            // HAVE TO call the base
            base.OnSizeAllocated(width, height); 

            if(this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;
                UpdateLayout();
            }
        }

        private void UpdateLayout()
        {
            if(width > height)
            {
                SetupLandscapeLayout();
            }
            else
            {
                SetupPortraitLayout();
            }
        }

        private void SetupLandscapeLayout()
        {
            if(LandscapeLayoutType != null)
            {
                Content = (View)Activator.CreateInstance(LandscapeLayoutType);
            }
        }

        private void SetupPortraitLayout()
        {
            if (PortraitLayoutType != null)
            {
                Content = (View)Activator.CreateInstance(PortraitLayoutType);
            }
        }
    }
}