using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Security.Cryptography;

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
        string clave = ObtenerClave();
        string firma = CalcularHMAC(deviceId, clave);

        Debug.Log("🆔 DeviceID: " + deviceId);
        Debug.Log("🔐 Firma: " + firma);

        string jsonBody = JsonUtility.ToJson(new PeticionLicencia
        {
            deviceId = deviceId,
            firma = firma
        });

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("📩 Respuesta: " + response);

            if (response.Contains("\"valid\": true"))
            {
                Debug.Log("✅ Licencia válida. Acceso concedido.");
                mensajeUI?.SetActive(true);
                yield return new WaitForSeconds(1f);
                mensajeUI?.SetActive(false);
            }
            else
            {
                Debug.LogWarning("❌ Licencia no válida. Cerrando el juego.");
                invalidLicenseUI?.SetActive(true);
                yield return new WaitForSeconds(3f);
                Application.Quit();
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

    static string[] clavePartes = { "3xA!", "zFh9", "2@7#", "LmPn", "2025", "_sec", "ure_", "key" };
    static string ObtenerClave() => string.Join("", clavePartes);

    static string CalcularHMAC(string mensaje, string clave)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clave)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(mensaje));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    [System.Serializable]
    public class PeticionLicencia
    {
        public string deviceId;
        public string firma;
    }
}
