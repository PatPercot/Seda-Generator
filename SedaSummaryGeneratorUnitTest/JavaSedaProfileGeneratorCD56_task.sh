#! /bin/sh
# Ce script permet de lancer le générateur de bordereaux Java
# PARAMETRES :
# - task : tâche à exécuter dans le fichier de configuration job.config
# CODES D'ERREUR :
# - 0 : Succès.
# - 1 : Erreur grave, non tracées dans OUT_SUMMARY
# - 2 : Erreurs, tracées dans OUT_SUMMARY

#ARGUMENTS
TASK=$1

#GENERATOR : Modifier à chaque changement de version
JAVASEDAPROFILEGENERATOR=lib-cd56/JavaSedaProfileGenerator-1.0.4.jar

#LIB
ANTLR_LIB=lib-cd56/antlr-2.7.7.jar
ANTLR_RUNTIME_LIB=lib-cd56/antlr-runtime-3.5.jar
CHEMISTRY_OPENCMIS_CLIENT_API_LIB=lib-cd56/chemistry-opencmis-client-api-0.13.0.jar
CHEMISTRY_OPENCMIS_CLIENT_BINDINGS_LIB=lib-cd56/chemistry-opencmis-client-bindings-0.13.0.jar
CHEMISTRY_OPENCMIS_CLIENT_IMPL_LIB=lib-cd56/chemistry-opencmis-client-impl-0.13.0.jar
CHEMISTRY_OPENCMIS_CLIENT_COMMONS_API_LIB=lib-cd56/chemistry-opencmis-commons-api-0.13.0.jar
CHEMISTRY_OPENCMIS_CLIENT_COMMONS_IMPL_LIB=lib-cd56/chemistry-opencmis-commons-impl-0.13.0.jar
CHEMISTRY_OPENCMIS_CLIENT_SERVER_SUPPORT_LIB=lib-cd56/chemistry-opencmis-server-support-0.13.0.jar
COMMONS_LANG_LIB=lib-cd56/commons-lang-2.3.jar
LOGBACK_LIB=lib-cd56/logback-classic-1.0.7.jar
LOGBACK_CORE_LIB=lib-cd56/logback-core-1.0.7.jar
POSTGRESQL_LIB=lib-cd56/postgresql-9.4-1201-jdbc41.jar
SERIALIZER_LIB=lib-cd56/serializer-2.7.1.jar
SLF4J_LIB=lib-cd56/slf4j-api-1.7.12.jar
STAX2_LIB=lib-cd56/stax2-api-3.1.4.jar
STRINGTEMPLATE_LIB=lib-cd56/stringtemplate-3.2.1.jar
WOODSTOX_LIB=lib-cd56/woodstox-core-asl-4.4.0.jar
XALAN_LIB=lib-cd56/xalan-2.7.1.jar
XMLAPIS_LIB=lib-cd56/xml-apis-1.3.04.jar

JAVA_BIN=java

CLASSPATH=$ANTLR_LIB:$ANTLR_RUNTIME_LIB:$CHEMISTRY_OPENCMIS_CLIENT_API_LIB:$CHEMISTRY_OPENCMIS_CLIENT_BINDINGS_LIB:$CHEMISTRY_OPENCMIS_CLIENT_IMPL_LIB:$CHEMISTRY_OPENCMIS_CLIENT_COMMONS_API_LIB:$CHEMISTRY_OPENCMIS_CLIENT_COMMONS_IMPL_LIB:$CHEMISTRY_OPENCMIS_CLIENT_SERVER_SUPPORT_LIB:$COMMONS_LANG_LIB:$LOGBACK_LIB:$LOGBACK_CORE_LIB:$POSTGRESQL_LIB:$SERIALIZER_LIB:$SLF4J_LIB:$STAX2_LIB:$STRINGTEMPLATE_LIB:$WOODSTOX_LIB:$XALAN_LIB:$XMLAPIS_LIB:$JAVASEDAPROFILEGENERATOR:$LOGBACK_DIR
$JAVA_BIN -classpath $CLASSPATH unit.SedaGeneratorUnit ./sedaGeneratorUnit.properties ./job.config $TASK
