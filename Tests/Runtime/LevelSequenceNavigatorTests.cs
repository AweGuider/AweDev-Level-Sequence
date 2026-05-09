using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace AweDev.LevelSequence.Tests
{
    public class LevelSequenceNavigatorTests
    {
        [Test]
        public void SetLevelById_UsesConfiguredLoader()
        {
            LevelDefinition first = CreateLevel("first", "FirstScene");
            LevelSequence sequence = CreateSequence(first);
            RecordingLoader loader = new();
            LevelSequenceNavigator navigator = new(sequence, loader);

            bool result = navigator.SetLevelById("first");

            Assert.IsTrue(result);
            Assert.AreSame(first, loader.loadedLevel);
            Assert.AreSame(first, navigator.currentLevel);
        }

        [Test]
        public void MoveToNextLevel_LoadsNextSequenceEntry()
        {
            LevelDefinition first = CreateLevel("first", "FirstScene");
            LevelDefinition second = CreateLevel("second", "SecondScene");
            LevelSequence sequence = CreateSequence(first, second);
            RecordingLoader loader = new();
            LevelSequenceNavigator navigator = new(sequence, loader);
            navigator.SetCurrentLevel(first, false);

            bool result = navigator.MoveToNextLevel();

            Assert.IsTrue(result);
            Assert.AreSame(second, loader.loadedLevel);
        }

        [Test]
        public void MoveToNextLevel_FailsAtEndOfSequence()
        {
            LevelDefinition first = CreateLevel("first", "FirstScene");
            LevelSequence sequence = CreateSequence(first);
            RecordingLoader loader = new();
            LevelSequenceNavigator navigator = new(sequence, loader);
            LevelNavigationFailure failure = default;
            navigator.NavigationFailed += nextFailure => failure = nextFailure;
            navigator.SetCurrentLevel(first, false);

            bool result = navigator.MoveToNextLevel();

            Assert.IsFalse(result);
            Assert.IsNull(loader.loadedLevel);
            Assert.That(failure.reason, Does.Contain("No next level"));
        }

        private static LevelDefinition CreateLevel(string levelId, string sceneName)
        {
            LevelDefinition level = ScriptableObject.CreateInstance<LevelDefinition>();
            SetPrivateField(level, "_levelId", levelId);
            SetPrivateField(level, "_displayName", levelId);
            SetPrivateField(level, "_sceneName", sceneName);
            return level;
        }

        private static LevelSequence CreateSequence(params LevelDefinition[] levels)
        {
            LevelSequence sequence = ScriptableObject.CreateInstance<LevelSequence>();
            SetPrivateField(sequence, "_levels", new List<LevelDefinition>(levels));
            return sequence;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(fieldInfo, $"Missing field {fieldName}.");
            fieldInfo.SetValue(target, value);
        }

        private sealed class RecordingLoader : ILevelSceneLoader
        {
            public LevelDefinition loadedLevel;

            public bool TryLoadLevel(LevelDefinition level, out string failureReason)
            {
                loadedLevel = level;
                failureReason = null;
                return true;
            }
        }
    }
}
