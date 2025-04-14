namespace ADOExport.Models
{
    internal class AreaResponse
    {
        internal int Count { get; set; }
        internal required List<AreaRoot> Value { get; set; }
    }

    internal class AreaRoot
    {
        internal required string Name { get; set; }
        internal required List<Area> Children { get; set; }
    }

    internal class Area
    {
        internal int Id { get; set; }
        internal required string Identifier { get; set; }
        internal required string Name { get; set; }
        internal required string Path { get; set; }
    }
}
