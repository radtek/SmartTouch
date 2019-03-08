echo off
sc query "Smart CRM - Smart Search Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Smart Search Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Smart Search Processor"	
)
