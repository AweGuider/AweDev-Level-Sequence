using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace AweDev.LevelSequence.Editor.Tests
{
    public class LevelSequenceValidatorTests
    {
        [Test]
        public void FindDuplicateLevelIds_ReturnsDuplicateGroups()
        {
            LevelDefinition first = CreateLevel("duplicate");
            LevelDefinition second = CreateLevel("duplicate");
            LevelSequence sequence = CreateSequence(first, second);

            List<DuplicateLevelIdGroup> groups = LevelSequenceValidator.FindDuplicateLevelIds(sequence);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual("duplicate", groups[0].levelId);
            Assert.AreEqual(2, groups[0].levels.Count);
        }

        [Test]
        public void BuildReport_InvalidatesMissingScenePath()
        {
            LevelDefinition level = CreateLevel("missing-scene");
            LevelSequence sequence = CreateSequence(level);

            LevelSequenceBuildSettingsReport report = LevelSequenceBuildSettingsSync.BuildReport(sequence);

            Assert.AreEqual(1, report.invalidLevels.Count);
            Assert.That(report.invalidLevels[0].reason, Does.Contain("no scene path"));
        }

        private static LevelDefinition CreateLevel(string levelId)
        {
            LevelDefinition level = ScriptableObject.CreateInstance<LevelDefinition>();
            SetPrivateField(level, "_levelId", levelId);
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
    }
}
