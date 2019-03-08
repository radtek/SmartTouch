echo off
sc query "Smart CRM - Index Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Index Processor is running.
	sc stop "Smart CRM - Index Processor"	
) else (
    echo "serviceName is not running"
)