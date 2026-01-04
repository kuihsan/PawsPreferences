using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CatService : MonoBehaviour
{

    public IEnumerator LoadCat(System.Action<Texture2D> callback)
    {

        string url =
            $"https://cataas.com/cat?width=512&height=512&format=png&ts={System.Guid.NewGuid()}";

        UnityWebRequest req = UnityWebRequestTexture.GetTexture(
            url,
            true 
        );

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Cat load failed: {req.error}\nURL: {url}");
            callback?.Invoke(null);
            yield break;
        }

        Texture2D tex = DownloadHandlerTexture.GetContent(req);
        callback?.Invoke(tex);
    }
}
