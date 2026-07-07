using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

/// <summary>
/// A minimal UIViewController that forwards lifecycle calls to <see cref="FlyoutContainerManager"/>.
/// The handler sets this as its ViewController so UIKit lifecycle events reach the manager.
/// </summary>
internal class FlyoutContainerViewController : UIViewController
{
    readonly WeakReference<FlyoutContainerManager> _managerRef;

    internal FlyoutContainerViewController(FlyoutContainerManager manager)
    {
        _managerRef = new WeakReference<FlyoutContainerManager>(manager);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Set background so status bar area doesn't show black/clear behind the safe area offset
        View!.BackgroundColor = UIColor.SystemBackground;

        if (_managerRef.TryGetTarget(out var manager))
            manager.SetupContainerViews(this);
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);

        if (_managerRef.TryGetTarget(out var manager))
            manager.OnViewDidAppear();
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);

        if (_managerRef.TryGetTarget(out var manager))
            manager.OnViewWillDisappear();
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();

        if (_managerRef.TryGetTarget(out var manager))
            manager.OnParentViewDidLayoutSubviews();
    }

    public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
    {
        base.ViewWillTransitionToSize(toSize, coordinator);

        if (_managerRef.TryGetTarget(out var manager))
            manager.OnParentViewWillTransitionToSize(toSize);
    }

    public override UIViewController? ChildViewControllerForStatusBarHidden()
    {
        return ChildViewControllers?.Length > 0
            ? ChildViewControllers[^1]
            : base.ChildViewControllerForStatusBarHidden();
    }

    public override UIViewController? ChildViewControllerForStatusBarStyle()
    {
        return ChildViewControllers?.Length > 0
            ? ChildViewControllers[^1]
            : base.ChildViewControllerForStatusBarStyle();
    }

    public override UIViewController? ChildViewControllerForHomeIndicatorAutoHidden
    {
        get => ChildViewControllers?.Length > 0
            ? ChildViewControllers[^1]
            : base.ChildViewControllerForHomeIndicatorAutoHidden;
    }
}
