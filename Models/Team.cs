namespace ADOExport.Models
{
    public class TeamResponse
    {
        public List<Team> Value { get; set; }
    }

    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
