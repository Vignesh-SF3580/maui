using System;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		partial void HandlePlatformUnloadedLoaded()
		{
#if PLATFORM
			// Clean up previous token if any
			_loadedUnloadedToken?.Dispose();
			_loadedUnloadedToken = null;

			// If we don't have any subscribers, no need to wire up platform events
			if (_unloaded is null && _loaded is null)
				return;

			// For iOS, use the Shell-aware version of OnUnloaded for Pages
			if (this is Page page && _unloaded is not null)
			{
				_loadedUnloadedToken = this.OnUnloaded(() =>
				{
					// Additional check for Shell tab scenarios to prevent false Unloaded events
					if (IsShellTabScenario(page))
					{
						// In Shell tab switching, pages are temporarily removed from window during transitions
						// but they're not actually being unloaded, so don't fire the Unloaded event
						return;
					}

					SendUnloaded(updateWiring: false);
				});
			}
			else
			{
				// For non-Page elements or when only Loaded is subscribed, use standard behavior
				if (_loaded is not null)
				{
					var loadedToken = this.OnLoaded(() => SendLoaded(updateWiring: false));
					
					if (_unloaded is not null)
					{
						var unloadedToken = this.OnUnloaded(() => SendUnloaded(updateWiring: false));
						_loadedUnloadedToken = new CompositeDisposable(loadedToken, unloadedToken);
					}
					else
					{
						_loadedUnloadedToken = loadedToken;
					}
				}
				else if (_unloaded is not null)
				{
					_loadedUnloadedToken = this.OnUnloaded(() => SendUnloaded(updateWiring: false));
				}
			}
#endif
		}

		static bool IsShellTabScenario(Page page)
		{
			// Check if the page is part of an active Shell
			if (Shell.Current is not Shell shell)
				return false;

			// If the page is still in the navigation stack, it's likely a tab switch scenario
			// rather than actual page removal
			if (page.Navigation?.NavigationStack?.Contains(page) == true)
				return true;

			// Additional check: if the page is a root page of a Shell content that's still active
			var shellContent = FindParentShellContent(page);
			if (shellContent != null)
			{
				// If the ShellContent is still part of the active Shell structure, 
				// this is likely a tab switch
				return IsShellContentActive(shellContent, shell);
			}

			return false;
		}

		static ShellContent? FindParentShellContent(Page page)
		{
			var parent = page.Parent;
			while (parent != null)
			{
				if (parent is ShellContent shellContent)
					return shellContent;
				parent = parent.Parent;
			}
			return null;
		}

		static bool IsShellContentActive(ShellContent shellContent, Shell shell)
		{
			// Check if the ShellContent is part of the current Shell structure
			foreach (var item in shell.Items)
			{
				if (item is TabBar tabBar)
				{
					foreach (var tab in tabBar.Items)
					{
						if (tab == shellContent)
							return true;
					}
				}
				else if (item is ShellSection section)
				{
					foreach (var content in section.Items)
					{
						if (content == shellContent)
							return true;
					}
				}
				else if (item == shellContent)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// A disposable that manages multiple disposables.
		/// </summary>
		private class CompositeDisposable : IDisposable
		{
			private readonly IDisposable[] _disposables;
			private bool _disposed;

			public CompositeDisposable(params IDisposable[] disposables)
			{
				_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
			}

			public void Dispose()
			{
				if (!_disposed)
				{
					foreach (var disposable in _disposables)
					{
						disposable?.Dispose();
					}
					_disposed = true;
				}
			}
		}
	}
}