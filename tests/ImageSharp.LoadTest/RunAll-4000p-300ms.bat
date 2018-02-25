@echo off
cd ./bin/Release/net461

set fn=Result-4000p-300ms.txt 
echo writing into %fn%
echo ============== > %fn%

echo NoPooling ..
ImageSharp.LoadTest.exe NoPooling 4000 1500 300  >>  %fn%

echo CreateWithConservativePooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithConservativePooling2 4000 1500 300  >>  %fn%

echo CreateWithConservativePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithConservativePooling 4000 1500 300  >>  %fn%

echo CreateWithModeratePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithModeratePooling 4000 1500 300  >>  %fn%

echo CreateWithNormalPooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithNormalPooling2 4000 1500 300  >>  %fn%

echo CreateWithNormalPooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithNormalPooling 4000 1500 300  >>  %fn%

echo CreateWithAggressivePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithAggressivePooling 4000 1500 300  >>  %fn%

echo CreateWithAggressivePooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithAggressivePooling2 4000 1500 300  >>  %fn%

echo DONE.
