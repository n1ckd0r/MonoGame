#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion
using System;
using System.Drawing;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Microsoft.Xna.Framework {
	class iOSGameViewController : UIViewController {
		iOSGamePlatform _platform;

		public iOSGameViewController (iOSGamePlatform platform)
		{
			if (platform == null)
				throw new ArgumentNullException ("platform");
			_platform = platform;
			SupportedOrientations = DisplayOrientation.Default;

			NSArray obj = (NSArray)NSBundle.MainBundle.ObjectForInfoDictionary("UISupportedInterfaceOrientations");

			for(int idx = 0; idx < obj.Count;++idx)
			{
				string value = obj.GetItem<NSString>(idx).ToString();

				switch(value)
				{
				// NOTE: in XNA, Orientation Left is a 90 degree rotation counterclockwise, while on iOS
                // it is a 90 degree rotation CLOCKWISE. They are BACKWARDS!
                case "UIInterfaceOrientationLandscapeLeft":
                    SupportedOrientations |= DisplayOrientation.LandscapeRight;
                    break;

                case "UIInterfaceOrientationLandscapeRight":
                    SupportedOrientations |= DisplayOrientation.LandscapeLeft;
                    break;

                case "UIInterfaceOrientationPortrait":
                    SupportedOrientations |= DisplayOrientation.Portrait;
                    break;

                case "UIInterfaceOrientationPortraitUpsideDown":
                    SupportedOrientations |= DisplayOrientation.PortraitDown;
                    break;
				}
			}
		}

		public event EventHandler<EventArgs> InterfaceOrientationChanged;

		public DisplayOrientation SupportedOrientations { get; set; }

		public override void LoadView ()
		{
			RectangleF frame = CalculateFrame();
			base.View = new iOSGameView(_platform, frame);
		}

		public new iOSGameView View {
			get { return (iOSGameView) base.View; }
		}

        #region Autorotation for iOS 5 or older
        [Obsolete]
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
            DisplayOrientation supportedOrientations = OrientationConverter.Normalize (SupportedOrientations);
			var toOrientation = OrientationConverter.ToDisplayOrientation (toInterfaceOrientation);
			return (toOrientation & supportedOrientations) == toOrientation;
		}
        #endregion

        #region Autorotation for iOS 6 or newer
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
        {
            return OrientationConverter.ToUIInterfaceOrientationMask(this.SupportedOrientations);
        }
        
        public override bool ShouldAutorotate ()
        {
			return SupportedOrientations.HasFlag(DisplayOrientation.LandscapeLeft) || SupportedOrientations.HasFlag(DisplayOrientation.LandscapeRight) || _platform.Game.Initialized;
        }
        #endregion

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);

			RectangleF frame = CalculateFrame();
			base.View.Frame = frame;

			var handler = InterfaceOrientationChanged;
			if (handler != null)
				handler (this, EventArgs.Empty);
        }

        #region Hide statusbar for iOS 7 or newer
        public override bool PrefersStatusBarHidden()
        {
            return _platform.Game.graphicsDeviceManager.IsFullScreen;
        }
        #endregion

		private RectangleF CalculateFrame()
        {
            RectangleF frame;
            if (ParentViewController != null && ParentViewController.View != null)
            {
                frame = new RectangleF(PointF.Empty, ParentViewController.View.Frame.Size);
            } 
            else
            {
                UIScreen screen = UIScreen.MainScreen;

                // iOS 7 and older reverses width/height in landscape mode when reporting resolution,
                // iOS 8+ reports resolution correctly in all cases
                if (InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
                {
                    frame = new RectangleF(0, 0, Math.Max(screen.Bounds.Width, screen.Bounds.Height), Math.Min(screen.Bounds.Width, screen.Bounds.Height));
                } else
                {
                    frame = new RectangleF(0, 0, screen.Bounds.Width, screen.Bounds.Height);
                }
            }

            return frame;
        }
    }
}
