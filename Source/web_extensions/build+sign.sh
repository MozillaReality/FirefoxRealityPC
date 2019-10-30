#! /bin/bash

webexts=( "webcompat_vimeo" "webcompat_youtube" )

function usage {
    echo "Usage: $(basename $0) --amo-api-key AMO_API_KEY --amo-api-secret AMO_API_SECRET"
    exit 1
}

if [ $# -eq 0 ]; then
    usage
fi

# -e = exit on errors
#set -e

# -x = debug
#set -x

# Parse parameters
while test $# -gt 0
do
    case "$1" in
        --amo-api-key) AMO_API_KEY="$2"
            shift
            ;;
        --amo-api-secret) AMO_API_SECRET="$2"
            shift
            ;;
        --debug) DEBUG=
            ;;
        --*) echo "bad option $1"
            usage
            ;;
        *) echo "bad argument $1"
            usage
            ;;
    esac
    shift
done

# Test for web-ext presence.
if ! npm list -g --depth=0 web-ext | grep -q web-ext; then
  echo "npm module web-ext is required"
  exit 1
fi

# Package and sign our webextensions.
for i in "${webexts[@]}"
do
    echo "Processing $i"
    ARCHIVE=$(web-ext build --source-dir $i --artifacts-dir ${PWD} --overwrite-dest)
    if [ $? -ne 0 ]; then
      echo "Error building web extension"
      exit 1
    fi
    web-ext sign --source-dir $i --artifacts-dir ${PWD} --overwrite-dest --api-key "${AMO_API_KEY}" --api-secret "${AMO_API_SECRET}"
done

