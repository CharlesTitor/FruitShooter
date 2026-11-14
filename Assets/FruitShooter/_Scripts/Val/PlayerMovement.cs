using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 10f;

    public Joystick joystick;
    public Animator animator;

    private void Update()
    {
        float movementX = joystick.Horizontal;
        float movementZ = joystick.Vertical;
        Vector3 movement = new Vector3(movementX, 0f, movementZ);

        transform.position+= movement*speed*Time.deltaTime;
        if (movement.magnitude > 0.01f)
        {
            //hace que el personaje mire hacia donde camina
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation,targetRotation,rotationSpeed*Time.deltaTime);
            animator.SetBool("isWalking", true);
            Debug.Log("Se esta moviendo");
        }
        else
        {
            animator.SetBool("isWalking", false);
            Debug.Log("No se esta moviendo");
        }
    }

}
