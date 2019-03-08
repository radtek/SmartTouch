echo off
sc query "Smart CRM - Bulk Operation Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Bulk Operation Processor is running.
	sc stop "Smart CRM - Bulk Operation Processor"	
) else (
    echo "serviceName is not running"
)