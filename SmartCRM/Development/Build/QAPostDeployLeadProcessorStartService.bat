echo off
sc query "Smart CRM - Lead Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Lead Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Lead Processor"	
)