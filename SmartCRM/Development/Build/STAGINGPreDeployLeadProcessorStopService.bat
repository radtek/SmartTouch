runas.exe /user:administrator
echo off
sc query "Smart CRM - Lead Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Lead Processor is running.
	sc stop "Smart CRM - Lead Processor"	
) else (
    echo "serviceName is not running"
)
