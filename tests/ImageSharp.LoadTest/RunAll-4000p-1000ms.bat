@echo off
cd ./bin/Release/net461

set fn=Result-4000p-1000ms.txt 
echo writing into %fn%
echo ============== > %fn%


echo CreateWithAggressivePooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithAggressivePooling2 4000 1500 1000  >>  %fn%

echo CreateWithAggressivePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithAggressivePooling 4000 1500 1000  >>  %fn%

echo CreateWithNormalPooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithNormalPooling 4000 1500 1000  >>  %fn%

echo CreateWithNormalPooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithNormalPooling2 4000 1500 1000  >>  %fn%

echo CreateWithModeratePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithModeratePooling 4000 1500 1000  >>  %fn%

echo CreateWithConservativePooling ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithConservativePooling 4000 1500 1000  >>  %fn%

echo CreateWithConservativePooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithConservativePooling2 4000 1500 1000  >>  %fn%

echo ============== >> %fn% 
echo NoPooling ..
ImageSharp.LoadTest.exe NoPooling 4000 1500 1000  >>  %fn%

echo === OLD === >> %fn%
cd ../../Release_Old/net461
ImageSharp.LoadTest.exe _ 4000 1500 1000  >>  ../../Release/net461/%fn%

echo DONE.
