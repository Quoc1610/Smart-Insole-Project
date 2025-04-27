using UnityEngine;
using Android;
using UnityEngine.Android;

public class BluetoothPermissionHandler : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID
        RequestBluetoothPermissions();
#endif
    }

    void RequestBluetoothPermissions()
    {
        // Fine Location (needed for BLE scanning on Android <12)
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        // BLE Scan Permission
        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"))
        {
            Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
        }

        // BLE Connect Permission
        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {
            Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
        }
    }
}