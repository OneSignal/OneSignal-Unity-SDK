#! /bin/sh

#let users pass in the final filename for the .unitypackage if they wish
if [ $# -eq 0 ]
 then 
   package_name=OneSignalSDK.unitypackage
else 
   package_name=$1
fi

#paths
project_path=$(pwd)/OneSignalExample
log_file=$(pwd)/build.log
generated_path=$project_path/$package_name
final_path=$(pwd)/$package_name
temp_location=$(pwd)/onesignal_temp
icons_location=$project_path/Assets/AppIcons
icons_temp_location=$(pwd)/onesignal_temp/tempAppIcons
android_location=$project_path/Assets/Plugins/Android
config_location=$android_location/OneSignalConfig.plugin
package_manifest=$project_path/Packages/manifest.json
temp_package_manifest=$temp_location/Packages/manifest.json

mkdir -p $temp_location

# Removed generated Android manifest files
rm $config_location/AndroidManifest.xml 2> /dev/null 
rm $config_location/AndroidManifest.xml.meta 2> /dev/null 

# temp move out Packages/manifest.json
# This prevents possible crashes due to old versions in this manifest
mkdir -p $temp_location/Packages/
mv $package_manifest $temp_package_manifest

#temporarily remove Android AppIcons 
mv $icons_location $icons_temp_location
mv $icons_location.meta $icons_temp_location.meta

## START - Clean Android files
# This removes any .aar files we don't want to bundle in our package

# temporarily move some necessary files
mv $config_location $temp_location/OneSignalConfig.plugin
mv $config_location.meta $temp_location/OneSignalConfig.plugin.meta

# get rid of a bunch of unnecessary files
rm -r $android_location
mkdir $android_location

# put the config files back
mv $temp_location/OneSignalConfig.plugin $config_location
mv $temp_location/OneSignalConfig.plugin.meta $config_location.meta

## END - Clean Android files

# Create the .unitypackage
echo "Creating unitypackage."
# Setting standalone keeps AndroidManifest.xml from being regenerated
#   buildTarget must be before exportPackage for this to work
/Applications/Unity/Hub/Editor/2019.2.14f1/Unity.app/Contents/MacOS/Unity \
   -batchMode \
   -buildTarget standalone \
   -projectPath $project_path \
   -exportPackage Assets $package_name \
   -logFile $log_file \
   -nographics \
   -quit
   
if [ $? = 0 ] ; then
  echo "Created package successfully."
  error_code=0
else
  echo "Creating package failed. Exited with $?."
  error_code=1
fi

#the .unitypackage file is created in the OneSignalExample directory
#move it to the root of the repo
mv $generated_path $final_path

# move back Packages/manifest.json
mv $temp_package_manifest $package_manifest

#move the icons back to their original location
mv $icons_temp_location $icons_location
mv $icons_temp_location.meta $icons_location.meta

#delete the temp folder 
rm -r $temp_location

echo 'Build logs location:'
echo $log_file

echo "Finished with code $error_code"

exit $error_code
