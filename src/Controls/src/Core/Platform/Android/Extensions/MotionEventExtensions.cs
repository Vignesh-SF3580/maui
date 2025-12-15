using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform;

internal static class MotionEventExtensions
{
	public static bool IsSecondary(this MotionEvent me)
	{
		var buttonState = me?.ButtonState ?? MotionEventButtonState.Primary;

		return
		  buttonState == MotionEventButtonState.Secondary ||
		  buttonState == MotionEventButtonState.StylusSecondary;
	}

	internal static Point? CalculatePosition(this MotionEvent? e, IElement? sourceElement, IElement? relativeElement)
	{
		var context = sourceElement?.Handler?.MauiContext?.Context;

		if (context == null)
			return null;

		if (e == null)
			return null;

		if (relativeElement == null)
		{
			return new Point(context.FromPixels(e.RawX), context.FromPixels(e.RawY));
		}

		if (relativeElement == sourceElement)
		{
			return new Point(context.FromPixels(e.GetX()), context.FromPixels(e.GetY()));
		}

		if (relativeElement?.Handler?.PlatformView is AView aView)
		{
			var location = aView.GetLocationOnScreenPx();

			var x = e.RawX - location.X;
			var y = e.RawY - location.Y;

			// Account for Translation transformations applied to the view hierarchy
			var (translationXPx, translationYPx) = GetAccumulatedTranslationOffsetPx(relativeElement, context);
			x -= translationXPx;
			y -= translationYPx;

			return new Point(context.FromPixels(x), context.FromPixels(y));
		}

		return null;
	}

	/// <summary>
	/// Calculates the accumulated translation offset from a view and all its parent views.
	/// Returns the offset in pixels to be subtracted from touch coordinates.
	/// </summary>
	private static (double x, double y) GetAccumulatedTranslationOffsetPx(IElement? element, Context? context)
	{
		if (element == null || context == null)
			return (0, 0);

		double totalX = 0;
		double totalY = 0;

		// Walk up the visual tree accumulating translations
		var current = element;
		while (current != null)
		{
			if (current is VisualElement visualElement)
			{
				totalX += visualElement.TranslationX;
				totalY += visualElement.TranslationY;
			}

			current = (current as VisualElement)?.Parent;
		}

		// Convert from device-independent units to pixels
		var translationXPx = context.ToPixels(totalX);
		var translationYPx = context.ToPixels(totalY);

		return (translationXPx, translationYPx);
	}
}
