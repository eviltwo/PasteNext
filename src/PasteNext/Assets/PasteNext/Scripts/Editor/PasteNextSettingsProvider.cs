using System.Collections.Generic;
using UnityEditor;

namespace PasteNext
{
    public class PasteNextSettingsProvider : SettingsProvider
    {
        private const string SettingPath = "Project/PasteNext";
        private static readonly string[] Keywords = { };

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new PasteNextSettingsProvider(SettingPath, SettingsScope.Project, Keywords);
        }

        public PasteNextSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var settings = PasteNextSettings.instance;
                settings.EnableOnPaste = EditorGUILayout.Toggle("Enable On Paste", settings.EnableOnPaste);
                settings.EnableOnDuplicate = EditorGUILayout.Toggle("Enable On Duplicate", settings.EnableOnDuplicate);
                settings.RemoveNameBrackets = EditorGUILayout.Toggle("Remove Name Brackets", settings.RemoveNameBrackets);
                if (check.changed)
                {
                    settings.Save();
                }
            }
        }
    }
}
