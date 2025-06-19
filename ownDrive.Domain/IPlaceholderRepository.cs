using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ownDrive.Domain
{
	public interface IPlaceholderRepository
	{
		public bool Connected { get; }

		public Task Connect();
		public Task Disconnect();
		public Task<Stream> DownloadFileAsync(string path, CancellationToken token = default);
		public Task GetFileListAsync(string subDir, CancellationToken token = default);
		public Task RemoveAsync(string path, CancellationToken token = default);
		public Task UploadFileAsync(string path, CancellationToken token);
	}
}