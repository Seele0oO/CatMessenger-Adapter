#!/bin/bash
# package-and-upload.sh

PROJECTS=("CatMessenger.Core" "CatMessenger.Matrix" "CatMessenger.Telegram")
for PROJECT in "${PROJECTS[@]}"; do
  TAR_PATH="./${PROJECT}/bin/Release/net8.0/${PROJECT}.tar.gz"
  tar -czf $TAR_PATH -C ./${PROJECT}/bin/Release/net8.0 publish
  echo "::set-output name=${PROJECT}_tar_path::$TAR_PATH"
done
