#!/bin/bash

echo $1
if [ "$1" != "" ]
then
	sh ./UploadBundle.sh majampachinko-develop majampachinko_bundle_develop $1
fi