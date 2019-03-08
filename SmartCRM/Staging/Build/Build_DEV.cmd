echo warning: your are deploying DEV environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /p:EnvironmentName=DEV;UserName=Bharath;Password=Test@12345
pause
