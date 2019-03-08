echo off
sc query "Smart CRM - Notification Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Notification Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Notification Processor"	
)
