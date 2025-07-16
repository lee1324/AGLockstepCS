u3d_folder="/Users/imac/Documents/AG/u3d_dev/Assets/Scripts/MainPackage/Game/AGSyncCS"
rm -rf "${u3d_folder}/02Client" "${u3d_folder}/03Common" "${u3d_folder}/04Protocals" "${u3d_folder}/ServerConfig.cs"
cp -R 02Client 03Common 04Protocals "${u3d_folder}"
cp 00Servers/ServerConfig.cs "${u3d_folder}/ServerConfig.cs"