using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace WuHuan.DocRedirect
{
    internal class DocRedirect
    {
        private static string defaultDocRedirectServer;
        private static string userDocRedirectServer;

        private const string k_RedirectNone = "none";

        [InitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            EditorApplication.delayCall += InitRedirect;
        }

        private static void InitRedirect()
        {
            if (!string.IsNullOrEmpty(userDocRedirectServer))
            {
                return;
            }

            defaultDocRedirectServer = String.Empty;
            userDocRedirectServer = EditorPrefs.GetString("DocRedirect.docRedirectionServer", k_RedirectNone);

            var field = typeof(Help).GetField("k_DocRedirectServer", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                var array = (string[])field.GetValue(null);
                if (array != null && array.Length >= 5)
                {
                    defaultDocRedirectServer = array[4];

                    if (!string.IsNullOrEmpty(userDocRedirectServer) && userDocRedirectServer != k_RedirectNone)
                    {
                        array[4] = userDocRedirectServer;
                        field.SetValue(null, array);

                        var property = typeof(Help).GetProperty("baseDocumentationUrl", BindingFlags.NonPublic | BindingFlags.Static);
                        if (property != null)
                        {
                            var baseUrl = (string)property.GetValue(null);
                            if (baseUrl != null)
                            {
                                baseUrl = userDocRedirectServer;
                                property.SetValue(null, baseUrl);
                            }
                        }
                    }
                }
            }
        }

        private static void SetRedirect(string url)
        {
            userDocRedirectServer = url;
            EditorPrefs.SetString("DocRedirect.docRedirectionServer", userDocRedirectServer);

            var field = typeof(Help).GetField("k_DocRedirectServer", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                var array = (string[])field.GetValue(null);
                if (array != null && array.Length >= 5)
                {
                    if (!string.IsNullOrEmpty(userDocRedirectServer) && userDocRedirectServer != k_RedirectNone)
                    {
                        array[4] = userDocRedirectServer;
                        field.SetValue(null, array);

                        var property = typeof(Help).GetProperty("baseDocumentationUrl", BindingFlags.NonPublic | BindingFlags.Static);
                        if (property != null)
                        {
                            var baseUrl = (string)property.GetValue(null);
                            if (baseUrl != null)
                            {
                                baseUrl = userDocRedirectServer;
                                property.SetValue(null, baseUrl);
                            }
                        }
                    }
                }
            }
        }

        private static void ResetRedirect()
        {
            if (!string.IsNullOrEmpty(defaultDocRedirectServer))
            {
                SetRedirect(defaultDocRedirectServer);
            }
        }

        private static void TestRedirect()
        {
            if (!string.IsNullOrEmpty(userDocRedirectServer) && userDocRedirectServer != k_RedirectNone)
            {
                var documentPath = string.Join("/", userDocRedirectServer, "class-RectTransform");
                var version = InternalEditorUtility.GetUnityVersion();
                documentPath += $"?version={version.Major}.{version.Minor}";

                Application.OpenURL(documentPath);
            }
        }

        [SettingsProvider]
        internal static SettingsProvider CreateDocRedirectProvider()
        {
            return new SettingsProvider("Preferences/Doc Redirect", SettingsScope.User)
            {
                guiHandler = OnGUI
            };
        }

        class Styles
        {
            public static GUIContent docRedirectServer = new GUIContent("Doc Redirect Server");
            public static GUIContent testServer = new GUIContent("Open Test");
            public static GUIContent resetServer = new GUIContent("Reset");
        }

        internal static void OnGUI(string searchContext)
        {
            InitRedirect();

            string server = userDocRedirectServer;
            if (!string.IsNullOrEmpty(server) && server != k_RedirectNone)
            {
            }
            else
            {
                server = defaultDocRedirectServer;
            }

            EditorGUI.BeginChangeCheck();
            server = EditorGUILayout.TextField(Styles.docRedirectServer, server);
            if (EditorGUI.EndChangeCheck())
            {
                SetRedirect(server);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(Styles.testServer, GUILayout.Width(100)))
            {
                TestRedirect();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Styles.resetServer, GUILayout.Width(60)))
            {
                ResetRedirect();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
