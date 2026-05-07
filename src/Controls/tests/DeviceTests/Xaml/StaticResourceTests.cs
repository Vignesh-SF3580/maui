using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Xaml)]
public class StaticResourceTests : IDisposable
{
    [Fact("Issue #23903 / #35018: Missing ControlTemplate with exception handler should not throw but invoke handler")]
    [RequiresUnreferencedCode("XAML parsing may require unreferenced code")]
    public void MissingControlTemplate_WithExceptionHandler_ShouldNotThrow()
    {
        // Issue #35018 fixed the behavior introduced by #33859:
        // When ExceptionHandler2 is set (Hot Reload / IDE context), a missing StaticResource
        // now reports the error to the handler and returns null instead of throwing.
        // This prevents iOS Hot Reload crashes where exceptions propagate through UIKit
        // lifecycle callbacks during Shell item setup and corrupt app state (#35018).
        // Without a handler (normal launch), exceptions are still thrown as before.

        Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) => { };

        var xaml = """
			<?xml version="1.0" encoding="UTF-8"?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             ControlTemplate="{StaticResource InvalidTemplate}">
			</ContentPage>
			""";

        var page = new ContentPage();

        // Should NOT throw when ExceptionHandler2 is set — error is reported to the handler
        bool exceptionThrown = false;
        try
        {
            page.LoadFromXaml(xaml);
        }
        catch (Exception)
        {
            exceptionThrown = true;
        }

        Assert.False(exceptionThrown, "Expected no exception to be thrown when ExceptionHandler2 is set (Hot Reload context)");
    }

    public void Dispose()
    {
        Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
    }
}