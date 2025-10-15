@echo off
setlocal enabledelayedexpansion

:: Path to ffmpeg (edit this if it's not in your PATH)
set FFMPEG="C:\ffmpeg\bin\ffmpeg.exe"

:: Folder where your videos are (the current folder by default)
set INPUT_FOLDER=%~dp0

echo ----------------------------------------
echo Fixing all MP4 videos in: %INPUT_FOLDER%
echo ----------------------------------------

for %%f in ("%INPUT_FOLDER%*.mp4") do (
    set "filename=%%~nf"
    echo Processing: %%f
    %FFMPEG% -i "%%f" -c:v libx264 -crf 18 -pix_fmt yuv420p ^
    -color_primaries bt709 -color_trc bt709 -colorspace bt709 ^
    -c:a copy "%INPUT_FOLDER%!filename!_fixed.mp4"
    echo.
)

echo All videos processed!
pause
