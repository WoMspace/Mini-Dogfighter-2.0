@echo off
:loop
adb logcat | findstr /c:WOM
goto loop