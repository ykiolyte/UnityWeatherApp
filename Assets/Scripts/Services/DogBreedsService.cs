using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;

public class DogBreedsService
{
    private const string DogBreedsApiUrl = "https://dogapi.dog/api/v2/breeds";
    private readonly RequestQueue _requestQueue;

    public DogBreedsService(RequestQueue requestQueue)
    {
        _requestQueue = requestQueue;
    }

    // Получение списка пород
    public async UniTask<List<BreedData>> GetBreedsAsync(CancellationToken ct)
    {
        var tcs = new UniTaskCompletionSource<List<BreedData>>();

        _requestQueue.AddRequest(async (cancellationToken) =>
        {
            UnityWebRequest request = UnityWebRequest.Get(DogBreedsApiUrl);
            try
            {
                await request.SendWebRequest().ToUniTask().AttachExternalCancellation(cancellationToken);
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var json = request.downloadHandler.text;
                    var breedsData = ParseBreedsData(json);
                    tcs.TrySetResult(breedsData);
                }
                else
                {
                    tcs.TrySetException(new Exception(request.error));
                }
            }
            finally
            {
                request.Dispose();
            }
            return request;
        });

        return await tcs.Task;
    }

    // Получение деталей породы по ID
    public async UniTask<string> GetBreedDetailsAsync(string breedId, CancellationToken ct)
    {
        var tcs = new UniTaskCompletionSource<string>();

        _requestQueue.AddRequest(async (cancellationToken) =>
        {
            UnityWebRequest request = UnityWebRequest.Get($"{DogBreedsApiUrl}/{breedId}");
            try
            {
                await request.SendWebRequest().ToUniTask().AttachExternalCancellation(cancellationToken);
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var json = request.downloadHandler.text;
                    var breedDetails = ParseBreedDetails(json);
                    tcs.TrySetResult(breedDetails);
                }
                else
                {
                    tcs.TrySetException(new Exception(request.error));
                }
            }
            finally
            {
                request.Dispose();
            }
            return request;
        });

        return await tcs.Task;
    }

    // Парсинг списка пород
    private List<BreedData> ParseBreedsData(string json)
    {
        Debug.Log("Breeds API Response: " + json);
        var breedsData = JsonUtility.FromJson<BreedsResponse>(json);
        var breeds = new List<BreedData>();
        if (breedsData != null && breedsData.data.Length > 0)
        {
            breeds.AddRange(breedsData.data);
        }
        else
        {
            Debug.LogError("No breeds data available.");
        }
        return breeds;
    }

    // Парсинг деталей породы
    private string ParseBreedDetails(string json)
    {
        Debug.Log("Breed Details API Response: " + json);
        var breedDetails = JsonUtility.FromJson<BreedDetailsResponse>(json);
        if (breedDetails != null && breedDetails.data != null)
        {
            return $"{breedDetails.data.attributes.name}\n{breedDetails.data.attributes.description}";
        }
        else
        {
            Debug.LogError("No breed details available.");
            return "No details available.";
        }
    }

    // Отмена всех запросов
    public void CancelRequests()
    {
        _requestQueue.CancelAllRequests();
    }

    // Классы для парсинга JSON
    [Serializable]
    private class BreedsResponse
    {
        public BreedData[] data;
    }

    [Serializable]
    public class BreedAttributes
    {
        public string name;
        public string description;
    }

    [Serializable]
    private class BreedDetailsResponse
    {
        public BreedDetailsData data;
    }

    [Serializable]
    private class BreedDetailsData
    {
        public string id;
        public BreedAttributes attributes;
    }
}