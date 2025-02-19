using UnityEngine;

public class AndroidPopupMessage : MonoBehaviour
{
    public static void ShowPopupMessage(string message)
    {
#if UNITY_ANDROID
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaClass alertDialogClass = new AndroidJavaClass("android.app.AlertDialog");
            AndroidJavaObject alertDialogBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity);

            alertDialogBuilder.Call<AndroidJavaObject>("setMessage", message)
                            .Call<AndroidJavaObject>("setPositiveButton", "OK", null)
                            .Call<AndroidJavaObject>("show");
        }));
#endif
    }
}