using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.Extensions;
using Vanara.PInvoke;
using static Vanara.PInvoke.CldApi;

namespace ownDrive.Domain
{
	public class Placeholder
	{
		public string RelativePath { get; }
		public nint Identity { get; }
		public uint IdentityLength { get; }
		public string Basename { get; }
		public long Size { get; }
		public FileAttributes Attributes { get; }
		public DateTime ChangeTime { get; }
		public DateTime CreationTime { get; }
		public DateTime LastAccessTime { get; }
		public DateTime LastWriteTime { get; }

		private readonly CF_PLACEHOLDER_STATE _state;

		private protected Placeholder(string relativePath, WIN32_FIND_DATA info)
		{
			if (Path.IsPathRooted(relativePath))
			{
				throw new ArgumentException("Path to file must be relative");
			}

			if (relativePath.EndsWith('\\'))
			{
				relativePath = relativePath[..^1];
			}
			RelativePath = relativePath;

			Identity = Marshal.StringToCoTaskMemUni(RelativePath);
			IdentityLength = (uint)(RelativePath.Length * Marshal.SizeOf(relativePath[0]));
			Basename = info.cFileName;
			Size = (long)info.FileSize;
			Attributes = info.dwFileAttributes;
			ChangeTime = info.ftLastWriteTime.ToDateTime().ToUniversalTime();
			CreationTime = info.ftCreationTime.ToDateTime().ToUniversalTime();
			LastAccessTime = info.ftLastAccessTime.ToDateTime().ToUniversalTime();
			LastWriteTime = info.ftLastWriteTime.ToDateTime().ToUniversalTime();
			_state = CfGetPlaceholderStateFromFindData(info);
		}

		public static void CreatePlaceholder(string root, IList<FileInfo> info)
		{
			List<CF_PLACEHOLDER_CREATE_INFO> createInfo = [];
			foreach (var item in info)
			{
				createInfo.Add(new CF_PLACEHOLDER_CREATE_INFO(item));
			}
			CfCreatePlaceholders(root, [.. createInfo], (uint)info.Count, CF_CREATE_FLAGS.CF_CREATE_FLAG_NONE, out _);
		}
	}
}