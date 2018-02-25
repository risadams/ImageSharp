@echo off
cd ./bin/Release/net461

set fn=Result-CompareDefaults-10000Requests-3200p-200ms.txt 
echo writing into %fn%
echo ============== > %fn%

echo 10000Req + CreateWithNormalPooling2 ..
echo ============== >> %fn% 
ImageSharp.LoadTest.exe CreateWithNormalPooling2 3200 1000 200 10000  >>  %fn%

echo 10000Req + OLD ..
echo === OLD === >> %fn%
cd ../../Release_Old/net461
ImageSharp.LoadTest.exe _ 3200 1000 200 10000  >>  ../../Release/net461/%fn%

echo DONE.
