echo off
sc query "Smart CRM - Smart Search Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Smart Search Processor is running.
	sc stop "Smart CRM - Smart Search Processor"	
) else (
    echo "serviceName is not running"
)
