using System.Collections.Generic;
using System.Linq;

static class Foo
{
	public static IEnumerable<Item> Where1(IQueryable<Item> items) => items.AsEnumerable().Where(Filter);
	public static IEnumerable<Tag> Select1(IQueryable<Item> items) => items.AsEnumerable().Select(Project);
	public static IEnumerable<Item> OrderBy1(IQueryable<Item> items) => items.AsEnumerable().OrderBy(KeyAccessor);

	static bool Filter(Item x) => x.Name.StartsWith("Foo");
	static Tag Project(Item x) => x.Tag;
	static string KeyAccessor(Item x) => x.Name;
}

sealed class Item
{
	public string Name { get; set; }
	public Tag Tag { get; set; }
}

sealed class Tag
{
	public string Name { get; set; }
}
