using System.Numerics;

namespace Toonsplat;

internal sealed class Drawing {
	internal string name = "";

	internal int type;
	internal Vector3 position;
	internal Vector3 position2;
	internal float angle = 90f;
	internal float widthOrRadius = 2f;
	internal bool addOwnHitbox;
	internal bool addHostHitbox;

	internal int hostType;
	internal string objectFilter = "";
	internal bool targetableHosts = true;
	internal bool untargetableHosts = true;
	internal bool visibleHosts = true;
	internal bool invisibleHosts = true;

	internal Vector4 color = Vector4.One;
	internal bool fill = true;
	internal float lineThickness = 2f;

	internal static readonly string[] types = { "rectangle/line", "cone/circle" };
	internal static readonly string[] relators = { "world", "object(s)" };
}