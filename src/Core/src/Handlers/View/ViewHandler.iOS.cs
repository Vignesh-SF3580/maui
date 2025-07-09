using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		readonly static ConditionalWeakTable<PlatformView, ViewHandler> FocusHandlerMapping = new();
		readonly static ConditionalWeakTable<PlatformView, UITapGestureRecognizer> FocusGestureMapping = new();
		
		// Global focus manager to track currently focused view
		static WeakReference<IView>? _currentlyFocusedView;

		// Centralized focus management method
		internal static void SetViewFocused(IView view)
		{
			// First, unfocus the currently focused view (if any)
			if (_currentlyFocusedView?.TryGetTarget(out var currentlyFocused) == true && currentlyFocused != view)
			{
				currentlyFocused.IsFocused = false;
			}
			
			// Then focus the new view
			view.IsFocused = true;
			_currentlyFocusedView = new WeakReference<IView>(view);
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		public static void MapContextFlyout(IViewHandler handler, IView view)
		{
#if MACCATALYST
			if (view is IContextFlyoutElement contextFlyoutContainer)
			{
				MapContextFlyout(handler, contextFlyoutContainer);
			}
#endif
		}

#if MACCATALYST
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		internal static void MapContextFlyout(IElementHandler handler, IContextFlyoutElement contextFlyoutContainer)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"The handler's {nameof(handler.MauiContext)} cannot be null.");

			if (handler.PlatformView is PlatformView uiView)
			{
				MauiUIContextMenuInteraction? currentInteraction = null;

				foreach (var interaction in uiView.Interactions)
				{
					if (interaction is MauiUIContextMenuInteraction menuInteraction)
						currentInteraction = menuInteraction;
				}

				if (contextFlyoutContainer.ContextFlyout != null)
				{
					if (currentInteraction == null)
						uiView.AddInteraction(new MauiUIContextMenuInteraction(handler));
				}
				else if (currentInteraction != null)
				{
					uiView.RemoveInteraction(currentInteraction);
				}
			}
		}
#endif

		partial void ConnectingHandler(PlatformView? platformView)
		{
			if (platformView is not null)
			{
				// Add focus handling for generic views like StackLayout that don't have specific handlers
				// Skip text input controls and button controls as they handle focus in their own handlers
				if (ShouldAddFocusGestureRecognizer(platformView))
				{
					FocusHandlerMapping.Add(platformView, this);
					SetupFocusGestureRecognizer(platformView);
				}
			}
		}

		partial void DisconnectingHandler(PlatformView platformView)
		{
			CleanupFocusGestureRecognizer(platformView);
			FocusHandlerMapping.Remove(platformView);
		}

		static bool ShouldAddFocusGestureRecognizer(PlatformView platformView)
		{
			// Don't add gesture recognizer to controls that handle focus in their own handlers
			return platformView is not (UITextField or UITextView or UIButton);
		}

		static void SetupFocusGestureRecognizer(PlatformView platformView)
		{
			var tapGesture = new UITapGestureRecognizer(HandleViewTapped);
			tapGesture.ShouldRecognizeSimultaneously = (recognizer, otherRecognizer) => true;
			platformView.AddGestureRecognizer(tapGesture);
			platformView.UserInteractionEnabled = true;
			
			FocusGestureMapping.Add(platformView, tapGesture);
		}

		static void CleanupFocusGestureRecognizer(PlatformView platformView)
		{
			if (FocusGestureMapping.TryGetValue(platformView, out var gestureRecognizer))
			{
				platformView.RemoveGestureRecognizer(gestureRecognizer);
				FocusGestureMapping.Remove(platformView);
				gestureRecognizer?.Dispose();
			}
		}

		static void HandleViewTapped(UITapGestureRecognizer tapGesture)
		{
			if (tapGesture.View is PlatformView platformView && 
				FocusHandlerMapping.TryGetValue(platformView, out ViewHandler? handler))
			{
				// Handle focus event following Editor/Entry pattern with proper focus management
				if (handler.VirtualView is IView view)
					SetViewFocused(view);
			}
		}

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		internal static void UpdateTransformation(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}
	}
}