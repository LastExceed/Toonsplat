using System.Numerics;
using Dalamud.Logging;
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

		var localPlayer = Plugin.ClientState.LocalPlayer;
		if (localPlayer == null) return;

		var target = localPlayer.TargetObject!;

		foreach (var drawable in ConfigUi.drawables) {
			var hosts =
				drawable.hostTypeIndex == 1
					? Plugin.ObjectTable
						.Where(gameObject => gameObject.Name.ToString().Contains(drawable.objectFilter) && gameObject != localPlayer && gameObject.TargetObject != localPlayer)
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
				if ((hostPosition - localPlayer.Position).Length() > 20f) continue; //todo: allow configuring this

				var atleastPartiallyOnScreen = false;

				var finalWidthOrRadius = drawable.widthOrRadius;
				if (drawable.addOwnHitbox) ; //finalWidthOrRadius += ???
				if (drawable.addHostHitbox) finalWidthOrRadius += hostHitboxSize;

				var originInWorld = hostPosition + drawable.position.Rotate(hostRotation);
				var isOriginOnScreen = Plugin.GameGui.WorldToScreen(originInWorld, out var originOnScreen);
				atleastPartiallyOnScreen |= isOriginOnScreen;

				if (drawable.shapeIndex == 0) { // rect / line
					var endInWorld = hostPosition + drawable.position2.Rotate(hostRotation);

					if (finalWidthOrRadius == 0) { //line
						atleastPartiallyOnScreen |= Plugin.GameGui.WorldToScreen(endInWorld, out var endOnScreen);

						if (!atleastPartiallyOnScreen) continue;

						drawList.AddLine(
							originOnScreen,
							endOnScreen,
							ImGui.ColorConvertFloat4ToU32(drawable.color),
							drawable.lineThickness
						);
					} else { //quad
						atleastPartiallyOnScreen |= Plugin.GameGui.WorldToScreen(
							originInWorld + new Vector3(-finalWidthOrRadius / 2, 0, 0).Rotate(hostRotation),
							out var p1OnScreen
						);
						atleastPartiallyOnScreen |= Plugin.GameGui.WorldToScreen(
							originInWorld + new Vector3(finalWidthOrRadius / 2, 0, 0).Rotate(hostRotation),
							out var p2OnScreen
						);
						atleastPartiallyOnScreen |= Plugin.GameGui.WorldToScreen(
							endInWorld + new Vector3(finalWidthOrRadius / 2, 0, 0).Rotate(hostRotation),
							out var p3OnScreen
						);
						atleastPartiallyOnScreen |= Plugin.GameGui.WorldToScreen(
							endInWorld + new Vector3(-finalWidthOrRadius / 2, 0, 0).Rotate(hostRotation),
							out var p4OnScreen
						);

						if (!atleastPartiallyOnScreen) continue;

						if (drawable.fill)
							drawList.AddQuadFilled(
								p1OnScreen,
								p2OnScreen,
								p3OnScreen,
								p4OnScreen,
								ImGui.ColorConvertFloat4ToU32(drawable.color)
							);
						else
							drawList.AddQuad(
								p1OnScreen,
								p2OnScreen,
								p3OnScreen,
								p4OnScreen,
								ImGui.ColorConvertFloat4ToU32(drawable.color),
								drawable.lineThickness
							);
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

						var isVertexOnScreen = Plugin.GameGui.WorldToScreen(originInWorld + vertexInWorld, out var vertexOnScreen);
						atleastPartiallyOnScreen |= isVertexOnScreen;

						if (isVertexOnScreen) vertices.Add(vertexOnScreen);
					}

					if (!atleastPartiallyOnScreen) continue;

					if (drawable.angle == 360f)
						vertices.RemoveAt(vertices.Count - 1);
					else if (isOriginOnScreen)
						vertices.Insert(0, originOnScreen);

					if (drawable.fill)
						drawList.AddConvexPolyFilled(
							ref vertices.ToArray()[0],
							vertices.Count,
							ImGui.ColorConvertFloat4ToU32(drawable.color)
						);
					else
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

	private static bool IsOnScreenIllegally(this Vector2 self, bool allegedlyOnScreen) {
		var canvasSize = ImGui.GetIO().DisplaySize; //todo: to computed property
		var isInBounds = self.X > 0 && self.X < canvasSize.X && self.Y > 0 && self.Y < canvasSize.Y;
		return !allegedlyOnScreen && isInBounds;
	}
}