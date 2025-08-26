#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RemoteControl
{
    public static class RemoteMenu
    {
        [MenuItem("Window/Remote Control/Toggle Server")]
        public static void Toggle()
        {
            if (RemoteServer.IsRunning) RemoteServer.Stop();
            else RemoteServer.Start();
        }
    }
}
#endif
