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
			var hosts =
				drawable.hostType == 1
					? Plugin.ObjectTable
						.Where(gameObject => gameObject.Name.ToString().Contains(drawable.objectFilter))
						.ToList()
						.ConvertAll(static gameObject => (gameObject.Position, gameObject.HitboxRadius, gameObject.Rotation))
					: new List<(Vector3, float, float)> { (Vector3.Zero, 0f, 0f) };

			foreach (var (hostPosition, hostHitboxSize, hostRotation) in hosts) {
				var finalWidthOrRadius = drawable.widthOrRadius;
				if (drawable.addOwnHitbox) ; //finalWidthOrRadius +=
				if (drawable.addHostHitbox) finalWidthOrRadius += hostHitboxSize;

				var originInWorld = hostPosition + drawable.position;
				Plugin.GameGui.WorldToScreen(originInWorld, out var originOnScreen);

				if (drawable.type == 0) { // rect / line
					var endInWorld = hostPosition + drawable.position2;

					if (finalWidthOrRadius == 0) { //line
						Plugin.GameGui.WorldToScreen(endInWorld, out var endOnScreen);

						drawList.AddLine(
							originOnScreen,
							endOnScreen,
							ImGui.ColorConvertFloat4ToU32(drawable.color),
							drawable.lineThickness
						);
					} else { //quad
						Plugin.GameGui.WorldToScreen(originInWorld + new Vector3(-finalWidthOrRadius / 2, 0, 0), out var p1OnScreen);
						Plugin.GameGui.WorldToScreen(originInWorld + new Vector3(finalWidthOrRadius / 2, 0, 0), out var p2OnScreen);
						Plugin.GameGui.WorldToScreen(endInWorld + new Vector3(finalWidthOrRadius / 2, 0, 0), out var p3OnScreen);
						Plugin.GameGui.WorldToScreen(endInWorld + new Vector3(-finalWidthOrRadius / 2, 0, 0), out var p4OnScreen);

						drawList.AddQuadFilled(
							p1OnScreen,
							p2OnScreen,
							p3OnScreen,
							p4OnScreen,
							ImGui.ColorConvertFloat4ToU32(drawable.color)
						);
					}
				} else { // cone / circle
					var totalRadians = drawable.angle / 180f * Math.PI;
					var segmentCount = (int)(drawable.angle / 3f);

					var vertices = new List<Vector2>();

					for (var vertexIndex = 0; vertexIndex < segmentCount + 1; vertexIndex++) {
						var vertexRadians = totalRadians / 2 - totalRadians * (vertexIndex / (float)segmentCount);

						var vertexInWorld = new Vector3(
							(float)Math.Sin(vertexRadians),
							0f,
							(float)Math.Cos(vertexRadians)
						) * finalWidthOrRadius;

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

	private static Vector2 RotateAndMapToScreen(Vector3 point, Vector3 pivot, float rotation, bool ignoreRotation) {
		var point2d = new Vector2(point.X, point.Z);
		var pivot2d = new Vector2(pivot.X, pivot.Z);

		point2d -= pivot2d; // temporarily translate point to origin:

		var sin = (float)Math.Sin(rotation);
		var cos = (float)Math.Cos(rotation);
		point2d.X = point2d.X * cos - point2d.Y * sin;
		point2d.Y = point2d.X * sin + point2d.Y * cos;

		// translate point back:
		point2d += pivot2d;

		Plugin.GameGui.WorldToScreen(new Vector3(point2d.X, point.Y, pivot2d.Y), out var pointOnScreen);
		return pointOnScreen;
	}
}