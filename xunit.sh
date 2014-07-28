#!/bin/sh -x

mono --runtime=v4.0 .nuget/NuGet.exe install xunit.runners -Version 2.0.0-beta-build2700 -o packages
mono --runtime=v4.0 .nuget/NuGet.exe install xunit.core -Version 2.0.0-beta-build2700 -o packages

cp packages/xunit.runners.2.0.0-beta-build2700/tools/* ./build
cp packages/xunit.core.2.0.0-beta-build2700/lib/portable-net45+win+wpa81+wp80+monotouch+monoandroid/* ./build

runTest(){
   cp $1* ./build
   
   mono --runtime=v4.0 ./build/xunit.console.exe ./build/$2
   if [ $? -ne 0 ]
   then   
     exit 1
   fi
}

runTest Waffle.xunit

exit 
