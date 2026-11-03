using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;

    private float _currentHealth;
    private bool _isDead = false;

    private void Awake()
    {
        _currentHealth= _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (_isDead)
        {
            return;
        }

        _currentHealth -= damage;

        if (_currentHealth < 0f)
        {
            _currentHealth = 0f;
        }
        Debug.Log(gameObject.name +" received "+damage+" damage. Health: "+_currentHealth);

        
        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;

        Debug.Log(gameObject.name + " has been defeated.");
    }
}
