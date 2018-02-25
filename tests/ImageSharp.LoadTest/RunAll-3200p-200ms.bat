@echo off
cd ./bin/Release/net461

set fn=Result-3200p-200ms.txt 
echo writing into %fn%
echo ============== > %fn%

echo CreateWithAggressivePooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithAggressivePooling2 3200 1000 200  >>  %fn%

echo CreateWithAggressivePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithAggressivePooling 3200 1000 200  >>  %fn%

echo CreateWithNormalPooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithNormalPooling 3200 1000 200  >>  %fn%

echo CreateWithNormalPooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithNormalPooling2 3200 1000 200  >>  %fn%

echo CreateWithModeratePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithModeratePooling 3200 1000 200  >>  %fn%

echo CreateWithConservativePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithConservativePooling 3200 1000 200  >>  %fn%

echo CreateWithConservativePooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithConservativePooling2 3200 1000 200  >>  %fn%

echo NoPooling ..
ImageSharp.LoadTest.exe NoPooling 3200 1000 200  >>  %fn%

echo DONE.
