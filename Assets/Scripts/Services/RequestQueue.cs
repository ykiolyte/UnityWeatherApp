using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RequestQueue : IDisposable
{
    private Queue<Func<CancellationToken, UniTask<UnityWebRequest>>> _requestQueue = new Queue<Func<CancellationToken, UniTask<UnityWebRequest>>>();
    private bool _isProcessing = false;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public void AddRequest(Func<CancellationToken, UniTask<UnityWebRequest>> requestFunc)
    {
        _requestQueue.Enqueue(requestFunc);
        if (!_isProcessing)
        {
            ProcessQueue().Forget();
        }
    }

    private async UniTaskVoid ProcessQueue()
    {
        _isProcessing = true;

        while (_requestQueue.Count > 0)
        {
            var requestFunc = _requestQueue.Dequeue();

            try
            {
                var result = await requestFunc(_cancellationTokenSource.Token);
                HandleResponse(result);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Request was canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Request failed: {ex.Message}");
            }
        }

        _isProcessing = false;
    }

    private void HandleResponse(UnityWebRequest response)
    {
        if (response.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request succeeded: " + response.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Request failed: " + response.error);
        }
    }

    public void CancelAllRequests()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _requestQueue.Clear();
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}