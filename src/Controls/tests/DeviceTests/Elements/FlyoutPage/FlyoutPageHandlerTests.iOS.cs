using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.DeviceTests.TestCases;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

// This file intentionally does NOT alias FlyoutViewHandler to the Compatibility.PhoneFlyoutPageRenderer
// (as the existing renderer test suites do). `FlyoutViewHandler` here resolves to the real
// Microsoft.Maui.Handlers.FlyoutViewHandler via the `using Microsoft.Maui.Handlers;` directive above.

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// FlyoutViewHandler equivalents of the PhoneFlyoutPageRenderer device tests
	/// (see DeviceTest/PhoneFlyoutPageRenderer_DeviceTests_Analysis.md). A new, parallel
	/// class — existing renderer tests are untouched. All tests are prefixed "[FlyoutViewHandler] ".
	/// </summary>
	[Category(TestCategory.FlyoutPage)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class FlyoutPageHandlerTests : ControlsHandlerTestBase
	{
		bool IsPad => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;

		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);

					handlers.AddHandler(typeof(Controls.Label), typeof(LabelHandler));
					handlers.AddHandler(typeof(Controls.Toolbar), typeof(ToolbarHandler));

					// The new FlyoutPage Handler — this is what's under test in this file.
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));

					// NavigationPage/TabbedPage are unchanged by this migration — keep the
					// existing iOS compatibility renderers so combinations behave as they do today.
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));

					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Controls.Window, WindowHandlerStub>();
					handlers.AddHandler<Entry, EntryHandler>();
				});
			});
		}

		FlyoutPage CreateFlyoutPage([DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, Page detail, Page flyout)
		{
			var flyoutPage = (FlyoutPage)Activator.CreateInstance(type);
			flyoutPage.Detail = detail;
			flyoutPage.Flyout = flyout;
			return flyoutPage;
		}

		bool CanDeviceDoSplitMode(FlyoutPage page)
		{
			return ((IFlyoutPageController)page).ShouldShowSplitMode;
		}

		// The new Handler's platform container is `FlyoutContainerViewController` (internal,
		// visible here via InternalsVisibleTo), replacing the old renderer's own VC as the
		// responder-chain anchor used to detect "is this view hosted inside the FlyoutPage".
		UIView FindPlatformFlyoutView(UIView uiView) =>
			uiView.FindResponder<FlyoutContainerViewController>()?.View;

		async Task CloseFlyout(FlyoutPage flyoutPage)
		{
			flyoutPage.IsPresented = false;

			await Task.Yield();

			bool flyoutHasExpectedBounds()
			{
				if (IsPad)
				{
					// When used on an iPad the flyout overlaps the details
					var flyoutBounds = flyoutPage.Flyout.GetBoundingBox();
					var screenBounds = flyoutPage.GetBoundingBox();
					return
						-flyoutBounds.Width == flyoutBounds.X || //ltr
						screenBounds.Width == flyoutBounds.X;    //rtl
				}
				else
				{
					// When used on an iPhone the details page just covers the flyout
					// When the flyout opens the details page is moved to the right
					var detailsBound = flyoutPage.Detail.GetBoundingBox();
					return 0 == detailsBound.X;
				}
			}

			await AssertEventually(flyoutHasExpectedBounds);
		}

		// =========================================================
		// Tests from FlyoutPageTests.cs / FlyoutPageTests.iOS.cs (9 methods, 15 cases)
		// =========================================================

		[Theory(DisplayName = "[FlyoutViewHandler] Swapping Detail Page Works For Split Flyout Behavior")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutViewHandler_SwappingDetailPageWorksForSplitFlyoutBehavior(Type flyoutPageType)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var flyoutPage = CreateFlyoutPage(
					flyoutPageType,
					new NavigationPage(new ContentPage() { Content = new Border(), Title = "Detail" }),
					new ContentPage() { Title = "Flyout" });

				await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(flyoutPage), async (handler) =>
				{
					var currentDetailPage = flyoutPage.Detail;

					// Set with new page
					var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };
					flyoutPage.Detail = navPage;

					// For NavigationPages, check the CurrentPage instead
					var pageToCheck = navPage is NavigationPage np ? np.CurrentPage : navPage;
					if (!pageToCheck.HasNavigatedTo)
					{
						await OnNavigatedToAsync(navPage);
					}

					// Set back to previous page
					flyoutPage.Detail = currentDetailPage;

					// Check the current page again
					var previousPageToCheck = currentDetailPage is NavigationPage cp ? cp.CurrentPage : currentDetailPage;
					if (!previousPageToCheck.HasNavigatedTo)
					{
						await OnNavigatedToAsync(currentDetailPage);
					}
				});
			});
		}

		[Theory(DisplayName = "[FlyoutViewHandler] FlyoutPage With Toolbar")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutViewHandler_FlyoutPageWithToolbar(Type flyoutPageType)
		{
			SetupBuilder();

			var flyoutPage =
				CreateFlyoutPage(
					flyoutPageType,
					new NavigationPage(new ContentPage() { Title = "Detail" }),
					new ContentPage() { Title = "Flyout" });

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, (handler) =>
			{
				// validate that nothing crashes
				return Task.CompletedTask;
			});
		}

		[Theory(DisplayName = "[FlyoutViewHandler] Details View Updates w/NavigationPage")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutViewHandler_DetailsViewUpdatesWithNavigationPage(Type flyoutPageType)
		{
			SetupBuilder();

			var flyoutPage =
				CreateFlyoutPage(
					flyoutPageType,
					new NavigationPage(new ContentPage() { Title = "Detail" }),
					new ContentPage() { Title = "Flyout" });

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				var details2 = new NavigationPage(new ContentPage() { Title = "Detail" });

				flyoutPage.Detail = details2;
				await OnLoadedAsync(details2.CurrentPage);
				var detailView2 = (details2.CurrentPage.Handler as IPlatformViewHandler)?.PlatformView;
				Assert.NotNull(detailView2);
			});
		}

		[Theory(DisplayName = "[FlyoutViewHandler] Details View Updates"
#if MACCATALYST
			, Skip = "Fails on Mac Catalyst, fixme"
#endif
		)]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutViewHandler_DetailsViewUpdates(Type flyoutPageType)
		{
			SetupBuilder();
			var flyoutPage =
				CreateFlyoutPage(
					flyoutPageType,
					new ContentPage() { Title = "Detail" },
					new ContentPage() { Title = "Flyout" });

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				var flyoutView = flyoutPage.ToPlatform();
				var detailView = flyoutPage.Detail.ToPlatform();
				var dl = FindPlatformFlyoutView(detailView);
				Assert.Equal(flyoutView, dl);

				var details2 = new ContentPage() { Title = "Detail" };
				flyoutPage.Detail = details2;

				await OnLoadedAsync(details2);
				await detailView.OnUnloadedAsync();
				dl = FindPlatformFlyoutView(details2.ToPlatform());
				Assert.Equal(flyoutView, dl);
				Assert.Null(FindPlatformFlyoutView(detailView));
			});
		}

		[Theory(DisplayName = "[FlyoutViewHandler] Details Page Measures Correctly In Split Mode")]
		[InlineData(false
#if MACCATALYST
			, Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
		[InlineData(true)]
		public async Task FlyoutViewHandler_DetailsPageMeasuresCorrectlyInSplitMode(bool isRtl)
		{
			SetupBuilder();
			var flyoutLabel = new Label() { Text = "Content" };
			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage()
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split,
				Detail = new ContentPage()
				{
					Title = "Detail",
					Content = new Label()
				},
				Flyout = new ContentPage()
				{
					Title = "Flyout",
					Content = flyoutLabel
				},
				FlowDirection = (isRtl) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
			});

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				if (!CanDeviceDoSplitMode(flyoutPage))
					return;

				await AssertEventually(() => flyoutPage.Flyout.GetBoundingBox().Width > 0);

				var detailBounds = flyoutPage.Detail.GetBoundingBox();
				var flyoutBounds = flyoutPage.Flyout.GetBoundingBox();
				var windowBounds = flyoutPage.GetBoundingBox();

				Assert.True(detailBounds.Height <= windowBounds.Height, $"Details is measuring too high. Details - {detailBounds} Window - {windowBounds}");
				Assert.True(flyoutBounds.Height <= windowBounds.Height, $"Flyout is measuring too high Flyout - {flyoutBounds} Window - {windowBounds}");
				Assert.True(flyoutBounds.Width + detailBounds.Width <= windowBounds.Width,
					$"Flyout and Details width exceed the width of the window. Details - {detailBounds}  Flyout - {flyoutBounds} Window - {windowBounds}");

				Assert.True(detailBounds.X + detailBounds.Width <= windowBounds.Width,
					$"Right edge of Details View is off the screen. Details - {detailBounds} Window - {windowBounds}");

				if (isRtl)
				{
					Assert.Equal(flyoutBounds.X, detailBounds.Width);
				}
				else
				{
					Assert.Equal(flyoutBounds.Width, detailBounds.X);
				}

				Assert.Equal(detailBounds.Width, windowBounds.Width - flyoutBounds.Width);
			});
		}

		[Fact(DisplayName = "[FlyoutViewHandler] Back Button Enabled Changes with push/pop + page change")]
		public async Task FlyoutViewHandler_BackButtonEnabledChangesWithPushPopAndPageChanges()
		{
			SetupBuilder();

			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split,
				Flyout = new ContentPage() { Title = "Hello world" }
			});

			var first = new NavigationPage(new ContentPage());
			var second = new NavigationPage(new ContentPage());

			flyoutPage.Detail = first;

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				Assert.False(IsBackButtonVisible(handler));

				await first.PushAsync(new ContentPage());
				await AssertEventually(() => IsBackButtonVisible(handler));
				Assert.True(IsBackButtonVisible(handler));

				flyoutPage.Detail = second;
				Assert.False(IsBackButtonVisible(handler));

				await second.PushAsync(new ContentPage());
				await AssertEventually(() => IsBackButtonVisible(handler));
				Assert.True(IsBackButtonVisible(handler));
			});
		}

		[Fact(DisplayName = "[FlyoutViewHandler] FlyoutPage as Modal Does Not Leak")]
		public async Task FlyoutViewHandler_DoesNotLeakAsModal()
		{
			SetupBuilder();

			var references = new List<WeakReference>();
			var launcherPage = new ContentPage();
			var window = new Window(launcherPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				var flyoutPage = new FlyoutPage
				{
					Flyout = new ContentPage
					{
						Title = "Flyout",
						IconImageSource = "icon.png"
					},
					Detail = new ContentPage { Title = "Detail" }
				};

				await launcherPage.Navigation.PushModalAsync(flyoutPage, true);

				references.Add(new WeakReference(flyoutPage));
				references.Add(new WeakReference(flyoutPage.Flyout));
				references.Add(new WeakReference(flyoutPage.Detail));

				await launcherPage.Navigation.PopModalAsync();
			});

			await AssertionExtensions.WaitForGC(references.ToArray());
		}

#if MACCATALYST
		[Fact(DisplayName = "[FlyoutViewHandler] Flyout Page Takes Into Account Safe Area by Default", Skip = "Fails on Mac Catalyst, fixme")]
		public async Task FlyoutViewHandler_FlyoutPageTakesIntoAccountSafeAreaByDefault()
		{
			SetupBuilder();
			var flyoutLabel = new Label() { Text = "Content" };
			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage()
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split,
				Detail = new ContentPage()
				{
					Title = "Detail",
					Content = new Label()
				},
				Flyout = new ContentPage()
				{
					Title = "Flyout",
					Content = flyoutLabel
				}
			});

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				var offset = (float)UIApplication.SharedApplication.GetSafeAreaInsetsForWindow().Top;
				await AssertEventually(() => flyoutLabel.ToPlatform().GetLocationOnScreen().Y > 1);
				var flyoutLocation = flyoutLabel.ToPlatform().GetLocationOnScreen();
				Assert.True(Math.Abs(offset - flyoutLocation.Y) < 1.0);
			});
		}
#endif

		[Theory(DisplayName = "[FlyoutViewHandler] Details View PopOver Layout Is Correct For Idiom")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task FlyoutViewHandler_DetailsViewPopOverLayoutIsCorrectForIdiom(bool isRtl)
		{
			SetupBuilder();
			var flyoutLabel = new Label() { Text = "Content" };
			var flyoutLayout = new VerticalStackLayout() { BackgroundColor = Colors.Blue };
			flyoutLayout.Add(flyoutLabel);

			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage()
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover,
				IsPresented = true,
				Detail = new ContentPage()
				{
					Title = "Detail",
					Content = new Label() { Text = "Detail", BackgroundColor = Colors.Red }
				},
				Flyout = new ContentPage()
				{
					Title = "Flyout",
					Content = flyoutLayout
				},
				FlowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
			});

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				await AssertEventually(() => flyoutPage.Flyout.GetBoundingBox().Width > 0);
				var screenBounds = flyoutPage.GetBoundingBox();
				var detailBounds = flyoutPage.Detail.GetBoundingBox();
				var flyoutBounds = flyoutPage.Flyout.GetBoundingBox();

				// When used on an iPad the flyout overlaps the details
				if (IsPad)
				{
					Assert.Equal(0, detailBounds.X);
				}
				else if (isRtl)
				{
					Assert.Equal(-flyoutBounds.Width, detailBounds.X);
				}
				else
				{
					Assert.Equal(flyoutBounds.Width, detailBounds.X);
				}

				if (isRtl)
				{
					Assert.Equal(screenBounds.Width - flyoutBounds.Width, flyoutBounds.X);
				}
				else
				{
					Assert.Equal(0, flyoutBounds.X);
				}

				await CloseFlyout(flyoutPage);

				var detailBoundsNotPresented = flyoutPage.Detail.GetBoundingBox();
				var flyoutBoundsNotPresented = flyoutPage.Flyout.GetBoundingBox();

				if (IsPad)
				{
					Assert.Equal(detailBoundsNotPresented, detailBounds);

					if (isRtl)
					{
						Assert.Equal(screenBounds.Width, flyoutBoundsNotPresented.X);
					}
					else
					{
						Assert.Equal(-flyoutBoundsNotPresented.Width, flyoutBoundsNotPresented.X);
					}
				}
				else
				{
					Assert.Equal(0, detailBoundsNotPresented.X);
				}
			});
		}

		// =========================================================
		// Tests from ModalTests.cs (2 methods, 24 cases via shared PageTypes)
		// =========================================================

		[Theory(DisplayName = "[FlyoutViewHandler] Swapping Root Page While Modal Page Is Open Doesnt Crash"
#if WINDOWS
			, Skip = "Fails on Windows (Packaged)"
#endif
		)]
		[ClassData(typeof(PageTypes))]
		public async Task FlyoutViewHandler_SwappingRootPageWhileModalPageIsOpenDoesntCrash(Page rootPage, Page newRootPage)
		{
			SetupBuilder();

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (_) =>
				{
					var modalPage = new NavigationPage(new ContentPage());
					await rootPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
					rootPage.Window.Page = newRootPage;
					await OnLoadedAsync(newRootPage);
				});
		}

		[Theory(DisplayName = "[FlyoutViewHandler] Basic Push And Pop")]
		[ClassData(typeof(PageTypes))]
		public async Task FlyoutViewHandler_BasicPushAndPop(Page rootPage, Page modalPage)
		{
			SetupBuilder();

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (_) =>
				{
					var currentPage = rootPage.GetCurrentPage();

					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
					Assert.Single(currentPage.Navigation.ModalStack);
					await currentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);
				});

			Assert.Empty(rootPage.GetCurrentPage().Navigation.ModalStack);
		}

		// Shared data source — mirrors ModalTests.cs's private `PageTypes` class. 6 of the 12
		// combinations involve FlyoutPage (either as root, index i==2, or as the modal page in
		// the last yield of every iteration).
		class PageTypes : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				for (int i = 0; i < 3; i++)
				{
					Func<Page> rootPage;

					if (i == 0)
						rootPage = () => new NavigationPage(new ContentPage());
					else if (i == 1)
						rootPage = () => new Shell() { CurrentItem = new ContentPage() };
					else
						rootPage = () => new FlyoutPage()
						{
							Flyout = new ContentPage() { Title = "Flyout" },
							Detail = new NavigationPage(new ContentPage()) { Title = "Detail" },
						};

					yield return new object[] {
						rootPage(), new NavigationPage(new ContentPage())
					};

					yield return new object[] {
						rootPage(), new ContentPage()
					};

					yield return new object[] {
						rootPage(), new TabbedPage()
						{
							Children =
							{
								new ContentPage(),
								new NavigationPage(new ContentPage())
							}
						}
					};

					yield return new object[] {
						rootPage(), new FlyoutPage()
						{
							Flyout = new ContentPage() { Title = "Flyout" },
							Detail = new ContentPage() { Title = "Detail" },
						}
					};
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		}

		// =========================================================
		// Tests from ToolbarTests.cs (1 method, 5 cases)
		// =========================================================

		[Theory(DisplayName = "[FlyoutViewHandler] Toolbar Updates Correctly When Swapping Main Page With Already Used Page")]
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(ContentPage)}, {nameof(FlyoutPage)}WithNavigationPage"
#if WINDOWS
			, Skip = "Currently Failing on Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(FlyoutPage)}, {nameof(FlyoutPage)}WithNavigationPage"
#if WINDOWS
			, Skip = "Currently Failing on Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(NavigationPage)}, {nameof(FlyoutPage)}WithNavigationPage"
#if WINDOWS
			, Skip = "Currently Failing on Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		[InlineData($"{nameof(Shell)}, {nameof(ContentPage)}, {nameof(Shell)}"
#if WINDOWS
			, Skip = "Currently Failing on  Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		[InlineData($"FlyoutPageWithNavigationPage, NavigationPageWithFlyoutPage, FlyoutPageWithNavigationPage"
#if WINDOWS
			, Skip = "Currently Failing on Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		public async Task FlyoutViewHandler_ToolbarUpdatesCorrectlyWhenSwappingMainPageWithAlreadyUsedPage(string pages)
		{
			string[] pageSet = pages.Split(',');

			SetupBuilder();
			Dictionary<ControlsPageTypesTestCase, Page> createdPages
				= new Dictionary<ControlsPageTypesTestCase, Page>();

			var nextPage = GetPage(pageSet[0]);
			Window window = null!;

			await InvokeOnMainThreadAsync(() =>
			{
				// This reads DisplayInfo, so it needs main thread
				window = new Window(nextPage);
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(window.Page);

				for (int i = 1; i < pageSet.Length; i++)
				{
					nextPage = GetPage(pageSet[i]);
					window.Page = nextPage;

					var currentPage = window.Page;

					currentPage = Controls.Platform.PageExtensions.GetCurrentPage(currentPage);

					await OnLoadedAsync(currentPage);

					var shouldHaveToolbar =
						pageSet[i].Contains("NavigationPage", StringComparison.OrdinalIgnoreCase) ||
						pageSet[i].Contains("Shell", StringComparison.OrdinalIgnoreCase);

					await AssertEventually(() => shouldHaveToolbar == IsNavigationBarVisible(currentPage.Handler));
					Assert.Equal(shouldHaveToolbar, IsNavigationBarVisible(currentPage.Handler));
				}
			});

			Page GetPage(string name)
			{
				var result = (ControlsPageTypesTestCase)Enum.Parse(typeof(ControlsPageTypesTestCase), name);

				if (!createdPages.ContainsKey(result))
					createdPages[result] = ControlsPageTypesTestCases.CreatePageType(result, new ContentPage()
					{
						Title = "Page Title",
						Content = new VerticalStackLayout()
						{
							new Label()
							{
								Text = "FlyoutViewHandler_ToolbarUpdatesCorrectlyWhenSwappingMainPageWithAlreadyUsedPage"
							}
						}
					});

				return createdPages[result];
			}
		}

		// =========================================================
		// Tests from WindowTests.cs (1 method, 4 cases via WindowPageSwapTestCases)
		// =========================================================

		[Theory(DisplayName = "[FlyoutViewHandler] Main Page Swap Tests")]
		[ClassData(typeof(WindowPageSwapTestCases))]
		public async Task FlyoutViewHandler_MainPageSwapTests(WindowPageSwapTestCase swapOrder)
		{
			SetupBuilder();

			var firstRootPage = swapOrder.GetNextPageType();
			var window = new Window(firstRootPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(swapOrder.Page);
				while (!swapOrder.IsFinished())
				{
					var previousRootPage = window.Page?.GetType();
					var nextRootPage = swapOrder.GetNextPageType();
					window.Page = nextRootPage;

					try
					{
						await OnLoadedAsync(swapOrder.Page);
					}
					catch (Exception exc)
					{
						throw new Exception($"Failed to swap to {nextRootPage} from {previousRootPage}", exc);
					}
				}
			});
		}
	}
}
