/*****************************************************************************************
 * 
 * Générateur de bordereaux de transfert guidé par le profil d'archivage
 *                         -----------------------
 * Département du Morbihan
 * Direction des systèmes d'information
 * Service Études
 * Patrick Percot
 * Mars-mai 2015
 * 
 * Réutilisation du code autorisée dans l'intérêt des collectivités territoriales
 * 
 * ****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SedaSummaryGenerator {
    /* 
     * Cette classe permet de calculer le nombre de documents 
     * contenus dans les unités documentaires (filles comprises)
     * Elle implémente un arbre de ContainsNode
     * Tout d'abord l'arbre doit être construit avec la méthode addNewNode
     * incNbDocs permet de mettre à jour le nombre de documents dans le nœud courant
     * La méthode computeNbDocuments permet à la fin de création de l'arbre
     * de calculer le nombre de documents à chaque niveau de l'arborescence
     * Ensuite getNbDocuments permet de connaître le nombre de documents dans une branche
     * La méthode next() permet de lire l'arbre comme un vecteur pour la seconde passe
     * La méthode getRelativeContext renvoie une chaîne contenant le nom du tag
     * précédé éventuellement des tag numérotés qui font partie de ses parents
     * */
    public class ContainsNode {
        private String tabs; // pour l'indentation des traces
        private int nbDocuments;
        private bool mandatory; // indique que l'unité documentaire est obligatoire 
                                // (contenue dans rng:oneOrMore ou spécifiée dans rng:element 
                                // elle doit donc contenir un ou plusieurs documents
                                // si cette condition n'est pas remplie, une alerte doit être levée
        private String objectIdentifier;
        private List<ContainsNode> childrens;
        private ContainsNode parent;

        /*
         * Le premier nœud doit être créé avec un parentNode null
         * */
        public ContainsNode(String nodeId, ContainsNode parentNode, bool bContainsIsMandatory) {
            objectIdentifier = nodeId;
            nbDocuments = 0;
            childrens = new List<ContainsNode>();
            parent = parentNode;
            if (parentNode == null) {
                tabs = "\t";
            } else {
                tabs = parentNode.tabs + "\t";
            }
            mandatory = bContainsIsMandatory;
        }

        public String getName() {
            return objectIdentifier;
        }

        public ContainsNode getParent() {
            return parent;
        }

        /*
         * Ajoute un nœud au nœud courant
         * */
        public ContainsNode addNewNode(String newNodeId, bool bContainsIsMandatory) {
            int numeroTag = 0;
            if (newNodeId.EndsWith("+")) {
                numeroTag++;
                String nodeName = newNodeId.Substring(0,newNodeId.Length-1);
                foreach (ContainsNode child in this.childrens) {
                    // Le nom des frères peut commener par nodeName
                    // Le nom du frère est donc de la forme nodeName[numero]
                    String brotherName = child.getName();
                    Regex r = new Regex(nodeName + @"\[#(\d+)\]");
                    Match m = r.Match(brotherName);
                    if (m.Success) {
                        try {
                            int newNumeroTag = Convert.ToInt32(m.Groups[1].ToString()) + 1;
                            if (newNumeroTag > numeroTag)
                                numeroTag = newNumeroTag;
                        } catch (Exception e) { e.ToString(); }
                    }
                }
            }
            if (numeroTag > 0)
                    newNodeId = newNodeId.Substring(0, newNodeId.Length - 1) + "[#" + numeroTag + "]";
            ContainsNode newNode = new ContainsNode(newNodeId, this, bContainsIsMandatory);
            this.childrens.Add(newNode);
            return newNode;
        }


        /*
         * Utilisé à partir de la racine, permet de couper les branches vides
         * Les branches sont coupées à partir des nœuds qui ont 0 document
         * Seul le nœud qui a 0 document est conservé, ses enfants sont éliminés
         * */
        public void trunkChildrenOfEmptyBranches() {
            if (this.nbDocuments == 0)
                childrens.Clear();
            foreach (ContainsNode node in childrens) {
                node.trunkChildrenOfEmptyBranches();
            }
        }

        /*
         * Utilisé à partir de la racine, permet de parcourir l'ensemble de l'arbre
         * comme un vecteur.
         * Retourne null si il n'y a plus d'éléments dans l'arbre
         * */
        public ContainsNode next() {
            if (this.childrens.Count > 0)
                return this.childrens[0]; // on retourne le premier fils
            return this.nextBrother();
        }

        protected ContainsNode nextBrother() {
            bool callerReached = false;
            if (this.parent != null) {
                foreach (ContainsNode child in this.parent.childrens) {
                    if (callerReached)
                        return child;
                    if (child == this)
                        callerReached = true;
                }
            }
            return parent == null ? null : parent.nextBrother();
        }


        /*
         * Permet de récupérer le chemin relatif du nœud courant
         * retourne le nom du tag précédé des tags numérotés de ses ancètres
         * les tags non numérotés sont exclus de cette liste
         * exemple : 
         *    /TAG_A[#1]/TAG_B/TAG_C[#2]/TAG_D/THIS_TAG
         * a pour chemin relatif
         *    TAG_A[#1]//TAG_C[#2]//THIS_TAG
         * 
         * TODO: à optimiser, attribut chemin relatif, calcul des chemins 
         * relatifs déclenché par exemple à computeNbDocuments
         * */
        public String getRelativeContext() {
            String context = String.Empty;
            ContainsNode node = this;
            while ((node = node.getParent()) != null) {
                String parentName = node.getName();
                Regex r = new Regex(@"\[#(\d+)\]");
                MatchCollection matches = r.Matches(parentName);
                if (matches.Count != 0) {
                    context = parentName + "//" + context;
                }
            }
            context += this.getName();
            return context;
        }
        /*
         * Permet de mettre à jour le nombre de documents dans le nœud courant
         * La méthode computeNbDocuments sera chargée en fin de création 
         * de l'arbre de calculer le nombre de documents à chque niveau de l'arborescence
         * */
        public void incNbDocs(int counter) {
            nbDocuments += counter;
        }

        /*
         * Cette méthode récursive calcule et met à jour le nombre de documents contenus dans le nœud et ses enfants.
         * Pour calculer le nombre total de documents, il faut l'exécuter sur le nœud racine
         * */
        public int computeNbDocuments() {
            foreach (ContainsNode node in childrens) {
                nbDocuments += node.computeNbDocuments();
            }
            return nbDocuments;
        }

        /*
         * Cette méthode retourne le nombre de documents du nœud
         * */
        public int getNbDocuments() {
            return nbDocuments;
        }

        /*
         * Cette méthode retourne l'attribut obligatoire du nœud
         * */
        public bool getMandatory() {
            return mandatory;
        }

        /*
         * Pour la détection de doublons, doit être appelé sur
         * le nœud racine pour une évaluation complète
         * */
        public StringCollection checkDuplicates(ContainsNode rootNode, String excluded) {
            Dictionary<String, Int32> identifiers = new Dictionary<String, Int32>();
            List<ContainsNode> lstNodes = new List<ContainsNode>();
            getNodesList(rootNode, lstNodes);
            foreach (ContainsNode checkedNode in lstNodes) {
                checkedNode.findDuplicates(lstNodes, checkedNode, excluded, identifiers);
            }
            StringCollection duplicates = new StringCollection();
            Dictionary<String, int>.KeyCollection keys = identifiers.Keys;
            foreach (String key in keys) {
                if (identifiers[key] > 1) {
                    duplicates.Add ("L'identifiant " + key + " n'est pas unique, il est utilisé " + identifiers[key] + " fois.");
                }
            }
            return duplicates;
        }

        protected void findDuplicates(List<ContainsNode> lstNodes, ContainsNode checkedNode, String excluded, Dictionary<String, Int32> identifiers) {
            // on ne teste que si on a atteint l'identifiant pour éviter de traiter tout l'arbre avec chaque identifiant
            bool bTester = false;
            foreach (ContainsNode curNode in lstNodes) {
                if (curNode != checkedNode) {
                    if (bTester != false)
                        continue;
                    if (curNode.objectIdentifier.Equals(checkedNode.objectIdentifier))
                        if (!curNode.objectIdentifier.Equals(excluded)) {
                            if (identifiers.ContainsKey(curNode.objectIdentifier)) {
                                identifiers[curNode.objectIdentifier] = identifiers[curNode.objectIdentifier] + 1;
                            }
                        }
                } else {
                    if (!identifiers.ContainsKey(curNode.objectIdentifier)) {
                        identifiers.Add(curNode.objectIdentifier, 1);
                    }
                    bTester = true;
                }
            }
        }

        /* 
         * Création de la liste de tous les nœuds de l'arbre
         * */
        protected void getNodesList(ContainsNode startNode, List<ContainsNode> lstNodes) {
            foreach (ContainsNode curNode in startNode.childrens) {
                lstNodes.Add(curNode);
                getNodesList(curNode, lstNodes);
            }
        }

        /*
         * Pour le traçage de l'arbre à des fins de débogage
         * */
        public String dump(bool afficherDocs = true) {
            String str = tabs + objectIdentifier;
            if (afficherDocs == true)
                str += " docs : '" + nbDocuments + "'";
            str += "\n";
            foreach (ContainsNode node in childrens) {
                str += node.dump(afficherDocs);
            }
            return str;
        }
    }
}
