#!/bin/bash
prjPath=`dirname $0`
cd $prjPath

package_names[1]="GRTools.GitPackageResolver"
package_names[2]="GRTools.Utils"
package_names[3]="GRTools.Thread"
package_names[4]="GRTools.Sqlite"
package_names[5]="GRTools.Addressables"
package_names[6]="GRTools.Localization"
package_names[7]="GRTools.Localization.TextMeshPro"
package_names[8]="GRTools.Localization.Addressables"

package_paths[1]="Assets/GRTools/PackageResolver"
package_paths[2]="Assets/GRTools/Utils"
package_paths[3]="Assets/GRTools/Thread"
package_paths[4]="Assets/GRTools/DataBase/Sqlite"
package_paths[5]="Assets/GRTools/Addressables"
package_paths[6]="Assets/GRTools/Localization"
package_paths[7]="Assets/GRTools/LocalizationExtra/LocalizationTMP"
package_paths[8]="Assets/GRTools/LocalizationExtra/LocalizationAddressables"

echo "Select one package to update:"

index=1

while [ $index -lt ${#package_names[@]} ]
do
	package=${package_names[$index]}
	let index++
  echo "$index $package"
  
done

read index

package_name=${package_names[$index]}
package_relative_path=${package_paths[$index]}
package_absolute_path=$prjPath"/"${package_paths[$index]}
json_path=${package_relative_path}"/package.json"

echo "1. Read 【${package_name}】 pakcage.json:"

version=`./jq-osx-amd64 -r .version < $json_path`

display_name=`./jq-osx-amd64 -r .displayName < $json_path`

tag_name=${display_name}'@'${version}

echo "DisplayName: $display_name"
echo "Version: $version"
echo "Tag: $tag_name"
echo

echo "2. Split Git"
git subtree split --prefix=${package_relative_path} --branch ${display_name}

echo
echo "3. Tag"
git tag ${tag_name} ${display_name}
echo
echo "3. Push"
echo `git push origin ${display_name} --tags`
echo
echo "Done"
cd