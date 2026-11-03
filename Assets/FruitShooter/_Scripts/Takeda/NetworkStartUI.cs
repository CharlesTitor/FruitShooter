using UnityEngine;
using Unity.Netcode;

public class NetworkStartUI : MonoBehaviour
{
    private void OnGUI()
    {
        float w = 200f, h = 40f, x = 10f, y = 10f, indentLength = 10f;
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(x, y, w, h), "Host")) NetworkManager.Singleton.StartHost();
            if (GUI.Button(new Rect(x, y + h + indentLength, w, h), "Client")) NetworkManager.Singleton.StartClient();
            if (GUI.Button(new Rect(x, y + 2 * (h + indentLength), w, h), "Server")) NetworkManager.Singleton.StartServer();
        }
    }
}
