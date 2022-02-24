using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Plugin;

namespace Toonsplat;

sealed class Plugin : IDalamudPlugin {
	public string Name => "Toonsplat";
	internal static GameGui GameGui;
	internal static ObjectTable ObjectTable;
	internal static ClientState ClientState;

	public Plugin(
		DalamudPluginInterface pluginInterface,
		GameGui gameGui,
		ObjectTable objectTable,
		ClientState clientState
	) {
		GameGui = gameGui;
		ObjectTable = objectTable;
		ClientState = clientState;
		(pluginInterface.GetPluginConfig() as Configuration ?? new Configuration()).Initialize(pluginInterface);
		pluginInterface.UiBuilder.Draw += Settings.Draw;
		pluginInterface.UiBuilder.Draw += Canvas.Draw;
		pluginInterface.UiBuilder.OpenConfigUi += static () => Settings.isOpen = true;
	}

	public void Dispose() {
	}
}