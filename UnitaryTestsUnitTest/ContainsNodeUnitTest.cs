using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SedaSummaryGenerator;

namespace UnitaryTestsUnitTest {
    [TestClass]
    public class ContainsNodeUnitTest {
        [TestMethod]
        public void C50_rootContainsTest() {
            ContainsNode root = new ContainsNode("root", null, true);
            Assert.AreNotEqual(null, root);
            Assert.AreEqual(0, root.getNbDocuments());
            Assert.AreEqual(null, root.next());
            Assert.AreEqual("root", root.getName());
            Assert.AreEqual(null, root.getParent());
        }

        [TestMethod]
        public void C51_rootChildrensContainsTest() {
            ContainsNode root = new ContainsNode("root", null, true);
            ContainsNode child1 = new ContainsNode("child1", root, true);
            ContainsNode child11 = new ContainsNode("child11", child1, true);
            ContainsNode child2 = new ContainsNode("child2", root, true);
            ContainsNode child21 = new ContainsNode("child21", child2, true);
            ContainsNode child22 = new ContainsNode("child22", child2, true);

            Assert.AreEqual("child11", child11.getName(), "child11.getName()");
            Assert.AreEqual("child1", child11.getParent().getName(), "child11.getParent()");

            Assert.AreEqual("child22", child22.getName(), "child22.getName()");
            Assert.AreEqual("child2", child22.getParent().getName(), "child22.getParent()");

            Assert.AreEqual("child2", child2.getName(), "child2.getName()");
            Assert.AreEqual("root", child2.getParent().getName(), "child2.getParent()");
        }

        [TestMethod]
        public void C52_treeNbDocumentsTest() {
            ContainsNode root = new ContainsNode("root", null, true);
            ContainsNode n1 = root.addNewNode("n1", true);
            ContainsNode n11 = n1.addNewNode("n11", true);
            n11.incNbDocs(2);
            ContainsNode n12 = n1.addNewNode("n12", true);
            n11.incNbDocs(1);
            ContainsNode n2 = root.addNewNode("n2", true);
            n2.incNbDocs(1);
            ContainsNode n21 = n2.addNewNode("n21", true);
            n21.incNbDocs(5);
            ContainsNode n22 = n2.addNewNode("n22", true);

            Assert.AreEqual(9, root.computeNbDocuments(), "root.computeNbDocuments()");
            Assert.AreEqual(9, root.getNbDocuments(), "root.getNbDocuments()");
            int nbNodes = 1; // Au moins root
            ContainsNode node = root;
            while ((node = node.next()) != null) {
                nbNodes++;
            }
            Assert.AreEqual(7, nbNodes, "next()");
        }

        [TestMethod]
        // Des nœuds non obligatoires mais contenant des documents sont conservés
        public void C53_treeNonMandatoryButNotEmptyTest() {
            ContainsNode root = new ContainsNode("root", null, true);
            ContainsNode n1 = root.addNewNode("n1", true);
            ContainsNode n11 = n1.addNewNode("n11", false);
            n11.incNbDocs(2);
            ContainsNode n12 = n1.addNewNode("n12", true);
            n11.incNbDocs(1);
            ContainsNode n2 = root.addNewNode("n2", false);
            n2.incNbDocs(1);
            ContainsNode n21 = n2.addNewNode("n21", true);
            n21.incNbDocs(5);
            ContainsNode n22 = n2.addNewNode("n22", true);

            Assert.AreEqual(9, root.computeNbDocuments(), "root.computeNbDocuments()");
            Assert.AreEqual(9, root.getNbDocuments(), "root.getNbDocuments()");
            root.trunkChildrenOfEmptyBranches();
            int nbNodes = 1; // Au moins root
            ContainsNode node = root;
            while ((node = node.next()) != null) {
                nbNodes++;
            }
            Assert.AreEqual(7, nbNodes, "next()");
        }

        [TestMethod]
        // Des nœuds non obligatoires mais contenant des documents sont conservés
        public void C54_treeNonMandatoryAndEmptyTest() {
            ContainsNode root = new ContainsNode("root", null, true);
            ContainsNode n1 = root.addNewNode("n1", true);
            ContainsNode n11 = n1.addNewNode("n11", false);

            ContainsNode n12 = n1.addNewNode("n12", true);
            n11.incNbDocs(1);
            ContainsNode n2 = root.addNewNode("n2", false);

            ContainsNode n21 = n2.addNewNode("n21", true);

            ContainsNode n22 = n2.addNewNode("n22", true);

            Assert.AreEqual(1, root.computeNbDocuments(), "root.computeNbDocuments()");
            Assert.AreEqual(1, root.getNbDocuments(), "root.getNbDocuments()");
            root.trunkChildrenOfEmptyBranches();
            int nbNodes = 1; // Au moins root
            ContainsNode node = root;
            while ((node = node.next()) != null) {
                nbNodes++;
            }
            Assert.AreEqual(5, nbNodes, "next()");
        }

        [TestMethod]
        public void C55_getRelativeContextTest() {
            ContainsNode root = new ContainsNode("root", null, true);
            ContainsNode A_1 = root.addNewNode("A[#1]", true);
            ContainsNode AA_1 = A_1.addNewNode("AA", false);
                                AA_1.addNewNode("AAA[#1]", true);
                                AA_1.addNewNode("AAA[#2]", true);
            ContainsNode tst_1 = AA_1.addNewNode("AAA[#3]", true);

            ContainsNode BB_1 = A_1.addNewNode("BB", true);
                                BB_1.addNewNode("BBB[#1]", true);
            ContainsNode tst_2 = BB_1.addNewNode("BBB[#2]", true);
                                BB_1.addNewNode("BBB[#3]", true);

            ContainsNode A_2 = root.addNewNode("A[#2]", false);
            ContainsNode AA_2 = A_2.addNewNode("AA", false);
            ContainsNode tst_3 = AA_2.addNewNode("AAA[#1]", true);
                                AA_2.addNewNode("AAA[#2]", true);
                                AA_2.addNewNode("AAA[#3]", true);

            ContainsNode BB_2 = A_2.addNewNode("BB", true);
                                BB_2.addNewNode("BBB[#1]", true);
                                BB_2.addNewNode("BBB[#2]", true);
                                BB_2.addNewNode("BBB[#3]", true);
            ContainsNode tst_4 = BB_2.addNewNode("BBB[#4]", true);

            Assert.AreEqual("A[#1]//AAA[#3]", tst_1.getRelativeContext(), false, "tst_1.getRelativeContext()");
            Assert.AreEqual("A[#1]//BBB[#2]", tst_2.getRelativeContext(), false, "tst_2.getRelativeContext()");
            Assert.AreEqual("A[#2]//AAA[#1]", tst_3.getRelativeContext(), false, "tst_3.getRelativeContext()");
            Assert.AreEqual("A[#2]//BBB[#4]", tst_4.getRelativeContext(), false, "tst_4.getRelativeContext()");
        }


    }
}
