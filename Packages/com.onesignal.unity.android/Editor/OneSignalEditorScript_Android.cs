/**
 * Modified MIT License
 *
 * Copyright 2018 OneSignal
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * 1. The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * 2. All copies of substantial portions of the Software may only be used in connection
 * with services provided by OneSignal.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.IO;
using OneSignalPush.Editor;
using UnityEditor;

[InitializeOnLoad]
public class OneSignalEditorScriptAndroid
{
   const string k_AndroidConfigFolder = "Packages/com.onesignal.unity.android/Plugins/Android/OneSignalConfig";
   const string k_PackageManifestPath = "Packages/com.onesignal.unity.android/package.json";

   static OneSignalEditorScriptAndroid()
   {
      CreateOneSignalAndroidManifest();
      PackageManifestSanityCheck();
   }

   static void PackageManifestSanityCheck()
   {
      Manifest mainManifest = new Manifest();
      mainManifest.Fetch();

      bool manifestUpdated = false;

      //Check if Google scope registry exists in manifest.
      //Add new scope if we don't have one
      if (!mainManifest.IsRegistryExists(ScopeRegistriesConfig.GoogleScopeRegistryUrl))
      {
         mainManifest.AddScopeRegistry(ScopeRegistriesConfig.GoogleScopeRegistry);
         manifestUpdated = true;
      }

      if (manifestUpdated)
         mainManifest.ApplyChanges();

      manifestUpdated = false;

      Manifest manifest = new Manifest(k_PackageManifestPath);
      manifest.Fetch();

      //Check if we have com.google.external-dependency-manager dependency in manifest.
      if (!manifest.IsDependencyExists(ScopeRegistriesConfig.EDM4UName))
      {
         //Add com.google.external-dependency-manager dependency if we don't have one
         manifest.AddDependency(ScopeRegistriesConfig.EDM4UName, ScopeRegistriesConfig.EDM4UVersion);
         manifestUpdated = true;
      }
      else
      {
         //Ok, com.google.external-dependency-manager dependency exists in manifest file
         //Need to check it's version
         Dependency foundationDependency = manifest.GetDependency(ScopeRegistriesConfig.EDM4UName);
         if (!foundationDependency.Version.Equals(ScopeRegistriesConfig.EDM4UVersion))
         {
            //Set proper dependency version
            foundationDependency.SetVersion(ScopeRegistriesConfig.EDM4UVersion);
            manifestUpdated = true;
         }
      }

      if (manifestUpdated)
         manifest.ApplyChanges();
   }

   // Copies `AndroidManifestTemplate.xml` to `AndroidManifest.xml`
   // then replace `${manifestApplicationId}` with current packagename in the Unity settings.
   static void CreateOneSignalAndroidManifest()
   {
      var configFullPath  =  Path.GetFullPath($"{k_AndroidConfigFolder}");
      var manifestPath =  Path.GetFullPath($"{configFullPath}{Path.DirectorySeparatorChar}AndroidManifest.xml");
      var manifestTemplatePath =  Path.GetFullPath($"{configFullPath}{Path.DirectorySeparatorChar}AndroidManifestTemplate.xml");

      File.Copy(manifestTemplatePath, manifestPath, true);
      var streamReader = new StreamReader(manifestPath);
      var body = streamReader.ReadToEnd();
      streamReader.Close();

      #if UNITY_5_6_OR_NEWER
         body = body.Replace("${manifestApplicationId}", PlayerSettings.applicationIdentifier);
      #else
         body = body.Replace("${manifestApplicationId}", PlayerSettings.bundleIdentifier);
      #endif
      using (var streamWriter = new StreamWriter(manifestPath, false)) {
         streamWriter.Write(body);
      }
   }
}