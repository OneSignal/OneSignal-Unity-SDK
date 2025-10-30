#!/bin/bash

manual="
    OneSignal Unity SDK - composeRelease
      Performs operations necessary to ready current changes for release.
      - Updates all version numbers required to properly identify the SDK and generates a commit
      - Sets up a new release/* branch pegged to the new version number
      - Creates a pull request back to the main branch
      - Generates a *.unitypackage file to distribute
      - Composes a GitHub release and uploads the *.unitypackage file

    Usage:
      ./composeRelease.sh {bump_command} {postfix (optional)}

    Examples:
      (with an initial verison of 2.14.3) 
      ./composeRelease.sh minor preview                 Change all verison numbers to 2.15.0-preview
      ./composeRelease.sh major                         Change all verison numbers to 3.0.0
      ./composeRelease.sh patch                         Change all verison numbers to 2.14.4
      ./composeRelease.sh specify 3.2.1-beta.123        Change all verison numbers to 3.2.1-beta.123
"

# check for help command before continuing
if [[ "$#" -ge "1" && "$1" = '-h' || "$1" = '--help' || "$1" = "-?" ]]
then
    echo -e "${manual}"
    exit 0
fi

# gather args
bump_command=$1
postfix=$2

# validate input
if [[ -z "$bump_command" || ! "$bump_command" =~ ^(major|minor|patch|specify)$ ]] 
then
    echo -e "composeRelease must be run with one of the following arguments
     major      Increment the MAJOR version by 1 and reset MINOR and PATCH to 0
     minor      Increment the MINOR version by 1 and reset PATCH to 0
     patch      Increment the PATCH version by 1
     specify    Provide a specific version number"
    exit 1
fi

if [[ "$bump_command" = "specify" ]]
then
    if [[ -z "$postfix" ]]
    then
        echo -e "a valid version must be specified"
        exit 1
    fi
elif [[ -n "$postfix" && ! "$postfix" =~ ^(preview|beta)$ ]] 
then
    echo -e "a postfix can be included with either of the following arguments
     (none)     If no argument then no postfix will be added
     preview    Attach the 'preview' postfix
     beta       Attach the 'beta' postfix"
    exit 1
fi

# is github cli available to compose release?
if ! command -v gh &> /dev/null
then
    echo "GitHub CLI could not be found. Please visit https://cli.github.com/ or run brew install gh. Make sure you login after installation via gh auth login"
    exit 1
fi

# try to find unity executable
unity_project_version_path="OneSignalExample/ProjectSettings/ProjectVersion.txt"
unity_project_version=$(cat "${unity_project_version_path}" | sed -n 's/^m_EditorVersion: //p')

# Unity project path from CI environment if available
unity_project_path="${UNITY_PROJECT_PATH:-OneSignalExample}"

# Common installation locations (CI, macOS, local)
unity_candidates=(
  "/home/runner/Unity/Hub/Editor/${unity_project_version}/Editor/Unity"                   # Linux (buildalon/unity-setup)
  "/Applications/Unity/Hub/Editor/${unity_project_version}/Unity.app/Contents/MacOS/Unity" # macOS
)

unity_executable=""
for candidate in "${unity_candidates[@]}"; do
  if [[ -x "$candidate" ]]; then
    unity_executable="$candidate"
    break
  fi
done

if [[ -z "$unity_executable" ]]; then
  echo "❌ Could not locate Unity executable for version ${unity_project_version}"
  echo "Checked the following paths:"
  printf ' - %s\n' "${unity_candidates[@]}"
  exit 1
else
  echo "✅ Found Unity executable: ${unity_executable}"
fi

# VERSION file will act as the source of truth
version_filepath="OneSignalExample/Assets/OneSignal/VERSION"
current_version=$(cat "$version_filepath")

echo "Current Version is ${current_version}"


if [[ "$bump_command" = "specify" ]]
then
    new_version="${postfix}"
else
    # loose semver checking; use official standard if going advanced
    # https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
    version_regex="([0-9]+)\.([0-9]+)\.([0-9]+)(-([a-zA-Z]+))?(\.([0-9])+)?"

    current_major=$([[ ${current_version} =~ $version_regex ]] && echo "${BASH_REMATCH[1]}")
    current_minor=$([[ ${current_version} =~ $version_regex ]] && echo "${BASH_REMATCH[2]}")
    current_patch=$([[ ${current_version} =~ $version_regex ]] && echo "${BASH_REMATCH[3]}")

    # get new verison number
    if [[ "$bump_command" = "major" ]]
    then
        new_major=$((current_major + 1))
        new_minor=0
        new_patch=0
    elif [[ "$bump_command" = "minor" ]]
    then
        new_major="${current_major}"
        new_minor=$((current_minor + 1))
        new_patch=0
    elif [[ "$bump_command" = "patch" ]]
    then
        new_major="${current_major}"
        new_minor="${current_minor}"
        new_patch=$((current_patch + 1))
    else
        echo "How did we get here?"
        exit 1
    fi

    new_version="${new_major}.${new_minor}.${new_patch}"

    if [[ -n "$postfix" ]]
    then
        new_version="${new_version}-${postfix}"
    fi 
fi

echo "    New version is ${new_version}"

# update VERSION file
echo -n "${new_version}" > ${version_filepath}
echo "Updated - ${version_filepath}"

# update version numbers
onesignal_path="com.onesignal.unity.core/Runtime/OneSignal.cs"
onesignal_version_regex="Version = \"${current_version}\";"
onesignal_new_version="Version = \"${new_version}\";"
onesignal_file=$(cat "$onesignal_path")

onesignal_version=$([[ ${onesignal_file} =~ $onesignal_version_regex ]] && echo "${BASH_REMATCH[0]}")
onesignal_file=${onesignal_file/$onesignal_version/$onesignal_new_version}

echo "${onesignal_file}" > ${onesignal_path}
echo "Updated - ${onesignal_path}"

# The version number sent to iOS and Android use a 000000 format. For example: 5.1.9 should be sent as 050109
toHeaderVersion() {
    local version=$1

    a=(${version//./ })

    for i in ${!a[@]}; do
        a[$i]=$(printf "%02d" ${a[$i]})
    done

    retval=$(IFS=; echo "${a[*]}");
    return
}

# update Android header
toHeaderVersion $current_version
current_header_version=$retval
toHeaderVersion $new_version
new_header_version=$retval

onesignalplatform_path="com.onesignal.unity.core/Runtime/OneSignalPlatform.cs"
onesignalplatform_version_regex="VersionHeader = \"${current_header_version}\";"
onesignalplatform_new_version="VersionHeader = \"${new_header_version}\";"
onesignalplatform_file=$(cat "$onesignalplatform_path")

onesignalplatform_version=$([[ ${onesignalplatform_file} =~ $onesignalplatform_version_regex ]] && echo "${BASH_REMATCH[0]}")
onesignalplatform_file=${onesignalplatform_file/$onesignalplatform_version/$onesignalplatform_new_version}

echo "${onesignalplatform_file}" > ${onesignalplatform_path}
echo "Updated - ${onesignalplatform_path}"

# update iOS header
uiapplicationonesignalunity_path="com.onesignal.unity.ios/Runtime/Plugins/iOS/UIApplication+OneSignalUnity.mm"
uiapplicationonesignalunity_version_regex="setSdkVersion:@\"${current_header_version}\"];"
uiapplicationonesignalunity_new_version="setSdkVersion:@\"${new_header_version}\"];"
uiapplicationonesignalunity_file=$(cat "$uiapplicationonesignalunity_path")

uiapplicationonesignalunity_version=$([[ ${uiapplicationonesignalunity_file} =~ $uiapplicationonesignalunity_version_regex ]] && echo "${BASH_REMATCH[0]}")
uiapplicationonesignalunity_file=${uiapplicationonesignalunity_file/$uiapplicationonesignalunity_version/$uiapplicationonesignalunity_new_version}

echo "${uiapplicationonesignalunity_file}" > ${uiapplicationonesignalunity_path}
echo "Updated - ${uiapplicationonesignalunity_path}"

# update package.json files
packagejson_path="com.onesignal.unity.*/package.json"
packagejson_version_regex="\"version\": \"${current_version}\","
packagejson_new_version="\"version\": \"${new_version}\","

# just going to keep these all in sync for now
packagejson_core_regex="\"com.onesignal.unity.core\": \"${current_version}\""
packagejson_new_core="\"com.onesignal.unity.core\": \"${new_version}\""

for packagejson_filepath in $packagejson_path
do
    packagejson_file=$(cat "$packagejson_filepath")

    packagejson_version=$([[ ${packagejson_file} =~ $packagejson_version_regex ]] && echo "${BASH_REMATCH[0]}")
    packagejson_file=${packagejson_file/$packagejson_version/$packagejson_new_version}

    packagejson_core=$([[ ${packagejson_file} =~ $packagejson_core_regex ]] && echo "${BASH_REMATCH[0]}")
    packagejson_file=${packagejson_file/$packagejson_core/$packagejson_new_core}

    echo "${packagejson_file}" > ${packagejson_filepath}
    echo "Updated - ${packagejson_filepath}"
done

# update packages-lock.json
packageslockjson_path="OneSignalExample/Packages/packages-lock.json"
packageslockjson_file=$(cat "$packageslockjson_path")

packageslockjson_core=$([[ ${packageslockjson_file} =~ $packagejson_core_regex ]] && echo "${BASH_REMATCH[0]}")
packageslockjson_file=${packageslockjson_file/$packageslockjson_core/$packagejson_new_core}

echo "${packageslockjson_file}" > ${packageslockjson_path}
echo "Updated - ${packageslockjson_path}"

# update .asmdef files
asmdef_path="OneSignalExample/Assets/OneSignal/*/OneSignal.UnityPackage.*.asmdef"
asmdef_version_regex="\"expression\": \"${current_version}\","
asmdef_new_version="\"expression\": \"${new_version}\","

for asmdef_filepath in $asmdef_path
do
    asmdef_file=$(cat "$asmdef_filepath")

    asmdef_version=$([[ ${asmdef_file} =~ $asmdef_version_regex ]] && echo "${BASH_REMATCH[0]}")
    asmdef_file=${asmdef_file/$asmdef_version/$asmdef_new_version}

    echo "${asmdef_file}" > ${asmdef_filepath}
    echo "Updated - ${asmdef_filepath}"
done

# update CHANGELOG
changelog_path="OneSignalExample/Assets/OneSignal/CHANGELOG.md"
changelog_file=$(cat "$changelog_path")

# append new version to notes
changelog_notes_unreleased="\#\# \[Unreleased\]"
changelog_notes_new_version="## [Unreleased]
## [${new_version}]"
changelog_file="${changelog_file/${changelog_notes_unreleased}/$changelog_notes_new_version}"

# update links
compare_link_unreleased="\[Unreleased\]: https:\/\/github\.com\/OneSignal\/OneSignal-Unity-SDK\/compare\/${current_version}\.\.\.HEAD"
compare_links_new_version="[Unreleased]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/${new_version}...HEAD
[${new_version}]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/${current_version}...${new_version}"
changelog_file="${changelog_file/${compare_link_unreleased}/${compare_links_new_version}}"

# output changes
echo "${changelog_file}" > ${changelog_path}
echo "Updated - ${changelog_path}"

executeUnityMethod() {
    local project_path=$1
    local build_target=$2
    local method_name=$3
    local log_path="${PWD}/logs/${method_name}-${build_target}-$(date +%Y%m%d%H%M%S).txt"
    
    ${unity_executable} -projectpath "${project_path}"\
                        -quit\
                        -batchmode\
                        -nographics\
                        -buildTarget "${build_target}"\
                        -executeMethod "${method_name}"\
                        -logFile "${log_path}"
   
    local method_result=$?
    
    if [[ ${method_result} -ne 0 ]]; then
        echo "Unity method ${method_name}} failed with ${method_result}"
    else
        echo "Unity method completed"
    fi
}

echo "Cleaning up Unity locks..."
pkill -f Unity || true
rm -f OneSignalExample/Temp/UnityLockfile

# update project version
projectsettings_path="OneSignalExample/ProjectSettings/ProjectSettings.asset"
executeUnityMethod "OneSignalExample" "Android" "OneSignalSDK.OneSignalPackagePublisher.UpdateProjectVersion"

# build a unitypackage for release
package_path="OneSignalExample/OneSignal-v${new_version}.unitypackage"
executeUnityMethod "OneSignalExample" "Android" "OneSignalSDK.OneSignalPackagePublisher.ExportUnityPackage"

# preserve current workspace
current_branch=$(git branch --show-current)
git add ${version_filepath} ${packagejson_path} ${projectsettings_path} ${changelog_path} ${onesignal_path} ${onesignalplatform_path} ${uiapplicationonesignalunity_path} ${packageslockjson_path} ${asmdef_path}
git stash push --keep-index

# generate new release branch and commit all changes
release_branch="release/${new_version}"
git checkout -b "${release_branch}" # todo - branch off main once these changes are there
git commit -m "Bumped version to ${new_version}"
git push --set-upstream origin "${release_branch}"

# create a pull request and draft release for these changes
gh pr create\
    --base main\
    --head "${release_branch}"\
    --title "Release ${new_version}"\
    --body "Pull request for version ${new_version}"

gh release create "${new_version}" "${package_path}"\
    --draft\
    --title "${new_version} Release"\
    --notes "TODO"

# return to workspace
git checkout "${current_branch}"
git stash pop
