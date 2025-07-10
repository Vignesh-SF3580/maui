#nullable disable
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal sealed class ShellFragmentContainer : Fragment
	{
		Page _page;
		IMauiContext _mauiContext;

		public ShellContent ShellContentTab { get; private set; }

		public ShellFragmentContainer(ShellContent shellContent, IMauiContext mauiContext)
		{
			ShellContentTab = shellContent;
			_mauiContext = mauiContext;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_page = ((IShellContentController)ShellContentTab).GetOrCreateContent();
			
			// Check for preloaded handler to improve navigation performance
			if (ShellContentTab.PreloadedHandler is IPlatformViewHandler preloadedHandler && 
				preloadedHandler.PlatformView != null &&
				preloadedHandler.VirtualView == _page)
			{
				// Use the preloaded handler if available to avoid expensive on-demand creation
				// Update the handler's context to match current navigation context
				if (preloadedHandler.MauiContext is MauiContext scopedMauiContext)
				{
					// Update the scoped context with current inflater and fragment manager
					scopedMauiContext.AddWeakSpecific(inflater);
					scopedMauiContext.AddWeakSpecific(ChildFragmentManager);
				}
				
				// Set the preloaded handler on the page
				_page.Handler = preloadedHandler;
			}
			else
			{
				// Fallback to creating handler on-demand if no preloaded handler is available
				_page.ToPlatform(_mauiContext, RequireContext(), inflater, ChildFragmentManager);
			}

			return new ShellPageContainer(RequireContext(), (IPlatformViewHandler)_page.Handler, true)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
			};
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			((IShellContentController)ShellContentTab).RecyclePage(_page);
			_page = null;
		}

		public override void OnDestroy()
		{
			_mauiContext
				.GetDispatcher()
				.Dispatch(Dispose);

			base.OnDestroy();
		}
	}
}