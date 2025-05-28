const API_KEY = "ira_q11t7De9AEbraMJiKFGUg68t2lNNGL2ouVu7"; // IPRegistry
const LAMBDA_URL = "https://v3cfc9fokf.execute-api.eu-north-1.amazonaws.com/ValidateRegisterLicense";

let userIP = "";
let country = "";
let isVPN = false;

document.addEventListener("DOMContentLoaded", async () => {
  const status = document.getElementById("status");

  const ipInfo = await fetch(`https://api.ipregistry.co/?key=${API_KEY}`);
  const data = await ipInfo.json();

  userIP = data.ip;
  country = data.location.country.name;
  isVPN = data.security.is_vpn || data.security.is_proxy;

  if (isVPN) {
    status.textContent = "No se permite registrar licencias desde VPN o proxy.";
    document.getElementById("registerBtn").disabled = true;
    return;
  }

  status.textContent = `Tu IP: ${userIP}, País: ${country}`;
});

document.getElementById("registerBtn").addEventListener("click", async () => {
  const deviceId = prompt("Introduce tu Device ID:");

  if (!deviceId) return;

  const clave = "3xA!zFh92@7#LmPn2025_secure_key";
  const firma = await hmacSHA256(clave, deviceId);

  const resp = await fetch(LAMBDA_URL, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ deviceId, firma })
  });

  const json = await resp.json();
  alert(json.reason);
});

async function hmacSHA256(key, msg) {
  const enc = new TextEncoder();
  const keyData = await crypto.subtle.importKey(
    "raw", enc.encode(key), { name: "HMAC", hash: "SHA-256" }, false, ["sign"]
  );
  const sig = await crypto.subtle.sign("HMAC", keyData, enc.encode(msg));
  return Array.from(new Uint8Array(sig)).map(b => b.toString(16).padStart(2, '0')).join('');
}
