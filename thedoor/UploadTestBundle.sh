#!/bin/bash

echo $1
if [ "$1" != "" ]
then
	sh ./UploadBundle.sh majampachinko-test1 majampachinko_bundle_test $1
fi