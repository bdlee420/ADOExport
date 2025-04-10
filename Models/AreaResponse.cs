namespace ADOExport.Models
{
    public class AreaResponse
    {
        public int Count { get; set; }
        public List<AreaRoot> Value { get; set; }
    }

    public class AreaRoot
    {
        public string Name { get; set; }
        public List<Area> Children { get; set; }
    }

    public class Area
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
