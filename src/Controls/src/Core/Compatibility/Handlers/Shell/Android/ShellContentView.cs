#nullable disable
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	// This is used to monitor an xplat View and apply layout changes
	internal sealed class ShellViewRenderer
	{
		public IViewHandler Handler { get; private set; }
		IView _view;
		WeakReference<Context> _context;
		readonly IMauiContext _mauiContext;
		IView MauiView => View;
		public AView PlatformView { get; private set; }

		// These are used by layout calls made by android if the layouts
		// are invalidated. This ensures that the layout is performed
		// using the same input values
		public double Width { get; private set; }
		public double Height { get; private set; }
		public double? MaxWidth { get; private set; }
		public double? MaxHeight { get; private set; }
		public double X { get; private set; }
		public double Y { get; private set; }

		public ShellViewRenderer(Context context, View view, IMauiContext mauiContext)
		{
			_mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));
			_context = new WeakReference<Context>(context);
			View = view;
		}

		public IView View
		{
			get { return _view; }
			set
			{
				OnViewSet(value);
			}
		}

		public void TearDown()
		{
			MauiView?.Handler?.DisconnectHandler();
			View = null;
			Handler = null;
			_view = null;
			_context = null;
		}

		public Graphics.Size Measure(int widthMeasureSpec, int heightMeasureSpec, int? maxHeightPixels, int? maxWidthPixels)
		{
			var width = widthMeasureSpec.GetSize();
			var height = heightMeasureSpec.GetSize();
			var maxWidth = maxWidthPixels;
			var maxHeight = maxHeightPixels;

			Context context;

			if (Handler == null || !(_context.TryGetTarget(out context)) || !PlatformView.IsAlive())
				return Graphics.Size.Zero;

			if (View?.Handler == null)
				return Graphics.Size.Zero;

			var layoutParams = PlatformView.LayoutParameters;

			if (height > maxHeight)
				heightMeasureSpec = MeasureSpecMode.AtMost.MakeMeasureSpec(maxHeight.Value);

			if (width > maxWidth)
				widthMeasureSpec = MeasureSpecMode.AtMost.MakeMeasureSpec(maxWidth.Value);

			if (layoutParams.Width != LP.MatchParent && width > 0)
				layoutParams.Width = width;
			else
				widthMeasureSpec = MeasureSpecMode.Unspecified.MakeMeasureSpec(0);

			if (layoutParams.Height != LP.MatchParent && height > 0)
				layoutParams.Height = height;
			else
				heightMeasureSpec = MeasureSpecMode.Unspecified.MakeMeasureSpec(0);

			PlatformView.LayoutParameters = layoutParams;

			return ((IPlatformViewHandler)_view.Handler).MeasureVirtualView(widthMeasureSpec, heightMeasureSpec);
		}

		public void OnViewSet(IView view)
		{
			if (view == View)
				return;

			if (View != null && PlatformView.IsAlive())
			{
				PlatformView.RemoveFromParent();
				View.Handler?.DisconnectHandler();
			}

			_view = view;
			if (view != null)
			{
				Context context;

				if (!(_context.TryGetTarget(out context)))
					return;

				// Check if this is a Page with preloaded handler to improve flyout navigation performance
				if (view is Page page && TryGetPreloadedHandler(page, out var preloadedHandler))
				{
					// Use the preloaded handler if available
					PlatformView = preloadedHandler.PlatformView as AView;
					Handler = preloadedHandler;
					page.Handler = preloadedHandler;
				}
				else
				{
					// Fallback to creating handler on-demand
					PlatformView = view.ToPlatform(_mauiContext);
					Handler = view.Handler;
				}
			}
			else
			{
				PlatformView = null;
			}
		}

		bool TryGetPreloadedHandler(Page page, out IViewHandler preloadedHandler)
		{
			preloadedHandler = null;

			// Try to find a ShellContent with a preloaded handler for this page
			// Look for ShellContent in the page's logical parent hierarchy
			var current = page.Parent;
			while (current != null)
			{
				if (current is ShellContent shellContent && 
					((IShellContentController)shellContent).Page == page && 
					shellContent.PreloadedHandler != null)
				{
					preloadedHandler = shellContent.PreloadedHandler;
					return true;
				}
				current = current.Parent;
			}

			// If not found in parent hierarchy, search in the Shell (fallback)
			if (page.Parent is Shell shell)
			{
				foreach (var item in shell.Items)
				{
					foreach (var section in item.Items)
					{
						foreach (var content in section.Items)
						{
							if (content is ShellContent shellContent && 
								((IShellContentController)shellContent).Page == page && 
								shellContent.PreloadedHandler != null)
							{
								preloadedHandler = shellContent.PreloadedHandler;
								return true;
							}
						}
					}
				}
			}

			return false;
		}
	}
}
