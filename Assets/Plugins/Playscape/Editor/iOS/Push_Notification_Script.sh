#!/bin/bash

array=()

while read -r -d $'\0'; do
	array+=("$REPLY")
done < <(find "$1" -name "Info.plist" -print0)

INFO_PLIST=${array[0]}

#   store app name into PRODUCT_NAME var
BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleIdentifier" "$INFO_PLIST" | tr '"' "'")

IDENTIFIER_ELEMENTS=(${BUNDLE_ID//./ })
PRODUCT_NAME=${IDENTIFIER_ELEMENTS[${#IDENTIFIER_ELEMENTS[@]} - 1]}

#   there are two temp vars for checking prevoius changes of info.plist files
URL_TYPES=$(/usr/libexec/PlistBuddy -c "Print :CFBundleURLTypes" $INFO_PLIST | tr '"' "'")
TEMP_URL_SCHEMES=$(/usr/libexec/PlistBuddy -c "Print :CFBundleURLTypes:0:CFBundleURLSchemes:0" $INFO_PLIST | tr '"' "'")

#   In case if we already have necessary changes
if [ "$URL_TYPES" = "" ] || [ "$TEMP_URL_SCHEMES" != "$PRODUCT_NAME" ]; then
	/usr/libexec/PlistBuddy -c "Add CFBundleURLTypes array" "$INFO_PLIST"
	/usr/libexec/PlistBuddy -c "Add CFBundleURLTypes:0 dict" "$INFO_PLIST"
	/usr/libexec/PlistBuddy -c "Add CFBundleURLTypes:0:CFBundleURLSchemes array" "$INFO_PLIST"
	/usr/libexec/Plistbuddy -c "Add CFBundleURLTypes:0:CFBundleURLSchemes:0 string $PRODUCT_NAME" "$INFO_PLIST"
fi