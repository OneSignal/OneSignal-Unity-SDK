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
config_location=$android_location/OneSignalConfig

if [ ! -d $temp_location ]; then 
   mkdir $temp_location
fi

# Removed generated Android manifest files
if [[ -e "$config_location/AndroidManifest.xml" ]]; then
   rm $config_location/AndroidManifest.xml
fi
if [[ -e "$config_location/AndroidManifest.xml.meta" ]]; then 
   rm $config_location/AndroidManifest.xml.meta
fi

#temporarily remove Android AppIcons 
mv $icons_location $icons_temp_location
mv $icons_location.meta $icons_temp_location.meta

#temporarily move some necessary files
mv $config_location $temp_location/OneSignalConfig
mv $config_location.meta $temp_location/OneSignalConfig.meta

#get rid of a bunch of unnecessary files
rm -r $android_location
mkdir $android_location

#put the config files back
mv $temp_location/OneSignalConfig $config_location
mv $temp_location/OneSignalConfig.meta $config_location.meta

# Create the .unitypackage
echo "Creating unitypackage."
# Setting standalone keeps AndroidManifest.xml from being regenerated
#   buildTarget must be before exportPackage for this to work
/Applications/Unity/Hub/Editor/2019.1.11f1/Unity.app/Contents/MacOS/Unity \
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

#move the icons back to their original location
mv $icons_temp_location $icons_location
mv $icons_temp_location.meta $icons_location.meta

#delete the temp folder 
rm -r $temp_location

echo 'Build logs location:'
echo $log_file

echo "Finished with code $error_code"

exit $error_code
