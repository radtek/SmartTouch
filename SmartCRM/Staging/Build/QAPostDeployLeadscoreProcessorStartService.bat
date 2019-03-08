echo off
sc query "Smart CRM - Lead Score Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Lead Score Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Lead Score Processor"	
)
