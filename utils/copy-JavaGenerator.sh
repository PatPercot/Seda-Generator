#!/bin/bash

# ***************************************************
#
# Copie des exécutables du générateur pour tests unitaires
# Les tests unitaires ne sont réalisés que sur le code Java de CD56
#
# ***************************************************

# On se place dans le répertoire parent
cd ..

# Copie des exécutables Java CD56
# echo Suppression des exécutables java CD56
# [ ! -e lib-cd56 ] && mkdir lib-cd56
# rm lib-cd56/*

echo Copie des exécutables Java
cp ../CD56-Java-Seda-Generator/target/lib/* SedaSummaryGeneratorUnitTest/lib-cd56
cp ../CD56-Java-Seda-Generator/target/JavaSedaProfileGenerator-*.jar SedaSummaryGeneratorUnitTest/lib-cd56

# On revient dans utils
cd -

echo Copie achevée
