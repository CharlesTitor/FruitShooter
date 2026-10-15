using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 40f;
    [SerializeField] private float _disappearingTime = 3f;
    [SerializeField] private float _damage = 25f;

    void Start() //si no choca con nadie en 3 segundos se destruye
    {
        Destroy(gameObject, _disappearingTime);
    }

    void Update() //mueve la bala para adelante
    {
        transform.position+= transform.forward*_speed*Time.deltaTime;
    }

    void OnTriggerEnter(Collider objectOfCollision) //revisa si ya choco con alguien
    {
        Health targetHealth =objectOfCollision.GetComponentInParent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }

}