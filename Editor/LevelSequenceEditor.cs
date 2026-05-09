using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AweDev.LevelSequence.Editor
{
    [CustomEditor(typeof(LevelSequence))]
    public class LevelSequenceEditor : UnityEditor.Editor
    {
        private const float RowHeight = 28f;
        private const float GripWidth = 24f;
        private const float IndexWidth = 34f;

        private SerializedProperty _buildStartLevelProperty;
        private SerializedProperty _levelsProperty;
        private ReorderableList _levelsList;

        private void OnEnable()
        {
            _buildStartLevelProperty = serializedObject.FindProperty("_buildStartLevel");
            _levelsProperty = serializedObject.FindProperty("_levels");

            _levelsList = new ReorderableList(serializedObject, _levelsProperty, true, true, true, true)
            {
                elementHeight = RowHeight,
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                drawElementBackgroundCallback = DrawElementBackground
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawBuildSettingsSection();
            EditorGUILayout.Space(4f);
            DrawSummary();
            EditorGUILayout.Space(8f);

            _levelsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSummary()
        {
            int nullCount = 0;
            int missingDisplayNameCount = 0;
            int missingSceneNameCount = 0;
            LevelSequence levelSequence = target as LevelSequence;
            List<DuplicateLevelIdGroup> duplicateGroups = LevelSequenceValidator.FindDuplicateLevelIds(levelSequence);

            for (int i = 0; i < _levelsProperty.arraySize; i++)
            {
                LevelDefinition level = GetLevel(i);

                if (level == null)
                {
                    nullCount++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(level.displayName)) missingDisplayNameCount++;
                if (string.IsNullOrWhiteSpace(level.sceneName)) missingSceneNameCount++;
            }

            EditorGUILayout.LabelField("Level Sequence", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Entries", _levelsProperty.arraySize.ToString());

            if (nullCount == 0 && duplicateGroups.Count == 0 && missingDisplayNameCount == 0 && missingSceneNameCount == 0)
            {
                EditorGUILayout.HelpBox("Sequence looks healthy.", MessageType.Info);
                return;
            }

            List<string> warnings = new();
            if (nullCount > 0) warnings.Add($"{nullCount} null entries");
            if (duplicateGroups.Count > 0) warnings.Add($"{duplicateGroups.Count} duplicate level IDs");
            if (missingDisplayNameCount > 0) warnings.Add($"{missingDisplayNameCount} missing display names");
            if (missingSceneNameCount > 0) warnings.Add($"{missingSceneNameCount} missing scene names");

            EditorGUILayout.HelpBox(string.Join(", ", warnings), MessageType.Warning);

            if (duplicateGroups.Count > 0)
            {
                EditorGUILayout.HelpBox("Duplicate level IDs will block player builds only when package build blocking is enabled.", MessageType.Error);

                foreach (DuplicateLevelIdGroup duplicateGroup in duplicateGroups)
                {
                    EditorGUILayout.LabelField(LevelSequenceValidator.FormatDuplicateGroup(duplicateGroup), EditorStyles.wordWrappedMiniLabel);
                }
            }
        }

        private void DrawBuildSettingsSection()
        {
            LevelSequence levelSequence = target as LevelSequence;
            LevelSequenceBuildSettingsReport report = LevelSequenceBuildSettingsSync.BuildReport(levelSequence);

            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_buildStartLevelProperty);
            EditorGUILayout.LabelField("Effective Build Start Level", GetEffectiveBuildStartLevelLabel(levelSequence), EditorStyles.miniLabel);

            EditorGUILayout.HelpBox(
                report.isSynced
                    ? "All Level Sequence scenes are enabled in Build Settings."
                    : LevelSequenceBuildSettingsSync.FormatSummary(report),
                report.isSynced ? MessageType.Info : MessageType.Warning);

            using (new EditorGUI.DisabledScope(!report.hasFixableIssues))
            {
                if (GUILayout.Button("Sync Build Settings"))
                {
                    LevelSequenceBuildSettingsSyncResult result = LevelSequenceBuildSettingsSync.Sync(report);
                    Debug.Log($"[LevelSequenceEditor] {LevelSequenceBuildSettingsSync.FormatSyncResult(result)}", target);
                }
            }
        }

        private string GetEffectiveBuildStartLevelLabel(LevelSequence levelSequence)
        {
            if (levelSequence == null) return "None";

            LevelDefinition effectiveStartLevel = levelSequence.GetEffectiveBuildStartLevel();
            if (effectiveStartLevel == null) return "None";

            string source = levelSequence.buildStartLevel != null ? "override" : "first sequence entry";
            return $"{GetLevelLabel(effectiveStartLevel, -1)} ({source})";
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, $"Levels ({_levelsProperty.arraySize})");
        }

        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Event.current.type != EventType.Repaint) return;

            Color backgroundColor = index % 2 == 0
                ? new Color(0.20f, 0.20f, 0.20f, 0.25f)
                : new Color(0.15f, 0.15f, 0.15f, 0.25f);

            if (isActive)
            {
                backgroundColor = new Color(0.25f, 0.45f, 0.85f, 0.35f);
            }

            EditorGUI.DrawRect(rect, backgroundColor);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _levelsProperty.GetArrayElementAtIndex(index);
            LevelDefinition level = GetLevel(index);

            Rect rowRect = new(rect.x, rect.y + 3f, rect.width, EditorGUIUtility.singleLineHeight);
            Rect gripRect = new(rowRect.x, rowRect.y, GripWidth, rowRect.height);
            EditorGUI.LabelField(gripRect, "::", EditorStyles.centeredGreyMiniLabel);

            Rect indexRect = new(gripRect.xMax, rowRect.y, IndexWidth, rowRect.height);
            EditorGUI.LabelField(indexRect, $"{index + 1:00}", EditorStyles.miniLabel);

            Rect objectRect = new(indexRect.xMax, rowRect.y, rowRect.xMax - indexRect.xMax, rowRect.height);
            GUIContent content = new(GetLevelLabel(level, index), GetLevelTooltip(level));
            EditorGUI.PropertyField(objectRect, element, content);
        }

        private LevelDefinition GetLevel(int index)
        {
            if (index < 0 || index >= _levelsProperty.arraySize) return null;

            SerializedProperty element = _levelsProperty.GetArrayElementAtIndex(index);
            return element.objectReferenceValue as LevelDefinition;
        }

        private string GetLevelLabel(LevelDefinition level, int index)
        {
            if (level == null) return $"Missing Level ({index + 1})";

            return !string.IsNullOrWhiteSpace(level.displayName)
                ? level.displayName
                : !string.IsNullOrWhiteSpace(level.levelId)
                    ? level.levelId
                    : level.name;
        }

        private string GetLevelTooltip(LevelDefinition level)
        {
            if (level == null) return "Missing Level Definition reference.";

            return $"Display: {level.displayName}\nID: {level.levelId}\nScene: {level.sceneName}\nAsset: {level.name}";
        }
    }
}
