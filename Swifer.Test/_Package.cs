using System.Collections.Generic;

namespace Swifter.Test
{
    public class _Package
    {
        public string id { get; set; }

        public string version { get; set; }

        public string type { get; set; }

        public Payload[] payloads { get; set; }

        public File[] files { get; set; }

        public int installSize { get; set; }

        public FileAssociation[] fileAssociations { get; set; }

        public ProgId[] progIds { get; set; }

        public Dictionary<string, string> dependencies { get; set; }

        public class Payload
        {
            public string fileName { get; set; }
            public string sha256 { get; set; }
            public int size { get; set; }
            public string url { get; set; }
        }

        public class File
        {
            public string fileName { get; set; }
            public string sha256 { get; set; }
            public bool ngen { get; set; }
        }

        public class FileAssociation
        {
            public string extension { get; set; }
            public string progId { get; set; }
        }

        public class ProgId
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string defaultIconPath { get; set; }
            public int defaultIconPosition { get; set; }
            public bool dde { get; set; }
            public string ddeApplication { get; set; }
            public string ddeTopic { get; set; }
        }
    }
}
