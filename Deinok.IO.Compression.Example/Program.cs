using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.Compression.Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await TarGzAsync();
        }

        public static async Task TarGzAsync(CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient();
            using var httpResponseMessage = await httpClient.GetAsync("https://ftp.gnu.org/gnu/tar/tar-1.32.tar.gz", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            await using var stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
            await using var gZipStream = new GZipStream(stream, CompressionMode.Decompress);
            await using var tarArchive = new TarArchive(gZipStream);
            await tarArchive.ReadHeaderAsync(cancellationToken);
            tarArchive.ToString();
        }
    }
}