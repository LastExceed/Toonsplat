using System.Numerics;
using ImGuiNET;

namespace Toonsplat;

static class Canvas {
	internal static void Draw() {
		ImGui.Begin(
			"canvas",
			ImGuiWindowFlags.NoInputs |
			ImGuiWindowFlags.NoNav |
			ImGuiWindowFlags.NoTitleBar |
			ImGuiWindowFlags.NoScrollbar |
			ImGuiWindowFlags.NoBackground |
			ImGuiWindowFlags.AlwaysUseWindowPadding
		);
		ImGui.SetWindowPos(new Vector2(0, 0));
		ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

		var drawList = ImGui.GetWindowDrawList();

		foreach (var drawable in Settings.drawables) {
			var localPlayer = Plugin.ClientState.LocalPlayer;
			var target = localPlayer.TargetObject;

			var hosts =
				drawable.hostTypeIndex == 1
					? Plugin.ObjectTable
						.Where(gameObject => gameObject.Name.ToString().Contains(drawable.objectFilter))
						.ToList()
						.ConvertAll(static gameObject => (gameObject.Position, gameObject.HitboxRadius, gameObject.Rotation))
					: new List<(Vector3, float, float)> {
						drawable.hostTypeIndex switch {
							0 => (Vector3.Zero, 0f, 0f),
							2 => (localPlayer.Position, localPlayer.HitboxRadius, localPlayer.Rotation),
							3 => (target.Position, target.HitboxRadius, target.Rotation),
							_ => throw new ArgumentOutOfRangeException()
						}
					};

			foreach (var (hostPosition, hostHitboxSize, hostRotation) in hosts) {
				var finalWidthOrRadius = drawable.widthOrRadius;
				if (drawable.addOwnHitbox) ; //finalWidthOrRadius +=
				if (drawable.addHostHitbox) finalWidthOrRadius += hostHitboxSize;

				var originInWorld = hostPosition + drawable.position.Rotate(hostRotation);
				Plugin.GameGui.WorldToScreen(originInWorld, out var originOnScreen);

				if (drawable.shapeIndex == 0) { // rect / line
					var endInWorld = hostPosition + drawable.position2.Rotate(hostRotation);

					if (finalWidthOrRadius == 0) { //line
						Plugin.GameGui.WorldToScreen(endInWorld, out var endOnScreen);

						drawList.AddLine(
							originOnScreen,
							endOnScreen,
							ImGui.ColorConvertFloat4ToU32(drawable.color),
							drawable.lineThickness
						);
					} else { //quad
						Plugin.GameGui.WorldToScreen(originInWorld + new Vector3(-finalWidthOrRadius / 2, 0, 0).Rotate(hostRotation), out var p1OnScreen);
						Plugin.GameGui.WorldToScreen(originInWorld + new Vector3(finalWidthOrRadius / 2, 0, 0).Rotate(hostRotation), out var p2OnScreen);
						Plugin.GameGui.WorldToScreen(endInWorld + new Vector3(finalWidthOrRadius / 2, 0, 0).Rotate(hostRotation), out var p3OnScreen); //todo: fix rect width only working in 1 direction
						Plugin.GameGui.WorldToScreen(endInWorld + new Vector3(-finalWidthOrRadius / 2, 0, 0).Rotate(hostRotation), out var p4OnScreen);

						if (drawable.fill) {
							drawList.AddQuadFilled(
								p1OnScreen,
								p2OnScreen,
								p3OnScreen,
								p4OnScreen,
								ImGui.ColorConvertFloat4ToU32(drawable.color)
							);
						} else {
							drawList.AddQuad(
								p1OnScreen,
								p2OnScreen,
								p3OnScreen,
								p4OnScreen,
								ImGui.ColorConvertFloat4ToU32(drawable.color),
								drawable.lineThickness
							);
						}
					}
				} else { // cone / circle
					var totalRadians = drawable.angle / 180f * Math.PI;
					var segmentCount = (int)(drawable.angle / 3f);

					var vertices = new List<Vector2>();

					for (var vertexIndex = 0; vertexIndex < segmentCount + 1; vertexIndex++) {
						var vertexRadians = totalRadians / 2 - totalRadians * (vertexIndex / (float)segmentCount);

						var vertexInWorld = (new Vector3(
							(float)Math.Sin(vertexRadians),
							0f,
							(float)Math.Cos(vertexRadians)
						) * finalWidthOrRadius).Rotate(hostRotation); //todo: format

						Plugin.GameGui.WorldToScreen(originInWorld + vertexInWorld, out var vertexOnScreen);
						vertices.Add(vertexOnScreen);
					}

					if (drawable.angle == 360f) {
						vertices.RemoveAt(vertices.Count - 1);
					} else {
						vertices.Insert(0, originOnScreen);
					}

					if (drawable.fill) {
						drawList.AddConvexPolyFilled(
							ref vertices.ToArray()[0],
							vertices.Count,
							ImGui.ColorConvertFloat4ToU32(drawable.color)
						);
					} else {
						drawList.AddPolyline(
							ref vertices.ToArray()[0],
							vertices.Count,
							ImGui.ColorConvertFloat4ToU32(drawable.color),
							ImDrawFlags.Closed,
							drawable.lineThickness
						);
					}
				}
			}
		}

		ImGui.End();
	}

	private static Vector3 Rotate(this Vector3 point, float angle) {
		var sin = (float)Math.Sin(-angle);
		var cos = (float)Math.Cos(-angle);

		return new Vector3(
			point.X * cos - point.Z * sin,
			point.Y,
			point.Z = point.X * sin + point.Z * cos
		);
	}
}