using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AweDev.LevelSequence.Editor
{
    [CustomEditor(typeof(LevelDefinition))]
    public class LevelDefinitionEditor : UnityEditor.Editor
    {
        private const float MaxPreviewHeight = 220f;
        private const float MinPreviewHeight = 80f;
        private const float ClearButtonSize = 20f;
        private const string PngExtension = ".png";

        private SerializedProperty _scriptProperty;
        private SerializedProperty _levelIdProperty;
        private SerializedProperty _displayNameProperty;
        private SerializedProperty _descriptionProperty;
        private SerializedProperty _referenceImageProperty;
        private SerializedProperty _sceneAssetProperty;
        private SerializedProperty _sceneNameProperty;
        private SerializedProperty _scenePathProperty;

        private void OnEnable()
        {
            _scriptProperty = serializedObject.FindProperty("m_Script");
            _levelIdProperty = serializedObject.FindProperty("_levelId");
            _displayNameProperty = serializedObject.FindProperty("_displayName");
            _descriptionProperty = serializedObject.FindProperty("_description");
            _referenceImageProperty = serializedObject.FindProperty("_referenceImage");
            _sceneAssetProperty = serializedObject.FindProperty("_sceneAsset");
            _sceneNameProperty = serializedObject.FindProperty("_sceneName");
            _scenePathProperty = serializedObject.FindProperty("_scenePath");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_scriptProperty);
            }

            EditorGUILayout.PropertyField(_levelIdProperty);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            DrawActiveSequenceLevelIdWarning();

            EditorGUILayout.Space(6f);
            EditorGUILayout.PropertyField(_displayNameProperty);
            EditorGUILayout.PropertyField(_descriptionProperty);

            EditorGUILayout.Space(6f);
            EditorGUILayout.PropertyField(_sceneAssetProperty);
            EditorGUILayout.PropertyField(_sceneNameProperty);
            EditorGUILayout.PropertyField(_scenePathProperty);

            EditorGUILayout.Space(6f);
            DrawReferenceImageSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawActiveSequenceLevelIdWarning()
        {
            if (target is not LevelDefinition levelDefinition) return;
            if (string.IsNullOrWhiteSpace(levelDefinition.levelId)) return;
            if (!LevelSequenceEditorSettings.TryGetActiveLevelSequence(out LevelSequence levelSequence, out string errorMessage))
            {
                EditorGUILayout.HelpBox($"Level ID validation is not fully configured: {errorMessage}", MessageType.Warning);
                return;
            }

            DuplicateLevelIdGroup duplicateGroup = LevelSequenceValidator
                .FindDuplicateLevelIds(levelSequence)
                .FirstOrDefault(group => group.levelId == levelDefinition.levelId && group.levels.Contains(levelDefinition));

            if (duplicateGroup == null) return;

            EditorGUILayout.HelpBox(
                $"levelId '{duplicateGroup.levelId}' is duplicated in the active Level Sequence: {string.Join(", ", duplicateGroup.levels.Select(level => level.name))}.",
                MessageType.Error);
        }

        private void DrawReferenceImageSection()
        {
            EditorGUILayout.LabelField("Reference", EditorStyles.boldLabel);
            Texture2D referenceImage = _referenceImageProperty.objectReferenceValue as Texture2D;
            EditorGUILayout.PropertyField(_referenceImageProperty);
            EditorGUILayout.Space(6f);
            DrawReferenceCaptureControls(referenceImage);
            EditorGUILayout.Space(6f);

            Rect previewRect = GetReferencePreviewRect(referenceImage);
            if (referenceImage == null)
            {
                EditorGUI.DrawRect(previewRect, new Color(0.18f, 0.18f, 0.18f, 0.25f));
                GUIStyle centeredHelpStyle = new(EditorStyles.wordWrappedLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                EditorGUI.LabelField(previewRect, "Assign a reference image to make this level easier to identify.", centeredHelpStyle);
                return;
            }

            EditorGUI.DrawPreviewTexture(previewRect, referenceImage, null, ScaleMode.ScaleToFit);
            DrawClearButton(previewRect);
        }

        private void DrawReferenceCaptureControls(Texture2D referenceImage)
        {
            LevelDefinition levelDefinition = target as LevelDefinition;
            Camera captureCamera = GetGameCaptureCamera();
            string outputFolder = LevelSequenceEditorSettings.referenceImageOutputFolder;

            EditorGUI.BeginChangeCheck();
            string nextOutputFolder = EditorGUILayout.TextField("Capture Folder", outputFolder);
            if (EditorGUI.EndChangeCheck())
            {
                LevelSequenceEditorSettings.referenceImageOutputFolder = nextOutputFolder;
                outputFolder = LevelSequenceEditorSettings.referenceImageOutputFolder;
            }

            string generatedAssetPath = GetGeneratedReferenceImageAssetPath(levelDefinition, outputFolder);
            EditorGUILayout.LabelField("Generated File", string.IsNullOrWhiteSpace(generatedAssetPath) ? "Unavailable" : generatedAssetPath, EditorStyles.miniLabel);

            bool canCapture = GetCaptureReadiness(captureCamera, outputFolder, generatedAssetPath, out string readinessMessage, out MessageType readinessType);

            using (new EditorGUI.DisabledScope(!canCapture))
            {
                if (GUILayout.Button("Capture From Game Camera"))
                {
                    CaptureReferenceImageFromGameCamera(captureCamera, generatedAssetPath, referenceImage);
                }
            }

            if (!string.IsNullOrWhiteSpace(readinessMessage))
            {
                EditorGUILayout.HelpBox(readinessMessage, readinessType);
            }
        }

        private Rect GetReferencePreviewRect(Texture2D texture)
        {
            float viewWidth = EditorGUIUtility.currentViewWidth - 40f;
            float aspectRatio = texture != null && texture.width > 0 && texture.height > 0
                ? (float)texture.height / texture.width
                : 0.4f;
            float previewHeight = Mathf.Clamp(viewWidth * aspectRatio, MinPreviewHeight, MaxPreviewHeight);

            return GUILayoutUtility.GetRect(viewWidth, previewHeight, GUILayout.ExpandWidth(true));
        }

        private bool GetCaptureReadiness(Camera captureCamera, string outputFolder, string generatedAssetPath, out string message, out MessageType messageType)
        {
            if (captureCamera == null)
            {
                message = "No active game camera found. Add an enabled scene camera or tag an enabled camera as MainCamera.";
                messageType = MessageType.Info;
                return false;
            }

            if (!IsTargetSavedAsset())
            {
                message = "Save this Level Definition as a project asset before capturing a reference image.";
                messageType = MessageType.Warning;
                return false;
            }

            if (!TryValidateAssetFolderPath(outputFolder, out string folderError))
            {
                message = folderError;
                messageType = MessageType.Warning;
                return false;
            }

            if (string.IsNullOrWhiteSpace(generatedAssetPath))
            {
                message = "Reference image filename could not be generated. Check the filename format in Project Settings > AweDev > Level Sequence.";
                messageType = MessageType.Warning;
                return false;
            }

            message = $"Captures use {captureCamera.name} at {LevelSequenceEditorSettings.referenceImageCaptureWidth}x{LevelSequenceEditorSettings.referenceImageCaptureHeight}. Repeated captures overwrite the generated file.";
            messageType = MessageType.Info;
            return true;
        }

        private void DrawClearButton(Rect previewRect)
        {
            Rect clearButtonRect = new(
                previewRect.xMax - ClearButtonSize - 2f,
                previewRect.y + 2f,
                ClearButtonSize,
                ClearButtonSize);

            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.25f, 0.25f);

            if (GUI.Button(clearButtonRect, "X", EditorStyles.miniButton))
            {
                Undo.RecordObject(target, "Clear Level Reference Image");
                _referenceImageProperty.objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            GUI.backgroundColor = previousColor;
        }

        private void CaptureReferenceImageFromGameCamera(Camera captureCamera, string generatedAssetPath, Texture2D currentReferenceImage)
        {
            if (captureCamera == null || string.IsNullOrWhiteSpace(generatedAssetPath)) return;

            string currentReferencePath = currentReferenceImage != null
                ? AssetDatabase.GetAssetPath(currentReferenceImage)
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(currentReferencePath) && currentReferencePath != generatedAssetPath)
            {
                bool shouldReplace = EditorUtility.DisplayDialog(
                    "Replace Reference Image?",
                    $"This Level Definition currently references:\n{currentReferencePath}\n\nCapture will assign and overwrite the generated reference image:\n{generatedAssetPath}\n\nContinue?",
                    "Capture and Replace",
                    "Cancel");

                if (!shouldReplace) return;
            }

            Texture2D capturedTexture = null;

            try
            {
                EnsureAssetFolderExists(LevelSequenceEditorSettings.referenceImageOutputFolder);
                capturedTexture = CaptureCamera(captureCamera);
                byte[] pngBytes = capturedTexture.EncodeToPNG();
                if (pngBytes == null || pngBytes.Length == 0)
                {
                    Debug.LogWarning("[LevelDefinitionEditor] Failed to encode reference image capture.", target);
                    return;
                }

                File.WriteAllBytes(GetAbsolutePath(generatedAssetPath), pngBytes);
                AssetDatabase.ImportAsset(generatedAssetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(generatedAssetPath);
                if (importedTexture == null)
                {
                    Debug.LogWarning($"[LevelDefinitionEditor] Failed to import captured reference image at '{generatedAssetPath}'.", target);
                    return;
                }

                Undo.RecordObject(target, "Capture Level Reference Image");
                _referenceImageProperty.objectReferenceValue = importedTexture;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Debug.Log($"[LevelDefinitionEditor] Captured reference image at '{generatedAssetPath}'.", target);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[LevelDefinitionEditor] Reference image capture failed: {exception.Message}", target);
            }
            finally
            {
                if (capturedTexture != null)
                {
                    DestroyImmediate(capturedTexture);
                }
            }
        }

        private Texture2D CaptureCamera(Camera captureCamera)
        {
            int captureWidth = LevelSequenceEditorSettings.referenceImageCaptureWidth;
            int captureHeight = LevelSequenceEditorSettings.referenceImageCaptureHeight;
            RenderTexture previousTargetTexture = captureCamera.targetTexture;
            RenderTexture previousActiveTexture = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(captureWidth, captureHeight, 24);
            Texture2D texture = new(captureWidth, captureHeight, TextureFormat.RGB24, false);

            try
            {
                captureCamera.targetTexture = renderTexture;
                captureCamera.Render();

                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
                texture.Apply();
                return texture;
            }
            catch
            {
                DestroyImmediate(texture);
                throw;
            }
            finally
            {
                captureCamera.targetTexture = previousTargetTexture;
                RenderTexture.active = previousActiveTexture;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        private Camera GetGameCaptureCamera()
        {
            if (IsUsableCaptureCamera(Camera.main))
            {
                return Camera.main;
            }

            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera camera in cameras)
            {
                if (IsUsableCaptureCamera(camera))
                {
                    return camera;
                }
            }

            return null;
        }

        private bool IsUsableCaptureCamera(Camera camera)
        {
            return camera != null && camera.enabled && camera.gameObject.activeInHierarchy;
        }

        private bool IsTargetSavedAsset()
        {
            return !string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(target));
        }

        private string GetGeneratedReferenceImageAssetPath(LevelDefinition levelDefinition, string outputFolder)
        {
            outputFolder = LevelSequenceEditorSettings.NormalizeAssetFolderPath(outputFolder);
            if (!TryValidateAssetFolderPath(outputFolder, out _)) return string.Empty;

            string filename = ResolveFilenameFormat(levelDefinition);
            if (string.IsNullOrWhiteSpace(filename)) return string.Empty;

            if (!filename.EndsWith(PngExtension, System.StringComparison.OrdinalIgnoreCase))
            {
                filename += PngExtension;
            }

            return $"{outputFolder}/{filename}";
        }

        private string ResolveFilenameFormat(LevelDefinition levelDefinition)
        {
            string fallbackName = !string.IsNullOrWhiteSpace(target.name) ? target.name : "LevelDefinition";
            string levelId = levelDefinition != null && !string.IsNullOrWhiteSpace(levelDefinition.levelId)
                ? levelDefinition.levelId
                : fallbackName;
            string displayName = levelDefinition != null && !string.IsNullOrWhiteSpace(levelDefinition.displayName)
                ? levelDefinition.displayName
                : levelId;
            string sceneName = levelDefinition != null && !string.IsNullOrWhiteSpace(levelDefinition.sceneName)
                ? levelDefinition.sceneName
                : levelId;

            string filename = LevelSequenceEditorSettings.referenceImageFilenameFormat
                .Replace("{levelId}", levelId)
                .Replace("{assetName}", fallbackName)
                .Replace("{displayName}", displayName)
                .Replace("{sceneName}", sceneName);

            filename = SanitizeFileName(filename);
            return string.IsNullOrWhiteSpace(filename) ? $"{SanitizeFileName(fallbackName)}_reference" : filename;
        }

        private bool TryValidateAssetFolderPath(string assetFolderPath, out string errorMessage)
        {
            assetFolderPath = LevelSequenceEditorSettings.NormalizeAssetFolderPath(assetFolderPath);

            if (string.IsNullOrWhiteSpace(assetFolderPath))
            {
                errorMessage = "Reference image output folder is missing.";
                return false;
            }

            if (assetFolderPath != "Assets" && !assetFolderPath.StartsWith("Assets/"))
            {
                errorMessage = "Reference image output folder must be inside the project Assets folder.";
                return false;
            }

            if (assetFolderPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || assetFolderPath.Contains("//"))
            {
                errorMessage = "Reference image output folder contains invalid path characters.";
                return false;
            }

            string[] pathSegments = assetFolderPath.Split('/');
            foreach (string pathSegment in pathSegments)
            {
                if (pathSegment.IndexOfAny(Path.GetInvalidFileNameChars()) < 0) continue;

                errorMessage = "Reference image output folder contains invalid path characters.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void EnsureAssetFolderExists(string assetFolderPath)
        {
            string absolutePath = GetAbsolutePath(LevelSequenceEditorSettings.NormalizeAssetFolderPath(assetFolderPath));
            Directory.CreateDirectory(absolutePath);
            AssetDatabase.Refresh();
        }

        private string GetAbsolutePath(string assetPath)
        {
            string normalizedPath = assetPath.Replace('\\', '/');
            string relativePath = normalizedPath.StartsWith("Assets/")
                ? normalizedPath["Assets/".Length..]
                : string.Empty;

            return string.IsNullOrEmpty(relativePath)
                ? Application.dataPath
                : Path.Combine(Application.dataPath, relativePath);
        }

        private string SanitizeFileName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return string.Empty;

            StringBuilder builder = new(rawName.Length);
            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (char character in rawName)
            {
                builder.Append(System.Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
            }

            return builder.ToString().Trim();
        }
    }
}
