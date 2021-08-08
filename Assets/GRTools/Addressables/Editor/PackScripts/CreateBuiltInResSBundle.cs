using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

public class CreateBuiltInResSBundle : IBuildTask
    {
        static readonly GUID k_BuiltInGuid = new GUID("0000000000000000f000000000000000");
        /// <inheritdoc />
        public int Version { get { return 1; } }

#pragma warning disable 649
        [InjectContext(ContextUsage.In)]
        IDependencyData m_DependencyData;

        [InjectContext(ContextUsage.InOut, true)]
        IBundleExplictObjectLayout m_Layout;
#pragma warning restore 649

        /// <summary>
        /// Stores the name for the built-in shaders bundle.
        /// </summary>
        public string BundleName { get; set; }
        public string ShaderBundleName { get; set; }

        /// <summary>
        /// Create the built-in shaders bundle.
        /// </summary>
        /// <param name="shaderBundleName">The name of the shader bundle</param>
        /// <param name="bundleName">The name of the other builtin resources bundle.</param>
        public CreateBuiltInResSBundle(string shaderBundleName, string bundleName)
        {
            ShaderBundleName = shaderBundleName;
            BundleName = bundleName;
        }

        /// <inheritdoc />
        public ReturnCode Run()
        {
            HashSet<ObjectIdentifier> buildInObjects = new HashSet<ObjectIdentifier>();
            foreach (AssetLoadInfo dependencyInfo in m_DependencyData.AssetInfo.Values)
                buildInObjects.UnionWith(dependencyInfo.referencedObjects.Where(x => x.guid == k_BuiltInGuid));

            foreach (SceneDependencyInfo dependencyInfo in m_DependencyData.SceneInfo.Values)
                buildInObjects.UnionWith(dependencyInfo.referencedObjects.Where(x => x.guid == k_BuiltInGuid));

            ObjectIdentifier[] usedSet = buildInObjects.ToArray();
            Type[] usedTypes = ContentBuildInterface.GetTypeForObjects(usedSet);

            if (m_Layout == null)
                m_Layout = new BundleExplictObjectLayout();
            
            Type shader = typeof(Shader);
            for (int i = 0; i < usedTypes.Length; i++)
            {
                m_Layout.ExplicitObjectLocation.Add(usedSet[i], usedTypes[i] == shader ? ShaderBundleName : BundleName);
            }

            if (m_Layout.ExplicitObjectLocation.Count == 0)
                m_Layout = null;

            return ReturnCode.Success;
        }
    }