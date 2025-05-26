using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LicenseValidator : MonoBehaviour
{
    public GameObject mensajeUI;
    public GameObject invalidLicenseUI;
    private string endpoint = "https://azo3cfdjoi.execute-api.eu-north-1.amazonaws.com/validar";

    void Start()
    {
        StartCoroutine(ValidarLicencia());
    }

    IEnumerator ValidarLicencia()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("🆔 Mi deviceId es: " + deviceId);

        string jsonBody = JsonUtility.ToJson(new DeviceIdWrapper { deviceId = deviceId });

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("📩 Respuesta: " + response);

            if (response.Contains("true"))
            {
                Debug.Log("✅ Licencia valida. Acceso concedido.");
                mensajeUI.SetActive(true);
                yield return new WaitForSeconds(1f);
                mensajeUI.SetActive(false);
            }
            else
            {
                Debug.LogWarning("❌ Licencia no valida. Cerrando el juego.");
                if (invalidLicenseUI != null)
                {
                    invalidLicenseUI.SetActive(true);
                }
                yield return new WaitForSeconds(3f);
                Application.Quit();

                // Esto es solo para asegurarse en editor
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
        else
        {
            Debug.LogError("❌ Error en la validación: " + request.error);
        }
    }

    [System.Serializable]
    public class DeviceIdWrapper
    {
        public string deviceId;
    }
}
