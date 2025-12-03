#nullable disable
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal sealed class ShellFragmentContainer : Fragment
	{
		Page _page;
		IMauiContext _mauiContext;
		static readonly Dictionary<Page, ShellPageContainer> _cachedPlatformViews = new Dictionary<Page, ShellPageContainer>();

		public ShellContent ShellContentTab { get; private set; }

		public ShellFragmentContainer(ShellContent shellContent, IMauiContext mauiContext)
		{
			ShellContentTab = shellContent;
			_mauiContext = mauiContext;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_page = ((IShellContentController)ShellContentTab).GetOrCreateContent();
			
			// Check if we have a cached platform view for this page to improve navigation performance
			if (_cachedPlatformViews.TryGetValue(_page, out var cachedContainer))
			{
				// Remove from previous parent if it has one
				(cachedContainer.Parent as ViewGroup)?.RemoveView(cachedContainer);
				
				// Ensure layout parameters are correct
				cachedContainer.LayoutParameters = new LP(LP.MatchParent, LP.MatchParent);
				
				return cachedContainer;
			}
			
			// Create platform view for the first time (this is the expensive operation)
			_page.ToPlatform(_mauiContext, RequireContext(), inflater, ChildFragmentManager);

			var pageContainer = new ShellPageContainer(RequireContext(), (IPlatformViewHandler)_page.Handler, true)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
			};
			
			// Cache the platform view for future navigations
			_cachedPlatformViews[_page] = pageContainer;
			
			return pageContainer;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			// Don't dispose the cached platform view, just notify the page it's not visible
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
		
		/// <summary>
		/// Clears cached platform views for pages that are no longer reachable.
		/// This prevents memory leaks when ShellContent is removed from the Shell.
		/// </summary>
		internal static void ClearCachedView(Page page)
		{
			if (page != null && _cachedPlatformViews.TryGetValue(page, out var cachedContainer))
			{
				_cachedPlatformViews.Remove(page);
				cachedContainer?.Dispose();
			}
		}
	}
}