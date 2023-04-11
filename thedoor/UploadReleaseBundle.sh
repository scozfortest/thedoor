#!/bin/bash

echo $1
if [ "$1" != "" ]
then
	sh ./UploadBundle.sh majampachinko-release majampachinko_bundle_release $1
fi