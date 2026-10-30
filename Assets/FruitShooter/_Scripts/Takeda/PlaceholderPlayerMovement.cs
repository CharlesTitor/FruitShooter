using UnityEngine;
using Unity.Netcode;

public class PlaceholderPlayerMovement : NetworkBehaviour //does not inherit from MonoBehaviour
{
    
    void Start() //potentially replaceable with public override void OnNetworkSpawn()
    {
        if(IsOwner){
            GetComponent<Renderer>().material.color = Color.red; //example of a code to execute only for a player you control
        }
    }

    
    void Update()
    {
        if (!IsOwner) return; //kicks out anyone who isn't the owner of this player and prevents them from accessing the code under this

        float horizontal = Input.GetAxisRaw("Horizontal"); //example of a code for movement, intentionally contains magic number and is to be replaced
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        transform.Translate(direction * 5f * Time.deltaTime);
    }
}
