#!/bin/bash
# author: Lennart Ochel

NPP_PATH=$(which notepad++)

if [ "$1" == 'release' ]; then
  cp bin/Release/NppModelica.dll ${NPP_PATH%/*}/plugins
elif [ "$#" -eq 0 ] || [ "$1" == 'debug' ]; then
  cp bin/Debug/NppModelica.dll ${NPP_PATH%/*}/plugins
else
  echo "Invalid argument: $@"
  echo "Usage: ./launch_npp.sh [release|debug]"
  exit 1
fi

notepad++ &
