using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;

namespace System.IO.Compression
{
    public class TarArchive : IAsyncDisposable, IDisposable
    {
        
        public object Header { get; set; }

        private readonly PipeReader pipeReader;

        private readonly Stream stream;
        private readonly StreamReader streamReader;
        
        public TarArchive(Stream stream, bool leaveOpen = false)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            var streamPipeReaderOptions = new StreamPipeReaderOptions(leaveOpen: leaveOpen);
            this.pipeReader = PipeReader.Create(this.stream, streamPipeReaderOptions);
            this.streamReader = new StreamReader(stream, Encoding.ASCII, leaveOpen);
        }
        
        public void Dispose()
        {
            this.streamReader.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            this.Dispose();
            await ValueTask.CompletedTask;
        }

        public async ValueTask ReadHeaderAsync(CancellationToken cancellationToken = default)
        {
            var tarArchiveHeader = new TarArchiveHeader();

            {
                var readResult = await this.pipeReader.ReadAsync(cancellationToken);
                var readOnlySequence = readResult.Buffer.Slice(0, 100);
                tarArchiveHeader.Name = Encoding.ASCII.GetString(readOnlySequence).TrimEnd('\0');
                this.pipeReader.AdvanceTo(readOnlySequence.End);
            }

            {
                var readResult = await this.pipeReader.ReadAsync(cancellationToken);
                var readOnlySequence = readResult.Buffer.Slice(0, 8);
                tarArchiveHeader.Mode = Encoding.ASCII.GetString(readOnlySequence).TrimEnd('\0');
                this.pipeReader.AdvanceTo(readOnlySequence.End);
            }
            
            {
                var readResult = await this.pipeReader.ReadAsync(cancellationToken);
                var readOnlySequence = readResult.Buffer.Slice(0, 8);
                tarArchiveHeader.UserId = Encoding.ASCII.GetString(readOnlySequence).TrimEnd('\0');
                this.pipeReader.AdvanceTo(readOnlySequence.End);
            }
            
            {
                var readResult = await this.pipeReader.ReadAsync(cancellationToken);
                var readOnlySequence = readResult.Buffer.Slice(0, 8);
                tarArchiveHeader.GroupId = Encoding.ASCII.GetString(readOnlySequence).TrimEnd('\0');
                this.pipeReader.AdvanceTo(readOnlySequence.End);
            }
            
            {
                var readResult = await this.pipeReader.ReadAsync(cancellationToken);
                var readOnlySequence = readResult.Buffer.Slice(0, 12);
                var sizeString = Encoding.ASCII.GetString(readOnlySequence).TrimEnd('\0');
                tarArchiveHeader.Size = ulong.Parse(sizeString);
                this.pipeReader.AdvanceTo(readOnlySequence.End);
            }
            
            {
                var readResult = await this.pipeReader.ReadAsync(cancellationToken);
                var readOnlySequence = readResult.Buffer.Slice(0, 12);
                var lastModifiedTimeString = Encoding.ASCII.GetString(readOnlySequence).TrimEnd('\0');
                var lastModifiedTimeLong = Convert.ToInt64(lastModifiedTimeString, 8);
                tarArchiveHeader.LastModifiedTime = DateTimeOffset.FromUnixTimeSeconds(lastModifiedTimeLong);
                this.pipeReader.AdvanceTo(readOnlySequence.End);
            }
            
            {
                var readResult = await this.pipeReader.ReadAsync(cancellationToken);
                var readOnlySequence = readResult.Buffer.Slice(0, 8);
                var checksumString = Encoding.ASCII.GetString(readOnlySequence).TrimEnd('\0');
                tarArchiveHeader.Checksum = uint.Parse(checksumString);
                this.pipeReader.AdvanceTo(readOnlySequence.End);
            }
            

            using var memoryOwner = MemoryPool<char>.Shared.Rent(512);
            var memory = memoryOwner.Memory.Slice(0, 512);
            var charsRead = await this.streamReader.ReadBlockAsync(memory, cancellationToken);

            var fileNameMemory = memory.Slice(0, 100).TrimEnd('\0');

            var fileModeMemory = memory.Slice(100, 8).TrimEnd('\0');
            var fileMode = new string(fileModeMemory.Span);
            
            var ownerIdMemory = memory.Slice(108, 8).TrimEnd('\0');
            var ownerId = new string(ownerIdMemory.Span);
            
            var groupIdMemory = memory.Slice(116, 8).TrimEnd('\0');
            var groupId = new string(ownerIdMemory.Span);
            
            var fileSizeMemory = memory.Slice(124, 12).TrimEnd('\0');
            var fileSize = new string(fileSizeMemory.Span);
            
            var lastModificationMemory = memory.Slice(136, 12).TrimEnd('\0');
            var lastModification = new string(lastModificationMemory.Span);
            
            var checksumMemory = memory.Slice(148, 8).TrimEnd('\0');
            var checksum = new string(checksumMemory.Span);
            
            var typeFlagMemory = memory.Slice(156, 1).TrimEnd('\0');
            var typeFlag = new string(typeFlagMemory.Span);
            
            var linkFileNameMemory = memory.Slice(157, 100).TrimEnd('\0');
            var linkFileName = new string(linkFileNameMemory.Span);
            
            var uStarMemory = memory.Slice(257, 6).TrimEnd('\0');
            var uStar = new string(uStarMemory.Span);
            
            var uStarVersionMemory = memory.Slice(263, 2).TrimEnd('\0');
            var uStarVersion = new string(uStarVersionMemory.Span);
            
            var ownerUserNameMemory = memory.Slice(265, 32).TrimEnd('\0');
            var ownerUserName = new string(ownerUserNameMemory.Span);
            
            var ownerGroupNameMemory = memory.Slice(297, 32).TrimEnd('\0');
            var ownerGroupName = new string(ownerGroupNameMemory.Span);
            
            var deviceMajorNumberMemory = memory.Slice(329, 8).TrimEnd('\0');
            var deviceMajorNumber = new string(deviceMajorNumberMemory.Span);
            
            var deviceMinorNumberMemory = memory.Slice(337, 8).TrimEnd('\0');
            var deviceMinorNumber = new string(deviceMinorNumberMemory.Span);
            
            var fileNamePrefixMemory = memory.Slice(345, 155).TrimEnd('\0');
            var fileNamePrefix = new string(fileNamePrefixMemory.Span);
        }

    }
}