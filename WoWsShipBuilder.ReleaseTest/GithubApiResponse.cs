namespace WoWsShipBuilder.ReleaseTest
{
    public class GithubApiResponse
    {
        public GithubApiResponse(long id, string name, bool prerelease, bool draft)
        {
            Id = id;
            Name = name;
            Prerelease = prerelease;
            Draft = draft;
        }

        public long Id { get; }

        public string Name { get; }

        public bool Prerelease { get; }

        public bool Draft { get; }
    }
}
