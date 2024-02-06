using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PasteNext
{
    [InitializeOnLoad]
    public static class CreatedObjectModifier
    {
        private enum CommandType
        {
            None = 0,
            Paste,
            Duplicate,
        }

        private static CommandType _lastCommand;
        private static List<GameObject> _lastSelectedObjects = new List<GameObject>();
        private static List<(GameObject, int)> _goSiblingIndexBuffer = new List<(GameObject, int)>();
        private static List<GameObject> _goBuffer = new List<GameObject>();

        static CreatedObjectModifier()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
        }

        private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            if (!IsEnableSettingsAny())
            {
                return;
            }

            CollectEventDetails();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!IsEnableSettingsAny())
            {
                return;
            }

            CollectEventDetails();
        }

        private static void CollectEventDetails()
        {
            _lastCommand = CommandType.None;

            var e = Event.current;
            if (e == null)
            {
                return;
            }

            if (e.type != EventType.ExecuteCommand)
            {
                return;
            }

            switch (e.commandName)
            {
                case "Paste":
                    _lastCommand = CommandType.Paste;
                    break;
                case "Duplicate":
                    _lastCommand = CommandType.Duplicate;
                    break;
                default:
                    return;
            }

            ChooseLastObjectEachParents(Selection.gameObjects, _lastSelectedObjects);
        }

        private static void ChooseLastObjectEachParents(GameObject[] gameObjects, List<GameObject> result)
        {
            result.Clear();
            if (gameObjects == null || gameObjects.Length == 0)
            {
                return;
            }

            _goSiblingIndexBuffer.Clear();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                var go = gameObjects[i];
                var parent = go.transform.parent;
                var siblingIndex = go.transform.GetSiblingIndex();
                var foundItem = false;
                for (int j = 0; j < _goSiblingIndexBuffer.Count; j++)
                {
                    var goSiblingIndex = _goSiblingIndexBuffer[j];
                    if (goSiblingIndex.Item1.transform.parent == parent)
                    {
                        foundItem = true;
                        if (goSiblingIndex.Item2 < siblingIndex)
                        {
                            _goSiblingIndexBuffer[j] = (go, siblingIndex);
                        }
                    }
                }

                if (!foundItem)
                {
                    _goSiblingIndexBuffer.Add((go, siblingIndex));
                }
            }

            for (int i = 0; i < _goSiblingIndexBuffer.Count; i++)
            {
                result.Add(_goSiblingIndexBuffer[i].Item1);
            }
        }

        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            if (_lastCommand == CommandType.None)
            {
                return;
            }

            if (!IsEnableSettingsAny())
            {
                return;
            }

            var settings = PasteNextSettings.instance;
            GetCreatedGameObjects(stream, _goBuffer);
            var gameObjectCount = _goBuffer.Count;
            for (int i = gameObjectCount - 1; i >= 0; i--) // Reverse for sibling index order.
            {
                var go = _goBuffer[i];

                GameObject baseObject = null;
                for (int j = 0; j < _lastSelectedObjects.Count; j++)
                {
                    if (_lastSelectedObjects[j].transform.parent == go.transform.parent)
                    {
                        baseObject = _lastSelectedObjects[j];
                        break;
                    }
                }

                if (baseObject != null)
                {
                    if ((_lastCommand == CommandType.Paste && settings.EnableOnPaste) || (_lastCommand == CommandType.Duplicate && settings.EnableOnDuplicate))
                    {
                        var targetSiblingIndex = baseObject.transform.GetSiblingIndex() + 1;
                        go.transform.SetSiblingIndex(targetSiblingIndex);
                    }
                }

                if (settings.RemoveNameBrackets)
                {
                    var regex = new Regex(@"\s*\(\d+\)$");
                    go.name = regex.Replace(go.name, "");
                }
            }

            _lastCommand = CommandType.None;
        }

        private static void GetCreatedGameObjects(ObjectChangeEventStream stream, List<GameObject> results)
        {
            results.Clear();
            var length = stream.length;
            for (int i = 0; i < length; i++)
            {
                if (stream.GetEventType(i) == ObjectChangeKind.CreateGameObjectHierarchy)
                {
                    stream.GetCreateGameObjectHierarchyEvent(i, out var args);
                    var gameObject = EditorUtility.InstanceIDToObject(args.instanceId) as GameObject;
                    if (gameObject != null)
                    {
                        results.Add(gameObject);
                    }
                }
            }
        }

        private static bool IsEnableSettingsAny()
        {
            var settings = PasteNextSettings.instance;
            return settings.EnableOnPaste || settings.EnableOnDuplicate || settings.RemoveNameBrackets;
        }
    }
}
