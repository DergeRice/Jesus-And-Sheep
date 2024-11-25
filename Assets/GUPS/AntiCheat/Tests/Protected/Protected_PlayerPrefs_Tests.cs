// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

// Test
using NUnit.Framework;

// Unity
using UnityEngine;
using UnityEngine.TestTools;

// GUPS - AntiCheat
using GUPS.AntiCheat.Protected.Prefs;

namespace GUPS.EasyLocalization.Tests
{
    public class Protected_PlayerPrefs_Tests
    {

#if UNITY_EDITOR

        [SetUp]
        public void Setup_Global_Settings()
        {
            GUPS.AntiCheat.Settings.GlobalSettings.LoadOrCreateAsset();
        }

#endif

        [Test]
        public void Protected_PlayerPref_Bool_Test()
        {
            // Arrange
            ProtectedPlayerPrefs.SetBool("bool", true);

            // Act
            bool result = ProtectedPlayerPrefs.GetBool("bool");

            // Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void Protected_PlayerPref_Int_Test()
        {
            // Arrange
            ProtectedPlayerPrefs.SetInt("int", 1);

            // Act
            int result = ProtectedPlayerPrefs.GetInt("int");

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void Protected_PlayerPref_Float_Test()
        {
            // Arrange
            ProtectedPlayerPrefs.SetFloat("float", 1.0f);

            // Act
            float result = ProtectedPlayerPrefs.GetFloat("float");

            // Assert
            Assert.AreEqual(1.0f, result);
        }

        [Test]
        public void Protected_PlayerPref_String_Test()
        {
            // Arrange
            ProtectedPlayerPrefs.SetString("string", "string");

            // Act
            string result = ProtectedPlayerPrefs.GetString("string");

            // Assert
            Assert.AreEqual("string", result);
        }

        [Test]
        public void Protected_PlayerPref_Vector2_Test()
        {
            // Arrange
            ProtectedPlayerPrefs.SetVector2("vector2", new Vector2(1.0f, 2.0f));

            // Act
            Vector2 result = ProtectedPlayerPrefs.GetVector2("vector2");

            // Assert
            Assert.AreEqual(new Vector2(1.0f, 2.0f), result);
        }

        [Test]
        public void Protected_PlayerPref_Vector3_Test()
        {
            // Arrange
            ProtectedPlayerPrefs.SetVector3("vector3", new Vector3(1.0f, 2.0f, 3.0f));

            // Act
            Vector3 result = ProtectedPlayerPrefs.GetVector3("vector3");

            // Assert
            Assert.AreEqual(new Vector3(1.0f, 2.0f, 3.0f), result);
        }

        [Test]
        public void Protected_PlayerPref_Vector4_Test()
        {
            // Arrange
            ProtectedPlayerPrefs.SetVector4("vector4", new Vector4(1.0f, 2.0f, 3.0f, 4.0f));

            // Act
            Vector4 result = ProtectedPlayerPrefs.GetVector4("vector4");

            // Assert
            Assert.AreEqual(new Vector4(1.0f, 2.0f, 3.0f, 4.0f), result);
        }

        [Test]
        public void Protected_PlayerPref_Quaternion_Test()
        {
            // Arrange
            ProtectedPlayerPrefs.SetQuaternion("quaternion", new Quaternion(1.0f, 2.0f, 3.0f, 4.0f));

            // Act
            Quaternion result = ProtectedPlayerPrefs.GetQuaternion("quaternion");

            // Assert
            Assert.AreEqual(new Quaternion(1.0f, 2.0f, 3.0f, 4.0f), result);
        }

        [Test]
        public void Protected_FileBased_PlayerPref_Bool_Test()
        {
            // Arrange
            ProtectedFileBasedPlayerPrefs.SetBool("bool", true);

            // Act
            bool result = ProtectedFileBasedPlayerPrefs.GetBool("bool");

            // Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void Protected_FileBased_PlayerPref_Int_Test()
        {
            // Arrange
            ProtectedFileBasedPlayerPrefs.SetInt("int", 1);

            // Act
            int result = ProtectedFileBasedPlayerPrefs.GetInt("int");

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void Protected_FileBased_PlayerPref_Float_Test()
        {
            // Arrange
            ProtectedFileBasedPlayerPrefs.SetFloat("float", 1.0f);

            // Act
            float result = ProtectedFileBasedPlayerPrefs.GetFloat("float");

            // Assert
            Assert.AreEqual(1.0f, result);
        }

        [Test]
        public void Protected_FileBased_PlayerPref_String_Test()
        {
            // Arrange
            ProtectedFileBasedPlayerPrefs.SetString("string", "string");

            // Act
            string result = ProtectedFileBasedPlayerPrefs.GetString("string");

            // Assert
            Assert.AreEqual("string", result);
        }

        [Test]
        public void Protected_FileBased_PlayerPref_Vector2_Test()
        {
            // Arrange
            ProtectedFileBasedPlayerPrefs.SetVector2("vector2", new Vector2(1.0f, 2.0f));

            // Act
            Vector2 result = ProtectedFileBasedPlayerPrefs.GetVector2("vector2");

            // Assert
            Assert.AreEqual(new Vector2(1.0f, 2.0f), result);
        }

        [Test]
        public void Protected_FileBased_PlayerPref_Vector3_Test()
        {
            // Arrange
            ProtectedFileBasedPlayerPrefs.SetVector3("vector3", new Vector3(1.0f, 2.0f, 3.0f));

            // Act
            Vector3 result = ProtectedFileBasedPlayerPrefs.GetVector3("vector3");

            // Assert
            Assert.AreEqual(new Vector3(1.0f, 2.0f, 3.0f), result);
        }

        [Test]
        public void Protected_FileBased_PlayerPref_Vector4_Test()
        {
            // Arrange
            ProtectedFileBasedPlayerPrefs.SetVector4("vector4", new Vector4(1.0f, 2.0f, 3.0f, 4.0f));

            // Act
            Vector4 result = ProtectedFileBasedPlayerPrefs.GetVector4("vector4");

            // Assert
            Assert.AreEqual(new Vector4(1.0f, 2.0f, 3.0f, 4.0f), result);
        }

        [Test]
        public void Protected_FileBased_PlayerPref_Quaternion_Test()
        {
            // Arrange
            ProtectedFileBasedPlayerPrefs.SetQuaternion("quaternion", new Quaternion(1.0f, 2.0f, 3.0f, 4.0f));

            // Act
            Quaternion result = ProtectedFileBasedPlayerPrefs.GetQuaternion("quaternion");

            // Assert
            Assert.AreEqual(new Quaternion(1.0f, 2.0f, 3.0f, 4.0f), result);
        }
    }
}
