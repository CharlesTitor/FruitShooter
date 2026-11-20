/*
Reference: ChatGPT Conversation
Conversation: Adaptar cámara y disparo
Game: Fruit Shooter
Game Type: 3D
Date: 13/11/2026
Purpose: La IA me ayudo a adaptar, con eventos, que al estar presionado el boton se pueda disparar "infinitamente"
https://chatgpt.com/share/6af79f90-4eb0-83e8-b707-d1db2f414168
*/
using System.Collections;
using TMPro;
using UnityEngine;

public class RifleController : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _muzzlePoint;
     [SerializeField] private TMP_Text _ammoText;

    [SerializeField] private float _fireRate=0.2f;
    [SerializeField] private int _magazineSize =30;
    [SerializeField] private float _reloadTime= 1.5f;

    private int _currentAmmo;
    private float _nextFireTime =0f;
    private bool _isReloading=false;    
    private bool isShooting;

    private void Update()
    {
        if (isShooting)
        {
            Shoot();
        }
    }

    private void Start()
    {
        _currentAmmo = _magazineSize;
        UpdateAmmoText();
    }

    public void StartShooting()
    {
        isShooting = true;
    }

    public void StopShooting()
    {
        isShooting = false;
    }
    
    public void Shoot()
    {
        if (_isReloading || Time.time<_nextFireTime)
        {
            return;
        }
        if (_currentAmmo<=0)
        {
            Debug.Log("Magazine empty");
            return;
        }
        _nextFireTime = Time.time + _fireRate;
        _currentAmmo--;

        GameObject newBullet=Instantiate(_bulletPrefab,_muzzlePoint.position,_muzzlePoint.rotation);
        UpdateAmmoText();
    }

    public void Reload()
    {
        if (_isReloading || _currentAmmo == _magazineSize)
        {
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;

        yield return new WaitForSeconds(_reloadTime);

        _currentAmmo = _magazineSize;
        _isReloading = false;

        UpdateAmmoText();

        Debug.Log("Reload complete");
    }

    public void OnShootButtonPressed()
    {
        Shoot();
    }

    public void OnReloadButtonPressed()
    {
        Reload();
    }

    private void UpdateAmmoText()
    {
        if (_ammoText != null)
        {
            _ammoText.text =_currentAmmo + " / " + _magazineSize;
        }
    }
}
