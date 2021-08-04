
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace GRTools.PackageResolver
{
    [InitializeOnLoad]
    public class PackageManagerExtentionExample : IPackageManagerExtension
    {
        static PackageManagerExtentionExample()
        {
            PackageManagerExtensions.RegisterExtension(new PackageManagerExtentionExample());
        }

        // ScrollView tasksContainer;
        private TemplateContainer root;
        public VisualElement CreateExtensionUI()
        {
            if (root == null)
            {
                root = new TemplateContainer()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                };
            }
            return root;
        }

        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
            root.contentContainer.Clear();
            // if (packageInfo.author.name == "Warl-G")
            // {
                root.contentContainer.Add(UsedBy(packageInfo));
                root.contentContainer.Add(Dependencies(packageInfo));
            // }
        }

        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {

        }

        public void OnPackageRemoved(PackageInfo packageInfo)
        {

        }

        private VisualElement UsedBy(PackageInfo packageInfo)
        {
            
            var usedPackages = Resolver.UsedBy(packageInfo.name);
            if (usedPackages.Length > 0)
            {
                var versionElement = new TemplateContainer()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column,
                    }
                };
                versionElement.Add(new Label("Is Used By: "));
                foreach (var package in usedPackages)
                {
                    versionElement.Add(new Label(package));
                }
                
                return versionElement;
            }

            return null;
        }

        private static AddRequest _request;
        private VisualElement Dependencies(PackageInfo packageInfo)
        {
            var meta = PackageMeta.FromPackageDir(packageInfo.resolvedPath);
            if (meta.dependencyPackages.Length > 0)
            {
                var list = new TemplateContainer()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column,
                    }
                };
                list.Add(new Label("Use: "));
                foreach (var package in  meta.dependencyPackages)
                {
                    var element = new TemplateContainer()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                        }
                    };
                    if (Resolver.IsInstalled(package.name))
                    {
                        element.Add(new Label("Imported")
                        {
                            style =
                            {
                                width = 80,
                                unityTextAlign = TextAnchor.MiddleCenter
                            }
                        });
                    }
                    else
                    {
                        var button = new Button(() =>
                        {
                            if (_request == null)
                            {
                                _request = Client.Add(package.url);
                                EditorApplication.update += Progress;
                                EditorUtility.DisplayProgressBar("PackageResolver", "Adding Dependency Packages", 0);
                            }
                        }){
                            style =
                            {
                                width = 80,
                                unityTextAlign = TextAnchor.MiddleCenter
                            }
                        };
                        button.text = "Import";
                        element.Add(button);
                    }
                    element.Add(new Label(package.name));
                    if (package.version != new SemVersion(0,0,0))
                    {
                        element.Add(new Label(package.version.ToString()));
                    }
                    
                    list.Add(element);
                }
                return list;
            }

            return null;
        }

        private static void Progress()
        {
            if (_request.IsCompleted)
            {
                EditorApplication.update -= Progress;
                EditorUtility.ClearProgressBar();
                if (_request.Status == StatusCode.Failure)
                {
                    Debug.Log(_request.Error);
                }
                else if (_request.Status == StatusCode.Success)
                {
                    Debug.Log("Importing Success");
                }
            }
        }
    }
}