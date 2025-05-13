using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace InternProject.Logging
{
    public class WebGLFileSaver
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void UNITY_SAVE(string content, string name, string MIMEType);

        [DllImport("__Internal")]
        private static extern void UNITY_SAVE_BYTEARRAY(byte[] array, int byteLength, string name, string MIMEType);

        [DllImport("__Internal")]
        private static extern void init();

        [DllImport("__Internal")]
        private static extern bool UNITY_IS_SUPPORTED();

#endif

        static bool hasinit = false;

        public static void SaveFile(string content, string fileName, string MIMEType = "text/plain;charset=utf-8")
        {
            if (!CheckSupportAndInit()) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            UNITY_SAVE(content, fileName, MIMEType);
#endif
        }

        public static void SaveFile(byte[] content, string fileName, string MIMEType = "text/plain;charset=utf-8")
        {
            if (content == null)
            {
                Debug.LogError("null parameter passed for content byte array");
                return;
            }
            if (!CheckSupportAndInit()) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            UNITY_SAVE_BYTEARRAY(content, content.Length, fileName, MIMEType);
#endif
        }

        static bool CheckSupportAndInit()
        {
            if (Application.isEditor)
            {
                Debug.Log("Saving will not work in editor.");
                return false;
            }
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Debug.Log("Saving must be on a WebGL build.");
                return false;
            }

            CheckInit();

            if (!IsSavingSupported())
            {
                Debug.LogWarning("Saving is not supported on this device.");
                return false;
            }
            return true;
        }

        static void CheckInit()
        {
            if (!hasinit)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                init();
                hasinit = true;
#endif
            }
        }

        public static bool IsSavingSupported()
        {
            if (Application.isEditor)
            {
                Debug.Log("Saving will not work in editor.");
                return false;
            }
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Debug.Log("Saving must be on a WebGL build.");
                return false;
            }
            CheckInit();
#if UNITY_WEBGL && !UNITY_EDITOR
            return UNITY_IS_SUPPORTED();
#else
            return false;
#endif
        }
    }
}