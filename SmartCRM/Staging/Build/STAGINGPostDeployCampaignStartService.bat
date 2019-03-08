echo off
sc query "Smart CRM - Campaign Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Campaign Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Campaign Processor"	
)

