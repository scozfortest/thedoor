#!/bin/bash

#googleProjectID, storagePath, Application.version, MaJamPachinko.Editor.BuildSetting.BundleVersion 
echo $1 $2 $3
if [ "$1" != "" ] && [ "$2" != "" ] && [ "$3" != "" ]
then
	gcloud config set project $1
	gsutil cp -r ServerData/$3/* gs://$2/$3/
	gsutil setmeta -h "Cache-Control:no-store" -r gs://$2/$3/
	#gsutil setmeta -h "Cache-Control:no-store" gs://$2/$3/Android/catalog_$3.hash
	#gsutil setmeta -h "Cache-Control:no-store" gs://$2/$3/Android/catalog_$3.json
	#gsutil setmeta -h "Cache-Control:no-store" gs://$2/$3/iOS/catalog_$3.hash
	#gsutil setmeta -h "Cache-Control:no-store" gs://$2/$3/iOS/catalog_$3.json	
	exit 0
fi
echo 'Error: parameter contain empty.'
exit -1
