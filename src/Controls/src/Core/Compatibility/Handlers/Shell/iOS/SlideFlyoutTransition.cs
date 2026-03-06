#nullable disable
using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class SlideFlyoutTransition : IShellFlyoutTransition
	{
		internal double Height { get; private set; } = -1d;
		internal double Width { get; private set; } = -1d;

		public virtual bool UpdateFlyoutSize(double height, double width)
		{
			if (Height != height ||
				Width != width)
			{
				Height = height;
				Width = width;
				return true;
			}

			return false;
		}

		public virtual void LayoutViews(CGRect bounds, nfloat openPercent, UIView flyout, UIView shell, FlyoutBehavior behavior)
		{
			if (behavior == FlyoutBehavior.Locked)
				openPercent = 1;

			nfloat flyoutHeight;
			nfloat flyoutWidth;

			if (Width != -1d)
				flyoutWidth = (nfloat)Width;
			else if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				flyoutWidth = 320;
			else
				flyoutWidth = (nfloat)(Math.Min(bounds.Width, bounds.Height) * 0.8);

			if (Height == -1d)
				flyoutHeight = bounds.Height;
			else
				flyoutHeight = (nfloat)Height;

			nfloat openLimit = flyoutWidth;
			nfloat openPixels = openLimit * openPercent;

			if (behavior == FlyoutBehavior.Locked)
			{
				// On iOS 26+, the Liquid Glass navigation bar fires explicit CALayer
				// animations when its parent view's frame changes (even when wrapped in
				// UIView.PerformWithoutAnimation or CATransaction.DisableActions, which
				// only suppress implicit animations). These animations trigger
				// animationDidStop without a matching animationDidStart in WDA, permanently
				// breaking its animation-idle tracking, causing XCUITest to hang.
				// Keep shell at full bounds on iOS 26+ to prevent this.
				if (!OperatingSystem.IsIOSVersionAtLeast(26))
				{
					shell.Frame = new CGRect(bounds.X + flyoutWidth, bounds.Y, bounds.Width - flyoutWidth, flyoutHeight);
				}
				else
				{
					shell.Frame = bounds;
				}
			}
			else
				shell.Frame = bounds;

			var shellWidth = shell.Frame.Width;

			if (shell.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft)
			{
				var positionX = shellWidth - openPixels;
				flyout.Frame = new CGRect(positionX, 0, flyoutWidth, flyoutHeight);
			}
			else
			{
				flyout.Frame = new CGRect(-openLimit + openPixels, 0, flyoutWidth, flyoutHeight);
			}
		}
	}
}
