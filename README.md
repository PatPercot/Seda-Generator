# Seda-Generator

Archivage numérique - Digital archiving - ProBANT

Générateur automatique de bordereaux de transfert SEDA ProBANT

Disponible 
   sur GitHub   https://github.com/PatPercot/Seda-Generator
Et sur CodePlex https://sedaautogenerator.codeplex.com/


Pour démarrer rapidement : lire le fichier Documentation/QuickStart.txt

ProBANT signifie Production de bordereaux pour l'archivage numérique territorial



Pour vous informer un peu plus sur la génération de bordereaux de transfert :

Ce générateur de bordereaux de transferts n'est pas vraiment le seul générateur existant.
Mais il est le seul générateur publié sous licence libre.
Vous en trouverez aussi une version Java sur GitHub : https://github.com/CGI-France/Java-Seda-Generator

À ce jour, il y a peu de générateurs. Mais demain tous les SAE auront besoin de s’appuyer sur eux, 
parce qu’ils apportent une solution aux problèmes ci-dessous :
-	le bordereau, c’est le bât qui blesse, la source de la majeure partie des problèmes d’archivage numérique ;
-	la production de bordereaux de transferts est une chose complexe, les applications métier ne savent pas 
    les générer correctement (l’arborescence est complexe et la majeure partie des données est de nature 
	archivistique) et le coût de développement des bordereaux est un frein au développement des SAE ;
-	à chaque modification du profil (aussi simple soit-elle), il faut modifier le code qui produit 
    le bordereau (et accessoirement payer le prestataire) ;
-	à chaque changement de version du SEDA, il faut récrire le code de génération du bordereau.
 
Mode de fonctionnement des générateurs
Principe : toutes les données qui sont précisées dans le profil ne sont pas demandées aux applications métier.
Une solution aisée à mettre en œuvre est l’utilisation des chemins XPath. Les données métier spécifient 
le chemin de chacune des données à archiver.
-	Inconvénient numéro 1 : les profils riches (comme les marchés, beaucoup de documents, arborescence complexe) 
    ont des chemins XPath très longs et difficiles à produire ;
-	Inconvénient numéro 2 : les modifications de structure nécessitent une intervention dans le code qui produit 
    les données métier ;
-	Inconvénient numéro 3 : le changement de version du SEDA impose de récrire le code de production des 
    données métier.
 
Pour être le plus possible indépendant de la structure des profils d’archivage, le générateur doit s’appuyer 
sur les typologies documentaires. Les typologies documentaires sont ce qu’il y a de plus proche du métier et 
ce qui a donc le plus de chances d’être appréhendé correctement par les applications métier. Les typologies 
documentaires peuvent aussi définir les métadonnées de gestion du cycle de vie et les règles de diffusion 
des documents. On peut éventuellement l’écrire dans un schéma qui est en fait un tableau de gestion avec 
une codification. Une typologie documentaire peut contenir des sous-typologies documentaires. 
 
Ces identifiants de typologies documentaire peuvent également être utilisés en dehors d’un générateur 
comme moyen de documenter un profil d’archivage. Par exemple, en renseignant la manière dont l’identifiant 
doit être construit, à quel schéma il se réfère mais aussi à quoi l’unité documentaire se réfère. En effet, 
si le nom de l’unité documentaire est renseigné par les informations collectées lors du transfert, 
la compréhension du profil nécessite des explications qui peuvent être fournies grâce à cet attribut. 
 
De plus, les typologies documentaires n’évoluent pas en fonction de la version du SEDA. Les mêmes données 
métier permettent de générer un bordereau SEDA 0.2, SEDA 1.0 et je l’espère SEDA 2.0 (il suffit pour cela 
de changer le profil d’archivage).
 

 
