namespace ADOExport.Models
{
    internal class AreaResponse
    {
        internal int Count { get; set; }
        internal List<AreaRoot> Value { get; set; }
    }

    internal class AreaRoot
    {
        internal string Name { get; set; }
        internal List<Area> Children { get; set; }
    }

    internal class Area
    {
        internal int Id { get; set; }
        internal string Identifier { get; set; }
        internal string Name { get; set; }
        internal string Path { get; set; }
    }
}
