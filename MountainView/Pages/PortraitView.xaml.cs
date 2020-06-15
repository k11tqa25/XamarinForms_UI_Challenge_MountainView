using MountainView.ViewModels;
using SkiaSharp;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MountainView.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PortraitView : ContentView
    {
        private SKBitmap currentBitmap;
        private Label currentHeading;
        private Label currentBody;

        private SKBitmap offscreenBitmap;
        private Label offscreenHeading;
        private Label offscreenBody;

        private double transitionValue = 0;
        private double outgoingImageOffset = 0;
        //private int currentIndex = 0;

        private SlideShowViewModel viewModel;

        public PortraitView()
        {
            InitializeComponent();

            this.BindingContext = viewModel = ViewModelLocator.SlideshowViewModel;

            currentBody = Body1;
            currentHeading = Heading1;
            offscreenBody = Body2;
            offscreenHeading = Heading2;
            offscreenBody.Opacity = 0;
            offscreenHeading.Opacity = 0;
            currentBitmap = BitmapExtensions.LoadBitmapResource(this.GetType(), viewModel.CurrentLocation.ImageResource);
        }

        //private int GetNextIndex()
        //{
        //    var nextIndex = currentIndex + 1;
        //    if (nextIndex > Location.LocationPages.Count - 1) nextIndex = 0;
        //    return nextIndex;
        //}

        private void UpdateOffScreenElements()
        {
            offscreenHeading.Text = viewModel.NextLocation.Title;
            offscreenBody.Text = viewModel.NextLocation.Description;
            offscreenBitmap = BitmapExtensions.LoadBitmapResource(this.GetType(), viewModel.NextLocation.ImageResource);
        }

        private void ImageSkiaCanvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKRect imageRect = new SKRect(0, 0, info.Width, info.Height);
            // render out the bitmap
            SKRect outgoingImageRect;
            if (transitionValue >= 1)
                outgoingImageRect = new SKRect(0 - (float)outgoingImageOffset, 0, info.Width, info.Height);
            else
                outgoingImageRect = new SKRect(0, 0, info.Width, info.Height);
            canvas.DrawBitmap(currentBitmap, outgoingImageRect, BitmapStretch.AspectFill);

            if (transitionValue <= 0) return;

            // draw our clipping path
            if (offscreenBitmap != null)
            {
                // animate in the rect that the image is being rendered to
                int movementAmount = 600;
                float offset = (float)(movementAmount - movementAmount * (transitionValue / 2));
                SKRect incomingRect = new SKRect(0, 0, info.Width + offset, info.Height);

                // draw the faded version of the image
                using (SKPaint transparentPaint = new SKPaint())
                {
                    var opacity = Math.Max((transitionValue - 0.5) * 0.5, 0);
                    transparentPaint.Color = transparentPaint.Color.WithAlpha((byte)(0xff * opacity));
                    canvas.DrawBitmap(
                    bitmap: offscreenBitmap,
                    dest: incomingRect,
                    stretch: BitmapStretch.AspectFill,
                    paint: transparentPaint);
                }

                var clipPath = CalculateClipPath(info, transitionValue);
                canvas.ClipPath(clipPath, SKClipOperation.Intersect);
                canvas.DrawBitmap(offscreenBitmap, incomingRect, BitmapStretch.AspectFill);
            }
        }

        private SKPath CalculateClipPath(SKImageInfo info, double transitionValue)
        {
            // calculate offset
            var xDelta = transitionValue > 1 ? info.Width : info.Width * transitionValue;
            var yDelta = transitionValue < 1 ? 0 : (info.Height / 2) * (transitionValue - 1);
            var xPos = info.Width - xDelta;
            var yPos1 = (info.Height / 2) - yDelta;
            var yPos2 = (info.Height / 2) + yDelta;

            // construct our path
            SKPath path = new SKPath();
            path.MoveTo(info.Width, 0);
            path.LineTo((float)xPos, (float)yPos1);
            path.LineTo((float)xPos, (float)yPos2);
            path.LineTo(info.Width, info.Height);
            return path;
        }

        private void CycleElement()
        {
            if (currentHeading == Heading1)
            {
                currentHeading = Heading2;
                currentBody = Body2;
                offscreenHeading = Heading1;
                offscreenBody = Body1;
            }
            else
            {
                currentHeading = Heading1;
                currentBody = Body1;
                offscreenHeading = Heading2;
                offscreenBody = Body2;
            }
        }

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            // see if the animation is running
            if (this.AnimationIsRunning("TransitionAnimation"))
                return;

            // update the elements
            UpdateOffScreenElements();

            // user has swipped
            var onScreenHeadingSlideOut = new Animation(v => currentHeading.TranslationX = v, 0, -this.Width, Easing.SinIn);
            var onScreenHeadingFadeOut = new Animation(v => currentHeading.Opacity = v, 1, 0, Easing.SinIn);
            var onScreenBodySlideOut = new Animation(v => currentBody.TranslationX = v, 0, -this.Width, Easing.SinIn);
            var onScreenBodyFadeOut = new Animation(v => currentBody.Opacity = v, 1, 0, Easing.SinIn);

            var offScreenHeadingSlideIn = new Animation(v => offscreenHeading.TranslationX = v, this.Width, 0, Easing.SinIn);
            var offScreenHeadingFadeIn = new Animation(v => offscreenHeading.Opacity = v, 0, 1, Easing.SinIn);
            var offScreenBodySlideIn = new Animation(v => offscreenBody.TranslationX = v, this.Width, 0, Easing.SinIn);
            var offScreenBodyFadeIn = new Animation(v => offscreenBody.Opacity = v, 0, 1, Easing.SinIn);

            // animation for skia
            var skiaAnimation = new Animation(
                callback: v =>
                {
                    transitionValue = v;
                    ImageSkiaCanvas.InvalidateSurface();
                }, start: 0, end: 2);

            var outgoingImageAnimation = new Animation(
                callback: v =>
                {
                    outgoingImageOffset = v;
                    ImageSkiaCanvas.InvalidateSurface();
                }, start: 0, end: this.Width);

            var parentAnimation = new Animation();

            parentAnimation.Add(0, 1, onScreenHeadingSlideOut);
            parentAnimation.Add(0, 1, onScreenHeadingFadeOut);
            parentAnimation.Add(.2, 1, onScreenBodySlideOut);
            parentAnimation.Add(0, 1, onScreenBodyFadeOut);

            parentAnimation.Add(0, 1, offScreenHeadingSlideIn);
            parentAnimation.Add(0, 1, offScreenHeadingFadeIn);
            parentAnimation.Add(.2, 1, offScreenBodySlideIn);
            parentAnimation.Add(0, 1, offScreenBodyFadeIn);

            parentAnimation.Add(0, 1, skiaAnimation);
            parentAnimation.Add(0.5, 1, outgoingImageAnimation);

            parentAnimation.Commit(this, "TransitionAnimation", 16, 800, null,
                (v, c) =>
                {
                    viewModel.MoveNext();
                    CycleElement();
                    currentBitmap = offscreenBitmap;
                });
        }
    }
}