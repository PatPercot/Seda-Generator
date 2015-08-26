/*****************************************************************************************
 * 
 * Générateur de bordereaux de transfert guidé par le profil d'archivage
 *                         -----------------------
 * Département du Morbihan
 * Direction des systèmes d'information
 * Service Études
 * Patrick Percot
 * Mars 2015
 * 
 * Réutilisation du code autorisée dans l'intérêt des collectivités territoriales
 * 
 * ****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SedaSummaryGenerator {
    [Serializable()]
    class SedaSumGenException : System.Exception {
        public SedaSumGenException() : base() { }
        public SedaSumGenException(string message) : base(message) { }
        public SedaSumGenException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected SedaSumGenException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }

    }

    [Serializable()]
    class SedaSumGenNoProfileException : System.Exception {
        public SedaSumGenNoProfileException() : base() { }
        public SedaSumGenNoProfileException(string message) : base(message) { }
        public SedaSumGenNoProfileException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected SedaSumGenNoProfileException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }

    }

    [Serializable()]
    class SedaSumGenNoArchiveDocumentsException : System.Exception {
        public SedaSumGenNoArchiveDocumentsException() : base() { }
        public SedaSumGenNoArchiveDocumentsException(string message) : base(message) { }
        public SedaSumGenNoArchiveDocumentsException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected SedaSumGenNoArchiveDocumentsException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }

    }

    [Serializable()]
    class SedaSumGenNoInformationsException : System.Exception {
        public SedaSumGenNoInformationsException() : base() { }
        public SedaSumGenNoInformationsException(string message) : base(message) { }
        public SedaSumGenNoInformationsException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected SedaSumGenNoInformationsException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }

    }
}
