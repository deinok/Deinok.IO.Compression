namespace System.IO.Compression
{
    public class TarArchiveHeader
    {

        public uint Checksum { get; set; }

        public string GroupId { get; set; }

        public DateTimeOffset LastModifiedTime { get; set; }

        public string Mode { get; set; }
        
        public string Name { get; set; }
        
        public ulong Size { get; set; }

        public string UserId { get; set; }

    }
    
}