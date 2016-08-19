copy ..\BusinessDataController\bin\Release\* .
copy ..\RngProfileControllerTester\bin\Release\* .
copy ..\SedaSummaryGeneratorTester\bin\Release\* .

REM à partir de la version 1.0.3 le générateur à utiliser est le générateur Java
del SedaSummaryGeneratorLauncher.exe
del *.pdb
del *.vshost.exe.config
del *.vshost.exe
del *.vshost.*
