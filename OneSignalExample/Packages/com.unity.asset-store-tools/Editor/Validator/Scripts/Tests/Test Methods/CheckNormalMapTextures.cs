using AssetStoreTools.Validator.Data;
using AssetStoreTools.Validator.TestDefinitions;
using AssetStoreTools.Validator.TestMethods.Utility;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetStoreTools.Validator.TestMethods
{
    internal class CheckNormalMapTextures : ITestScript
    {
        public const int TextureCacheLimit = 8;
        
        public TestResult Run(ValidationTestConfig config)
        {
            var result = new TestResult() { Result = TestResult.ResultStatus.Undefined };

            var materials = AssetUtility.GetObjectsFromAssets<Material>(config.ValidationPaths, AssetType.Material);
            var badTextures = new List<Texture>();
            var badPaths = new List<string>();

            foreach (var mat in materials)
            {
                for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
                {
                    if ((mat.shader.GetPropertyFlags(i) & UnityEngine.Rendering.ShaderPropertyFlags.Normal) != 0)
                    {
                        var propertyName = mat.shader.GetPropertyName(i);
                        var assignedTexture = mat.GetTexture(propertyName);

                        if (assignedTexture == null)
                            continue;

                        var texturePath = AssetUtility.ObjectToAssetPath(assignedTexture);
                        var textureImporter = AssetUtility.GetAssetImporter(texturePath) as TextureImporter;
                        if (textureImporter == null)
                            continue;

                        if (textureImporter.textureType != TextureImporterType.NormalMap && !badTextures.Contains(assignedTexture))
                        {
                            if (badTextures.Count < TextureCacheLimit)
                            {
                                badTextures.Add(assignedTexture);
                            }
                            else
                            {
                                string path = AssetDatabase.GetAssetPath(assignedTexture);
                                badPaths.Add(path);
                            }
                        }
                    }
                }  
                
                EditorUtility.UnloadUnusedAssetsImmediate();
            }

            if (badTextures.Count == 0)
            {
                result.Result = TestResult.ResultStatus.Pass;
                result.AddMessage("All normal map textures have the correct texture type!");
            }
            else if(badPaths.Count != 0)
            {
                foreach (Texture texture in badTextures)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    badPaths.Add(path);
                }
                
                string paths = string.Join("\n", badPaths);

                result.Result = TestResult.ResultStatus.VariableSeverityIssue;
                result.AddMessage("The following textures are not set to type 'Normal Map'", null);
                result.AddMessage(paths);
            }
            else
            {
                result.Result = TestResult.ResultStatus.VariableSeverityIssue;
                result.AddMessage("The following textures are not set to type 'Normal Map'", null, badTextures.ToArray());
            }

            return result;
        }
    }
}
