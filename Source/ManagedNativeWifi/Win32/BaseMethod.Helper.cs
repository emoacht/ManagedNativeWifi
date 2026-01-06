using System;
using System.ComponentModel;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;

namespace ManagedNativeWifi.Win32
{
	internal static partial class BaseMethod
	{
		public static bool ThrowsOnAnyFailure { get; set; }

		private static bool CheckResult(string methodName, uint result, bool throwOnFailure, uint reasonCode = 0)
		{
			if ((WIN32_ERROR)result is WIN32_ERROR.ERROR_SUCCESS)
				return true;

			switch ((WIN32_ERROR)result)
			{
				case WIN32_ERROR.ERROR_INVALID_PARAMETER:
				case WIN32_ERROR.ERROR_INVALID_STATE:
				case WIN32_ERROR.ERROR_NOT_FOUND:
				case WIN32_ERROR.ERROR_NOT_SUPPORTED:
				case WIN32_ERROR.ERROR_SERVICE_NOT_ACTIVE:
				case WIN32_ERROR.ERROR_NDIS_DOT11_AUTO_CONFIG_ENABLED:
				case WIN32_ERROR.ERROR_NDIS_DOT11_MEDIA_IN_USE:
				case WIN32_ERROR.ERROR_NDIS_DOT11_POWER_STATE_INVALID:
				case WIN32_ERROR.ERROR_BUSY:
				case WIN32_ERROR.ERROR_GEN_FAILURE:
					if (throwOnFailure || ThrowsOnAnyFailure)
						goto default;
					return false;

				case WIN32_ERROR.ERROR_ACCESS_DENIED:
					throw new UnauthorizedAccessException(CreateExceptionMessage(methodName, result));

				case WIN32_ERROR.ERROR_NOT_ENOUGH_MEMORY:
					throw new OutOfMemoryException(CreateExceptionMessage(methodName, result)); // To be considered

				case WIN32_ERROR.ERROR_INVALID_HANDLE:
				case WIN32_ERROR.ERROR_ALREADY_EXISTS:
				case WIN32_ERROR.ERROR_BAD_PROFILE:
				case WIN32_ERROR.ERROR_NO_MATCH:
				default:
					throw new Win32Exception((int)result, CreateExceptionMessage(methodName, result, reasonCode));
			}
		}

		private static unsafe string CreateExceptionMessage(string methodName, uint errorCode, uint reasonCode = 0)
		{
			const int bufferSize = 512;
			var buffer = stackalloc char[bufferSize];

			var message = new StringBuilder($"MethodName: {methodName}, ErrorCode: {errorCode}");
			var messageLength = WinApi.FormatMessage(
				FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_SYSTEM,
				null,
				errorCode,
				0x0409, // US (English)
				buffer,
				bufferSize);

			var formattedMessage = new string(buffer, 0, (int)messageLength);

			if (0 < messageLength)
				message.Append($", ErrorMessage: {formattedMessage}");

			if (0 < reasonCode)
			{
				message.Append($", ReasonCode: {reasonCode}");

				for (var i = 0; i < bufferSize; i++)
				{
					buffer[i] = '\0';
				}

				var result = (WIN32_ERROR)WinApi.WlanReasonCodeToString(
					reasonCode,
					bufferSize,
					buffer);

				formattedMessage = new string(buffer).Trim();

				if (result is WIN32_ERROR.ERROR_SUCCESS)
					message.Append($", ReasonMessage: {formattedMessage}");
			}

			return message.ToString();
		}
	}
}