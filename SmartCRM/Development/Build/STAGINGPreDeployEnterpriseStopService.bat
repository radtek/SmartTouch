echo off
sc query "LMITSchedulerService" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "LMITSchedulerService is running.
	sc stop "LMITSchedulerService"	
) else (
    echo "serviceName is not running"
)