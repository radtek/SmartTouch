
for /d %%a in ("E:\Production\windowsservices\smartcrm-campaign-processor*") do xcopy "E:\Production\windowsservices\Dependencydll" /e /y "%%a"

for /d %%a in ("E:\Production\windowsservices\crm-scheduler-service*") do xcopy "E:\Production\windowsservices\Dependencydll" /e /y "%%a"



