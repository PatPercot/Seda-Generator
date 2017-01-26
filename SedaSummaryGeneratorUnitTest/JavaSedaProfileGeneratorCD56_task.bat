@ECHO OFF
REM Ce script permet de lancer le générateur de bordereaux Java
REM PARAMETRES :
REM - task : tâche à exécuter dans le fichier de configuration job.config
REM CODES D'ERREUR :
REM - 0 : Succès.
REM - 1 : Erreur grave, non tracées dans OUT_SUMMARY
REM - 2 : Erreurs, tracées dans OUT_SUMMARY

REM ARGUMENTS
SET TASK=%1

REM GENERATOR : Modifier à chaque changement de version
SET JAVASEDAPROFILEGENERATOR=lib-cd56/JavaSedaProfileGenerator-1.0.4.jar

REM LIB
SET LOGBACK_LIB=lib-cd56/logback-classic-1.0.7.jar
SET LOGBACK_CORE_LIB=lib-cd56/logback-core-1.0.7.jar
SET POSTGRESQL_LIB=lib-cd56/postgresql-9.4-1201-jdbc41.jar
SET SAXON_LIB=lib-cd56/saxon-8.7.jar
SET SERIALIZER_LIB=lib-cd56/serializer-2.7.1.jar
SET SLF4J_LIB=lib-cd56/slf4j-api-1.7.12.jar
SET XALAN_LIB=lib-cd56/xalan-2.7.1.jar
SET XMLAPIS_LIB=lib-cd56/xml-apis-1.3.04.jar
SET COMMONS_LANG_LIB=lib-cd56/commons-lang-2.3.jar

SET JAVA_BIN=java

SET CLASSPATH=%LOGBACK_LIB%;%LOGBACK_CORE_LIB%;%POSTGRESQL_LIB%;%SAXON_LIB%;%SERIALIZER_LIB%;%SLF4J_LIB%;%XALAN_LIB%;%XMLAPIS_LIB%;%COMMONS_LANG_LIB%;%JAVASEDAPROFILEGENERATOR%;%LOGBACK_DIR%

%JAVA_BIN% -classpath %CLASSPATH% unit.SedaGeneratorUnit ./sedaGeneratorUnit.properties ./job.config %TASK%
