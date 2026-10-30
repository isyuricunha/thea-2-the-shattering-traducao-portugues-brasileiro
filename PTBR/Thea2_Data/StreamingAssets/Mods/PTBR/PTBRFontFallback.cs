using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Thea2.Common;

namespace GameScript
{
    public class PTBRFontFallback : ScriptBase
    {
        private static bool refreshScheduled;

        public static string PTBR_GetCurrentPlayerName(object obj)
        {
            TMP_FontAsset fallback = TMP_FontAsset.defaultFontAsset;
            if (fallback != null)
            {
                List<TMP_FontAsset> fonts = TMP_Settings.fallbackFontAssets;
                if (fonts != null && !fonts.Contains(fallback))
                {
                    fonts.Add(fallback);
                    Debug.Log("[PTBR] Liberation Sans registered as global font fallback.");
                }
                TMP_FontAsset[] loaded = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                foreach (TMP_FontAsset font in loaded)
                {
                    if (font == fallback) continue;
                    if (font.fallbackFontAssets == null)
                        font.fallbackFontAssets = new List<TMP_FontAsset>();
                    if (!font.fallbackFontAssets.Contains(fallback))
                        font.fallbackFontAssets.Add(fallback);
                }
            }
            else Debug.LogWarning("[PTBR] Default TMP font unavailable; font fallback was not applied.");
            if (fallback != null && !refreshScheduled)
            {
                refreshScheduled = true;
                GameObject refresh = new GameObject("PTBR Font Refresh");
                refresh.AddComponent<PTBRFontRefresh>();
            }
            return UIScripts.UIS_GetCurentPlayerName(obj);
        }
    }
    public class PTBRFontRefresh : MonoBehaviour
    {
        private void LateUpdate()
        {
            TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
            int refreshed = 0;
            foreach (TMP_Text text in texts)
            {
                if (text == null || !text.gameObject.activeInHierarchy) continue;
                // This TMP version replaces missing characters in its parsed buffer.
                // Restore the original string to force a fresh parse after fallback setup.
                string original = text.text;
                text.text = string.Empty;
                text.text = original;
                text.SetAllDirty();
                text.ForceMeshUpdate();
                refreshed++;
            }
            Debug.Log("[PTBR] Refreshed existing text components: " + refreshed);
            Destroy(gameObject);
        }
    }
}
