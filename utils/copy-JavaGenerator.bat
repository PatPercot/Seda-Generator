@echo off
REM ***************************************************
REM
REM Copie des exécutables du générateur pour tests unitaires
REM Les tests unitaires ne sont réalisés que sur le code Java de CD56
REM
REM ***************************************************


REM On se place dans le répertoire parent
echo Déplacement dans le répertoire parent
cd ..

REM Copie des exécutables Java CD56
REM echo Suppression des exécutables java CD56
REM if not exist lib-cd56 mkdir lib-cd56
REM del /Q lib-cd56\*

echo Copie des bibliothèques Java
copy ..\CD56-Java-Seda-Generator\target\lib\* SedaSummaryGeneratorUnitTest\lib-cd56 
echo Copie des exécutables Java CD56
copy ..\CD56-Java-Seda-Generator\target\JavaSedaProfileGenerator-*.jar SedaSummaryGeneratorUnitTest\lib-cd56 1>NUL

echo Retour dans le repertoire utils
REM On revient dans utils
cd utils

echo Copie achevée
echo !!!!!!!!!!
echo Modifier la version dans CS-Seda-Generator\SedaSummaryGeneratorUnitTest\JavaSedaProfileGeneratorCD56_task.bat
echo Modifier la version dans CS-Seda-Generator\SedaSummaryGeneratorUnitTest\JavaSedaProfileGeneratorCD56_task.sh
echo !!!!!!!!!!

