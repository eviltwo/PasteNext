using UnityEditor;

namespace PasteNext
{
    [FilePath("ProjectSettings/PasteNextSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class PasteNextSettings : ScriptableSingleton<PasteNextSettings>
    {
        public bool EnableOnPaste = true;
        public bool EnableOnDuplicate = true;
        public bool RemoveNameBrackets = false;

        public void Save()
        {
            Save(true);
        }
    }
}

