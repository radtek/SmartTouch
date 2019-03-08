echo off
sc query "LMITSchedulerService" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "LMITSchedulerService is running.	
) else (
    echo "serviceName is not running"
	sc start "LMITSchedulerService"	
)

