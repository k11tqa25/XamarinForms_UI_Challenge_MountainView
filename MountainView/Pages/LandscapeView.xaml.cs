using MountainView.ViewModels;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MountainView.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LandscapeView : ContentView
    {
        private SlideShowViewModel viewModel;
        private SKBitmap currentBitmap;
        private SKBitmap currentBitmapAspectCorrected;

        public LandscapeView()
        {
            InitializeComponent();
            this.BindingContext = viewModel = ViewModelLocator.SlideshowViewModel;
            currentBitmap = BitmapExtensions.LoadBitmapResource(this.GetType(), viewModel.CurrentLocation.ImageResource);
        }

        private void SKCanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKRect imageRect = new SKRect(0, 0, info.Width, info.Height);
            currentBitmapAspectCorrected = CreateAspectCoorectedBitmap(currentBitmap, imageRect);

            if (!isAnimating)
            {
                canvas.DrawBitmap(currentBitmapAspectCorrected, imageRect);
                return;
            }
            else
            {
                // work out our heights and widths of our rectangles
                var bandHeight = currentBitmapAspectCorrected.Height / bandTranslationValues.Length;
                var bandWidth = currentBitmapAspectCorrected.Width;

                for (int i = 0; i < bandTranslationValues.Length; i++)
                {
                    var bandyOffset = i * bandHeight;
                    var bandxOffest = (float)bandTranslationValues[i];

                    // calculate the source rectangle
                    SKRect source = new SKRect(0, bandyOffset, bandWidth, bandyOffset + bandHeight);

                    // calculate the destination (consider the animation value)
                    SKRect dest = new SKRect(bandxOffest, bandyOffset, bandxOffest + bandWidth, bandyOffset + bandHeight);

                    // draw the bitmap
                    canvas.DrawBitmap(currentBitmapAspectCorrected, source, dest);
                }
            }
        }

        private SKBitmap CreateAspectCoorectedBitmap(SKBitmap sourceBitmap, SKRect destRect)
        {
            // create a bitmap

            SKBitmap aspectFixedBitmap = new SKBitmap((int)destRect.Width, (int)destRect.Height);

            // create a canvas for that bitmap
            using (SKCanvas aspectBitmaptCanvas = new SKCanvas(aspectFixedBitmap))
            {
                // render the image onto the canvas
                aspectBitmaptCanvas.DrawBitmap(sourceBitmap, destRect, BitmapStretch.AspectFill);
            }
            return aspectFixedBitmap;
        }


        const int numberOfBands = 5;
        private double[] bandTranslationValues = new double[5];
        private bool isAnimating;

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            // see if the animation is running
            if (this.AnimationIsRunning("TransitionAnimation"))
                return;

            var screenWidthPixels = ImageSkiaCanvas.CanvasSize.Width; // Get the actual pixels

            // make sure we are all starting at 0;
            bandTranslationValues = new double[numberOfBands];

            var parentAnimation = new Animation();


            // create animation for each band
            parentAnimation.Add(.5, .95, new Animation(v => bandTranslationValues[0] = v, 0, -screenWidthPixels, Easing.SinInOut));
            parentAnimation.Add(.35, .90, new Animation(v => bandTranslationValues[1] = v, 0, -screenWidthPixels, Easing.SinInOut));
            parentAnimation.Add(.25, .85, new Animation(v => bandTranslationValues[2] = v, 0, -screenWidthPixels, Easing.SinInOut));
            parentAnimation.Add(.15, .8, new Animation(v => bandTranslationValues[3] = v, 0, -screenWidthPixels, Easing.SinInOut));
            parentAnimation.Add(0, .75, new Animation(v => bandTranslationValues[4] = v, 0, -screenWidthPixels, Easing.SinInOut));

            var skiaAnimation = new Animation(
                callback: v =>
                {
                    isAnimating = true;
                    ImageSkiaCanvas.InvalidateSurface();
                }, start: 0, end: screenWidthPixels, easing: Easing.SinInOut);

            parentAnimation.Add(0, 1, skiaAnimation);

            parentAnimation.Commit(this, "TransitionAnimation", 16, 2000, Easing.SinInOut,
                (v, c) =>
                {
                    isAnimating = false;
                });
        }
    }
}