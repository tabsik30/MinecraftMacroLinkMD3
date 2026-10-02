using MacroDeck.Plugin.Hosting;
using MacroDeck.Plugin.Testing;
using MacroDeck.Ui.Dsl;
using MacroDeck.Ui.Model.Serialization;
using MacroDeck.Ui.Model.Surfaces;
using MacroDeck.Ui.Runtime;
using MacroLink.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;

namespace MacroLink.Tests;

/// <summary>
/// Behaviour tests through <see cref="PluginTestHarness"/>: the plugin's own capability handlers run,
/// but nothing crosses a socket. This is where you test what your integration does.
/// </summary>
[TestFixture]
public sealed class PluginIntegrationTests
{
	[Test]
	public void Xp_progress_widget_uses_a_green_read_only_range_bar()
	{
		var progress = new UiState<double>(0.42);
		var surface = new UiSurface { Kind = UiSurfaceKinds.Widget, SessionMode = UiSessionModes.Shared };
		using var view = new UiView(surface, PluginIntegration.BuildXpProgressWidget(progress));
		var initialTree = UiCanonicalJson.Serialize(view.Tree);

		Assert.That(initialTree, Does.Contain("\"type\":\"ui.range-bar\""));
		Assert.That(initialTree, Does.Contain("#35C759"));
		Assert.That(initialTree, Does.Not.Contain("\"type\":\"ui.slider\""));

		progress.Value = 0.75;
		var updatedTree = UiCanonicalJson.Serialize(view.Tree);
		Assert.That(updatedTree, Does.Contain("75%"));
	}

	[Test]
	public async Task The_plugin_builds_and_initializes()
	{
		await using var harness = PluginTestHarness.Create(builder =>
		{
			builder.Services.AddSingleton(_ => new MinecraftLinkService(new LoggerConfiguration().CreateLogger()));
			builder.RegisterIntegration<PluginIntegration>();
		});

		Assert.DoesNotThrowAsync(harness.InitializeIntegrationsAsync);
	}
}
