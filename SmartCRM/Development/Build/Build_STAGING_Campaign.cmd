echo warning: your are deploying STAGING environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:Campaign /p:EnvironmentName=STAGING;UserName=ststage;Password=UZe895E46ny2Kj8
pause