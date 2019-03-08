echo off
sc query "Smart CRM - Index Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Index Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Index Processor"	
)
