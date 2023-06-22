using Dev4Agriculture.ISO11783.ISOXML.LinkListFile;

namespace Dev4Agriculture.ISO11783.ISOXML.DTO
{

    public class ISOLinkEntry
    {
        public string Id { get; set; }
        public ISOLinkGroupType type { get; set; }
        public string Designator { get; set; }

    }
}
