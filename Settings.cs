using System.Collections.Generic;
using ImGuiNET;

namespace Toonsplat;

static class Settings {
	internal static bool visible;

	internal static readonly List<Drawing> drawables = new();
	static Drawing selectedDrawable => drawables[selectedDrawableIndex];
	static int selectedDrawableIndex;

	internal static void Draw() {
		if (!visible) return;

		ImGui.Begin("Test Window", ref visible);

		ImGui.BeginGroup();
		{
			ImGui.SetNextItemWidth(100f);
			var items = drawables.ConvertAll(static drawing => drawing.name).ToArray();
			ImGui.ListBox("", ref selectedDrawableIndex, items, items.Length);
			var addingDrawing = ImGui.Button("+");
			if (addingDrawing) {
				drawables.Add(new Drawing());
				selectedDrawableIndex = drawables.Count - 1;
			}
		}
		ImGui.EndGroup();

		if (drawables.Count == 0) return;

		ImGui.SameLine();
		ImGui.BeginGroup();
		{
			ImGui.InputText("name", ref selectedDrawable.name, 32);
			ImGui.Separator();
			ImGui.Combo("type", ref selectedDrawable.type, Drawing.types, Drawing.types.Length);
			ImGui.DragFloat3("position", ref selectedDrawable.position, 0.1f);
			if (selectedDrawable.type == 0) {
				ImGui.DragFloat3("position2", ref selectedDrawable.position2, 0.1f);
				ImGui.DragFloat("width", ref selectedDrawable.widthOrRadius, 0.1f);
			}
			else {
				ImGui.DragFloat("angle", ref selectedDrawable.angle, 1f, 0f, 360f);
				ImGui.DragFloat("radius", ref selectedDrawable.widthOrRadius, 0.1f);
			}
			ImGui.Text("add hitbox size: ");
			ImGui.SameLine();
			ImGui.Checkbox("own", ref selectedDrawable.addOwnHitbox);
			if (selectedDrawable.hostType == 1) {
				ImGui.SameLine();
				ImGui.Checkbox("host", ref selectedDrawable.addHostHitbox);
			}
			ImGui.Separator();
			ImGui.Combo("relative to", ref selectedDrawable.hostType, Drawing.relators, Drawing.relators.Length);
			if (selectedDrawable.hostType == 1) {
				ImGui.InputText("object filter", ref selectedDrawable.objectFilter, 32);
				ImGui.Checkbox("targetable", ref selectedDrawable.targetableHosts);
				ImGui.SameLine();
				ImGui.Checkbox("untargetable", ref selectedDrawable.untargetableHosts);
				ImGui.Checkbox("visible", ref selectedDrawable.visibleHosts);
				ImGui.SameLine();
				ImGui.Checkbox("invisible", ref selectedDrawable.invisibleHosts);
			}
			ImGui.Separator();
			ImGui.SetNextItemWidth(200f);
			ImGui.ColorPicker4("color", ref selectedDrawable.color);
			ImGui.Checkbox("fill", ref selectedDrawable.fill);
			ImGui.DragFloat("line thickness", ref selectedDrawable.lineThickness, 0.1f);
		}
		ImGui.EndGroup();
		ImGui.End();
	}
}