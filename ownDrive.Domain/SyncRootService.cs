using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.CldApi;

namespace ownDrive.Domain
{
	public class SyncRootService
	{
		const int _bufferSize = 1024 * 512; // Buffer size for P/Invoke Call to CFExecute max 1 MB

        private readonly SyncRoot _syncroot;
		private readonly IPlaceholderRepository _placeholderRepository;
		public SyncRootService(SyncRoot syncRoot, IPlaceholderRepository placeholderRepository)
		{
			_syncroot = syncRoot;
			_placeholderRepository = placeholderRepository;
		}

		public void ConnectSyncRootTransferCallbacks()
		{
            CF_CALLBACK_REGISTRATION[] callbackTable = [
				new() {
					Callback = new CF_CALLBACK(OnFetchData),
					Type = CF_CALLBACK_TYPE.CF_CALLBACK_TYPE_FETCH_DATA
				},
				new()
				{
					Callback = new CF_CALLBACK(OnFetchPlaceholders),
					Type = CF_CALLBACK_TYPE.CF_CALLBACK_TYPE_FETCH_PLACEHOLDERS
				},
				CF_CALLBACK_REGISTRATION.CF_CALLBACK_REGISTRATION_END
			];

			_syncroot.Connect(callbackTable);
		}

		public void DisconnectSyncRootTransferCallbacks() => _syncroot.Disconnect();

		private void OnFetchData(in CF_CALLBACK_INFO cbInfo, in CF_CALLBACK_PARAMETERS cbParams)
		{
			var opInfo = CreateOperationInfo(cbInfo, CF_OPERATION_TYPE.CF_OPERATION_TYPE_TRANSFER_DATA);
			if (!_placeholderRepository.Connected)
			{
				var b = Array.Empty<byte>();
				TransferData(opInfo, in b, cbParams.FetchData.RequiredFileOffset, cbParams.FetchData.RequiredLength, new NTStatus((uint)NtStatus.STATUS_CLOUD_FILE_NETWORK_UNAVAILABLE));

				return;
			}

			var fdParams = new
			{
				Path = _syncroot.GetRelativaPath(cbInfo.NormalizedPath),
				Offset = cbParams.FetchData.RequiredFileOffset,
				Length = cbParams.FetchData.RequiredLength,
			};

			// ADD CANCELATION TOKEN USAGE

			Task.Run(() => {
				var stream = _placeholderRepository.DownloadFileAsync(fdParams.Path).Result;
				try
				{
					byte[] buffer = new byte[_bufferSize];
					long offset = fdParams.Offset;
					long remainder = fdParams.Length;

					long completed = 0;
					long total = fdParams.Length;

					while (remainder>0)
					{
						int bytesToRead = (remainder > _bufferSize) ? _bufferSize : (int)remainder;
                        int readBytes = stream.Read(buffer, 0, bytesToRead);
                        NTStatus transferStatus = (remainder > _bufferSize) ? NTStatus.STATUS_SUCCESS : NTStatus.STATUS_END_OF_FILE;

						TransferData(opInfo, buffer, offset, readBytes, transferStatus);

						offset += readBytes;
						completed += readBytes;
						remainder -=readBytes;

						CfReportProviderProgress(opInfo.ConnectionKey, opInfo.TransferKey, total, completed);
					}
				}
				catch(Exception ex)
				{
                    throw new NotImplementedException("Exception was thrown but catch block does not inplemented", ex);
                }
				finally {
					stream.Dispose();	
				}
			}/*Put cancellation token here*/);
		}

		private void OnFetchPlaceholders(in CF_CALLBACK_INFO cbInfo, in CF_CALLBACK_PARAMETERS cbParams)
		{
			var opInfo = CreateOperationInfo(cbInfo, CF_OPERATION_TYPE.CF_OPERATION_TYPE_TRANSFER_PLACEHOLDERS);
			if (!_placeholderRepository.Connected)
			{
				var e = Array.Empty<CF_PLACEHOLDER_CREATE_INFO>();
				TransferPlaceholders(opInfo, in e, 0, new NTStatus((uint)NtStatus.STATUS_CLOUD_FILE_NETWORK_UNAVAILABLE));

				return;
            }

            // ADD CANCELATION TOKEN USAGE

            Task.Run(() =>
			{
				throw new NotImplementedException();
            }/*Put cancellation token here*/);
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

		private static void TransferData(CF_OPERATION_INFO opInfo, in byte[] buffer, long offset, long length, NTStatus completionStatus)
		{
			var buf = new SafeNativeArray<byte>(buffer);
			CF_OPERATION_PARAMETERS.TRANSFERDATA tdParams = new()
			{
				Buffer = buf,
				Offset = offset,
				Length = length,
				Flags = CF_OPERATION_TRANSFER_DATA_FLAGS.CF_OPERATION_TRANSFER_DATA_FLAG_NONE, //Required?
				CompletionStatus = completionStatus
			};
			var opParams = CF_OPERATION_PARAMETERS.Create(tdParams);
			CfExecute(opInfo, ref opParams);
		}

		private static void TransferPlaceholders(CF_OPERATION_INFO opInfo, in CF_PLACEHOLDER_CREATE_INFO[] pcInfo, uint count, NTStatus completionStatus)
		{
			var cInfo = new SafeNativeArray<CF_PLACEHOLDER_CREATE_INFO>(pcInfo);
			CF_OPERATION_PARAMETERS.TRANSFERPLACEHOLDERS tpParams = new()
			{
				PlaceholderArray = cInfo,
				PlaceholderCount = count,
				PlaceholderTotalCount = count,
				Flags = CF_OPERATION_TRANSFER_PLACEHOLDERS_FLAGS.CF_OPERATION_TRANSFER_PLACEHOLDERS_FLAG_NONE,
				CompletionStatus = completionStatus,
			};
			var opParams = CF_OPERATION_PARAMETERS.Create(tpParams);
			CfExecute(opInfo, ref opParams);
		}
	}
}