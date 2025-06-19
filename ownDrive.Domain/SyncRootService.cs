using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.CldApi;

namespace ownDrive.Domain
{
	public class SyncRootService
	{
		private readonly SyncRoot _syncroot;
		private readonly IPlaceholderRepository _placeholderRepository;
		public SyncRootService(SyncRoot syncRoot, IPlaceholderRepository placeholderRepository)
		{
			_syncroot = syncRoot;
			_placeholderRepository = placeholderRepository;
		}

		public void ConnectSyncRootTransferCallbacks()
		{
			CF_CALLBACK_REGISTRATION[] callbackTable = {
				new() {
					Callback=new CF_CALLBACK(OnFetchData),
					Type = CF_CALLBACK_TYPE.CF_CALLBACK_TYPE_FETCH_DATA
				},
				CF_CALLBACK_REGISTRATION.CF_CALLBACK_REGISTRATION_END
			};

			_syncroot.Connect(callbackTable);
		}

		public void DisconnectSyncRootTransferCallbacks() => _syncroot.Disconnect();

		private void OnFetchData(in CF_CALLBACK_INFO cbInfo, in CF_CALLBACK_PARAMETERS cbParams)
		{
			var opInfo = CreateOperationInfo(cbInfo, CF_OPERATION_TYPE.CF_OPERATION_TYPE_TRANSFER_DATA);
			if (!_placeholderRepository.Connected)
			{
				var b = Array.Empty<byte>();
				TransferData(opInfo, in b, cbParams.FetchData.RequiredFileOffset, cbParams.FetchData.RequiredLength, NtStatus.STATUS_CLOUD_FILE_NETWORK_UNAVAILABLE);

				return;
			}

			Task.Run(() => { });
			throw new NotImplementedException("OnFetchData not implemented yet");
		}

		private static CF_OPERATION_INFO CreateOperationInfo(CF_CALLBACK_INFO cbInfo, CF_OPERATION_TYPE opType)
		{
			CF_OPERATION_INFO opInfo = new()
			{
				Type = opType,
				ConnectionKey = cbInfo.ConnectionKey,
				TransferKey = cbInfo.TransferKey,
				CorrelationVector = cbInfo.CorrelationVector,
				RequestKey = cbInfo.RequestKey,
			};
			opInfo.StructSize = (uint)Marshal.SizeOf(opInfo);

			return opInfo;
		}

		private static void TransferData(CF_OPERATION_INFO opInfo, in byte[] buffer, long offset, long length, NtStatus completionStatus)
		{
			var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			var ptr = (IntPtr)handle;
			try
			{
				CF_OPERATION_PARAMETERS.TRANSFERDATA tdParams = new()
				{
					Buffer = ptr,
					Offset = offset,
					Length = length,
					Flags = CF_OPERATION_TRANSFER_DATA_FLAGS.CF_OPERATION_TRANSFER_DATA_FLAG_NONE, //Required?
					CompletionStatus = new NTStatus((uint)completionStatus)
				};
				var opParams = CF_OPERATION_PARAMETERS.Create(tdParams);
				CfExecute(opInfo, ref opParams);
			}
			finally
			{
				handle.Free();
			}
		}
	}
}