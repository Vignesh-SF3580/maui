using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class FlyoutPage
    {
        // Track the flyout page we're subscribed to for property changes
        static WeakReference<Page>? _subscribedFlyout;

        // ═══════════════════════════════════════════════
        // Write-Back Callbacks (IFlyoutContainerDelegate → Controls)
        // ═══════════════════════════════════════════════

        internal static void OnPresentedChangedByGesture(IFlyoutView view, bool isPresented)
        {
            if (view is FlyoutPage fp)
            {
                // Guard: don't write IsPresented=false when ShouldShowSplitMode is active.
                // During rotation (ViewWillTransitionToSize), the orientation hasn't actually
                // changed yet, so ShouldShowSplitMode may still return true. Writing false
                // at this point triggers OnIsPresentedPropertyChanging validation which throws
                // InvalidOperationException. The old renderer had the same guard in UpdatePresented.
                if (!isPresented && ((IFlyoutPageController)fp).ShouldShowSplitMode)
                    return;

                fp.IsPresented = isPresented;
            }
            else
            {
                view.IsPresented = isPresented;
            }
        }

        internal static void OnLayoutBoundsChanged(IFlyoutView view, Rect flyoutBounds, Rect detailBounds)
        {
            if (view is IFlyoutPageController controller)
            {
                controller.FlyoutBounds = flyoutBounds;
                controller.DetailBounds = detailBounds;
            }
        }

        internal static void OnLeftBarButtonNeedsUpdate(IFlyoutView view)
        {
            if (view is not FlyoutPage fp)
                return;

            // Subscribe to Flyout's property changes for icon/title updates
            SubscribeToFlyoutPropertyChanges(fp);

            // Get the detail's ViewController via its handler
            if (fp.Detail?.Handler is not IPlatformViewHandler detailHandler)
                return;

            var detailVC = detailHandler.ViewController;
            if (detailVC is null)
                return;

            // If detail VC is a UINavigationController, use its root VC (same as renderer)
            var targetVC = detailVC is UINavigationController nav
                ? nav.ViewControllers?.FirstOrDefault() ?? detailVC
                : detailVC;

            UpdateFlyoutLeftBarButton(targetVC, fp);
        }

        static void SubscribeToFlyoutPropertyChanges(FlyoutPage flyoutPage)
        {
            var flyout = flyoutPage.Flyout;
            if (flyout is null)
                return;

            // Unsubscribe from previous flyout if different
            if (_subscribedFlyout is not null && _subscribedFlyout.TryGetTarget(out var oldFlyout))
            {
                if (ReferenceEquals(oldFlyout, flyout))
                    return; // Already subscribed to this flyout

                oldFlyout.PropertyChanged -= OnFlyoutPagePropertyChanged;
            }

            flyout.PropertyChanged += OnFlyoutPagePropertyChanged;
            _subscribedFlyout = new WeakReference<Page>(flyout);
        }

        static void OnFlyoutPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Page.IconImageSourceProperty.PropertyName ||
                e.PropertyName == Page.TitleProperty.PropertyName)
            {
                if (sender is Page flyoutPage && flyoutPage.Parent is FlyoutPage fp)
                {
                    OnLeftBarButtonNeedsUpdate(fp);
                }
            }
        }

        static void UpdateFlyoutLeftBarButton(UIViewController targetVC, FlyoutPage flyoutPage)
        {
            if (!flyoutPage.ShouldShowToolbarButton())
            {
                targetVC.NavigationItem.LeftBarButtonItem = null;
                return;
            }

            EventHandler onItemTapped = (sender, e) =>
            {
                flyoutPage.IsPresented = !flyoutPage.IsPresented;
            };

            // Load icon asynchronously (same pattern as renderer)
            var mauiContext = flyoutPage.FindMauiContext();
            if (mauiContext is null)
                return;

            flyoutPage.Flyout.IconImageSource.LoadImage(mauiContext, result =>
            {
                var icon = result?.Value;

                if (icon is not null)
                {
                    // Scale icon to fit nav bar (max 44pt height) — same as renderer
                    var originalSize = icon.Size;
                    if (originalSize.Height > 44)
                    {
                        if (flyoutPage.Flyout.IconImageSource is not FontImageSource fontImageSource ||
                            !fontImageSource.IsSet(FontImageSource.SizeProperty))
                        {
                            icon = icon.ResizeImageSource(originalSize.Width, 44f, originalSize);
                        }
                    }

                    try
                    {
                        targetVC.NavigationItem.LeftBarButtonItem =
                            new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, onItemTapped);
                    }
                    catch (Exception)
                    {
                        // Match renderer: catch potential exception from UIBarButtonItem creation
                    }
                }

                if (icon is null || targetVC.NavigationItem.LeftBarButtonItem is null)
                {
                    // Fallback: use Flyout.Title as text button
                    targetVC.NavigationItem.LeftBarButtonItem =
                        new UIBarButtonItem(flyoutPage.Flyout?.Title ?? string.Empty, UIBarButtonItemStyle.Plain, onItemTapped);
                }
            });
        }

        // ═══════════════════════════════════════════════
        // iOS-Specific Mappers
        // ═══════════════════════════════════════════════

        internal static void MapApplyShadow(IFlyoutViewHandler handler, IFlyoutView view)
        {
            if (handler is FlyoutViewHandler h && h._manager is { } manager && view is BindableObject bo)
            {
                var applyShadow = PlatformConfiguration.iOSSpecific.FlyoutPage.GetApplyShadow(bo);
                manager.UpdateApplyShadow(applyShadow);
            }
        }

        internal static void MapFlowDirection(IFlyoutViewHandler handler, IFlyoutView view)
        {
            if (handler is FlyoutViewHandler h && h._manager is { } manager && view is IView v)
                manager.UpdateFlowDirection(v.FlowDirection);
        }
    }
}
