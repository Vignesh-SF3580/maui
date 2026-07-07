using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class FlyoutPage
    {
        // ═══════════════════════════════════════════════
        // Write-Back Callbacks (IFlyoutContainerDelegate → Controls)
        // ═══════════════════════════════════════════════

        internal static void OnPresentedChangedByGesture(IFlyoutView view, bool isPresented)
        {
            view.IsPresented = isPresented;
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

            // Get the detail's ViewController via its handler
            if (fp.Detail?.Handler is not IPlatformViewHandler detailHandler)
                return;

            var detailVC = detailHandler.ViewController;
            if (detailVC is null)
                return;

            // If detail VC is a UINavigationController, use its top VC
            var targetVC = detailVC is UINavigationController nav
                ? nav.TopViewController ?? detailVC
                : detailVC;

            UpdateFlyoutLeftBarButton(targetVC, fp);
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
                    targetVC.NavigationItem.LeftBarButtonItem =
                        new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, onItemTapped);
                }
                else
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
