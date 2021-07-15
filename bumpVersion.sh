#!/bin/bash

updatePrimaryVersion() {
    # VERSION file will act as the source of truth
    version_filepath="OneSignalExample/Assets/OneSignal/VERSION"
    version=$(cat $version_filepath)

    echo "Current Version is ${version}"

    # loose semver checking; use official standard if going advanced
    # https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
    version_regex="([0-9]*)\.([0-9]*)\.([0-9]*)(-[A-z]*)?"

    current_major=$([[ ${version} =~ $version_regex ]] && echo "${BASH_REMATCH[1]}")
    current_minor=$([[ ${version} =~ $version_regex ]] && echo "${BASH_REMATCH[2]}")
    current_patch=$([[ ${version} =~ $version_regex ]] && echo "${BASH_REMATCH[3]}")

    # get new verison number
    if [[ "$to_bump" = "major" ]]
    then
        new_major=$((current_major + 1))
        new_minor=0
        new_patch=0
    elif [[ "$to_bump" = "minor" ]]
    then
        new_major="${current_major}"
        new_minor=$((current_minor + 1))
        new_patch=0
    elif [[ "$to_bump" = "patch" ]]
    then
        new_major="${current_major}"
        new_minor="${current_minor}"
        new_patch=$((current_patch + 1))
    else
        echo "How did we get here?"
        exit 1
    fi

    new_version="${new_major}.${new_minor}.${new_patch}"
    echo "    New version is ${new_version}"

    # update VERSION file
    echo "${new_version}" > ${version_filepath}
    echo "Updated - ${version_filepath}"

    # update package.json files
    packagejson_path="com.onesignal.unity.*/package.json"
    packagejson_version_regex="\"version\": \"${version_regex}\","
    packagejson_new_version="\"version\": \"${new_version}\","

    # just going to keep these all in sync for now
    packagejson_core_regex="\"com.onesignal.unity.core\": \"${version_regex}\""
    packagejson_new_core="\"com.onesignal.unity.core\": \"${new_version}\""

    for packagejson_filepath in $packagejson_path
    do
        packagejson_file=$(cat $packagejson_filepath)

        packagejson_version=$([[ ${packagejson_file} =~ $packagejson_version_regex ]] && echo "${BASH_REMATCH[0]}")
        packagejson_file=${packagejson_file/$packagejson_version/$packagejson_new_version}

        packagejson_core=$([[ ${packagejson_file} =~ $packagejson_core_regex ]] && echo "${BASH_REMATCH[0]}")
        packagejson_file=${packagejson_file/$packagejson_core/$packagejson_new_core}

        echo "${packagejson_file}" > ${packagejson_filepath}
        echo "Updated - ${packagejson_filepath}"
    done

    # update project version TODO
    # projectsettings_path="OneSignalExample/ProjectSettings/ProjectSettings.asset"
    # projectsettings_regex="bundleVersion: "
    
}

updateBuildVersion() {
    echo "NYI"
}

to_bump=$1
postfix=$2

# validate input
if [[ -z "$to_bump" || ! "$to_bump" =~ ^(major|minor|patch|build)$ ]] 
then
    echo -e "bumpVersion must be run with one of the following arguments
     major      Increment the MAJOR version by 1 and reset MINOR and PATCH to 0
     minor      Increment the MINOR version by 1 and reset PATCH to 0
     patch      Increment the PATCH version by 1
     build      Increment the project BUILD version by 1 (NYI)"
    exit 1
fi

if [[ -n "$postfix" && ! "$postfix" =~ ^(preview)$ ]] 
then
    echo -e "a postfix can be included with either of the following arguments
      (none)    If no argument then no postfix will be added
     preview    Attach the 'preview' postfix"
    exit 1
fi

if [[ "$to_bump" = "build" ]]
then
    updateBuildVersion
else
    updatePrimaryVersion
fi