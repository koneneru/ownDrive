using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.CldApi;

namespace ownDrive.Domain
{
	public class SyncRoot
	{
		private readonly Dictionary<string, Placeholder> _placeholders;

		public string Id { get; }
		public string Name { get; }
		public string Path { get; }
		public string Provider { get; }
		public string Token { get; }
		public CF_CONNECTION_KEY ConnectionKey { get; private set; }

		public IReadOnlyCollection<KeyValuePair<string, Placeholder>> Placeholders => _placeholders;

		internal SyncRoot(string id, string name, string path, string provider, string token, IEnumerable<Placeholder> files)
		{
			Id = id;
			Name = name;
			Path = path;
			Provider = provider;
			Token = token;
			_placeholders = BuildPlaceholdersDictionary(files);
		}

		internal bool Connect(CF_CALLBACK_REGISTRATION[] callbackTable)
		{
			CF_CONNECTION_KEY key;
			try
			{
				CfConnectSyncRoot(
					Path,
					callbackTable,
					IntPtr.Zero,
					CF_CONNECT_FLAGS.CF_CONNECT_FLAG_REQUIRE_PROCESS_INFO |
					CF_CONNECT_FLAGS.CF_CONNECT_FLAG_REQUIRE_FULL_FILE_PATH,
					out key).ThrowIfFailed();
			}
			catch (Exception ex)
			{
				throw new NotImplementedException("Exception was thrown but catch block does not inplemented", ex);
			}

			ConnectionKey = key;
			return true;
		}

		internal bool Disconnect()
		{
			try
			{
				CfDisconnectSyncRoot(ConnectionKey).ThrowIfFailed();
			}
			catch (Exception ex)
			{
				throw new NotImplementedException("Exception was thrown but catch block does not inplemented", ex);
			}

			return true;
		}

		private static Dictionary<string, Placeholder> BuildPlaceholdersDictionary(IEnumerable<Placeholder> files)
		{
			Dictionary<string, Placeholder> d = [];
			foreach (var file in files)
			{
				d.Add(file.RelativePath, file);
			}
			return d;
		}
	}
}