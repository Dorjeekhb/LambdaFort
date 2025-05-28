using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

public class LicenseRegistrar : MonoBehaviour
{
    private string endpoint = "https://v3cfc9fokf.execute-api.eu-north-1.amazonaws.com/ValidateRegisterLicense";

    void Start()
    {
        StartCoroutine(RegistrarLicencia());
    }

    IEnumerator RegistrarLicencia()
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
            Debug.Log("📩 Respuesta: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ Error al registrar licencia: " + request.error);
        }
    }

    private static string[] clavePartes = {
        "3xA!", "zFh9", "2@7#", "LmPn", "2025", "_sec", "ure_", "key"
    };

    private static string ObtenerClave()
    {
        return string.Join("", clavePartes);
    }

    private static string CalcularHMAC(string mensaje, string clave)
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
