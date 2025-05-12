using NUnit.Framework;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui28711 : ContentPage
{
	public Maui28711() => InitializeComponent();

	public Maui28711(bool useCompiledXaml) { }

	[TestFixture]
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Test]
		public void NamedBrushDoesNotCrash([Values] bool useCompiledXaml)
		{
			Assert.DoesNotThrow(() => new Maui28711(useCompiledXaml));
		}
	}
}