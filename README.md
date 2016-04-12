# Seda-Generator

Archivage num�rique - Digital archiving - ProBANT

G�n�rateur automatique de bordereaux de transfert SEDA ProBANT

Disponible 
   sur GitHub   https://github.com/PatPercot/Seda-Generator
Et sur CodePlex https://sedaautogenerator.codeplex.com/


Pour d�marrer rapidement : lire le fichier Documentation/QuickStart.txt

ProBANT signifie Production de bordereaux pour l'archivage num�rique territorial



Pour vous informer un peu plus sur la g�n�ration de bordereaux de transfert :

Ce g�n�rateur de bordereaux de transferts n'est pas vraiment le seul g�n�rateur existant.
Mais il est le seul g�n�rateur publi� sous licence libre.
Vous en trouverez aussi une version Java sur GitHub : https://github.com/CGI-France/Java-Seda-Generator

� ce jour, il y a peu de g�n�rateurs. Mais demain tous les SAE auront besoin de s�appuyer sur eux, 
parce qu�ils apportent une solution aux probl�mes ci-dessous :
-	le bordereau, c�est le b�t qui blesse, la source de la majeure partie des probl�mes d�archivage num�rique ;
-	la production de bordereaux de transferts est une chose complexe, les applications m�tier ne savent pas 
    les g�n�rer correctement (l�arborescence est complexe et la majeure partie des donn�es est de nature 
	archivistique) et le co�t de d�veloppement des bordereaux est un frein au d�veloppement des SAE ;
-	� chaque modification du profil (aussi simple soit-elle), il faut modifier le code qui produit 
    le bordereau (et accessoirement payer le prestataire) ;
-	� chaque changement de version du SEDA, il faut r�crire le code de g�n�ration du bordereau.
 
Mode de fonctionnement des g�n�rateurs
Principe : toutes les donn�es qui sont pr�cis�es dans le profil ne sont pas demand�es aux applications m�tier.
Une solution ais�e � mettre en �uvre est l�utilisation des chemins XPath. Les donn�es m�tier sp�cifient 
le chemin de chacune des donn�es � archiver.
-	Inconv�nient num�ro 1 : les profils riches (comme les march�s, beaucoup de documents, arborescence complexe) 
    ont des chemins XPath tr�s longs et difficiles � produire ;
-	Inconv�nient num�ro 2 : les modifications de structure n�cessitent une intervention dans le code qui produit 
    les donn�es m�tier ;
-	Inconv�nient num�ro 3 : le changement de version du SEDA impose de r�crire le code de production des 
    donn�es m�tier.
 
Pour �tre le plus possible ind�pendant de la structure des profils d�archivage, le g�n�rateur doit s�appuyer 
sur les typologies documentaires. Les typologies documentaires sont ce qu�il y a de plus proche du m�tier et 
ce qui a donc le plus de chances d��tre appr�hend� correctement par les applications m�tier. Les typologies 
documentaires peuvent aussi d�finir les m�tadonn�es de gestion du cycle de vie et les r�gles de diffusion 
des documents. On peut �ventuellement l��crire dans un sch�ma qui est en fait un tableau de gestion avec 
une codification. Une typologie documentaire peut contenir des sous-typologies documentaires. 
 
Ces identifiants de typologies documentaire peuvent �galement �tre utilis�s en dehors d�un g�n�rateur 
comme moyen de documenter un profil d�archivage. Par exemple, en renseignant la mani�re dont l�identifiant 
doit �tre construit, � quel sch�ma il se r�f�re mais aussi � quoi l�unit� documentaire se r�f�re. En effet, 
si le nom de l�unit� documentaire est renseign� par les informations collect�es lors du transfert, 
la compr�hension du profil n�cessite des explications qui peuvent �tre fournies gr�ce � cet attribut. 
 
De plus, les typologies documentaires n��voluent pas en fonction de la version du SEDA. Les m�mes donn�es 
m�tier permettent de g�n�rer un bordereau SEDA 0.2, SEDA 1.0 et je l�esp�re SEDA 2.0 (il suffit pour cela 
de changer le profil d�archivage).
 

 
