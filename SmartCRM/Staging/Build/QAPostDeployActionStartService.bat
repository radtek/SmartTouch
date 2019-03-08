echo off
sc query "Smart CRM - Action Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Action Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Action Processor"	
)
