using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace KidQuiz.Data
{
    public enum ApiErrorKind
    {
        None,
        Network,
        HttpError,
        Timeout,
        ParseError
    }

    public readonly struct ApiResult<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public ApiErrorKind ErrorKind { get; }
        public string ErrorMessage { get; }

        private ApiResult(bool isSuccess, T value, ApiErrorKind errorKind, string errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorKind = errorKind;
            ErrorMessage = errorMessage;
        }

        public static ApiResult<T> Success(T value)
        {
            return new ApiResult<T>(true, value, ApiErrorKind.None, null);
        }

        public static ApiResult<T> Failure(ApiErrorKind kind, string message)
        {
            return new ApiResult<T>(false, default, kind, message);
        }
    }

    // Never throws - every failure path comes back as a failed ApiResult.
    // Awaits UnityWebRequest via TaskCompletionSource, no polling loops.
    public sealed class ApiClient
    {
        private const int TimeoutSeconds = 6;

        public async Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct)
        {
            return await SendWithRetryAsync<T>(() => UnityWebRequest.Get(url), ct, expectBody: true);
        }

        public async Task<bool> PutAsync(string url, object body, CancellationToken ct)
        {
            string json;
            try
            {
                json = JsonConvert.SerializeObject(body);
            }
            catch (JsonException)
            {
                return false;
            }

            var result = await SendWithRetryAsync<object>(() =>
            {
                var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT);
                byte[] payload = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(payload);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                return request;
            }, ct, expectBody: false);

            return result.IsSuccess;
        }

        public async Task<bool> DeleteAsync(string url, CancellationToken ct)
        {
            var result = await SendWithRetryAsync<object>(() => UnityWebRequest.Delete(url), ct, expectBody: false);
            return result.IsSuccess;
        }

        // Only retries on Network errors, which typically fail fast (bad DNS, refused
        // connection). Retrying a Timeout would mean waiting through two full timeouts
        // before falling back to offline questions - not worth it for how rarely a
        // retried request succeeds where the first one didn't.
        private async Task<ApiResult<T>> SendWithRetryAsync<T>(Func<UnityWebRequest> requestFactory, CancellationToken ct, bool expectBody)
        {
            var result = await SendOnceAsync<T>(requestFactory, ct, expectBody);

            if (!result.IsSuccess && result.ErrorKind == ApiErrorKind.Network && !ct.IsCancellationRequested)
            {
                result = await SendOnceAsync<T>(requestFactory, ct, expectBody);
            }

            return result;
        }

        private async Task<ApiResult<T>> SendOnceAsync<T>(Func<UnityWebRequest> requestFactory, CancellationToken ct, bool expectBody)
        {
            using UnityWebRequest request = requestFactory();
            request.timeout = TimeoutSeconds;

            var tcs = new TaskCompletionSource<bool>();
            using var registration = ct.Register(() => tcs.TrySetCanceled());

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ => tcs.TrySetResult(true);

            try
            {
                await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                request.Abort();
                return ApiResult<T>.Failure(ApiErrorKind.Network, "Request cancelled.");
            }

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                bool isTimeout = request.error != null &&
                                  request.error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0;
                return ApiResult<T>.Failure(isTimeout ? ApiErrorKind.Timeout : ApiErrorKind.Network, request.error);
            }

            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                return ApiResult<T>.Failure(ApiErrorKind.HttpError, $"{request.responseCode}: {request.error}");
            }

            if (!expectBody)
            {
                return ApiResult<T>.Success(default);
            }

            try
            {
                T value = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                return ApiResult<T>.Success(value);
            }
            catch (JsonException ex)
            {
                return ApiResult<T>.Failure(ApiErrorKind.ParseError, ex.Message);
            }
        }
    }
}
