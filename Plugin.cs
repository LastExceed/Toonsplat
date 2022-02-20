using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Plugin;

namespace Toonsplat;

sealed class Plugin : IDalamudPlugin {
	public string Name => "Toonsplat";
	internal static GameGui GameGui;
	internal static ObjectTable ObjectTable;

	public Plugin(
		DalamudPluginInterface pluginInterface,
		GameGui gameGui,
		ObjectTable objectTable
	) {
		GameGui = gameGui;
		ObjectTable = objectTable;
		(pluginInterface.GetPluginConfig() as Configuration ?? new Configuration()).Initialize(pluginInterface);
		pluginInterface.UiBuilder.Draw += Settings.Draw;
		pluginInterface.UiBuilder.Draw += Canvas.Draw;
		pluginInterface.UiBuilder.OpenConfigUi += static () => Settings.visible = true;
	}

	public void Dispose() {
	}
}