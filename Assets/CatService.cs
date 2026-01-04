using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CatService : MonoBehaviour
{
    private const string CAT_URL = "https://cataas.com/cat";

    public IEnumerator LoadCat(System.Action<Texture2D> callback)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(CAT_URL))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                callback(DownloadHandlerTexture.GetContent(req));
            }
            else
            {
                Debug.LogError(req.error);
            }
        }
    }
}
