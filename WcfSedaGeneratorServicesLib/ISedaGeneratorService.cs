using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfSedaGeneratorServicesLib {
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom d'interface "IService1" à la fois dans le code et le fichier de configuration.
    [ServiceContract]
    public interface ISedaGeneratorService {
        // retourne des erreurs s'il s'en produit
        // sinon retourne une chaîne vide
        [OperationContract]
        string checkProfileFile(String filename);

        // retourne des erreurs s'il s'en produit
        // sinon retourne une chaîne vide
        // Appel avec :
        // espaceCoConvertToCsvFile("Documents de l'espace collaboratif des archives départementales de septembre 2014"
        //                         , "\\vm-devshare\d$\DEV_PPE\tmp\esco-ad/esco-ad/fichiers"
        //                         , "preprodarchives.cg56.fr"
        //                         , "\\vm-devshare\d$\DEV_PPE\tmp/esco-ad/liste-fichiers-archive-escoad-02.txt");
        [OperationContract]
        string espaceCoConvertToCsvFile(String title, String repBase, String urlBase, String filename);

        // retourne des erreurs s'il s'en produit
        // sinon retourne une chaîne vide
        // Appel avec :
        // produireBordereauVersement("ACCORD_TEST_ENRSON"
        //                            , "\\vm-devshare\d$\DEV_PPE\tmp/esco-ad/liste-fichiers-archive-escoad-02.txt"
        //                            , "\\vm-devshare\d$\DEV_PPE\tmp/esco-ad/bordereau-ESCOAD.xml");
        [OperationContract]
        string produireBordereauVersement(String accordVersement, String documentsFile, String bordereauFile);

        /*
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);
        */
        // TODO: ajoutez vos opérations de service ici
    }


    /*
    // Utilisez un contrat de données (comme illustré dans l'exemple ci-dessous) pour ajouter des types composites aux opérations de service.
    // Vous pouvez ajouter des fichiers XSD au projet. Une fois le projet généré, vous pouvez utiliser directement les types de données qui y sont définis, avec l'espace de noms "WcfSedaGeneratorServicesLib.ContractType".
    [DataContract]
    public class CompositeType {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
     * */
}
