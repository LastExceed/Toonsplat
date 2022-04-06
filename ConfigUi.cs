using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Toonsplat;

static class ConfigUi {
	internal static bool isOpen;

	internal static readonly List<Drawable> drawables = new() {
		new Drawable {
			name = "red edge",
			hostTypeIndex = 2,
			position = new Vector3(0f, 1.5f, 0f),
			position2 = new Vector3(1f, 1f, 5f),
			widthOrRadius = 0f,
			color = new Vector4(255f, 0f, 0f, 255f)
		},
		new Drawable {
			name = "green edge",
			hostTypeIndex = 2,
			position = new Vector3(0f, 1.5f, 0f),
			position2 = new Vector3(-1f, 1f, 5f),
			widthOrRadius = 0f,
			color = new Vector4(0f, 255f, 0f, 255f)
		},
		new Drawable {
			name = "blue edge",
			hostTypeIndex = 2,
			position = new Vector3(0f, 1.5f, 0f),
			position2 = new Vector3(0f, 2f, 5f),
			widthOrRadius = 0f,
			color = new Vector4(0f, 0f, 255f, 255f)
		}
	};
	static Drawable selectedDrawable => drawables[selectedDrawableIndex];
	static int selectedDrawableIndex;

	internal static void Draw() {
		if (!isOpen) return;

		ImGui.Begin("Test Window", ref isOpen);

		ImGui.BeginGroup();
		{
			ImGui.SetNextItemWidth(100f);
			var items = drawables.ConvertAll(static drawing => drawing.name).ToArray();
			ImGui.ListBox("", ref selectedDrawableIndex, items, items.Length);
			var addingDrawing = ImGui.Button("+");
			if (addingDrawing) {
				drawables.Add(new Drawable());
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
			ImGui.Combo("shape", ref selectedDrawable.shapeIndex, Drawable.shapeTypes, Drawable.shapeTypes.Length);
			ImGui.Button("here");
			ImGui.SameLine();
			ImGui.DragFloat3("position", ref selectedDrawable.position, 0.1f);
			if (selectedDrawable.shapeIndex == 0) {
				ImGui.Button("here");
				ImGui.SameLine();
				ImGui.DragFloat3("position2", ref selectedDrawable.position2, 0.1f);
				ImGui.DragFloat("width", ref selectedDrawable.widthOrRadius, 0.1f, 0f, 1000f);
			} else {
				ImGui.DragFloat("angle", ref selectedDrawable.angle, 1f, 0f, 360f);
				ImGui.DragFloat("radius", ref selectedDrawable.widthOrRadius, 0.1f, 0f, 1000f);
			}
			ImGui.Text("add hitbox size: ");
			ImGui.SameLine();
			ImGui.Checkbox("own", ref selectedDrawable.addOwnHitbox);
			if (selectedDrawable.hostTypeIndex == 1) {
				ImGui.SameLine();
				ImGui.Checkbox("host", ref selectedDrawable.addHostHitbox);
			}
			ImGui.Separator();
			ImGui.Combo("relative to", ref selectedDrawable.hostTypeIndex, Drawable.hostTypes, Drawable.hostTypes.Length);
			if (selectedDrawable.hostTypeIndex == 1) {
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
			ImGui.DragFloat("line thickness", ref selectedDrawable.lineThickness, 0.1f, 0f, 1000f);
		}
		ImGui.EndGroup();
		ImGui.End();
	}
}